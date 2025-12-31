using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// 提供针对单精度复数与实数数据执行一维离散傅里叶变换 (DFT) 的静态方法集合。
/// 封装 FFTW (single precision) 的 1D 复数-复数、实数-复数、复数-实数正/逆变换接口，并负责规划 (plan) 的创建与销毁。
/// </summary>
/// <remarks>
/// 使用本类型的方法需注意：
/// 1. 使用 <see cref="Span{T}"/> 重载的方法会在内部通过 <c>fixed</c> 语句暂时固定内存，调用结束即解除。<br/>
/// 2. 所有方法均非线程安全；不要在不同线程上同时对存在重叠内存区域的缓冲区调用这些方法。<br/>
/// 3. 逆变换（尤其是 Complex→Real）通常需要执行归一化 (按 1/N 缩放)。本类的 C2R 方法提供可选缩放操作。复数逆变换完成后亦可手动进行缩放。<br/>
/// 4. 对于实数与复数之间的变换，长度关系遵循 FFTW 的紧凑存储格式：<br/>
///    - R2C：实数输入长度 N 与复数输出半谱长度 k 满足 k ≥ floor(N/2) + 1。<br/>
///    - C2R：复数输入半谱长度 k 与实数输出长度 N 满足 k ≥ floor(N/2) + 1。<br/>
/// </remarks>
public static partial class Dft1D
{
    // ==========================
    // 精简友好命名的公共 API（Span<T>）
    // ==========================

    // Complex-to-Complex (Out-of-Place)
    /// <summary>
    /// 执行复数→复数的一维正向傅里叶变换（非原地，输出写入独立缓冲区）。
    /// </summary>
    /// <param name="input">复数输入数据。</param>
    /// <param name="output">复数输出数据缓冲区（长度须与输入相同）。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplex(input, output, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行复数→复数的一维逆向傅里叶变换（非原地，输出写入独立缓冲区）。
    /// 注意：FFTW 的逆变换默认不执行归一化，常需在外部按 1/N 缩放。
    /// </summary>
    /// <param name="input">复数输入数据。</param>
    /// <param name="output">复数输出数据缓冲区（长度须与输入相同）。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplex(input, output, fftw_direction.Backward, flags);

    // Complex-to-Complex (In-Place)
    /// <summary>
    /// 执行复数缓冲区的一维正向傅里叶变换（原地，结果覆盖输入）。
    /// </summary>
    /// <param name="buffer">复数输入与输出共享的缓冲区。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void ForwardInPlace(Span<ComplexFp32> buffer, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplexInPlace(buffer, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行复数缓冲区的一维逆向傅里叶变换（原地，结果覆盖输入）。
    /// 注意：FFTW 的逆变换默认不执行归一化，常需在外部按 1/N 缩放。
    /// </summary>
    /// <param name="buffer">复数输入与输出共享的缓冲区。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void InverseInPlace(Span<ComplexFp32> buffer, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplexInPlace(buffer, fftw_direction.Backward, flags);

    // Real-to-Complex (Forward)
    /// <summary>
    /// 执行实数→复数的一维正向傅里叶变换（紧凑半谱输出）。
    /// </summary>
    /// <param name="realInput">实数输入数据，长度为 N。</param>
    /// <param name="complexOutput">复数输出半谱缓冲区，需满足：complexOutput.Length ≥ realInput.Length / 2 + 1。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(ReadOnlySpan<float> realInput, Span<ComplexFp32> complexOutput, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DR2C(realInput, complexOutput, flags);

    // Complex-to-Real (Inverse)
    /// <summary>
    /// 执行复数半谱→实数的一维逆向傅里叶变换，可选按 1/N 自动归一化。
    /// </summary>
    /// <param name="complexInput">复数输入半谱缓冲区，需满足：complexInput.Length ≥ realOutput.Length / 2 + 1。</param>
    /// <param name="realOutput">实数输出缓冲区，长度为 N。</param>
    /// <param name="scale">是否按 1/N（N = realOutput.Length）进行归一化，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(ReadOnlySpan<ComplexFp32> complexInput, Span<float> realOutput, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DC2R(complexInput, realOutput, scale, flags);

    // ==========================
    // 原始实现（内部管线，设为 internal）
    // ==========================

