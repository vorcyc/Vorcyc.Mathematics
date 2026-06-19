using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// A unified scalar regressor interface.
/// </summary>
public interface IRegressor<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>Fits the regression model.</summary>
    void Fit(T[,] x, T[] y);

    /// <summary>Predicts a continuous target value.</summary>
    T Predict(T[] sample);
}
