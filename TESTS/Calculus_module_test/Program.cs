using Vorcyc.Mathematics.Calculus;
using Vorcyc.Mathematics.Calculus.NumericalMethods;
using Vorcyc.Mathematics.Calculus.Optimization;
using Vorcyc.Mathematics.Calculus.Series;

Console.WriteLine("=== Calculus Library Tests ===\n");

TestFourierSeries();
TestTaylorSeries();
TestDerivative();
TestIntegration();
TestTaylorVsDerivative();
TestLimits();
TestNewtonRaphson();
TestBisectionAndBrent();
TestRungeKutta();
TestRungeKuttaSystem();
TestDualNumber();
TestGradientAndJacobian();
TestIndefiniteIntegration();
TestHessian();
TestAdaptiveRungeKutta();
TestFourierFromSamples();
TestHighOrderDerivative();
TestGaussLegendre();
TestAdaptiveSimpson();
TestNewtonRaphsonSystem();
TestAdaptiveRungeKuttaSystem();
TestOdeTrajectory();
TestOptimization();
TestLBfgs();
TestHyperDualAndPade();
TestChebyshevAndDoubleIntegral();
TestImplicitEulerAndBvp();

Console.WriteLine("\nAll tests completed.");

static void TestFourierSeries()
{
    Console.WriteLine("--- FourierSeries Tests ---");
    SingleVariableFunction<double> sawtooth = x => x;
    var fourier = new FourierSeries<double>(sawtooth, 2.0, 1e-7);
    double x = 1.0;
    double result = fourier.Calculate(x, order: 5, segments: 1000);
    Console.WriteLine($"f(x) = x at x = {x}, Fourier (order 5): {result:F6} (Expected ~1.0)");

    double a0 = fourier.GetFourierCoefficient(true, 0);
    double b1 = fourier.GetFourierCoefficient(false, 1);
    Console.WriteLine($"a0 = {a0:F6} (Expected 2.0, series uses a0/2)");
    Console.WriteLine($"b1 = {b1:F6} (Expected ~-0.6366)");
}

static void TestTaylorSeries()
{
    Console.WriteLine("\n--- TaylorSeries Tests ---");
    var taylor = new TaylorSeries<double>(x => Math.Exp(x), 0.0, 1e-5);
    double approx = taylor.Calculate(0.5, order: 4);
    double exact = Math.Exp(0.5);
    Console.WriteLine($"exp(0.5), Taylor (order 4): {approx:F6}, Exact: {exact:F6}");
}

static void TestDerivative()
{
    Console.WriteLine("\n--- Derivative Tests ---");
    var deriv = new Derivative<double>(x => x * x, 1e-8);
    double d = deriv.Calculate(2.0);
    Console.WriteLine($"d/dx(x²) at x = 2: {d:F6} (Expected 4)");

    double d2 = deriv.Calculate(2.0, order: 2, method: Derivative<double>.Method.Central);
    Console.WriteLine($"d²/dx²(x²) at x = 2: {d2:F6} (Expected 2)");
}

static void TestIntegration()
{
    Console.WriteLine("\n--- Integration Tests ---");
    var integ = new Integration<double>(1e-7);
    double result = integ.Integrate(0.0, 1.0, 1000, x => x * x);
    Console.WriteLine($"∫x² dx [0,1] Trapezoidal: {result:F6} (Expected 0.333333)");

    result = integ.Integrate(0.0, 1.0, 8, x => x * x, method: Integration<double>.Method.Romberg);
    Console.WriteLine($"∫x² dx [0,1] Romberg: {result:F8} (Expected 0.33333333)");
}

