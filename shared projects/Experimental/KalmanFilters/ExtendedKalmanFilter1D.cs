﻿namespace Vorcyc.Mathematics.Experimental.KalmanFilters;

using System.Numerics;

/// <summary>
/// 表示一个一维扩展卡尔曼滤波器。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
/// <remarks>
/// 扩展卡尔曼滤波器是一种递归算法，用于通过结合非线性系统的数学模型和测量数据来估计动态系统的状态。它在许多领域中都有广泛应用，包括导航、控制系统、信号处理和经济学等。
/// 
/// 扩展卡尔曼滤波器的基本思想是利用系统的先验知识（预测）和测量数据（观测）来更新系统状态的估计。它包括两个主要步骤：
/// 
/// 1. 预测步骤：根据系统的非线性数学模型，预测当前时刻的系统状态和误差协方差。
/// 2. 更新步骤：利用当前时刻的测量数据，更新系统状态的估计和误差协方差。
/// 
/// 扩展卡尔曼滤波器的优点在于它能够在噪声环境中提供对系统状态的最佳估计，并且计算效率高，适合实时应用。
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
/// var ekf = new ExtendedKalmanFilter1D&lt;float&gt;(A, B, H, Q, R, initialState, initialP);
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
///     var predictedState = ekf.Predict(u, NonlinearStateTransitionFunction);
///     var updatedState = ekf.Update(z, NonlinearMeasurementFunction);
///     
///     filteredSamples[i] = updatedState;
/// }
/// </code>
/// </remarks>
//[Filter(design: FilterDesignMethod.ExtendedKalman, structure: FilterStructure.Kalman, description: "一维扩展卡尔曼滤波器")]
public class ExtendedKalmanFilter1D<T>
    where T : struct, INumber<T>
{
    private T A; // 状态转移系数
    private T B; // 控制输入系数
    private T H; // 观测系数
    private T Q; // 过程噪声协方差
    private T R; // 测量噪声协方差
    private T P; // 估计误差协方差
    private T x; // 状态估计

    /// <summary>
    /// 初始化扩展卡尔曼滤波器的实例。
    /// </summary>
    /// <param name="A">状态转移系数。</param>
    /// <param name="B">控制输入系数。</param>
    /// <param name="H">观测系数。</param>
    /// <param name="Q">过程噪声协方差。</param>
    /// <param name="R">测量噪声协方差。</param>
    /// <param name="initialState">初始状态估计。</param>
    /// <param name="initialP">初始估计误差协方差。</param>
    public ExtendedKalmanFilter1D(T A, T B, T H, T Q, T R, T initialState, T initialP)
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
        // x = f(x, u)
        x = stateTransitionFunc(x, u);

        // P = A * P * A + Q
        P = A * P * A + Q;

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
        // K = P * H / (H * P * H + R)
        T K = P * H / (H * P * H + R);

        // x = x + K * (z - h(x))
        x = x + K * (z - measurementFunc(x));

        // P = (T.One - K * H) * P
        P = (T.One - K * H) * P;

        return x;
    }
}