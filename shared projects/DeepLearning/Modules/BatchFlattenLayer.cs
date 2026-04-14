namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Flattens N×H×W×C into N×1×1×(H·W·C).
/// </summary>
public sealed class BatchFlattenLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private BatchShape _inputShape;

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
        => BatchShape.Vector(inputShape.Batch, inputShape.Height * inputShape.Width * inputShape.Channels);

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        _inputShape = input.Shape;
        var output = new BatchTensor<T>(input.Batch, 1, 1, _inputShape.Height * _inputShape.Width * _inputShape.Channels);
        input.Values.CopyTo(output.Values);
        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        var gradInput = new BatchTensor<T>(_inputShape.Batch, _inputShape.Height, _inputShape.Width, _inputShape.Channels);
        gradOutput.Values.CopyTo(gradInput.Values);
        return gradInput;
    }
}
