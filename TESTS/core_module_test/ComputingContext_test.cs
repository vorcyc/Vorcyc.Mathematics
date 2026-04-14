using Vorcyc.Mathematics;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.Statistics;

namespace core_module_test;

internal static class ComputingContext_test
{
    public static void Go()
    {
        Console.WriteLine("--- ComputingContext ---");

        const float rate = 8000f;
        const int length = 4096;
        var tone = new Signal(length, rate);
        tone.GenerateWave(WaveShape.Sine, 440f, Behaviour.Replace);

        var defaultFreq = tone.TransformToFrequencyDomain().Frequency;

        using (ComputingScope.Enter(ComputingContext.Normal))
        {
            var scoped = new Signal(length, rate);
            scoped.GenerateWave(WaveShape.Sine, 440f, Behaviour.Replace);
            var scopedFreq = scoped.TransformToFrequencyDomain().Frequency;
            Console.WriteLine($"Scope Normal FFT peak: {scopedFreq:F1} Hz");
        }

        var explicitFreq = tone.TransformToFrequencyDomain(ComputingContext.Simd).Frequency;
        Console.WriteLine($"Default peak: {defaultFreq:F1} Hz, explicit SIMD: {explicitFreq:F1} Hz");
        Console.WriteLine($"Resolve order: explicit={ComputingContext.Resolve(ComputingContext.Parallel) == ComputingContext.Parallel}");

        TestStatisticsContext();
        TestMatrixContext();
        TestVectorSpanContext();
        TestSignalContext();
        TestTensorStatisticsContext();
        TestComputingScopeResolution();
        TestFftInverseContext();
        TestLargeParallelEquivalence();
        TestFftRoundtripModes();
        TestVectorSpanAllOps();
        TestMatrixVectorMultiply();
        TestSignalFftPeakModes();
        TestStatisticsExtended();
    }

    private static void TestLargeParallelEquivalence()
    {
        var values = new float[200_000];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = MathF.Sin(i * 0.001f) + 0.25f;
        }

        var span = values.AsSpan();
        float baseline = span.Sum();
        float normal = span.Sum(ComputingContext.Normal);
        float simd = span.Sum(ComputingContext.Simd);
        float parallel = span.Sum(ComputingContext.Parallel);

        if (!AllClose(baseline, normal, 0.2f)
            || !AllClose(baseline, simd, 0.2f)
            || !AllClose(baseline, parallel, 0.2f))
        {
            throw new InvalidOperationException(
                $"Large Sum equivalence failed: base={baseline}, normal={normal}, simd={simd}, parallel={parallel}");
        }

        var a = new Matrix(64, 64);
        var b = new Matrix(64, 64);
        for (var i = 0; i < 64; i++)
        {
            for (var j = 0; j < 64; j++)
            {
                a[i, j] = (i + 1) * 0.01f + j;
                b[i, j] = (j + 1) * 0.02f - i;
            }
        }

        var baseMul = a * b;
        var parMul = Matrix.Multiply(a, b, ComputingContext.Parallel);
        for (var i = 0; i < 64; i++)
        {
            for (var j = 0; j < 64; j++)
            {
                if (MathF.Abs(baseMul[i, j] - parMul[i, j]) > 1e-2f)
                {
                    throw new InvalidOperationException("Large Matrix Multiply Parallel mismatch.");
                }
            }
        }

