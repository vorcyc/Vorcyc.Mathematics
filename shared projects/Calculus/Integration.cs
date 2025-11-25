
namespace Vorcyc.Mathematics.Calculus;

using System.Numerics;

/// <summary>
/// 提供数值积分计算的实例类，支持泛型浮点类型。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public class Integration<T> where T : struct, IFloatingPointIeee754<T>
{

    private readonly T _defaultH;
    private readonly T _minH;

    private readonly Dictionary<T, T> _cache;

    public enum Method
    {
        Trapezoidal,
        Simpson
    }

    public Integration(T defaultH)
    {
        _defaultH = defaultH;
        _minH = T.CreateChecked(1e-15);
        _cache = new Dictionary<T, T>(EqualityComparer<T>.Default);
    }

    /// <summary>
    /// 计算定积分，从 <paramref name="a"/> 到 <paramref name="b"/>。
    /// </summary>
    /// <param name="a">积分下限</param>
    /// <param name="b">积分上限</param>
    /// <param name="n">分段数，默认值为 1000</param>
    /// <param name="func">被积函数</param>
    /// <param name="h">步长，可选，优先级高于默认步长</param>
    /// <param name="method">积分方法，默认为梯形法则</param>
    /// <returns>定积分值</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="func"/> 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">当 <paramref name="n"/> 小于 1、<paramref name="h"/> 过小或方法不支持时抛出</exception>
    public T Integrate(T a, T b, int n, SingleVariableFunction<T> func, T? h = null, Method method = Method.Trapezoidal)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        if (n < 1) throw new ArgumentException("分段数必须大于等于 1", nameof(n));
        T step = h ?? _defaultH;
        if (step <= _minH) throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));

        return method switch
        {
            Method.Trapezoidal => IntegrateTrapezoidal(a, b, n, func),
            Method.Simpson => IntegrateSimpson(a, b, n, func),
            _ => throw new ArgumentException("不支持的积分方法")
        };
    }

    private T IntegrateTrapezoidal(T a, T b, int n, SingleVariableFunction<T> func)
    {
        T GetValue(T x)
        {
            if (!_cache.TryGetValue(x, out T value))
            {
                value = func(x);
                _cache[x] = value;
            }
            return value;
        }

        T h = (b - a) / T.CreateChecked(n);
        T sum = (GetValue(a) + GetValue(b)) / T.CreateChecked(2);

        for (int i = 1; i < n; i++)
        {
            T x = a + T.CreateChecked(i) * h;
            sum += GetValue(x);
        }
        return sum * h;
    }

    private T IntegrateSimpson(T a, T b, int n, SingleVariableFunction<T> func)
    {
        if (n % 2 != 0) throw new ArgumentException("辛普森法则要求分段数为偶数", nameof(n));

        T GetValue(T x)
        {
            if (!_cache.TryGetValue(x, out T value))
            {
                value = func(x);
                _cache[x] = value;
            }
            return value;
        }

        T h = (b - a) / T.CreateChecked(n);
        T sum = GetValue(a) + GetValue(b);

        for (int i = 1; i < n; i++)
        {
            T x = a + T.CreateChecked(i) * h;
            sum += GetValue(x) * (i % 2 == 0 ? T.CreateChecked(2) : T.CreateChecked(4));
        }
        return sum * h / T.CreateChecked(3);
    }

    public void ClearCache()
    {
        _cache.Clear();
    }
}


///// <summary>
///// 提供 <see cref="Integration{T}"/> 类的使用示例。
///// </summary>
//public static class IntegrationExample
//{
//    public static void Demo()
//    {
//        Console.WriteLine("=== 使用 double 测试 ===");
//        var integ1 = new Integration<double>(x => x * x, 1e-7);
//        double result1 = integ1.Integrate(0.0, 1.0);
//        Console.WriteLine($"∫x² dx from 0 to 1 (Trapezoidal) = {result1}");

//        double result2 = integ1.Integrate(0.0, 1.0, method: Integration<double>.Method.Simpson);
//        Console.WriteLine($"∫x² dx from 0 to 1 (Simpson) = {result2}");

//        double indefResult = integ1.IndefiniteIntegrate(1.0);
//        Console.WriteLine($"F(1) = ∫x² dx from 0 to 1 = {indefResult}");

//        var indefFunc = integ1.GetIndefiniteIntegral();
//        Console.WriteLine($"F(2) = ∫x² dx from 0 to 2 = {indefFunc(2.0)}");

//        var integ2 = new Integration<double>(x => Math.Sin(x), 1e-7, 0.0);
//        double result3 = integ2.Integrate(0.0, Math.PI);
//        Console.WriteLine($"∫sin(x) dx from 0 to π (Trapezoidal) = {result3}");

//        double indefSin = integ2.IndefiniteIntegrate(Math.PI / 2);
//        Console.WriteLine($"F(π/2) = ∫sin(x) dx from 0 to π/2 = {indefSin}");

//        Console.WriteLine("\n=== 使用 float 测试 ===");
//        var integ3 = new Integration<float>(x => x * x, 1e-4f);
//        float result4 = integ3.Integrate(0.0f, 1.0f);
//        Console.WriteLine($"∫x² dx from 0 to 1 (Trapezoidal) = {result4}");

//        float indefFloat = integ3.IndefiniteIntegrate(1.0f);
//        Console.WriteLine($"F(1) = ∫x² dx from 0 to 1 = {indefFloat}");
//    }
//}