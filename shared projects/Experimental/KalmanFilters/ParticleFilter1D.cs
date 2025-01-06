namespace Vorcyc.Mathematics.Experimental.KalmanFilters;

using System;
using System.Numerics;

/// <summary>
/// 表示一个一维粒子滤波器。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
/// <remarks>
/// 粒子滤波器是一种递归算法，用于通过结合系统的数学模型和测量数据来估计动态系统的状态。它在许多领域中都有广泛应用，包括导航、控制系统、信号处理和经济学等。
/// 
/// 粒子滤波器的基本思想是使用粒子群来表示概率分布，并通过重要性采样和重采样来更新粒子。它包括两个主要步骤：
/// 
/// 1. 预测步骤：根据系统的数学模型，预测当前时刻的粒子状态。
/// 2. 更新步骤：利用当前时刻的测量数据，更新粒子的权重，并进行重采样。
/// 
/// 粒子滤波器的优点在于它能够处理高度非线性和非高斯分布的系统，并且计算效率高，适合实时应用。
/// 
/// 示例代码：
/// <code>
/// var numParticles = 1000;
/// var initialState = 0.0f;
/// var initialStateStdDev = 1.0f;
/// var processNoiseStdDev = 0.1f;
/// var measurementNoiseStdDev = 0.1f;
/// 
/// var pf = new ParticleFilter1D&lt;float&gt;(numParticles, initialState, initialStateStdDev, processNoiseStdDev, measurementNoiseStdDev);
/// 
/// float[] audioSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
/// float[] filteredSamples = new float[audioSamples.Length];
/// 
/// for (int i = 0; i &lt; audioSamples.Length; i++)
/// {
///     var u = 0.0f; // 控制输入为0
///     var z = audioSamples[i]; // 当前测量值
///     
///     pf.Predict(u);
///     pf.Update(z);
///     
///     filteredSamples[i] = pf.Estimate();
/// }
/// </code>
/// </remarks>
//[Filter(design: FilterDesignMethod.ParticleFilter, structure: FilterStructure.Kalman, description: "一维粒子滤波器")]
public class ParticleFilter1D<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private int numParticles;
    private T[] particles;
    private T[] weights;
    private T processNoiseStdDev;
    private T measurementNoiseStdDev;
    private Random random;

    /// <summary>
    /// 初始化粒子滤波器的实例。
    /// </summary>
    /// <param name="numParticles">粒子数量。</param>
    /// <param name="initialState">初始状态估计。</param>
    /// <param name="initialStateStdDev">初始状态标准差。</param>
    /// <param name="processNoiseStdDev">过程噪声标准差。</param>
    /// <param name="measurementNoiseStdDev">测量噪声标准差。</param>
    public ParticleFilter1D(int numParticles, T initialState, T initialStateStdDev, T processNoiseStdDev, T measurementNoiseStdDev)
    {
        this.numParticles = numParticles;
        this.particles = new T[numParticles];
        this.weights = new T[numParticles];
        this.processNoiseStdDev = processNoiseStdDev;
        this.measurementNoiseStdDev = measurementNoiseStdDev;
        this.random = new Random();

        // 初始化粒子和权重
        for (int i = 0; i < numParticles; i++)
        {
            particles[i] = initialState + initialStateStdDev * T.CreateChecked(random.NextDouble());
            weights[i] = T.One / T.CreateChecked(numParticles);
        }
    }

    /// <summary>
    /// 执行预测步骤。
    /// </summary>
    /// <param name="u">控制输入。</param>
    public void Predict(T u)
    {
        for (int i = 0; i < numParticles; i++)
        {
            // 预测粒子状态
            particles[i] = particles[i] + u + processNoiseStdDev * T.CreateChecked(random.NextDouble());
        }
    }

    /// <summary>
    /// 执行更新步骤。
    /// </summary>
    /// <param name="z">测量值。</param>
    public void Update(T z)
    {
        T weightSum = T.Zero;

        for (int i = 0; i < numParticles; i++)
        {
            // 计算粒子的权重
            T measurementError = z - particles[i];
            weights[i] = T.Exp(-measurementError * measurementError / (T.CreateChecked(2) * measurementNoiseStdDev * measurementNoiseStdDev));
            weightSum += weights[i];
        }

        // 归一化权重
        for (int i = 0; i < numParticles; i++)
        {
            weights[i] /= weightSum;
        }

        // 重采样
        Resample();
    }

    /// <summary>
    /// 估计当前状态。
    /// </summary>
    /// <returns>当前状态的估计值。</returns>
    public T Estimate()
    {
        T estimate = T.Zero;

        for (int i = 0; i < numParticles; i++)
        {
            estimate += particles[i] * weights[i];
        }

        return estimate;
    }

    private void Resample()
    {
        T[] newParticles = new T[numParticles];
        T[] cumulativeWeights = new T[numParticles];
        cumulativeWeights[0] = weights[0];

        for (int i = 1; i < numParticles; i++)
        {
            cumulativeWeights[i] = cumulativeWeights[i - 1] + weights[i];
        }

        for (int i = 0; i < numParticles; i++)
        {
            T randomValue = T.CreateChecked(random.NextDouble());
            int index = Array.BinarySearch(cumulativeWeights, randomValue);

            if (index < 0)
            {
                index = ~index;
            }

            newParticles[i] = particles[index];
        }

        particles = newParticles;
    }
}