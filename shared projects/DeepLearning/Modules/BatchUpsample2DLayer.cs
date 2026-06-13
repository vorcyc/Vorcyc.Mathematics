namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// 2× nearest-neighbor upsampling on NHWC tensors.
/// </summary>
public sealed class BatchUpsample2DLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private BatchShape _inputShape;

    public BatchUpsample2DLayer(string? name = null) : base(name) { }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
        => new(inputShape.Batch, inputShape.Height * 2, inputShape.Width * 2, inputShape.Channels);

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        _inputShape = input.Shape;
        var output = new BatchTensor<T>(input.Batch, input.Height * 2, input.Width * 2, input.Channels);

        int height = input.Height, width = input.Width, channels = input.Channels;
        long workPerSample = (long)height * width * channels * 4;
        ComputingContextExecution.ForEach(null, 0, input.Batch, n =>
        {
            for (int c = 0; c < channels; c++)
            {
                for (int ay = 0; ay < height; ay++)
                {
                    var y = 2 * ay;
                    for (int ax = 0; ax < width; ax++)
                    {
                        var x = 2 * ax;
                        var value = input[n, ay, ax, c];
                        for (int fy = 0; fy < 2; fy++)
                        {
                            for (int fx = 0; fx < 2; fx++)
                            {
                                output[n, y + fy, x + fx, c] = value;
                            }
                        }
                    }
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
        var gradInput = new BatchTensor<T>(_inputShape.Batch, _inputShape.Height, _inputShape.Width, _inputShape.Channels);

        int height = _inputShape.Height, width = _inputShape.Width, channels = _inputShape.Channels;
        long workPerSample = (long)height * width * channels * 4;
        ComputingContextExecution.ForEach(null, 0, _inputShape.Batch, n =>
        {
            for (int c = 0; c < channels; c++)
            {
                for (int ay = 0; ay < height; ay++)
                {
                    var y = 2 * ay;
                    for (int ax = 0; ax < width; ax++)
                    {
                        var x = 2 * ax;
                        T sum = T.Zero;
                        for (int fy = 0; fy < 2; fy++)
                        {
                            for (int fx = 0; fx < 2; fx++)
                            {
                                sum += gradOutput[n, y + fy, x + fx, c];
                            }
                        }

                        gradInput[n, ay, ax, c] = sum;
                    }
                }
            }
        }, workPerSample);

        return gradInput;
    }
}
