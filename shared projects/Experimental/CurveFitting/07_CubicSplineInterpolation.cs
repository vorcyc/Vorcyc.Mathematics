using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

/// <summary>
/// 三次样条插值：平滑插值穿过所有数据点。
/// </summary>
/// <typeparam name="T"></typeparam>
internal class CubicSplineInterpolation<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{

    private readonly T[] _xData;
    private readonly T[] _a, _b, _c, _d;
    private readonly int _n;

    public CubicSplineInterpolation(Span<T> xData, Span<T> yData)
    {
        if (xData.Length != yData.Length || xData.Length < 2)
            throw new ArgumentException("Data points must be equal in number and at least 2.");
        if (!IsMonotonicIncreasing(xData))
            throw new ArgumentException("X data points must be monotonically increasing.");

        _xData = xData.ToArray();
        _n = xData.Length - 1;
        var coefficients = ComputeCubicSplineCoefficients(xData, yData);
        _a = coefficients.a;
        _b = coefficients.b;
        _c = coefficients.c;
        _d = coefficients.d;
    }

    public T Predict(T x)
    {
        if (x < _xData[0] || x > _xData[_n])
            throw new ArgumentOutOfRangeException(nameof(x), "Prediction point out of data range.");

        int i = 0;
        while (i < _n && x > _xData[i + 1]) i++;
        if (i == _n) i--;

        T dx = x - _xData[i];
        return _a[i] + _b[i] * dx + _c[i] * dx * dx + _d[i] * dx * dx * dx;
    }

    // 计算三次样条的系数
    private static (T[] a, T[] b, T[] c, T[] d) ComputeCubicSplineCoefficients(Span<T> xData, Span<T> yData)
    {
        int n = xData.Length - 1;
        T[] h = new T[n];     // 区间长度
        T[] alpha = new T[n]; // 中间变量
        T[] l = new T[n + 1]; // 三对角矩阵的对角线
        T[] mu = new T[n];    // 三对角矩阵的上对角线
        T[] z = new T[n + 1]; // 中间变量
        T[] a = new T[n];     // 系数a
        T[] b = new T[n];     // 系数b
        T[] c = new T[n + 1]; // 系数c (二阶导数)
        T[] d = new T[n];     // 系数d

        // 计算区间长度
        for (int i = 0; i < n; i++)
            h[i] = xData[i + 1] - xData[i];

        // 计算alpha
        for (int i = 1; i < n; i++)
            alpha[i] = T.CreateChecked(3) * (yData[i + 1] - yData[i]) / h[i] -
                       T.CreateChecked(3) * (yData[i] - yData[i - 1]) / h[i - 1];

        // 追赶法求解三对角线性方程组（自然边界条件：c[0] = c[n] = 0）
        l[0] = T.One;
        mu[0] = T.Zero;
        z[0] = T.Zero;

        for (int i = 1; i < n; i++)
        {
            l[i] = T.CreateChecked(2) * (xData[i + 1] - xData[i - 1]) - h[i - 1] * mu[i - 1];
            mu[i] = h[i] / l[i];
            z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
        }

        l[n] = T.One;
        z[n] = T.Zero;
        c[n] = T.Zero;

        // 回代求解c
        for (int j = n - 1; j >= 0; j--)
        {
            c[j] = z[j] - mu[j] * c[j + 1];
            b[j] = (yData[j + 1] - yData[j]) / h[j] - h[j] * (c[j + 1] + T.CreateChecked(2) * c[j]) / T.CreateChecked(3);
            d[j] = (c[j + 1] - c[j]) / (T.CreateChecked(3) * h[j]);
            a[j] = yData[j];
        }

        return (a, b, c, d);
    }

    // 检查序列是否单调递增
    private static bool IsMonotonicIncreasing(Span<T> data)
    {
        for (int i = 1; i < data.Length; i++)
            if (data[i].CompareTo(data[i - 1]) <= 0)
                return false;
        return true;
    }


