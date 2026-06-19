using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Vorcyc.Mathematics.Numerics;

/// <summary>
/// Represents a two-dimensional point structure with floating-point coordinates.
/// </summary>
public readonly struct PointFp32
{
    /// <summary>
    /// Represents a <see cref="PointFp32"/> structure whose coordinates are all zero.
    /// </summary>
    public readonly static PointFp32 Empty = new PointFp32(0, 0);

    /// <summary>
    /// Gets the X coordinate.
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Gets the Y coordinate.
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="PointFp32"/> is empty.
    /// </summary>
    [Browsable(false)]
    public bool IsEmpty => X == 0f && Y == 0f;

    /// <summary>
    /// Initializes a new instance of the <see cref="PointFp32"/> structure with the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public PointFp32(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Returns a new <see cref="PointFp32"/> that is the sum of the specified point and size.
    /// </summary>
    /// <param name="pt">The point to add.</param>
    /// <param name="sz">The size to add.</param>
    /// <returns>The new <see cref="PointFp32"/> instance.</returns>
    public static PointFp32 Add(PointFp32 pt, Size sz)
    {
        return new PointFp32(pt.X + sz.Width, pt.Y + sz.Height);
    }

    /// <summary>
    /// Returns a new <see cref="PointFp32"/> that is the sum of the specified point and size.
    /// </summary>
    /// <param name="pt">The point to add.</param>
    /// <param name="sz">The size to add.</param>
    /// <returns>The new <see cref="PointFp32"/> instance.</returns>
    public static PointFp32 Add(PointFp32 pt, SizeFp32 sz)
    {
        return new PointFp32(pt.X + sz.Width, pt.Y + sz.Height);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object obj)
    {
        if (obj is PointFp32 pointF)
        {
            return X == pointF.X && Y == pointF.Y;
        }
        return false;
    }

    /// <summary>
    /// Serves as the hash function for a particular type.
    /// </summary>
    /// <returns>The hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    /// <summary>
    /// Returns a new <see cref="PointFp32"/> that is the sum of the specified point and size.
    /// </summary>
    /// <param name="pt">The point to add.</param>
    /// <param name="sz">The size to add.</param>
    /// <returns>The new <see cref="PointFp32"/> instance.</returns>
    public static PointFp32 operator +(PointFp32 pt, Size sz)
    {
        return Add(pt, sz);
    }

    /// <summary>
    /// Returns a new <see cref="PointFp32"/> that is the sum of the specified point and size.
    /// </summary>
    /// <param name="pt">The point to add.</param>
    /// <param name="sz">The size to add.</param>
    /// <returns>The new <see cref="PointFp32"/> instance.</returns>
    public static PointFp32 operator +(PointFp32 pt, SizeFp32 sz)
    {
        return Add(pt, sz);
    }

    /// <summary>
    /// Determines whether two <see cref="PointFp32"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="PointFp32"/> instance.</param>
    /// <param name="right">The second <see cref="PointFp32"/> instance.</param>
    /// <returns>true if the two instances are equal; otherwise, false.</returns>
    public static bool operator ==(PointFp32 left, PointFp32 right)
    {
        return left.X == right.X && left.Y == right.Y;
    }

    /// <summary>
    /// Determines whether two <see cref="PointFp32"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="PointFp32"/> instance.</param>
    /// <param name="right">The second <see cref="PointFp32"/> instance.</param>
    /// <returns>true if the two instances are not equal; otherwise, false.</returns>
    public static bool operator !=(PointFp32 left, PointFp32 right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns a new <see cref="PointFp32"/> that is the difference of the specified point and size.
    /// </summary>
    /// <param name="pt">The point to subtract from.</param>
    /// <param name="sz">The size to subtract.</param>
    /// <returns>The new <see cref="PointFp32"/> instance.</returns>
    public static PointFp32 operator -(PointFp32 pt, Size sz)
    {
        return Subtract(pt, sz);
    }

    /// <summary>
    /// Returns a new <see cref="PointFp32"/> that is the difference of the specified point and size.
    /// </summary>
    /// <param name="pt">The point to subtract from.</param>
    /// <param name="sz">The size to subtract.</param>
    /// <returns>The new <see cref="PointFp32"/> instance.</returns>
    public static PointFp32 operator -(PointFp32 pt, SizeFp32 sz)
    {
        return Subtract(pt, sz);
    }

    /// <summary>
    /// Returns a new <see cref="PointFp32"/> that is the difference of the specified point and size.
    /// </summary>
    /// <param name="pt">The point to subtract from.</param>
    /// <param name="sz">The size to subtract.</param>
    /// <returns>The new <see cref="PointFp32"/> instance.</returns>
    public static PointFp32 Subtract(PointFp32 pt, Size sz)
    {
        return new PointFp32(pt.X - sz.Width, pt.Y - sz.Height);
    }

    /// <summary>
    /// Returns a new <see cref="PointFp32"/> that is the difference of the specified point and size.
    /// </summary>
    /// <param name="pt">The point to subtract from.</param>
    /// <param name="sz">The size to subtract.</param>
    /// <returns>The new <see cref="PointFp32"/> instance.</returns>
    public static PointFp32 Subtract(PointFp32 pt, SizeFp32 sz)
    {
        return new PointFp32(pt.X - sz.Width, pt.Y - sz.Height);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return string.Format(CultureInfo.CurrentCulture, "{{X={0}, Y={1}}}", X, Y);
    }
}
