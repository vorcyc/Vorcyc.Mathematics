namespace Vorcyc.Mathematics.Numerics;

using System.ComponentModel;
using System.Globalization;

/// <summary>
/// 表示一个具有浮点数坐标和尺寸的矩形结构体。
/// </summary>
public struct RectangleFP32
{
    /// <summary>
    /// 表示一个空的 <see cref="RectangleFP32"/> 结构体。
    /// </summary>
    public readonly static RectangleFP32 Empty = new RectangleFP32(0, 0, 0, 0);

    /// <summary>
    /// 获取或设置矩形的 X 坐标。
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// 获取或设置矩形的 Y 坐标。
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// 获取或设置矩形的宽度。
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// 获取或设置矩形的高度。
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// 获取矩形的底部坐标。
    /// </summary>
    [Browsable(false)]
    public float Bottom => Y + Height;

    /// <summary>
    /// 获取一个值，该值指示矩形是否为空。
    /// </summary>
    [Browsable(false)]
    public bool IsEmpty => Width <= 0f || Height <= 0f;

    /// <summary>
    /// 获取矩形的左侧坐标。
    /// </summary>
    [Browsable(false)]
    public float Left => X;

    /// <summary>
    /// 获取或设置矩形的位置。
    /// </summary>
    [Browsable(false)]
    public PointFp32 Location
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    /// <summary>
    /// 获取矩形的右侧坐标。
    /// </summary>
    [Browsable(false)]
    public float Right => X + Width;

