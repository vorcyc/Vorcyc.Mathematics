using System.Numerics;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// 基于 FFTW 双精度接口的 n 维离散傅里叶变换 (DFT) 辅助类。
/// 提供简洁公共 API：Forward / Inverse（按参数类型区分 C2C、R2C、C2R）；
/// 原始实现已改为 internal 并统一处理计划的创建、执行与销毁。
/// </summary>
/// <remarks>
/// - FFTW 默认不进行归一化；若需缩放请在外部处理，或在 C2R 的 Inverse 重载中启用 <c>scale</c>。<br/>
/// - 传入缓冲区应为连续内存且大小满足维度元素总数；已固定重载要求缓冲区处于 Pinned 状态。<br/>
/// - 多维接口要求 <c>dims</c> 长度等于维度数 rank；计划创建失败会抛出 <see cref="InvalidOperationException"/>。<br/>
/// - 原地与非原地调用的可用性取决于数据布局与 FFTW 限制。<br/>
/// </remarks>
public static partial class DftND
{
    // ==========================
    // 精简友好命名的公共 API
    // ==========================

    // C2C Forward（复数→复数，n 维）
    /// <summary>
    /// 执行 n 维复数→复数正向傅里叶变换（C2C，Span 重载）。
    /// </summary>
    /// <param name="input">输入数据（线性展平的 n 维 <see cref="Complex"/>）。</param>
    /// <param name="output">输出数据（线性展平的 n 维 <see cref="Complex"/>）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(Span<Complex> input, Span<Complex> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行 n 维复数→复数正向傅里叶变换（C2C，PinnableArray 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 n 维 <see cref="Complex"/>）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 n 维 <see cref="Complex"/>）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<Complex> input, PinnableArray<Complex> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Forward, flags);



    // C2C Inverse（复数→复数，n 维）
    /// <summary>
    /// 执行 n 维复数→复数逆向傅里叶变换（C2C，Span 重载）。FFTW 的逆变换默认不执行归一化。
    /// </summary>
    public static void Inverse(Span<Complex> input, Span<Complex> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Backward, flags);

    /// <summary>
    /// 执行 n 维复数→复数逆向傅里叶变换（C2C，PinnableArray 重载）。FFTW 的逆变换默认不执行归一化。
    /// </summary>
    public static void Inverse(PinnableArray<Complex> input, PinnableArray<Complex> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Backward, flags);


    // R2C Forward（实数→复数，n 维，半谱）
    /// <summary>
    /// 执行 n 维实数→复数正向变换（R2C，Span 重载，输出为紧凑半谱）。
    /// </summary>
    public static void Forward(Span<double> realInput, Span<Complex> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDR2C(realInput, complexOutput, dims, flags);

    /// <summary>
    /// 执行 n 维实数→复数正向变换（R2C，PinnableArray 重载，输出为紧凑半谱）。
    /// </summary>
    public static void Forward(PinnableArray<double> realInput, PinnableArray<Complex> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDR2C(realInput, complexOutput, dims, flags);


    // C2R Inverse（复数→实数，n 维，支持归一化）
    /// <summary>
    /// 执行 n 维复数半谱→实数逆向变换（C2R，Span 重载），可选按 1/∏dims 自动归一化。
    /// </summary>
    public static void Inverse(Span<Complex> complexInput, Span<double> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
        => DftNDC2R(complexInput, realOutput, dims, scale, flags);

    /// <summary>
    /// 执行 n 维复数半谱→实数逆向变换（C2R，PinnableArray 重载），可选按 1/∏dims 自动归一化。
    /// </summary>
    public static void Inverse(PinnableArray<Complex> complexInput, PinnableArray<double> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
        => DftNDC2R(complexInput, realOutput, dims, scale, flags);


    // ==========================
    // 原始实现（内部管线，设为 internal）
    // ==========================

    #region NDComplex (internal)

    /// <summary>
    /// 执行 n 维复数到复数 (C2C) DFT（指针重载）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="Complex"/> 内存。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="Complex"/> 内存。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>不执行归一化；原地模式需确保布局与 FFTW 限制相容。</remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    internal static void DftNDComplex_Double(IntPtr input, IntPtr output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
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
    internal static void DftNDComplex(Span<Complex> input, Span<Complex> output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);

        unsafe
        {
            fixed (Complex* inPtr = input)
            fixed (Complex* outPtr = output)
            {
                var plan = fftw.dft(dims.Length, dims, (IntPtr)inPtr, (IntPtr)outPtr, direction, flags);
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
    internal static void DftNDComplex(PinnableArray<Complex> input, PinnableArray<Complex> output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);

        var plan = fftw.dft(dims.Length, dims, input, output, direction, flags);
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

    #region R2C (internal)

    /// <summary>
    /// 执行 n 维实数到复数 (R2C) DFT（指针重载，半谱输出）。
    /// </summary>
    internal static void DftNDR2C_Double(IntPtr realInput, IntPtr complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(realInput);
        ArgumentNullException.ThrowIfZero(complexOutput);

        var plan = fftw.dft_r2c(dims.Length, dims, realInput, complexOutput, flags);
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
    /// 执行 n 维实数到复数 (R2C) DFT（Span 重载，半谱输出）。
    /// </summary>
    internal static void DftNDR2C(Span<double> realInput, Span<Complex> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(realInput);
        ArgumentNullException.ThrowIfEmpty(complexOutput);

        unsafe
        {
            fixed (double* inPtr = realInput)
            fixed (Complex* outPtr = complexOutput)
            {
                var plan = fftw.dft_r2c(dims.Length, dims, (IntPtr)inPtr, (IntPtr)outPtr, flags);
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
    /// 执行 n 维实数到复数 (R2C) DFT（已固定数组重载，半谱输出）。
    /// </summary>
    internal static void DftNDR2C(PinnableArray<double> realInput, PinnableArray<Complex> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(realInput);
        InvalidOperationException.ThrowIfUnpinned(complexOutput);

        var plan = fftw.dft_r2c(dims.Length, dims, realInput, complexOutput, flags);
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

    #region NDC2R (internal)

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（指针重载），可选按 1/∏dims 归一化缩放。
    /// </summary>
    internal static void DftNDC2R(IntPtr complexInput, IntPtr realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(complexInput);
        ArgumentNullException.ThrowIfZero(realOutput);

        var plan = fftw.dft_c2r(dims.Length, dims, complexInput, realOutput, flags);
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
    internal static void DftNDC2R(Span<Complex> complexInput, Span<double> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(complexInput);
        ArgumentNullException.ThrowIfEmpty(realOutput);

        unsafe
        {
            fixed (Complex* inPtr = complexInput)
            fixed (double* outPtr = realOutput)
            {
                var plan = fftw.dft_c2r(dims.Length, dims, (IntPtr)inPtr, (IntPtr)outPtr, flags);
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
    internal static void DftNDC2R(PinnableArray<Complex> complexInput, PinnableArray<double> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(complexInput);
        InvalidOperationException.ThrowIfUnpinned(realOutput);

        var plan = fftw.dft_c2r(dims.Length, dims, complexInput, realOutput, flags);
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