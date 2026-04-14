using BenchmarkDotNet.Attributes;
using Vorcyc.Mathematics.Calculus;
using Vorcyc.Mathematics.Calculus.NumericalMethods;
using Vorcyc.Mathematics.Calculus.Optimization;
using Vorcyc.Mathematics.Calculus.Series;
using Vorcyc.Mathematics.LinearAlgebra;

namespace basic_benchmark;

[MemoryDiagnoser]
public class CalculusBenchmark
{
    private double[] _x0 = null!;
    private Derivative<double> _deriv = null!;
    private BfgsOptimizer<double> _bfgs = null!;
    private LBfgsOptimizer<double> _lbfgs = null!;
    private RungeKuttaSystem<double> _rkSystem = null!;
    private NewtonRaphsonSystem<double> _newtonSystem = null!;
    private LevenbergMarquardt<double> _lm = null!;
    private Integration<double> _integration = null!;
    private Integration<double> _boundIntegration = null!;
    private SingleVariableFunction<double> _indefIntegral = null!;
    private Jacobian<double> _jacobian = null!;
    private Hessian<double> _hessian = null!;
    private ShootingBvpSolver<double> _bvp = null!;
    private TaylorSeries<double> _taylor = null!;
    private Limits<double> _limits = null!;
    private FourierSeries<double> _fourier = null!;
    private Brent<double> _brent = null!;
    private NewtonRaphson<double> _newton = null!;
    private Bisection<double> _bisection = null!;
    private AdaptiveRungeKutta<double> _adaptiveRk = null!;
    private ChebyshevFitWorkspace<double> _chebWorkspace = null!;
    private ChebyshevSeries<double> _chebSeries = null!;

    [Params(4, 16)]
    public int Dimension { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _x0 = new double[Dimension];
        for (int i = 0; i < Dimension; i++)
            _x0[i] = 0.5 * i;

        _deriv = new Derivative<double>(Quadratic, 1e-5);
        _bfgs = new BfgsOptimizer<double>(ShiftedQuadratic, 1e-5);
        _lbfgs = new LBfgsOptimizer<double>(ShiftedQuadratic, 1e-5);
        _rkSystem = new RungeKuttaSystem<double>((x, y, dydx) =>
        {
            dydx[0] = y[1];
            dydx[1] = -y[0];
        });
        _newtonSystem = new NewtonRaphsonSystem<double>((p, r) =>
        {
            r[0] = p[0] * p[0] + p[1] * p[1] - 4.0;
            r[1] = p[0] - p[1];
        }, dimension: 2, defaultH: 1e-7);
        _lm = new LevenbergMarquardt<double>((p, r) =>
        {
            r[0] = p[0] - 1.0;
            r[1] = p[1] - 2.0;
        }, dimension: 2, residualCount: 2, defaultH: 1e-6);
        _integration = new Integration<double>(1e-8);
        _boundIntegration = new Integration<double>(Math.Sin, 1e-8, lowerBound: 0.0);
        _indefIntegral = _boundIntegration.GetIndefiniteIntegral(n: 200);
        _jacobian = new Jacobian<double>((p, r) =>
        {
            r[0] = p[0] * p[0] + p[1];
            r[1] = p[0] * p[1];
        }, outputDim: 2, defaultH: 1e-7);
        _hessian = new Hessian<double>((p) => p[0] * p[0] + p[1] * p[1], 1e-5);
        _bvp = new ShootingBvpSolver<double>((x, y, dydx) =>
        {
            dydx[0] = y[1];
            dydx[1] = -y[0];
        });
        _taylor = new TaylorSeries<double>(Math.Exp, 0.0, 1e-5);
        _limits = new Limits<double>(x => x * x, 1e-2);
        _fourier = new FourierSeries<double>(x => x - Math.Floor(x), 1.0, 1e-7);
        _brent = new Brent<double>(x => x * x - 2.0);
        _newton = new NewtonRaphson<double>(x => x * x - 2.0, 1e-7);
        _bisection = new Bisection<double>(x => x * x - 2.0);
        _adaptiveRk = new AdaptiveRungeKutta<double>((x, y) => -y);
        _chebWorkspace = new ChebyshevFitWorkspace<double>();
        _chebSeries = ChebyshevSeries<double>.FromFunction(
            x => x * x, order: 8, a: -1.0, b: 1.0, sampleCount: 64, _chebWorkspace);
    }

