using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

// 有问题！！

internal static class SupportVectorRegression
{
    /// <summary>
    /// 支持向量回归 (SVR)：单列输入，非线性 RBF 核，使用 SMO。
    /// </summary>
    public static FitResult<T> Fit<T>(Span<T> xData, Span<T> yData,
        T epsilon = default, T C = default, T lengthScale = default,
        int maxIterations = 1000, T tolerance = default)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length || xData.Length < 1)
            throw new ArgumentException("数据点数量必须相等且至少有1个点");

        int n = xData.Length;
        T[] xDataArray = xData.ToArray();
        T[] yDataArray = yData.ToArray();

        // 默认参数
        T eps = epsilon == T.Zero ? T.CreateChecked(0.1) : epsilon;
        T c = C == T.Zero ? T.CreateChecked(100.0) : C;
        T l = lengthScale == T.Zero ? T.CreateChecked(0.5) : lengthScale;
        T tol = tolerance == T.Zero ? T.CreateChecked(1e-3) : tolerance;

        // 初始化拉格朗日乘子和偏置
        T[] alpha = new T[n];
        T[] alphaStar = new T[n];
        T b = T.Zero;

        // SMO 优化
        (alpha, alphaStar, b) = SmoOptimization(xDataArray, yDataArray, alpha, alphaStar, b, eps, c, l, maxIterations, tol);

        // 调试输出
        Console.WriteLine("Alpha: " + string.Join(", ", alpha.Select(a => $"{a:F3}")));
        Console.WriteLine("AlphaStar: " + string.Join(", ", alphaStar.Select(a => $"{a:F3}")));
        T sumConstraint = T.Zero;
        for (int i = 0; i < n; i++)
            sumConstraint += alpha[i] - alphaStar[i];
        Console.WriteLine($"Sum constraint (should be ~0): {sumConstraint:F6}");
        Console.WriteLine($"Bias b = {b:F3}");

        // 预测函数
        Func<T, T> predict = x =>
        {
            T sum = b;
            for (int i = 0; i < n; i++)
            {
                T diff = alpha[i] - alphaStar[i];
                if (T.Abs(diff) > tol)
                    sum += diff * ComputeKernel(x, xDataArray[i], l);
            }
            return sum;
        };

        // 计算 MSE
        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T predicted = predict(xDataArray[i]);
            T diff = predicted - yDataArray[i];
            mse += diff * diff;
        }
        mse /= T.CreateChecked(n);

        T[] parameters = new T[] { eps, c, l };
        return new FitResult<T>(predict, parameters, mse);
    }

    /// <summary>
    /// 支持向量回归 (SVR)：多列输入，非线性 RBF 核，使用 SMO。
    /// </summary>
    public static MultiColumnFitResult<T> Fit<T>(DataRow<T>[] xData, Span<T> yData,
        T epsilon = default, T C = default, T lengthScale = default,
        int maxIterations = 1000, T tolerance = default)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length || xData.Length < 1)
            throw new ArgumentException("数据点数量必须相等且至少有1个点");
        if (xData.Any(row => row.ColumnCount == 0))
            throw new ArgumentException("X 数据中不能包含空行");

        int n = xData.Length;
        int inputDim = xData[0].ColumnCount;
        if (xData.Any(row => row.ColumnCount != inputDim))
            throw new ArgumentException("所有数据点的输入维度必须一致");

        T[] yDataArray = yData.ToArray();

        // 默认参数
        T eps = epsilon == T.Zero ? T.CreateChecked(0.1) : epsilon;
        T c = C == T.Zero ? T.CreateChecked(100.0) : C;
        T l = lengthScale == T.Zero ? T.CreateChecked(0.5) : lengthScale;
        T tol = tolerance == T.Zero ? T.CreateChecked(1e-3) : tolerance;

        // 初始化拉格朗日乘子和偏置
        T[] alpha = new T[n];
        T[] alphaStar = new T[n];
        T b = T.Zero;

        // SMO 优化
        (alpha, alphaStar, b) = SmoOptimization(xData, yDataArray, alpha, alphaStar, b, eps, c, l, maxIterations, tol);

        // 调试输出
        Console.WriteLine("Alpha: " + string.Join(", ", alpha.Select(a => $"{a:F3}")));
        Console.WriteLine("AlphaStar: " + string.Join(", ", alphaStar.Select(a => $"{a:F3}")));
        T sumConstraint = T.Zero;
        for (int i = 0; i < n; i++)
            sumConstraint += alpha[i] - alphaStar[i];
        Console.WriteLine($"Sum constraint (should be ~0): {sumConstraint:F6}");
        Console.WriteLine($"Bias b = {b:F3}");

        // 预测函数
        Func<DataRow<T>, T> predict = x =>
        {
            T sum = b;
            for (int i = 0; i < n; i++)
            {
                T diff = alpha[i] - alphaStar[i];
                if (T.Abs(diff) > tol)
                    sum += diff * ComputeKernel(x, xData[i], l);
            }
            return sum;
        };

        // 计算 MSE
        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T predicted = predict(xData[i]);
            T diff = predicted - yDataArray[i];
            mse += diff * diff;
        }
        mse /= T.CreateChecked(n);

        T[] parameters = new T[] { eps, c, l };
        return new MultiColumnFitResult<T>(predict, parameters, mse);
    }

    // 单变量核函数（RBF核）
    private static T ComputeKernel<T>(T x1, T x2, T lengthScale)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T diff = x1 - x2;
        T exponent = -(diff * diff) / (T.CreateChecked(2) * lengthScale * lengthScale);
        return T.Exp(exponent);
    }

    // 多变量核函数（RBF核）
    private static T ComputeKernel<T>(DataRow<T> x1, DataRow<T> x2, T lengthScale)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T sumSquaredDiff = T.Zero;
        for (int i = 0; i < x1.ColumnCount; i++)
        {
            T diff = x1[i] - x2[i];
            sumSquaredDiff += diff * diff;
        }
        T exponent = -sumSquaredDiff / (T.CreateChecked(2) * lengthScale * lengthScale);
        return T.Exp(exponent);
    }

    // 单变量 SMO 优化
    private static (T[] alpha, T[] alphaStar, T b) SmoOptimization<T>(T[] xData, T[] yData, T[] alpha, T[] alphaStar, T b,
        T epsilon, T C, T lengthScale, int maxIterations, T tolerance)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = xData.Length;
        T zero = T.Zero;
        Random rand = new Random();

        T[] errors = new T[n];
        for (int i = 0; i < n; i++)
            errors[i] = ComputeError(xData, yData, alpha, alphaStar, b, xData[i], lengthScale) - yData[i];

        for (int iter = 0; iter < maxIterations; iter++)
        {
            int numChanged = 0;
            for (int i = 0; i < n; i++)
            {
                T Ei = errors[i];
                if ((Ei < -tolerance - epsilon && alpha[i] < C) || (Ei > tolerance + epsilon && alpha[i] > zero) ||
                    (-Ei < -tolerance - epsilon && alphaStar[i] < C) || (-Ei > tolerance + epsilon && alphaStar[i] > zero))
                {
                    // 随机选择 j
                    int j = rand.Next(n);
                    while (j == i) j = rand.Next(n);

                    T Ej = errors[j];

                    T oldAlphaI = alpha[i];
                    T oldAlphaStarI = alphaStar[i];
                    T oldAlphaJ = alpha[j];
                    T oldAlphaStarJ = alphaStar[j];

                    // 计算 L 和 H
                    T gamma = oldAlphaI - oldAlphaStarI + oldAlphaJ - oldAlphaStarJ;
                    T L, H;
                    if (gamma > zero)
                    {
                        L = T.Max(zero, gamma - C);
                        H = T.Min(C, gamma);
                    }
                    else
                    {
                        L = T.Max(zero, gamma);
                        H = T.Min(C, gamma + C);
                    }

                    if (L == H) continue;

                    T kii = ComputeKernel(xData[i], xData[i], lengthScale);
                    T kjj = ComputeKernel(xData[j], xData[j], lengthScale);
                    T kij = ComputeKernel(xData[i], xData[j], lengthScale);
                    T eta = kii + kjj - T.CreateChecked(2) * kij;

                    if (eta <= zero) continue;

                    // 更新 alpha[i] 和 alpha[j]
                    T newAlphaI = oldAlphaI + (Ei - Ej) / eta;
                    newAlphaI = T.Max(L, T.Min(H, newAlphaI));
                    T deltaAlphaI = newAlphaI - oldAlphaI;
                    alpha[i] = newAlphaI;
                    alpha[j] = oldAlphaJ - deltaAlphaI;

                    // 更新 alphaStar[i] 和 alphaStar[j]
                    T newAlphaStarI = oldAlphaStarI - (Ei + Ej - T.CreateChecked(2) * epsilon) / eta;
                    newAlphaStarI = T.Max(zero, T.Min(C, newAlphaStarI));
                    T deltaAlphaStarI = newAlphaStarI - oldAlphaStarI;
                    alphaStar[i] = newAlphaStarI;
                    alphaStar[j] = oldAlphaStarJ - deltaAlphaStarI;

                    // 更新 b
                    T b1 = b - Ei - deltaAlphaI * kii - deltaAlphaStarI * kij;
                    T b2 = b - Ej - deltaAlphaI * kij - deltaAlphaStarI * kjj;
                    if (alpha[i] > zero && alpha[i] < C)
                        b = b1;
                    else if (alpha[j] > zero && alpha[j] < C)
                        b = b2;
                    else
                        b = (b1 + b2) / T.CreateChecked(2);

                    // 更新误差
                    for (int k = 0; k < n; k++)
                        errors[k] = ComputeError(xData, yData, alpha, alphaStar, b, xData[k], lengthScale) - yData[k];

                    numChanged++;
                }
            }
            if (numChanged == 0) break;
        }
        return (alpha, alphaStar, b);
    }

    // 多变量 SMO 优化
    private static (T[] alpha, T[] alphaStar, T b) SmoOptimization<T>(DataRow<T>[] xData, T[] yData, T[] alpha, T[] alphaStar, T b,
        T epsilon, T C, T lengthScale, int maxIterations, T tolerance)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = xData.Length;
        T zero = T.Zero;
        Random rand = new Random();

        T[] errors = new T[n];
        for (int i = 0; i < n; i++)
            errors[i] = ComputeError(xData, yData, alpha, alphaStar, b, xData[i], lengthScale) - yData[i];

        for (int iter = 0; iter < maxIterations; iter++)
        {
            int numChanged = 0;
            for (int i = 0; i < n; i++)
            {
                T Ei = errors[i];
                if ((Ei < -tolerance - epsilon && alpha[i] < C) || (Ei > tolerance + epsilon && alpha[i] > zero) ||
                    (-Ei < -tolerance - epsilon && alphaStar[i] < C) || (-Ei > tolerance + epsilon && alphaStar[i] > zero))
                {
                    // 随机选择 j
                    int j = rand.Next(n);
                    while (j == i) j = rand.Next(n);

                    T Ej = errors[j];

                    T oldAlphaI = alpha[i];
                    T oldAlphaStarI = alphaStar[i];
                    T oldAlphaJ = alpha[j];
                    T oldAlphaStarJ = alphaStar[j];

                    // 计算 L 和 H
                    T gamma = oldAlphaI - oldAlphaStarI + oldAlphaJ - oldAlphaStarJ;
                    T L, H;
                    if (gamma > zero)
                    {
                        L = T.Max(zero, gamma - C);
                        H = T.Min(C, gamma);
                    }
                    else
                    {
                        L = T.Max(zero, gamma);
                        H = T.Min(C, gamma + C);
                    }

                    if (L == H) continue;

                    T kii = ComputeKernel(xData[i], xData[i], lengthScale);
                    T kjj = ComputeKernel(xData[j], xData[j], lengthScale);
                    T kij = ComputeKernel(xData[i], xData[j], lengthScale);
                    T eta = kii + kjj - T.CreateChecked(2) * kij;

                    if (eta <= zero) continue;

                    // 更新 alpha[i] 和 alpha[j]
                    T newAlphaI = oldAlphaI + (Ei - Ej) / eta;
                    newAlphaI = T.Max(L, T.Min(H, newAlphaI));
                    T deltaAlphaI = newAlphaI - oldAlphaI;
                    alpha[i] = newAlphaI;
                    alpha[j] = oldAlphaJ - deltaAlphaI;

                    // 更新 alphaStar[i] 和 alphaStar[j]
                    T newAlphaStarI = oldAlphaStarI - (Ei + Ej - T.CreateChecked(2) * epsilon) / eta;
                    newAlphaStarI = T.Max(zero, T.Min(C, newAlphaStarI));
                    T deltaAlphaStarI = newAlphaStarI - oldAlphaStarI;
                    alphaStar[i] = newAlphaStarI;
                    alphaStar[j] = oldAlphaStarJ - deltaAlphaStarI;

                    // 更新 b
                    T b1 = b - Ei - deltaAlphaI * kii - deltaAlphaStarI * kij;
                    T b2 = b - Ej - deltaAlphaI * kij - deltaAlphaStarI * kjj;
                    if (alpha[i] > zero && alpha[i] < C)
                        b = b1;
                    else if (alpha[j] > zero && alpha[j] < C)
                        b = b2;
                    else
                        b = (b1 + b2) / T.CreateChecked(2);

                    // 更新误差
                    for (int k = 0; k < n; k++)
                        errors[k] = ComputeError(xData, yData, alpha, alphaStar, b, xData[k], lengthScale) - yData[k];

                    numChanged++;
                }
            }
            if (numChanged == 0) break;
        }
        return (alpha, alphaStar, b);
    }

    // 单变量误差计算
    private static T ComputeError<T>(T[] xData, T[] yData, T[] alpha, T[] alphaStar, T b, T xi, T lengthScale)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (int j = 0; j < xData.Length; j++)
            sum += (alpha[j] - alphaStar[j]) * ComputeKernel(xi, xData[j], lengthScale);
        return sum + b;
    }

    // 多变量误差计算
    private static T ComputeError<T>(DataRow<T>[] xData, T[] yData, T[] alpha, T[] alphaStar, T b, DataRow<T> xi, T lengthScale)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (int j = 0; j < xData.Length; j++)
            sum += (alpha[j] - alphaStar[j]) * ComputeKernel(xi, xData[j], lengthScale);
        return sum + b;
    }

    // 测试方法
    public static void RunTests()
    {
        Console.WriteLine("Running Support Vector Regression Tests...");

        TestSingleVariableModel();
        TestMultiVariableModel();
        TestInvalidInput();

        Console.WriteLine("All tests completed!");
    }

    private static void TestSingleVariableModel()
    {
        Console.WriteLine("\nTest 1: Single-Variable Model (y = x^2)");

        double[] xData = { 0, 1, 2, 3, 4 };
        double[] yData = xData.Select(x => x * x).ToArray();

        var result = SupportVectorRegression.Fit(xData, yData, epsilon: 0.1, C: 100.0, lengthScale: 0.5);

        double tolerance = 0.5;
        bool mseOk = result.MeanSquaredError < 1.0;
        Console.WriteLine($"MSE = {result.MeanSquaredError:F6} (Expected small): {(mseOk ? "PASS" : "FAIL")}");

        double testInput = 2.5;
        double expected = testInput * testInput;
        double predicted = result.Predict(testInput);
        bool predictOk = Math.Abs(predicted - expected) < tolerance;
        Console.WriteLine($"Prediction at 2.5 = {predicted:F3} (Expected ~{expected:F3}): {(predictOk ? "PASS" : "FAIL")}");
    }

    private static void TestMultiVariableModel()
    {
        Console.WriteLine("\nTest 2: Multi-Variable Model (y = x1 + x2^2)");

        DataRow<double>[] xData = new DataRow<double>[]
        {
            new DataRow<double>(0, 0),
            new DataRow<double>(1, 1),
            new DataRow<double>(2, 2),
            new DataRow<double>(3, 3)
        };
        double[] yData = xData.Select(x => x[0] + x[1] * x[1]).ToArray();

        var result = SupportVectorRegression.Fit(xData, yData, epsilon: 0.1, C: 100.0, lengthScale: 0.5);

        double tolerance = 0.5;
        bool mseOk = result.MeanSquaredError < 1.0;
        Console.WriteLine($"MSE = {result.MeanSquaredError:F6} (Expected small): {(mseOk ? "PASS" : "FAIL")}");

        DataRow<double> testInput = new DataRow<double>(1.5, 1.5);
        double expected = 1.5 + 1.5 * 1.5;
        double predicted = result.Predict!(testInput);
        bool predictOk = Math.Abs(predicted - expected) < tolerance;
        Console.WriteLine($"Prediction at [1.5, 1.5] = {predicted:F3} (Expected ~{expected:F3}): {(predictOk ? "PASS" : "FAIL")}");
    }

    private static void TestInvalidInput()
    {
        Console.WriteLine("\nTest 3: Invalid Input");

        double[] emptyXSingle = Array.Empty<double>();
        double[] emptyYSingle = Array.Empty<double>();
        bool threwEmptySingle = false;
        try
        {
            SupportVectorRegression.Fit<double>(emptyXSingle, emptyYSingle);
        }
        catch (ArgumentException)
        {
            threwEmptySingle = true;
        }

        double[] mismatchXSingle = { 1, 2 };
        double[] mismatchYSingle = { 1 };
        bool threwMismatchSingle = false;
        try
        {
            SupportVectorRegression.Fit<double>(mismatchXSingle, mismatchYSingle);
        }
        catch (ArgumentException)
        {
            threwMismatchSingle = true;
        }

        DataRow<double>[] emptyXMulti = Array.Empty<DataRow<double>>();
        double[] emptyYMulti = Array.Empty<double>();
        bool threwEmptyMulti = false;
        try
        {
            SupportVectorRegression.Fit(emptyXMulti, emptyYMulti);
        }
        catch (ArgumentException)
        {
            threwEmptyMulti = true;
        }

        DataRow<double>[] mismatchXMulti = new DataRow<double>[] { new DataRow<double>(1), new DataRow<double>(2) };
        double[] mismatchYMulti = { 1 };
        bool threwMismatchMulti = false;
        try
        {
            SupportVectorRegression.Fit(mismatchXMulti, mismatchYMulti);
        }
        catch (ArgumentException)
        {
            threwMismatchMulti = true;
        }

        DataRow<double>[] inconsistentX = new DataRow<double>[] { new DataRow<double>(1), new DataRow<double>(1, 2) };
        double[] validY = { 1, 2 };
        bool threwInconsistentDim = false;
        try
        {
            SupportVectorRegression.Fit(inconsistentX, validY);
        }
        catch (ArgumentException)
        {
            threwInconsistentDim = true;
        }

        Console.WriteLine($"Empty data (single) test: {(threwEmptySingle ? "PASS" : "FAIL")}");
        Console.WriteLine($"Mismatched lengths (single) test: {(threwMismatchSingle ? "PASS" : "FAIL")}");
        Console.WriteLine($"Empty data (multi) test: {(threwEmptyMulti ? "PASS" : "FAIL")}");
        Console.WriteLine($"Mismatched lengths (multi) test: {(threwMismatchMulti ? "PASS" : "FAIL")}");
        Console.WriteLine($"Inconsistent input dimension test: {(threwInconsistentDim ? "PASS" : "FAIL")}");
    }
}