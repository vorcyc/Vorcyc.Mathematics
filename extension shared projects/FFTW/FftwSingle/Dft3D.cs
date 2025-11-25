using Vorcyc.Mathematics.Extensions.FFTW.Interop;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.Extensions.FFTW.FftwSingle;

/// <summary>
/// 基于 FFTW 单精度接口的三维复数到复数 (C2C) 离散傅里叶变换帮助类。
/// 封装指针、Span 与已固定数组三种重载，统一创建/执行/销毁 FFTW 计划的流程。
/// </summary>
/// <remarks>
/// - 本类方法不执行归一化；如需 1/(nx*ny*nz) 等缩放，请在外部完成。<br/>
/// - 支持原地与非原地调用，是否允许原地取决于 FFTW 的布局与限制。<br/>
/// - 输入/输出缓冲区应为连续内存，长度至少为 nx*ny*nz。<br/>
/// </remarks>
public static class Dft3D
{

    // 3D complex-to-complex

    /// <summary>
    /// 执行三维复数到复数 (C2C) DFT（指针重载）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="ComplexFp32"/> 内存，大小至少为 nx*ny*nz。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="ComplexFp32"/> 内存，大小至少为 nx*ny*nz。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>（前向或反向）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - FFTW 默认不进行归一化；结果幅值与方向有关。<br/>
    /// - 若进行原地变换，请确保数据布局与 FFTW 限制相容。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    public static void Dft3DComplex(IntPtr input, IntPtr output, int nx, int ny, int nz, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);

        var plan = fftwf.dft_3d(nx, ny, nz, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create 3D complex plan.");

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
    /// 执行三维复数到复数 (C2C) DFT（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据缓冲区（线性展平的 3D <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="output">输出数据缓冲区（线性展平的 3D <see cref="ComplexFp32"/> 数据）。</param>
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
    public static void Dft3DComplex(Span<ComplexFp32> input, Span<ComplexFp32> output, int nx, int ny, int nz, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (input.Length < nx * ny * nz) throw new ArgumentException("Input span is too small for the specified dimensions.", nameof(input));
        if (output.Length < nx * ny * nz) throw new ArgumentException("Output span is too small for the specified dimensions.", nameof(output));
        unsafe
        {
            fixed (ComplexFp32* inputPtr = input)
            fixed (ComplexFp32* outputPtr = output)
            {
                Dft3DComplex((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, nz, direction, flags);
            }
        }
    }

    /// <summary>
    /// 执行三维复数到复数 (C2C) DFT（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 3D <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 3D <see cref="ComplexFp32"/> 数据）。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法直接将已固定内存传入 FFTW 计划；不执行归一化。<br/>
    /// - 在允许的情形下可采用原地模式；请确保内存已固定(Pinned)。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)或计划创建失败时抛出。</exception>
    public static void Dft3DComplex(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, int nx, int ny, int nz, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        var plan = fftwf.dft_3d(nx, ny, nz, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create 3D complex plan.");
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