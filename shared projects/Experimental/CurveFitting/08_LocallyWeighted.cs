using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal class LocallyWeightedRegression<T> where T : unmanaged, IFloatingPointIeee754<T>
{
    private readonly T[] _xData;
    private readonly T[] _yData;
    private readonly T _bandwidth;

    public LocallyWeightedRegression(Span<T> xData, Span<T> yData, T? bandwidth = null)
    {
        if (xData.Length != yData.Length || xData.Length < 2)
            throw new ArgumentException("数据点数量必须相等且至少有2个点");

        _xData = xData.ToArray();
        _yData = yData.ToArray();
        T defaultBandwidth = (_xData.AsSpan().Max_SIMD() - _xData.AsSpan().Max_SIMD()) * T.CreateChecked(0.3);
        _bandwidth = bandwidth ?? defaultBandwidth;
        if (_bandwidth <= T.Zero)
            throw new ArgumentException("带宽必须大于0");
    }

    public FitResult<T> Fit()
    {
        int n = _xData.Length;
        T[] fittedValues = new T[n];

        Func<T, T> predict = x =>
        {
            T[] weights = new T[n];
            for (int i = 0; i < n; i++)
            {
                T dx = (x - _xData[i]) / _bandwidth;
                weights[i] = WeightFunction(dx);
            }

            T wxSum = T.Zero, wySum = T.Zero, wxxSum = T.Zero, wxySum = T.Zero, wSum = T.Zero;
            for (int i = 0; i < n; i++)
            {
                T w = weights[i];
                T xi = _xData[i];
                T yi = _yData[i];
                wSum += w;
                wxSum += w * xi;
                wySum += w * yi;
                wxxSum += w * xi * xi;
                wxySum += w * xi * yi;
            }

            T denominator = wSum * wxxSum - wxSum * wxSum;
            if (denominator == T.Zero)
                return wySum / wSum;

            T slope = (wSum * wxySum - wxSum * wySum) / denominator;
            T intercept = (wxxSum * wySum - wxSum * wxySum) / denominator;
            return slope * x + intercept;
        };

        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            fittedValues[i] = predict(_xData[i]);
            T diff = fittedValues[i] - _yData[i];
            mse += diff * diff;
        }
        mse /= T.CreateChecked(n);

        return new FitResult<T>(predict, Array.Empty<T>(), mse);
    }

    // 三次权重函数 w(u) = (1 - |u|^3)^3, |u| < 1
    private static T WeightFunction(T u)
    {
        T absU = T.Abs(u);
        if (absU >= T.One)
            return T.Zero;

        T temp = T.One - absU * absU * absU;
        return temp * temp * temp;
    }


    /// <summary>
    /// 局部加权回归 (LOWESS)：局部趋势拟合。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="bandwidth">带宽，控制局部加权的范围，默认值为数据范围的 0.3</param>
    /// <returns>拟合结果</returns>
    internal static FitResult<T> Fit_Normal(Span<T> xData, Span<T> yData, T? bandwidth = null)
    {
        if (xData.Length != yData.Length || xData.Length < 2)
            throw new ArgumentException("数据点数量必须相等且至少有2个点");

        // 创建 LocallyWeightedRegression 实例并调用 Fit 方法
        var lowess = new LocallyWeightedRegression<T>(xData, yData, bandwidth);
        return lowess.Fit();
    }



    public static void RunTests()
    {
        Console.WriteLine("Running LOWESS Fit Tests...");

        TestLinearData();
        TestNoisySineData();
        TestInvalidInput();

        Console.WriteLine("All tests completed!");
    }

    private static void TestLinearData()
    {
        Console.WriteLine("\nTest 1: Linear Data");

        double[] x = { 0, 1, 2, 3, 4 };
        double[] y = { 0, 2, 4, 6, 8 };

        var result =LocallyWeightedRegression<double>.Fit_Normal(x, y);

        double tolerance = 0.1;
        bool[] pointOk = new bool[x.Length];
        for (int i = 0; i < x.Length; i++)
        {
            double predicted = result.Predict(x[i]);
            pointOk[i] = Math.Abs(predicted - y[i]) < tolerance;
            Console.WriteLine($"Point {x[i]}: Predicted={predicted:F3}, Expected={y[i]}: {(pointOk[i] ? "PASS" : "FAIL")}");
        }

        Console.WriteLine($"MSE = {result.MeanSquaredError:F6} (Expected small): {(result.MeanSquaredError < 0.1 ? "PASS" : "FAIL")}");
    }

    private static void TestNoisySineData()
    {
        Console.WriteLine("\nTest 2: Noisy Sine Data");

        Random rand = new Random(42);
        int n = 20;
        double[] x = new double[n];
        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = i * 0.2;
            double noise = rand.NextDouble() * 0.5 - 0.25;
            y[i] = Math.Sin(x[i]) + noise;
        }

        var result = LocallyWeightedRegression<double>.Fit_Normal(x, y, bandwidth: 0.5);

        double tolerance = 0.5;
        bool[] pointOk = new bool[n];
        for (int i = 0; i < n; i++)
        {
            double predicted = result.Predict(x[i]);
            double expected = Math.Sin(x[i]);
            pointOk[i] = Math.Abs(predicted - expected) < tolerance;
            Console.WriteLine($"Point {x[i]:F1}: Predicted={predicted:F3}, Expected={expected:F3}: {(pointOk[i] ? "PASS" : "FAIL")}");
        }

        Console.WriteLine($"MSE = {result.MeanSquaredError:F6} (Expected <0.1): {(result.MeanSquaredError < 0.1 ? "PASS" : "FAIL")}");
    }

    private static void TestInvalidInput()
    {
        Console.WriteLine("\nTest 3: Invalid Input");

        double[] shortX = { 1 };
        double[] shortY = { 1 };
        bool threwShort = false;
        try
        {
            LocallyWeightedRegression<double>.Fit_Normal(shortX, shortY);
        }
        catch (ArgumentException)
        {
            threwShort = true;
        }

        double[] mismatchX = { 1, 2, 3 };
        double[] mismatchY = { 1, 2 };
        bool threwMismatch = false;
        try
        {
            LocallyWeightedRegression<double>.Fit_Normal(mismatchX, mismatchY);
        }
        catch (ArgumentException)
        {
            threwMismatch = true;
        }

        double[] validX = { 1, 2, 3 };
        double[] validY = { 1, 2, 3 };
        bool threwInvalidBandwidth = false;
        try
        {
            LocallyWeightedRegression<double>.Fit_Normal(validX, validY, bandwidth: -1.0);
        }
        catch (ArgumentException)
        {
            threwInvalidBandwidth = true;
        }

        Console.WriteLine($"Too few points test: {(threwShort ? "PASS" : "FAIL")}");
        Console.WriteLine($"Mismatched lengths test: {(threwMismatch ? "PASS" : "FAIL")}");
        Console.WriteLine($"Invalid bandwidth test: {(threwInvalidBandwidth ? "PASS" : "FAIL")}");
    }
}