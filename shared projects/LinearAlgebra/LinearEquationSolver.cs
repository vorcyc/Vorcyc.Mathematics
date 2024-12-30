using System.Numerics;

namespace Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// 提供求解线性方程组的方法。
/// </summary>
public static class LinearEquationSolver
{
    /// <summary>
    /// 使用高斯消元法求解线性方程组 Ax = b。
    /// </summary>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。</exception>
    public static Vector GaussianEliminationSolve(Matrix A, Vector b)
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("Matrix A must be square.");

        if (A.Rows != b.Dimension)
            throw new ArgumentException("Matrix A and vector b dimensions do not match.");

        int n = A.Rows;
        float[][] augmentedMatrix = new float[n][];

        // 构造增广矩阵
        for (int i = 0; i < n; i++)
        {
            augmentedMatrix[i] = new float[n + 1];
            for (int j = 0; j < n; j++)
            {
                augmentedMatrix[i][j] = A[i][j];
            }
            augmentedMatrix[i][n] = b.Elements[i];
        }

        // 执行高斯消元法
        for (int i = 0; i < n; i++)
        {
            // 找到主元行
            int maxRow = i;
            for (int k = i + 1; k < n; k++)
            {
                if (Math.Abs(augmentedMatrix[k][i]) > Math.Abs(augmentedMatrix[maxRow][i]))
                {
                    maxRow = k;
                }
            }

            // 交换主元行和当前行
            float[] temp = augmentedMatrix[maxRow];
            augmentedMatrix[maxRow] = augmentedMatrix[i];
            augmentedMatrix[i] = temp;

            // 将当前列的所有行变为0
            for (int k = i + 1; k < n; k++)
            {
                float factor = augmentedMatrix[k][i] / augmentedMatrix[i][i];
                for (int j = i; j <= n; j++)
                {
                    augmentedMatrix[k][j] -= factor * augmentedMatrix[i][j];
                }
            }
        }

