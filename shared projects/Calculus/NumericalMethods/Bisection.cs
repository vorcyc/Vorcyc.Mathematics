using System.Numerics;

namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

/// <summary>
/// 使用二分法求解 f(x) = 0 在区间 [a, b] 内的根。
/// </summary>
/// <typeparam name="T">浮点类型</typeparam>
public sealed class Bisection<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly SingleVariableFunction<T> _func;
    private readonly T _half;

    /// <summary>
    /// 初始化 <see cref="Bisection{T}"/> 实例。
    /// </summary>
    public Bisection(SingleVariableFunction<T> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _half = T.CreateChecked(0.5);
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

        T left = a;
        T right = b;

        for (int i = 0; i < maxIterations; i++)
        {
            T mid = (left + right) * _half;
            T fmid = _func(mid);

            if (T.Abs(fmid) < tol || T.Abs(right - left) < tol)
                return mid;

            if (fa * fmid < T.Zero)
            {
                right = mid;
                fb = fmid;
            }
            else
            {
                left = mid;
                fa = fmid;
            }
        }

        return (left + right) * _half;
    }
}
