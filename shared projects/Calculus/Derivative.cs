using System.Numerics;

namespace Vorcyc.Mathematics.Calculus;

/// <summary>
/// 提供导数和偏导数计算的实例类，支持数值方法和自动微分。
/// </summary>
/// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的值类型。</typeparam>
public class Derivative<T> where T : struct, IFloatingPointIeee754<T>
{


    private readonly SingleVariableFunction<T>? _singleFunc;
    private readonly MultiVariableFunction<T>? _multiFunc;
    private readonly Func<DualNumber<T>, DualNumber<T>>? _singleADFunc;
    private readonly Func<ReadOnlySpan<DualNumber<T>>, DualNumber<T>>? _multiADFunc;
    private readonly bool _isMultiVariable;

    private readonly T _defaultH;
    private readonly T _minH;

    // 实例级缓存
    private readonly Dictionary<T, T> _singleCache;
    private readonly Dictionary<string, T> _multiCache;

    public enum Method { Forward, Backward, Central, CentralFourthOrder }

    /// <summary>
    /// 为单变量函数初始化 <see cref="Derivative{T}"/> 实例。
    /// </summary>
    /// <param name="func">单变量函数。</param>
    /// <param name="defaultH">默认步长。</param>
    public Derivative(SingleVariableFunction<T> func, T defaultH)
    {
        _singleFunc = func ?? throw new ArgumentNullException(nameof(func));
        _multiFunc = null;
        _singleADFunc = null;
        _multiADFunc = null;
        _isMultiVariable = false;
        _defaultH = defaultH;
        _minH = T.CreateChecked(1e-15);
        _singleCache = new Dictionary<T, T>(EqualityComparer<T>.Default);
        _multiCache = new Dictionary<string, T>();
    }

    /// <summary>
    /// 为多变量函数初始化 <see cref="Derivative{T}"/> 实例。
    /// </summary>
    /// <param name="func">多变量函数。</param>
    /// <param name="defaultH">默认步长。</param>
    public Derivative(MultiVariableFunction<T> func, T defaultH)
    {
        _multiFunc = func ?? throw new ArgumentNullException(nameof(func));
        _singleFunc = null;
        _singleADFunc = null;
        _multiADFunc = null;
        _isMultiVariable = true;
        _defaultH = defaultH;
        _minH = T.CreateChecked(1e-15);
        _singleCache = new Dictionary<T, T>(EqualityComparer<T>.Default);
        _multiCache = new Dictionary<string, T>();
    }

    /// <summary>
    /// 为单变量自动微分函数初始化 <see cref="Derivative{T}"/> 实例。
    /// </summary>
    /// <param name="func">单变量自动微分函数。</param>
    public Derivative(Func<DualNumber<T>, DualNumber<T>> func)
    {
        _singleADFunc = func ?? throw new ArgumentNullException(nameof(func));
        _singleFunc = null;
        _multiFunc = null;
        _multiADFunc = null;
        _isMultiVariable = false;
        _defaultH = T.CreateChecked(1e-7);
        _minH = T.CreateChecked(1e-15);
        _singleCache = new Dictionary<T, T>(EqualityComparer<T>.Default);
        _multiCache = new Dictionary<string, T>();
    }

    /// <summary>
    /// 为多变量自动微分函数初始化 <see cref="Derivative{T}"/> 实例。
    /// </summary>
    /// <param name="func">多变量自动微分函数。</param>
    public Derivative(Func<ReadOnlySpan<DualNumber<T>>, DualNumber<T>> func)
    {
        _multiADFunc = func ?? throw new ArgumentNullException(nameof(func));
        _singleFunc = null;
        _multiFunc = null;
        _singleADFunc = null;
        _isMultiVariable = true;
        _defaultH = T.CreateChecked(1e-7);
        _minH = T.CreateChecked(1e-15);
        _singleCache = new Dictionary<T, T>(EqualityComparer<T>.Default);
        _multiCache = new Dictionary<string, T>();
    }

