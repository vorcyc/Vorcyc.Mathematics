namespace Calculus_example;

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
                "integrate" => IntegrationDemo.Run(),
                "derivative" => DerivativeDemo.Run(),
                "ode" => OdeDemo.Run(),
                "root" => RootFindingDemo.Run(),
                "series" => SeriesDemo.Run(),
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
        Console.WriteLine("=== Vorcyc.Mathematics.Calculus — 示例概览 ===\n");
        IntegrationDemo.Run();
        Console.WriteLine();
        DerivativeDemo.Run();
        Console.WriteLine();
        OdeDemo.Run();
        Console.WriteLine();
        RootFindingDemo.Run();
        Console.WriteLine();
        SeriesDemo.Run();
        return 0;
    }

    static int PrintHelp()
    {
        Console.WriteLine("""
            Vorcyc.Mathematics — Calculus 示例

            用法:
              dotnet run --project Examples/Calculus_example
              dotnet run --project Examples/Calculus_example -- <command>

            命令:
              overview     依次运行全部演示（默认）
              integrate    定积分 ∫x² dx、Gauss-Legendre
              derivative   数值导数与梯度
              ode          龙格-库塔解指数衰减 ODE
              root         牛顿法 / Brent 求根
              series       泰勒级数与傅里叶级数
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
