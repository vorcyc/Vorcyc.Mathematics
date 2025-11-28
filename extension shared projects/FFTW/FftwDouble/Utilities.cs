using System.Numerics;
using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// FFTW 双精度工具方法集合。
/// 提供 1D 复数 DFT 的 FLOPs 估计与计划打印辅助函数（不执行数据变换）。
/// </summary>
/// <remarks>
/// - 这些方法仅创建临时计划以查询/打印信息；计划创建失败抛出 <see cref="InvalidOperationException"/>。<br/>
/// - 传入的缓冲区应为连续内存，类型为 <see cref="Complex"/>（双精度）。<br/>
/// </remarks>
public static partial class Utilities
{
    // =========================
    // 1D Complex-to-Complex (C2C)
    // 不暴露 internal 的 fftw_direction；改为显式 Forward/Backward 方法
    // =========================

    /// <summary>
    /// 估算一次 1D 复数到复数 (C2C) DFT 的 FLOPs（方向：Forward）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="Complex"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="Complex"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <returns>包含三元组 (adds, muls, fmas) 的元组，单位为操作次数（double）。</returns>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static (double adds, double muls, double fmas) MeasureFlopsFor1DComplexForward_Double(
        IntPtr input,
        IntPtr output,
        int n,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftw.dft_1d(n, input, output, fftw_direction.Forward, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for flops measurement (Forward).");
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
    /// 估算一次 1D 复数到复数 (C2C) DFT 的 FLOPs（方向：Backward）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区，线性展平的 <see cref="Complex"/> 数据，长度至少为 <paramref name="n"/>。</param>
    /// <param name="output">已固定的输出缓冲区，线性展平的 <see cref="Complex"/> 数据，长度至少为 <paramref name="n"/>。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <returns>包含三元组 (adds, muls, fmas) 的元组，单位为操作次数（double）。</returns>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static (double adds, double muls, double fmas) MeasureFlopsFor1DComplexBackward(
        PinnableArray<Complex> input,
        PinnableArray<Complex> output,
        int n,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftw.dft_1d(n, input, output, fftw_direction.Backward, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for flops measurement (Backward).");
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
    /// 创建并打印 1D 复数到复数 (C2C) DFT 计划（方向：Forward）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="Complex"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="Complex"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void PrintPlan1DComplexForward_Double(
        IntPtr input,
        IntPtr output,
        int n,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftw.dft_1d(n, input, output, fftw_direction.Forward, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for printing (Forward).");
        try
        {
            fftw.print_plan(plan);
        }
        finally
        {
            fftw.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 创建并打印 1D 复数到复数 (C2C) DFT 计划（方向：Backward）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="Complex"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="Complex"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void PrintPlan1DComplexBackward_Double(
        IntPtr input,
        IntPtr output,
        int n,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftw.dft_1d(n, input, output, fftw_direction.Backward, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for printing (Backward).");
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