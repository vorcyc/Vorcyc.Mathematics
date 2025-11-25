namespace Vorcyc.Mathematics.Experimental.KalmanFilters;

using System.Numerics;

/// <summary>
/// 表示一个一维平方根卡尔曼滤波器。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
/// <remarks>
/// 平方根卡尔曼滤波器是一种递归算法，用于通过结合系统的数学模型和测量数据来估计动态系统的状态。它在许多领域中都有广泛应用，包括导航、控制系统、信号处理和经济学等。
/// 
/// 平方根卡尔曼滤波器的基本思想是利用协方差矩阵的平方根分解来提高数值稳定性和计算效率。它包括两个主要步骤：
/// 
/// 1. 预测步骤：根据系统的数学模型，预测当前时刻的系统状态和误差协方差的平方根。
/// 2. 更新步骤：利用当前时刻的测量数据，更新系统状态的估计和误差协方差的平方根。
/// 
/// 平方根卡尔曼滤波器的优点在于它能够在噪声环境中提供对系统状态的最佳估计，并且计算效率高，适合实时应用。
/// 
/// 示例代码：
/// <code>
/// var A = 1.0f;
/// var B = 0.5f;
/// var H = 1.0f;
/// var Q = 0.1f;
/// var R = 0.1f;
/// var initialState = 0.0f;
/// var initialS = 1.0f;
/// 
/// var srkf = new SquareRootKalmanFilter1D&lt;float&gt;(A, B, H, Q, R, initialState, initialS);
/// 
/// float[] audioSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
/// float[] filteredSamples = new float[audioSamples.Length];
/// 
/// for (int i = 0; i &lt; audioSamples.Length; i++)
/// {
///     var u = 0.0f; // 控制输入为0
///     var z = audioSamples[i]; // 当前测量值
///     
///     var predictedState = srkf.Predict(u);
///     var updatedState = srkf.Update(z);
///     
///     filteredSamples[i] = updatedState;
/// }
/// </code>
/// </remarks>
//[Filter(design: FilterDesignMethod.SquareRootKalman, structure: FilterStructure.Kalman, description: "一维平方根卡尔曼滤波器")]
public class SquareRootKalmanFilter1D<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T A; // 状态转移系数
    private T B; // 控制输入系数
    private T H; // 观测系数
    private T Q; // 过程噪声协方差
    private T R; // 测量噪声协方差
    private T S; // 估计误差协方差的平方根
    private T x; // 状态估计

    /// <summary>
    /// 初始化平方根卡尔曼滤波器的实例。
    /// </summary>
    /// <param name="A">状态转移系数。</param>
    /// <param name="B">控制输入系数。</param>
    /// <param name="H">观测系数。</param>
    /// <param name="Q">过程噪声协方差。</param>
    /// <param name="R">测量噪声协方差。</param>
    /// <param name="initialState">初始状态估计。</param>
    /// <param name="initialS">初始估计误差协方差的平方根。</param>
    public SquareRootKalmanFilter1D(T A, T B, T H, T Q, T R, T initialState, T initialS)
    {
        this.A = A;
        this.B = B;
        this.H = H;
        this.Q = Q;
        this.R = R;
        this.x = initialState;
        this.S = initialS;
    }

    /// <summary>
    /// 执行预测步骤。
    /// </summary>
    /// <param name="u">控制输入。</param>
    /// <returns>预测的状态估计。</returns>
    public T Predict(T u)
    {
        // x = A * x + B * u
        x = A * x + B * u;

        // S = sqrt(A * S * S^T * A^T + Q)
        S = T.Sqrt(A * S * S * A + Q);

        return x;
    }

    /// <summary>
    /// 执行更新步骤。
    /// </summary>
    /// <param name="z">测量值。</param>
    /// <returns>更新后的状态估计。</returns>
    public T Update(T z)
    {
        // K = S * H^T * (H * S * S^T * H^T + R)^-1
        T SHt = S * H;
        T K = SHt / (H * SHt + R);

        // x = x + K * (z - H * x)
        x = x + K * (z - H * x);

        // S = sqrt((I - K * H) * S * S^T)
        T I = T.One;
        S = T.Sqrt((I - K * H) * S * S);

        return x;
    }
}