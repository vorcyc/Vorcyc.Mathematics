using System.Numerics;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW.FftwDouble;

/// <summary>
/// 基于 FFTW 双精度接口的一维离散傅里叶变换 (DFT) 辅助类。
/// 提供复数↔复数 (C2C)、实数→复数 (R2C)、复数→实数 (C2R) 的指针、Span 与已固定数组重载，并统一处理计划的创建与销毁。
/// </summary>
/// <remarks>
/// 使用说明：<br/>
/// 1) 使用 <see cref="PinnableArray{T}"/> 的重载要求缓冲区已固定 (Pinned)；未固定将抛出异常。<br/>
/// 2) 使用 <see cref="Span{T}"/> 的重载会在内部通过 <c>fixed</c> 暂时固定内存。<br/>
/// 3) 所有方法均非线程安全；不要在多个线程中对有内存重叠的缓冲区并发调用。<br/>
/// 4) FFTW 默认不执行归一化。逆变换通常需要按 1/N 缩放；C2R 重载提供可选缩放参数。<br/>
/// 5) R2C/C2R 的长度关系遵循 FFTW 紧凑半谱约定：R2C 输入 N，输出长度为 N/2+1；C2R 输入长度为 N/2+1，输出为 N。<br/>
/// </remarks>
public static class Dft1D
{
    // 1D complex-to-complex

    /// <summary>
    /// 对双精度复数缓冲区执行一维复数到复数 (C2C) DFT（原地变换）。
    /// </summary>
    /// <param name="buffer">已固定的复数缓冲区；结果将覆盖原数据。</param>
    /// <param name="direction">变换方向：<see cref="fftw_direction.Forward"/> 或 <see cref="fftw_direction.Backward"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 逆向变换后通常需按 1/N 归一化（N 为长度）。<br/>
    /// - 方法内部创建/执行/销毁计划。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当缓冲区未固定(Pinned)或计划创建失败时抛出。</exception>
    public static void Dft1DComplexInPlace(PinnableArray<Complex> buffer, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
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
    /// <param name="buffer">复数输入与输出共享的缓冲区（原地）。</param>
    /// <param name="direction">变换方向。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 输入数据在执行后被输出覆盖。<br/>
    /// - 逆向变换的归一化需在外部处理。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为空(Length==0)时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void Dft1DComplexInPlace(Span<Complex> buffer, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
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
    /// <param name="input">已固定的复数输入缓冲区。</param>
    /// <param name="output">已固定的复数输出缓冲区，长度须与输入相同。</param>
    /// <param name="direction">变换方向。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 不执行归一化；逆向变换后需外部按 1/N 缩放。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    public static void Dft1DComplex(PinnableArray<Complex> input, PinnableArray<Complex> output, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
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
    /// <param name="input">复数输入数据。</param>
    /// <param name="output">复数输出缓冲区，长度须与输入相同。</param>
    /// <param name="direction">变换方向。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 方法内部固定指针并调用 FFTW。<br/>
    /// - 逆向变换后通常需要外部归一化。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当输入或输出为空(Length==0)时抛出。</exception>
    /// <exception cref="ArgumentException">当两者长度不一致时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void Dft1DComplex(Span<Complex> input, Span<Complex> output, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
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
    /// <param name="realInput">已固定的实数输入缓冲区，长度为 N。</param>
    /// <param name="complexOutput">已固定的复数输出缓冲区，长度应为 N/2+1（紧凑半谱）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 输出采用半谱紧凑存储（含 DC 与 Nyquist）。<br/>
    /// - 不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    public static void Dft1DR2C(PinnableArray<float> realInput, PinnableArray<Complex> complexOutput, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(realInput);
        InvalidOperationException.ThrowIfUnpinned(complexOutput);

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
    /// <param name="realInput">实数输入数据，长度 N。</param>
    /// <param name="complexOutput">复数输出缓冲区，长度应为 N/2+1（即 <c>realInput.Length == complexOutput.Length * 2 - 2</c>）。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 输出为紧凑半谱格式。<br/>
    /// - 不包含归一化步骤。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当输入或输出为空(Length==0)时抛出。</exception>
    /// <exception cref="ArgumentException">当长度关系不满足要求时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void Dft1DR2C(Span<double> realInput, Span<Complex> complexOutput, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(realInput);
        ArgumentNullException.ThrowIfEmpty(complexOutput);
        if (realInput.Length != complexOutput.Length * 2 - 2)
        {
            throw new ArgumentException("Input length must be equal to output length * 2 - 2 for real-to-complex transform.");
        }
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
    /// <param name="complexInput">已固定的复数输入半谱缓冲区，长度为 N/2+1。</param>
    /// <param name="realOutput">已固定的实数输出缓冲区，长度为 N。</param>
    /// <param name="scale">是否按 1/N 归一化（N 为实数输出长度）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 输入应为对应 R2C 的半谱紧凑格式。<br/>
    /// - 启用缩放将对输出按 1/N 均匀缩放。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    public static void Dft1DC2R(PinnableArray<Complex> complexInput, PinnableArray<double> realOutput, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(complexInput);
        InvalidOperationException.ThrowIfUnpinned(realOutput);

        var plan = fftw.dft_c2r_1d(complexInput.Length, complexInput, realOutput, flags);

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
            unsafe
            {
                var count = complexInput.Length;
                var factor = 1.0 / count;
                for (var i = 0; i < count; i++)
                {
                    realOutput[i] *= factor;
                }
            }
        }
    }

    /// <summary>
    /// 执行双精度复数→实数的一维 C2R 逆变换（Span 重载），可选按 1/N 归一化。
    /// </summary>
    /// <param name="complexInput">复数输入半谱缓冲区，长度为 N/2+1。</param>
    /// <param name="realOutput">实数输出缓冲区，长度为 N（即 <c>realOutput.Length == complexInput.Length * 2 - 2</c>）。</param>
    /// <param name="scale">是否进行按 1/N 的归一化。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 输入必须是对应 R2C 的半谱格式。<br/>
    /// - 未缩放时输出为未归一化结果。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当输入或输出为空(Length==0)时抛出。</exception>
    /// <exception cref="ArgumentException">当长度关系不满足要求时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void Dft1DC2R(Span<Complex> complexInput, Span<double> realOutput, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(complexInput);
        ArgumentNullException.ThrowIfEmpty(realOutput);
        if (realOutput.Length != complexInput.Length * 2 - 2)
        {
            throw new ArgumentException("Output length must be equal to input length * 2 - 2 for complex-to-real transform.");
        }
        unsafe
        {
            fixed (Complex* pComplexInput = complexInput)
            fixed (double* pRealOutput = realOutput)
            {
                var plan = fftw.dft_c2r_1d(complexInput.Length, (IntPtr)pComplexInput, (IntPtr)pRealOutput, flags);
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
            unsafe
            {
                var count = complexInput.Length;
                var factor = 1.0 / count;
                for (var i = 0; i < count; i++)
                {
                    realOutput[i] *= factor;
                }
            }
        }
    }
}