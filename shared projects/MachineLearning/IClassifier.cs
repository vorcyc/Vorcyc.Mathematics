using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 统一的整数标签分类器接口。
/// </summary>
public interface IClassifier<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>拟合分类模型。</summary>
    void Fit(T[,] x, int[] y);

    /// <summary>预测类别。</summary>
    int Predict(T[] sample);
}
