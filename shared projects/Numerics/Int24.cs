namespace Vorcyc.Mathematics.Numerics;

using System.Numerics;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.Framework.Utilities;

/*
*  我原来的 C++ 版本是
*  从 https://stackoverflow.com/questions/2682725/int24-24-bit-integral-datatype 找的
*/

/// <summary>
/// 表示一个3字节、24位的有符号整数。
/// </summary>
/// <remarks>
/// <para>
/// 该类的行为类似于大多数其他内置的有符号整数，但允许实现一个3字节、24位的整数，
/// 这种实现通常在许多数字信号处理领域和不同类型的协议解析中使用。一个有符号的24位整数
/// 通常用于在磁盘上节省存储空间，其值范围为-8388608到8388607，但有符号的Int16值范围
/// 为-32768到32767太小。
/// </para>
/// <para>
/// 该结构内部使用Int32进行存储和大多数其他常见的预期整数功能，因此使用24位整数不会节省内存。
/// 但是，如果24位有符号整数范围（-8388608到8388607）适合您的数据需求，您可以通过仅存储
/// 该整数实际消耗的三个字节来节省磁盘空间。您可以调用Int24.GetBytes函数返回一个可以序列化
/// 到所需目标的三字节二进制数组，然后调用Int24.GetValue函数从这三个字节中恢复Int24值。
/// </para>
/// <para>
/// Int24的所有标准运算符都已完全定义，可与Int24和Int32有符号整数一起使用；您会发现，
/// 除了例外，Int24可以与Int24或Int32进行比较和数值计算。必要的类型转换应尽量减少，
/// 典型的使用应非常简单 - 就像使用任何其他本机有符号整数一样。
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
    /// 使用指定的高、中、低字节初始化 <see cref="Int24"/> 结构的新实例。
    /// </summary>
    /// <param name="upper">高8位。</param>
    /// <param name="middle">中间8位。</param>
    /// <param name="low">低8位。</param>
    public Int24(byte upper, byte middle, byte low)
    {
        _upper = upper;
        _middle = middle;
        _low = low;
    }

    /// <summary>
    /// 使用指定的整数值初始化 <see cref="Int24"/> 结构的新实例。
    /// </summary>
    /// <param name="value">要存储的整数值。</param>
    /// <exception cref="ArgumentOutOfRangeException">当值超出 <see cref="Int24"/> 的范围时引发。</exception>
    public Int24(int value)
    {
        if (value < MinValue_Int32 || value > MaxValue_Int32)
            throw new ArgumentOutOfRangeException(nameof(value), $"The value must be >= {MinValue_Int32} and <= {MaxValue_Int32}");

        _low = (byte)value;
        _middle = (byte)(value >> 8);
        _upper = (byte)(value >> 16);
    }

    /// <summary>
    /// 获取 <see cref="Int24"/> 的最大值。
    /// </summary>
    public static Int24 MaxValue => new(0b_0111_1111, 0b_1111_1111, 0b_1111_1111);

    /// <summary>
    /// 获取 <see cref="Int24"/> 的最小值。
    /// </summary>
    public static Int24 MinValue => new(0b_1000_0000, 0b_0000_0000, 0b_0000_0000);

    /// <summary>
    /// 获取表示零的 <see cref="Int24"/> 值。
    /// </summary>
    public static Int24 Zero => new(0, 0, 0);

    /// <summary>
    /// 获取 <see cref="Int24"/> 的加法单位元。
    /// </summary>
    public static Int24 AdditiveIdentity => default;

    #region 类型转换

    /// <summary>
    /// 将 <see cref="Int24"/> 转换为 <see cref="int"/>。
    /// </summary>
    /// <param name="value">要转换的 <see cref="Int24"/> 值。</param>
    public static implicit operator int(Int24 value)
    {
        if ((value._upper & 0b_1000_0000) != 0) // Is this a negative? Then we need to sign extend.
            return (0xff << 24) | value._upper << 16 | value._middle << 8 | value._low;
        else
            return value._low | (value._middle << 8) | (value._upper << 16);
    }

    /// <summary>
    /// 将 <see cref="Int24"/> 转换为 <see cref="float"/>。
    /// </summary>
    /// <param name="value">要转换的 <see cref="Int24"/> 值。</param>
    public static implicit operator float(Int24 value)
    {
        return (int)value;
    }

    /// <summary>
    /// 将 <see cref="int"/> 转换为 <see cref="Int24"/>。
    /// </summary>
    /// <param name="value">要转换的 <see cref="int"/> 值。</param>
    public static implicit operator Int24(int value)
    {
        if (value < MinValue_Int32 || value > MaxValue_Int32)
            throw new ArgumentOutOfRangeException(nameof(value), $"The value must be >= {MinValue_Int32} and <= {MaxValue_Int32}");

        return new Int24((byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    #endregion

    /// <summary>
    /// 实现两个 <see cref="Int24"/> 实例的加法运算。
    /// </summary>
    /// <param name="left">第一个 <see cref="Int24"/> 实例。</param>
    /// <param name="right">第二个 <see cref="Int24"/> 实例。</param>
    /// <returns>两个 <see cref="Int24"/> 实例的和。</returns>
    /// <exception cref="OverflowException">当结果超出 <see cref="Int24"/> 的范围时引发。</exception>
    public static Int24 operator +(Int24 left, Int24 right)
    {
        int intValue = checked((int)left + (int)right);
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    /// <summary>
    /// 实现两个 <see cref="Int24"/> 实例的减法运算。
    /// </summary>
    /// <param name="left">第一个 <see cref="Int24"/> 实例。</param>
    /// <param name="right">第二个 <see cref="Int24"/> 实例。</param>
    /// <returns>两个 <see cref="Int24"/> 实例的差。</returns>
    /// <exception cref="OverflowException">当结果超出 <see cref="Int24"/> 的范围时引发。</exception>
    public static Int24 operator -(Int24 left, Int24 right)
    {
        int intValue = checked((int)left - (int)right);
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    /// <summary>
    /// 实现两个 <see cref="Int24"/> 实例的乘法运算。
    /// </summary>
    /// <param name="left">第一个 <see cref="Int24"/> 实例。</param>
    /// <param name="right">第二个 <see cref="Int24"/> 实例。</param>
    /// <returns>两个 <see cref="Int24"/> 实例的积。</returns>
    /// <exception cref="OverflowException">当结果超出 <see cref="Int24"/> 的范围时引发。</exception>
    public static Int24 operator *(Int24 left, Int24 right)
    {
        int intValue = checked((int)left * (int)right);
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    /// <summary>
    /// 实现两个 <see cref="Int24"/> 实例的除法运算。
    /// </summary>
    /// <param name="left">第一个 <see cref="Int24"/> 实例。</param>
    /// <param name="right">第二个 <see cref="Int24"/> 实例。</param>
    /// <returns>两个 <see cref="Int24"/> 实例的商。</returns>
    /// <exception cref="OverflowException">当结果超出 <see cref="Int24"/> 的范围时引发。</exception>
    public static Int24 operator /(Int24 left, Int24 right)
    {
        int intValue = checked((int)left / (int)right);
        if (intValue > MaxValue_Int32 || intValue < MinValue_Int32)
            throw new OverflowException();
        return intValue;
    }

    /// <summary>
    /// 实现 <see cref="Int24"/> 和 <see cref="float"/> 实例的除法运算。
    /// </summary>
    /// <param name="left">第一个 <see cref="Int24"/> 实例。</param>
    /// <param name="right">第二个 <see cref="float"/> 实例。</param>
    /// <returns><see cref="Int24"/> 和 <see cref="float"/> 实例的商。</returns>
    public static float operator /(Int24 left, float right)
    {
        return (float)left / right;
    }

    #region IComparable, IComparable<Int24>

    /// <summary>
    /// 比较当前实例与另一个 <see cref="Int24"/> 实例。
    /// </summary>
    /// <param name="other">另一个 <see cref="Int24"/> 实例。</param>
    /// <returns>一个值，指示当前实例是否小于、等于或大于另一个实例。</returns>
    public int CompareTo(Int24 other)
    {
        return CompareTo((int)other);
    }

    /// <summary>
    /// 比较当前实例与另一个对象。
    /// </summary>
    /// <param name="obj">要比较的对象。</param>
    /// <returns>一个值，指示当前实例是否小于、等于或大于另一个对象。</returns>
    /// <exception cref="ArgumentException">当对象不是 <see cref="int"/> 或 <see cref="Int24"/> 时引发。</exception>
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
    /// 返回当前实例的字符串表示形式。
    /// </summary>
    /// <returns>当前实例的字符串表示形式。</returns>
    public override string ToString()
    {
        return ((int)this).ToString();
    }

    /// <summary>
    /// 返回当前实例的字符串表示形式。
    /// </summary>
    /// <param name="format">格式字符串。</param>
    /// <param name="formatProvider">格式提供程序。</param>
    /// <returns>当前实例的字符串表示形式。</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ((int)this).ToString(format, formatProvider);
    }

    #endregion
}