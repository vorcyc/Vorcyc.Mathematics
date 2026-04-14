using Vorcyc.Mathematics.Calculus;
using Vorcyc.Mathematics.Calculus.Series;

namespace Calculus_example;

internal static class SeriesDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 级数展开 (Taylor / Fourier) ---");

        var taylor = new TaylorSeries<double>(x => Math.Exp(x), center: 0.0, defaultH: 1e-8);
        double x = 0.5;
        double taylorApprox = taylor.Calculate(x, order: 6);
        Console.WriteLine($"exp({x}), Taylor 阶 6: {taylorApprox:F8}  (精确 {Math.Exp(x):F8})");

        SingleVariableFunction<double> saw = t => t;
        var fourier = new FourierSeries<double>(saw, period: 2.0, defaultH: 1e-6);
        double fourierVal = fourier.Calculate(0.8, order: 10, segments: 800);
        Console.WriteLine($"锯齿波 f(x)=x, 周期 2, x=0.8: Fourier 阶 10 → {fourierVal:F4}  (期望 ≈0.8)");
        return 0;
    }
}
