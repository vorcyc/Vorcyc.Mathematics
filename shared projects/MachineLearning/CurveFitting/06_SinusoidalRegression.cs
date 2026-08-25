using System.Numerics;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.MachineLearning.CurveFitting;

internal static class SinusoidalRegression
{
    /// <summary>
    /// 正弦回归：拟合 y = A * sin(Bx + C) + D（标量路径）。
    /// </summary>
    public static FitResult<T> Fit_Normal<T>(
        Span<T> xData, Span<T> yData, int maxIterations = 100, CancellationToken cancellationToken = default)
        where T : unmanaged, IFloatingPointIeee754<T>
        => Fit(xData, yData, maxIterations, ComputingContext.Normal, cancellationToken);

    public static FitResult<T> Fit<T>(
        Span<T> xData, Span<T> yData, int maxIterations = 100, ComputingContext? computingContext = null,
        CancellationToken cancellationToken = default)
        where T : unmanaged, IFloatingPointIeee754<T>
        => FitCore(xData, yData, maxIterations, computingContext, cancellationToken);

    public static FitResult<T> Fit_SIMD<T>(
        Span<T> xData, Span<T> yData, int maxIterations = 100, ComputingContext? computingContext = null,
        CancellationToken cancellationToken = default)
        where T : unmanaged, IFloatingPointIeee754<T>
        => Fit(xData, yData, maxIterations, computingContext, cancellationToken);

