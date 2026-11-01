using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Cheby2 = Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

namespace SP_module_test;

/// <summary>
/// 0.11.1, found while re-checking Chebyshev II against SciPy:
/// (1) <c>ZiFilter.Change(TransferFunction)</c> replaced the coefficients but left <c>Tf</c> at the design the filter was created with,
/// so after <c>Change(...)</c> of a Chebyshev II filter (single-precision classes) <c>Tf</c>, and <c>ZeroPhase</c>, which starts from <c>Tf.Zi</c>,
/// described the old design. The double-precision / generic classes were not affected.
/// (2) The low-pass to band-stop pole / zero mapping took the second root as <c>alpha · (1 − beta)</c>, which cancels
/// all digits where beta is close to 1 (the poles / zeros of a Chebyshev II prototype with a large attenuation are tiny): a first-order
/// band-stop of 80 dB had its pole at 0.99976 instead of 0.99988 in single precision. The second root is now the product of the roots divided by the first.
/// The band-pass designs (same expression in their mapping, not affected in practice) are kept as references.
/// References are scipy.signal.cheby2(n, rs, [2 * f1, 2 * f2], 'bandstop' | 'bandpass').
/// </summary>
internal static class Cheby2_011_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing Chebyshev II Change() / Tf and band designs (0.11.1)...");

        ChangeKeepsTfInSync();
        ChangeWithAnotherLengthLeavesTfAlone();
        BandDesignsMatchScipy();

        if (_failures != 0)
            throw new InvalidOperationException($"Chebyshev II 0.11.1: {_failures} assertion(s) failed.");

        Console.WriteLine("Cheby2_011_test: PASS");
    }

    static void BandDesignsMatchScipy()
    {
        (string Kind, int N, double Rs, double F1, double F2, bool Single, double[] B, double[] A)[] refs =
        [
            ("bs", 1, 80, 0.1, 0.2, true,
                [0.00030767366303912985, -0.00038030556240272915, 0.00030767366303912985],
                [1.0, -0.00038030556235180146, -0.9993846526738709]),
            ("bs", 2, 60, 0.1, 0.2, true,
                [0.016435373554842078, -0.03859327753958732, 0.05342644494334891, -0.03859327753958732, 0.01643537355484208],
                [1.0, -0.23676443667605895, -1.5930551368741876, 0.15957788159688457, 0.679352328927221]),
            ("bs", 1, 100, 0.05, 0.15, true,
                [3.0775888188906364e-05, -5.235906833129039e-05, 3.077588818890636e-05],
                [1.0, -5.235907218048563e-05, -0.9999384482274716]),
            ("bs", 3, 60, 0.15, 0.3, true,
                [0.019588308035864217, -0.0183927612590874, 0.05205986985513368, -0.03301136468479926, 0.05205986985513368, -0.0183927612590874, 0.019588308035864217],
                [1.0, -0.2089883101826534, -1.7907006085542068, 0.20127363243352647, 1.2276097661903875, -0.06208220945384635, -0.2936128018541843]),
            ("bs", 2, 80, 0.01, 0.45, true,
                [0.00010686940496710547, -1.9397629628407684e-05, -0.0001717636551940218, -1.9397629628407694e-05, 0.00010686940496710546],
                [1.0, -0.005103440185927566, -1.9923570514200355, 0.00506464492667082, 0.9923990265747757]),
            ("bs", 1, 100, 0.001, 0.499, false,
                [6.283267595847289e-08, -1.7579087454073095e-21, 6.283267595847289e-08],
                [1.0, 4.972551259641023e-10, -0.999999873837393]),
            ("bp", 2, 40, 0.01, 0.45, true,
                [0.21583295559889817, -0.010318876654896126, -0.40933656206434, -0.010318876654896322, 0.21583295559889826],
                [1.0, -1.5701106552572017, 1.037613197746274, -0.4936646757220417, 0.19532171559936698]),
            ("bp", 1, 80, 0.01, 0.45, true,
                [0.0005239436998249706, 0.0, -0.0005239436998249706],
                [1.0, -1.337033698190768, 0.9989521126003503]),
        ];

        foreach (var (kind, n, rs, f1, f2, single, b, a) in refs)
        {
            var name = $"{(kind == "bs" ? "band-stop" : "band-pass")} N={n} Rs={rs} [{f1}, {f2}]";

            var tf64 = kind == "bs"
                ? new Cheby2.BandStopFilter64(f1, f2, n, rs).Tf
                : new Cheby2.BandPassFilter64(f1, f2, n, rs).Tf;
            ExpectCoefficients($"{name} (double)", tf64.Numerator.ToArray(), tf64.Denominator.ToArray(), b, a, 2e-9);

            if (single)
            {
                var tf32 = kind == "bs"
                    ? new Cheby2.BandStopFilter((float)f1, (float)f2, n, (float)rs).Tf
                    : new Cheby2.BandPassFilter((float)f1, (float)f2, n, (float)rs).Tf;
                ExpectCoefficients($"{name} (float)", tf32.Numerator.Select(v => (double)v).ToArray(), tf32.Denominator.Select(v => (double)v).ToArray(), b, a, 3e-6);

                // the generic classes share the pole mapping of the double-precision ones, here in single precision
                var generic = kind == "bs"
                    ? new Cheby2.BandStopFilter<float>((float)f1, (float)f2, n, (float)rs).Tf
                    : new Cheby2.BandPassFilter<float>((float)f1, (float)f2, n, (float)rs).Tf;
                ExpectCoefficients($"{name} (<float>)", generic.Numerator.Select(v => (double)v).ToArray(), generic.Denominator.Select(v => (double)v).ToArray(), b, a, 3e-6);
            }
        }
    }

    static void ExpectCoefficients(string name, double[] b, double[] a, double[] refB, double[] refA, double tolerance)
    {
        if (b.Length != refB.Length || a.Length != refA.Length)
        {
            Expect(name, false, $"lengths {b.Length}/{a.Length} instead of {refB.Length}/{refA.Length}");
            return;
        }

        var worst = 0.0;
        for (var i = 0; i < b.Length; i++)
            worst = Math.Max(worst, Math.Abs(b[i] - refB[i]));
        for (var i = 0; i < a.Length; i++)
            worst = Math.Max(worst, Math.Abs(a[i] - refA[i]));
        Expect(name, worst <= tolerance, $"max|Δ| {worst:E2} (limit {tolerance:E0})");
    }

    static void ChangeKeepsTfInSync()
    {
        var rng = new Random(7);
        var samples = new float[400];
        for (var i = 0; i < samples.Length; i++)
            samples[i] = (float)(Math.Sin(0.05 * i) + 0.5 * Math.Sin(1.7 * i) + 0.1 * (rng.NextDouble() - 0.5));
        var signal = Signal.FromCopy(samples, 1000f);

        Compare("low-pass", () => { var f = new Cheby2.LowPassFilter(0.1f, 4, 40f); f.Change(0.2f, 50f); return f; }, () => new Cheby2.LowPassFilter(0.2f, 4, 50f), signal);
        Compare("high-pass", () => { var f = new Cheby2.HighPassFilter(0.1f, 4, 40f); f.Change(0.25f, 50f); return f; }, () => new Cheby2.HighPassFilter(0.25f, 4, 50f), signal);
        Compare("band-pass", () => { var f = new Cheby2.BandPassFilter(0.1f, 0.2f, 2, 40f); f.Change(0.15f, 0.3f, 50f); return f; }, () => new Cheby2.BandPassFilter(0.15f, 0.3f, 2, 50f), signal);
        Compare("band-stop", () => { var f = new Cheby2.BandStopFilter(0.1f, 0.2f, 2, 40f); f.Change(0.15f, 0.3f, 50f); return f; }, () => new Cheby2.BandStopFilter(0.15f, 0.3f, 2, 50f), signal);
    }

    // a filter that was designed for another frequency and then changed is the same filter as one that was designed for the new frequency
    static void Compare(string name, Func<ZiFilter> changed, Func<ZiFilter> fresh, Signal signal)
    {
        var a = changed();
        var b = fresh();

        Expect($"{name}: Tf numerator after Change", a.Tf.Numerator.ToArray().SequenceEqual(b.Tf.Numerator.ToArray()));
        Expect($"{name}: Tf denominator after Change", a.Tf.Denominator.ToArray().SequenceEqual(b.Tf.Denominator.ToArray()));

        var viaChanged = a.ZeroPhase(signal).Samples.ToArray();
        var viaFresh = b.ZeroPhase(signal).Samples.ToArray();
        Expect($"{name}: ZeroPhase after Change", viaChanged.SequenceEqual(viaFresh));
    }

    static void ChangeWithAnotherLengthLeavesTfAlone()
    {
        // the coefficients are only replaced when the lengths match: Tf must keep describing the running filter
        var filter = new ZiFilter(new[] { 1f, 2f }, new[] { 1f, 0.5f });
        filter.Change(new TransferFunction(new[] { 1f, 2f, 3f }, new[] { 1f, 0.5f, 0.1f }));
        Expect("Change with another length: Tf unchanged", filter.Tf.Numerator.Count() == 2 && filter.Tf.Denominator.Count() == 2);
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok)
            return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }
}
