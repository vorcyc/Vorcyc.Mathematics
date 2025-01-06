namespace Vorcyc.Mathematics.Experimental.KalmanFilters;

using System.Numerics;

/// <summary>
/// 表示一个一维无迹卡尔曼滤波器。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
/// <remarks>
/// 无迹卡尔曼滤波器是一种递归算法，用于通过结合非线性系统的数学模型和测量数据来估计动态系统的状态。它在许多领域中都有广泛应用，包括导航、控制系统、信号处理和经济学等。
/// 
/// 无迹卡尔曼滤波器的基本思想是利用无迹变换（Unscented Transform）来处理非线性系统，而不需要线性化。它包括两个主要步骤：
/// 
/// 1. 预测步骤：根据系统的非线性数学模型，预测当前时刻的系统状态和误差协方差。
/// 2. 更新步骤：利用当前时刻的测量数据，更新系统状态的估计和误差协方差。
/// 
/// 无迹卡尔曼滤波器的优点在于它能够在噪声环境中提供对系统状态的最佳估计，并且计算效率高，适合实时应用。
/// 
/// 示例代码：
/// <code>
/// var A = 1.0f;
/// var B = 0.5f;
/// var H = 1.0f;
/// var Q = 0.1f;
/// var R = 0.1f;
/// var initialState = 0.0f;
/// var initialP = 1.0f;
/// 
/// var ukf = new UnscentedKalmanFilter1D&lt;float&gt;(A, B, H, Q, R, initialState, initialP);
/// 
/// float NonlinearStateTransitionFunction(float x, float u)
/// {
///     // 定义非线性状态转移函数
///     return x + u;
/// }
/// 
/// float NonlinearMeasurementFunction(float x)
/// {
///     // 定义非线性观测函数
///     return x;
/// }
/// 
/// float[] audioSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
/// float[] filteredSamples = new float[audioSamples.Length];
/// 
/// for (int i = 0; i &lt; audioSamples.Length; i++)
/// {
///     var u = 0.0f; // 控制输入为0
///     var z = audioSamples[i]; // 当前测量值
///     
///     var predictedState = ukf.Predict(u, NonlinearStateTransitionFunction);
///     var updatedState = ukf.Update(z, NonlinearMeasurementFunction);
///     
///     filteredSamples[i] = updatedState;
/// }
/// </code>
/// </remarks>
//[Filter(design: FilterDesignMethod.UnscentedKalman, structure: FilterStructure.Kalman, description: "一维无迹卡尔曼滤波器")]
public class UnscentedKalmanFilter1D<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T A; // 状态转移系数
    private T B; // 控制输入系数
    private T H; // 观测系数
    private T Q; // 过程噪声协方差
    private T R; // 测量噪声协方差
    private T P; // 估计误差协方差
    private T x; // 状态估计

    /// <summary>
    /// 初始化无迹卡尔曼滤波器的实例。
    /// </summary>
    /// <param name="A">状态转移系数。</param>
    /// <param name="B">控制输入系数。</param>
    /// <param name="H">观测系数。</param>
    /// <param name="Q">过程噪声协方差。</param>
    /// <param name="R">测量噪声协方差。</param>
    /// <param name="initialState">初始状态估计。</param>
    /// <param name="initialP">初始估计误差协方差。</param>
    public UnscentedKalmanFilter1D(T A, T B, T H, T Q, T R, T initialState, T initialP)
    {
        this.A = A;
        this.B = B;
        this.H = H;
        this.Q = Q;
        this.R = R;
        this.x = initialState;
        this.P = initialP;
    }

    /// <summary>
    /// 执行预测步骤。
    /// </summary>
    /// <param name="u">控制输入。</param>
    /// <param name="stateTransitionFunc">状态转移函数。</param>
    /// <returns>预测的状态估计。</returns>
    public T Predict(T u, Func<T, T, T> stateTransitionFunc)
    {
        // 生成 sigma 点
        var sigmaPoints = GenerateSigmaPoints(x, P);

        // 通过状态转移函数预测 sigma 点
        var predictedSigmaPoints = TransformSigmaPoints(sigmaPoints, stateTransitionFunc, u);

        // 计算预测状态和协方差
        x = CalculateMean(predictedSigmaPoints);
        P = CalculateCovariance(predictedSigmaPoints, x) + Q;

        return x;
    }

    /// <summary>
    /// 执行更新步骤。
    /// </summary>
    /// <param name="z">测量值。</param>
    /// <param name="measurementFunc">观测函数。</param>
    /// <returns>更新后的状态估计。</returns>
    public T Update(T z, Func<T, T> measurementFunc)
    {
        // 生成 sigma 点
        var sigmaPoints = GenerateSigmaPoints(x, P);

        // 通过观测函数预测 sigma 点
        var predictedMeasurementSigmaPoints = TransformSigmaPoints(sigmaPoints, measurementFunc);

        // 计算预测测量值和协方差
        var predictedMeasurement = CalculateMean(predictedMeasurementSigmaPoints);
        var measurementCovariance = CalculateCovariance(predictedMeasurementSigmaPoints, predictedMeasurement) + R;

        // 计算交叉协方差
        var crossCovariance = CalculateCrossCovariance(sigmaPoints, x, predictedMeasurementSigmaPoints, predictedMeasurement);

        // 计算卡尔曼增益
        var K = crossCovariance / measurementCovariance;

        // 更新状态和协方差
        x = x + K * (z - predictedMeasurement);
        P = P - K * measurementCovariance * K;

        return x;
    }

    private T[] GenerateSigmaPoints(T mean, T covariance)
    {
        // 生成 sigma 点的实现
        int n = 1; // 状态维度
        T[] sigmaPoints = new T[2 * n + 1];
        T sqrtCovariance = T.Sqrt(covariance);

        sigmaPoints[0] = mean;
        sigmaPoints[1] = mean + sqrtCovariance;
        sigmaPoints[2] = mean - sqrtCovariance;

        return sigmaPoints;
    }

    private T[] TransformSigmaPoints(T[] sigmaPoints, Func<T, T, T> transformFunc, T u)
    {
        // 通过状态转移函数预测 sigma 点的实现
        T[] transformedSigmaPoints = new T[sigmaPoints.Length];
        for (int i = 0; i < sigmaPoints.Length; i++)
        {
            transformedSigmaPoints[i] = transformFunc(sigmaPoints[i], u);
        }
        return transformedSigmaPoints;
    }

    private T[] TransformSigmaPoints(T[] sigmaPoints, Func<T, T> transformFunc)
    {
        // 通过观测函数预测 sigma 点的实现
        T[] transformedSigmaPoints = new T[sigmaPoints.Length];
        for (int i = 0; i < sigmaPoints.Length; i++)
        {
            transformedSigmaPoints[i] = transformFunc(sigmaPoints[i]);
        }
        return transformedSigmaPoints;
    }

    private T CalculateMean(T[] sigmaPoints)
    {
        // 计算 sigma 点的均值
        T mean = T.Zero;
        for (int i = 0; i < sigmaPoints.Length; i++)
        {
            mean += sigmaPoints[i];
        }
        return mean / T.CreateChecked(sigmaPoints.Length);
    }

    private T CalculateCovariance(T[] sigmaPoints, T mean)
    {
        // 计算 sigma 点的协方差
        T covariance = T.Zero;
        for (int i = 0; i < sigmaPoints.Length; i++)
        {
            T diff = sigmaPoints[i] - mean;
            covariance += diff * diff;
        }
        return covariance / T.CreateChecked(sigmaPoints.Length);
    }

    private T CalculateCrossCovariance(T[] sigmaPoints, T mean, T[] predictedMeasurementSigmaPoints, T predictedMeasurement)
    {
        // 计算交叉协方差
        T crossCovariance = T.Zero;
        for (int i = 0; i < sigmaPoints.Length; i++)
        {
            T diffState = sigmaPoints[i] - mean;
            T diffMeasurement = predictedMeasurementSigmaPoints[i] - predictedMeasurement;
            crossCovariance += diffState * diffMeasurement;
        }
        return crossCovariance / T.CreateChecked(sigmaPoints.Length);
    }
}
