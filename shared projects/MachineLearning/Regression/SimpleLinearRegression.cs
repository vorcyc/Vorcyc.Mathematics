using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// 一元线性回归工具。
/// </summary>
/// <typeparam name="T">浮点数类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
/// <remarks>
/// 该类实现了一元线性回归，通过最小二乘法拟合直线模型 y = slope * x + intercept。
/// 提供了从数据点或数组拟合模型、预测 y 值或 x 值，以及访问斜率、截距和 R² 的功能。
/// 
/// 优化版本包括：
/// - 使用 SIMD 优化的 <see cref="Span{T}.Sum{T}"/> 方法提升性能。
/// - 数值稳定性检查，防止除零。
/// - 添加 R² 计算，用于模型评估。
/// </remarks>
/// <example>
/// 以下是如何使用 <see cref="SimpleLinearRegression{T}"/> 类的示例：
/// <code>
/// var data = new Point<double>[]
/// {
///     new Point<double>(1.0, 2.0),
///     new Point<double>(2.0, 3.0),
///     new Point<double>(3.0, 5.0),
///     new Point<double>(4.0, 4.0),
///     new Point<double>(5.0, 6.0)
/// };
/// var regression = new SimpleLinearRegression<double>();
/// var (slope, intercept) = regression.Fit(data);
/// Console.WriteLine($"Slope: {slope}, Intercept: {intercept}");
/// var x = 6.0;
/// var y = regression.GetY(x);
/// Console.WriteLine($"For x = {x}, predicted y = {y}");
/// Console.WriteLine($"R²: {regression.RSquared}");
/// </code>
/// </example>
public class SimpleLinearRegression<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private T _slope;         // 斜率
    private T _intercept;     // 截距
    private bool _isFitted;   // 是否已拟合
    private T _rSquared;      // 决定系数 R²

    /// <summary>
    /// 初始化 <see cref="SimpleLinearRegression{T}"/> 类的新实例。
    /// </summary>
    public SimpleLinearRegression()
    {
        _isFitted = false;
    }

    /// <summary>
    /// 获取线性回归模型的斜率。
    /// </summary>
    public T Slope => _isFitted ? _slope : throw new InvalidOperationException("模型尚未拟合。");

    /// <summary>
    /// 获取线性回归模型的截距。
    /// </summary>
    public T Intercept => _isFitted ? _intercept : throw new InvalidOperationException("模型尚未拟合。");

    /// <summary>
    /// 获取模型的决定系数 R²，表示模型对数据的解释能力。
    /// </summary>
    public T RSquared => _isFitted ? _rSquared : throw new InvalidOperationException("模型尚未拟合。");

    /// <summary>
    /// 获取机器学习任务类型。
    /// </summary>
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>
    /// 从数据点数组拟合线性回归模型，返回斜率和截距。
    /// </summary>
    /// <param name="data">包含数据点的数组。</param>
    /// <returns>返回一个元组，包含斜率和截距。</returns>
    /// <exception cref="ArgumentException">当 <paramref name="data"/> 为 null 或数据点少于 2 时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计算中出现除零时抛出。</exception>
    public (T slope, T intercept) Fit(Point<T>[] data)
    {
        if (data == null || data.Length < 2)
            throw new ArgumentException("数据点数组不能为 null，且数量必须至少为 2。", nameof(data));

        T sumX = T.Zero, sumY = T.Zero, sumXY = T.Zero, sumX2 = T.Zero;
        T n = T.CreateChecked(data.Length);

        foreach (var point in data)
        {
            sumX += point.X;
            sumY += point.Y;
            sumXY += point.X * point.Y;
            sumX2 += point.X * point.X;
        }

        var result = ComputeCoefficients(sumX, sumY, sumXY, sumX2, n);
        _rSquared = ComputeRSquared(data.AsSpan(), sumY / n); // 在拟合完成后计算 R²
        return result;
    }

    /// <summary>
    /// 从自变量和因变量数组拟合线性回归模型，返回斜率和截距。
    /// </summary>
    /// <param name="x">自变量的数组。</param>
    /// <param name="y">因变量的数组。</param>
    /// <returns>返回一个元组，包含斜率和截距。</returns>
    /// <exception cref="ArgumentException">当 <paramref name="x"/> 或 <paramref name="y"/> 为空、长度不匹配或少于 2 时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计算中出现除零时抛出。</exception>
    public (T slope, T intercept) Fit(Span<T> x, Span<T> y)
    {
        if (x.IsEmpty || y.IsEmpty || x.Length != y.Length || x.Length < 2)
            throw new ArgumentException("自变量和因变量数组不能为空，长度必须相等且至少为 2。", nameof(x));

        T sumX = x.Sum();
        T sumY = y.Sum();
        T sumXY = T.Zero, sumX2 = T.Zero;
        T n = T.CreateChecked(x.Length);

        for (int i = 0; i < x.Length; i++)
        {
            sumXY += x[i] * y[i];
            sumX2 += x[i] * x[i];
        }

        var result = ComputeCoefficients(sumX, sumY, sumXY, sumX2, n);
        _rSquared = ComputeRSquared(x, y, sumY / n); // 在拟合完成后计算 R²
        return result;
    }

    /// <summary>
    /// 根据给定的 y 值计算 x 值。
    /// </summary>
    /// <param name="y">因变量的值。</param>
    /// <returns>预测的 x 值。</returns>
    /// <exception cref="InvalidOperationException">当模型尚未拟合或斜率为零时抛出。</exception>
    public T GetX(T y)
    {
        if (!_isFitted)
            throw new InvalidOperationException("模型尚未拟合。");
        if (_slope == T.Zero)
            throw new InvalidOperationException("斜率为零，无法计算 x 值。");

        return (y - _intercept) / _slope;
    }

    /// <summary>
    /// 根据给定的 x 值计算 y 值。
    /// </summary>
    /// <param name="x">自变量的值。</param>
    /// <returns>预测的 y 值。</returns>
    /// <exception cref="InvalidOperationException">当模型尚未拟合时抛出。</exception>
    public T GetY(T x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("模型尚未拟合。");

        return _slope * x + _intercept;
    }

    /// <summary>
    /// 计算回归系数并更新模型状态。
    /// </summary>
    /// <param name="sumX">x 的总和。</param>
    /// <param name="sumY">y 的总和。</param>
    /// <param name="sumXY">x*y 的总和。</param>
    /// <param name="sumX2">x² 的总和。</param>
    /// <param name="n">数据点数量。</param>
    /// <returns>斜率和截距的元组。</returns>
    private (T slope, T intercept) ComputeCoefficients(T sumX, T sumY, T sumXY, T sumX2, T n)
    {
        T denominator = n * sumX2 - sumX * sumX;
        if (T.Abs(denominator) < T.CreateChecked(1e-10))
            throw new InvalidOperationException("数据点过于共线或方差过小，无法计算回归系数。");

        T avgX = sumX / n;
        T avgY = sumY / n;

        _slope = (n * sumXY - sumX * sumY) / denominator;
        _intercept = avgY - _slope * avgX;
        _isFitted = true; // 设置为 true，确保后续调用有效

        return (_slope, _intercept);
    }

    /// <summary>
    /// 计算 R² 值（基于 <see cref="Point{T}"/> 数据）。
    /// </summary>
    private T ComputeRSquared(ReadOnlySpan<Point<T>> data, T avgY)
    {
        T ssTot = T.Zero, ssRes = T.Zero;
        for (int i = 0; i < data.Length; i++)
        {
            T yPred = _slope * data[i].X + _intercept; // 直接使用系数计算
            T yActual = data[i].Y;
            ssTot += (yActual - avgY) * (yActual - avgY);
            ssRes += (yActual - yPred) * (yActual - yPred);
        }
        return ssTot != T.Zero ? T.One - (ssRes / ssTot) : T.Zero;
    }

    /// <summary>
    /// 计算 R² 值（基于 x 和 y 数组）。
    /// </summary>
    private T ComputeRSquared(Span<T> x, Span<T> y, T avgY)
    {
        T ssTot = T.Zero, ssRes = T.Zero;
        for (int i = 0; i < x.Length; i++)
        {
            T yPred = _slope * x[i] + _intercept; // 直接使用系数计算
            T yActual = y[i];
            ssTot += (yActual - avgY) * (yActual - avgY);
            ssRes += (yActual - yPred) * (yActual - yPred);
        }
        return ssTot != T.Zero ? T.One - (ssRes / ssTot) : T.Zero;
    }
}