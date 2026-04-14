using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// 特征预处理器接口。
/// </summary>
public interface IPreprocessor<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>估计预处理参数。</summary>
    void Fit(T[,] x);

    /// <summary>变换矩阵。</summary>
    T[,] Transform(T[,] x);

    /// <summary>变换单向量。</summary>
    T[] Transform(T[] x);
}