    // 添加方法以获取系数，用于 Fit 方法
    public T[] GetCoefficients()
    {
        T[] parameters = new T[4 * _n];
        for (int i = 0; i < _n; i++)
        {
            parameters[4 * i] = _a[i];
            parameters[4 * i + 1] = _b[i];
            parameters[4 * i + 2] = _c[i];
            parameters[4 * i + 3] = _d[i];
        }
        return parameters;
    }


    /// <summary>
    /// 三次样条插值拟合：通过给定的点生成平滑曲线。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点（必须单调递增）</param>
    /// <param name="yData">Y 数据点</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> Fit_CubicSpline(Span<T> xData, Span<T> yData)
    {
        // 创建样条插值实例
        var spline = new CubicSplineInterpolation<T>(xData, yData);

        // 获取预测函数
        Func<T, T> predict = spline.Predict;

        // 获取参数
        T[] parameters = spline.GetCoefficients();

        // 计算MSE
        T mse = T.Zero;
        for (int i = 0; i < xData.Length; i++)
        {
            T predicted = predict(xData[i]);
            T diff = predicted - yData[i];
            mse += diff * diff;
        }
        mse /= T.CreateChecked(xData.Length);

        return new FitResult<T>(predict, parameters, mse);
    }


    public static void RunTests()
    {
        Console.WriteLine("Running Spline Fit Tests...");

        TestLinearData();
        TestQuadraticData();

        Console.WriteLine("All tests completed!");
    }

    private static void TestLinearData()
    {
        Console.WriteLine("\nTest 1: Linear Data");

        double[] x = { 0, 1, 2, 3, 4 };
        double[] y = { 0, 2, 4, 6, 8 };

        var result = CubicSplineInterpolation<double>.Fit_CubicSpline(x, y);

        double tolerance = 1e-10;
        bool[] pointOk = new bool[x.Length];
        for (int i = 0; i < x.Length; i++)
        {
            double predicted = result.Predict(x[i]);
            pointOk[i] = Math.Abs(predicted - y[i]) < tolerance;
            Console.WriteLine($"Point {x[i]}: Predicted={predicted:F10}, Expected={y[i]}: {(pointOk[i] ? "PASS" : "FAIL")}");
        }

        double midX = 2.5;
        double expected = 5.0;
        double midPredicted = result.Predict(midX);
        bool midOk = Math.Abs(midPredicted - expected) < 0.1;
        Console.WriteLine($"Midpoint {midX}: Predicted={midPredicted:F10}, Expected={expected}: {(midOk ? "PASS" : "FAIL")}");

        Console.WriteLine($"MSE = {result.MeanSquaredError:F10} (Expected ~0): {(result.MeanSquaredError < tolerance ? "PASS" : "FAIL")}");
    }

    private static void TestQuadraticData()
    {
        Console.WriteLine("\nTest 2: Quadratic Data");

        double[] x = { 0, 1, 2, 3 };
        double[] y = { 0, 1, 4, 9 };

        var result = CubicSplineInterpolation<double>.Fit_CubicSpline(x, y);

        double tolerance = 1e-10;
        bool[] pointOk = new bool[x.Length];
        for (int i = 0; i < x.Length; i++)
        {
            double predicted = result.Predict(x[i]);
            pointOk[i] = Math.Abs(predicted - y[i]) < tolerance;
            Console.WriteLine($"Point {x[i]}: Predicted={predicted:F10}, Expected={y[i]}: {(pointOk[i] ? "PASS" : "FAIL")}");
        }

        double midX = 1.5;
        double expected = 2.25;
        double midPredicted = result.Predict(midX);
        bool midOk = Math.Abs(midPredicted - expected) < 0.5;
        Console.WriteLine($"Midpoint {midX}: Predicted={midPredicted:F10}, Expected={expected}: {(midOk ? "PASS" : "FAIL")}");

        Console.WriteLine($"MSE = {result.MeanSquaredError:F10} (Expected ~0): {(result.MeanSquaredError < tolerance ? "PASS" : "FAIL")}");
    }
}
