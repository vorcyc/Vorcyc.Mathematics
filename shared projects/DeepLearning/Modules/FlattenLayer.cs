namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Flattens a W×H×D tensor into a 1×1×(W·H·D) vector.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class FlattenLayer<T> : LayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private TensorShape _inputShape;

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override TensorShape GetOutputShape(TensorShape inputShape)
        => new(1, 1, inputShape.ElementCount);

    /// <inheritdoc/>
    public override Tensor<T> Forward(Tensor<T> input, bool training = true)
    {
        _inputShape = TensorShape.From(input);
        var output = new Tensor<T>(1, 1, _inputShape.ElementCount);
        input.Values.CopyTo(output.Values);
        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override Tensor<T> Backward(Tensor<T> gradOutput)
    {
        EnsureCached();
        if (gradOutput.Width != 1 || gradOutput.Height != 1 || gradOutput.Depth != _inputShape.ElementCount)
        {
            throw new ArgumentException("Gradient shape does not match flattened output.", nameof(gradOutput));
        }

        var gradInput = new Tensor<T>(_inputShape.Width, _inputShape.Height, _inputShape.Depth);
        gradOutput.Values.CopyTo(gradInput.Values);
        return gradInput;
    }
}
