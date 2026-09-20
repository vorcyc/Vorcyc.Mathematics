using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace SP_module_test;

/// <summary>0.10.20: empty zpk is identity [1]; all-pole TransferFunction ctor does not throw.</summary>
internal static class Tf_020_test
{
    static int _failures;

    static readonly ComplexFp32[] Pair =
    [
        new ComplexFp32(0.6f, 0.4f),
        new ComplexFp32(0.6f, -0.4f),
    ];

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing TransferFunction empty zpk (0.10.20)...");

        AssertZpToTfEmpty();
        AssertConjugatePairUnchanged();
        AssertAllPoleCtor();
        AssertGenericEmpty();
        AssertResponses();

        if (_failures != 0)
            throw new InvalidOperationException($"TF 0.10.20: {_failures} assertion(s) failed.");

        Console.WriteLine("Tf_020_test: PASS");
    }

    static void AssertZpToTfEmpty()
    {
        ExpectClose("ZpToTf([])", [1f], TransferFunction.ZpToTf(Array.Empty<ComplexFp32>()));
        ExpectClose("ZpToTf(null)", [1f], TransferFunction.ZpToTf((ComplexFp32[])null!));
        ExpectClose("ZpToTf(re empty)", [1f], TransferFunction.ZpToTf(Array.Empty<float>()));
        ExpectClose("generic ZpToTf([])", [1f], TransferFunction<float>.ZpToTf(Array.Empty<Complex<float>>()));
        ExpectClose("generic ZpToTf(null)", [1f], TransferFunction<float>.ZpToTf((Complex<float>[])null!));
        ExpectClose("generic double []", [1.0], TransferFunction<double>.ZpToTf(Array.Empty<Complex<double>>()));
    }

    static void AssertConjugatePairUnchanged()
    {
        ExpectClose("conj pair", [1f, -1.2f, 0.52f], TransferFunction.ZpToTf(Pair), 1e-5f);
    }

    static void AssertAllPoleCtor()
    {
        var tf = new TransferFunction(Array.Empty<ComplexFp32>(), Pair, 1f);
        ExpectClose("all-pole b", [1f], tf.Numerator);
        ExpectClose("all-pole a", [1f, -1.2f, 0.52f], tf.Denominator, 1e-5f);

        var g2 = new TransferFunction(Array.Empty<ComplexFp32>(), Pair, 2f);
        ExpectClose("gain 2 b", [2f], g2.Numerator);

        var withZ = new TransferFunction(
            [new ComplexFp32(0.3f, 0.3f), new ComplexFp32(0.3f, -0.3f)], Pair, 1f);
        Expect("with zeros b len", withZ.Numerator.Length == 3);
        Expect("with zeros a len", withZ.Denominator.Length == 3);
    }

    static void AssertGenericEmpty()
    {
        Complex<float>[] poles = [new(0.6f, 0.4f), new(0.6f, -0.4f)];
        var tf = new TransferFunction<float>(Array.Empty<Complex<float>>(), poles, 1f);
        ExpectClose("generic all-pole b", [1f], tf.Numerator);
        ExpectClose("generic all-pole a", [1f, -1.2f, 0.52f], tf.Denominator, 1e-5f);
    }

    static void AssertResponses()
    {
        var tf = new TransferFunction(Array.Empty<ComplexFp32>(), Pair, 1f);
        var fr = tf.FrequencyResponse(64);
        Expect("FR length", fr.Length == 33);
        Expect("FR finite", fr.Real.All(float.IsFinite) && fr.Imag.All(float.IsFinite));
        var ir = tf.ImpulseResponse(32);
        Expect("IR length", ir.Length == 32);
        Expect("IR finite", ir.All(float.IsFinite));
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }

    static void ExpectClose(string name, float[] expected, float[] actual, float tol = 0)
    {
        if (actual is null)
        {
            Expect(name, false, "null");
            return;
        }
        if (expected.Length != actual.Length)
        {
            Expect(name, false, $"len {actual.Length} ≠ {expected.Length}");
            return;
        }
        float max = 0;
        for (int i = 0; i < expected.Length; i++)
            max = Math.Max(max, Math.Abs(expected[i] - actual[i]));
        Expect(name, max <= tol, $"maxΔ={max}");
    }

    static void ExpectClose(string name, double[] expected, double[] actual, double tol = 0)
    {
        if (actual is null)
        {
            Expect(name, false, "null");
            return;
        }
        if (expected.Length != actual.Length)
        {
            Expect(name, false, $"len {actual.Length} ≠ {expected.Length}");
            return;
        }
        double max = 0;
        for (int i = 0; i < expected.Length; i++)
            max = Math.Max(max, Math.Abs(expected[i] - actual[i]));
        Expect(name, max <= tol, $"maxΔ={max}");
    }
}
