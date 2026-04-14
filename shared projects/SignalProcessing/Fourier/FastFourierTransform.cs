using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

/*25.5.10 与 SIMD 和 Parallel版本一起实现。
 * 按理说应该有 SIMDParallel 同时的版本，并且达到最优，但是暂时有问题，所以暂时不放上来。
 * 
 * 那么基于现在的版本，小规模的时候用 Normal版本，，大规模的时候用 Parallel版本 是最优选择。
 */


/// <summary>
/// FFT code version.
/// </summary>
/// <remarks>
/// Prefer <see cref="ComputingContext"/> and <see cref="ComputingScope"/> for execution policy.
/// </remarks>
[Obsolete("Use ComputingContext and ComputingScope instead. This enum will be removed in a future release.", false)]
public enum FftVersion
{
    /// <summary>
    /// 普通版本，默认版本。当数据量小时推荐。
    /// </summary>
    Normal,
    /// <summary>
    /// SIMD指令优化的版本。由于目前的设计，当数据量过大时会栈溢出。
    /// </summary>
    SIMD,
    /// <summary>
    /// 并行优化的版本，当数据量大时推荐。
    /// </summary>
    Parallel
}

/// <summary>
/// Provides methods for FFT.
/// </summary>
/// <remarks>
/// <para>5.5.10 与 SIMD 和 Parallel版本一起实现，作为3种版本的统一接口。</para>
/// <para>本应该有 SIMDParallel 同时的版本，并且达到最优，但是暂时有问题，所以暂时不放上来。</para>
/// <para>那么基于现在的版本，小规模的时候用 Normal版本，大规模的时候用 Parallel版本 是最优选择。</para>
/// </remarks>
public unsafe static class FastFourierTransform
{
    // 委托定义，覆盖所有 Forward 和 Inverse 重载
    private delegate bool ForwardPtrFloatDelegate(float* input, ComplexFp32* output, int N);
    private delegate bool ForwardArrayFloatDelegate(float[] input, int offset, ComplexFp32* output, int N);
    private delegate bool ForwardArrayOutFloatDelegate(float[] input, int offset, out ComplexFp32[] output, int N);
    private delegate bool ForwardSpanFloatDelegate(ReadOnlySpan<float> input, Span<ComplexFp32> output);
    private delegate bool ForwardPtrComplexDelegate(ComplexFp32* input, ComplexFp32* output, int N);
    private delegate bool ForwardSpanComplexDelegate(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output);
    private delegate bool ForwardArrayComplexDelegate(ComplexFp32[] input, int offset, ComplexFp32[] output, int N);
    private delegate bool ForwardInplacePtrDelegate(ComplexFp32* data, int N);
    private delegate bool ForwardInplaceArrayDelegate(ComplexFp32[] data, int offset, int N);
    private delegate bool ForwardInplaceSpanDelegate(Span<ComplexFp32> data);

    private delegate bool InversePtrDelegate(ComplexFp32* input, ComplexFp32* output, int N, bool scale);
    private delegate bool InverseArrayDelegate(ComplexFp32[] input, int inOffset, out ComplexFp32[] output, int outOffset, int N, bool scale);
    private delegate bool InverseSpanDelegate(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, bool scale);
    private delegate bool InverseInplacePtrDelegate(ComplexFp32* data, int N, bool scale);
    private delegate bool InverseInplaceArrayDelegate(ComplexFp32[] data, int offset, int N, bool scale);
    private delegate bool InverseInplaceSpanDelegate(Span<ComplexFp32> data, bool scale);

