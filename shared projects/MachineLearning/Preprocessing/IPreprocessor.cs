using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// Feature preprocessor interface.
/// </summary>
public interface IPreprocessor<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>Estimates the preprocessing parameters.</summary>
    void Fit(T[,] x);

    /// <summary>Transforms a matrix.</summary>
    T[,] Transform(T[,] x);

    /// <summary>Transforms a single vector.</summary>
    T[] Transform(T[] x);
}
