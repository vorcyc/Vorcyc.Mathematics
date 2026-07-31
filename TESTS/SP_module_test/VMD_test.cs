using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

namespace SP_module_test;

public static class VMD_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing VariationalModeDecomposition...");

        AssertTwoToneCenters();
        AssertReconstruction();
        AssertFacade();

        if (_failures != 0)
            throw new InvalidOperationException($"VMD: {_failures} assertion(s) failed.");

        Console.WriteLine("VariationalModeDecomposition: PASS");
    }

    static void AssertTwoToneCenters()
    {
        int n = 1024;
        float fs = 1024f;
        var x = new double[n];
        for (int i = 0; i < n; i++)
        {
            double t = i / (double)fs;
            x[i] = Math.Cos(2 * Math.PI * 5 * t) + 0.5 * Math.Cos(2 * Math.PI * 37 * t);
        }

        var r = VariationalModeDecomposition.Decompose<double>(x, new VmdOptions
        {
            ModeCount = 2,
            Alpha = 2000,
            Tau = 0,
            Tolerance = 1e-6,
            MaxIterations = 200,
            OmegaInit = 1,
            SamplingRate = fs,
        });

        Expect("modes=2", r.ModeCount == 2);
        Expect("finite modes", r.Modes.All(m => m.All(double.IsFinite)));
        Expect("iters>0", r.Iterations > 0);

        var freqs = r.CenterFrequenciesHz.OrderBy(f => f).ToArray();
        Expect("low ~5Hz", Math.Abs(freqs[0] - 5) < 2.5, $"f0={freqs[0]}");
        Expect("high ~37Hz", Math.Abs(freqs[1] - 37) < 8, $"f1={freqs[1]}");
    }

    static void AssertReconstruction()
    {
        int n = 512;
        var x = new float[n];
        for (int i = 0; i < n; i++)
            x[i] = MathF.Sin(2f * MathF.PI * 3f * i / n) + 0.4f * MathF.Sin(2f * MathF.PI * 19f * i / n);

        var r = VariationalModeDecomposition.Decompose<float>(x, new VmdOptions
        {
            ModeCount = 2,
            Alpha = 2000,
            Tau = 0,
            MaxIterations = 150,
            SamplingRate = n,
        });

        float maxErr = 0;
        for (int i = 0; i < n; i++)
        {
            float sum = r.Residual[i];
            foreach (var m in r.Modes) sum += m[i];
            maxErr = Math.Max(maxErr, Math.Abs(sum - x[i]));
        }
        // τ=0 → modes should nearly reconstruct; allow moderate error from padding/ADMM
        Expect("reconstruct", maxErr < 0.35f, $"maxErr={maxErr}");
    }

    static void AssertFacade()
    {
        var x = new float[256];
        for (int i = 0; i < x.Length; i++)
            x[i] = MathF.Sin(2f * MathF.PI * 4f * i / x.Length);

        var r = ModeDecomposer.Vmd<float>(x, new VmdOptions { ModeCount = 1, MaxIterations = 80, SamplingRate = 256 });
        Expect("facade", r.ModeCount == 1 && r.Modes[0].Length == 256);
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL: {name}" + (detail is null ? "" : $" ({detail})"));
    }
}
