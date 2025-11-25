using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW.FftwDouble;

/// <summary>
/// 基于 FFTW 双精度接口的实数到实数 (R2R) 变换帮助类。
/// 提供一维、二维、三维以及 n 维 R2R 变换的指针、Span 与已固定数组 (Pinned) 重载。
/// </summary>
/// <remarks>
/// - 所有方法均不执行归一化；如需 1/N、1/(nx*ny)、1/(nx*ny*nz) 或 1/∏dims 的缩放请在外部处理。<br/>
/// - 支持原地与非原地调用；是否允许原地取决于具体 <see cref="fftw_kind"/> 以及 FFTW 的实现限制。<br/>
/// - 输入与输出缓冲区必须为连续内存，长度满足各维度元素总数；使用已固定重载时需确保缓冲区处于 Pinned 状态。<br/>
/// - 多维接口要求 <c>dims</c> 与 <c>kinds</c> 长度一致；计划创建失败将抛出 <see cref="InvalidOperationException"/>。<br/>
/// - 每次调用均同步创建、执行并销毁 FFTW 计划；在高频循环中可自行缓存计划以提升性能。<br/>
/// - 执行期间请避免并发访问输入/输出缓冲区以防止数据竞争。<br/>
/// </remarks>
public static class RealToReal
{

    #region Real-to-real (1D, 2D, 3D, n-D)

