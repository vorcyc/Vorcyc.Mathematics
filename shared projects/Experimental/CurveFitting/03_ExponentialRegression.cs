using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class ExponentialRegression
{
    /// <summary>
    /// 指数回归：拟合 y = a * exp(b * x)。
    /// </summary>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResult<T> Fit_Normal<T>(Span<T> xData, Span<T> yData)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = xData.Length;

        // Step 1: 对 yData 取对数
        T[] logYData = new T[n];
        for (int i = 0; i < n; i++)
        {
            if (yData[i] <= T.Zero)
                throw new ArgumentException("yData must be positive for logarithm transformation.");
            logYData[i] = T.Log(yData[i]);
        }

        // Step 2: 使用线性回归拟合 log(y) = log(a) + b * x
        var linearResult = PolynomialRegression.Fit_Normal(xData, logYData, 1);
        T logA = linearResult.Parameters[0];
        T b = linearResult.Parameters[1];
        T a = T.Exp(logA);

        // Step 3: 定义预测函数
        Func<T, T> predict = x => a * T.Exp(b * x);

        // Step 4: 计算均方误差 (MSE)
        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T error = yData[i] - predict(xData[i]);
            mse += error * error;
        }
        mse /= T.CreateChecked(n);

        return new FitResult<T>(predict, [a, b], mse);
    }

    /// <summary>
    /// 指数回归：拟合 y = a * exp(b * x)，使用 SIMD 优化。
    /// </summary>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResult<T> Fit_SIMD<T>(Span<T> xData, Span<T> yData)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = xData.Length;
        int vectorSize = Vector<T>.Count; // half 就不行，还是只能支持 float 或 double

        if (n < 2)
            throw new ArgumentException("At least two data points are required.");
        if (xData.Length != yData.Length)
            throw new ArgumentException("xData and yData must have the same length.");

        // Step 1: 对 yData 取对数（使用 SIMD）
        T[] logYData = new T[n];
        int i = 0;
        for (; i <= n - vectorSize; i += vectorSize)
        {
            var yVec = new Vector<T>(yData.Slice(i, vectorSize));
            for (int j = 0; j < vectorSize; j++)
            {
                if (yVec[j] <= T.Zero)
                    throw new ArgumentException("yData must be positive for logarithm transformation.");
                logYData[i + j] = T.Log(yVec[j]);
            }
        }
        // 处理剩余元素
        for (; i < n; i++)
        {
            if (yData[i] <= T.Zero)
                throw new ArgumentException("yData must be positive for logarithm transformation.");
            logYData[i] = T.Log(yData[i]);
        }

        // Step 2: 使用线性回归拟合 log(y) = log(a) + b * x
        var linearResult = PolynomialRegression.Fit_SIMD(xData, logYData, 1);
        T logA = linearResult.Parameters[0];
        T b = linearResult.Parameters[1];
        T a = T.Exp(logA);

        // Step 3: 定义预测函数
        Func<T, T> predict = x => a * T.Exp(b * x);

        // Step 4: 计算均方误差 (MSE) 使用 SIMD
        T mse = T.Zero;
        i = 0;
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

        return new FitResult<T>(predict, new T[] { a, b }, mse);
    }

    internal static void TEST()
    {
        // 生成 1000 个测试数据点
        int dataSize = 1000;
        var xData = new float[dataSize];
        var yData = new float[dataSize];

        // 填充数据：y = 2 * exp(0.005 * x)
        for (int i = 0; i < dataSize; i++)
        {
            xData[i] = (i * 0.1f);         // x 从 0 到 99.9，步长 0.1
            yData[i] = (2.0f * MathF.Exp(0.005f * i * 0.1f)); // y = 2 * exp(0.005 * x)
        }

        // 调用 Exponential 方法，拟合指数函数
        var result = Fit_SIMD<float>(xData, yData);

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
            var predicted = result.Predict(xData[i]);
            Console.WriteLine($"x: {xData[i]:F1}, y实际: {yData[i]:F1}, y预测: {predicted:F4}");
        }
        for (int i = dataSize - 5; i < dataSize; i++)
        {
            var predicted = result.Predict(xData[i]);
            Console.WriteLine($"x: {xData[i]:F1}, y实际: {yData[i]:F1}, y预测: {predicted:F4}");
        }



    }
}
