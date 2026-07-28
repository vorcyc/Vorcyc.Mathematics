using Vorcyc.Mathematics;
using Vorcyc.Mathematics.MachineLearning.CurveFitting;

namespace ML_module_test;

/// <summary>
/// CurveFitter.Sinusoidal 正确性回归：无 NaN、预测贴近真值、参数可辨识（规范化后）。
/// </summary>
public static class CurveFitting_Sinusoidal_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing CurveFitter.Sinusoidal...");

        AssertCase("perfect-pi", 2.0, Math.PI, 0.0, 1.0, 40, 0.0, 0.1);
        AssertCase("phase-pi2", 1.5, 2.0, Math.PI / 2, 0.5, 50, 0.0, 0.15);
        AssertCase("phase-wrap", 1.2, 3.0, 2.0, -1.0, 60, -1.0, 0.1);
        AssertCase("multi-5hz", 1.0, 2 * Math.PI * 5, 0.3, 0.0, 256, 0.0, 1.0 / 256);
        AssertCase("sample-index", 1.0, 2 * Math.PI * 5 / 256.0, 0.0, 0.0, 256, 0.0, 1.0);
        AssertCase("neg-amp-input", -2.5, 1.7, 0.4, 3.0, 80, 0.0, 0.08);
        AssertCase("half-period", 1.0, Math.PI, 0.0, 0.0, 50, 0.0, 0.02);
        AssertCase("noisy", 1.5, 2.0, 0.5, 2.0, 120, 0.0, 0.1, noise: 0.1, seed: 42);
        AssertCase("parallel-ctx", 1.0, 2 * Math.PI * 5, 0.0, 0.0, 256, 0.0, 1.0 / 256,
            context: ComputingContext.Parallel);

        AssertFloatVssLike();
        AssertNormalEqualsParallel();

        if (_failures != 0)
            throw new InvalidOperationException($"CurveFitter.Sinusoidal: {_failures} assertion(s) failed.");

        Console.WriteLine("CurveFitter.Sinusoidal: PASS");
    }

    static void AssertFloatVssLike()
    {
        int n = 512;
        float[] x = new float[n];
        float[] y = new float[n];
        float freq = 3f;
        for (int i = 0; i < n; i++)
        {
            x[i] = i;
            y[i] = MathF.Sin(2f * MathF.PI * freq * i / n) + 0.2f;
        }

        var r = CurveFitter<float>.Sinusoidal(x, y, 500);
        float maxErr = 0;
        for (int i = 0; i < n; i++)
        {
            float clean = MathF.Sin(2f * MathF.PI * freq * i / n) + 0.2f;
            maxErr = MathF.Max(maxErr, MathF.Abs(r.Predict(x[i]) - clean));
        }

        Expect("float-vss finite", r.Parameters.All(float.IsFinite) && float.IsFinite(r.MeanSquaredError));
        Expect("float-vss maxErr", maxErr < 1e-4f, $"maxErr={maxErr}");
        Expect("float-vss A>0 B>0", r.Parameters[0] > 0 && r.Parameters[1] > 0);
        Expect("float-vss |B|", MathF.Abs(r.Parameters[1] - 2f * MathF.PI * freq / n) < 1e-3f,
            $"B={r.Parameters[1]}");
    }

    static void AssertNormalEqualsParallel()
    {
        int n = 400;
        double[] x = new double[n];
        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = i * 0.05;
            y[i] = 1.1 * Math.Sin(1.3 * x[i] + 0.4) + 0.2;
        }

        var a = CurveFitter<double>.Sinusoidal(x, y, 300, ComputingContext.Normal);
        var b = CurveFitter<double>.Sinusoidal(x, y, 300, ComputingContext.Parallel);
        double dmax = 0;
        for (int i = 0; i < 4; i++)
            dmax = Math.Max(dmax, Math.Abs(a.Parameters[i] - b.Parameters[i]));
        Expect("Normal==Parallel", dmax < 1e-12, $"dmax={dmax:E3}");
    }

    static void AssertCase(
        string name, double A, double B, double C, double D,
        int n, double x0, double dx,
        double noise = 0, int seed = 1, ComputingContext? context = null)
    {
        var rng = new Random(seed);
        double[] x = new double[n];
        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = x0 + i * dx;
            y[i] = A * Math.Sin(B * x[i] + C) + D;
            if (noise > 0)
                y[i] += (rng.NextDouble() * 2 - 1) * noise;
        }

        var r = CurveFitter<double>.Sinusoidal(x, y, 500, context);
        Expect($"{name} finite", r.Parameters.All(double.IsFinite) && double.IsFinite(r.MeanSquaredError));

        double maxErr = 0, mse = 0;
        for (int i = 0; i < n; i++)
        {
            double clean = A * Math.Sin(B * x[i] + C) + D;
            double e = r.Predict(x[i]) - clean;
            maxErr = Math.Max(maxErr, Math.Abs(e));
            mse += e * e;
        }
        mse /= n;

        double mseTol = noise > 0 ? Math.Max(1e-4, noise * noise) : 1e-10;
        double maxTol = noise > 0 ? Math.Max(0.05, noise * 2.5) : 1e-5;
        Expect($"{name} predMSE", mse <= mseTol, $"mse={mse:E3}");
        Expect($"{name} maxErr", maxErr <= maxTol, $"max={maxErr:E3}");

        // 规范化后：A≥0、B≥0；与真值比较时把真值也规范化
        Canonical(ref A, ref B, ref C);
        double tolA = 0.05 + 0.02 * Math.Abs(A) + noise;
        double tolB = 0.05 + 0.02 * Math.Abs(B) + noise * 0.5;
        double tolC = noise > 0 ? 0.35 : 0.12;
        double tolD = 0.05 + 0.02 * Math.Abs(D) + noise;

        Expect($"{name} A", Math.Abs(r.Parameters[0] - A) <= tolA,
            $"got={r.Parameters[0]:G6} expected={A:G6}");
        Expect($"{name} B", Math.Abs(r.Parameters[1] - B) <= tolB,
            $"got={r.Parameters[1]:G6} expected={B:G6}");
        Expect($"{name} C", Math.Abs(WrapPi(r.Parameters[2] - C)) <= tolC,
            $"got={r.Parameters[2]:G6} expected={C:G6}");
        Expect($"{name} D", Math.Abs(r.Parameters[3] - D) <= tolD,
            $"got={r.Parameters[3]:G6} expected={D:G6}");
        Expect($"{name} canonical signs", r.Parameters[0] >= 0 && r.Parameters[1] >= 0);
    }

    static void Canonical(ref double a, ref double b, ref double c)
    {
        if (b < 0)
        {
            a = -a;
            b = -b;
            c = -c;
        }
        if (a < 0)
        {
            a = -a;
            c += Math.PI;
        }
        c = WrapPi(c);
    }

    static double WrapPi(double c)
    {
        c %= 2 * Math.PI;
        if (c > Math.PI) c -= 2 * Math.PI;
        if (c <= -Math.PI) c += 2 * Math.PI;
        return c;
    }

    static void Expect(string name, bool ok, string detail = "")
    {
        if (ok)
        {
            Console.WriteLine($"  PASS  {name}");
            return;
        }
        _failures++;
        Console.WriteLine($"  FAIL  {name}{(detail.Length > 0 ? " | " + detail : "")}");
    }
}
