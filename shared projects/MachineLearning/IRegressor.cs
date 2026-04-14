using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 统一的标量回归器接口。
/// </summary>
public interface IRegressor<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>拟合回归模型。</summary>
    void Fit(T[,] x, T[] y);

    /// <summary>预测连续目标值。</summary>
    T Predict(T[] sample);
}
