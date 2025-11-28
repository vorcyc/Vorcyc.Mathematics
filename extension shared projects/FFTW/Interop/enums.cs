namespace Vorcyc.Mathematics.Extensions.FFTW.Interop;


/// <summary>
/// FFTW planner flags
/// </summary>
[Flags]
public enum fftw_flags : uint
{
    /// <summary>
    /// Tells FFTW to find an optimized plan by actually computing several FFTs and measuring their execution time. 
    /// Depending on your machine, this can take some time (often a few seconds). Default (0x0). 
    /// </summary>
    Measure = 0,
    /// <summary>
    /// Specifies that an out-of-place transform is allowed to overwrite its 
    /// input array with arbitrary data; this can sometimes allow more efficient algorithms to be employed.
    /// </summary>
    DestroyInput = 1,
    /// <summary>
    /// Rarely used. Specifies that the algorithm may not impose any unusual alignment requirements on the input/output 
    /// arrays (i.e. no SIMD). This flag is normally not necessary, since the planner automatically detects 
    /// misaligned arrays. The only use for this flag is if you want to use the guru interface to execute a given 
    /// plan on a different array that may not be aligned like the original. 
    /// </summary>
    Unaligned = 2,
    /// <summary>
    /// Not used.
    /// </summary>
    ConserveMemory = 4,
    /// <summary>
    /// Like Patient, but considers an even wider range of algorithms, including many that we think are 
    /// unlikely to be fast, to produce the most optimal plan but with a substantially increased planning time. 
    /// </summary>
    Exhaustive = 8,
    /// <summary>
    /// Specifies that an out-of-place transform must not change its input array. 
    /// </summary>
    /// <remarks>
    /// This is ordinarily the default, 
    /// except for c2r and hc2r (i.e. complex-to-real) transforms for which DestroyInput is the default. 
    /// In the latter cases, passing PreserveInput will attempt to use algorithms that do not destroy the 
    /// input, at the expense of worse performance; for multi-dimensional c2r transforms, however, no 
    /// input-preserving algorithms are implemented and the planner will return null if one is requested.
    /// </remarks>
    PreserveInput = 16,
    /// <summary>
    /// Like Measure, but considers a wider range of algorithms and often produces a 搈ore optimal?plan 
    /// (especially for large transforms), but at the expense of several times longer planning time 
    /// (especially for large transforms).
    /// </summary>
    Patient = 32,
    /// <summary>
    /// Specifies that, instead of actual measurements of different algorithms, a simple heuristic is 
    /// used to pick a (probably sub-optimal) plan quickly. With this flag, the input/output arrays 
    /// are not overwritten during planning. 
    /// </summary>
    Estimate = 64
}

/// <summary>
/// Defines direction of operation
/// </summary>
internal enum fftw_direction : int
{
    /// <summary>
    /// Computes a regular DFT
    /// </summary>
    Forward = -1,
    /// <summary>
    /// Computes the inverse DFT
    /// </summary>
    Backward = 1
}

/// <summary>
/// 实数到实数 (Real-to-Real) 变换类型枚举。对应 FFTW 的各类 R2R 变换基类型。
/// </summary>
/// <remarks>
/// 这些类型用于指定实数序列的不同正交/半正交变换：
/// - R2HC/HC2R：实数与半复数谱 (half-complex) 的互转（紧凑存储的余弦/正弦系数对）。<br/>
/// - DHT：离散哈特利变换 (Discrete Hartley Transform)。<br/>
/// - REDFT**：各型离散余弦变换 (DCT)。<br/>
/// - RODFT**：各型离散正弦变换 (DST)。<br/>
/// 具体边界条件与采样点约定由 **00/01/10/11** 后缀区分。
/// </remarks>
internal enum fftw_kind : uint
{
    /// <summary>
    /// 实数到半复数 (Real-to-Half-Complex) 变换。
    /// 将实数序列映射为紧凑的半复数谱表示，包含余弦/正弦分量的交织存储。
    /// </summary>
    R2HC = 0,

    /// <summary>
    /// 半复数到实数 (Half-Complex-to-Real) 变换。
    /// 将紧凑的半复数谱恢复为实数序列，通常与 <see cref="R2HC"/> 成对使用。
    /// </summary>
    HC2R = 1,

    /// <summary>
    /// 离散哈特利变换 (DHT)。
    /// 使用哈特利核 (cas(x) = cos(x) + sin(x)) 的实数—实数变换，频谱为实值。
    /// </summary>
    DHT = 2,

    /// <summary>
    /// 离散余弦变换 DCT-I (REDFT00)。
    /// 适用于两端点都参与且采用对称边界条件的场景（通常用于特定 PDE/边值问题）。
    /// </summary>
    REDFT00 = 3,

    /// <summary>
    /// 离散余弦变换 DCT-II (REDFT01)。
    /// 最常见的 DCT 类型之一，广泛用于信号压缩（如图像/音频的能量集中）。
    /// </summary>
    REDFT01 = 4,

    /// <summary>
    /// 离散余弦变换 DCT-III (REDFT10)。
    /// 为 DCT-II 的逆变换（在特定归一化下），常用于重建或逆处理。
    /// </summary>
    REDFT10 = 5,

    /// <summary>
    /// 离散余弦变换 DCT-IV (REDFT11)。
    /// 采用对称且无端点重复的采样，常用于某些滤波与谱分析场景。
    /// </summary>
    REDFT11 = 6,

    /// <summary>
    /// 离散正弦变换 DST-I (RODFT00)。
    /// 两端为奇延拓边界条件，适用于特定边值问题的谱方法。
    /// </summary>
    RODFT00 = 7,

    /// <summary>
    /// 离散正弦变换 DST-II (RODFT01)。
    /// 常见的 DST 类型之一，与相应的余弦变换在应用中互补。
    /// </summary>
    RODFT01 = 8,

    /// <summary>
    /// 离散正弦变换 DST-III (RODFT10)。
    /// 为 DST-II 的逆（在特定归一化下），用于重建或逆处理。
    /// </summary>
    RODFT10 = 9,

    /// <summary>
    /// 离散正弦变换 DST-IV (RODFT11)。
    /// 对称采样且无端点重复，适用于某些谱分析与滤波场景。
    /// </summary>
    RODFT11 = 10
}