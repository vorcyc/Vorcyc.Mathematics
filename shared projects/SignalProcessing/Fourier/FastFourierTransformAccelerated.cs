using System.Buffers;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

/// <summary>
/// Bridges the public AoS <see cref="ComplexFp32"/> FFT surface onto the shared SoA
/// <see cref="FftButterflyFp32"/> kernel. Used by context-aware
/// <see cref="FastFourierTransform"/> overloads (and therefore
/// <see cref="ITimeDomainSignal.TransformToFrequencyDomain"/>).
/// </summary>
/// <remarks>
/// When <see cref="FftButterflyFp32.WillAccelerate"/> is false the original scalar
/// <see cref="FastFourierTransformNormal"/> path is kept: the butterfly scalar
/// kernel is slower than the hand-written Normal FFT below the SIMD crossover.
/// </remarks>
internal static class FastFourierTransformAccelerated
{
    public static bool Forward(float[] input, int offset, out ComplexFp32[] output, int N, ComputingContext? context)
    {
        output = N > 0 ? new ComplexFp32[N] : [];
        if (input is null || offset < 0 || N < 1 || offset > input.Length - N)
            return false;

        return Forward(input.AsSpan(offset, N), output, context);
    }

    public static bool Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output, ComputingContext? context)
    {
        int n = input.Length;
        if (input.IsEmpty || n < 1 || !n.IsPowerOf2() || output.Length < n)
            return false;

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(n);
        if (!FftButterflyFp32.WillAccelerate(mode, n))
            return FastFourierTransformNormal.Forward(input, output);

        return TransformReal(input, output, mode, context);
    }

    public static bool Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, ComputingContext? context)
    {
        int n = input.Length;
        if (input.IsEmpty || n < 1 || !n.IsPowerOf2() || output.Length < n)
            return false;

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(n);
        if (!FftButterflyFp32.WillAccelerate(mode, n))
            return FastFourierTransformNormal.Forward(input, output);

        return TransformComplex(input, output, inverse: false, scale: false, mode, context);
    }

    public static bool Forward(ComplexFp32[] input, int offset, ComplexFp32[] output, int N, ComputingContext? context)
    {
        if (input is null || output is null || offset < 0 || N < 1 || offset > input.Length - N || output.Length < N)
            return false;

        return Forward(input.AsSpan(offset, N), output.AsSpan(0, N), context);
    }

    public static bool ForwardInPlace(Span<ComplexFp32> data, ComputingContext? context)
    {
        int n = data.Length;
        if (data.IsEmpty || n < 1 || !n.IsPowerOf2())
            return false;

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(n);
        if (!FftButterflyFp32.WillAccelerate(mode, n))
            return FastFourierTransformNormal.Forward(data);

        return TransformComplex(data, data, inverse: false, scale: false, mode, context);
    }

    public static bool ForwardInPlace(ComplexFp32[] data, int offset, int N, ComputingContext? context)
    {
        if (data is null || offset < 0 || N < 1 || offset > data.Length - N)
            return false;

        return ForwardInPlace(data.AsSpan(offset, N), context);
    }

    public static bool Inverse(
        ComplexFp32[] input,
        int inOffset,
        out ComplexFp32[] output,
        int outOffset,
        int N,
        bool scale,
        ComputingContext? context)
    {
        if (outOffset != 0)
            return FastFourierTransformNormal.Inverse(input, inOffset, out output, outOffset, N, scale);

        output = N > 0 ? new ComplexFp32[N] : [];
        if (input is null || inOffset < 0 || N < 1 || inOffset > input.Length - N)
            return false;

        return Inverse(input.AsSpan(inOffset, N), output, scale, context);
    }

    public static bool Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, bool scale, ComputingContext? context)
    {
        int n = input.Length;
        if (input.IsEmpty || n < 1 || !n.IsPowerOf2() || output.Length < n)
            return false;

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(n);
        if (!FftButterflyFp32.WillAccelerate(mode, n))
            return FastFourierTransformNormal.Inverse(input, output, scale);

        return TransformComplex(input, output, inverse: true, scale, mode, context);
    }

    public static bool InverseInPlace(Span<ComplexFp32> data, bool scale, ComputingContext? context)
    {
        int n = data.Length;
        if (data.IsEmpty || n < 1 || !n.IsPowerOf2())
            return false;

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(n);
        if (!FftButterflyFp32.WillAccelerate(mode, n))
            return FastFourierTransformNormal.Inverse(data, scale);

        return TransformComplex(data, data, inverse: true, scale, mode, context);
    }

    public static bool InverseInPlace(ComplexFp32[] data, int offset, int N, bool scale, ComputingContext? context)
    {
        if (data is null || offset < 0 || N < 1 || offset > data.Length - N)
            return false;

        return InverseInPlace(data.AsSpan(offset, N), scale, context);
    }

    private static bool TransformReal(
        ReadOnlySpan<float> input,
        Span<ComplexFp32> output,
        CpuExecutionMode mode,
        ComputingContext? context)
    {
        int n = input.Length;
        float[] re = ArrayPool<float>.Shared.Rent(n);
        float[] im = ArrayPool<float>.Shared.Rent(n);
        try
        {
            input.CopyTo(re.AsSpan(0, n));
            im.AsSpan(0, n).Clear();
            FftButterflyFp32.Transform(re, im, n, inverse: false, mode, context);
            Pack(re, im, output, n, scale: false);
            return true;
        }
        finally
        {
            ArrayPool<float>.Shared.Return(re);
            ArrayPool<float>.Shared.Return(im);
        }
    }

    private static bool TransformComplex(
        ReadOnlySpan<ComplexFp32> input,
        Span<ComplexFp32> output,
        bool inverse,
        bool scale,
        CpuExecutionMode mode,
        ComputingContext? context)
    {
        int n = input.Length;
        float[] re = ArrayPool<float>.Shared.Rent(n);
        float[] im = ArrayPool<float>.Shared.Rent(n);
        try
        {
            for (int i = 0; i < n; i++)
            {
                re[i] = input[i].Real;
                im[i] = input[i].Imaginary;
            }

            FftButterflyFp32.Transform(re, im, n, inverse, mode, context);
            Pack(re, im, output, n, scale);
            return true;
        }
        finally
        {
            ArrayPool<float>.Shared.Return(re);
            ArrayPool<float>.Shared.Return(im);
        }
    }

    private static void Pack(float[] re, float[] im, Span<ComplexFp32> output, int n, bool scale)
    {
        if (scale)
        {
            float s = 1f / n;
            for (int i = 0; i < n; i++)
                output[i] = new ComplexFp32(re[i] * s, im[i] * s);
            return;
        }

        for (int i = 0; i < n; i++)
            output[i] = new ComplexFp32(re[i], im[i]);
    }
}
