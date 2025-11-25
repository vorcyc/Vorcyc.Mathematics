/*
 *  LINQ 的精度更高！之所以这样是因为 LINQ 都以 Double来执行，最后再强制转成如 float。
 *  所以如果你直接选择泛型，就意味着精度不保；如果想都确保精度，那么需要都统一使用 double 类型，然后再强制转换成目标类型。
 */

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class LinearRegession
{

    #region Normal

    /// <summary>
    /// 线性回归：拟合直线 y = ax + b。
    /// </summary>
    /// <typeparam name="T">浮点类型，支持 IEEE 754 运算，例如 double 或 float。</typeparam>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    /// <returns>拟合结果，包括预测函数和参数。</returns>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResult<T> Fit_Normal<T>(Span<T> xData, Span<T> yData)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = xData.Length;

        T xMean = xData.Average_Normal();
        T yMean = yData.Average_Normal();

        // 计算斜率 a 和截距 b
        T numerator = T.Zero, denominator = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T dx = xData[i] - xMean;
            numerator += dx * (yData[i] - yMean);
            denominator += dx * dx;
        }

        T a = numerator / denominator; // 斜率
        T b = yMean - a * xMean;       // 截距

        // 预测函数
        Func<T, T> predict = x => a * x + b;

        // 计算均方误差
        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T error = yData[i] - predict(xData[i]);
            mse += error * error;
        }
        mse /= T.CreateChecked(n);

        return new FitResult<T>(predict, [a, b], mse);
    }

    #endregion


    #region SIMD

    /*
     * 对于这个方法， 针对 float 和 double 的并无特殊 SIMD 优化的空间。
     * 所以直接使用泛型的即可
     * 
     */

    /// <summary>
    /// 线性回归：使用SIMD优化拟合直线 y = ax + b。
    /// </summary>
    /// <typeparam name="T">浮点类型，支持 IEEE 754 运算，例如 double 或 float。</typeparam>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    /// <returns>拟合结果，包括预测函数和参数。</returns>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FitResult<T> Fit_SIMD<T>(Span<T> xData, Span<T> yData)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = xData.Length;
        if (n < 2) throw new ArgumentException("At least two data points are required.");

        // Step 1: 计算均值
        int vectorSize = Vector<T>.Count;


        T nT = T.CreateChecked(n);
        T xMean = xData.Average_SIMD();
        T yMean = yData.Average_SIMD();

        // Step 2: 计算斜率 a 和截距 b
        T numerator = T.Zero, denominator = T.Zero;


        // 创建均值的向量
        var xMeanVec = new Vector<T>(xMean);
        var yMeanVec = new Vector<T>(yMean);

        int i = 0;
        for (; i <= n - vectorSize; i += vectorSize)
        {
            var xVec = new Vector<T>(xData.Slice(i, vectorSize));
            var yVec = new Vector<T>(yData.Slice(i, vectorSize));
            var dxVec = xVec - xMeanVec; // x_i - xMean
            var dyVec = yVec - yMeanVec; // y_i - yMean

            numerator += Vector.Sum(Vector.Multiply(dxVec, dyVec));   // \(\sum (x_i - \bar{x})(y_i - \bar{y})\)
            denominator += Vector.Sum(Vector.Multiply(dxVec, dxVec)); // \(\sum (x_i - \bar{x})^2\)
        }

        // 处理剩余元素
        for (; i < n; i++)
        {
            T dx = xData[i] - xMean;
            T dy = yData[i] - yMean;
            numerator += dx * dy;
            denominator += dx * dx;
        }

        if (denominator == T.Zero)
            throw new InvalidOperationException("Cannot fit a line: all x values are identical.");

        T a = numerator / denominator; // 斜率
        T b = yMean - a * xMean;       // 截距

        // Step 3: 预测函数
        Func<T, T> predict = x => a * x + b;

        // Step 4: 计算均方误差 (MSE)
        T mse = T.Zero;
        i = 0;

        var aVec = new Vector<T>(a);
        var bVec = new Vector<T>(b);

        for (; i <= n - vectorSize; i += vectorSize)
        {
            var xVec = new Vector<T>(xData.Slice(i, vectorSize));
            var yVec = new Vector<T>(yData.Slice(i, vectorSize));
            var predVec = Vector.Multiply(aVec, xVec) + bVec; // ax + b
            var errorVec = yVec - predVec;
            mse += Vector.Sum(Vector.Multiply(errorVec, errorVec));
        }

        for (; i < n; i++)
        {
            T error = yData[i] - predict(xData[i]);
            mse += error * error;
        }
        mse /= nT;

        return new FitResult<T>(predict, new[] { a, b }, mse);
    }





    ///// <summary>
    ///// 线性回归：拟合直线 y = ax + b，使用 SIMD 优化。
    ///// </summary>
    ///// <typeparam name="T">浮点类型，支持 IEEE 754 运算，例如 double 或 float。</typeparam>
    ///// <param name="xData">自变量数据。</param>
    ///// <param name="yData">因变量数据。</param>
    ///// <returns>拟合结果，包括预测函数和参数。</returns>
    //[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    //internal static FitResult<T> Fit_SIMD<T>(Span<T> xData, Span<T> yData)
    //    where T : unmanaged, IFloatingPointIeee754<T>
    //{
    //    if (typeof(T) == typeof(double))
    //    {
    //        return Fit_SIMD_Double(MemoryMarshal.Cast<T, double>(xData), MemoryMarshal.Cast<T, double>(yData)).ToGeneric<T>();
    //    }
    //    else if (typeof(T) == typeof(float))
    //    {
    //        return Fit_SIMD_Float(MemoryMarshal.Cast<T, float>(xData), MemoryMarshal.Cast<T, float>(yData)).ToGeneric<T>();
    //    }
    //    else
    //    {
    //        throw new NotSupportedException("Only double and float types are supported for SIMD operations.");
    //    }
    //}

    //private static FitResultOfDouble Fit_SIMD_Double(Span<double> xData, Span<double> yData)
    //{
    //    int n = xData.Length;
    //    if (n < 2) throw new ArgumentException("At least two data points are required.");

    //    // Step 1: 计算均值
    //    double xSum = 0.0, ySum = 0.0;
    //    int vectorSize = Vector<double>.Count;
    //    int i = 0;

    //    for (; i <= n - vectorSize; i += vectorSize)
    //    {
    //        var xVec = new Vector<double>(xData.Slice(i, vectorSize));
    //        var yVec = new Vector<double>(yData.Slice(i, vectorSize));
    //        xSum += Vector.Sum(xVec);
    //        ySum += Vector.Sum(yVec);
    //    }

    //    // 处理剩余元素
    //    for (; i < n; i++)
    //    {
    //        xSum += xData[i];
    //        ySum += yData[i];
    //    }

    //    double xMean = xSum / n;
    //    double yMean = ySum / n;

    //    // Step 2: 计算斜率 a 和截距 b
    //    double numerator = 0.0, denominator = 0.0;
    //    i = 0;

    //    // 创建均值的向量
    //    var xMeanVec = new Vector<double>(xMean);
    //    var yMeanVec = new Vector<double>(yMean);

    //    for (; i <= n - vectorSize; i += vectorSize)
    //    {
    //        var xVec = new Vector<double>(xData.Slice(i, vectorSize));
    //        var yVec = new Vector<double>(yData.Slice(i, vectorSize));
    //        var dxVec = xVec - xMeanVec; // x_i - xMean
    //        var dyVec = yVec - yMeanVec; // y_i - yMean

    //        numerator += Vector.Sum(Vector.Multiply(dxVec, dyVec));   // \(\sum (x_i - \bar{x})(y_i - \bar{y})\)
    //        denominator += Vector.Sum(Vector.Multiply(dxVec, dxVec)); // \(\sum (x_i - \bar{x})^2\)
    //    }

    //    // 处理剩余元素
    //    for (; i < n; i++)
    //    {
    //        double dx = xData[i] - xMean;
    //        double dy = yData[i] - yMean;
    //        numerator += dx * dy;
    //        denominator += dx * dx;
    //    }

    //    if (denominator == 0.0)
    //        throw new InvalidOperationException("Cannot fit a line: all x values are identical.");

    //    double a = numerator / denominator; // 斜率
    //    double b = yMean - a * xMean;       // 截距

    //    // Step 3: 预测函数
    //    Func<double, double> predict = x => a * x + b;

    //    // Step 4: 计算均方误差 (MSE)
    //    double mse = 0.0;
    //    i = 0;

    //    var aVec = new Vector<double>(a);
    //    var bVec = new Vector<double>(b);

    //    for (; i <= n - vectorSize; i += vectorSize)
    //    {
    //        var xVec = new Vector<double>(xData.Slice(i, vectorSize));
    //        var yVec = new Vector<double>(yData.Slice(i, vectorSize));
    //        var predVec = Vector.Multiply(aVec, xVec) + bVec; // ax + b
    //        var errorVec = yVec - predVec;
    //        mse += Vector.Sum(Vector.Multiply(errorVec, errorVec));
    //    }

    //    for (; i < n; i++)
    //    {
    //        double error = yData[i] - predict(xData[i]);
    //        mse += error * error;
    //    }
    //    mse /= n;

    //    return new FitResultOfDouble(predict, [a, b], mse);
    //}

    //private static FitResultOfSingle Fit_SIMD_Float(Span<float> xData, Span<float> yData)
    //{
    //    int n = xData.Length;
    //    if (n < 2) throw new ArgumentException("At least two data points are required.");

    //    // Step 1: 计算均值
    //    float xSum = 0.0f, ySum = 0.0f;
    //    int vectorSize = Vector<float>.Count;
    //    int i = 0;

    //    for (; i <= n - vectorSize; i += vectorSize)
    //    {
    //        var xVec = new Vector<float>(xData.Slice(i, vectorSize));
    //        var yVec = new Vector<float>(yData.Slice(i, vectorSize));
    //        xSum += Vector.Sum(xVec);
    //        ySum += Vector.Sum(yVec);
    //    }

    //    // 处理剩余元素
    //    for (; i < n; i++)
    //    {
    //        xSum += xData[i];
    //        ySum += yData[i];
    //    }

    //    float xMean = xSum / n;
    //    float yMean = ySum / n;

    //    // Step 2: 计算斜率 a 和截距 b
    //    float numerator = 0.0f, denominator = 0.0f;
    //    i = 0;

    //    // 创建均值的向量
    //    var xMeanVec = new Vector<float>(xMean);
    //    var yMeanVec = new Vector<float>(yMean);

    //    for (; i <= n - vectorSize; i += vectorSize)
    //    {
    //        var xVec = new Vector<float>(xData.Slice(i, vectorSize));
    //        var yVec = new Vector<float>(yData.Slice(i, vectorSize));
    //        var dxVec = xVec - xMeanVec; // x_i - xMean
    //        var dyVec = yVec - yMeanVec; // y_i - yMean

    //        numerator += Vector.Sum(Vector.Multiply(dxVec, dyVec));   // \(\sum (x_i - \bar{x})(y_i - \bar{y})\)
    //        denominator += Vector.Sum(Vector.Multiply(dxVec, dxVec)); // \(\sum (x_i - \bar{x})^2\)
    //    }

    //    // 处理剩余元素
    //    for (; i < n; i++)
    //    {
    //        float dx = xData[i] - xMean;
    //        float dy = yData[i] - yMean;
    //        numerator += dx * dy;
    //        denominator += dx * dx;
    //    }

    //    if (denominator == 0.0f)
    //        throw new InvalidOperationException("Cannot fit a line: all x values are identical.");

    //    float a = numerator / denominator; // 斜率
    //    float b = yMean - a * xMean;       // 截距

    //    // Step 3: 预测函数
    //    Func<float, float> predict = x => a * x + b;

    //    // Step 4: 计算均方误差 (MSE)
    //    float mse = 0.0f;
    //    i = 0;

    //    var aVec = new Vector<float>(a);
    //    var bVec = new Vector<float>(b);

    //    for (; i <= n - vectorSize; i += vectorSize)
    //    {
    //        var xVec = new Vector<float>(xData.Slice(i, vectorSize));
    //        var yVec = new Vector<float>(yData.Slice(i, vectorSize));
    //        var predVec = Vector.Multiply(aVec, xVec) + bVec; // ax + b
    //        var errorVec = yVec - predVec;
    //        mse += Vector.Sum(Vector.Multiply(errorVec, errorVec));
    //    }

    //    for (; i < n; i++)
    //    {
    //        float error = yData[i] - predict(xData[i]);
    //        mse += error * error;
    //    }
    //    mse /= n;

    //    return new FitResultOfSingle(predict, new[] { a, b }, mse);
    //}

    #endregion



    internal static void TEST_Half()
    {
        // 生成 20,000 个测试数据点
        int dataSize = 80;
        var xData = new Half[dataSize];
        var yData = new Half[dataSize];

        // 填充数据：y = 2 + 3x
        for (int i = 0; i < dataSize; i++)
        {
            xData[i] = (Half)(i * 0.1f);         // x 从 0 到 1999.9，步长 0.1
            yData[i] = (Half)2.0f + (Half)3.0f * xData[i]; // y = 2 + 3x
        }

        var result_normal = CurveFitter<Half>.Linear(xData, yData, OptimizationMode.Normal);
        Console.WriteLine($"y = {result_normal.Parameters[0]:F2}x + {result_normal.Parameters[1]:F2}, MSE = {result_normal.MeanSquaredError:F4}");
        Console.WriteLine($"Predict(6) = {result_normal.Predict((Half)6):F2}");

        var result_simd = CurveFitter<Half>.Linear(xData, yData, OptimizationMode.SIMD);
        Console.WriteLine($"y = {result_simd.Parameters[0]:F2}x + {result_simd.Parameters[1]:F2}, MSE = {result_simd.MeanSquaredError:F4}");
        Console.WriteLine($"Predict(6) = {result_simd.Predict((Half)6):F2}");

        //var result_parallel = Fit<float>.Linear(xData, yData, OptimizationMode.Parallel);
        //Console.WriteLine($"y = {result_parallel.Parameters[0]:F2}x + {result_parallel.Parameters[1]:F2}, MSE = {result_parallel.MeanSquaredError:F4}");
        //Console.WriteLine($"Predict(6) = {result_parallel.Predict(6):F2}");
    }


    internal static void TEST_Single()
    {
        // 生成 20,000 个测试数据点
        int dataSize = 2000000;
        var xData = new float[dataSize];
        var yData = new float[dataSize];

        // 填充数据：y = 2 + 3x
        for (int i = 0; i < dataSize; i++)
        {
            xData[i] = (i * 0.1f);         // x 从 0 到 1999.9，步长 0.1
            yData[i] = 2.0f + 3.0f * xData[i]; // y = 2 + 3x
        }

        //"我的标准".PrintLine(ConsoleColor.Red);
        //xData.AsSpan().Sum_Simple().PrintLine(ConsoleColor.Red);


        //"LINQ".PrintLine(ConsoleColor.Yellow);
        //xData.Sum().PrintLine(ConsoleColor.Yellow);

        //"我的泛型+SIMD".PrintLine(ConsoleColor.Cyan);
        //SpanHelper.Sum<float>(xData).PrintLine(ConsoleColor.Cyan);


        //"我的非泛型+SIMD".PrintLine(ConsoleColor.Green);
        //SpanHelper.Sum(xData).PrintLine(ConsoleColor.Green);



        // 使用 Fit_Normal 方法进行拟合
        var result_normal = LinearRegession.Fit_Normal<float>(xData, yData);
        Console.WriteLine("Fit_Normal:");
        Console.WriteLine($"y = {result_normal.Parameters[0]:F2}x + {result_normal.Parameters[1]:F2}, MSE = {result_normal.MeanSquaredError:F4}");
        Console.WriteLine($"Predict(6) = {result_normal.Predict(6):F2}");

        // 使用 Fit_SIMD 方法进行拟合
        var result_simd = LinearRegession.Fit_SIMD<float>(xData, yData);
        Console.WriteLine("Fit_SIMD:");
        Console.WriteLine($"y = {result_simd.Parameters[0]:F2}x + {result_simd.Parameters[1]:F2}, MSE = {result_simd.MeanSquaredError:F4}");
        Console.WriteLine($"Predict(6) = {result_simd.Predict(6):F2}");

        // 使用 Fit_Single 方法进行拟合
        //var result_single = LinearRegession.Fit_Single(xData, yData);
        //Console.WriteLine("Fit_Single:");
        //Console.WriteLine($"y = {result_single.Parameters[0]:F2}x + {result_single.Parameters[1]:F2}, MSE = {result_single.MeanSquaredError:F4}");
        //Console.WriteLine($"Predict(6) = {result_single.Predict(6):F2}");
    }

    /*
     *  LINQ 的精度更高！之所以这样是因为 LINQ 都以 Double来执行，最后再强制转成如 float。
     *  所以如果你直接选择泛型，就意味着精度不保；如果想都确保精度，那么需要都统一使用 double 类型，然后再强制转换成目标类型。
     */

    internal static void TEST_Double()
    {
        // 生成 20,000 个测试数据点
        int dataSize = 2000000;
        var xData = new double[dataSize];
        var yData = new double[dataSize];

        // 填充数据：y = 2 + 3x
        for (int i = 0; i < dataSize; i++)
        {
            xData[i] = (i * 0.1f);         // x 从 0 到 1999.9，步长 0.1
            yData[i] = 2.0f + 3.0f * xData[i]; // y = 2 + 3x
        }

        //"我的标准".PrintLine(ConsoleColor.Red);
        //xData.AsSpan().Sum_Simple().PrintLine(ConsoleColor.Red);


        //"LINQ".PrintLine(ConsoleColor.Yellow);
        //xData.Sum().PrintLine(ConsoleColor.Yellow);

        //"我的泛型+SIMD".PrintLine(ConsoleColor.Cyan);
        //SpanHelper.Sum<double>(xData).PrintLine(ConsoleColor.Cyan);


        //"我的非泛型+SIMD".PrintLine(ConsoleColor.Green);
        //SpanHelper.Sum(xData).PrintLine(ConsoleColor.Green);


        // 使用 Fit_Normal 方法进行拟合
        var result_normal = LinearRegession.Fit_Normal<double>(xData, yData);
        Console.WriteLine("Fit_Normal:");
        Console.WriteLine($"y = {result_normal.Parameters[0]:F2}x + {result_normal.Parameters[1]:F2}, MSE = {result_normal.MeanSquaredError:F4}");
        Console.WriteLine($"Predict(6) = {result_normal.Predict(6):F2}");

        // 使用 Fit_SIMD 方法进行拟合
        var result_simd = LinearRegession.Fit_SIMD<double>(xData, yData);
        Console.WriteLine("Fit_SIMD:");
        Console.WriteLine($"y = {result_simd.Parameters[0]:F2}x + {result_simd.Parameters[1]:F2}, MSE = {result_simd.MeanSquaredError:F4}");
        Console.WriteLine($"Predict(6) = {result_simd.Predict(6):F2}");

        //// 使用 Fit_Single 方法进行拟合
        //var result_single = LinearRegession.Fit_Single(xData, yData);
        //Console.WriteLine("Fit_Single:");
        //Console.WriteLine($"y = {result_single.Parameters[0]:F2}x + {result_single.Parameters[1]:F2}, MSE = {result_single.MeanSquaredError:F4}");
        //Console.WriteLine($"Predict(6) = {result_single.Predict(6):F2}");
    }
}

