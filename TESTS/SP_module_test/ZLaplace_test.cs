using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace SP_module_test;

/// <summary>Z / Laplace / bilinear API smoke + equivalence checks for 0.10.8.</summary>
internal static class ZLaplace_test
{
    public static void Go()
    {
        "=== Z / Laplace / AnalogDigital ===".PrintLine(ConsoleColor.Cyan);

        DtftImpulse();
        LaplaceFirstOrder();
        Stability();
        BilinearMatchesIirLpTf();
        InverseBilinearRoundtrip();

        "ZLaplace_test OK".PrintLine(ConsoleColor.Green);
    }

    static void DtftImpulse()
    {
        // δ[n] → X(e^{jω}) = 1
        float[] x = [1f, 0, 0, 0, 0, 0, 0, 0];
        var X = ZTransform.Dtft(x, 32);
        for (int k = 0; k < X.Length; k++)
        {
            if (MathF.Abs(X[k].Real - 1f) > 1e-5f || MathF.Abs(X[k].Imaginary) > 1e-5f)
                throw new Exception($"Dtft impulse bin {k}: {X[k]}");
        }

        // Explicit Normal / Simd / Parallel must agree (Simd ignores Auto size ladders)
        var rng = new Random(1);
        var longX = new float[4096];
        for (int i = 0; i < longX.Length; i++)
            longX[i] = (float)(rng.NextDouble() * 2 - 1);
        var serial = ZTransform.Dtft(longX, 512, ComputingContext.Normal);
        var simd = ZTransform.Dtft(longX, 512, ComputingContext.Simd);
        var parallel = ZTransform.Dtft(longX, 512, ComputingContext.Parallel);
        for (int k = 0; k < serial.Length; k++)
        {
            if (MathF.Abs(serial[k].Real - parallel[k].Real) > 1e-4f
                || MathF.Abs(serial[k].Imaginary - parallel[k].Imaginary) > 1e-4f)
                throw new Exception($"Dtft Normal≠Parallel at {k}");
            if (MathF.Abs(serial[k].Real - simd[k].Real) > 1e-4f
                || MathF.Abs(serial[k].Imaginary - simd[k].Imaginary) > 1e-4f)
                throw new Exception($"Dtft Normal≠Simd at {k}");
        }

        // Multi-ω H(e^{jω}) ≡ pointwise EvaluateFrequencyResponse
        float[] b = [1f, 0.5f];
        float[] a = [1f, -0.3f];
        float[] omegas = new float[64];
        for (int i = 0; i < omegas.Length; i++)
            omegas[i] = i * MathF.PI / omegas.Length;
        var grid = ZTransform.EvaluateFrequencyResponse(b, a, omegas, ComputingContext.Simd);
        for (int i = 0; i < omegas.Length; i++)
        {
            var pt = ZTransform.EvaluateFrequencyResponse(b, a, omegas[i]);
            if (MathF.Abs(grid[i].Real - pt.Real) > 1e-5f
                || MathF.Abs(grid[i].Imaginary - pt.Imaginary) > 1e-5f)
                throw new Exception($"Z multi-ω FR mismatch at {i}");
        }

        // H(z)=1 for b=[1], a=[1]
        var h = ZTransform.Evaluate([1f], [1f], new ComplexFp32(0.5f, 0.1f));
        if (MathF.Abs(h.Real - 1f) > 1e-6f || MathF.Abs(h.Imaginary) > 1e-6f)
            throw new Exception($"Evaluate identity: {h}");

        "  DTFT / Evaluate / ComputingContext OK".PrintLine();
    }