static void TestTaylorVsDerivative()
{
    Console.WriteLine("\n--- Taylor vs Derivative Test ---");
    var taylor = new TaylorSeries<double>(x => Math.Exp(x), 0.0, 1e-5);
    var deriv = new Derivative<double>(x => Math.Exp(x), 1e-8);

    for (int n = 0; n <= 3; n++)
    {
        double taylorCoef = taylor.GetTaylorCoefficient(n);
        double derivVal = n == 0 ? 1.0 : deriv.Calculate(0.0, order: n) / Factorial(n);
        Console.WriteLine($"exp(x), order {n}: Taylor = {taylorCoef:F6}, Deriv/n! = {derivVal:F6}");
    }
}

static void TestLimits()
{
    Console.WriteLine("\n--- Limits Tests ---");
    var limits = new Limits<double>(x => x * x, 1e-2);
    double fromLeft = limits.CalculateLimit(0.0, Limits<double>.Direction.Left, tolerance: 1e-6);
    double fromRight = limits.CalculateLimit(0.0, Limits<double>.Direction.Right, tolerance: 1e-6);
    Console.WriteLine($"lim x² as x→0⁻: {fromLeft:F6}, x→0⁺: {fromRight:F6} (Expected 0)");
}

static void TestNewtonRaphson()
{
    Console.WriteLine("\n--- NewtonRaphson Tests ---");
    var nr = new NewtonRaphson<double>(x => x * x - 2.0, 1e-7);
    double root = nr.Solve(1.0);
    Console.WriteLine($"√2 via Newton: {root:F8} (Expected 1.41421356)");
}

static void TestBisectionAndBrent()
{
    Console.WriteLine("\n--- Bisection / Brent Tests ---");
    SingleVariableFunction<double> f = x => x * x - 2.0;

    var bisection = new Bisection<double>(f);
    double rootB = bisection.Solve(0.0, 2.0);
    Console.WriteLine($"√2 via Bisection: {rootB:F8}");

    var brent = new Brent<double>(f);
    double rootR = brent.Solve(0.0, 2.0);
    Console.WriteLine($"√2 via Brent: {rootR:F8}");
}

static void TestRungeKutta()
{
    Console.WriteLine("\n--- RungeKutta Tests ---");
    var rk = new RungeKutta<double>((x, y) => x + y, 1e-7);
    double forward = rk.Solve(0.0, 1.0, 1.0, 100);
    Console.WriteLine($"dy/dx=x+y, y(0)=1, y(1) forward: {forward:F6} (Expected ~3.436)");

    double backward = rk.Solve(1.0, forward, 0.0, 100);
    Console.WriteLine($"Reverse integration y(0): {backward:F6} (Expected ~1.0)");
}

static void TestRungeKuttaSystem()
{
    Console.WriteLine("\n--- RungeKuttaSystem Tests ---");
  // Harmonic oscillator: y'' + y = 0 → y1' = y2, y2' = -y1
    var system = new RungeKuttaSystem<double>((x, y, dydx) =>
    {
        dydx[0] = y[1];
        dydx[1] = -y[0];
    });

    double[] y0 = [0.0, 1.0];
    double[] yEnd = system.Solve(0.0, y0, Math.PI / 2, 500);
    Console.WriteLine($"Harmonic oscillator at π/2: y={yEnd[0]:F6}, y'={yEnd[1]:F6} (Expected ~1, ~0)");
}

static void TestDualNumber()
{
    Console.WriteLine("\n--- DualNumber Tests ---");
    var x = new DualNumber<double>(2.0, 1.0);
    var sq = x * x;
    Console.WriteLine($"d/dx(x²) at x=2 via AD: {sq.Deriv:F6} (Expected 4)");

    var sinAd = DualNumber<double>.Sin(new DualNumber<double>(0.0, 1.0));
    Console.WriteLine($"d/dx(sin x) at x=0 via AD: {sinAd.Deriv:F6} (Expected 1)");

    var deriv = new Derivative<double>(v => DualNumber<double>.Exp(v));
    Console.WriteLine($"d/dx(e^x) at x=1 via Derivative+AD: {deriv.CalculateAD(1.0):F6} (Expected {Math.E:F6})");
}

