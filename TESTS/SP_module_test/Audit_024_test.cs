using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Bess = Vorcyc.Mathematics.SignalProcessing.Filters.Bessel;
using Bw = Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;
using Cheby1 = Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;
using Cheby2 = Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;
using Ellip = Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic;

namespace SP_module_test;

/// <summary>
/// 0.10.24: (1) IIR band-pass designs normalised the gain at the arithmetic centre of the band edges. The band-pass
/// transform sends the prototype's DC to the warped geometric centre, so every band-pass design had a gain error
/// (up to ~72 dB for Chebyshev II, whose edges are stopband edges). (2) Single-precision elliptic designs returned
/// NaN coefficients (and silently wrong high-pass filters) once Rs was far above Rp. (3) Bessel poles were wrong in
/// single precision from order 7 (NaN at 8-9) and NaN in double from order 17. References are SciPy output
/// and the double-precision designs.
/// </summary>
internal static class Audit_024_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing 0.10.24 audit regressions (IIR band-pass centre gain, single-precision elliptic, Bessel high orders)...");

        AssertUnityGainAtCentre();
        AssertMatchesScipy();
        AssertEllipticSinglePrecisionLargeAttenuation();
        AssertBesselHighOrders();

        if (_failures != 0)
            throw new InvalidOperationException($"Audit 0.10.24 (SP): {_failures} assertion(s) failed.");

        Console.WriteLine("Audit_024_test: PASS");
    }

    // The digital frequency (rad/sample) the prototype's DC is mapped to by the band-pass transform.
    static double Centre(double lo, double hi) => 2 * Math.Atan(Math.Sqrt(Math.Tan(Math.PI * lo) * Math.Tan(Math.PI * hi)));

    static void AssertUnityGainAtCentre()
    {
        foreach (var (lo, hi) in new[] { (0.025, 0.075), (0.1, 0.25), (0.2, 0.4), (0.01, 0.2) })
        {
            var w0 = Centre(lo, hi);
            foreach (var order in new[] { 1, 2, 3, 4 })
            {
                var designs = new (string Name, TransferFunction<double> Tf)[]
                {
                    ("Butterworth", new Bw.BandPassFilter64(lo, hi, order).Tf),
                    ("Bessel", new Bess.BandPassFilter64(lo, hi, order).Tf),
                    ("Chebyshev I", new Cheby1.BandPassFilter64(lo, hi, order, 1).Tf),
                    ("Chebyshev II", new Cheby2.BandPassFilter64(lo, hi, order, 80).Tf),
                    ("Elliptic", new Ellip.BandPassFilter64(lo, hi, order, 1, 40).Tf),
                };
                foreach (var (name, tf) in designs)
                {
                    var g = Mag(tf.Numerator.ToArray(), tf.Denominator.ToArray(), w0);
                    Expect($"{name} BP N={order} [{lo},{hi}]: unity gain at the centre", Math.Abs(g - 1) < 1e-6, $"|H|={g:R}");
                }
            }
        }

        // float track (float coefficients are evaluated in double, hence the looser bound)
        foreach (var (lo, hi) in new[] { (0.1, 0.25), (0.2, 0.4) })
        {
            var w0 = Centre(lo, hi);
            foreach (var order in new[] { 2, 3 })
            {
                float l = (float)lo, h = (float)hi;
                var designs = new (string Name, TransferFunction Tf)[]
                {
                    ("Butterworth", new Bw.BandPassFilter(l, h, order).Tf),
                    ("Bessel", new Bess.BandPassFilter(l, h, order).Tf),
                    ("Chebyshev I", new Cheby1.BandPassFilter(l, h, order, 1f).Tf),
                    ("Chebyshev II", new Cheby2.BandPassFilter(l, h, order, 40f).Tf),
                    ("Elliptic", new Ellip.BandPassFilter(l, h, order, 1f, 40f).Tf),
                };
                foreach (var (name, tf) in designs)
                {
                    var g = Mag(tf.Numerator.Select(v => (double)v).ToArray(), tf.Denominator.Select(v => (double)v).ToArray(), w0);
                    Expect($"{name} BP (float) N={order} [{lo},{hi}]: unity gain at the centre", Math.Abs(g - 1) < 5e-3, $"|H|={g:R}");
                }
            }
        }
    }

    static void AssertMatchesScipy()
    {
        // scipy.signal.{cheby2,butter,cheby1,ellip}(n, ..., [2*lo, 2*hi], 'bandpass'). Gain is checked as well:
        // odd orders and Butterworth / Chebyshev II have the same peak gain as SciPy; an even-order Chebyshev I peaks at
        // 10^(-Rp/20) in SciPy but is normalised to 1 at the centre here, hence the scale.
        var refs = new (string Name, TransferFunction<double> Tf, double[] B, double[] A, double Scale)[]
        {
            ("Chebyshev II N=1 Rs=80 [0.025,0.075]", new Cheby2.BandPassFilter64(0.025, 0.075, 1, 80).Tf,
                [1.5838193259307073e-05, 0.0, -1.5838193259307073e-05],
                [1.0, -1.9257926092459328, 0.9999683236134813], 1),
            ("Chebyshev II N=2 Rs=40 [0.05,0.15]", new Cheby2.BandPassFilter64(0.05, 0.15, 2, 40).Tf,
                [0.011353393934590282, -0.031896325543232996, 0.04192222675089453, -0.031896325543232996, 0.01135339393459028],
                [1.0, -3.29275050918135, 4.584123888231756, -3.0865145994652514, 0.8787775737757553], 1),
            ("Chebyshev II N=4 Rs=80 [0.025,0.075]", new Cheby2.BandPassFilter64(0.025, 0.075, 4, 80).Tf,
                [0.00011248837620278607, -0.0007907982440843921, 0.0025168303587732237, -0.004749386875467016, 0.005821735593613721, -0.004749386875467018, 0.0025168303587732245, -0.0007907982440843925, 0.0001124883762027861],
                [1.0, -7.5696724537040305, 25.350868850983566, -49.04266604252347, 59.93127646282639, -47.37005333138406, 23.65117741069569, -6.821310563416754, 0.8704079111518909], 1),
            ("Butterworth N=1 [0.025,0.075]", new Bw.BandPassFilter64(0.025, 0.075, 1).Tf,
                [0.13672873599731955, 0.0, -0.13672873599731955],
                [1.0, -1.6625077511098136, 0.7265425280053609], 1),
            ("Butterworth N=3 [0.1,0.25]", new Bw.BandPassFilter64(0.1, 0.25, 3).Tf,
                [0.04953299635725318, 0.0, -0.14859898907175956, 0.0, 0.14859898907175956, 0.0, -0.04953299635725318],
                [1.0, -2.120602876626562, 2.7247492027684794, -2.289518118076803, 1.4662465331703471, -0.5651792123082665, 0.1377613012598929], 1),
            ("Chebyshev I N=1 Rp=3 [0.025,0.075]", new Cheby1.BandPassFilter64(0.025, 0.075, 1, 3).Tf,
                [0.13700924619520197, 0.0, -0.13700924619520197],
                [1.0, -1.6619675380879155, 0.7259815076095961], 1),
            ("Chebyshev I N=3 Rp=1 [0.1,0.25]", new Cheby1.BandPassFilter64(0.1, 0.25, 3, 1).Tf,
                [0.03438496635181604, 0.0, -0.10315489905544811, 0.0, 0.10315489905544811, 0.0, -0.03438496635181604],
                [1.0, -2.3338328850672956, 3.505369335885402, -3.4481390656988102, 2.6254429300567352, -1.2477900791460323, 0.3983603122140274], 1),
            ("Chebyshev I N=2 Rp=1 [0.025,0.075] (even order)", new Cheby1.BandPassFilter64(0.025, 0.075, 2, 1).Tf,
                [0.020515223631714954, 0.0, -0.04103044726342991, 0.0, 0.020515223631714954],
                [1.0, -3.4843143734729525, 4.705268869821799, -2.926968602390233, 0.7105934766511971], Math.Pow(10, 1.0 / 20)),
            ("Elliptic N=1 Rp=3 Rs=40 [0.025,0.075]", new Ellip.BandPassFilter64(0.025, 0.075, 1, 3, 40).Tf,
                [0.13700924619520197, 0.0, -0.13700924619520197],
                [1.0, -1.6619675380879155, 0.7259815076095961], 1),
            ("Elliptic N=3 Rp=1 Rs=40 [0.1,0.25]", new Ellip.BandPassFilter64(0.1, 0.25, 3, 1, 40).Tf,
                [0.055480858912020585, -0.038005055915587865, -0.07248891153536746, -7.391535238812608e-17, 0.07248891153536746, 0.038005055915587935, -0.0554808589120206],
                [1.0, -2.327991917467305, 3.490263479206476, -3.445286817504699, 2.636585529130079, -1.253503837322596, 0.399132444644064], 1),
        };

        foreach (var (name, tf, b, a, scale) in refs)
        {
            var tb = tf.Numerator.ToArray();
            var ta = tf.Denominator.ToArray();
            var sameShape = tb.Length == b.Length && ta.Length == a.Length;
            Expect($"{name}: coefficient count", sameShape, $"{tb.Length}/{ta.Length} vs {b.Length}/{a.Length}");
            if (!sameShape) continue;

            double worst = 0;
            for (int i = 0; i < a.Length; i++)
                worst = Math.Max(worst, Math.Abs(ta[i] / ta[0] - a[i]));
            for (int i = 0; i < b.Length; i++)
                worst = Math.Max(worst, Math.Abs(tb[i] / ta[0] - b[i] * scale));
            Expect($"{name}: matches SciPy", worst < 1e-8, $"maxΔ={worst:E2}");
        }
    }

    // Single precision used to lose 1 - r*r (r = eps_p / eps_s) once Rs was far above Rp: NaN numerators, or high-pass zeros at DC.
    static void AssertEllipticSinglePrecisionLargeAttenuation()
    {
        foreach (var (order, rp, rs) in new[] { (1, 0.01, 60.0), (3, 1.0, 80.0), (4, 0.1, 100.0), (5, 1.0, 100.0), (3, 3.0, 120.0) })
        {
            var tag = $"N={order} Rp={rp} Rs={rs}";
            float f = 0.15f, lo = 0.1f, hi = 0.2f, p = (float)rp, s = (float)rs;
            var bandOrder = Math.Min(order, 3);

            var designs = new (string Name, TransferFunction Tf, TransferFunction<double> Ref)[]
            {
                ("low-pass", new Ellip.LowPassFilter(f, order, p, s).Tf, new Ellip.LowPassFilter64(f, order, rp, rs).Tf),
                ("high-pass", new Ellip.HighPassFilter(f, order, p, s).Tf, new Ellip.HighPassFilter64(f, order, rp, rs).Tf),
                ("band-pass", new Ellip.BandPassFilter(lo, hi, bandOrder, p, s).Tf, new Ellip.BandPassFilter64(lo, hi, bandOrder, rp, rs).Tf),
                ("band-stop", new Ellip.BandStopFilter(lo, hi, bandOrder, p, s).Tf, new Ellip.BandStopFilter64(lo, hi, bandOrder, rp, rs).Tf),
            };
            foreach (var (name, tf, reference) in designs)
            {
                var d = MaxDiff(tf.Numerator.Select(v => (double)v).ToArray(), tf.Denominator.Select(v => (double)v).ToArray(),
                    reference.Numerator.ToArray(), reference.Denominator.ToArray());
                Expect($"Elliptic {name} (float) {tag}: matches the double design", d < 5e-3, $"maxΔ|H|={d:E2}");
            }

            var generic = new Ellip.LowPassFilter<float>(f, order, p, s).Tf;
            var dg = MaxDiff(generic.Numerator.Select(v => (double)v).ToArray(), generic.Denominator.Select(v => (double)v).ToArray(),
                designs[0].Ref.Numerator.ToArray(), designs[0].Ref.Denominator.ToArray());
            Expect($"Elliptic low-pass <float> {tag}: matches the double design", dg < 5e-3, $"maxΔ|H|={dg:E2}");
        }
    }

    // Durand-Kerner on the Bessel polynomial gave wrong single-precision poles from order 7 (NaN at 8-9) and NaN in double from order 17.
    static void AssertBesselHighOrders()
    {
        foreach (var order in new[] { 7, 8, 9, 12, 16, 17, 20 })
        {
            var wide = Bess.PrototypeBessel.Poles<double>(order);
            var narrow = Bess.PrototypeBessel.Poles(order);
            var finite = wide.All(p => double.IsFinite(p.Real) && double.IsFinite(p.Imaginary) && p.Real < 0);
            Expect($"Bessel N={order}: double poles are finite and stable", finite);
            if (!finite) continue;

            // The normalisation makes the product of the poles have magnitude 1.
            var product = Complex.One;
            foreach (var p in wide) product *= new Complex(p.Real, p.Imaginary);
            Expect($"Bessel N={order}: |product of poles| = 1", Math.Abs(product.Magnitude - 1) < 1e-4, $"{product.Magnitude:R}");

            double worst = 0;
            foreach (var p in wide)
            {
                var nearest = narrow.Min(q => Math.Sqrt(Math.Pow(q.Real - p.Real, 2) + Math.Pow(q.Imaginary - p.Imaginary, 2)));
                worst = Math.Max(worst, nearest / Math.Sqrt(p.Real * p.Real + p.Imaginary * p.Imaginary));
            }
            Expect($"Bessel N={order}: single-precision poles match the double poles", worst < 1e-5, $"max rel Δ={worst:E2}");
        }

        // scipy.signal.besselap(17, 'delay') times the VMath normalisation 10^(-log10(a1)/17); upper half plane incl. the real pole.
        (double Re, double Im)[] ref17 =
        [
            (-0.9087141161336392, 0), (-0.9016273850787282, 0.1360267995173024), (-0.8801100704438629, 0.27253471564788045),
            (-0.8433414495836131, 0.4100759282910023), (-0.7897644147799704, 0.5493724405281087), (-0.7166893842372348, 0.6914936286393609),
            (-0.6193710717342141, 0.838249725282699), (-0.4884629337672708, 0.9932971956316785), (-0.2998489459990075, 1.166761272925668),
        ];
        var poles17 = Bess.PrototypeBessel.Poles<double>(17);
        double worst17 = 0;
        foreach (var (re, im) in ref17)
        {
            var nearest = poles17.Min(p => Math.Sqrt(Math.Pow(p.Real - re, 2) + Math.Pow(p.Imaginary - im, 2)));
            worst17 = Math.Max(worst17, nearest / Math.Sqrt(re * re + im * im));
        }
        Expect("Bessel N=17: poles match SciPy", worst17 < 1e-6, $"max rel Δ={worst17:E2}");

        foreach (var order in new[] { 8, 9, 12 })
        {
            var tf = new Bess.LowPassFilter(0.2f, order).Tf;
            var dc = tf.Numerator.Sum(v => (double)v) / tf.Denominator.Sum(v => (double)v);
            Expect($"Bessel low-pass (float) N={order}: finite with unit DC gain", tf.Numerator.All(float.IsFinite) && tf.Denominator.All(float.IsFinite) && Math.Abs(dc - 1) < 1e-3, $"DC={dc:R}");
        }
    }

    // NaN compares false, so a non-finite design fails the bound above.
    static double MaxDiff(double[] b1, double[] a1, double[] b2, double[] a2)
    {
        double worst = 0;
        for (int i = 1; i < 128; i++)
        {
            var w = Math.PI * i / 128;
            worst = Math.Max(worst, Math.Abs(Mag(b1, a1, w) - Mag(b2, a2, w)));
        }
        return worst;
    }

    static double Mag(double[] num, double[] den, double w)
    {
        Complex n = 0, d = 0;
        for (int i = 0; i < num.Length; i++) n += num[i] * Complex.FromPolarCoordinates(1, -w * i);
        for (int i = 0; i < den.Length; i++) d += den[i] * Complex.FromPolarCoordinates(1, -w * i);
        return (n / d).Magnitude;
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }
}
