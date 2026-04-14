using Vorcyc.Mathematics.Calculus.NumericalMethods;

namespace Calculus_example;

internal static class OdeDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 常微分方程 (RungeKutta) ---");

        // y' = -2y, y(0) = 1  →  y(t) = e^{-2t}
        var rk = new RungeKutta<double>((_, y) => -2.0 * y, 1e-7);
        const int steps = 200;
        double yAt1 = rk.Solve(0, 1, 1, steps);
        double yExact = Math.Exp(-2);

        Console.WriteLine($"y' = -2y, y(0)=1, RK4 步数 {steps}");
        Console.WriteLine($"  t=1.0: 数值 {yAt1:F6}, 解析 {yExact:F6}, 误差 {Math.Abs(yAt1 - yExact):E2}");
        return 0;
    }
}
