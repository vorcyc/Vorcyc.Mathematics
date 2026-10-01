using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;
using Vorcyc.Mathematics.SignalProcessing.Operations;
using Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;
using Vorcyc.Mathematics.SignalProcessing.Windowing;
using Cheby1 = Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;
using Cheby2 = Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;
using Ellip = Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic;

namespace SP_module_test;

/// <summary>
/// 0.10.23 audit regressions: Goertzel bin, IirCombPeak gain, window length 1 / Gaussian symmetry,
/// Chebyshev-II stopband attenuation, order-1 Elliptic, Ols block convolver with a long kernel.
/// </summary>
internal static class Audit_023_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing 0.10.23 audit regressions (signal processing)...");

        AssertGoertzelMatchesDft();
        AssertCombPeakGain();
        AssertWindows();
        AssertChebyshevIIAttenuation();
        AssertEllipticOrderOne();
        AssertOlsLongKernel();

        if (_failures != 0)
            throw new InvalidOperationException($"Audit 0.10.23 (SP): {_failures} assertion(s) failed.");

        Console.WriteLine("Audit_023_test: PASS");
    }

    static void AssertGoertzelMatchesDft()
    {
        const int n = 64;
        var rng = new Random(23);
        var x = new double[n];
        for (int i = 0; i < n; i++)
            x[i] = rng.NextDouble() * 2 - 1;

        var g = new Goertzel(n);
        double worst = 0;
        foreach (var k in new[] { 0, 1, 5, 13, 31, 32 })
        {
            Complex dft = 0;
            for (int i = 0; i < n; i++)
                dft += x[i] * Complex.FromPolarCoordinates(1, -2 * Math.PI * k * i / n);
            worst = Math.Max(worst, (g.Direct(x, k) - dft).Magnitude);
        }
        Expect("Goertzel (double) equals the DFT bin", worst < 1e-9, $"maxΔ={worst:E2}");

        var xf = x.Select(v => (float)v).ToArray();
        var gf = g.Direct(xf, 5);
        Complex d5 = 0;
        for (int i = 0; i < n; i++)
            d5 += xf[i] * Complex.FromPolarCoordinates(1, -2 * Math.PI * 5 * i / n);
        Expect("Goertzel (float) equals the DFT bin", (new Complex(gf.Real, gf.Imaginary) - d5).Magnitude < 1e-3);
    }

    static void AssertCombPeakGain()
    {
        const int n = 10;
        var tf = DesignFilter.IirCombPeak(1f / n, 2f / (n * 0.05f));
        var b = tf.Numerator.Select(v => (double)v).ToArray();
        var a = tf.Denominator.Select(v => (double)v).ToArray();
        Expect("IirCombPeak gain at DC is 1", Math.Abs(Mag(b, a, 1e-9) - 1) < 1e-3);
        Expect("IirCombPeak gain at the 1st harmonic is 1", Math.Abs(Mag(b, a, 2 * Math.PI / n) - 1) < 1e-3);
        Expect("IirCombPeak gain between the teeth is ~0", Mag(b, a, Math.PI / n) < 1e-2);
    }

    static void AssertWindows()
    {
        foreach (var (name, w) in new (string, float[])[]
        {
            ("Hamming", WindowBuilder.Hamming(1)), ("Blackman", WindowBuilder.Blackman(1)),
            ("Hann", WindowBuilder.Hann(1)), ("Gaussian", WindowBuilder.Gaussian(1)),
            ("Kaiser", WindowBuilder.Kaiser(1)), ("BartlettHann", WindowBuilder.BartlettHann(1)),
            ("Lanczos", WindowBuilder.Lanczos(1)), ("Flattop", WindowBuilder.Flattop(1)),
        })
        {
            Expect($"{name}(1) == [1]", w.Length == 1 && w[0] == 1f, w.Length == 1 ? $"got {w[0]}" : $"len {w.Length}");
        }

        foreach (var len in new[] { 8, 9, 64 })
        {
            var g = WindowBuilder.Gaussian(len);
            bool symmetric = true;
            for (int i = 0; i < len; i++)
                symmetric &= Math.Abs(g[i] - g[len - 1 - i]) < 1e-6f;
            Expect($"Gaussian({len}) is symmetric", symmetric);
        }
    }

    static void AssertChebyshevIIAttenuation()
    {
        foreach (var (order, rs, f) in new[] { (4, 40.0, 0.2), (5, 40.0, 0.2), (6, 60.0, 0.1) })
        {
            var tf = new Cheby2.LowPassFilter64(f, order, rs).Tf;
            var b = tf.Numerator.ToArray();
            var a = tf.Denominator.ToArray();
            double wc = 2 * Math.PI * f;
            double edgeDb = Db(Mag(b, a, wc));
            double stopMax = double.NegativeInfinity;
            for (int i = 0; i <= 200; i++)
                stopMax = Math.Max(stopMax, Db(Mag(b, a, wc + (Math.PI - wc) * i / 200.0)));
            Expect($"Chebyshev-II LP N={order} Rs={rs}: DC gain is 1", Math.Abs(Mag(b, a, 1e-9) - 1) < 2e-3);
            Expect($"Chebyshev-II LP N={order} Rs={rs}: -Rs dB at the stopband edge", Math.Abs(edgeDb + rs) < 0.15, $"edge={edgeDb:F2} dB");
            Expect($"Chebyshev-II LP N={order} Rs={rs}: stopband stays below -Rs", stopMax < -rs + 0.15, $"max={stopMax:F2} dB");
        }

        var hp = new Cheby2.HighPassFilter64(0.2, 4, 40).Tf;
        var hb = hp.Numerator.ToArray();
        var ha = hp.Denominator.ToArray();
        Expect("Chebyshev-II HP: Nyquist gain is 1", Math.Abs(Mag(hb, ha, Math.PI - 1e-9) - 1) < 2e-3);
        Expect("Chebyshev-II HP: -Rs dB at the stopband edge", Math.Abs(Db(Mag(hb, ha, 2 * Math.PI * 0.2)) + 40) < 0.15);

        var def = new Cheby2.LowPassFilter64(0.2, 4).Tf;
        Expect("Chebyshev-II default attenuation is 20 dB",
            Math.Abs(Db(Mag(def.Numerator.ToArray(), def.Denominator.ToArray(), 2 * Math.PI * 0.2)) + 20) < 0.15);

        try
        {
            Cheby2.PrototypeChebyshevII.Poles(4, 0f);
            Expect("Chebyshev-II Poles(ripple = 0) throws", false);
        }
        catch (ArgumentOutOfRangeException)
        {
            Expect("Chebyshev-II Poles(ripple = 0) throws", true);
        }
    }

    static void AssertEllipticOrderOne()
    {
        var lp = new Ellip.LowPassFilter64(0.2, 1, 1, 40).Tf;
        var bp = new Ellip.BandPassFilter64(0.1, 0.25, 1, 1, 40).Tf;
        var bs = new Ellip.BandStopFilter64(0.1, 0.25, 1, 1, 40).Tf;
        foreach (var (name, tf) in new[] { ("LP", lp), ("BP", bp), ("BS", bs) })
        {
            Expect($"Elliptic {name} order 1: finite coefficients",
                tf.Numerator.All(double.IsFinite) && tf.Denominator.All(double.IsFinite));
        }

        // The order-1 elliptic design is the order-1 Chebyshev-I design.
        var c1 = new Cheby1.LowPassFilter64(0.2, 1, 1).Tf;
        double worst = 0;
        for (int i = 1; i < 100; i++)
        {
            double w = Math.PI * i / 100.0;
            worst = Math.Max(worst, Math.Abs(Mag(lp.Numerator.ToArray(), lp.Denominator.ToArray(), w) - Mag(c1.Numerator.ToArray(), c1.Denominator.ToArray(), w)));
        }
        Expect("Elliptic order 1 equals Chebyshev-I order 1", worst < 1e-3, $"maxΔ={worst:E2}");
    }

    static void AssertOlsLongKernel()
    {
        // Kernel longer than half of the FFT size used to overrun the block buffer.
        var rng = new Random(2310);
        var x = new float[200];
        var k = new float[40];
        for (int i = 0; i < x.Length; i++) x[i] = (float)(rng.NextDouble() * 2 - 1);
        for (int i = 0; i < k.Length; i++) k[i] = (float)(rng.NextDouble() * 2 - 1);
        const int blockFft = 64;

        var expected = new Convolver<float>().Convolve(x, k);
        var ols = new OlsBlockConvolver(k, blockFft).ApplyTo(Signal.FromCopy(x, 1)).Samples.ToArray();
        Expect("OlsBlockConvolver (kernel 40, fft 64) length", ols.Length == expected.Length, $"{ols.Length} vs {expected.Length}");
        Expect("OlsBlockConvolver (kernel 40, fft 64) values", MaxDiff(expected, ols) < 1e-4f, $"maxΔ={MaxDiff(expected, ols):E2}");

        var olsG = new OlsBlockConvolver<float>(k, blockFft).ApplyTo(x);
        Expect("OlsBlockConvolver<float> (kernel 40, fft 64) values", MaxDiff(expected, olsG) < 1e-4f, $"maxΔ={MaxDiff(expected, olsG):E2}");
    }

    static float MaxDiff(float[] a, float[] b)
    {
        float max = 0;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            max = Math.Max(max, Math.Abs(a[i] - b[i]));
        return max;
    }

    static double Mag(double[] num, double[] den, double w)
    {
        Complex n = 0, d = 0;
        for (int i = 0; i < num.Length; i++) n += num[i] * Complex.FromPolarCoordinates(1, -w * i);
        for (int i = 0; i < den.Length; i++) d += den[i] * Complex.FromPolarCoordinates(1, -w * i);
        return (n / d).Magnitude;
    }

    static double Db(double magnitude) => 20 * Math.Log10(Math.Max(magnitude, 1e-300));

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }
}
