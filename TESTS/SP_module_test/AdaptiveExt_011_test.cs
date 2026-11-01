using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace SP_module_test;

/// <summary>0.11.1: NSS-LMS, OC-NLMS, line enhancer / notch / sinusoid canceller, LSL lattice, sliding-window RLS, complex-valued, multichannel, Volterra and kernel adaptive filters.</summary>
internal static class AdaptiveExt_011_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing adaptive filters, second set (0.11.1)...");

        PadasipParity();
        NssLms();
        OcNlmsOffset();
        LineEnhancer();
        NotchFilter();
        SinusoidalCanceller();
        LslLattice();
        SlidingWindowRls();
        ComplexFilters();
        MultichannelNlms();
        VolterraNlms();
        KernelLms();
        InPlaceBuffers();
        ArgumentValidation();

        if (_failures != 0)
            throw new InvalidOperationException($"Adaptive filters 0.11.1: {_failures} assertion(s) failed.");

        Console.WriteLine("AdaptiveExt_011_test: PASS");
    }

    // ---------------------------------------------------------------- parity with padasip

    /// <summary>
    /// Final weights and Σy² of padasip 1.2.2 (double precision) on the same data: a deterministic LCG stream
    /// x(n) ~ U(−√3, √3) rounded to float, d = h * x + 0.01 · U(−√3, √3), 8 taps, 400 samples, zero initial weights.
    /// </summary>
    static void PadasipParity()
    {
        const int taps = 8, length = 400;
        double[] h = [0.62, -0.41, 0.33, 0.18, -0.12, 0.07, 0.04, -0.02];

        uint state = 123456789;
        double Uniform()
        {
            state = unchecked(1664525u * state + 1013904223u);
            return state / 4294967296.0;
        }
        double Unit() => (Uniform() - 0.5) * 2 * Math.Sqrt(3);

        var x = new float[length];
        for (var i = 0; i < length; i++)
            x[i] = (float)Unit();
        var noise = new double[length];
        for (var i = 0; i < length; i++)
            noise[i] = 0.01 * Unit();
        var d = new float[length];
        for (var n = 0; n < length; n++)
        {
            double s = 0;
            for (var k = 0; k < taps; k++)
                if (n - k >= 0)
                    s += h[k] * x[n - k];
            d[n] = (float)(s + noise[n]);
        }

        (string Name, float[] W, double SumY2)[] references =
        [
            ("LMS", [0.6105593f, -0.3992529f, 0.3241746f, 0.1761595f, -0.1151699f, 0.06810319f, 0.04220648f, -0.02298456f], 176.4842),
            ("NLMS", [0.6217868f, -0.4127377f, 0.3301626f, 0.1772764f, -0.1152832f, 0.07250338f, 0.03853062f, -0.02204064f], 274.1213),
            ("LMF", [0.2075232f, -0.1453992f, 0.1292389f, 0.04875665f, -0.03015666f, 0.0117348f, 0.03052671f, -0.0277042f], 16.29649),
            ("NLMF", [0.564481f, -0.3614293f, 0.2714177f, 0.1478482f, -0.0995966f, 0.0638101f, 0.0349988f, -0.03699852f], 191.0825),
            ("SignLMS", [0.44f, -0.25f, 0.216f, 0.134f, -0.108f, 0.038f, 0.04f, -0.002f], 42.58216),
            ("NssLMS", [0.5880476f, -0.439509f, 0.3504493f, 0.1553652f, -0.1643196f, 0.07366823f, 0.0500607f, 0.001996193f], 280.4508),
            ("RLS", [0.6199492f, -0.4093877f, 0.3303783f, 0.1799385f, -0.1182294f, 0.07072396f, 0.03918818f, -0.02007729f], 277.1586),
            ("GNGD", [0.6215848f, -0.4122259f, 0.330033f, 0.1776196f, -0.115526f, 0.07233727f, 0.03853621f, -0.021846f], 269.8481),
            ("GMCC", [0.6212278f, -0.4115974f, 0.3295148f, 0.1785355f, -0.1152597f, 0.07192167f, 0.03813013f, -0.02165f], 253.5074),
            ("Llncosh", [0.6208861f, -0.4103096f, 0.3297875f, 0.1790097f, -0.1164838f, 0.07152252f, 0.0385594f, -0.02097082f], 213.4579),
            ("APA", [0.624019f, -0.4161905f, 0.3294555f, 0.1775808f, -0.1115977f, 0.0732901f, 0.03812845f, -0.02225954f], 276.4607),
            ("GASS", [0.6187406f, -0.4073163f, 0.3288354f, 0.1790926f, -0.1171856f, 0.07031624f, 0.0399024f, -0.0210501f], 198.8854),
            ("GASSBig", [0.6214634f, -0.4125257f, 0.329569f, 0.1782178f, -0.1145641f, 0.07211548f, 0.03792826f, -0.02197583f], 253.727),
        ];

        AdaptiveFilter Make(string name) => name switch
        {
            "LMS" => new LmsFilter(taps, 0.01f),
            "NLMS" => new NlmsFilter(taps, 0.5f, 1e-3f),
            // the LMF / NLMF of this library minimize e^4 (the gradient carries the factor 4), padasip's e^4 / 4: the step differs by 4
            "LMF" => new LmfFilter(taps, 0.001f / 4),
            "NLMF" => new NlmfFilter(taps, 0.4f / 4, 1e-3f),
            "SignLMS" => new SignLmsFilter(taps, 0.002f),
            "NssLMS" => new NssLmsFilter(taps, 0.1f, 1e-3f),
            "RLS" => new RlsFilter(taps, 0.99f, 100f),
            "GNGD" => new GngdFilter(taps, 0.5f, 1f, 0.1f),
            // padasip: mu · lambda · alpha is the step of the weights and lambda^(-1/alpha) the kernel width
            "GMCC" => new GmccFilter(taps, 0.05f * 0.5f * 2f, 2f, (float)Math.Pow(1 / 0.5, 1 / 2.0)),
            "Llncosh" => new LlncoshFilter(taps, 0.01f, 3f),
            "APA" => new ApaFilter(taps, 4, 0.5f, 0.001f),
            "GASS" => new GassLmsFilter(taps, GassVariant.Mathews, 0.01f, 0.0001f, 0.9f, 1e-6f, 10f),
            "GASSBig" => new GassLmsFilter(taps, GassVariant.Mathews, 0.02f, 0.005f, 0.9f, 1e-6f, 10f),
            _ => throw new ArgumentException(name),
        };

        foreach (var (name, reference, sumY2) in references)
        {
            var filter = Make(name);
            double s2 = 0;
            for (var t = 0; t < length; t++)
            {
                var y = filter.Process(x[t], d[t]);
                s2 += (double)y * y;
            }

            var w = filter.Kernel;
            var scale = reference.Max(Math.Abs);
            var maxDiff = 0.0;
            for (var k = 0; k < taps; k++)
                maxDiff = Math.Max(maxDiff, Math.Abs(w[k] - reference[k]));

            Expect($"{name}: final weights equal those of padasip", maxDiff <= 2e-4 * scale, $"max|Δw| {maxDiff:E2} (scale {scale:F3})");
            Expect($"{name}: Σy² equals that of padasip", Math.Abs(s2 - sumY2) <= 1e-3 * sumY2, $"{s2:F4} vs {sumY2:F4}");
        }
    }

    // ---------------------------------------------------------------- NSS-LMS, OC-NLMS

    static void NssLms()
    {
        const int n = 8;
        var h = MakeSystem(n, 301);
        var (x, d) = MakeData(h, 20000, 302, 0);
        var filter = new NssLmsFilter(n, 0.05f, 1e-3f);
        Feed(filter, x, d);
        var err = WeightErrorDb(filter.Kernel, h);
        Expect("NSS-LMS identifies a system", err < -20, $"{err:F1} dB");

        // the update only moves the weights by ±mu / (eps + ‖x‖²) per sample: with a silent regressor and eps > 0 nothing happens
        var silent = new NssLmsFilter(n, 0.05f, 1e-3f);
        silent.Init(h);
        for (var t = 0; t < 100; t++)
            silent.Process(0f, 1f);
        ExpectClose("NSS-LMS keeps the weights on silence", silent.Kernel, h, 0);
    }

    static void OcNlmsOffset()
    {
        const int n = 8;
        var h = MakeSystem(n, 311);

        (double oc, double nlms) Run(double offset, int memory)
        {
            var rng = new Random(312);
            var x = new float[20000];
            for (var i = 0; i < x.Length; i++)
                x[i] = (float)(offset + Gauss(rng));
            var oc = new OcNlmsFilter(n, 0.5f, 1e-3f, memory);
            var plain = new NlmsFilter(n, 0.5f, 1e-3f);
            for (var t = 0; t < x.Length; t++)
            {
                double s = 0;
                for (var k = 0; k < n; k++)
                    if (t - k >= 0)
                        s += h[k] * x[t - k];
                var d = (float)(s + 0.001 * Gauss(rng));
                oc.Process(x[t], d);
                plain.Process(x[t], d);
            }
            return (WeightErrorDb(oc.Kernel, h), WeightErrorDb(plain.Kernel, h));
        }

        foreach (var memory in new[] { 20, 100 })
        {
            var (ocBig, nlmsBig) = Run(121, memory);
            Expect($"OC-NLMS (memory {memory}) is not disturbed by a DC offset of 121", ocBig < -50, $"{ocBig:F1} dB");
            Expect($"NLMS is disturbed by a DC offset of 121 (reference)", nlmsBig > -20, $"{nlmsBig:F1} dB");

            var (ocZero, nlmsZero) = Run(0, memory);
            Expect($"OC-NLMS (memory {memory}) equals NLMS without an offset", ocZero < -50 && nlmsZero < -50, $"{ocZero:F1} dB / {nlmsZero:F1} dB");
        }

        // the output of the filter contains the mean terms: for a system with an offset in the target a trained filter reproduces it
        {
            var rng = new Random(313);
            var oc = new OcNlmsFilter(n, 0.5f, 1e-3f, 100);
            var x = new float[20000];
            for (var i = 0; i < x.Length; i++)
                x[i] = (float)(5 + Gauss(rng));
            var late = 0.0;
            for (var t = 0; t < x.Length; t++)
            {
                double s = 50;
                for (var k = 0; k < n; k++)
                    if (t - k >= 0)
                        s += h[k] * x[t - k];
                var y = oc.Process(x[t], (float)s, out var e);
                if (t >= x.Length - 2000)
                    late += (double)e * e;
            }
            Expect("OC-NLMS learns a target with an offset", late / 2000 < 1e-3, $"mse {late / 2000:E2}");
        }
    }

    // ---------------------------------------------------------------- line enhancer, notch, sinusoid canceller

    static void LineEnhancer()
    {
        const int len = 60000;
        var rng = new Random(1);
        var clean = new float[len];
        var x = new float[len];
        for (var i = 0; i < len; i++)
        {
            clean[i] = (float)Math.Sin(2 * Math.PI * 0.1 * i);
            x[i] = clean[i] + (float)Gauss(rng);
        }

        var ale = new AdaptiveLineEnhancer(32, 1, 0.05f);
        var enhanced = new float[len];
        var residual = new float[len];
        ale.ProcessAll(x, enhanced, residual);

        double ps = 0, pe = 0, pi = 0;
        for (var i = len - 20000; i < len; i++)
        {
            ps += clean[i] * clean[i];
            pe += (enhanced[i] - clean[i]) * (enhanced[i] - clean[i]);
            pi += (x[i] - clean[i]) * (x[i] - clean[i]);
        }
        var snrIn = Db(ps / pi);
        var snrOut = Db(ps / pe);
        Expect("ALE improves the SNR of a sinusoid in noise", snrOut > snrIn + 8, $"{snrIn:F1} dB -> {snrOut:F1} dB");

        var worst = 0f;
        for (var i = 0; i < len; i++)
            worst = Math.Max(worst, Math.Abs(enhanced[i] + residual[i] - x[i]));
        Expect("ALE: enhanced + residual = input", worst < 1e-4f, $"{worst:E2}");

        Expect("ALE exposes its parameters", ale.Delay == 1 && ale.Filter.Order == 32);

        ale.Reset();
        Expect("ALE: Reset clears the history", ale.Process(1f) == 0f);

        // a custom predictor
        var custom = new AdaptiveLineEnhancer(new RlsFilter(16, 0.999f), 2);
        var customOut = new float[len];
        custom.ProcessAll(x, customOut);
        Expect("ALE accepts any adaptive filter", customOut.All(float.IsFinite) && custom.Delay == 2);

        ExpectThrows<ArgumentException>("ALE delay 0", () => new AdaptiveLineEnhancer(8, 0));
        ExpectThrows<ArgumentNullException>("ALE null filter", () => new AdaptiveLineEnhancer(null!, 1));
        ExpectThrows<ArgumentException>("ALE short output", () => new AdaptiveLineEnhancer(8).ProcessAll(new float[10], new float[9]));
        ExpectThrows<ArgumentException>("ALE short residual", () => new AdaptiveLineEnhancer(8).ProcessAll(new float[10], new float[10], new float[9]));
    }

    static void NotchFilter()
    {
        foreach (var f0 in new[] { 0.03, 0.1, 0.4 })
        {
            var rng = new Random(2);
            var anf = new AdaptiveNotchFilter(0.2, 0.95, 0.01);
            double pout = 0, pnoise = 0;
            var frequencyAfterStart = 0.0;
            const int len = 20000;
            for (var i = 0; i < len; i++)
            {
                var noise = 0.1 * Gauss(rng);
                var y = anf.Process((float)(Math.Sin(2 * Math.PI * f0 * i + 0.7) + noise));
                if (i == 2000)
                    frequencyAfterStart = anf.Frequency;
                if (i >= len - 5000)
                {
                    pout += y * y;
                    pnoise += noise * noise;
                }
            }
            Expect($"ANF locks to f = {f0} within 2000 samples", Math.Abs(frequencyAfterStart - f0) < 0.002, $"{frequencyAfterStart:F4}");
            Expect($"ANF removes the sinusoid f = {f0} (what is left is the noise)", Math.Abs(Db(pout / pnoise)) < 1.5, $"{Db(pout / pnoise):F1} dB");
        }

        {
            // tracking of a frequency step
            var rng = new Random(3);
            var anf = new AdaptiveNotchFilter(0.15, 0.95, 0.01);
            double phase = 0;
            var at = new Dictionary<int, double>();
            for (var i = 0; i < 45000; i++)
            {
                phase += 2 * Math.PI * (i < 20000 ? 0.10 : 0.12);
                anf.Process((float)(Math.Sin(phase) + 0.1 * Gauss(rng)));
                if (i is 19999 or 44999)
                    at[i] = anf.Frequency;
            }
            Expect("ANF tracks f = 0.10", Math.Abs(at[19999] - 0.10) < 0.002, $"{at[19999]:F4}");
            Expect("ANF follows a step to f = 0.12", Math.Abs(at[44999] - 0.12) < 0.002, $"{at[44999]:F4}");
        }

        {
            // a converged notch passes broadband noise with almost unit gain
            var rng = new Random(4);
            var anf = new AdaptiveNotchFilter(0.1, 0.95, 0.0001);
            double pin = 0, pout = 0;
            for (var i = 0; i < 60000; i++)
            {
                var v = (float)Gauss(rng);
                var y = anf.Process(v);
                if (i > 30000)
                {
                    pin += v * v;
                    pout += y * y;
                }
            }
            Expect("ANF broadband gain is close to 0 dB", Math.Abs(Db(pout / pin)) < 1, $"{Db(pout / pin):F2} dB");
        }

        {
            var a = new AdaptiveNotchFilter(0.2);
            var input = Enumerable.Range(0, 2000).Select(i => (float)Math.Sin(2 * Math.PI * 0.1 * i)).ToArray();
            var output = new float[input.Length];
            a.ProcessAll(input, output);
            var moved = a.Frequency;
            Expect("ANF moves from the starting frequency", Math.Abs(moved - 0.2) > 0.05, $"{moved:F4}");
            a.Reset();
            Expect("ANF Reset restores the starting frequency", Math.Abs(a.Frequency - 0.2) < 1e-12);
            Expect("ANF exposes the pole radius", Math.Abs(a.PoleRadius - 0.95) < 1e-12);

            var b = new AdaptiveNotchFilter(0.2);
            var again = new float[input.Length];
            b.ProcessAll(input, again);
            ExpectClose("ANF ProcessAll ≡ a sample loop, and Reset restarts from scratch", output, again, 0);
            var afterReset = new float[input.Length];
            a.ProcessAll(input, afterReset);
            ExpectClose("ANF after Reset gives the same output", output, afterReset, 0);
        }

        {
            // Started at the right frequency, the notch must not be thrown off by the first samples: the power estimate is not yet filled up then
            // (the start-up transient of the filter itself still moves the frequency by about 0.025, the first steps used to pin it at 0 or 0.5).
            var anf = new AdaptiveNotchFilter(0.1, 0.95, 0.01);
            var worst = 0.0;
            for (var i = 0; i < 2000; i++)
            {
                anf.Process((float)Math.Cos(2 * Math.PI * 0.1 * i));
                if (i >= 1)
                    worst = Math.Max(worst, Math.Abs(anf.Frequency - 0.1));
            }
            Expect("ANF started at the right frequency is not thrown off", worst < 0.05, $"max deviation {worst:F4}");
            Expect("ANF started at the right frequency stays there", Math.Abs(anf.Frequency - 0.1) < 0.002, $"{anf.Frequency:F4}");
        }

        ExpectThrows<ArgumentOutOfRangeException>("ANF frequency 0", () => new AdaptiveNotchFilter(0));
        ExpectThrows<ArgumentOutOfRangeException>("ANF frequency 0.5", () => new AdaptiveNotchFilter(0.5));
        ExpectThrows<ArgumentOutOfRangeException>("ANF radius 1", () => new AdaptiveNotchFilter(0.1, 1));
        ExpectThrows<ArgumentOutOfRangeException>("ANF mu 0", () => new AdaptiveNotchFilter(0.1, 0.95, 0));
        ExpectThrows<ArgumentOutOfRangeException>("ANF averaging 1", () => new AdaptiveNotchFilter(0.1, 0.95, 0.01, 1));
        ExpectThrows<ArgumentException>("ANF short output", () => new AdaptiveNotchFilter().ProcessAll(new float[4], new float[3]));
    }

    static void SinusoidalCanceller()
    {
        // 50 Hz hum at fs = 1000 Hz with two harmonics
        {
            var rng = new Random(5);
            const int len = 40000;
            var canceller = new SinusoidalInterferenceCanceller(0.05, 3, 0.02);
            double pres = 0, pnoise = 0;
            for (var i = 0; i < len; i++)
            {
                var noise = 0.1 * Gauss(rng);
                var hum = 5 * Math.Sin(2 * Math.PI * 0.05 * i + 0.3) + 2 * Math.Sin(2 * Math.PI * 0.10 * i + 1.1) + 1 * Math.Sin(2 * Math.PI * 0.15 * i - 0.4);
                var y = canceller.Process((float)(hum + noise), out var estimate);
                var residual = y - noise;
                if (i >= len - 10000)
                {
                    pres += residual * residual;
                    pnoise += noise * noise;
                }
                if (i == 100)
                    Expect("SIC: output = input - interference estimate", Math.Abs(y - ((float)(hum + noise) - estimate)) < 1e-5);
            }
            Expect("SIC removes the hum and its harmonics (residual far below the noise)", Db(pres / pnoise) < -15, $"{Db(pres / pnoise):F1} dB");
            Expect("SIC estimates the amplitude of the fundamental", Math.Abs(canceller.Amplitude(1) - 5) < 0.1, $"{canceller.Amplitude(1):F3}");
            Expect("SIC estimates the amplitude of the 2nd harmonic", Math.Abs(canceller.Amplitude(2) - 2) < 0.1, $"{canceller.Amplitude(2):F3}");
            Expect("SIC estimates the amplitude of the 3rd harmonic", Math.Abs(canceller.Amplitude(3) - 1) < 0.1, $"{canceller.Amplitude(3):F3}");
            Expect("SIC Harmonics", canceller.Harmonics == 3);
        }

        // width of the notch: -3 dB at about mu / (4 pi) from the centre
        double Gain(double offset)
        {
            var c = new SinusoidalInterferenceCanceller(0.05, 1, 0.02);
            double pin = 0, pout = 0;
            for (var i = 0; i < 60000; i++)
            {
                var v = (float)Math.Sin(2 * Math.PI * (0.05 + offset) * i);
                var y = c.Process(v);
                if (i >= 50000)
                {
                    pin += v * v;
                    pout += y * y;
                }
            }
            return Db(pout / pin);
        }
        Expect("SIC removes the tone at the centre of the notch", Gain(0) < -60, $"{Gain(0):F1} dB");
        var near = Gain(0.001);
        Expect("SIC notch is narrow (-5.5 dB at 0.001 cycles/sample from the centre)", near > -8 && near < -3, $"{near:F1} dB");
        var far = Gain(0.05);
        Expect("SIC does not touch the rest of the spectrum", Math.Abs(far) < 0.5, $"{far:F2} dB");

        {
            // the frequency can change while the filter runs
            var c = new SinusoidalInterferenceCanceller(0.05, 1, 0.05);
            double power = 0;
            for (var i = 0; i < 40000; i++)
            {
                if (i == 20000)
                    c.Frequency = 0.07;
                var f = i < 20000 ? 0.05 : 0.07;
                var y = c.Process((float)Math.Sin(2 * Math.PI * f * (i < 20000 ? i : i - 20000) + 0.4));
                if (i >= 39000)
                    power += y * y;
            }
            Expect("SIC follows a change of the frequency property", power / 1000 < 1e-3, $"{power / 1000:E2}");
            c.Reset();
            Expect("SIC Reset forgets the amplitudes", c.Amplitude(1) == 0);
        }

        {
            var input = Enumerable.Range(0, 1000).Select(i => (float)Math.Sin(0.3 * i)).ToArray();
            var a = new SinusoidalInterferenceCanceller(0.05, 2);
            var b = new SinusoidalInterferenceCanceller(0.05, 2);
            var output = new float[input.Length];
            a.ProcessAll(input, output);
            var expected = input.Select(v => b.Process(v)).ToArray();
            ExpectClose("SIC ProcessAll ≡ a sample loop", output, expected, 0);
        }

        ExpectThrows<ArgumentException>("SIC harmonics 0", () => new SinusoidalInterferenceCanceller(0.05, 0));
        ExpectThrows<ArgumentOutOfRangeException>("SIC frequency 0", () => new SinusoidalInterferenceCanceller(0));
        ExpectThrows<ArgumentOutOfRangeException>("SIC harmonic above Nyquist", () => new SinusoidalInterferenceCanceller(0.2, 3));
        ExpectThrows<ArgumentOutOfRangeException>("SIC mu 0", () => new SinusoidalInterferenceCanceller(0.05, 1, 0));
        ExpectThrows<ArgumentOutOfRangeException>("SIC Amplitude(0)", () => new SinusoidalInterferenceCanceller(0.05, 2).Amplitude(0));
        ExpectThrows<ArgumentOutOfRangeException>("SIC Amplitude(3)", () => new SinusoidalInterferenceCanceller(0.05, 2).Amplitude(3));
        ExpectThrows<ArgumentOutOfRangeException>("SIC Frequency setter", () => new SinusoidalInterferenceCanceller(0.05, 3).Frequency = 0.2);
        ExpectThrows<ArgumentException>("SIC short output", () => new SinusoidalInterferenceCanceller(0.05).ProcessAll(new float[4], new float[3]));
    }

    // ---------------------------------------------------------------- LSL lattice, sliding-window RLS

    static void LslLattice()
    {
        const int n = 8;
        foreach (var ar in new[] { 0.0, 0.9 })
        foreach (var lambda in new[] { 1.0f, 0.999f, 0.99f })
        {
            var h = MakeSystem(n, 321);
            var (x, d) = MakeData(h, 3000, 322, 1e-3, ar);
            var lsl = new LslLatticeFilter(n - 1, lambda, 1e-3f);
            var rls = new RlsFilter(n, lambda, 1e3f);

            double maxY = 0, scaleY = 0;
            for (var t = 0; t < x.Length; t++)
            {
                var yl = lsl.Process(x[t], d[t]);
                var yr = rls.Process(x[t], d[t]);
                if (t > 50)
                {
                    maxY = Math.Max(maxY, Math.Abs(yl - yr));
                    scaleY = Math.Max(scaleY, Math.Abs(yr));
                }
            }
            var name = $"λ={lambda}, AR({ar})";
            Expect($"LSL output ≡ RLS output ({name})", maxY / scaleY < 2e-5, $"{maxY / scaleY:E2}");

            var wl = lsl.GetKernel();
            ExpectClose($"LSL equivalent kernel ≡ RLS weights ({name})", wl, rls.Kernel, 1e-5f);

            var exact = DirectLs(x, d, n, x.Length - 1, lambda, 1e-3);
            var worst = 0.0;
            for (var k = 0; k < n; k++)
                worst = Math.Max(worst, Math.Abs(wl[k] - exact[k]));
            Expect($"LSL equivalent kernel ≡ the exact LS solution ({name})", worst < 1e-5, $"{worst:E2}");
        }

        {
            // gaps of silence are skipped like in RLS; the error output is the a priori error
            var h = MakeSystem(n, 323);
            var (x, d) = MakeData(h, 3000, 324, 1e-3, 0);
            for (var t = 1000; t < 1500; t++)
                x[t] = d[t] = 0;
            var lsl = new LslLatticeFilter(n - 1, 0.999f, 1e-3f);
            var rls = new RlsFilter(n, 0.999f, 1e3f);
            double maxY = 0, scaleY = 0;
            for (var t = 0; t < x.Length; t++)
            {
                var yl = lsl.Process(x[t], d[t], out var e);
                var yr = rls.Process(x[t], d[t]);
                if (t > 50)
                {
                    maxY = Math.Max(maxY, Math.Abs(yl - yr));
                    scaleY = Math.Max(scaleY, Math.Abs(yr));
                }
                if (Math.Abs(e - (d[t] - yl)) > 1e-5f)
                    Expect("LSL error = desired - output", false, $"t={t}");
            }
            Expect("LSL ≡ RLS across a gap of silence", maxY / scaleY < 1e-3, $"{maxY / scaleY:E2}");
            Expect("LSL stays finite across a gap of silence", lsl.GetKernel().All(float.IsFinite));

            lsl.Reset();
            Expect("LSL Reset restarts the filter", lsl.GetKernel().All(v => v == 0));
        }

        {
            // a constant or a noise-free sinusoid leaves the error energies of the higher stages to decay below the rounding noise of the input energy
            foreach (var kind in new[] { "constant", "tone" })
            {
                var lsl = new LslLatticeFilter(3, 0.99f);
                var finite = true;
                for (var t = 0; t < 30000; t++)
                {
                    var x = kind == "constant" ? 1f : (float)Math.Sin(0.3 * t);
                    finite &= float.IsFinite(lsl.Process(x, 2 * x));
                }

                var kernel = lsl.GetKernel();
                var omega = kind == "constant" ? 0.0 : 0.3;
                var response = System.Numerics.Complex.Zero;
                for (var i = 0; i < kernel.Length; i++)
                    response += kernel[i] * System.Numerics.Complex.Exp(new System.Numerics.Complex(0, -omega * i));
                Expect($"LSL stays finite on a {kind} input", finite && kernel.All(float.IsFinite));
                Expect($"LSL solves the {kind} input", Math.Abs(System.Numerics.Complex.Abs(response) - 2) < 1e-3, $"|H| = {System.Numerics.Complex.Abs(response):F4}");
            }

            // and it recovers when the input becomes broadband again
            var recovering = new LslLatticeFilter(7, 0.99f);
            for (var t = 0; t < 20000; t++)
            {
                var x = (float)Math.Sin(0.3 * t);
                recovering.Process(x, 2 * x);
            }
            var system = MakeSystem(8, 325);
            var (noise, target) = MakeData(system, 4000, 326, 0);
            double late = 0;
            for (var t = 0; t < noise.Length; t++)
            {
                var y = recovering.Process(noise[t], target[t]);
                if (t >= 3000)
                    late += Math.Pow(y - target[t], 2);
            }
            Expect("LSL recovers after a sinusoid", late / 1000 < 1e-6, $"mse {late / 1000:E2}");
        }

        ExpectThrows<ArgumentException>("LSL stages 0", () => new LslLatticeFilter(0));
        ExpectThrows<ArgumentException>("LSL lambda 1.1", () => new LslLatticeFilter(4, 1.1f));
        ExpectThrows<ArgumentException>("LSL lambda 0", () => new LslLatticeFilter(4, 0f));
        ExpectThrows<ArgumentException>("LSL initial energy 0", () => new LslLatticeFilter(4, 0.99f, 0f));
    }

    static void SlidingWindowRls()
    {
        foreach (var (n, window, ar) in new[] { (6, 20, 0.0), (6, 50, 0.9), (16, 64, 0.0), (8, 8, 0.0) })
        foreach (var refresh in new int?[] { null, 0 })
        {
            var h = MakeSystem(n, 331);
            var (x, d) = MakeData(h, 1500, 332, 0.01, ar);
            var filter = new SlidingWindowRlsFilter(n, window, 1e-3f, refresh);
            var worst = 0.0;
            for (var t = 0; t < x.Length; t++)
            {
                filter.Process(x[t], d[t]);
                if (t % 5 == 0 && t >= 3)
                {
                    var exact = DirectLs(x, d, n, t, 1.0, 1e-3, window);
                    var w = filter.Kernel;
                    double dw = 0, scale = 1;
                    for (var k = 0; k < n; k++)
                    {
                        dw = Math.Max(dw, Math.Abs(w[k] - exact[k]));
                        scale = Math.Max(scale, Math.Abs(exact[k]));
                    }
                    worst = Math.Max(worst, dw / scale);
                }
            }
            Expect($"SW-RLS ≡ exact LS over the window (N={n}, window {window}, AR {ar}, refresh {(refresh is null ? "default" : "never")})", worst < 1e-5, $"{worst:E2}");
        }

        {
            // no drift without the refresh: the downdate keeps the matrix accurate for a very long run
            const int n = 8;
            var h = MakeSystem(n, 333);
            var (x, d) = MakeData(h, 120000, 334, 0.01, 0);
            var filter = new SlidingWindowRlsFilter(n, 30, 1e-3f, 0);
            Feed(filter, x, d);
            var exact = DirectLs(x, d, n, x.Length - 1, 1.0, 1e-3, 30);
            var w = filter.Kernel;
            var worst = 0.0;
            for (var k = 0; k < n; k++)
                worst = Math.Max(worst, Math.Abs(w[k] - exact[k]));
            Expect("SW-RLS does not drift over 120000 samples without a refresh", worst < 1e-5, $"{worst:E2}");
        }

        {
            // the solution depends on the last <window> samples only: a changed system is tracked exactly after one window
            const int n = 8;
            var h1 = MakeSystem(n, 335);
            var h2 = MakeSystem(n, 336);
            var (x, _) = MakeData(h1, 3000, 337, 0);
            var d = new float[x.Length];
            for (var t = 0; t < x.Length; t++)
            {
                var h = t < 1500 ? h1 : h2;
                double s = 0;
                for (var k = 0; k < n; k++)
                    if (t - k >= 0)
                        s += h[k] * x[t - k];
                d[t] = (float)s;
            }
            var filter = new SlidingWindowRlsFilter(n, 50, 1e-6f);
            var rls = new RlsFilter(n, 0.98f, 1e6f);
            double swError = 0, rlsError = 0;
            for (var t = 0; t < 1500 + 70 + n; t++)
            {
                filter.Process(x[t], d[t]);
                rls.Process(x[t], d[t]);
            }
            swError = WeightErrorDb(filter.Kernel, h2);
            rlsError = WeightErrorDb(rls.Kernel, h2);
            Expect("SW-RLS tracks a changed system within one window", swError < -80, $"{swError:F1} dB (RLS λ=0.98: {rlsError:F1} dB)");
        }

        {
            // frozen filtering touches neither the weights nor the window
            const int n = 8;
            var h = MakeSystem(n, 338);
            var (x, d) = MakeData(h, 1000, 339, 0.01, 0);
            var plain = new SlidingWindowRlsFilter(n, 40);
            var interrupted = new SlidingWindowRlsFilter(n, 40);
            for (var t = 0; t < 500; t++)
            {
                plain.Process(x[t], d[t]);
                interrupted.Process(x[t], d[t]);
            }
            var before = interrupted.Kernel;
            interrupted.ProcessAllSamples(x.AsSpan(500, 100).ToArray());
            ExpectClose("SW-RLS: frozen filtering leaves the weights untouched", interrupted.Kernel, before, 0);
        }

        ExpectThrows<ArgumentException>("SW-RLS window shorter than order", () => new SlidingWindowRlsFilter(8, 7));
        ExpectThrows<ArgumentException>("SW-RLS delta 0", () => new SlidingWindowRlsFilter(8, 16, 0f));
        ExpectThrows<ArgumentException>("SW-RLS negative refresh interval", () => new SlidingWindowRlsFilter(8, 16, 1e-3f, -1));
        ExpectThrows<ArgumentException>("SW-RLS order 0", () => new SlidingWindowRlsFilter(0, 16));
    }

    // ---------------------------------------------------------------- complex-valued filters

    static void ComplexFilters()
    {
        const int n = 5, length = 6000;
        var rng = new Random(11);

        // real data: the complex filters are the real ones
        {
            var h = MakeSystem(n, 341);
            var (xr, dr) = MakeData(h, 3000, 342, 0.01);

            void Compare(string name, ComplexAdaptiveFilter complex, AdaptiveFilter real)
            {
                var maxY = 0.0;
                var maxImaginary = 0.0;
                for (var t = 0; t < xr.Length; t++)
                {
                    var yc = complex.Process(new ComplexFp32(xr[t], 0), new ComplexFp32(dr[t], 0));
                    var yr = real.Process(xr[t], dr[t]);
                    maxY = Math.Max(maxY, Math.Abs(yc.Real - yr));
                    maxImaginary = Math.Max(maxImaginary, Math.Abs(yc.Imaginary));
                }
                var w = real.Kernel;
                var maxW = 0.0;
                for (var k = 0; k < n; k++)
                    maxW = Math.Max(maxW, Math.Abs(complex.Weights[k].Real - w[k]) + Math.Abs(complex.Weights[k].Imaginary));
                Expect($"{name}: real data gives the real filter", maxY < 1e-5 && maxImaginary == 0 && maxW < 1e-5, $"max|Δy| {maxY:E2}, max|Im y| {maxImaginary:E2}, max|Δw| {maxW:E2}");
            }

            Compare("Complex LMS", new ComplexLmsFilter(n, 0.01f), new LmsFilter(n, 0.01f));
            Compare("Complex NLMS", new ComplexNlmsFilter(n, 0.5f, 1e-3f), new NlmsFilter(n, 0.5f, 1e-3f));
            Compare("Complex RLS", new ComplexRlsFilter(n, 0.99f, 100f), new RlsFilter(n, 0.99f, 100f));
        }

        var hTrue = new System.Numerics.Complex[n];
        for (var k = 0; k < n; k++)
            hTrue[k] = new System.Numerics.Complex(Gauss(rng), Gauss(rng)) / (1 + k);
        var x = new ComplexFp32[length];
        var d = new ComplexFp32[length];
        var xc = new System.Numerics.Complex[length];
        for (var t = 0; t < length; t++)
        {
            x[t] = new ComplexFp32((float)(0.7 * Gauss(rng)), (float)(0.7 * Gauss(rng)));
            xc[t] = ToComplex(x[t]);
        }
        for (var t = 0; t < length; t++)
        {
            System.Numerics.Complex s = 0;
            for (var k = 0; k < n && t - k >= 0; k++)
                s += hTrue[k] * xc[t - k];
            s += new System.Numerics.Complex(Gauss(rng), Gauss(rng)) * 0.01;
            d[t] = new ComplexFp32((float)s.Real, (float)s.Imaginary);
        }
        var dc = d.Select(ToComplex).ToArray();

        // textbook implementation in double precision with System.Numerics.Complex
        {
            const double mu = 0.02, muN = 0.6, eps = 1e-3;
            var hl = new System.Numerics.Complex[n];
            var hn = new System.Numerics.Complex[n];
            var lms = new ComplexLmsFilter(n, (float)mu);
            var nlms = new ComplexNlmsFilter(n, (float)muN, (float)eps);
            double maxYl = 0, maxYn = 0, rms = 0;
            for (var t = 0; t < length; t++)
            {
                System.Numerics.Complex yl = 0, yn = 0;
                var power = 0.0;
                for (var k = 0; k < n; k++)
                {
                    var xk = t - k >= 0 ? xc[t - k] : System.Numerics.Complex.Zero;
                    yl += hl[k] * xk;
                    yn += hn[k] * xk;
                    power += xk.Real * xk.Real + xk.Imaginary * xk.Imaginary;
                }
                var el = dc[t] - yl;
                var en = dc[t] - yn;
                for (var k = 0; k < n; k++)
                {
                    var xk = t - k >= 0 ? xc[t - k] : System.Numerics.Complex.Zero;
                    hl[k] += mu * el * System.Numerics.Complex.Conjugate(xk);
                    hn[k] += muN * en * System.Numerics.Complex.Conjugate(xk) / (eps + power);
                }
                maxYl = Math.Max(maxYl, System.Numerics.Complex.Abs(ToComplex(lms.Process(x[t], d[t])) - yl));
                maxYn = Math.Max(maxYn, System.Numerics.Complex.Abs(ToComplex(nlms.Process(x[t], d[t])) - yn));
                rms += dc[t].Real * dc[t].Real + dc[t].Imaginary * dc[t].Imaginary;
            }
            rms = Math.Sqrt(rms / length);
            Expect("Complex LMS ≡ textbook", maxYl / rms < 2e-5, $"{maxYl / rms:E2}");
            Expect("Complex NLMS ≡ textbook", maxYn / rms < 2e-5, $"{maxYn / rms:E2}");
        }

        // RLS ≡ the exact exponentially weighted LS solution (normal equations in double precision), at every step
        {
            const int taps = 4, steps = 120;
            const double lambda = 0.98, c0 = 100;
            var rls = new ComplexRlsFilter(taps, (float)lambda, (float)c0);
            var worst = 0.0;
            for (var t = 0; t < steps; t++)
            {
                rls.Process(x[t], d[t]);

                var matrix = new System.Numerics.Complex[taps, taps];
                var rhs = new System.Numerics.Complex[taps];
                for (var i = 0; i < taps; i++)
                    matrix[i, i] = Math.Pow(lambda, t + 1) / c0;
                for (var i = 0; i <= t; i++)
                {
                    var weight = Math.Pow(lambda, t - i);
                    var phi = new System.Numerics.Complex[taps];
                    for (var k = 0; k < taps; k++)
                        phi[k] = System.Numerics.Complex.Conjugate(i - k >= 0 ? xc[i - k] : System.Numerics.Complex.Zero);
                    for (var a = 0; a < taps; a++)
                    {
                        for (var b = 0; b < taps; b++)
                            matrix[a, b] += weight * phi[a] * System.Numerics.Complex.Conjugate(phi[b]);
                        rhs[a] += weight * phi[a] * dc[i];
                    }
                }
                var exact = SolveComplex(matrix, rhs);
                double num = 0, den = 0;
                for (var k = 0; k < taps; k++)
                {
                    num = Math.Max(num, System.Numerics.Complex.Abs(ToComplex(rls.Weights[k]) - exact[k]));
                    den = Math.Max(den, System.Numerics.Complex.Abs(exact[k]));
                }
                worst = Math.Max(worst, num / Math.Max(den, 1e-3));
            }
            Expect("Complex RLS ≡ the exact LS solution at every step", worst < 1e-5, $"{worst:E2}");
        }

        // identification of a complex channel
        {
            double Error(ComplexAdaptiveFilter f)
            {
                double e = 0, p = 0;
                for (var k = 0; k < n; k++)
                {
                    e += Math.Pow(System.Numerics.Complex.Abs(ToComplex(f.Weights[k]) - hTrue[k]), 2);
                    p += Math.Pow(System.Numerics.Complex.Abs(hTrue[k]), 2);
                }
                return Db(e / p);
            }
            var lms = new ComplexLmsFilter(n, 0.02f);
            var nlms = new ComplexNlmsFilter(n, 0.6f);
            var rls = new ComplexRlsFilter(n, 0.999f);
            var output = new ComplexFp32[length];
            var error = new ComplexFp32[length];
            lms.ProcessAll(x, d, output, error);
            nlms.ProcessAll(x, d, output, error);
            rls.ProcessAll(x, d, output, error);
            Expect("Complex LMS identifies a complex channel", Error(lms) < -45, $"{Error(lms):F1} dB");
            Expect("Complex NLMS identifies a complex channel", Error(nlms) < -35, $"{Error(nlms):F1} dB");
            Expect("Complex RLS identifies a complex channel", Error(rls) < -60, $"{Error(rls):F1} dB");

            var last = new ComplexFp32(output[^1].Real + error[^1].Real, output[^1].Imaginary + error[^1].Imaginary);
            Expect("Complex ProcessAll: error = desired - output", Math.Abs(last.Real - d[^1].Real) < 1e-4f && Math.Abs(last.Imaginary - d[^1].Imaginary) < 1e-4f);
        }

        // a rotation of the input and of the desired signal rotates the taps: this is what the conjugate in the update guarantees
        {
            const double theta = 0.7, phi = -1.9;
            var rx = new ComplexFp32[length];
            var rd = new ComplexFp32[length];
            for (var t = 0; t < length; t++)
            {
                var a = ToComplex(x[t]) * System.Numerics.Complex.FromPolarCoordinates(1, theta);
                var b = ToComplex(d[t]) * System.Numerics.Complex.FromPolarCoordinates(1, phi);
                rx[t] = new ComplexFp32((float)a.Real, (float)a.Imaginary);
                rd[t] = new ComplexFp32((float)b.Real, (float)b.Imaginary);
            }
            var rotation = System.Numerics.Complex.FromPolarCoordinates(1, phi - theta);

            (string, ComplexAdaptiveFilter, ComplexAdaptiveFilter)[] pairs =
            [
                ("LMS", new ComplexLmsFilter(n, 0.02f), new ComplexLmsFilter(n, 0.02f)),
                ("NLMS", new ComplexNlmsFilter(n, 0.6f), new ComplexNlmsFilter(n, 0.6f)),
                ("RLS", new ComplexRlsFilter(n, 0.999f), new ComplexRlsFilter(n, 0.999f)),
            ];
            foreach (var (name, plain, rotated) in pairs)
            {
                plain.ProcessAll(x, d, default);
                rotated.ProcessAll(rx, rd, default);
                var worst = 0.0;
                for (var k = 0; k < n; k++)
                    worst = Math.Max(worst, System.Numerics.Complex.Abs(ToComplex(rotated.Weights[k]) - ToComplex(plain.Weights[k]) * rotation));
                Expect($"Complex {name}: rotated signals give rotated taps", worst < 2e-5, $"{worst:E2}");
            }
        }

        // Init / Reset / frozen filtering
        {
            var f = new ComplexNlmsFilter(3);
            f.Init([new ComplexFp32(1, 2), new ComplexFp32(-1, 0.5f), new ComplexFp32(0, 0)]);
            var y = f.Process(new ComplexFp32(1, 0));
            Expect("Complex Init sets the taps (and Process(x) does not adapt)", y.Real == 1 && y.Imaginary == 2 && f.Weights[0].Real == 1 && f.Weights[0].Imaginary == 2);
            var y2 = f.Process(new ComplexFp32(0, 1));
            // x = [j, 1]: h0·j + h1·1 = (1+2j)j + (−1+0.5j) = (−2 + j) + (−1 + 0.5j)
            Expect("Complex frozen filtering uses the history", Math.Abs(y2.Real - (-3)) < 1e-6 && Math.Abs(y2.Imaginary - 1.5) < 1e-6, $"{y2.Real},{y2.Imaginary}");
            f.Reset();
            Expect("Complex Reset keeps the taps and clears the history", f.Weights[0].Real == 1 && f.Process(new ComplexFp32(1, 0)).Real == 1);
            Expect("Complex Order", f.Order == 3 && f.Weights.Length == 3);
        }

        // RLS with a gap of silence and with a pure tone stays finite
        {
            var rls = new ComplexRlsFilter(4, 0.99f);
            var sample = new ComplexFp32(0.3f, -0.2f);
            for (var t = 0; t < 2000; t++)
                rls.Process(sample, sample);
            for (var t = 0; t < 2000; t++)
                rls.Process(default, default);
            for (var t = 0; t < 500; t++)
                rls.Process(x[t], d[t]);
            var finite = true;
            for (var k = 0; k < 4; k++)
                finite &= float.IsFinite(rls.Weights[k].Real) && float.IsFinite(rls.Weights[k].Imaginary);
            Expect("Complex RLS stays finite on a constant input and on silence", finite);
        }

        ExpectThrows<ArgumentException>("Complex order 0", () => new ComplexLmsFilter(0));
        ExpectThrows<ArgumentException>("Complex LMS mu 0", () => new ComplexLmsFilter(4, 0f));
        ExpectThrows<ArgumentException>("Complex NLMS mu 0", () => new ComplexNlmsFilter(4, 0f));
        ExpectThrows<ArgumentException>("Complex NLMS negative eps", () => new ComplexNlmsFilter(4, 0.5f, -1f));
        ExpectThrows<ArgumentException>("Complex RLS lambda 1.1", () => new ComplexRlsFilter(4, 1.1f));
        ExpectThrows<ArgumentException>("Complex RLS init corr 0", () => new ComplexRlsFilter(4, 0.99f, 0f));
        ExpectThrows<ArgumentException>("Complex Init wrong length", () => new ComplexLmsFilter(4).Init(new ComplexFp32[3]));
        ExpectThrows<ArgumentException>("Complex ProcessAll length mismatch", () => new ComplexLmsFilter(4).ProcessAll(new ComplexFp32[10], new ComplexFp32[9], new ComplexFp32[10]));
        ExpectThrows<ArgumentException>("Complex ProcessAll short output", () => new ComplexLmsFilter(4).ProcessAll(new ComplexFp32[10], new ComplexFp32[10], new ComplexFp32[9]));
        ExpectThrows<ArgumentException>("Complex ProcessAll short error", () => new ComplexLmsFilter(4).ProcessAll(new ComplexFp32[10], new ComplexFp32[10], new ComplexFp32[10], new ComplexFp32[9]));
    }

    // ---------------------------------------------------------------- multichannel NLMS

    static void MultichannelNlms()
    {
        var rng = new Random(5);

        // one channel is the NLMS filter
        {
            const int n = 6;
            var h = MakeSystem(n, 351);
            var (x, d) = MakeData(h, 3000, 352, 0.01);
            var multi = new MultichannelNlmsFilter(1, n, 0.5f, 1e-3f);
            var plain = new NlmsFilter(n, 0.5f, 1e-3f);
            var frame = new float[1];
            double maxY = 0;
            for (var t = 0; t < x.Length; t++)
            {
                frame[0] = x[t];
                maxY = Math.Max(maxY, Math.Abs(multi.Process(frame, d[t]) - plain.Process(x[t], d[t])));
            }
            Expect("Multichannel NLMS with one channel ≡ NLMS (output)", maxY < 1e-5, $"{maxY:E2}");
            ExpectClose("Multichannel NLMS with one channel ≡ NLMS (weights)", multi.GetWeights(0).ToArray(), plain.Kernel, 1e-5f);
        }

        {
            const int channels = 3, n = 6, length = 30000;
            var scale = new[] { 1.0, 0.3, 2.0 };
            var inputs = new float[channels][];
            var h = new double[channels, n];
            for (var c = 0; c < channels; c++)
            {
                inputs[c] = new float[length];
                for (var t = 0; t < length; t++)
                    inputs[c][t] = (float)(scale[c] * Gauss(rng));
                for (var k = 0; k < n; k++)
                    h[c, k] = Gauss(rng) / (1 + k);
            }
            var d = new float[length];
            for (var t = 0; t < length; t++)
            {
                double s = 0;
                for (var c = 0; c < channels; c++)
                    for (var k = 0; k < n && t - k >= 0; k++)
                        s += h[c, k] * inputs[c][t - k];
                d[t] = (float)s;
            }

            var filter = new MultichannelNlmsFilter(channels, n, 0.5f, 1e-3f);
            var y = new float[length];
            var e = new float[length];
            filter.ProcessAll(inputs, d, y, e);

            // textbook NLMS on the stacked regressor
            var w = new double[channels * n];
            double maxDy = 0, rms = 0;
            for (var t = 0; t < length; t++)
            {
                var regressor = new double[channels * n];
                for (var c = 0; c < channels; c++)
                    for (var k = 0; k < n; k++)
                        regressor[c * n + k] = t - k >= 0 ? inputs[c][t - k] : 0;
                double yy = 0, power = 0;
                for (var i = 0; i < regressor.Length; i++)
                {
                    yy += w[i] * regressor[i];
                    power += regressor[i] * regressor[i];
                }
                var error = d[t] - yy;
                for (var i = 0; i < regressor.Length; i++)
                    w[i] += 0.5 * error * regressor[i] / (1e-3 + power);
                maxDy = Math.Max(maxDy, Math.Abs(yy - y[t]));
                rms += d[t] * d[t];
            }
            Expect("Multichannel NLMS ≡ the textbook algorithm on the stacked regressor", maxDy / Math.Sqrt(rms / length) < 2e-5, $"{maxDy / Math.Sqrt(rms / length):E2}");

            double err = 0, norm = 0, late = 0, latePower = 0;
            for (var c = 0; c < channels; c++)
                for (var k = 0; k < n; k++)
                {
                    err += Math.Pow(filter.GetWeights(c)[k] - h[c, k], 2);
                    norm += h[c, k] * h[c, k];
                }
            for (var t = length - 2000; t < length; t++)
            {
                late += e[t] * e[t];
                latePower += d[t] * d[t];
            }
            Expect("Multichannel NLMS identifies a 3-input system", Db(err / norm) < -80 && Db(late / latePower) < -80, $"{Db(err / norm):F1} dB, {Db(late / latePower):F1} dB");

            var snapshot = filter.GetWeights(1).ToArray();
            filter.Process(new float[] { 1, 2, 3 });
            Expect("Multichannel NLMS: frozen filtering leaves the taps untouched", snapshot.SequenceEqual(filter.GetWeights(1).ToArray()));

            filter.Reset();
            Expect("Multichannel NLMS: Reset keeps the taps and clears the history", snapshot.SequenceEqual(filter.GetWeights(1).ToArray()) && filter.Process(new float[] { 0, 0, 0 }) == 0);
            Expect("Multichannel NLMS properties", filter.Channels == channels && filter.Order == n);

            var initial = Enumerable.Range(0, channels * n).Select(i => (float)i).ToArray();
            filter.Init(initial);
            Expect("Multichannel NLMS Init", filter.GetWeights(2)[0] == 12f && filter.GetWeights(0)[5] == 5f);
        }

        ExpectThrows<ArgumentException>("Multichannel channels 0", () => new MultichannelNlmsFilter(0, 4));
        ExpectThrows<ArgumentException>("Multichannel order 0", () => new MultichannelNlmsFilter(2, 0));
        ExpectThrows<ArgumentException>("Multichannel mu 0", () => new MultichannelNlmsFilter(2, 4, 0f));
        ExpectThrows<ArgumentException>("Multichannel negative eps", () => new MultichannelNlmsFilter(2, 4, 0.5f, -1f));
        ExpectThrows<ArgumentException>("Multichannel wrong frame size", () => new MultichannelNlmsFilter(2, 4).Process(new float[3], 1f));
        ExpectThrows<ArgumentException>("Multichannel wrong frame size (frozen)", () => new MultichannelNlmsFilter(2, 4).Process(new float[3]));
        ExpectThrows<ArgumentOutOfRangeException>("Multichannel GetWeights(-1)", () => new MultichannelNlmsFilter(2, 4).GetWeights(-1));
        ExpectThrows<ArgumentOutOfRangeException>("Multichannel GetWeights(2)", () => new MultichannelNlmsFilter(2, 4).GetWeights(2));
        ExpectThrows<ArgumentException>("Multichannel Init wrong length", () => new MultichannelNlmsFilter(2, 4).Init(new float[7]));
        ExpectThrows<ArgumentException>("Multichannel ProcessAll wrong number of inputs", () => new MultichannelNlmsFilter(2, 4).ProcessAll(new float[1][] { new float[5] }, new float[5], new float[5]));
        ExpectThrows<ArgumentException>("Multichannel ProcessAll length mismatch", () => new MultichannelNlmsFilter(2, 4).ProcessAll([new float[5], new float[4]], new float[5], new float[5]));
        ExpectThrows<ArgumentException>("Multichannel ProcessAll short output", () => new MultichannelNlmsFilter(2, 4).ProcessAll([new float[5], new float[5]], new float[5], new float[4]));
        ExpectThrows<ArgumentException>("Multichannel ProcessAll short error", () => new MultichannelNlmsFilter(2, 4).ProcessAll([new float[5], new float[5]], new float[5], new float[5], new float[4]));
        ExpectThrows<ArgumentNullException>("Multichannel ProcessAll null", () => new MultichannelNlmsFilter(2, 4).ProcessAll(null!, new float[5], new float[5]));
    }

    // ---------------------------------------------------------------- Volterra NLMS

    static void VolterraNlms()
    {
        const int n1 = 8, m = 4, length = 40000;
        var rng = new Random(8);

        var reference = new VolterraNlmsFilter(n1, m);
        var h1 = new double[n1];
        for (var k = 0; k < n1; k++)
            h1[k] = Gauss(rng) * 0.6 / (1 + k);
        var q = new double[m * (m + 1) / 2];
        for (var i = 0; i < m; i++)
            for (var j = i; j < m; j++)
                q[reference.QuadraticIndex(i, j)] = Gauss(rng) * 0.5 / (1 + i + j);

        float[] Respond(float[] input)
        {
            var output = new float[input.Length];
            for (var t = 0; t < input.Length; t++)
            {
                double s = 0;
                for (var k = 0; k < n1; k++)
                    s += h1[k] * (t - k >= 0 ? input[t - k] : 0);
                for (var i = 0; i < m; i++)
                    for (var j = i; j < m; j++)
                        s += q[reference.QuadraticIndex(i, j)] * (t - i >= 0 ? input[t - i] : 0) * (t - j >= 0 ? input[t - j] : 0);
                output[t] = (float)s;
            }
            return output;
        }

        var x = new float[length];
        for (var t = 0; t < length; t++)
            x[t] = (float)(0.8 * Gauss(rng));
        var d = Respond(x);

        var filter = new VolterraNlmsFilter(n1, m, 0.4f, 0.4f, 1e-3f);
        var linear = new NlmsFilter(n1, 0.8f, 1e-3f);
        var y = new float[length];
        var e = new float[length];
        var el = new float[length];
        filter.ProcessAll(x, d, y, e);
        linear.ProcessAll(x, d, new float[length], el);

        double late = 0, lateLinear = 0, latePower = 0;
        for (var t = length - 4000; t < length; t++)
        {
            late += e[t] * e[t];
            lateLinear += el[t] * el[t];
            latePower += d[t] * d[t];
        }
        Expect("Volterra NLMS learns a nonlinear system", Db(late / latePower) < -80, $"{Db(late / latePower):F1} dB");
        Expect("a linear NLMS cannot (reference)", Db(lateLinear / latePower) > -10, $"{Db(lateLinear / latePower):F1} dB");

        var kernel = filter.Kernel;
        double we = 0, wp = 0;
        for (var k = 0; k < n1; k++)
        {
            we += Math.Pow(kernel[k] - h1[k], 2);
            wp += h1[k] * h1[k];
        }
        var quadratic = filter.QuadraticKernel;
        double qe = 0, qp = 0;
        for (var i = 0; i < q.Length; i++)
        {
            qe += Math.Pow(quadratic[i] - q[i], 2);
            qp += q[i] * q[i];
        }
        Expect("Volterra NLMS finds the linear coefficients", Db(we / wp) < -80, $"{Db(we / wp):F1} dB");
        Expect("Volterra NLMS finds the quadratic coefficients", Db(qe / qp) < -80, $"{Db(qe / qp):F1} dB");

        // the textbook algorithm
        {
            var w1 = new double[n1];
            var w2 = new double[q.Length];
            double maxDy = 0, rms = 0;
            for (var t = 0; t < length; t++)
            {
                var regressor = new double[n1];
                for (var k = 0; k < n1; k++)
                    regressor[k] = t - k >= 0 ? x[t - k] : 0;
                var products = new double[q.Length];
                double yy = 0;
                for (var k = 0; k < n1; k++)
                    yy += w1[k] * regressor[k];
                for (var i = 0; i < m; i++)
                    for (var j = i; j < m; j++)
                    {
                        var index = reference.QuadraticIndex(i, j);
                        products[index] = regressor[i] * regressor[j];
                        yy += w2[index] * products[index];
                    }
                var error = d[t] - yy;
                double p1 = 0, p2 = 0;
                for (var k = 0; k < n1; k++)
                    p1 += regressor[k] * regressor[k];
                for (var i = 0; i < products.Length; i++)
                    p2 += products[i] * products[i];
                for (var k = 0; k < n1; k++)
                    w1[k] += 0.4 * error * regressor[k] / (1e-3 + p1);
                for (var i = 0; i < products.Length; i++)
                    w2[i] += 0.4 * error * products[i] / (1e-3 + p2);
                maxDy = Math.Max(maxDy, Math.Abs(yy - y[t]));
                rms += d[t] * d[t];
            }
            Expect("Volterra NLMS ≡ the textbook algorithm", maxDy / Math.Sqrt(rms / length) < 2e-5, $"{maxDy / Math.Sqrt(rms / length):E2}");
        }

        // frozen filtering reproduces the system, also through ApplyTo
        {
            var test = new float[3000];
            for (var t = 0; t < test.Length; t++)
                test[t] = (float)(0.8 * Gauss(rng));
            var truth = Respond(test);

            filter.Reset();
            var frozen = filter.ProcessAllSamples(test);
            Expect("Volterra frozen filtering: output length", frozen.Length == test.Length + n1 - 1);
            double num = 0, den = 0;
            for (var t = 0; t < test.Length; t++)
            {
                num += Math.Pow(frozen[t] - truth[t], 2);
                den += truth[t] * truth[t];
            }
            Expect("Volterra frozen filter reproduces the system on new data", Db(num / den) < -80, $"{Db(num / den):F1} dB");

            filter.Reset();
            var viaApply = filter.ApplyTo(Signal.FromCopy(test, 1)).Samples.ToArray();
            ExpectClose("Volterra ApplyTo ≡ the sample loop", viaApply, frozen, 0);
        }

        // a long linear part would send ApplyTo through overlap-save, which would drop the quadratic part
        {
            var long1 = new VolterraNlmsFilter(80, 3);
            long1.InitQuadratic([1f, 0f, 0f, 1f, 0f, 1f]);
            var input = new float[500];
            for (var t = 0; t < input.Length; t++)
                input[t] = (float)Gauss(rng);
            var viaApply = long1.ApplyTo(Signal.FromCopy(input, 1)).Samples.ToArray();
            long1.Reset();
            var viaLoop = long1.ProcessAllSamples(input);
            ExpectClose("Volterra ApplyTo with a long linear part keeps the quadratic part", viaApply, viaLoop, 0);
            Expect("Volterra quadratic part is in the output", Math.Abs(viaLoop[10] - (input[10] * input[10] + input[9] * input[9] + input[8] * input[8])) < 1e-4);
        }

        // Reset / Init / InitQuadratic
        {
            var f = new VolterraNlmsFilter(4, 2);
            f.InitQuadratic([1f, 2f, 3f]);
            f.Reset();
            Expect("Volterra Reset keeps the quadratic coefficients", f.QuadraticKernel.SequenceEqual([1f, 2f, 3f]));
            f.Init([0f, 0f, 0f, 0f]);
            Expect("Volterra Init(linear) keeps the quadratic coefficients", f.QuadraticKernel.SequenceEqual([1f, 2f, 3f]));
            // y(n) = 1·x² + 2·x·x(n-1) + 3·x(n-1)²
            f.Process(2f);
            var y2 = f.Process(3f);
            Expect("Volterra quadratic form", Math.Abs(y2 - (1 * 9 + 2 * 3 * 2 + 3 * 4)) < 1e-5, $"{y2}");
            Expect("Volterra QuadraticKernel is a copy", !ReferenceEquals(f.QuadraticKernel, f.QuadraticKernel));
            Expect("Volterra properties", f.QuadraticOrder == 2 && f.Order == 4);
        }

        {
            var f = new VolterraNlmsFilter(5, 3);
            var seen = new HashSet<int>();
            var layout = true;
            var expected = 0;
            for (var i = 0; i < 3; i++)
                for (var j = i; j < 3; j++)
                {
                    layout &= f.QuadraticIndex(i, j) == expected && f.QuadraticIndex(j, i) == expected;
                    seen.Add(expected++);
                }
            Expect("Volterra QuadraticIndex: row by row, symmetric", layout && seen.Count == f.QuadraticKernel.Length);
            ExpectThrows<ArgumentOutOfRangeException>("Volterra QuadraticIndex out of range", () => f.QuadraticIndex(3, 0));
            ExpectThrows<ArgumentOutOfRangeException>("Volterra QuadraticIndex negative", () => f.QuadraticIndex(0, -1));
        }

        ExpectThrows<ArgumentException>("Volterra order 0", () => new VolterraNlmsFilter(0, 1));
        ExpectThrows<ArgumentException>("Volterra quadratic order 0", () => new VolterraNlmsFilter(4, 0));
        ExpectThrows<ArgumentException>("Volterra quadratic order above the linear order", () => new VolterraNlmsFilter(4, 5));
        ExpectThrows<ArgumentException>("Volterra mu 0", () => new VolterraNlmsFilter(4, 2, 0f));
        ExpectThrows<ArgumentException>("Volterra quadratic mu 0", () => new VolterraNlmsFilter(4, 2, 0.5f, 0f));
        ExpectThrows<ArgumentException>("Volterra mu + quadratic mu ≥ 2", () => new VolterraNlmsFilter(4, 2, 1.2f, 0.8f));
        ExpectThrows<ArgumentException>("Volterra negative eps", () => new VolterraNlmsFilter(4, 2, 0.3f, 0.3f, -1f));
        ExpectThrows<ArgumentException>("Volterra InitQuadratic wrong length", () => new VolterraNlmsFilter(4, 2).InitQuadratic(new float[4]));
    }

    // ---------------------------------------------------------------- kernel LMS

    static void KernelLms()
    {
        const int length = 4000;
        // Hénon map: x(n+1) = 1 - 1.4 x(n)² + 0.3 x(n-1)
        var x = new float[length + 1];
        x[0] = x[1] = 0.1f;
        for (var n = 1; n < length; n++)
            x[n + 1] = (float)(1 - 1.4 * x[n] * x[n] + 0.3 * x[n - 1]);

        {
            var klms = new KernelLmsFilter(2, 0.5f, 0.5f, 0.1f);
            var linear = new NlmsFilter(2, 0.5f, 1e-3f);
            double train = 0, trainLinear = 0, trainPower = 0;
            const int half = 3000;
            for (var n = 0; n < half; n++)
            {
                var y = klms.Process(x[n], x[n + 1]);
                var yl = linear.Process(x[n], x[n + 1]);
                if (n >= half - 500)
                {
                    train += Math.Pow(x[n + 1] - y, 2);
                    trainLinear += Math.Pow(x[n + 1] - yl, 2);
                    trainPower += x[n + 1] * x[n + 1];
                }
            }
            var size = klms.DictionarySize;

            // the frozen dictionary predicts the rest of the series
            double test = 0, testPower = 0;
            for (var n = half; n < length; n++)
            {
                var y = klms.Process(x[n]);
                test += Math.Pow(x[n + 1] - y, 2);
                testPower += x[n + 1] * x[n + 1];
            }
            Expect("QKLMS learns the one-step prediction of the Hénon map", Db(train / trainPower) < -20, $"{Db(train / trainPower):F1} dB");
            Expect("QKLMS generalizes to the unseen part of the series", Db(test / testPower) < -20, $"{Db(test / testPower):F1} dB");
            Expect("a linear predictor cannot (reference)", Db(trainLinear / trainPower) > -3, $"{Db(trainLinear / trainPower):F1} dB");
            Expect("QKLMS quantization keeps the dictionary small", size > 10 && size < 400, $"{size}");
            Expect("frozen prediction does not change the dictionary", klms.DictionarySize == size);

            klms.Reset();
            Expect("QKLMS Reset keeps the dictionary", klms.DictionarySize == size);
            klms.ClearDictionary();
            Expect("QKLMS ClearDictionary forgets everything", klms.DictionarySize == 0 && klms.Process(0.3f) == 0f);
        }

        {
            // a non-finite sample must neither throw on a full dictionary nor poison the dictionary
            var full = new KernelLmsFilter(1, 0.5f, 0.5f, 0.01f, 3);
            for (var i = 0; i < 10; i++)
                full.Process(i, i);
            var threw = false;
            try
            {
                full.Process(float.NaN, 0f);
                full.Process(1f, 1f);
            }
            catch (Exception)
            {
                threw = true;
            }
            Expect("QKLMS: a NaN sample on a full dictionary does not throw", !threw);
            Expect("QKLMS: the output is finite after a NaN sample", float.IsFinite(full.Process(2f)));

            var open = new KernelLmsFilter(2);
            for (var i = 0; i < 50; i++)
                open.Process((float)Math.Sin(i), (float)Math.Cos(i));
            var size = open.DictionarySize;
            open.Process(float.NaN, 1f);
            open.Process(float.PositiveInfinity, 1f);
            open.Process(0.3f, float.NaN);
            for (var i = 0; i < 5; i++)
                open.Process(0.4f + i * 0.1f, 1f);
            Expect("QKLMS: non-finite samples do not enter the dictionary", float.IsFinite(open.Process(0.5f)) && open.DictionarySize >= size);
        }

        {
            // the kernel and the update rule on a hand-made example: one center at u = 1 with the coefficient mu · d = 0.25
            var klms = new KernelLmsFilter(1, 0.5f, 0.5f, 0.1f);
            var first = klms.Process(1f, 0.5f);
            var apart = klms.Process(1.5f);
            Expect("QKLMS: the first output is zero and creates a center", first == 0f && klms.DictionarySize == 1);
            // 0.25 · exp(−0.5² / (2 · 0.5²))
            Expect("QKLMS: Gaussian kernel", Math.Abs(apart - 0.25 * Math.Exp(-0.5)) < 1e-6, $"{apart}");
            var merged = klms.Process(1.02f, 1f);
            // the input is closer than the quantization size to the center: its coefficient is updated, no center is added
            Expect("QKLMS: quantization merges a close input", klms.DictionarySize == 1);
            var updated = klms.Process(1f);
            var prior = 0.25 * Math.Exp(-0.02 * 0.02 / (2 * 0.25));
            var expected = 0.25 + 0.5 * (1 - prior);
            Expect("QKLMS: the coefficient of the nearest center follows the LMS rule", Math.Abs(merged - prior) < 1e-6 && Math.Abs(updated - expected) < 1e-5, $"{merged} {updated} {expected}");
        }

        {
            // static nonlinearity y = sin(2u)
            var rng = new Random(2);
            var klms = new KernelLmsFilter(1, 0.5f, 0.4f, 0.05f);
            double e = 0, p = 0;
            for (var n = 0; n < 5000; n++)
            {
                var u = (float)(rng.NextDouble() * 4 - 2);
                var target = (float)Math.Sin(2 * u);
                var y = klms.Process(u, target, out var error);
                if (Math.Abs(error - (target - y)) > 1e-6f)
                    Expect("QKLMS error = desired - output", false);
                if (n >= 4000)
                {
                    e += Math.Pow(target - y, 2);
                    p += target * target;
                }
            }
            Expect("QKLMS learns a static nonlinearity", Db(e / p) < -30, $"{Db(e / p):F1} dB");
        }

        {
            // the dictionary stops growing at its limit
            var klms = new KernelLmsFilter(2, 0.5f, 0.5f, 0f, 100);
            for (var n = 0; n < 1000; n++)
                klms.Process(x[n], x[n + 1]);
            Expect("QKLMS dictionary limit", klms.DictionarySize == 100 && klms.MaxDictionarySize == 100, $"{klms.DictionarySize}");
            Expect("QKLMS Order", klms.Order == 2);
        }

        {
            // ProcessAll ≡ a sample loop
            var a = new KernelLmsFilter(2);
            var b = new KernelLmsFilter(2);
            var output = new float[500];
            var error = new float[500];
            a.ProcessAll(x.AsSpan(0, 500), x.AsSpan(1, 500), output, error);
            var expectedOutput = new float[500];
            var expectedError = new float[500];
            for (var n = 0; n < 500; n++)
                expectedOutput[n] = b.Process(x[n], x[n + 1], out expectedError[n]);
            ExpectClose("QKLMS ProcessAll output", output, expectedOutput, 0);
            ExpectClose("QKLMS ProcessAll error", error, expectedError, 0);
        }

        ExpectThrows<ArgumentException>("QKLMS order 0", () => new KernelLmsFilter(0));
        ExpectThrows<ArgumentException>("QKLMS mu 0", () => new KernelLmsFilter(2, 0f));
        ExpectThrows<ArgumentException>("QKLMS kernel width 0", () => new KernelLmsFilter(2, 0.5f, 0f));
        ExpectThrows<ArgumentOutOfRangeException>("QKLMS negative quantization", () => new KernelLmsFilter(2, 0.5f, 1f, -0.1f));
        ExpectThrows<ArgumentException>("QKLMS dictionary size 0", () => new KernelLmsFilter(2, 0.5f, 1f, 0.1f, 0));
        ExpectThrows<ArgumentException>("QKLMS ProcessAll length mismatch", () => new KernelLmsFilter(2).ProcessAll(new float[5], new float[4], new float[5]));
        ExpectThrows<ArgumentException>("QKLMS ProcessAll short output", () => new KernelLmsFilter(2).ProcessAll(new float[5], new float[5], new float[4]));
        ExpectThrows<ArgumentException>("QKLMS ProcessAll short error", () => new KernelLmsFilter(2).ProcessAll(new float[5], new float[5], new float[5], new float[4]));
    }

    // ---------------------------------------------------------------- buffers that overlap

    static void InPlaceBuffers()
    {
        // ProcessAll with the output written over the desired signal: the error is still desired - output
        const int length = 400;
        var h = MakeSystem(6, 361);
        var (x, d) = MakeData(h, length, 362, 0.01);

        {
            var inPlace = d.ToArray();
            var error = new float[length];
            new LmsFilter(6, 0.01f).ProcessAll(x, inPlace, inPlace, error);
            var expectedOutput = new float[length];
            var expectedError = new float[length];
            new LmsFilter(6, 0.01f).ProcessAll(x, d, expectedOutput, expectedError);
            ExpectClose("ProcessAll with output = desired: output", inPlace, expectedOutput, 0);
            ExpectClose("ProcessAll with output = desired: error", error, expectedError, 0);
        }

        {
            var other = x.Reverse().ToArray();
            var inPlace = d.ToArray();
            var error = new float[length];
            new MultichannelNlmsFilter(2, 6).ProcessAll([x, other], inPlace, inPlace, error);
            var expectedOutput = new float[length];
            var expectedError = new float[length];
            new MultichannelNlmsFilter(2, 6).ProcessAll([x, other], d, expectedOutput, expectedError);
            ExpectClose("Multichannel ProcessAll with output = desired: output", inPlace, expectedOutput, 0);
            ExpectClose("Multichannel ProcessAll with output = desired: error", error, expectedError, 0);
        }

        {
            var cx = x.Select((v, i) => new ComplexFp32(v, x[(i + 3) % length])).ToArray();
            var cd = d.Select(v => new ComplexFp32(v, -v)).ToArray();
            var inPlace = cd.ToArray();
            var error = new ComplexFp32[length];
            new ComplexNlmsFilter(6).ProcessAll(cx, inPlace, inPlace, error);
            var expectedOutput = new ComplexFp32[length];
            var expectedError = new ComplexFp32[length];
            new ComplexNlmsFilter(6).ProcessAll(cx, cd, expectedOutput, expectedError);
            Expect("Complex ProcessAll with output = desired: output", inPlace.SequenceEqual(expectedOutput));
            Expect("Complex ProcessAll with output = desired: error", error.SequenceEqual(expectedError));
        }

        {
            var inPlace = d.ToArray();
            var error = new float[length];
            new KernelLmsFilter(2).ProcessAll(x, inPlace, inPlace, error);
            var expectedOutput = new float[length];
            var expectedError = new float[length];
            new KernelLmsFilter(2).ProcessAll(x, d, expectedOutput, expectedError);
            ExpectClose("QKLMS ProcessAll with output = desired: output", inPlace, expectedOutput, 0);
            ExpectClose("QKLMS ProcessAll with output = desired: error", error, expectedError, 0);
        }
    }

    // ---------------------------------------------------------------- argument validation of the AdaptiveFilter-derived classes

    static void ArgumentValidation()
    {
        ExpectThrows<ArgumentException>("NSS-LMS mu 0", () => new NssLmsFilter(4, 0f));
        ExpectThrows<ArgumentException>("NSS-LMS negative eps", () => new NssLmsFilter(4, 0.1f, -1f));
        ExpectThrows<ArgumentException>("NSS-LMS order 0", () => new NssLmsFilter(0));
        ExpectThrows<ArgumentException>("OC-NLMS mu 0", () => new OcNlmsFilter(4, 0f));
        ExpectThrows<ArgumentException>("OC-NLMS negative eps", () => new OcNlmsFilter(4, 0.1f, -1f));
        ExpectThrows<ArgumentException>("OC-NLMS memory 1", () => new OcNlmsFilter(4, 0.1f, 1f, 1));
        ExpectThrows<ArgumentException>("OC-NLMS order 0", () => new OcNlmsFilter(0));
    }

    // ---------------------------------------------------------------- helpers

    static System.Numerics.Complex ToComplex(ComplexFp32 z) => new(z.Real, z.Imaginary);

    static System.Numerics.Complex[] SolveComplex(System.Numerics.Complex[,] matrix, System.Numerics.Complex[] rhs)
    {
        var n = rhs.Length;
        var a = (System.Numerics.Complex[,])matrix.Clone();
        var b = (System.Numerics.Complex[])rhs.Clone();
        for (var c = 0; c < n; c++)
        {
            var pivot = c;
            for (var r = c + 1; r < n; r++)
                if (System.Numerics.Complex.Abs(a[r, c]) > System.Numerics.Complex.Abs(a[pivot, c]))
                    pivot = r;
            if (pivot != c)
            {
                for (var k = 0; k < n; k++)
                    (a[c, k], a[pivot, k]) = (a[pivot, k], a[c, k]);
                (b[c], b[pivot]) = (b[pivot], b[c]);
            }
            for (var r = c + 1; r < n; r++)
            {
                var factor = a[r, c] / a[c, c];
                for (var k = c; k < n; k++)
                    a[r, k] -= factor * a[c, k];
                b[r] -= factor * b[c];
            }
        }
        var x = new System.Numerics.Complex[n];
        for (var r = n - 1; r >= 0; r--)
        {
            var s = b[r];
            for (var k = r + 1; k < n; k++)
                s -= a[r, k] * x[k];
            x[r] = s / a[r, r];
        }
        return x;
    }

    /// <summary>
    /// Exact least-squares weights at the time <paramref name="t"/> for prewindowed data: either all samples weighted by lambda^(t-s) with the
    /// diagonal loading delta·lambda^(t+1), or (finite <paramref name="window"/>) the last <paramref name="window"/> samples with the loading delta.
    /// </summary>
    static double[] DirectLs(float[] x, float[] d, int n, int t, double lambda, double delta, int window = int.MaxValue)
    {
        var a = new double[n, n];
        var rhs = new double[n];
        for (var i = 0; i < n; i++)
            a[i, i] = delta * (window == int.MaxValue ? Math.Pow(lambda, t + 1) : 1.0);
        var first = window == int.MaxValue ? 0 : Math.Max(0, t - window + 1);
        for (var s = first; s <= t; s++)
        {
            var weight = window == int.MaxValue ? Math.Pow(lambda, t - s) : 1.0;
            for (var i = 0; i < n; i++)
            {
                double xi = s - i >= 0 ? x[s - i] : 0;
                rhs[i] += weight * d[s] * xi;
                for (var k = 0; k < n; k++)
                {
                    double xk = s - k >= 0 ? x[s - k] : 0;
                    a[i, k] += weight * xi * xk;
                }
            }
        }

        var m = new double[n, n + 1];
        for (var i = 0; i < n; i++)
        {
            for (var k = 0; k < n; k++)
                m[i, k] = a[i, k];
            m[i, n] = rhs[i];
        }
        for (var c = 0; c < n; c++)
        {
            var pivot = c;
            for (var r = c + 1; r < n; r++)
                if (Math.Abs(m[r, c]) > Math.Abs(m[pivot, c]))
                    pivot = r;
            for (var k = 0; k <= n; k++)
                (m[c, k], m[pivot, k]) = (m[pivot, k], m[c, k]);
            for (var r = 0; r < n; r++)
            {
                if (r == c)
                    continue;
                var factor = m[r, c] / m[c, c];
                for (var k = c; k <= n; k++)
                    m[r, k] -= factor * m[c, k];
            }
        }
        var w = new double[n];
        for (var i = 0; i < n; i++)
            w[i] = m[i, n] / m[i, i];
        return w;
    }

    static double Db(double power) => 10 * Math.Log10(Math.Max(power, 1e-30));

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

    static void Feed(AdaptiveFilter filter, float[] x, float[] d)
    {
        for (var t = 0; t < x.Length; t++)
            filter.Process(x[t], d[t]);
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
