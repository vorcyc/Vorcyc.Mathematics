using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class NonlinearRegression
{
    /// <summary>
    /// 非线性回归：拟合复杂非线性模型。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="model">非线性模型函数，形式为 f(x, parameters)</param>
    /// <param name="initialParams">初始参数猜测</param>
    /// <param name="maxIterations">最大迭代次数，默认100</param>
    /// <param name="tolerance">收敛容差，默认1e-6</param>
    /// <param name="initialLambda">初始阻尼因子，默认0.001</param>
    /// <param name="lambdaIncreaseFactor">阻尼因子放大因子，默认10</param>
    /// <param name="lambdaDecreaseFactor">阻尼因子缩小因子，默认10</param>
    /// <param name="stepSize">数值偏导数步长，默认1e-6</param>
    /// <param name="residualTolerance">残差平方和阈值，默认null（不启用）</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> Fit_Normal<T>(Span<T> xData, Span<T> yData,
        Func<T, T[], T> model, T[] initialParams,
        int maxIterations = 5000,
        T? tolerance = null,
        T? initialLambda = null,
        T? lambdaIncreaseFactor = null,
        T? lambdaDecreaseFactor = null,
        T? stepSize = null,
        T? residualTolerance = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length || xData.Length < 1)
            throw new ArgumentException("数据点数量必须相等且至少有1个点");
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        if (initialParams == null || initialParams.Length == 0)
            throw new ArgumentNullException(nameof(initialParams), "初始参数不能为空");
        if (maxIterations <= 0)
            throw new ArgumentException("最大迭代次数必须大于0");

        // 转换为数组以避免 Span 在 lambda 中的问题
        T[] xDataArray = xData.ToArray();
        T[] yDataArray = yData.ToArray();

        int n = xDataArray.Length;
        int m = initialParams.Length;

        // 参数优化
        T[] parameters = (T[])initialParams.Clone();

        // 配置参数
        T defaultTolerance = T.CreateChecked(1e-6);
        T effectiveTolerance = tolerance ?? defaultTolerance;
        T defaultLambda = T.CreateChecked(0.001);
        T currentLambda = initialLambda ?? defaultLambda;
        T defaultIncrease = T.CreateChecked(10);
        T defaultDecrease = T.CreateChecked(10);
        T increaseFactor = lambdaIncreaseFactor ?? defaultIncrease;
        T decreaseFactor = lambdaDecreaseFactor ?? defaultDecrease;
        T defaultStepSize = T.CreateChecked(1e-6);
        T h = stepSize ?? defaultStepSize;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            // 计算残差
            T[] residuals = new T[n];
            for (int i = 0; i < n; i++)
                residuals[i] = model(xDataArray[i], parameters) - yDataArray[i];

            // 计算雅可比矩阵（数值近似）
            T[][] jacobian = new T[n][];
            for (int i = 0; i < n; i++)
            {
                jacobian[i] = new T[m];
                for (int j = 0; j < m; j++)
                {
                    T[] tempParams = (T[])parameters.Clone();
                    tempParams[j] += h;
                    T f1 = model(xDataArray[i], tempParams);
                    tempParams[j] = parameters[j] - h;
                    T f2 = model(xDataArray[i], tempParams);
                    jacobian[i][j] = (f1 - f2) / (h + h);
                }
            }

            // 计算 J^T * J 和 J^T * r
            T[][] jtJ = new T[m][];
            for (int i = 0; i < m; i++)
            {
                jtJ[i] = new T[m];
                for (int j = 0; j < m; j++)
                {
                    T sum = T.Zero;
                    for (int k = 0; k < n; k++)
                        sum += jacobian[k][i] * jacobian[k][j];
                    jtJ[i][j] = sum + (i == j ? currentLambda * sum : T.Zero); // 添加阻尼
                }
            }

            T[] jtr = new T[m];
            for (int i = 0; i < m; i++)
            {
                T sum = T.Zero;
                for (int k = 0; k < n; k++)
                    sum += jacobian[k][i] * residuals[k];
                jtr[i] = -sum;
            }

            // 求解更新步长
            T[] delta = SolveLinearSystem(jtJ, jtr);

            // 更新参数并检查收敛
            T[] newParams = new T[m];
            T change = T.Zero;
            for (int i = 0; i < m; i++)
            {
                newParams[i] = parameters[i] + delta[i];
                change += delta[i] * delta[i];
            }

            // 计算新残差平方和
            T newSsr = T.Zero;
            for (int i = 0; i < n; i++)
            {
                T r = model(xDataArray[i], newParams) - yDataArray[i];
                newSsr += r * r;
            }

            T oldSsr = T.Zero;
            for (int i = 0; i < n; i++)
                oldSsr += residuals[i] * residuals[i];

            // 根据残差调整 lambda 并检查终止条件
            if (newSsr < oldSsr)
            {
                Array.Copy(newParams, parameters, m);
                currentLambda /= decreaseFactor;
                if (T.Sqrt(change) < effectiveTolerance ||
                    (residualTolerance.HasValue && newSsr < residualTolerance.Value))
                    break;
            }
            else
            {
                currentLambda *= increaseFactor;
            }
        }

        // 预测函数
        Func<T, T> predict = x => model(x, parameters);

        // 计算 MSE
        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T predicted = predict(xDataArray[i]);
            T diff = predicted - yDataArray[i];
            mse += diff * diff;
        }
        mse /= T.CreateChecked(n);

        return new FitResult<T>(predict, parameters, mse);
    }

    // 高斯消元法求解线性方程组
    private static T[] SolveLinearSystem<T>(T[][] A, T[] b)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = b.Length;
        T[] x = new T[n];
        T[][] augmented = new T[n][];

        for (int i = 0; i < n; i++)
        {
            augmented[i] = new T[n + 1];
            Array.Copy(A[i], 0, augmented[i], 0, n);
            augmented[i][n] = b[i];
        }

        for (int i = 0; i < n; i++)
        {
            T pivot = augmented[i][i];
            if (pivot == T.Zero)
                throw new InvalidOperationException("矩阵奇异，无法求解");

            for (int j = i + 1; j < n; j++)
            {
                T factor = augmented[j][i] / pivot;
                for (int k = i; k <= n; k++)
                    augmented[j][k] -= factor * augmented[i][k];
            }
        }

        for (int i = n - 1; i >= 0; i--)
        {
            T sum = augmented[i][n];
            for (int j = i + 1; j < n; j++)
                sum -= augmented[i][j] * x[j];
            x[i] = sum / augmented[i][i];
        }

        return x;
    }


    /// <summary>
    /// 非线性回归：拟合复杂非线性模型，支持多变量输入。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点，每行是一个数据点的多变量输入</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="model">非线性模型函数，形式为 f(xVector, parameters)</param>
    /// <param name="initialParams">初始参数猜测</param>
    /// <param name="maxIterations">最大迭代次数，默认100</param>
    /// <param name="tolerance">收敛容差，默认1e-6</param>
    /// <param name="initialLambda">初始阻尼因子，默认0.001</param>
    /// <param name="lambdaIncreaseFactor">阻尼因子放大因子，默认10</param>
    /// <param name="lambdaDecreaseFactor">阻尼因子缩小因子，默认10</param>
    /// <param name="stepSize">数值偏导数步长，默认1e-6</param>
    /// <param name="residualTolerance">残差平方和阈值，默认null（不启用）</param>
    /// <returns>拟合结果</returns>
    public static MultiColumnFitResult<T> Fit_MultiColumn_Normal<T>(DataRow<T>[] xData, Span<T> yData,
        Func<DataRow<T>, T[], T> model, T[] initialParams,
        int maxIterations = 5000,
        T? tolerance = null,
        T? initialLambda = null,
        T? lambdaIncreaseFactor = null,
        T? lambdaDecreaseFactor = null,
        T? stepSize = null,
        T? residualTolerance = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length || xData.Length < 1)
            throw new ArgumentException("数据点数量必须相等且至少有1个点");
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        if (initialParams == null || initialParams.Length == 0)
            throw new ArgumentNullException(nameof(initialParams), "初始参数不能为空");
        if (maxIterations <= 0)
            throw new ArgumentException("最大迭代次数必须大于0");
        if (xData.Any(row => row.ColumnCount == 0))
            throw new ArgumentException("X 数据中不能包含空行");

        int n = xData.Length; // 数据点数
        int m = initialParams.Length; // 参数数
        int inputDim = xData[0].ColumnCount; // 输入变量维度
        if (xData.Any(row => row.ColumnCount != inputDim))
            throw new ArgumentException("所有数据点的输入维度必须一致");

        // 转换为数组以避免 Span 在 lambda 中的问题
        T[] yDataArray = yData.ToArray();

        // 参数优化
        T[] parameters = (T[])initialParams.Clone();

        // 配置参数
        T defaultTolerance = T.CreateChecked(1e-6);
        T effectiveTolerance = tolerance ?? defaultTolerance;
        T defaultLambda = T.CreateChecked(0.001);
        T currentLambda = initialLambda ?? defaultLambda;
        T defaultIncrease = T.CreateChecked(10);
        T defaultDecrease = T.CreateChecked(10);
        T increaseFactor = lambdaIncreaseFactor ?? defaultIncrease;
        T decreaseFactor = lambdaDecreaseFactor ?? defaultDecrease;
        T defaultStepSize = T.CreateChecked(1e-6);
        T h = stepSize ?? defaultStepSize;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            // 计算残差
            T[] residuals = new T[n];
            for (int i = 0; i < n; i++)
                residuals[i] = model(xData[i], parameters) - yDataArray[i];

            // 计算雅可比矩阵（数值近似）
            T[][] jacobian = new T[n][];
            for (int i = 0; i < n; i++)
            {
                jacobian[i] = new T[m];
                for (int j = 0; j < m; j++)
                {
                    T[] tempParams = (T[])parameters.Clone();
                    tempParams[j] += h;
                    T f1 = model(xData[i], tempParams);
                    tempParams[j] = parameters[j] - h;
                    T f2 = model(xData[i], tempParams);
                    jacobian[i][j] = (f1 - f2) / (h + h);
                }
            }

            // 计算 J^T * J 和 J^T * r
            T[][] jtJ = new T[m][];
            for (int i = 0; i < m; i++)
            {
                jtJ[i] = new T[m];
                for (int j = 0; j < m; j++)
                {
                    T sum = T.Zero;
                    for (int k = 0; k < n; k++)
                        sum += jacobian[k][i] * jacobian[k][j];
                    jtJ[i][j] = sum + (i == j ? currentLambda * sum : T.Zero);
                }
            }

            T[] jtr = new T[m];
            for (int i = 0; i < m; i++)
            {
                T sum = T.Zero;
                for (int k = 0; k < n; k++)
                    sum += jacobian[k][i] * residuals[k];
                jtr[i] = -sum;
            }

            // 求解更新步长
            T[] delta = SolveLinearSystem(jtJ, jtr);

            // 更新参数并检查收敛
            T[] newParams = new T[m];
            T change = T.Zero;
            for (int i = 0; i < m; i++)
            {
                newParams[i] = parameters[i] + delta[i];
                change += delta[i] * delta[i];
            }

            // 计算新残差平方和
            T newSsr = T.Zero;
            for (int i = 0; i < n; i++)
            {
                T r = model(xData[i], newParams) - yDataArray[i];
                newSsr += r * r;
            }

            T oldSsr = T.Zero;
            for (int i = 0; i < n; i++)
                oldSsr += residuals[i] * residuals[i];

            // 根据残差调整 lambda 并检查终止条件
            if (newSsr < oldSsr)
            {
                Array.Copy(newParams, parameters, m);
                currentLambda /= decreaseFactor;
                if (T.Sqrt(change) < effectiveTolerance ||
                    (residualTolerance.HasValue && newSsr < residualTolerance.Value))
                    break;
            }
            else
            {
                currentLambda *= increaseFactor;
            }
        }

        // 预测函数
        Func<DataRow<T>, T> predict = x => model(x, parameters);

        // 计算 MSE
        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T predicted = predict(xData[i]);
            T diff = predicted - yDataArray[i];
            mse += diff * diff;
        }
        mse /= T.CreateChecked(n);

        return new MultiColumnFitResult<T>(predict, parameters, mse);
    }



    // 测试方法
    public static void RunTests()
    {
        Console.WriteLine("Running Nonlinear Fit Tests...");

        TestMultiVariableModel();
        TestInvalidInput();

        Console.WriteLine("All tests completed!");
    }

    private static void TestMultiVariableModel()
    {
        Console.WriteLine("\nTest 1: Multi-Variable Model (y = a * x1 + b * x2^2)");

        DataRow<double>[] xData = new DataRow<double>[]
        {
            new DataRow<double>(1, 1),
            new DataRow<double>(2, 2),
            new DataRow<double>(3, 3),
            new DataRow<double>(4, 4)
        };
        double[] yData = { 2, 10, 24, 44 }; // y ≈ 2*x1 + 3*x2^2
        Func<DataRow<double>, double[], double> model = (x, p) => p[0] * x[0] + p[1] * x[1] * x[1];
        double[] initialParams = { 1.0, 1.0 };

        var result = NonlinearRegression.Fit_MultiColumn_Normal(xData, yData, model, initialParams);

        double tolerance = 0.1;
        bool aOk = Math.Abs(result.Parameters[0] - 2.0) < tolerance;
        bool bOk = Math.Abs(result.Parameters[1] - 3.0) < tolerance;
        Console.WriteLine($"a = {result.Parameters[0]:F3} (Expected ~2): {(aOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"b = {result.Parameters[1]:F3} (Expected ~3): {(bOk ? "PASS" : "FAIL")}");
        Console.WriteLine($"MSE = {result.MeanSquaredError:F6} (Expected small): {(result.MeanSquaredError < 0.1 ? "PASS" : "FAIL")}");

        // 测试多列预测
        DataRow<double> testInput = new DataRow<double>(2.5, 2.5);
        double expected = 2.0 * 2.5 + 3.0 * 2.5 * 2.5; // 5 + 18.75 = 23.75
        double predicted = result.Predict!(testInput);
        bool predictOk = Math.Abs(predicted - expected) < tolerance;
        Console.WriteLine($"Prediction at [2.5, 2.5] = {predicted:F3} (Expected ~23.75): {(predictOk ? "PASS" : "FAIL")}");
    }

    private static void TestInvalidInput()
    {
        Console.WriteLine("\nTest 2: Invalid Input");

        DataRow<double>[] emptyX = Array.Empty<DataRow<double>>();
        double[] emptyY = Array.Empty<double>();
        bool threwEmpty = false;
        try
        {
            NonlinearRegression.Fit_MultiColumn_Normal(emptyX, emptyY, (x, p) => p[0] * x[0], new double[] { 1 });
        }
        catch (ArgumentException)
        {
            threwEmpty = true;
        }

        DataRow<double>[] mismatchX = new DataRow<double>[] { new DataRow<double>(1), new DataRow<double>(2) };
        double[] mismatchY = { 1 };
        bool threwMismatch = false;
        try
        {
            NonlinearRegression.Fit_MultiColumn_Normal(mismatchX, mismatchY, (x, p) => p[0] * x[0], new double[] { 1 });
        }
        catch (ArgumentException)
        {
            threwMismatch = true;
        }

        DataRow<double>[] validX = new DataRow<double>[] { new DataRow<double>(1), new DataRow<double>(2) };
        double[] validY = { 1, 2 };
        bool threwNullModel = false;
        try
        {
            NonlinearRegression.Fit_MultiColumn_Normal<double>(validX, validY, null, new double[] { 1 });
        }
        catch (ArgumentNullException)
        {
            threwNullModel = true;
        }

        bool threwNullParams = false;
        try
        {
            NonlinearRegression.Fit_MultiColumn_Normal(validX, validY, (x, p) => p[0] * x[0], null);
        }
        catch (ArgumentNullException)
        {
            threwNullParams = true;
        }

        bool threwInvalidMaxIter = false;
        try
        {
            NonlinearRegression.Fit_MultiColumn_Normal(validX, validY, (x, p) => p[0] * x[0], new double[] { 1 }, maxIterations: 0);
        }
        catch (ArgumentException)
        {
            threwInvalidMaxIter = true;
        }

        DataRow<double>[] inconsistentX = new DataRow<double>[] { new DataRow<double>(1), new DataRow<double>(1, 2) };
        bool threwInconsistentDim = false;
        try
        {
            NonlinearRegression.Fit_MultiColumn_Normal(inconsistentX, validY, (x, p) => p[0] * x[0], new double[] { 1 });
        }
        catch (ArgumentException)
        {
            threwInconsistentDim = true;
        }

        Console.WriteLine($"Empty data test: {(threwEmpty ? "PASS" : "FAIL")}");
        Console.WriteLine($"Mismatched lengths test: {(threwMismatch ? "PASS" : "FAIL")}");
        Console.WriteLine($"Null model test: {(threwNullModel ? "PASS" : "FAIL")}");
        Console.WriteLine($"Null parameters test: {(threwNullParams ? "PASS" : "FAIL")}");
        Console.WriteLine($"Invalid max iterations test: {(threwInvalidMaxIter ? "PASS" : "FAIL")}");
        Console.WriteLine($"Inconsistent input dimension test: {(threwInconsistentDim ? "PASS" : "FAIL")}");
    }
}

