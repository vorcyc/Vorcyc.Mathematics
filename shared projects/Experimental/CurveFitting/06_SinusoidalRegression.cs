using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class SinusoidalRegression
{



    ///// <summary>
    ///// 正弦回归：拟合 y = A * sin(Bx + C) + D。
    ///// </summary>
    //public static FitResult<T> Fit_Normal<T>(Span<T> xData, Span<T> yData)
    //    where T : unmanaged, IFloatingPointIeee754<T>
    //{
    //    if (xData.Length != yData.Length)
    //        throw new ArgumentException("The length of xData and yData must be the same.");

    //    int n = xData.Length;
    //    T tn = T.CreateChecked(n);

    //    var yMin = yData.Min_Normal();
    //    var yMax = yData.Max_Normal();
    //    var xMin = xData.Min_Normal();
    //    var xMax = xData.Max_Normal();

    //    // 初始猜测参数
    //    T A = (yMax - yMin) / T.CreateChecked(2);
    //    T D = (yMax + yMin) / T.CreateChecked(2);
    //    T B = T.CreateChecked(2 * Math.PI) / (xMax - yMin);
    //    T C = T.Zero;

    //    // 定义预测函数
    //    Func<T, T, T, T, T, T> predict = (a, b, c, d, x) => a * T.Sin(b * x + c) + d;

    //    // 使用非线性最小二乘法拟合参数
    //    for (int iter = 0; iter < 100000; iter++)
    //    {
    //        T sumA = T.Zero, sumB = T.Zero, sumC = T.Zero, sumD = T.Zero;
    //        T sumError = T.Zero;

    //        for (int i = 0; i < n; i++)
    //        {
    //            T x = xData[i];
    //            T y = yData[i];
    //            T yPred = predict(A, B, C, D, x);
    //            T error = y - yPred;

    //            sumA += error * T.Sin(B * x + C);
    //            sumB += error * A * x * T.Cos(B * x + C);
    //            sumC += error * A * T.Cos(B * x + C);
    //            sumD += error;
    //            sumError += error * error;
    //        }

    //        A += sumA / tn;
    //        B += sumB / tn;
    //        C += sumC / tn;
    //        D += sumD / tn;

    //        if (sumError < T.CreateChecked(1e-6))
    //            break;
    //    }

    //    // 计算均方误差
    //    T mse = T.Zero;
    //    for (int i = 0; i < n; i++)
    //    {
    //        T error = yData[i] - predict(A, B, C, D, xData[i]);
    //        mse += error * error;
    //    }
    //    mse /= tn;

    //    // 返回拟合结果
    //    return new FitResult<T>(x => predict(A, B, C, D, x), [A, B, C, D], mse);
    //}



    /// <summary>
    /// 正弦回归：拟合 y = A * sin(Bx + C) + D。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="maxIterations">最大迭代次数，默认100</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> Fit_Normal<T>(Span<T> xData, Span<T> yData, int maxIterations = 100)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length || xData.Length < 4)
            throw new ArgumentException("数据点数量必须相等且至少有4个点");
        if (maxIterations <= 0)
            throw new ArgumentException("最大迭代次数必须大于0");

        int n = xData.Length;

        // 计算数据的统计特性用于初始估计
        T yMean = yData.Average_SIMD();
        T yRange = yData.Max_SIMD() - yData.Min_SIMD();
        T xRange = xData.Max_SIMD() - xData.Min_SIMD();

        // 初始参数估计
        T initA = yRange / T.CreateChecked(2); // 振幅初始值
        T initB = T.Tau / xRange;             // 频率初始值 (2π/周期)
        T initC = T.Zero;                     // 相位初始值
        T initD = yMean;                      // 偏移初始值

        // 使用非线性最小二乘法优化参数
        T[] parameters = new T[] { initA, initB, initC, initD };

        // 定义目标函数
        Func<T[], T[], T> residual = (p, x) =>
        {
            T A = p[0], B = p[1], C = p[2], D = p[3];
            return A * T.Sin(B * x[0] + C) + D;
        };

        // 执行拟合
        parameters = NonlinearLeastSquares<T>(xData, yData, parameters, residual, maxIterations);

        // 创建预测函数
        Func<T, T> predict = x =>
        {
            return parameters[0] * T.Sin(parameters[1] * x + parameters[2]) + parameters[3];
        };

        // 计算均方误差 (MSE)
        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T predicted = predict(xData[i]);
            T diff = predicted - yData[i];
            mse += diff * diff;
        }
        mse /= T.CreateChecked(n);

        return new FitResult<T>(predict, parameters, mse);
    }

    // 修改非线性最小二乘法方法
    private static T[] NonlinearLeastSquares<T>(
        Span<T> xData,
        Span<T> yData,
        T[] initialParams,
        Func<T[], T[], T> model,
        int maxIterations)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T tolerance = T.CreateChecked(1e-6);
        T[] parameters = (T[])initialParams.Clone();
        int n = xData.Length;
        int m = parameters.Length;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            T[] residuals = new T[n];
            T[][] jacobian = new T[n][];

            for (int i = 0; i < n; i++)
            {
                T[] x = new T[] { xData[i] };
                residuals[i] = model(parameters, x) - yData[i];

                jacobian[i] = new T[m];
                T h = T.CreateChecked(1e-6);

                for (int j = 0; j < m; j++)
                {
                    T[] tempParams = (T[])parameters.Clone();
                    tempParams[j] += h;
                    T f1 = model(tempParams, x);
                    tempParams[j] = parameters[j] - h;
                    T f2 = model(tempParams, x);
                    jacobian[i][j] = (f1 - f2) / (h + h);
                }
            }

            T[] delta = SolveNormalEquations(jacobian, residuals);

            T change = T.Zero;
            for (int j = 0; j < m; j++)
            {
                parameters[j] -= delta[j];
                change += delta[j] * delta[j];
            }

            if (T.Sqrt(change) < tolerance)
                break;
        }

        return parameters;
    }

    // 解决正规方程
    private static T[] SolveNormalEquations<T>(T[][] jacobian, T[] residuals)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = jacobian.Length;
        int m = jacobian[0].Length;

        // 计算 J^T * J
        T[][] jtJ = new T[m][];
        for (int i = 0; i < m; i++)
        {
            jtJ[i] = new T[m];
            for (int j = 0; j < m; j++)
            {
                T sum = T.Zero;
                for (int k = 0; k < n; k++)
                    sum += jacobian[k][i] * jacobian[k][j];
                jtJ[i][j] = sum;
            }
        }

        // 计算 J^T * r
        T[] jtr = new T[m];
        for (int i = 0; i < m; i++)
        {
            T sum = T.Zero;
            for (int k = 0; k < n; k++)
                sum += jacobian[k][i] * residuals[k];
            jtr[i] = sum;
        }

        // 这里应使用更鲁棒的线性代数求解器
        // 为简化起见，使用简单的高斯消元法
        return GaussElimination(jtJ, jtr);
    }

    // 高斯消元法求解
    private static T[] GaussElimination<T>(T[][] A, T[] b)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = b.Length;
        T[] x = new T[n];
        T[][] augmented = new T[n][];

        // 创建增广矩阵
        for (int i = 0; i < n; i++)
        {
            augmented[i] = new T[n + 1];
            Array.Copy(A[i], 0, augmented[i], 0, n);
            augmented[i][n] = b[i];
        }

        // 前向消元
        for (int i = 0; i < n; i++)
        {
            T pivot = augmented[i][i];
            for (int j = i + 1; j < n; j++)
            {
                T factor = augmented[j][i] / pivot;
                for (int k = i; k <= n; k++)
                    augmented[j][k] -= factor * augmented[i][k];
            }
        }

        // 回代
        for (int i = n - 1; i >= 0; i--)
        {
            T sum = augmented[i][n];
            for (int j = i + 1; j < n; j++)
                sum -= augmented[i][j] * x[j];
            x[i] = sum / augmented[i][i];
        }

        return x;
    }



    public static void RunTests()
    {
        Console.WriteLine("Running Sine Fit Tests...");

        TestPerfectSine();
        TestNoisySine();
        TestInvalidInput();

        Console.WriteLine("All tests completed!");
    }

    // 测试1：完美正弦函数
    private static void TestPerfectSine()
    {
        Console.WriteLine("\nTest 1: Perfect Sine Function");

        // 生成测试数据: y = 2 * sin(πx) + 1
        int n = 20;
        double[] x = new double[n];
        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = i * 0.1;
            y[i] = 2.0 * Math.Sin(Math.PI * x[i]) + 1.0;
        }

        var result = Fit_Normal<double>(x, y);

        // 检查参数（允许一定误差）
        double tolerance = 0.1;
        bool aOk = Math.Abs(result.Parameters[0] - 2.0) < tolerance;  // A ≈ 2
        bool bOk = Math.Abs(result.Parameters[1] - Math.PI) < tolerance;  // B ≈ π
        bool cOk = Math.Abs(result.Parameters[2]) < tolerance;  // C ≈ 0
        bool dOk = Math.Abs(result.Parameters[3] - 1.0) < tolerance;  // D ≈ 1
        bool mseOk = result.MeanSquaredError < 0.01;

        Console.WriteLine($"A = {result.Parameters[0]:F3} (Expected ~2.0): {(aOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"B = {result.Parameters[1]:F3} (Expected ~π): {(bOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"C = {result.Parameters[2]:F3} (Expected ~0.0): {(cOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"D = {result.Parameters[3]:F3} (Expected ~1.0): {(dOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"MSE = {result.MeanSquaredError:F6} (Expected <0.01): {(mseOk ? "PASS" : "FAIL")}");
    }

    // 测试2：带噪音的正弦函数
    private static void TestNoisySine()
    {
        Console.WriteLine("\nTest 2: Noisy Sine Function");

        // 生成带噪音的测试数据: y = 1.5 * sin(2x + 0.5) + 2 + noise
        Random rand = new Random(42);
        int n = 30;
        double[] x = new double[n];
        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = i * 0.2;
            double noise = rand.NextDouble() * 0.2 - 0.1; // ±0.1的噪音
            y[i] = 1.5 * Math.Sin(2.0 * x[i] + 0.5) + 2.0 + noise;
        }

        var result = Fit_Normal<double>(x, y);

        // 检查参数（允许更大误差）
        double tolerance = 0.3;
        bool aOk = Math.Abs(result.Parameters[0] - 1.5) < tolerance;  // A ≈ 1.5
        bool bOk = Math.Abs(result.Parameters[1] - 2.0) < tolerance;  // B ≈ 2
        bool cOk = Math.Abs(result.Parameters[2] - 0.5) < tolerance;  // C ≈ 0.5
        bool dOk = Math.Abs(result.Parameters[3] - 2.0) < tolerance;  // D ≈ 2
        bool mseOk = result.MeanSquaredError < 0.1;

        Console.WriteLine($"A = {result.Parameters[0]:F3} (Expected ~1.5): {(aOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"B = {result.Parameters[1]:F3} (Expected ~2.0): {(bOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"C = {result.Parameters[2]:F3} (Expected ~0.5): {(cOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"D = {result.Parameters[3]:F3} (Expected ~2.0): {(dOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"MSE = {result.MeanSquaredError:F6} (Expected <0.1): {(mseOk ? "PASS" : "FAIL")}");

        // 测试预测函数
        double testX = 1.0;
        double expected = 1.5 * Math.Sin(2.0 * testX + 0.5) + 2.0;
        double predicted = result.Predict(testX);
        bool predictOk = Math.Abs(predicted - expected) < 0.5;
        Console.WriteLine($"Prediction at x={testX}: {predicted:F3} (Expected ~{expected:F3}): {(predictOk ? "PASS" : "FAIL")}");
    }

    // 测试3：无效输入
    private static void TestInvalidInput()
    {
        Console.WriteLine("\nTest 3: Invalid Input");

        // 测试过少的数据点
        double[] shortX = { 1, 2, 3 };
        double[] shortY = { 1, 2, 3 };
        bool threwShort = false;
        try
        {
            Fit_Normal<double>(shortX, shortY);
        }
        catch (ArgumentException)
        {
            threwShort = true;
        }

        // 测试长度不匹配
        double[] x = { 1, 2, 3, 4 };
        double[] y = { 1, 2, 3 };
        bool threwMismatch = false;
        try
        {
            Fit_Normal<double>(x, y);
        }
        catch (ArgumentException)
        {
            threwMismatch = true;
        }

        Console.WriteLine($"Too few points test: {(threwShort ? "PASS" : "FAIL")}");
        Console.WriteLine($"Mismatched lengths test: {(threwMismatch ? "PASS" : "FAIL")}");
    }
}



