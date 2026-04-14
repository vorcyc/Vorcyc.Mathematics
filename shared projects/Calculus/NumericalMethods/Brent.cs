using System.Numerics;

namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

/// <summary>
/// 使用 Brent 法在区间 [a, b] 上稳健地求解 f(x) = 0。
/// </summary>
/// <typeparam name="T">浮点类型</typeparam>
public sealed class Brent<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly SingleVariableFunction<T> _func;
    private readonly T _half;
    private readonly T _two;
    private readonly T _three;

    /// <summary>
    /// 初始化 <see cref="Brent{T}"/> 实例。
    /// </summary>
    public Brent(SingleVariableFunction<T> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _half = T.CreateChecked(0.5);
        _two = T.CreateChecked(2);
        _three = T.CreateChecked(3);
    }

    /// <summary>
    /// 在 [a, b] 上求解，要求 f(a) 与 f(b) 异号。
    /// </summary>
    public T Solve(T a, T b, int maxIterations = 100, T? tolerance = null)
    {
        if (maxIterations < 1) throw new ArgumentException("迭代次数必须大于等于 1", nameof(maxIterations));
        T tol = tolerance ?? T.CreateChecked(1e-10);

        T fa = _func(a);
        T fb = _func(b);
        if (fa == T.Zero) return a;
        if (fb == T.Zero) return b;
        if (fa * fb > T.Zero)
            throw new ArgumentException("区间端点函数值必须异号", nameof(b));

        T c = a;
        T fc = fa;
        T d = b - a;
        T e = d;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            if (T.Abs(fc) < T.Abs(fb))
            {
                (a, b) = (b, a);
                (fa, fb) = (fb, fa);
                (c, fc) = (b, fb);
            }

            T m = _half * (c - b);
            T tol1 = _two * tol * T.Max(T.One, T.Abs(b)) + T.Epsilon;
            if (T.Abs(m) <= tol1 || fb == T.Zero)
                return b;

            if (T.Abs(e) >= tol1 && T.Abs(fa) > T.Abs(fb))
            {
                T s = fb / fa;
                T p, q;

                if (a == c)
                {
                    p = _two * m * s;
                    q = T.One - s;
                }
                else
                {
                    q = fa / fc;
                    T r = fb / fc;
                    p = s * (_two * m * q * (q - r) - (b - a) * (r - T.One));
                    q = (q - T.One) * (r - T.One) * (s - T.One);
                }

                if (p > T.Zero) q = -q;
                else p = -p;

                T min1 = _three * m * q - T.Abs(tol1 * q);
                T min2 = T.Abs(e * q);
                if (_two * p < T.Min(min1, min2))
                {
                    e = d;
                    d = p / q;
                }
                else
                {
                    d = m;
                    e = m;
                }
            }
            else
            {
                d = m;
                e = m;
            }

            a = b;
            fa = fb;

            if (T.Abs(d) > tol1)
                b += d;
            else
                b += m > T.Zero ? tol1 : -tol1;

            fb = _func(b);
            if (fb * fc > T.Zero)
            {
                c = a;
                fc = fa;
                e = b - a;
                d = e;
            }
        }

        return b;
    }
}
