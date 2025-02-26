namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;

/// <summary>
/// 提供求解线性方程组 Ax = b 的方法。
/// </summary>
/// <remarks>
/// 该类包含多种线性方程组求解算法，包括高斯消元法、LU 分解法、Jacobi 迭代法和 Gauss-Seidel 迭代法。
/// 所有方法假设输入矩阵 A 为方阵，且支持泛型浮点数类型。
/// 
/// 优化版本包括：
/// - 移除对 <c>Vector{T}</c> 的依赖，使用 <c>T[]</c> 表示向量。
/// - 添加数值稳定性检查，防止除零。
/// - 使用向量化计算提升性能（可选）。
/// </remarks>
public static class LinearEquationSolver
{
    /// <summary>
    /// 使用高斯消元法（带主元选择）求解线性方程组 Ax = b。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当 <paramref name="A"/> 不是方阵或与 <paramref name="b"/> 维度不匹配时抛出。</exception>
    /// <exception cref="InvalidOperationException">当矩阵不可逆时抛出。</exception>
    public static T[] GaussianEliminationSolve<T>(Matrix<T> A, T[] b) where T : struct, IFloatingPointIeee754<T>
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("矩阵 A 必须是方阵。", nameof(A));
        if (A.Rows != b.Length)
            throw new ArgumentException("矩阵 A 的行数必须与向量 b 的长度匹配。", nameof(b));

        int n = A.Rows;
        var augmentedMatrix = new T[n, n + 1];