    private static double Quadratic(ReadOnlySpan<double> args)
    {
        double sum = 0;
        for (int i = 0; i < args.Length; i++)
            sum += args[i] * args[i];
        return sum;
    }

    private double ShiftedQuadratic(ReadOnlySpan<double> args)
    {
        double sum = 0;
        for (int i = 0; i < args.Length; i++)
            sum += (args[i] - i) * (args[i] - i);
        return sum;
    }

    [Benchmark]
    public double[] Gradient() => _deriv.Gradient(_x0);

    [Benchmark]
    public double[] BfgsMinimize() => _bfgs.Minimize((double[])_x0.Clone(), maxIterations: 30);

    [Benchmark]
    public double[] LBfgsMinimize() => _lbfgs.Minimize((double[])_x0.Clone(), maxIterations: 30);

    [Benchmark]
    public double[] NewtonSystemSolve() => _newtonSystem.Solve([1.0, 0.5]);

    [Benchmark]
    public double[] LmSolve() => _lm.Solve([0.0, 0.0]);

    [Benchmark]
    public double AdaptiveSimpson() =>
        _integration.Integrate(0.0, Math.PI, 24, Math.Sin, method: Integration<double>.Method.AdaptiveSimpson);

    [Benchmark]
    public double[] RkSystemSolve()
    {
        double[] y0 = new double[2];
        y0[1] = 1.0;
        return _rkSystem.Solve(0, y0, Math.PI, 500);
    }

    [Benchmark]
    public double[] BvpSolve() =>
        _bvp.Solve(0.0, Math.PI, 0.0, 0.0, initialSlopeGuess: 1.0, steps: 200);

    [Benchmark]
    public double TaylorExp() => _taylor.Calculate(0.5, order: 8);

    [Benchmark]
    public double LimitXSquared() => _limits.CalculateLimit(0.0, Limits<double>.Direction.Both, tolerance: 1e-8);

    [Benchmark]
    public OdeTrajectory<double> AdaptiveRkTrajectory() =>
        _adaptiveRk.SolveTrajectory(0.0, 1.0, 5.0, 1e-6, 1e-9);

    [Benchmark]
    public double FourierCoefficients() =>
        _fourier.Calculate(0.25, order: 8, segments: 200);

    [Benchmark]
    public double BrentSqrt2() => _brent.Solve(1.0, 2.0);

    [Benchmark]
    public double NewtonSqrt2() => _newton.Solve(1.5);

    [Benchmark]
    public double BisectionSqrt2() => _bisection.Solve(1.0, 2.0);

    [Benchmark]
    public double SimpsonSinPi() =>
        _integration.Integrate(0.0, Math.PI, 200, Math.Sin, method: Integration<double>.Method.Simpson);

    [Benchmark]
    public double AdaptiveRkDecay() =>
        _adaptiveRk.Solve(0.0, 1.0, 5.0, 1e-6, 1e-9);

    [Benchmark]
    public double IndefiniteIntegralSweep()
    {
        double sum = 0;
        for (int i = 1; i <= 50; i++)
            sum += _indefIntegral(i * 0.1);
        return sum;
    }

    [Benchmark]
    public Matrix<double> JacobianCentral() =>
        _jacobian.Calculate(_x0);

    [Benchmark]
    public Matrix<double> HessianCentral() =>
        _hessian.Calculate(_x0);

    [Benchmark]
    public double ChebyshevEvaluate() => _chebSeries.Evaluate(0.25);

    [Benchmark]
    public ChebyshevSeries<double> ChebyshevFit() =>
        ChebyshevSeries<double>.FromFunction(
            x => x * x, order: 8, a: -1.0, b: 1.0, sampleCount: 64, _chebWorkspace);

    [Benchmark]
    public double IntegrateDoubleXY() =>
        _integration.IntegrateDouble(0.0, 1.0, 0.0, 1.0, 8, (x, y) => x * y);
}
