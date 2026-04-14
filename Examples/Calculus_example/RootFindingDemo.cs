using Vorcyc.Mathematics.Calculus;
using Vorcyc.Mathematics.Calculus.NumericalMethods;

namespace Calculus_example;

internal static class RootFindingDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 求根 (NewtonRaphson / Brent) ---");

        SingleVariableFunction<double> f = x => x * x - 2.0;

        var newton = new NewtonRaphson<double>(f, 1e-7);
        double rootNewton = newton.Solve(1.0);

        var brent = new Brent<double>(f);
        double rootBrent = brent.Solve(0.0, 2.0);

        Console.WriteLine($"f(x) = x² - 2");
        Console.WriteLine($"  Newton-Raphson (初值 1.0): {rootNewton:F8}  (期望 √2)");
        Console.WriteLine($"  Brent [0, 2]:              {rootBrent:F8}");
        Console.WriteLine($"  f(√2) ≈ {f(rootBrent):E2}");
        return 0;
    }
}
