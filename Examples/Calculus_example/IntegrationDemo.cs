using Vorcyc.Mathematics.Calculus;

namespace Calculus_example;

internal static class IntegrationDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 数值积分 (Integration) ---");

        var integrator = new Integration<double>(1e-8);
        SingleVariableFunction<double> f = x => x * x;

        double trap = integrator.Integrate(0, 1, 200, f);
        double romberg = integrator.Integrate(0, 1, 8, f, method: Integration<double>.Method.Romberg);
        double gauss = integrator.Integrate(0, 1, 8, f, method: Integration<double>.Method.GaussLegendre);

        Console.WriteLine($"∫₀¹ x² dx");
        Console.WriteLine($"  梯形法 (200 段): {trap:F8}  (精确 1/3)");
        Console.WriteLine($"  Romberg:         {romberg:F8}");
        Console.WriteLine($"  Gauss-Legendre:  {gauss:F8}");
        return 0;
    }
}
