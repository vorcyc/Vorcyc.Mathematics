namespace Vorcyc.Mathematics.Calculus.Series; 

using System.Numerics;


/// <summary>
/// 提供泰勒级数展开计算的实例类，支持泛型浮点类型。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public class TaylorSeries<T> where T : struct, IFloatingPointIeee754<T>
{

    private readonly SingleVariableFunction<T> _func; // 被展开的函数
    private readonly T _center;                    // 展开中心点
    private readonly Derivative<T> _derivative;    // 用于计算导数

    // 缓存导数值，避免重复计算
    private readonly Dictionary<int, T> _derivativeCache;

    /// <summary>
    /// 初始化 <see cref="TaylorSeries{T}"/> 实例。
    /// </summary>
    /// <param name="func">要展开的单变量函数</param>
    /// <param name="center">泰勒级数的展开中心点</param>
    /// <param name="defaultH">导数计算的默认步长</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="func"/> 为 null 时抛出</exception>
    public TaylorSeries(SingleVariableFunction<T> func, T center, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _center = center;
        _derivative = new Derivative<T>(func, defaultH);
        _derivativeCache = new Dictionary<int, T>();
    }

    /// <summary>
    /// 计算泰勒级数在指定点的值，截断到指定阶数。
    /// </summary>
    /// <param name="x">计算点</param>
    /// <param name="order">最高阶数，默认为 5</param>
    /// <returns>泰勒级数近似值</returns>
    public T Calculate(T x, int order = 5)
    {
        if (order < 0) throw new ArgumentException("阶数必须大于等于 0", nameof(order));

        T sum = T.Zero;
        T xMinusCenter = x - _center;

        // 计算泰勒级数：f(a) + f'(a)(x-a)/1! + f''(a)(x-a)²/2! + ...
        for (int n = 0; n <= order; n++)
        {
            T coef = GetTaylorCoefficient(n); // 导数除以阶乘
            T term = coef * Pow(xMinusCenter, n);
            sum += term;
        }
        return sum;
    }

    /// <summary>
    /// 获取泰勒级数的系数（导数值除以阶乘）。
    /// </summary>
    /// <param name="order">阶数</param>
    /// <returns>第 <paramref name="order"/> 阶泰勒系数</returns>
    public T GetTaylorCoefficient(int order)
    {
        if (!_derivativeCache.TryGetValue(order, out T coef))
        {
            if (order == 0)
            {
                // 第 0 阶，直接使用函数值
                coef = _func(_center); // Factorial(0) = 1，所以这里不用除以阶乘
            }
            else
            {
                // 第 1 阶及以上，使用导数
                T deriv = _derivative.Calculate(_center, order);
                coef = deriv / Factorial(order);
            }
            _derivativeCache[order] = coef;
        }
        return coef;
    }
    /// <summary>
    /// 获取泰勒级数展开的函数。
    /// </summary>
    /// <param name="order">最高阶数，默认为 5</param>
    /// <returns>表示泰勒级数的函数</returns>
    public SingleVariableFunction<T> GetSeries(int order = 5)
    {
        return x => Calculate(x, order);
    }

    // 计算 x 的 n 次幂
    private T Pow(T x, int n)
    {
        T result = T.One;
        for (int i = 0; i < n; i++)
        {
            result *= x;
        }
        return result;
    }

    // 计算阶乘
    private T Factorial(int n)
    {
        if (n <= 1) return T.One;
        T result = T.One;
        for (int i = 2; i <= n; i++)
        {
            result *= T.CreateChecked(i);
        }
        return result;
    }

    /// <summary>
    /// 清空导数缓存。
    /// </summary>
    public void ClearCache()
    {
        _derivativeCache.Clear();
        _derivative.ClearCache();
    }
}