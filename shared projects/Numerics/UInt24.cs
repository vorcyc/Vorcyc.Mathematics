namespace Vorcyc.Mathematics.Numerics;

using System;
using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct UInt24
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
    private byte _upper;
    [FieldOffset(1)]
    private byte _middle;
    [FieldOffset(0)]
    private byte _low;



    private UInt24(byte upper, byte middle, byte low)
    {
        _upper = upper;
        _middle = middle;
        _low = low;
    }

    public UInt24(uint value)
    {
        if (value < 0 || value > MaxValue_UInt32)
            throw new ArgumentOutOfRangeException("value");
        _low = (byte)value;
        _middle = (byte)(value >> 8);
        _upper = (byte)(value >> 16);
    }


    private const uint MaxValue_UInt32 = 16777215;
    private const uint MinValue_UInt32 = 0;

    public static UInt24 MaxValue => new UInt24(255, 255, 255);

    public static UInt24 MinValue => new UInt24(0, 0, 0);

    public static UInt24 AdditiveIdentity => default;

    #region type conversion

    public static implicit operator uint(UInt24 value)
    {
        return (uint)(value._low | (value._middle << 8) | (value._upper << 16));
    }

    public static implicit operator UInt24(uint value)
    {
        var result = new UInt24();
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


    public static UInt24 operator +(UInt24 left, UInt24 right)
    {
        uint uintValue = (uint)left + (uint)right;
        if (uintValue > MaxValue_UInt32 || uintValue < MinValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }

    public static UInt24 operator -(UInt24 left, UInt24 right)
    {
        uint uintValue = (uint)left - (uint)right;
        if (uintValue > MaxValue_UInt32 || uintValue < MinValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }

    public static UInt24 operator *(UInt24 left, UInt24 right)
    {
        uint uintValue = (uint)left * (uint)right;
        if (uintValue > MaxValue_UInt32 || uintValue < MinValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }

    public static UInt24 operator /(UInt24 left, UInt24 right)
    {
        uint uintValue = (uint)left / (uint)right;
        if (uintValue > MaxValue_UInt32 || uintValue < MinValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }
}
