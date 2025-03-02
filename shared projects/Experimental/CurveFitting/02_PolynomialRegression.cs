namespace Vorcyc.Mathematics.Experimental.CurveFitting;

using System.Numerics;
using System.Runtime.CompilerServices;

internal static class PolynomialRegression
{

    #region Normal

    /// <summary>
    /// 多项式回归：拟合 y = a0 + a1*x + a2*x^2 + ... + an*x^n。
    /// </summary>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    /// <param name="degree">多项式次数。</param>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResult<T> Fit_Normal<T>(Span<T> xData, Span<T> yData, int degree)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (degree < 0)
            throw new ArgumentException("Degree must be non-negative.");
        if (degree >= xData.Length)
            throw new ArgumentException("Degree must be less than the number of data points.");

        int n = xData.Length;
        int m = degree + 1; // 系数数量

        // Step 1: 构造 Vandermonde 矩阵 A (n x m)
        T[,] A = new T[n, m];
        for (int i = 0; i < n; i++)
        {
            A[i, 0] = T.One; // x^0 = 1
            for (int j = 1; j < m; j++)
            {
                A[i, j] = A[i, j - 1] * xData[i]; // x^j = x^(j-1) * x
            }
        }

        // Step 2: 计算 A^T A (m x m) 和 A^T y (m x 1)
        T[,] ATA = new T[m, m];
        T[] ATy = new T[m];

