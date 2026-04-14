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

    internal static float[] GetMagnitudes(ComplexFp32[] fftResult, int retainedLength)
    {
        float[] magnitudes = new float[retainedLength];
        for (int i = 0; i < magnitudes.Length; i++)
        {
            magnitudes[i] = fftResult[i].Magnitude;
        }

        magnitudes[0] = 0;
        return magnitudes;
    }

    internal static float GetCentroid(float[] magnitudes, float samplingRate)
    {
        var n = magnitudes.Length;
        var binWidth = samplingRate / n;

        var numerator = 0.0f;
        var denominator = 0.0f;
        for (var i = 0; i < n; i++)
        {
            var freq = i * binWidth;
            numerator += freq * magnitudes[i];
            denominator += magnitudes[i];
        }

        return numerator / denominator;
    }

    internal static float GetFrequency(float[] magnitudes, float samplingRate, float resolution)
    {
        var (maxIndex, _) = magnitudes.LocateMax();
        float frequency = maxIndex * resolution;
        if (frequency > samplingRate / 2)
        {
            frequency = samplingRate - frequency;
        }

        return frequency;
    }

    internal static float[] GetPhases(ComplexFp32[] fftResult, int retainedLength)
    {
        float[] phases = new float[retainedLength];
        for (int i = 0; i < phases.Length; i++)
        {
            phases[i] = fftResult[i].Phase;
        }

        return phases;
    }

    internal static float[] GetAngularVelocities(float[] phases, float samplingRate)
    {
        float[] angularVelocity = new float[phases.Length - 1];
        for (int i = 0; i < phases.Length - 1; i++)
        {
            angularVelocity[i] = (phases[i + 1] - phases[i]) * samplingRate;
        }

        return angularVelocity;
    }

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
