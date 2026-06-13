namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Global average pooling: N×H×W×C → N×1×1×C.
/// </summary>
public sealed class BatchGlobalAveragePool2DLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private BatchShape _inputShape;

    public BatchGlobalAveragePool2DLayer(string? name = null) : base(name) { }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
        => new(inputShape.Batch, 1, 1, inputShape.Channels);

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        _inputShape = input.Shape;
        var output = new BatchTensor<T>(input.Batch, 1, 1, input.Channels);
        int height = input.Height, width = input.Width, channels = input.Channels;
        int spatial = height * width;
        var scale = T.One / T.CreateTruncating(spatial);

        ComputingContextExecution.ForEach(null, 0, input.Batch, n =>
        {
            for (int c = 0; c < channels; c++)
            {
                T sum = T.Zero;
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        sum += input[n, h, w, c];
                    }
                }

                output[n, 0, 0, c] = sum * scale;
            }
        }, spatial * channels);

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        int height = _inputShape.Height, width = _inputShape.Width, channels = _inputShape.Channels;
        int spatial = height * width;
        var scale = T.One / T.CreateTruncating(spatial);
        var gradInput = new BatchTensor<T>(_inputShape.Batch, height, width, channels);

        ComputingContextExecution.ForEach(null, 0, _inputShape.Batch, n =>
        {
            for (int c = 0; c < channels; c++)
            {
                var grad = gradOutput[n, 0, 0, c] * scale;
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        gradInput[n, h, w, c] = grad;
                    }
                }
            }
        }, spatial * channels);

        return gradInput;
    }
}
