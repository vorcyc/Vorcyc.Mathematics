namespace Vorcyc.Mathematics.Calculus;

using System.Numerics;

/// <summary>
/// 提供数值极限计算的实例类，支持泛型浮点类型。
/// 支持从左侧、右侧或双侧趋近目标点计算极限。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public sealed class Limits<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly SingleVariableFunction<T> _func;
    private readonly T _defaultH;
    private readonly T _minH;
    private readonly T _half;
    private readonly T _two;
    private readonly T _aitkenEps;

    private T _s0;
    private T _s1;
    private T _s2;

    /// <summary>
    /// 极限趋近方向的枚举类型。
    /// </summary>
    public enum Direction
    {
        Left,
        Right,
        Both
    }

    /// <summary>
    /// 初始化 <see cref="Limits{T}"/> 实例。
    /// </summary>
    public Limits(SingleVariableFunction<T> func, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _defaultH = defaultH;
        _minH = T.CreateChecked(1e-15);
        _half = T.CreateChecked(0.5);
        _two = T.CreateChecked(2);
        _aitkenEps = T.CreateChecked(1e-20);
    }

    /// <summary>
    /// 计算函数在指定点的极限。
    /// </summary>
    public T CalculateLimit(T a, Direction direction = Direction.Both, int maxSteps = 100, T? tolerance = null, T? h = null)
    {
        if (maxSteps < 1) throw new ArgumentException("迭代步数必须大于等于 1", nameof(maxSteps));
        T step = h ?? _defaultH;
        if (step <= _minH) throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));
        T tol = tolerance ?? T.CreateChecked(1e-10);

        return direction switch
        {
            Direction.Left => CalculateOneSidedLimit(a, step, maxSteps, tol, fromLeft: true),
            Direction.Right => CalculateOneSidedLimit(a, step, maxSteps, tol, fromLeft: false),
            Direction.Both => CalculateBothLimit(a, step, maxSteps, tol),
            _ => throw new ArgumentException("不支持的趋近方向")
        };
    }

    private T CalculateOneSidedLimit(T a, T h, int maxSteps, T tolerance, bool fromLeft)
    {
        T currentH = h;
        T prev = T.Zero;
        T value = T.Zero;
        int history = 0;

        for (int i = 0; i < maxSteps; i++)
        {
            T x = fromLeft ? a - currentH : a + currentH;
            value = _func(x);

            if (i > 0)
            {
                T scale = T.Max(T.Max(T.Abs(value), T.Abs(prev)), T.One);
                if (T.Abs(value - prev) < tolerance * scale)
                    return value;
            }

            if (history < 3)
            {
                if (history == 0) _s0 = value;
                else if (history == 1) _s1 = value;
                else _s2 = value;
                history++;
            }
            else
            {
                _s0 = _s1;
                _s1 = _s2;
                _s2 = value;
            }

            if (history == 3)
            {
                T denom = _s0 - _two * _s1 + _s2;
                if (T.Abs(denom) > _aitkenEps)
                {
                    T delta = _s2 - _s1;
                    T aitken = _s2 - delta * delta / denom;
                    T scale = T.Max(T.Max(T.Abs(aitken), T.Abs(_s2)), T.One);
                    if (T.Abs(aitken - _s2) < tolerance * scale)
                        return aitken;
                }
            }

            prev = value;
            currentH *= _half;
            if (currentH < _minH)
                return value;
        }

        return value;
    }

    private T CalculateBothLimit(T a, T h, int maxSteps, T tolerance)
    {
        T leftLimit = CalculateOneSidedLimit(a, h, maxSteps, tolerance, fromLeft: true);
        T rightLimit = CalculateOneSidedLimit(a, h, maxSteps, tolerance, fromLeft: false);
        T scale = T.Max(T.Max(T.Abs(leftLimit), T.Abs(rightLimit)), T.One);

        if (T.Abs(leftLimit - rightLimit) < tolerance * scale)
            return (leftLimit + rightLimit) * _half;

        throw new ArgumentException("双侧极限不相等，极限不存在");
    }

    /// <summary>
    /// 清空实例缓存（保留 API 兼容性）。
    /// </summary>
    public void ClearCache() { }
}
