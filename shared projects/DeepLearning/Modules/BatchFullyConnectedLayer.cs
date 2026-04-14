namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Fully connected layer on NHWC batch tensors without layout conversion.
/// Applies y = xWᵀ + b per spatial position (shared weights).
/// </summary>
public sealed class BatchFullyConnectedLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Parameter<T> _weight;
    private readonly Parameter<T> _bias;

    public BatchFullyConnectedLayer(int inputSize, int outputSize, string? name = null)
        : base(name)
    {
        if (inputSize <= 0 || outputSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(inputSize), "Layer sizes must be positive.");
        }

        InputSize = inputSize;
        OutputSize = outputSize;
        _weight = new Parameter<T>(new Tensor<T>(1, outputSize, inputSize), $"{name}.weight");
        _bias = new Parameter<T>(new Tensor<T>(1, 1, outputSize), $"{name}.bias");

        var limit = T.CreateTruncating(Math.Sqrt(6.0 / (inputSize + outputSize)));
        TensorUtilities.FillUniformRandom(_weight.Value, limit);
        _bias.Value.Fill(T.Zero);
    }

    /// <summary>Gets the input feature count (channels).</summary>
    public int InputSize { get; }

    /// <summary>Gets the output feature count (channels).</summary>
    public int OutputSize { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [_weight, _bias];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
    {
        ValidateInputShape(inputShape);
        return new BatchShape(inputShape.Batch, inputShape.Height, inputShape.Width, OutputSize);
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        ValidateInputShape(input.Shape);
        var output = new BatchTensor<T>(input.Batch, input.Height, input.Width, OutputSize);
        var weight = _weight.Value;
        var bias = _bias.Value;
        int spatial = input.Height * input.Width;

        long workPerSample = (long)spatial * OutputSize * InputSize;
        ComputingContextExecution.ForEach(null, 0, input.Batch, n =>
        {
            for (int s = 0; s < spatial; s++)
            {
                int h = s / input.Width;
                int w = s % input.Width;
                int inBase = ((n * input.Height + h) * input.Width + w) * input.Channels;
                int outBase = ((n * output.Height + h) * output.Width + w) * OutputSize;

                for (int o = 0; o < OutputSize; o++)
                {
                    T sum = bias[0, 0, o];
                    BatchNormMath.AccumulateDotSimd(
                        input.Values.Slice(inBase, InputSize),
                        weight.Values.Slice(o * InputSize, InputSize),
                        ref sum);
                    output.Values[outBase + o] = sum;
                }
            }
        }, workPerSample);

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        var input = CachedInput!;
        var gradInput = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        gradInput.Values.Clear();
        var weight = _weight.Value;
        int spatial = input.Height * input.Width;

        for (int n = 0; n < input.Batch; n++)
        {
            for (int s = 0; s < spatial; s++)
            {
                int h = s / input.Width;
                int w = s % input.Width;
                int inBase = ((n * input.Height + h) * input.Width + w) * input.Channels;
                int outBase = ((n * gradOutput.Height + h) * gradOutput.Width + w) * OutputSize;

                for (int o = 0; o < OutputSize; o++)
                {
                    var gradO = gradOutput.Values[outBase + o];
                    _bias.Gradient[0, 0, o] += gradO;

                    for (int i = 0; i < InputSize; i++)
                    {
                        var inVal = input.Values[inBase + i];
                        _weight.Gradient[0, o, i] += gradO * inVal;
                        gradInput.Values[inBase + i] += gradO * weight[0, o, i];
                    }
                }
            }
        }

        return gradInput;
    }

    private void ValidateInputShape(BatchShape shape)
    {
        if (shape.Channels != InputSize)
        {
            throw new ArgumentException(
                $"Expected {InputSize} input channels, got {shape.Channels}.",
                nameof(shape));
        }
    }
}
