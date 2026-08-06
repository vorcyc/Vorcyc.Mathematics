using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

/// <summary>
/// Static AoS (<see cref="ComplexFp32"/>) FFT entry point.
/// </summary>
/// <remarks>
/// <para>
/// 0.10.12: all <c>Forward</c> / <c>Inverse</c> overloads (span, array, pointer, in-place)
/// dispatch through <c>FftButterflyFp32</c> when SIMD / parallel is beneficial,
/// otherwise the scalar <c>FastFourierTransformNormal</c> kernel.
/// Execution policy is <see cref="ComputingContext"/> / <see cref="ComputingScope"/>
/// (pointer and parameterless overloads pass <c>context: null</c>, which still honors the scope).
/// </para>
/// <para>
/// The legacy <c>FftVersion</c> enum, <c>FastFourierTransform.Version</c> global switch,
/// and AoS <c>FastFourierTransformSIMD</c> / <c>FastFourierTransformParallel</c> kernels
/// have been removed.
/// </para>
/// </remarks>
public static unsafe class FastFourierTransform
{
    #region Forward

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a real-number sequence to complex-number sequence.
    /// </summary>
    public static bool Forward(float* input, ComplexFp32* output, int N)
    {
        if (input is null || output is null || N < 1)
            return false;
        return FastFourierTransformAccelerated.Forward(
            new ReadOnlySpan<float>(input, N),
            new Span<ComplexFp32>(output, N),
            context: null);
    }

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a real-number array to complex-number array.
    /// </summary>
    public static bool Forward(float[] input, int offset, out ComplexFp32[] output, int N)
        => Forward(input, offset, out output, N, context: null);

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a real-number array to complex-number array.
    /// </summary>
    /// <param name="context">Optional execution policy. When null, uses <see cref="ComputingContext.Resolve"/>.</param>
    public static bool Forward(float[] input, int offset, out ComplexFp32[] output, int N, ComputingContext? context)
        => FastFourierTransformAccelerated.Forward(input, offset, out output, N, context);

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a real-number span to complex-number span.
    /// </summary>
    public static bool Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output)
        => Forward(input, output, context: null);

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a real-number span to complex-number span.
    /// </summary>
    /// <param name="context">Optional execution policy. When null, uses <see cref="ComputingContext.Resolve"/>.</param>
    public static bool Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output, ComputingContext? context)
        => FastFourierTransformAccelerated.Forward(input, output, context);

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a complex-number sequence to complex-number sequence.
    /// </summary>
    public static bool Forward(ComplexFp32* input, ComplexFp32* output, int N)
    {
        if (input is null || output is null || N < 1)
            return false;
        return FastFourierTransformAccelerated.Forward(
            new ReadOnlySpan<ComplexFp32>(input, N),
            new Span<ComplexFp32>(output, N),
            context: null);
    }

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a complex-number span to complex-number span.
    /// </summary>
    public static bool Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output)
        => Forward(input, output, context: null);

    /// <summary>
    /// Performs a forward FFT on complex spans with an optional execution policy.
    /// </summary>
    public static bool Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, ComputingContext? context)
        => FastFourierTransformAccelerated.Forward(input, output, context);

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a complex-number array to complex-number array.
    /// </summary>
    public static bool Forward(ComplexFp32[] input, int offset, ComplexFp32[] output, int N)
        => FastFourierTransformAccelerated.Forward(input, offset, output, N, context: null);

    /// <summary>
    /// Performs a forward Fast Fourier Transform, inplace version.
    /// </summary>
    public static bool Forward(ComplexFp32* data, int N)
    {
        if (data is null || N < 1)
            return false;
        return FastFourierTransformAccelerated.ForwardInPlace(new Span<ComplexFp32>(data, N), context: null);
    }

    /// <summary>
    /// Performs a forward Fast Fourier Transform, inplace version.
    /// </summary>
    public static bool Forward(ComplexFp32[] data, int offset, int N)
        => FastFourierTransformAccelerated.ForwardInPlace(data, offset, N, context: null);

    /// <summary>
    /// Performs a forward Fast Fourier Transform, inplace version.
    /// </summary>
    public static bool Forward(Span<ComplexFp32> data)
        => Forward(data, context: null);

    /// <summary>
    /// Performs an inplace forward FFT with an optional execution policy.
    /// </summary>
    public static bool Forward(Span<ComplexFp32> data, ComputingContext? context)
        => FastFourierTransformAccelerated.ForwardInPlace(data, context);

    #endregion

    #region Inverse

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, converting a complex-number sequence to complex-number sequence.
    /// </summary>
    public static bool Inverse(ComplexFp32* input, ComplexFp32* output, int N, bool scale = true)
    {
        if (input is null || output is null || N < 1)
            return false;
        return FastFourierTransformAccelerated.Inverse(
            new ReadOnlySpan<ComplexFp32>(input, N),
            new Span<ComplexFp32>(output, N),
            scale,
            context: null);
    }

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, converting a complex-number array to complex-number array.
    /// </summary>
    public static bool Inverse(ComplexFp32[] input, int inOffset, out ComplexFp32[] output, int outOffset, int N, bool scale = true)
        => Inverse(input, inOffset, out output, outOffset, N, scale, context: null);

    /// <summary>
    /// Performs an inverse FFT on complex arrays with an optional execution policy.
    /// </summary>
    public static bool Inverse(
        ComplexFp32[] input,
        int inOffset,
        out ComplexFp32[] output,
        int outOffset,
        int N,
        bool scale,
        ComputingContext? context)
        => FastFourierTransformAccelerated.Inverse(input, inOffset, out output, outOffset, N, scale, context);

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, converting a complex-number span to complex-number span.
    /// </summary>
    public static bool Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, bool scale = true)
        => Inverse(input, output, scale, context: null);

    /// <summary>
    /// Performs an inverse FFT on complex spans with an optional execution policy.
    /// </summary>
    public static bool Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, bool scale, ComputingContext? context)
        => FastFourierTransformAccelerated.Inverse(input, output, scale, context);

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, inplace version.
    /// </summary>
    public static bool Inverse(ComplexFp32* data, int N, bool scale = true)
    {
        if (data is null || N < 1)
            return false;
        return FastFourierTransformAccelerated.InverseInPlace(new Span<ComplexFp32>(data, N), scale, context: null);
    }

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, inplace version.
    /// </summary>
    public static bool Inverse(ComplexFp32[] data, int offset, int N, bool scale = true)
        => FastFourierTransformAccelerated.InverseInPlace(data, offset, N, scale, context: null);

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, inplace version.
    /// </summary>
    public static bool Inverse(Span<ComplexFp32> data, bool scale = true)
        => Inverse(data, scale, context: null);

    /// <summary>
    /// Performs an inplace inverse FFT with an optional execution policy.
    /// </summary>
    public static bool Inverse(Span<ComplexFp32> data, bool scale, ComputingContext? context)
        => FastFourierTransformAccelerated.InverseInPlace(data, scale, context);

    #endregion
}
