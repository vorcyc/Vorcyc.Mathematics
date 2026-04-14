namespace Vorcyc.Mathematics.DeepLearning.Optimizers;

using System.Numerics;

/// <summary>
/// A gradient-based optimizer for trainable parameters.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IOptimizer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Applies one optimization step.
    /// </summary>
    void Step(IEnumerable<Parameter<T>> parameters);

    /// <summary>
    /// Resets all parameter gradients to zero.
    /// </summary>
    void ZeroGrad(IEnumerable<Parameter<T>> parameters);

    /// <summary>
    /// Updates the optimizer learning rate.
    /// </summary>
    void SetLearningRate(T learningRate);
}
