namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// 2×2 average pooling with stride 2 on NHWC tensors.
/// </summary>
public sealed class BatchAvgPool2DLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private BatchShape _inputShape;

    public BatchAvgPool2DLayer(string? name = null) : base(name) { }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
        => new(inputShape.Batch, inputShape.Height / 2, inputShape.Width / 2, inputShape.Channels);

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        _inputShape = input.Shape;
        var output = new BatchTensor<T>(input.Batch, input.Height / 2, input.Width / 2, input.Channels);
        int inHeight = input.Height, inWidth = input.Width, channels = input.Channels;
        int outHeight = output.Height, outWidth = output.Width;

        ComputingContextExecution.ForEach(null, 0, input.Batch, n =>
        {
            for (int c = 0; c < channels; c++)
            {
                for (int ay = 0; ay < outHeight; ay++)
                {
                    var y = 2 * ay;
                    for (int ax = 0; ax < outWidth; ax++)
                    {
                        var x = 2 * ax;
                        T sum = T.Zero;
                        int count = 0;
                        for (int fy = 0; fy < 2; fy++)
                        {
                            for (int fx = 0; fx < 2; fx++)
                            {
                                var oy = y + fy;
                                var ox = x + fx;
                                if (oy < inHeight && ox < inWidth)
                                {
                                    sum += input[n, oy, ox, c];
                                    count++;
                                }
                            }
                        }

                        output[n, ay, ax, c] = sum / T.CreateTruncating(count);
                    }
                }
            }
        }, (long)outHeight * outWidth * channels * 4);

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        var gradInput = new BatchTensor<T>(_inputShape.Batch, _inputShape.Height, _inputShape.Width, _inputShape.Channels);
        int inHeight = _inputShape.Height, inWidth = _inputShape.Width, channels = _inputShape.Channels;
        int outHeight = gradOutput.Height, outWidth = gradOutput.Width;

        ComputingContextExecution.ForEach(null, 0, _inputShape.Batch, n =>
        {
            for (int c = 0; c < channels; c++)
            {
                for (int ay = 0; ay < outHeight; ay++)
                {
                    var y = 2 * ay;
                    for (int ax = 0; ax < outWidth; ax++)
                    {
                        var x = 2 * ax;
                        int count = 0;
                        for (int fy = 0; fy < 2; fy++)
                        {
                            for (int fx = 0; fx < 2; fx++)
                            {
                                var oy = y + fy;
                                var ox = x + fx;
                                if (oy < inHeight && ox < inWidth)
                                {
                                    count++;
                                }
                            }
                        }

                        var grad = gradOutput[n, ay, ax, c] / T.CreateTruncating(count);
                        for (int fy = 0; fy < 2; fy++)
                        {
                            for (int fx = 0; fx < 2; fx++)
                            {
                                var oy = y + fy;
                                var ox = x + fx;
                                if (oy < inHeight && ox < inWidth)
                                {
                                    gradInput[n, oy, ox, c] += grad;
                                }
                            }
                        }
                    }
                }
            }
        }, (long)outHeight * outWidth * channels * 4);

        return gradInput;
    }
}
