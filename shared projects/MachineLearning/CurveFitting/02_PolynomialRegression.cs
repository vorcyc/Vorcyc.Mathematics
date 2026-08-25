namespace Vorcyc.Mathematics.MachineLearning.CurveFitting;
using System.Numerics;
using System.Runtime.CompilerServices;
internal static class PolynomialRegression
{
    #region Normal
    /// <summary>
    /// 多项式回归：拟合 y = a0 + a1*x + a2*x^2 + ... + an*x^n。
    /// 内部将 x 仿射映射到 [-1,1] 再建 Vandermonde，降低正规方程病态；
    /// <see cref="FitResult{T}.Parameters"/> 仍换算回原始 x 的幂基系数；预测走缩放域求值。
    /// </summary>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    /// <param name="degree">多项式次数。</param>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResult<T> Fit_Normal<T>(
        Span<T> xData, Span<T> yData, int degree, CancellationToken cancellationToken = default)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (degree < 0)
            throw new ArgumentException("Degree must be non-negative.");
        if (degree >= xData.Length)
            throw new ArgumentException("Degree must be less than the number of data points.");
        int n = xData.Length;
        int m = degree + 1; // 系数数量
        BuildAffineToUnitInterval(xData, out T alpha, out T beta, cancellationToken);
        T[,] A = new T[n, m];
        for (int i = 0; i < n; i++)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
            T u = alpha * xData[i] + beta;
            A[i, 0] = T.One;
            for (int j = 1; j < m; j++)
                A[i, j] = A[i, j - 1] * u;
        }
        var ataD = new double[m, m];
        var atyD = new double[m];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < m; j++)
            {
                double sum = 0;
                for (int k = 0; k < n; k++)
                {
                    CurveFittingExecution.ThrowIfCancelled(cancellationToken, k);
                    sum += double.CreateChecked(A[k, i]) * double.CreateChecked(A[k, j]);
                }
                ataD[i, j] = sum;
            }
        }
        for (int i = 0; i < m; i++)
        {
            double sum = 0;
            for (int k = 0; k < n; k++)
            {
                CurveFittingExecution.ThrowIfCancelled(cancellationToken, k);
                sum += double.CreateChecked(A[k, i]) * double.CreateChecked(yData[k]);
            }
            atyD[i] = sum;
        }
        double[] coeffsUD = SolveLinearSystem_Double(ataD, atyD);
        double alphaD = double.CreateChecked(alpha);
        double betaD = double.CreateChecked(beta);
        double[] coeffsXD = ConvertCoeffsFromAffineDouble(coeffsUD, alphaD, betaD);
        T[] coeffsU = new T[m];
        T[] coeffsX = new T[m];
        for (int i = 0; i < m; i++)
        {
            coeffsU[i] = T.CreateChecked(coeffsUD[i]);
            coeffsX[i] = T.CreateChecked(coeffsXD[i]);
        }
        Func<T, T> predict = x => EvalPoly(coeffsU, alpha * x + beta);
        T mse = T.Zero;
        for (int k = 0; k < n; k++)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, k);
            T error = yData[k] - predict(xData[k]);
            mse += error * error;
        }
        mse /= T.CreateChecked(n);
        return new FitResult<T>(predict, coeffsX, mse);
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
    /// 与 <see cref="Fit_Normal{T}"/> 相同：x→[-1,1] 拟合，参数换回原始 x 幂基。
    /// </summary>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    /// <param name="degree">多项式次数。</param>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResult<T> Fit_SIMD<T>(
        Span<T> xData, Span<T> yData, int degree, CancellationToken cancellationToken = default)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (degree < 0)
            throw new ArgumentException("Degree must be non-negative.");
        if (degree >= xData.Length)
            throw new ArgumentException("Degree must be less than the number of data points.");
        int n = xData.Length;
        int m = degree + 1;
        int vectorSize = Vector<T>.Count;
        BuildAffineToUnitInterval(xData, out T alpha, out T beta, cancellationToken);
        T[,] A = new T[n, m];
        for (int ti = 0; ti < n; ti++)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, ti);
            T u = alpha * xData[ti] + beta;
            A[ti, 0] = T.One;
            for (int j = 1; j < m; j++)
                A[ti, j] = A[ti, j - 1] * u;
        }
        var ataD = new double[m, m];
        var atyD = new double[m];
        Span<T> colI = stackalloc T[vectorSize];
        Span<T> colJ = stackalloc T[vectorSize];
        for (int ti = 0; ti < m; ti++)
        {
            for (int j = 0; j < m; j++)
            {
                double sum = 0;
                int k = 0;
                for (; k <= n - vectorSize; k += vectorSize)
                {
                    CurveFittingExecution.ThrowIfCancelled(cancellationToken, k);
                    for (int v = 0; v < vectorSize; v++)
                    {
                        colI[v] = A[k + v, ti];
                        colJ[v] = A[k + v, j];
                    }
                    var vecI = new Vector<T>(colI);
                    var vecJ = new Vector<T>(colJ);
                    sum += double.CreateChecked(Vector.Sum(Vector.Multiply(vecI, vecJ)));
                }
                for (; k < n; k++)
                    sum += double.CreateChecked(A[k, ti]) * double.CreateChecked(A[k, j]);
                ataD[ti, j] = sum;
            }
        }
        for (int ti = 0; ti < m; ti++)
        {
            double sum = 0;
            int k = 0;
            for (; k <= n - vectorSize; k += vectorSize)
            {
                CurveFittingExecution.ThrowIfCancelled(cancellationToken, k);
                for (int v = 0; v < vectorSize; v++)
                    colI[v] = A[k + v, ti];
                var vecI = new Vector<T>(colI);
                var vecY = new Vector<T>(yData.Slice(k, vectorSize));
                sum += double.CreateChecked(Vector.Sum(Vector.Multiply(vecI, vecY)));
            }
            for (; k < n; k++)
                sum += double.CreateChecked(A[k, ti]) * double.CreateChecked(yData[k]);
            atyD[ti] = sum;
        }
        double[] coeffsUD = SolveLinearSystem_Double(ataD, atyD);
        double alphaD = double.CreateChecked(alpha);
        double betaD = double.CreateChecked(beta);
        double[] coeffsXD = ConvertCoeffsFromAffineDouble(coeffsUD, alphaD, betaD);
        T[] coeffsU = new T[m];
        T[] coeffsX = new T[m];
        for (int ti = 0; ti < m; ti++)
        {
            coeffsU[ti] = T.CreateChecked(coeffsUD[ti]);
            coeffsX[ti] = T.CreateChecked(coeffsXD[ti]);
        }
        Func<T, T> predict = x => EvalPoly(coeffsU, alpha * x + beta);
        T mse = T.Zero;
        int i = 0;
        Span<T> predSpan = stackalloc T[vectorSize];
        for (; i <= n - vectorSize; i += vectorSize)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
            for (int j = 0; j < vectorSize; j++)
            {
                predSpan[j] = predict(xData[i + j]);
            }
            var predVec = new Vector<T>(predSpan);
            var yVec = new Vector<T>(yData.Slice(i, vectorSize));
            var errorVec = yVec - predVec;
            mse += Vector.Sum(Vector.Multiply(errorVec, errorVec));
        }
        for (; i < n; i++)
        {
            T error = yData[i] - predict(xData[i]);
            mse += error * error;
        }
        mse /= T.CreateChecked(n);
        return new FitResult<T>(predict, coeffsX, mse);
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
        Span<T> pivotRow = stackalloc T[vectorSize];
        Span<T> currentRow = stackalloc T[vectorSize];
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
            // 消元（使用 SIMD）；缓冲在方法级复用，避免循环内 stackalloc（CA2014）
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
        Span<T> rowSpan = stackalloc T[vectorSize];
        Span<T> xSpan = stackalloc T[vectorSize];
        for (int i = n - 1; i >= 0; i--)
        {
            T sum = T.Zero;
            int j = i + 1;
            // SIMD 计算 sum
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

    #region Affine map helpers
    /// <summary>高斯消元（double），供正规方程在累加后求解。</summary>
    private static double[] SolveLinearSystem_Double(double[,] A, double[] b)
    {
        int n = b.Length;
        var augmented = new double[n, n + 1];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                augmented[i, j] = A[i, j];
            augmented[i, n] = b[i];
        }
        for (int pivot = 0; pivot < n - 1; pivot++)
        {
            double maxVal = Math.Abs(augmented[pivot, pivot]);
            int maxRow = pivot;
            for (int i = pivot + 1; i < n; i++)
            {
                double absVal = Math.Abs(augmented[i, pivot]);
                if (absVal > maxVal)
                {
                    maxVal = absVal;
                    maxRow = i;
                }
            }
            if (maxVal == 0)
                throw new InvalidOperationException("Matrix is singular or nearly singular.");
            if (maxRow != pivot)
            {
                for (int j = 0; j <= n; j++)
                {
                    double temp = augmented[pivot, j];
                    augmented[pivot, j] = augmented[maxRow, j];
                    augmented[maxRow, j] = temp;
                }
            }
            for (int i = pivot + 1; i < n; i++)
            {
                double factor = augmented[i, pivot] / augmented[pivot, pivot];
                for (int j = pivot; j <= n; j++)
                    augmented[i, j] -= factor * augmented[pivot, j];
            }
        }
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = 0;
            for (int j = i + 1; j < n; j++)
                sum += augmented[i, j] * x[j];
            x[i] = (augmented[i, n] - sum) / augmented[i, i];
        }
        return x;
    }

    /// <summary>
    /// u = αx + β，将 [xMin,xMax] 映到 [-1,1]。degenerate 时 α=β=0（全体 u=0）。
    /// </summary>
    private static void BuildAffineToUnitInterval<T>(
        Span<T> xData, out T alpha, out T beta, CancellationToken cancellationToken = default)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T xMin = xData[0];
        T xMax = xData[0];
        for (int i = 1; i < xData.Length; i++)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
            T x = xData[i];
            if (x < xMin) xMin = x;
            if (x > xMax) xMax = x;
        }
        T range = xMax - xMin;
        if (range == T.Zero || !T.IsFinite(range))
        {
            alpha = T.Zero;
            beta = T.Zero;
            return;
        }
        T two = T.CreateChecked(2);
        alpha = two / range;
        beta = -(xMin + xMax) / range;
    }

    /// <summary>Horner：Σ c_k t^k。</summary>
    private static T EvalPoly<T>(T[] coeffs, T t)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T result = coeffs[0];
        T pow = t;
        for (int j = 1; j < coeffs.Length; j++)
        {
            result += coeffs[j] * pow;
            pow *= t;
        }
        return result;
    }

    /// <summary>
    /// 将 Σ b_k (αx+β)^k 展开为 Σ a_j x^j（二项式，double）。
    /// </summary>
    private static double[] ConvertCoeffsFromAffineDouble(double[] coeffsU, double alpha, double beta)
    {
        int m = coeffsU.Length;
        var coeffsX = new double[m];
        for (int k = 0; k < m; k++)
        {
            double bk = coeffsU[k];
            if (bk == 0)
                continue;
            double binom = 1;
            for (int j = 0; j <= k; j++)
            {
                if (j > 0)
                    binom = binom * (k - j + 1) / j;
                double alphaPow = 1;
                for (int t = 0; t < j; t++)
                    alphaPow *= alpha;
                double betaPow = 1;
                for (int t = 0; t < k - j; t++)
                    betaPow *= beta;
                coeffsX[j] += bk * binom * alphaPow * betaPow;
            }
        }
        return coeffsX;
    }

    /// <summary>
    /// 将 Σ b_k (αx+β)^k 展开为 Σ a_j x^j（二项式）。
    /// </summary>
    private static T[] ConvertCoeffsFromAffine<T>(T[] coeffsU, T alpha, T beta)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        double[] u = new double[coeffsU.Length];
        for (int i = 0; i < coeffsU.Length; i++)
            u[i] = double.CreateChecked(coeffsU[i]);
        double[] x = ConvertCoeffsFromAffineDouble(u, double.CreateChecked(alpha), double.CreateChecked(beta));
        var result = new T[x.Length];
        for (int i = 0; i < x.Length; i++)
            result[i] = T.CreateChecked(x[i]);
        return result;
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
