using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace SP_module_test;

/// <summary>0.10.18: FIR/Zi/FDA/BiQuad/OnePole/designed IIR stay in T.</summary>
internal static class Filter_018_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing FIR / Zi / FDA / BiQuad / designed IIR (0.10.18)...");

        AssertFirMatchesClassic();
        AssertZiMatchesClassic();
        AssertGroupDelayAndZi();
        AssertFdaIirFloatMatches();
        AssertBiQuadAndOnePole();
        AssertDesignedButterworth();
        AssertFirWinAndRemez();

        if (_failures != 0)
            throw new InvalidOperationException($"Filter 0.10.18: {_failures} assertion(s) failed.");

        Console.WriteLine("Filter_018_test: PASS");
    }

    static void AssertFirMatchesClassic()
    {
        float[] k = [0.1f, 0.2f, 0.4f, 0.2f, 0.1f];
        var x = Ramp(64);
        var classic = new FirFilter(k);
        var generic = new FirFilter<float>(k);
        var y1 = classic.ProcessAllSamples(x);
        var y2 = generic.ProcessAllSamples(x);
        ExpectClose("FIR float ≡ FirFilter", y1, y2, 0);
    }

    static void AssertZiMatchesClassic()
    {
        float[] b = [0.2f, 0.3f, 0.2f];
        float[] a = [1f, -0.4f, 0.25f];
        var x = Ramp(128);
        var classic = new ZiFilter(b, a);
        var generic = new ZiFilter<float>(b, a);
        var y1 = new float[x.Length];
        for (int i = 0; i < x.Length; i++)
            y1[i] = classic.Process(x[i]);
        var y2 = generic.ApplyTo(x);
        ExpectClose("Zi Process float ≡ ZiFilter", y1, y2, 0);

        classic.Reset();
        generic.Reset();
        var sig = Signal.FromCopy(x, 1);
        var z1 = classic.ZeroPhase(sig).Samples.ToArray();
        var z2 = generic.ZeroPhase(x);
        ExpectClose("Zi ZeroPhase float ≡ ZiFilter", z1, z2, 1e-6f);
    }

    static void AssertGroupDelayAndZi()
    {
        float[] b = [0.2f, 0.2f];
        float[] a = [1f, -0.5f];
        var tf32 = new TransferFunction(b, a);
        var tfG = new TransferFunction<float>(b, a);
        var gd1 = tf32.GroupDelay(64);
        var gd2 = tfG.GroupDelay(64);
        ExpectClose("GroupDelay float ≡ TransferFunction", gd1, gd2, 2e-5f);

        var zi1 = tf32.Zi;
        var zi2 = tfG.Zi;
        ExpectClose("TF.Zi float ≡ TransferFunction", zi1, zi2, 1e-5f);

        var ss = tfG.StateSpace;
        Expect("SS finite", ss.B.All(float.IsFinite) && ss.C.All(float.IsFinite));
    }

    static void AssertFdaIirFloatMatches()
    {
        var poles32 = Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.PrototypeButterworth.Poles(4);
        var polesG = Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.PrototypeButterworth.Poles<float>(4);
        Expect("Butter poles count", poles32.Length == polesG.Length);
        float maxP = 0;
        for (int i = 0; i < poles32.Length; i++)
            maxP = Math.Max(maxP, Math.Abs(poles32[i].Real - polesG[i].Real) + Math.Abs(poles32[i].Imaginary - polesG[i].Imaginary));
        Expect("Butter poles ≡", maxP < 1e-6f, $"maxΔ={maxP}");

        var tf32 = DesignFilter.IirLpTf(0.1f, poles32);
        var tfG = DesignFilter.IirLpTf(0.1f, polesG);
        ExpectClose("IirLpTf num", tf32.Numerator, tfG.Numerator, 2e-6f);
        ExpectClose("IirLpTf den", tf32.Denominator, tfG.Denominator, 2e-6f);

        var tf64 = DesignFilter.IirLpTf(0.1, Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.PrototypeButterworth.Poles<double>(4));
        Expect("IirLpTf<double> finite", tf64.Numerator.All(double.IsFinite) && tf64.Denominator.All(double.IsFinite));
    }

    static void AssertBiQuadAndOnePole()
    {
        var x = Ramp(64);
        var bq = new Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.LowPassFilter(0.1f, 0.7f);
        var bqG = new Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.LowPassFilter<float>(0.1f, 0.7f);
        var y1 = new float[x.Length];
        for (int i = 0; i < x.Length; i++)
            y1[i] = bq.Process(x[i]);
        var y2 = bqG.ApplyTo(x);
        ExpectClose("BiQuad LP float ≡", y1, y2, 1e-6f);

        var op = new Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.LowPassFilter(0.05f);
        var opG = new Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.LowPassFilter<float>(0.05f);
        op.Reset();
        var z1 = new float[x.Length];
        for (int i = 0; i < x.Length; i++)
            z1[i] = op.Process(x[i]);
        var z2 = opG.ApplyTo(x);
        ExpectClose("OnePole LP float ≡", z1, z2, 1e-6f);
    }

    static void AssertDesignedButterworth()
    {
        var x = Ramp(128);
        var f = new Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.LowPassFilter(0.1f, 4);
        var g = new Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.LowPassFilter<float>(0.1f, 4);
        var y1 = new float[x.Length];
        for (int i = 0; i < x.Length; i++)
            y1[i] = f.Process(x[i]);
        var y2 = g.ApplyTo(x);
        ExpectClose("Butterworth LP float ≡", y1, y2, 2e-6f);

        var f64 = new Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.LowPassFilter<double>(0.1, 4);
        var xd = new double[64];
        xd[0] = 1;
        var y64 = f64.ApplyTo(xd);
        Expect("Butterworth64 IR finite", y64.All(double.IsFinite) && y64.Sum(Math.Abs) > 0.1);
    }

    static void AssertFirWinAndRemez()
    {
        var k32 = DesignFilter.FirWinLp(21, 0.2f);
        var kG = DesignFilter.FirWinLp<float>(21, 0.2f);
        ExpectClose("FirWinLp float ≡", k32, kG, 2e-6f);

        var k64 = DesignFilter.FirWinLp<double>(21, 0.2);
        Expect("FirWinLp<double> finite", k64.All(double.IsFinite));

        var r32 = DesignFilter.FirEquirippleLp(21, 0.2f, 0.3f, 1f, 1f);
        var rG = DesignFilter.FirEquirippleLp<float>(21, 0.2f, 0.3f, 1f, 1f);
        ExpectClose("FirEquirippleLp float ≡", r32, rG, 2e-5f);
    }

    static float[] Ramp(int n)
    {
        var x = new float[n];
        for (int i = 0; i < n; i++)
            x[i] = (float)Math.Sin(2 * Math.PI * 0.03 * i);
        return x;
    }

    static void ExpectClose(string name, float[] a, float[] b, float tol)
    {
        if (a.Length != b.Length)
        {
            Expect(name, false, $"len {a.Length}≠{b.Length}");
            return;
        }
        float max = 0;
        for (int i = 0; i < a.Length; i++)
            max = Math.Max(max, Math.Abs(a[i] - b[i]));
        Expect(name, max <= tol, $"maxΔ={max}");
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }
}
