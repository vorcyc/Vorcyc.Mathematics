#if NET7_0_OR_GREATER
namespace Vorcyc.Mathematics.Numerics;

using System.Numerics;

/// <summary>
/// Represents a point in 2D plane with generic math supports.
/// </summary>
/// <typeparam name="T"></typeparam>
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


    public static readonly Point<T> Empty;

    private T _x; // Do not rename (binary serialization)
    private T _y; // Do not rename (binary serialization)

    public Point(T x, T y)
    {
        this._x = x;
        this._y = y;
    }

    public readonly bool IsEmpty => _x == T.Zero && _y == T.Zero;

    public T X
    {
        readonly get => _x;
        set => _x = value;
    }

    public T Y
    {
        readonly get => _y;
        set => _y = value;
    }

    /// <summary>
    /// Deconstruct to <see cref="ValueTuple{T1,T2}"/>
    /// </summary>
    /// <param name="x">The X coordinate. </param>
    /// <param name="y">The Y coordinate. </param>
    public void Deconstruct(out T x, out T y)
    {
        x = _x;
        y = _y;
    }

    public static Point<T> Add(Point<T> pt, Size<T> sz) => new(pt.X + sz.Width, pt.Y + sz.Height);

    public static Point<T> Subtract(Point<T> pt, Size<T> sz) => new(pt.X - sz.Width, pt.Y - sz.Height);


    public static Point<T> operator +(Point<T> pt, Size<T> sz) => Add(pt, sz);

    public static Point<T> operator +(Point<T> pt, (T width, T height) sz) => new(pt._x + sz.width, pt._y + sz.height);

    public static Point<T> operator +(Point<T> pt, T scalar) => new(pt._x + scalar, pt._y + scalar);

    public static Point<T> operator -(Point<T> pt, Size<T> sz) => Subtract(pt, sz);

    public static Point<T> operator -(Point<T> pt, (T width, T height) sz) => new(pt._x - sz.width, pt._y - sz.height);

    public static Point<T> operator -(Point<T> pt, T scalar) => new(pt._x - scalar, pt._y - scalar);




    public bool Equals(Point<T> other) => this._x == other._x && this._y == other._y;

    public override bool Equals(object obj)
    {
        return obj is Point<T> && Equals((Point<T>)obj);
    }


    public override int GetHashCode() => HashCode.Combine(X.GetHashCode(), Y.GetHashCode());


    public override readonly string ToString() => $"{{X={_x}, Y={_y}}}";


}

#endif