using System.Numerics;

namespace Vorcyc.Mathematics.Calculus;

/// <summary>
/// 数值微分/积分的步长选取与 Richardson 外推工具。
/// </summary>
internal static class NumericalStep
{
    /// <summary>
    /// 按机器精度与导数阶数估计最优差分步长：h ≈ ε^(1/(n+2)) · max(|x|, 1)。
    /// </summary>
    public static T Optimal<T>(T x, int order) where T : struct, IFloatingPointIeee754<T>
    {
        if (order < 1) throw new ArgumentException("阶数必须大于等于 1", nameof(order));
        return OptimalMagnitude(T.Max(T.One, T.Abs(x)), order);
    }

    /// <summary>
    /// 按已有量级 max(|x|, 1) 估计最优差分步长（梯度等批量场景只需计算一次）。
    /// </summary>
    public static T OptimalMagnitude<T>(T scale, int order) where T : struct, IFloatingPointIeee754<T>
    {
        if (order < 1) throw new ArgumentException("阶数必须大于等于 1", nameof(order));
        if (scale < T.One) scale = T.One;
        T exponent = T.One / T.CreateChecked(order + 2);
        return T.Pow(MachineEpsilon<T>(), exponent) * scale;
    }

    /// <summary>
    /// 返回浮点类型的机器精度 ε（非 <see cref="IFloatingPointIeee754{T}.Epsilon"/> 最小正数）。
    /// </summary>
    public static T MachineEpsilon<T>() where T : struct, IFloatingPointIeee754<T>
    {
        if (typeof(T) == typeof(double))
            return T.CreateTruncating(2.220446049250313e-16);
        if (typeof(T) == typeof(float))
            return T.CreateTruncating(1.192092896e-07f);
        if (typeof(T) == typeof(Half))
            return T.CreateTruncating(0.0009765625);

        T eps = T.One;
        T half = T.One / T.CreateChecked(2);
        while (T.One + eps / T.CreateChecked(2) != T.One)
            eps /= T.CreateChecked(2);
        return eps;
    }

    /// <summary>
    /// 对中心差分一阶导数做 Richardson 外推：R = (4·D(h/2) − D(h)) / 3。
    /// </summary>
    public static T RichardsonFirstOrder<T>(T x, SingleVariableFunction<T> func, T h) where T : struct, IFloatingPointIeee754<T>
    {
        T d1 = CentralFirst(x, h, func);
        T half = h / T.CreateChecked(2);
        T d2 = CentralFirst(x, half, func);
        T four = T.CreateChecked(4);
        T three = T.CreateChecked(3);
        return (four * d2 - d1) / three;
    }

    /// <summary>
    /// 对 O(h²) 中心差分二阶导数做 Richardson 外推。
    /// </summary>
    public static T RichardsonSecondOrder<T>(T x, SingleVariableFunction<T> func, T h) where T : struct, IFloatingPointIeee754<T>
    {
        T two = T.CreateChecked(2);
        T fx = func(x);
        T h2 = h * h;
        T d1 = (func(x + h) - two * fx + func(x - h)) / h2;

        T half = h / two;
        T half2 = half * half;
        T d2 = (func(x + half) - two * fx + func(x - half)) / half2;

        T four = T.CreateChecked(4);
        T three = T.CreateChecked(3);
        return (four * d2 - d1) / three;
    }

    public static T CentralFirst<T>(T x, T h, SingleVariableFunction<T> func) where T : struct, IFloatingPointIeee754<T>
    {
        T two = T.CreateChecked(2);
        return (func(x + h) - func(x - h)) / (two * h);
    }

    public static T CentralSecond<T>(T x, T h, SingleVariableFunction<T> func) where T : struct, IFloatingPointIeee754<T>
    {
        T two = T.CreateChecked(2);
        T h2 = h * h;
        return (func(x + h) - two * func(x) + func(x - h)) / h2;
    }

    public static T CentralThird<T>(T x, T h, SingleVariableFunction<T> func) where T : struct, IFloatingPointIeee754<T>
    {
        T two = T.CreateChecked(2);
        T h3 = h * h * h;
        return (func(x + two * h) - two * func(x + h) + two * func(x - h) - func(x - two * h)) / (two * h3);
    }

    public static T CentralFourth<T>(T x, T h, SingleVariableFunction<T> func) where T : struct, IFloatingPointIeee754<T>
    {
        T two = T.CreateChecked(2);
        T four = T.CreateChecked(4);
        T six = T.CreateChecked(6);
        T h4 = h * h * h * h;
        return (func(x + two * h) - four * func(x + h) + six * func(x)
                - four * func(x - h) + func(x - two * h)) / h4;
    }

    /// <summary>三阶中心差分 Richardson 外推（O(h²) 模板）；h 与 h/2 共用 6 个采样点。</summary>
    public static T RichardsonThirdOrder<T>(T x, SingleVariableFunction<T> func, T h) where T : struct, IFloatingPointIeee754<T>
    {
        T two = T.CreateChecked(2);
        T half = h * T.CreateChecked(0.5);
        T twoH = two * h;
        T h3 = two * h * h * h;

        T fm2h = func(x - twoH);
        T fmh = func(x - h);
        T fph = func(x + h);
        T fp2h = func(x + twoH);
        T d1 = (fp2h - two * fph + two * fmh - fm2h) / h3;

        T fmhh = func(x - half);
        T fphh = func(x + half);
        T half3 = two * half * half * half;
        T d2 = (fph - two * fphh + two * fmhh - fmh) / half3;

        T four = T.CreateChecked(4);
        T three = T.CreateChecked(3);
        return (four * d2 - d1) / three;
    }

    /// <summary>四阶中心差分 Richardson 外推（O(h⁴) 模板）；h 与 h/2 共用 7 个采样点。</summary>
    public static T RichardsonFourthOrder<T>(T x, SingleVariableFunction<T> func, T h) where T : struct, IFloatingPointIeee754<T>
    {
        T two = T.CreateChecked(2);
        T four = T.CreateChecked(4);
        T six = T.CreateChecked(6);
        T half = h * T.CreateChecked(0.5);
        T twoH = two * h;
        T h4 = h * h * h * h;

        T fm2h = func(x - twoH);
        T fmh = func(x - h);
        T fx = func(x);
        T fph = func(x + h);
        T fp2h = func(x + twoH);
        T d1 = (fp2h - four * fph + six * fx - four * fmh + fm2h) / h4;

        T fmhh = func(x - half);
        T fphh = func(x + half);
        T half4 = half * half * half * half;
        T d2 = (fph - four * fphh + six * fx - four * fmhh + fmh) / half4;

        T sixteen = T.CreateChecked(16);
        T fifteen = T.CreateChecked(15);
        return (sixteen * d2 - d1) / fifteen;
    }
}