    /// <summary>
    /// 计算单变量函数的导数（数值方法）。
    /// </summary>
    /// <param name="x">计算导数的点。</param>
    /// <param name="order">导数的阶数，默认为1。</param>
    /// <param name="h">步长，默认为null。</param>
    /// <param name="method">数值方法，默认为中央差分法。</param>
    /// <returns>导数值。</returns>
    public T Calculate(T x, int order = 1, T? h = null, Method method = Method.Central)
    {
        if (_isMultiVariable || _singleADFunc != null || _multiADFunc != null)
            throw new InvalidOperationException("此实例不支持数值单变量导数计算");
        if (order < 1) throw new ArgumentException("导数阶数必须大于等于 1", nameof(order));
        T step = h ?? _defaultH;
        if (step <= _minH) throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));
        return CalculateSingleDerivative(x, order, step, method);
    }

    private T CalculateSingleDerivative(T x, int order, T h, Method method)
    {
        T GetValue(T point)
        {
            if (!_singleCache.TryGetValue(point, out T value))
            {
                value = _singleFunc(point);
                _singleCache[point] = value;
            }
            return value;
        }

        if (order == 1)
        {
            T two = T.CreateChecked(2);
            T eight = T.CreateChecked(8);
            T twelve = T.CreateChecked(12);

            return method switch
            {
                Method.Forward => (GetValue(x + h) - GetValue(x)) / h,
                Method.Backward => (GetValue(x) - GetValue(x - h)) / h,
                Method.Central => (GetValue(x + h) - GetValue(x - h)) / (two * h),
                Method.CentralFourthOrder => (-GetValue(x + two * h) + eight * GetValue(x + h) - eight * GetValue(x - h) + GetValue(x - two * h)) / (twelve * h),
                _ => throw new ArgumentException("不支持的数值方法")
            };
        }
        return new Derivative<T>(t => CalculateSingleDerivative(t, order - 1, h, method), _defaultH)
            .Calculate(x, 1, h, method);
    }

    /// <summary>
    /// 计算多变量函数的偏导数（数值方法）。
    /// </summary>
    /// <param name="point">计算偏导数的点。</param>
    /// <param name="variableIndex">变量索引。</param>
    /// <param name="order">偏导数的阶数，默认为1。</param>
    /// <param name="h">步长，默认为null。</param>
    /// <param name="method">数值方法，默认为中央差分法。</param>
    /// <returns>偏导数值。</returns>
    public T CalculatePartial(Span<T> point, int variableIndex, int order = 1, T? h = null, Method method = Method.Central)
    {
        if (!_isMultiVariable || _singleADFunc != null || _multiADFunc != null)
            throw new InvalidOperationException("此实例不支持数值偏导数计算");
        if (point.IsEmpty) throw new ArgumentNullException(nameof(point));
        if (variableIndex < 0 || variableIndex >= point.Length) throw new ArgumentException("变量索引超出范围", nameof(variableIndex));
        if (order < 1) throw new ArgumentException("偏导数阶数必须大于等于 1", nameof(order));
        T step = h ?? _defaultH;
        if (step <= _minH) throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));
        return CalculatePartialDerivative(point, variableIndex, order, step, method);
    }

    private T CalculatePartialDerivative(Span<T> point, int variableIndex, int order, T h, Method method)
    {
        T GetValue(Span<T> p)
        {
            string key = string.Join(",", p.ToArray().Select(v => v.ToString()));
            if (!_multiCache.TryGetValue(key, out T value))
            {
                value = _multiFunc(p);
                _multiCache[key] = value;
            }
            return value;
        }

        if (order == 1)
        {
            T[] pointPlusH = point.ToArray();
            T[] pointMinusH = point.ToArray();
            pointPlusH[variableIndex] += h;
            if (method == Method.Central || method == Method.CentralFourthOrder) pointMinusH[variableIndex] -= h;

            T two = T.CreateChecked(2);
            T eight = T.CreateChecked(8);
            T twelve = T.CreateChecked(12);

            return method switch
            {
                Method.Forward => (GetValue(pointPlusH) - GetValue(point)) / h,
                Method.Backward => (GetValue(point) - GetValue(pointMinusH)) / h,
                Method.Central => (GetValue(pointPlusH) - GetValue(pointMinusH)) / (two * h),
                Method.CentralFourthOrder => (-GetValue(pointPlusH) + eight * GetValue(pointPlusH) - eight * GetValue(pointMinusH) + GetValue(pointMinusH)) / (twelve * h),
                _ => throw new ArgumentException("不支持的数值方法")
            };
        }

        T[] newPoint = point.ToArray();
        return new Derivative<T>(t =>
        {
            newPoint[variableIndex] = t;
            return CalculatePartialDerivative(newPoint, variableIndex, order - 1, h, method);
        }, _defaultH).Calculate(point[variableIndex], 1, h, method);
    }

    /// <summary>
    /// 使用自动微分计算单变量导数。
    /// </summary>
    /// <param name="x">计算导数的点。</param>
    /// <returns>导数值。</returns>
    public T CalculateAD(T x)
    {
        if (_singleADFunc == null) throw new InvalidOperationException("此实例不支持单变量自动微分");
        DualNumber<T> input = new DualNumber<T>(x, T.One);
        return _singleADFunc(input).Deriv;
    }

    /// <summary>
    /// 使用自动微分计算多变量偏导数。
    /// </summary>
    /// <param name="point">计算偏导数的点。</param>
    /// <param name="variableIndex">变量索引。</param>
    /// <returns>偏导数值。</returns>
    public T CalculatePartialAD(Span<T> point, int variableIndex)
    {
        if (_multiADFunc == null) throw new InvalidOperationException("此实例不支持多变量自动微分");
        if (point.IsEmpty) throw new ArgumentNullException(nameof(point));
        if (variableIndex < 0 || variableIndex >= point.Length) throw new ArgumentException("变量索引超出范围", nameof(variableIndex));

        DualNumber<T>[] inputs = new DualNumber<T>[point.Length];
        for (int i = 0; i < point.Length; i++)
        {
            inputs[i] = new DualNumber<T>(point[i], i == variableIndex ? T.One : T.Zero);
        }
        return _multiADFunc(inputs).Deriv;
    }

    /// <summary>
    /// 获取梯度（仅限自动微分）。
    /// </summary>
    /// <param name="point">计算梯度的点。</param>
    /// <returns>梯度向量。</returns>
    public T[] GradientAD(Span<T> point)
    {
        if (_multiADFunc == null) throw new InvalidOperationException("此实例不支持多变量自动微分");
        T[] gradient = new T[point.Length];
        for (int i = 0; i < point.Length; i++)
        {
            gradient[i] = CalculatePartialAD(point, i);
        }
        return gradient;
    }

    /// <summary>
    /// 清空实例的缓存。
    /// </summary>
    public void ClearCache()
    {
        _singleCache.Clear();
        _multiCache.Clear();
    }
}


