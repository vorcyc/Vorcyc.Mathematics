using System.Numerics;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// 基于 FFTW 双精度接口的一维离散傅里叶变换 (DFT) 辅助类。
/// 提供简洁公共 API：Forward / Inverse / ForwardInPlace / InverseInPlace；原始实现改为 internal 并统一处理计划的创建与销毁。
/// </summary>
/// <remarks>
/// 使用说明：<br/>
/// 1) 使用 <see cref="PinnableArray{T}"/> 的重载要求缓冲区已固定 (Pinned)；未固定将抛出异常。<br/>
/// 2) 使用 <see cref="Span{T}"/> 的重载会在内部通过 <c>fixed</c> 暂时固定内存。<br/>
/// 3) 所有方法均非线程安全；不要在多个线程中对有内存重叠的缓冲区并发调用。<br/>
/// 4) FFTW 默认不执行归一化。逆变换通常需要按 1/N 缩放；C2R 重载提供可选缩放参数。<br/>
/// 5) R2C/C2R 的长度关系遵循 FFTW 紧凑半谱约定：R2C 输入 N，输出长度为 N/2+1；C2R 输入长度为 N/2+1，输出为 N。<br/>
/// </remarks>
public static partial class Dft1D
{
    // ==========================
    // 精简友好命名的公共 API
    // ==========================

    // Complex-to-Complex (Out-of-Place)
    /// <summary>
    /// 执行复数→复数的一维正向傅里叶变换（非原地）。
    /// </summary>
    /// <param name="input">复数输入缓冲区（已固定）。</param>
    /// <param name="output">复数输出缓冲区（已固定，长度与输入相同）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<Complex> input, PinnableArray<Complex> output, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplex(input, output, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行复数→复数的一维正向傅里叶变换（非原地）。
    /// </summary>
    /// <param name="input">复数输入数据。</param>
    /// <param name="output">复数输出数据缓冲区（长度须与输入相同）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(ReadOnlySpan<Complex> input, Span<Complex> output, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplex(input, output, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行复数→复数的一维逆向傅里叶变换（非原地）。
    /// 注意：FFTW 的逆变换默认不执行归一化，通常需在外部按 1/N 缩放。
    /// </summary>
    /// <param name="input">复数输入缓冲区（已固定）。</param>
    /// <param name="output">复数输出缓冲区（已固定，长度与输入相同）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(PinnableArray<Complex> input, PinnableArray<Complex> output, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplex(input, output, fftw_direction.Backward, flags);

    /// <summary>
    /// 执行复数→复数的一维逆向傅里叶变换（非原地）。
    /// 注意：FFTW 的逆变换默认不执行归一化，通常需在外部按 1/N 缩放。
    /// </summary>
    /// <param name="input">复数输入数据。</param>
    /// <param name="output">复数输出数据缓冲区（长度须与输入相同）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(ReadOnlySpan<Complex> input, Span<Complex> output, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplex(input, output, fftw_direction.Backward, flags);

    // Complex-to-Complex (In-Place)
    /// <summary>
    /// 执行复数缓冲区一维正向傅里叶变换（原地，结果覆盖输入）。
    /// </summary>
    /// <param name="buffer">复数输入与输出共享的已固定缓冲区。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void ForwardInPlace(PinnableArray<Complex> buffer, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplexInPlace(buffer, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行复数缓冲区一维正向傅里叶变换（原地，结果覆盖输入）。
    /// </summary>
    /// <param name="buffer">复数输入与输出共享的缓冲区。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void ForwardInPlace(Span<Complex> buffer, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplexInPlace(buffer, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行复数缓冲区一维逆向傅里叶变换（原地，结果覆盖输入）。
    /// 注意：FFTW 的逆变换默认不执行归一化，通常需在外部按 1/N 缩放。
    /// </summary>
    /// <param name="buffer">复数输入与输出共享的已固定缓冲区。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void InverseInPlace(PinnableArray<Complex> buffer, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplexInPlace(buffer, fftw_direction.Backward, flags);

    /// <summary>
    /// 执行复数缓冲区一维逆向傅里叶变换（原地，结果覆盖输入）。
    /// 注意：FFTW 的逆变换默认不执行归一化，通常需在外部按 1/N 缩放。
    /// </summary>
    /// <param name="buffer">复数输入与输出共享的缓冲区。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void InverseInPlace(Span<Complex> buffer, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DComplexInPlace(buffer, fftw_direction.Backward, flags);

    // Real-to-Complex (Forward)
    /// <summary>
    /// 执行实数→复数的一维正向傅里叶变换（R2C，紧凑半谱输出）。
    /// </summary>
    /// <param name="realInput">已固定的实数输入缓冲区，长度为 N。</param>
    /// <param name="complexOutput">已固定的复数输出缓冲区，长度应不小于 N/2+1（紧凑半谱）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<double> realInput, PinnableArray<Complex> complexOutput, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DR2C(realInput, complexOutput, flags);

