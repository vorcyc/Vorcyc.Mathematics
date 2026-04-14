using System.Globalization;
using System.Text;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Options;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace SignalProcessing_example;

internal static class MfccDemo
{
    public static int Run(string[] args)
    {
        Console.WriteLine("--- MFCC 特征 (MfccExtractor + HTK 预设) ---");

        const int samplingRate = 16000;
        const float speechHz = 220f;
        var signal = new Signal(samplingRate, samplingRate);
        signal.GenerateWave(WaveShape.Sine, speechHz, Behaviour.Replace);

        var options = new MfccHtkOptions(
            samplingRate,
            featureCount: 13,
            frameDuration: 0.025f,
            lowFrequency: 80f,
            highFrequency: 7000f);

        var extractor = new MfccExtractor(options);
        var frames = extractor.ComputeFrom(signal);

        Console.WriteLine($"帧数: {frames.Count}, 每帧维度: {extractor.FeatureCount}");
        Console.WriteLine($"帧长/步长: {extractor.FrameSize}/{extractor.HopSize} 样本");
        Console.WriteLine($"特征名: {string.Join(", ", extractor.FeatureDescriptions.Take(4))}, …");

        if (frames.Count > 0)
        {
            var first = frames[0];
            Console.WriteLine($"第 0 帧 MFCC: {string.Join(", ", first.Select(v => v.ToString("F3", CultureInfo.InvariantCulture)))}");
        }

        string? csvPath = ParseCsvPath(args);
        if (csvPath is not null)
        {
            WriteCsv(csvPath, frames, extractor.FeatureDescriptions);
            Console.WriteLine($"已写入 CSV: {csvPath}");
        }

        return 0;
    }

    static string? ParseCsvPath(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--csv")
            {
                return args[i + 1];
            }
        }

        return null;
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
