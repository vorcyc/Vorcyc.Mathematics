namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

using System.Numerics;
using Vorcyc.Mathematics.Calculus;

/// <summary>
/// 基于 RK4 步长折半的自适应常微分方程组求解器。
/// </summary>
/// <typeparam name="T">浮点类型</typeparam>
public sealed class AdaptiveRungeKuttaSystem<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly RungeKuttaSystem<T> _rk;
    private readonly T _minStep;
    private readonly T _maxStep;
    private readonly T _safety;
    private readonly T _half;
    private readonly T _four;
    private readonly T _quarter;
    private readonly T _endTol;
    private readonly T _errorFloor;
    private readonly T _scaleFloor;

    private T[]? _y;
    private T[]? _yFull;
    private T[]? _yHalf;
    private T[]? _yMid;
    private int _dim;

    public AdaptiveRungeKuttaSystem(OdeSystemFunction<T> func, T? minStep = null, T? maxStep = null, T? safetyFactor = null)
    {
        _rk = new RungeKuttaSystem<T>(func ?? throw new ArgumentNullException(nameof(func)));
        _minStep = minStep ?? T.CreateChecked(1e-8);
        _maxStep = maxStep ?? T.CreateChecked(1.0);
        _safety = safetyFactor ?? T.CreateChecked(0.9);
        _half = T.CreateChecked(0.5);
        _four = T.CreateChecked(4);
        _quarter = T.CreateChecked(0.25);
        _endTol = T.CreateChecked(1e-11);
        _errorFloor = T.CreateChecked(1e-8);
        _scaleFloor = T.CreateChecked(1e-15);
    }

    /// <summary>
    /// 从 x0 积分到 xEnd。
    /// </summary>
    public T[] Solve(T x0, ReadOnlySpan<T> y0, T xEnd, T relativeTolerance, T absoluteTolerance, T? initialStep = null)
    {
        if (y0.IsEmpty) throw new ArgumentException("初始状态不能为空", nameof(y0));
        if (x0 == xEnd) return y0.ToArray();

        EnsureBuffers(y0.Length);
        y0.CopyTo(_y!);
        IntegrateCore(x0, xEnd, relativeTolerance, absoluteTolerance, initialStep, odeEvent: null, maxPoints: 0);
        return (T[])_y!.Clone();
    }

    /// <summary>
    /// 自适应积分并记录轨迹。
    /// </summary>
    public OdeSystemTrajectory<T> SolveTrajectory(
        T x0, ReadOnlySpan<T> y0, T xEnd,
        T relativeTolerance, T absoluteTolerance,
        T? initialStep = null,
        OdeSystemEvent<T>? odeEvent = null,
        int maxPoints = 10_000)
    {
        if (y0.IsEmpty) throw new ArgumentException("初始状态不能为空", nameof(y0));
        if (maxPoints < 1) throw new ArgumentException("maxPoints 必须大于等于 1", nameof(maxPoints));

        int dim = y0.Length;
        EnsureBuffers(dim);

        var xs = new T[maxPoints];
        var states = new T[dim][];
        for (int i = 0; i < dim; i++)
            states[i] = new T[maxPoints];

        int count = 0;
        xs[count] = x0;
        for (int i = 0; i < dim; i++)
            states[i][count] = y0[i];
        count++;

        if (x0 == xEnd)
            return TrimTrajectory(xs, states, dim, count);

        if (odeEvent != null && odeEvent(x0, y0))
            return TrimTrajectory(xs, states, dim, count);

        y0.CopyTo(_y!);
        count = IntegrateCore(x0, xEnd, relativeTolerance, absoluteTolerance, initialStep, odeEvent, maxPoints, xs, states, count);
        return TrimTrajectory(xs, states, dim, count);
    }

    private int IntegrateCore(
        T x0, T xEnd,
        T relativeTolerance, T absoluteTolerance,
        T? initialStep,
        OdeSystemEvent<T>? odeEvent,
        int maxPoints,
        T[]? xs = null,
        T[][]? states = null,
        int count = 1)
    {
        T x = x0;
        T h = initialStep ?? T.Min(_maxStep, T.Abs(xEnd - x0) / T.CreateChecked(20));
        h = T.CopySign(h, xEnd - x0);
        bool forward = xEnd > x0;
        bool record = xs is not null && states is not null;
        int totalSteps = 0;

        while (forward ? x < xEnd - _endTol : x > xEnd + _endTol)
        {
            if (++totalSteps > 5_000)
                throw new InvalidOperationException("自适应积分超过最大步数");

            T remaining = xEnd - x;
            if (T.Abs(h) > T.Abs(remaining))
                h = remaining;

            T scaledError = T.Zero;
            int attempts = 0;
            bool accepted = false;

            while (!accepted && attempts < 12)
            {
                T halfH = h * _half;
                _rk.Step(x, _y!, h, _yFull!);
                _rk.Step(x, _y!, halfH, _yMid!);
                _rk.Step(x + halfH, _yMid!, halfH, _yHalf!);

                scaledError = CalculusVectorOps.ScaledMaxError(
                    _yFull!, _yHalf!, relativeTolerance, absoluteTolerance, _scaleFloor);
                if (scaledError <= T.One)
                {
                    _yHalf!.AsSpan().CopyTo(_y!);
                    accepted = true;
                    break;
                }

                h *= _half;
                if (T.Abs(h) < _minStep)
                {
                    _yFull!.AsSpan().CopyTo(_y!);
                    accepted = true;
                    scaledError = T.One;
                    break;
                }
                attempts++;
            }

            if (!accepted)
                _yFull!.AsSpan().CopyTo(_y!);

            x += h;

            if (record && count < maxPoints)
            {
                xs![count] = x;
                for (int i = 0; i < _dim; i++)
                    states![i][count] = _y![i];
                count++;

                if (odeEvent != null && odeEvent(x, _y!))
                    return count;
            }

            if (scaledError > T.Zero)
                h = OdeStepControl.GrowStep(h, scaledError, h, _minStep, _maxStep, _safety, _four, _quarter, _errorFloor);
        }

        return count;
    }

    private void EnsureBuffers(int dim)
    {
        if (dim <= _dim && _y is not null)
            return;

        _dim = dim;
        _y = new T[dim];
        _yFull = new T[dim];
        _yHalf = new T[dim];
        _yMid = new T[dim];
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
