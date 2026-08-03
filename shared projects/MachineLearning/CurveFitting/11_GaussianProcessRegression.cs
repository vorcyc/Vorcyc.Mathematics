using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.CurveFitting;

internal static class GaussianProcessRegression
{
    /// <summary>
    /// 高斯过程回归 (GPR)：单列输入，平滑预测（含预测标准差 / 方差）。
    /// </summary>
    public static GaussianProcessFitResult<T> Fit<T>(Span<T> xData, Span<T> yData,
        T lengthScale = default, T signalVariance = default, T noiseVariance = default,
        ComputingContext? computingContext = null,
        CancellationToken cancellationToken = default)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length || xData.Length < 1)
            throw new ArgumentException("数据点数量必须相等且至少有1个点");

        int n = xData.Length;
        T l = lengthScale == T.Zero ? T.One : lengthScale;
        T sigmaF = signalVariance == T.Zero ? T.One : signalVariance;
        T sigmaN = noiseVariance == T.Zero ? T.CreateChecked(0.01) : noiseVariance;

        T[] xDataArray = xData.ToArray();
        T[] yDataArray = yData.ToArray();

        // O(n²) 核：count=n, workPerItem=n → 总工作量 n²（交由 ComputingContextExecution 分发）
        var dispatch = CurveFittingExecution.ResolveDispatch<T>(computingContext, n, n);
        bool useSimd = dispatch == CurveFitDispatchKind.Simd;

        T[][] K = new T[n][];
        for (int i = 0; i < n; i++)
            K[i] = new T[n];

        ComputingContextExecution.ForEach(computingContext, 0, n, i =>
        {
            CurveFittingExecution.FillRbfKernelRow(xDataArray, xDataArray[i], l, sigmaF, K[i], useSimd);
            K[i][i] += sigmaN;
        }, workPerItem: n, cancellationToken: cancellationToken);

        T[][] KInv = InvertMatrix(K, computingContext, cancellationToken);

        T[] alpha = new T[n];
        ComputingContextExecution.ForEach(computingContext, 0, n, i =>
        {
            alpha[i] = CurveFittingExecution.Dot<T>(KInv[i], yDataArray, useSimd);
        }, workPerItem: n, cancellationToken: cancellationToken);

        Func<T, T> predict = x =>
        {
            T[] k = new T[n];
            CurveFittingExecution.FillRbfKernelRow(xDataArray, x, l, sigmaF, k, useSimd);
            return CurveFittingExecution.Dot<T>(k, alpha, useSimd);
        };

        // Var(f*) = k(x*,x*) − k*^T K⁻¹ k*；RBF 自核 k(x*,x*) = σ_f²
        Func<T, T> predictVariance = x =>
        {
            T[] k = new T[n];
            CurveFittingExecution.FillRbfKernelRow(xDataArray, x, l, sigmaF, k, useSimd);
            T[] v = new T[n];
            for (int i = 0; i < n; i++)
                v[i] = CurveFittingExecution.Dot<T>(KInv[i], k, useSimd);
            T variance = sigmaF - CurveFittingExecution.Dot<T>(k, v, useSimd);
            return variance < T.Zero ? T.Zero : variance;
        };

        T[] fitted = new T[n];
        ComputingContextExecution.ForEach(computingContext, 0, n, i =>
        {
            fitted[i] = predict(xDataArray[i]);
        }, workPerItem: n, cancellationToken: cancellationToken);

        T mse = CurveFittingExecution.MeanSquaredError<T>(fitted, yDataArray, useSimd);
        return new GaussianProcessFitResult<T>(predict, predictVariance, [l, sigmaF, sigmaN], mse);
    }

    /// <summary>
    /// 高斯过程回归 (GPR)：多列输入，平滑预测带置信区间。
    /// </summary>
    public static MultiColumnGaussianProcessFitResult<T> Fit<T>(CurveFitRow<T>[] xData, Span<T> yData,
        T lengthScale = default, T signalVariance = default, T noiseVariance = default,
        ComputingContext? computingContext = null,
        CancellationToken cancellationToken = default)
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

        T l = lengthScale == T.Zero ? T.One : lengthScale;
        T sigmaF = signalVariance == T.Zero ? T.One : signalVariance;
        T sigmaN = noiseVariance == T.Zero ? T.CreateChecked(0.01) : noiseVariance;

        T[] yDataArray = yData.ToArray();
        long kernelWork = (long)n * Math.Max(1, inputDim);
        var dispatch = CurveFittingExecution.ResolveDispatch<T>(computingContext, n, kernelWork);
        bool useSimd = dispatch == CurveFitDispatchKind.Simd;

        T[][] K = new T[n][];
        for (int i = 0; i < n; i++)
            K[i] = new T[n];

        ComputingContextExecution.ForEach(computingContext, 0, n, i =>
        {
            for (int j = 0; j < n; j++)
            {
                K[i][j] = ComputeKernel(xData[i], xData[j], l, sigmaF);
                if (i == j)
                    K[i][j] += sigmaN;
            }
        }, workPerItem: kernelWork, cancellationToken: cancellationToken);

        T[][] KInv = InvertMatrix(K, computingContext, cancellationToken);

        T[] alpha = new T[n];
        ComputingContextExecution.ForEach(computingContext, 0, n, i =>
        {
            alpha[i] = CurveFittingExecution.Dot<T>(KInv[i], yDataArray, useSimd);
        }, workPerItem: n, cancellationToken: cancellationToken);

        Func<CurveFitRow<T>, T> predict = x =>
        {
            T[] k = new T[n];
            for (int i = 0; i < n; i++)
                k[i] = ComputeKernel(x, xData[i], l, sigmaF);
            return CurveFittingExecution.Dot<T>(k, alpha, useSimd);
        };

        Func<CurveFitRow<T>, T> predictVariance = x =>
        {
            T[] k = new T[n];
            for (int i = 0; i < n; i++)
                k[i] = ComputeKernel(x, xData[i], l, sigmaF);
            T[] v = new T[n];
            for (int i = 0; i < n; i++)
                v[i] = CurveFittingExecution.Dot<T>(KInv[i], k, useSimd);
            T variance = sigmaF - CurveFittingExecution.Dot<T>(k, v, useSimd);
            return variance < T.Zero ? T.Zero : variance;
        };

        T[] fitted = new T[n];
        ComputingContextExecution.ForEach(computingContext, 0, n, i =>
        {
            fitted[i] = predict(xData[i]);
        }, workPerItem: n, cancellationToken: cancellationToken);

        T mse = CurveFittingExecution.MeanSquaredError<T>(fitted, yDataArray, useSimd);
        return new MultiColumnGaussianProcessFitResult<T>(predict, predictVariance, [l, sigmaF, sigmaN], mse);
    }

    private static T ComputeKernel<T>(T x1, T x2, T lengthScale, T signalVariance)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T diff = x1 - x2;
        T exponent = -(diff * diff) / (T.CreateChecked(2) * lengthScale * lengthScale);
        return signalVariance * T.Exp(exponent);
    }

    private static T ComputeKernel<T>(CurveFitRow<T> x1, CurveFitRow<T> x2, T lengthScale, T signalVariance)
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

    /// <summary>
    /// 高斯-约当求逆。消元行更新经 <see cref="ComputingContextExecution.ForEach"/> 分发
    ///（与 LinearAlgebra 矩阵乘同一套 ComputingContext 策略）；每主元步可取消。
    /// </summary>
    private static T[][] InvertMatrix<T>(
        T[][] A,
        ComputingContext? computingContext,
        CancellationToken cancellationToken)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = A.Length;
        T[][] augmented = new T[n][];
        for (int i = 0; i < n; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            augmented[i] = new T[2 * n];
            Array.Copy(A[i], 0, augmented[i], 0, n);
            augmented[i][i + n] = T.One;
        }

        // 每主元：对 n 行各做约 2n 次更新 → workPerItem=2n，总工作量 2n²
        // 是否并行完全由 ComputingContextExecution（与 Matrix / 核装配同一套策略）
        long eliminateWorkPerRow = 2L * n;

        for (int i = 0; i < n; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            T pivot = augmented[i][i];
            if (pivot == T.Zero)
                throw new InvalidOperationException("矩阵奇异，无法求逆");

            for (int j = 0; j < 2 * n; j++)
                augmented[i][j] /= pivot;

            int pivotIndex = i;
            T[] pivotRow = augmented[i];
            ComputingContextExecution.ForEach(computingContext, 0, n, k =>
            {
                if (k == pivotIndex)
                    return;
                T factor = augmented[k][pivotIndex];
                if (factor == T.Zero)
                    return;
                T[] row = augmented[k];
                for (int j = 0; j < 2 * n; j++)
                    row[j] -= factor * pivotRow[j];
            }, workPerItem: eliminateWorkPerRow, cancellationToken: cancellationToken);
        }

        T[][] inverse = new T[n][];
        for (int i = 0; i < n; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            inverse[i] = new T[n];
            Array.Copy(augmented[i], n, inverse[i], 0, n);
        }
        return inverse;
    }

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

        double stdAtTrain = result.PredictStd(1.0);
        double stdFar = result.PredictStd(10.0);
        bool stdOk = stdAtTrain >= 0 && stdFar >= stdAtTrain;
        Console.WriteLine($"PredictStd(1.0)={stdAtTrain:F4}, PredictStd(10.0)={stdFar:F4} (far ≥ near): {(stdOk ? "PASS" : "FAIL")}");
    }

    private static void TestMultiVariableModel()
    {
        Console.WriteLine("\nTest 2: Multi-Variable Model (y = sin(x1) + x2^2)");

        CurveFitRow<double>[] xData = new CurveFitRow<double>[]
        {
            new CurveFitRow<double>(0, 0),
            new CurveFitRow<double>(1, 1),
            new CurveFitRow<double>(2, 2),
            new CurveFitRow<double>(3, 3)
        };
        double[] yData = new double[xData.Length];
        for (int i = 0; i < xData.Length; i++)
            yData[i] = Math.Sin(xData[i][0]) + xData[i][1] * xData[i][1];

        var result = GaussianProcessRegression.Fit(xData, yData, lengthScale: 1.0, signalVariance: 1.0, noiseVariance: 0.01);

        double tolerance = 0.5;
        bool mseOk = result.MeanSquaredError < 0.1;
        Console.WriteLine($"MSE = {result.MeanSquaredError:F6} (Expected small): {(mseOk ? "PASS" : "FAIL")}");

        CurveFitRow<double> testInput = new CurveFitRow<double>(1.5, 1.5);
        double expected = Math.Sin(1.5) + 1.5 * 1.5;
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

        CurveFitRow<double>[] emptyXMulti = Array.Empty<CurveFitRow<double>>();
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

        CurveFitRow<double>[] mismatchXMulti = new CurveFitRow<double>[] { new CurveFitRow<double>(1), new CurveFitRow<double>(2) };
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

        CurveFitRow<double>[] inconsistentX = new CurveFitRow<double>[] { new CurveFitRow<double>(1), new CurveFitRow<double>(1, 2) };
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
