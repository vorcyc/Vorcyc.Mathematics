using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class LogarithmicRegression
{
    /// <summary>
    /// 对数回归：拟合 y = a + b * ln(x)。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现 IFloatingPointIeee754 接口。</typeparam>
    /// <param name="xData">自变量数据数组。</param>
    /// <param name="yData">因变量数据数组。</param>
    /// <returns>返回拟合结果，包括预测函数、拟合参数和均方误差。</returns>
    /// <exception cref="ArgumentException">当 xData 和 yData 的长度不同时抛出。</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FitResult<T> Fit_Normal<T>(Span<T> xData, Span<T> yData)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length)
            throw new ArgumentException("The length of xData and yData must be the same.");

        int n = xData.Length;
        T tn = T.CreateChecked(n);

        // 计算各种和
        T sumX = T.Zero;
        T sumY = T.Zero;
        T sumLnX = T.Zero;
        T sumLnX2 = T.Zero;
        T sumYLnX = T.Zero;

        for (int i = 0; i < n; i++)
        {
            if (xData[i] <= T.Zero)
                throw new ArgumentException("xData must be positive for logarithm transformation.");

            T lnX = T.Log(xData[i]);
            sumX += xData[i];
            sumY += yData[i];
            sumLnX += lnX;
            sumLnX2 += lnX * lnX;
            sumYLnX += yData[i] * lnX;
        }

        // 计算系数 a 和 b
        T b = (tn * sumYLnX - sumY * sumLnX) / (tn * sumLnX2 - sumLnX * sumLnX);
        T a = (sumY - b * sumLnX) / tn;

        // 定义预测函数
        Func<T, T> predict = x => a + b * T.Log(x);

        // 计算均方误差
        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T error = yData[i] - predict(xData[i]);
            mse += error * error;
        }
        mse /= tn;

        // 返回拟合结果
        return new FitResult<T>(predict, new T[] { a, b }, mse);
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResult<T> Fit_SIMD<T>(Span<T> xData, Span<T> yData)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (typeof(T) == typeof(float))
        {
            var xDataFloat = MemoryMarshal.Cast<T, float>(xData);
            var yDataFloat = MemoryMarshal.Cast<T, float>(yData);
            return Fit_SIMD_Single(xDataFloat, yDataFloat).ToGeneric<T>();
        }
        else if (typeof(T) == typeof(double))
        {
            var xDataDouble = MemoryMarshal.Cast<T, double>(xData);
            var yDataDouble = MemoryMarshal.Cast<T, double>(yData);
            return Fit_SIMD_Double(xDataDouble, yDataDouble).ToGeneric<T>();
        }
        else
        {
            throw new NotSupportedException("Only float and double types are supported.");
        }
    }



    /// <summary>
    /// 对数回归：拟合 y = a + b * ln(x)，使用 SIMD 优化。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现 IFloatingPointIeee754 接口。</typeparam>
    /// <param name="xData">自变量数据数组。</param>
    /// <param name="yData">因变量数据数组。</param>
    /// <returns>返回拟合结果，包括预测函数、拟合参数和均方误差。</returns>
    /// <exception cref="ArgumentException">当 xData 和 yData 的长度不同时抛出。</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResultOfSingle Fit_SIMD_Single(Span<float> xData, Span<float> yData)
    {
        int n = xData.Length;
        int vectorSize = Vector<float>.Count;
        float tn = n;

        // 计算各种和
        var sumX = 0f;
        var sumY = 0f;
        var sumLnX = 0f;
        var sumLnX2 = 0f;
        var sumYLnX = 0f;

        int i = 0;
        for (; i <= n - vectorSize; i += vectorSize)
        {
            var xVec = new Vector<float>(xData.Slice(i, vectorSize));
            var yVec = new Vector<float>(yData.Slice(i, vectorSize));
            var lnXVec = Vector.Log(xVec);

            sumX += Vector.Sum(xVec);
            sumY += Vector.Sum(yVec);
            sumLnX += Vector.Sum(lnXVec);
            sumLnX2 += Vector.Sum(lnXVec * lnXVec);
            sumYLnX += Vector.Sum(yVec * lnXVec);
        }

        // 处理剩余元素
        for (; i < n; i++)
        {
            if (xData[i] <= 0f)
                throw new ArgumentException("xData must be positive for logarithm transformation.");

            var lnX = MathF.Log(xData[i]);
            sumX += xData[i];
            sumY += yData[i];
            sumLnX += lnX;
            sumLnX2 += lnX * lnX;
            sumYLnX += yData[i] * lnX;
        }

        // 计算系数 a 和 b
        var b = (tn * sumYLnX - sumY * sumLnX) / (tn * sumLnX2 - sumLnX * sumLnX);
        var a = (sumY - b * sumLnX) / tn;

        // 定义预测函数
        Func<float, float> predict = x => a + b * float.Log(x);

        // 计算均方误差
        var mse = 0f;
        i = 0;
        for (; i <= n - vectorSize; i += vectorSize)
        {
            Span<float> predSpan = stackalloc float[vectorSize];
            for (int j = 0; j < vectorSize; j++)
            {
                predSpan[j] = predict(xData[i + j]);
            }
            var predVec = new Vector<float>(predSpan);
            var yVec = new Vector<float>(yData.Slice(i, vectorSize));
            var errorVec = yVec - predVec;
            mse += Vector.Sum(Vector.Multiply(errorVec, errorVec));
        }

        // 处理剩余元素
        for (; i < n; i++)
        {
            var error = yData[i] - predict(xData[i]);
            mse += error * error;
        }
        mse /= tn;

        // 返回拟合结果
        return new FitResultOfSingle(predict, [a, b], mse);
    }

    /// <summary>
    /// 对数回归：拟合 y = a + b * ln(x)，使用 SIMD 优化。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现 IFloatingPointIeee754 接口。</typeparam>
    /// <param name="xData">自变量数据数组。</param>
    /// <param name="yData">因变量数据数组。</param>
    /// <returns>返回拟合结果，包括预测函数、拟合参数和均方误差。</returns>
    /// <exception cref="ArgumentException">当 xData 和 yData 的长度不同时抛出。</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResultOfDouble Fit_SIMD_Double(Span<double> xData, Span<double> yData)
    {
        int n = xData.Length;
        int vectorSize = Vector<double>.Count;
        double tn = n;

        // 计算各种和
        var sumX = 0.0;
        var sumY = 0.0;
        var sumLnX = 0.0;
        var sumLnX2 = 0.0;
        var sumYLnX = 0.0;

        int i = 0;
        for (; i <= n - vectorSize; i += vectorSize)
        {
            var xVec = new Vector<double>(xData.Slice(i, vectorSize));
            var yVec = new Vector<double>(yData.Slice(i, vectorSize));
            var lnXVec = Vector.Log(xVec);

            sumX += Vector.Sum(xVec);
            sumY += Vector.Sum(yVec);
            sumLnX += Vector.Sum(lnXVec);
            sumLnX2 += Vector.Sum(lnXVec * lnXVec);
            sumYLnX += Vector.Sum(yVec * lnXVec);
        }

        // 处理剩余元素
        for (; i < n; i++)
        {
            if (xData[i] <= 0.0)
                throw new ArgumentException("xData must be positive for logarithm transformation.");

            var lnX = double.Log(xData[i]);
            sumX += xData[i];
            sumY += yData[i];
            sumLnX += lnX;
            sumLnX2 += lnX * lnX;
            sumYLnX += yData[i] * lnX;
        }

        // 计算系数 a 和 b
        var b = (tn * sumYLnX - sumY * sumLnX) / (tn * sumLnX2 - sumLnX * sumLnX);
        var a = (sumY - b * sumLnX) / tn;

        // 定义预测函数
        Func<double, double> predict = x => a + b * double.Log(x);

        // 计算均方误差
        var mse = 0.0;
        i = 0;
        for (; i <= n - vectorSize; i += vectorSize)
        {
            Span<double> predSpan = stackalloc double[vectorSize];
            for (int j = 0; j < vectorSize; j++)
            {
                predSpan[j] = predict(xData[i + j]);
            }
            var predVec = new Vector<double>(predSpan);
            var yVec = new Vector<double>(yData.Slice(i, vectorSize));
            var errorVec = yVec - predVec;
            mse += Vector.Sum(Vector.Multiply(errorVec, errorVec));
        }

        // 处理剩余元素
        for (; i < n; i++)
        {
            var error = yData[i] - predict(xData[i]);
            mse += error * error;
        }
        mse /= tn;

        // 返回拟合结果
        return new FitResultOfDouble(predict, [a, b], mse);
    }



    internal static void TEST()
    {
        // 生成 1000 个测试数据点
        int dataSize = 1000;
        var xData = new double[dataSize];
        var yData = new double[dataSize];

        // 填充数据：y = 2 + 3 * ln(x + 1)
        for (int i = 0; i < dataSize; i++)
        {
            xData[i] = (i * 0.1) + 1.0;         // x 从 1.0 到 100.9，步长 0.1
            yData[i] = (2.0 + 3.0 * Math.Log(xData[i])); // y = 2 + 3 * ln(x)
        }

        // 调用 Logarithmic 方法，拟合对数函数
        var result = Fit_Normal<double>(xData, yData);

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

    internal static void CompareFitMethods()
    {
        // 生成 1000 个测试数据点
        int dataSize = 1000;
        var xData = new double[dataSize];
        var yData = new double[dataSize];

        // 填充数据：y = 2 + 3 * ln(x + 1)
        for (int i = 0; i < dataSize; i++)
        {
            xData[i] = (i * 0.1) + 1.0;         // x 从 1.0 到 100.9，步长 0.1
            yData[i] = (2.0 + 3.0 * Math.Log(xData[i])); // y = 2 + 3 * ln(x)
        }

        // 测量 Fit_Normal 的执行时间
        var stopwatch = Stopwatch.StartNew();
        var resultNormal = Fit_Normal<double>(xData, yData);
        stopwatch.Stop();
        var normalTime = stopwatch.ElapsedMilliseconds;

        // 测量 Fit_SIMD 的执行时间
        stopwatch.Restart();
        var resultSIMD = Fit_SIMD<double>(xData, yData);
        stopwatch.Stop();
        var simdTime = stopwatch.ElapsedMilliseconds;

        // 输出结果
        Console.WriteLine("Fit_Normal 拟合参数 (Coefficients):");
        for (int i = 0; i < resultNormal.Parameters.Length; i++)
        {
            Console.WriteLine($"a{i}: {resultNormal.Parameters[i]:F4}");
        }
        Console.WriteLine($"Fit_Normal 均方误差 (MSE): {resultNormal.MeanSquaredError:F4}");
        Console.WriteLine($"Fit_Normal 执行时间: {normalTime} ms");

        Console.WriteLine("\nFit_SIMD 拟合参数 (Coefficients):");
        for (int i = 0; i < resultSIMD.Parameters.Length; i++)
        {
            Console.WriteLine($"a{i}: {resultSIMD.Parameters[i]:F4}");
        }
        Console.WriteLine($"Fit_SIMD 均方误差 (MSE): {resultSIMD.MeanSquaredError:F4}");
        Console.WriteLine($"Fit_SIMD 执行时间: {simdTime} ms");

        // 输出前 5 个和后 5 个实际值和拟合值
        Console.WriteLine("\nFit_Normal 预测值（前 5 个和后 5 个）：");
        for (int i = 0; i < 5; i++)
        {
            var predicted = resultNormal.Predict(xData[i]);
            Console.WriteLine($"x: {xData[i]:F1}, y实际: {yData[i]:F1}, y预测: {predicted:F4}");
        }
        for (int i = dataSize - 5; i < dataSize; i++)
        {
            var predicted = resultNormal.Predict(xData[i]);
            Console.WriteLine($"x: {xData[i]:F1}, y实际: {yData[i]:F1}, y预测: {predicted:F4}");
        }

        Console.WriteLine("\nFit_SIMD 预测值（前 5 个和后 5 个）：");
        for (int i = 0; i < 5; i++)
        {
            var predicted = resultSIMD.Predict(xData[i]);
            Console.WriteLine($"x: {xData[i]:F1}, y实际: {yData[i]:F1}, y预测: {predicted:F4}");
        }
        for (int i = dataSize - 5; i < dataSize; i++)
        {
            var predicted = resultSIMD.Predict(xData[i]);
            Console.WriteLine($"x: {xData[i]:F1}, y实际: {yData[i]:F1}, y预测: {predicted:F4}");
        }
    }

}
