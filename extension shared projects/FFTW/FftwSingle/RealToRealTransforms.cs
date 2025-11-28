using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Extensions.FFTW.Interop;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// 基于 FFTW 单精度接口的实数到实数 (R2R) 变换帮助类。
/// 封装了一维、二维、三维及 n 维 R2R 变换的指针、Span 与已固定数组重载。
/// 增加基于 <see cref="fftw_kind"/> 的友好别名方法，并将原始底层实现改为 internal。
/// </summary>
/// <remarks>
/// - 本类方法不执行归一化；如需 1/N、1/(nx*ny) 或 1/∏dims 等缩放，请在外部完成。<br/>
/// - 支持原地与非原地调用，但是否允许原地取决于具体 <see cref="fftw_kind"/> 与 FFTW 的限制。<br/>
/// - 输入/输出缓冲区需为连续内存且大小足够；已固定重载要求缓冲区处于 Pinned 状态。<br/>
/// - 多维接口要求 <c>dims</c> 与 <c>kinds</c> 长度一致；计划创建失败将抛出 <see cref="InvalidOperationException"/>。<br/>
/// - 方法内部会创建、执行并销毁 FFTW 计划；调用期间应确保对缓冲区的独占访问以避免数据竞争。<br/>
/// </remarks>
public static partial class RealToRealTransforms
{
    // ==========================
    // 友好公共 API（基于 fftw_kind 的别名）
    // ==========================

    #region R2HC / HC2R / DHT - 1D

    /// <summary>
    /// 执行一维实数到半复数谱 (R2HC) 变换（Span 重载）。
    /// </summary>
    /// <remarks>
    /// - 不执行归一化；如需缩放请在外部完成。<br/>
    /// - 输入与输出长度必须相同；是否支持原地取决于 FFTW 限制。
    /// </remarks>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出半复数紧凑谱（以实数形式存放）。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void R2HC1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行一维半复数谱到实数 (HC2R) 变换（Span 重载）。
    /// </summary>
    /// <remarks>
    /// - 输出为实数序列；不执行归一化。<br/>
    /// - 输入与输出长度必须相同。
    /// </remarks>
    /// <param name="input">输入半复数紧凑谱（以实数形式存放）。</param>
    /// <param name="output">输出实数序列。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void HC2R1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行一维离散哈特利变换 (DHT)（Span 重载）。
    /// </summary>
    /// <remarks>
    /// - 使用 cas(x)=cos(x)+sin(x) 核；不执行归一化。<br/>
    /// - 输入与输出长度必须相同。
    /// </remarks>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出哈特利谱（实数）。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void Dht1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.DHT, flags);

    /// <summary>
    /// 执行一维实数到半复数谱 (R2HC) 变换（已固定数组重载）。
    /// </summary>
    /// <param name="input">已固定的输入数组。</param>
    /// <param name="output">已固定的输出数组。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void R2HC1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行一维半复数谱到实数 (HC2R) 变换（已固定数组重载）。
    /// </summary>
    /// <param name="input">已固定的输入数组（半复数紧凑谱）。</param>
    /// <param name="output">已固定的输出数组（实数序列）。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void HC2R1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行一维离散哈特利变换 (DHT)（已固定数组重载）。
    /// </summary>
    /// <param name="input">已固定的输入数组。</param>
    /// <param name="output">已固定的输出数组。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void Dht1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.DHT, flags);

    #endregion

    #region DCT (REDFT**) - 1D

