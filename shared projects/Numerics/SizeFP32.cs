using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Vorcyc.Mathematics.Numerics;

/// <summary>
/// 表示一个具有浮点数宽度和高度的二维尺寸结构体。
/// </summary>
public readonly struct SizeFp32
{
    /// <summary>
    /// 表示一个宽度和高度均为零的 <see cref="SizeFp32"/> 结构体。
    /// </summary>
    public readonly static SizeFp32 Empty = new SizeFp32(0, 0);

    /// <summary>
    /// 获取宽度。
    /// </summary>
    public float Width { get; }

    /// <summary>
    /// 获取高度。
    /// </summary>
    public float Height { get; }

    /// <summary>
    /// 获取一个值，该值指示此 <see cref="SizeFp32"/> 是否为空。
    /// </summary>
    [Browsable(false)]
    public bool IsEmpty => Width == 0f && Height == 0f;

    /// <summary>
    /// 初始化 <see cref="SizeFp32"/> 结构体的新实例，该实例具有指定的尺寸。
    /// </summary>
    /// <param name="size">要复制的 <see cref="SizeFp32"/> 实例。</param>
    public SizeFp32(SizeFp32 size)
    {
        Width = size.Width;
        Height = size.Height;
    }

    /// <summary>
    /// 初始化 <see cref="SizeFp32"/> 结构体的新实例，该实例具有指定的点。
    /// </summary>
    /// <param name="pt">要复制的 <see cref="PointF"/> 实例。</param>
    public SizeFp32(PointF pt)
    {
        Width = pt.X;
        Height = pt.Y;
    }

    /// <summary>
    /// 初始化 <see cref="SizeFp32"/> 结构体的新实例，该实例具有指定的宽度和高度。
    /// </summary>
    /// <param name="width">宽度。</param>
    /// <param name="height">高度。</param>
    public SizeFp32(float width, float height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// 返回两个 <see cref="SizeFp32"/> 结构体的和。
    /// </summary>
    /// <param name="sz1">第一个 <see cref="SizeFp32"/> 实例。</param>
    /// <param name="sz2">第二个 <see cref="SizeFp32"/> 实例。</param>
    /// <returns>两个 <see cref="SizeFp32"/> 结构体的和。</returns>
    public static SizeFp32 Add(SizeFp32 sz1, SizeFp32 sz2)
    {
        return new SizeFp32(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
    }

    /// <summary>
    /// 确定指定对象是否等于当前对象。
    /// </summary>
    /// <param name="obj">要与当前对象进行比较的对象。</param>
    /// <returns>如果指定对象等于当前对象，则为 true；否则为 false。</returns>
    public override bool Equals(object? obj)
    {
        if (obj is SizeFp32 sizeF)
        {
            return Width == sizeF.Width && Height == sizeF.Height;
        }
        return false;
    }

    /// <summary>
    /// 用作特定类型的哈希函数。
    /// </summary>
    /// <returns>当前对象的哈希代码。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    /// <summary>
    /// 返回两个 <see cref="SizeFp32"/> 结构体的和。
    /// </summary>
    /// <param name="sz1">第一个 <see cref="SizeFp32"/> 实例。</param>
    /// <param name="sz2">第二个 <see cref="SizeFp32"/> 实例。</param>
    /// <returns>两个 <see cref="SizeFp32"/> 结构体的和。</returns>
    public static SizeFp32 operator +(SizeFp32 sz1, SizeFp32 sz2)
    {
        return Add(sz1, sz2);
    }

    /// <summary>
    /// 确定两个 <see cref="SizeFp32"/> 实例是否相等。
    /// </summary>
    /// <param name="sz1">第一个 <see cref="SizeFp32"/> 实例。</param>
    /// <param name="sz2">第二个 <see cref="SizeFp32"/> 实例。</param>
    /// <returns>如果两个实例相等，则为 true；否则为 false。</returns>
    public static bool operator ==(SizeFp32 sz1, SizeFp32 sz2)
    {
        return sz1.Width == sz2.Width && sz1.Height == sz2.Height;
    }

    /// <summary>
    /// 将 <see cref="SizeFp32"/> 显式转换为 <see cref="PointF"/>。
    /// </summary>
    /// <param name="size">要转换的 <see cref="SizeFp32"/> 实例。</param>
    public static explicit operator PointF(SizeFp32 size)
    {
        return new PointF(size.Width, size.Height);
    }

    /// <summary>
    /// 确定两个 <see cref="SizeFp32"/> 实例是否不相等。
    /// </summary>
    /// <param name="sz1">第一个 <see cref="SizeFp32"/> 实例。</param>
    /// <param name="sz2">第二个 <see cref="SizeFp32"/> 实例。</param>
    /// <returns>如果两个实例不相等，则为 true；否则为 false。</returns>
    public static bool operator !=(SizeFp32 sz1, SizeFp32 sz2)
    {
        return !(sz1 == sz2);
    }

    /// <summary>
    /// 返回两个 <see cref="SizeFp32"/> 结构体的差。
    /// </summary>
    /// <param name="sz1">第一个 <see cref="SizeFp32"/> 实例。</param>
    /// <param name="sz2">第二个 <see cref="SizeFp32"/> 实例。</param>
    /// <returns>两个 <see cref="SizeFp32"/> 结构体的差。</returns>
    public static SizeFp32 operator -(SizeFp32 sz1, SizeFp32 sz2)
    {
        return Subtract(sz1, sz2);
    }

    /// <summary>
    /// 返回两个 <see cref="SizeFp32"/> 结构体的差。
    /// </summary>
    /// <param name="sz1">第一个 <see cref="SizeFp32"/> 实例。</param>
    /// <param name="sz2">第二个 <see cref="SizeFp32"/> 实例。</param>
    /// <returns>两个 <see cref="SizeFp32"/> 结构体的差。</returns>
    public static SizeFp32 Subtract(SizeFp32 sz1, SizeFp32 sz2)
    {
        return new SizeFp32(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
    }

    /// <summary>
    /// 将当前 <see cref="SizeFp32"/> 实例转换为 <see cref="PointF"/>。
    /// </summary>
    /// <returns>转换后的 <see cref="PointF"/> 实例。</returns>
    public PointF ToPointF()
    {
        return (PointF)this;
    }

    /// <summary>
    /// 返回表示当前对象的字符串。
    /// </summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override string ToString()
    {
        return $"{{Width={Width.ToString(CultureInfo.CurrentCulture)}, Height={Height.ToString(CultureInfo.CurrentCulture)}}}";
    }
}
