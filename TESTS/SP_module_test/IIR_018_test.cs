using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace SP_module_test;

/// <summary>0.10.18: IirFilter&lt;T&gt; / IirFilter64 stay in T; float path matches IirFilter.</summary>
internal static class IIR_018_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing IirFilter<T> / IirFilter64 (0.10.18)...");

        AssertFloatMatchesClassic();
        AssertDoublePreservesVsCast();
        AssertFdaWiden();
        AssertImpulseAndNormalize();

        if (_failures != 0)
            throw new InvalidOperationException($"IIR 0.10.18: {_failures} assertion(s) failed.");

        Console.WriteLine("IIR_018_test: PASS");
    }

    static void AssertFloatMatchesClassic()
    {
        float[] b = [0.2f, 0.3f, 0.2f];
        float[] a = [1f, -0.4f, 0.25f];
        var rng = new Random(18);
        var x = new float[256];
        for (int i = 0; i < x.Length; i++)
            x[i] = (float)(rng.NextDouble() * 2 - 1);

        var classic = new IirFilter(b, a);
        var generic = new IirFilter<float>(b, a);
        var y1 = new float[x.Length];
        var y2 = generic.ApplyTo(x);

        for (int i = 0; i < x.Length; i++)
            y1[i] = classic.Process(x[i]);

        float max = 0;
        for (int i = 0; i < x.Length; i++)
            max = Math.Max(max, Math.Abs(y1[i] - y2[i]));
        Expect("float ≡ IirFilter", max == 0, $"maxΔ={max}");

        var df = generic.ApplyDifferenceEquation(x);
        classic.Reset();
        // DifferenceEquation path on classic via FilteringMethod is Signal-based; compare DF vs itself stability
        Expect("df finite", df.All(float.IsFinite));
    }

    static void AssertDoublePreservesVsCast()
    {
        // Mildly resonant pair: double state should not match the cast-to-float classic path exactly.
        double[] b = [0.05, 0.1, 0.05];
        double[] a = [1.0, -1.8, 0.85];
        var x = new double[512];
        for (int i = 0; i < x.Length; i++)
            x[i] = Math.Sin(2 * Math.PI * 0.03 * i);

        var precise = new IirFilter64(b, a).ApplyTo(x);
        var cast = new IirFilter(b, a);
        var yCast = new float[x.Length];
        for (int i = 0; i < x.Length; i++)
            yCast[i] = cast.Process((float)x[i]);

        Expect("double finite", precise.All(double.IsFinite));
        double max = 0;
        for (int i = 0; i < x.Length; i++)
            max = Math.Max(max, Math.Abs(precise[i] - yCast[i]));
        Expect("double ≠ float-cast (resonant)", max > 1e-8, $"maxΔ={max}");
        Expect("but same ballpark", max < 0.05, $"maxΔ={max}");
    }

    static void AssertFdaWiden()
    {
        var tf = DesignFilter.IirLpTf(0.1f, PrototypeButterworthPoles());
        var f64 = new IirFilter<double>(tf);
        var x = new double[128];
        x[0] = 1;
        var y = f64.ApplyTo(x);
        Expect("fda widen finite", y.All(double.IsFinite));
        Expect("ir energy", y.Sum(v => Math.Abs(v)) > 0.1);
    }

    static Vorcyc.Mathematics.Numerics.ComplexFp32[] PrototypeButterworthPoles()
    {
        return Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.PrototypeButterworth.Poles(4);
    }

    static void AssertImpulseAndNormalize()
    {
        var tf = new TransferFunction<double>([0.2, 0.2], [2.0, -0.5]);
        tf.Normalize();
        Expect("a0=1", Math.Abs(tf.Denominator[0] - 1) < 1e-12);
        Expect("b0", Math.Abs(tf.Numerator[0] - 0.1) < 1e-12);

        var ir = tf.ImpulseResponse(8);
        var f = new IirFilter<double>(tf);
        var x = new double[8];
        x[0] = 1;
        var y = f.ApplyDifferenceEquation(x);
        double max = 0;
        for (int i = 0; i < 8; i++)
            max = Math.Max(max, Math.Abs(ir[i] - y[i]));
        Expect("IR ≡ DF", max < 1e-12, $"maxΔ={max}");

        var (re, im) = tf.FrequencyResponse(32);
        Expect("FR bins", re.Length == 17 && im.Length == 17);
        Expect("FR finite", re.All(double.IsFinite) && im.All(double.IsFinite));
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }
}
