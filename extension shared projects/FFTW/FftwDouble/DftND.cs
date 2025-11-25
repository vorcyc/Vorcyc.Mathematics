using System.Numerics;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW.FftwDouble;

/// <summary>
/// 基于 FFTW 双精度接口的 n 维离散傅里叶变换 (DFT) 辅助类。
/// 提供复数↔复数 (C2C)、实数→复数 (R2C)、复数→实数 (C2R) 的指针、Span 与已固定数组重载，并统一处理计划的创建、执行与销毁。
/// </summary>
/// <remarks>
/// - FFTW 默认不进行归一化；若需缩放请在外部处理，或在 C2R 重载中使用 <c>scale</c> 参数。<br/>
/// - 传入缓冲区应为连续内存且大小满足维度元素总数；已固定重载要求缓冲区处于 Pinned 状态。<br/>
/// - 多维接口要求 <c>dims</c> 长度等于维度数 rank；计划创建失败会抛出 <see cref="InvalidOperationException"/>。<br/>
/// - 原地与非原地调用的可用性取决于数据布局与 FFTW 限制。<br/>
/// </remarks>
public static class DftND
{

    // Generic n-D helpers

    #region NDComplex

    /// <summary>
    /// 执行 n 维复数到复数 (C2C) DFT（指针重载）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="Complex"/> 内存。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="Complex"/> 内存。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 不执行归一化；原地模式需确保布局与 FFTW 限制相容。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void DftNDComplex(IntPtr input, IntPtr output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);

        var rank = dims.Length;
        var n = dims.ToArray(); // FFTW needs int[]

