using System.Numerics;

namespace Vorcyc.Mathematics.Calculus;

/// <summary>
/// 表示一个对偶数，用于前向模式自动微分。
/// </summary>
public struct DualNumber<T>
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>
    /// 函数值。
    /// </summary>
    public T Value;

    /// <summary>
    /// 导数值。
    /// </summary>
    public T Deriv;

    /// <summary>
    /// 使用指定的函数值和导数值初始化 <see cref="DualNumber{T}"/> 结构的新实例。
    /// </summary>
    public DualNumber(T value, T deriv = default)
    {
        Value = value;
        Deriv = deriv;
    }

    public static DualNumber<T> operator +(DualNumber<T> a, DualNumber<T> b) =>
        new(a.Value + b.Value, a.Deriv + b.Deriv);

    public static DualNumber<T> operator +(DualNumber<T> a, T b) =>
        new(a.Value + b, a.Deriv);

    public static DualNumber<T> operator +(T a, DualNumber<T> b) =>
        new(a + b.Value, b.Deriv);

    public static DualNumber<T> operator -(DualNumber<T> a, DualNumber<T> b) =>
        new(a.Value - b.Value, a.Deriv - b.Deriv);

    public static DualNumber<T> operator -(DualNumber<T> a, T b) =>
        new(a.Value - b, a.Deriv);

    public static DualNumber<T> operator -(T a, DualNumber<T> b) =>
        new(a - b.Value, -b.Deriv);

    public static DualNumber<T> operator -(DualNumber<T> a) =>
        new(-a.Value, -a.Deriv);

    public static DualNumber<T> operator *(DualNumber<T> a, DualNumber<T> b) =>
        new(a.Value * b.Value, a.Value * b.Deriv + a.Deriv * b.Value);

    public static DualNumber<T> operator *(DualNumber<T> a, T b) =>
        new(a.Value * b, a.Deriv * b);

    public static DualNumber<T> operator *(T a, DualNumber<T> b) =>
        new(a * b.Value, a * b.Deriv);

    public static DualNumber<T> operator /(DualNumber<T> a, DualNumber<T> b) =>
        new(a.Value / b.Value, (a.Deriv * b.Value - a.Value * b.Deriv) / (b.Value * b.Value));

    public static DualNumber<T> operator /(DualNumber<T> a, T b) =>
        new(a.Value / b, a.Deriv / b);

    public static DualNumber<T> operator /(T a, DualNumber<T> b) =>
        new(a / b.Value, -a * b.Deriv / (b.Value * b.Value));

    /// <summary>sin(x) 的对偶扩展。</summary>
    public static DualNumber<T> Sin(DualNumber<T> x) =>
        new(T.Sin(x.Value), x.Deriv * T.Cos(x.Value));

    /// <summary>cos(x) 的对偶扩展。</summary>
    public static DualNumber<T> Cos(DualNumber<T> x) =>
        new(T.Cos(x.Value), -x.Deriv * T.Sin(x.Value));

    /// <summary>exp(x) 的对偶扩展。</summary>
    public static DualNumber<T> Exp(DualNumber<T> x)
    {
        T v = T.Exp(x.Value);
        return new DualNumber<T>(v, x.Deriv * v);
    }

    /// <summary>ln(x) 的对偶扩展。</summary>
    public static DualNumber<T> Log(DualNumber<T> x) =>
        new(T.Log(x.Value), x.Deriv / x.Value);

    /// <summary>幂函数 base^exponent（exponent 为常数）的对偶扩展。</summary>
    public static DualNumber<T> Pow(DualNumber<T> baseValue, T exponent)
    {
        T v = T.Pow(baseValue.Value, exponent);
        return new DualNumber<T>(v, exponent * T.Pow(baseValue.Value, exponent - T.One) * baseValue.Deriv);
    }

    /// <summary>平方根的对偶扩展。</summary>
    public static DualNumber<T> Sqrt(DualNumber<T> x) =>
        new(T.Sqrt(x.Value), x.Deriv / (T.CreateChecked(2) * T.Sqrt(x.Value)));

    /// <summary>tan(x) 的对偶扩展。</summary>
    public static DualNumber<T> Tan(DualNumber<T> x)
    {
        T t = T.Tan(x.Value);
        return new DualNumber<T>(t, x.Deriv * (T.One + t * t));
    }

    /// <summary>asin(x) 的对偶扩展。</summary>
    public static DualNumber<T> Asin(DualNumber<T> x)
    {
        T s = T.Sqrt(T.One - x.Value * x.Value);
        return new DualNumber<T>(T.Asin(x.Value), x.Deriv / s);
    }

    /// <summary>atan(x) 的对偶扩展。</summary>
    public static DualNumber<T> Atan(DualNumber<T> x)
    {
        T d = T.One + x.Value * x.Value;
        return new DualNumber<T>(T.Atan(x.Value), x.Deriv / d);
    }

    /// <summary>atan2(y,x) 的对偶扩展（x,y 均为对偶数）。</summary>
    public static DualNumber<T> Atan2(DualNumber<T> y, DualNumber<T> x)
    {
        T r2 = x.Value * x.Value + y.Value * y.Value;
        T v = T.Atan2(y.Value, x.Value);
        T d = (x.Value * y.Deriv - y.Value * x.Deriv) / r2;
        return new DualNumber<T>(v, d);
    }

    /// <summary>|x| 的对偶扩展（x=0 处导数取 0）。</summary>
    public static DualNumber<T> Abs(DualNumber<T> x) =>
        new(T.Abs(x.Value), x.Value == T.Zero ? T.Zero : T.CopySign(T.One, x.Value) * x.Deriv);

    /// <summary>max(a,b) 的对偶扩展。</summary>
    public static DualNumber<T> Max(DualNumber<T> a, DualNumber<T> b) =>
        a.Value >= b.Value ? a : b;

    /// <summary>min(a,b) 的对偶扩展。</summary>
    public static DualNumber<T> Min(DualNumber<T> a, DualNumber<T> b) =>
        a.Value <= b.Value ? a : b;

    /// <summary>a^b（a 为对偶数，b 为常数）已在 <see cref="Pow"/> 中实现。</summary>
    public static DualNumber<T> Pow(DualNumber<T> a, DualNumber<T> b)
    {
        T v = T.Pow(a.Value, b.Value);
        T da = b.Value * T.Pow(a.Value, b.Value - T.One) * a.Deriv;
        T db = v * T.Log(a.Value) * b.Deriv;
        return new DualNumber<T>(v, da + db);
    }
}