    // 委托字段，默认指向 Normal 版本
    private static ForwardPtrFloatDelegate _forwardPtrFloat = FastFourierTransformNormal.Forward;
    private static ForwardArrayOutFloatDelegate _forwardArrayOutFloat = FastFourierTransformNormal.Forward;
    private static ForwardSpanFloatDelegate _forwardSpanFloat = FastFourierTransformNormal.Forward;
    private static ForwardPtrComplexDelegate _forwardPtrComplex = FastFourierTransformNormal.Forward;
    private static ForwardSpanComplexDelegate _forwardSpanComplex = FastFourierTransformNormal.Forward;
    private static ForwardArrayComplexDelegate _forwardArrayComplex = FastFourierTransformNormal.Forward;
    private static ForwardInplacePtrDelegate _forwardInplacePtr = FastFourierTransformNormal.Forward;
    private static ForwardInplaceArrayDelegate _forwardInplaceArray = FastFourierTransformNormal.Forward;
    private static ForwardInplaceSpanDelegate _forwardInplaceSpan = FastFourierTransformNormal.Forward;

    private static InversePtrDelegate _inversePtr = FastFourierTransformNormal.Inverse;
    private static InverseArrayDelegate _inverseArray = FastFourierTransformNormal.Inverse;
    private static InverseSpanDelegate _inverseSpan = FastFourierTransformNormal.Inverse;
    private static InverseInplacePtrDelegate _inverseInplacePtr = FastFourierTransformNormal.Inverse;
    private static InverseInplaceArrayDelegate _inverseInplaceArray = FastFourierTransformNormal.Inverse;
    private static InverseInplaceSpanDelegate _inverseInplaceSpan = FastFourierTransformNormal.Inverse;

#pragma warning disable CS0618 // FftVersion is obsolete; retained for legacy pointer overloads and internal dispatch.
    private static FftVersion _version = FftVersion.Normal;

    private static FftVersion ResolveContext(ComputingContext? context, int? transformLength)
    {
        var resolved = ComputingContext.Resolve(context);
        return resolved.ResolveCpuMode(transformLength) switch
        {
            CpuExecutionMode.Normal => FftVersion.Normal,
            CpuExecutionMode.Simd => FftVersion.SIMD,
            CpuExecutionMode.Parallel => FftVersion.Parallel,
            _ => FftVersion.Normal
        };
    }

    private static ForwardSpanFloatDelegate ForwardSpanFloatFor(FftVersion version)
        => version switch
        {
            FftVersion.SIMD => FastFourierTransformSIMD.Forward,
            FftVersion.Parallel => FastFourierTransformParallel.Forward,
            _ => FastFourierTransformNormal.Forward
        };

    private static ForwardArrayOutFloatDelegate ForwardArrayOutFloatFor(FftVersion version)
        => version switch
        {
            FftVersion.SIMD => FastFourierTransformSIMD.Forward,
            FftVersion.Parallel => FastFourierTransformParallel.Forward,
            _ => FastFourierTransformNormal.Forward
        };

    private static ForwardSpanComplexDelegate ForwardSpanComplexFor(FftVersion version)
        => version switch
        {
            FftVersion.SIMD => FastFourierTransformSIMD.Forward,
            FftVersion.Parallel => FastFourierTransformParallel.Forward,
            _ => FastFourierTransformNormal.Forward
        };

    private static InverseSpanDelegate InverseSpanFor(FftVersion version)
        => version switch
        {
            FftVersion.SIMD => FastFourierTransformSIMD.Inverse,
            FftVersion.Parallel => FastFourierTransformParallel.Inverse,
            _ => FastFourierTransformNormal.Inverse
        };

    private static InverseArrayDelegate InverseArrayFor(FftVersion version)
        => version switch
        {
            FftVersion.SIMD => FastFourierTransformSIMD.Inverse,
            FftVersion.Parallel => FastFourierTransformParallel.Inverse,
            _ => FastFourierTransformNormal.Inverse
        };
#pragma warning restore CS0618

