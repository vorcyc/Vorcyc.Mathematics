using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Vorcyc.Mathematics.Numerics;

/// <summary>
/// Represents a two-dimensional size structure with floating-point width and height.
/// </summary>
public readonly struct SizeFp32
{
    /// <summary>
    /// Represents a <see cref="SizeFp32"/> structure whose width and height are both zero.
    /// </summary>
    public readonly static SizeFp32 Empty = new SizeFp32(0, 0);

    /// <summary>
    /// Gets the width.
    /// </summary>
    public float Width { get; }

    /// <summary>
    /// Gets the height.
    /// </summary>
    public float Height { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="SizeFp32"/> is empty.
    /// </summary>
    [Browsable(false)]
    public bool IsEmpty => Width == 0f && Height == 0f;

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeFp32"/> structure with the specified size.
    /// </summary>
    /// <param name="size">The <see cref="SizeFp32"/> instance to copy.</param>
    public SizeFp32(SizeFp32 size)
    {
        Width = size.Width;
        Height = size.Height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeFp32"/> structure with the specified point.
    /// </summary>
    /// <param name="pt">The <see cref="PointF"/> instance to copy.</param>
    public SizeFp32(PointF pt)
    {
        Width = pt.X;
        Height = pt.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeFp32"/> structure with the specified width and height.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public SizeFp32(float width, float height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Returns the sum of two <see cref="SizeFp32"/> structures.
    /// </summary>
    /// <param name="sz1">The first <see cref="SizeFp32"/> instance.</param>
    /// <param name="sz2">The second <see cref="SizeFp32"/> instance.</param>
    /// <returns>The sum of the two <see cref="SizeFp32"/> structures.</returns>
    public static SizeFp32 Add(SizeFp32 sz1, SizeFp32 sz2)
    {
        return new SizeFp32(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is SizeFp32 sizeF)
        {
            return Width == sizeF.Width && Height == sizeF.Height;
        }
        return false;
    }

    /// <summary>
    /// Serves as a hash function for the type.
    /// </summary>
    /// <returns>The hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    /// <summary>
    /// Returns the sum of two <see cref="SizeFp32"/> structures.
    /// </summary>
    /// <param name="sz1">The first <see cref="SizeFp32"/> instance.</param>
    /// <param name="sz2">The second <see cref="SizeFp32"/> instance.</param>
    /// <returns>The sum of the two <see cref="SizeFp32"/> structures.</returns>
    public static SizeFp32 operator +(SizeFp32 sz1, SizeFp32 sz2)
    {
        return Add(sz1, sz2);
    }

    /// <summary>
    /// Determines whether two <see cref="SizeFp32"/> instances are equal.
    /// </summary>
    /// <param name="sz1">The first <see cref="SizeFp32"/> instance.</param>
    /// <param name="sz2">The second <see cref="SizeFp32"/> instance.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(SizeFp32 sz1, SizeFp32 sz2)
    {
        return sz1.Width == sz2.Width && sz1.Height == sz2.Height;
    }

    /// <summary>
    /// Explicitly converts a <see cref="SizeFp32"/> to a <see cref="PointF"/>.
    /// </summary>
    /// <param name="size">The <see cref="SizeFp32"/> instance to convert.</param>
    public static explicit operator PointF(SizeFp32 size)
    {
        return new PointF(size.Width, size.Height);
    }

    /// <summary>
    /// Determines whether two <see cref="SizeFp32"/> instances are not equal.
    /// </summary>
    /// <param name="sz1">The first <see cref="SizeFp32"/> instance.</param>
    /// <param name="sz2">The second <see cref="SizeFp32"/> instance.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(SizeFp32 sz1, SizeFp32 sz2)
    {
        return !(sz1 == sz2);
    }

    /// <summary>
    /// Returns the difference of two <see cref="SizeFp32"/> structures.
    /// </summary>
    /// <param name="sz1">The first <see cref="SizeFp32"/> instance.</param>
    /// <param name="sz2">The second <see cref="SizeFp32"/> instance.</param>
    /// <returns>The difference of the two <see cref="SizeFp32"/> structures.</returns>
    public static SizeFp32 operator -(SizeFp32 sz1, SizeFp32 sz2)
    {
        return Subtract(sz1, sz2);
    }

    /// <summary>
    /// Returns the difference of two <see cref="SizeFp32"/> structures.
    /// </summary>
    /// <param name="sz1">The first <see cref="SizeFp32"/> instance.</param>
    /// <param name="sz2">The second <see cref="SizeFp32"/> instance.</param>
    /// <returns>The difference of the two <see cref="SizeFp32"/> structures.</returns>
    public static SizeFp32 Subtract(SizeFp32 sz1, SizeFp32 sz2)
    {
        return new SizeFp32(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
    }

    /// <summary>
    /// Converts the current <see cref="SizeFp32"/> instance to a <see cref="PointF"/>.
    /// </summary>
    /// <returns>The converted <see cref="PointF"/> instance.</returns>
    public PointF ToPointF()
    {
        return (PointF)this;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"{{Width={Width.ToString(CultureInfo.CurrentCulture)}, Height={Height.ToString(CultureInfo.CurrentCulture)}}}";
    }
}