//public static class DerivativeExample
//{
//    public static void Demo()
//    {
//        Console.WriteLine("=== 使用 double 测试 ===");
//        var deriv1 = new Derivative<double>(x => x * x, 1e-7);
//        Console.WriteLine($"f'(2) = {deriv1.Calculate(2.0)}");

//        var deriv2 = new Derivative<double>(x => x * x);
//        Console.WriteLine($"f'(2) with AD = {deriv2.CalculateAD(2.0)}");

//        var deriv3 = new Derivative<double>(args => args[0] * args[0] + args[1] * args[1], 1e-7);
//        double[] pointDArray = { 2.0, 3.0 };
//        Span<double> pointD = pointDArray.AsSpan();
//        Console.WriteLine($"∂f/∂x at (2,3) = {deriv3.CalculatePartial(pointD, 0)}");

//        var deriv4 = new Derivative<double>(args => args[0] * args[0] + args[1] * args[1]);
//        Console.WriteLine($"∂f/∂x at (2,3) with AD = {deriv4.CalculatePartialAD(pointD, 0)}");
//        Console.WriteLine($"∂f/∂y at (2,3) with AD = {deriv4.CalculatePartialAD(pointD, 1)}");

//        var gradD = deriv4.GradientAD(pointD);
//        Console.WriteLine($"Gradient at (2,3) with AD: ({gradD[0]}, {gradD[1]})");

//        Console.WriteLine("\n=== 使用 float 测试 ===");
//        var deriv5 = new Derivative<float>(x => x * x, 1e-4f);
//        Console.WriteLine($"f'(2) = {deriv5.Calculate(2.0f)}");

//        var deriv6 = new Derivative<float>(x => x * x);
//        Console.WriteLine($"f'(2) with AD = {deriv6.CalculateAD(2.0f)}");

//        var deriv7 = new Derivative<float>(args => args[0] * args[0] + args[1] * args[1], 1e-4f);
//        float[] pointFArray = { 2.0f, 3.0f };
//        Span<float> pointF = pointFArray.AsSpan();
//        Console.WriteLine($"∂f/∂x at (2,3) = {deriv7.CalculatePartial(pointF, 0)}");
//    }
//}