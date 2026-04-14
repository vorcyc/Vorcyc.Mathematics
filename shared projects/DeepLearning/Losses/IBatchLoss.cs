namespace Vorcyc.Mathematics.DeepLearning.Losses;

using System.Numerics;

/// <summary>
/// A differentiable loss function for <see cref="BatchTensor{T}"/> predictions.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IBatchLoss<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>Computes the scalar loss value.</summary>
    T Compute(BatchTensor<T> prediction, BatchTensor<T> target);

    /// <summary>Returns the gradient of the loss with respect to the prediction tensor.</summary>
    BatchTensor<T> Backward(BatchTensor<T> prediction, BatchTensor<T> target);
}
