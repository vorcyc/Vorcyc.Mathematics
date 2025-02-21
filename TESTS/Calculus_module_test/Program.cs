using Vorcyc.Mathematics.Calculus;
using Vorcyc.Mathematics.Calculus.NumericalMethods;
using Vorcyc.Mathematics.Calculus.Series;

Console.WriteLine("=== Calculus Library Tests ===\n");

TestFourierSeries();
TestTaylorSeries();
TestDerivative();
TestIntegration();
TestTaylorVsDerivative();

Console.WriteLine("\nAll tests completed. Press any key to exit.");
Console.ReadKey();

static void TestFourierSeries()
{
    Console.WriteLine("--- FourierSeries Tests ---");
    SingleVariableFunction<double> sawtooth = x => x;
    var fourier = new FourierSeries<double>(sawtooth, 2.0, 1e-7);
    double x = 1.0;
    double result = fourier.Calculate(x, order: 5, segments: 1000);
    Console.WriteLine($"f(x) = x at x = {x}, Fourier (order 5): {result:F6} (Expected ~1.0)");

    double a0 = fourier.GetFourierCoefficient(true, 0);
    double a1 = fourier.GetFourierCoefficient(true, 1);
    double b1 = fourier.GetFourierCoefficient(false, 1);
    Console.WriteLine($"a0 = {a0:F6} (Expected 1.0)");
    Console.WriteLine($"a1 = {a1:F6} (Expected ~0)");
    Console.WriteLine($"b1 = {b1:F6} (Expected ~-0.6366)");
}

static void TestTaylorSeries()
{
    Console.WriteLine("\n--- TaylorSeries Tests ---");
    SingleVariableFunction<double> sin = x => Math.Sin(x);
    var taylor = new TaylorSeries<double>(sin, 0.0, 1e-7);
    double x = Math.PI / 4;
    double approx = taylor.Calculate(x, order: 5);
    double exact = Math.Sin(x);
    Console.WriteLine($"sin({x:F6}) at x = π/4, Taylor (order 5): {approx:F6}, Exact: {exact:F6}");

    double c0 = taylor.GetTaylorCoefficient(0);
    double c1 = taylor.GetTaylorCoefficient(1);
    double c2 = taylor.GetTaylorCoefficient(2);
    Console.WriteLine($"c0 = {c0:F6} (Expected 0)");
    Console.WriteLine($"c1 = {c1:F6} (Expected 1)");
    Console.WriteLine($"c2 = {c2:F6} (Expected 0)");

    SingleVariableFunction<double> exp = x => Math.Exp(x);
    var taylorExp = new TaylorSeries<double>(exp, 0.0, 1e-7);
    approx = taylorExp.Calculate(1.0, 5);
    exact = Math.Exp(1.0);
    Console.WriteLine($"exp(1), Taylor (order 5): {approx:F6}, Exact: {exact:F6}");
}

static void TestDerivative()
{
    Console.WriteLine("\n--- Derivative Tests ---");
    SingleVariableFunction<double> square = x => x * x;
    var deriv = new Derivative<double>(square, 1e-7);
    double d = deriv.Calculate(2.0);
    Console.WriteLine($"d/dx(x²) at x = 2: {d:F6} (Expected 4)");

    SingleVariableFunction<double> sin = x => Math.Sin(x);
    var derivSin = new Derivative<double>(sin, 1e-7);
    double d2 = derivSin.Calculate(0.0, order: 2);
    Console.WriteLine($"d²/dx²(sin(x)) at x = 0: {d2:F6} (Expected -1)");
}

static void TestIntegration()
{
    Console.WriteLine("\n--- Integration Tests ---");
    SingleVariableFunction<double> square = x => x * x;
    var integ = new Integration<double>(1e-7);
    double result = integ.Integrate(0.0, 1.0, 1000, square);
    Console.WriteLine($"∫x² dx from 0 to 1: {result:F6} (Expected 0.333333)");

    SingleVariableFunction<double> sin = x => Math.Sin(x);
    result = integ.Integrate(0.0, Math.PI, 1000, sin, method: Integration<double>.Method.Simpson);
    Console.WriteLine($"∫sin(x) dx from 0 to π (Simpson): {result:F6} (Expected 2)");
}

static void TestTaylorVsDerivative()
{
    Console.WriteLine("\n--- Taylor vs Derivative Test ---");
    SingleVariableFunction<double> exp = x => Math.Exp(x);
    var taylor = new TaylorSeries<double>(exp, 0.0, 1e-7);
    var deriv = new Derivative<double>(exp, 1e-7);

    for (int n = 0; n <= 3; n++)
    {
        double taylorCoef = taylor.GetTaylorCoefficient(n);
        double derivVal = (n == 0) ? exp(0.0) : deriv.Calculate(0.0, order: n) / Factorial(n);
        Console.WriteLine($"exp(x), order {n}: Taylor Coef = {taylorCoef:F6}, Deriv/n! = {derivVal:F6}");
    }
}

static double Factorial(int n)
{
    if (n <= 1) return 1.0;
    double result = 1.0;
    for (int i = 2; i <= n; i++) result *= i;
    return result;
}