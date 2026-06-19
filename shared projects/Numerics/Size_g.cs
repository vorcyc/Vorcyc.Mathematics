namespace Vorcyc.Mathematics.Numerics;

/// <summary>
/// Represents a two-dimensional size structure with a width and a height.
/// </summary>
/// <typeparam name="T">The numeric type, which must implement the <see cref="System.Numerics.INumber{T}"/> interface.</typeparam>
public readonly struct Size<T>
    where T : System.Numerics.INumber<T>
{
    /// <summary>
    /// Gets the width.
    /// </summary>
    public T Width { get; }

    /// <summary>
    /// Gets the height.
    /// </summary>
    public T Height { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Size{T}"/> structure with the specified width and height.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Size(T width, T height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Deconstructs the current instance into its width and height.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void Deconstruct(out T width, out T height)
    {
        width = Width;
        height = Height;
    }

    /// <summary>
    /// Implements the addition operation of a size and a value.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="value">The value.</param>
    /// <returns>A new <see cref="Size{T}"/> instance whose width and height each have the specified value added.</returns>
    public static Size<T> operator +(Size<T> size, T value) => new Size<T>(size.Width + value, size.Height + value);

    /// <summary>
    /// Implements the subtraction operation of a size and a value.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="value">The value.</param>
    /// <returns>A new <see cref="Size{T}"/> instance whose width and height each have the specified value subtracted.</returns>
    public static Size<T> operator -(Size<T> size, T value) => new Size<T>(size.Width - value, size.Height - value);

    /// <summary>
    /// Implements the multiplication operation of a size and a value.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="value">The value.</param>
    /// <returns>A new <see cref="Size{T}"/> instance whose width and height are each multiplied by the specified value.</returns>
    public static Size<T> operator *(Size<T> size, T value) => new Size<T>(size.Width * value, size.Height * value);

    /// <summary>
    /// Implements the division operation of a size and a value.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="value">The value.</param>
    /// <returns>A new <see cref="Size{T}"/> instance whose width and height are each divided by the specified value.</returns>
    public static Size<T> operator /(Size<T> size, T value) => new Size<T>(size.Width / value, size.Height / value);

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"({Width},{Height})";
}
