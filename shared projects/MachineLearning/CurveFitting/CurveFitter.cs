namespace Vorcyc.Mathematics.MachineLearning.CurveFitting;
using System;
using System.Numerics;
/// <summary>
/// 提供静态方法用于执行不同类型的曲线拟合。
/// </summary>
/// <remarks>
/// <para>LINQ 的精度更高！之所以这样是因为 LINQ 都以 Double来执行，最后再强制转成如 float。</para>
/// <para>所以如果你直接选择泛型，就意味着精度不保；如果想都确保精度，那么需要都统一使用 double 类型，然后再强制转换成目标类型。</para>
/// </remarks>
public static class CurveFitter<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{
    // 检查输入数据的有效性
    private static void ValidateInput(Span<T> xData, Span<T> yData)
    {
        if (xData.IsEmpty || yData.IsEmpty)
            throw new ArgumentException("Input data cannot be empty.");
        if (xData.Length != yData.Length || xData.Length == 0)
            throw new ArgumentException("xData and yData must have the same non-zero length.");
    }
    private static void ValidateInput(CurveFitRow<T>[] xData, Span<T> yData)
    {
        ArgumentNullException.ThrowIfNull(xData);
        if (xData.Length == 0 || yData.IsEmpty)
            throw new ArgumentException("Input data cannot be empty.");
        if (xData.Length != yData.Length || xData.Length == 0)
            throw new ArgumentException("xData and yData must have the same non-zero length.");
    }
    private static void ValidateType()
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Only float and double are supported.");
    }
    /// <summary>
    /// Statistics / VectorSpan 式分发。线性族暂无独立并行内核：Parallel（含达阈值）回退到现有 <c>Fit_SIMD</c>。
    /// </summary>
    private static CurveFitDispatchKind ResolveDispatch(ComputingContext? context, int dataLength)
        => CurveFittingExecution.ResolveDispatch<T>(context, dataLength, workPerItem: 1);

    /// <summary>
    /// 线性回归：拟合直线 y = ax + b。
    /// </summary>
    /// <param name="computingContext">可选计算策略。解析顺序与库内 Statistics / VectorSpan 相同：Normal 标量；Parallel 达阈值时本方法暂回退 SIMD；否则 SIMD（仅 float/double）。</param>
    public static FitResult<T> Linear(Span<T> xData, Span<T> yData, ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        return ResolveDispatch(computingContext, xData.Length) == CurveFitDispatchKind.Normal
            ? LinearRegession.Fit_Normal(xData, yData)
            : LinearRegession.Fit_SIMD(xData, yData);
    }

    /// <summary>
    /// 多项式回归：拟合 y = a0 + a1*x + a2*x^2 + ... + an*x^n。
    /// </summary>
    public static FitResult<T> Polynomial(Span<T> xData, Span<T> yData, int degree, ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        return ResolveDispatch(computingContext, xData.Length) == CurveFitDispatchKind.Normal
            ? PolynomialRegression.Fit_Normal(xData, yData, degree)
            : PolynomialRegression.Fit_SIMD(xData, yData, degree);
    }
    /// <summary>
    /// 指数回归：拟合 y = a * e^(bx)。
    /// </summary>
    public static FitResult<T> Exponential(Span<T> xData, Span<T> yData, ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        return ResolveDispatch(computingContext, xData.Length) == CurveFitDispatchKind.Normal
            ? ExponentialRegression.Fit_Normal(xData, yData)
            : ExponentialRegression.Fit_SIMD(xData, yData);
    }
    /// <summary>
    /// 对数回归：拟合 y = a + b * ln(x)。
    /// </summary>
    public static FitResult<T> Logarithmic(Span<T> xData, Span<T> yData, ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        return ResolveDispatch(computingContext, xData.Length) == CurveFitDispatchKind.Normal
            ? LogarithmicRegression.Fit_Normal(xData, yData)
            : LogarithmicRegression.Fit_SIMD(xData, yData);
    }
    /// <summary>
    ///  幂回归：拟合 y = a * x^b。
    /// </summary>
    public static FitResult<T> Power(Span<T> xData, Span<T> yData, ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        return ResolveDispatch(computingContext, xData.Length) == CurveFitDispatchKind.Normal
            ? PowerRegression.Fit_Normal(xData, yData)
            : PowerRegression.Fit_SIMD(xData, yData);
    }
    /// <summary>
    /// 正弦回归：拟合 y = A * sin(Bx + C) + D。
    /// </summary>
    public static FitResult<T> Sinusoidal(
        Span<T> xData, Span<T> yData, int maxIterations = 100, ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        ValidateType();
        return SinusoidalRegression.Fit(xData, yData, maxIterations, computingContext);
    }
    /// <summary>
    /// 三次样条插值拟合：通过给定的点生成平滑曲线。
    /// </summary>
    /// <param name="xData">X 数据点（必须单调递增）</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="computingContext">可选的计算执行策略。样条系数求解串行；MSE 可按策略并行。</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> CubicSpline(
        Span<T> xData, Span<T> yData, ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        ValidateType();
        return CubicSplineInterpolation<T>.Fit_CubicSpline(xData, yData, computingContext);
    }
    /// <summary>
    /// 局部加权回归 (LOWESS)：局部趋势拟合。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="bandwidth">带宽，控制局部加权的范围，默认值为数据范围的 0.3</param>
    /// <param name="computingContext">可选的计算执行策略。拟合点评估可按策略并行。</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> LocallyWeighted(
        Span<T> xData, Span<T> yData, T? bandwidth = null, ComputingContext? computingContext = null)
    {
        if (xData.Length != yData.Length || xData.Length < 2)
            throw new ArgumentException("数据点数量必须相等且至少有2个点");
        return new LocallyWeightedRegression<T>(xData, yData, bandwidth).Fit(computingContext);
    }

    /// <summary>
    /// 移动平均拟合：平滑时间序列数据。
    /// </summary>
    /// <param name="windowSize">移动窗口大小</param>
    /// <param name="computingContext">可选的计算执行策略。各窗口平滑可按策略并行。</param>
    public static FitResult<T> MovingAverage(
        T[] xData, T[] yData, int windowSize, ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        return MovingAverageFitter.Fit(xData, yData, windowSize, computingContext);
    }
    /// <summary>
    /// 非线性回归：拟合复杂非线性模型。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="model">非线性模型函数，形式为 f(x, parameters)</param>
    /// <param name="initialParams">初始参数猜测</param>
    /// <param name="computingContext">可选的计算执行策略。</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> Fit_Normal(Span<T> xData, Span<T> yData,
        Func<T, T[], T> model, T[] initialParams, ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        return NonlinearRegression.Fit_Normal(
            xData, yData, model, initialParams, computingContext: computingContext);
    }
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
    /// <param name="computingContext">可选的计算执行策略。残差/雅可比装配可按策略并行。</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> Nonlinear(Span<T> xData, Span<T> yData,
        Func<T, T[], T> model, T[] initialParams,
        int maxIterations = 5000,
        T? tolerance = null,
        T? initialLambda = null,
        T? lambdaIncreaseFactor = null,
        T? lambdaDecreaseFactor = null,
        T? stepSize = null,
        T? residualTolerance = null,
        ComputingContext? computingContext = null)
    {
        return NonlinearRegression.Fit_Normal(
            xData, yData, model, initialParams, maxIterations, tolerance, initialLambda,
            lambdaIncreaseFactor, lambdaDecreaseFactor, stepSize, residualTolerance, computingContext);
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
    /// <param name="computingContext">可选的计算执行策略。残差/雅可比装配可按策略并行。</param>
    /// <returns>拟合结果</returns>
    public static MultiColumnFitResult<T> Nonlinear(CurveFitRow<T>[] xData, Span<T> yData,
        Func<CurveFitRow<T>, T[], T> model, T[] initialParams,
        int maxIterations = 5000,
        T? tolerance = null,
        T? initialLambda = null,
        T? lambdaIncreaseFactor = null,
        T? lambdaDecreaseFactor = null,
        T? stepSize = null,
        T? residualTolerance = null,
        ComputingContext? computingContext = null)
    {
        return NonlinearRegression.Fit_MultiColumn_Normal(
            xData, yData, model, initialParams, maxIterations, tolerance, initialLambda,
            lambdaIncreaseFactor, lambdaDecreaseFactor, stepSize, residualTolerance, computingContext);
    }
    /// <summary>
    /// 高斯过程回归 (GPR)：单列输入，平滑预测。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点，单列输入</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="lengthScale">核函数长度尺度，默认1.0</param>
    /// <param name="signalVariance">信号方差，默认1.0</param>
    /// <param name="noiseVariance">噪声方差，默认0.01</param>
    /// <param name="computingContext">可选的计算执行策略。核矩阵装配等可按策略并行。</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> GaussianProcess(Span<T> xData, Span<T> yData,
        T lengthScale = default, T signalVariance = default, T noiseVariance = default,
        ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        return GaussianProcessRegression.Fit(
            xData, yData, lengthScale, signalVariance, noiseVariance, computingContext);
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
    /// <param name="computingContext">可选的计算执行策略。核矩阵装配等可按策略并行。</param>
    /// <returns>拟合结果</returns>
    public static MultiColumnFitResult<T> GaussianProcess(CurveFitRow<T>[] xData, Span<T> yData,
        T lengthScale = default, T signalVariance = default, T noiseVariance = default,
        ComputingContext? computingContext = null)
    {
        ValidateInput(xData, yData);
        return GaussianProcessRegression.Fit(
            xData, yData, lengthScale, signalVariance, noiseVariance, computingContext);
    }
    /// <summary>
    /// 贝叶斯回归：带参数不确定性估计。
    /// </summary>
    /// <param name="computingContext">可选的计算执行策略。</param>
    public static BayesianFitResult<T> BayesianLinear(
        CurveFitRow<T>[] xData, T[] yData, T alpha, T beta, ComputingContext? computingContext = null)
    {
        return BayesianLinearRegression<T>.Fit(xData, yData, alpha, beta, computingContext);
    }
}
