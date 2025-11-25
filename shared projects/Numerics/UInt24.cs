namespace Vorcyc.Mathematics.Numerics;

using System;
using System.Numerics;
using System.Runtime.InteropServices;

/// <summary>
/// 表示一个 24 位无符号整数。
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
    /// 使用指定的字节初始化 <see cref="UInt24"/> 结构体的新实例。
    /// </summary>
    /// <param name="upper">高位字节。</param>
    /// <param name="middle">中位字节。</param>
    /// <param name="low">低位字节。</param>
    private UInt24(byte upper, byte middle, byte low)
    {
        _upper = upper;
        _middle = middle;
        _low = low;
    }

    /// <summary>
    /// 使用指定的 32 位无符号整数初始化 <see cref="UInt24"/> 结构体的新实例。
    /// </summary>
    /// <param name="value">要转换的 32 位无符号整数。</param>
    /// <exception cref="ArgumentOutOfRangeException">当值超出 24 位无符号整数的范围时引发。</exception>
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
    /// 获取 <see cref="UInt24"/> 的最大值。
    /// </summary>
    public static UInt24 MaxValue => new UInt24(255, 255, 255);

    /// <summary>
    /// 获取 <see cref="UInt24"/> 的最小值。
    /// </summary>
    public static UInt24 MinValue => new UInt24(0, 0, 0);

    /// <summary>
    /// 获取 <see cref="UInt24"/> 的加法单位元。
    /// </summary>
    public static UInt24 AdditiveIdentity => default;

    #region 类型转换

    /// <summary>
    /// 将 <see cref="UInt24"/> 隐式转换为 <see cref="uint"/>。
    /// </summary>
    /// <param name="value">要转换的 <see cref="UInt24"/> 实例。</param>
    public static implicit operator uint(UInt24 value)
    {
        return (uint)(value._low | (value._middle << 8) | (value._upper << 16));
    }

    /// <summary>
    /// 将 <see cref="uint"/> 隐式转换为 <see cref="UInt24"/>。
    /// </summary>
    /// <param name="value">要转换的 <see cref="uint"/> 值。</param>
    public static implicit operator UInt24(uint value)
    {
        if (value > MaxValue_UInt32)
            throw new ArgumentOutOfRangeException(nameof(value));
        return new UInt24((byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    #endregion

    /// <summary>
    /// 返回两个 <see cref="UInt24"/> 值的和。
    /// </summary>
    /// <param name="left">第一个 <see cref="UInt24"/> 值。</param>
    /// <param name="right">第二个 <see cref="UInt24"/> 值。</param>
    /// <returns>两个 <see cref="UInt24"/> 值的和。</returns>
    /// <exception cref="OverflowException">当结果超出 <see cref="UInt24"/> 的范围时引发。</exception>
    public static UInt24 operator +(UInt24 left, UInt24 right)
    {
        uint uintValue = checked((uint)left + (uint)right);
        if (uintValue > MaxValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }

    /// <summary>
    /// 返回两个 <see cref="UInt24"/> 值的差。
    /// </summary>
    /// <param name="left">第一个 <see cref="UInt24"/> 值。</param>
    /// <param name="right">第二个 <see cref="UInt24"/> 值。</param>
    /// <returns>两个 <see cref="UInt24"/> 值的差。</returns>
    /// <exception cref="OverflowException">当结果超出 <see cref="UInt24"/> 的范围时引发。</exception>
    public static UInt24 operator -(UInt24 left, UInt24 right)
    {
        uint uintValue = checked((uint)left - (uint)right);
        if (uintValue > MaxValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }

    /// <summary>
    /// 返回两个 <see cref="UInt24"/> 值的积。
    /// </summary>
    /// <param name="left">第一个 <see cref="UInt24"/> 值。</param>
    /// <param name="right">第二个 <see cref="UInt24"/> 值。</param>
    /// <returns>两个 <see cref="UInt24"/> 值的积。</returns>
    /// <exception cref="OverflowException">当结果超出 <see cref="UInt24"/> 的范围时引发。</exception>
    public static UInt24 operator *(UInt24 left, UInt24 right)
    {
        uint uintValue = checked((uint)left * (uint)right);
        if (uintValue > MaxValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }

    /// <summary>
    /// 返回两个 <see cref="UInt24"/> 值的商。
    /// </summary>
    /// <param name="left">第一个 <see cref="UInt24"/> 值。</param>
    /// <param name="right">第二个 <see cref="UInt24"/> 值。</param>
    /// <returns>两个 <see cref="UInt24"/> 值的商。</returns>
    /// <exception cref="OverflowException">当结果超出 <see cref="UInt24"/> 的范围时引发。</exception>
    public static UInt24 operator /(UInt24 left, UInt24 right)
    {
        uint uintValue = checked((uint)left / (uint)right);
        if (uintValue > MaxValue_UInt32)
            throw new OverflowException();
        return uintValue;
    }
}
