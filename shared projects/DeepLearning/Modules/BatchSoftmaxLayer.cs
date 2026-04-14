namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Softmax over the channel dimension for each NHWC position.
/// </summary>
public sealed class BatchSoftmaxLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    public BatchSoftmaxLayer(string? name = null) : base(name) { }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape) => inputShape;

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var output = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        int spatial = input.Height * input.Width;

        for (int n = 0; n < input.Batch; n++)
        {
            for (int s = 0; s < spatial; s++)
            {
                int h = s / input.Width;
                int w = s % input.Width;
                int baseIndex = ((n * input.Height + h) * input.Width + w) * input.Channels;

                T max = input.Values[baseIndex];
                for (int c = 1; c < input.Channels; c++)
                {
                    max = T.Max(max, input.Values[baseIndex + c]);
                }

                T sum = T.Zero;
                for (int c = 0; c < input.Channels; c++)
                {
                    var exp = T.Exp(input.Values[baseIndex + c] - max);
                    output.Values[baseIndex + c] = exp;
                    sum += exp;
                }

                for (int c = 0; c < input.Channels; c++)
                {
                    output.Values[baseIndex + c] /= sum;
                }
            }
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        var output = CachedOutput!;
        var gradInput = new BatchTensor<T>(output.Batch, output.Height, output.Width, output.Channels);
        int spatial = output.Height * output.Width;

        for (int n = 0; n < output.Batch; n++)
        {
            for (int s = 0; s < spatial; s++)
            {
                int h = s / output.Width;
                int w = s % output.Width;
                int baseIndex = ((n * output.Height + h) * output.Width + w) * output.Channels;

                for (int c = 0; c < output.Channels; c++)
                {
                    T sum = T.Zero;
                    var yi = output.Values[baseIndex + c];
                    for (int j = 0; j < output.Channels; j++)
                    {
                        var yj = output.Values[baseIndex + j];
                        var delta = c == j ? T.One : T.Zero;
                        sum += gradOutput.Values[baseIndex + j] * yi * (delta - yj);
                    }

                    gradInput.Values[baseIndex + c] = sum;
                }
            }
        }

        return gradInput;
    }
}
