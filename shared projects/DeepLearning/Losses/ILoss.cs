namespace Vorcyc.Mathematics.DeepLearning.Losses;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// A differentiable loss function.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface ILoss<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Computes the scalar loss value.
    /// </summary>
    T Compute(Tensor<T> prediction, Tensor<T> target);

    /// <summary>
    /// Returns the gradient of the loss with respect to the prediction tensor.
    /// </summary>
    Tensor<T> Backward(Tensor<T> prediction, Tensor<T> target);
}
