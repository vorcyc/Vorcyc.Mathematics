using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace SignalProcessing_example;

internal static class FilterDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 数字滤波 (Butterworth LowPass + FilterOnline) ---");

        const float rate = 8000f;
        const int length = 2048;
        const float toneHz = 200f;

        var clean = new Signal(length, rate);
        clean.GenerateWave(WaveShape.Sine, toneHz, Behaviour.Replace);

        var noisy = clean.Clone();
        var rng = new Random(7);
        var samples = noisy.Samples;
        for (var i = 0; i < noisy.Length; i++)
        {
            samples[i] += (rng.NextSingle() - 0.5f) * 0.35f;
        }

        noisy.NotifySamplesModified();

        float cutoffNorm = 400f / rate;
        var lowPass = new LowPassFilter(cutoffNorm, order: 4);
        var filtered = lowPass.FilterOnline(noisy);

        float rmsBefore = Rms(noisy);
        float rmsAfter = Rms(filtered);
        float highBandBefore = HighBandEnergy(noisy, rate);
        float highBandAfter = HighBandEnergy(filtered, rate);

        Console.WriteLine($"含噪信号 RMS: {rmsBefore:F4} → 滤波后: {rmsAfter:F4}");
        Console.WriteLine($"高频能量比 (>{toneHz * 2:F0} Hz): {highBandBefore:F4} → {highBandAfter:F4}");
        return 0;
    }

    static float Rms(Signal signal)
    {
        double sum = 0;
        var span = signal.Samples;
        for (var i = 0; i < signal.Length; i++)
        {
            sum += span[i] * span[i];
        }

        return MathF.Sqrt((float)(sum / signal.Length));
    }

    static float HighBandEnergy(Signal signal, float rate)
    {
        var spectrum = signal.TransformToFrequencyDomain();
        var mags = spectrum.Magnitudes;
        float resolution = spectrum.Resolution;
        double total = 0;
        double high = 0;
        for (var i = 0; i < mags.Length; i++)
        {
            double e = mags[i] * mags[i];
            total += e;
            if (i * resolution > 500f)
            {
                high += e;
            }
        }

        return total < 1e-12 ? 0f : (float)(high / total);
    }
}