    /// <summary>
    /// 执行一维 DCT-I (REDFT00)（Span 重载）。
    /// </summary>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出 DCT-I 结果。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void DctI1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行一维 DCT-II (REDFT01)（Span 重载）。
    /// </summary>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出 DCT-II 结果。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void DctII1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行一维 DCT-III (REDFT10)（Span 重载）。
    /// </summary>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出 DCT-III 结果。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void DctIII1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行一维 DCT-IV (REDFT11)（Span 重载）。
    /// </summary>
    /// <param name="input">输入实数序列。</param>
    /// <param name="output">输出 DCT-IV 结果。</param>
    /// <param name="flags">FFTW 规划标志。</param>
    public static void DctIV1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT11, flags);

    /// <summary>
    /// 执行一维 DCT-I (REDFT00)（已固定数组重载）。
    /// </summary>
    public static void DctI1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行一维 DCT-II (REDFT01)（已固定数组重载）。
    /// </summary>
    public static void DctII1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行一维 DCT-III (REDFT10)（已固定数组重载）。
    /// </summary>
    public static void DctIII1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行一维 DCT-IV (REDFT11)（已固定数组重载）。
    /// </summary>
    public static void DctIV1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.REDFT11, flags);

    #endregion

    #region DST (RODFT**) - 1D

    /// <summary>
    /// 执行一维 DST-I (RODFT00)（Span 重载）。
    /// </summary>
    public static void DstI1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行一维 DST-II (RODFT01)（Span 重载）。
    /// </summary>
    public static void DstII1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行一维 DST-III (RODFT10)（Span 重载）。
    /// </summary>
    public static void DstIII1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行一维 DST-IV (RODFT11)（Span 重载）。
    /// </summary>
    public static void DstIV1D(Span<float> input, Span<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT11, flags);

    /// <summary>
    /// 执行一维 DST-I (RODFT00)（已固定数组重载）。
    /// </summary>
    public static void DstI1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行一维 DST-II (RODFT01)（已固定数组重载）。
    /// </summary>
    public static void DstII1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行一维 DST-III (RODFT10)（已固定数组重载）。
    /// </summary>
    public static void DstIII1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行一维 DST-IV (RODFT11)（已固定数组重载）。
    /// </summary>
    public static void DstIV1D(PinnableArray<float> input, PinnableArray<float> output, fftw_flags flags = fftw_flags.Estimate)
        => R2R1D(input, output, fftw_kind.RODFT11, flags);

    #endregion

    #region 2D 友好别名

    /// <summary>
    /// 执行二维 R2HC（两轴均为 R2HC）（Span 重载）。
    /// </summary>
    /// <param name="nx">X 维长度。</param>
    /// <param name="ny">Y 维长度。</param>
    public static void R2HC2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.R2HC, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行二维 HC2R（两轴均为 HC2R）（Span 重载）。
    /// </summary>
    public static void HC2R2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.HC2R, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行二维 DHT（两轴均为 DHT）（Span 重载）。
    /// </summary>
    public static void Dht2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.DHT, fftw_kind.DHT, flags);

    /// <summary>
    /// 执行二维 R2HC（已固定数组重载）。
    /// </summary>
    public static void R2HC2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.R2HC, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行二维 HC2R（已固定数组重载）。
    /// </summary>
    public static void HC2R2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.HC2R, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行二维 DHT（已固定数组重载）。
    /// </summary>
    public static void Dht2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.DHT, fftw_kind.DHT, flags);

    /// <summary>
    /// 执行二维 DCT-I（两轴均为 REDFT00）（Span 重载）。
    /// </summary>
    public static void DctI2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT00, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行二维 DCT-II（两轴均为 REDFT01）（Span 重载）。
    /// </summary>
    public static void DctII2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT01, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行二维 DCT-III（两轴均为 REDFT10）（Span 重载）。
    /// </summary>
    public static void DctIII2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT10, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行二维 DCT-IV（两轴均为 REDFT11）（Span 重载）。
    /// </summary>
    public static void DctIV2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT11, fftw_kind.REDFT11, flags);

    /// <summary>
    /// 执行二维 DCT-I（已固定数组重载）。
    /// </summary>
    public static void DctI2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT00, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行二维 DCT-II（已固定数组重载）。
    /// </summary>
    public static void DctII2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT01, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行二维 DCT-III（已固定数组重载）。
    /// </summary>
    public static void DctIII2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT10, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行二维 DCT-IV（已固定数组重载）。
    /// </summary>
    public static void DctIV2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.REDFT11, fftw_kind.REDFT11, flags);

    /// <summary>
    /// 执行二维 DST-I（两轴均为 RODFT00）（Span 重载）。
    /// </summary>
    public static void DstI2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT00, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行二维 DST-II（两轴均为 RODFT01）（Span 重载）。
    /// </summary>
    public static void DstII2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT01, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行二维 DST-III（两轴均为 RODFT10）（Span 重载）。
    /// </summary>
    public static void DstIII2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT10, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行二维 DST-IV（两轴均为 RODFT11）（Span 重载）。
    /// </summary>
    public static void DstIV2D(Span<float> input, Span<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT11, fftw_kind.RODFT11, flags);

    /// <summary>
    /// 执行二维 DST-I（已固定数组重载）。
    /// </summary>
    public static void DstI2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT00, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行二维 DST-II（已固定数组重载）。
    /// </summary>
    public static void DstII2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT01, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行二维 DST-III（已固定数组重载）。
    /// </summary>
    public static void DstIII2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT10, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行二维 DST-IV（已固定数组重载）。
    /// </summary>
    public static void DstIV2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_flags flags = fftw_flags.Estimate)
        => R2R2D(input, output, nx, ny, fftw_kind.RODFT11, fftw_kind.RODFT11, flags);

    #endregion

    #region 3D 友好别名

    /// <summary>
    /// 执行三维 R2HC（各轴均为 R2HC）（Span 重载）。
    /// </summary>
    public static void R2HC3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.R2HC, fftw_kind.R2HC, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行三维 HC2R（各轴均为 HC2R）（Span 重载）。
    /// </summary>
    public static void HC2R3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.HC2R, fftw_kind.HC2R, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行三维 DHT（各轴均为 DHT）（Span 重载）。
    /// </summary>
    public static void Dht3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.DHT, fftw_kind.DHT, fftw_kind.DHT, flags);

    /// <summary>
    /// 执行三维 R2HC（已固定数组重载）。
    /// </summary>
    public static void R2HC3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.R2HC, fftw_kind.R2HC, fftw_kind.R2HC, flags);

    /// <summary>
    /// 执行三维 HC2R（已固定数组重载）。
    /// </summary>
    public static void HC2R3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.HC2R, fftw_kind.HC2R, fftw_kind.HC2R, flags);

    /// <summary>
    /// 执行三维 DHT（已固定数组重载）。
    /// </summary>
    public static void Dht3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.DHT, fftw_kind.DHT, fftw_kind.DHT, flags);

    /// <summary>
    /// 执行三维 DCT-I（各轴均为 REDFT00）（Span 重载）。
    /// </summary>
    public static void DctI3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT00, fftw_kind.REDFT00, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行三维 DCT-II（各轴均为 REDFT01）（Span 重载）。
    /// </summary>
    public static void DctII3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT01, fftw_kind.REDFT01, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行三维 DCT-III（各轴均为 REDFT10）（Span 重载）。
    /// </summary>
    public static void DctIII3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT10, fftw_kind.REDFT10, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行三维 DCT-IV（各轴均为 REDFT11）（Span 重载）。
    /// </summary>
    public static void DctIV3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT11, fftw_kind.REDFT11, fftw_kind.REDFT11, flags);

    /// <summary>
    /// 执行三维 DCT-I（已固定数组重载）。
    /// </summary>
    public static void DctI3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT00, fftw_kind.REDFT00, fftw_kind.REDFT00, flags);

    /// <summary>
    /// 执行三维 DCT-II（已固定数组重载）。
    /// </summary>
    public static void DctII3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT01, fftw_kind.REDFT01, fftw_kind.REDFT01, flags);

    /// <summary>
    /// 执行三维 DCT-III（已固定数组重载）。
    /// </summary>
    public static void DctIII3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT10, fftw_kind.REDFT10, fftw_kind.REDFT10, flags);

    /// <summary>
    /// 执行三维 DCT-IV（已固定数组重载）。
    /// </summary>
    public static void DctIV3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.REDFT11, fftw_kind.REDFT11, fftw_kind.REDFT11, flags);

    /// <summary>
    /// 执行三维 DST-I（各轴均为 RODFT00）（Span 重载）。
    /// </summary>
    public static void DstI3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT00, fftw_kind.RODFT00, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行三维 DST-II（各轴均为 RODFT01）（Span 重载）。
    /// </summary>
    public static void DstII3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT01, fftw_kind.RODFT01, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行三维 DST-III（各轴均为 RODFT10）（Span 重载）。
    /// </summary>
    public static void DstIII3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT10, fftw_kind.RODFT10, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行三维 DST-IV（各轴均为 RODFT11）（Span 重载）。
    /// </summary>
    public static void DstIV3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT11, fftw_kind.RODFT11, fftw_kind.RODFT11, flags);

    /// <summary>
    /// 执行三维 DST-I（已固定数组重载）。
    /// </summary>
    public static void DstI3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT00, fftw_kind.RODFT00, fftw_kind.RODFT00, flags);

    /// <summary>
    /// 执行三维 DST-II（已固定数组重载）。
    /// </summary>
    public static void DstII3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT01, fftw_kind.RODFT01, fftw_kind.RODFT01, flags);

    /// <summary>
    /// 执行三维 DST-III（已固定数组重载）。
    /// </summary>
    public static void DstIII3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT10, fftw_kind.RODFT10, fftw_kind.RODFT10, flags);

    /// <summary>
    /// 执行三维 DST-IV（已固定数组重载）。
    /// </summary>
    public static void DstIV3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_flags flags = fftw_flags.Estimate)
        => R2R3D(input, output, nx, ny, nz, fftw_kind.RODFT11, fftw_kind.RODFT11, fftw_kind.RODFT11, flags);

    #endregion

    #region n 维（各维度同类变换的简化别名）

    /// <summary>
    /// 执行 n 维 DCT-I（各维度均为 REDFT00）（Span 重载）。
    /// </summary>
    /// <param name="dims">各维长度。</param>
    public static void DctI(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT00, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-II（各维度均为 REDFT01）（Span 重载）。
    /// </summary>
    public static void DctII(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT01, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-III（各维度均为 REDFT10）（Span 重载）。
    /// </summary>
    public static void DctIII(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT10, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-IV（各维度均为 REDFT11）（Span 重载）。
    /// </summary>
    public static void DctIV(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT11, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-I（各维度均为 RODFT00）（Span 重载）。
    /// </summary>
    public static void DstI(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT00, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-II（各维度均为 RODFT01）（Span 重载）。
    /// </summary>
    public static void DstII(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT01, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-III（各维度均为 RODFT10）（Span 重载）。
    /// </summary>
    public static void DstIII(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT10, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-IV（各维度均为 RODFT11）（Span 重载）。
    /// </summary>
    public static void DstIV(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT11, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 R2HC（各维度均为 R2HC）（Span 重载）。
    /// </summary>
    public static void R2HC(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.R2HC, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 HC2R（各维度均为 HC2R）（Span 重载）。
    /// </summary>
    public static void HC2R(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.HC2R, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DHT（各维度均为 DHT）（Span 重载）。
    /// </summary>
    public static void Dht(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.DHT, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-I（各维度均为 REDFT00）（已固定数组重载）。
    /// </summary>
    public static void DctI(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT00, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-II（各维度均为 REDFT01）（已固定数组重载）。
    /// </summary>
    public static void DctII(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT01, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-III（各维度均为 REDFT10）（已固定数组重载）。
    /// </summary>
    public static void DctIII(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT10, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DCT-IV（各维度均为 REDFT11）（已固定数组重载）。
    /// </summary>
    public static void DctIV(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.REDFT11, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-I（各维度均为 RODFT00）（已固定数组重载）。
    /// </summary>
    public static void DstI(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT00, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-II（各维度均为 RODFT01）（已固定数组重载）。
    /// </summary>
    public static void DstII(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT01, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-III（各维度均为 RODFT10）（已固定数组重载）。
    /// </summary>
    public static void DstIII(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT10, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DST-IV（各维度均为 RODFT11）（已固定数组重载）。
    /// </summary>
    public static void DstIV(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.RODFT11, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 R2HC（各维度均为 R2HC）（已固定数组重载）。
    /// </summary>
    public static void R2HC(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.R2HC, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 HC2R（各维度均为 HC2R）（已固定数组重载）。
    /// </summary>
    public static void HC2R(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>(Enumerable.Repeat(fftw_kind.HC2R, dims.Length).ToArray()), flags);

    /// <summary>
    /// 执行 n 维 DHT（各维度均为 DHT）（已固定数组重载）。
    /// </summary>
    public static void Dht(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, fftw_flags flags = fftw_flags.Estimate)
        => R2RND(input, output, dims, new ReadOnlySpan<fftw_kind>([.. Enumerable.Repeat(fftw_kind.DHT, dims.Length)]), flags);

    #endregion











    // ==========================
    // 原始实现（内部管线，设为 internal）
    // ==========================

    #region Real-to-real (1D, 2D, 3D, n-D)

    /// <summary>
    /// 执行一维实数到实数 (R2R) 变换（支持原地/非原地，取决于 <paramref name="kind"/> 及 FFTW 限制）。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="output"/> 为零指针时抛出。</exception>
    /// <exception cref="InvalidOperationException">当规划(plan)创建失败时抛出。</exception>
    internal static void R2R1D_Single(IntPtr input, IntPtr output, int n, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
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
    internal static void R2R1D(Span<float> input, Span<float> output, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (input.Length != output.Length) throw new ArgumentException("Input and output spans must have the same length.", nameof(output));
        unsafe
        {
            fixed (float* inputPtr = input)
            fixed (float* outputPtr = output)
            {
                R2R1D_Single((IntPtr)inputPtr, (IntPtr)outputPtr, input.Length, kind, flags);
            }
        }
    }

    /// <summary>
    /// 执行一维实数到实数 (R2R) 变换（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    internal static void R2R1D(PinnableArray<float> input, PinnableArray<float> output, fftw_kind kind, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        if (input.Length != output.Length) throw new ArgumentException("Input and output arrays must have the same length.", nameof(output));
        R2R1D_Single(input, output, input.Length, kind, flags);
    }

    #endregion

    #region 2D

    /// <summary>
    /// 执行二维实数到实数 (R2R) 变换（指针重载）。
    /// </summary>
    internal static void R2R2D_Single(IntPtr input, IntPtr output, int nx, int ny, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
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
    internal static void R2R2D(Span<float> input, Span<float> output, int nx, int ny, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        unsafe
        {
            fixed (float* inputPtr = input)
            fixed (float* outputPtr = output)
            {
                R2R2D_Single((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, kindx, kindy, flags);
            }
        }
    }

    /// <summary>
    /// 执行二维实数到实数 (R2R) 变换（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    internal static void R2R2D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, fftw_kind kindx, fftw_kind kindy, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        R2R2D_Single(input, output, nx, ny, kindx, kindy, flags);
    }

    #endregion

    #region 3D

    /// <summary>
    /// 执行三维实数到实数 (R2R) 变换（指针重载）。
    /// </summary>
    internal static void R2R3D_Single(IntPtr input, IntPtr output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
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
    internal static void R2R3D(Span<float> input, Span<float> output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        unsafe
        {
            fixed (float* inputPtr = input)
            fixed (float* outputPtr = output)
            {
                R2R3D_Single((IntPtr)inputPtr, (IntPtr)outputPtr, nx, ny, nz, kindx, kindy, kindz, flags);
            }
        }
    }

    /// <summary>
    /// 执行三维实数到实数 (R2R) 变换（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    internal static void R2R3D(PinnableArray<float> input, PinnableArray<float> output, int nx, int ny, int nz, fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        R2R3D_Single(input, output, nx, ny, nz, kindx, kindy, kindz, flags);
    }

    #endregion

    #region ND

    /// <summary>
    /// 执行 n 维实数到实数 (R2R) 变换（支持原地/非原地，具体取决于各维度的 <paramref name="kinds"/> 及 FFTW 限制）。
    /// </summary>
    internal static void R2RND_Single(IntPtr input, IntPtr output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfZero(input);
        ArgumentNullException.ThrowIfZero(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));

        var rank = dims.Length;
        var plan = fftwf.r2r(rank, dims, input, output, kinds, flags);
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
    internal static void R2RND(Span<float> input, Span<float> output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));
        unsafe
        {
            fixed (float* inputPtr = input)
            fixed (float* outputPtr = output)
            {
                R2RND_Single((IntPtr)inputPtr, (IntPtr)outputPtr, dims, kinds, flags);
            }
        }
    }

    /// <summary>
    /// 执行 n 维实数到实数 (R2R) 变换（已固定的 <see cref="PinnableArray{T}"/> 重载）。
    /// </summary>
    internal static void R2RND(PinnableArray<float> input, PinnableArray<float> output, ReadOnlySpan<int> dims, ReadOnlySpan<fftw_kind> kinds, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        if (dims.Length != kinds.Length) throw new ArgumentException("dims.Length must equal kinds.Length.", nameof(kinds));
        R2RND_Single(input, output, dims, kinds, flags);
    }

    #endregion
}