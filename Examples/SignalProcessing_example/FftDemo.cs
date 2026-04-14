using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace SignalProcessing_example;

internal static class FftDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 频域分析 (TransformToFrequencyDomain) ---");

        const float rate = 8000f;
        const float targetHz = 440f;
        const int length = 4096;

        var tone = new Signal(length, rate);
        tone.GenerateWave(WaveShape.Sine, targetHz, Behaviour.Replace);

        var spectrum = tone.TransformToFrequencyDomain(context: null, WindowType.Hamming);
        float detected = spectrum.Frequency;

        Console.WriteLine($"目标频率: {targetHz:F1} Hz");
        Console.WriteLine($"FFT 检测主频: {detected:F1} Hz");
        Console.WriteLine($"频谱质心: {spectrum.Centroid:F1} Hz");
        Console.WriteLine($"频率分辨率: {spectrum.Resolution:F2} Hz/bin");

        Console.WriteLine();
        Console.WriteLine("--- ComputingContext / ComputingScope ---");

        float explicitSimd = tone.TransformToFrequencyDomain(ComputingContext.Simd, WindowType.Hamming).Frequency;
        Console.WriteLine($"显式 SIMD FFT 主频: {explicitSimd:F1} Hz");

        using (ComputingScope.Enter(ComputingContext.Parallel))
        {
            var scopedTone = new Signal(length, rate);
            scopedTone.GenerateWave(WaveShape.Sine, targetHz, Behaviour.Replace);
            float scopedFreq = scopedTone.TransformToFrequencyDomain(context: null, WindowType.Hamming).Frequency;
            Console.WriteLine($"Scope Parallel FFT 主频: {scopedFreq:F1} Hz");
        }

        return 0;
    }
}