static void TestGradientAndJacobian()
{
    Console.WriteLine("\n--- Gradient / Jacobian Tests ---");
    var scalar = new Derivative<double>(args => args[0] * args[0] + args[1] * args[1], 1e-7);
    double[] point = [2.0, 3.0];
    double[] grad = scalar.Gradient(point);
    Console.WriteLine($"∇(x²+y²) at (2,3): ({grad[0]:F6}, {grad[1]:F6}) (Expected 4, 6)");

    var jac = new Jacobian<double>((p, output) =>
    {
        output[0] = p[0] * p[0];
        output[1] = p[0] * p[1];
    }, outputDim: 2, defaultH: 1e-7);

    var matrix = jac.Calculate(point);
    Console.WriteLine($"Jacobian[0,0]={matrix[0, 0]:F6} (Expected 4), Jacobian[1,0]={matrix[1, 0]:F6} (Expected 3)");
}

static void TestIndefiniteIntegration()
{
    Console.WriteLine("\n--- Indefinite Integration Tests ---");
    var integ = new Integration<double>(x => x * x, 1e-7, lowerBound: 0.0);
    double f1 = integ.IndefiniteIntegrate(1.0, n: 1000);
    Console.WriteLine($"F(1)=∫₀¹ x² dx: {f1:F6} (Expected 0.333333)");

    var F = integ.GetIndefiniteIntegral();
    double f2 = F(2.0);
    Console.WriteLine($"F(2)=∫₀² x² dx: {f2:F6} (Expected 2.666667)");
}

static void TestHessian()
{
    Console.WriteLine("\n--- Hessian Tests ---");
    var hessian = new Hessian<double>(args => args[0] * args[0] + args[1] * args[1], 1e-5);
    double[] point = [2.0, 3.0];
    var H = hessian.Calculate(point);
    Console.WriteLine($"H[0,0]={H[0, 0]:F4}, H[1,1]={H[1, 1]:F4}, H[0,1]={H[0, 1]:F4} (Expected 2, 2, 0)");
}

static void TestAdaptiveRungeKutta()
{
    Console.WriteLine("\n--- AdaptiveRungeKutta Tests ---");
    var ark = new AdaptiveRungeKutta<double>((x, y) => -y);
    double yEnd = ark.Solve(0.0, 1.0, 2.0, 1e-6, 1e-9);
    double exact = Math.Exp(-2.0);
    Console.WriteLine($"dy/dx=-y, y(2): {yEnd:F8}, exact: {exact:F8}");

    var traj = ark.SolveTrajectory(0.0, 1.0, 2.0, 1e-6, 1e-9);
    Console.WriteLine($"Adaptive trajectory: {traj.Count} points, y(2)={traj.Y[^1]:F8}");
}

static void TestFourierFromSamples()
{
    Console.WriteLine("\n--- FourierSeriesFromSamples Tests ---");
    int n = 64;
    float period = (float)(2 * Math.PI);
    var samples = new float[n];
    for (int i = 0; i < n; i++)
        samples[i] = MathF.Sin(2f * MathF.PI * i / n);

    var fs = new FourierSeriesFromSamples(samples, period);
    float b1 = fs.GetSineCoefficient(1);
    float recon = fs.Evaluate(MathF.PI / 2f, order: 3);
    Console.WriteLine($"sin(x) b1={b1:F4} (Expected ~1), f(π/2)≈{recon:F4}");
}

static void TestHighOrderDerivative()
{
    Console.WriteLine("\n--- High-Order Derivative Tests ---");
    var deriv = new Derivative<double>(x => Math.Exp(x), 1e-4);
  var taylor = new TaylorSeries<double>(x => Math.Exp(x), 0.0, 1e-4);
    double d3 = deriv.Calculate(0.0, order: 3);
    double taylor3 = taylor.GetTaylorCoefficient(3);
    Console.WriteLine($"exp'''(0)={d3:F6} (Expected 1), Taylor c3={taylor3:F6} (Expected 1/6≈0.166667)");
}

