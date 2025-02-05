using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Vorcyc.Mathematics.Numerics;

/// <summary>
/// 表示一个具有浮点数坐标的二维点结构体。
/// </summary>
public readonly struct PointFp32
{
    /// <summary>
    /// 表示一个坐标均为零的 <see cref="PointFp32"/> 结构体。
    /// </summary>
    public readonly static PointFp32 Empty = new PointFp32(0, 0);

    /// <summary>
    /// 获取 X 坐标。
    /// </summary>
    public float X { get; }

    /// <summary>
    /// 获取 Y 坐标。
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// 获取一个值，该值指示此 <see cref="PointFp32"/> 是否为空。
    /// </summary>
    [Browsable(false)]
    public bool IsEmpty => X == 0f && Y == 0f;

    /// <summary>
    /// 初始化 <see cref="PointFp32"/> 结构体的新实例，该实例具有指定的坐标。
    /// </summary>
    /// <param name="x">X 坐标。</param>
    /// <param name="y">Y 坐标。</param>
    public PointFp32(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// 返回一个新的 <see cref="PointFp32"/>，它是指定点和尺寸的和。
    /// </summary>
    /// <param name="pt">要加的点。</param>
    /// <param name="sz">要加的尺寸。</param>
    /// <returns>新的 <see cref="PointFp32"/> 实例。</returns>
    public static PointFp32 Add(PointFp32 pt, Size sz)
    {
        return new PointFp32(pt.X + sz.Width, pt.Y + sz.Height);
    }

    /// <summary>
    /// 返回一个新的 <see cref="PointFp32"/>，它是指定点和尺寸的和。
    /// </summary>
    /// <param name="pt">要加的点。</param>
    /// <param name="sz">要加的尺寸。</param>
    /// <returns>新的 <see cref="PointFp32"/> 实例。</returns>
    public static PointFp32 Add(PointFp32 pt, SizeFp32 sz)
    {
        return new PointFp32(pt.X + sz.Width, pt.Y + sz.Height);
    }

    /// <summary>
    /// 确定指定对象是否等于当前对象。
    /// </summary>
    /// <param name="obj">要与当前对象进行比较的对象。</param>
    /// <returns>如果指定对象等于当前对象，则为 true；否则为 false。</returns>
    public override bool Equals(object obj)
    {
        if (obj is PointFp32 pointF)
        {
            return X == pointF.X && Y == pointF.Y;
        }
        return false;
    }

    /// <summary>
    /// 用作特定类型的哈希函数。
    /// </summary>
    /// <returns>当前对象的哈希代码。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    /// <summary>
    /// 返回一个新的 <see cref="PointFp32"/>，它是指定点和尺寸的和。
    /// </summary>
    /// <param name="pt">要加的点。</param>
    /// <param name="sz">要加的尺寸。</param>
    /// <returns>新的 <see cref="PointFp32"/> 实例。</returns>
    public static PointFp32 operator +(PointFp32 pt, Size sz)
    {
        return Add(pt, sz);
    }

    /// <summary>
    /// 返回一个新的 <see cref="PointFp32"/>，它是指定点和尺寸的和。
    /// </summary>
    /// <param name="pt">要加的点。</param>
    /// <param name="sz">要加的尺寸。</param>
    /// <returns>新的 <see cref="PointFp32"/> 实例。</returns>
    public static PointFp32 operator +(PointFp32 pt, SizeFp32 sz)
    {
        return Add(pt, sz);
    }

    /// <summary>
    /// 确定两个 <see cref="PointFp32"/> 实例是否相等。
    /// </summary>
    /// <param name="left">第一个 <see cref="PointFp32"/> 实例。</param>
    /// <param name="right">第二个 <see cref="PointFp32"/> 实例。</param>
    /// <returns>如果两个实例相等，则为 true；否则为 false。</returns>
    public static bool operator ==(PointFp32 left, PointFp32 right)
    {
        return left.X == right.X && left.Y == right.Y;
    }

    /// <summary>
    /// 确定两个 <see cref="PointFp32"/> 实例是否不相等。
    /// </summary>
    /// <param name="left">第一个 <see cref="PointFp32"/> 实例。</param>
    /// <param name="right">第二个 <see cref="PointFp32"/> 实例。</param>
    /// <returns>如果两个实例不相等，则为 true；否则为 false。</returns>
    public static bool operator !=(PointFp32 left, PointFp32 right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 返回一个新的 <see cref="PointFp32"/>，它是指定点和尺寸的差。
    /// </summary>
    /// <param name="pt">要减的点。</param>
    /// <param name="sz">要减的尺寸。</param>
    /// <returns>新的 <see cref="PointFp32"/> 实例。</returns>
    public static PointFp32 operator -(PointFp32 pt, Size sz)
    {
        return Subtract(pt, sz);
    }

    /// <summary>
    /// 返回一个新的 <see cref="PointFp32"/>，它是指定点和尺寸的差。
    /// </summary>
    /// <param name="pt">要减的点。</param>
    /// <param name="sz">要减的尺寸。</param>
    /// <returns>新的 <see cref="PointFp32"/> 实例。</returns>
    public static PointFp32 operator -(PointFp32 pt, SizeFp32 sz)
    {
        return Subtract(pt, sz);
    }

    /// <summary>
    /// 返回一个新的 <see cref="PointFp32"/>，它是指定点和尺寸的差。
    /// </summary>
    /// <param name="pt">要减的点。</param>
    /// <param name="sz">要减的尺寸。</param>
    /// <returns>新的 <see cref="PointFp32"/> 实例。</returns>
    public static PointFp32 Subtract(PointFp32 pt, Size sz)
    {
        return new PointFp32(pt.X - sz.Width, pt.Y - sz.Height);
    }

    /// <summary>
    /// 返回一个新的 <see cref="PointFp32"/>，它是指定点和尺寸的差。
    /// </summary>
    /// <param name="pt">要减的点。</param>
    /// <param name="sz">要减的尺寸。</param>
    /// <returns>新的 <see cref="PointFp32"/> 实例。</returns>
    public static PointFp32 Subtract(PointFp32 pt, SizeFp32 sz)
    {
        return new PointFp32(pt.X - sz.Width, pt.Y - sz.Height);
    }

    /// <summary>
    /// 返回表示当前对象的字符串。
    /// </summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override string ToString()
    {
        return string.Format(CultureInfo.CurrentCulture, "{{X={0}, Y={1}}}", X, Y);
    }
}
