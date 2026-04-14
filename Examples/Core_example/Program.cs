namespace Core_example;

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
                "matrix" => MatrixDemo.Run(),
                "statistics" => StatisticsDemo.Run(),
                "vector" => VectorDemo.Run(),
                "context" => ComputingContextDemo.Run(),
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
        Console.WriteLine("=== Vorcyc.Mathematics — Core / LinearAlgebra / Statistics 示例 ===\n");
        MatrixDemo.Run();
        Console.WriteLine();
        StatisticsDemo.Run();
        Console.WriteLine();
        VectorDemo.Run();
        Console.WriteLine();
        ComputingContextDemo.Run();
        return 0;
    }

    static int PrintHelp()
    {
        Console.WriteLine("""
            Vorcyc.Mathematics — Core 示例

            用法:
              dotnet run --project Examples/Core_example
              dotnet run --project Examples/Core_example -- <command>

            命令:
              overview     依次运行全部演示（默认）
              matrix       矩阵乘法与 ComputingContext
              statistics   Span 统计归约（Sum / Average / StdDev）
              vector       VectorSpan 点积、范数、Axpy
              context      ComputingScope 与解析优先级
              help         显示本帮助
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
