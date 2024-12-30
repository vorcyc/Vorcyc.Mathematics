namespace Vorcyc.Mathematics.Numerics;

using System.Numerics;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.Utilities;

/*
*  我原来的 C++ 版本是
*  从 https://stackoverflow.com/questions/2682725/int24-24-bit-integral-datatype 找的
*/

/// <summary>Represents a 3-byte, 24-bit signed integer.</summary>
/// <remarks>
/// <para>
/// This class behaves like most other intrinsic signed integers but allows a 3-byte, 24-bit integer implementation
/// that is often found in many digital-signal processing arenas and different kinds of protocol parsing.  A signed
/// 24-bit integer is typically used to save storage space on disk where its value range of -8388608 to 8388607 is
/// sufficient, but the signed Int16 value range of -32768 to 32767 is too small.
/// </para>
/// <para>
/// This structure uses an Int32 internally for storage and most other common expected integer functionality, so using
/// a 24-bit integer will not save memory.  However, if the 24-bit signed integer range (-8388608 to 8388607) suits your
/// data needs you can save disk space by only storing the three bytes that this integer actually consumes.  You can do
/// this by calling the Int24.GetBytes function to return a three byte binary array that can be serialized to the desired
/// destination and then calling the Int24.GetValue function to restore the Int24 value from those three bytes.
/// </para>
/// <para>
/// All the standard operators for the Int24 have been fully defined for use with both Int24 and Int32 signed integers;
/// you should find that without the exception Int24 can be compared and numerically calculated with an Int24 or Int32.
/// Necessary casting should be minimal and typical use should be very simple - just as if you are using any other native
/// signed integer.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public struct Int24
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
    private byte _upper;

    [FieldOffset(1)]
    private byte _middle;

    [FieldOffset(0)]
    private byte _low;


    private const int MaxValue_Int32 = 8388607;

    private const int MinValue_Int32 = -8388608;


    public Int24(byte upper, byte middle, byte low)
    {
        _upper = upper;
        _middle = middle;
        _low = low;
    }

    public Int24(int value)
    {
        if (value < MinValue_Int32 || value > MaxValue_Int32)
            throw new ArgumentOutOfRangeException($"The value must >={MinValue_Int32} and <={MaxValue_Int32}");

        _low = (byte)value;
        _middle = (byte)(value >> 8);
        _upper = (byte)(value >> 16);
    }

    //public static Int24_new MaxValue => new(127, 255, 255);
    public static Int24 MaxValue => new(0b_0111_1111, 0b_1111_1111, 0b_1111_1111);

    //23.11.7 最小值不对，修改了这个bug
    //public static Int24 MinValue => new(128, 0, 0);
    public static Int24 MinValue => new(0b_1000_0000, 0b_0000_0000, 0b_0000_0000);

    public static Int24 Zero => new(0, 0, 0);


    public static Int24 AdditiveIdentity => default;

    #region type conversion

    public static implicit operator int(Int24 value)
    {
        //if ((value._upper & 0x80).ToBool()) //! Is this a negative?  Then we need to siingn extend.
        //也就是高位 >= 128 就是负
        // 现在也可以用 Convert.ToBoolean() 替代
        if ((value._upper & 0b_1000_0000).ToBool()) //! Is this a negative?  Then we need to siingn extend.
            return (0xff << 24) | value._upper << 16 | value._middle << 8 | value._low << 0;
        else
            return value._low << 0 | (value._middle << 8) | (value._upper << 16);
    }

    public static implicit operator float(Int24 value)
    {
        return (int)value;
    }

    public static implicit operator Int24(int value)
    {
        var result = new Int24();
        if (BitConverter.IsLittleEndian)
        {
            result._upper = (byte)(value >> 16); //high是数组索引的0
            result._middle = (byte)(value >> 8);
            result._low = (byte)(value);
        }
        else
        {
            result._low = (byte)(value >> 16);
            result._middle = (byte)(value >> 8);
            result._upper = (byte)(value);
        }
        return result;
    }

    #endregion


    public static Int24 operator +(Int24 left, Int24 right)
    {
        int intValue = (int)left + (int)right;
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    public static Int24 operator -(Int24 left, Int24 right)
    {
        int intValue = (int)left - (int)right;
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    public static Int24 operator *(Int24 left, Int24 right)
    {
        int intValue = (int)left * (int)right;
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    public static Int24 operator /(Int24 left, Int24 right)
    {
        int intValue = (int)left / (int)right;
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    public static float operator /(Int24 left, float right)
    {
        return (float)left / right;
    }


    #region IComparable  , ICompareable<T>


    public int CompareTo(Int24 other)
    {
        return CompareTo((int)other);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;

        if (!(obj is int) && !(obj is Int24))
            throw new ArgumentException("Argument must be an Int32 or an Int24");

        int num = (int)obj;
        int t = (int)this;
        return (t < num ? -1 : (t > num ? 1 : 0));
    }



    #endregion


    #region IFormattable

    public override string ToString()
    {
        return ((int)this).ToString();
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ((int)this).ToString(format, formatProvider);
    }

    #endregion

}