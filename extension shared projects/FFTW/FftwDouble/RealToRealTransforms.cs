using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// 基于 FFTW 双精度接口的实数到实数 (R2R) 变换帮助类。
/// 提供一维、二维、三维以及 n 维 R2R 变换的指针、Span 与已固定数组 (Pinned) 重载。
/// 增加基于 <see cref="fftw_kind"/> 的友好别名方法，保留并完整延续所有 XML 注释与说明。
/// </summary>
/// <remarks>
/// - 所有方法均不执行归一化；如需 1/N、1/(nx*ny)、1/(nx*ny*nz) 或 1/∏dims 的缩放请在外部处理。<br/>
/// - 支持原地与非原地调用；是否允许原地取决于具体 <see cref="fftw_kind"/> 以及 FFTW 的实现限制。<br/>
/// - 输入与输出缓冲区必须为连续内存，长度满足各维度元素总数；使用已固定重载时需确保缓冲区处于 Pinned 状态。<br/>
/// - 多维接口要求 <c>dims</c> 与 <c>kinds</c> 长度一致；计划创建失败将抛出 <see cref="InvalidOperationException"/>。<br/>
/// - 每次调用均同步创建、执行并销毁 FFTW 计划；在高频循环中可自行缓存计划以提升性能。<br/>
/// - 执行期间请避免并发访问输入/输出缓冲区以防止数据竞争。<br/>
/// </remarks>
public static partial class RealToRealTransforms
{



    // ==========================
    // 友好公共 API（基于 fftw_kind 的别名）
    // ==========================

    #region R2HC / HC2R / DHT - 1D

