namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

using System.Numerics;

/// <summary>
/// 使用龙格-库塔法（RK4）求解常微分方程 dy/dx = f(x,y) 的实例类，支持泛型浮点类型。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public sealed class RungeKutta<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly DifferentialFunction<T> _func;
    private readonly T _two;
    private readonly T _invSix;
    private readonly T _endTol;

    public RungeKutta(DifferentialFunction<T> func, T defaultH)
    {
        _ = defaultH;
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _two = T.CreateChecked(2);
        _invSix = T.One / T.CreateChecked(6);
        _endTol = T.CreateChecked(1e-15);
    }

    public T Solve(T x0, T y0, T xEnd, int steps = 100, T? h = null)
    {
        if (steps < 1) throw new ArgumentException("步数必须大于等于 1", nameof(steps));
        if (x0 == xEnd) return y0;

        T step = h ?? (xEnd - x0) / T.CreateChecked(steps);
        T x = x0;
        T y = y0;

        while (T.Abs(xEnd - x) > _endTol)
        {
            T remaining = xEnd - x;
            T currentStep = T.Abs(remaining) < T.Abs(step) ? remaining : step;
            y = Rk4Step(x, y, currentStep);
            x += currentStep;
        }

        return y;
    }

    public OdeTrajectory<T> SolveTrajectory(
        T x0, T y0, T xEnd,
        int steps = 100,
        T? h = null,
        OdeEvent<T>? odeEvent = null,
        int maxPoints = 10_000)
    {
        if (steps < 1) throw new ArgumentException("步数必须大于等于 1", nameof(steps));

        int cap = Math.Min(maxPoints, steps + 1);
        var xs = new T[cap];
        var ys = new T[cap];
        int count = 0;

        xs[count] = x0;
        ys[count] = y0;
        count++;

        if (x0 == xEnd)
            return TrimTrajectory(xs, ys, count);

        if (odeEvent != null && odeEvent(x0, y0))
            return TrimTrajectory(xs, ys, count);

        T step = h ?? (xEnd - x0) / T.CreateChecked(steps);
        T x = x0;
        T y = y0;

        while (T.Abs(xEnd - x) > _endTol)
        {
            if (count >= cap)
                break;

            T remaining = xEnd - x;
            T currentStep = T.Abs(remaining) < T.Abs(step) ? remaining : step;
            y = Rk4Step(x, y, currentStep);
            x += currentStep;

            xs[count] = x;
            ys[count] = y;
            count++;

            if (odeEvent != null && odeEvent(x, y))
                break;
        }

        return TrimTrajectory(xs, ys, count);
    }

    public T Step(T x, T y, T step) => Rk4Step(x, y, step);

    private T Rk4Step(T x, T y, T step)
    {
        T half = step / _two;
        T stepOverSix = step * _invSix;
        T twoStepOverSix = _two * stepOverSix;

        T k1 = _func(x, y);
        T k2 = _func(x + half, y + k1 * half);
        T k3 = _func(x + half, y + k2 * half);
        T k4 = _func(x + step, y + k3 * step);
        return y + stepOverSix * (k1 + k4) + twoStepOverSix * (k2 + k3);
    }

    private static OdeTrajectory<T> TrimTrajectory(T[] xs, T[] ys, int count)
    {
        if (count == xs.Length)
            return new OdeTrajectory<T>(xs, ys);

        var xTrim = new T[count];
        var yTrim = new T[count];
        Array.Copy(xs, xTrim, count);
        Array.Copy(ys, yTrim, count);
        return new OdeTrajectory<T>(xTrim, yTrim);
    }

    public void ClearCache() { }
}