        var plan = fftw.dft(rank, n, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
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
    /// 执行 n 维复数到复数 (C2C) DFT（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据缓冲区（线性展平的 n 维 <see cref="Complex"/> 数据）。</param>
    /// <param name="output">输出数据缓冲区（线性展平的 n 维 <see cref="Complex"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 内部通过 <c>fixed</c> 固定内存并创建临时计划；不执行归一化。<br/>
    /// - 建议确保输入与输出长度均不小于 ∏dims。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为空(Length==0)时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void DftNDComplex(Span<Complex> input, Span<Complex> output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);

        var rank = dims.Length;

        unsafe
        {
            fixed (Complex* inPtr = input)
            fixed (Complex* outPtr = output)
            {
                var plan = fftw.dft(rank, dims, (IntPtr)inPtr, (IntPtr)outPtr, direction, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
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
    /// 执行 n 维复数到复数 (C2C) DFT（已固定数组重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 n 维 <see cref="Complex"/> 数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 n 维 <see cref="Complex"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="direction">变换方向。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 直接使用已固定内存创建计划；不执行归一化。<br/>
    /// - 在允许的情形下可采用原地模式。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    public static void DftNDComplex(PinnableArray<Complex> input, PinnableArray<Complex> output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);

        var rank = dims.Length;
        var plan = fftw.dft(rank, dims, input, output, direction, flags);

        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
        try
        {
            fftw.execute(plan);
        }
        finally
        {
            fftw.destroy_plan(plan);
        }
    }

    #endregion

    #region R2C

    /// <summary>
    /// 执行 n 维实数到复数 (R2C) DFT（指针重载）。
    /// </summary>
    /// <param name="realInput">输入缓冲区指针，指向连续的 <see cref="double"/> 内存。</param>
    /// <param name="complexOutput">输出缓冲区指针，指向连续的 <see cref="Complex"/> 内存。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 不执行归一化；输出采用紧凑半谱存储约定。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="realInput"/> 或 <paramref name="complexOutput"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void DftNDR2C(IntPtr realInput, IntPtr complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(realInput);
        ArgumentNullException.ThrowIfZero(complexOutput);

        var rank = dims.Length;
        var plan = fftw.dft_r2c(rank, dims, realInput, complexOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");

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
    /// 执行 n 维实数到复数 (R2C) DFT（Span 重载）。
    /// </summary>
    /// <param name="realInput">输入数据缓冲区（线性展平的 n 维 <see cref="double"/> 数据）。</param>
    /// <param name="complexOutput">输出数据缓冲区（线性展平的 n 维 <see cref="Complex"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 内部通过 <c>fixed</c> 固定内存并创建临时计划；不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="realInput"/> 或 <paramref name="complexOutput"/> 为空(Length==0)时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void DftNDR2C(Span<double> realInput, Span<Complex> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(realInput);
        ArgumentNullException.ThrowIfEmpty(complexOutput);
        var rank = dims.Length;
        unsafe
        {
            fixed (double* inPtr = realInput)
            fixed (Complex* outPtr = complexOutput)
            {
                var plan = fftw.dft_r2c(rank, dims, (IntPtr)inPtr, (IntPtr)outPtr, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");
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
    /// 执行 n 维实数到复数 (R2C) DFT（已固定数组重载）。
    /// </summary>
    /// <param name="realInput">已固定的输入缓冲区（线性展平的 n 维 <see cref="double"/> 数据）。</param>
    /// <param name="complexOutput">已固定的输出缓冲区（线性展平的 n 维 <see cref="Complex"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 在允许的情形下可采用原地模式；需确保缓冲区已固定(Pinned)。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    public static void DftNDR2C(PinnableArray<double> realInput, PinnableArray<Complex> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(realInput);
        InvalidOperationException.ThrowIfUnpinned(complexOutput);
        var rank = dims.Length;
        var plan = fftw.dft_r2c(rank, dims, realInput, complexOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");
        try
        {
            fftw.execute(plan);
        }
        finally
        {
            fftw.destroy_plan(plan);
        }
    }

    #endregion

    #region NDC2R

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（指针重载），可选按 1/∏dims 归一化缩放。
    /// </summary>
    /// <param name="complexInput">输入缓冲区指针，指向连续的 <see cref="Complex"/> 内存（半谱约定）。</param>
    /// <param name="realOutput">输出缓冲区指针，指向连续的 <see cref="double"/> 内存。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="scale">是否按元素总数进行 1/∏dims 归一化缩放；默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - FFTW 默认不执行归一化；启用 <paramref name="scale"/> 时将对输出按总元素数进行均匀缩放。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="complexInput"/> 或 <paramref name="realOutput"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void DftNDC2R(IntPtr complexInput, IntPtr realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(complexInput);
        ArgumentNullException.ThrowIfZero(realOutput);

        var rank = dims.Length;
        var plan = fftw.dft_c2r(rank, dims, complexInput, realOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");

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
            var total = 1;
            for (var i = 0; i < dims.Length; i++) total *= dims[i];

            unsafe
            {
                var factor = 1.0 / total;
                var dest = (double*)realOutput;
                for (var i = 0; i < total; i++)
                {
                    dest[i] *= factor;
                }
            }
        }
    }

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（Span 重载），可选按 1/∏dims 归一化缩放。
    /// </summary>
    /// <param name="complexInput">输入数据缓冲区（线性展平的 n 维 <see cref="Complex"/> 数据，半谱约定）。</param>
    /// <param name="realOutput">输出数据缓冲区（线性展平的 n 维 <see cref="double"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="scale">是否按元素总数进行 1/∏dims 归一化缩放；默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 内部通过 <c>fixed</c> 固定内存并创建临时计划；启用缩放将对输出进行均匀缩放。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="complexInput"/> 或 <paramref name="realOutput"/> 为空(Length==0)时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void DftNDC2R(Span<Complex> complexInput, Span<double> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(complexInput);
        ArgumentNullException.ThrowIfEmpty(realOutput);
        var rank = dims.Length;
        unsafe
        {
            fixed (Complex* inPtr = complexInput)
            fixed (double* outPtr = realOutput)
            {
                var plan = fftw.dft_c2r(rank, dims, (IntPtr)inPtr, (IntPtr)outPtr, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");
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
            var total = 1;
            for (var i = 0; i < dims.Length; i++) total *= dims[i];
            unsafe
            {
                var factor = 1.0 / total;
                fixed (double* outPtr = realOutput)
                {
                    for (var i = 0; i < total; i++)
                    {
                        outPtr[i] *= factor;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（已固定数组重载），可选按 1/∏dims 归一化缩放。
    /// </summary>
    /// <param name="complexInput">已固定的输入缓冲区（线性展平的 n 维 <see cref="Complex"/> 数据，半谱约定）。</param>
    /// <param name="realOutput">已固定的输出缓冲区（线性展平的 n 维 <see cref="double"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="scale">是否按元素总数进行 1/∏dims 归一化缩放；默认 <c>true</c>。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 在允许的情形下可采用原地模式；需确保缓冲区已固定(Pinned)。<br/>
    /// - 启用缩放将对输出按总元素数进行均匀缩放。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    public static void DftNDC2R(PinnableArray<Complex> complexInput, PinnableArray<double> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(complexInput);
        InvalidOperationException.ThrowIfUnpinned(realOutput);
        var rank = dims.Length;
        var plan = fftw.dft_c2r(rank, dims, complexInput, realOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");
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
            var total = 1;
            for (var i = 0; i < dims.Length; i++) total *= dims[i];
            unsafe
            {
                var factor = 1.0 / total;
                for (var i = 0; i < total; i++)
                {
                    realOutput[i] *= factor;
                }
            }
        }
    }

    #endregion

}