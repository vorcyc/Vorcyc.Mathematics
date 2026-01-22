using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// Frequency-domain characteristics interface, defining commonly used properties and methods for frequency-domain analysis.
/// </summary>
public interface IFrequencyDomainCharacteristics
{
    /// <summary>
    /// Gets the magnitude array of the frequency-domain signal.
    /// </summary>
    float[] Magnitudes { get; }

    /// <summary>
    /// Gets the spectral centroid (mass center).
    /// </summary>
    float Centroid { get; }

    /// <summary>
    /// Gets the frequency of the frequency-domain signal.
    /// </summary>
    float Frequency { get; }

    /// <summary>
    /// Gets the phase array of the frequency-domain signal.
    /// </summary>
    float[] Phases { get; }

    /// <summary>
    /// Gets the angular velocity array of the frequency-domain signal.
    /// </summary>
    float[] AngularVelocities { get; }



    /// <summary>
    /// Calculates the magnitude array of the frequency-domain signal.
    /// </summary>
    /// <param name="fftResult">The FFT result array.</param>
    /// <param name="retainedLength">The retained or adopted length.</param>
    /// <returns>The magnitude array.</returns>
    internal static float[] GetMagnitudes(ComplexFp32[] fftResult, int retainedLength)
    {
        float[] magnitudes = new float[retainedLength];
        for (int i = 0; i < magnitudes.Length; i++)
            magnitudes[i] = fftResult[i].Magnitude;
        // 忽略直流分量（第一个元素）
        magnitudes[0] = 0;
        return magnitudes;
    }

    /// <summary>
    /// Calculates the spectral centroid (mass center).
    /// </summary>
    /// <param name="magnitudes"></param>
    /// <param name="samplingRate"></param>
    /// <returns></returns>
    internal static float GetCentroid(float[] magnitudes, float samplingRate)
    {
        int N = magnitudes.Length;
        float[] freqs = Enumerable.Range(0, N).Select(i => i * samplingRate / N).ToArray();
        // 计算质心
        float numerator = freqs.Select((f, i) => f * magnitudes[i]).Sum();
        float denominator = magnitudes.Sum();
        float centroid = numerator / denominator;
        return centroid;
    }

    /// <summary>
    /// Calculates the frequency of the frequency-domain signal.
    /// </summary>
    /// <param name="magnitudes">The magnitude array.</param>
    /// <param name="samplingRate">The sampling rate.</param>
    /// <param name="resolution">The frequency resolution.</param>
    /// <returns>The frequency of the frequency-domain signal.</returns>
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
    /// Calculates the phase array of the frequency-domain signal.
    /// </summary>
    /// <param name="fftResult">The FFT result array.</param>
    /// <param name="retainedLength">The retained or adopted length.</param>
    /// <returns>The phase array.</returns>
    internal static float[] GetPhases(ComplexFp32[] fftResult, int retainedLength)
    {
        float[] phases = new float[retainedLength];
        for (int i = 0; i < phases.Length; i++)
        {
            phases[i] = fftResult[i].Phase;
        }
        return phases;
    }

    /// <summary>
    /// Calculates the angular velocity array of the frequency-domain signal.
    /// </summary>
    /// <param name="phases">The phase array.</param>
    /// <param name="samplingRate">The sampling rate.</param>
    /// <returns>The angular velocity array.</returns>
    internal static float[] GetAngularVelocities(float[] phases, float samplingRate)
    {
        float[] angularVelocity = new float[phases.Length - 1];
        for (int i = 0; i < phases.Length - 1; i++)
        {
            angularVelocity[i] = (phases[i + 1] - phases[i]) * samplingRate;
        }
        return angularVelocity;
    }


    /// <summary>
    /// Calculates the power spectral density.
    /// </summary>
    /// <param name="magnitudes"></param>
    /// <param name="samplingRate"></param>
    /// <returns></returns>
    internal static float[] GetPowerSpectralDensity(float[] magnitudes, float samplingRate)
    {
        float[] psd = new float[magnitudes.Length];
        for (int i = 0; i < magnitudes.Length; i++)
        {
            psd[i] = magnitudes[i] * magnitudes[i] / magnitudes.Length * 2 / samplingRate;
        }
        return psd;
    }
}