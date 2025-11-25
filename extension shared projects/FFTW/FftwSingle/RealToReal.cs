using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW.FftwSingle;

/// <summary>
/// 基于 FFTW 单精度接口的实数到实数 (R2R) 变换帮助类。
/// 封装了一维、二维、三维及 n 维 R2R 变换的指针、Span 与已固定数组重载。
/// </summary>
/// <remarks>
/// - 本类方法不执行归一化；如需 1/N、1/(nx*ny) 或 1/∏dims 等缩放，请在外部完成。<br/>
/// - 支持原地与非原地调用，但是否允许原地取决于具体 <see cref="fftw_kind"/> 与 FFTW 的限制。<br/>
/// - 输入/输出缓冲区需为连续内存且大小足够；已固定重载要求缓冲区处于 Pinned 状态。<br/>
/// - 多维接口要求 <c>dims</c> 与 <c>kinds</c> 长度一致；计划创建失败将抛出 <see cref="InvalidOperationException"/>。<br/>
/// - 方法内部会创建、执行并销毁 FFTW 计划；调用期间应确保对缓冲区的独占访问以避免数据竞争。<br/>
/// </remarks>
public static class RealToReal
{

    #region Real-to-real (1D, 2D, 3D, n-D)

    /// <summary>
    /// 执行一维实数到实数 (R2R) 变换（支持原地/非原地，取决于 <paramref name="kind"/> 及 FFTW 限制）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向长度为 <paramref name="n"/> 的单精度实数数组。</param>
    /// <param name="output">输出缓冲区指针，指向长度为 <paramref name="n"/> 的单精度实数数组。</param>
    /// <param name="n">变换长度 (N)。</param>
    /// <param name="kind">R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法不执行归一化；如需 1/N 等缩放请在外部完成。<br/>
    /// - 指针必须有效且指向至少 N 个 <see cref="float"/> 的连续内存。<br/>
    /// - 若进行原地变换，请确保 FFTW 对应种类允许原地模式。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    public static void R2R1D(IntPtr input, IntPtr output, int n, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        var plan = fftwf.r2r_1d(n, input, output, kind, flags);

        InvalidOperationException.ThrowIfZero(plan, "Failed to create r2r 1D plan.");
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
    /// 执行一维实数到实数 (R2R) 变换（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据，长度必须与 <paramref name="output"/> 相同且大于 0。</param>
    /// <param name="output">输出数据缓冲区，长度必须与 <paramref name="input"/> 相同。</param>
    /// <param name="kind">R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 方法内部通过 <c>fixed</c> 固定内存并调用指针重载。<br/>
    /// - 不执行归一化；必要时请在外部缩放。<br/>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// 当 <paramref name="input"/> 或 <paramref name="output"/> 为空，或两者长度不一致时抛出。
    /// </exception>
    public static void R2R1D(Span<float> input, Span<float> output, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (input.Length != output.Length) throw new ArgumentException("Input and output spans must have the same length.", nameof(output));
        unsafe
        {
            fixed (float* inputPtr = input)
            fixed (float* outputPtr = output)
            {
                R2R1D((IntPtr)inputPtr, (IntPtr)outputPtr, input.Length, kind, flags);
            }
        }
    }

    /// <summary>
    /// 执行一维实数到实数 (R2R) 变换（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区，长度必须与 <paramref name="output"/> 相同。</param>
    /// <param name="output">已固定的输出缓冲区，长度必须与 <paramref name="input"/> 相同。</param>
    /// <param name="kind">R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法会调用指针重载执行变换；不执行归一化。<br/>
    /// - 当需要原地模式时，可将 <paramref name="input"/> 与 <paramref name="output"/> 指向相同内存（前提是 FFTW 允许）。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)时抛出。</exception>
    /// <exception cref="ArgumentException">当输入与输出长度不一致时抛出。</exception>
    public static void R2R1D(PinnableArray<float> input, PinnableArray<float> output, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        if (input.Length != output.Length) throw new ArgumentException("Input and output arrays must have the same length.", nameof(output));
        R2R1D(input, output, input.Length, kind, flags);
    }


    #endregion


    #region 2D

