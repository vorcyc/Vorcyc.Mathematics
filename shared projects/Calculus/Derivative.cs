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

    private T[]? _pt;
    private T[]? _ptPlus;
    private T[]? _ptMinus;
    private T[]? _ptPlus2;
    private T[]? _ptMinus2;
    private int _ptDim;

    /// <summary>
    /// Specifies the finite difference method used for numerical differentiation.
    /// </summary>
    /// <remarks>Use this enumeration to select the desired finite difference scheme when performing numerical
    /// differentiation. The available methods include forward, backward, central, and a higher-accuracy central
    /// fourth-order approach. The choice of method affects the accuracy and stability of the derivative
    /// calculation.</remarks>
    public enum Method
    {
        /// <summary>
        /// Represents the forward direction or movement.
        /// </summary>
        Forward,
        /// <summary>
        /// Represents the backward direction or movement.
        /// </summary>
        Backward,
        /// <summary>
        /// Represents the central entity or component within the context of the application.
        /// </summary>
        Central,
        /// <summary>
        /// Represents a central fourth-order finite difference scheme or operator.
        /// </summary>
        /// <remarks>This type is typically used in numerical analysis or scientific computing scenarios
        /// where high-accuracy central difference approximations are required. It may be used for discretizing
        /// derivatives in partial differential equations or similar applications.</remarks>
        CentralFourthOrder
    }

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
        T floor = NumericalStep.Optimal(x, order);
        T step = h ?? floor;
        if (step < floor)
            step = floor;
        if (step <= _minH)
            step = T.Max(_defaultH, floor);
        if (step <= _minH)
            throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));

        if (order == 1 && method == Method.Central && h is null)
            return NumericalStep.RichardsonFirstOrder(x, _singleFunc!, step);

        if (order == 2 && method == Method.Central && h is null)
            return NumericalStep.RichardsonSecondOrder(x, _singleFunc!, step);

        if (order == 3 && method == Method.Central && h is null)
            return NumericalStep.RichardsonThirdOrder(x, _singleFunc!, step);

        if (order == 4 && method == Method.Central && h is null)
            return NumericalStep.RichardsonFourthOrder(x, _singleFunc!, step);

        return CalculateSingleDerivative(x, order, step, method);
    }

    private T CalculateSingleDerivative(T x, int order, T h, Method method)
    {
        SingleVariableFunction<T> func = _singleFunc!;

        if (method == Method.Central && order is >= 2 and <= 4)
        {
            T two = T.CreateChecked(2);
            T h2 = h * h;
            return order switch
            {
                2 => (func(x + h) - two * func(x) + func(x - h)) / h2,
                3 => (func(x + two * h) - two * func(x + h) + two * func(x - h) - func(x - two * h))
                     / (two * h2 * h),
                4 => (func(x + two * h) - T.CreateChecked(4) * func(x + h) + T.CreateChecked(6) * func(x)
                     - T.CreateChecked(4) * func(x - h) + func(x - two * h)) / (h2 * h2),
                _ => throw new ArgumentException("不支持的导数阶数")
            };
        }

        if (order == 1)
            return DifferentiateFirstOrder(x, func, h, method);

        SingleVariableFunction<T> lowerOrder = t => CalculateSingleDerivative(t, order - 1, h, method);
        return DifferentiateFirstOrder(x, lowerOrder, h, method);
    }

    private static T DifferentiateFirstOrder(T x, SingleVariableFunction<T> func, T h, Method method)
    {
        T two = T.CreateChecked(2);
        T eight = T.CreateChecked(8);
        T twelve = T.CreateChecked(12);

        return method switch
        {
            Method.Forward => (func(x + h) - func(x)) / h,
            Method.Backward => (func(x) - func(x - h)) / h,
            Method.Central => (func(x + h) - func(x - h)) / (two * h),
            Method.CentralFourthOrder => (-func(x + two * h) + eight * func(x + h) - eight * func(x - h) + func(x - two * h)) / (twelve * h),
            _ => throw new ArgumentException("不支持的数值方法")
        };
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
        T floor = NumericalStep.Optimal(point[variableIndex], order);
        T step = h ?? floor;
        if (step < floor)
            step = floor;
        if (step <= _minH)
            step = T.Max(_defaultH, floor);
        if (step <= _minH)
            throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));
        return CalculatePartialDerivative(point, variableIndex, order, step, method);
    }

    private T CalculatePartialDerivative(Span<T> point, int variableIndex, int order, T h, Method method)
    {
        int n = point.Length;
        EnsurePartialBuffers(n);
        T Eval(Span<T> p) => _multiFunc!(p);

        if (order == 1)
        {
            point.CopyTo(_ptPlus!);
            point.CopyTo(_ptMinus!);
            _ptPlus![variableIndex] += h;
            if (method == Method.Central || method == Method.CentralFourthOrder)
                _ptMinus![variableIndex] -= h;

            T two = T.CreateChecked(2);
            T eight = T.CreateChecked(8);
            T twelve = T.CreateChecked(12);

            if (method == Method.CentralFourthOrder)
            {
                point.CopyTo(_ptPlus2!);
                point.CopyTo(_ptMinus2!);
                _ptPlus2![variableIndex] += two * h;
                _ptMinus2![variableIndex] -= two * h;
                return (-Eval(_ptPlus2) + eight * Eval(_ptPlus)
                        - eight * Eval(_ptMinus) + Eval(_ptMinus2)) / (twelve * h);
            }

            return method switch
            {
                Method.Forward => (Eval(_ptPlus) - Eval(point)) / h,
                Method.Backward => (Eval(point) - Eval(_ptMinus)) / h,
                Method.Central => (Eval(_ptPlus) - Eval(_ptMinus)) / (two * h),
                _ => throw new ArgumentException("不支持的数值方法")
            };
        }

        if (method == Method.Central && order is >= 2 and <= 4)
        {
            T two = T.CreateChecked(2);
            T h2 = h * h;
            point.CopyTo(_pt!);
            point.CopyTo(_ptPlus!);
            point.CopyTo(_ptMinus!);
            _ptPlus![variableIndex] += h;
            _ptMinus![variableIndex] -= h;

            point.CopyTo(_ptPlus2!);
            point.CopyTo(_ptMinus2!);
            _ptPlus2![variableIndex] += two * h;
            _ptMinus2![variableIndex] -= two * h;

            return order switch
            {
                2 => (Eval(_ptPlus) - two * Eval(_pt) + Eval(_ptMinus)) / h2,
                3 => (Eval(_ptPlus2) - two * Eval(_ptPlus) + two * Eval(_ptMinus) - Eval(_ptMinus2)) / (two * h2 * h),
                4 => (Eval(_ptPlus2) - T.CreateChecked(4) * Eval(_ptPlus) + T.CreateChecked(6) * Eval(_pt)
                     - T.CreateChecked(4) * Eval(_ptMinus) + Eval(_ptMinus2)) / (h2 * h2),
                _ => throw new ArgumentException("不支持的导数阶数")
            };
        }

        T xi = point[variableIndex];
        point.CopyTo(_pt!);
        return NumericalStep.CentralFirst(xi, h, t =>
        {
            _pt![variableIndex] = t;
            return CalculatePartialDerivative(_pt, variableIndex, order - 1, h, method);
        });
    }

    private void EnsurePartialBuffers(int n)
    {
        if (n <= _ptDim && _pt is not null)
            return;

        _ptDim = n;
        _pt = new T[n];
        _ptPlus = new T[n];
        _ptMinus = new T[n];
        _ptPlus2 = new T[n];
        _ptMinus2 = new T[n];
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
    /// 使用超对偶数自动微分计算单变量二阶导数 f''(x)。
    /// </summary>
    public static T SecondDerivativeAD(T x, Func<HyperDualNumber<T>, HyperDualNumber<T>> func)
    {
        var input = new HyperDualNumber<T>(x, T.One, T.One, T.Zero);
        return func(input).E12;
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
    /// 计算多变量标量函数的数值梯度。
    /// </summary>
    /// <param name="point">计算梯度的点。</param>
    /// <param name="h">步长，默认为构造时的默认步长。</param>
    /// <param name="method">数值差分方法。</param>
    /// <returns>梯度向量。</returns>
    public T[] Gradient(Span<T> point, T? h = null, Method method = Method.Central)
    {
        if (point.IsEmpty) throw new ArgumentException("输入点不能为空", nameof(point));
        T[] gradient = new T[point.Length];
        Gradient(point, gradient, h, method);
        return gradient;
    }

    /// <summary>
    /// 将梯度写入 <paramref name="destination"/>，避免额外分配。
    /// </summary>
    public void Gradient(Span<T> point, Span<T> destination, T? h = null, Method method = Method.Central)
    {
        if (!_isMultiVariable || _multiFunc == null || _singleADFunc != null || _multiADFunc != null)
            throw new InvalidOperationException("此实例不支持数值梯度计算");
        if (point.IsEmpty) throw new ArgumentException("输入点不能为空", nameof(point));
        if (destination.Length < point.Length)
            throw new ArgumentException("目标缓冲区长度不足", nameof(destination));

        if (method == Method.Central)
        {
            GradientCentral(point, destination, h);
            return;
        }

        for (int i = 0; i < point.Length; i++)
            destination[i] = CalculatePartial(point, i, 1, h, method);
    }

    private void GradientCentral(Span<T> point, Span<T> destination, T? h)
    {
        int n = point.Length;
        EnsurePartialBuffers(n);
        MultiVariableFunction<T> eval = _multiFunc!;

        T step;
        if (h is not null)
        {
            step = h.GetValueOrDefault();
            if (step <= _minH)
                throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));
        }
        else
        {
            T scale = T.One;
            for (int i = 0; i < n; i++)
            {
                T ax = T.Abs(point[i]);
                if (ax > scale) scale = ax;
            }
            T floor = NumericalStep.OptimalMagnitude(scale, 1);
            step = floor;
            if (step <= _minH)
                step = T.Max(_defaultH, floor);
        }

        T invTwoStep = T.One / (T.CreateChecked(2) * step);
        T[] ptPlus = _ptPlus!;
        T[] ptMinus = _ptMinus!;

        for (int i = 0; i < n; i++)
        {
            point.CopyTo(ptPlus);
            point.CopyTo(ptMinus);
            ptPlus[i] += step;
            ptMinus[i] -= step;
            destination[i] = (eval(ptPlus) - eval(ptMinus)) * invTwoStep;
        }
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
    /// 清空实例的缓存（保留 API；当前无数值导数结果缓存）。
    /// </summary>
    public void ClearCache() { }
}