static void TestGaussLegendre()
{
    Console.WriteLine("\n--- Gauss-Legendre Tests ---");
    var integ = new Integration<double>(1e-7);
    double result = integ.Integrate(0.0, 1.0, 8, x => x * x, method: Integration<double>.Method.GaussLegendre);
    Console.WriteLine($"∫x² dx [0,1] GL-8: {result:F10} (Expected 0.3333333333)");
}

static void TestAdaptiveSimpson()
{
    Console.WriteLine("\n--- Adaptive Simpson Tests ---");
    var integ = new Integration<double>(1e-8);
    double poly = integ.Integrate(0.0, 1.0, 20, x => x * x, method: Integration<double>.Method.AdaptiveSimpson);
    double osc = integ.Integrate(0.0, Math.PI, 24, x => Math.Sin(x), method: Integration<double>.Method.AdaptiveSimpson);
    Console.WriteLine($"∫x² dx [0,1] Adaptive: {poly:F10} (Expected 0.3333333333)");
    Console.WriteLine($"∫sin(x) dx [0,π]: {osc:F10} (Expected 2.0)");
}

static void TestNewtonRaphsonSystem()
{
    Console.WriteLine("\n--- NewtonRaphsonSystem Tests ---");
    // x² + y² = 4, x - y = 0 → (√2, √2)
    var solver = new NewtonRaphsonSystem<double>((p, r) =>
    {
        r[0] = p[0] * p[0] + p[1] * p[1] - 4.0;
        r[1] = p[0] - p[1];
    }, dimension: 2, defaultH: 1e-7);

    double[] root = solver.Solve([1.0, 0.5]);
    Console.WriteLine($"Root: ({root[0]:F8}, {root[1]:F8}) (Expected √2, √2)");
}

static void TestAdaptiveRungeKuttaSystem()
{
    Console.WriteLine("\n--- AdaptiveRungeKuttaSystem Tests ---");
    var ark = new AdaptiveRungeKuttaSystem<double>((x, y, dydx) =>
    {
        dydx[0] = y[1];
        dydx[1] = -y[0];
    });

    double[] yEnd = ark.Solve(0.0, [0.0, 1.0], Math.PI / 2, 1e-4, 1e-6, initialStep: 0.05);
    Console.WriteLine($"Harmonic oscillator (adaptive) at π/2: y={yEnd[0]:F6}, y'={yEnd[1]:F6}");

    var traj = ark.SolveTrajectory(0.0, [0.0, 1.0], Math.PI, 1e-4, 1e-6, initialStep: 0.05);
    Console.WriteLine($"Adaptive system trajectory: {traj.Count} points, y(π)={traj.States[0][^1]:F6}");
}

static void TestOdeTrajectory()
{
    Console.WriteLine("\n--- ODE Trajectory / Event Tests ---");
    var rk = new RungeKutta<double>((x, y) => x + y, 1e-7);
    var traj = rk.SolveTrajectory(0.0, 1.0, 1.0, 50);
    Console.WriteLine($"Trajectory points: {traj.Count}, y(1)={traj.Y[^1]:F6}");

    var eventTraj = rk.SolveTrajectory(0.0, 1.0, 2.0, 200, odeEvent: (x, y) => y > 5.0);
    Console.WriteLine($"Event stop at x={eventTraj.X[^1]:F4}, y={eventTraj.Y[^1]:F4}");
}

