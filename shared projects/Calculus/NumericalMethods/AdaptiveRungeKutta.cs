namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

using System.Numerics;

/// <summary>
/// 基于 RK4 步长折半（Richardson 外推）的自适应常微分方程求解器。
/// </summary>
/// <typeparam name="T">浮点类型</typeparam>
public sealed class AdaptiveRungeKutta<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly RungeKutta<T> _rk;
    private readonly T _minStep;
    private readonly T _maxStep;
    private readonly T _safety;
    private readonly T _half;
    private readonly T _five;
    private readonly T _quarter;
    private readonly T _endTol;
    private readonly T _errorFloor;
    private readonly T _scaleFloor;

    private T _yMid;

    /// <summary>
    /// 初始化自适应 RK 求解器。
    /// </summary>
    public AdaptiveRungeKutta(DifferentialFunction<T> func, T? minStep = null, T? maxStep = null, T? safetyFactor = null)
    {
        _rk = new RungeKutta<T>(func, minStep ?? T.CreateChecked(1e-12));
        _minStep = minStep ?? T.CreateChecked(1e-12);
        _maxStep = maxStep ?? T.CreateChecked(1.0);
        _safety = safetyFactor ?? T.CreateChecked(0.9);
        _half = T.CreateChecked(0.5);
        _five = T.CreateChecked(5);
        _quarter = T.CreateChecked(0.25);
        _endTol = T.CreateChecked(1e-12);
        _errorFloor = T.CreateChecked(1e-10);
        _scaleFloor = T.CreateChecked(1e-15);
    }

    /// <summary>
    /// 从 x0 积分到 xEnd，返回终点 y 值。
    /// </summary>
    public T Solve(T x0, T y0, T xEnd, T relativeTolerance, T absoluteTolerance, T? initialStep = null)
    {
        if (x0 == xEnd) return y0;

        T x = x0;
        T y = y0;
        T h = initialStep ?? T.Min(_maxStep, T.Abs(xEnd - x0) / T.CreateChecked(10));
        h = T.CopySign(h, xEnd - x0);
        bool forward = xEnd > x0;
        int totalSteps = 0;

        while (forward ? x < xEnd - _endTol : x > xEnd + _endTol)
        {
            if (++totalSteps > 10_000)
                throw new InvalidOperationException("自适应积分超过最大步数");

            T remaining = xEnd - x;
            if (T.Abs(h) > T.Abs(remaining))
                h = remaining;

            if (!TryAdaptiveStep(x, y, h, relativeTolerance, absoluteTolerance, out T yTrial, out T scaledError, out T acceptedH))
                throw new InvalidOperationException("自适应步长低于最小步长，积分失败");

            x += acceptedH;
            y = yTrial;
            h = GrowStep(acceptedH, scaledError, h);
        }

        return y;
    }

    /// <summary>
    /// 自适应积分并记录轨迹。
    /// </summary>
    public OdeTrajectory<T> SolveTrajectory(
        T x0, T y0, T xEnd,
        T relativeTolerance, T absoluteTolerance,
        T? initialStep = null,
        OdeEvent<T>? odeEvent = null,
        int maxPoints = 10_000)
    {
        if (maxPoints < 1) throw new ArgumentException("maxPoints 必须大于等于 1", nameof(maxPoints));

        var xs = new T[maxPoints];
        var ys = new T[maxPoints];
        int count = 0;

        xs[count] = x0;
        ys[count] = y0;
        count++;

        if (x0 == xEnd)
            return Trim(xs, ys, count);

        if (odeEvent != null && odeEvent(x0, y0))
            return Trim(xs, ys, count);

        T x = x0;
        T y = y0;
        T h = initialStep ?? T.Min(_maxStep, T.Abs(xEnd - x0) / T.CreateChecked(10));
        h = T.CopySign(h, xEnd - x0);
        bool forward = xEnd > x0;
        int totalSteps = 0;

        while (forward ? x < xEnd - _endTol : x > xEnd + _endTol)
        {
            if (++totalSteps > 10_000)
                throw new InvalidOperationException("自适应积分超过最大步数");
            if (count >= maxPoints)
                break;

            T remaining = xEnd - x;
            if (T.Abs(h) > T.Abs(remaining))
                h = remaining;

            if (!TryAdaptiveStep(x, y, h, relativeTolerance, absoluteTolerance, out T yTrial, out T scaledError, out T acceptedH))
                throw new InvalidOperationException("自适应步长低于最小步长，积分失败");

            x += acceptedH;
            y = yTrial;

            xs[count] = x;
            ys[count] = y;
            count++;

            if (odeEvent != null && odeEvent(x, y))
                break;

            h = GrowStep(acceptedH, scaledError, h);
        }

        return Trim(xs, ys, count);
    }

    private T GrowStep(T acceptedH, T scaledError, T h) =>
        OdeStepControl.GrowStep(acceptedH, scaledError, h, _minStep, _maxStep, _safety, _five, _quarter, _errorFloor);

    private bool TryAdaptiveStep(
        T x, T y, T h,
        T relativeTolerance, T absoluteTolerance,
        out T yTrial, out T scaledError, out T acceptedH)
    {
        int attempts = 0;
        T step = h;
        do
        {
            T halfStep = step * _half;
            _yMid = _rk.Step(x, y, halfStep);
            T yFull = _rk.Step(x, y, step);
            yTrial = _rk.Step(x, _yMid, halfStep);
            T scale = absoluteTolerance + relativeTolerance * T.Max(T.Abs(y), T.Abs(yTrial));
            scaledError = T.Abs(yFull - yTrial) / T.Max(scale, _scaleFloor);

            if (scaledError <= T.One || attempts >= 15)
            {
                acceptedH = step;
                return true;
            }

            step *= _half;
            if (T.Abs(step) < _minStep)
            {
                yTrial = default;
                scaledError = default;
                acceptedH = default;
                return false;
            }
            attempts++;
        } while (true);
    }

    private static OdeTrajectory<T> Trim(T[] xs, T[] ys, int count)
    {
        if (count == xs.Length)
            return new OdeTrajectory<T>(xs, ys);

        var xTrim = new T[count];
        var yTrim = new T[count];
        Array.Copy(xs, xTrim, count);
        Array.Copy(ys, yTrim, count);
        return new OdeTrajectory<T>(xTrim, yTrim);
    }
}
