using Vorcyc.Mathematics.SignalProcessing.Effects;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace SignalProcessing_example;

internal static class EffectDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 音效处理 (TremoloEffect) ---");

        const float rate = 44100f;
        const int length = (int)(rate * 0.5f);

        var dry = new Signal(length, rate);
        dry.GenerateWave(WaveShape.Sine, 440f, Behaviour.Replace);

        var wet = dry.Clone();
        var tremolo = new TremoloEffect((int)rate)
        {
            Depth = 0.6f,
            Frequency = 5f,
            Index = 0.8f
        };

        tremolo.Apply(wet);

        float dryRms = Rms(dry);
        float wetRms = Rms(wet);
        float modulationDepth = PeakToPeak(wet) / MathF.Max(PeakToPeak(dry), 1e-6f);

        Console.WriteLine($"Dry RMS: {dryRms:F4}, Wet RMS: {wetRms:F4}");
        Console.WriteLine($"峰峰值调制比: {modulationDepth:F3} (Tremolo 应引入幅度起伏)");
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

    static float PeakToPeak(Signal signal)
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        var span = signal.Samples;
        for (var i = 0; i < signal.Length; i++)
        {
            min = MathF.Min(min, span[i]);
            max = MathF.Max(max, span[i]);
        }

        return max - min;
    }
}