    static void LaplaceFirstOrder()
    {
        // H(s) = 1/(s+1) → H(j0)=1, H(j∞)→0
        float[] num = [1f];
        float[] den = [1f, 1f];
        var h0 = LaplaceTransform.Evaluate(num, den, ComplexFp32.Zero);
        if (MathF.Abs(h0.Real - 1f) > 1e-5f)
            throw new Exception($"H(0)={h0}");

        float[] w = [0f, 1f, 10f, 100f];
        var H = LaplaceTransform.FrequencyResponse(num, den, w);
        if (H[0].Magnitude < 0.99f || H[^1].Magnitude > 0.02f)
            throw new Exception($"H(jω) unexpected: |H0|={H[0].Magnitude} |Hinf|={H[^1].Magnitude}");

        var wDense = new float[8192];
        for (int i = 0; i < wDense.Length; i++)
            wDense[i] = i * 0.01f;
        var serial = LaplaceTransform.FrequencyResponse(num, den, wDense, ComputingContext.Normal);
        var simd = LaplaceTransform.FrequencyResponse(num, den, wDense, ComputingContext.Simd);
        var parallel = LaplaceTransform.FrequencyResponse(num, den, wDense, ComputingContext.Parallel);
        for (int i = 0; i < serial.Length; i++)
        {
            if (MathF.Abs(serial[i].Real - parallel[i].Real) > 1e-5f
                || MathF.Abs(serial[i].Imaginary - parallel[i].Imaginary) > 1e-5f)
                throw new Exception($"Laplace FR Normal≠Parallel at {i}");
            if (MathF.Abs(serial[i].Real - simd[i].Real) > 1e-5f
                || MathF.Abs(serial[i].Imaginary - simd[i].Imaginary) > 1e-5f)
                throw new Exception($"Laplace FR Normal≠Simd at {i}");
        }

        "  Laplace H(s) / ComputingContext OK".PrintLine();
    }

    static void Stability()
    {
        var stableP = new ComplexFp32[] { new(-0.5f, 0.2f), new(-0.5f, -0.2f) };
        var unstableP = new ComplexFp32[] { new(0.1f, 0f) };
        if (!LaplaceTransform.IsStable(stableP) || LaplaceTransform.IsStable(unstableP))
            throw new Exception("Laplace IsStable");

        var zOk = new ComplexFp32[] { new(0.5f, 0.1f), new(0.5f, -0.1f) };
        var zBad = new ComplexFp32[] { new(1.1f, 0f) };
        if (!ZTransform.IsStable(zOk) || ZTransform.IsStable(zBad))
            throw new Exception("Z IsStable");
        if (ZTransform.MaxPoleRadius(zOk) >= 1f)
            throw new Exception("MaxPoleRadius");

        "  Stability OK".PrintLine();
    }

    static void BilinearMatchesIirLpTf()
    {
        float fc = 0.1f;
        var proto = PrototypeButterworth.Poles(4);
        var viaApi = AnalogDigitalTransform.BilinearLowpass(fc, proto);
        var viaDesign = DesignFilter.IirLpTf(fc, proto);

        if (viaApi.Numerator.Length != viaDesign.Numerator.Length
            || viaApi.Denominator.Length != viaDesign.Denominator.Length)
            throw new Exception("Bilinear length mismatch vs IirLpTf");

        for (int i = 0; i < viaApi.Numerator.Length; i++)
        {
            if (MathF.Abs(viaApi.Numerator[i] - viaDesign.Numerator[i]) > 2e-5f)
                throw new Exception($"num[{i}] {viaApi.Numerator[i]} vs {viaDesign.Numerator[i]}");
        }
        for (int i = 0; i < viaApi.Denominator.Length; i++)
        {
            if (MathF.Abs(viaApi.Denominator[i] - viaDesign.Denominator[i]) > 2e-5f)
                throw new Exception($"den[{i}] {viaApi.Denominator[i]} vs {viaDesign.Denominator[i]}");
        }

        if (!ZTransform.IsStable(viaApi.Poles))
            throw new Exception("Butterworth LP should be stable");

        "  Bilinear ≡ IirLpTf OK".PrintLine();
    }

    static void InverseBilinearRoundtrip()
    {
        float fc = 0.12f;
        var proto = PrototypeButterworth.Poles(3);
        var (z, p) = LaplaceTransform.BilinearMap(ReadOnlySpan<ComplexFp32>.Empty, proto, fc);
        var (zBack, pBack) = LaplaceTransform.InverseBilinearMap(z, p, fc);

        // zeros at −1 are dropped → empty analog zeros
        if (zBack.Length != 0)
            throw new Exception($"Expected no finite inverse zeros, got {zBack.Length}");

        if (pBack.Length != proto.Length)
            throw new Exception($"Pole count {pBack.Length} vs {proto.Length}");

        // Match poles up to conjugation order
        foreach (var expected in proto)
        {
            bool found = false;
            foreach (var got in pBack)
            {
                if (MathF.Abs(got.Real - expected.Real) < 2e-4f
                    && MathF.Abs(got.Imaginary - expected.Imaginary) < 2e-4f)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                throw new Exception($"Missing round-trip pole {expected}");
        }

        "  Inverse bilinear round-trip OK".PrintLine();
    }
}
