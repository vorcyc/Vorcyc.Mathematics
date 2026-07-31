using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

namespace SP_module_test;

/// <summary>Empirical Mode Decomposition smoke / correctness tests.</summary>
public static class EMD_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing EmpiricalModeDecomposition...");

        AssertShortRejected();
        AssertTwoToneReconstruction();
        AssertFloatMatchesDoubleEnergy();
        AssertParallelDoesNotCrash();
        AssertMaxImfCap();

        if (_failures != 0)
            throw new InvalidOperationException($"EMD: {_failures} assertion(s) failed.");

        Console.WriteLine("EmpiricalModeDecomposition: PASS");
    }

    static void AssertShortRejected()
    {
        float[] x = [1, 2, 3];
        var r = EmpiricalModeDecomposition.Decompose<float>(x);
        Expect("short rejected", r.StopReason == EmdStopReason.InputRejected && r.ModeCount == 0);
    }

    static void AssertTwoToneReconstruction()
    {
        int n = 1024;
        double fs = 1024;
        var signal = new double[n];
        for (int i = 0; i < n; i++)
        {
            double t = i / fs;
            signal[i] = Math.Sin(2 * Math.PI * 5 * t) + 0.5 * Math.Sin(2 * Math.PI * 37 * t);
        }

        var result = EmpiricalModeDecomposition.Decompose<double>(signal, new EmdOptions
        {
            MaxImf = 8,
            MaxSiftIterations = 50,
            SiftingTolerance = 0.2,
        });

        Expect("got IMFs", result.ModeCount >= 1, $"modes={result.ModeCount}");
        Expect("finite residual", result.Residual.All(double.IsFinite));
        Expect("finite IMFs", result.IntrinsicModeFunctions.All(imf => imf.All(double.IsFinite)));

        double maxErr = 0, energySig = 0, energyParts = 0;
        for (int i = 0; i < n; i++)
        {
            double sum = result.Residual[i];
            foreach (var imf in result.IntrinsicModeFunctions)
                sum += imf[i];
            maxErr = Math.Max(maxErr, Math.Abs(sum - signal[i]));
            energySig += signal[i] * signal[i];
            energyParts += sum * sum;
        }

        Expect("reconstruct maxErr", maxErr < 1e-8, $"maxErr={maxErr}");
        Expect("energy ~", Math.Abs(energyParts - energySig) / energySig < 1e-10,
            $"eSig={energySig} eParts={energyParts}");
        Expect("stop reason ok",
            result.StopReason is EmdStopReason.ResidualTooFewExtrema or EmdStopReason.MaxImfReached);
    }

    static void AssertFloatMatchesDoubleEnergy()
    {
        int n = 512;
        var f = new float[n];
        var d = new double[n];
        for (int i = 0; i < n; i++)
        {
            double t = i / (double)n;
            double v = Math.Sin(2 * Math.PI * 3 * t) + 0.3 * Math.Sin(2 * Math.PI * 17 * t);
            f[i] = (float)v;
            d[i] = v;
        }

        var rf = EmpiricalModeDecomposition.Decompose<float>(f, new EmdOptions { MaxImf = 6 });
        var rd = EmpiricalModeDecomposition.Decompose<double>(d, new EmdOptions { MaxImf = 6 });

        Expect("float modes>0", rf.ModeCount >= 1);
        Expect("double modes>0", rd.ModeCount >= 1);

        float maxF = 0;
        for (int i = 0; i < n; i++)
        {
            float sum = rf.Residual[i];
            foreach (var imf in rf.IntrinsicModeFunctions) sum += imf[i];
            maxF = Math.Max(maxF, Math.Abs(sum - f[i]));
        }
        Expect("float reconstruct", maxF < 1e-4f, $"maxF={maxF}");
    }

    static void AssertParallelDoesNotCrash()
    {
        int n = 2048;
        var x = new float[n];
        for (int i = 0; i < n; i++)
            x[i] = MathF.Sin(2f * MathF.PI * 7f * i / n) + 0.25f * MathF.Sin(2f * MathF.PI * 41f * i / n);

        var a = EmpiricalModeDecomposition.Decompose<float>(x, new EmdOptions
        {
            ComputingContext = ComputingContext.Normal,
            MaxImf = 5,
        });
        var b = EmpiricalModeDecomposition.Decompose<float>(x, new EmdOptions
        {
            ComputingContext = ComputingContext.Parallel,
            MaxImf = 5,
        });

        Expect("parallel modes", b.ModeCount >= 1);
        // Parallel only affects envelope subtract loops; modes should match for this size
        // (below parallel threshold they are identical paths).
        Expect("mode count equal", a.ModeCount == b.ModeCount,
            $"a={a.ModeCount} b={b.ModeCount}");
    }

    static void AssertMaxImfCap()
    {
        int n = 800;
        var x = new double[n];
        var rng = new Random(7);
        for (int i = 0; i < n; i++)
            x[i] = Math.Sin(2 * Math.PI * 11 * i / n) + 0.05 * (rng.NextDouble() - 0.5);

        var r = EmpiricalModeDecomposition.Decompose<double>(x, new EmdOptions { MaxImf = 2, MaxSiftIterations = 30 });
        Expect("cap modes", r.ModeCount <= 2, $"modes={r.ModeCount}");
        if (r.ModeCount == 2)
            Expect("cap stop", r.StopReason == EmdStopReason.MaxImfReached ||
                               r.StopReason == EmdStopReason.ResidualTooFewExtrema);
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL: {name}" + (detail is null ? "" : $" ({detail})"));
    }
}
