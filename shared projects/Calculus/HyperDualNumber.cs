using System.Numerics;

namespace Vorcyc.Mathematics.Calculus;

/// <summary>
/// 超对偶数 u + ε₁u₁ + ε₂u₂ + ε₁ε₂u₁₂，用于二阶导数与混合偏导（ε₁²=ε₂²=0）。
/// </summary>
public struct HyperDualNumber<T>
    where T : struct, IFloatingPointIeee754<T>
{
    public T Value;
    public T E1;
    public T E2;
    public T E12;

    public HyperDualNumber(T value, T e1 = default, T e2 = default, T e12 = default)
    {
        Value = value;
        E1 = e1;
        E2 = e2;
        E12 = e12;
    }

    public static HyperDualNumber<T> operator +(HyperDualNumber<T> a, HyperDualNumber<T> b) =>
        new(a.Value + b.Value, a.E1 + b.E1, a.E2 + b.E2, a.E12 + b.E12);

    public static HyperDualNumber<T> operator +(HyperDualNumber<T> a, T b) =>
        new(a.Value + b, a.E1, a.E2, a.E12);

    public static HyperDualNumber<T> operator +(T a, HyperDualNumber<T> b) =>
        new(a + b.Value, b.E1, b.E2, b.E12);

    public static HyperDualNumber<T> operator -(HyperDualNumber<T> a, HyperDualNumber<T> b) =>
        new(a.Value - b.Value, a.E1 - b.E1, a.E2 - b.E2, a.E12 - b.E12);

    public static HyperDualNumber<T> operator -(HyperDualNumber<T> a, T b) =>
        new(a.Value - b, a.E1, a.E2, a.E12);

    public static HyperDualNumber<T> operator -(T a, HyperDualNumber<T> b) =>
        new(a - b.Value, -b.E1, -b.E2, -b.E12);

    public static HyperDualNumber<T> operator -(HyperDualNumber<T> a) =>
        new(-a.Value, -a.E1, -a.E2, -a.E12);

    public static HyperDualNumber<T> operator *(HyperDualNumber<T> a, HyperDualNumber<T> b) =>
        new(
            a.Value * b.Value,
            a.Value * b.E1 + a.E1 * b.Value,
            a.Value * b.E2 + a.E2 * b.Value,
            a.Value * b.E12 + a.E1 * b.E2 + a.E2 * b.E1 + a.E12 * b.Value);

    public static HyperDualNumber<T> operator *(HyperDualNumber<T> a, T b) =>
        new(a.Value * b, a.E1 * b, a.E2 * b, a.E12 * b);

    public static HyperDualNumber<T> operator *(T a, HyperDualNumber<T> b) =>
        new(a * b.Value, a * b.E1, a * b.E2, a * b.E12);

    public static HyperDualNumber<T> operator /(HyperDualNumber<T> a, HyperDualNumber<T> b)
    {
        T inv = T.One / b.Value;
        T inv2 = inv * inv;
        return new HyperDualNumber<T>(
            a.Value * inv,
            (a.E1 * b.Value - a.Value * b.E1) * inv2,
            (a.E2 * b.Value - a.Value * b.E2) * inv2,
            (a.E12 * b.Value - a.Value * b.E12 - a.E1 * b.E2 - a.E2 * b.E1 + T.CreateChecked(2) * a.Value * b.E1 * b.E2 * inv) * inv2);
    }

    public static HyperDualNumber<T> operator /(HyperDualNumber<T> a, T b) =>
        new(a.Value / b, a.E1 / b, a.E2 / b, a.E12 / b);

    public static HyperDualNumber<T> Sin(HyperDualNumber<T> x)
    {
        T s = T.Sin(x.Value);
        T c = T.Cos(x.Value);
        return new HyperDualNumber<T>(s, x.E1 * c, x.E2 * c, x.E12 * c - x.E1 * x.E2 * s);
    }

    public static HyperDualNumber<T> Cos(HyperDualNumber<T> x)
    {
        T s = T.Sin(x.Value);
        T c = T.Cos(x.Value);
        return new HyperDualNumber<T>(c, -x.E1 * s, -x.E2 * s, -x.E12 * s - x.E1 * x.E2 * c);
    }

    public static HyperDualNumber<T> Exp(HyperDualNumber<T> x)
    {
        T v = T.Exp(x.Value);
        return new HyperDualNumber<T>(v, x.E1 * v, x.E2 * v, (x.E12 + x.E1 * x.E2) * v);
    }

    public static HyperDualNumber<T> Log(HyperDualNumber<T> x)
    {
        T inv = T.One / x.Value;
        T inv2 = inv * inv;
        return new HyperDualNumber<T>(
            T.Log(x.Value),
            x.E1 * inv,
            x.E2 * inv,
            (x.E12 - x.E1 * x.E2) * inv2);
    }

    public static HyperDualNumber<T> Pow(HyperDualNumber<T> b, T exponent)
    {
        T v = T.Pow(b.Value, exponent);
        T coeff = exponent * T.Pow(b.Value, exponent - T.One);
        T coeff2 = exponent * (exponent - T.One) * T.Pow(b.Value, exponent - T.CreateChecked(2));
        return new HyperDualNumber<T>(
            v,
            coeff * b.E1,
            coeff * b.E2,
            coeff * b.E12 + coeff2 * b.E1 * b.E2);
    }

    public static HyperDualNumber<T> Sqrt(HyperDualNumber<T> x)
    {
        T s = T.Sqrt(x.Value);
        T halfInv = T.One / (T.CreateChecked(2) * s);
        T quadInv = -halfInv / (T.CreateChecked(2) * x.Value);
        return new HyperDualNumber<T>(
            s,
            x.E1 * halfInv,
            x.E2 * halfInv,
            x.E12 * halfInv + x.E1 * x.E2 * quadInv);
    }
}