    /// <summary>
    /// 执行一维实数到实数 (R2R) 变换（指针重载，支持原地/非原地，取决于 <paramref name="kind"/>）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向长度为 <paramref name="n"/> 的连续双精度实数 (<see cref="double"/>) 内存。</param>
    /// <param name="output">输出缓冲区指针，指向长度为 <paramref name="n"/> 的连续双精度实数内存。</param>
    /// <param name="n">变换长度 N。</param>
    /// <param name="kind">R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 不执行归一化；如需 1/N 缩放请在外部完成。<br/>
    /// - 指针须有效且至少指向 n 个 <see cref="double"/> 的连续内存。<br/>
    /// - 原地模式需确保对应变换种类允许。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">计划创建失败时抛出。</exception>
    public static void R2R1D(IntPtr input, IntPtr output, int n, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        var plan = fftw.r2r_1d(n, input, output, kind, flags);

        InvalidOperationException.ThrowIfZero(plan, "Failed to create r2r 1D plan.");
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
    /// 执行一维实数到实数 (R2R) 变换（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据 Span，长度必须与 <paramref name="output"/> 相同且大于 0。</param>
    /// <param name="output">输出数据 Span，与 <paramref name="input"/> 长度一致。</param>
    /// <param name="kind">R2R 变换类型。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 使用 <c>fixed</c> 固定后调用指针重载。<br/>
    /// - 不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="ArgumentException">当两者长度不一致时抛出。</exception>
    public static void R2R1D(Span<double> input, Span<double> output, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (input.Length != output.Length) throw new ArgumentException("Input and output spans must have the same length.", nameof(output));
        unsafe
        {
            fixed (double* inputPtr = input)
            fixed (double* outputPtr = output)
            {
                R2R1D((IntPtr)inputPtr, (IntPtr)outputPtr, input.Length, kind, flags);
            }
        }
    }

    /// <summary>
    /// 执行一维实数到实数 (R2R) 变换（已固定数组重载）。
    /// </summary>
    /// <param name="input">已固定输入缓冲区。</param>
    /// <param name="output">已固定输出缓冲区。</param>
    /// <param name="kind">R2R 变换类型。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 可原地（input 与 output 指向同一内存）使用，需 FFTW 支持。<br/>
    /// - 不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定时抛出。</exception>
    /// <exception cref="ArgumentException">长度不一致时抛出。</exception>
    public static void R2R1D(PinnableArray<double> input, PinnableArray<double> output, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        if (input.Length != output.Length) throw new ArgumentException("Input and output arrays must have the same length.", nameof(output));
        R2R1D(input, output, input.Length, kind, flags);
    }


    #endregion


    #region 2D

    /// <summary>
    /// 执行二维实数到实数 (R2R) 变换（指针重载）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向至少 nx*ny 个连续 <see cref="double"/>。</param>
    /// <param name="output">输出缓冲区指针，与输入同尺寸。</param>
    /// <param name="nx">X 维长度。</param>
    /// <param name="ny">Y 维长度。</param>
    /// <param name="kindx">X 方向 R2R 类型。</param>
    /// <param name="kindy">Y 方向 R2R 类型。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 不执行归一化。<br/>
    /// - 原地需对应类型允许。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">指针为零时。</exception>
    /// <exception cref="InvalidOperationException">计划创建失败时。</exception>
    public static void R2R2D(IntPtr input, IntPtr output, int nx, int ny, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        var plan = fftw.r2r_2d(nx, ny, input, output, kindx, kindy, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create r2r 2D plan.");
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
    /// 执行二维实数到实数 (R2R) 变换（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据（展平二维）。</param>
    /// <param name="output">输出数据（展平二维）。</param>
    /// <param name="kindx">X 方向 R2R 类型。</param>
    /// <param name="kindy">Y 方向 R2R 类型。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 将 input.Length 视为 nx，output.Length 视为 ny，需调用者保证语义正确。<br/>
    /// - 不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="ArgumentException">长度与实际维度语义不符时可能导致错误使用。</exception>
    public static void R2R2D(Span<double> input, Span<double> output, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        int nx = input.Length;
        int ny = output.Length;
        unsafe
        {
            fixed (double* inputPtr = input)
            fixed (double* outputPtr = output)
            {
                R2R2D((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, kindx, kindy, flags);
            }
        }
    }

    /// <summary>
    /// 执行二维实数到实数 (R2R) 变换（已固定数组重载）。
    /// </summary>
    /// <param name="input">已固定输入展平二维数据。</param>
    /// <param name="output">已固定输出展平二维数据。</param>
    /// <param name="kindx">X 方向 R2R 类型。</param>
    /// <param name="kindy">Y 方向 R2R 类型。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - input.Length 作为 nx，output.Length 作为 ny。<br/>
    /// - 不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">未固定时。</exception>
    public static void R2R2D(PinnableArray<double> input, PinnableArray<double> output, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        int nx = input.Length;
        int ny = output.Length;
        R2R2D(input, output, nx, ny, kindx, kindy, flags);
    }


    #endregion


    #region 3D

    /// <summary>
    /// 执行三维实数到实数 (R2R) 变换（指针重载）。
    /// </summary>
    /// <param name="input">输入指针，至少 nx*ny*nz 个连续 <see cref="double"/>。</param>
    /// <param name="output">输出指针，尺寸与输入一致。</param>
    /// <param name="nx">X 维长度。</param>
    /// <param name="ny">Y 维长度。</param>
    /// <param name="nz">Z 维长度。</param>
    /// <param name="kindx">X 方向 R2R 类型。</param>
    /// <param name="kindy">Y 方向 R2R 类型。</param>
    /// <param name="kindz">Z 方向 R2R 类型。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 不执行归一化。<br/>
    /// - 原地模式需类型支持。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">指针为零。</exception>
    /// <exception cref="InvalidOperationException">计划创建失败。</exception>
    public static void R2R3D(IntPtr input, IntPtr output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        var plan = fftw.r2r_3d(nx, ny, nz, input, output, kindx, kindy, kindz, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create r2r 3D plan.");
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
    /// 执行三维实数到实数 (R2R) 变换（Span 重载）。
    /// </summary>
    /// <param name="input">展平三维输入数据。</param>
    /// <param name="output">展平三维输出数据。</param>
    /// <param name="nx">X 维长度。</param>
    /// <param name="ny">Y 维长度。</param>
    /// <param name="nz">Z 维长度。</param>
    /// <param name="kindx">X 方向类型。</param>
    /// <param name="kindy">Y 方向类型。</param>
    /// <param name="kindz">Z 方向类型。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 调用者需确保长度 >= nx*ny*nz。<br/>
    /// - 不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">输入或输出为空。</exception>
    public static void R2R3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        unsafe
        {
            fixed (double* inputPtr = input)
            fixed (double* outputPtr = output)
            {
                R2R3D((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, nz, kindx, kindy, kindz, flags);
            }
        }
    }

    /// <summary>
    /// 执行三维实数到实数 (R2R) 变换（已固定数组重载）。
    /// </summary>
    /// <param name="input">已固定展平三维输入。</param>
    /// <param name="output">已固定展平三维输出。</param>
    /// <param name="nx">X 维长度。</param>
    /// <param name="ny">Y 维长度。</param>
    /// <param name="nz">Z 维长度。</param>
    /// <param name="kindx">X 方向类型。</param>
    /// <param name="kindy">Y 方向类型。</param>
    /// <param name="kindz">Z 方向类型。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 可原地使用（需支持）。<br/>
    /// - 不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">未固定时。</exception>
    public static void R2R3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        R2R3D(input, output, nx, ny, nz, kindx, kindy, kindz, flags);
    }

    #endregion


    #region ND

    /// <summary>
    /// 执行 n 维实数到实数 (R2R) 变换（指针重载）。
    /// </summary>
    /// <param name="input">输入指针，指向至少 ∏dims 个连续 <see cref="double"/>。</param>
    /// <param name="output">输出指针，与输入尺寸一致。</param>
    /// <param name="dims">各维尺寸数组。</param>
    /// <param name="kinds">各维对应 R2R 类型，长度须与 <paramref name="dims"/> 一致。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 不执行归一化。<br/>
    /// - 原地模式需全部维度类型允许。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">指针为零。</exception>
    /// <exception cref="ArgumentException">长度不一致。</exception>
    /// <exception cref="InvalidOperationException">计划创建失败。</exception>
    public static void R2RND(IntPtr input, IntPtr output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));

        var rank = dims.Length;
        var n = dims.ToArray();
        var k = kinds.ToArray();

        var plan = fftw.r2r(rank, n, input, output, k, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create r2r n-D plan.");
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
    /// 执行 n 维实数到实数 (R2R) 变换（Span 重载）。
    /// </summary>
    /// <param name="input">展平 n 维输入数据。</param>
    /// <param name="output">展平 n 维输出数据。</param>
    /// <param name="dims">各维尺寸。</param>
    /// <param name="kinds">各维 R2R 类型。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 长度需满足总元素数。<br/>
    /// - 不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="ArgumentException">dims 与 kinds 长度不一致。</exception>
    /// <exception cref="ArgumentNullException">输入或输出为空。</exception>
    public static void R2RND(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));
        unsafe
        {
            fixed (double* inputPtr = input)
            fixed (double* outputPtr = output)
            {
                R2RND((IntPtr)inputPtr, (IntPtr)outputPtr, dims, kinds, flags);
            }
        }
    }

    /// <summary>
    /// 执行 n 维实数到实数 (R2R) 变换（已固定数组重载）。
    /// </summary>
    /// <param name="input">已固定展平 n 维输入。</param>
    /// <param name="output">已固定展平 n 维输出。</param>
    /// <param name="dims">各维尺寸。</param>
    /// <param name="kinds">各维 R2R 类型。</param>
    /// <param name="flags">规划策略。</param>
    /// <remarks>
    /// - 可原地使用（需支持）。<br/>
    /// - 不执行归一化。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">输入或输出未固定。</exception>
    /// <exception cref="ArgumentException">dims 与 kinds 长度不一致。</exception>
    public static void R2RND(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));
        R2RND(input, output, dims, kinds, flags);
    }


    #endregion



}