    /// <summary>
    /// 获取或设置矩形的大小。
    /// </summary>
    [Browsable(false)]
    public SizeFp32 Size
    {
        get => new SizeFp32(Width, Height);
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    /// <summary>
    /// 获取矩形的顶部坐标。
    /// </summary>
    [Browsable(false)]
    public float Top => Y;

    /// <summary>
    /// 初始化 <see cref="RectangleFP32"/> 结构体的新实例，该实例具有指定的坐标和尺寸。
    /// </summary>
    /// <param name="x">X 坐标。</param>
    /// <param name="y">Y 坐标。</param>
    /// <param name="width">宽度。</param>
    /// <param name="height">高度。</param>
    public RectangleFP32(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// 初始化 <see cref="RectangleFP32"/> 结构体的新实例，该实例具有指定的位置和大小。
    /// </summary>
    /// <param name="location">位置。</param>
    /// <param name="size">大小。</param>
    public RectangleFP32(PointFp32 location, SizeFp32 size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.Width;
        Height = size.Height;
    }

    /// <summary>
    /// 检查指定的点是否在矩形内。
    /// </summary>
    /// <param name="x">点的 X 坐标。</param>
    /// <param name="y">点的 Y 坐标。</param>
    /// <returns>如果点在矩形内，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public bool Contains(float x, float y) => X <= x && x < X + Width && Y <= y && y < Y + Height;

    /// <summary>
    /// 检查指定的点是否在矩形内。
    /// </summary>
    /// <param name="pt">点。</param>
    /// <returns>如果点在矩形内，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public bool Contains(PointFp32 pt) => Contains(pt.X, pt.Y);

    /// <summary>
    /// 检查指定的矩形是否在当前矩形内。
    /// </summary>
    /// <param name="rect">矩形。</param>
    /// <returns>如果矩形在当前矩形内，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public bool Contains(RectangleFP32 rect) => X <= rect.X && rect.X + rect.Width <= X + Width && Y <= rect.Y && rect.Y + rect.Height <= Y + Height;

    /// <summary>
    /// 确定指定对象是否等于当前对象。
    /// </summary>
    /// <param name="obj">要与当前对象进行比较的对象。</param>
    /// <returns>如果指定对象等于当前对象，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public override bool Equals(object obj) => obj is RectangleFP32 rectangle && this == rectangle;

    /// <summary>
    /// 返回两个 <see cref="RectangleFP32"/> 结构体的和。
    /// </summary>
    /// <param name="left">第一个 <see cref="RectangleFP32"/> 实例。</param>
    /// <param name="right">第二个 <see cref="RectangleFP32"/> 实例。</param>
    /// <returns>两个 <see cref="RectangleFP32"/> 结构体的和。</returns>
    public static RectangleFP32 FromLTRB(float left, float top, float right, float bottom) => new RectangleFP32(left, top, right - left, bottom - top);

    /// <summary>
    /// 返回此实例的哈希代码。
    /// </summary>
    /// <returns>此实例的哈希代码。</returns>
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

    /// <summary>
    /// 按指定的量扩展矩形。
    /// </summary>
    /// <param name="x">扩展的 X 量。</param>
    /// <param name="y">扩展的 Y 量。</param>
    public void Inflate(float x, float y)
    {
        X -= x;
        Y -= y;
        Width += 2 * x;
        Height += 2 * y;
    }

    /// <summary>
    /// 按指定的大小扩展矩形。
    /// </summary>
    /// <param name="size">扩展的大小。</param>
    public void Inflate(SizeFp32 size) => Inflate(size.Width, size.Height);

    /// <summary>
    /// 返回按指定的量扩展的矩形。
    /// </summary>
    /// <param name="rect">要扩展的矩形。</param>
    /// <param name="x">扩展的 X 量。</param>
    /// <param name="y">扩展的 Y 量。</param>
    /// <returns>扩展后的矩形。</returns>
    public static RectangleFP32 Inflate(RectangleFP32 rect, float x, float y)
    {
        var rectangle = rect;
        rectangle.Inflate(x, y);
        return rectangle;
    }

    /// <summary>
    /// 计算当前矩形与指定矩形的交集。
    /// </summary>
    /// <param name="rect">要计算交集的矩形。</param>
    public void Intersect(RectangleFP32 rect)
    {
        var rectangle = Intersect(this, rect);
        X = rectangle.X;
        Y = rectangle.Y;
        Width = rectangle.Width;
        Height = rectangle.Height;
    }

    /// <summary>
    /// 返回两个矩形的交集。
    /// </summary>
    /// <param name="a">第一个矩形。</param>
    /// <param name="b">第二个矩形。</param>
    /// <returns>两个矩形的交集。</returns>
    public static RectangleFP32 Intersect(RectangleFP32 a, RectangleFP32 b)
    {
        var x1 = Math.Max(a.X, b.X);
        var x2 = Math.Min(a.X + a.Width, b.X + b.Width);
        var y1 = Math.Max(a.Y, b.Y);
        var y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);
        if (x2 < x1 || y2 < y1)
        {
            return Empty;
        }
        return new RectangleFP32(x1, y1, x2 - x1, y2 - y1);
    }

    /// <summary>
    /// 检查当前矩形是否与指定矩形相交。
    /// </summary>
    /// <param name="rect">要检查的矩形。</param>
    /// <returns>如果矩形相交，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public bool IntersectsWith(RectangleFP32 rect) => rect.X < X + Width && X < rect.X + rect.Width && rect.Y < Y + Height && Y < rect.Y + rect.Height;

    /// <summary>
    /// 按指定的量偏移矩形。
    /// </summary>
    /// <param name="pos">偏移量。</param>
    public void Offset(PointFp32 pos) => Offset(pos.X, pos.Y);

    /// <summary>
    /// 按指定的量偏移矩形。
    /// </summary>
    /// <param name="x">X 方向的偏移量。</param>
    /// <param name="y">Y 方向的偏移量。</param>
    public void Offset(float x, float y)
    {
        X += x;
        Y += y;
    }

    /// <summary>
    /// 确定两个 <see cref="RectangleFP32"/> 实例是否相等。
    /// </summary>
    /// <param name="left">第一个 <see cref="RectangleFP32"/> 实例。</param>
    /// <param name="right">第二个 <see cref="RectangleFP32"/> 实例。</param>
    /// <returns>如果两个实例相等，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public static bool operator ==(RectangleFP32 left, RectangleFP32 right) => left.X == right.X && left.Y == right.Y && left.Width == right.Width && left.Height == right.Height;

    /// <summary>
    /// 将 <see cref="System.Drawing.Rectangle"/> 隐式转换为 <see cref="RectangleFP32"/>。
    /// </summary>
    /// <param name="r">要转换的 <see cref="Rectangle"/> 实例。</param>
    public static implicit operator RectangleFP32(System.Drawing.Rectangle r) => new RectangleFP32(r.X, r.Y, r.Width, r.Height);

    /// <summary>
    /// 确定两个 <see cref="RectangleFP32"/> 实例是否不相等。
    /// </summary>
    /// <param name="left">第一个 <see cref="RectangleFP32"/> 实例。</param>
    /// <param name="right">第二个 <see cref="RectangleFP32"/> 实例。</param>
    /// <returns>如果两个实例不相等，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public static bool operator !=(RectangleFP32 left, RectangleFP32 right) => !(left == right);

    /// <summary>
    /// 返回表示当前对象的字符串。
    /// </summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override string ToString() => $"{{X={X.ToString(CultureInfo.CurrentCulture)}, Y={Y.ToString(CultureInfo.CurrentCulture)}, Width={Width.ToString(CultureInfo.CurrentCulture)}, Height={Height.ToString(CultureInfo.CurrentCulture)}}}";

    /// <summary>
    /// 返回两个矩形的并集。
    /// </summary>
    /// <param name="a">第一个矩形。</param>
    /// <param name="b">第二个矩形。</param>
    /// <returns>两个矩形的并集。</returns>
    public static RectangleFP32 Union(RectangleFP32 a, RectangleFP32 b)
    {
        var x1 = Math.Min(a.X, b.X);
        var x2 = Math.Max(a.X + a.Width, b.X + b.Width);
        var y1 = Math.Min(a.Y, b.Y);
        var y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);
        return new RectangleFP32(x1, y1, x2 - x1, y2 - y1);
    }
}
