namespace Vorcyc.Mathematics.Experimental.CurveFitting;

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
            throw new ArgumentNullException("Input data cannot be null.");
        if (xData.Length != yData.Length || xData.Length == 0)
            throw new ArgumentException("xData and yData must have the same non-zero length.");
    }

    private static void ValidateInput(DataRow<T>[] xData, Span<T> yData)
    {
        if (xData.Length == 0 || yData.IsEmpty)
            throw new ArgumentNullException("Input data cannot be null.");
        if (xData.Length != yData.Length || xData.Length == 0)
            throw new ArgumentException("xData and yData must have the same non-zero length.");
    }


    private static void ValidateType()
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Only float and double are supported.");
    }


    /// <summary>
    /// 线性回归：拟合直线 y = ax + b，使用 SIMD 优化。
    /// </summary>
    /// <typeparam name="T">浮点类型，支持 IEEE 754 运算，例如 double 或 float。</typeparam>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    /// <param name="optimizationMode">代码的优化模式。默认为 <see cref="OptimizationMode.SIMD"/> 且仅支持 <see cref="float"/> 及 <see cref="double"/> 作为 <typeparamref name="T"/></param>
    /// <returns>拟合结果，包括预测函数和参数。</returns>
    public static FitResult<T> Linear(Span<T> xData, Span<T> yData, OptimizationMode optimizationMode = OptimizationMode.SIMD)
    {
        ValidateInput(xData, yData);
        if (optimizationMode == OptimizationMode.Normal)
            return LinearRegession.Fit_Normal(xData, yData);
        else
        {
            ValidateType();
            return LinearRegession.Fit_SIMD(xData, yData);
        }
    }



    /// <summary>
    /// 多项式回归：拟合 y = a0 + a1*x + a2*x^2 + ... + an*x^n。
    /// </summary>
    /// <typeparam name="T">浮点类型，支持 IEEE 754 运算，例如 double 或 float。</typeparam>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    /// <param name="degree">多项式次数</param>
    /// <param name="optimizationMode">代码的优化模式。默认为 <see cref="OptimizationMode.SIMD"/> 且仅支持 <see cref="float"/> 及 <see cref="double"/> 作为 <typeparamref name="T"/></param>
    /// <returns>拟合结果，包括预测函数和参数。</returns>
    public static FitResult<T> Polynomial(Span<T> xData, Span<T> yData, int degree, OptimizationMode optimizationMode = OptimizationMode.SIMD)
    {
        ValidateInput(xData, yData);
        if (optimizationMode == OptimizationMode.Normal)
            return PolynomialRegression.Fit_Normal(xData, yData, degree);
        else
        {
            ValidateType();
            return PolynomialRegression.Fit_SIMD(xData, yData, degree);
        }
    }


    /// <summary>
    /// 指数回归：拟合 y = a * e^(bx)。
    /// </summary>
    /// <typeparam name="T">浮点类型，支持 IEEE 754 运算，例如 double 或 float。</typeparam>
    /// <param name="xData">自变量数据。</param>
    /// <param name="yData">因变量数据。</param>
    /// <param name="degree">多项式次数</param>
    /// <param name="optimizationMode">代码的优化模式。默认为 <see cref="OptimizationMode.SIMD"/> 且仅支持 <see cref="float"/> 及 <see cref="double"/> 作为 <typeparamref name="T"/></param>
    /// <returns>拟合结果，包括预测函数和参数。</returns>
    public static FitResult<T> Exponential(Span<T> xData, Span<T> yData, OptimizationMode optimizationMode = OptimizationMode.SIMD)
    {
        ValidateInput(xData, yData);
        if (optimizationMode == OptimizationMode.Normal)
            return ExponentialRegression.Fit_Normal(xData, yData);
        else
        {
            ValidateType();
            return ExponentialRegression.Fit_SIMD(xData, yData);
        }
    }

    /// <summary>
    /// 对数回归：拟合 y = a + b * ln(x)。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现 IFloatingPointIeee754 接口。</typeparam>
    /// <param name="xData">自变量数据数组。</param>
    /// <param name="yData">因变量数据数组。</param>
    /// <param name="optimizationMode">代码的优化模式。默认为 <see cref="OptimizationMode.SIMD"/> 且仅支持 <see cref="float"/> 及 <see cref="double"/> 作为 <typeparamref name="T"/></param>
    /// <returns>返回拟合结果，包括预测函数、拟合参数和均方误差。</returns>
    /// <exception cref="ArgumentException">当 xData 和 yData 的长度不同时抛出。</exception>
    public static FitResult<T> Logarithmic(Span<T> xData, Span<T> yData, OptimizationMode optimizationMode = OptimizationMode.SIMD)
    {
        ValidateInput(xData, yData);
        if (optimizationMode == OptimizationMode.Normal)
            return LogarithmicRegression.Fit_Normal(xData, yData);
        else
        {
            ValidateType();
            return LogarithmicRegression.Fit_SIMD(xData, yData);
        }
    }

    /// <summary>
    ///  幂回归：拟合 y = a * x^b。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现 IFloatingPointIeee754 接口。</typeparam>
    /// <param name="xData">自变量数据数组。</param>
    /// <param name="yData">因变量数据数组。</param>
    /// <param name="optimizationMode">代码的优化模式。默认为 <see cref="OptimizationMode.SIMD"/> 且仅支持 <see cref="float"/> 及 <see cref="double"/> 作为 <typeparamref name="T"/></param>
    /// <returns>返回拟合结果，包括预测函数、拟合参数和均方误差。</returns>
    /// <exception cref="ArgumentException">当 xData 和 yData 的长度不同时抛出。</exception>
    public static FitResult<T> Power(Span<T> xData, Span<T> yData, OptimizationMode optimizationMode = OptimizationMode.SIMD)
    {
        ValidateInput(xData, yData);
        if (optimizationMode == OptimizationMode.Normal)
            return PowerRegression.Fit_Normal(xData, yData);
        else
        {
            ValidateType();
            return PowerRegression.Fit_SIMD(xData, yData);
        }
    }

    /// <summary>
    /// 正弦回归：拟合 y = A * sin(Bx + C) + D。
    /// </summary>
    /// <param name="xData">自变量数据数组。</param>
    /// <param name="yData">因变量数据数组。</param>
    /// <param name="maxIterations">最大迭代次数</param>
    /// <returns></returns>
    public static FitResult<T> Sinusoidal(Span<T> xData, Span<T> yData, int maxIterations = 100)
    {
        ValidateInput(xData, yData);
        ValidateType();
        return SinusoidalRegression.Fit_Normal(xData, yData, maxIterations);
    }


    /// <summary>
    /// 三次样条插值拟合：通过给定的点生成平滑曲线。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点（必须单调递增）</param>
    /// <param name="yData">Y 数据点</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> CubicSpline(Span<T> xData, Span<T> yData)
    {
        ValidateInput(xData, yData);
        ValidateType();
        return CubicSplineInterpolation<T>.Fit_CubicSpline(xData, yData);
    }

    /// <summary>
    /// 局部加权回归 (LOWESS)：局部趋势拟合。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="bandwidth">带宽，控制局部加权的范围，默认值为数据范围的 0.3</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> LocallyWeighted(Span<T> xData, Span<T> yData, T? bandwidth = null)
    {
        if (xData.Length != yData.Length || xData.Length < 2)
            throw new ArgumentException("数据点数量必须相等且至少有2个点");

        // 创建 LocallyWeightedRegression 实例并调用 Fit 方法
        var lowess = new LocallyWeightedRegression<T>(xData, yData, bandwidth);
        return lowess.Fit();
    }



    /// <summary>
    /// 移动平均拟合：平滑时间序列数据。
    /// </summary>
    /// <param name="windowSize">移动窗口大小</param>
    public static FitResult<T> MovingAverage(T[] xData, T[] yData, int windowSize)
    {
        ValidateInput(xData, yData);
        return MovingAverageFitter.Fit_Normal(xData, yData, windowSize);
    }


    /// <summary>
    /// 非线性回归：拟合复杂非线性模型。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点</param>
    /// <param name="yData">Y 数据点</param>
    /// <param name="model">非线性模型函数，形式为 f(x, parameters)</param>
    /// <param name="initialParams">初始参数猜测</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> Fit_Normal(Span<T> xData, Span<T> yData,
        Func<T, T[], T> model, T[] initialParams)
    {
        ValidateInput(xData, yData);
        return NonlinearRegression.Fit_Normal(xData, yData, model, initialParams);

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
    /// <returns>拟合结果</returns>
    public static FitResult<T> Nonlinear(Span<T> xData, Span<T> yData,
        Func<T, T[], T> model, T[] initialParams,
        int maxIterations = 5000,
        T? tolerance = null,
        T? initialLambda = null,
        T? lambdaIncreaseFactor = null,
        T? lambdaDecreaseFactor = null,
        T? stepSize = null,
        T? residualTolerance = null)
    {
        return NonlinearRegression.Fit_Normal(xData, yData, model, initialParams, maxIterations, tolerance, initialLambda, lambdaIncreaseFactor, lambdaDecreaseFactor, stepSize, residualTolerance);
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
    public static MultiColumnFitResult<T> Nonlinear(DataRow<T>[] xData, Span<T> yData,
        Func<DataRow<T>, T[], T> model, T[] initialParams,
        int maxIterations = 5000,
        T? tolerance = null,
        T? initialLambda = null,
        T? lambdaIncreaseFactor = null,
        T? lambdaDecreaseFactor = null,
        T? stepSize = null,
        T? residualTolerance = null)
    {
        return NonlinearRegression.Fit_MultiColumn_Normal(xData, yData, model, initialParams, maxIterations, tolerance, initialLambda, lambdaIncreaseFactor, lambdaDecreaseFactor, stepSize, residualTolerance);
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
    /// <returns>拟合结果</returns>
    public static FitResult<T> GaussianProcess(Span<T> xData, Span<T> yData,
        T lengthScale = default, T signalVariance = default, T noiseVariance = default)
    {
        ValidateInput(xData, yData);
        return GaussianProcessRegression.Fit(xData, yData, lengthScale, signalVariance, noiseVariance);
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
    /// <returns>拟合结果</returns>
    public static MultiColumnFitResult<T> GaussianProcess(DataRow<T>[] xData, Span<T> yData,
        T lengthScale = default, T signalVariance = default, T noiseVariance = default)
    {
        ValidateInput(xData, yData);
        return GaussianProcessRegression.Fit(xData, yData, lengthScale, signalVariance, noiseVariance);
    }


    /// <summary>
    /// 使用神经网络进行回归拟合，适用于单列输入数据，旨在捕捉复杂非线性关系。
    /// 该方法通过多层感知器（MLP）训练一个神经网络，返回拟合结果。
    /// </summary>
    /// <typeparam name="T">数据类型，必须是支持 IEEE 754 浮点运算的非托管类型，例如 <see cref="float"/> 或 <see cref="double"/>。</typeparam>
    /// <param name="xData">
    /// 输入数据，表示自变量，形状为 [样本数]。
    /// 每个元素是一个标量值，通常需要归一化到 [0, 1] 以适配神经网络的激活函数（如 Sigmoid）。
    /// </param>
    /// <param name="yData">
    /// 目标数据，表示因变量，形状为 [样本数]。
    /// 每个元素是对应的输出值，与 <paramref name="xData"/> 一一对应，通常也需要归一化到 [0, 1]。
    /// </param>
    /// <param name="epochs">
    /// 训练轮数，控制神经网络的迭代次数，默认为 5000。
    /// 增加此值可以提高拟合精度，但可能增加计算时间，建议根据数据复杂度和收敛情况调整。
    /// </param>
    /// <param name="hiddenNodes">
    /// 隐藏层节点数，控制神经网络的容量，默认为 10。
    /// 更多的节点可以捕捉更复杂的非线性关系，但可能导致过拟合，建议根据任务复杂度选择。
    /// </param>
    /// <param name="learningRate">
    /// 学习率，控制权重更新的步长，可为空，默认为 0.1。
    /// 较小的值（如 0.01）可能使训练更稳定但收敛较慢，较大的值（如 0.5）可能加速收敛但不稳定。
    /// </param>
    /// <param name="trainingProgressCallback">
    /// 训练进度回调函数，可为空，用于监控训练过程。
    /// 接受三个参数：当前轮次 (epoch)、总轮次 (totalEpochs) 和平均误差 (averageError)，可用于日志记录或早停。
    /// 示例用法：(epoch, total, error) => Console.WriteLine($"Epoch {epoch}/{total}, Error: {error}");
    /// </param>
    /// <returns>
    /// 返回 <see cref="FitResult{T}"/> 对象，包含：
    /// - <see cref="FitResult{T}.Predict"/>：单输入预测函数。
    /// - <see cref="FitResult{T}.Parameters"/>：网络参数（权重和偏置）。
    /// - <see cref="FitResult{T}.MeanSquaredError"/>：训练后的均方误差。
    /// </returns>
    /// <exception cref="ArgumentException">
    /// 当 <paramref name="xData"/> 和 <paramref name="yData"/> 长度不匹配，或数据为空时抛出。
    /// </exception>
    /// <remarks>
    /// 建议使用 Accord.NET 或 ML.NET 以获得更高效的实现和更多功能。
    /// 输入和输出数据应归一化到 [0, 1] 以获得最佳拟合效果。
    /// </remarks>
    public static FitResult<T> NeuralNetwork(Span<T> xData, Span<T> yData,
        int epochs = 5000, int hiddenNodes = 10, T? learningRate = null,
        TrainingProgressHandler<T>? trainingProgressCallback = null)
    {
        ValidateInput(xData, yData);
        // 建议使用 Accord.NET 或 ML.NET
        ValidateType();
        return NeuralNetworkFitter.Fit_SingleColumn(xData, yData, epochs, hiddenNodes, learningRate, trainingProgressCallback);
    }

    /// <summary>
    /// 使用神经网络进行回归拟合，适用于多列输入数据，旨在捕捉复杂非线性关系。
    /// 该方法通过多层感知器（MLP）训练一个神经网络，返回拟合结果，支持多列输入预测。
    /// </summary>
    /// <typeparam name="T">数据类型，必须是支持 IEEE 754 浮点运算的非托管类型，例如 <see cref="float"/> 或 <see cref="double"/>。</typeparam>
    /// <param name="xData">
    /// 输入数据，表示多列自变量，形状为 [样本数]。
    /// 每个元素是一个 <see cref="Row{T}"/> 对象，包含多个特征值，通常需要归一化到 [0, 1]。
    /// 所有行的列数必须一致。
    /// </param>
    /// <param name="yData">
    /// 目标数据，表示因变量，形状为 [样本数]。
    /// 每个元素是对应的输出值，与 <paramref name="xData"/> 一一对应，通常也需要归一化到 [0, 1]。
    /// </param>
    /// <param name="epochs">
    /// 训练轮数，控制神经网络的迭代次数，默认为 5000。
    /// 增加此值可以提高拟合精度，但可能增加计算时间，建议根据数据复杂度和收敛情况调整。
    /// </param>
    /// <param name="hiddenNodes">
    /// 隐藏层节点数，控制神经网络的容量，默认为 10。
    /// 更多的节点可以捕捉更复杂的非线性关系，但可能导致过拟合，建议根据任务复杂度选择。
    /// </param>
    /// <param name="learningRate">
    /// 学习率，控制权重更新的步长，可为空，默认为 0.1。
    /// 较小的值（如 0.01）可能使训练更稳定但收敛较慢，较大的值（如 0.5）可能加速收敛但不稳定。
    /// </param>
    /// <param name="trainingProgressCallback">
    /// 训练进度回调函数，可为空，用于监控训练过程。
    /// 接受三个参数：当前轮次 (epoch)、总轮次 (totalEpochs) 和平均误差 (averageError)，可用于日志记录或早停。
    /// 示例用法：(epoch, total, error) => Console.WriteLine($"Epoch {epoch}/{total}, Error: {error}");
    /// </param>
    /// <returns>
    /// 返回 <see cref="MultiColumnFitResult{T}"/> 对象，包含：
    /// - <see cref="MultiColumnFitResult{T}.Parameters"/>：网络参数（权重和偏置）。
    /// - <see cref="MultiColumnFitResult{T}.MeanSquaredError"/>：训练后的均方误差。
    /// - <see cref="MultiColumnFitResult{T}.MultiPredict"/>：多列输入预测函数。
    /// </returns>
    /// <exception cref="ArgumentException">
    /// 当 <paramref name="xData"/> 和 <paramref name="yData"/> 长度不匹配、数据为空，或行中列数不一致时抛出。
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// </exception>
    /// <remarks>
    /// 建议使用 Accord.NET 或 ML.NET 以获得更高效的实现和更多功能。
    /// 输入和输出数据应归一化到 [0, 1] 以获得最佳拟合效果。
    /// </remarks>
    public static MultiColumnFitResult<T> NeuralNetwork(DataRow<T>[] xData, Span<T> yData,
        int epochs = 5000, int hiddenNodes = 10, T? learningRate = null,
        TrainingProgressHandler<T>? trainingProgressCallback = null)
    {
        ValidateInput(xData, yData);
        // 建议使用 Accord.NET 或 ML.NET
        ValidateType();
        return NeuralNetworkFitter.Fit_MultiColumn(xData, yData, epochs, hiddenNodes, learningRate, trainingProgressCallback);
    }



    //失败

    ///// <summary>
    ///// 支持向量回归 (SVR)：鲁棒非线性拟合。
    ///// </summary>
    //public static FitResult SupportVector(double[] xData, double[] yData)
    //{
    //    ValidateInput(xData, yData);
    //    // 建议使用 Accord.NET
    //    throw new NotImplementedException("Requires SVR library.");
    //}

    ///// <summary>
    ///// 贝叶斯回归：带参数不确定性估计。
    ///// </summary>
    //public static FitResult Bayesian(double[] xData, double[] yData)
    //{
    //    ValidateInput(xData, yData);
    //    // 建议使用统计库
    //    throw new NotImplementedException("Requires Bayesian inference library.");
    //}

    /// <summary>
    /// 贝叶斯回归：带参数不确定性估计。
    /// </summary>
    public static BayesianFitResult<T> BayesianLinear(DataRow<T>[] xData, T[] yData, T alpha, T beta)
    {
        //ValidateInput(xData, yData);
        // 建议使用统计库
        return BayesianLinearRegression<T>.Fit(xData, yData, alpha, beta);
    }

}