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

        Console.WriteLine("=== Vorcyc.Mathematics.DeepLearning — 示例概览 ===\n");

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

            Vorcyc.Mathematics — DeepLearning 示例



            用法:

              dotnet run --project Examples/DeepLearning_example

              dotnet run --project Examples/DeepLearning_example -- <command>



            命令:

              overview    依次运行全部演示（默认）

              xor         XOR 非线性回归 (Sequential + Trainer)

              classify    二维二分类 (CrossEntropy + SGD)

              batch       批 XOR (FitBatched + ComputingContext.Parallel)

              cnn         4×4 图案 CNN+MLP (FitCnnMlp)

              audio       MFCC 均值特征二分类 (FitBatchSequential)

              serialize   ModelSerializer v3 保存/加载

              curvefit    CurveFitter 神经网络拟合 y = x²

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

