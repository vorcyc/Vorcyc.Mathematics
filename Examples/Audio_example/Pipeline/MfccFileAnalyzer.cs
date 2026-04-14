using System.Globalization;
using System.Text;
using Audio_example.Io;
using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Audio_example.Pipeline;

internal static class MfccFileAnalyzer
{
    public static int Run(string wavPath, string? csvPath)
    {
        var info = WavBridge.Probe(wavPath);
        using var signal = WavBridge.ReadAsSignal(wavPath, PitchClassifierConfig.TargetSampleRate);

        var mfcc = AudioTrainingSamples.CreateDefaultMfccExtractor(
            PitchClassifierConfig.TargetSampleRate,
            PitchClassifierConfig.FeatureCount);

        var frames = mfcc.ComputeFrom(signal);
        var spectrum = signal.TransformToFrequencyDomain();

        Console.WriteLine($"--- WAV 分析 (NAudio → Signal) ---");
        Console.WriteLine($"文件: {Path.GetFullPath(wavPath)}");
        Console.WriteLine($"格式: {info.SampleRate} Hz, {info.Channels} ch, {info.BitsPerSample}-bit, {info.Duration.TotalSeconds:F2} s");
        Console.WriteLine($"重采样后: {signal.Length} 样本 @ {signal.SamplingRate:F0} Hz");
        Console.WriteLine($"RMS: {signal.GetRms():F4}");
        Console.WriteLine($"FFT 主频: {spectrum.Frequency:F1} Hz");
        Console.WriteLine($"MFCC: {frames.Count} 帧 × {mfcc.FeatureCount} 维");

        if (frames.Count > 0)
        {
            var mean = MeanFrames(frames);
            Console.WriteLine($"帧均值 MFCC[0..2]: {mean[0]:F3}, {mean[1]:F3}, {mean[2]:F3}");
        }

        if (csvPath is not null)
        {
            WriteCsv(csvPath, frames, mfcc.FeatureDescriptions);
            Console.WriteLine($"MFCC CSV: {Path.GetFullPath(csvPath)}");
        }

        return 0;
    }

    static float[] MeanFrames(List<float[]> frames)
    {
        if (frames.Count == 0)
        {
            return [];
        }

        int dim = frames[0].Length;
        var mean = new float[dim];
        foreach (var frame in frames)
        {
            for (int i = 0; i < dim; i++)
            {
                mean[i] += frame[i];
            }
        }

        for (int i = 0; i < dim; i++)
        {
            mean[i] /= frames.Count;
        }

        return mean;
    }

    static void WriteCsv(string path, List<float[]> frames, List<string> headers)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', headers));
        foreach (var frame in frames)
        {
            sb.AppendLine(string.Join(',', frame.Select(v => v.ToString("G6", CultureInfo.InvariantCulture))));
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, sb.ToString());
    }
}
