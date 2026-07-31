using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

namespace SP_module_test;

public static class ExtendedModeDecomposition_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing extended mode decomposition (EEMD/CEEMDAN/MEMD/MVMD/SSA/EWT)...");

        AssertEemd();
        AssertCeemdan();
        AssertCeemdanTorresReconstruction();
        AssertCeemdanParallelContext();
        AssertMemd();
        AssertMvmd();
        AssertSsa();
        AssertEwt();
        AssertCancelEmd();
        AssertHhtSpectrum();
        AssertHhtAnalyzeModesNoEmd();
        AssertHhtAnalyzeResidualOnModes();
        AssertCeemdanMaxImfFromEmdOptions();
        AssertEwtResidualIndependent();
        AssertVmdNullContextFft();

        if (_failures != 0)
            throw new InvalidOperationException($"ExtendedModeDecomposition: {_failures} assertion(s) failed.");

        Console.WriteLine("Extended mode decomposition: PASS");
    }

    static float[] Tone(int n, float f, float fs)
    {
        var x = new float[n];
        for (int i = 0; i < n; i++)
            x[i] = MathF.Sin(2f * MathF.PI * f * i / fs);
        return x;
    }

    static void AssertEemd()
    {
        var x = Tone(256, 5, 256);
        for (int i = 0; i < x.Length; i++)
            x[i] += 0.3f * MathF.Sin(2f * MathF.PI * 23f * i / 256f);

        var r = ModeDecomposer.Eemd<float>(x, new EemdOptions
        {
            EnsembleCount = 8,
            NoiseRatio = 0.15,
            RandomSeed = 1,
            EmdOptions = new EmdOptions { MaxImf = 4, MaxSiftIterations = 30 },
        });
        Expect("eemd modes", r.ModeCount >= 1);
        Expect("eemd finite", r.IntrinsicModeFunctions.All(m => m.All(float.IsFinite)));
    }

    static void AssertCeemdan()
    {
        var x = Tone(256, 4, 256);
        var r = ModeDecomposer.Ceemdan<float>(x, new CeemdanOptions
        {
            EnsembleCount = 6,
            NoiseRatio = 0.2,
            MaxImf = 3,
            RandomSeed = 2,
            EmdOptions = new EmdOptions { MaxSiftIterations = 25 },
        });
        Expect("ceemdan modes", r.ModeCount >= 1);
    }

    static void AssertCeemdanTorresReconstruction()
    {
        var x = Tone(128, 5, 128);
        for (int i = 0; i < x.Length; i++)
            x[i] += 0.35f * MathF.Sin(2f * MathF.PI * 17f * i / 128f);

        var r = ModeDecomposer.Ceemdan<float>(x, new CeemdanOptions
        {
            EnsembleCount = 8,
            NoiseRatio = 0.2,
            MaxImf = 4,
            RandomSeed = 11,
            EmdOptions = new EmdOptions { MaxSiftIterations = 30 },
        });

        float maxErr = 0;
        for (int i = 0; i < x.Length; i++)
        {
            float s = r.Residual[i];
            foreach (var m in r.IntrinsicModeFunctions)
                s += m[i];
            maxErr = Math.Max(maxErr, Math.Abs(s - x[i]));
        }
        Expect("ceemdan Torres exact recon", maxErr < 1e-4f, $"maxErr={maxErr}");
    }

    static void AssertCeemdanParallelContext()
    {
        var x = Tone(96, 3, 96);
        var r = ModeDecomposer.Ceemdan<float>(x, new CeemdanOptions
        {
            EnsembleCount = 6,
            MaxImf = 2,
            RandomSeed = 7,
            ComputingContext = ComputingContext.Parallel,
            EmdOptions = new EmdOptions { MaxSiftIterations = 20 },
        });
        Expect("ceemdan parallel modes", r.ModeCount >= 1);
        Expect("ceemdan parallel finite", r.IntrinsicModeFunctions.All(m => m.All(float.IsFinite)));
    }

    static void AssertMemd()
    {
        int n = 128;
        var ch0 = Tone(n, 3, n);
        var ch1 = Tone(n, 11, n);
        var r = ModeDecomposer.Memd<float>([ch0, ch1], new MemdOptions
        {
            MaxImf = 3,
            DirectionCount = 16,
            MaxSiftIterations = 20,
        });
        Expect("memd modes", r.IntrinsicModeFunctions.Count >= 1);
        Expect("memd channels", r.IntrinsicModeFunctions[0].Length == 2);
        Expect("memd len", r.IntrinsicModeFunctions[0][0].Length == n);
    }

    static void AssertMvmd()
    {
        int n = 256;
        float fs = 256;
        var ch0 = new float[n];
        var ch1 = new float[n];
        for (int i = 0; i < n; i++)
        {
            double t = i / fs;
            ch0[i] = (float)(Math.Cos(2 * Math.PI * 5 * t) + 0.4 * Math.Cos(2 * Math.PI * 30 * t));
            ch1[i] = (float)(Math.Cos(2 * Math.PI * 5 * t + 0.3) + 0.4 * Math.Cos(2 * Math.PI * 30 * t));
        }

        var r = ModeDecomposer.Mvmd<float>([ch0, ch1], new MvmdOptions
        {
            ModeCount = 2,
            Alpha = 2000,
            MaxIterations = 80,
            SamplingRate = fs,
        });
        Expect("mvmd modes", r.ModeCount == 2);
        Expect("mvmd ch", r.Modes[0].Length == 2);
        Expect("mvmd freqs", r.CenterFrequenciesHz.Length == 2);
    }

    static void AssertSsa()
    {
        var x = Tone(200, 5, 200);
        var r = ModeDecomposer.Ssa<float>(x, new SsaOptions
        {
            WindowLength = 40,
            ComponentCount = 4,
            GroupSize = 1,
            ComputingContext = ComputingContext.Parallel,
        });
        Expect("ssa comps", r.Components.Count >= 1);
        Expect("ssa recon",
            Enumerable.Range(0, x.Length).Max(i =>
            {
                float s = r.Residual[i];
                foreach (var c in r.Components) s += c[i];
                return Math.Abs(s - x[i]);
            }) < 1e-3f);
    }

    static void AssertEwt()
    {
        var x = Tone(512, 7, 512);
        for (int i = 0; i < x.Length; i++)
            x[i] += 0.5f * MathF.Sin(2f * MathF.PI * 40f * i / 512f);

        var r = ModeDecomposer.Ewt<float>(x, new EwtOptions
        {
            MaxBands = 3,
            SamplingRate = 512,
        });
        Expect("ewt modes|resid", r.Modes.Count >= 1 || r.Residual.Length == x.Length);
        Expect("ewt residual len", r.Residual.Length == x.Length);
    }

    static void AssertCancelEmd()
    {
        var x = Tone(2048, 3, 2048);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        bool threw = false;
        try
        {
            ModeDecomposer.Emd<float>(x, new EmdOptions { MaxImf = 8 }, cts.Token);
        }
        catch (OperationCanceledException)
        {
            threw = true;
        }
        Expect("emd cancel", threw);
    }

    static void AssertHhtSpectrum()
    {
        var x = Tone(256, 8, 256);
        var r = ModeDecomposer.Hht<float>(x, 256, new HhtOptions
        {
            BuildSpectrum = true,
            SpectrumTimeStride = 4,
            EmdOptions = new EmdOptions { MaxImf = 3, MaxSiftIterations = 30 },
        });
        Expect("hht spectrum", r.Spectrum.Count > 0);
        Expect("hht spectrum finite", r.Spectrum.All(s =>
            double.IsFinite(s.TimeSeconds) && double.IsFinite(s.FrequencyHz) && double.IsFinite(s.Amplitude)));
    }

    static void AssertHhtAnalyzeModesNoEmd()
    {
        var x = Tone(128, 10, 128);
        var r = ModeDecomposer.Instantaneous<float>(
            [x],
            128,
            residual: null,
            new HhtOptions
            {
                BuildSpectrum = false,
                ComputingContext = ComputingContext.Create(CpuExecutionMode.Normal),
            });
        Expect("hht modes-only count", r.ModeCount == 1);
        Expect("hht modes-only amp len", r.InstantaneousAmplitudes[0].Length == x.Length);
        Expect("hht modes-only finite", r.InstantaneousAmplitudes[0].All(float.IsFinite));
    }

    static void AssertHhtAnalyzeResidualOnModes()
    {
        var x = Tone(64, 5, 64);
        var resid = new float[64];
        for (int i = 0; i < resid.Length; i++)
            resid[i] = 0.01f * i;

        var r = ModeDecomposer.Instantaneous<float>(
            [x],
            64,
            residual: resid,
            new HhtOptions { AnalyzeResidual = true, BuildSpectrum = false });
        Expect("hht analyze-residual modes", r.ModeCount == 2);
    }

    static void AssertCeemdanMaxImfFromEmdOptions()
    {
        var x = Tone(256, 4, 256);
        var r = ModeDecomposer.Ceemdan<float>(x, new CeemdanOptions
        {
            EnsembleCount = 4,
            MaxImf = 16,
            RandomSeed = 3,
            EmdOptions = new EmdOptions { MaxImf = 2, MaxSiftIterations = 20 },
        });
        Expect("ceemdan emd MaxImf tightens", r.ModeCount <= 2);
    }

    static void AssertEwtResidualIndependent()
    {
        var x = Tone(256, 5, 256);
        var r = ModeDecomposer.Ewt<float>(x, new EwtOptions { MaxBands = 2, SamplingRate = 256 });
        if (r.Modes.Count == 0)
        {
            Expect("ewt residual independent (skip)", true);
            return;
        }
        float before = r.Residual[0];
        r.Modes[0][0] = before + 123f;
        Expect("ewt residual not aliased to mode", r.Residual[0] == before);
    }

    static void AssertVmdNullContextFft()
    {
        // null ComputingContext must still run (resolves via ComputingScope / defaults)
        var x = Tone(256, 6, 256);
        for (int i = 0; i < x.Length; i++)
            x[i] += 0.4f * MathF.Sin(2f * MathF.PI * 30f * i / 256f);

        var r = ModeDecomposer.Vmd<float>(x, new VmdOptions
        {
            ModeCount = 2,
            Alpha = 2000,
            MaxIterations = 60,
            SamplingRate = 256,
            ComputingContext = null,
        });
        Expect("vmd null-ctx modes", r.ModeCount == 2);
        Expect("vmd null-ctx finite", r.Modes.All(m => m.All(float.IsFinite)));
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL: {name}" + (detail is null ? "" : $" ({detail})"));
    }
}
