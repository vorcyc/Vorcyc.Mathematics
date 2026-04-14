namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Offline fill helpers for <see cref="ISampleGenerator"/> instances.
/// </summary>
public static class SampleGeneratorExtensions
{
    /// <summary>
    /// Fills <paramref name="signal"/> by calling <see cref="ISampleGenerator.NextSample"/> for each sample.
    /// </summary>
    public static void Fill(this ISampleGenerator generator, Signal signal)
    {
        generator.Reset();
        for (var i = 0; i < signal.Length; i++)
        {
            signal[i] = generator.NextSample();
        }
    }

    /// <summary>
    /// Creates a new <see cref="Signal"/> filled from <paramref name="generator"/>.
    /// </summary>
    public static Signal ToSignal(this ISampleGenerator generator, int length, float samplingRate)
    {
        var signal = new Signal(length, samplingRate);
        generator.Fill(signal);
        return signal;
    }

    /// <summary>
    /// Sets LFO frequency when <paramref name="generator"/> supports <see cref="IAmplitudeOscillator"/>.
    /// </summary>
    public static void SetLfoFrequency(this ISampleGenerator generator, float frequency)
    {
        if (generator is IAmplitudeOscillator osc)
        {
            osc.Frequency = frequency;
        }
    }

    /// <summary>
    /// Sets LFO output range when <paramref name="generator"/> supports <see cref="IAmplitudeOscillator"/>.
    /// </summary>
    public static void SetLfoRange(this ISampleGenerator generator, float min, float max)
    {
        if (generator is IAmplitudeOscillator osc)
        {
            osc.Min = min;
            osc.Max = max;
        }
    }
}
