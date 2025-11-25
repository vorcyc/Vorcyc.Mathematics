using System.Numerics;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW.FftwDouble;

/// <summary>
/// FFTW 双精度工具方法集合。
/// 提供 1D 复数 DFT 的 FLOPs 估计与计划打印辅助函数（不执行数据变换）。
/// </summary>
/// <remarks>
/// - 这些方法仅创建临时计划以查询/打印信息；计划创建失败抛出 <see cref="InvalidOperationException"/>。<br/>
/// - 传入的缓冲区应为连续内存，类型为 <see cref="Complex"/>（双精度）。<br/>
/// </remarks>
public static class Utilities
{

    // Utilities

    /// <summary>
    /// 估算一次 1D 复数到复数 (C2C) DFT 的浮点运算量（加法、乘法与 FMA 次数）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="Complex"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="Complex"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <returns>包含三元组 (adds, muls, fmas) 的元组，单位为操作次数（double）。</returns>
    /// <remarks>
    /// - 使用 FFTW 的 <c>fftw.flops</c> 获取运算量估计；结果对应单次执行的理论操作数。<br/>
    /// - 不执行归一化，也不校验指针有效性；请确保内存合法且连续。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static (double adds, double muls, double fmas) MeasureFlopsFor1DComplex(IntPtr input, IntPtr output, int n, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftw.dft_1d(n, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for flops measurement.");
        try
        {
            double add = 0, mul = 0, fma = 0;
            fftw.flops(plan, ref add, ref mul, ref fma);
            return (add, mul, fma);
        }
        finally
        {
            fftw.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 估算一次 1D 复数到复数 (C2C) DFT 的浮点运算量（已固定缓冲区重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区，线性展平的 <see cref="Complex"/> 数据，长度至少为 <paramref name="n"/>。</param>
    /// <param name="output">已固定的输出缓冲区，线性展平的 <see cref="Complex"/> 数据，长度至少为 <paramref name="n"/>。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <returns>包含三元组 (adds, muls, fmas) 的元组，单位为操作次数（double）。</returns>
    /// <remarks>
    /// - 使用 <c>fftw.flops</c> 获取运算量估计；结果对应单次执行的理论操作数。<br/>
    /// - 不执行归一化；请确保缓冲区已固定(Pinned)且大小充足。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static (double adds, double muls, double fmas) MeasureFlopsFor1DComplex(
        PinnableArray<Complex> input,
        PinnableArray<Complex> output,
        int n, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftw.dft_1d(n, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for flops measurement.");
        try
        {
            double add = 0, mul = 0, fma = 0;
            fftw.flops(plan, ref add, ref mul, ref fma);
            return (add, mul, fma);
        }
        finally
        {
            fftw.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 创建并打印 1D 复数到复数 (C2C) DFT 计划，用于调试和分析 FFTW 的规划结果。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="Complex"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="Complex"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="direction">变换方向，见 <see cref="fftw_direction"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 使用 <c>fftw.print_plan</c> 将计划信息输出到标准输出，以便诊断与性能分析。<br/>
    /// - 本方法不执行数据变换。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void PrintPlan1DComplex(IntPtr input, IntPtr output, int n, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftw.dft_1d(n, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for printing.");
        try
        {
            fftw.print_plan(plan);
        }
        finally
        {
            fftw.destroy_plan(plan);
        }
    }
}