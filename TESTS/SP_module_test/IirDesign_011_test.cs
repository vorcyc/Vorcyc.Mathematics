using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Bes = Vorcyc.Mathematics.SignalProcessing.Filters.Bessel;
using Bw = Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;
using Cheby1 = Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;
using Cheby2 = Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;
using DesignFilter = Vorcyc.Mathematics.SignalProcessing.Filters.Fda.DesignFilter;
using Ell = Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic;
using SysComplex = System.Numerics.Complex;

namespace SP_module_test;

/// <summary>
/// 0.11.1, found while re-checking Chebyshev II against SciPy:
/// (1) <c>TransferFunction.NormalizeAt</c> evaluated the expanded polynomials, which cancel completely in single precision for low
/// cutoff frequencies / higher orders (NaN coefficients; the generic classes silently skipped the normalization instead),
/// (2) the <c>Change*</c> methods of the IIR / FIR / state-vector filters left <c>Tf</c> at the design the filter was created with,
/// (3) order 0, the end points 0 and 0.5 of the frequency, NaN and an infinite attenuation were accepted and produced degenerate filters,
/// (4) the second root of every pole / zero pair of the band-pass transformation lost its digits for wide bands (the band-stop was fixed before),
/// (5) the Butterworth, Chebyshev I, Elliptic and Bessel prototypes accepted order 0 and failed with unrelated exceptions for negative orders.
/// References are the zpk gains, poles and zeros of scipy.signal.butter / cheby2.
/// </summary>
internal static class IirDesign_011_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing IIR design robustness (0.11.1)...");

        Run(DesignGainsMatchScipy);
        Run(DesignsAreFinite);
        Run(NormalizeAtFromZerosAndPoles);
        Run(NormalizeAtKeepsUnitGainOfTheCoefficients);
        Run(NormalizeAtFromCoefficients);
        Run(NormalizeAtDegenerate);
        Run(ChangeKeepsTfInSync);
        Run(ChangeKeepsTfInSyncGeneric);
        Run(ArgumentValidation);
        Run(BandPassMatchesScipy);
        Run(BandPassSecondRootGeneric);
        Run(PrototypeOrderValidation);
        Run(RejectedChangeLeavesTheFilterAsItWas);

        if (_failures != 0)
            throw new InvalidOperationException($"IIR design 0.11.1: {_failures} assertion(s) failed.");

        Console.WriteLine("IirDesign_011_test: PASS");
    }

    // an exception is a failure of that group, the other groups still run
    static void Run(Action group)
    {
        try
        {
            group();
        }
        catch (Exception ex)
        {
            Expect(group.Method.Name, false, $"threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    static T? Try<T>(string name, Func<T> make) where T : class
    {
        try
        {
            return make();
        }
        catch (Exception ex)
        {
            Expect(name, false, $"threw {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    // ---------------------------------------------------------------- normalization gain

    static void DesignGainsMatchScipy()
    {
        // (kind, f1, f2, order, Rs, SciPy's k, tolerance in dB for single and for double precision): the gain of the numerator is k because
        // the numerator is k times a monic polynomial. All of them are designs whose expanded coefficients cannot keep the response in single precision.
        // Where the coefficients still agree with the zeros and poles to within 1 dB (double precision: usually to 1e-5 dB) they decide the gain
        // (the filter runs with them); where they do not, the gain of the zeros and poles decides and it is SciPy's k.
        (string Kind, double F1, double F2, int N, double Rs, double K, double SingleDb, double DoubleDb)[] refs =
        [
            ("lp", 0.001, 0, 4, 40, 0.009958420723775013, 0.1, 1e-3),
            ("lp", 0.005, 0, 7, 60, 0.00010524230492680647, 0.1, 1e-3),
            ("hp", 0.499, 0, 4, 40, 0.009958420723775011, 0.1, 1e-3),
            ("bs", 0.01, 0.05, 5, 60, 0.4159910769816118, 1.25, 1e-3),
            ("lp", 0.001, 0, 6, 60, 0.0009935472144073437, 0.1, 1e-6),     // double: the coefficients are 6 dB off
            ("hp", 0.499, 0, 6, 60, 0.0009935472144073445, 0.1, 1.25),
            ("lp", 0.002, 0, 8, 80, 9.832484286580562e-05, 0.1, 1e-6),     // double: the coefficients are 55 dB off
        ];

        static double Db(double value, double k) => Math.Abs(20 * Math.Log10(value / k));

        foreach (var (kind, f1, f2, n, rs, k, singleDb, doubleDb) in refs)
        {
            var name = $"{kind} N={n} Rs={rs} f={f1}{(kind == "bs" ? "," + f2 : "")}";

            // the single precision classes
            var tf32 = Try($"{name} (float): design", () => kind switch
            {
                "lp" => new Cheby2.LowPassFilter((float)f1, n, (float)rs).Tf,
                "hp" => new Cheby2.HighPassFilter((float)f1, n, (float)rs).Tf,
                _ => new Cheby2.BandStopFilter((float)f1, (float)f2, n, (float)rs).Tf,
            });
            if (tf32 is not null)
            {
                Expect($"{name} (float): finite coefficients", tf32.Numerator.All(float.IsFinite) && tf32.Denominator.All(float.IsFinite));
                Expect($"{name} (float): gain equals SciPy's", Db(tf32.Numerator[0], k) <= singleDb, $"{tf32.Numerator[0]:E6} vs {k:E6}");
            }

            // the generic classes in single and in double precision
            var generic32 = Try($"{name} (<float>): design", () => kind switch
            {
                "lp" => new Cheby2.LowPassFilter<float>((float)f1, n, (float)rs).Tf,
                "hp" => new Cheby2.HighPassFilter<float>((float)f1, n, (float)rs).Tf,
                _ => new Cheby2.BandStopFilter<float>((float)f1, (float)f2, n, (float)rs).Tf,
            });
            if (generic32 is not null)
            {
                Expect($"{name} (<float>): finite coefficients", generic32.Numerator.All(float.IsFinite) && generic32.Denominator.All(float.IsFinite));
                Expect($"{name} (<float>): gain equals SciPy's", Db(generic32.Numerator[0], k) <= singleDb, $"{generic32.Numerator[0]:E6} vs {k:E6}");
            }

            var tf64 = Try($"{name} (double): design", () => kind switch
            {
                "lp" => new Cheby2.LowPassFilter64(f1, n, rs).Tf,
                "hp" => new Cheby2.HighPassFilter64(f1, n, rs).Tf,
                _ => new Cheby2.BandStopFilter64(f1, f2, n, rs).Tf,
            });
            if (tf64 is not null)
                Expect($"{name} (double): gain equals SciPy's", Db(tf64.Numerator[0], k) <= doubleDb, $"{tf64.Numerator[0]:E12} vs {k:E12}");
        }
    }

    static void DesignsAreFinite()
    {
        // no design of the grid may end in NaN / Infinity coefficients, whatever its quality in single precision is
        var bad = new List<string>();
        foreach (var f in new[] { 0.001f, 0.005f, 0.1f, 0.45f, 0.499f })
        foreach (var n in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 10 })
        foreach (var rs in new[] { 20f, 60f, 100f })
        {
            void Check(string design, Func<TransferFunction> make)
            {
                try
                {
                    var tf = make();
                    if (!(tf.Numerator.All(float.IsFinite) && tf.Denominator.All(float.IsFinite)))
                        bad.Add($"{design} f={f} N={n} Rs={rs}");
                }
                catch (Exception ex)
                {
                    bad.Add($"{design} f={f} N={n} Rs={rs} ({ex.GetType().Name})");
                }
            }
            void CheckGeneric(string design, Func<TransferFunction<float>> make)
            {
                try
                {
                    var tf = make();
                    if (!(tf.Numerator.All(float.IsFinite) && tf.Denominator.All(float.IsFinite)))
                        bad.Add($"{design}<float> f={f} N={n} Rs={rs}");
                }
                catch (Exception ex)
                {
                    bad.Add($"{design}<float> f={f} N={n} Rs={rs} ({ex.GetType().Name})");
                }
            }

            Check("LP", () => new Cheby2.LowPassFilter(f, n, rs).Tf);
            Check("HP", () => new Cheby2.HighPassFilter(f, n, rs).Tf);
            CheckGeneric("LP", () => new Cheby2.LowPassFilter<float>(f, n, rs).Tf);
            CheckGeneric("HP", () => new Cheby2.HighPassFilter<float>(f, n, rs).Tf);

            var hi = Math.Min(f * 5, 0.499f);
            if (hi > f)
            {
                Check("BP", () => new Cheby2.BandPassFilter(f, hi, n, rs).Tf);
                Check("BS", () => new Cheby2.BandStopFilter(f, hi, n, rs).Tf);
                CheckGeneric("BP", () => new Cheby2.BandPassFilter<float>(f, hi, n, rs).Tf);
                CheckGeneric("BS", () => new Cheby2.BandStopFilter<float>(f, hi, n, rs).Tf);
            }
        }
        Expect("Chebyshev II designs of the grid have finite coefficients (float and <float>)", bad.Count == 0, $"{bad.Count} designs, e.g. {string.Join("; ", bad.Take(4))}");
    }

    static double MagnitudeFromRoots(double gain, IEnumerable<SysComplex> zeros, IEnumerable<SysComplex> poles, double omega)
    {
        var w = SysComplex.FromPolarCoordinates(1, omega);
        var h = new SysComplex(gain, 0);
        foreach (var z in zeros)
            h *= w - z;
        foreach (var p in poles)
            h /= w - p;
        return h.Magnitude;
    }

    static double MagnitudeFromCoefficients(IEnumerable<double> b, IEnumerable<double> a, double omega)
    {
        SysComplex Eval(IEnumerable<double> c)
        {
            var sum = SysComplex.Zero;
            var k = 0;
            foreach (var v in c)
                sum += v * SysComplex.FromPolarCoordinates(1, -omega * k++);
            return sum;
        }
        return (Eval(b) / Eval(a)).Magnitude;
    }

    static void NormalizeAtFromZerosAndPoles()
    {
        // the zeros are on the unit circle close to z = 1: the expanded numerator is a sum of large terms that cancels to the rounding noise
        ComplexFp32[] zeros =
        [
            ComplexFp32.FromPolarCoordinates(1f, 0.001f), ComplexFp32.FromPolarCoordinates(1f, -0.001f),
            ComplexFp32.FromPolarCoordinates(1f, 0.002f), ComplexFp32.FromPolarCoordinates(1f, -0.002f),
        ];
        ComplexFp32[] poles =
        [
            ComplexFp32.FromPolarCoordinates(0.995f, 0.003f), ComplexFp32.FromPolarCoordinates(0.995f, -0.003f),
            ComplexFp32.FromPolarCoordinates(0.99f, 0.0045f), ComplexFp32.FromPolarCoordinates(0.99f, -0.0045f),
        ];

        var tf = new TransferFunction(zeros, poles);
        tf.NormalizeAt(0f);
        Expect("NormalizeAt (zpk, float): finite coefficients", tf.Numerator.All(float.IsFinite));
        var magnitude = MagnitudeFromRoots(tf.Numerator[0], zeros.Select(z => new SysComplex(z.Real, z.Imaginary)), poles.Select(p => new SysComplex(p.Real, p.Imaginary)), 0);
        Expect("NormalizeAt (zpk, float): unit gain at the frequency", Math.Abs(magnitude - 1) < 1e-5, $"|H| = {magnitude:E6}");

        // a gain given to the constructor is replaced by the normalization
        var withGain = new TransferFunction(zeros, poles, 123f);
        withGain.NormalizeAt(0.7f);
        var atFrequency = MagnitudeFromRoots(withGain.Numerator[0], zeros.Select(z => new SysComplex(z.Real, z.Imaginary)), poles.Select(p => new SysComplex(p.Real, p.Imaginary)), 0.7);
        Expect("NormalizeAt (zpk, float) replaces the constructor gain", Math.Abs(atFrequency - 1) < 1e-5, $"|H| = {atFrequency:E6}");

        // the same for the generic classes
        foreach (var precision in new[] { "float", "double" })
        {
            if (precision == "double")
            {
                // eight zeros close to z = 1 cancel to the noise of double precision as well
                var z8 = new Complex<double>[8];
                var p8 = new Complex<double>[8];
                for (var i = 0; i < 4; i++)
                {
                    z8[2 * i] = Complex<double>.FromPolarCoordinates(1.0, 0.001 * (i + 1));
                    z8[2 * i + 1] = Complex<double>.FromPolarCoordinates(1.0, -0.001 * (i + 1));
                    p8[2 * i] = Complex<double>.FromPolarCoordinates(0.995, 0.002 * (i + 1));
                    p8[2 * i + 1] = Complex<double>.FromPolarCoordinates(0.995, -0.002 * (i + 1));
                }

                var generic = new TransferFunction<double>(z8, p8);
                generic.NormalizeAt(0.0);
                var m = MagnitudeFromRoots(generic.Numerator[0], z8.Select(v => new SysComplex(v.Real, v.Imaginary)), p8.Select(v => new SysComplex(v.Real, v.Imaginary)), 0);
                Expect("NormalizeAt (zpk, <double>): unit gain", Math.Abs(m - 1) < 1e-9, $"|H| = {m:E6}");
            }
            else
            {
                var zf = zeros.Select(v => new Complex<float>(v.Real, v.Imaginary)).ToArray();
                var pf = poles.Select(v => new Complex<float>(v.Real, v.Imaginary)).ToArray();
                var generic = new TransferFunction<float>(zf, pf);
                generic.NormalizeAt(0f);
                Expect("NormalizeAt (zpk, <float>): finite coefficients", generic.Numerator.All(float.IsFinite));
                var m = MagnitudeFromRoots(generic.Numerator[0], zeros.Select(v => new SysComplex(v.Real, v.Imaginary)), poles.Select(v => new SysComplex(v.Real, v.Imaginary)), 0);
                Expect("NormalizeAt (zpk, <float>): unit gain", Math.Abs(m - 1) < 1e-5, $"|H| = {m:E6}");
            }
        }
    }

    static void NormalizeAtKeepsUnitGainOfTheCoefficients()
    {
        // Moderate designs: the stored coefficients still describe the zeros and poles, so their own response is exactly 1 where the design is
        // normalized. (A gain taken from the zeros and poles would be off by the rounding of the coefficients: 0.3 dB for the single precision
        // band-pass 0.2..0.3, order 1, 100 dB, whose poles are 3e-6 away from the unit circle.)
        static double CentreSingle(float low, float high) => 2f * MathF.Atan(MathF.Sqrt(MathF.Tan(MathF.PI * low) * MathF.Tan(MathF.PI * high)));
        static double CentreDouble(double low, double high) => 2 * Math.Atan(Math.Sqrt(Math.Tan(Math.PI * low) * Math.Tan(Math.PI * high)));

        void Single(string name, TransferFunction tf, TransferFunction<float> generic, double omega)
        {
            var m = MagnitudeFromCoefficients(tf.Numerator.Select(v => (double)v), tf.Denominator.Select(v => (double)v), omega);
            Expect($"{name} (float): unit gain of the coefficients", Math.Abs(m - 1) < 1e-5, $"|H| = {m:E6}");
            m = MagnitudeFromCoefficients(generic.Numerator.Select(v => (double)v), generic.Denominator.Select(v => (double)v), omega);
            Expect($"{name} (<float>): unit gain of the coefficients", Math.Abs(m - 1) < 1e-5, $"|H| = {m:E6}");
        }

        void Double(string name, TransferFunction<double> tf, double omega)
        {
            var m = MagnitudeFromCoefficients(tf.Numerator, tf.Denominator, omega);
            Expect($"{name} (double): unit gain of the coefficients", Math.Abs(m - 1) < 1e-10, $"|H| = {m:E6}");
        }

        Single("low-pass 0.1, order 4, 60 dB", new Cheby2.LowPassFilter(0.1f, 4, 60f).Tf, new Cheby2.LowPassFilter<float>(0.1f, 4, 60f).Tf, 0);
        Single("high-pass 0.3, order 4, 60 dB", new Cheby2.HighPassFilter(0.3f, 4, 60f).Tf, new Cheby2.HighPassFilter<float>(0.3f, 4, 60f).Tf, MathF.PI);
        Single("band-pass 0.2..0.3, order 1, 100 dB", new Cheby2.BandPassFilter(0.2f, 0.3f, 1, 100f).Tf, new Cheby2.BandPassFilter<float>(0.2f, 0.3f, 1, 100f).Tf, CentreSingle(0.2f, 0.3f));
        Single("band-pass 0.1..0.3, order 2, 60 dB", new Cheby2.BandPassFilter(0.1f, 0.3f, 2, 60f).Tf, new Cheby2.BandPassFilter<float>(0.1f, 0.3f, 2, 60f).Tf, CentreSingle(0.1f, 0.3f));
        Single("band-stop 0.1..0.2, order 2, 40 dB", new Cheby2.BandStopFilter(0.1f, 0.2f, 2, 40f).Tf, new Cheby2.BandStopFilter<float>(0.1f, 0.2f, 2, 40f).Tf, 0);

        Double("low-pass 0.1, order 4, 60 dB", new Cheby2.LowPassFilter64(0.1, 4, 60).Tf, 0);
        Double("high-pass 0.3, order 4, 60 dB", new Cheby2.HighPassFilter64(0.3, 4, 60).Tf, Math.PI);
        Double("band-pass 0.2..0.3, order 1, 100 dB", new Cheby2.BandPassFilter64(0.2, 0.3, 1, 100).Tf, CentreDouble(0.2, 0.3));
        Double("band-stop 0.1..0.2, order 2, 40 dB", new Cheby2.BandStopFilter64(0.1, 0.2, 2, 40).Tf, 0);
    }

    static void NormalizeAtFromCoefficients()
    {
        double[] b = [0.2, 0.3, 0.1];
        double[] a = [1, -0.5, 0.2];

        var tf = new TransferFunction(b.Select(v => (float)v).ToArray(), a.Select(v => (float)v).ToArray());
        tf.NormalizeAt(1.0f);
        var m = MagnitudeFromCoefficients(tf.Numerator.Select(v => (double)v), tf.Denominator.Select(v => (double)v), 1.0);
        Expect("NormalizeAt (coefficients, float): unit gain", Math.Abs(m - 1) < 1e-6, $"|H| = {m:E6}");

        var generic = new TransferFunction<double>(b, a);
        generic.NormalizeAt(1.0);
        m = MagnitudeFromCoefficients(generic.Numerator, generic.Denominator, 1.0);
        Expect("NormalizeAt (coefficients, <double>): unit gain", Math.Abs(m - 1) < 1e-12, $"|H| = {m:E6}");

        var generic32 = new TransferFunction<float>(b.Select(v => (float)v).ToArray(), a.Select(v => (float)v).ToArray());
        generic32.NormalizeAt(1.0f);
        m = MagnitudeFromCoefficients(generic32.Numerator.Select(v => (double)v), generic32.Denominator.Select(v => (double)v), 1.0);
        Expect("NormalizeAt (coefficients, <float>): unit gain", Math.Abs(m - 1) < 1e-6, $"|H| = {m:E6}");
    }

    static void NormalizeAtDegenerate()
    {
        // a response that is zero (or not defined) at the frequency cannot be normalized: it must not end in NaN or be skipped silently
        ExpectThrows<InvalidOperationException>("NormalizeAt at a zero of the response (float)", () => new TransferFunction([1f, -1f], [1f]).NormalizeAt(0f));
        ExpectThrows<InvalidOperationException>("NormalizeAt of an all-zero numerator (float)", () => new TransferFunction([0f, 0f], [1f]).NormalizeAt(0.3f));
        ExpectThrows<InvalidOperationException>("NormalizeAt at a pole on the unit circle (float)", () => new TransferFunction([1f], [1f, -1f]).NormalizeAt(0f));
        ExpectThrows<InvalidOperationException>("NormalizeAt at a zero of the response (<double>)", () => new TransferFunction<double>([1.0, -1.0], [1.0]).NormalizeAt(0.0));
        ExpectThrows<InvalidOperationException>("NormalizeAt of an all-zero numerator (<double>)", () => new TransferFunction<double>([0.0, 0.0], [1.0]).NormalizeAt(0.3));
        ExpectThrows<InvalidOperationException>("NormalizeAt at a zero of the response (<float>)", () => new TransferFunction<float>([1f, -1f], [1f]).NormalizeAt(0f));
    }

    // ---------------------------------------------------------------- Change() keeps Tf in sync

    static void ChangeKeepsTfInSync()
    {
        float[] bA = [0.2f, 0.3f, 0.1f], aA = [1f, -0.5f, 0.2f];
        float[] bB = [0.4f, 0.1f, 0.05f], aB = [1f, -0.3f, 0.1f];
        TransferFunction MakeA() => new(bA.ToArray(), aA.ToArray());
        TransferFunction MakeB() => new(bB.ToArray(), aB.ToArray());

        void ExpectTf(string name, TransferFunction tf, float[] b, float[] a)
            => Expect(name, tf.Numerator.SequenceEqual(b) && tf.Denominator.SequenceEqual(a), $"b = [{string.Join(", ", tf.Numerator)}] a = [{string.Join(", ", tf.Denominator)}]");

        // IIR filter
        {
            var f = new IirFilter(MakeA());
            f.Change(MakeB());
            ExpectTf("IirFilter.Change updates Tf", f.Tf, bB, aB);

            f = new IirFilter(MakeA());
            f.ChangeNumeratorCoeffs(bB);
            ExpectTf("IirFilter.ChangeNumeratorCoeffs updates Tf", f.Tf, bB, aA);
            f.ChangeDenominatorCoeffs(aB);
            ExpectTf("IirFilter.ChangeDenominatorCoeffs updates Tf", f.Tf, bB, aB);

            f = new IirFilter(MakeA());
            f.Change(new TransferFunction([1f, 2f, 3f, 4f], [1f, 0.1f]));
            ExpectTf("IirFilter.Change with other lengths leaves Tf alone", f.Tf, bA, aA);
        }

        // state-vector filter
        {
            var f = new ZiFilter(MakeA());
            f.Change(MakeB());
            ExpectTf("ZiFilter.Change updates Tf", f.Tf, bB, aB);

            f = new ZiFilter(MakeA());
            f.ChangeNumeratorCoeffs(bB);
            ExpectTf("ZiFilter.ChangeNumeratorCoeffs updates Tf", f.Tf, bB, aA);
            f.ChangeDenominatorCoeffs(aB);
            ExpectTf("ZiFilter.ChangeDenominatorCoeffs updates Tf", f.Tf, bB, aB);

            f = new ZiFilter(MakeA());
            f.Change(new TransferFunction([1f, 2f, 3f, 4f], [1f, 0.1f]));
            ExpectTf("ZiFilter.Change with longer coefficients leaves Tf alone", f.Tf, bA, aA);
            Expect("ZiFilter.Change with longer coefficients leaves the coefficients alone", SameOutput(f, new ZiFilter(MakeA())));

            // numerator and denominator of different lengths: the constructor pads the shorter one with zeros, so does Change
            float[] bShort = [1f, 2f, 3f], aLong = [1f, 0.5f, 0.2f, 0.1f, 0.05f];
            float[] bShort2 = [4f, 5f, 6f], aLong2 = [1f, 0.4f, 0.3f, 0.2f, 0.1f];
            var padded = new ZiFilter(new TransferFunction(bShort.ToArray(), aLong.ToArray()));
            padded.Change(new TransferFunction(bShort2.ToArray(), aLong2.ToArray()));
            ExpectTf("ZiFilter.Change with a shorter numerator updates Tf", padded.Tf, bShort2, aLong2);
            Expect("ZiFilter.Change with a shorter numerator filters like a new filter", SameOutput(padded, new ZiFilter(new TransferFunction(bShort2.ToArray(), aLong2.ToArray()))));

            // the shorter numerator replaces a full-length one: no coefficient of the old numerator may survive
            var full = new ZiFilter(new TransferFunction([1f, 2f, 3f, 4f, 5f], aLong.ToArray()));
            full.Change(new TransferFunction(bShort2.ToArray(), aLong2.ToArray()));
            Expect("ZiFilter.Change with a shorter numerator clears the old coefficients", SameOutput(full, new ZiFilter(new TransferFunction(bShort2.ToArray(), aLong2.ToArray()))));

            // a filter that was built from a shorter denominator keeps its state size, so a denominator of that length is accepted
            var fir = new ZiFilter(new TransferFunction([1f, 2f, 3f, 4f], [1f]));
            fir.Change(new TransferFunction([5f, 6f, 7f, 8f], [1f]));
            ExpectTf("ZiFilter.Change of a FIR-like transfer function updates Tf", fir.Tf, [5f, 6f, 7f, 8f], [1f]);
            Expect("ZiFilter.Change of a FIR-like transfer function filters like a new filter", SameOutput(fir, new ZiFilter(new TransferFunction([5f, 6f, 7f, 8f], [1f]))));
        }

        // FIR filter
        {
            var f = new FirFilter(new TransferFunction([1f, 2f, 3f]));
            f.ChangeKernel([4f, 5f, 6f]);
            ExpectTf("FirFilter.ChangeKernel updates Tf", f.Tf, [4f, 5f, 6f], [1f]);
        }

        // the Chebyshev II filters: Change() then Tf is the design of the new frequency
        {
            var changed = new Cheby2.LowPassFilter(0.1f, 4, 40f);
            changed.Change(0.2f, 50f);
            var fresh = new Cheby2.LowPassFilter(0.2f, 4, 50f);
            ExpectTf("Chebyshev II low-pass: Change updates Tf", changed.Tf, fresh.Tf.Numerator, fresh.Tf.Denominator);
        }
    }

    static void ChangeKeepsTfInSyncGeneric()
    {
        double[] bA = [0.2, 0.3, 0.1], aA = [1, -0.5, 0.2];
        double[] bB = [0.4, 0.1, 0.05], aB = [1, -0.3, 0.1];
        TransferFunction<double> MakeA() => new(bA.ToArray(), aA.ToArray());
        TransferFunction<double> MakeB() => new(bB.ToArray(), aB.ToArray());

        void ExpectTf(string name, TransferFunction<double> tf, double[] b, double[] a)
            => Expect(name, tf.Numerator.SequenceEqual(b) && tf.Denominator.SequenceEqual(a), $"b = [{string.Join(", ", tf.Numerator)}] a = [{string.Join(", ", tf.Denominator)}]");

        {
            var f = new IirFilter<double>(MakeA());
            _ = f.Tf;      // the transfer function is cached when it is read
            f.ChangeNumeratorCoeffs(bB);
            ExpectTf("IirFilter<T>.ChangeNumeratorCoeffs updates the cached Tf", f.Tf, bB, aA);
            f.ChangeDenominatorCoeffs(aB);
            ExpectTf("IirFilter<T>.ChangeDenominatorCoeffs updates the cached Tf", f.Tf, bB, aB);

            f = new IirFilter<double>(MakeA());
            f.Change(MakeB());
            ExpectTf("IirFilter<T>.Change updates Tf", f.Tf, bB, aB);

            f = new IirFilter<double>(MakeA());
            f.Change(new TransferFunction<double>([1.0, 2.0, 3.0, 4.0], [1.0, 0.1]));
            ExpectTf("IirFilter<T>.Change with other lengths leaves Tf alone", f.Tf, bA, aA);
        }

        {
            var f = new ZiFilter<double>(MakeA());
            f.ChangeNumeratorCoeffs(bB);
            ExpectTf("ZiFilter<T>.ChangeNumeratorCoeffs updates Tf", f.Tf, bB, aA);
            f.ChangeDenominatorCoeffs(aB);
            ExpectTf("ZiFilter<T>.ChangeDenominatorCoeffs updates Tf", f.Tf, bB, aB);

            f = new ZiFilter<double>(MakeA());
            f.Change(MakeB());
            ExpectTf("ZiFilter<T>.Change updates Tf", f.Tf, bB, aB);

            // a padded numerator is not shared with the transfer function the filter was built from
            f = new ZiFilter<double>(new TransferFunction<double>([1.0, 2.0, 3.0], [1.0, 0.5, 0.2, 0.1, 0.05]));
            f.ChangeNumeratorCoeffs([4.0, 5.0, 6.0, 7.0, 8.0]);
            ExpectTf("ZiFilter<T>.ChangeNumeratorCoeffs updates Tf (padded numerator)", f.Tf, [4.0, 5.0, 6.0, 7.0, 8.0], [1.0, 0.5, 0.2, 0.1, 0.05]);

            f = new ZiFilter<double>(MakeA());
            f.Change(new TransferFunction<double>([1.0, 2.0, 3.0, 4.0], [1.0, 0.1]));
            ExpectTf("ZiFilter<T>.Change with longer coefficients leaves Tf alone", f.Tf, bA, aA);
            Expect("ZiFilter<T>.Change with longer coefficients leaves the coefficients alone", SameOutput(f, new ZiFilter<double>(MakeA())));

            // the shorter numerator replaces a full-length one: no coefficient of the old numerator may survive
            double[] aLong = [1, 0.5, 0.2, 0.1, 0.05], bNew = [4, 5, 6], aNew = [1, 0.4, 0.3, 0.2, 0.1];
            var full = new ZiFilter<double>(new TransferFunction<double>([1.0, 2.0, 3.0, 4.0, 5.0], aLong.ToArray()));
            full.Change(new TransferFunction<double>(bNew.ToArray(), aNew.ToArray()));
            ExpectTf("ZiFilter<T>.Change with a shorter numerator updates Tf", full.Tf, bNew, aNew);
            Expect("ZiFilter<T>.Change with a shorter numerator clears the old coefficients", SameOutput(full, new ZiFilter<double>(new TransferFunction<double>(bNew.ToArray(), aNew.ToArray()))));
        }

        {
            var f = new FirFilter<double>(new TransferFunction<double>([1.0, 2.0, 3.0]));
            _ = f.Tf;
            f.ChangeKernel([4.0, 5.0, 6.0]);
            ExpectTf("FirFilter<T>.ChangeKernel updates the cached Tf", f.Tf, [4.0, 5.0, 6.0], [1.0]);
        }
    }

    // ---------------------------------------------------------------- argument validation

    static void ArgumentValidation()
    {
        // order
        ExpectThrows<ArgumentException>("Chebyshev II low-pass order 0", () => new Cheby2.LowPassFilter(0.1f, 0));
        ExpectThrows<ArgumentException>("Chebyshev II high-pass order -1", () => new Cheby2.HighPassFilter(0.1f, -1));
        ExpectThrows<ArgumentException>("Chebyshev II band-pass order 0", () => new Cheby2.BandPassFilter(0.1f, 0.2f, 0));
        ExpectThrows<ArgumentException>("Chebyshev II band-stop order 0", () => new Cheby2.BandStopFilter(0.1f, 0.2f, 0));
        ExpectThrows<ArgumentException>("Chebyshev II <double> low-pass order 0", () => new Cheby2.LowPassFilter64(0.1, 0));
        ExpectThrows<ArgumentException>("Chebyshev II <double> band-stop order -3", () => new Cheby2.BandStopFilter64(0.1, 0.2, -3));
        ExpectThrows<ArgumentException>("Chebyshev II prototype poles order 0", () => Cheby2.PrototypeChebyshevII.Poles(0));
        ExpectThrows<ArgumentException>("Chebyshev II prototype zeros order 0", () => Cheby2.PrototypeChebyshevII.Zeros(0));

        // the end points 0 and 0.5, NaN and infinity are no frequencies of a design
        foreach (var f in new[] { 0f, 0.5f, float.NaN, float.PositiveInfinity, float.NegativeInfinity, -0.1f, 0.6f })
        {
            ExpectThrows<ArgumentException>($"Chebyshev II low-pass f={f}", () => new Cheby2.LowPassFilter(f, 4));
            ExpectThrows<ArgumentException>($"Chebyshev II high-pass f={f}", () => new Cheby2.HighPassFilter(f, 4));
            ExpectThrows<ArgumentException>($"Chebyshev II <float> low-pass f={f}", () => new Cheby2.LowPassFilter<float>(f, 4));
            ExpectThrows<ArgumentException>($"Chebyshev II <float> high-pass f={f}", () => new Cheby2.HighPassFilter<float>(f, 4));
            ExpectThrows<ArgumentException>($"Butterworth low-pass f={f}", () => new Bw.LowPassFilter(f, 4));
            ExpectThrows<ArgumentException>($"Chebyshev I high-pass f={f}", () => new Cheby1.HighPassFilter(f, 4));
        }
        foreach (var (low, high) in new[] { (0f, 0.2f), (0.2f, 0.5f), (0f, 0.5f), (float.NaN, 0.2f), (0.1f, float.NaN), (0.2f, 0.1f), (0.2f, 0.2f) })
        {
            ExpectThrows<ArgumentException>($"Chebyshev II band-pass ({low}, {high})", () => new Cheby2.BandPassFilter(low, high, 2));
            ExpectThrows<ArgumentException>($"Chebyshev II band-stop ({low}, {high})", () => new Cheby2.BandStopFilter(low, high, 2));
            ExpectThrows<ArgumentException>($"Chebyshev II <float> band-pass ({low}, {high})", () => new Cheby2.BandPassFilter<float>(low, high, 2));
            ExpectThrows<ArgumentException>($"Chebyshev II <float> band-stop ({low}, {high})", () => new Cheby2.BandStopFilter<float>(low, high, 2));
            ExpectThrows<ArgumentException>($"Butterworth band-stop ({low}, {high})", () => new Bw.BandStopFilter(low, high, 2));
        }
        ExpectThrows<ArgumentException>("Chebyshev II <double> low-pass f=0", () => new Cheby2.LowPassFilter64(0, 4));
        ExpectThrows<ArgumentException>("Chebyshev II <double> low-pass f=0.5", () => new Cheby2.LowPassFilter64(0.5, 4));
        ExpectThrows<ArgumentException>("Chebyshev II <double> band-pass (0, 0.2)", () => new Cheby2.BandPassFilter64(0, 0.2, 2));

        // attenuation
        foreach (var rs in new[] { 0f, -5f, float.NaN, float.PositiveInfinity, 1e30f })
        {
            ExpectThrows<ArgumentException>($"Chebyshev II Rs={rs}", () => new Cheby2.LowPassFilter(0.1f, 4, rs));
            ExpectThrows<ArgumentException>($"Chebyshev II <float> Rs={rs}", () => new Cheby2.LowPassFilter<float>(0.1f, 4, rs));
        }
        ExpectThrows<ArgumentException>("Chebyshev II <double> Rs=+Inf", () => new Cheby2.LowPassFilter64(0.1, 4, double.PositiveInfinity));

        // what is valid stays valid, down to the practical limits
        ExpectNoThrow("Chebyshev II low-pass f=0.001", () => new Cheby2.LowPassFilter(0.001f, 2));
        ExpectNoThrow("Chebyshev II high-pass f=0.499", () => new Cheby2.HighPassFilter(0.499f, 2));
        ExpectNoThrow("Chebyshev II band-pass (0.001, 0.499)", () => new Cheby2.BandPassFilter(0.001f, 0.499f, 1));
        ExpectNoThrow("Chebyshev II band-stop (0.001, 0.499)", () => new Cheby2.BandStopFilter(0.001f, 0.499f, 1));
        ExpectNoThrow("Chebyshev II order 1", () => new Cheby2.LowPassFilter(0.2f, 1));
        ExpectNoThrow("Chebyshev II Rs=120", () => new Cheby2.LowPassFilter64(0.2, 3, 120));
    }

    // ---------------------------------------------------------------- band-pass transformation

    static void BandPassMatchesScipy()
    {
        // scipy.signal.butter(N, [0.002, 0.98], 'bandpass', output='zpk') and scipy.signal.cheby2(4, 40, [0.002, 0.98], 'bandpass', output='zpk'):
        // 0.001..0.49 cycles/sample is so wide that the second root of every pole / zero pair, alpha * (1 - beta) with beta close to 1, lost its
        // digits in single precision: the roots were off by up to 1.9e-6 (poles) and 6.1e-6 (zeros), now they are within 1.3e-7.
        (double Re, double Im)[] butterworth3 =
        [
            (0.9968488956272019, 0.005424858550372927), (0.9937352382684428, 0.0), (-0.9390508364711233, 0.0), (-0.9676499165407313, 0.05272829308703197),
            (0.9968488956272019, -0.005424858550372927), (-0.9676499165407313, -0.05272829308703197),
        ];
        (double Re, double Im)[] butterworth4 =
        [
            (0.9975819536414908, 0.005791280834265685), (0.9942082008580504, 0.0023913822970949386), (-0.9432969580790134, 0.02271886163489493), (-0.9746110624249316, 0.05665310575063779),
            (0.9975819536414908, -0.005791280834265685), (0.9942082008580504, -0.0023913822970949386), (-0.9432969580790134, -0.02271886163489493), (-0.9746110624249316, -0.05665310575063779),
        ];
        (double Re, double Im)[] chebyshev2Poles =
        [
            (0.9957432279890096, 0.01163616932581023), (0.989894685154096, 0.004795939701034551), (-0.902406317598949, 0.04387225499103637), (-0.9524744931734477, 0.11176838762160543),
            (0.9957432279890096, -0.01163616932581023), (0.989894685154096, -0.004795939701034551), (-0.902406317598949, -0.04387225499103637), (-0.9524744931734477, -0.11176838762160543),
        ];
        (double Re, double Im)[] chebyshev2Zeros =
        [
            (0.9999971087576925, 0.0024046779941082523), (0.9999831510753044, 0.005804960422333201), (-0.998315415294006, 0.05802009640078342), (-0.9997107287570521, 0.024051170616900677),
            (0.9999971087576925, -0.0024046779941082523), (0.9999831510753044, -0.005804960422333201), (-0.998315415294006, -0.05802009640078342), (-0.9997107287570521, -0.024051170616900677),
        ];

        static double Distance(ComplexFp32 a, (double Re, double Im) b) => SysComplex.Abs(new SysComplex(a.Real - b.Re, a.Imaginary - b.Im));

        void Compare(string name, ComplexFp32[] design, (double Re, double Im)[] reference)
        {
            Expect($"{name}: number of roots", design.Length == reference.Length, $"{design.Length} instead of {reference.Length}");

            // every root of SciPy has a root of the design next to it and the other way round
            var worst = Math.Max(reference.Max(r => design.Min(d => Distance(d, r))), design.Max(d => reference.Min(r => Distance(d, r))));
            Expect($"{name}: roots equal SciPy's", worst < 3e-7, $"the worst root is {worst:E2} away");
        }

        Compare("Butterworth band-pass 0.001..0.49, order 3, poles", new Bw.BandPassFilter(0.001f, 0.49f, 3).Tf.Poles, butterworth3);
        Compare("Butterworth band-pass 0.001..0.49, order 4, poles", new Bw.BandPassFilter(0.001f, 0.49f, 4).Tf.Poles, butterworth4);

        var chebyshev2 = new Cheby2.BandPassFilter(0.001f, 0.49f, 4, 40f).Tf;
        Compare("Chebyshev II band-pass 0.001..0.49, order 4, 40 dB, poles", chebyshev2.Poles, chebyshev2Poles);
        Compare("Chebyshev II band-pass 0.001..0.49, order 4, 40 dB, zeros", chebyshev2.Zeros, chebyshev2Zeros);
    }

    static void BandPassSecondRootGeneric()
    {
        // The generic transformation is private and the filters keep only the coefficients of its result, so the roots are read where they
        // are made: before the bilinear transformation. The wide bands are the ones whose second root cancels (1 - beta, beta close to 1).
        var single = WorstSecondRootError(0.001f, 0.499f, 4);
        Expect("band-pass second roots (<float>) 0.001..0.499", single < 1e-5, $"relative error {single:E2}");

        var wide = WorstSecondRootError(1e-6, 0.5 - 1e-6, 4);
        Expect("band-pass second roots (<double>) 1e-6..0.499999", wide < 1e-9, $"relative error {wide:E2}");

        var narrow = WorstSecondRootError(0.1, 0.2, 5);
        Expect("band-pass second roots (<double>) 0.1..0.2", narrow < 1e-12, $"relative error {narrow:E2}");
    }

    // Relative error of the second roots of the Butterworth poles that DesignFilter.IirBpTf<T> makes, against the same transformation in double precision
    // from the same warped frequencies, with the second root as the product f0² of both roots divided by the first one.
    static double WorstSecondRootError<T>(T low, T high, int order)
        where T : unmanaged, System.Numerics.IFloatingPointIeee754<T>, System.Numerics.IMinMaxValue<T>
    {
        var transform = typeof(DesignFilter).GetMethod("SpectralTransformBp", BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(typeof(T));
        var poles = Bw.PrototypeButterworth.Poles<T>(order);
        var arguments = new object?[] { poles, low, high, null, null };
        transform.Invoke(null, arguments);
        var re = (T[])arguments[3]!;
        var im = (T[])arguments[4]!;

        var w1 = double.CreateChecked(T.Tan(T.Pi * low));
        var w2 = double.CreateChecked(T.Tan(T.Pi * high));
        var f0 = Math.Sqrt(w1 * w2);
        var bw = w2 - w1;

        var worst = 0.0;
        for (var k = 0; k < poles.Length; k++)
        {
            var alpha = bw / 2 * new SysComplex(double.CreateChecked(poles[k].Real), double.CreateChecked(poles[k].Imaginary));
            var beta = SysComplex.Sqrt(1 - SysComplex.Pow(f0 / alpha, 2));
            var second = f0 * f0 / (alpha * (1 + beta));
            var designed = new SysComplex(double.CreateChecked(re[poles.Length + k]), double.CreateChecked(im[poles.Length + k]));
            worst = Math.Max(worst, SysComplex.Abs(designed - second) / SysComplex.Abs(second));
        }

        return worst;
    }

    // ---------------------------------------------------------------- order of the other prototypes

    static void PrototypeOrderValidation()
    {
        foreach (var order in new[] { 0, -1, -7 })
        {
            void Rejects(string what, Action make) => ExpectThrows<ArgumentOutOfRangeException>($"{what}, order {order}", make);

            // the prototypes
            Rejects("Butterworth prototype poles", () => Bw.PrototypeButterworth.Poles(order));
            Rejects("Butterworth <float> prototype poles", () => Bw.PrototypeButterworth.Poles<float>(order));
            Rejects("Butterworth <double> prototype poles", () => Bw.PrototypeButterworth.Poles<double>(order));
            Rejects("Chebyshev I prototype poles", () => Cheby1.PrototypeChebyshevI.Poles(order));
            Rejects("Chebyshev I <float> prototype poles", () => Cheby1.PrototypeChebyshevI.Poles<float>(order));
            Rejects("Chebyshev I <double> prototype poles", () => Cheby1.PrototypeChebyshevI.Poles<double>(order));
            Rejects("Elliptic prototype poles", () => Ell.PrototypeElliptic.Poles(order));
            Rejects("Elliptic prototype zeros", () => Ell.PrototypeElliptic.Zeros(order, 1f, 20f));
            Rejects("Elliptic prototype zeros (double ripples)", () => Ell.PrototypeElliptic.Zeros(order, 1.0, 20.0));
            Rejects("Elliptic <float> prototype poles", () => Ell.PrototypeElliptic.Poles<float>(order));
            Rejects("Elliptic <float> prototype zeros", () => Ell.PrototypeElliptic.Zeros<float>(order));
            Rejects("Elliptic <double> prototype poles", () => Ell.PrototypeElliptic.Poles<double>(order));
            Rejects("Elliptic <double> prototype zeros", () => Ell.PrototypeElliptic.Zeros<double>(order));
            Rejects("Bessel prototype poles", () => Bes.PrototypeBessel.Poles(order));
            Rejects("Bessel <float> prototype poles", () => Bes.PrototypeBessel.Poles<float>(order));
            Rejects("Bessel <double> prototype poles", () => Bes.PrototypeBessel.Poles<double>(order));

            // the filters
            Rejects("Butterworth low-pass", () => new Bw.LowPassFilter(0.1f, order));
            Rejects("Butterworth high-pass", () => new Bw.HighPassFilter(0.1f, order));
            Rejects("Butterworth band-pass", () => new Bw.BandPassFilter(0.1f, 0.2f, order));
            Rejects("Butterworth band-stop", () => new Bw.BandStopFilter(0.1f, 0.2f, order));
            Rejects("Butterworth <float> low-pass", () => new Bw.LowPassFilter<float>(0.1f, order));
            Rejects("Butterworth <double> band-pass", () => new Bw.BandPassFilter64(0.1, 0.2, order));
            Rejects("Chebyshev I low-pass", () => new Cheby1.LowPassFilter(0.1f, order));
            Rejects("Chebyshev I high-pass", () => new Cheby1.HighPassFilter(0.1f, order));
            Rejects("Chebyshev I band-pass", () => new Cheby1.BandPassFilter(0.1f, 0.2f, order));
            Rejects("Chebyshev I band-stop", () => new Cheby1.BandStopFilter(0.1f, 0.2f, order));
            Rejects("Chebyshev I <float> high-pass", () => new Cheby1.HighPassFilter<float>(0.1f, order));
            Rejects("Chebyshev I <double> band-stop", () => new Cheby1.BandStopFilter64(0.1, 0.2, order));
            Rejects("Elliptic low-pass", () => new Ell.LowPassFilter(0.1f, order));
            Rejects("Elliptic high-pass", () => new Ell.HighPassFilter(0.1f, order));
            Rejects("Elliptic band-pass", () => new Ell.BandPassFilter(0.1f, 0.2f, order));
            Rejects("Elliptic band-stop", () => new Ell.BandStopFilter(0.1f, 0.2f, order));
            Rejects("Elliptic <float> low-pass", () => new Ell.LowPassFilter<float>(0.1f, order));
            Rejects("Elliptic <double> band-pass", () => new Ell.BandPassFilter64(0.1, 0.2, order));
            Rejects("Bessel low-pass", () => new Bes.LowPassFilter(0.1f, order));
            Rejects("Bessel high-pass", () => new Bes.HighPassFilter(0.1f, order));
            Rejects("Bessel band-pass", () => new Bes.BandPassFilter(0.1f, 0.2f, order));
            Rejects("Bessel band-stop", () => new Bes.BandStopFilter(0.1f, 0.2f, order));
            Rejects("Bessel <float> high-pass", () => new Bes.HighPassFilter<float>(0.1f, order));
            Rejects("Bessel <double> band-stop", () => new Bes.BandStopFilter64(0.1, 0.2, order));
        }

        // order 1 is the smallest design
        ExpectNoThrow("Butterworth order 1", () => new Bw.LowPassFilter(0.2f, 1));
        ExpectNoThrow("Chebyshev I order 1", () => new Cheby1.HighPassFilter(0.2f, 1));
        ExpectNoThrow("Elliptic order 1", () => new Ell.LowPassFilter(0.2f, 1));
        ExpectNoThrow("Bessel order 1", () => new Bes.BandPassFilter(0.1f, 0.2f, 1));
        ExpectNoThrow("Butterworth <double> order 1", () => new Bw.LowPassFilter64(0.2, 1));
        ExpectNoThrow("Elliptic <double> order 1", () => new Ell.HighPassFilter64(0.2, 1));
    }

    // ---------------------------------------------------------------- Change() with a design that is rejected

    static void RejectedChangeLeavesTheFilterAsItWas()
    {
        // The design is validated while it is made. The properties of the filter (Frequency, Ripple, ...) have to follow only a design that was accepted:
        // they were assigned first, so a rejected frequency stayed in them while the coefficients were still the old ones.
        string?[] spaces =
        [
            typeof(Bw.LowPassFilter).Namespace, typeof(Cheby1.LowPassFilter).Namespace, typeof(Cheby2.LowPassFilter).Namespace,
            typeof(Ell.LowPassFilter).Namespace, typeof(Bes.LowPassFilter).Namespace,
        ];
        Dictionary<string, string> propertyOf = new()
        {
            ["frequency"] = "Frequency", ["frequencyLow"] = "FrequencyLow", ["frequencyHigh"] = "FrequencyHigh",
            ["ripple"] = "Ripple", ["ripplePass"] = "RipplePassband", ["rippleStop"] = "RippleStopband",
        };

        var exercised = 0;
        foreach (var definition in typeof(Bw.LowPassFilter).Assembly.GetTypes())
        {
            if (!definition.IsPublic || !definition.IsClass || definition.IsAbstract || !spaces.Contains(definition.Namespace) || definition.Name.StartsWith("Prototype"))
                continue;

            var types = definition.IsGenericTypeDefinition
                ? new[] { definition.MakeGenericType(typeof(float)), definition.MakeGenericType(typeof(double)) }
                : [definition];

            foreach (var type in types)
            {
                // the 64 bit classes inherit Change from the generic class
                var change = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SingleOrDefault(m => m.Name == "Change");
                if (change is null)
                    continue;

                var family = definition.Namespace![(definition.Namespace.LastIndexOf('.') + 1)..];
                var label = $"{family}.{definition.Name.Split('`')[0]}{(definition.IsGenericTypeDefinition ? $"<{type.GenericTypeArguments[0].Name}>" : "")}.Change";
                exercised++;

                var parameters = change.GetParameters();
                var band = parameters.Any(p => p.Name == "frequencyLow");

                var ctor = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
                var filter = Unwrap(() => ctor.Invoke(DesignArguments(ctor.GetParameters(), new() { ["frequency"] = 0.1, ["frequencyLow"] = 0.1, ["frequencyHigh"] = 0.2, ["order"] = 3 })))!;
                var properties = PropertiesOf(filter);
                var coefficients = CoefficientsOf(filter);

                // the ripples differ from the ones of the filter: when they are assigned before the design is rejected, the properties show it
                Dictionary<string, double> Rejected(double low, double high) => band
                    ? new() { ["frequencyLow"] = low, ["frequencyHigh"] = high, ["ripple"] = 33, ["ripplePass"] = 2, ["rippleStop"] = 40 }
                    : new() { ["frequency"] = low, ["ripple"] = 33, ["ripplePass"] = 2, ["rippleStop"] = 40 };

                foreach (var (what, values) in band
                    ? new[] { ("NaN", Rejected(double.NaN, 0.2)), ("the end point 0.5", Rejected(0.1, 0.5)), ("reversed edges", Rejected(0.3, 0.2)) }
                    : [("NaN", Rejected(double.NaN, 0)), ("the end point 0.5", Rejected(0.5, 0)), ("the end point 0", Rejected(0, 0))])
                {
                    ExpectThrows<ArgumentException>($"{label}({what})", () => Unwrap(() => change.Invoke(filter, DesignArguments(parameters, values))));
                    Expect($"{label}({what}): the properties are as they were", PropertiesOf(filter) == properties, $"{PropertiesOf(filter)} instead of {properties}");
                    Expect($"{label}({what}): the coefficients are as they were", CoefficientsOf(filter) == coefficients);
                }

                // an accepted design is followed by the properties and by the coefficients
                var accepted = new Dictionary<string, double>
                {
                    ["frequency"] = 0.15, ["frequencyLow"] = 0.12, ["frequencyHigh"] = 0.25, ["ripple"] = 25, ["ripplePass"] = 2, ["rippleStop"] = 40,
                };
                ExpectNoThrow($"{label}(valid)", () => Unwrap(() => change.Invoke(filter, DesignArguments(parameters, accepted))));
                foreach (var p in parameters.Where(p => p.Name is not null && propertyOf.ContainsKey(p.Name)))
                {
                    var actual = Convert.ToDouble(type.GetProperty(propertyOf[p.Name!])!.GetValue(filter), CultureInfo.InvariantCulture);
                    Expect($"{label}(valid): {propertyOf[p.Name!]} follows", Math.Abs(actual - accepted[p.Name!]) < 1e-6, $"{actual} instead of {accepted[p.Name!]}");
                }
                Expect($"{label}(valid): the coefficients follow", CoefficientsOf(filter) != coefficients);
            }
        }

        // 20 single precision classes, 20 generic classes in single and in double precision
        Expect("every Change method of the pole filters is exercised", exercised == 60, $"{exercised} of 60");
    }

    static object?[] DesignArguments(ParameterInfo[] parameters, Dictionary<string, double> values)
    {
        var arguments = new object?[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            var target = Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType;
            arguments[i] = p.Name is not null && values.TryGetValue(p.Name, out var value)
                ? Convert.ChangeType(value, target, CultureInfo.InvariantCulture)
                : p.DefaultValue is DBNull ? null : p.DefaultValue;
        }
        return arguments;
    }

    static object? Unwrap(Func<object?> call)
    {
        try
        {
            return call();
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    static string PropertiesOf(object filter)
    {
        var type = filter.GetType();
        return string.Join(";", type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0 && p.DeclaringType!.Namespace == type.Namespace)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => $"{p.Name}={Convert.ToString(p.GetValue(filter), CultureInfo.InvariantCulture)}"));
    }

    static string CoefficientsOf(object filter)
    {
        var tf = filter.GetType().GetProperty("Tf")!.GetValue(filter)!;
        string Join(string name) => string.Join(",", ((System.Collections.IEnumerable)tf.GetType().GetProperty(name)!.GetValue(tf)!)
            .Cast<object>().Select(v => Convert.ToString(v, CultureInfo.InvariantCulture)));
        return Join("Numerator") + "|" + Join("Denominator");
    }

    // ---------------------------------------------------------------- helpers

    static bool SameOutput(ZiFilter p, ZiFilter q)
    {
        var rng = new Random(5);
        for (var i = 0; i < 64; i++)
        {
            var x = (float)rng.NextDouble();
            if (p.Process(x) != q.Process(x))
                return false;
        }
        return true;
    }

    static bool SameOutput(ZiFilter<double> p, ZiFilter<double> q)
    {
        var rng = new Random(5);
        for (var i = 0; i < 64; i++)
        {
            var x = rng.NextDouble();
            if (p.Process(x) != q.Process(x))
                return false;
        }
        return true;
    }

    static void ExpectNoThrow(string name, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Expect(name, false, $"threw {ex.GetType().Name}: {ex.Message}");
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