static void TestOptimization()
{
    Console.WriteLine("\n--- Optimization Tests ---");
    var bfgs = new BfgsOptimizer<double>((p) => (p[0] - 2) * (p[0] - 2) + (p[1] + 1) * (p[1] + 1), 1e-5);
    double[] min = bfgs.Minimize([0.0, 0.0]);
    Console.WriteLine($"BFGS min at ({min[0]:F4}, {min[1]:F4}) (Expected 2, -1)");

    var lm = new LevenbergMarquardt<double>((p, r) =>
    {
        r[0] = p[0] - 1.0;
        r[1] = p[1] - 2.0;
    }, dimension: 2, residualCount: 2, defaultH: 1e-6);
    double[] fit = lm.Solve([0.0, 0.0]);
    Console.WriteLine($"LM fit ({fit[0]:F4}, {fit[1]:F4}) (Expected 1, 2)");
}

static void TestLBfgs()
{
    Console.WriteLine("\n--- L-BFGS Tests ---");
    var lbfgs = new LBfgsOptimizer<double>((p) => (p[0] - 2) * (p[0] - 2) + (p[1] + 1) * (p[1] + 1), 1e-5);
    double[] min = lbfgs.Minimize([0.0, 0.0]);
    Console.WriteLine($"L-BFGS min at ({min[0]:F4}, {min[1]:F4}) (Expected 2, -1)");
}

static void TestHyperDualAndPade()
{
    Console.WriteLine("\n--- HyperDual / Padé Tests ---");
    double d2 = Derivative<double>.SecondDerivativeAD(0.0, x => HyperDualNumber<double>.Exp(x));
    Console.WriteLine($"exp''(0) via HyperDual: {d2:F6} (Expected 1)");

    var taylor = new TaylorSeries<double>(x => Math.Exp(x), 0.0, 1e-4);
    var coefs = new double[5];
    for (int i = 0; i < coefs.Length; i++)
        coefs[i] = taylor.GetTaylorCoefficient(i);
    var pade = new PadeApproximant<double>(coefs, m: 2, n: 2);
    double approx = pade.Evaluate(0.5);
    Console.WriteLine($"exp(0.5) Padé[2/2]: {approx:F6}, exact: {Math.Exp(0.5):F6}");
}

static void TestChebyshevAndDoubleIntegral()
{
    Console.WriteLine("\n--- Chebyshev / Double Integral Tests ---");
    var cheb = ChebyshevSeries<double>.FromFunction(x => x * x, order: 4, a: -1.0, b: 1.0, sampleCount: 32);
    double c0 = cheb.Evaluate(-1.0);
    double c1 = cheb.Evaluate(1.0);
    Console.WriteLine($"x² on [-1,1]: cheb(-1)={c0:F4}, cheb(1)={c1:F4} (Expected 1, 1)");

    var integ = new Integration<double>(1e-8);
    double dbl = integ.IntegrateDouble(0, 1, 0, 1, 8, (x, y) => x * y);
    Console.WriteLine($"∬xy dxdy [0,1]²: {dbl:F6} (Expected 0.25)");

    double improper = integ.IntegrateToInfinity(1.0, x => 1.0 / (x * x));
    Console.WriteLine($"∫₁^∞ 1/x² dx: {improper:F6} (Expected 1)");
}

static void TestImplicitEulerAndBvp()
{
    Console.WriteLine("\n--- Implicit Euler / BVP Tests ---");
    var stiffSolver = new ImplicitEuler<double>((x, y) => -1000.0 * (y - Math.Cos(x)), 1e-6);
    double yStiff = stiffSolver.Solve(0.0, 0.0, 0.1, 20);
    Console.WriteLine($"Stiff decay y(0.1): {yStiff:F6}");

    var bvp = new ShootingBvpSolver<double>((x, y, dydx) =>
    {
        dydx[0] = y[1];
        dydx[1] = -y[0];
    });
    double[] end = bvp.Solve(0.0, Math.PI, 0.0, 0.0, initialSlopeGuess: 1.0, steps: 200);
    Console.WriteLine($"y''+y=0, y(π)={end[0]:F6} (Expected ~0)");
}

static double Factorial(int n)
{
    if (n <= 1) return 1.0;
    double result = 1.0;
    for (int i = 2; i <= n; i++) result *= i;
    return result;
}