    /// <summary>
    /// 执行一维实数到半复数谱 (R2HC) 变换（Span 重载，双精度）。
    /// </summary>
    /// <param name="input">输入实数序列（<see cref="double"/>）。</param>
    /// <param name="output">输出半复数紧凑谱（以实数形式存放）。</param>
    /// <param name="flags">FFTW 规划标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 不执行归一化；如需缩放请在外部完成。<br/>
    /// - 输入与输出长度必须相同；是否支持原地取决于 FFTW 限制。
    /// </remarks>
    public static void R2HC1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行一维半复数谱到实数 (HC2R) 变换（Span 重载，双精度）。
    /// </summary>
    /// <param name="input">输入半复数紧凑谱（以实数形式存放）。</param>
    /// <param name="output">输出实数序列。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    /// <remarks>
    /// - 输出为实数序列；不执行归一化。<br/>
    /// - 输入与输出长度必须相同。
    /// </remarks>
    public static void HC2R1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行一维离散哈特利变换 (DHT)（Span 重载，双精度）。
    /// </summary>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出哈特利谱（实数）。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    /// <remarks>
    /// - 使用 cas(x)=cos(x)+sin(x) 核；不执行归一化。<br/>
    /// - 输入与输出长度必须相同。
    /// </remarks>
    public static void Dht1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.DHT, flags);

    /// <summary>
    /// 执行一维实数到半复数谱 (R2HC) 变换（已固定数组重载，双精度）。
    /// </summary>
    /// <param name="input">已固定的输入数组。</param>
    /// <param name="output">已固定的输出数组。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void R2HC1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行一维半复数谱到实数 (HC2R) 变换（已固定数组重载，双精度）。
    /// </summary>
    /// <param name="input">已固定的输入数组（半复数紧凑谱）。</param>
    /// <param name="output">已固定的输出数组（实数序列）。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void HC2R1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行一维离散哈特利变换 (DHT)（已固定数组重载，双精度）。
    /// </summary>
    /// <param name="input">已固定的输入数组。</param>
    /// <param name="output">已固定的输出数组。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void Dht1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.DHT, flags);

    #endregion

    #region DCT (REDFT**) - 1D

    /// <summary>
    /// 执行一维 DCT-I (REDFT00)（Span 重载，双精度）。
    /// </summary>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出 DCT-I 结果。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void DctI1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行一维 DCT-II (REDFT01)（Span 重载，双精度）。
    /// </summary>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出 DCT-II 结果。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void DctII1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行一维 DCT-III (REDFT10)（Span 重载，双精度）。
    /// </summary>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出 DCT-III 结果。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void DctIII1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行一维 DCT-IV (REDFT11)（Span 重载，双精度）。
    /// </summary>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出 DCT-IV 结果。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void DctIV1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT11, flags);

    /// <summary>
    /// 执行一维 DCT-I (REDFT00)（已固定数组重载，双精度）。
    /// </summary>
    public static void DctI1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行一维 DCT-II (REDFT01)（已固定数组重载，双精度）。
    /// </summary>
    public static void DctII1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行一维 DCT-III (REDFT10)（已固定数组重载，双精度）。
    /// </summary>
    public static void DctIII1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行一维 DCT-IV (REDFT11)（已固定数组重载，双精度）。
    /// </summary>
    public static void DctIV1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT11, flags);

    #endregion

    #region DST (RODFT**) - 1D

    /// <summary>
    /// 执行一维 DST-I (RODFT00)（Span 重载，双精度）。
    /// </summary>
    public static void DstI1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行一维 DST-II (RODFT01)（Span 重载，双精度）。
    /// </summary>
    public static void DstII1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行一维 DST-III (RODFT10)（Span 重载，双精度）。
    /// </summary>
    public static void DstIII1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行一维 DST-IV (RODFT11)（Span 重载，双精度）。
    /// </summary>
    public static void DstIV1D(Span<double> input, Span<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT11, flags);

    /// <summary>
    /// 执行一维 DST-I (RODFT00)（已固定数组重载，双精度）。
    /// </summary>
    public static void DstI1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行一维 DST-II (RODFT01)（已固定数组重载，双精度）。
    /// </summary>
    public static void DstII1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行一维 DST-III (RODFT10)（已固定数组重载，双精度）。
    /// </summary>
    public static void DstIII1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行一维 DST-IV (RODFT11)（已固定数组重载，双精度）。
    /// </summary>
    public static void DstIV1D(PinnableArray<double> input, PinnableArray<double> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT11, flags);

    #endregion

    #region 2D 友好别名

    /// <summary>
    /// 执行二维 R2HC（两轴均为 R2HC）（Span 重载，双精度）。
    /// </summary>
    /// <param name="input">展平二维输入数据。</param>
    /// <param name="output">展平二维输出数据。</param>
    /// <param name="nx">X 维长度。</param>
    /// <param name="ny">Y 维长度。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void R2HC2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.R2HC, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行二维 HC2R（两轴均为 HC2R）（Span 重载，双精度）。
    /// </summary>
    public static void HC2R2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.HC2R, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行二维 DHT（两轴均为 DHT）（Span 重载，双精度）。
    /// </summary>
    public static void Dht2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.DHT, fftw_kind.DHT, flags);

    /// <summary>
    /// 执行二维 R2HC（已固定数组重载，双精度）。
    /// </summary>
    public static void R2HC2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.R2HC, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行二维 HC2R（已固定数组重载，双精度）。
    /// </summary>
    public static void HC2R2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.HC2R, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行二维 DHT（已固定数组重载，双精度）。
    /// </summary>
    public static void Dht2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.DHT, fftw_kind.DHT, flags);

    /// <summary>
    /// 执行二维 DCT-I（两轴均为 REDFT00）（Span 重载，双精度）。
    /// </summary>
    public static void DctI2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT00, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行二维 DCT-II（两轴均为 REDFT01）（Span 重载，双精度）。
    /// </summary>
    public static void DctII2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT01, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行二维 DCT-III（两轴均为 REDFT10）（Span 重载，双精度）。
    /// </summary>
    public static void DctIII2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT10, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行二维 DCT-IV（两轴均为 REDFT11）（Span 重载，双精度）。
    /// </summary>
    public static void DctIV2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT11, fftw_kind.REDFT11, flags);

    /// <summary>
    /// 执行二维 DCT-I（已固定数组重载，双精度）。
    /// </summary>
    public static void DctI2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT00, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行二维 DCT-II（已固定数组重载，双精度）。
    /// </summary>
    public static void DctII2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT01, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行二维 DCT-III（已固定数组重载，双精度）。
    /// </summary>
    public static void DctIII2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT10, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行二维 DCT-IV（已固定数组重载，双精度）。
    /// </summary>
    public static void DctIV2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT11, fftw_kind.REDFT11, flags);

    /// <summary>
    /// 执行二维 DST-I（两轴均为 RODFT00）（Span 重载，双精度）。
    /// </summary>
    public static void DstI2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT00, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行二维 DST-II（两轴均为 RODFT01）（Span 重载，双精度）。
    /// </summary>
    public static void DstII2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT01, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行二维 DST-III（两轴均为 RODFT10）（Span 重载，双精度）。
    /// </summary>
    public static void DstIII2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT10, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行二维 DST-IV（两轴均为 RODFT11）（Span 重载，双精度）。
    /// </summary>
    public static void DstIV2D(Span<double> input, Span<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT11, fftw_kind.RODFT11, flags);

    /// <summary>
    /// 执行二维 DST-I（已固定数组重载，双精度）。
    /// </summary>
    public static void DstI2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT00, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行二维 DST-II（已固定数组重载，双精度）。
    /// </summary>
    public static void DstII2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT01, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行二维 DST-III（已固定数组重载，双精度）。
    /// </summary>
    public static void DstIII2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT10, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行二维 DST-IV（已固定数组重载，双精度）。
    /// </summary>
    public static void DstIV2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT11, fftw_kind.RODFT11, flags);

    #endregion

    #region 3D 友好别名

    /// <summary>
    /// 执行三维 R2HC（各轴均为 R2HC）（Span 重载，双精度）。
    /// </summary>
    public static void R2HC3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.R2HC, fftw_kind.R2HC, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行三维 HC2R（各轴均为 HC2R）（Span 重载，双精度）。
    /// </summary>
    public static void HC2R3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.HC2R, fftw_kind.HC2R, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行三维 DHT（各轴均为 DHT）（Span 重载，双精度）。
    /// </summary>
    public static void Dht3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.DHT, fftw_kind.DHT, fftw_kind.DHT, flags);

    /// <summary>
    /// 执行三维 R2HC（已固定数组重载，双精度）。
    /// </summary>
    public static void R2HC3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.R2HC, fftw_kind.R2HC, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行三维 HC2R（已固定数组重载，双精度）。
    /// </summary>
    public static void HC2R3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.HC2R, fftw_kind.HC2R, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行三维 DHT（已固定数组重载，双精度）。
    /// </summary>
    public static void Dht3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.DHT, fftw_kind.DHT, fftw_kind.DHT, flags);

    /// <summary>
    /// 执行三维 DCT-I（各轴均为 REDFT00）（Span 重载，双精度）。
    /// </summary>
    public static void DctI3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT00, fftw_kind.REDFT00, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行三维 DCT-II（各轴均为 REDFT01）（Span 重载，双精度）。
    /// </summary>
    public static void DctII3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT01, fftw_kind.REDFT01, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行三维 DCT-III（各轴均为 REDFT10）（Span 重载，双精度）。
    /// </summary>
    public static void DctIII3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT10, fftw_kind.REDFT10, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行三维 DCT-IV（各轴均为 REDFT11）（Span 重载，双精度）。
    /// </summary>
    public static void DctIV3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT11, fftw_kind.REDFT11, fftw_kind.REDFT11, flags);

    /// <summary>
    /// 执行三维 DCT-I（已固定数组重载，双精度）。
    /// </summary>
    public static void DctI3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT00, fftw_kind.REDFT00, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行三维 DCT-II（已固定数组重载，双精度）。
    /// </summary>
    public static void DctII3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT01, fftw_kind.REDFT01, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行三维 DCT-III（已固定数组重载，双精度）。
    /// </summary>
    public static void DctIII3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT10, fftw_kind.REDFT10, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行三维 DCT-IV（已固定数组重载，双精度）。
    /// </summary>
    public static void DctIV3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT11, fftw_kind.REDFT11, fftw_kind.REDFT11, flags);

    /// <summary>
    /// 执行三维 DST-I（各轴均为 RODFT00）（Span 重载，双精度）。
    /// </summary>
    public static void DstI3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT00, fftw_kind.RODFT00, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行三维 DST-II（各轴均为 RODFT01）（Span 重载，双精度）。
    /// </summary>
    public static void DstII3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT01, fftw_kind.RODFT01, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行三维 DST-III（各轴均为 RODFT10）（Span 重载，双精度）。
    /// </summary>
    public static void DstIII3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT10, fftw_kind.RODFT10, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行三维 DST-IV（各轴均为 RODFT11）（Span 重载，双精度）。
    /// </summary>
    public static void DstIV3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT11, fftw_kind.RODFT11, fftw_kind.RODFT11, flags);

    /// <summary>
    /// 执行三维 DST-I（已固定数组重载，双精度）。
    /// </summary>
    public static void DstI3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT00, fftw_kind.RODFT00, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行三维 DST-II（已固定数组重载，双精度）。
    /// </summary>
    public static void DstII3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT01, fftw_kind.RODFT01, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行三维 DST-III（已固定数组重载，双精度）。
    /// </summary>
    public static void DstIII3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT10, fftw_kind.RODFT10, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行三维 DST-IV（已固定数组重载，双精度）。
    /// </summary>
    public static void DstIV3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT11, fftw_kind.RODFT11, fftw_kind.RODFT11, flags);

    #endregion

    #region n 维（各维度同类变换的简化别名）

    /// <summary>
    /// 执行 n 维 DCT-I（各维度均为 REDFT00）（Span 重载，双精度）。
    /// </summary>
    /// <param name="input">展平 n 维输入数据。</param>
    /// <param name="output">展平 n 维输出数据。</param>
    /// <param name="dims">各维长度。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void DctI(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT00, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-II（各维度均为 REDFT01）（Span 重载，双精度）。
    /// </summary>
    public static void DctII(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT01, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-III（各维度均为 REDFT10）（Span 重载，双精度）。
    /// </summary>
    public static void DctIII(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT10, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-IV（各维度均为 REDFT11）（Span 重载，双精度）。
    /// </summary>
    public static void DctIV(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT11, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-I（各维度均为 RODFT00）（Span 重载，双精度）。
    /// </summary>
    public static void DstI(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT00, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-II（各维度均为 RODFT01）（Span 重载，双精度）。
    /// </summary>
    public static void DstII(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT01, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-III（各维度均为 RODFT10）（Span 重载，双精度）。
    /// </summary>
    public static void DstIII(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT10, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-IV（各维度均为 RODFT11）（Span 重载，双精度）。
    /// </summary>
    public static void DstIV(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT11, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 R2HC（各维度均为 R2HC）（Span 重载，双精度）。
    /// </summary>
    public static void R2HC(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.R2HC, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 HC2R（各维度均为 HC2R）（Span 重载，双精度）。
    /// </summary>
    public static void HC2R(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.HC2R, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DHT（各维度均为 DHT）（Span 重载，双精度）。
    /// </summary>
    public static void Dht(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.DHT, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-I（各维度均为 REDFT00）（已固定数组重载，双精度）。
    /// </summary>
    public static void DctI(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT00, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-II（各维度均为 REDFT01）（已固定数组重载，双精度）。
    /// </summary>
    public static void DctII(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT01, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-III（各维度均为 REDFT10）（已固定数组重载，双精度）。
    /// </summary>
    public static void DctIII(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT10, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-IV（各维度均为 REDFT11）（已固定数组重载，双精度）。
    /// </summary>
    public static void DctIV(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT11, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-I（各维度均为 RODFT00）（已固定数组重载，双精度）。
    /// </summary>
    public static void DstI(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT00, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-II（各维度均为 RODFT01）（已固定数组重载，双精度）。
    /// </summary>
    public static void DstII(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT01, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-III（各维度均为 RODFT10）（已固定数组重载，双精度）。
    /// </summary>
    public static void DstIII(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT10, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-IV（各维度均为 RODFT11）（已固定数组重载，双精度）。
    /// </summary>
    public static void DstIV(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT11, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 R2HC（各维度均为 R2HC）（已固定数组重载，双精度）。
    /// </summary>
    public static void R2HC(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.R2HC, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 HC2R（各维度均为 HC2R）（已固定数组重载，双精度）。
    /// </summary>
    public static void HC2R(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.HC2R, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DHT（各维度均为 DHT）（已固定数组重载，双精度）。
    /// </summary>
    public static void Dht(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.DHT, dims.Length).ToArray()), flags);

    #endregion





    // ==========================
    // 原始实现（内部管线，完整保留 XML 注释）
    // ==========================

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
    internal static void R2R1D_Double(IntPtr input, IntPtr output, int n, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
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
    internal static void R2R1D(Span<double> input, Span<double> output, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (input.Length != output.Length) throw new ArgumentException("Input and output spans must have the same length.", nameof(output));
        unsafe
        {
            fixed (double* inputPtr = input)
            fixed (double* outputPtr = output)
            {
                R2R1D_Double((IntPtr)inputPtr, (IntPtr)outputPtr, input.Length, kind, flags);
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
    internal static void R2R1D(PinnableArray<double> input, PinnableArray<double> output, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        if (input.Length != output.Length) throw new ArgumentException("Input and output arrays must have the same length.", nameof(output));
        R2R1D_Double(input, output, input.Length, kind, flags);
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
    internal static void R2R2D_Double(IntPtr input, IntPtr output, int nx, int ny, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
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
    internal static void R2R2D(Span<double> input, Span<double> output, int nx, int ny, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        unsafe
        {
            fixed (double* inputPtr = input)
            fixed (double* outputPtr = output)
            {
                R2R2D_Double((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, kindx, kindy, flags);
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
    internal static void R2R2D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        R2R2D_Double(input, output, nx, ny, kindx, kindy, flags);
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
    internal static void R2R3D_Double(IntPtr input, IntPtr output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
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
    internal static void R2R3D(Span<double> input, Span<double> output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        unsafe
        {
            fixed (double* inputPtr = input)
            fixed (double* outputPtr = output)
            {
                R2R3D_Double((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, nz, kindx, kindy, kindz, flags);
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
    internal static void R2R3D(PinnableArray<double> input, PinnableArray<double> output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        R2R3D_Double(input, output, nx, ny, nz, kindx, kindy, kindz, flags);
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
    internal static void R2RND_Double(IntPtr input, IntPtr output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));

        var rank = dims.Length;

        var plan = fftw.r2r(rank, dims, input, output, kinds, flags);
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
    internal static void R2RND(Span<double> input, Span<double> output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));
        unsafe
        {
            fixed (double* inputPtr = input)
            fixed (double* outputPtr = output)
            {
                R2RND_Double((IntPtr)inputPtr, (IntPtr)outputPtr, dims, kinds, flags);
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
    internal static void R2RND(PinnableArray<double> input, PinnableArray<double> output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));
        R2RND_Double(input, output, dims, kinds, flags);
    }

    #endregion
}