        Console.WriteLine("Large-array Parallel/SIMD equivalence OK");
    }

    private static void TestStatisticsContext()
    {
        Span<float> values = stackalloc float[128];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = i + 1;
        }

        float expected = values.Sum();
        float normal = values.Sum(ComputingContext.Normal);
        float parallel = values.Sum(ComputingContext.Parallel);
        float scoped;

        using (ComputingScope.Enter(ComputingContext.Simd))
        {
            scoped = values.Sum();
        }

        if (MathF.Abs(expected - normal) > 1e-4f || MathF.Abs(expected - parallel) > 1e-4f || MathF.Abs(expected - scoped) > 1e-4f)
        {
            throw new InvalidOperationException("Statistics Sum context dispatch mismatch.");
        }

        float max = values.CompareMax(ComputingContext.Parallel);
        if (max != 128f)
        {
            throw new InvalidOperationException("Statistics CompareMax context dispatch mismatch.");
        }

        var (_, baselineVar) = values.Variance();
        var (_, contextVar) = values.Variance(ComputingContext.Normal);
        if (MathF.Abs(baselineVar - contextVar) > 1e-3f)
        {
            throw new InvalidOperationException("Statistics Variance context dispatch mismatch.");
        }

        Console.WriteLine($"Statistics context Sum/Variance OK (sum={expected:F0}, max={max}, var={baselineVar:F2})");
    }

    private static void TestVectorSpanContext()
    {
        Span<float> a = stackalloc float[] { 1f, 2f, 3f, 4f };
        Span<float> b = stackalloc float[] { 2f, 3f, 4f, 5f };
        float baseline = VectorSpan.Dot(a, b);
        float explicitDot = VectorSpan.Dot(a, b, ComputingContext.Simd);
        if (MathF.Abs(baseline - explicitDot) > 1e-5f)
        {
            throw new InvalidOperationException("VectorSpan Dot context dispatch mismatch.");
        }

        Console.WriteLine($"VectorSpan Dot context OK (dot={baseline:F1})");
    }

    private static void TestMatrixContext()
    {
        var a = new Matrix(4, 4);
        var b = new Matrix(4, 4);
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                a[i, j] = i + j;
                b[i, j] = j - i;
            }
        }

        var baseline = a * b;
        var explicitResult = Matrix.Multiply(a, b, ComputingContext.Simd);

        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                if (MathF.Abs(baseline[i, j] - explicitResult[i, j]) > 1e-4f)
                {
                    throw new InvalidOperationException("Matrix Multiply context dispatch mismatch.");
                }
            }
        }

        Console.WriteLine("Matrix Multiply context OK");
    }

    private static void TestSignalContext()
    {
        const float rate = 8000f;
        var tone = new Signal(512, rate);
        tone.GenerateWave(WaveShape.Sine, 440f, Behaviour.Replace);

        float baseline = tone.Rms;
        float explicitRms = tone.GetRms(ComputingContext.Normal);
        if (MathF.Abs(baseline - explicitRms) > 1e-4f)
        {
            throw new InvalidOperationException("Signal GetRms context mismatch.");
        }

        Console.WriteLine($"Signal GetRms context OK (rms={baseline:F4})");
    }

    private static void TestTensorStatisticsContext()
    {
        var tensor = new Tensor4D<float>(2, 2, 2, 1);
        for (var i = 0; i < tensor.Values.Length; i++)
        {
            tensor.Values[i] = i + 1;
        }

        var baseline = TensorStatistics.MeanAlongAxis(tensor, TensorStatistics.Tensor4DAxis.Dim3);
        var withContext = TensorStatistics.MeanAlongAxis(tensor, TensorStatistics.Tensor4DAxis.Dim3, ComputingContext.Normal);
        if (baseline.Length != withContext.Length)
        {
            throw new InvalidOperationException("TensorStatistics context length mismatch.");
        }

        for (var i = 0; i < baseline.Length; i++)
        {
            if (MathF.Abs(baseline[i] - withContext[i]) > 1e-4f)
            {
                throw new InvalidOperationException("TensorStatistics MeanAlongAxis context mismatch.");
            }
        }

        Console.WriteLine("TensorStatistics MeanAlongAxis context OK");
    }

    private static void TestComputingScopeResolution()
    {
        Span<float> values = stackalloc float[8];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = i + 1;
        }

        float baseline = values.Sum(context: null);
        using (ComputingScope.Enter(ComputingContext.Normal))
        {
            float scoped = values.Sum(context: null);
            if (MathF.Abs(baseline - scoped) > 1e-4f)
            {
                throw new InvalidOperationException("ComputingScope Sum resolution mismatch.");
            }
        }

        Console.WriteLine("ComputingScope resolves through null context OK");
    }

    private static void TestFftInverseContext()
    {
        var spectrum = new ComplexFp32[8];
        for (var i = 0; i < spectrum.Length; i++)
        {
            spectrum[i] = new ComplexFp32(i * 0.1f, 0f);
        }

        var time = new ComplexFp32[spectrum.Length];
        if (!FastFourierTransform.Inverse(spectrum, time, scale: true, ComputingContext.Simd))
        {
            throw new InvalidOperationException("Inverse FFT with context failed.");
        }

        Console.WriteLine("FFT Inverse context OK");
    }

    private static bool AllClose(float a, float b, float absTol)
        => MathF.Abs(a - b) <= absTol;

    private static void TestFftRoundtripModes()
    {
        const int n = 1024;
        var time = new float[n];
        for (var i = 0; i < n; i++)
        {
            time[i] = MathF.Sin(2f * MathF.PI * 50f * i / 8000f);
        }

        foreach (var ctx in new[] { ComputingContext.Normal, ComputingContext.Simd, ComputingContext.Parallel })
        {
            var spectrum = new ComplexFp32[n];
            var restored = new ComplexFp32[n];
            if (!FastFourierTransform.Forward(time, spectrum, ctx))
            {
                throw new InvalidOperationException($"FFT forward failed for {ctx}.");
            }

            if (!FastFourierTransform.Inverse(spectrum, restored, scale: true, ctx))
            {
                throw new InvalidOperationException($"FFT inverse failed for {ctx}.");
            }

            float maxErr = 0f;
            for (var i = 0; i < n; i++)
            {
                maxErr = MathF.Max(maxErr, MathF.Abs(time[i] - restored[i].Real));
            }

            if (maxErr > 1e-3f)
            {
                throw new InvalidOperationException($"FFT roundtrip max error {maxErr} for {ctx}.");
            }
        }

        Console.WriteLine("FFT roundtrip Normal/SIMD/Parallel OK");
    }

    private static void TestVectorSpanAllOps()
    {
        var a = new float[8192];
        var b = new float[8192];
        var scratch = new float[8192];
        for (var i = 0; i < a.Length; i++)
        {
            a[i] = i * 0.001f;
            b[i] = (i % 7) * 0.01f;
        }

        var aSpan = a.AsSpan();
        var bSpan = b.AsSpan();
        var scratchSpan = scratch.AsSpan();

        float baseSum = VectorSpan.Sum(aSpan);
        float ctxSum = VectorSpan.Sum(aSpan, ComputingContext.Parallel);
        if (!AllClose(baseSum, ctxSum, 0.5f))
        {
            throw new InvalidOperationException("VectorSpan Sum parallel mismatch.");
        }

        float baseDot = VectorSpan.Dot(aSpan, bSpan);
        float ctxDot = VectorSpan.Dot(aSpan, bSpan, ComputingContext.Parallel);
        if (!AllClose(baseDot, ctxDot, 1f))
        {
            throw new InvalidOperationException("VectorSpan Dot parallel mismatch.");
        }

        float baseNorm = VectorSpan.Norm(aSpan);
        float ctxNorm = VectorSpan.Norm(aSpan, ComputingContext.Parallel);
        if (!AllClose(baseNorm, ctxNorm, 1e-3f))
        {
            throw new InvalidOperationException("VectorSpan Norm parallel mismatch.");
        }

        a.CopyTo(scratch);
        VectorSpan.Add(scratchSpan, bSpan, scratchSpan, ComputingContext.Parallel);
        for (var i = 0; i < a.Length; i++)
        {
            if (MathF.Abs(scratch[i] - (a[i] + b[i])) > 1e-4f)
            {
                throw new InvalidOperationException("VectorSpan Add parallel mismatch.");
            }
        }

        a.CopyTo(scratch);
        VectorSpan.Scale(scratchSpan, 1.5f, scratchSpan, ComputingContext.Parallel);
        for (var i = 0; i < a.Length; i++)
        {
            if (MathF.Abs(scratch[i] - a[i] * 1.5f) > 1e-4f)
            {
                throw new InvalidOperationException("VectorSpan Scale parallel mismatch.");
            }
        }

        Console.WriteLine("VectorSpan Sum/Dot/Norm/Add/Scale parallel OK");
    }

    private static void TestMatrixVectorMultiply()
    {
        var matrix = new Matrix(32, 32);
        var vector = new float[32];
        for (var i = 0; i < 32; i++)
        {
            vector[i] = i * 0.1f + 1f;
            for (var j = 0; j < 32; j++)
            {
                matrix[i, j] = (i + j) * 0.01f;
            }
        }

        var baseline = matrix.Multiply(vector);
        var result = new float[32];
        matrix.Multiply(vector, result, ComputingContext.Parallel);

        for (var i = 0; i < 32; i++)
        {
            if (MathF.Abs(baseline[i] - result[i]) > 1e-3f)
            {
                throw new InvalidOperationException("Matrix vector multiply parallel mismatch.");
            }
        }

        Console.WriteLine("Matrix vector multiply parallel OK");
    }

    private static void TestSignalFftPeakModes()
    {
        const float rate = 8000f;
        const float targetHz = 440f;
        var tone = new Signal(4096, rate);
        tone.GenerateWave(WaveShape.Sine, targetHz, Behaviour.Replace);

        float defaultPeak = tone.TransformToFrequencyDomain().Frequency;
        float normalPeak = tone.TransformToFrequencyDomain(ComputingContext.Normal).Frequency;
        float simdPeak = tone.TransformToFrequencyDomain(ComputingContext.Simd).Frequency;
        float parallelPeak = tone.TransformToFrequencyDomain(ComputingContext.Parallel).Frequency;

        if (MathF.Abs(defaultPeak - normalPeak) > 2f
            || MathF.Abs(defaultPeak - simdPeak) > 2f
            || MathF.Abs(defaultPeak - parallelPeak) > 2f)
        {
            throw new InvalidOperationException(
                $"Signal FFT peak mismatch: default={defaultPeak}, normal={normalPeak}, simd={simdPeak}, parallel={parallelPeak}");
        }

        Console.WriteLine($"Signal FFT peak modes OK (~{defaultPeak:F0} Hz)");
    }

    private static void TestStatisticsExtended()
    {
        var values = new float[100_000];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = MathF.Sin(i * 0.002f) + i * 1e-5f;
        }

        var span = values.AsSpan();
        float baseMin = span.CompareMin();
        float ctxMin = span.CompareMin(ComputingContext.Parallel);
        float baseMax = span.CompareMax();
        float ctxMax = span.CompareMax(ComputingContext.Parallel);

        if (!AllClose(baseMin, ctxMin, 1e-3f) || !AllClose(baseMax, ctxMax, 1e-3f))
        {
            throw new InvalidOperationException("CompareMin/Max parallel mismatch.");
        }

        var (_, baseVar) = span.Variance();
        var (_, ctxVar) = span.Variance(ComputingContext.Parallel);
        if (!AllClose(baseVar, ctxVar, 0.5f))
        {
            throw new InvalidOperationException("Variance parallel mismatch.");
        }

        var weights = new float[values.Length];
        for (var i = 0; i < weights.Length; i++)
        {
            weights[i] = 1f + (i % 5) * 0.01f;
        }

        float baseWavg = Basic.WeightedAverage(values, weights);
        float ctxWavg = Basic.WeightedAverage(values, weights, ComputingContext.Parallel);
        if (!AllClose(baseWavg, ctxWavg, 0.5f))
        {
            throw new InvalidOperationException("WeightedAverage parallel mismatch.");
        }

        Console.WriteLine("Statistics extended parallel OK");
    }
}
