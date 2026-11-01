using System.Diagnostics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace SP_module_test;

/// <summary>0.11.x: adaptive filters (fixed RLS / NLMS / NLMF / defaults, new LMS-family, RLS-family, APA, frequency-domain, lattice and FxLMS algorithms).</summary>
internal static class Adaptive_011_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing adaptive filters (0.11.x)...");

        BaseClassContract();
        ArgumentValidation();
        ZeroEpsilonStaysFinite();
        InitKeepsTheSignalHistory();
        DefaultsConverge();
        NlmsNlmfNormalization();
        RlsMatchesTextbook();
        RlsStaysStable();
        RlsInputScaleAndGaps();
        QrdRlsKeepsExternalWeights();
        QrdRlsMatchesRls();
        Identities();
        TimeVaryingStepSizes();
        SparseSystems();
        ColoredInput();
        ImpulsiveNoise();
        FrequencyDomain();
        Lattice();
        FxLms();

        if (_failures != 0)
            throw new InvalidOperationException($"Adaptive filters 0.11.x: {_failures} assertion(s) failed.");

        Console.WriteLine("Adaptive_011_test: PASS");
    }

    // ---------------------------------------------------------------- base class

    static void BaseClassContract()
    {
        const int n = 12;
        var h = MakeSystem(n, 10);
        var (x1, d1) = MakeData(h, 3000, 11);
        var (x2, d2) = MakeData(h, 1500, 12);

        AdaptiveFilter[] Make() =>
        [
            new LmsFilter(n, 0.01f), new NlmsFilter(n, 0.5f, 1e-3f), new SignLmsFilter(n, 1e-3f), new LmfFilter(n, 0.01f),
            new NlmfFilter(n, 0.05f, 1e-3f), new VariableStepLmsFilter(n), new RlsFilter(n, 0.995f), new QrdRlsFilter(n, 0.995f),
            new SignErrorLmsFilter(n), new SignDataLmsFilter(n), new VssLmsFilter(n), new GassLmsFilter(n), new GngdFilter(n),
            new LlncoshFilter(n), new GmccFilter(n), new PnlmsFilter(n), new IpnlmsFilter(n), new BlockLmsFilter(n), new ApaFilter(n),
            new NssLmsFilter(n, 0.05f), new OcNlmsFilter(n, 0.5f, 1e-3f, 50), new SlidingWindowRlsFilter(n, 40),
        ];

        // Reset() restarts the signal history and keeps the weights: the same as a fresh filter that was Init()-ed with these weights
        var trained = Make();
        var fresh = Make();
        for (var a = 0; a < trained.Length; a++)
        {
            var name = trained[a].GetType().Name;
            for (var t = 0; t < x1.Length; t++)
                trained[a].Process(x1[t], d1[t]);
            trained[a].Reset();
            fresh[a].Init(trained[a].Kernel);
            ExpectClose($"{name}: Reset ≡ fresh Init", RunOutputs(trained[a], x2, d2), RunOutputs(fresh[a], x2, d2), 1e-5f);
        }

        // frozen filtering (ProcessAllSamples / Process(float)) must not break the following adaptation
        var viaBuffer = Make();
        var viaBaseReference = Make();
        var viaSamples = Make();
        for (var a = 0; a < viaBuffer.Length; a++)
        {
            var name = viaBuffer[a].GetType().Name;
            for (var t = 0; t < 2000; t++)
            {
                viaBuffer[a].Process(x1[t], d1[t]);
                viaBaseReference[a].Process(x1[t], d1[t]);
                viaSamples[a].Process(x1[t], d1[t]);
            }

            var frozen = viaBuffer[a].ProcessAllSamples(x1.AsSpan(2000, 500).ToArray());
            // the same call through a FirFilter reference: the base class method is virtual and must not bypass the algorithm
            FirFilter asFir = viaBaseReference[a];
            var frozenViaBase = asFir.ProcessAllSamples(x1.AsSpan(2000, 500).ToArray());
            var expected = new List<float>();
            for (var t = 2000; t < 2500; t++)
                expected.Add(viaSamples[a].Process(x1[t]));
            for (var k = 0; k < n - 1; k++)
                expected.Add(viaSamples[a].Process(0f));

            ExpectClose($"{name}: ProcessAllSamples ≡ Process(float)", frozen, expected.ToArray(), 1e-5f);
            ExpectClose($"{name}: ProcessAllSamples through a FirFilter reference", frozenViaBase, expected.ToArray(), 1e-5f);

            var resumed = RunOutputs(viaSamples[a], x2, d2);
            ExpectClose($"{name}: adaptation resumes after frozen filtering", RunOutputs(viaBuffer[a], x2, d2), resumed, 1e-5f);
            ExpectClose($"{name}: adaptation resumes after frozen filtering through a FirFilter reference", RunOutputs(viaBaseReference[a], x2, d2), resumed, 1e-5f);
        }

        // APA keeps its own history of the input: frozen samples have to be in it when the adaptation resumes
        {
            var apaFrozen = new ApaFilter(n, 4);
            var apaAdaptive = new ApaFilter(n, 4);
            for (var t = 0; t < 2000; t++)
            {
                apaFrozen.Process(x1[t], d1[t]);
                apaAdaptive.Process(x1[t], d1[t]);
            }

            // the same samples are fed without a desired signal / with the filter output as the desired signal (zero error)
            for (var t = 2000; t < 2300; t++)
            {
                var y = apaFrozen.Process(x1[t]);
                apaAdaptive.Process(x1[t], y);
            }

            ExpectClose("APA history follows frozen samples", RunOutputs(apaFrozen, x2, d2), RunOutputs(apaAdaptive, x2, d2), 1e-4f);
        }

        // ApplyTo: the sample loop (short kernels, with the state of a fresh filter), overlap-save (long kernels) and the direct form agree
        {
            foreach (var order in new[] { 12, 80 })
            {
                var hs = MakeSystem(order, 14);
                var (xs, ds) = MakeData(hs, 4000, 15);
                var filter = new NlmsFilter(order, 0.5f, 1e-3f);
                Feed(filter, xs, ds);
                filter.Reset();
                var signal = Signal.FromCopy(x2, 1);
                var automatic = filter.ApplyTo(signal).Samples.ToArray();
                var direct = filter.ApplyTo(signal, FilteringMethod.DifferenceEquation).Samples.ToArray();
                ExpectClose($"ApplyTo order {order}: automatic ≡ direct form", automatic, direct, 1e-3f);

                // and the adaptation still works afterwards
                var resumed = RunOutputs(filter, x2, d2);
                Expect($"ApplyTo order {order}: finite after resume", resumed.All(float.IsFinite));
            }
        }

        // Process(..., out error), ProcessAll(...)
        var lms = new LmsFilter(n, 0.01f);
        var other = new LmsFilter(n, 0.01f);
        var output = new float[x2.Length];
        var error = new float[x2.Length];
        lms.ProcessAll(x2, d2, output, error);
        var expectedOutput = new float[x2.Length];
        var expectedError = new float[x2.Length];
        for (var t = 0; t < x2.Length; t++)
        {
            expectedOutput[t] = other.Process(x2[t], d2[t], out expectedError[t]);
        }
        ExpectClose("ProcessAll output", output, expectedOutput, 0);
        ExpectClose("ProcessAll error", error, expectedError, 0);
        Expect("error = desired - output", Math.Abs(error[7] - (d2[7] - output[7])) == 0);
        Expect("Order", lms.Order == n);
        Expect("Kernel length", lms.Kernel.Length == n);
    }

    static void ArgumentValidation()
    {
        ExpectThrows<ArgumentException>("order 0", () => new LmsFilter(0));
        ExpectThrows<ArgumentException>("order -1", () => new NlmsFilter(-1));
        ExpectThrows<ArgumentException>("Init wrong length", () => new LmsFilter(4).Init(new float[3]));
        ExpectThrows<ArgumentException>("ProcessAll length mismatch", () => new LmsFilter(4).ProcessAll(new float[10], new float[9], new float[10]));
        ExpectThrows<ArgumentException>("ProcessAll short output", () => new LmsFilter(4).ProcessAll(new float[10], new float[10], new float[9]));
        ExpectThrows<ArgumentException>("RLS lambda 0", () => new RlsFilter(4, 0f));
        ExpectThrows<ArgumentException>("RLS lambda 1.1", () => new RlsFilter(4, 1.1f));
        ExpectThrows<ArgumentException>("RLS init corr", () => new RlsFilter(4, 0.99f, 0f));
        ExpectThrows<ArgumentException>("QRD-RLS lambda", () => new QrdRlsFilter(4, 2f));
        ExpectThrows<ArgumentException>("IPNLMS alpha", () => new IpnlmsFilter(4, 0.5f, 1.5f));
        ExpectThrows<ArgumentException>("PNLMS rho", () => new PnlmsFilter(4, 0.5f, 2f));
        ExpectThrows<ArgumentException>("APA projection", () => new ApaFilter(4, 0));
        ExpectThrows<ArgumentException>("VSS bounds", () => new VssLmsFilter(4, null, 0.97f, 4.8e-4f, 0.1f, 0.01f));
        ExpectThrows<ArgumentOutOfRangeException>("GMCC alpha", () => new GmccFilter(4, 0.01f, 0.5f));
        ExpectThrows<ArgumentException>("FDAF block size", () => new FrequencyDomainAdaptiveFilter(48));
        ExpectThrows<ArgumentException>("FDAF block length", () => new FrequencyDomainAdaptiveFilter(16).ProcessBlock(new float[8], new float[16], new float[16]));
        ExpectThrows<ArgumentException>("FDAF Init length", () => new FrequencyDomainAdaptiveFilter(16, 2).Init(new float[16]));
        ExpectThrows<ArgumentException>("GAL stages", () => new GalLatticeFilter(0));
        ExpectThrows<ArgumentException>("FxLMS empty secondary path", () => new FxLmsFilter(4, ReadOnlySpan<float>.Empty));

        // ranges that make an algorithm dead or meaningless
        ExpectThrows<ArgumentException>("IPNLMS alpha 1 (every gain would be zero)", () => new IpnlmsFilter(4, 0.5f, 1f));
        ExpectThrows<ArgumentException>("GASS alpha 1", () => new GassLmsFilter(4, GassVariant.Ang, null, null, 1f));
        ExpectThrows<ArgumentException>("GAL muLattice 1", () => new GalLatticeFilter(4, 1f));
        ExpectThrows<ArgumentException>("PNLMS negative eps", () => new PnlmsFilter(4, 0.5f, 0.01f, 0.01f, -1f));
        ExpectThrows<ArgumentException>("IPNLMS negative eps", () => new IpnlmsFilter(4, 0.5f, 0f, -1f));
        ExpectThrows<ArgumentException>("FxLMS negative eps", () => new FxLmsFilter(4, new float[] { 1f }, 0.1f, true, -1f));

        // the sign filter of the original set used Math.Sign, which throws for NaN
        ExpectNoThrow("Sign-Sign LMS with NaN input", () =>
        {
            var filter = new SignLmsFilter(4);
            filter.Process(float.NaN, 1f);
            filter.Process(1f, float.NaN);
        });
    }

    // ---------------------------------------------------------------- robustness

    static void ZeroEpsilonStaysFinite()
    {
        // eps = 0 turns the normalization of a silent regressor into 0 / 0: it must not poison the weights
        var silence = new float[200];
        AdaptiveFilter[] filters = [new PnlmsFilter(8, 0.5f, 0.01f, 0.01f, 0f), new IpnlmsFilter(8, 0.5f, 0f, 0f), new NlmsFilter(8, 0.5f, 0f), new NlmfFilter(8, 0.05f, 0f)];
        foreach (var filter in filters)
        {
            Feed(filter, silence, silence);
            Expect($"{filter.GetType().Name} eps=0 stays finite on silence", filter.Kernel.All(float.IsFinite));
        }

        var fdaf = new FrequencyDomainAdaptiveFilter(16, 2, 0.5f, 0.9f, 0f);
        fdaf.ProcessAll(silence.AsSpan(0, 64), silence.AsSpan(0, 64), new float[64]);
        Expect("FDAF eps=0 stays finite on silence", fdaf.Kernel.All(float.IsFinite));

        var lattice = new GalLatticeFilter(4, 0.001f, 0.3f, 0.99f, 0f);
        for (var t = 0; t < 50; t++)
            lattice.Process(0f, 0f);
        Expect("GAL eps=0 stays finite on silence", lattice.LadderCoefficients.All(float.IsFinite) && lattice.ReflectionCoefficients.All(float.IsFinite));

        var fx = new FxLmsFilter(4, new float[] { 0f, 1f }, 0.1f, true, 0f);
        var y = 0f;
        for (var t = 0; t < 50; t++)
            y = fx.Process(0f, 1f);
        Expect("FxLMS eps=0 stays finite on silence", float.IsFinite(y) && fx.Kernel.All(float.IsFinite));
    }

    static void InitKeepsTheSignalHistory()
    {
        // Init() replaces the weights and restarts the adaptation state, but not the signal history: starting from the true system
        // of noise-free data there is nothing to adapt, whatever internal history the algorithm keeps
        const int n = 12;
        var h = MakeSystem(n, 91);
        var (x, d) = MakeData(h, 4000, 92, 0);

        (string, AdaptiveFilter)[] filters =
        [
            ("APA", new ApaFilter(n, 4)), ("NLMS", new NlmsFilter(n, 0.5f, 1e-3f)), ("GNGD", new GngdFilter(n)), ("PNLMS", new PnlmsFilter(n)),
            ("RLS", new RlsFilter(n, 0.995f)), ("QRD-RLS", new QrdRlsFilter(n, 0.995f)), ("BlockLMS", new BlockLmsFilter(n)),
        ];

        foreach (var (name, filter) in filters)
        {
            for (var t = 0; t < 3000; t++)
                filter.Process(x[t], d[t]);

            filter.Init(h);
            var worst = double.NegativeInfinity;
            for (var t = 3000; t < 3300; t++)
            {
                filter.Process(x[t], d[t]);
                worst = Math.Max(worst, WeightErrorDb(filter.Kernel, h));
            }
            Expect($"{name}: Init mid-stream does not disturb the weights", worst < -30, $"worst {worst:F1} dB");
        }
    }

    static void RlsInputScaleAndGaps()
    {
        const int n = 8;
        const double lambda = 0.99;
        var h = MakeSystem(n, 5);
        var (x0, d0) = MakeData(h, 20000, 6, 1e-3);

        // the problem is scale free: the textbook RLS (no safeguards, double precision) gives the same weights for every amplitude,
        // so the safeguards of the library must not depend on the amplitude either
        foreach (var scale in new[] { 1.0, 1e-3, 1e-6 })
        {
            var x = x0.Select(v => (float)(v * scale)).ToArray();
            var d = d0.Select(v => (float)(v * scale)).ToArray();
            var reference = TextbookRlsWeights(x, d, n, lambda, 100);
            var rls = new RlsFilter(n, (float)lambda);
            var qrd = new QrdRlsFilter(n, (float)lambda);
            Feed(rls, x, d);
            Feed(qrd, x, d);
            ExpectClose($"RLS with input amplitude {scale} ≡ textbook RLS", rls.Kernel, reference, 1e-3f);
            ExpectClose($"QRD-RLS with input amplitude {scale} ≡ textbook RLS", qrd.Kernel, reference, 1e-3f);
        }

        var (x1, d1) = MakeData(h, 20000, 6);
        var (x2, d2) = MakeData(h, 20000, 7);
        foreach (var filter in new AdaptiveFilter[] { new RlsFilter(n, (float)lambda), new QrdRlsFilter(n, (float)lambda) })
        {
            var name = filter.GetType().Name;

            // a gap of digital silence carries no information: the filter is untouched and does not have to converge again
            Feed(filter, x1, d1);
            for (var i = 0; i < n; i++)
                filter.Process(0f, 0f);
            var before = filter.Kernel;
            for (var i = 0; i < 100_000; i++)
                filter.Process(0f, 0f);
            ExpectClose($"{name}: weights are not changed by a gap", filter.Kernel, before, 0);
            double acc = 0;
            for (var t = 0; t < 200; t++)
            {
                var y = filter.Process(x2[t], d2[t]);
                if (t >= n)
                    acc += (d2[t] - y) * (d2[t] - y);
            }
            var mse = 10 * Math.Log10(acc / (200 - n));
            Expect($"{name}: no re-convergence after a gap", mse < -25, $"{mse:F1} dB");

            // a constant input excites a single direction: forgetting must not wind up the others
            var dc = 0.5f * h.Sum();
            for (var i = 0; i < 300_000; i++)
                filter.Process(0.5f, dc);
            Feed(filter, x2, d2);
            var err = WeightErrorDb(filter.Kernel, h);
            Expect($"{name}: recovers after 300k samples of a constant input", err < -40, $"{err:F1} dB");
        }
    }

    static void QrdRlsKeepsExternalWeights()
    {
        // the weights of the QRD-RLS are the solution of R w = p: a change from outside (ChangeKernel) must not be undone by the next sample
        const int n = 8;
        var h = MakeSystem(n, 5);
        var (x, d) = MakeData(h, 5000, 6);
        var qrd = new QrdRlsFilter(n, 0.995f);
        Feed(qrd, x, d);
        Expect("QRD-RLS converged", WeightErrorDb(qrd.Kernel, h) < -40);

        qrd.ChangeKernel(new float[n]);
        var y = qrd.Process(x[10], d[10]);
        Expect("QRD-RLS: ChangeKernel takes effect immediately", y == 0f);
        Expect("QRD-RLS: ChangeKernel is not undone by the adaptation", WeightErrorDb(qrd.Kernel, h) > -3, $"{WeightErrorDb(qrd.Kernel, h):F1} dB");
    }

    // ---------------------------------------------------------------- defaults

    static void DefaultsConverge()
    {
        foreach (var n in new[] { 8, 32 })
        {
            var h = MakeSystem(n, 1);
            var (x, d) = MakeData(h, 30000, 2);
            (string, AdaptiveFilter)[] filters =
            [
                ("LMS", new LmsFilter(n)), ("NLMS", new NlmsFilter(n)), ("SignLMS", new SignLmsFilter(n)), ("LMF", new LmfFilter(n)),
                ("NLMF", new NlmfFilter(n)), ("VariableStepLMS", new VariableStepLmsFilter(n)), ("RLS", new RlsFilter(n)),
                ("QRD-RLS", new QrdRlsFilter(n)), ("SignError", new SignErrorLmsFilter(n)), ("SignData", new SignDataLmsFilter(n)),
                ("VSS", new VssLmsFilter(n)), ("GASS-Benveniste", new GassLmsFilter(n, GassVariant.Benveniste)),
                ("GASS-Ang", new GassLmsFilter(n, GassVariant.Ang)), ("GASS-Mathews", new GassLmsFilter(n, GassVariant.Mathews)),
                ("GNGD", new GngdFilter(n)), ("Llncosh", new LlncoshFilter(n)), ("GMCC", new GmccFilter(n)), ("PNLMS", new PnlmsFilter(n)),
                ("IPNLMS", new IpnlmsFilter(n)), ("BlockLMS", new BlockLmsFilter(n)), ("APA", new ApaFilter(n)),
            ];

            foreach (var (name, filter) in filters)
            {
                Feed(filter, x, d);
                var err = WeightErrorDb(filter.Kernel, h);
                Expect($"default {name} N={n} converges", err < -20, $"{err:F1} dB");
            }
        }
    }

    // ---------------------------------------------------------------- fixed algorithms

    static void NlmsNlmfNormalization()
    {
        // one tap, unit input: NLMS with mu = 1 and eps = 0 reaches the solution in a single step
        var nlms = new NlmsFilter(1, 1f, 0f);
        nlms.Process(1f, 3f);
        Expect("NLMS first step w = 3", Math.Abs(nlms.Kernel[0] - 3f) < 1e-6f, $"{nlms.Kernel[0]}");

        var nlmf = new NlmfFilter(1, 1f, 0f);
        nlmf.Process(1f, 3f);
        Expect("NLMF first step w = 4 e³ = 108", Math.Abs(nlmf.Kernel[0] - 108f) < 1e-3f, $"{nlmf.Kernel[0]}");

        // multi-tap: the trajectory equals the textbook NLMS
        const int n = 8;
        var h = MakeSystem(n, 8);
        var (x, d) = MakeData(h, 200, 9, 0.01);
        var filter = new NlmsFilter(n, 0.5f, 1e-3f);
        var w = new double[n];
        double maxDiff = 0;
        for (var t = 0; t < x.Length; t++)
        {
            filter.Process(x[t], d[t]);
            double norm = 1e-3, y = 0;
            for (var k = 0; k < n; k++)
            {
                double u = t - k >= 0 ? x[t - k] : 0;
                y += w[k] * u;
                norm += u * u;
            }
            var e = d[t] - y;
            for (var k = 0; k < n; k++)
            {
                double u = t - k >= 0 ? x[t - k] : 0;
                w[k] += 0.5 * e * u / norm;
            }
            var wl = filter.Kernel;
            for (var k = 0; k < n; k++)
                maxDiff = Math.Max(maxDiff, Math.Abs(wl[k] - w[k]));
        }
        Expect("NLMS ≡ textbook NLMS", maxDiff < 1e-5, $"maxΔ={maxDiff:E2}");
    }

    static void RlsMatchesTextbook()
    {
        const int n = 16;
        var h = MakeSystem(n, 3);
        var (x, d) = MakeData(h, 5000, 4);

        foreach (var lambda in new[] { 0.95, 0.99, 0.999, 1.0 })
        {
            var filter = new RlsFilter(n, (float)lambda, 100f);
            var p = new double[n, n];
            for (var i = 0; i < n; i++)
                p[i, i] = 100;
            var w = new double[n];
            var u = new double[n];
            var pu = new double[n];
            double maxDiff = 0;

            for (var t = 0; t < x.Length; t++)
            {
                for (var k = 0; k < n; k++)
                    u[k] = t - k >= 0 ? x[t - k] : 0;
                double y = 0;
                for (var k = 0; k < n; k++)
                    y += w[k] * u[k];
                var e = d[t] - y;
                for (var i = 0; i < n; i++)
                {
                    double s = 0;
                    for (var k = 0; k < n; k++)
                        s += p[i, k] * u[k];
                    pu[i] = s;
                }
                var g = lambda;
                for (var k = 0; k < n; k++)
                    g += u[k] * pu[k];
                for (var i = 0; i < n; i++)
                    w[i] += pu[i] / g * e;
                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                        p[i, j] = (p[i, j] - pu[i] * pu[j] / g) / lambda;

                maxDiff = Math.Max(maxDiff, Math.Abs(filter.Process(x[t], d[t]) - y));
            }

            Expect($"RLS λ={lambda} ≡ textbook RLS", maxDiff < 1e-4, $"maxΔy={maxDiff:E2}");
            Expect($"RLS λ={lambda} converges", WeightErrorDb(filter.Kernel, h) < -50, $"{WeightErrorDb(filter.Kernel, h):F1} dB");
        }
    }

    static void RlsStaysStable()
    {
        // forgetting factor below 1 must not blow up when the input is silent for a long time
        const int n = 8;
        var h = MakeSystem(n, 5);
        var (x, d) = MakeData(h, 20000, 6);
        var filter = new RlsFilter(n, 0.99f);
        Feed(filter, x, d);
        for (var i = 0; i < 300_000; i++)
            filter.Process(0f, 0f);
        var (x2, d2) = MakeData(h, 20000, 7);
        Feed(filter, x2, d2);
        var err = WeightErrorDb(filter.Kernel, h);
        Expect("RLS recovers after 300k silent samples", err < -40, $"{err:F1} dB");

        var qrd = new QrdRlsFilter(n, 0.99f);
        Feed(qrd, x, d);
        for (var i = 0; i < 300_000; i++)
            qrd.Process(0f, 0f);
        Feed(qrd, x2, d2);
        err = WeightErrorDb(qrd.Kernel, h);
        Expect("QRD-RLS recovers after 300k silent samples", err < -40, $"{err:F1} dB");

        // O(N²) per sample: 20000 samples of a 64 taps filter take ~0.1 s (an O(N³) update takes ~15 s)
        var rx = new float[20000];
        var rd = new float[20000];
        var rng = new Random(1);
        for (var i = 0; i < rx.Length; i++)
        {
            rx[i] = (float)Gauss(rng);
            rd[i] = (float)Gauss(rng);
        }
        var big = new RlsFilter(64, 0.999f);
        var sw = Stopwatch.StartNew();
        Feed(big, rx, rd);
        Expect("RLS N=64 speed", sw.Elapsed.TotalSeconds < 5, $"{sw.Elapsed.TotalSeconds:F1} s");
    }

    static void QrdRlsMatchesRls()
    {
        const int n = 16;
        var h = MakeSystem(n, 21);
        var (x, d) = MakeData(h, 3000, 22, 0.01);

        foreach (var lambda in new[] { 0.995f, 1f })
        {
            var rls = new RlsFilter(n, lambda);
            var qrd = new QrdRlsFilter(n, lambda);
            float maxDiff = 0;
            for (var t = 0; t < x.Length; t++)
            {
                var yr = rls.Process(x[t], d[t]);
                var yq = qrd.Process(x[t], d[t]);
                // the regularization of the first samples is forgotten at slightly different times
                if (t > 1500)
                    maxDiff = Math.Max(maxDiff, Math.Abs(yr - yq));
            }
            Expect($"QRD-RLS λ={lambda} ≡ RLS after the start-up", maxDiff < 1e-4f, $"maxΔy={maxDiff:E2}");
            ExpectClose($"QRD-RLS λ={lambda} weights ≡ RLS weights", rls.Kernel, qrd.Kernel, 1e-4f);
        }

        // ill-conditioned (strongly colored) input
        var hc = MakeSystem(n, 23);
        var (xc, dc) = MakeData(hc, 6000, 24, 1e-3, 0.97);
        var qrdColored = new QrdRlsFilter(n, 0.999f);
        Feed(qrdColored, xc, dc);
        Expect("QRD-RLS colored input", WeightErrorDb(qrdColored.Kernel, hc) < -40, $"{WeightErrorDb(qrdColored.Kernel, hc):F1} dB");
    }

    // ---------------------------------------------------------------- new algorithms

    static void Identities()
    {
        const int n = 16;
        var h = MakeSystem(n, 21);
        var (x, d) = MakeData(h, 3000, 22, 0.01);

        void Same(string name, AdaptiveFilter a, AdaptiveFilter b)
        {
            var diff = MaxOutputDiff(a, b, x, d);
            var wa = a.Kernel;
            var wb = b.Kernel;
            for (var k = 0; k < n; k++)
                diff = Math.Max(diff, Math.Abs(wa[k] - wb[k]));
            Expect(name, diff < 1e-5, $"maxΔ={diff:E2}");
        }

        Same("IPNLMS(α=-1) ≡ NLMS", new IpnlmsFilter(n, 0.5f, -1f, 1e-3f), new NlmsFilter(n, 0.5f, n * 1e-3f));
        Same("PNLMS(ρ=1) ≡ NLMS", new PnlmsFilter(n, 0.5f, 1f, 0.01f, 1e-3f), new NlmsFilter(n, 0.5f, 1e-3f));
        Same("APA(K=1) ≡ NLMS", new ApaFilter(n, 1, 0.5f, 1e-3f), new NlmsFilter(n, 0.5f, 1e-3f));
        Same("GNGD(ρ=0) ≡ NLMS", new GngdFilter(n, 0.5f, 1e-2f, 0f), new NlmsFilter(n, 0.5f, 1e-2f));
        Same("GASS Benveniste(ρ=0) ≡ LMS", new GassLmsFilter(n, GassVariant.Benveniste, 0.01f, 0f, 0.9f, 1e-4f, 0.1f), new LmsFilter(n, 0.01f));
        Same("GASS Ang(ρ=0) ≡ LMS", new GassLmsFilter(n, GassVariant.Ang, 0.01f, 0f, 0.9f, 1e-4f, 0.1f), new LmsFilter(n, 0.01f));
        Same("GASS Mathews(ρ=0) ≡ LMS", new GassLmsFilter(n, GassVariant.Mathews, 0.01f, 0f, 0.9f, 1e-4f, 0.1f), new LmsFilter(n, 0.01f));
        Same("VSS(γ=0, α=1) ≡ LMS", new VssLmsFilter(n, 0.01f, 1f, 0f, 1e-4f, 0.1f), new LmsFilter(n, 0.01f));
        Same("BlockLMS(L=1) ≡ LMS", new BlockLmsFilter(n, 1, 0.01f), new LmsFilter(n, 0.01f));
        Same("Llncosh(small λ) ≈ LMS(μλ)", new LlncoshFilter(n, 10f, 1e-3f), new LmsFilter(n, 0.01f));
        Same("GMCC(wide kernel) ≈ LMS", new GmccFilter(n, 0.01f, 2f, 1e3f), new LmsFilter(n, 0.01f));

        // sign algorithms: one step by hand
        var signError = new SignErrorLmsFilter(2, 0.1f);
        signError.Process(2f, 1f);                  // e = 1, x = [2, 0]
        ExpectClose("Sign-Error step", signError.Kernel, [0.2f, 0f], 1e-6f);
        var signData = new SignDataLmsFilter(2, 0.1f);
        signData.Process(-2f, 1f);                  // e = 1, sign(x) = [-1, 0]
        ExpectClose("Sign-Data step", signData.Kernel, [-0.1f, 0f], 1e-6f);
    }

    static void TimeVaryingStepSizes()
    {
        const int n = 8;
        var h = MakeSystem(n, 31);
        var (x, d) = MakeData(h, 20000, 32, 1e-3);

        var vss = new VssLmsFilter(n);
        var first = vss.Mu;
        var inBounds = true;
        float peak = 0;
        for (var t = 0; t < x.Length; t++)
        {
            vss.Process(x[t], d[t]);
            inBounds &= vss.Mu >= 0.1f / n / 10 - 1e-9f && vss.Mu <= 0.1f / n + 1e-9f;
            peak = Math.Max(peak, vss.Mu);
        }
        Expect("VSS step stays within its bounds", inBounds);
        Expect("VSS step grows for large errors and shrinks once converged", peak > first && vss.Mu < 0.5f * peak, $"initial={first}, peak={peak}, final={vss.Mu}");
        vss.Reset();
        Expect("VSS Reset restores the initial step", vss.Mu == first);

        foreach (var variant in new[] { GassVariant.Benveniste, GassVariant.Ang, GassVariant.Mathews })
        {
            var gass = new GassLmsFilter(n, variant);
            var ok = true;
            var moved = false;
            var initial = gass.Mu;
            for (var t = 0; t < x.Length; t++)
            {
                gass.Process(x[t], d[t]);
                ok &= gass.Mu >= 0.1f / n / 10 - 1e-9f && gass.Mu <= 0.1f / n + 1e-9f && float.IsFinite(gass.Mu);
                moved |= gass.Mu != initial;
            }
            Expect($"GASS {variant} step adapts within its bounds", ok && moved);
            gass.Reset();
            Expect($"GASS {variant} Reset restores the initial step", gass.Mu == initial);
        }

        var gngd = new GngdFilter(n, 0.75f, 1f, 0.5f);
        var epsilonOk = true;
        var epsilonMoved = false;
        for (var t = 0; t < x.Length; t++)
        {
            gngd.Process(x[t], d[t]);
            epsilonOk &= gngd.Epsilon >= 0 && float.IsFinite(gngd.Epsilon);
            epsilonMoved |= gngd.Epsilon != 1f;
        }
        Expect("GNGD epsilon adapts and stays non-negative", epsilonOk && epsilonMoved);
        Expect("GNGD converges", WeightErrorDb(gngd.Kernel, h) < -40, $"{WeightErrorDb(gngd.Kernel, h):F1} dB");
    }

    static void SparseSystems()
    {
        const int n = 128;
        var h = new float[n];
        h[7] = 0.8f;
        h[40] = -0.5f;
        h[90] = 0.3f;
        var (x, d) = MakeData(h, 500, 31, 1e-3);

        double After500(AdaptiveFilter f)
        {
            Feed(f, x, d);
            return WeightErrorDb(f.Kernel, h);
        }

        var nlms = After500(new NlmsFilter(n, 0.5f, 1e-3f));
        var pnlms = After500(new PnlmsFilter(n));
        var ipnlms = After500(new IpnlmsFilter(n));
        Expect("PNLMS converges faster than NLMS on a sparse system", pnlms < nlms - 6, $"NLMS {nlms:F1} dB, PNLMS {pnlms:F1} dB");
        Expect("IPNLMS converges faster than NLMS on a sparse system", ipnlms < nlms - 6, $"NLMS {nlms:F1} dB, IPNLMS {ipnlms:F1} dB");
    }

    static void ColoredInput()
    {
        const int n = 32;
        var h = MakeSystem(n, 41);
        var (x, d) = MakeData(h, 1000, 42, 1e-3, 0.95);

        double After1000(AdaptiveFilter f)
        {
            Feed(f, x, d);
            return WeightErrorDb(f.Kernel, h);
        }

        var nlms = After1000(new NlmsFilter(n, 0.5f, 1e-3f));
        var apa = After1000(new ApaFilter(n, 2, 0.5f, 1e-3f));
        var rls = After1000(new RlsFilter(n, 0.999f));
        Expect("APA converges faster than NLMS on colored input", apa < nlms - 15, $"NLMS {nlms:F1} dB, APA {apa:F1} dB");
        Expect("RLS converges faster than NLMS on colored input", rls < nlms - 15, $"NLMS {nlms:F1} dB, RLS {rls:F1} dB");
    }

    static void ImpulsiveNoise()
    {
        const int n = 16;
        var h = MakeSystem(n, 51);
        var (x, clean) = MakeData(h, 40000, 52, 1e-3);
        var d = (float[])clean.Clone();
        var rng = new Random(53);
        for (var i = 0; i < d.Length; i++)
        {
            if (rng.NextDouble() < 0.02)
                d[i] += (float)(Gauss(rng) * 50);
        }

        double Run(AdaptiveFilter f)
        {
            Feed(f, x, d);
            return WeightErrorDb(f.Kernel, h);
        }

        var lms = Run(new LmsFilter(n, 0.002f));
        var signError = Run(new SignErrorLmsFilter(n, 0.0005f));
        var llncosh = Run(new LlncoshFilter(n, 0.002f, 1f));
        var gmcc = Run(new GmccFilter(n, 0.005f, 2f, 1f));
        Expect("LMS is disturbed by the outliers", lms > -10, $"{lms:F1} dB");
        Expect("Sign-Error LMS is robust against impulsive noise", signError < -20, $"{signError:F1} dB");
        Expect("Llncosh is robust against impulsive noise", llncosh < -20, $"{llncosh:F1} dB");
        Expect("GMCC is robust against impulsive noise", gmcc < -20, $"{gmcc:F1} dB");
    }

    static void FrequencyDomain()
    {
        // white input, single partition and several partitions
        foreach (var (block, partitions) in new[] { (32, 1), (64, 4) })
        {
            var n = block * partitions;
            var h = MakeSystem(n, 61);
            var (x, d) = MakeData(h, 40000, 62);
            var filter = new FrequencyDomainAdaptiveFilter(block, partitions);
            var y = new float[x.Length];
            var processed = filter.ProcessAll(x, d, y);
            Expect($"FDAF B={block} P={partitions} processes whole blocks", processed == x.Length);
            var err = WeightErrorDb(filter.Kernel, h);
            Expect($"FDAF B={block} P={partitions} converges", err < -40, $"{err:F1} dB");
            Expect($"FDAF B={block} P={partitions} order", filter.Order == n);

            double acc = 0;
            for (var t = x.Length - 2000; t < x.Length; t++)
                acc += (d[t] - y[t]) * (d[t] - y[t]);
            Expect($"FDAF B={block} P={partitions} output error", 10 * Math.Log10(acc / 2000) < -45, $"{10 * Math.Log10(acc / 2000):F1} dB");
        }

        // unconstrained gradient is fine for a single partition
        {
            var h = MakeSystem(32, 63);
            var (x, d) = MakeData(h, 40000, 64);
            var filter = new FrequencyDomainAdaptiveFilter(32, 1, 0.5f, 0.9f, 1e-3f, false);
            filter.ProcessAll(x, d, new float[x.Length]);
            Expect("unconstrained FDAF (1 partition) converges", WeightErrorDb(filter.Kernel, h) < -35, $"{WeightErrorDb(filter.Kernel, h):F1} dB");
        }

        // colored input: per-bin normalization beats the time-domain NLMS
        {
            const int block = 64, partitions = 4, n = block * partitions;
            var h = MakeSystem(n, 65);
            var (x, d) = MakeData(h, 5000 / block * block, 66, 1e-3, 0.9);
            var fdaf = new FrequencyDomainAdaptiveFilter(block, partitions);
            fdaf.ProcessAll(x, d, new float[x.Length]);
            var nlms = new NlmsFilter(n, 0.5f, 1e-3f);
            Feed(nlms, x, d);
            var errFd = WeightErrorDb(fdaf.Kernel, h);
            var errTd = WeightErrorDb(nlms.Kernel, h);
            Expect("FDAF converges faster than NLMS on colored input", errFd < errTd - 20, $"NLMS {errTd:F1} dB, FDAF {errFd:F1} dB");
        }

        // Init / Kernel round trip, ProcessBlock ≡ ProcessAll, Reset keeps the weights
        {
            var rng = new Random(67);
            var w = new float[128];
            for (var i = 0; i < w.Length; i++)
                w[i] = (float)Gauss(rng);
            var a = new FrequencyDomainAdaptiveFilter(32, 4);
            a.Init(w);
            ExpectClose("FDAF Init/Kernel round trip", a.Kernel, w, 1e-4f);

            var x = new float[256];
            var d = new float[256];
            for (var i = 0; i < x.Length; i++)
            {
                x[i] = (float)Gauss(rng);
                d[i] = (float)Gauss(rng);
            }

            var b = new FrequencyDomainAdaptiveFilter(32, 4);
            b.Init(w);
            var all = new float[256];
            var blocks = new float[256];
            a.ProcessAll(x, d, all);
            for (var s = 0; s < 256; s += 32)
                b.ProcessBlock(x.AsSpan(s, 32), d.AsSpan(s, 32), blocks.AsSpan(s, 32));
            ExpectClose("FDAF ProcessBlock ≡ ProcessAll", all, blocks, 0);

            var before = a.Kernel;
            a.Reset();
            ExpectClose("FDAF Reset keeps the weights", a.Kernel, before, 0);

            // with fixed weights (step -> tiny) the output is the linear convolution
            var fixedFilter = new FrequencyDomainAdaptiveFilter(32, 4, 1e-9f);
            fixedFilter.Init(w);
            var y = new float[256];
            fixedFilter.ProcessAll(x, d, y);
            var expected = new float[256];
            for (var t = 0; t < 256; t++)
            {
                double s = 0;
                for (var k = 0; k < w.Length; k++)
                    if (t - k >= 0)
                        s += w[k] * x[t - k];
                expected[t] = (float)s;
            }
            ExpectClose("FDAF output is the linear convolution", y, expected, 1e-3f);
        }
    }

    static void Lattice()
    {
        // strongly colored input, moderate noise: the lattice reaches the noise floor much faster than LMS
        const int n = 16;
        var h = MakeSystem(n, 71);
        var (x, d) = MakeData(h, 20000, 72, 1e-2, 0.99);

        double TailMse(Func<float, float, float> process, int from)
        {
            double acc = 0;
            var count = 0;
            for (var t = 0; t < x.Length; t++)
            {
                var y = process(x[t], d[t]);
                if (t >= from)
                {
                    acc += (d[t] - y) * (d[t] - y);
                    count++;
                }
            }
            return 10 * Math.Log10(acc / count);
        }

        var lattice = new GalLatticeFilter(n - 1);
        var latticeMse = TailMse(lattice.Process, 15000);
        var lms = new LmsFilter(n, 0.005f);
        var lmsMse = TailMse(lms.Process, 15000);
        Expect("GAL reaches the noise floor on colored input", latticeMse < -34, $"{latticeMse:F1} dB (noise floor -40 dB)");
        Expect("GAL beats LMS on colored input", latticeMse < lmsMse - 3, $"LMS {lmsMse:F1} dB, GAL {latticeMse:F1} dB");
        Expect("reflection coefficients are stable", lattice.ReflectionCoefficients.All(k => Math.Abs(k) < 1));
        Expect("lattice dimensions", lattice.Stages == n - 1 && lattice.ReflectionCoefficients.Length == n - 1 && lattice.LadderCoefficients.Length == n);

        lattice.Reset();
        Expect("GAL Reset clears the coefficients", lattice.ReflectionCoefficients.All(k => k == 0) && lattice.LadderCoefficients.All(g => g == 0));

        // white input: the reflection coefficients stay small and the ladder identifies the system
        var (xw, dw) = MakeData(h, 40000, 73, 1e-3);
        var white = new GalLatticeFilter(n - 1, 0.0005f, 0.3f);
        for (var t = 0; t < xw.Length; t++)
            white.Process(xw[t], dw[t]);
        var ladder = white.LadderCoefficients;
        Expect("GAL ladder identifies the system for white input", WeightErrorDb(ladder, h) < -20, $"{WeightErrorDb(ladder, h):F1} dB");
    }

    static void FxLms()
    {
        var rng = new Random(81);
        const int length = 40000;
        var x = new float[length];
        for (var i = 0; i < length; i++)
            x[i] = (float)Gauss(rng);

        var primary = new float[16];
        primary[2] = 0.6f; primary[3] = -0.3f; primary[4] = 0.2f; primary[5] = 0.15f; primary[7] = -0.1f; primary[10] = 0.05f;
        var secondary = new float[8];
        secondary[1] = 0.5f; secondary[2] = 0.2f; secondary[3] = 0.1f; secondary[4] = -0.05f;

        var d = new float[length];
        for (var i = 0; i < length; i++)
        {
            double s = 0;
            for (var k = 0; k < primary.Length; k++)
                if (i - k >= 0)
                    s += primary[k] * x[i - k];
            d[i] = (float)s;
        }

        double Reduction(float[] estimate, bool normalized, float mu)
        {
            var filter = new FxLmsFilter(32, estimate, mu, normalized);
            var y = new float[length];
            var e = new float[length];
            double pd = 0, pe = 0;
            for (var n = 0; n < length; n++)
            {
                double s = 0;
                for (var j = 1; j < secondary.Length; j++)
                    if (n - j >= 0)
                        s += secondary[j] * y[n - j];
                e[n] = (float)(d[n] - s);
                y[n] = filter.Process(x[n], e[n]);
                if (n >= length - 5000)
                {
                    pd += d[n] * d[n];
                    pe += e[n] * e[n];
                }
            }
            return 10 * Math.Log10(pe / pd);
        }

        var inexact = secondary.Select((v, i) => v * (1 + 0.1f * ((i % 2) * 2 - 1))).ToArray();
        var exactNormalized = Reduction(secondary, true, 0.1f);
        var exactPlain = Reduction(secondary, false, 0.002f);
        var inexactNormalized = Reduction(inexact, true, 0.1f);
        Expect("FxNLMS cancels the noise", exactNormalized < -30, $"{exactNormalized:F1} dB");
        Expect("FxLMS cancels the noise", exactPlain < -30, $"{exactPlain:F1} dB");
        Expect("FxNLMS tolerates a ±10% secondary path error", inexactNormalized < -30, $"{inexactNormalized:F1} dB");

        var fx = new FxLmsFilter(4, secondary, 0.1f);
        fx.Init([1f, 2f, 3f, 4f]);
        ExpectClose("FxLMS Init/Kernel", fx.Kernel, [1f, 2f, 3f, 4f], 0);
        Expect("FxLMS Order", fx.Order == 4);
    }

    // ---------------------------------------------------------------- helpers

    static double Gauss(Random rng)
    {
        var u1 = 1.0 - rng.NextDouble();
        var u2 = rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    static float[] MakeSystem(int n, int seed)
    {
        var rng = new Random(seed);
        var h = new float[n];
        double norm = 0;
        for (var k = 0; k < n; k++)
        {
            h[k] = (float)(Gauss(rng) * Math.Exp(-k / (n / 4.0 + 1)));
            norm += h[k] * h[k];
        }
        norm = Math.Sqrt(norm);
        for (var k = 0; k < n; k++)
            h[k] = (float)(h[k] / norm);
        return h;
    }

    /// <summary>Unit-variance AR(1) input (white for ar = 0) and the output of the system h plus white noise.</summary>
    static (float[] x, float[] d) MakeData(float[] h, int length, int seed, double noise = 1e-3, double ar = 0)
    {
        var rng = new Random(seed);
        var x = new float[length];
        var d = new float[length];
        double previous = 0;
        for (var i = 0; i < length; i++)
        {
            previous = ar * previous + Math.Sqrt(1 - ar * ar) * Gauss(rng);
            x[i] = (float)previous;
        }
        for (var i = 0; i < length; i++)
        {
            double s = 0;
            for (var k = 0; k < h.Length; k++)
                if (i - k >= 0)
                    s += h[k] * x[i - k];
            d[i] = (float)(s + noise * Gauss(rng));
        }
        return (x, d);
    }

    /// <summary>Textbook exponentially weighted RLS in double precision without any safeguard; returns the final weights.</summary>
    static float[] TextbookRlsWeights(float[] x, float[] d, int n, double lambda, double delta)
    {
        var p = new double[n, n];
        for (var i = 0; i < n; i++)
            p[i, i] = delta;
        var w = new double[n];
        var u = new double[n];
        var pu = new double[n];

        for (var t = 0; t < x.Length; t++)
        {
            for (var k = 0; k < n; k++)
                u[k] = t - k >= 0 ? x[t - k] : 0;
            double y = 0;
            for (var k = 0; k < n; k++)
                y += w[k] * u[k];
            var e = d[t] - y;
            for (var i = 0; i < n; i++)
            {
                double s = 0;
                for (var k = 0; k < n; k++)
                    s += p[i, k] * u[k];
                pu[i] = s;
            }
            var g = lambda;
            for (var k = 0; k < n; k++)
                g += u[k] * pu[k];
            for (var i = 0; i < n; i++)
                w[i] += pu[i] / g * e;
            for (var i = 0; i < n; i++)
                for (var j = 0; j < n; j++)
                    p[i, j] = (p[i, j] - pu[i] * pu[j] / g) / lambda;
        }

        return w.Select(v => (float)v).ToArray();
    }

    static void Feed(AdaptiveFilter filter, float[] x, float[] d)
    {
        for (var t = 0; t < x.Length; t++)
            filter.Process(x[t], d[t]);
    }

    static float[] RunOutputs(AdaptiveFilter filter, float[] x, float[] d)
    {
        var y = new float[x.Length];
        for (var t = 0; t < x.Length; t++)
            y[t] = filter.Process(x[t], d[t]);
        return y;
    }

    static double MaxOutputDiff(AdaptiveFilter a, AdaptiveFilter b, float[] x, float[] d)
    {
        double max = 0;
        for (var t = 0; t < x.Length; t++)
            max = Math.Max(max, Math.Abs(a.Process(x[t], d[t]) - b.Process(x[t], d[t])));
        return max;
    }

    /// <summary>Relative squared weight error ‖w − h‖² / ‖h‖² in dB (NaN / infinity count as a failure).</summary>
    static double WeightErrorDb(float[] w, float[] h)
    {
        double err = 0, norm = 0;
        for (var k = 0; k < h.Length; k++)
        {
            var diff = w[k] - h[k];
            err += diff * diff;
            norm += h[k] * h[k];
        }
        var db = 10 * Math.Log10(Math.Max(err / norm, 1e-30));
        return double.IsNaN(db) ? double.PositiveInfinity : db;
    }

    static void ExpectClose(string name, float[] a, float[] b, float tol)
    {
        if (a.Length != b.Length)
        {
            Expect(name, false, $"len {a.Length}≠{b.Length}");
            return;
        }
        float max = 0;
        for (var i = 0; i < a.Length; i++)
        {
            max = Math.Max(max, Math.Abs(a[i] - b[i]));    // Math.Max propagates NaN
        }
        Expect(name, max <= tol, $"maxΔ={max:E2}");
    }

    static void ExpectNoThrow(string name, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Expect(name, false, $"threw {ex.GetType().Name}");
        }
    }

    static void ExpectThrows<T>(string name, Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }
        catch (Exception ex)
        {
            Expect(name, false, $"threw {ex.GetType().Name} instead of {typeof(T).Name}");
            return;
        }
        Expect(name, false, "no exception");
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok)
            return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }
}