    /// <summary>
    /// 执行二维实数到实数 (R2R) 变换（支持原地/非原地，具体取决于 <paramref name="kindx"/>、<paramref name="kindy"/> 及 FFTW 限制）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向包含二维数据的连续单精度实数内存。</param>
    /// <param name="output">输出缓冲区指针，指向与输入同尺寸的连续单精度实数内存。</param>
    /// <param name="nx">X 维度大小（列或第一维长度）。</param>
    /// <param name="ny">Y 维度大小（行或第二维长度）。</param>
    /// <param name="kindx">X 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="kindy">Y 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法不执行归一化；如需 1/(nx*ny) 等缩放请在外部完成。<br/>
    /// - 指针必须指向至少 nx*ny 个 <see cref="float"/> 的连续内存。<br/>
    /// - 若进行原地变换，请确保对应 R2R 种类允许原地模式。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    public static void R2R2D(IntPtr input, IntPtr output, int nx, int ny, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        var plan = fftwf.r2r_2d(nx, ny, input, output, kindx, kindy, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create r2r 2D plan.");
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
    /// 执行二维实数到实数 (R2R) 变换（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据缓冲区（线性展平的二维数据）。</param>
    /// <param name="output">输出数据缓冲区（线性展平的二维数据）。</param>
    /// <param name="kindx">X 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="kindy">Y 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 方法内部通过 <c>fixed</c> 固定内存并调用指针重载。<br/>
    /// - 不执行归一化；必要时请在外部缩放。<br/>
    /// - 当前实现将 <paramref name="input"/> 的长度视为 nx，将 <paramref name="output"/> 的长度视为 ny；请确保两者与实际二维尺寸一致。<br/>
    /// </remarks>
    /// <exception cref="ArgumentException">当输入或输出为空时可能导致无效行为；请确保长度与期望的二维尺寸匹配。</exception>
    public static void R2R2D(Span<float> input, Span<float> output, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        int nx = input.Length;
        int ny = output.Length;
        unsafe
        {
            fixed (float* inputPtr = input)
            fixed (float* outputPtr = output)
            {
                R2R2D((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, kindx, kindy, flags);
            }
        }
    }

    /// <summary>
    /// 执行二维实数到实数 (R2R) 变换（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的二维数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的二维数据）。</param>
    /// <param name="kindx">X 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="kindy">Y 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法会调用指针重载执行变换；不执行归一化。<br/>
    /// - 当需要原地模式时，可将 <paramref name="input"/> 与 <paramref name="output"/> 指向相同内存（前提是 FFTW 允许）。<br/>
    /// - 当前实现将 <paramref name="input"/> 的长度视为 nx，将 <paramref name="output"/> 的长度视为 ny；请确保两者与实际二维尺寸一致。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)时抛出。</exception>
    public static void R2R2D(PinnableArray<float> input, PinnableArray<float> output, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
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
    /// 执行三维实数到实数 (R2R) 变换（支持原地/非原地，具体取决于 <paramref name="kindx"/>、<paramref name="kindy"/>、<paramref name="kindz"/> 及 FFTW 限制）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向包含 3D 数据的连续单精度实数内存，大小至少为 nx*ny*nz。</param>
    /// <param name="output">输出缓冲区指针，指向与输入同尺寸的连续单精度实数内存。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="kindx">X 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="kindy">Y 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="kindz">Z 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法不执行归一化；如需 1/(nx*ny*nz) 等缩放请在外部完成。<br/>
    /// - 指针必须有效且指向至少 nx*ny*nz 个 <see cref="float"/> 的连续内存。<br/>
    /// - 若进行原地变换，请确保对应 R2R 种类允许原地模式。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    public static void R2R3D(IntPtr input, IntPtr output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        var plan = fftwf.r2r_3d(nx, ny, nz, input, output, kindx, kindy, kindz, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create r2r 3D plan.");
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
    /// 执行三维实数到实数 (R2R) 变换（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据缓冲区（线性展平的 3D 数据）。</param>
    /// <param name="output">输出数据缓冲区（线性展平的 3D 数据）。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="kindx">X 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="kindy">Y 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="kindz">Z 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 方法内部通过 <c>fixed</c> 固定内存并调用指针重载。<br/>
    /// - 不执行归一化；必要时请在外部缩放。<br/>
    /// - 请确保输入与输出的总长度满足 nx*ny*nz 的线性展平约定。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为空时抛出。</exception>
    public static void R2R3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        unsafe
        {
            fixed (float* inputPtr = input)
            fixed (float* outputPtr = output)
            {
                R2R3D((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, nz, kindx, kindy, kindz, flags);
            }
        }
    }

    /// <summary>
    /// 执行三维实数到实数 (R2R) 变换（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 3D 数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 3D 数据）。</param>
    /// <param name="nx">X 维度大小（第一维长度）。</param>
    /// <param name="ny">Y 维度大小（第二维长度）。</param>
    /// <param name="nz">Z 维度大小（第三维长度）。</param>
    /// <param name="kindx">X 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="kindy">Y 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="kindz">Z 方向的 R2R 变换类型，见 <see cref="fftw_kind"/>。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法会调用指针重载执行变换；不执行归一化。<br/>
    /// - 当需要原地模式时，可将 <paramref name="input"/> 与 <paramref name="output"/> 指向相同内存（前提是 FFTW 允许）。<br/>
    /// - 请确保两者内存均已固定(Pinned)，且总长度满足 nx*ny*nz 的线性展平约定。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)时抛出。</exception>
    public static void R2R3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        R2R3D(input, output, nx, ny, nz, kindx, kindy, kindz, flags);
    }

    #endregion


    #region ND


    /// <summary>
    /// 执行 n 维实数到实数 (R2R) 变换（支持原地/非原地，具体取决于各维度的 <paramref name="kinds"/> 及 FFTW 限制）。
    /// </summary>
    /// <param name="input">输入缓冲区指针，指向包含 n 维数据的连续单精度实数内存。</param>
    /// <param name="output">输出缓冲区指针，指向与输入同尺寸的连续单精度实数内存。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="kinds">每一维对应的 R2R 变换类型数组，长度必须与 <paramref name="dims"/> 相同。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法不执行归一化；如需 1/∏dims 等缩放请在外部完成。<br/>
    /// - 指针必须有效且指向至少 ∏dims 个 <see cref="float"/> 的连续内存。<br/>
    /// - 若进行原地变换，请确保各维度的 R2R 种类允许原地模式。<br/>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="ArgumentException">当 <paramref name="dims"/> 与 <paramref name="kinds"/> 的长度不一致时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    public static void R2RND(IntPtr input, IntPtr output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));

        var rank = dims.Length;
        var n = dims.ToArray();
        var k = kinds.ToArray();

        var plan = fftwf.r2r(rank, n, input, output, k, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create r2r n-D plan.");
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
    /// 执行 n 维实数到实数 (R2R) 变换（Span 重载）。
    /// </summary>
    /// <param name="input">输入数据缓冲区（线性展平的 n 维数据）。</param>
    /// <param name="output">输出数据缓冲区（线性展平的 n 维数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="kinds">每一维对应的 R2R 变换类型数组，长度必须与 <paramref name="dims"/> 相同。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 方法内部通过 <c>fixed</c> 固定内存并调用指针重载。<br/>
    /// - 不执行归一化；必要时请在外部缩放。<br/>
    /// - 请确保输入与输出的总长度满足 ∏dims 的线性展平约定。<br/>
    /// </remarks>
    /// <exception cref="ArgumentException">当 <paramref name="dims"/> 与 <paramref name="kinds"/> 的长度不一致时抛出。</exception>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为空时抛出。</exception>
    public static void R2RND(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));
        unsafe
        {
            fixed (float* inputPtr = input)
            fixed (float* outputPtr = output)
            {
                R2RND((IntPtr)inputPtr, (IntPtr)outputPtr, dims, kinds, flags);
            }
        }
    }

    /// <summary>
    /// 执行 n 维实数到实数 (R2R) 变换（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    /// <param name="input">已固定的输入缓冲区（线性展平的 n 维数据）。</param>
    /// <param name="output">已固定的输出缓冲区（线性展平的 n 维数据）。</param>
    /// <param name="dims">每一维的尺寸数组（长度即维度数 rank）。</param>
    /// <param name="kinds">每一维对应的 R2R 变换类型数组，长度必须与 <paramref name="dims"/> 相同。</param>
    /// <param name="flags">规划策略，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 本方法会调用指针重载执行变换；不执行归一化。<br/>
    /// - 当需要原地模式时，可将 <paramref name="input"/> 与 <paramref name="output"/> 指向相同内存（前提是 FFTW 允许）。<br/>
    /// - 请确保两者均已固定(Pinned)，且总长度满足 ∏dims 的线性展平约定。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定(Pinned)时抛出。</exception>
    /// <exception cref="ArgumentException">当 <paramref name="dims"/> 与 <paramref name="kinds"/> 的长度不一致时抛出。</exception>
    public static void R2RND(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));
        R2RND(input, output, dims, kinds, flags);
    }


    #endregion



}
