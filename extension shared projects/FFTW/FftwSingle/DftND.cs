using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
/// - 传入缓冲区应为连续内存且大小至少为目标总长度（C2C/实数输出为 ∏dims；R2C/C2R 的复数半谱为紧凑长度：仅最后一维为 N/2+1）。<br/>
/// - 已固定重载要求缓冲区处于 Pinned 状态。<br/>
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
    /// <param name="input">输入数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。长度至少为 ∏dims。</param>
    /// <param name="output">输出数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行 n 维复数→复数正向傅里叶变换（C2C，PinnableArray 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。长度至少为 ∏dims。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据）。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Forward, flags);

    // C2C Inverse（复数→复数，n 维）

    /// <summary>
    /// 执行 n 维复数→复数逆向傅里叶变换（C2C，Span 重载）。
    /// 注意：FFTW 的逆变换默认不执行归一化；如需 1/∏dims 等缩放请在外部处理。
    /// </summary>
    /// <param name="input">输入数据。长度至少为 ∏dims。</param>
    /// <param name="output">输出数据。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Backward, flags);

    /// <summary>
    /// 执行 n 维复数→复数逆向傅里叶变换（C2C，PinnableArray 重载）。
    /// 注意：FFTW 的逆变换默认不执行归一化；如需 1/∏dims 等缩放请在外部处理。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区。长度至少为 ∏dims。</param>
    /// <param name="output">已固定的输出缓冲区。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Backward, flags);

    // R2C Forward（实数→复数，n 维）

    /// <summary>
    /// 执行 n 维实数→复数正向傅里叶变换（R2C，Span 重载，输出为紧凑半谱）。
    /// </summary>
    /// <param name="realInput">输入数据缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。长度至少为 ∏dims。</param>
    /// <param name="complexOutput">输出数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据，半谱约定）。长度至少为紧凑半谱长度（仅最后一维 N/2+1）。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(ReadOnlySpan<float> realInput, Span<ComplexFp32> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDR2C(realInput, complexOutput, dims, flags);

    /// <summary>
    /// 执行 n 维实数→复数正向变换（R2C，PinnableArray 重载，输出为紧凑半谱）。
    /// </summary>
    /// <param name="realInput">已固定的输入缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。长度至少为 ∏dims。</param>
    /// <param name="complexOutput">已固定的复数输出半谱缓冲区。长度至少为紧凑半谱长度（仅最后一维 N/2+1）。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<float> realInput, PinnableArray<ComplexFp32> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDR2C(realInput, complexOutput, dims, flags);

    // C2R Inverse（复数→实数，n 维）

    /// <summary>
    /// 执行 n 维复数半谱→实数逆向傅里叶变换（C2R，Span 重载），可选按 1/∏dims 自动归一化。
    /// </summary>
    /// <param name="complexInput">输入数据缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据，半谱约定）。长度至少为紧凑半谱长度（仅最后一维 N/2+1）。</param>
    /// <param name="realOutput">输出数据缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="scale">是否按元素总数进行 1/∏dims 归一化，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(ReadOnlySpan<ComplexFp32> complexInput, Span<float> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
        => DftNDC2R(complexInput, realOutput, dims, scale, flags);

    /// <summary>
    /// 执行 n 维复数半谱→实数逆向傅里叶变换（C2R，PinnableArray 重载），可选按 1/∏dims 自动归一化。
    /// </summary>
    /// <param name="complexInput">已固定的复数输入缓冲区（线性展平的 n 维 <see cref="ComplexFp32"/> 数据，半谱约定）。长度至少为紧凑半谱长度（仅最后一维 N/2+1）。</param>
    /// <param name="realOutput">已固定的实数输出缓冲区（线性展平的 n 维 <see cref="float"/> 数据）。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
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
    internal static void DftNDComplex_Single(IntPtr input, IntPtr output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);

        var rank = dims.Length;
        var n = dims.ToArray(); // FFTW 需要 int[]

        var plan = fftwf.dft(rank, n, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
        try { fftwf.execute(plan); }
        finally { fftwf.destroy_plan(plan); }
    }

    /// <summary>
    /// 执行 n 维复数到复数 (C2C) DFT（Span 重载）。
    /// </summary>
    internal static void DftNDComplex(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);

        // 宽松检查：输入/输出长度至少为 ∏dims
        var total = Product(dims);
        if (input.Length < total)
            throw new ArgumentException("Input length must be at least the product of dims.", nameof(input));
        if (output.Length < total)
            throw new ArgumentException("Output length must be at least the product of dims.", nameof(output));

        var rank = dims.Length;
        unsafe
        {
            fixed (ComplexFp32* inPtr = input)
            fixed (ComplexFp32* outPtr = output)
            {
                var plan = fftwf.dft(rank, dims, (IntPtr)inPtr, (IntPtr)outPtr, direction, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
                try { fftwf.execute(plan); }
                finally { fftwf.destroy_plan(plan); }
            }
        }
    }

    /// <summary>
    /// 执行 n 维复数到复数 (C2C) DFT（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    internal static void DftNDComplex(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);

        var total = Product(dims);
        if (input.Length < total)
            throw new ArgumentException("Input buffer length must be at least the product of dims.", nameof(input));
        if (output.Length < total)
            throw new ArgumentException("Output buffer length must be at least the product of dims.", nameof(output));

        var rank = dims.Length;
        var plan = fftwf.dft(rank, dims, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
        try { fftwf.execute(plan); }
        finally { fftwf.destroy_plan(plan); }
    }

    #endregion

    #region R2C (internal)

    /// <summary>
    /// 执行 n 维实数到复数 (R2C) DFT（指针重载）。
    /// </summary>
    internal static void DftNDR2C_Single(IntPtr realInput, IntPtr complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(realInput);
        ArgumentNullException.ThrowIfZero(complexOutput);

        var rank = dims.Length;
        var plan = fftwf.dft_r2c(rank, dims, realInput, complexOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");

        try { fftwf.execute(plan); }
        finally { fftwf.destroy_plan(plan); }
    }

    /// <summary>
    /// 执行 n 维实数到复数 (R2C) DFT（Span 重载）。
    /// </summary>
    internal static void DftNDR2C(ReadOnlySpan<float> realInput, Span<ComplexFp32> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(realInput);
        ArgumentNullException.ThrowIfEmpty(complexOutput);

        // 宽松检查：real >= ∏dims；complex >= 紧凑半谱长度
        var realTotal = Product(dims);
        var complexTotal = CompactSpectrumLength(dims);
        if (realInput.Length < realTotal)
            throw new ArgumentException("realInput length must be at least the product of dims.", nameof(realInput));
        if (complexOutput.Length < complexTotal)
            throw new ArgumentException("complexOutput length must be at least the compact half-spectrum length (last dim N/2+1).", nameof(complexOutput));

        var rank = dims.Length;
        unsafe
        {
            fixed (float* inPtr = realInput)
            fixed (ComplexFp32* outPtr = complexOutput)
            {
                var plan = fftwf.dft_r2c(rank, dims, (IntPtr)inPtr, (IntPtr)outPtr, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");
                try { fftwf.execute(plan); }
                finally { fftwf.destroy_plan(plan); }
            }
        }
    }

    /// <summary>
    /// 执行 n 维实数到复数 (R2C) DFT（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    internal static void DftNDR2C(PinnableArray<float> realInput, PinnableArray<ComplexFp32> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(realInput);
        InvalidOperationException.ThrowIfUnpinned(complexOutput);

        var realTotal = Product(dims);
        var complexTotal = CompactSpectrumLength(dims);
        if (realInput.Length < realTotal)
            throw new ArgumentException("realInput length must be at least the product of dims.", nameof(realInput));
        if (complexOutput.Length < complexTotal)
            throw new ArgumentException("complexOutput length must be at least the compact half-spectrum length (last dim N/2+1).", nameof(complexOutput));

        var rank = dims.Length;
        var plan = fftwf.dft_r2c(rank, dims, realInput, complexOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");
        try { fftwf.execute(plan); }
        finally { fftwf.destroy_plan(plan); }
    }

    #endregion

    #region NDC2R (internal)

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（指针重载），可选对结果进行 1/∏dims 缩放。
    /// </summary>
    internal static void DftNDC2R_Single(IntPtr complexInput, IntPtr realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(complexInput);
        ArgumentNullException.ThrowIfZero(realOutput);

        var rank = dims.Length;
        var plan = fftwf.dft_c2r(rank, dims, complexInput, realOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");

        try { fftwf.execute(plan); }
        finally { fftwf.destroy_plan(plan); }

        if (scale)
        {
            // 按总元素数进行 1/∏dims 归一化（复用 SIMD 缩放）
            var total = Product(dims);
            unsafe
            {
                var span = new Span<float>((void*)realOutput, total);
                ScaleInPlace(span, 1f / total);
            }
        }
    }

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（Span 重载），可选对结果进行 1/∏dims 缩放。
    /// </summary>
    internal static void DftNDC2R(ReadOnlySpan<ComplexFp32> complexInput, Span<float> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(complexInput);
        ArgumentNullException.ThrowIfEmpty(realOutput);

        // 宽松检查：complex >= 紧凑半谱长度；real >= ∏dims
        var realTotal = Product(dims);
        var complexTotal = CompactSpectrumLength(dims);
        if (complexInput.Length < complexTotal)
            throw new ArgumentException("complexInput length must be at least the compact half-spectrum length (last dim N/2+1).", nameof(complexInput));
        if (realOutput.Length < realTotal)
            throw new ArgumentException("realOutput length must be at least the product of dims.", nameof(realOutput));

        var rank = dims.Length;
        unsafe
        {
            fixed (ComplexFp32* inPtr = complexInput)
            fixed (float* outPtr = realOutput)
            {
                var plan = fftwf.dft_c2r(rank, dims, (IntPtr)inPtr, (IntPtr)outPtr, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");
                try { fftwf.execute(plan); }
                finally { fftwf.destroy_plan(plan); }
            }
        }
        if (scale)
        {
            // 按总元素数进行 1/∏dims 归一化（使用 SIMD 缩放）
            ScaleInPlace(realOutput, 1f / realTotal);
        }
    }

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（已固定的 <see cref="PinnableArray{T}"/> 重载），可选对结果进行 1/∏dims 缩放。
    /// </summary>
    internal static void DftNDC2R(PinnableArray<ComplexFp32> complexInput, PinnableArray<float> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(complexInput);
        InvalidOperationException.ThrowIfUnpinned(realOutput);

        var realTotal = Product(dims);
        var complexTotal = CompactSpectrumLength(dims);
        if (complexInput.Length < complexTotal)
            throw new ArgumentException("complexInput length must be at least the compact half-spectrum length (last dim N/2+1).", nameof(complexInput));
        if (realOutput.Length < realTotal)
            throw new ArgumentException("realOutput length must be at least the product of dims.", nameof(realOutput));

        var rank = dims.Length;
        var plan = fftwf.dft_c2r(rank, dims, complexInput, realOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");
        try { fftwf.execute(plan); }
        finally { fftwf.destroy_plan(plan); }

        if (scale)
        {
            // PinnableArray 为兼容性采用标量；若类型提供 AsSpan 可改为 SIMD 路径
            ScaleInPlace(realOutput, 1f / realTotal);
        }
    }

    #endregion

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