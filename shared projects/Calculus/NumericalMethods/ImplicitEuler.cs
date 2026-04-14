using System.Numerics;

namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

/// <summary>
/// 隐式 Euler 法（后向 Euler），适用于刚性 ODE；标量方程用牛顿迭代解隐式步。
/// </summary>
public sealed class ImplicitEuler<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly DifferentialFunction<T> _func;
    private readonly T _fdStep;
    private readonly T _invFdStep;
    private readonly T _endTol;
    private readonly T _minDg;

    public ImplicitEuler(DifferentialFunction<T> func, T fdStep)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _fdStep = fdStep;
        _invFdStep = T.One / fdStep;
        _endTol = T.CreateChecked(1e-14);
        _minDg = T.CreateChecked(1e-15);
    }

    /// <summary>从 x0 积分到 xEnd。</summary>
    public T Solve(T x0, T y0, T xEnd, int steps = 100, T? h = null, int newtonIterations = 8)
    {
        if (steps < 1) throw new ArgumentException("步数必须大于等于 1", nameof(steps));
        if (x0 == xEnd) return y0;

        T step = h ?? (xEnd - x0) / T.CreateChecked(steps);
        T x = x0;
        T y = y0;
        bool forward = xEnd > x0;

        while (forward ? x < xEnd - _endTol : x > xEnd + _endTol)
        {
            T remaining = xEnd - x;
            T currentStep = T.Abs(remaining) < T.Abs(step) ? remaining : step;
            T xNext = x + currentStep;
            y = ImplicitStep(xNext, y, currentStep, newtonIterations);
            x = xNext;
        }

        return y;
    }

    private T ImplicitStep(T xNext, T yPrev, T h, int iterations)
    {
        T y = yPrev;
        for (int i = 0; i < iterations; i++)
        {
            T f = _func(xNext, y);
            T g = y - yPrev - h * f;
            T df = (_func(xNext, y + _fdStep) - f) * _invFdStep;
            T dg = T.One - h * df;
            if (T.Abs(dg) < _minDg)
                break;
            y -= g / dg;
        }

        return y;
    }
}
