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
        int height = input.Height, width = input.Width, channels = input.Channels;

        ComputingContextExecution.ForEach(null, 0, input.Batch, n =>
        {
            for (int s = 0; s < spatial; s++)
            {
                int h = s / width;
                int w = s % width;
                int baseIndex = ((n * height + h) * width + w) * channels;

                T max = input.Values[baseIndex];
                for (int c = 1; c < channels; c++)
                {
                    max = T.Max(max, input.Values[baseIndex + c]);
                }

                T sum = T.Zero;
                for (int c = 0; c < channels; c++)
                {
                    var exp = T.Exp(input.Values[baseIndex + c] - max);
                    output.Values[baseIndex + c] = exp;
                    sum += exp;
                }

                for (int c = 0; c < channels; c++)
                {
                    output.Values[baseIndex + c] /= sum;
                }
            }
        }, (long)spatial * channels);

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
        int height = output.Height, width = output.Width, channels = output.Channels;

        ComputingContextExecution.ForEach(null, 0, output.Batch, n =>
        {
            for (int s = 0; s < spatial; s++)
            {
                int h = s / width;
                int w = s % width;
                int baseIndex = ((n * height + h) * width + w) * channels;

                for (int c = 0; c < channels; c++)
                {
                    T sum = T.Zero;
                    var yi = output.Values[baseIndex + c];
                    for (int j = 0; j < channels; j++)
                    {
                        var yj = output.Values[baseIndex + j];
                        var delta = c == j ? T.One : T.Zero;
                        sum += gradOutput.Values[baseIndex + j] * yi * (delta - yj);
                    }

                    gradInput.Values[baseIndex + c] = sum;
                }
            }
        }, (long)spatial * channels * channels);

        return gradInput;
    }
}