    private static FitResult<T> FitCore<T>(
        Span<T> xData, Span<T> yData, int maxIterations, ComputingContext? computingContext,
        CancellationToken cancellationToken)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length || xData.Length < 4)
            throw new ArgumentException("数据点数量必须相等且至少有4个点");
        if (maxIterations <= 0)
            throw new ArgumentException("最大迭代次数必须大于0");

        int n = xData.Length;
        T[] xArr = xData.ToArray();
        T[] yArr = yData.ToArray();

        T xMin = xArr[0], xMax = xArr[0];
        T yMin = yArr[0], yMax = yArr[0];
        T ySum = T.Zero;
        for (int i = 0; i < n; i++)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
            T x = xArr[i], y = yArr[i];
            if (x < xMin) xMin = x;
            if (x > xMax) xMax = x;
            if (y < yMin) yMin = y;
            if (y > yMax) yMax = y;
            ySum += y;
        }

        T xRange = xMax - xMin;
        if (xRange <= T.Zero)
            throw new ArgumentException("xData 必须具有非零跨度。");

        T yMean = ySum / T.CreateChecked(n);
        T yRange = yMax - yMin;

        // 将 x 缩放到 [0,1]，改善 B 的条件数；拟合后再还原。
        T[] xScaled = new T[n];
        for (int i = 0; i < n; i++)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
            xScaled[i] = (xArr[i] - xMin) / xRange;
        }

        T initA = yRange / T.CreateChecked(2);
        if (initA <= T.Zero)
            initA = T.One;
        T initD = yMean;
        T initB = EstimateAngularFrequency(xScaled, yArr, yMean, cancellationToken);
        T[] seed = RefinePhaseGrid(xScaled, yArr, initA, initB, initD, cancellationToken);

        T[] parameters = LevenbergMarquardt(xScaled, yArr, seed, maxIterations, computingContext, cancellationToken);

        // B_scaled = B_original * xRange  =>  B_original = B_scaled / xRange
        parameters[1] /= xRange;
        // C_scaled = B_original * xMin + C_original  =>  C_original = C_scaled - B_original * xMin
        parameters[2] -= parameters[1] * xMin;

        CanonicalizeParameters(parameters);
        EnsureFinite(parameters);

        Func<T, T> predict = x =>
            parameters[0] * T.Sin(parameters[1] * x + parameters[2]) + parameters[3];

        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
            T diff = predict(xArr[i]) - yArr[i];
            mse += diff * diff;
        }
        mse /= T.CreateChecked(n);
        if (!T.IsFinite(mse))
            throw new InvalidOperationException("正弦拟合未能收敛到有限参数。");

        return new FitResult<T>(predict, parameters, mse);
    }

    /// <summary>
    /// 对去均值后的 y 用过零点估计角频率（在已缩放到单位区间的 x 上）。
    /// </summary>
    private static T EstimateAngularFrequency<T>(T[] xScaled, T[] y, T yMean, CancellationToken cancellationToken)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = y.Length;
        Span<T> crossings = n <= 256 ? stackalloc T[n] : new T[n];
        int count = 0;
        for (int i = 0; i < n - 1; i++)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
            T y0 = y[i] - yMean;
            T y1 = y[i + 1] - yMean;
            if (y0 == T.Zero)
            {
                crossings[count++] = xScaled[i];
            }
            else if (y0 * y1 < T.Zero)
            {
                T t = y0 / (y0 - y1);
                crossings[count++] = xScaled[i] + t * (xScaled[i + 1] - xScaled[i]);
            }
        }

        T fallback = T.Tau; // 假设缩放后约一个周期
        if (count < 2)
            return fallback;

        T gapSum = T.Zero;
        int gaps = 0;
        for (int i = 1; i < count; i++)
        {
            T gap = crossings[i] - crossings[i - 1];
            if (gap > T.Zero)
            {
                gapSum += gap;
                gaps++;
            }
        }

        if (gaps == 0)
            return fallback;

        // 相邻过零 ≈ 半周期
        T halfPeriod = gapSum / T.CreateChecked(gaps);
        T period = halfPeriod + halfPeriod;
        if (period <= T.CreateChecked(1e-12))
            return fallback;

        return T.Tau / period;
    }

    /// <summary>
    /// 在固定 B 下网格搜索相位，并用线性最小二乘估计 A、D。
    /// </summary>
    private static T[] RefinePhaseGrid<T>(
        T[] x, T[] y, T initA, T initB, T initD, CancellationToken cancellationToken)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = x.Length;
        const int grid = 16;
        T bestSsr = T.PositiveInfinity;
        T bestA = initA, bestC = T.Zero, bestD = initD;

        for (int g = 0; g < grid; g++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            T c = T.Tau * T.CreateChecked(g) / T.CreateChecked(grid);
            T sumS = T.Zero, sumS2 = T.Zero, sumY = T.Zero, sumSY = T.Zero;
            for (int i = 0; i < n; i++)
            {
                CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
                T s = T.Sin(initB * x[i] + c);
                sumS += s;
                sumS2 += s * s;
                sumY += y[i];
                sumSY += s * y[i];
            }

            T nT = T.CreateChecked(n);
            T det = nT * sumS2 - sumS * sumS;
            T a, d;
            if (T.Abs(det) < T.CreateChecked(1e-12))
            {
                a = initA;
                d = initD;
            }
            else
            {
                a = (nT * sumSY - sumS * sumY) / det;
                d = (sumY * sumS2 - sumS * sumSY) / det;
            }

            T ssr = T.Zero;
            for (int i = 0; i < n; i++)
            {
                CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
                T r = y[i] - (a * T.Sin(initB * x[i] + c) + d);
                ssr += r * r;
            }

            if (ssr < bestSsr)
            {
                bestSsr = ssr;
                bestA = a;
                bestC = c;
                bestD = d;
            }
        }

        if (!T.IsFinite(bestA) || bestA == T.Zero)
            bestA = initA;
        if (!T.IsFinite(bestD))
            bestD = initD;

        return [bestA, initB, bestC, bestD];
    }

    private static T[] LevenbergMarquardt<T>(
        T[] xData, T[] yData, T[] initialParams, int maxIterations, ComputingContext? context,
        CancellationToken cancellationToken)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = xData.Length;
        int m = 4;
        T[] parameters = (T[])initialParams.Clone();
        T tolerance = T.CreateChecked(1e-8);
        T currentLambda = T.CreateChecked(0.001);
        T increaseFactor = T.CreateChecked(10);
        T decreaseFactor = T.CreateChecked(10);
        const long workPerItem = 48;

        var dispatch = CurveFittingExecution.ResolveDispatch<T>(context, n, workPerItem);
        bool useSimd = dispatch == CurveFitDispatchKind.Simd;

        T[] residuals = new T[n];
        T[][] jacobian = new T[n][];
        for (int i = 0; i < n; i++)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
            jacobian[i] = new T[m];
        }
        T[] colI = new T[n];
        T[] colJ = new T[n];

        void FillRow(int i)
        {
            T A = parameters[0], B = parameters[1], C = parameters[2], D = parameters[3];
            T x = xData[i];
            T phase = B * x + C;
            T s = T.Sin(phase);
            T c = T.Cos(phase);
            residuals[i] = A * s + D - yData[i];
            jacobian[i][0] = s;
            jacobian[i][1] = A * x * c;
            jacobian[i][2] = A * c;
            jacobian[i][3] = T.One;
        }

        for (int iter = 0; iter < maxIterations; iter++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ComputingContextExecution.ForEach(context, 0, n, FillRow, workPerItem, cancellationToken: cancellationToken);

            T[][] jtJ = new T[m][];
            for (int i = 0; i < m; i++)
            {
                jtJ[i] = new T[m];
                for (int r = 0; r < n; r++)
                {
                    CurveFittingExecution.ThrowIfCancelled(cancellationToken, r);
                    colI[r] = jacobian[r][i];
                }
                for (int j = 0; j < m; j++)
                {
                    for (int r = 0; r < n; r++)
                    {
                        CurveFittingExecution.ThrowIfCancelled(cancellationToken, r);
                        colJ[r] = jacobian[r][j];
                    }
                    T sum = CurveFittingExecution.Dot<T>(colI, colJ, useSimd);
                    jtJ[i][j] = sum + (i == j ? currentLambda * sum : T.Zero);
                }
            }

            T[] jtr = new T[m];
            for (int i = 0; i < m; i++)
            {
                for (int r = 0; r < n; r++)
                {
                    CurveFittingExecution.ThrowIfCancelled(cancellationToken, r);
                    colI[r] = jacobian[r][i];
                }
                jtr[i] = -CurveFittingExecution.Dot<T>(colI, residuals, useSimd);
            }

            T[] delta;
            try
            {
                delta = SolveLinearSystemPivoted(jtJ, jtr);
            }
            catch (InvalidOperationException)
            {
                currentLambda *= increaseFactor;
                continue;
            }

            T[] newParams = new T[m];
            T change = T.Zero;
            for (int i = 0; i < m; i++)
            {
                newParams[i] = parameters[i] + delta[i];
                change += delta[i] * delta[i];
            }

            T oldSsr = CurveFittingExecution.Dot<T>(residuals, residuals, useSimd);
            T newSsr = T.Zero;
            for (int i = 0; i < n; i++)
            {
                CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
                T A = newParams[0], B = newParams[1], C = newParams[2], D = newParams[3];
                T r = A * T.Sin(B * xData[i] + C) + D - yData[i];
                newSsr += r * r;
            }

            if (newSsr < oldSsr && T.IsFinite(newSsr))
            {
                Array.Copy(newParams, parameters, m);
                currentLambda /= decreaseFactor;
                if (T.Sqrt(change) < tolerance)
                    break;
            }
            else
            {
                currentLambda *= increaseFactor;
            }
        }

        return parameters;
    }

    private static T[] SolveLinearSystemPivoted<T>(T[][] A, T[] b)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = b.Length;
        T[][] aug = new T[n][];
        for (int i = 0; i < n; i++)
        {
            aug[i] = new T[n + 1];
            Array.Copy(A[i], 0, aug[i], 0, n);
            aug[i][n] = b[i];
        }

        for (int i = 0; i < n; i++)
        {
            int pivotRow = i;
            T pivotAbs = T.Abs(aug[i][i]);
            for (int r = i + 1; r < n; r++)
            {
                T a = T.Abs(aug[r][i]);
                if (a > pivotAbs)
                {
                    pivotAbs = a;
                    pivotRow = r;
                }
            }

            if (pivotAbs == T.Zero || !T.IsFinite(pivotAbs))
                throw new InvalidOperationException("矩阵奇异，无法求解");

            if (pivotRow != i)
                (aug[i], aug[pivotRow]) = (aug[pivotRow], aug[i]);

            T pivot = aug[i][i];
            for (int j = i + 1; j < n; j++)
            {
                T factor = aug[j][i] / pivot;
                for (int k = i; k <= n; k++)
                    aug[j][k] -= factor * aug[i][k];
            }
        }

        T[] x = new T[n];
        for (int i = n - 1; i >= 0; i--)
        {
            T sum = aug[i][n];
            for (int j = i + 1; j < n; j++)
                sum -= aug[i][j] * x[j];
            x[i] = sum / aug[i][i];
        }
        return x;
    }

    /// <summary>
    /// 规范化参数：B≥0、A≥0、C∈(-π, π]，消除等价表示歧义且不改变预测值。
    /// </summary>
    private static void CanonicalizeParameters<T>(T[] p)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        // A sin((-B)x + C) = (-A) sin(Bx - C)
        if (p[1] < T.Zero)
        {
            p[0] = -p[0];
            p[1] = -p[1];
            p[2] = -p[2];
        }

        // (-A) sin(Bx + C) = A sin(Bx + C + π)
        if (p[0] < T.Zero)
        {
            p[0] = -p[0];
            p[2] += T.Pi;
        }

        T twoPi = T.Tau;
        T pi = T.Pi;
        T c = p[2] % twoPi;
        if (c > pi)
            c -= twoPi;
        else if (c <= -pi)
            c += twoPi;
        p[2] = c;
    }

    private static void EnsureFinite<T>(T[] parameters)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            if (!T.IsFinite(parameters[i]))
                throw new InvalidOperationException("正弦拟合未能收敛到有限参数。");
        }
    }

    public static void RunTests()
    {
        Console.WriteLine("Running Sine Fit Tests...");
        TestPerfectSine();
        TestMultiPeriodSine();
        TestNoisySine();
        TestInvalidInput();
        Console.WriteLine("All tests completed!");
    }

    private static void TestPerfectSine()
    {
        Console.WriteLine("\nTest 1: Perfect Sine Function");
        int n = 20;
        double[] x = new double[n];
        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = i * 0.1;
            y[i] = 2.0 * Math.Sin(Math.PI * x[i]) + 1.0;
        }
        var result = Fit_Normal<double>(x, y);
        double tolerance = 0.15;
        bool aOk = Math.Abs(result.Parameters[0] - 2.0) < tolerance;
        bool bOk = Math.Abs(result.Parameters[1] - Math.PI) < tolerance;
        bool dOk = Math.Abs(result.Parameters[3] - 1.0) < tolerance;
        bool mseOk = result.MeanSquaredError < 0.01 && double.IsFinite(result.MeanSquaredError);
        Console.WriteLine($"A = {result.Parameters[0]:F3} (Expected ~2.0): {(aOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"B = {result.Parameters[1]:F3} (Expected ~π): {(bOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"D = {result.Parameters[3]:F3} (Expected ~1.0): {(dOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"MSE = {result.MeanSquaredError:F6}: {(mseOk ? "PASS" : "FAIL")}");
    }

    private static void TestMultiPeriodSine()
    {
        Console.WriteLine("\nTest 1b: Multi-period Sine (VSS-style)");
        int n = 256;
        double[] x = new double[n];
        double[] y = new double[n];
        double freq = 5.0; // Hz over [0,1)
        for (int i = 0; i < n; i++)
        {
            x[i] = i / (double)n;
            y[i] = Math.Sin(2.0 * Math.PI * freq * x[i]);
        }
        var result = Fit_Normal<double>(x, y, 200);
        bool finite = result.Parameters.All(double.IsFinite) && double.IsFinite(result.MeanSquaredError);
        bool mseOk = result.MeanSquaredError < 1e-6;
        double expectedB = 2.0 * Math.PI * freq;
        bool bOk = Math.Abs(Math.Abs(result.Parameters[1]) - expectedB) < 0.2;
        Console.WriteLine($"Finite params: {(finite ? "PASS" : "FAIL")}");
        Console.WriteLine($"B = {result.Parameters[1]:F3} (Expected ~{expectedB:F3}): {(bOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"MSE = {result.MeanSquaredError:E3}: {(mseOk ? "PASS" : "FAIL")}");
    }

    private static void TestNoisySine()
    {
        Console.WriteLine("\nTest 2: Noisy Sine Function");
        Random rand = new(42);
        int n = 30;
        double[] x = new double[n];
        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = i * 0.2;
            double noise = rand.NextDouble() * 0.2 - 0.1;
            y[i] = 1.5 * Math.Sin(2.0 * x[i] + 0.5) + 2.0 + noise;
        }
        var result = Fit_Normal<double>(x, y);
        double tolerance = 0.35;
        bool aOk = Math.Abs(Math.Abs(result.Parameters[0]) - 1.5) < tolerance;
        bool bOk = Math.Abs(Math.Abs(result.Parameters[1]) - 2.0) < tolerance;
        bool dOk = Math.Abs(result.Parameters[3] - 2.0) < tolerance;
        bool mseOk = result.MeanSquaredError < 0.1 && double.IsFinite(result.MeanSquaredError);
        Console.WriteLine($"A = {result.Parameters[0]:F3} (Expected ~±1.5): {(aOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"B = {result.Parameters[1]:F3} (Expected ~±2.0): {(bOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"D = {result.Parameters[3]:F3} (Expected ~2.0): {(dOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"MSE = {result.MeanSquaredError:F6}: {(mseOk ? "PASS" : "FAIL")}");
    }

    private static void TestInvalidInput()
    {
        Console.WriteLine("\nTest 3: Invalid Input");
        double[] shortX = { 1, 2, 3 };
        double[] shortY = { 1, 2, 3 };
        bool threwShort = false;
        try { Fit_Normal<double>(shortX, shortY); }
        catch (ArgumentException) { threwShort = true; }

        double[] x = { 1, 2, 3, 4 };
        double[] y = { 1, 2, 3 };
        bool threwMismatch = false;
        try { Fit_Normal<double>(x, y); }
        catch (ArgumentException) { threwMismatch = true; }

        Console.WriteLine($"Too few points test: {(threwShort ? "PASS" : "FAIL")}");
        Console.WriteLine($"Mismatched lengths test: {(threwMismatch ? "PASS" : "FAIL")}");
    }
}
