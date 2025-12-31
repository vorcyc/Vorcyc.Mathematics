using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
/// - 传入缓冲区应为连续内存且大小不小于维度元素总数；已固定重载要求缓冲区处于 Pinned 状态。<br/>
/// - 多维接口要求 <c>dims</c> 长度等于维度数 rank；计划创建失败会抛出 <see cref="InvalidOperationException"/>。<br/>
/// - 原地与非原地调用的可用性取决于数据布局与 FFTW 限制。<br/>
/// </remarks>
public static partial class DftND
{
    // ==========================
    // 维度工具（内部）
    // ==========================

    /// <summary>
    /// 计算实数布局的元素总数 ∏dims。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Product(ReadOnlySpan<int> dims)
    {
        var total = 1;
        for (int i = 0; i < dims.Length; i++) total *= dims[i];
        return total;
    }

    /// <summary>
    /// 计算 R2C/C2R 紧凑半谱的复数元素总数：仅最后一维压缩为 Nn/2+1，其余维不变。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CompactSpectrumLength(ReadOnlySpan<int> dims)
    {
        if (dims.Length == 0) return 0;
        var total = 1;
        for (int i = 0; i < dims.Length - 1; i++) total *= dims[i];
        var last = dims[dims.Length - 1];
        total *= (last / 2) + 1;
        return total;
    }

    // ==========================
    // 精简友好命名的公共 API
    // ==========================

    // C2C Forward（复数→复数，n 维）
    /// <summary>
    /// 执行 n 维复数→复数正向傅里叶变换（C2C，Span 重载）。
    /// </summary>
    /// <param name="input">输入数据（线性展平的 n 维 <see cref="Complex"/>）。长度至少为 ∏dims。</param>
    /// <param name="output">输出数据（线性展平的 n 维 <see cref="Complex"/>）。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(ReadOnlySpan<Complex> input, Span<Complex> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行 n 维复数→复数正向傅里叶变换（C2C，PinnableArray 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 n 维 <see cref="Complex"/>）。长度至少为 ∏dims。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 n 维 <see cref="Complex"/>）。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<Complex> input, PinnableArray<Complex> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Forward, flags);

    // C2C Inverse（复数→复数，n 维）
    /// <summary>
    /// 执行 n 维复数→复数逆向傅里叶变换（C2C，Span 重载）。FFTW 的逆变换默认不执行归一化。
    /// </summary>
    /// <param name="input">输入数据。长度至少为 ∏dims。</param>
    /// <param name="output">输出数据。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(ReadOnlySpan<Complex> input, Span<Complex> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Backward, flags);

    /// <summary>
    /// 执行 n 维复数→复数逆向傅里叶变换（C2C，PinnableArray 重载）。FFTW 的逆变换默认不执行归一化。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区。长度至少为 ∏dims。</param>
    /// <param name="output">已固定的输出缓冲区。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(PinnableArray<Complex> input, PinnableArray<Complex> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDComplex(input, output, dims, fftw_direction.Backward, flags);

    // R2C Forward（实数→复数，n 维，半谱）
    /// <summary>
    /// 执行 n 维实数→复数正向变换（R2C，Span 重载，输出为紧凑半谱）。
    /// </summary>
    /// <param name="realInput">实数输入数据。长度至少为 ∏dims。</param>
    /// <param name="complexOutput">复数输出半谱数据。长度至少为紧凑半谱长度（仅最后一维 N/2+1）。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(ReadOnlySpan<double> realInput, Span<Complex> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDR2C(realInput, complexOutput, dims, flags);

    /// <summary>
    /// 执行 n 维实数→复数正向变换（R2C，PinnableArray 重载，输出为紧凑半谱）。
    /// </summary>
    /// <param name="realInput">已固定的实数输入数据。长度至少为 ∏dims。</param>
    /// <param name="complexOutput">已固定的复数输出半谱数据。长度至少为紧凑半谱长度（仅最后一维 N/2+1）。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<double> realInput, PinnableArray<Complex> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => DftNDR2C(realInput, complexOutput, dims, flags);

    // C2R Inverse（复数→实数，n 维，支持归一化）
    /// <summary>
    /// 执行 n 维复数半谱→实数逆向变换（C2R，Span 重载），可选按 1/∏dims 自动归一化。
    /// </summary>
    /// <param name="complexInput">复数半谱输入数据。长度至少为紧凑半谱长度（仅最后一维 N/2+1）。</param>
    /// <param name="realOutput">实数输出数据。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="scale">是否按 1/∏dims 进行归一化，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(ReadOnlySpan<Complex> complexInput, Span<double> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
        => DftNDC2R(complexInput, realOutput, dims, scale, flags);

    /// <summary>
    /// 执行 n 维复数半谱→实数逆向变换（C2R，PinnableArray 重载），可选按 1/∏dims 自动归一化。
    /// </summary>
    /// <param name="complexInput">已固定的复数半谱输入数据。长度至少为紧凑半谱长度（仅最后一维 N/2+1）。</param>
    /// <param name="realOutput">已固定的实数输出数据。长度至少为 ∏dims。</param>
    /// <param name="dims">每一维的尺寸数组。</param>
    /// <param name="scale">是否按 1/∏dims 进行归一化，默认 <c>true</c>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(PinnableArray<Complex> complexInput, PinnableArray<double> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
        => DftNDC2R(complexInput, realOutput, dims, scale, flags);

    // ==========================
    // 原始实现（内部管线，设为 internal）
    // ==========================

    #region NDComplex (internal)

    /// <summary>
    /// 执行 n 维复数到复数 (C2C) DFT（指针重载）。
    /// </summary>
    internal static void DftNDComplex_Double(IntPtr input, IntPtr output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);

