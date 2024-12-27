namespace Vorcyc.Mathematics.MachineLearning;

using System.Numerics;

/// <summary>
/// 一元线性回归工具
/// </summary>
/// <typeparam name="T">浮点数类型，必须实现 <see cref="IFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
/// <example>
/// 以下是如何使用 <see cref="SimpleLinearRegression{T}"/> 类的示例:
/// <code>
/// var data = new Point&lt;double&gt;[]
/// {
///     new Point&lt;double&gt;(1.0, 2.0),
///     new Point&lt;double&gt;(2.0, 3.0),
///     new Point&lt;double&gt;(3.0, 5.0),
///     new Point&lt;double&gt;(4.0, 4.0),
///     new Point&lt;double&gt;(5.0, 6.0)
/// };
///
/// var regression = new SimpleLinearRegression&lt;double&gt;();
/// var (slope, intercept) = regression.Learn(data);
///
/// Console.WriteLine($"Slope: {slope}, Intercept: {intercept}");
///
/// var x = 6.0;
/// var y = regression.GetY(x);
/// Console.WriteLine($"For x = {x}, predicted y = {y}");
/// </code>
/// </example>
public class SimpleLinearRegression<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private T? _slope, _intercept;

    /// <summary>
    /// 计算线性回归模型的<b><i>斜率</i></b>和<b><i>截距</i></b>。
    /// </summary>
    /// <param name="data">包含数据点的数组。</param>
    /// <returns>返回一个元组，包含斜率和截距。</returns>
    public (T slope, T intercept) Learn(Point<T>[] data)
    {
        if (data is null || data.Length < 2)
        {
            throw new ArgumentException("Data cannot be null or the number of data points must be at least 2.");
        }

        T sumX = T.Zero;
        T sumY = T.Zero;
        T sumXY = T.Zero;
        T sumX2 = T.Zero;
        T n = T.CreateChecked(data.Length);

        foreach (var point in data)
        {
            sumX += point.X;
            sumY += point.Y;
            sumXY += point.X * point.Y;
            sumX2 += point.X * point.X;
        }

        T avgX = sumX / n;
        T avgY = sumY / n;

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        var intercept = avgY - slope * avgX;

        _slope = slope;
        _intercept = intercept;

        return (slope, intercept);
    }

    /// <summary>
    /// 计算线性回归模型的<b><i>斜率</i></b>和<b><i>截距</i></b>。
    /// </summary>
    /// <param name="x">自变量的数组。</param>
    /// <param name="y">因变量的数组。</param>
    /// <returns>返回一个元组，包含斜率和截距。</returns>
    public (T slope, T intercept) Learn(Span<T> x, Span<T> y)
    {
        if (x.IsEmpty || y.IsEmpty || x.Length != y.Length || x.Length < 2)
        {
            throw new ArgumentException("The lengths of x and y must be equal and greater than 1.");
        }

        T sumX = x.Sum();
        T sumY = y.Sum();
        T sumXY = T.Zero;
        T sumX2 = T.Zero;
        T n = T.CreateChecked(x.Length);

        for (int i = 0; i < x.Length; i++)
        {
            sumXY += x[i] * y[i];
            sumX2 += x[i] * x[i];
        }

        T avgX = sumX / n;
        T avgY = sumY / n;

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        var intercept = avgY - slope * avgX;

        _slope = slope;
        _intercept = intercept;

        return (slope, intercept);
    }

    /// <summary>
    /// 获取线性回归模型的斜率。
    /// </summary>
    public T? Slope => _slope;

    /// <summary>
    /// 获取线性回归模型的截距。
    /// </summary>
    public T? Intercept => _intercept;

    /// <summary>
    /// 获取机器学习任务类型。
    /// </summary>
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>
    /// 根据给定的 y 值、斜率和截距计算 x 值。
    /// </summary>
    /// <param name="y">因变量的值。</param>
    /// <returns>返回计算得到的 x 值。</returns>
    public T GetX(T y)
    {
        if (_slope is null || _intercept is null)
        {
            throw new InvalidOperationException("The regression model has not been learned yet.");
        }
        return (y - _intercept.Value) / _slope.Value;
    }

    /// <summary>
    /// 根据给定的 x 值、斜率和截距计算 y 值。
    /// </summary>
    /// <param name="x">自变量的值。</param>
    /// <returns>返回计算得到的 y 值。</returns>
    public T GetY(T x)
    {
        if (_slope is null || _intercept is null)
        {
            throw new InvalidOperationException("The regression model has not been learned yet.");
        }

        return _slope.Value * x + _intercept.Value;
    }
}
