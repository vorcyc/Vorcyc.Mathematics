using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// FFTW 单精度工具方法集合。
/// 提供 1D 复数 DFT 的 FLOPs 估计与计划打印辅助函数。
/// </summary>
/// <remarks>
/// - 这些方法仅用于创建临时计划以查询/打印信息，不执行实际数据变换。<br/>
/// - 计划创建失败会抛出 <see cref="InvalidOperationException"/>。<br/>
/// </remarks>
public static partial class Utilities
{

    // 原始 Utilities（改为 internal）

    /// <summary>
    /// 估算一次 1D 复数到复数 (C2C) DFT 的浮点运算量（加法、乘法与 FMA 次数）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="ComplexFp32"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="ComplexFp32"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="direction">变换方向（internal）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <returns>包含三元组 (adds, muls, fmas) 的元组，单位为操作次数（double）。</returns>
    /// <remarks>
    /// - 使用 FFTW 的 <c>fftwf.flops</c> 获取运算量估计；结果对应单次执行的理论操作数。<br/>
    /// - 本方法不执行归一化，也不验证指针有效性；请确保内存合法且连续。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    internal static (double adds, double muls, double fmas) MeasureFlopsFor1DComplex_Single(
        IntPtr input,
        IntPtr output,
        int n,
        fftw_direction direction,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftwf.dft_1d(n, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for flops measurement.");
        try
        {
            double add = 0, mul = 0, fma = 0;
            fftwf.flops(plan, ref add, ref mul, ref fma);
            return (add, mul, fma);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 估算一次 1D 复数到复数 (C2C) DFT 的浮点运算量（已固定缓冲区重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区，线性展平的 <see cref="ComplexFp32"/> 数据，长度至少为 <paramref name="n"/>。</param>
    /// <param name="output">已固定的输出缓冲区，线性展平的 <see cref="ComplexFp32"/> 数据，长度至少为 <paramref name="n"/>。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="direction">变换方向（internal）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <returns>包含三元组 (adds, muls, fmas) 的元组，单位为操作次数（double）。</returns>
    /// <remarks>
    /// - 使用 FFTW 的 <c>fftwf.flops</c> 获取运算量估计；结果对应单次执行的理论操作数。<br/>
    /// - 本方法不执行归一化；请确保缓冲区已固定(Pinned)且大小充足。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    internal static (double adds, double muls, double fmas) MeasureFlopsFor1DComplex(
        PinnableArray<ComplexFp32> input,
        PinnableArray<ComplexFp32> output,
        int n,
        fftw_direction direction,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftwf.dft_1d(n, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for flops measurement.");
        try
        {
            double add = 0, mul = 0, fma = 0;
            fftwf.flops(plan, ref add, ref mul, ref fma);
            return (add, mul, fma);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 创建并打印 1D 复数到复数 (C2C) DFT 计划，以便调试和分析 FFTW 的规划结果。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向连续的 <see cref="ComplexFp32"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="output">输出缓冲区指针，指向连续的 <see cref="ComplexFp32"/> 内存，长度至少为 <paramref name="n"/>。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="direction">变换方向（internal）。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 使用 <c>fftwf.print_plan</c> 将计划信息输出到标准输出，用于诊断与性能分析。<br/>
    /// - 本方法不会执行变换。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    internal static void PrintPlan1DComplex_Single(
        IntPtr input,
        IntPtr output,
        int n,
        fftw_direction direction,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftwf.dft_1d(n, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for printing.");
        try
        {
            fftwf.print_plan(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    // 新的友好命名 Public API（不暴露 internal 的 fftw_direction）

    /// <summary>
    /// 估算一次 1D 复数到复数 (C2C) DFT 的 FLOPs（方向：Forward，指针版）。
    /// </summary>
    public static (double adds, double muls, double fmas) MeasureFlopsFor1DComplexForward_Single(
        IntPtr input,
        IntPtr output,
        int n,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftwf.dft_1d(n, input, output, fftw_direction.Forward, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for flops measurement (Forward).");
        try
        {
            double add = 0, mul = 0, fma = 0;
            fftwf.flops(plan, ref add, ref mul, ref fma);
            return (add, mul, fma);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 估算一次 1D 复数到复数 (C2C) DFT 的 FLOPs（方向：Backward，已固定缓冲区）。
    /// </summary>
    public static (double adds, double muls, double fmas) MeasureFlopsFor1DComplexBackward_Single(
        PinnableArray<ComplexFp32> input,
        PinnableArray<ComplexFp32> output,
        int n,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftwf.dft_1d(n, input, output, fftw_direction.Backward, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for flops measurement (Backward).");
        try
        {
            double add = 0, mul = 0, fma = 0;
            fftwf.flops(plan, ref add, ref mul, ref fma);
            return (add, mul, fma);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 创建并打印 1D 复数到复数 (C2C) DFT 计划（方向：Forward）。
    /// </summary>
    public static void PrintPlan1DComplexForward_Single(
        IntPtr input,
        IntPtr output,
        int n,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftwf.dft_1d(n, input, output, fftw_direction.Forward, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for printing (Forward).");
        try
        {
            fftwf.print_plan(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 创建并打印 1D 复数到复数 (C2C) DFT 计划（方向：Backward）。
    /// </summary>
    public static void PrintPlan1DComplexBackward_Single(
        IntPtr input,
        IntPtr output,
        int n,
        fftw_flags flags = fftw_flags.Estimate)
    {
        var plan = fftwf.dft_1d(n, input, output, fftw_direction.Backward, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create plan for printing (Backward).");
        try
        {
            fftwf.print_plan(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }
}