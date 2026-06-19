using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// Supports writing a matrix transform into an external buffer to reduce intermediate allocations during pipeline inference.
/// </summary>
public interface IMatrixTransformInto<T> : IPreprocessor<T>
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>
    /// Transforms <paramref name="source"/> and writes the result into <paramref name="destination"/> (shapes must match).
    /// </summary>
    void TransformInto(T[,] source, T[,] destination);
}
