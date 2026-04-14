using Audio_example.Pipeline;

namespace Audio_example;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            string command = args.Length == 0 ? "overview" : args[0].ToLowerInvariant();
            return command switch
            {
                "overview" or "all" => RunOverview(),
                "prepare" => RunPrepare(args),
                "analyze" => RunAnalyze(args),
                "train" => RunTrain(args),
                "predict" => RunPredict(args),
                "help" or "-h" or "--help" => PrintHelp(),
                _ => UnknownCommand(command),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    static int RunOverview()
    {
        Console.WriteLine("=== Vorcyc.Mathematics — Audio 示例 (NAudio + MFCC + DL) ===\n");

        string dataRoot = ResolveDataRoot([]);
        RunPrepare(["prepare", dataRoot]);
        Console.WriteLine();

        int trainCode = RunTrain(["train", "--data", dataRoot, "--epochs", "2500"]);
        Console.WriteLine();

        string probe = Path.Combine(dataRoot, PitchClassifierConfig.ClassHighDir, "tone_1200hz.wav");
        if (!File.Exists(probe))
        {
            probe = Directory.EnumerateFiles(
                Path.Combine(dataRoot, PitchClassifierConfig.ClassHighDir),
                "*.wav").First();
        }

        RunAnalyze(["analyze", probe]);
        Console.WriteLine();
        RunPredict(["predict", probe]);
        return trainCode;
    }

    static int RunPrepare(string[] args)
    {
        string dataRoot = ResolveDataRoot(args, defaultIndex: 1);
        DemoDataFactory.Prepare(dataRoot);
        int count = DemoDataFactory.CountPreparedFiles(dataRoot);
        Console.WriteLine($"--- 演示数据已写入 {Path.GetFullPath(dataRoot)} ---");
        Console.WriteLine($"WAV 文件数: {count}");
        Console.WriteLine($"  {PitchClassifierConfig.ClassLowDir}/  → 标签 low (低频)");
        Console.WriteLine($"  {PitchClassifierConfig.ClassHighDir}/ → 标签 high (高频)");
        Console.WriteLine("可用自己的 WAV 替换上述目录中的文件，然后重新 train。");
        return 0;
    }

    static int RunAnalyze(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("用法: analyze <file.wav> [--csv out.csv]");
        }

        string wavPath = args[1];
        string? csv = ParseOption(args, "--csv");
        return MfccFileAnalyzer.Run(wavPath, csv);
    }

    static int RunTrain(string[] args)
    {
        string dataRoot = ResolveOption(args, "--data") ?? PitchClassifierConfig.DataRoot;
        string modelPath = ResolveOption(args, "--model") ?? PitchClassifierConfig.DefaultModelPath;
        int epochs = int.Parse(ResolveOption(args, "--epochs") ?? "3000", System.Globalization.CultureInfo.InvariantCulture);

        if (DemoDataFactory.CountPreparedFiles(dataRoot) == 0)
        {
            Console.WriteLine("data/ 为空，自动执行 prepare …");
            DemoDataFactory.Prepare(dataRoot);
        }

        return PitchClassifierTrainer.Train(dataRoot, modelPath, epochs);
    }

    static int RunPredict(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("用法: predict <file.wav> [--model path]");
        }

        string wavPath = args[1];
        string modelPath = ResolveOption(args, "--model") ?? PitchClassifierConfig.DefaultModelPath;
        return PitchClassifierTrainer.Predict(wavPath, modelPath);
    }

    static string ResolveDataRoot(string[] args, int defaultIndex = 0)
    {
        if (args.Length > defaultIndex && !args[defaultIndex].StartsWith('-'))
        {
            return args[defaultIndex];
        }

        return PitchClassifierConfig.DataRoot;
    }

    static string? ResolveOption(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    static string? ParseOption(string[] args, string name) => ResolveOption(args, name);

    static int PrintHelp()
    {
        Console.WriteLine("""
            Vorcyc.Mathematics — Audio 示例 (NAudio)

            依赖: NAudio 读/写 WAV；Vorcyc Signal + MFCC + BatchSequential 训练。

            用法:
              dotnet run --project Examples/Audio_example
              dotnet run --project Examples/Audio_example -- <command>

            命令:
              overview     准备数据 → 训练 → 分析 → 推理（默认）
              prepare      生成 16-bit PCM 演示 WAV 到 data/class_low|class_high
              analyze      NAudio 读取 WAV，输出 RMS / FFT / MFCC（可选 --csv）
              train        从 data/ 加载 WAV 训练 MFCC 分类器（--epochs --model）
              predict      对单个 WAV 推理（--model）
              help         显示本帮助

            示例:
              dotnet run --project Examples/Audio_example -- prepare
              dotnet run --project Examples/Audio_example -- analyze data/class_low/tone_220hz.wav
              dotnet run --project Examples/Audio_example -- train --epochs 2500
              dotnet run --project Examples/Audio_example -- predict my.wav
            """);
        return 0;
    }

    static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"未知命令: {command}");
        PrintHelp();
        return 1;
    }
}
