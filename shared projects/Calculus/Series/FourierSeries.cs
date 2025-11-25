namespace Vorcyc.Mathematics.Calculus.Series;

using System.Numerics;


/// <summary>
/// 提供傅里叶级数展开计算的实例类，支持泛型浮点类型。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public class FourierSeries<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly SingleVariableFunction<T> _func;
    private readonly T _period;
    private readonly Integration<T> _integration;

    private readonly Dictionary<(bool, int), T> _coeffCache;

    public FourierSeries(SingleVariableFunction<T> func, T period, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _period = period;
        _integration = new Integration<T>(defaultH);
        _coeffCache = new Dictionary<(bool, int), T>();
    }

    public T Calculate(T x, int order = 5, int segments = 1000)
    {
        if (order < 0) throw new ArgumentException("阶数必须大于等于 0", nameof(order));

        T sum = GetFourierCoefficient(true, 0, segments) / T.CreateChecked(2);
        T twoPiOverT = T.CreateChecked(2 * Math.PI) / _period;

        for (int n = 1; n <= order; n++)
        {
            T a_n = GetFourierCoefficient(true, n, segments);
            T b_n = GetFourierCoefficient(false, n, segments);
            T nOmegaX = T.CreateChecked(n) * twoPiOverT * x;
            sum += a_n * T.Cos(nOmegaX) + b_n * T.Sin(nOmegaX);
        }
        return sum;
    }

    /// <summary>
    /// 获取傅里叶级数的系数（aₙ 或 bₙ）。
    /// </summary>
    /// <param name="isCosine">true 表示余弦系数 aₙ，false 表示正弦系数 bₙ</param>
    /// <param name="n">谐波阶数</param>
    /// <param name="segments">积分分段数，默认值为 1000</param>
    /// <returns>第 <paramref name="n"/> 阶傅里叶系数</returns>
    /// <exception cref="ArgumentException">当 <paramref name="n"/> 小于 0 或 <paramref name="segments"/> 小于 1 时抛出</exception>
    /// <exception cref="InvalidOperationException">当积分结果无效时抛出</exception>
    public T GetFourierCoefficient(bool isCosine, int n, int segments = 1000)
    {
        if (n < 0) throw new ArgumentException("阶数必须大于等于 0", nameof(n));
        if (segments < 1) throw new ArgumentException("分段数必须大于等于 1", nameof(segments));

        var key = (isCosine, n);
        if (!_coeffCache.TryGetValue(key, out T coeff))
        {
            T twoOverT = T.CreateChecked(2) / _period;
            T twoPiOverT = T.CreateChecked(2 * Math.PI) / _period;
            T nOmega = T.CreateChecked(n) * twoPiOverT;

            // 计算傅里叶系数
            coeff = _integration.Integrate(
                T.Zero, _period, segments,
                x => _func(x) * (isCosine ? T.Cos(nOmega * x) : T.Sin(nOmega * x))
            ) * twoOverT;

            if (T.IsNaN(coeff) || T.IsInfinity(coeff))
                throw new InvalidOperationException($"傅里叶系数 a_{n} 或 b_{n} 计算结果无效");

            _coeffCache[key] = coeff;
        }
        return coeff;
    }

    public SingleVariableFunction<T> GetSeries(int order = 5, int segments = 1000)
    {
        return x => Calculate(x, order, segments);
    }

    public void ClearCache()
    {
        _coeffCache.Clear();
        _integration.ClearCache();
    }
}