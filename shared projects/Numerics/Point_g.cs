#if NET7_0_OR_GREATER
namespace Vorcyc.Mathematics.Numerics;

using System.Numerics;

/// <summary>
/// Represents a point on a two-dimensional plane, supporting generic mathematical operations.
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

    /// <summary>
    /// Gets or sets the X coordinate of the point.
    /// </summary>
    public T X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the point.
    /// </summary>
    public T Y { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Point{T}"/> struct with the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate of the point.</param>
    /// <param name="y">The Y coordinate of the point.</param>
    public Point(T x, T y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets a value indicating whether this point is empty.
    /// </summary>
    public readonly bool IsEmpty => X == T.Zero && Y == T.Zero;

    /// <summary>
    /// Deconstructs the point into its X and Y coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void Deconstruct(out T x, out T y)
    {
        x = X;
        y = Y;
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
    /// <param name="sz">A tuple representing a size.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> operator +(Point<T> pt, (T width, T height) sz) => new(pt.X + sz.width, pt.Y + sz.height);

    /// <summary>
    /// Adds a scalar to a point.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> operator +(Point<T> pt, T scalar) => new(pt.X + scalar, pt.Y + scalar);

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
    /// <param name="sz">A tuple representing a size.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> operator -(Point<T> pt, (T width, T height) sz) => new(pt.X - sz.width, pt.Y - sz.height);

    /// <summary>
    /// Subtracts a scalar from a point.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The resulting point.</returns>
    public static Point<T> operator -(Point<T> pt, T scalar) => new(pt.X - scalar, pt.Y - scalar);

    /// <summary>
    /// Computes the distance between two points.
    /// </summary>
    /// <typeparam name="TFloatingPointNumber">The floating-point number type.</typeparam>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The distance between the two points.</returns>
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
    /// <returns><c>true</c> if the specified point is equal to the current point; otherwise, <c>false</c>.</returns>
    public bool Equals(Point<T> other) => X.Equals(other.X) && Y.Equals(other.Y);

    /// <summary>
    /// Determines whether the specified object is equal to the current point.
    /// </summary>
    /// <param name="obj">The object to compare with the current point.</param>
    /// <returns><c>true</c> if the specified object is equal to the current point; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is Point<T> other && Equals(other);

    /// <summary>
    /// Returns the hash code for the current point.
    /// </summary>
    /// <returns>The hash code for the current point.</returns>
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>
    /// Returns a string that represents the current point.
    /// </summary>
    /// <returns>A string that represents the current point.</returns>
    public override readonly string ToString() => $"{{X={X}, Y={Y}}}";
}

#endif
