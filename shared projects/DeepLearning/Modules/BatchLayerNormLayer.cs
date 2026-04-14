namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Layer normalization over the channel dimension at each spatial position.
/// </summary>
public sealed class BatchLayerNormLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Parameter<T> _scale;
    private readonly Parameter<T> _shift;
    private BatchTensor<T>? _normalizedInput;
    private T[]? _mean;
    private T[]? _variance;

    public BatchLayerNormLayer(int channels, string? name = null)
        : base(name)
    {
        Channels = channels;
        _scale = new Parameter<T>(new Tensor<T>(1, 1, channels), $"{name}.scale");
        _shift = new Parameter<T>(new Tensor<T>(1, 1, channels), $"{name}.shift");
        _scale.Value.Fill(T.One);
        _shift.Value.Fill(T.Zero);
    }

    public int Channels { get; }
    public T Epsilon { get; set; } = T.CreateChecked(1e-5);

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [_scale, _shift];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
    {
        if (inputShape.Channels != Channels)
        {
            throw new ArgumentException($"Expected {Channels} channels, got {inputShape.Channels}.");
        }

        return inputShape;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        int spatial = input.Batch * input.Height * input.Width;
        var output = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        _normalizedInput = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        _mean = new T[spatial];
        _variance = new T[spatial];
        var workspace = new T[Channels];

        for (int s = 0; s < spatial; s++)
        {
            int n = s / (input.Height * input.Width);
            int rem = s % (input.Height * input.Width);
            int h = rem / input.Width;
            int w = rem % input.Width;
            int outBase = s * Channels;

            for (int c = 0; c < Channels; c++)
            {
                workspace[c] = input[n, h, w, c];
            }

            BatchNormMath.ComputeMeanAndVariance(workspace, out var mean, out var variance);
            _mean[s] = mean;
            _variance[s] = variance;
            var invStd = T.One / T.Sqrt(variance + Epsilon);

            for (int c = 0; c < Channels; c++)
            {
                var norm = (workspace[c] - mean) * invStd;
                _normalizedInput.Values[outBase + c] = norm;
                output.Values[outBase + c] = norm * _scale.Value[0, 0, c] + _shift.Value[0, 0, c];
            }
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        if (_normalizedInput is null || _mean is null || _variance is null)
        {
            throw new InvalidOperationException("Forward must be called before Backward.");
        }

        int spatial = gradOutput.Batch * gradOutput.Height * gradOutput.Width;
        var gradInput = new BatchTensor<T>(gradOutput.Batch, gradOutput.Height, gradOutput.Width, gradOutput.Channels);
        var countT = T.CreateTruncating(Channels);

        for (int s = 0; s < spatial; s++)
        {
            int baseIndex = s * Channels;
            var variance = _variance[s];
            var invStd = T.One / T.Sqrt(variance + Epsilon);

            T gradNormSum = T.Zero;
            T gradNormDot = T.Zero;

            for (int c = 0; c < Channels; c++)
            {
                var gradOut = gradOutput.Values[baseIndex + c];
                _shift.Gradient[0, 0, c] += gradOut;
                _scale.Gradient[0, 0, c] += gradOut * _normalizedInput.Values[baseIndex + c];
            }

            for (int c = 0; c < Channels; c++)
            {
                var scale = _scale.Value[0, 0, c];
                var gradNorm = gradOutput.Values[baseIndex + c] * scale;
                var norm = _normalizedInput.Values[baseIndex + c];
                gradNormSum += gradNorm;
                gradNormDot += gradNorm * norm;
            }

            for (int c = 0; c < Channels; c++)
            {
                var scale = _scale.Value[0, 0, c];
                var gradNorm = gradOutput.Values[baseIndex + c] * scale;
                var norm = _normalizedInput.Values[baseIndex + c];
                gradInput.Values[baseIndex + c] = invStd / countT * (countT * gradNorm - gradNormSum - norm * gradNormDot);
            }

        }

        return gradInput;
    }
}
