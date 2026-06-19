namespace DeepLearning_example;

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
                "xor" => XorDemo.Run(),
                "classify" => ClassifyDemo.Run(),
                "batch" => BatchTrainDemo.Run(),
                "cnn" => CnnMlpDemo.Run(),
                "audio" => AudioClassifyDemo.Run(),
                "serialize" => SerializeDemo.Run(),
                "curvefit" => CurveFitDemo.Run(),
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
        Console.WriteLine("=== Vorcyc.Mathematics.DeepLearning 鈥?绀轰緥姒傝 ===\n");
        XorDemo.Run();
        Console.WriteLine();
        ClassifyDemo.Run();
        Console.WriteLine();
        BatchTrainDemo.Run();
        Console.WriteLine();
        CnnMlpDemo.Run();
        Console.WriteLine();
        AudioClassifyDemo.Run();
        Console.WriteLine();
        SerializeDemo.Run();
        Console.WriteLine();
        CurveFitDemo.Run();
        return 0;
    }

    static int PrintHelp()
    {
        Console.WriteLine("""
            Vorcyc.Mathematics 鈥?DeepLearning 绀轰緥

            鐢ㄦ硶:
              dotnet run --project Examples/DeepLearning_example
              dotnet run --project Examples/DeepLearning_example -- <command>

            鍛戒护:
              overview    渚濇杩愯鍏ㄩ儴婕旂ず锛堥粯璁わ級
              xor         XOR 闈炵嚎鎬у洖褰?(Sequential + Trainer)
              classify    浜岀淮浜屽垎绫?(CrossEntropy + SGD)
              batch       鎵?XOR (FitBatched + ComputingContext.Parallel)
              cnn         4脳4 鍥炬 CNN+MLP (FitCnnMlp)
              audio       MFCC 鍧囧€肩壒寰佷簩鍒嗙被 (FitBatchSequential)
              serialize   ModelSerializer v3 淇濆瓨/鍔犺浇
              curvefit    CurveFitter 绁炵粡缃戠粶鎷熷悎 y = x虏
              help        鏄剧ず鏈府鍔?
            """);
        return 0;
    }

    static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"鏈煡鍛戒护: {command}");
        PrintHelp();
        return 1;
    }
}
