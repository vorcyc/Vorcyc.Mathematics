using System.Diagnostics;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Options;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace SignalProcessing_example;

internal static class ContextDemo
{
    public static int Run()
    {
        Console.WriteLine("--- ComputingContext：时域特征与并行 MFCC ---");

        const int samplingRate = 16000;
        const int seconds = 2;
        var signal = new Signal(samplingRate * seconds, samplingRate);
        signal.GenerateWave(WaveShape.Sine, 440f, Behaviour.Replace);
        signal.GenerateWave(WaveShape.Sine, 880f, Behaviour.ElementWiseAdd);
        signal.NormalizeMax();

        float rmsDefault = signal.GetRms();
        float rmsParallel = signal.GetRms(ComputingContext.Parallel);
        Console.WriteLine($"RMS: default={rmsDefault:F4}, Parallel={rmsParallel:F4}");

        var options = new MfccHtkOptions(
            samplingRate,
            featureCount: 13,
            frameDuration: 0.025f,
            lowFrequency: 80f,
            highFrequency: 7000f);

        var extractor = new MfccExtractor(options);

        var sw = Stopwatch.StartNew();
        var serial = extractor.ComputeFrom(signal);
        sw.Stop();
        long serialMs = sw.ElapsedMilliseconds;

        sw.Restart();
        var parallel = extractor.ParallelComputeFrom(signal, parallelThreads: 0, context: ComputingContext.Parallel);
        sw.Stop();
        long parallelMs = sw.ElapsedMilliseconds;

        Console.WriteLine($"MFCC 帧数: serial={serial.Count}, parallel={parallel.Count}");
        if (serial.Count > 0 && parallel.Count > 0)
        {
            float diff = MathF.Abs(serial[0][0] - parallel[0][0]);
            Console.WriteLine($"第 0 帧 MFCC[0] 差: {diff:E3}");
        }

        Console.WriteLine($"耗时: serial={serialMs} ms, parallel={parallelMs} ms");

        using (ComputingScope.Enter(ComputingContext.Parallel))
        {
            var scoped = extractor.ComputeFrom(signal);
            Console.WriteLine($"ComputingScope 内 ComputeFrom 帧数: {scoped.Count}");
        }

        return 0;
    }
}
