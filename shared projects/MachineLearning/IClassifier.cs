using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// A unified integer-label classifier interface.
/// </summary>
public interface IClassifier<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>Fits the classification model.</summary>
    void Fit(T[,] x, int[] y);

    /// <summary>Predicts the class.</summary>
    int Predict(T[] sample);
}