        // 回代求解
        float[] x = new float[n];
        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = augmentedMatrix[i][n] / augmentedMatrix[i][i];
            for (int k = i - 1; k >= 0; k--)
            {
                augmentedMatrix[k][n] -= augmentedMatrix[k][i] * x[i];
            }
        }

        return new Vector(x);
    }

    /// <summary>
    /// 使用高斯消元法求解线性方程组 Ax = b。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。</exception>
    public static Vector<T> GaussianEliminationSolve<T>(Matrix<T> A, Vector<T> b) where T : IFloatingPointIeee754<T>
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("Matrix A must be square.");

        if (A.Rows != b.Dimension)
            throw new ArgumentException("Matrix A and vector b dimensions do not match.");

        int n = A.Rows;
        T[,] augmentedMatrix = new T[n, n + 1];

        // 构造增广矩阵
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                augmentedMatrix[i, j] = A[i, j];
            }
            augmentedMatrix[i, n] = b.Elements[i];
        }

        // 执行高斯消元法
        for (int i = 0; i < n; i++)
        {
            // 找到主元行
            int maxRow = i;
            for (int k = i + 1; k < n; k++)
            {
                if (T.Abs(augmentedMatrix[k, i]) > T.Abs(augmentedMatrix[maxRow, i]))
                {
                    maxRow = k;
                }
            }

            // 交换主元行和当前行
            for (int j = 0; j <= n; j++)
            {
                T temp = augmentedMatrix[maxRow, j];
                augmentedMatrix[maxRow, j] = augmentedMatrix[i, j];
                augmentedMatrix[i, j] = temp;
            }

            // 将当前列的所有行变为0
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
        T[] x = new T[n];
        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = augmentedMatrix[i, n] / augmentedMatrix[i, i];
            for (int k = i - 1; k >= 0; k--)
            {
                augmentedMatrix[k, n] -= augmentedMatrix[k, i] * x[i];
            }
        }

        return new Vector<T>(x);
    }

    /// <summary>
    /// 使用 LU 分解法求解线性方程组 Ax = b。
    /// </summary>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。</exception>
    public static Vector LUSolve(Matrix A, Vector b)
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("Matrix A must be square.");

        if (A.Rows != b.Dimension)
            throw new ArgumentException("Matrix A and vector b dimensions do not match.");

        int n = A.Rows;
        Matrix L, U;
        A.LUDecomposition(out L, out U);

        // 前向替换求解 Ly = b
        float[] y = new float[n];
        for (int i = 0; i < n; i++)
        {
            y[i] = b.Elements[i];
            for (int j = 0; j < i; j++)
            {
                y[i] -= L[i][j] * y[j];
            }
            y[i] /= L[i][i];
        }

        // 回代求解 Ux = y
        float[] x = new float[n];
        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = y[i];
            for (int j = i + 1; j < n; j++)
            {
                x[i] -= U[i][j] * x[j];
            }
            x[i] /= U[i][i];
        }

        return new Vector(x);
    }

    /// <summary>
    /// 使用 LU 分解法求解线性方程组 Ax = b。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。</exception>
    public static Vector<T> LUSolve<T>(Matrix<T> A, Vector<T> b) where T : IFloatingPointIeee754<T>
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("Matrix A must be square.");

        if (A.Rows != b.Dimension)
            throw new ArgumentException("Matrix A and vector b dimensions do not match.");

        int n = A.Rows;
        Matrix<T> L, U;
        A.LUDecomposition(out L, out U);

        // 前向替换求解 Ly = b
        T[] y = new T[n];
        for (int i = 0; i < n; i++)
        {
            y[i] = b.Elements[i];
            for (int j = 0; j < i; j++)
            {
                y[i] -= L[i, j] * y[j];
            }
            y[i] /= L[i, i];
        }

        // 回代求解 Ux = y
        T[] x = new T[n];
        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = y[i];
            for (int j = i + 1; j < n; j++)
            {
                x[i] -= U[i, j] * x[j];
            }
            x[i] /= U[i, i];
        }

        return new Vector<T>(x);
    }

    /// <summary>
    /// 使用 Jacobi 法求解线性方程组 Ax = b。
    /// </summary>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <param name="tolerance">收敛容差。</param>
    /// <param name="maxIterations">最大迭代次数。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。</exception>
    public static Vector JacobiSolve(Matrix A, Vector b, float tolerance = 1e-10f, int maxIterations = 1000)
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("Matrix A must be square.");

        if (A.Rows != b.Dimension)
            throw new ArgumentException("Matrix A and vector b dimensions do not match.");

        int n = A.Rows;
        float[] x = new float[n];
        float[] xNew = new float[n];

        for (int iter = 0; iter < maxIterations; iter++)
        {
            for (int i = 0; i < n; i++)
            {
                float sum = b.Elements[i];
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        sum -= A[i][j] * x[j];
                    }
                }
                xNew[i] = sum / A[i][i];
            }

            // 检查收敛性
            float maxDiff = 0;
            for (int i = 0; i < n; i++)
            {
                maxDiff = Math.Max(maxDiff, Math.Abs(xNew[i] - x[i]));
            }

            if (maxDiff < tolerance)
            {
                break;
            }

            Array.Copy(xNew, x, n);
        }

        return new Vector(x);
    }

    /// <summary>
    /// 使用 Jacobi 法求解线性方程组 Ax = b。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <param name="tolerance">收敛容差。</param>
    /// <param name="maxIterations">最大迭代次数。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。</exception>
    public static Vector<T> JacobiSolve<T>(Matrix<T> A, Vector<T> b, T tolerance, int maxIterations = 1000) where T : IFloatingPointIeee754<T>
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("Matrix A must be square.");

        if (A.Rows != b.Dimension)
            throw new ArgumentException("Matrix A and vector b dimensions do not match.");

        int n = A.Rows;
        T[] x = new T[n];
        T[] xNew = new T[n];

        for (int iter = 0; iter < maxIterations; iter++)
        {
            for (int i = 0; i < n; i++)
            {
                T sum = b.Elements[i];
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        sum -= A[i, j] * x[j];
                    }
                }
                xNew[i] = sum / A[i, i];
            }

            // 检查收敛性
            T maxDiff = T.Zero;
            for (int i = 0; i < n; i++)
            {
                maxDiff = T.Max(maxDiff, T.Abs(xNew[i] - x[i]));
            }

            if (maxDiff < tolerance)
            {
                break;
            }

            Array.Copy(xNew, x, n);
        }

        return new Vector<T>(x);
    }

    /// <summary>
    /// 使用 Gauss-Seidel 法求解线性方程组 Ax = b。
    /// </summary>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <param name="tolerance">收敛容差。</param>
    /// <param name="maxIterations">最大迭代次数。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。</exception>
    public static Vector GaussSeidelSolve(Matrix A, Vector b, float tolerance = 1e-10f, int maxIterations = 1000)
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("Matrix A must be square.");

        if (A.Rows != b.Dimension)
            throw new ArgumentException("Matrix A and vector b dimensions do not match.");

        int n = A.Rows;
        float[] x = new float[n];

        for (int iter = 0; iter < maxIterations; iter++)
        {
            float maxDiff = 0;
            for (int i = 0; i < n; i++)
            {
                float sum = b.Elements[i];
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        sum -= A[i][j] * x[j];
                    }
                }
                float xNew = sum / A[i][i];
                maxDiff = Math.Max(maxDiff, Math.Abs(xNew - x[i]));
                x[i] = xNew;
            }

            if (maxDiff < tolerance)
            {
                break;
            }
        }

        return new Vector(x);
    }

    /// <summary>
    /// 使用 Gauss-Seidel 法求解线性方程组 Ax = b。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="A">系数矩阵 A。</param>
    /// <param name="b">常数向量 b。</param>
    /// <param name="tolerance">收敛容差。</param>
    /// <param name="maxIterations">最大迭代次数。</param>
    /// <returns>解向量 x。</returns>
    /// <exception cref="ArgumentException">当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。</exception>
    public static Vector<T> GaussSeidelSolve<T>(Matrix<T> A, Vector<T> b, T tolerance, int maxIterations = 1000) where T : IFloatingPointIeee754<T>
    {
        if (A.Rows != A.Columns)
            throw new ArgumentException("Matrix A must be square.");

        if (A.Rows != b.Dimension)
            throw new ArgumentException("Matrix A and vector b dimensions do not match.");

        int n = A.Rows;
        T[] x = new T[n];

        for (int iter = 0; iter < maxIterations; iter++)
        {
            T maxDiff = T.Zero;
            for (int i = 0; i < n; i++)
            {
                T sum = b.Elements[i];
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        sum -= A[i, j] * x[j];
                    }
                }
                T xNew = sum / A[i, i];
                maxDiff = T.Max(maxDiff, T.Abs(xNew - x[i]));
                x[i] = xNew;
            }

            if (maxDiff < tolerance)
            {
                break;
            }
        }

        return new Vector<T>(x);
    }
}
