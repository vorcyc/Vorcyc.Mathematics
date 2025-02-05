#if NET7_0_OR_GREATER
namespace Vorcyc.Mathematics.Numerics;

using System.Numerics;

/// <summary>
/// 表示一个二维平面上的点，支持泛型数学运算。
/// </summary>
/// <typeparam name="T">坐标的数值类型。</typeparam>
public struct Point<T> :
    IAdditionOperators<Point<T>, Size<T>, Point<T>>,
    IAdditionOperators<Point<T>, (T width, T height), Point<T>>,
    IAdditionOperators<Point<T>, T, Point<T>>,
    ISubtractionOperators<Point<T>, Size<T>, Point<T>>,
    ISubtractionOperators<Point<T>, (T width, T height), Point<T>>,
    ISubtractionOperators<Point<T>, T, Point<T>>,
    IEquatable<Point<T>>
    where T : struct, INumber<T>
{
    /// <summary>
    /// 表示一个空点。
    /// </summary>
    public static readonly Point<T> Empty = new(T.Zero, T.Zero);

    /// <summary>
    /// 获取或设置点的 X 坐标。
    /// </summary>
    public T X { get; set; }

    /// <summary>
    /// 获取或设置点的 Y 坐标。
    /// </summary>
    public T Y { get; set; }

    /// <summary>
    /// 初始化 <see cref="Point{T}"/> 结构体的新实例，具有指定的坐标。
    /// </summary>
    /// <param name="x">点的 X 坐标。</param>
    /// <param name="y">点的 Y 坐标。</param>
    public Point(T x, T y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// 获取一个值，该值指示此点是否为空。
    /// </summary>
    public readonly bool IsEmpty => X == T.Zero && Y == T.Zero;

    /// <summary>
    /// 将点解构为其 X 和 Y 坐标。
    /// </summary>
    /// <param name="x">X 坐标。</param>
    /// <param name="y">Y 坐标。</param>
    public void Deconstruct(out T x, out T y)
    {
        x = X;
        y = Y;
    }

    /// <summary>
    /// 将大小添加到点。
    /// </summary>
    /// <param name="pt">点。</param>
    /// <param name="sz">大小。</param>
    /// <returns>结果点。</returns>
    public static Point<T> Add(Point<T> pt, Size<T> sz) => new(pt.X + sz.Width, pt.Y + sz.Height);

    /// <summary>
    /// 从点中减去大小。
    /// </summary>
    /// <param name="pt">点。</param>
    /// <param name="sz">大小。</param>
    /// <returns>结果点。</returns>
    public static Point<T> Subtract(Point<T> pt, Size<T> sz) => new(pt.X - sz.Width, pt.Y - sz.Height);

    /// <summary>
    /// 将大小添加到点。
    /// </summary>
    /// <param name="pt">点。</param>
    /// <param name="sz">大小。</param>
    /// <returns>结果点。</returns>
    public static Point<T> operator +(Point<T> pt, Size<T> sz) => Add(pt, sz);

    /// <summary>
    /// 将元组添加到点。
    /// </summary>
    /// <param name="pt">点。</param>
    /// <param name="sz">表示大小的元组。</param>
    /// <returns>结果点。</returns>
    public static Point<T> operator +(Point<T> pt, (T width, T height) sz) => new(pt.X + sz.width, pt.Y + sz.height);

    /// <summary>
    /// 将标量添加到点。
    /// </summary>
    /// <param name="pt">点。</param>
    /// <param name="scalar">标量值。</param>
    /// <returns>结果点。</returns>
    public static Point<T> operator +(Point<T> pt, T scalar) => new(pt.X + scalar, pt.Y + scalar);

    /// <summary>
    /// 从点中减去大小。
    /// </summary>
    /// <param name="pt">点。</param>
    /// <param name="sz">大小。</param>
    /// <returns>结果点。</returns>
    public static Point<T> operator -(Point<T> pt, Size<T> sz) => Subtract(pt, sz);

    /// <summary>
    /// 从点中减去元组。
    /// </summary>
    /// <param name="pt">点。</param>
    /// <param name="sz">表示大小的元组。</param>
    /// <returns>结果点。</returns>
    public static Point<T> operator -(Point<T> pt, (T width, T height) sz) => new(pt.X - sz.width, pt.Y - sz.height);

    /// <summary>
    /// 从点中减去标量。
    /// </summary>
    /// <param name="pt">点。</param>
    /// <param name="scalar">标量值。</param>
    /// <returns>结果点。</returns>
    public static Point<T> operator -(Point<T> pt, T scalar) => new(pt.X - scalar, pt.Y - scalar);

    /// <summary>
    /// 计算两个点之间的距离。
    /// </summary>
    /// <typeparam name="TFloatingPointNumber">浮点数类型。</typeparam>
    /// <param name="a">第一个点。</param>
    /// <param name="b">第二个点。</param>
    /// <returns>两个点之间的距离。</returns>
    public static TFloatingPointNumber Distance<TFloatingPointNumber>(Point<TFloatingPointNumber> a, Point<TFloatingPointNumber> b)
        where TFloatingPointNumber : struct, IFloatingPointIeee754<TFloatingPointNumber>
    {
        TFloatingPointNumber dx = a.X - b.X;
        TFloatingPointNumber dy = a.Y - b.Y;
        return TFloatingPointNumber.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 确定指定的点是否等于当前点。
    /// </summary>
    /// <param name="other">要与当前点进行比较的点。</param>
    /// <returns>如果指定的点等于当前点，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public bool Equals(Point<T> other) => X.Equals(other.X) && Y.Equals(other.Y);

    /// <summary>
    /// 确定指定对象是否等于当前点。
    /// </summary>
    /// <param name="obj">要与当前点进行比较的对象。</param>
    /// <returns>如果指定对象等于当前点，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public override bool Equals(object? obj) => obj is Point<T> other && Equals(other);

    /// <summary>
    /// 返回当前点的哈希代码。
    /// </summary>
    /// <returns>当前点的哈希代码。</returns>
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>
    /// 返回表示当前点的字符串。
    /// </summary>
    /// <returns>表示当前点的字符串。</returns>
    public override readonly string ToString() => $"{{X={X}, Y={Y}}}";
}

#endif
