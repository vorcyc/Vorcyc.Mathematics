namespace Vorcyc.Mathematics.Framework;

/// <summary>
/// 表示基于频率响应特性的滤波器类型。
/// </summary>
public enum FilterResponseType
{
    /// <summary>
    /// 低通滤波器：允许低频通过，阻止高频。
    /// </summary>
    LowPass,
    /// <summary>
    /// 高通滤波器：允许高频通过，阻止低频。
    /// </summary>
    HighPass,
    /// <summary>
    /// 带阻滤波器：阻止特定频率范围内的信号通过，允许其他频率通过。
    /// </summary>
    BandStop,
    /// <summary>
    /// 带通滤波器：允许特定频率范围内的信号通过，阻止其他频率通过。
    /// </summary>
    BandPass,
    /// <summary>
    /// 全通滤波器：允许所有频率通过，但改变相位。
    /// </summary>
    AllPass,
    /// <summary>
    /// 陷波滤波器：衰减一个窄频带内的信号，允许其他频率通过。
    /// </summary>
    Notch,
    /// <summary>
    /// 峰值滤波器：提升或衰减一个窄频带内的信号。
    /// </summary>
    Peak,
    /// <summary>
    /// 低架滤波器：提升或衰减低于某个截止频率的频率。
    /// </summary>
    LowShelf,
    /// <summary>
    /// 高架滤波器：提升或衰减高于某个截止频率的频率。
    /// </summary>
    HighShelf
}

/// <summary>
/// 表示基于设计方法的滤波器类型。
/// </summary>
public enum FilterDesignMethod
{
    /// <summary>
    /// 自适应滤波器：根据输入信号特性动态调整参数。
    /// </summary>
    Adaptive,
    /// <summary>
    /// 双二阶滤波器：由两个二阶滤波器级联而成。
    /// </summary>
    BiQuad,
    /// <summary>
    /// 单极滤波器：使用一个极点的简单滤波器。
    /// </summary>
    OnePole,
    /// <summary>
    /// 多相滤波器：用于多相信号处理，提高计算效率。
    /// </summary>
    Polyphase,
    /// <summary>
    /// 标准卡尔曼滤波器：适用于线性系统。
    /// </summary>
    StandardKalman,
    /// <summary>
    /// 扩展卡尔曼滤波器：适用于非线性系统。
    /// </summary>
    ExtendedKalman,
    /// <summary>
    /// 无迹卡尔曼滤波器：适用于非线性系统。
    /// </summary>
    UnscentedKalman,
    /// <summary>
    /// 平方根卡尔曼滤波器：提高数值稳定性和计算效率。
    /// </summary>
    SquareRootKalman,
    /// <summary>
    /// 信息滤波器：使用信息矩阵表示不确定性。
    /// </summary>
    InformationFilter,
    /// <summary>
    /// 粒子滤波器：适用于高度非线性和非高斯分布的系统。
    /// </summary>
    ParticleFilter
}

/// <summary>
/// 滤波器特性。
/// </summary>
public enum FilterCharacteristic
{
    /// <summary>
    /// 贝塞尔滤波器：线性相位响应，保持波形形状。
    /// </summary>
    Bessel,
    /// <summary>
    /// 巴特沃斯滤波器：平坦的频率响应，无波纹。
    /// </summary>
    Butterworth,
    /// <summary>
    /// 切比雪夫I型滤波器：通带内有波纹，但阻带内衰减更快。
    /// </summary>
    ChebyshevI,
    /// <summary>
    /// 切比雪夫II型滤波器：阻带内有波纹，但通带内更平坦。
    /// </summary>
    ChebyshevII,
    /// <summary>
    /// 椭圆滤波器：通带和阻带内都有波纹，但具有最陡峭的过渡带。
    /// </summary>
    Elliptic,
    /// <summary>
    /// 勒让德滤波器：基于勒让德多项式，通带响应平坦。
    /// </summary>
    Legendre,
    /// <summary>
    /// 卡尔曼滤波器：用于估计动态系统的状态。
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
    public FilterResponseType ResponseType { get; }

    /// <summary>
    /// 获取滤波器的设计类型。
    /// </summary>
    public FilterDesignMethod Design { get; }

    /// <summary>
    /// 获取滤波器的结构类型。
    /// </summary>
    public FilterCharacteristic Characteristic { get; }

    /// <summary>
    /// 获取滤波器的描述。
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 初始化 <see cref="FilterAttribute"/> 类的新实例。
    /// </summary>
    /// <param name="responseType">滤波器的类型。</param>
    /// <param name="design">滤波器的设计类型。</param>
    /// <param name="characteristic">滤波器的结构类型。</param>
    /// <param name="description">滤波器的描述。</param>
    public FilterAttribute(FilterResponseType responseType , FilterDesignMethod design , FilterCharacteristic characteristic, string description = "")
    {
        ResponseType = responseType;
        Design = design;
        Characteristic = characteristic;
        Description = description;
    }
}
