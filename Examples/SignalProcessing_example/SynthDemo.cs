using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace SignalProcessing_example;

internal static class SynthDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 信号合成 (Signal + GenerateWave) ---");

        const float rate = 44100f;
        const int length = (int)rate;
        var chord = new Signal(length, rate);

        chord.GenerateWave(WaveShape.Sine, 261.63f, Behaviour.Replace);
        chord.GenerateWave(WaveShape.Sine, 329.63f, Behaviour.ElementWiseAdd);
        chord.GenerateWave(WaveShape.Sine, 392.00f, Behaviour.ElementWiseAdd);
        chord.NormalizeMax();

        float peak = 0f;
        var samples = chord.Samples;
        for (var i = 0; i < chord.Length; i++)
        {
            peak = MathF.Max(peak, MathF.Abs(samples[i]));
        }

        var segment = chord[0, Math.Min(1024, chord.Length)];
        Console.WriteLine($"采样率: {chord.SamplingRate:F0} Hz, 时长: {chord.Duration.TotalSeconds:F2} s");
        Console.WriteLine($"峰值幅度: {peak:F4} (归一化后应接近 1)");
        Console.WriteLine($"零拷贝切片 SignalSegment 长度: {segment?.Length ?? 0}");
        return 0;
    }
}
