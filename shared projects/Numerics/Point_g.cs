#if NET7_0_OR_GREATER
namespace Vorcyc.Mathematics.Numerics;

using System.Numerics;

/// <summary>
/// Represents a point in 2D plane with generic math supports.
/// </summary>
/// <typeparam name="T">The numeric type of the coordinates.</typeparam>
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
    /// Represents an empty point.
    /// </summary>
    public static readonly Point<T> Empty = new(T.Zero, T.Zero);

    private T _x; // Do not rename (binary serialization)
    private T _y; // Do not rename (binary serialization)

    /// <summary>
    /// Initializes a new instance of the <see cref="Point{T}"/> struct with the specified coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the point.</param>
    /// <param name="y">The y-coordinate of the point.</param>
    public Point(T x, T y)
    {
        this._x = x;
        this._y = y;
    }

    /// <summary>
    /// Gets a value indicating whether this point is empty.
    /// </summary>
    public readonly bool IsEmpty => _x == T.Zero && _y == T.Zero;

    /// <summary>
    /// Gets or sets the x-coordinate of the point.
    /// </summary>
    public T X
    {
        readonly get => _x;
        set => _x = value;
    }

    /// <summary>
    /// Gets or sets the y-coordinate of the point.
    /// </summary>
    public T Y
    {
        readonly get => _y;
        set => _y = value;
    }

    /// <summary>
    /// Deconstructs the point into its x and y coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    public void Deconstruct(out T x, out T y)
    {
        x = _x;
        y = _y;
    }

    /// <summary>
    /// Adds a size to a point.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <param name="sz">The size.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> Add(Point<T> pt, Size<T> sz) => new(pt.X + sz.Width, pt.Y + sz.Height);

    /// <summary>
    /// Subtracts a size from a point.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <param name="sz">The size.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> Subtract(Point<T> pt, Size<T> sz) => new(pt.X - sz.Width, pt.Y - sz.Height);

    /// <summary>
    /// Adds a size to a point.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <param name="sz">The size.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> operator +(Point<T> pt, Size<T> sz) => Add(pt, sz);

    /// <summary>
    /// Adds a tuple to a point.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <param name="sz">The tuple representing the size.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> operator +(Point<T> pt, (T width, T height) sz) => new(pt._x + sz.width, pt._y + sz.height);

    /// <summary>
    /// Adds a scalar to a point.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> operator +(Point<T> pt, T scalar) => new(pt._x + scalar, pt._y + scalar);

    /// <summary>
    /// Subtracts a size from a point.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <param name="sz">The size.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> operator -(Point<T> pt, Size<T> sz) => Subtract(pt, sz);

    /// <summary>
    /// Subtracts a tuple from a point.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <param name="sz">The tuple representing the size.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> operator -(Point<T> pt, (T width, T height) sz) => new(pt._x - sz.width, pt._y - sz.height);

    /// <summary>
    /// Subtracts a scalar from a point.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> operator -(Point<T> pt, T scalar) => new(pt._x - scalar, pt._y - scalar);



    public static TFloatingPointNumber Distance<TFloatingPointNumber>(Point<TFloatingPointNumber> a, Point<TFloatingPointNumber> b)
        where TFloatingPointNumber : struct, IFloatingPointIeee754<TFloatingPointNumber>
    {
        TFloatingPointNumber dx = a.X - b.X;
        TFloatingPointNumber dy = a.Y - b.Y;
        return TFloatingPointNumber.Sqrt(dx * dx + dy * dy);
    }



    /// <summary>
    /// Determines whether the specified point is equal to the current point.
    /// </summary>
    /// <param name="other">The point to compare with the current point.</param>
    /// <returns>true if the specified point is equal to the current point; otherwise, false.</returns>
    public bool Equals(Point<T> other) => this._x == other._x && this._y == other._y;

    /// <summary>
    /// Determines whether the specified object is equal to the current point.
    /// </summary>
    /// <param name="obj">The object to compare with the current point.</param>
    /// <returns>true if the specified object is equal to the current point; otherwise, false.</returns>
    public override bool Equals(object obj)
    {
        return obj is Point<T> && Equals((Point<T>)obj);
    }

    /// <summary>
    /// Returns the hash code for the current point.
    /// </summary>
    /// <returns>A hash code for the current point.</returns>
    public override int GetHashCode() => HashCode.Combine(X.GetHashCode(), Y.GetHashCode());

    /// <summary>
    /// Returns a string that represents the current point.
    /// </summary>
    /// <returns>A string that represents the current point.</returns>
    public override readonly string ToString() => $"{{X={_x}, Y={_y}}}";
}

#endif