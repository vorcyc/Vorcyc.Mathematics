namespace Vorcyc.Mathematics.Numerics;

using System.Numerics;
using System.Runtime.InteropServices;

/*
*  我原来的 C++ 版本是
*  从 https://stackoverflow.com/questions/2682725/int24-24-bit-integral-datatype 找的
*/

/// <summary>
/// Represents a 3-byte, 24-bit signed integer.
/// </summary>
/// <remarks>
/// <para>
/// This type behaves like most other built-in signed integers but enables a 3-byte, 24-bit
/// integer, an implementation commonly used in many digital signal processing domains and in
/// parsing various kinds of protocols. A signed 24-bit integer is often used to save storage
/// space on disk, with a value range of -8388608 to 8388607, where a signed Int16 with a range
/// of -32768 to 32767 is too small.
/// </para>
/// <para>
/// Internally this struct uses an Int32 for storage and most other common expected integer
/// functionality, so using a 24-bit integer does not save memory. However, if the 24-bit signed
/// integer range (-8388608 to 8388607) fits your data needs, you can save disk space by storing
/// only the three bytes that the integer actually consumes. You can call the Int24.GetBytes
/// function to return a three-byte binary array that can be serialized to the desired target,
/// and then call the Int24.GetValue function to restore the Int24 value from those three bytes.
/// </para>
/// <para>
/// All standard operators of Int24 are fully defined and work with both Int24 and Int32 signed
/// integers; you will find that, with few exceptions, an Int24 can be compared with and used in
/// numerical calculations against an Int24 or an Int32. The necessary type conversions are kept
/// to a minimum, and typical usage should be very simple - just like using any other native signed integer.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public readonly struct Int24
    : IMinMaxValue<Int24>
    , IAdditionOperators<Int24, Int24, Int24>
    , ISubtractionOperators<Int24, Int24, Int24>
    , IMultiplyOperators<Int24, Int24, Int24>
    , IDivisionOperators<Int24, Int24, Int24>
    , IDivisionOperators<Int24, float, float>
    , IAdditiveIdentity<Int24, Int24>
    , IComparable, IComparable<Int24>
    , IFormattable
{
    [FieldOffset(2)]
    private readonly byte _upper;
    [FieldOffset(1)]
    private readonly byte _middle;
    [FieldOffset(0)]
    private readonly byte _low;

    private const int MaxValue_Int32 = 8388607;
    private const int MinValue_Int32 = -8388608;

    /// <summary>
    /// Initializes a new instance of the <see cref="Int24"/> struct using the specified upper, middle, and low bytes.
    /// </summary>
    /// <param name="upper">The upper 8 bits.</param>
    /// <param name="middle">The middle 8 bits.</param>
    /// <param name="low">The low 8 bits.</param>
    public Int24(byte upper, byte middle, byte low)
    {
        _upper = upper;
        _middle = middle;
        _low = low;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Int24"/> struct using the specified integer value.
    /// </summary>
    /// <param name="value">The integer value to store.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is outside the range of <see cref="Int24"/>.</exception>
    public Int24(int value)
    {
        if (value < MinValue_Int32 || value > MaxValue_Int32)
            throw new ArgumentOutOfRangeException(nameof(value), $"The value must be >= {MinValue_Int32} and <= {MaxValue_Int32}");

        _low = (byte)value;
        _middle = (byte)(value >> 8);
        _upper = (byte)(value >> 16);
    }

    /// <summary>
    /// Gets the maximum value of <see cref="Int24"/>.
    /// </summary>
    public static Int24 MaxValue => new(0b_0111_1111, 0b_1111_1111, 0b_1111_1111);

    /// <summary>
    /// Gets the minimum value of <see cref="Int24"/>.
    /// </summary>
    public static Int24 MinValue => new(0b_1000_0000, 0b_0000_0000, 0b_0000_0000);

    /// <summary>
    /// Gets the <see cref="Int24"/> value that represents zero.
    /// </summary>
    public static Int24 Zero => new(0, 0, 0);

    /// <summary>
    /// Gets the additive identity of <see cref="Int24"/>.
    /// </summary>
    public static Int24 AdditiveIdentity => default;

    #region 类型转换

    /// <summary>
    /// Converts an <see cref="Int24"/> to an <see cref="int"/>.
    /// </summary>
    /// <param name="value">The <see cref="Int24"/> value to convert.</param>
    public static implicit operator int(Int24 value)
    {
        if ((value._upper & 0b_1000_0000) != 0) // Is this a negative? Then we need to sign extend.
            return (0xff << 24) | value._upper << 16 | value._middle << 8 | value._low;
        else
            return value._low | (value._middle << 8) | (value._upper << 16);
    }

    /// <summary>
    /// Converts an <see cref="Int24"/> to a <see cref="float"/>.
    /// </summary>
    /// <param name="value">The <see cref="Int24"/> value to convert.</param>
    public static implicit operator float(Int24 value)
    {
        return (int)value;
    }

    /// <summary>
    /// Converts an <see cref="int"/> to an <see cref="Int24"/>.
    /// </summary>
    /// <param name="value">The <see cref="int"/> value to convert.</param>
    public static implicit operator Int24(int value)
    {
        if (value < MinValue_Int32 || value > MaxValue_Int32)
            throw new ArgumentOutOfRangeException(nameof(value), $"The value must be >= {MinValue_Int32} and <= {MaxValue_Int32}");

        return new Int24((byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    #endregion

    /// <summary>
    /// Implements the addition of two <see cref="Int24"/> instances.
    /// </summary>
    /// <param name="left">The first <see cref="Int24"/> instance.</param>
    /// <param name="right">The second <see cref="Int24"/> instance.</param>
    /// <returns>The sum of the two <see cref="Int24"/> instances.</returns>
    /// <exception cref="OverflowException">Thrown when the result is outside the range of <see cref="Int24"/>.</exception>
    public static Int24 operator +(Int24 left, Int24 right)
    {
        int intValue = checked((int)left + (int)right);
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    /// <summary>
    /// Implements the subtraction of two <see cref="Int24"/> instances.
    /// </summary>
    /// <param name="left">The first <see cref="Int24"/> instance.</param>
    /// <param name="right">The second <see cref="Int24"/> instance.</param>
    /// <returns>The difference of the two <see cref="Int24"/> instances.</returns>
    /// <exception cref="OverflowException">Thrown when the result is outside the range of <see cref="Int24"/>.</exception>
    public static Int24 operator -(Int24 left, Int24 right)
    {
        int intValue = checked((int)left - (int)right);
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    /// <summary>
    /// Implements the multiplication of two <see cref="Int24"/> instances.
    /// </summary>
    /// <param name="left">The first <see cref="Int24"/> instance.</param>
    /// <param name="right">The second <see cref="Int24"/> instance.</param>
    /// <returns>The product of the two <see cref="Int24"/> instances.</returns>
    /// <exception cref="OverflowException">Thrown when the result is outside the range of <see cref="Int24"/>.</exception>
    public static Int24 operator *(Int24 left, Int24 right)
    {
        int intValue = checked((int)left * (int)right);
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    /// <summary>
    /// Implements the division of two <see cref="Int24"/> instances.
    /// </summary>
    /// <param name="left">The first <see cref="Int24"/> instance.</param>
    /// <param name="right">The second <see cref="Int24"/> instance.</param>
    /// <returns>The quotient of the two <see cref="Int24"/> instances.</returns>
    /// <exception cref="OverflowException">Thrown when the result is outside the range of <see cref="Int24"/>.</exception>
    public static Int24 operator /(Int24 left, Int24 right)
    {
        int intValue = checked((int)left / (int)right);
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    /// <summary>
    /// Implements the division of an <see cref="Int24"/> and a <see cref="float"/> instance.
    /// </summary>
    /// <param name="left">The first <see cref="Int24"/> instance.</param>
    /// <param name="right">The second <see cref="float"/> instance.</param>
    /// <returns>The quotient of the <see cref="Int24"/> and <see cref="float"/> instances.</returns>
    public static float operator /(Int24 left, float right)
    {
        return (float)left / right;
    }

    #region IComparable, IComparable<Int24>

    /// <summary>
    /// Compares the current instance with another <see cref="Int24"/> instance.
    /// </summary>
    /// <param name="other">The other <see cref="Int24"/> instance.</param>
    /// <returns>A value that indicates whether the current instance is less than, equal to, or greater than the other instance.</returns>
    public int CompareTo(Int24 other)
    {
        return CompareTo((int)other);
    }

    /// <summary>
    /// Compares the current instance with another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>A value that indicates whether the current instance is less than, equal to, or greater than the other object.</returns>
    /// <exception cref="ArgumentException">Thrown when the object is not an <see cref="int"/> or an <see cref="Int24"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;

        if (!(obj is int) && !(obj is Int24))
            throw new ArgumentException("Argument must be an Int32 or an Int24");

        int num = (int)obj;
        int t = (int)this;
        return t.CompareTo(num);
    }

    #endregion

    #region IFormattable

    /// <summary>
    /// Returns the string representation of the current instance.
    /// </summary>
    /// <returns>The string representation of the current instance.</returns>
    public override string ToString()
    {
        return ((int)this).ToString();
    }

    /// <summary>
    /// Returns the string representation of the current instance.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>The string representation of the current instance.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ((int)this).ToString(format, formatProvider);
    }

    #endregion
}