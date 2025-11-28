using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// 基于 FFTW 单精度接口的二维复数到复数 (C2C) 离散傅里叶变换包装。
/// </summary>
/// <remarks>
/// - 不执行归一化缩放；如需 1/(nx*ny) 等缩放请在外部完成。<br/>
/// - 提供指针、已固定数组、Span 三种重载，便于在不同内存情形下调用。<br/>
/// - 公共 API 采用简洁名称：<c>Forward</c> 与 <c>Inverse</c>；内部实现方法标记为 <c>internal</c>。<br/>
/// </remarks>
public static partial class Dft2D
{
    // ==========================
    // 精简友好命名的公共 API
    // ==========================

    // Forward: 复数→复数（非原地/原地取决于指针是否相同）

    /// <summary>
    /// 执行二维复数到复数的正向傅里叶变换（C2C，PinnableArray 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平二维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平二维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="nx">X 维度长度。</param>
    /// <param name="ny">Y 维度长度。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => Dft2DComplex(input, output, nx, ny, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行二维复数到复数的正向傅里叶变换（C2C，Span 重载）。
    /// </summary>
    /// <param name="input">输入数据（线性展平二维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="output">输出数据（线性展平二维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="nx">X 维度长度。</param>
    /// <param name="ny">Y 维度长度。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(Span<ComplexFp32> input, Span<ComplexFp32> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => Dft2DComplex(input, output, nx, ny, fftw_direction.Forward, flags);

    // Inverse: 复数→复数（非原地/原地取决于指针是否相同）

    /// <summary>
    /// 执行二维复数到复数的逆向傅里叶变换（C2C，PinnableArray 重载）。
    /// 注意：FFTW 的逆变换不执行归一化；如需 1/(nx*ny) 缩放请在外部处理。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区。</param>
    /// <param name="output">已固定的输出缓冲区。</param>
    /// <param name="nx">X 维度长度。</param>
    /// <param name="ny">Y 维度长度。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => Dft2DComplex(input, output, nx, ny, fftw_direction.Backward, flags);

    /// <summary>
    /// 执行二维复数到复数的逆向傅里叶变换（C2C，Span 重载）。
    /// 注意：FFTW 的逆变换不执行归一化；如需 1/(nx*ny) 缩放请在外部处理。
    /// </summary>
    /// <param name="input">输入数据。</param>
    /// <param name="output">输出数据。</param>
    /// <param name="nx">X 维度长度。</param>
    /// <param name="ny">Y 维度长度。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(Span<ComplexFp32> input, Span<ComplexFp32> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => Dft2DComplex(input, output, nx, ny, fftw_direction.Backward, flags);

    // ==========================
    // 原始实现（内部管线，设为 internal）
    // ==========================

    /// <summary>
    /// 执行二维复数到复数 (C2C) DFT（指针重载）。支持原地/非原地（取决于指针是否相同）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向包含二维 <see cref="ComplexFp32"/> 数据的连续内存，大小至少为 nx*ny。</param>
    /// <param name="output">输出缓冲区指针，指向与输入同尺寸的连续 <see cref="ComplexFp32"/> 内存。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>（前向或反向）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - FFTW 默认不进行归一化；结果幅值与方向有关。<br/>
    /// - 指针必须有效且指向至少 nx*ny 个 <see cref="ComplexFp32"/> 的连续内存。<br/>
    /// - 若进行原地变换，请确保对应数据布局与 FFTW 限制相容。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void Dft2DComplex_Single(IntPtr input, IntPtr output, int nx, int ny, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        var plan = fftwf.dft_2d(nx, ny, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create 2D complex plan.");
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
    /// 执行二维复数到复数 (C2C) DFT（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区，线性展平的二维 <see cref="ComplexFp32"/> 数据。</param>
    /// <param name="output">已固定的输出缓冲区，线性展平的二维 <see cref="ComplexFp32"/> 数据。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法会直接将已固定内存传入 FFTW 计划；不执行归一化。<br/>
    /// - 输入与输出应至少包含 nx*ny 个元素；在允许的情形下可采用原地模式。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    /// <exception cref="ArgumentException">当输入与输出长度不一致时抛出。</exception>
    internal static void Dft2DComplex(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, int nx, int ny, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        ArgumentException.ThrowIfArrayLengthNotEqual(input, output, "Input and output PinnableArrays must have the same length.");

        var plan = fftwf.dft_2d(nx, ny, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create 2D complex plan.");
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
    /// 执行二维复数到复数 (C2C) DFT（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据缓冲区（线性展平的二维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="output">输出数据缓冲区（线性展平的二维 <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 方法内部通过 <c>fixed</c> 固定内存并调用指针重载；不执行归一化。<br/>
    /// - 建议确保 <paramref name="input"/> 与 <paramref name="output"/> 的长度均不小于 nx*ny，且二者长度一致。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为空(Length==0)时抛出。</exception>
    /// <exception cref="ArgumentException">当输入与输出长度不一致时抛出。</exception>
    internal static void Dft2DComplex(Span<ComplexFp32> input, Span<ComplexFp32> output, int nx, int ny, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (input.Length != output.Length) throw new ArgumentException("Input and output spans must have the same length.", nameof(output));
        unsafe
        {
            fixed (ComplexFp32* inputPtr = input)
            fixed (ComplexFp32* outputPtr = output)
            {
                Dft2DComplex_Single((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, direction, flags);
            }
        }
    }
}