        // 构造增广矩阵
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                augmentedMatrix[i, j] = A[i, j];
            augmentedMatrix[i, n] = b[i];
        }

        // 高斯消元（带主元选择）
        for (int i = 0; i < n; i++)
        {
            // 寻找主元
            int maxRow = i;
            for (int k = i + 1; k < n; k++)
            {
                if (T.Abs(augmentedMatrix[k, i]) > T.Abs(augmentedMatrix[maxRow, i]))
                    maxRow = k;
            }

            if (T.Abs(augmentedMatrix[maxRow, i]) < T.CreateChecked(1e-10))
                throw new InvalidOperationException("矩阵不可逆，无法求解。");

            // 交换行
            if (maxRow != i)
            {
                for (int j = 0; j <= n; j++)
                {
                    (augmentedMatrix[maxRow, j], augmentedMatrix[i, j]) = (augmentedMatrix[i, j], augmentedMatrix[maxRow, j]);
                }
            }

            // 消元
            for (int k = i + 1; k < n; k++)
            {
                T factor = augmentedMatrix[k, i] / augmentedMatrix[i, i];
                for (int j = i; j <= n; j++)
                {
                    augmentedMatrix[k, j] -= factor * augmentedMatrix[i, j];
                }
            }
        }

        // 回代求解
        var x = new T[n];
        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = augmentedMatrix[i, n];
            for (int j = i + 1; j < n; j++)
            {
                x[i] -= augmentedMatrix[i, j] * x[j];
            }
            x[i] /= augmentedMatrix[i, i];
        }

        return x;
    }

    /// <summary>
    /// 使用 LU 分解法求解线性方程组 Ax = b。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当 <paramref name="A"/> 不是方阵或与 <paramref name="b"/> 维度不匹配时抛出。</exception>
    /// <exception cref="InvalidOperationException">当矩阵不可逆时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] LUSolve<T>(Matrix<T> A, T[] b) where T : struct, IFloatingPointIeee754<T>
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("矩阵 A 必须是方阵。", nameof(A));
        if (A.Rows != b.Length)
            throw new ArgumentException("矩阵 A 的行数必须与向量 b 的长度匹配。", nameof(b));

        int n = A.Rows;
        A.LUDecomposition(out Matrix<T> L, out Matrix<T> U, out int[] P);

        // 检查矩阵是否可逆
        for (int i = 0; i < n; i++)
        {
            if (T.Abs(U[i, i]) < T.CreateChecked(1e-10))
                throw new InvalidOperationException("矩阵不可逆，无法求解。");
        }

        // 前向代入 Ly = Pb
        var Pb = new T[n];
        for (int i = 0; i < n; i++)
            Pb[i] = b[P[i]];

        var y = new T[n];
        for (int i = 0; i < n; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < i; j++)
                sum += L[i, j] * y[j];
            y[i] = (Pb[i] - sum) / L[i, i];
        }

        // 回代求解 Ux = y
        var x = new T[n];
        for (int i = n - 1; i >= 0; i--)
        {
            T sum = T.Zero;
            for (int j = i + 1; j < n; j++)
                sum += U[i, j] * x[j];
            x[i] = (y[i] - sum) / U[i, i];
        }

        return x;
    }

    /// <summary>
    /// 使用 Jacobi 迭代法求解线性方程组 Ax = b。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <param name="tolerance">收敛容差。</param>
    /// <param name="maxIterations">最大迭代次数，默认值为 1000。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当 <paramref name="A"/> 不是方阵或与 <paramref name="b"/> 维度不匹配时抛出。</exception>
    /// <exception cref="InvalidOperationException">当对角线元素为零或迭代未收敛时抛出。</exception>
    public static T[] JacobiSolve<T>(Matrix<T> A, T[] b, T tolerance, int maxIterations = 1000) where T : struct, IFloatingPointIeee754<T>
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("矩阵 A 必须是方阵。", nameof(A));
        if (A.Rows != b.Length)
            throw new ArgumentException("矩阵 A 的行数必须与向量 b 的长度匹配。", nameof(b));

        int n = A.Rows;
        var x = new T[n];
        var xNew = new T[n];

        // 检查对角线元素
        for (int i = 0; i < n; i++)
        {
            if (T.Abs(A[i, i]) < T.CreateChecked(1e-10))
                throw new InvalidOperationException("对角线元素不能为零或过小，Jacobi 法无法收敛。");
        }

        for (int iter = 0; iter < maxIterations; iter++)
        {
            T maxDiff = T.Zero;
            for (int i = 0; i < n; i++)
            {
                T sum = b[i];
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                        sum -= A[i, j] * x[j];
                }
                xNew[i] = sum / A[i, i];
                maxDiff = T.Max(maxDiff, T.Abs(xNew[i] - x[i]));
            }

            if (maxDiff < tolerance)
                return xNew;

            Array.Copy(xNew, x, n);
        }

        throw new InvalidOperationException($"Jacobi 迭代未能在 {maxIterations} 次迭代内收敛到容差 {tolerance}。");
    }

    /// <summary>
    /// 使用 Gauss-Seidel 迭代法求解线性方程组 Ax = b。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <param name="tolerance">收敛容差。</param>
    /// <param name="maxIterations">最大迭代次数，默认值为 1000。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当 <paramref name="A"/> 不是方阵或与 <paramref name="b"/> 维度不匹配时抛出。</exception>
    /// <exception cref="InvalidOperationException">当对角线元素为零或迭代未收敛时抛出。</exception>
    public static T[] GaussSeidelSolve<T>(Matrix<T> A, T[] b, T tolerance, int maxIterations = 1000) where T : struct, IFloatingPointIeee754<T>
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("矩阵 A 必须是方阵。", nameof(A));
        if (A.Rows != b.Length)
            throw new ArgumentException("矩阵 A 的行数必须与向量 b 的长度匹配。", nameof(b));

        int n = A.Rows;
        var x = new T[n];

        // 检查对角线元素
        for (int i = 0; i < n; i++)
        {
            if (T.Abs(A[i, i]) < T.CreateChecked(1e-10))
                throw new InvalidOperationException("对角线元素不能为零或过小，Gauss-Seidel 法无法收敛。");
        }

        for (int iter = 0; iter < maxIterations; iter++)
        {
            T maxDiff = T.Zero;
            for (int i = 0; i < n; i++)
            {
                T oldX = x[i];
                T sum = b[i];
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                        sum -= A[i, j] * x[j];
                }
                x[i] = sum / A[i, i];
                maxDiff = T.Max(maxDiff, T.Abs(x[i] - oldX));
            }

            if (maxDiff < tolerance)
                return x;
        }

        throw new InvalidOperationException($"Gauss-Seidel 迭代未能在 {maxIterations} 次迭代内收敛到容差 {tolerance}。");
    }
}