    /// <summary>
    /// Sets the FFT version to perform related methods.
    /// </summary>
    /// <remarks>
    /// Prefer per-call <c>Forward(..., ComputingContext?)</c> or <see cref="ComputingScope.Enter"/>.
    /// Only pointer-based overloads without a context parameter still use this global setting.
    /// </remarks>
    [Obsolete("Use ComputingContext on Forward/Inverse overloads or ComputingScope instead of mutating global FFT delegates.", false)]
    public static FftVersion Version
    {
        get => _version;
        set
        {
            if (_version == value)
                return;

            _version = value;
            switch (value)
            {
                case FftVersion.Normal:
                    _forwardPtrFloat = FastFourierTransformNormal.Forward;
                    _forwardArrayOutFloat = FastFourierTransformNormal.Forward;
                    _forwardSpanFloat = FastFourierTransformNormal.Forward;
                    _forwardPtrComplex = FastFourierTransformNormal.Forward;
                    _forwardSpanComplex = FastFourierTransformNormal.Forward;
                    _forwardArrayComplex = FastFourierTransformNormal.Forward;
                    _forwardInplacePtr = FastFourierTransformNormal.Forward;
                    _forwardInplaceArray = FastFourierTransformNormal.Forward;
                    _forwardInplaceSpan = FastFourierTransformNormal.Forward;
                    _inversePtr = FastFourierTransformNormal.Inverse;
                    _inverseArray = FastFourierTransformNormal.Inverse;
                    _inverseSpan = FastFourierTransformNormal.Inverse;
                    _inverseInplacePtr = FastFourierTransformNormal.Inverse;
                    _inverseInplaceArray = FastFourierTransformNormal.Inverse;
                    _inverseInplaceSpan = FastFourierTransformNormal.Inverse;
                    break;
                case FftVersion.SIMD:
                    _forwardPtrFloat = FastFourierTransformSIMD.Forward;
                    _forwardArrayOutFloat = FastFourierTransformSIMD.Forward;
                    _forwardSpanFloat = FastFourierTransformSIMD.Forward;
                    _forwardPtrComplex = FastFourierTransformSIMD.Forward;
                    _forwardSpanComplex = FastFourierTransformSIMD.Forward;
                    _forwardArrayComplex = FastFourierTransformSIMD.Forward;
                    _forwardInplacePtr = FastFourierTransformSIMD.Forward;
                    _forwardInplaceArray = FastFourierTransformSIMD.Forward;
                    _forwardInplaceSpan = FastFourierTransformSIMD.Forward;
                    _inversePtr = FastFourierTransformSIMD.Inverse;
                    _inverseArray = FastFourierTransformSIMD.Inverse;
                    _inverseSpan = FastFourierTransformSIMD.Inverse;
                    _inverseInplacePtr = FastFourierTransformSIMD.Inverse;
                    _inverseInplaceArray = FastFourierTransformSIMD.Inverse;
                    _inverseInplaceSpan = FastFourierTransformSIMD.Inverse;
                    break;
                case FftVersion.Parallel:
                    _forwardPtrFloat = FastFourierTransformParallel.Forward;
                    _forwardArrayOutFloat = FastFourierTransformParallel.Forward;
                    _forwardSpanFloat = FastFourierTransformParallel.Forward;
                    _forwardPtrComplex = FastFourierTransformParallel.Forward;
                    _forwardSpanComplex = FastFourierTransformParallel.Forward;
                    _forwardArrayComplex = FastFourierTransformParallel.Forward;
                    _forwardInplacePtr = FastFourierTransformParallel.Forward;
                    _forwardInplaceArray = FastFourierTransformParallel.Forward;
                    _forwardInplaceSpan = FastFourierTransformParallel.Forward;
                    _inversePtr = FastFourierTransformParallel.Inverse;
                    _inverseArray = FastFourierTransformParallel.Inverse;
                    _inverseSpan = FastFourierTransformParallel.Inverse;
                    _inverseInplacePtr = FastFourierTransformParallel.Inverse;
                    _inverseInplaceArray = FastFourierTransformParallel.Inverse;
                    _inverseInplaceSpan = FastFourierTransformParallel.Inverse;
                    break;
                default:
                    throw new InvalidOperationException("Unknown FFT version");
            }
        }
    }

