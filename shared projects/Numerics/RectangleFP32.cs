namespace Vorcyc.Mathematics.Numerics;

using System.ComponentModel;
using System.Globalization;

/// <summary>
/// Represents a rectangle structure with floating-point coordinates and dimensions.
/// </summary>
public struct RectangleFP32
{
    /// <summary>
    /// Represents an empty <see cref="RectangleFP32"/> structure.
    /// </summary>
    public readonly static RectangleFP32 Empty = new RectangleFP32(0, 0, 0, 0);

    /// <summary>
    /// Gets or sets the X coordinate of the rectangle.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the rectangle.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the rectangle.
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the rectangle.
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// Gets the bottom coordinate of the rectangle.
    /// </summary>
    [Browsable(false)]
    public float Bottom => Y + Height;

    /// <summary>
    /// Gets a value indicating whether the rectangle is empty.
    /// </summary>
    [Browsable(false)]
    public bool IsEmpty => Width <= 0f || Height <= 0f;

    /// <summary>
    /// Gets the left coordinate of the rectangle.
    /// </summary>
    [Browsable(false)]
    public float Left => X;

    /// <summary>
    /// Gets or sets the location of the rectangle.
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
    /// Gets the right coordinate of the rectangle.
    /// </summary>
    [Browsable(false)]
    public float Right => X + Width;

    /// <summary>
    /// Gets or sets the size of the rectangle.
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
    /// Gets the top coordinate of the rectangle.
    /// </summary>
    [Browsable(false)]
    public float Top => Y;

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleFP32"/> structure with the specified coordinates and dimensions.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public RectangleFP32(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleFP32"/> structure with the specified location and size.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="size">The size.</param>
    public RectangleFP32(PointFp32 location, SizeFp32 size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.Width;
        Height = size.Height;
    }

    /// <summary>
    /// Checks whether the specified point is inside the rectangle.
    /// </summary>
    /// <param name="x">The X coordinate of the point.</param>
    /// <param name="y">The Y coordinate of the point.</param>
    /// <returns><c>true</c> if the point is inside the rectangle; otherwise, <c>false</c>.</returns>
    public bool Contains(float x, float y) => X <= x && x < X + Width && Y <= y && y < Y + Height;

    /// <summary>
    /// Checks whether the specified point is inside the rectangle.
    /// </summary>
    /// <param name="pt">The point.</param>
    /// <returns><c>true</c> if the point is inside the rectangle; otherwise, <c>false</c>.</returns>
    public bool Contains(PointFp32 pt) => Contains(pt.X, pt.Y);

