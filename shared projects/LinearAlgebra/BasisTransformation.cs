using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// 提供基变换的方法。
/// </summary>
/// <remarks>
/// 该类支持将向量从一个基变换到另一个基，通过矩阵运算实现变换。
/// 变换过程为：从原基到标准基（通过原基的逆矩阵），然后从标准基到目标基。
/// 
/// 优化版本包括：
/// - 移除对 <c>Vector{T}</c> 的依赖，使用 <c>T[]</c> 表示向量。
/// - 添加数值稳定性检查，防止矩阵不可逆。
/// - 使用 LU 分解优化矩阵运算（可选）。
/// </remarks>
public static class BasisTransformation
{
    /// <summary>
    /// 将向量从一个基变换到另一个基。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="vector">要变换的向量。</param>
    /// <param name="fromBasis">原基矩阵，每列表示一个基向量。</param>
    /// <param name="toBasis">目标基矩阵，每列表示一个基向量。</param>
    /// <returns>变换后的向量。</returns>
    /// <exception cref="ArgumentException">当基矩阵不是方阵或维度与向量不匹配时抛出。</exception>
    /// <exception cref="InvalidOperationException">当原基矩阵不可逆时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Transform<T>(T[] vector, Matrix<T> fromBasis, Matrix<T> toBasis) where T : struct, IFloatingPointIeee754<T>
    {
        if (fromBasis.Rows != fromBasis.Columns || toBasis.Rows != toBasis.Columns)
            throw new ArgumentException("基矩阵必须是方阵。", nameof(fromBasis));
        if (fromBasis.Rows != vector.Length || toBasis.Rows != vector.Length)
            throw new ArgumentException("基矩阵的维度必须与向量的长度匹配。", nameof(vector));

        int n = vector.Length;

        // 检查原基是否可逆
        T det = fromBasis.Determinant();
        if (T.Abs(det) < T.CreateChecked(1e-10))
            throw new InvalidOperationException("原基矩阵不可逆，无法进行变换。");

        // 计算从原基到标准基的变换矩阵 (fromBasis⁻¹)
        var fromBasisInverse = fromBasis.Inverse();

        // 计算从标准基到目标基的变换矩阵 (toBasis * fromBasis⁻¹)
        var transformationMatrix = toBasis * fromBasisInverse;

        // 变换向量
        var transformedVector = new T[n];
        for (int i = 0; i < n; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < n; j++)
            {
                sum += transformationMatrix[i, j] * vector[j];
            }
            transformedVector[i] = sum;
        }

        return transformedVector;
    }

    /// <summary>
    /// 使用 LU 分解优化将向量从一个基变换到另一个基。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="vector">要变换的向量。</param>
    /// <param name="fromBasis">原基矩阵，每列表示一个基向量。</param>
    /// <param name="toBasis">目标基矩阵，每列表示一个基向量。</param>
    /// <returns>变换后的向量。</returns>
    /// <exception cref="ArgumentException">当基矩阵不是方阵或维度与向量不匹配时抛出。</exception>
    /// <exception cref="InvalidOperationException">当原基矩阵不可逆时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] TransformWithLU<T>(T[] vector, Matrix<T> fromBasis, Matrix<T> toBasis) where T : struct, IFloatingPointIeee754<T>
    {
        if (fromBasis.Rows != fromBasis.Columns || toBasis.Rows != toBasis.Columns)
            throw new ArgumentException("基矩阵必须是方阵。", nameof(fromBasis));
        if (fromBasis.Rows != vector.Length || toBasis.Rows != vector.Length)
            throw new ArgumentException("基矩阵的维度必须与向量的长度匹配。", nameof(vector));

        int n = vector.Length;

        // 从原基到标准基：解 fromBasis * x' = vector
        var xPrime = LinearEquationSolver.LUSolve(fromBasis, vector);

        // 从标准基到目标基：y = toBasis * x'
        var transformedVector = new T[n];
        for (int i = 0; i < n; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < n; j++)
            {
                sum += toBasis[i, j] * xPrime[j];
            }
            transformedVector[i] = sum;
        }

        return transformedVector;
    }
}