using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace SP_module_test;

/// <summary>
/// 0.10.12: context-aware FastFourierTransform (and TransformToFrequencyDomain)
/// must match the scalar Normal DFT and round-trip through Inverse.
/// </summary>
internal static class FFT_test
{
    public static void Go()
    {
        "=== FastFourierTransform SoA kernel (0.10.12) ===".PrintLine(ConsoleColor.Cyan);

        AgreeWithNormal(4096);   // below SIMD crossover → still Normal
        AgreeWithNormal(16384);  // SIMD / Parallel take FftButterflyFp32
        Roundtrip(16384, ComputingContext.Simd);
        Roundtrip(16384, ComputingContext.Parallel);
        RoundtripInPlace(16384, ComputingContext.Simd);
        DetectSine();
        GenericFacadeAgrees();

        "FFT_test OK".PrintLine(ConsoleColor.Green);
    }

    static void AgreeWithNormal(int n)
    {
        var rng = new Random(n);
        var samples = new float[n];
        for (int i = 0; i < n; i++)
            samples[i] = (float)(rng.NextDouble() * 2 - 1);

        var expected = new ComplexFp32[n];
        if (!FastFourierTransform.Forward(samples, expected, ComputingContext.Normal))
            throw new Exception($"Normal Forward failed n={n}");

        AssertClose("Simd", samples, expected, ComputingContext.Simd, n);
        AssertClose("Parallel", samples, expected, ComputingContext.Parallel, n);
    }

    static void AssertClose(string label, float[] samples, ComplexFp32[] expected, ComputingContext ctx, int n)
    {
        var actual = new ComplexFp32[n];
        if (!FastFourierTransform.Forward(samples, actual, ctx))
            throw new Exception($"{label} Forward failed n={n}");

        float maxAbs = 0f;
        for (int i = 0; i < n; i++)
        {
            float dr = actual[i].Real - expected[i].Real;
            float di = actual[i].Imaginary - expected[i].Imaginary;
            float abs = MathF.Sqrt(dr * dr + di * di);
            if (abs > maxAbs)
                maxAbs = abs;
        }

        // DIF SoA vs DIT Normal: same DFT, rounding only. 16384-pt SIMD stays well under 1e-2.
        float tol = n >= 8192 ? 2e-2f : 1e-4f;
        if (maxAbs > tol)
            throw new Exception($"{label} vs Normal n={n}: max |Δ|={maxAbs} (tol {tol})");
    }

    static void Roundtrip(int n, ComputingContext ctx)
    {
        var rng = new Random(n + 7);
        var samples = new float[n];
        for (int i = 0; i < n; i++)
            samples[i] = (float)(rng.NextDouble() * 2 - 1);

        var spec = new ComplexFp32[n];
        var time = new ComplexFp32[n];
        if (!FastFourierTransform.Forward(samples, spec, ctx))
            throw new Exception($"roundtrip Forward failed n={n}");
        if (!FastFourierTransform.Inverse(spec, time, scale: true, ctx))
            throw new Exception($"roundtrip Inverse failed n={n}");

        float maxAbs = 0f;
        for (int i = 0; i < n; i++)
        {
            float dr = time[i].Real - samples[i];
            float di = time[i].Imaginary;
            float abs = MathF.Sqrt(dr * dr + di * di);
            if (abs > maxAbs)
                maxAbs = abs;
        }

        if (maxAbs > 2e-4f)
            throw new Exception($"roundtrip {ctx.CpuMode} n={n}: max |Δ|={maxAbs}");
    }

    static void RoundtripInPlace(int n, ComputingContext ctx)
    {
        var rng = new Random(n + 11);
        var samples = new float[n];
        for (int i = 0; i < n; i++)
            samples[i] = (float)(rng.NextDouble() * 2 - 1);

        var spec = new ComplexFp32[n];
        for (int i = 0; i < n; i++)
            spec[i] = new ComplexFp32(samples[i], 0f);

        if (!FastFourierTransform.Forward(spec.AsSpan(), ctx))
            throw new Exception($"inplace Forward failed n={n}");
        if (!FastFourierTransform.Inverse(spec.AsSpan(), scale: true, ctx))
            throw new Exception($"inplace Inverse failed n={n}");

        float maxAbs = 0f;
        for (int i = 0; i < n; i++)
        {
            float dr = spec[i].Real - samples[i];
            float di = spec[i].Imaginary;
            float abs = MathF.Sqrt(dr * dr + di * di);
            if (abs > maxAbs)
                maxAbs = abs;
        }

        if (maxAbs > 2e-4f)
            throw new Exception($"inplace roundtrip {ctx.CpuMode} n={n}: max |Δ|={maxAbs}");
    }

    static void DetectSine()
    {
        const float rate = 8000f;
        const float targetHz = 440f;
        const int length = 16384;

        var tone = new Signal(length, rate);
        tone.GenerateWave(WaveShape.Sine, targetHz, Behaviour.Replace);

        float normal = tone.TransformToFrequencyDomain(ComputingContext.Normal, WindowType.Hamming).Frequency;
        float simd = tone.TransformToFrequencyDomain(ComputingContext.Simd, WindowType.Hamming).Frequency;
        float parallel = tone.TransformToFrequencyDomain(ComputingContext.Parallel, WindowType.Hamming).Frequency;

        if (MathF.Abs(normal - targetHz) > 2f)
            throw new Exception($"Normal FFT peak {normal} Hz, expected ~{targetHz}");
        if (MathF.Abs(simd - normal) > 0.5f || MathF.Abs(parallel - normal) > 0.5f)
            throw new Exception($"peak mismatch Normal={normal} Simd={simd} Parallel={parallel}");
    }

    static void GenericFacadeAgrees()
    {
        const int n = 256;
        var rng = new Random(18);

        var re32 = new float[n];
        var im32 = new float[n];
        var re32b = new float[n];
        var im32b = new float[n];
        var re64 = new double[n];
        var im64 = new double[n];
        var re64b = new double[n];
        var im64b = new double[n];
        for (int i = 0; i < n; i++)
        {
            re32[i] = re32b[i] = (float)(rng.NextDouble() * 2 - 1);
            im32[i] = im32b[i] = (float)(rng.NextDouble() * 2 - 1);
            re64[i] = re64b[i] = rng.NextDouble() * 2 - 1;
            im64[i] = im64b[i] = rng.NextDouble() * 2 - 1;
        }

        new Fft(n).Direct(re32, im32);
        new Fft<float>(n).Direct(re32b, im32b);
        float max32 = 0;
        for (int i = 0; i < n; i++)
        {
            max32 = Math.Max(max32, Math.Abs(re32[i] - re32b[i]));
            max32 = Math.Max(max32, Math.Abs(im32[i] - im32b[i]));
        }
        if (max32 > 0)
            throw new Exception($"Fft<float> vs Fft: max |Δ|={max32}");

        new Fft64(n).Direct(re64, im64);
        new Fft<double>(n).Direct(re64b, im64b);
        double max64 = 0;
        for (int i = 0; i < n; i++)
        {
            max64 = Math.Max(max64, Math.Abs(re64[i] - re64b[i]));
            max64 = Math.Max(max64, Math.Abs(im64[i] - im64b[i]));
        }
        if (max64 > 0)
            throw new Exception($"Fft<double> vs Fft64: max |Δ|={max64}");
    }
}
