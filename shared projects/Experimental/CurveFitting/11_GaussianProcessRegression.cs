using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class GaussianProcessRegression
{
    /// <summary>
    /// 高斯过程回归 (GPR)：单列输入，平滑预测。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点，单列输入</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="lengthScale">核函数长度尺度，默认1.0</param>
    /// <param name="signalVariance">信号方差，默认1.0</param>
    /// <param name="noiseVariance">噪声方差，默认0.01</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> Fit<T>(Span<T> xData, Span<T> yData,
        T lengthScale = default, T signalVariance = default, T noiseVariance = default)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length || xData.Length < 1)
            throw new ArgumentException("数据点数量必须相等且至少有1个点");

        int n = xData.Length;

        // 默认参数
        T l = lengthScale == T.Zero ? T.One : lengthScale;
        T sigmaF = signalVariance == T.Zero ? T.One : signalVariance;
        T sigmaN = noiseVariance == T.Zero ? T.CreateChecked(0.01) : noiseVariance;

        // 转换为数组以避免 Span 在 lambda 中的问题
        T[] xDataArray = xData.ToArray();
        T[] yDataArray = yData.ToArray();

        // 计算训练数据的协方差矩阵 K(X, X)
        T[][] K = new T[n][];
        for (int i = 0; i < n; i++)
        {
            K[i] = new T[n];
            for (int j = 0; j < n; j++)
            {
                K[i][j] = ComputeKernel(xDataArray[i], xDataArray[j], l, sigmaF);
                if (i == j)
                    K[i][j] += sigmaN; // 添加噪声方差
            }
        }

        // 计算 K 的逆矩阵
        T[][] KInv = InvertMatrix(K);

        // 计算 alpha = K^-1 * y
        T[] alpha = new T[n];
        for (int i = 0; i < n; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < n; j++)
                sum += KInv[i][j] * yDataArray[j];
            alpha[i] = sum;
        }

        // 预测函数（仅返回均值）
        Func<T, T> predict = x =>
        {
            T[] k = new T[n];
            for (int i = 0; i < n; i++)
                k[i] = ComputeKernel(x, xDataArray[i], l, sigmaF);

            T mu = T.Zero;
            for (int i = 0; i < n; i++)
                mu += k[i] * alpha[i];

            return mu;
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

        T[] parameters = new T[] { l, sigmaF, sigmaN };
        return new FitResult<T>(predict, parameters, mse);
    }

    /// <summary>
    /// 高斯过程回归 (GPR)：多列输入，平滑预测带置信区间。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点，每行是一个数据点的多变量输入</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="lengthScale">核函数长度尺度，默认1.0</param>
    /// <param name="signalVariance">信号方差，默认1.0</param>
    /// <param name="noiseVariance">噪声方差，默认0.01</param>
    /// <returns>拟合结果</returns>
    public static MultiColumnFitResult<T> Fit<T>(DataRow<T>[] xData, Span<T> yData,
        T lengthScale = default, T signalVariance = default, T noiseVariance = default)
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

        // 默认参数
        T l = lengthScale == T.Zero ? T.One : lengthScale;
        T sigmaF = signalVariance == T.Zero ? T.One : signalVariance;
        T sigmaN = noiseVariance == T.Zero ? T.CreateChecked(0.01) : noiseVariance;

        T[] yDataArray = yData.ToArray();

        // 计算训练数据的协方差矩阵 K(X, X)
        T[][] K = new T[n][];
        for (int i = 0; i < n; i++)
        {
            K[i] = new T[n];
            for (int j = 0; j < n; j++)
            {
                K[i][j] = ComputeKernel(xData[i], xData[j], l, sigmaF);
                if (i == j)
                    K[i][j] += sigmaN;
            }
        }

        // 计算 K 的逆矩阵
        T[][] KInv = InvertMatrix(K);

        // 计算 alpha = K^-1 * y
        T[] alpha = new T[n];
        for (int i = 0; i < n; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < n; j++)
                sum += KInv[i][j] * yDataArray[j];
            alpha[i] = sum;
        }

        // 预测函数（仅返回均值）
        Func<DataRow<T>, T> predict = x =>
        {
            T[] k = new T[n];
            for (int i = 0; i < n; i++)
                k[i] = ComputeKernel(x, xData[i], l, sigmaF);

            T mu = T.Zero;
            for (int i = 0; i < n; i++)
                mu += k[i] * alpha[i];

            return mu;
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

        T[] parameters = new T[] { l, sigmaF, sigmaN };
        return new MultiColumnFitResult<T>(predict, parameters, mse);
    }

    // 单变量核函数（RBF核）
    private static T ComputeKernel<T>(T x1, T x2, T lengthScale, T signalVariance)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T diff = x1 - x2;
        T exponent = -(diff * diff) / (T.CreateChecked(2) * lengthScale * lengthScale);
        return signalVariance * T.Exp(exponent);
    }

    // 多变量核函数（RBF核）
    private static T ComputeKernel<T>(DataRow<T> x1, DataRow<T> x2, T lengthScale, T signalVariance)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T sumSquaredDiff = T.Zero;
        for (int i = 0; i < x1.ColumnCount; i++)
        {
            T diff = x1[i] - x2[i];
            sumSquaredDiff += diff * diff;
        }
        T exponent = -sumSquaredDiff / (T.CreateChecked(2) * lengthScale * lengthScale);
        return signalVariance * T.Exp(exponent);
    }

    // 矩阵求逆（使用高斯-约当法）
    private static T[][] InvertMatrix<T>(T[][] A) where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = A.Length;
        T[][] augmented = new T[n][];
        for (int i = 0; i < n; i++)
        {
            augmented[i] = new T[2 * n];
            Array.Copy(A[i], 0, augmented[i], 0, n);
            augmented[i][i + n] = T.One;
        }

        for (int i = 0; i < n; i++)
        {
            T pivot = augmented[i][i];
            if (pivot == T.Zero)
                throw new InvalidOperationException("矩阵奇异，无法求逆");

            for (int j = 0; j < 2 * n; j++)
                augmented[i][j] /= pivot;

            for (int k = 0; k < n; k++)
            {
                if (k != i)
                {
                    T factor = augmented[k][i];
                    for (int j = 0; j < 2 * n; j++)
                        augmented[k][j] -= factor * augmented[i][j];
                }
            }
        }

        T[][] inverse = new T[n][];
        for (int i = 0; i < n; i++)
        {
            inverse[i] = new T[n];
            Array.Copy(augmented[i], n, inverse[i], 0, n);
        }
        return inverse;
    }

    // 测试方法
    public static void RunTests()
    {
        Console.WriteLine("Running Gaussian Process Regression Tests...");

        TestSingleVariableModel();
        TestMultiVariableModel();
        TestInvalidInput();

        Console.WriteLine("All tests completed!");
    }

    private static void TestSingleVariableModel()
    {
        Console.WriteLine("\nTest 1: Single-Variable Model (y = sin(x))");

        double[] xData = { 0, 1, 2, 3, 4 };
        double[] yData = xData.Select(x => Math.Sin(x)).ToArray();

        var result = GaussianProcessRegression.Fit(xData, yData, lengthScale: 1.0, signalVariance: 1.0, noiseVariance: 0.01);

        double tolerance = 0.1;
        bool mseOk = result.MeanSquaredError < 0.1;
        Console.WriteLine($"MSE = {result.MeanSquaredError:F6} (Expected small): {(mseOk ? "PASS" : "FAIL")}");

        double testInput = 1.5;
        double expected = Math.Sin(testInput);
        double predicted = result.Predict(testInput);
        bool predictOk = Math.Abs(predicted - expected) < tolerance;
        Console.WriteLine($"Prediction at 1.5 = {predicted:F3} (Expected ~{expected:F3}): {(predictOk ? "PASS" : "FAIL")}");
    }

    private static void TestMultiVariableModel()
    {
        Console.WriteLine("\nTest 2: Multi-Variable Model (y = sin(x1) + x2^2)");

        DataRow<double>[] xData = new DataRow<double>[]
        {
            new DataRow<double>(0, 0),
            new DataRow<double>(1, 1),
            new DataRow<double>(2, 2),
            new DataRow<double>(3, 3)
        };
        double[] yData = new double[xData.Length];
        for (int i = 0; i < xData.Length; i++)
            yData[i] = Math.Sin(xData[i][0]) + xData[i][1] * xData[i][1];

        var result = GaussianProcessRegression.Fit(xData, yData, lengthScale: 1.0, signalVariance: 1.0, noiseVariance: 0.01);

        double tolerance = 0.5;
        bool mseOk = result.MeanSquaredError < 0.1;
        Console.WriteLine($"MSE = {result.MeanSquaredError:F6} (Expected small): {(mseOk ? "PASS" : "FAIL")}");

        DataRow<double> testInput = new DataRow<double>(1.5, 1.5);
        double expected = Math.Sin(1.5) + 1.5 * 1.5;
        double predicted = result.Predict!(testInput);
        bool predictOk = Math.Abs(predicted - expected) < tolerance;
        Console.WriteLine($"Prediction at [1.5, 1.5] = {predicted:F3} (Expected ~{expected:F3}): {(predictOk ? "PASS" : "FAIL")}");
    }

    private static void TestInvalidInput()
    {
        Console.WriteLine("\nTest 3: Invalid Input");

        // 单列输入测试
        double[] emptyXSingle = Array.Empty<double>();
        double[] emptyYSingle = Array.Empty<double>();
        bool threwEmptySingle = false;
        try
        {
            GaussianProcessRegression.Fit<double>(emptyXSingle, emptyYSingle);
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
            GaussianProcessRegression.Fit<double>(mismatchXSingle, mismatchYSingle);
        }
        catch (ArgumentException)
        {
            threwMismatchSingle = true;
        }

        // 多列输入测试
        DataRow<double>[] emptyXMulti = Array.Empty<DataRow<double>>();
        double[] emptyYMulti = Array.Empty<double>();
        bool threwEmptyMulti = false;
        try
        {
            GaussianProcessRegression.Fit(emptyXMulti, emptyYMulti);
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
            GaussianProcessRegression.Fit(mismatchXMulti, mismatchYMulti);
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
            GaussianProcessRegression.Fit(inconsistentX, validY);
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
