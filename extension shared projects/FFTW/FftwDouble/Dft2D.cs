using System.Numerics;
using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// 基于 FFTW 双精度接口的二维复数到复数 (C2C) 离散傅里叶变换辅助类。
/// 提供简洁公共 API：Forward / Inverse；原始实现改为 internal 并统一封装计划的创建、执行与销毁流程。
/// </summary>
/// <remarks>
/// - FFTW 默认不执行归一化；若需 1/(nx*ny) 等缩放，请在外部完成。<br/>
/// - 可原地或非原地调用，是否允许原地取决于数据布局与 FFTW 限制。<br/>
/// - 输入/输出缓冲区需为连续内存，长度至少为 nx*ny（展平二维）。<br/>
/// </remarks>
public static partial class Dft2D
{
    // ==========================
    // 精简友好命名的公共 API
    // ==========================

    // Forward（C2C）
    /// <summary>
    /// 执行二维复数到复数的正向傅里叶变换（C2C，PinnableArray 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的二维 <see cref="Complex"/> 数据）。长度至少为 nx*ny。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的二维 <see cref="Complex"/> 数据）。长度至少为 nx*ny。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<Complex> input, PinnableArray<Complex> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => Dft2DComplex(input, output, nx, ny, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行二维复数到复数的正向傅里叶变换（C2C，Span 重载）。
    /// </summary>
    /// <param name="input">输入数据缓冲区（线性展平的二维 <see cref="Complex"/> 数据）。长度至少为 nx*ny。</param>
    /// <param name="output">输出数据缓冲区（线性展平的二维 <see cref="Complex"/> 数据）。长度至少为 nx*ny。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(ReadOnlySpan<Complex> input, Span<Complex> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => Dft2DComplex(input, output, nx, ny, fftw_direction.Forward, flags);

    // Inverse（C2C）
    /// <summary>
    /// 执行二维复数到复数的逆向傅里叶变换（C2C，PinnableArray 重载）。注意：FFTW 的逆变换不执行归一化。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区。长度至少为 nx*ny。</param>
    /// <param name="output">已固定的输出缓冲区。长度至少为 nx*ny。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(PinnableArray<Complex> input, PinnableArray<Complex> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => Dft2DComplex(input, output, nx, ny, fftw_direction.Backward, flags);

    /// <summary>
    /// 执行二维复数到复数的逆向傅里叶变换（C2C，Span 重载）。注意：FFTW 的逆变换不执行归一化。
    /// </summary>
    /// <param name="input">输入数据缓冲区。长度至少为 nx*ny。</param>
    /// <param name="output">输出数据缓冲区。长度至少为 nx*ny。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(ReadOnlySpan<Complex> input, Span<Complex> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => Dft2DComplex(input, output, nx, ny, fftw_direction.Backward, flags);

    // ==========================
    // 原始实现（内部管线，设为 internal）
    // ==========================

    /// <summary>
    /// 执行二维复数到复数 (C2C) DFT（指针重载，支持原地/非原地）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向至少 nx*ny 个连续的 <see cref="Complex"/> 元素。</param>
    /// <param name="output">输出缓冲区指针，指向至少 nx*ny 个连续的 <see cref="Complex"/> 元素。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>（前向或反向）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 不执行归一化；结果幅值与方向有关。<br/>
    /// - 原地模式需确保数据布局与 FFTW 限制相容。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void Dft2DComplex_Double(IntPtr input, IntPtr output, int nx, int ny, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        var plan = fftw.dft_2d(nx, ny, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create 2D complex plan.");
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
    /// 执行二维复数到复数 (C2C) DFT（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的二维 <see cref="Complex"/> 数据）。长度至少为 nx*ny。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的二维 <see cref="Complex"/> 数据）。长度至少为 nx*ny。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法直接使用已固定内存创建计划；不执行归一化。<br/>
    /// - 在允许的情形下可采用原地模式（输入与输出相同缓冲）。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    internal static void Dft2DComplex(PinnableArray<Complex> input, PinnableArray<Complex> output, int nx, int ny, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);

        var total = nx * ny;
        if (input.Length < total)
            throw new ArgumentException("Input buffer length must be at least nx * ny.", nameof(input));
        if (output.Length < total)
            throw new ArgumentException("Output buffer length must be at least nx * ny.", nameof(output));

        var plan = fftw.dft_2d(nx, ny, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create 2D complex plan.");
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
    /// 执行二维复数到复数 (C2C) DFT（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据缓冲区（线性展平的二维 <see cref="Complex"/> 数据）。长度至少为 nx*ny。</param>
    /// <param name="output">输出数据缓冲区（线性展平的二维 <see cref="Complex"/> 数据）。长度至少为 nx*ny。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 方法内部通过 <c>fixed</c> 固定内存并调用指针重载。<br/>
    /// - 输入/输出长度只需不小于 nx*ny；不进行归一化。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为空(Length==0)时抛出。</exception>
    /// <exception cref="ArgumentException">当输入或输出长度小于 nx*ny 时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void Dft2DComplex(ReadOnlySpan<Complex> input, Span<Complex> output, int nx, int ny, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);

        var total = nx * ny;
        if (input.Length < total)
            throw new ArgumentException("Input span length must be at least nx * ny.", nameof(input));
        if (output.Length < total)
            throw new ArgumentException("Output span length must be at least nx * ny.", nameof(output));

        unsafe
        {
            fixed (Complex* inputPtr = input)
            fixed (Complex* outputPtr = output)
            {
                Dft2DComplex_Double((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, direction, flags);
            }
        }
    }
}