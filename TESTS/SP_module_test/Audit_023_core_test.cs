using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Calculus.NumericalMethods;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.MachineLearning.Distances;
using Vorcyc.Mathematics.Statistics;

namespace SP_module_test;

/// <summary>
/// 0.10.23 audit regressions for the core / linear algebra / statistics / calculus / distance fixes
/// (kept here because the core / Calculus test directories are not tracked by git).
/// </summary>
internal static class Audit_023_core_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing 0.10.23 audit regressions (core)...");

        AssertHcfLcmRatio();
        AssertInvSqrt();
        AssertFindNthAndInterpolation();
        AssertPercentileValidation();
        AssertMatrices();
        AssertOdeStepDirection();
        AssertWeightedDistances();

        if (_failures != 0)
            throw new InvalidOperationException($"Audit 0.10.23 (core): {_failures} assertion(s) failed.");

        Console.WriteLine("Audit_023_core_test: PASS");
    }

    static void AssertHcfLcmRatio()
    {
        // The old subtraction-based recursion overflowed the stack for 0, negatives and large ratios.
        Expect("Hcf(12, 18) == 6", VMath.Hcf(12, 18) == 6);
        Expect("Hcf(0, 5) == 5", VMath.Hcf(0, 5) == 5);
        Expect("Hcf(5, 0) == 5", VMath.Hcf(5, 0) == 5);
        Expect("Hcf(0, 0) == 0", VMath.Hcf(0, 0) == 0);
        Expect("Hcf(-12, 18) == 6", VMath.Hcf(-12, 18) == 6);
        Expect("Hcf(1000000, 1) == 1", VMath.Hcf(1_000_000, 1) == 1);

        Expect("Lcm(4, 6) == 12", VMath.Lcm(4, 6) == 12);
        Expect("Lcm(0, 5) == 0", VMath.Lcm(0, 5) == 0);
        Expect("Lcm(-4, 6) == 12", VMath.Lcm(-4, 6) == 12);
        Expect("Lcm(100000, 100000) == 100000 (a * b overflows int)", VMath.Lcm(100_000, 100_000) == 100_000);

        Expect("ratio 6/-8 == -3/4", VMath.SimplestIntegerRatioOfFraction(6, -8) == (-3, 4));
        Expect("ratio 0/5 == 0/1", VMath.SimplestIntegerRatioOfFraction(0, 5) == (0, 1));
        ExpectThrows<DivideByZeroException>("ratio with a zero denominator", () => VMath.SimplestIntegerRatioOfFraction(1, 0));
    }

    static void AssertInvSqrt()
    {
        double worst = 0;
        foreach (var x in new[] { 0.25, 1.0, 2.0, 10.0, 1234.5 })
            worst = Math.Max(worst, Math.Abs(VMath.InvSqrt(x) * Math.Sqrt(x) - 1));
        Expect("InvSqrt(double) is within 0.5% of 1/sqrt(x)", worst < 5e-3, $"relErr={worst:E2}");
    }

    static void AssertFindNthAndInterpolation()
    {
        var a = new float[] { 5, 3, 9, 1, 7 };
        Expect("FindNth(array) returns the median", VMath.FindNth((float[])a.Clone(), 2, 0, a.Length - 1) == 5f);
        ExpectThrows<ArgumentOutOfRangeException>("FindNth(array) with n outside [start, end]", () => VMath.FindNth((float[])a.Clone(), 7, 0, a.Length - 1));

        var d = new double[] { 5, 3, 9, 1, 7 };
        Expect("FindNth(span) returns the median", VMath.FindNth<double>(d.AsSpan(), 2) == 5.0);
        ExpectThrows<ArgumentOutOfRangeException>("FindNth(span) with n outside the span", () => VMath.FindNth<double>(new double[] { 1, 2, 3 }.AsSpan(), 3));

        ExpectThrows<ArgumentException>("InterpolateLinear with a single knot",
            () => VMath.InterpolateLinear([1f], [1f], [1f], new float[1]));
    }

    static void AssertPercentileValidation()
    {
        ExpectThrows<ArgumentException>("Percentile of an empty sequence", () => new double[0].AsSpan().Percentile(0.5));
        ExpectThrows<ArgumentOutOfRangeException>("Percentile(1.5)", () => new double[] { 1, 2, 3 }.AsSpan().Percentile(1.5));
        Expect("Percentile(0.5) of 1,2,3 is 2", Math.Abs(new double[] { 1, 2, 3 }.AsSpan().Percentile(0.5) - 2) < 1e-12);
    }

    static void AssertMatrices()
    {
        // 1x1 float matrix: inverse of [4] is [0.25] (it used to be a zero matrix).
        var one = new MatrixFp32(1, 1);
        one[0, 0] = 4f;
        Expect("MatrixFp32 1x1 Inverse()", Math.Abs(one.Inverse()[0, 0] - 0.25f) < 1e-6f);

        // Wide matrix QR: A = Q * R with Q (m x m) and R (m x n).
        var wide = new MatrixFp32(2, 3);
        float[,] v = { { 1, 2, 3 }, { 4, 5, 6 } };
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 3; j++)
                wide[i, j] = v[i, j];
        wide.QRDecomposition(out var q, out var r);
        float worst = 0;
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 3; j++)
            {
                float s = 0;
                for (int k = 0; k < 2; k++)
                    s += q[i, k] * r[k, j];
                worst = Math.Max(worst, Math.Abs(s - v[i, j]));
            }
        Expect("MatrixFp32 wide QR reconstructs A", worst < 1e-4f, $"maxΔ={worst:E2}");

        var wideG = new Matrix<double>(2, 3);
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 3; j++)
                wideG[i, j] = v[i, j];
        wideG.QRDecomposition(out var qg, out var rg);
        double worstG = 0;
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 3; j++)
            {
                double s = 0;
                for (int k = 0; k < 2; k++)
                    s += qg[i, k] * rg[k, j];
                worstG = Math.Max(worstG, Math.Abs(s - v[i, j]));
            }
        Expect("Matrix<double> wide QR reconstructs A", worstG < 1e-9, $"maxΔ={worstG:E2}");
    }

    static void AssertOdeStepDirection()
    {
        // y' = y, y(1) = e  =>  y(0) = 1. Integrating backwards with a positive step used to loop forever.
        var rk = new RungeKutta<double>((x, y) => y, 0.01);
        Expect("RungeKutta backwards (no h)", Math.Abs(rk.Solve(1.0, Math.E, 0.0, steps: 200) - 1.0) < 1e-6);
        Expect("RungeKutta backwards (positive h)", Math.Abs(rk.Solve(1.0, Math.E, 0.0, steps: 200, h: 0.005) - 1.0) < 1e-6);
        Expect("RungeKutta forwards (negative h)", Math.Abs(rk.Solve(0.0, 1.0, 1.0, steps: 200, h: -0.005) - Math.E) < 1e-6);
        ExpectThrows<ArgumentException>("RungeKutta with h == 0", () => rk.Solve(0.0, 1.0, 1.0, steps: 10, h: 0.0));

        var sys = new RungeKuttaSystem<double>((double x, ReadOnlySpan<double> y, Span<double> dy) => { dy[0] = y[0]; });
        var back = sys.Solve(1.0, [Math.E], 0.0, steps: 200, h: 0.005);
        Expect("RungeKuttaSystem backwards (positive h)", Math.Abs(back[0] - 1.0) < 1e-6);

        var ie = new ImplicitEuler<double>((x, y) => -y, 1e-6);
        ExpectThrows<ArgumentException>("ImplicitEuler with h == 0", () => ie.Solve(0.0, 1.0, 1.0, steps: 10, h: 0.0));
    }

    static void AssertWeightedDistances()
    {
        // Weights never assigned: default to all ones as documented (used to be a NullReferenceException).
        Expect("WeightedEuclidean default weights", Math.Abs(WeightedEuclidean<double>.Distance([3.0, 4.0], [0.0, 0.0]) - 5.0) < 1e-12);
        Expect("WeightedSquareEuclidean default weights", Math.Abs(WeightedSquareEuclidean<double>.Distance([3.0, 4.0], [0.0, 0.0]) - 25.0) < 1e-12);

        WeightedEuclidean<double>.Weights = [1.0];
        ExpectThrows<ArgumentException>("WeightedEuclidean with too few weights", () => WeightedEuclidean<double>.Distance([3.0, 4.0], [0.0, 0.0]));
        WeightedEuclidean<double>.SetDimensions(2);
        Expect("WeightedEuclidean after SetDimensions", Math.Abs(WeightedEuclidean<double>.Distance([3.0, 4.0], [0.0, 0.0]) - 5.0) < 1e-12);
    }

    static void ExpectThrows<TException>(string name, Action action) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception e)
        {
            Expect(name, false, $"threw {e.GetType().Name} instead of {typeof(TException).Name}");
            return;
        }
        Expect(name, false, "did not throw");
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }
}
