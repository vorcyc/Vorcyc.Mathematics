namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

using System.Numerics;
using Vorcyc.Mathematics.Calculus;

/// <summary>
/// 使用四阶龙格-库塔法（RK4）求解常微分方程组 dy/dx = f(x, y)。
/// </summary>
/// <typeparam name="T">浮点类型</typeparam>
public sealed class RungeKuttaSystem<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly OdeSystemFunction<T> _func;
    private readonly T _two;
    private readonly T _six;
    private readonly T _invSix;

    private T[]? _k1;
    private T[]? _k2;
    private T[]? _k3;
    private T[]? _k4;
    private T[]? _temp;
    private int _bufDim;

    public RungeKuttaSystem(OdeSystemFunction<T> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _two = T.CreateChecked(2);
        _six = T.CreateChecked(6);
        _invSix = T.One / _six;
    }

    public T[] Solve(T x0, ReadOnlySpan<T> y0, T xEnd, int steps = 100, T? h = null)
    {
        if (steps < 1) throw new ArgumentException("步数必须大于等于 1", nameof(steps));
        if (y0.IsEmpty) throw new ArgumentException("初始状态不能为空", nameof(y0));

        var y = y0.ToArray();
        SolveInPlace(x0, y, xEnd, steps, h);
        return y;
    }

    /// <summary>
    /// 从 x0 积分到 xEnd，在 <paramref name="y"/> 中原地更新状态（调用前须含初始条件）。
    /// </summary>
    public void SolveInPlace(T x0, Span<T> y, T xEnd, int steps = 100, T? h = null)
    {
        if (steps < 1) throw new ArgumentException("步数必须大于等于 1", nameof(steps));
        if (y.IsEmpty) throw new ArgumentException("初始状态不能为空", nameof(y));
        if (x0 == xEnd) return;

        EnsureBuffers(y.Length);
        T step = h ?? (xEnd - x0) / T.CreateChecked(steps);
        T x = x0;
        bool forward = xEnd > x0;
        T endTol = T.CreateChecked(1e-14);

        while (forward ? x < xEnd - endTol : x > xEnd + endTol)
        {
            T remaining = xEnd - x;
            T currentStep = T.Abs(remaining) < T.Abs(step) ? remaining : step;
            StepInPlace(x, y, currentStep);
            x += currentStep;
        }
    }

    public OdeSystemTrajectory<T> SolveTrajectory(
        T x0, ReadOnlySpan<T> y0, T xEnd,
        int steps = 100,
        T? h = null,
        OdeSystemEvent<T>? odeEvent = null,
        int maxPoints = 10_000)
    {
        if (steps < 1) throw new ArgumentException("步数必须大于等于 1", nameof(steps));
        if (y0.IsEmpty) throw new ArgumentException("初始状态不能为空", nameof(y0));

        int dim = y0.Length;
        int cap = Math.Min(maxPoints, steps + 1);
        var xs = new T[cap];
        var states = new T[dim][];
        for (int i = 0; i < dim; i++)
            states[i] = new T[cap];

        var y = y0.ToArray();
        EnsureBuffers(dim);

        int count = 0;
        xs[count] = x0;
        for (int i = 0; i < dim; i++)
            states[i][count] = y[i];
        count++;

        if (x0 == xEnd)
            return TrimTrajectory(xs, states, dim, count);

        if (odeEvent != null && odeEvent(x0, y))
            return TrimTrajectory(xs, states, dim, count);

        T step = h ?? (xEnd - x0) / T.CreateChecked(steps);
        T x = x0;
        bool forward = xEnd > x0;
        T endTol = T.CreateChecked(1e-14);

        while (forward ? x < xEnd - endTol : x > xEnd + endTol)
        {
            if (count >= cap)
                break;

            T remaining = xEnd - x;
            T currentStep = T.Abs(remaining) < T.Abs(step) ? remaining : step;
            StepInPlace(x, y, currentStep);
            x += currentStep;

            xs[count] = x;
            for (int i = 0; i < dim; i++)
                states[i][count] = y[i];
            count++;

            if (odeEvent != null && odeEvent(x, y))
                break;
        }

        return TrimTrajectory(xs, states, dim, count);
    }

    public void Step(T x, ReadOnlySpan<T> y, T h, Span<T> output)
    {
        if (y.Length != output.Length)
            throw new ArgumentException("状态向量维数不匹配");

        EnsureBuffers(y.Length);
        y.CopyTo(output);
        StepInPlace(x, output, h);
    }

    private void StepInPlace(T x, Span<T> y, T step)
    {
        T half = step / _two;
        T stepOverSix = step * _invSix;
        T twoStepOverSix = _two * stepOverSix;
        T[] temp = _temp!;
        T[] k1 = _k1!;
        T[] k2 = _k2!;
        T[] k3 = _k3!;
        T[] k4 = _k4!;

        _func(x, y, k1);

        CalculusVectorOps.AssignPlusScaled(temp, y, k1, half);
        _func(x + half, temp, k2);

        CalculusVectorOps.AssignPlusScaled(temp, y, k2, half);
        _func(x + half, temp, k3);

        CalculusVectorOps.AssignPlusScaled(temp, y, k3, step);
        _func(x + step, temp, k4);

        CalculusVectorOps.Rk4Accumulate(y, k1, k2, k3, k4, stepOverSix, twoStepOverSix);
    }

    private void EnsureBuffers(int dim)
    {
        if (dim <= _bufDim && _k1 is not null)
            return;

        _bufDim = dim;
        _k1 = new T[dim];
        _k2 = new T[dim];
        _k3 = new T[dim];
        _k4 = new T[dim];
        _temp = new T[dim];
    }

    private static OdeSystemTrajectory<T> TrimTrajectory(T[] xs, T[][] states, int dim, int count)
    {
        if (count == xs.Length)
            return new OdeSystemTrajectory<T>(xs, states);

        var xTrim = new T[count];
        Array.Copy(xs, xTrim, count);
        var stateTrim = new T[dim][];
        for (int i = 0; i < dim; i++)
        {
            stateTrim[i] = new T[count];
            Array.Copy(states[i], stateTrim[i], count);
        }
        return new OdeSystemTrajectory<T>(xTrim, stateTrim);
    }
}