    #region Forward

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a real-number sequence to complex-number sequence.
    /// </summary>
    /// <param name="input">Input data: real-number sequence (time-domain).</param>
    /// <param name="output">Transform result: complex-number sequence (frequency-domain).</param>
    /// <param name="N">FFT length, must be a power of 2.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static unsafe bool Forward(float* input, ComplexFp32* output, int N)
    {
        return _forwardPtrFloat(input, output, N);
    }


    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a real-number array to complex-number array.
    /// </summary>
    /// <param name="input">Input data: real-number array (time-domain).</param>
    /// <param name="offset">Offset into the input array.</param>
    /// <param name="output">Transform result: complex-number array (frequency-domain).</param>
    /// <param name="N">FFT length, must be a power of 2.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static bool Forward(float[] input, int offset, out ComplexFp32[] output, int N)
        => Forward(input, offset, out output, N, context: null);

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a real-number array to complex-number array.
    /// </summary>
    /// <param name="context">Optional execution policy. When null, uses <see cref="ComputingContext.Resolve"/>.</param>
    public static bool Forward(float[] input, int offset, out ComplexFp32[] output, int N, ComputingContext? context)
    {
        var version = ResolveContext(context, N);
        return ForwardArrayOutFloatFor(version)(input, offset, out output, N);
    }

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a real-number span to complex-number span.
    /// </summary>
    /// <param name="input">Input data: real-number span (time-domain).</param>
    /// <param name="output">Transform result: complex-number span (frequency-domain).</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static bool Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output)
        => Forward(input, output, context: null);

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a real-number span to complex-number span.
    /// </summary>
    /// <param name="context">Optional execution policy. When null, uses <see cref="ComputingContext.Resolve"/>.</param>
    public static bool Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output, ComputingContext? context)
    {
        var version = ResolveContext(context, input.Length);
        return ForwardSpanFloatFor(version)(input, output);
    }

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a complex-number sequence to complex-number sequence.
    /// </summary>
    /// <param name="input">Input data: complex-number sequence (time-domain).</param>
    /// <param name="output">Transform result: complex-number sequence (frequency-domain).</param>
    /// <param name="N">FFT length, must be a power of 2.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static unsafe bool Forward(ComplexFp32* input, ComplexFp32* output, int N)
    {
        return _forwardPtrComplex(input, output, N);
    }

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a complex-number span to complex-number span.
    /// </summary>
    /// <param name="input">Input data: complex-number span (time-domain).</param>
    /// <param name="output">Transform result: complex-number span (frequency-domain).</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static bool Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output)
        => Forward(input, output, context: null);

    /// <summary>
    /// Performs a forward FFT on complex spans with an optional execution policy.
    /// </summary>
    public static bool Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, ComputingContext? context)
    {
        var version = ResolveContext(context, input.Length);
        return ForwardSpanComplexFor(version)(input, output);
    }

    /// <summary>
    /// Performs a forward Fast Fourier Transform, converting a complex-number array to complex-number array.
    /// </summary>
    /// <param name="input">Input data: complex-number array (time-domain).</param>
    /// <param name="offset">Offset into the input array.</param>
    /// <param name="output">Transform result: complex-number array (frequency-domain).</param>
    /// <param name="N">FFT length, must be a power of 2.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static bool Forward(ComplexFp32[] input, int offset, ComplexFp32[] output, int N)
    {
        return _forwardArrayComplex(input, offset, output, N);
    }

    /// <summary>
    /// Performs a forward Fast Fourier Transform, inplace version.
    /// </summary>
    /// <param name="data">Input and output data: complex-number sequence (time-domain to frequency-domain).</param>
    /// <param name="N">FFT length, must be a power of 2.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static unsafe bool Forward(ComplexFp32* data, int N)
    {
        return _forwardInplacePtr(data, N);
    }

    /// <summary>
    /// Performs a forward Fast Fourier Transform, inplace version.
    /// </summary>
    /// <param name="data">Input and output data: complex-number array (time-domain to frequency-domain).</param>
    /// <param name="offset">Offset into the data array.</param>
    /// <param name="N">FFT length, must be a power of 2.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static bool Forward(ComplexFp32[] data, int offset, int N)
    {
        return _forwardInplaceArray(data, offset, N);
    }

    /// <summary>
    /// Performs a forward Fast Fourier Transform, inplace version.
    /// </summary>
    /// <param name="data">Input and output data: complex-number span (time-domain to frequency-domain).</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static bool Forward(Span<ComplexFp32> data)
    {
        return _forwardInplaceSpan(data);
    }

    #endregion

    #region Inverse

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, converting a complex-number sequence to complex-number sequence.
    /// </summary>
    /// <param name="input">Input data: complex-number sequence (frequency-domain).</param>
    /// <param name="output">Transform result: complex-number sequence (time-domain).</param>
    /// <param name="N">FFT length, must be a power of 2.</param>
    /// <param name="scale">If true, scales the result by 1/N.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static unsafe bool Inverse(ComplexFp32* input, ComplexFp32* output, int N, bool scale = true)
    {
        return _inversePtr(input, output, N, scale);
    }

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, converting a complex-number array to complex-number array.
    /// </summary>
    /// <param name="input">Input data: complex-number array (frequency-domain).</param>
    /// <param name="inOffset">Offset into the input array.</param>
    /// <param name="output">Transform result: complex-number array (time-domain).</param>
    /// <param name="outOffset">Offset into the output array.</param>
    /// <param name="N">FFT length, must be a power of 2.</param>
    /// <param name="scale">If true, scales the result by 1/N.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
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
    {
        var version = ResolveContext(context, N);
        return InverseArrayFor(version)(input, inOffset, out output, outOffset, N, scale);
    }

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, converting a complex-number span to complex-number span.
    /// </summary>
    /// <param name="input">Input data: complex-number span (frequency-domain).</param>
    /// <param name="output">Transform result: complex-number span (time-domain).</param>
    /// <param name="scale">If true, scales the result by 1/N.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static bool Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, bool scale = true)
        => Inverse(input, output, scale, context: null);

    /// <summary>
    /// Performs an inverse FFT on complex spans with an optional execution policy.
    /// </summary>
    public static bool Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, bool scale, ComputingContext? context)
    {
        var version = ResolveContext(context, input.Length);
        return InverseSpanFor(version)(input, output, scale);
    }

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, inplace version.
    /// </summary>
    /// <param name="data">Input and output data: complex-number sequence (frequency-domain to time-domain).</param>
    /// <param name="N">FFT length, must be a power of 2.</param>
    /// <param name="scale">If true, scales the result by 1/N.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static unsafe bool Inverse(ComplexFp32* data, int N, bool scale = true)
    {
        return _inverseInplacePtr(data, N, scale);
    }

    /// <summary>
    /// Performs an inverse Fast Fourier Transform, inplace version.
    /// </summary>
    /// <param name="data">Input and output data: complex-number array (frequency-domain to time-domain).</param>
    /// <param name="offset">Offset into the data array.</param>
    /// <param name="N">FFT length, must be a power of 2.</param>
    /// <param name="scale">If true, scales the result by 1/N.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static bool Inverse(ComplexFp32[] data, int offset, int N, bool scale = true)
    {
        return _inverseInplaceArray(data, offset, N, scale);
    }

    /// <summary>
    /// Performs  Performs an inverse Fast Fourier Transform, inplace version.
    /// </summary>
    /// <param name="data">Input and output data: complex-number span (frequency-domain to time-domain).</param>
    /// <param name="scale">If true, scales the result by 1/N.</param>
    /// <returns>True if the transform succeeds, false if input parameters are invalid.</returns>
    public static bool Inverse(Span<ComplexFp32> data, bool scale = true)
        => Inverse(data, scale, context: null);

    /// <summary>
    /// Performs an inplace inverse FFT with an optional execution policy.
    /// </summary>
    public static bool Inverse(Span<ComplexFp32> data, bool scale, ComputingContext? context)
    {
        var version = ResolveContext(context, data.Length);
        return InverseSpanFor(version)(data, data, scale);
    }

    #endregion
}