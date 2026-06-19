namespace Vorcyc.Mathematics.Numerics;

using System;
using System.Numerics;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a 24-bit unsigned integer.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct UInt24
#if NET7_0_OR_GREATER
    : IMinMaxValue<UInt24>
    , IAdditionOperators<UInt24, UInt24, UInt24>
    , ISubtractionOperators<UInt24, UInt24, UInt24>
    , IMultiplyOperators<UInt24, UInt24, UInt24>
    , IDivisionOperators<UInt24, UInt24, UInt24>
    , IAdditiveIdentity<UInt24, UInt24>
#endif
{
    [FieldOffset(2)]
    private readonly byte _upper;
    [FieldOffset(1)]
    private readonly byte _middle;
    [FieldOffset(0)]
    private readonly byte _low;

    /// <summary>
    /// Initializes a new instance of the <see cref="UInt24"/> structure using the specified bytes.
    /// </summary>
    /// <param name="upper">The upper byte.</param>
    /// <param name="middle">The middle byte.</param>
    /// <param name="low">The low byte.</param>
    private UInt24(byte upper, byte middle, byte low)
    {
        _upper = upper;
        _middle = middle;
        _low = low;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UInt24"/> structure using the specified 32-bit unsigned integer.
    /// </summary>
    /// <param name="value">The 32-bit unsigned integer to convert.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is out of the range of a 24-bit unsigned integer.</exception>
    public UInt24(uint value)
    {
        if (value > MaxValue_UInt32)
            throw new ArgumentOutOfRangeException(nameof(value));
        _low = (byte)value;
        _middle = (byte)(value >> 8);
        _upper = (byte)(value >> 16);
    }

    private const uint MaxValue_UInt32 = 16777215;
    private const uint MinValue_UInt32 = 0;

    /// <summary>
    /// Gets the maximum value of a <see cref="UInt24"/>.
    /// </summary>
    public static UInt24 MaxValue => new UInt24(255, 255, 255);

    /// <summary>
    /// Gets the minimum value of a <see cref="UInt24"/>.
    /// </summary>
    public static UInt24 MinValue => new UInt24(0, 0, 0);

    /// <summary>
    /// Gets the additive identity of a <see cref="UInt24"/>.
    /// </summary>
    public static UInt24 AdditiveIdentity => default;

    #region 类型转换

    /// <summary>
    /// Implicitly converts a <see cref="UInt24"/> to a <see cref="uint"/>.
    /// </summary>
    /// <param name="value">The <see cref="UInt24"/> instance to convert.</param>
    public static implicit operator uint(UInt24 value)
    {
        return (uint)(value._low | (value._middle << 8) | (value._upper << 16));
    }

    /// <summary>
    /// Implicitly converts a <see cref="uint"/> to a <see cref="UInt24"/>.
    /// </summary>
    /// <param name="value">The <see cref="uint"/> value to convert.</param>
    public static implicit operator UInt24(uint value)
    {
        if (value > MaxValue_UInt32)
            throw new ArgumentOutOfRangeException(nameof(value));
        return new UInt24((byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    #endregion

    /// <summary>
    /// Returns the sum of two <see cref="UInt24"/> values.
    /// </summary>
    /// <param name="left">The first <see cref="UInt24"/> value.</param>
    /// <param name="right">The second <see cref="UInt24"/> value.</param>
    /// <returns>The sum of the two <see cref="UInt24"/> values.</returns>
    /// <exception cref="OverflowException">Thrown when the result is out of the range of a <see cref="UInt24"/>.</exception>
    public static UInt24 operator +(UInt24 left, UInt24 right)
    {
        uint uintValue = checked((uint)left + (uint)right);
        if (uintValue > MaxValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }

    /// <summary>
    /// Returns the difference of two <see cref="UInt24"/> values.
    /// </summary>
    /// <param name="left">The first <see cref="UInt24"/> value.</param>
    /// <param name="right">The second <see cref="UInt24"/> value.</param>
    /// <returns>The difference of the two <see cref="UInt24"/> values.</returns>
    /// <exception cref="OverflowException">Thrown when the result is out of the range of a <see cref="UInt24"/>.</exception>
    public static UInt24 operator -(UInt24 left, UInt24 right)
    {
        uint uintValue = checked((uint)left - (uint)right);
        if (uintValue > MaxValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }

    /// <summary>
    /// Returns the product of two <see cref="UInt24"/> values.
    /// </summary>
    /// <param name="left">The first <see cref="UInt24"/> value.</param>
    /// <param name="right">The second <see cref="UInt24"/> value.</param>
    /// <returns>The product of the two <see cref="UInt24"/> values.</returns>
    /// <exception cref="OverflowException">Thrown when the result is out of the range of a <see cref="UInt24"/>.</exception>
    public static UInt24 operator *(UInt24 left, UInt24 right)
    {
        uint uintValue = checked((uint)left * (uint)right);
        if (uintValue > MaxValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }

    /// <summary>
    /// Returns the quotient of two <see cref="UInt24"/> values.
    /// </summary>
    /// <param name="left">The first <see cref="UInt24"/> value.</param>
    /// <param name="right">The second <see cref="UInt24"/> value.</param>
    /// <returns>The quotient of the two <see cref="UInt24"/> values.</returns>
    /// <exception cref="OverflowException">Thrown when the result is out of the range of a <see cref="UInt24"/>.</exception>
    public static UInt24 operator /(UInt24 left, UInt24 right)
    {
        uint uintValue = checked((uint)left / (uint)right);
        if (uintValue > MaxValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }
}