    /// <summary>
    /// Checks whether the specified rectangle is inside the current rectangle.
    /// </summary>
    /// <param name="rect">The rectangle.</param>
    /// <returns><c>true</c> if the rectangle is inside the current rectangle; otherwise, <c>false</c>.</returns>
    public bool Contains(RectangleFP32 rect) => X <= rect.X && rect.X + rect.Width <= X + Width && Y <= rect.Y && rect.Y + rect.Height <= Y + Height;

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj) => obj is RectangleFP32 rectangle && this == rectangle;

    /// <summary>
    /// Creates a <see cref="RectangleFP32"/> structure from the specified left, top, right, and bottom edge coordinates.
    /// </summary>
    /// <param name="left">The left edge X coordinate.</param>
    /// <param name="top">The top edge Y coordinate.</param>
    /// <param name="right">The right edge X coordinate.</param>
    /// <param name="bottom">The bottom edge Y coordinate.</param>
    /// <returns>A <see cref="RectangleFP32"/> structure with the specified edges.</returns>
    public static RectangleFP32 FromLTRB(float left, float top, float right, float bottom) => new RectangleFP32(left, top, right - left, bottom - top);

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code for this instance.</returns>
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

    /// <summary>
    /// Inflates the rectangle by the specified amounts.
    /// </summary>
    /// <param name="x">The amount to inflate in the X direction.</param>
    /// <param name="y">The amount to inflate in the Y direction.</param>
    public void Inflate(float x, float y)
    {
        X -= x;
        Y -= y;
        Width += 2 * x;
        Height += 2 * y;
    }

    /// <summary>
    /// Inflates the rectangle by the specified size.
    /// </summary>
    /// <param name="size">The size to inflate by.</param>
    public void Inflate(SizeFp32 size) => Inflate(size.Width, size.Height);

    /// <summary>
    /// Returns a rectangle inflated by the specified amounts.
    /// </summary>
    /// <param name="rect">The rectangle to inflate.</param>
    /// <param name="x">The amount to inflate in the X direction.</param>
    /// <param name="y">The amount to inflate in the Y direction.</param>
    /// <returns>The inflated rectangle.</returns>
    public static RectangleFP32 Inflate(RectangleFP32 rect, float x, float y)
    {
        var rectangle = rect;
        rectangle.Inflate(x, y);
        return rectangle;
    }

    /// <summary>
    /// Computes the intersection of the current rectangle with the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to intersect with.</param>
    public void Intersect(RectangleFP32 rect)
    {
        var rectangle = Intersect(this, rect);
        X = rectangle.X;
        Y = rectangle.Y;
        Width = rectangle.Width;
        Height = rectangle.Height;
    }

    /// <summary>
    /// Returns the intersection of two rectangles.
    /// </summary>
    /// <param name="a">The first rectangle.</param>
    /// <param name="b">The second rectangle.</param>
    /// <returns>The intersection of the two rectangles.</returns>
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
    /// Checks whether the current rectangle intersects with the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to check.</param>
    /// <returns><c>true</c> if the rectangles intersect; otherwise, <c>false</c>.</returns>
    public bool IntersectsWith(RectangleFP32 rect) => rect.X < X + Width && X < rect.X + rect.Width && rect.Y < Y + Height && Y < rect.Y + rect.Height;

    /// <summary>
    /// Offsets the rectangle by the specified amount.
    /// </summary>
    /// <param name="pos">The offset amount.</param>
    public void Offset(PointFp32 pos) => Offset(pos.X, pos.Y);

    /// <summary>
    /// Offsets the rectangle by the specified amounts.
    /// </summary>
    /// <param name="x">The offset in the X direction.</param>
    /// <param name="y">The offset in the Y direction.</param>
    public void Offset(float x, float y)
    {
        X += x;
        Y += y;
    }

    /// <summary>
    /// Determines whether two <see cref="RectangleFP32"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="RectangleFP32"/> instance.</param>
    /// <param name="right">The second <see cref="RectangleFP32"/> instance.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(RectangleFP32 left, RectangleFP32 right) => left.X == right.X && left.Y == right.Y && left.Width == right.Width && left.Height == right.Height;

    /// <summary>
    /// Implicitly converts a <see cref="System.Drawing.Rectangle"/> to a <see cref="RectangleFP32"/>.
    /// </summary>
    /// <param name="r">The <see cref="Rectangle"/> instance to convert.</param>
    public static implicit operator RectangleFP32(System.Drawing.Rectangle r) => new RectangleFP32(r.X, r.Y, r.Width, r.Height);

    /// <summary>
    /// Determines whether two <see cref="RectangleFP32"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="RectangleFP32"/> instance.</param>
    /// <param name="right">The second <see cref="RectangleFP32"/> instance.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(RectangleFP32 left, RectangleFP32 right) => !(left == right);

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{{X={X.ToString(CultureInfo.CurrentCulture)}, Y={Y.ToString(CultureInfo.CurrentCulture)}, Width={Width.ToString(CultureInfo.CurrentCulture)}, Height={Height.ToString(CultureInfo.CurrentCulture)}}}";

    /// <summary>
    /// Returns the union of two rectangles.
    /// </summary>
    /// <param name="a">The first rectangle.</param>
    /// <param name="b">The second rectangle.</param>
    /// <returns>The union of the two rectangles.</returns>
    public static RectangleFP32 Union(RectangleFP32 a, RectangleFP32 b)
    {
        var x1 = Math.Min(a.X, b.X);
        var x2 = Math.Max(a.X + a.Width, b.X + b.Width);
        var y1 = Math.Min(a.Y, b.Y);
        var y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);
        return new RectangleFP32(x1, y1, x2 - x1, y2 - y1);
    }
}