        // 计算 A^T A
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < m; j++)
            {
                T sum = T.Zero;
                for (int k = 0; k < n; k++)
                {
                    sum += A[k, i] * A[k, j];
                }
                ATA[i, j] = sum;
            }
        }

        // 计算 A^T y
        for (int i = 0; i < m; i++)
        {
            T sum = T.Zero;
            for (int k = 0; k < n; k++)
            {
                sum += A[k, i] * yData[k];
            }
            ATy[i] = sum;
        }

        // Step 3: 解线性方程组 ATA * a = ATy
        T[] coeffs = SolveLinearSystem_Normal(ATA, ATy);

        // Step 4: 定义预测函数
        Func<T, T> predict = x =>
        {
            T result = coeffs[0];
            T xPow = x;
            for (int j = 1; j < m; j++)
            {
                result += coeffs[j] * xPow;
                xPow *= x;
            }
            return result;
        };

        // Step 5: 计算均方误差 (MSE)
        T mse = T.Zero;
        for (int k = 0; k < n; k++)
        {
            T error = yData[k] - predict(xData[k]);
            mse += error * error;
        }
        mse /= T.CreateChecked(n);

        return new FitResult<T>(predict, coeffs, mse);
    }


    // 高斯消元法解线性方程组 Ax = b
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T[] SolveLinearSystem_Normal<T>(T[,] A, T[] b)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = b.Length;
        T[,] augmented = new T[n, n + 1]; // 增广矩阵

        // 构建增广矩阵 [A | b]
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                augmented[i, j] = A[i, j];
            }
            augmented[i, n] = b[i];
        }

        // 高斯消元
        for (int pivot = 0; pivot < n - 1; pivot++)
        {
            // 寻找主元
            T maxVal = T.Abs(augmented[pivot, pivot]);
            int maxRow = pivot;
            for (int i = pivot + 1; i < n; i++)
            {
                T absVal = T.Abs(augmented[i, pivot]);
                if (absVal > maxVal)
                {
                    maxVal = absVal;
                    maxRow = i;
                }
            }

            if (maxVal == T.Zero)
                throw new InvalidOperationException("Matrix is singular or nearly singular.");

            // 行交换
            if (maxRow != pivot)
            {
                for (int j = 0; j <= n; j++)
                {
                    T temp = augmented[pivot, j];
                    augmented[pivot, j] = augmented[maxRow, j];
                    augmented[maxRow, j] = temp;
                }
            }

            // 消元
            for (int i = pivot + 1; i < n; i++)
            {
                T factor = augmented[i, pivot] / augmented[pivot, pivot];
                for (int j = pivot; j <= n; j++)
                {
                    augmented[i, j] -= factor * augmented[pivot, j];
                }
            }
        }

        // 回代求解
        T[] x = new T[n];
        for (int i = n - 1; i >= 0; i--)
        {
            T sum = T.Zero;
            for (int j = i + 1; j < n; j++)
            {
                sum += augmented[i, j] * x[j];
            }
            x[i] = (augmented[i, n] - sum) / augmented[i, i];
        }

        return x;
    }

    #endregion


    #region SIMD

    /// <summary>
    /// 多项式回归：拟合 y = a0 + a1*x + a2*x^2 + ... + an*x^n，使用 SIMD 优化。
    /// </summary>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    /// <param name="degree">多项式次数。</param>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResult<T> Fit_SIMD<T>(Span<T> xData, Span<T> yData, int degree)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (degree < 0)
            throw new ArgumentException("Degree must be non-negative.");
        if (degree >= xData.Length)
            throw new ArgumentException("Degree must be less than the number of data points.");

        int n = xData.Length;
        int m = degree + 1; // 系数数量
        int vectorSize = Vector<T>.Count;

        // Step 1: 构造 Vandermonde 矩阵 A (n x m)
        T[,] A = new T[n, m];
        for (int ti = 0; ti < n; ti++)
        {
            A[ti, 0] = T.One; // x^0 = 1
            for (int j = 1; j < m; j++)
            {
                A[ti, j] = A[ti, j - 1] * xData[ti]; // x^j = x^(j-1) * x
            }
        }

        // Step 2: 计算 A^T A (m x m) 和 A^T y (m x 1) 使用 SIMD
        T[,] ATA = new T[m, m];
        T[] ATy = new T[m];

        // 计算 A^T A
        for (int ti = 0; ti < m; ti++)
        {
            for (int j = 0; j < m; j++)
            {
                T sum = T.Zero;
                int k = 0;

                // 手动提取列数据到 Span<T>
                Span<T> colI = stackalloc T[vectorSize];
                Span<T> colJ = stackalloc T[vectorSize];

                for (; k <= n - vectorSize; k += vectorSize)
                {
                    for (int v = 0; v < vectorSize; v++)
                    {
                        colI[v] = A[k + v, ti];
                        colJ[v] = A[k + v, j];
                    }
                    var vecI = new Vector<T>(colI);
                    var vecJ = new Vector<T>(colJ);
                    sum += Vector.Sum(Vector.Multiply(vecI, vecJ));
                }

                // 处理剩余元素
                for (; k < n; k++)
                {
                    sum += A[k, ti] * A[k, j];
                }
                ATA[ti, j] = sum;
            }
        }

        // 计算 A^T y
        for (int ti = 0; ti < m; ti++)
        {
            T sum = T.Zero;
            int k = 0;

            Span<T> colI = stackalloc T[vectorSize];
            for (; k <= n - vectorSize; k += vectorSize)
            {
                for (int v = 0; v < vectorSize; v++)
                {
                    colI[v] = A[k + v, ti];
                }
                var vecI = new Vector<T>(colI);
                var vecY = new Vector<T>(yData.Slice(k, vectorSize));
                sum += Vector.Sum(Vector.Multiply(vecI, vecY));
            }

            // 处理剩余元素
            for (; k < n; k++)
            {
                sum += A[k, ti] * yData[k];
            }
            ATy[ti] = sum;
        }

        // Step 3: 解线性方程组 ATA * a = ATy
        T[] coeffs = SolveLinearSystem_SIMD(ATA, ATy);

        // Step 4: 定义预测函数
        Func<T, T> predict = x =>
        {
            T result = coeffs[0];
            T xPow = x;
            for (int j = 1; j < m; j++)
            {
                result += coeffs[j] * xPow;
                xPow *= x;
            }
            return result;
        };

        // Step 5: 计算均方误差 (MSE) 使用 SIMD
        T mse = T.Zero;
        int i = 0;
        for (; i <= n - vectorSize; i += vectorSize)
        {
            Span<T> predSpan = stackalloc T[vectorSize];
            for (int j = 0; j < vectorSize; j++)
            {
                predSpan[j] = predict(xData[i + j]);
            }
            var predVec = new Vector<T>(predSpan);
            var yVec = new Vector<T>(yData.Slice(i, vectorSize));
            var errorVec = yVec - predVec;
            mse += Vector.Sum(Vector.Multiply(errorVec, errorVec));
        }
        // 处理剩余元素
        for (; i < n; i++)
        {
            T error = yData[i] - predict(xData[i]);
            mse += error * error;
        }
        mse /= T.CreateChecked(n);

        return new FitResult<T>(predict, coeffs, mse);
    }


    // 高斯消元法解线性方程组 Ax = b，使用 SIMD 优化
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T[] SolveLinearSystem_SIMD<T>(T[,] A, T[] b)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = b.Length;
        T[,] augmented = new T[n, n + 1]; // 增广矩阵
        int vectorSize = Vector<T>.Count;

        // 构建增广矩阵 [A | b]
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                augmented[i, j] = A[i, j];
            }
            augmented[i, n] = b[i];
        }

        // 高斯消元
        for (int pivot = 0; pivot < n - 1; pivot++)
        {
            // 寻找主元（标量操作）
            T maxVal = T.Abs(augmented[pivot, pivot]);
            int maxRow = pivot;
            for (int i = pivot + 1; i < n; i++)
            {
                T absVal = T.Abs(augmented[i, pivot]);
                if (absVal > maxVal)
                {
                    maxVal = absVal;
                    maxRow = i;
                }
            }

            if (maxVal == T.Zero)
                throw new InvalidOperationException("Matrix is singular or nearly singular.");

            // 行交换
            if (maxRow != pivot)
            {
                for (int j = 0; j <= n; j++)
                {
                    T temp = augmented[pivot, j];
                    augmented[pivot, j] = augmented[maxRow, j];
                    augmented[maxRow, j] = temp;
                }
            }

            // 消元（使用 SIMD）
            Span<T> pivotRow = stackalloc T[vectorSize];
            for (int i = pivot + 1; i < n; i++)
            {
                T factor = augmented[i, pivot] / augmented[pivot, pivot];
                augmented[i, pivot] = T.Zero; // 显式置零

                int j = pivot + 1;
                for (; j < n - vectorSize + 1; j += vectorSize)
                {
                    // 加载 pivot 行数据
                    for (int v = 0; v < vectorSize; v++)
                    {
                        pivotRow[v] = augmented[pivot, j + v];
                    }
                    var pivotVec = new Vector<T>(pivotRow);
                    var factorVec = new Vector<T>(factor); // 创建一个全为 factor 的向量

                    // 加载当前行数据
                    Span<T> currentRow = stackalloc T[vectorSize];
                    for (int v = 0; v < vectorSize; v++)
                    {
                        currentRow[v] = augmented[i, j + v];
                    }
                    var currentVec = new Vector<T>(currentRow);

                    // SIMD 计算
                    var resultVec = currentVec - Vector.Multiply(factorVec, pivotVec);

                    // 写回结果
                    for (int v = 0; v < vectorSize; v++)
                    {
                        augmented[i, j + v] = resultVec[v];
                    }
                }

                // 处理剩余元素
                for (; j <= n; j++)
                {
                    augmented[i, j] -= factor * augmented[pivot, j];
                }
            }
        }

        // 回代（使用 SIMD）
        T[] x = new T[n];
        for (int i = n - 1; i >= 0; i--)
        {
            T sum = T.Zero;
            int j = i + 1;

            // SIMD 计算 sum
            Span<T> rowSpan = stackalloc T[vectorSize];
            Span<T> xSpan = stackalloc T[vectorSize];
            for (; j < n - vectorSize + 1; j += vectorSize)
            {
                for (int v = 0; v < vectorSize; v++)
                {
                    rowSpan[v] = augmented[i, j + v];
                    xSpan[v] = x[j + v];
                }
                var rowVec = new Vector<T>(rowSpan);
                var xVec = new Vector<T>(xSpan);
                sum += Vector.Sum(Vector.Multiply(rowVec, xVec));
            }

            // 处理剩余元素
            for (; j < n; j++)
            {
                sum += augmented[i, j] * x[j];
            }

            x[i] = (augmented[i, n] - sum) / augmented[i, i];
        }

        return x;
    }


    #endregion



    internal static void TEST()
    {
        // 生成 20,000 个测试数据点
        int dataSize = 2000000;
        double[] xData = new double[dataSize];
        double[] yData = new double[dataSize];

        // 填充数据：y = 2 + 3x
        for (int i = 0; i < dataSize; i++)
        {
            xData[i] = i * 0.1;         // x 从 0 到 1999.9，步长 0.1
            yData[i] = 2.0 + 3.0 * xData[i]; // y = 2 + 3x
        }

        // 调用 Polynomial 方法，拟合一次多项式
        var result = Fit_SIMD<double>(xData, yData, 5);

        // 输出结果
        Console.WriteLine("拟合参数 (Coefficients):");
        for (int i = 0; i < result.Parameters.Length; i++)
        {
            Console.WriteLine($"a{i}: {result.Parameters[i]:F4}");
        }
        Console.WriteLine($"均方误差 (MSE): {result.MeanSquaredError:F4}");

        // 验证前 5 个和后 5 个预测值
        Console.WriteLine("\n预测值（前 5 个和后 5 个）：");
        for (int i = 0; i < 5; i++)
        {
            double predicted = result.Predict(xData[i]);
            Console.WriteLine($"x: {xData[i]:F1}, y实际: {yData[i]:F1}, y预测: {predicted:F4}");
        }
        for (int i = dataSize - 5; i < dataSize; i++)
        {
            double predicted = result.Predict(xData[i]);
            Console.WriteLine($"x: {xData[i]:F1}, y实际: {yData[i]:F1}, y预测: {predicted:F4}");
        }
    }
}
