using System.Numerics;
using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// 基于 FFTW 双精度接口的三维复数到复数 (C2C) 离散傅里叶变换辅助类。
/// 提供简洁公共 API：Forward / Inverse；原始实现改为 internal 并统一封装计划的创建、执行与销毁流程。
/// </summary>
/// <remarks>
/// - FFTW 默认不执行归一化；若需 1/(nx*ny*nz) 等缩放，请在外部完成。<br/>
/// - 可原地或非原地调用，是否允许原地取决于数据布局与 FFTW 限制。<br/>
/// - 输入/输出缓冲区需为连续内存，展平后长度至少为 nx*ny*nz。<br/>
/// </remarks>
public static partial class Dft3D
{
    // ==========================
    // 精简友好命名的公共 API
    // ==========================

    // Forward（C2C）
    /// <summary>
    /// 执行三维复数到复数的正向傅里叶变换（C2C，Span 重载）。
    /// </summary>
    /// <param name="input">输入数据（线性展平的 3D <see cref="Complex"/>）。</param>
    /// <param name="output">输出数据（线性展平的 3D <see cref="Complex"/>）。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(Span<Complex> input, Span<Complex> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => Dft3DComplex(input, output, nx, ny, nz, fftw_direction.Forward, flags);

    /// <summary>
    /// 执行三维复数到复数的正向傅里叶变换（C2C，PinnableArray 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 3D <see cref="Complex"/> 数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 3D <see cref="Complex"/> 数据）。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Forward(PinnableArray<Complex> input, PinnableArray<Complex> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => Dft3DComplex(input, output, nx, ny, nz, fftw_direction.Forward, flags);

    // Inverse（C2C）

    /// <summary>
    /// 执行三维复数到复数的逆向傅里叶变换（C2C，Span 重载）。注意：FFTW 的逆变换不执行归一化。
    /// </summary>
    /// <param name="input">输入数据（线性展平的 3D <see cref="Complex"/>）。</param>
    /// <param name="output">输出数据（线性展平的 3D <see cref="Complex"/>）。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(Span<Complex> input, Span<Complex> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => Dft3DComplex(input, output, nx, ny, nz, fftw_direction.Backward, flags);

    /// <summary>
    /// 执行三维复数到复数的逆向傅里叶变换（C2C，PinnableArray 重载）。注意：FFTW 的逆变换不执行归一化。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 3D <see cref="Complex"/> 数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 3D <see cref="Complex"/> 数据）。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    public static void Inverse(PinnableArray<Complex> input, PinnableArray<Complex> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => Dft3DComplex(input, output, nx, ny, nz, fftw_direction.Backward, flags);

    // ==========================
    // 原始实现（内部管线，设为 internal）
    // ==========================

    /// <summary>
    /// 执行三维复数到复数 (C2C) DFT（指针重载，支持原地/非原地）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向至少 nx*ny*nz 个连续的 <see cref="Complex"/> 元素。</param>
    /// <param name="output">输出缓冲区指针，指向与输入同尺寸的连续 <see cref="Complex"/> 元素。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>（前向或反向）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 不执行归一化；结果幅值与方向有关。<br/>
    /// - 原地模式需确保数据布局与 FFTW 限制相容。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void Dft3DComplex_Double(IntPtr input, IntPtr output, int nx, int ny, int nz, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);

        var plan = fftw.dft_3d(nx, ny, nz, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create 3D complex plan.");

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
    /// 执行三维复数到复数 (C2C) DFT（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据缓冲区（线性展平的 3D <see cref="Complex"/> 数据）。</param>
    /// <param name="output">输出数据缓冲区（线性展平的 3D <see cref="Complex"/> 数据）。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 方法内部通过 <c>fixed</c> 固定内存并调用指针重载；不执行归一化。<br/>
    /// - 要求 <paramref name="input"/> 与 <paramref name="output"/> 的长度均不小于 nx*ny*nz。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为空(Length==0)时抛出。</exception>
    /// <exception cref="ArgumentException">当输入或输出长度小于指定维度总元素数时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void Dft3DComplex(Span<Complex> input, Span<Complex> output, int nx, int ny, int nz, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (input.Length < nx * ny * nz) throw new ArgumentException("Input span is too small for the specified dimensions.", nameof(input));
        if (output.Length < nx * ny * nz) throw new ArgumentException("Output span is too small for the specified dimensions.", nameof(output));
        unsafe
        {
            fixed (Complex* inputPtr = input)
            fixed (Complex* outputPtr = output)
            {
                Dft3DComplex_Double((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, nz, direction, flags);
            }
        }
    }

    /// <summary>
    /// 执行三维复数到复数 (C2C) DFT（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 3D <see cref="Complex"/> 数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 3D <see cref="Complex"/> 数据）。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法直接将已固定内存传入 FFTW 计划；不执行归一化。<br/>
    /// - 在允许的情形下可采用原地模式；请确保缓冲区已固定(Pinned)。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    internal static void Dft3DComplex(PinnableArray<Complex> input, PinnableArray<Complex> output, int nx, int ny, int nz, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        var plan = fftw.dft_3d(nx, ny, nz, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create 3D complex plan.");
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