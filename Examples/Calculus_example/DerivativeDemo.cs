using Vorcyc.Mathematics.Calculus;

namespace Calculus_example;

internal static class DerivativeDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 数值微分 (Derivative) ---");

        SingleVariableFunction<double> f = x => Math.Sin(x);
        var deriv = new Derivative<double>(f, 1e-7);

        double x0 = Math.PI / 4;
        double d1 = deriv.Calculate(x0);
        double d2 = deriv.Calculate(x0, order: 2, method: Derivative<double>.Method.Central);

        Console.WriteLine($"f(x) = sin(x) 在 x = π/4");
        Console.WriteLine($"  f'(x)  ≈ {d1:F6}  (精确 {Math.Cos(x0):F6})");
        Console.WriteLine($"  f''(x) ≈ {d2:F6}  (精确 {-Math.Sin(x0):F6})");

        var grad = new Derivative<double>(vars => vars[0] * vars[0] + vars[1] * vars[1], 1e-6);
        var g = grad.Gradient([2.0, 3.0]);
        Console.WriteLine($"∇(x²+y²) 在 (2,3) = ({g[0]:F2}, {g[1]:F2})  (期望 4, 6)");
        return 0;
    }
}
