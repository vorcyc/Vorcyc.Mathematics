using System.Numerics;

namespace Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// 提供基变换的方法。
/// </summary>
public class BasisTransformation
{
    /// <summary>
    /// 将向量从一个基变换到另一个基。
    /// </summary>
    /// <param name="vector">要变换的向量。</param>
    /// <param name="fromBasis">原基。</param>
    /// <param name="toBasis">目标基。</param>
    /// <returns>变换后的向量。</returns>
    /// <exception cref="ArgumentException">当基的维度不匹配时抛出。</exception>
    public static Vector Transform(Vector vector, Matrix fromBasis, Matrix toBasis)
    {
        if (fromBasis.Rows != fromBasis.Columns || toBasis.Rows != toBasis.Columns)
            throw new ArgumentException("Basis matrices must be square.");

        if (fromBasis.Rows != vector.Dimension || toBasis.Rows != vector.Dimension)
            throw new ArgumentException("Basis matrices and vector dimensions must match.");

        // 计算从原基到标准基的变换矩阵
        Matrix fromBasisInverse = fromBasis.Inverse();

        // 计算从标准基到目标基的变换矩阵
        Matrix transformationMatrix = toBasis * fromBasisInverse;

        // 变换向量
        Vector transformedVector = transformationMatrix * vector;

        return transformedVector;
    }

    /// <summary>
    /// 将向量从一个基变换到另一个基。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="vector">要变换的向量。</param>
    /// <param name="fromBasis">原基。</param>
    /// <param name="toBasis">目标基。</param>
    /// <returns>变换后的向量。</returns>
    /// <exception cref="ArgumentException">当基的维度不匹配时抛出。</exception>
    public static Vector<T> Transform<T>(Vector<T> vector, Matrix<T> fromBasis, Matrix<T> toBasis) where T : IFloatingPointIeee754<T>
    {
        if (fromBasis.Rows != fromBasis.Columns || toBasis.Rows != toBasis.Columns)
            throw new ArgumentException("Basis matrices must be square.");

        if (fromBasis.Rows != vector.Dimension || toBasis.Rows != vector.Dimension)
            throw new ArgumentException("Basis matrices and vector dimensions must match.");

        // 计算从原基到标准基的变换矩阵
        Matrix<T> fromBasisInverse = fromBasis.Inverse();

        // 计算从标准基到目标基的变换矩阵
        Matrix<T> transformationMatrix = toBasis * fromBasisInverse;

        // 变换向量
        Vector<T> transformedVector = transformationMatrix * vector;

        return transformedVector;
    }
}
