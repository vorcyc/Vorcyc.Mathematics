using Vorcyc.Mathematics.SignalProcessing.Windowing;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// 频域特性接口，定义了频域分析中常用的特性和方法。
/// </summary>
public interface IFrequencyDomainCharacteristics
{
    /// <summary>
    /// 获取频域信号的幅度数组。
    /// </summary>
    float[] Magnitudes { get; }

    /// <summary>
    /// 获取质心 ( Mass Center)
    /// </summary>
    float Centroid { get; }

    /// <summary>
    /// 获取频域信号的频率。
    /// </summary>
    float Frequency { get; }

    /// <summary>
    /// 获取频域信号的相位数组。
    /// </summary>
    float[] Phases { get; }

    /// <summary>
    /// 获取频域信号的角速度数组。
    /// </summary>
    float[] AngularVelocities { get; }



    /// <summary>
    /// 计算频域信号的幅度数组。
    /// </summary>
    /// <param name="fftResult">FFT 结果数组。</param>
    /// <param name="actualLength">实际长度。</param>
    /// <returns>幅度数组。</returns>
    internal static float[] GetMagnitudes(ComplexFp32[] fftResult, int actualLength)
    {
        float[] magnitudes = new float[actualLength];
        for (int i = 0; i < magnitudes.Length; i++)
            magnitudes[i] = fftResult[i].Magnitude;
        // 忽略直流分量（第一个元素）
        magnitudes[0] = 0;
        return magnitudes;
    }

    /// <summary>
    /// 计算质心 (Mass Center).
    /// </summary>
    /// <param name="fftResult"></param>
    /// <param name="actualLength"></param>
    /// <param name="samplingRate"></param>
    /// <returns></returns>
    internal static float GetCentroid(ComplexFp32[] fftResult, int actualLength, float samplingRate)
    {
        int N = actualLength;
        float[] magnitudes = GetMagnitudes(fftResult, actualLength);
        float[] freqs = Enumerable.Range(0, N).Select(i => i * samplingRate / N).ToArray();
        // 计算质心
        float numerator = freqs.Select((f, i) => f * magnitudes[i]).Sum();
        float denominator = magnitudes.Sum(); 
        float centroid = numerator / denominator;
        return centroid;
    }

    /// <summary>
    /// 计算频域信号的频率。
    /// </summary>
    /// <param name="magnitudes">幅度数组。</param>
    /// <param name="samplingRate">采样率。</param>
    /// <param name="resolution">频率分辨率。</param>
    /// <returns>频域信号的频率。</returns>
    internal static float GetFrequency(float[] magnitudes, float samplingRate, float resolution)
    {
        // 找到最大幅度对应的索引
        // int maxIndex = Array.IndexOf(magnitudes, magnitudes.Max());
        var (maxIndex, _) = magnitudes.LocateMax();
        // 计算频率
        float frequency = maxIndex * resolution;
        // 确保频率在合理范围内
        if (frequency > samplingRate / 2)
        {
            frequency = samplingRate - frequency;
        }
        return frequency;
    }

    /// <summary>
    /// 计算频域信号的相位数组。
    /// </summary>
    /// <param name="fftResult">FFT 结果数组。</param>
    /// <param name="actualLength">实际长度。</param>
    /// <returns>相位数组。</returns>
    internal static float[] GetPhases(ComplexFp32[] fftResult, int actualLength)
    {
        float[] phases = new float[actualLength];
        for (int i = 0; i < phases.Length; i++)
        {
            phases[i] = fftResult[i].Phase;
        }
        return phases;
    }

    /// <summary>
    /// 计算频域信号的角速度数组。
    /// </summary>
    /// <param name="phases">相位数组。</param>
    /// <param name="samplingRate">采样率。</param>
    /// <returns>角速度数组。</returns>
    internal static float[] GetAngularVelocities(float[] phases, float samplingRate)
    {
        float[] angularVelocity = new float[phases.Length - 1];
        for (int i = 0; i < phases.Length - 1; i++)
        {
            angularVelocity[i] = (phases[i + 1] - phases[i]) * samplingRate;
        }
        return angularVelocity;
    }
}


/// <summary>
/// 表示频域信号接口，继承了 IFrequencyDomainCharacteristics 接口。
/// </summary>
public interface IFrequencyDomain : IFrequencyDomainCharacteristics
{
    /// <summary>
    /// 获取频域转换位于时域的偏移量。
    /// </summary>
    int Offset { get; }

    /// <summary>
    /// 获取频域转换的长度。
    /// </summary>
    /// <remarks>
    /// 长度为 2 的 N 次方。有可能是补0为了FFT的长度，因此 >= ActualLength .
    /// </remarks>
    int TransformLength { get; }

    /// <summary>
    /// 获取实际信号的长度。
    /// </summary>
    int ActualLength { get; }

    /// <summary>
    /// 获取频率分辨率。
    /// </summary>
    float Resolution { get; }

    /// <summary>
    /// 获取应用的窗口类型。
    /// </summary>
    WindowType? WindowApplied { get; }

    /// <summary>
    /// 获取 FFT 结果的复数数组。
    /// </summary>
    ComplexFp32[] Result { get; }

    /// <summary>
    /// 获取原始时域信号。
    /// </summary>
    ITimeDomainSignal Signal { get; }

    /// <summary>
    /// 对 FFT 结果进行逆变换，并将结果写回信号的采样数据中。
    /// </summary>
    void Inverse();
}
