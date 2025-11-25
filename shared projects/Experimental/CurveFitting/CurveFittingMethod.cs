namespace Vorcyc.Mathematics.Experimental.CurveFitting;

/// <summary>
/// 表示不同的曲线拟合方法。
/// </summary>
public enum CurveFittingMethod
{
    // 经典回归方法

    /// <summary>
    /// 线性回归：适用于数据点呈线性关系的情况，通过直线拟合数据。
    /// </summary>
    /// <remarks>
    /// 数学意义：假设数据满足 \( y = ax + b + \epsilon \) 的形式，其中 \( a \) 是斜率，\( b \) 是截距，\( \epsilon \) 是随机误差。
    /// 使用最小二乘法最小化残差平方和 \( \sum (y_i - (ax_i + b))^2 \)，求解参数 \( a \) 和 \( b \)。
    /// </remarks>
    LinearRegression = 1,

    /// <summary>
    /// 多项式回归：适用于数据点呈非线性关系但可用多项式描述的情况。
    /// </summary>
    /// <remarks>
    /// 数学意义：拟合模型为 \( y = a_0 + a_1x + a_2x^2 + \cdots + a_nx^n \)，其中 \( n \) 是多项式次数。
    /// 通过最小二乘法求解系数 \( a_0, a_1, \ldots, a_n \)，适合捕捉平滑的非线性趋势。
    /// </remarks>
    PolynomialRegression = 2,

    /// <summary>
    /// 指数回归：适用于数据呈指数增长或衰减的情况。
    /// </summary>
    /// <remarks>
    /// 数学意义：模型为 \( y = a \cdot e^{bx} \) 或 \( y = a \cdot e^{-bx} \)，其中 \( a \) 是初始值，\( b \) 是增长/衰减率。
    /// 可通过取对数线性化 \( \ln(y) = \ln(a) + bx \)，再用最小二乘法求解。
    /// </remarks>
    ExponentialRegression = 3,

    /// <summary>
    /// 对数回归：适用于数据呈对数关系，初始快速增长后趋于平稳的情况。
    /// </summary>
    /// <remarks>
    /// 数学意义：模型为 \( y = a + b \ln(x) \)，其中 \( a \) 是偏移量，\( b \) 是对数影响因子。
    /// 通过最小二乘法优化参数，反映对数增长特性。
    /// </remarks>
    LogarithmicRegression = 4,

    /// <summary>
    /// 幂回归：适用于数据呈幂律关系的情况，如标度律或收益递减。
    /// </summary>
    /// <remarks>
    /// 数学意义：模型为 \( y = ax^b \)，其中 \( a \) 是比例因子，\( b \) 是幂指数。
    /// 可通过对数变换 \( \ln(y) = \ln(a) + b \ln(x) \) 线性化后求解。
    /// </remarks>
    PowerRegression = 5,

    /// <summary>
    /// 正弦回归：适用于数据具有周期性或波动特征的情况。
    /// </summary>
    /// <remarks>
    /// 数学意义：模型为 \( y = A \sin(Bx + C) + D \)，其中 \( A \) 是幅度，\( B \) 控制周期，\( C \) 是相位移，\( D \) 是垂直偏移。
    /// 通过非线性优化或傅里叶分析估计参数。
    /// </remarks>
    SinusoidalRegression = 6,

    // 插值与平滑方法

    /// <summary>
    /// 样条插值：适用于数据点之间的平滑插值，保证曲线穿过所有数据点。
    /// </summary>
    /// <remarks>
    /// 数学意义：使用分段多项式（如三次样条 \( s(x) = a_i + b_i(x-x_i) + c_i(x-x_i)^2 + d_i(x-x_i)^3 \)）拟合。
    /// 要求一阶和二阶导数连续，保证平滑性。
    /// </remarks>
    SplineInterpolation = 10,

    /// <summary>
    /// 局部加权回归 (LOWESS/LOESS)：适用于数据分布不均匀、需要局部趋势拟合的情况。
    /// </summary>
    /// <remarks>
    /// 数学意义：对每个点 \( x_0 \) 使用加权最小二乘法拟合局部模型 \( y = a + bx \)，权重 \( w_i = K(|x_i - x_0|) \) 随距离衰减。
    /// 强调局部特性，避免全局假设。
    /// </remarks>
    LocallyWeightedRegression = 11,

    /// <summary>
    /// 移动平均拟合：适用于时间序列数据平滑去噪。
    /// </summary>
    /// <remarks>
    /// 数学意义：计算局部平均值 \( y_t = \frac{1}{k} \sum_{i=t-k/2}^{t+k/2} y_i \)，其中 \( k \) 是窗口大小。
    /// 简单线性平滑，适合趋势提取。
    /// </remarks>
    MovingAverageFitting = 12,

    // 最小二乘法与优化技术

    /// <summary>
    /// 最小二乘法：通用的曲线拟合方法，通过最小化残差平方和优化参数。
    /// </summary>
    /// <remarks>
    /// 数学意义：目标是最小化 \( S = \sum (y_i - f(x_i; \theta))^2 \)，其中 \( f(x; \theta) \) 是拟合函数，\( \theta \) 是参数。
    /// 对于线性模型可直接求解，对于非线性模型需迭代优化。
    /// </remarks>
    LeastSquaresMethod = 20,

    /// <summary>
    /// 非线性回归：适用于数据呈复杂非线性关系的情况。
    /// </summary>
    /// <remarks>
    /// 数学意义：模型 \( y = f(x; \theta) \) 为非线性函数，通过迭代算法（如梯度下降）最小化 \( \sum (y_i - f(x_i; \theta))^2 \)。
    /// 参数 \( \theta \) 无闭式解，依赖数值优化。
    /// </remarks>
    NonlinearRegression = 21,

    // 现代与高级方法

    /// <summary>
    /// 高斯过程回归 (GPR)：适用于数据具有不确定性、需要平滑预测的情况。
    /// </summary>
    /// <remarks>
    /// 数学意义：假设 \( y = f(x) + \epsilon \)，其中 \( f(x) \) 服从高斯过程，协方差由核函数 \( k(x, x') \) 定义。
    /// 输出预测均值和方差，提供置信区间。
    /// </remarks>
    GaussianProcessRegression = 30,

    /// <summary>
    /// 神经网络回归：适用于数据关系极其复杂的情况。
    /// </summary>
    /// <remarks>
    /// 数学意义：模型 \( y = f(x; W, b) \) 由多层神经网络定义，\( W \) 和 \( b \) 是权重和偏置。
    /// 通过反向传播最小化损失函数（如均方误差）。
    /// </remarks>
    NeuralNetworkRegression = 31,

    /// <summary>
    /// 支持向量回归 (SVR)：适用于数据分布复杂且需要鲁棒性的情况。
    /// </summary>
    /// <remarks>
    /// 数学意义：目标是找到 \( f(x) = w^T \phi(x) + b \)，使预测值在 \( \epsilon \) 管道内，损失函数考虑间隔外误差。
    /// 使用核函数 \( \phi(x) \) 处理非线性。
    /// </remarks>
    SupportVectorRegression = 32,

    /// <summary>
    /// 贝叶斯回归：适用于需要参数不确定性建模或结合先验知识的情况。
    /// </summary>
    /// <remarks>
    /// 数学意义：假设 \( y = X\beta + \epsilon \)，参数 \( \beta \) 具有先验分布（如正态分布）。
    /// 通过贝叶斯定理计算后验分布 \( P(\beta|y) \)，提供参数不确定性。
    /// </remarks>
    BayesianRegression = 33
}