    /// <summary>
    /// 执行实数→复数的一维正向傅里叶变换（R2C，紧凑半谱输出）。
    /// </summary>
    /// <param name="realInput">实数输入数据，长度为 N。</param>
    /// <param name="complexOutput">复数输出半谱缓冲区，长度应不小于 N/2+1。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(ReadOnlySpan<double> realInput, Span<Complex> complexOutput, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DR2C(realInput, complexOutput, flags);

    // Complex-to-Real (Inverse)
    /// <summary>
    /// 执行复数半谱→实数的一维逆向傅里叶变换（C2R），可选按 1/N 自动归一化。
    /// </summary>
    /// <param name="complexInput">已固定的复数输入半谱缓冲区，长度应不小于 N/2+1。</param>
    /// <param name="realOutput">已固定的实数输出缓冲区，长度为 N。</param>
    /// <param name="scale">是否按 1/N 归一化，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(PinnableArray<Complex> complexInput, PinnableArray<double> realOutput, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DC2R(complexInput, realOutput, scale, flags);

    /// <summary>
    /// 执行复数半谱→实数的一维逆向傅里叶变换（C2R），可选按 1/N 自动归一化。
    /// </summary>
    /// <param name="complexInput">复数输入半谱缓冲区，长度应不小于 N/2+1。</param>
    /// <param name="realOutput">实数输出缓冲区，长度为 N。</param>
    /// <param name="scale">是否进行按 1/N 的归一化，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(ReadOnlySpan<Complex> complexInput, Span<double> realOutput, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
        => Dft1DC2R(complexInput, realOutput, scale, flags);

    // ==========================
    // 原始实现（内部管线，设为 internal）
    // ==========================

    // 1D complex-to-complex