        var plan = fftw.dft(dims.Length, dims, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
        try { fftw.execute(plan); }
        finally { fftw.destroy_plan(plan); }
    }

    /// <summary>
    /// 执行 n 维复数到复数 (C2C) DFT（Span 重载）。
    /// </summary>
    internal static void DftNDComplex(ReadOnlySpan<Complex> input, Span<Complex> output, ReadOnlySpan<int> dims, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);

        // 宽松检查：输入/输出长度至少为 ∏dims
        var total = Product(dims);
        if (input.Length < total)
            throw new ArgumentException("Input length must be at least the product of dims.", nameof(input));
        if (output.Length < total)
            throw new ArgumentException("Output length must be at least the product of dims.", nameof(output));

        unsafe
        {
            fixed (Complex* inPtr = input)
            fixed (Complex* outPtr = output)
            {
                var plan = fftw.dft(dims.Length, dims, (IntPtr)inPtr, (IntPtr)outPtr, direction, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
                try { fftw.execute(plan); }
                finally { fftw.destroy_plan(plan); }
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

        // 宽松检查：输入/输出长度至少为 ∏dims
        var total = Product(dims);
        if (input.Length < total)
            throw new ArgumentException("Input buffer length must be at least the product of dims.", nameof(input));
        if (output.Length < total)
            throw new ArgumentException("Output buffer length must be at least the product of dims.", nameof(output));

        var plan = fftw.dft(dims.Length, dims, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D complex plan.");
        try { fftw.execute(plan); }
        finally { fftw.destroy_plan(plan); }
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

        try { fftw.execute(plan); }
        finally { fftw.destroy_plan(plan); }
    }

    /// <summary>
    /// 执行 n 维实数到复数 (R2C) DFT（Span 重载，半谱输出）。
    /// </summary>
    internal static void DftNDR2C(ReadOnlySpan<double> realInput, Span<Complex> complexOutput, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
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

        unsafe
        {
            fixed (double* inPtr = realInput)
            fixed (Complex* outPtr = complexOutput)
            {
                var plan = fftw.dft_r2c(dims.Length, dims, (IntPtr)inPtr, (IntPtr)outPtr, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");
                try { fftw.execute(plan); }
                finally { fftw.destroy_plan(plan); }
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

        var realTotal = Product(dims);
        var complexTotal = CompactSpectrumLength(dims);
        if (realInput.Length < realTotal)
            throw new ArgumentException("realInput length must be at least the product of dims.", nameof(realInput));
        if (complexOutput.Length < complexTotal)
            throw new ArgumentException("complexOutput length must be at least the compact half-spectrum length (last dim N/2+1).", nameof(complexOutput));

        var plan = fftw.dft_r2c(dims.Length, dims, realInput, complexOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D r2c plan.");
        try { fftw.execute(plan); }
        finally { fftw.destroy_plan(plan); }
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

        try { fftw.execute(plan); }
        finally { fftw.destroy_plan(plan); }

        if (scale)
        {
            var total = Product(dims);
            unsafe
            {
                var dest = new Span<double>((void*)realOutput, total);
                ScaleInPlace(dest, 1.0 / total);
            }
        }
    }

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（Span 重载），可选按 1/∏dims 归一化缩放。
    /// </summary>
    internal static void DftNDC2R(ReadOnlySpan<Complex> complexInput, Span<double> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(complexInput);
        ArgumentNullException.ThrowIfEmpty(realOutput);

        var realTotal = Product(dims);
        var complexTotal = CompactSpectrumLength(dims);
        if (complexInput.Length < complexTotal)
            throw new ArgumentException("complexInput length must be at least the compact half-spectrum length (last dim N/2+1).", nameof(complexInput));
        if (realOutput.Length < realTotal)
            throw new ArgumentException("realOutput length must be at least the product of dims.", nameof(realOutput));

        unsafe
        {
            fixed (Complex* inPtr = complexInput)
            fixed (double* outPtr = realOutput)
            {
                var plan = fftw.dft_c2r(dims.Length, dims, (IntPtr)inPtr, (IntPtr)outPtr, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");
                try { fftw.execute(plan); }
                finally { fftw.destroy_plan(plan); }
            }
        }
        if (scale)
        {
            ScaleInPlace(realOutput, 1.0 / realTotal);
        }
    }

    /// <summary>
    /// 执行 n 维复数到实数 (C2R) 逆变换（已固定数组重载），可选按 1/∏dims 归一化缩放。
    /// </summary>
    internal static void DftNDC2R(PinnableArray<Complex> complexInput, PinnableArray<double> realOutput, ReadOnlySpan<int> dims, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(complexInput);
        InvalidOperationException.ThrowIfUnpinned(realOutput);

        var realTotal = Product(dims);
        var complexTotal = CompactSpectrumLength(dims);
        if (complexInput.Length < complexTotal)
            throw new ArgumentException("complexInput length must be at least the compact half-spectrum length (last dim N/2+1).", nameof(complexInput));
        if (realOutput.Length < realTotal)
            throw new ArgumentException("realOutput length must be at least the product of dims.", nameof(realOutput));

        var plan = fftw.dft_c2r(dims.Length, dims, complexInput, realOutput, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create n-D c2r plan.");
        try { fftw.execute(plan); }
        finally { fftw.destroy_plan(plan); }

        if (scale)
        {
            // 为兼容性，PinnableArray 版本保守使用标量循环；若类型提供 AsSpan() 可切换到 SIMD 路径
            ScaleInPlace(realOutput, 1.0 / realTotal);
        }
    }

    #endregion

    // ==========================
    // 缩放辅助（提取 + SIMD 优化）
    // ==========================

    /// <summary>
    /// 对 <see cref="Span{Double}"/> 进行就地缩放；在可用时使用 <see cref="Vector{T}"/> SIMD。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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