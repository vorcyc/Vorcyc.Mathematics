namespace Vorcyc.Mathematics.Framework;

/// <summary>
/// 表示滤波器的类型。
/// </summary>
public enum FilterType
{
    /// <summary>
    /// 无类型。
    /// </summary>
    None,
    /// <summary>
    /// 低通滤波器。
    /// </summary>
    LowPass,
    /// <summary>
    /// 高通滤波器。
    /// </summary>
    HighPass,
    /// <summary>
    /// 带通滤波器。
    /// </summary>
    BandPass,
    /// <summary>
    /// 带阻滤波器。
    /// </summary>
    BandStop,
    /// <summary>
    /// 全通滤波器。
    /// </summary>
    AllPass,
    /// <summary>
    /// 陷波滤波器。
    /// </summary>
    Notch,
    /// <summary>
    /// 峰值滤波器。
    /// </summary>
    Peak,
    /// <summary>
    /// 低架滤波器。
    /// </summary>
    LowShelf,
    /// <summary>
    /// 高架滤波器。
    /// </summary>
    HighShelf
}

/// <summary>
/// 表示滤波器的设计类型。
/// </summary>
public enum FilterDesign
{
    /// <summary>
    /// 无设计。
    /// </summary>
    None,
    /// <summary>
    /// 切比雪夫滤波器。
    /// </summary>
    Chebyshev,
    /// <summary>
    /// 巴特沃兹滤波器。
    /// </summary>
    Butterworth,
    /// <summary>
    /// 贝塞尔滤波器。
    /// </summary>
    Bessel,
    /// <summary>
    /// 椭圆滤波器。
    /// </summary>
    Elliptic,
    /// <summary>
    /// 高斯滤波器。
    /// </summary>
    Gaussian,
    /// <summary>
    /// 勒让德滤波器。
    /// </summary>
    Legendre,
    /// <summary>
    /// 反切比雪夫滤波器。
    /// </summary>
    InverseChebyshev,
    /// <summary>
    /// 椭圆滤波器（Cauer）。
    /// </summary>
    Cauer,
    /// <summary>
    /// 汤姆森滤波器。
    /// </summary>
    Thomson,
    /// <summary>
    /// 帕普利斯滤波器。
    /// </summary>
    Papoulis,
    /// <summary>
    /// Linkwitz-Riley滤波器。
    /// </summary>
    LinkwitzRiley,
    /// <summary>
    /// 升余弦滤波器。
    /// </summary>
    RaisedCosine,
    /// <summary>
    /// 卡尔曼滤波器。
    /// </summary>
    Kalman
}

/// <summary>
/// 表示滤波器的结构类型。
/// </summary>
public enum FilterStructure
{
    /// <summary>
    /// 无结构。
    /// </summary>
    None,
    /// <summary>
    /// 直接形式I滤波器。
    /// </summary>
    DirectFormI,
    /// <summary>
    /// 直接形式II滤波器。
    /// </summary>
    DirectFormII,
    /// <summary>
    /// 转置直接形式II滤波器。
    /// </summary>
    TransposedDirectFormII,
    /// <summary>
    /// 二阶节（BiQuad）滤波器。
    /// </summary>
    BiQuad,
    /// <summary>
    /// 格型滤波器。
    /// </summary>
    Lattice,
    /// <summary>
    /// 级联滤波器。
    /// </summary>
    Cascade,
    /// <summary>
    /// 并联滤波器。
    /// </summary>
    Parallel,
    /// <summary>
    /// 卡尔曼滤波器。
    /// </summary>
    Kalman
}

/// <summary>
/// 用于标记滤波器类的特性。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class FilterAttribute : Attribute
{
    /// <summary>
    /// 获取滤波器的类型。
    /// </summary>
    public FilterType Type { get; }

    /// <summary>
    /// 获取滤波器的设计类型。
    /// </summary>
    public FilterDesign Design { get; }

    /// <summary>
    /// 获取滤波器的结构类型。
    /// </summary>
    public FilterStructure Structure { get; }

    /// <summary>
    /// 获取滤波器的描述。
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 初始化 <see cref="FilterAttribute"/> 类的新实例。
    /// </summary>
    /// <param name="type">滤波器的类型。</param>
    /// <param name="design">滤波器的设计类型。</param>
    /// <param name="structure">滤波器的结构类型。</param>
    /// <param name="description">滤波器的描述。</param>
    public FilterAttribute(FilterType type = FilterType.None, FilterDesign design = FilterDesign.None, FilterStructure structure = FilterStructure.None, string description = "")
    {
        Type = type;
        Design = design;
        Structure = structure;
        Description = description;
    }
}
