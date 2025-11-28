using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// 基于 FFTW 单精度接口的 n 维离散傅里叶变换帮助类。
/// 提供复数-复数 (C2C)、实数-复数 (R2C) 与复数-实数 (C2R) 的简洁公共 API：Forward / Inverse；原始实现改为 internal 并统一负责计划创建/执行/销毁。
/// </summary>
/// <remarks>
/// 使用说明：
/// - 默认不进行归一化；如需缩放请在外部处理，或在 C2R 的 Inverse 重载中启用 <c>scale</c>。<br/>
/// - 传入缓冲区应为连续内存并满足维度元素总数；已固定重载要求缓冲区处于 Pinned 状态。<br/>
/// - 多维接口要求 <c>dims</c> 长度等于维度数 rank；计划创建失败会抛出 <see cref="InvalidOperationException"/>。<br/>
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
    /// <param name="input">输入数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="output">输出数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(Span<ComplexFp32> input, Span<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行 n 维复数→复数正向傅里叶变换（C2C，PinnableArray 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Forward, flags);

    // C2C Inverse（复数→复数，n 维）

    /// <summary>
    /// 执行 n 维复数→复数逆向傅里叶变换（C2C，Span 重载）。
    /// 注意：FFTW 的逆变换默认不执行归一化；如需 1/∏dims 等缩放请在外部处理。
    /// </summary>
    /// <param name="input">输入数据。</param>
    /// <param name="output">输出数据。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(Span<ComplexFp32> input, Span<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Backward, flags);

    /// <summary>
    /// 执行 n 维复数→复数逆向傅里叶变换（C2C，PinnableArray 重载）。
    /// 注意：FFTW 的逆变换默认不执行归一化；如需 1/∏dims 等缩放请在外部处理。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区。</param>
    /// <param name="output">已固定的输出缓冲区。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Backward, flags);

    // R2C Forward（实数→复数，n 维）


    /// <summary>
    /// 执行 n 维实数→复数正向傅里叶变换（R2C，Span 重载，输出为紧凑半谱）。
    /// </summary>
    /// <param name="realInput">输入数据缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。</param>
    /// <param name="complexOutput">输出数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据，半谱约定）。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(Span<float> realInput, Span<ComplexFp32> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDR2C(realInput, complexOutput, dims, flags);

    /// <summary>
    /// 执行 n 维实数→复数正向傅里叶变换（R2C，PinnableArray 重载，输出为紧凑半谱）。
    /// </summary>
    /// <param name="realInput">已固定的输入缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。</param>
    /// <param name="complexOutput">已固定的输出缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据，半谱约定）。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<float> realInput, PinnableArray<ComplexFp32> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDR2C(realInput, complexOutput, dims, flags);

    // C2R Inverse（复数→实数，n 维）

    /// <summary>
    /// 执行 n 维复数半谱→实数逆向傅里叶变换（C2R，Span 重载），可选按 1/∏dims 自动归一化。
    /// </summary>
    /// <param name="complexInput">输入数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据，半谱约定）。</param>
    /// <param name="realOutput">输出数据缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="scale">是否按元素总数进行 1/∏dims 归一化，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(Span<ComplexFp32> complexInput, Span<float> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
        => DftNDC2R(complexInput, realOutput, dims, scale, flags);

    /// <summary>
    /// 执行 n 维复数半谱→实数逆向傅里叶变换（C2R，PinnableArray 重载），可选按 1/∏dims 自动归一化。
    /// </summary>
    /// <param name="complexInput">已固定的复数输入缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据，半谱约定）。</param>
    /// <param name="realOutput">已固定的实数输出缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="scale">是否按元素总数进行 1/∏dims 归一化，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(PinnableArray<ComplexFp32> complexInput, PinnableArray<float> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
        => DftNDC2R(complexInput, realOutput, dims, scale, flags);

    // ==========================
    // 原始实现（内部管线，设为 internal）
    // ==========================

    #region NDComplex (internal)

    /// <summary>
    /// 执行 n 维复数到复数 (C2C) DFT（指针重载）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="ComplexFp32"/> 内存。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="ComplexFp32"/> 内存。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - FFTW 默认不执行归一化。<br/>
    /// - 若进行原地变换，请确保数据布局与 FFTW 限制相容。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void DftNDComplex_Single(IntPtr input, IntPtr output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);

        var rank = dims.Length;
        var n = dims.ToArray(); // FFTW 需要 int[]

        var plan = fftwf.dft(rank, n, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
        try
        {
            fftwf.execute(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 执行 n 维复数到复数 (C2C) DFT（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="output">输出数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 方法内部通过 <c>fixed</c> 固定内存并创建临时计划；不执行归一化。<br/>
    /// - 建议确保输入与输出长度均不小于 ∏dims。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为空(Length==0)时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void DftNDComplex(Span<ComplexFp32> input, Span<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);

        var rank = dims.Length;

        unsafe
        {
            fixed (ComplexFp32* inPtr = input)
            fixed (ComplexFp32* outPtr = output)
            {
                var plan = fftwf.dft(rank, dims, (IntPtr)inPtr, (IntPtr)outPtr, direction, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
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
    /// 执行 n 维复数到复数 (C2C) DFT（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法直接将已固定内存传入 FFTW 计划；不执行归一化。<br/>
    /// - 在允许的情形下可采用原地模式；请确保缓冲区已固定(Pinned)。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    internal static void DftNDComplex(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);

        var rank = dims.Length;
        var plan = fftwf.dft(rank, dims, input, output, direction, flags);

        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
        try
        {
            fftwf.execute(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    #endregion

    #region R2C (internal)

    /// <summary>
    /// 执行 n 维实数到复数 (R2C) DFT（指针重载）。
    /// </summary>
    /// <param name="realInput">输入缓冲区指针，指向连续的 <see cref="float"/> 内存。</param>
    /// <param name="complexOutput">输出缓冲区指针，指向连续的 <see cref="ComplexFp32"/> 内存。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - FFTW 默认不执行归一化；输出采用半谱存储约定。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="realInput"/> 或 <paramref name="complexOutput"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void DftNDR2C_Single(IntPtr realInput, IntPtr complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(realInput);
        ArgumentNullException.ThrowIfZero(complexOutput);

        var rank = dims.Length;
        var plan = fftwf.dft_r2c(rank, dims, realInput, complexOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");

        try
        {
            fftwf.execute(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 执行 n 维实数到复数 (R2C) DFT（Span 重载）。
    /// </summary>
    /// <param name="realInput">输入数据缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。</param>
    /// <param name="complexOutput">输出数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 方法内部通过 <c>fixed</c> 固定内存并创建临时计划；不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="realInput"/> 或 <paramref name="complexOutput"/> 为空(Length==0)时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void DftNDR2C(Span<float> realInput, Span<ComplexFp32> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(realInput);
        ArgumentNullException.ThrowIfEmpty(complexOutput);
        var rank = dims.Length;
        unsafe
        {
            fixed (float* inPtr = realInput)
            fixed (ComplexFp32* outPtr = complexOutput)
            {
                var plan = fftwf.dft_r2c(rank, dims, (IntPtr)inPtr, (IntPtr)outPtr, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");
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
    /// 执行 n 维实数到复数 (R2C) DFT（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    /// <param name="realInput">已固定的输入缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。</param>
    /// <param name="complexOutput">已固定的输出缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 在允许的情形下可采用原地模式；请确保缓冲区已固定(Pinned)。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    internal static void DftNDR2C(PinnableArray<float> realInput, PinnableArray<ComplexFp32> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(realInput);
        InvalidOperationException.ThrowIfUnpinned(complexOutput);
        var rank = dims.Length;
        var plan = fftwf.dft_r2c(rank, dims, realInput, complexOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");
        try
        {
            fftwf.execute(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    #endregion

    #region NDC2R (internal)

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（指针重载），可选对结果进行 1/∏dims 缩放。
    /// </summary>
    /// <param name="complexInput">输入缓冲区指针，指向连续的 <see cref="ComplexFp32"/> 内存（半谱约定）。</param>
    /// <param name="realOutput">输出缓冲区指针，指向连续的 <see cref="float"/> 内存。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="scale">是否按元素总数进行 1/∏dims 归一化缩放，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - FFTW 默认不执行归一化；启用 <paramref name="scale"/> 时将对输出按总元素数进行均匀缩放。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="complexInput"/> 或 <paramref name="realOutput"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void DftNDC2R_Single(IntPtr complexInput, IntPtr realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(complexInput);
        ArgumentNullException.ThrowIfZero(realOutput);

        var rank = dims.Length;
        var plan = fftwf.dft_c2r(rank, dims, complexInput, realOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");

        try
        {
            fftwf.execute(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }

        if (scale)
        {
            // 按总元素数进行 1/∏dims 归一化
            var total = 1;
            for (var i = 0; i < dims.Length; i++) total *= dims[i];

            unsafe
            {
                var factor = 1f / total;
                var dest = (float*)realOutput;
                for (var i = 0; i < total; i++)
                {
                    dest[i] *= factor;
                }
            }
        }
    }

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（Span 重载），可选对结果进行 1/∏dims 缩放。
    /// </summary>
    /// <param name="complexInput">输入数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据，半谱约定）。</param>
    /// <param name="realOutput">输出数据缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="scale">是否按元素总数进行 1/∏dims 归一化缩放，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 方法内部通过 <c>fixed</c> 固定内存并创建临时计划。<br/>
    /// - 启用 <paramref name="scale"/> 时按总元素数对输出进行均匀缩放。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="complexInput"/> 或 <paramref name="realOutput"/> 为空(Length==0)时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void DftNDC2R(Span<ComplexFp32> complexInput, Span<float> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(complexInput);
        ArgumentNullException.ThrowIfEmpty(realOutput);
        var rank = dims.Length;
        unsafe
        {
            fixed (ComplexFp32* inPtr = complexInput)
            fixed (float* outPtr = realOutput)
            {
                var plan = fftwf.dft_c2r(rank, dims, (IntPtr)inPtr, (IntPtr)outPtr, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");
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
        if (scale)
        {
            // 按总元素数进行 1/∏dims 归一化
            var total = 1;
            for (var i = 0; i < dims.Length; i++) total *= dims[i];
            unsafe
            {
                var factor = 1f / total;
                fixed (float* outPtr = realOutput)
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
    /// 执行 n 维复数到实数 (C2R) 逆变换（已固定的 <see cref="PinnableArray{T}"/> 重载），可选对结果进行 1/∏dims 缩放。
    /// </summary>
    /// <param name="complexInput">已固定的输入缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据，半谱约定）。</param>
    /// <param name="realOutput">已固定的输出缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="scale">是否按元素总数进行 1/∏dims 归一化缩放，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 在允许的情形下可采用原地模式；请确保缓冲区已固定(Pinned)。<br/>
    /// - 启用 <paramref name="scale"/> 时按总元素数对输出进行均匀缩放。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    internal static void DftNDC2R(PinnableArray<ComplexFp32> complexInput, PinnableArray<float> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(complexInput);
        InvalidOperationException.ThrowIfUnpinned(realOutput);
        var rank = dims.Length;
        var plan = fftwf.dft_c2r(rank, dims, complexInput, realOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");
        try
        {
            fftwf.execute(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
        if (scale)
        {
            // 按总元素数进行 1/∏dims 归一化
            var total = 1;
            for (var i = 0; i < dims.Length; i++) total *= dims[i];
            unsafe
            {
                var factor = 1f / total;
                for (var i = 0; i < total; i++)
                {
                    realOutput[i] *= factor;
                }
            }
        }
    }

    #endregion
}