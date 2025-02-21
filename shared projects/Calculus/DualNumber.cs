using System.Numerics;

namespace Vorcyc.Mathematics.Calculus;

/// <summary>
/// 表示一个对偶数，用于前向模式自动微分。
/// </summary>
public struct DualNumber<T>
    where T : struct, INumber<T>
{
    /// <summary>
    /// 函数值。
    /// </summary>
    public T Value;  // 函数值

    /// <summary>
    /// 导数值。
    /// </summary>
    public T Deriv;  // 导数值

    /// <summary>
    /// 使用指定的函数值和导数值初始化 <see cref="DualNumber{T}"/> 结构的新实例。
    /// </summary>
    /// <param name="value">函数值。</param>
    /// <param name="deriv">导数值，默认为0。</param>
    public DualNumber(T value, T deriv = default)
    {
        Value = value;
        Deriv = deriv;
    }

    /// <summary>
    /// 重载加法运算符，用于对两个 <see cref="DualNumber{T}"/> 实例进行加法运算。
    /// </summary>
    /// <param name="a">第一个 <see cref="DualNumber{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="DualNumber{T}"/> 实例。</param>
    /// <returns>两个 <see cref="DualNumber{T}"/> 实例的和。</returns>
    public static DualNumber<T> operator +(DualNumber<T> a, DualNumber<T> b) =>
        new DualNumber<T>(a.Value + b.Value, a.Deriv + b.Deriv);

    /// <summary>
    /// 重载减法运算符，用于对两个 <see cref="DualNumber{T}"/> 实例进行减法运算。
    /// </summary>
    /// <param name="a">第一个 <see cref="DualNumber{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="DualNumber{T}"/> 实例。</param>
    /// <returns>两个 <see cref="DualNumber{T}"/> 实例的差。</returns>
    public static DualNumber<T> operator -(DualNumber<T> a, DualNumber<T> b) =>
        new DualNumber<T>(a.Value - b.Value, a.Deriv - b.Deriv);

    /// <summary>
    /// 重载乘法运算符，用于对两个 <see cref="DualNumber{T}"/> 实例进行乘法运算。
    /// </summary>
    /// <param name="a">第一个 <see cref="DualNumber{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="DualNumber{T}"/> 实例。</param>
    /// <returns>两个 <see cref="DualNumber{T}"/> 实例的积。</returns>
    public static DualNumber<T> operator *(DualNumber<T> a, DualNumber<T> b) =>
        new DualNumber<T>(a.Value * b.Value, a.Value * b.Deriv + a.Deriv * b.Value);

    /// <summary>
    /// 重载除法运算符，用于对两个 <see cref="DualNumber{T}"/> 实例进行除法运算。
    /// </summary>
    /// <param name="a">第一个 <see cref="DualNumber{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="DualNumber{T}"/> 实例。</param>
    /// <returns>两个 <see cref="DualNumber{T}"/> 实例的商。</returns>
    public static DualNumber<T> operator /(DualNumber<T> a, DualNumber<T> b) =>
        new DualNumber<T>(a.Value / b.Value, (a.Deriv * b.Value - a.Value * b.Deriv) / (b.Value * b.Value));
}
