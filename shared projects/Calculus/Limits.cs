namespace Vorcyc.Mathematics.Calculus;

using System.Numerics;


/// <summary>
/// 提供数值极限计算的实例类，支持泛型浮点类型。
/// 支持从左侧、右侧或双侧趋近目标点计算极限。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public class Limits<T> where T : struct, IFloatingPointIeee754<T>
{

    private readonly SingleVariableFunction<T> _func; // 被计算极限的函数
    private readonly T _defaultH;                   // 默认初始步长
    private readonly T _minH;                      // 最小步长，防止浮点误差

    // 实例级缓存，用于存储函数值以避免重复计算
    private readonly Dictionary<T, T> _cache;

    /// <summary>
    /// 极限趋近方向的枚举类型。
    /// </summary>
    public enum Direction
    {
        /// <summary>从左侧趋近（x → a⁻）</summary>
        Left,
        /// <summary>从右侧趋近（x → a⁺）</summary>
        Right,
        /// <summary>双侧趋近（x → a），要求左右极限相等</summary>
        Both
    }

    /// <summary>
    /// 初始化 <see cref="Limits{T}"/> 实例。
    /// </summary>
    /// <param name="func">要计算极限的单变量函数</param>
    /// <param name="defaultH">默认初始步长，用于逼近目标点</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="func"/> 为 null 时抛出</exception>
    public Limits(SingleVariableFunction<T> func, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _defaultH = defaultH;
        _minH = T.CreateChecked(1e-15); // 设置最小步长为 1e-15
        _cache = new Dictionary<T, T>(EqualityComparer<T>.Default);
    }

    /// <summary>
    /// 计算函数在指定点的极限。
    /// </summary>
    /// <param name="a">目标点，极限趋近于此点</param>
    /// <param name="direction">趋近方向，默认为双侧趋近</param>
    /// <param name="maxSteps">最大迭代步数，默认值为 100</param>
    /// <param name="tolerance">收敛容差，默认值为 1e-10</param>
    /// <param name="h">初始步长，可选，优先级高于默认步长</param>
    /// <returns>极限值</returns>
    /// <exception cref="ArgumentException">当参数无效或极限不存在时抛出</exception>
    public T CalculateLimit(T a, Direction direction = Direction.Both, int maxSteps = 100, T? tolerance = null, T? h = null)
    {
        if (maxSteps < 1) throw new ArgumentException("迭代步数必须大于等于 1", nameof(maxSteps));
        T step = h ?? _defaultH; // 使用传入步长或默认步长
        if (step <= _minH) throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));
        T tol = tolerance ?? T.CreateChecked(1e-10); // 默认收敛容差

        return direction switch
        {
            Direction.Left => CalculateLeftLimit(a, step, maxSteps, tol),
            Direction.Right => CalculateRightLimit(a, step, maxSteps, tol),
            Direction.Both => CalculateBothLimit(a, step, maxSteps, tol),
            _ => throw new ArgumentException("不支持的趋近方向")
        };
    }

    // 计算左侧极限
    private T CalculateLeftLimit(T a, T h, int maxSteps, T tolerance)
    {
        T GetValue(T x)
        {
            if (!_cache.TryGetValue(x, out T value))
            {
                value = _func(x);
                _cache[x] = value;
            }
            return value;
        }

        T prevValue = T.Zero;
        T currentH = h;

        // 从左侧逐步逼近 a
        for (int i = 0; i < maxSteps; i++)
        {
            T x = a - currentH;
            T value = GetValue(x);

            // 检查收敛
            if (i > 0 && T.Abs(value - prevValue) < tolerance)
                return value;

            prevValue = value;
            currentH /= T.CreateChecked(2); // 步长减半
            if (currentH < _minH) break;
        }

        throw new ArgumentException("左侧极限未收敛或不存在");
    }

    // 计算右侧极限
    private T CalculateRightLimit(T a, T h, int maxSteps, T tolerance)
    {
        T GetValue(T x)
        {
            if (!_cache.TryGetValue(x, out T value))
            {
                value = _func(x);
                _cache[x] = value;
            }
            return value;
        }

        T prevValue = T.Zero;
        T currentH = h;

        // 从右侧逐步逼近 a
        for (int i = 0; i < maxSteps; i++)
        {
            T x = a + currentH;
            T value = GetValue(x);

            // 检查收敛
            if (i > 0 && T.Abs(value - prevValue) < tolerance)
                return value;

            prevValue = value;
            currentH /= T.CreateChecked(2); // 步长减半
            if (currentH < _minH) break;
        }

        throw new ArgumentException("右侧极限未收敛或不存在");
    }

    // 计算双侧极限
    private T CalculateBothLimit(T a, T h, int maxSteps, T tolerance)
    {
        // 计算左右极限
        T leftLimit = CalculateLeftLimit(a, h, maxSteps, tolerance);
        T rightLimit = CalculateRightLimit(a, h, maxSteps, tolerance);

        // 检查左右极限是否相等
        if (T.Abs(leftLimit - rightLimit) < tolerance)
            return (leftLimit + rightLimit) / T.CreateChecked(2); // 返回平均值

        throw new ArgumentException("双侧极限不相等，极限不存在");
    }

    /// <summary>
    /// 清空实例的函数值缓存。
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }
}