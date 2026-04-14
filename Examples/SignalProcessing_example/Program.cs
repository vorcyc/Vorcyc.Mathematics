namespace SignalProcessing_example;

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
                "synth" => SynthDemo.Run(),
                "fft" => FftDemo.Run(),
                "filter" => FilterDemo.Run(),
                "mfcc" => MfccDemo.Run(args),
                "effect" => EffectDemo.Run(),
                "context" => ContextDemo.Run(),
                "help" or "-h" or "--help" => PrintHelp(),
                _ => UnknownCommand(command),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    static int RunOverview()
    {
        Console.WriteLine("=== Vorcyc.Mathematics.SignalProcessing — 示例概览 ===\n");
        SynthDemo.Run();
        Console.WriteLine();
        FftDemo.Run();
        Console.WriteLine();
        FilterDemo.Run();
        Console.WriteLine();
        MfccDemo.Run([]);
        Console.WriteLine();
        EffectDemo.Run();
        Console.WriteLine();
        ContextDemo.Run();
        return 0;
    }

    static int PrintHelp()
    {
        Console.WriteLine("""
            Vorcyc.Mathematics — SignalProcessing 示例

            用法:
              dotnet run --project Examples/SignalProcessing_example
              dotnet run --project Examples/SignalProcessing_example -- <command>

            命令:
              overview    依次运行全部演示（默认）
              synth       合成 C 大三和弦并归一化
              fft         440 Hz 正弦波 FFT 主频检测
              filter      Butterworth 低通滤波含噪信号
              mfcc        HTK 风格 MFCC 特征提取（可选 --csv out.csv）
              effect      Tremolo 调制效果
              context     ComputingContext：RMS、并行 MFCC
              help        显示本帮助
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
