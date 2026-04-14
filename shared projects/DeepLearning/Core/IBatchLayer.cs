namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;

/// <summary>
/// A differentiable layer that operates on <see cref="BatchTensor{T}"/> inputs in NHWC layout.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IBatchLayer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>Gets the layer name.</summary>
    string Name { get; }

    /// <summary>Gets trainable parameters.</summary>
    IReadOnlyList<Parameter<T>> Parameters { get; }

    /// <summary>Computes the output shape.</summary>
    BatchShape GetOutputShape(BatchShape inputShape);

    /// <summary>Runs the forward pass.</summary>
    BatchTensor<T> Forward(BatchTensor<T> input, bool training = true);

    /// <summary>Runs the backward pass.</summary>
    BatchTensor<T> Backward(BatchTensor<T> gradOutput);
}
