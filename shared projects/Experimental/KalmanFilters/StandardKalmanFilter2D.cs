namespace Vorcyc.Mathematics.Experimental.KalmanFilters;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// 表示一个二维标准卡尔曼滤波器。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
/// <remarks>
/// 标准卡尔曼滤波器是一种递归算法，用于通过结合系统的数学模型和测量数据来估计动态系统的状态。它在许多领域中都有广泛应用，包括导航、控制系统、信号处理和经济学等。
/// 
/// 标准卡尔曼滤波器的基本思想是利用系统的先验知识（预测）和测量数据（观测）来更新系统状态的估计。它包括两个主要步骤：
/// 
/// 1. 预测步骤：根据系统的数学模型，预测当前时刻的系统状态和误差协方差。
/// 2. 更新步骤：利用当前时刻的测量数据，更新系统状态的估计和误差协方差。
/// 
/// 标准卡尔曼滤波器的优点在于它能够在噪声环境中提供对系统状态的最佳估计，并且计算效率高，适合实时应用。
/// 
/// 示例代码：
/// <code>
/// var A = new Matrix&lt;float&gt;(new float[,] { { 1, 0 }, { 0, 1 } });
/// var B = new Matrix&lt;float&gt;(new float[,] { { 0.5f }, { 1 } });
/// var H = new Matrix&lt;float&gt;(new float[,] { { 1, 0 }, { 0, 1 } });
/// var Q = new Matrix&lt;float&gt;(new float[,] { { 0.1f, 0 }, { 0, 0.1f } });
/// var R = new Matrix&lt;float&gt;(new float[,] { { 0.1f, 0 }, { 0, 0.1f } });
/// var initialState = new Matrix&lt;float&gt;(new float[,] { { 0 }, { 0 } });
/// var initialP = new Matrix&lt;float&gt;(new float[,] { { 1, 0 }, { 0, 1 } });
/// 
/// var kf = new StandardKalmanFilter2D&lt;float&gt;(A, B, H, Q, R, initialState, initialP);
/// 
/// var u = new Matrix&lt;float&gt;(new float[,] { { 1 } });
/// var z = new Matrix&lt;float&gt;(new float[,] { { 1 }, { 1 } });
/// 
/// var predictedState = kf.Predict(u);
/// var updatedState = kf.Update(z);
/// </code>
/// </remarks>
//[Filter(design: FilterDesignMethod.StandardKalman, structure: FilterStructure.Kalman, description: "二维标准卡尔曼滤波器")]
public class StandardKalmanFilter2D<T> where T : struct, INumber<T>
{
    private Matrix<T> A; // 状态转移矩阵
    private Matrix<T> B; // 控制输入矩阵
    private Matrix<T> H; // 观测矩阵
    private Matrix<T> Q; // 过程噪声协方差
    private Matrix<T> R; // 测量噪声协方差
    private Matrix<T> P; // 估计误差协方差
    private Matrix<T> x; // 状态估计

    /// <summary>
    /// 初始化卡尔曼滤波器的实例。
    /// </summary>
    /// <param name="A">状态转移矩阵。</param>
    /// <param name="B">控制输入矩阵。</param>
    /// <param name="H">观测矩阵。</param>
    /// <param name="Q">过程噪声协方差。</param>
    /// <param name="R">测量噪声协方差。</param>
    /// <param name="initialState">初始状态估计。</param>
    /// <param name="initialP">初始估计误差协方差。</param>
    public StandardKalmanFilter2D(Matrix<T> A, Matrix<T> B, Matrix<T> H, Matrix<T> Q, Matrix<T> R, Matrix<T> initialState, Matrix<T> initialP)
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
    /// <returns>预测的状态估计。</returns>
    public Matrix<T> Predict(Matrix<T> u)
    {
        // x = A * x + B * u
        x = A * x + B * u;

        // P = A * P * A^T + Q
        P = A * P * A.Transpose() + Q;

        return x;
    }

    /// <summary>
    /// 执行更新步骤。
    /// </summary>
    /// <param name="z">测量值。</param>
    /// <returns>更新后的状态估计。</returns>
    public Matrix<T> Update(Matrix<T> z)
    {
        // K = P * H^T * (H * P * H^T + R)^-1
        var Ht = H.Transpose();
        var S = H * P * Ht + R;
        var K = P * Ht * S.Inverse();

        // x = x + K * (z - H * x)
        x = x + K * (z - H * x);

        // P = (I - K * H) * P
        var I = Matrix<T>.Eye(P.Rows);
        P = (I - K * H) * P;

        return x;
    }
}
