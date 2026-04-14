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
        int spatial = input.Height * input.Width;
        var scale = T.One / T.CreateTruncating(spatial);

        for (int n = 0; n < input.Batch; n++)
        {
            for (int c = 0; c < input.Channels; c++)
            {
                T sum = T.Zero;
                for (int h = 0; h < input.Height; h++)
                {
                    for (int w = 0; w < input.Width; w++)
                    {
                        sum += input[n, h, w, c];
                    }
                }

                output[n, 0, 0, c] = sum * scale;
            }
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        int spatial = _inputShape.Height * _inputShape.Width;
        var scale = T.One / T.CreateTruncating(spatial);
        var gradInput = new BatchTensor<T>(_inputShape.Batch, _inputShape.Height, _inputShape.Width, _inputShape.Channels);

        for (int n = 0; n < _inputShape.Batch; n++)
        {
            for (int c = 0; c < _inputShape.Channels; c++)
            {
                var grad = gradOutput[n, 0, 0, c] * scale;
                for (int h = 0; h < _inputShape.Height; h++)
                {
                    for (int w = 0; w < _inputShape.Width; w++)
                    {
                        gradInput[n, h, w, c] = grad;
                    }
                }
            }
        }

        return gradInput;
    }
}