    /// <summary>
    /// 对双精度复数缓冲区执行一维复数到复数 (C2C) DFT（原地变换）。
    /// </summary>
    internal static void Dft1DComplexInPlace(PinnableArray<Complex> buffer, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(buffer);

        var plan = fftw.dft_1d(buffer.Length, buffer, buffer, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create in-place complex plan.");

        try
        {
            fftw.execute(plan);
        }
        finally
        {
            fftw.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 对双精度复数数据执行一维 C2C DFT（原地变换，Span 重载）。
    /// </summary>
    internal static void Dft1DComplexInPlace(Span<Complex> buffer, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(buffer);

        unsafe
        {
            fixed (Complex* p = buffer)
            {
                var ptr = (IntPtr)p;

                var plan = fftw.dft_1d(buffer.Length, ptr, ptr, direction, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create in-place complex plan.");
                try
                {
                    fftw.execute(plan);
                }
                finally
                {
                    fftw.destroy_plan(plan);
                }
            }
        }
    }

    /// <summary>
    /// 计算双精度复数输入的一维 C2C DFT（非原地，已固定重载）。
    /// </summary>
    internal static void Dft1DComplex(PinnableArray<Complex> input, PinnableArray<Complex> output, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);

        var plan = fftw.dft_1d(input.Length, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create complex plan.");

        try
        {
            fftw.execute(plan);
        }
        finally
        {
            fftw.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 执行双精度复数输入到双精度复数输出的一维 C2C DFT（非原地，Span 重载）。
    /// </summary>
    internal static void Dft1DComplex(ReadOnlySpan<Complex> input, Span<Complex> output, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (input.Length != output.Length)
        {
            throw new ArgumentException("Input and output spans must have the same length.");
        }
        unsafe
        {
            fixed (Complex* pInput = input)
            fixed (Complex* pOutput = output)
            {
                var plan = fftw.dft_1d(input.Length, (IntPtr)pInput, (IntPtr)pOutput, direction, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create complex plan.");
                try
                {
                    fftw.execute(plan);
                }
                finally
                {
                    fftw.destroy_plan(plan);
                }
            }
        }
    }

    // 1D real/complex

    /// <summary>
    /// 执行双精度实数输入到双精度复数输出的一维实数→复数 (R2C) DFT（已固定重载）。
    /// </summary>
    internal static void Dft1DR2C(PinnableArray<double> realInput, PinnableArray<Complex> complexOutput, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(realInput);
        InvalidOperationException.ThrowIfUnpinned(complexOutput);

        // 允许输出长度不小于 N/2+1（紧凑半谱）
        var required = (realInput.Length / 2) + 1;
        if (complexOutput.Length < required)
            throw new ArgumentException("For R2C: complexOutput.Length must be at least realInput.Length / 2 + 1 (compact half-spectrum).");

        var plan = fftw.dft_r2c_1d(realInput.Length, realInput, complexOutput, flags);

        InvalidOperationExceptionExtension.ThrowIfZero(plan, "Failed to create 1D real to complex plan.");

        try
        {
            fftw.execute(plan);
        }
        finally
        {
            fftw.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 执行双精度实数→复数的一维 R2C DFT（Span 重载）。
    /// </summary>
    internal static void Dft1DR2C(ReadOnlySpan<double> realInput, Span<Complex> complexOutput, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(realInput);
        ArgumentNullException.ThrowIfEmpty(complexOutput);

        // 允许输出长度不小于 N/2+1（紧凑半谱）
        var required = (realInput.Length / 2) + 1;
        if (complexOutput.Length < required)
            throw new ArgumentException("For R2C: complexOutput.Length must be at least realInput.Length / 2 + 1 (compact half-spectrum).");

        unsafe
        {
            fixed (double* pRealInput = realInput)
            fixed (Complex* pComplexOutput = complexOutput)
            {
                var plan = fftw.dft_r2c_1d(realInput.Length, (IntPtr)pRealInput, (IntPtr)pComplexOutput, flags);
                InvalidOperationExceptionExtension.ThrowIfZero(plan, "Failed to create 1D real to complex plan.");
                try
                {
                    fftw.execute(plan);
                }
                finally
                {
                    fftw.destroy_plan(plan);
                }
            }
        }
    }

    /// <summary>
    /// 执行双精度复数→实数的一维 C2R 逆变换（已固定重载），可选按 1/N 归一化。
    /// </summary>
    internal static void Dft1DC2R(PinnableArray<Complex> complexInput, PinnableArray<double> realOutput, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(complexInput);
        InvalidOperationException.ThrowIfUnpinned(realOutput);

        // 允许输入半谱长度不小于 N/2+1
        var required = (realOutput.Length / 2) + 1;
        if (complexInput.Length < required)
            throw new ArgumentException("For C2R: complexInput.Length must be at least realOutput.Length / 2 + 1 (compact half-spectrum).");

        var plan = fftw.dft_c2r_1d(realOutput.Length, complexInput, realOutput, flags);

        InvalidOperationException.ThrowIfZero(plan, "Failed to create 1D complex to real plan.");

        try
        {
            fftw.execute(plan);
        }
        finally
        {
            fftw.destroy_plan(plan);
        }

        if (scale)
        {
            // N 是实数输出长度（输入实数长度）
            var factor = 1.0 / realOutput.Length;

            // PinnableArray 版本保守使用标量循环
            ScaleInPlace(realOutput, factor);
        }
    }

    /// <summary>
    /// 执行双精度复数→实数的一维 C2R 逆变换（Span 重载），可选按 1/N 归一化。
    /// </summary>
    internal static void Dft1DC2R(ReadOnlySpan<Complex> complexInput, Span<double> realOutput, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(complexInput);
        ArgumentNullException.ThrowIfEmpty(realOutput);

        // 允许输入半谱长度不小于 N/2+1
        var required = (realOutput.Length / 2) + 1;
        if (complexInput.Length < required)
            throw new ArgumentException("For C2R: complexInput.Length must be at least realOutput.Length / 2 + 1 (compact half-spectrum).");

        unsafe
        {
            fixed (Complex* pComplexInput = complexInput)
            fixed (double* pRealOutput = realOutput)
            {
                var plan = fftw.dft_c2r_1d(realOutput.Length, (IntPtr)pComplexInput, (IntPtr)pRealOutput, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create 1D complex to real plan.");
                try
                {
                    fftw.execute(plan);
                }
                finally
                {
                    fftw.destroy_plan(plan);
                }
            }
        }
        if (scale)
        {
            // N 是实数输出长度（输入实数长度）
            var factor = 1.0 / realOutput.Length;

            // 使用 SIMD 加速的 Span<double> 缩放
            ScaleInPlace(realOutput, factor);
        }
    }

    // ==========================
    // 缩放辅助（提取 + SIMD 优化）
    // ==========================

    /// <summary>
    /// 对 <see cref="Span{Double}"/> 进行就地缩放；在可用时使用 <see cref="Vector{T}"/> SIMD。
    /// </summary>
    private static void ScaleInPlace(Span<double> data, double factor)
    {
        if (data.IsEmpty) return;

        if (Vector.IsHardwareAccelerated && data.Length >= Vector<double>.Count)
        {
            var vFactor = new Vector<double>(factor);
            var vecSpan = MemoryMarshal.Cast<double, Vector<double>>(data);

            for (int i = 0; i < vecSpan.Length; i++)
            {
                vecSpan[i] *= vFactor;
            }

            int processed = vecSpan.Length * Vector<double>.Count;
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