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
        var x = new T[b.Length];
        GaussianEliminationSolve(A, b, x);
        return x;
    }

    /// <summary>
    /// 高斯消元求解 Ax=b，结果写入 <paramref name="x"/>；可选复用 <paramref name="augmentedWorkspace"/>（尺寸 n×(n+1)）。
    /// </summary>
    public static void GaussianEliminationSolve<T>(
        Matrix<T> A,
        ReadOnlySpan<T> b,
        Span<T> x,
        T[,]? augmentedWorkspace = null) where T : struct, IFloatingPointIeee754<T>
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("矩阵 A 必须是方阵。", nameof(A));
        if (A.Rows != b.Length)
            throw new ArgumentException("矩阵 A 的行数必须与向量 b 的长度匹配。", nameof(b));
        if (x.Length < b.Length)
            throw new ArgumentException("解向量缓冲区长度不足", nameof(x));

        int n = A.Rows;
        T[,] augmented = augmentedWorkspace is not null
                          && augmentedWorkspace.GetLength(0) == n
                          && augmentedWorkspace.GetLength(1) == n + 1
            ? augmentedWorkspace
            : new T[n, n + 1];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                augmented[i, j] = A[i, j];
            augmented[i, n] = b[i];
        }

        for (int i = 0; i < n; i++)
        {
            int maxRow = i;
            for (int k = i + 1; k < n; k++)
            {
                if (T.Abs(augmented[k, i]) > T.Abs(augmented[maxRow, i]))
                    maxRow = k;
            }

            if (T.Abs(augmented[maxRow, i]) < T.CreateChecked(1e-10))
                throw new InvalidOperationException("矩阵不可逆，无法求解。");

            if (maxRow != i)
            {
                for (int j = 0; j <= n; j++)
                    (augmented[maxRow, j], augmented[i, j]) = (augmented[i, j], augmented[maxRow, j]);
            }

            for (int k = i + 1; k < n; k++)
            {
                T factor = augmented[k, i] / augmented[i, i];
                for (int j = i; j <= n; j++)
                    augmented[k, j] -= factor * augmented[i, j];
            }
        }

        for (int i = n - 1; i >= 0; i--)
        {
            T xi = augmented[i, n];
            for (int j = i + 1; j < n; j++)
                xi -= augmented[i, j] * x[j];
            x[i] = xi / augmented[i, i];
        }
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
    /// 使用 Cholesky 分解求解对称正定方程组 Ax = b（A = LLᵀ）。
    /// </summary>
    public static T[] CholeskySolve<T>(Matrix<T> A, T[] b)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("矩阵 A 必须是方阵。", nameof(A));
        if (A.Rows != b.Length)
            throw new ArgumentException("矩阵 A 的行数必须与向量 b 的长度匹配。", nameof(b));

        int n = A.Rows;
        Matrix<T> L = A.CholeskyDecomposition();

        var y = new T[n];
        for (int i = 0; i < n; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < i; j++)
                sum += L[i, j] * y[j];
            y[i] = (b[i] - sum) / L[i, i];
        }

        var x = new T[n];
        for (int i = n - 1; i >= 0; i--)
        {
            T sum = T.Zero;
            for (int j = i + 1; j < n; j++)
                sum += L[j, i] * x[j];
            x[i] = (y[i] - sum) / L[i, i];
        }

        return x;
    }

    /// <summary>
    /// 求解超定或适定最小二乘问题 min ‖Ax - y‖²（法方程 + Cholesky/LU）。
    /// </summary>
    /// <param name="designMatrix">设计矩阵 A (n×p)。</param>
    /// <param name="y">观测向量 (n)。</param>
    public static T[] SolveLeastSquares<T>(Matrix<T> designMatrix, ReadOnlySpan<T> y)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (y.Length != designMatrix.Rows)
            throw new ArgumentException("观测向量长度必须与矩阵行数一致。", nameof(y));

        var xt = designMatrix.Transpose();
        var xtx = xt * designMatrix;
        int p = designMatrix.Columns;
        var xty = new T[p];
        for (int j = 0; j < p; j++)
        {
            T sum = T.Zero;
            for (int i = 0; i < y.Length; i++)
                sum += xt[j, i] * y[i];
            xty[j] = sum;
        }

        return SolveNormalEquations(designMatrix, xtx, xty, y);
    }

    /// <summary>
    /// 求解岭回归最小二乘 min ‖Ax - y‖² + λ‖β‖²。
    /// </summary>
    public static T[] SolveRidgeLeastSquares<T>(
        Matrix<T> designMatrix,
        ReadOnlySpan<T> y,
        T lambda,
        bool regularizeIntercept = true)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (y.Length != designMatrix.Rows)
            throw new ArgumentException("观测向量长度必须与矩阵行数一致。", nameof(y));

        var xt = designMatrix.Transpose();
        var xtx = xt * designMatrix;
        int p = designMatrix.Columns;
        int start = regularizeIntercept ? 0 : 1;
        for (int i = start; i < p; i++)
            xtx[i, i] += lambda;

        var xty = new T[p];
        for (int j = 0; j < p; j++)
        {
            T sum = T.Zero;
            for (int i = 0; i < y.Length; i++)
                sum += xt[j, i] * y[i];
            xty[j] = sum;
        }

        return SolveNormalEquations(designMatrix, xtx, xty, y);
    }

    private static T[] SolveNormalEquations<T>(
        Matrix<T> designMatrix,
        Matrix<T> normalMatrix,
        T[] normalRhs,
        ReadOnlySpan<T> observations)
        where T : struct, IFloatingPointIeee754<T>
    {
        T threshold = T.CreateChecked(1e10);
        if (MatrixDiagnostics.IsIllConditioned(designMatrix, threshold))
            return SolveLeastSquaresSvd(designMatrix, observations);

        try
        {
            return CholeskySolve(normalMatrix, normalRhs);
        }
        catch (InvalidOperationException)
        {
            try
            {
                return LUSolve(normalMatrix, normalRhs);
            }
            catch (InvalidOperationException)
            {
                return SolveLeastSquaresSvd(designMatrix, observations);
            }
        }
    }

    /// <summary>
    /// 使用薄 SVD 伪逆求解最小二乘问题 min ‖Ax - y‖²，适用于秩亏或病态设计矩阵。
    /// </summary>
    public static T[] SolveLeastSquaresSvd<T>(
        Matrix<T> designMatrix,
        ReadOnlySpan<T> y,
        T? tolerance = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (y.Length != designMatrix.Rows)
            throw new ArgumentException("观测向量长度必须与矩阵行数一致。", nameof(y));

        var pseudoinverse = MatrixDecomposition.Pseudoinverse(designMatrix, tolerance);
        return pseudoinverse.Multiply(y);
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