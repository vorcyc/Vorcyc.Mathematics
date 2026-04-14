namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// A differentiable neural network layer.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface ILayer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Gets the display name of the layer.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets trainable parameters owned by this layer.
    /// </summary>
    IReadOnlyList<Parameter<T>> Parameters { get; }

    /// <summary>
    /// Computes the output shape for a given input shape without allocating data.
    /// </summary>
    TensorShape GetOutputShape(TensorShape inputShape);

    /// <summary>
    /// Runs the forward pass.
    /// </summary>
    /// <param name="input">Input tensor.</param>
    /// <param name="training">Whether the model is in training mode.</param>
    Tensor<T> Forward(Tensor<T> input, bool training = true);

    /// <summary>
    /// Runs the backward pass and accumulates gradients into <see cref="Parameter{T}.Gradient"/>.
    /// </summary>
    /// <param name="gradOutput">Gradient with respect to the layer output.</param>
    /// <returns>Gradient with respect to the layer input.</returns>
    Tensor<T> Backward(Tensor<T> gradOutput);
}