    /// <summary>
    /// 复数缓冲区原地一维 DFT，方向由 <paramref name="direction"/> 指定。
    /// </summary>
    /// <param name="buffer">复数输入与输出共享的缓冲区。</param>
    /// <param name="direction">变换方向：<see cref="fftw_direction.Forward"/> 或 <see cref="fftw_direction.Backward"/>。</param>
    /// <param name="flags">规划策略标志。</param>
    /// <exception cref="ArgumentException">当缓冲区为空时抛出。</exception>
    internal static void Dft1DComplexInPlace(Span<ComplexFp32> buffer, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(buffer);

        unsafe
        {
            fixed (ComplexFp32* p = buffer)
            {
                var ptr = (IntPtr)p;

                var plan = fftwf.dft_1d(buffer.Length, ptr, ptr, direction, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create in-place complex plan.");
                try
                {
                    fftwf.execute(plan);
                }
                finally
                {
                    fftwf.destroy_plan(plan);
                }
            }
        }
    }

    /// <summary>
    /// 复数→复数的一维 DFT（非原地），方向由 <paramref name="direction"/> 指定。
    /// </summary>
    /// <param name="input">复数输入数据。</param>
    /// <param name="output">复数输出数据缓冲区（长度须与输入相同）。</param>
    /// <param name="direction">变换方向：<see cref="fftw_direction.Forward"/> 或 <see cref="fftw_direction.Backward"/>。</param>
    /// <param name="flags">规划策略标志。</param>
    /// <exception cref="ArgumentException">当输入与输出长度不一致时抛出。</exception>
    internal static void Dft1DComplex(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        ArgumentException.ThrowIfArrayLengthNotEqual(input, output, "Input and output spans must have the same length.");

        unsafe
        {
            fixed (ComplexFp32* pInput = input)
            fixed (ComplexFp32* pOutput = output)
            {
                var plan = fftwf.dft_1d(input.Length, (IntPtr)pInput, (IntPtr)pOutput, direction, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create complex plan.");
                try
                {
                    fftwf.execute(plan);
                }
                finally
                {
                    fftwf.destroy_plan(plan);
                }
            }
        }
    }

    /// <summary>
    /// 实数→复数的一维正向变换 (R2C)，输出为紧凑半谱（包含 DC 与 Nyquist）。
    /// </summary>
    /// <param name="realInput">实数输入数据，长度为 N。</param>
    /// <param name="complexOutput">复数输出半谱缓冲区，需满足 complexOutput.Length ≥ realInput.Length / 2 + 1。</param>
    /// <param name="flags">规划策略标志。</param>
    /// <exception cref="ArgumentException">当长度关系不满足要求时抛出。</exception>
    internal static void Dft1DR2C(ReadOnlySpan<float> realInput, Span<ComplexFp32> complexOutput, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(realInput);
        ArgumentNullException.ThrowIfEmpty(complexOutput);

        var required = (realInput.Length / 2) + 1;
        if (complexOutput.Length < required)
            throw new ArgumentException("For R2C: complexOutput.Length must be at least realInput.Length / 2 + 1 (compact half-spectrum).");

        unsafe
        {
            fixed (float* pRealInput = realInput)
            fixed (ComplexFp32* pComplexOutput = complexOutput)
            {
                var plan = fftwf.dft_r2c_1d(realInput.Length, (IntPtr)pRealInput, (IntPtr)pComplexOutput, flags);
                InvalidOperationExceptionExtension.ThrowIfZero(plan, "Failed to create 1D real to complex plan.");
                try { fftwf.execute(plan); }
                finally { fftwf.destroy_plan(plan); }
            }
        }
    }

    /// <summary>
    /// 复数半谱→实数的一维逆向变换 (C2R)，可选按 1/N 自动归一化。
    /// </summary>
    /// <param name="complexInput">复数输入半谱缓冲区。</param>
    /// <param name="realOutput">实数输出缓冲区，需满足 complexInput.Length ≥ realOutput.Length / 2 + 1。</param>
    /// <param name="scale">是否按 1/N（N = realOutput.Length）进行归一化。</param>
    /// <param name="flags">规划策略标志。</param>
    /// <exception cref="ArgumentException">当长度关系不满足要求时抛出。</exception>
    internal static void Dft1DC2R(ReadOnlySpan<ComplexFp32> complexInput, Span<float> realOutput, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(complexInput);
        ArgumentNullException.ThrowIfEmpty(realOutput);

        var required = (realOutput.Length / 2) + 1;
        if (complexInput.Length < required)
            throw new ArgumentException("For C2R: complexInput.Length must be at least realOutput.Length / 2 + 1 (compact half-spectrum).");

        unsafe
        {
            fixed (ComplexFp32* pComplexInput = complexInput)
            fixed (float* pRealOutput = realOutput)
            {
                var plan = fftwf.dft_c2r_1d(realOutput.Length, (IntPtr)pComplexInput, (IntPtr)pRealOutput, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create 1D complex to real plan.");
                try { fftwf.execute(plan); }
                finally { fftwf.destroy_plan(plan); }
            }
        }

        if (scale)
        {
            // N 是实数输出长度（输入实数长度）
            var factor = 1f / realOutput.Length;
            ScaleInPlace(realOutput, factor); // 使用 SIMD 加速的 Span<float> 缩放
        }
    }

    // ==========================
    // 缩放辅助（提取 + SIMD 优化）
    // ==========================

    /// <summary>
    /// 对 <see cref="Span{Single}"/> 进行就地缩放；在可用时使用 <see cref="Vector{T}"/> SIMD。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ScaleInPlace(Span<float> data, float factor)
    {
        if (data.IsEmpty) return;

        if (Vector.IsHardwareAccelerated && data.Length >= Vector<float>.Count)
        {
            var vFactor = new Vector<float>(factor);
            var vecSpan = MemoryMarshal.Cast<float, Vector<float>>(data);

            for (int i = 0; i < vecSpan.Length; i++)
            {
                vecSpan[i] *= vFactor;
            }

            int processed = vecSpan.Length * Vector<float>.Count;
            for (int i = processed; i < data.Length; i++)
            {
                data[i] *= factor;
            }
        }
        else
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] *= factor;
            }
        }
    }
}