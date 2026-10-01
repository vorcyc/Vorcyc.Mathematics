using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Operations;
using Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace SP_module_test;

/// <summary>0.10.19: Convolver&lt;T&gt; / Operation.Convolve&lt;T&gt; / CrossCorrelate&lt;T&gt; stay in T.</summary>
internal static class Conv_019_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing Convolver<T> / Operation.Convolve<T> (0.10.19)...");

        AssertFloatMatchesClassic();
        AssertKnownImpulse();
        AssertXcorrIsReversedConv();
        AssertDoubleVsDirect();
        AssertOperationGeneric();
        AssertComplexVsDirect();
        AssertOlaOlsVsConvolver();
        AssertOlaOlsGeneric();
        AssertComplexGeneric();
        AssertLongSequence();

        if (_failures != 0)
            throw new InvalidOperationException($"Conv 0.10.19: {_failures} assertion(s) failed.");

        Console.WriteLine("Conv_019_test: PASS");
    }

    static void AssertFloatMatchesClassic()
    {
        var rng = new Random(19);
        var x = new float[64];
        var k = new float[9];
        for (int i = 0; i < x.Length; i++)
            x[i] = (float)(rng.NextDouble() * 2 - 1);
        for (int i = 0; i < k.Length; i++)
            k[i] = (float)(rng.NextDouble() * 2 - 1);

        var classic = new Convolver().Convolve(Signal.FromCopy(x, 1), Signal.FromCopy(k, 1)).Samples.ToArray();
        var generic = new Convolver<float>().Convolve(x, k);
        ExpectClose("Convolver<float> ≡ Convolver", classic, generic, 0);

        var xc1 = new Convolver().CrossCorrelate(Signal.FromCopy(x, 1), Signal.FromCopy(k, 1)).Samples.ToArray();
        var xc2 = new Convolver<float>().CrossCorrelate(x, k);
        ExpectClose("Convolver<float> xcorr ≡ Convolver", xc1, xc2, 0);
    }

    static void AssertKnownImpulse()
    {
        float[] x = [1, 2, 3];
        float[] k = [4, 5];
        // 4, 8+5, 12+10, 15
        float[] expect = [4, 13, 22, 15];
        var y = new Convolver<float>().Convolve(x, k);
        ExpectClose("known [1,2,3]*[4,5]", expect, y, 1e-5f);
    }

    static void AssertXcorrIsReversedConv()
    {
        float[] a = [0.2f, -0.1f, 0.4f, 0.3f];
        float[] b = [1f, 0.5f, -0.25f];
        var rev = new float[b.Length];
        for (int i = 0; i < b.Length; i++)
            rev[i] = b[b.Length - 1 - i];

        var xc = new Convolver<float>().CrossCorrelate(a, b);
        var conv = new Convolver<float>().Convolve(a, rev);
        ExpectClose("xcorr ≡ conv(reverse)", conv, xc, 0);
    }

    static void AssertDoubleVsDirect()
    {
        var rng = new Random(190);
        var x = new double[48];
        var k = new double[7];
        for (int i = 0; i < x.Length; i++)
            x[i] = rng.NextDouble() * 2 - 1;
        for (int i = 0; i < k.Length; i++)
            k[i] = rng.NextDouble() * 2 - 1;

        var fft = new Convolver64().Convolve(x, k);
        var direct = DirectConv(x, k);
        ExpectClose("Convolver64 vs direct", direct, fft, 1e-12);

        var xc = new Convolver<double>().CrossCorrelate(x, k);
        Expect("double xcorr finite", xc.All(double.IsFinite));
    }

    static void AssertOperationGeneric()
    {
        float[] xf = [1, 0, -1, 0.5f];
        float[] kf = [0.5f, 0.5f];
        var viaOp = Operation.Convolve(xf, kf);
        var viaT = Operation.Convolve<float>(xf, kf);
        ExpectClose("Operation.Convolve(float[]) ≡ Convolve<float>", viaOp, viaT, 0);

        double[] xd = [1, 0, -1, 0.5];
        double[] kd = [0.5, 0.5];
        var yd = Operation.Convolve(xd, kd);
        Expect("Operation.Convolve<double> length", yd.Length == xd.Length + kd.Length - 1);
        Expect("Operation.CrossCorrelate<double> length",
            Operation.CrossCorrelate(xd, kd).Length == xd.Length + kd.Length - 1);
    }

    static void AssertComplexVsDirect()
    {
        var rng = new Random(191);
        var aRe = new float[32];
        var aIm = new float[32];
        var bRe = new float[8];
        var bIm = new float[8];
        Fill(rng, aRe);
        Fill(rng, aIm);
        Fill(rng, bRe);
        Fill(rng, bIm);

        var a = new ComplexDiscreteSignalFp32(1, aRe, aIm, allocateNew: true);
        var b = new ComplexDiscreteSignalFp32(1, bRe, bIm, allocateNew: true);
        var y = new ComplexConvolver().Convolve(a, b);
        DirectComplexConv(aRe, aIm, bRe, bIm, out var dRe, out var dIm);
        ExpectClose("ComplexConvolver conv Re vs direct", dRe, y.Real, 2e-5f);
        ExpectClose("ComplexConvolver conv Im vs direct", dIm, y.Imag, 2e-5f);

        var xc = new ComplexConvolver().CrossCorrelate(a, b);
        var revRe = new float[bRe.Length];
        var revIm = new float[bIm.Length];
        for (int i = 0; i < bRe.Length; i++)
        {
            revRe[i] = bRe[bRe.Length - 1 - i];
            revIm[i] = bIm[bIm.Length - 1 - i];
        }
        DirectComplexConv(aRe, aIm, revRe, revIm, out var xRe, out var xIm);
        ExpectClose("ComplexConvolver xcorr Re vs reverse-conv", xRe, xc.Real, 2e-5f);
        ExpectClose("ComplexConvolver xcorr Im vs reverse-conv", xIm, xc.Imag, 2e-5f);
    }

    static void AssertOlaOlsVsConvolver()
    {
        var rng = new Random(192);
        var x = new float[200];
        var k = new float[11];
        Fill(rng, x);
        Fill(rng, k);

        var fft = new Convolver<float>().Convolve(x, k);
        var sig = Signal.FromCopy(x, 1);
        const int blockFft = 64;

        var ola = new OlaBlockConvolver(k, blockFft).ApplyTo(sig).Samples.ToArray();
        var ols = new OlsBlockConvolver(k, blockFft).ApplyTo(sig).Samples.ToArray();
        ExpectClose("OLA vs Convolver<float>", fft, ola, 2e-5f);
        ExpectClose("OLS vs Convolver<float>", fft, ols, 2e-5f);

        var k64 = new double[k.Length];
        for (int i = 0; i < k.Length; i++)
            k64[i] = k[i];
        var olaD = new OlaBlockConvolver(k64, blockFft).ApplyTo(sig).Samples.ToArray();
        ExpectClose("OLA(double ctor) ≡ OLA(float)", ola, olaD, 0);
    }

    static void AssertOlaOlsGeneric()
    {
        var rng = new Random(194);
        var x = new float[200];
        var k = new float[11];
        Fill(rng, x);
        Fill(rng, k);
        const int blockFft = 64;
        var sig = Signal.FromCopy(x, 1);

        var ola32 = new OlaBlockConvolver(k, blockFft).ApplyTo(sig).Samples.ToArray();
        var olaG = new OlaBlockConvolver<float>(k, blockFft).ApplyTo(x);
        ExpectClose("Ola<float> ≡ OlaBlockConvolver", ola32, olaG, 0);

        var ols32 = new OlsBlockConvolver(k, blockFft).ApplyTo(sig).Samples.ToArray();
        var olsG = new OlsBlockConvolver<float>(k, blockFft).ApplyTo(x);
        ExpectClose("Ols<float> ≡ OlsBlockConvolver", ols32, olsG, 0);

        var xd = new double[x.Length];
        var kd = new double[k.Length];
        for (int i = 0; i < x.Length; i++)
            xd[i] = x[i];
        for (int i = 0; i < k.Length; i++)
            kd[i] = k[i];

        var fft64 = new Convolver64().Convolve(xd, kd);
        var ola64 = new OlaBlockConvolver64(kd, blockFft).ApplyTo(xd);
        var ols64 = Operation.BlockConvolve(xd, kd, blockFft, FilteringMethod.OverlapSave);
        ExpectClose("Ola64 vs Convolver64", fft64, ola64, 1e-12);
        ExpectClose("Ols64 vs Convolver64", fft64, ols64, 1e-12);

        var fir = new FirFilter<float>(k);
        var fromFir = OlaBlockConvolver<float>.FromFilter(fir, blockFft).ApplyTo(x);
        ExpectClose("Ola.FromFilter ≡ Ola<float>", olaG, fromFir, 0);
    }

    static void AssertComplexGeneric()
    {
        var rng = new Random(195);
        var aRe = new float[32];
        var aIm = new float[32];
        var bRe = new float[8];
        var bIm = new float[8];
        Fill(rng, aRe);
        Fill(rng, aIm);
        Fill(rng, bRe);
        Fill(rng, bIm);

        var a32 = new ComplexDiscreteSignalFp32(1, aRe, aIm, allocateNew: true);
        var b32 = new ComplexDiscreteSignalFp32(1, bRe, bIm, allocateNew: true);
        var aT = new ComplexDiscreteSignal<float>(1, (float[])aRe.Clone(), (float[])aIm.Clone());
        var bT = new ComplexDiscreteSignal<float>(1, (float[])bRe.Clone(), (float[])bIm.Clone());

        var y32 = new ComplexConvolver().Convolve(a32, b32);
        var yT = new ComplexConvolver<float>().Convolve(aT, bT);
        ExpectClose("ComplexConvolver<float> conv Re ≡ classic", y32.Real, yT.Real, 0);
        ExpectClose("ComplexConvolver<float> conv Im ≡ classic", y32.Imag, yT.Imag, 0);

        var xc32 = new ComplexConvolver().CrossCorrelate(a32, b32);
        var xcT = Operation.CrossCorrelate(aT, bT);
        ExpectClose("ComplexConvolver<float> xcorr Re ≡ classic", xc32.Real, xcT.Real, 0);
        ExpectClose("ComplexConvolver<float> xcorr Im ≡ classic", xc32.Imag, xcT.Imag, 0);

        var adRe = new double[24];
        var adIm = new double[24];
        var bdRe = new double[6];
        var bdIm = new double[6];
        for (int i = 0; i < adRe.Length; i++)
        {
            adRe[i] = rng.NextDouble() * 2 - 1;
            adIm[i] = rng.NextDouble() * 2 - 1;
        }
        for (int i = 0; i < bdRe.Length; i++)
        {
            bdRe[i] = rng.NextDouble() * 2 - 1;
            bdIm[i] = rng.NextDouble() * 2 - 1;
        }
        var ad = new ComplexDiscreteSignal<double>(1, adRe, adIm);
        var bd = new ComplexDiscreteSignal<double>(1, bdRe, bdIm);
        var yd = new ComplexConvolver64().Convolve(ad, bd);
        DirectComplexConv(adRe, adIm, bdRe, bdIm, out var dRe, out var dIm);
        ExpectClose("ComplexConvolver64 conv Re vs direct", dRe, yd.Real, 2e-12);
        ExpectClose("ComplexConvolver64 conv Im vs direct", dIm, yd.Imag, 2e-12);

        double[] qRe = [1, 2, 1];
        double[] kRe = [1, 1];
        var prodRe = DirectConv(qRe, kRe);
        var prod = new ComplexDiscreteSignal<double>(1, prodRe, new double[prodRe.Length]);
        var ker = new ComplexDiscreteSignal<double>(1, kRe, new double[kRe.Length]);
        var rec = Operation.Deconvolve(prod, ker);
        ExpectClose("Deconvolve<double> exact poly", qRe, rec.Real, 1e-10);
    }

    static void AssertLongSequence()
    {
        var rng = new Random(193);
        var xf = new float[4096];
        var kf = new float[65];
        Fill(rng, xf);
        Fill(rng, kf);

        var classic = new Convolver().Convolve(Signal.FromCopy(xf, 1), Signal.FromCopy(kf, 1)).Samples.ToArray();
        var generic = new Convolver<float>().Convolve(xf, kf);
        ExpectClose("long Convolver<float> ≡ Convolver", classic, generic, 0);

        var xd = new double[4096];
        var kd = new double[65];
        for (int i = 0; i < xd.Length; i++)
            xd[i] = rng.NextDouble() * 2 - 1;
        for (int i = 0; i < kd.Length; i++)
            kd[i] = rng.NextDouble() * 2 - 1;

        var fft = new Convolver64().Convolve(xd, kd);
        var direct = DirectConv(xd, kd);
        ExpectClose("long Convolver64 vs direct", direct, fft, 1e-10);

        var xc = new Convolver<double>().CrossCorrelate(xd, kd);
        Expect("long double xcorr length", xc.Length == xd.Length + kd.Length - 1);
        Expect("long double xcorr finite", xc.All(double.IsFinite));
    }

    static void Fill(Random rng, float[] a)
    {
        for (int i = 0; i < a.Length; i++)
            a[i] = (float)(rng.NextDouble() * 2 - 1);
    }

    static void DirectComplexConv(float[] aRe, float[] aIm, float[] bRe, float[] bIm, out float[] cRe, out float[] cIm)
        => DirectComplexConvT(aRe, aIm, bRe, bIm, out cRe, out cIm);

    static void DirectComplexConv(double[] aRe, double[] aIm, double[] bRe, double[] bIm, out double[] cRe, out double[] cIm)
        => DirectComplexConvT(aRe, aIm, bRe, bIm, out cRe, out cIm);

    static void DirectComplexConvT<T>(T[] aRe, T[] aIm, T[] bRe, T[] bIm, out T[] cRe, out T[] cIm)
        where T : System.Numerics.IFloatingPointIeee754<T>
    {
        cRe = new T[aRe.Length + bRe.Length - 1];
        cIm = new T[cRe.Length];
        for (int i = 0; i < aRe.Length; i++)
        {
            for (int j = 0; j < bRe.Length; j++)
            {
                cRe[i + j] += aRe[i] * bRe[j] - aIm[i] * bIm[j];
                cIm[i + j] += aRe[i] * bIm[j] + aIm[i] * bRe[j];
            }
        }
    }

    static T[] DirectConv<T>(T[] a, T[] b) where T : System.Numerics.IFloatingPointIeee754<T>
    {
        var c = new T[a.Length + b.Length - 1];
        for (int i = 0; i < a.Length; i++)
        {
            for (int j = 0; j < b.Length; j++)
                c[i + j] += a[i] * b[j];
        }
        return c;
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL {name}" + (detail is null ? "" : $": {detail}"));
    }

    static void ExpectClose(string name, float[] a, float[] b, float tol)
    {
        if (a.Length != b.Length)
        {
            Expect(name, false, $"len {a.Length} vs {b.Length}");
            return;
        }
        float max = 0;
        for (int i = 0; i < a.Length; i++)
            max = Math.Max(max, Math.Abs(a[i] - b[i]));
        Expect(name, max <= tol, $"maxΔ={max} tol={tol}");
    }

    static void ExpectClose(string name, double[] a, double[] b, double tol)
    {
        if (a.Length != b.Length)
        {
            Expect(name, false, $"len {a.Length} vs {b.Length}");
            return;
        }
        double max = 0;
        for (int i = 0; i < a.Length; i++)
            max = Math.Max(max, Math.Abs(a[i] - b[i]));
        Expect(name, max <= tol, $"maxΔ={max} tol={tol}");
    }
}
