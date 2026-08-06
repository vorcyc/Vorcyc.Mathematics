using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// Frequency-domain characteristics interface.
/// </summary>
public interface IFrequencyDomainCharacteristics
{
    float[] Magnitudes { get; }

    float Centroid { get; }

    float Frequency { get; }

    float[] Phases { get; }

    float[] AngularVelocities { get; }

    /// <summary>
    /// Onesided magnitudes for bins <c>0 … retainedLength-1</c> (typically <c>N/2+1</c>).
    /// Does not zero DC.
    /// </summary>
    internal static float[] GetMagnitudes(ComplexFp32[] fftResult, int retainedLength)
    {
        int n = Math.Min(retainedLength, fftResult.Length);
        float[] magnitudes = new float[n];
        for (int i = 0; i < n; i++)
            magnitudes[i] = fftResult[i].Magnitude;
        return magnitudes;
    }

    /// <param name="fftLength">Full FFT length N (not onesided bin count).</param>
    internal static float GetCentroid(float[] magnitudes, float samplingRate, int fftLength)
    {
        if (fftLength < 1) fftLength = Math.Max(1, magnitudes.Length);
        var binWidth = samplingRate / fftLength;

        var numerator = 0.0f;
        var denominator = 0.0f;
        for (var i = 0; i < magnitudes.Length; i++)
        {
            var freq = i * binWidth;
            numerator += freq * magnitudes[i];
            denominator += magnitudes[i];
        }

        return denominator > 1e-30f ? numerator / denominator : 0f;
    }

    /// <summary>Dominant tone frequency; skips DC bin when looking for a peak.</summary>
    internal static float GetFrequency(float[] magnitudes, float samplingRate, float resolution)
    {
        if (magnitudes.Length == 0) return 0f;
        int start = magnitudes.Length > 1 ? 1 : 0;
        int maxIndex = start;
        float maxVal = magnitudes[start];
        for (int i = start + 1; i < magnitudes.Length; i++)
        {
            if (magnitudes[i] > maxVal)
            {
                maxVal = magnitudes[i];
                maxIndex = i;
            }
        }

        float frequency = maxIndex * resolution;
        if (frequency > samplingRate / 2)
            frequency = samplingRate - frequency;
        return frequency;
    }

    internal static float[] GetPhases(ComplexFp32[] fftResult, int retainedLength)
    {
        int n = Math.Min(retainedLength, fftResult.Length);
        float[] phases = new float[n];
        for (int i = 0; i < n; i++)
            phases[i] = fftResult[i].Phase;
        return phases;
    }

    internal static float[] GetAngularVelocities(float[] phases, float samplingRate)
    {
        if (phases.Length < 2)
            return [];
        float[] angularVelocity = new float[phases.Length - 1];
        for (int i = 0; i < phases.Length - 1; i++)
            angularVelocity[i] = (phases[i + 1] - phases[i]) * samplingRate;
        return angularVelocity;
    }

    /// <summary>
    /// Onesided power spectral density from magnitudes |X[k]|.
    /// <c>P = |X|² / (fs·N)</c>, with onesided ×2 on interior bins (not DC / Nyquist).
    /// </summary>
    /// <param name="fftLength">Full FFT length N.</param>
    internal static float[] GetPowerSpectralDensity(float[] magnitudes, float samplingRate, int fftLength)
    {
        if (fftLength < 1) fftLength = Math.Max(1, (magnitudes.Length - 1) * 2);
        float inv = 1f / (Math.Max(1e-30f, samplingRate) * fftLength);
        float[] psd = new float[magnitudes.Length];
        int last = magnitudes.Length - 1;
        for (int i = 0; i < magnitudes.Length; i++)
        {
            float p = magnitudes[i] * magnitudes[i] * inv;
            if (i > 0 && i < last)
                p *= 2f;
            psd[i] = p;
        }
        return psd;
    }
}
