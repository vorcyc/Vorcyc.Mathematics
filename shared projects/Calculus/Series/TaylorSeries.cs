namespace Vorcyc.Mathematics.Calculus.Series;

using System.Numerics;

/// <summary>
/// 提供泰勒级数展开计算的实例类，支持泛型浮点类型。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public sealed class TaylorSeries<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly SingleVariableFunction<T> _func;
    private readonly T _center;
    private readonly Derivative<T> _derivative;
    private T[] _coefficients;
    private T[] _factorials;
    private int _cachedMaxOrder;

    /// <summary>
    /// 初始化 <see cref="TaylorSeries{T}"/> 实例。
    /// </summary>
    public TaylorSeries(SingleVariableFunction<T> func, T center, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _center = center;
        _derivative = new Derivative<T>(func, defaultH);
        _coefficients = new T[4];
        _factorials = [T.One];
        _cachedMaxOrder = -1;
    }

    /// <summary>
    /// 计算泰勒级数在指定点的值，截断到指定阶数（Horner 求值）。
    /// </summary>
    public T Calculate(T x, int order = 5)
    {
        if (order < 0) throw new ArgumentException("阶数必须大于等于 0", nameof(order));

        EnsureCoefficients(order);
        T dx = x - _center;
        T sum = _coefficients[order];
        for (int n = order - 1; n >= 0; n--)
            sum = sum * dx + _coefficients[n];
        return sum;
    }

    /// <summary>
    /// 获取泰勒级数的系数（导数值除以阶乘）。
    /// </summary>
    public T GetTaylorCoefficient(int order)
    {
        if (order < 0) throw new ArgumentException("阶数必须大于等于 0", nameof(order));
        EnsureCoefficients(order);
        return _coefficients[order];
    }

    /// <summary>
    /// 获取泰勒级数展开的函数。
    /// </summary>
    public SingleVariableFunction<T> GetSeries(int order = 5) => x => Calculate(x, order);

    private void EnsureCoefficients(int order)
    {
        if (_cachedMaxOrder >= order)
            return;

        if (_coefficients.Length <= order)
            Array.Resize(ref _coefficients, Math.Max(order + 1, _coefficients.Length * 2));

        if (_cachedMaxOrder < 0)
        {
            _coefficients[0] = _func(_center);
            _cachedMaxOrder = 0;
        }

        for (int n = _cachedMaxOrder + 1; n <= order; n++)
            _coefficients[n] = _derivative.Calculate(_center, n) / Factorial(n);

        _cachedMaxOrder = order;
    }

    private T Factorial(int n)
    {
        if (n <= 0) return T.One;
        EnsureFactorialTable(n);
        return _factorials[n];
    }

    private void EnsureFactorialTable(int n)
    {
        if (_factorials.Length > n)
            return;

        int oldLen = _factorials.Length;
        Array.Resize(ref _factorials, n + 1);
        for (int i = oldLen; i <= n; i++)
            _factorials[i] = _factorials[i - 1] * T.CreateChecked(i);
    }

    /// <summary>
    /// 清空导数缓存。
    /// </summary>
    public void ClearCache()
    {
        _cachedMaxOrder = -1;
        _derivative.ClearCache();
    }
}
