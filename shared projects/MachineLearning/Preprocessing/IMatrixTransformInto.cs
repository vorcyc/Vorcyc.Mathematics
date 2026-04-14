using System.Numerics;



namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;



/// <summary>

/// 支持将矩阵变换写入外部缓冲，供流水线推理减少中间分配。

/// </summary>

public interface IMatrixTransformInto<T> : IPreprocessor<T>

    where T : struct, IFloatingPointIeee754<T>

{

    /// <summary>

    /// 将 <paramref name="source"/> 变换后写入 <paramref name="destination"/>（形状须一致）。

    /// </summary>

    void TransformInto(T[,] source, T[,] destination);

}


