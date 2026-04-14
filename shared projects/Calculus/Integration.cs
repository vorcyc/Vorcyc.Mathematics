
namespace Vorcyc.Mathematics.Calculus;

using System.Numerics;

/// <summary>
/// 提供数值积分计算的实例类，支持泛型浮点类型。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public class Integration<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly SingleVariableFunction<T>? _boundFunc;
    private readonly T _lowerBound;
    private readonly T _defaultH;
    private readonly T _minH;
    private readonly T _half;
    private readonly T _two;
    private readonly T _three;
    private readonly T _four;
    private readonly T _six;
    private readonly T _fifteen;
    private T[,]? _romberg;
    private int _rombergCapacity;
    private SimpsonFrame[]? _simpsonStack;
    private int _simpsonStackCapacity;

    private T[]? _indefNodes;
    private T[]? _indefFx;
    private T[]? _indefCum;
    private T _indefA;
    private T _indefUpper;
    private int _indefN;

    private int _glOrder = -1;
    private T[]? _glNodesT;
    private T[]? _glWeightsT;

    private SingleVariableFunction<T>? _improperSource;
    private T _improperA;
    private T _improperScale;
    private readonly T _improperSafeFloor;

    private struct SimpsonFrame
    {
        public T A;
        public T B;
        public T Fa;
        public T Fb;
        public T Fc;
        public T Whole;
        public T Tol;
        public int Depth;
    }

    public enum Method
    {
        Trapezoidal,
        Simpson,
        Romberg,
        GaussLegendre,
        AdaptiveSimpson
    }

    /// <summary>
    /// 使用默认步长初始化积分器（每次积分需传入被积函数）。
    /// </summary>
    public Integration(T defaultH)
    {
        _defaultH = defaultH;
        _minH = T.CreateChecked(1e-15);
        _half = T.CreateChecked(0.5);
        _two = T.CreateChecked(2);
        _three = T.CreateChecked(3);
        _four = T.CreateChecked(4);
        _six = T.CreateChecked(6);
        _fifteen = T.CreateChecked(15);
        _improperSafeFloor = T.CreateChecked(1e-15);
    }

    /// <summary>
    /// 绑定被积函数与不定积分下限，支持 <see cref="Integrate(T, T, int, Method, T?)"/> 与不定积分 API。
    /// </summary>
    public Integration(SingleVariableFunction<T> func, T defaultH, T lowerBound = default)
    {
        _boundFunc = func ?? throw new ArgumentNullException(nameof(func));
        _lowerBound = lowerBound;
        _defaultH = defaultH;
        _minH = T.CreateChecked(1e-15);
        _half = T.CreateChecked(0.5);
        _two = T.CreateChecked(2);
        _three = T.CreateChecked(3);
        _four = T.CreateChecked(4);
        _six = T.CreateChecked(6);
        _fifteen = T.CreateChecked(15);
        _improperSafeFloor = T.CreateChecked(1e-15);
    }

    /// <summary>
    /// 计算定积分，从 <paramref name="a"/> 到 <paramref name="b"/>。
    /// </summary>
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
            Method.Romberg => IntegrateRomberg(a, b, n, func),
            Method.GaussLegendre => IntegrateGaussLegendre(a, b, n, func),
            Method.AdaptiveSimpson => IntegrateAdaptiveSimpson(a, b, n, func, step),
            _ => throw new ArgumentException("不支持的积分方法")
        };
    }

    /// <summary>
    /// 使用构造时绑定的被积函数计算定积分。
    /// </summary>
    public T Integrate(T a, T b, int n = 1000, Method method = Method.Trapezoidal, T? h = null)
    {
        if (_boundFunc == null)
            throw new InvalidOperationException("此实例未绑定被积函数，请使用带 func 参数的 Integrate 重载或绑定函数的构造函数。");
        return Integrate(a, b, n, _boundFunc, h, method);
    }

    /// <summary>
    /// 计算数值不定积分 F(x) = ∫<sub>lowerBound</sub><sup>x</sup> f(t) dt。
    /// </summary>
    public T IndefiniteIntegrate(T x, int n = 1000, Method method = Method.Trapezoidal)
    {
        if (_boundFunc != null && method == Method.Trapezoidal && n >= 1)
            return EvaluateIndefiniteTrapezoidal(x, n);
        return Integrate(_lowerBound, x, n, method);
    }

    /// <summary>
    /// 返回数值不定积分函数 F(x) = ∫<sub>lowerBound</sub><sup>x</sup> f(t) dt。
    /// </summary>
    public SingleVariableFunction<T> GetIndefiniteIntegral(int n = 1000, Method method = Method.Trapezoidal) =>
        x => IndefiniteIntegrate(x, n, method);

    private T IntegrateTrapezoidal(T a, T b, int n, SingleVariableFunction<T> func)
    {
        T h = (b - a) / T.CreateChecked(n);
        T sum = (func(a) + func(b)) * _half;

        for (int i = 1; i < n; i++)
            sum += func(a + T.CreateChecked(i) * h);

        return sum * h;
    }

    private T IntegrateSimpson(T a, T b, int n, SingleVariableFunction<T> func)
    {
        if (n % 2 != 0) throw new ArgumentException("辛普森法则要求分段数为偶数", nameof(n));

        T h = (b - a) / T.CreateChecked(n);
        T sum = func(a) + func(b);

        for (int i = 1; i < n; i++)
        {
            T x = a + T.CreateChecked(i) * h;
            sum += func(x) * ((i & 1) == 1 ? _four : _two);
        }
        return sum * h / _three;
    }

    private T IntegrateRomberg(T a, T b, int maxLevel, SingleVariableFunction<T> func)
    {
        if (maxLevel < 1) throw new ArgumentException("Romberg 外推层数必须大于等于 1", nameof(maxLevel));

        EnsureRomberg(maxLevel);
        T[,] r = _romberg!;
        T h = b - a;
        r[0, 0] = (func(a) + func(b)) * h * _half;

        for (int i = 1; i <= maxLevel; i++)
        {
            h *= _half;
            T sum = T.Zero;
            int segments = 1 << i;
            for (int k = 1; k < segments; k += 2)
                sum += func(a + T.CreateChecked(k) * h);
            r[i, 0] = r[i - 1, 0] * _half + sum * h;

            for (int j = 1; j <= i; j++)
            {
                T factor = T.Pow(_four, T.CreateChecked(j));
                r[i, j] = (factor * r[i, j - 1] - r[i - 1, j - 1]) / (factor - T.One);
            }
        }

        return r[maxLevel, maxLevel];
    }

    private void EnsureRomberg(int maxLevel)
    {
        int size = maxLevel + 1;
        if (_romberg is not null && _rombergCapacity >= size)
            return;

        _romberg = new T[size, size];
        _rombergCapacity = size;
    }

    private T IntegrateGaussLegendre(T a, T b, int order, SingleVariableFunction<T> func)
    {
        if (order is not (4 or 8 or 16))
            throw new ArgumentException("Gauss-Legendre 阶数须为 4、8 或 16", nameof(order));

        EnsureGaussLegendreTable(order);
        T[] nodesT = _glNodesT!;
        T[] weightsT = _glWeightsT!;

        T half = (b - a) * _half;
        T mid = (a + b) * _half;
        T sum = T.Zero;

        for (int i = 0; i < nodesT.Length; i++)
        {
            T offset = half * nodesT[i];
            T weight = weightsT[i];
            sum += weight * (func(mid + offset) + func(mid - offset));
        }

        return half * sum;
    }

    /// <summary>
    /// 自适应 Simpson 积分：<paramref name="maxDepth"/> 为最大递归深度，<paramref name="tolerance"/> 为区间误差容差。
    /// </summary>
    private T IntegrateAdaptiveSimpson(T a, T b, int maxDepth, SingleVariableFunction<T> func, T tolerance)
    {
        if (maxDepth < 0) throw new ArgumentException("最大递归深度必须大于等于 0", nameof(maxDepth));
        if (tolerance <= T.Zero) throw new ArgumentException("容差必须为正", nameof(tolerance));

        T fa = func(a);
        T fb = func(b);
        T c = (a + b) * _half;
        T fc = func(c);
        T whole = SimpsonInterval(a, b, fa, fb, fc);
        return AdaptiveSimpsonIterative(a, b, tolerance, maxDepth, fa, fb, fc, whole, func);
    }

    private T SimpsonInterval(T a, T b, T fa, T fb, T fc)
        => (b - a) * (fa + _four * fc + fb) / _six;

    private T AdaptiveSimpsonIterative(
        T a, T b, T tolerance, int maxDepth,
        T fa, T fb, T fc, T whole,
        SingleVariableFunction<T> func)
    {
        EnsureSimpsonStack(maxDepth);
        SimpsonFrame[] stack = _simpsonStack!;
        int sp = 0;

        stack[sp++] = new SimpsonFrame
        {
            A = a,
            B = b,
            Fa = fa,
            Fb = fb,
            Fc = fc,
            Whole = whole,
            Tol = tolerance,
            Depth = maxDepth
        };

        T accumulator = T.Zero;

        while (sp > 0)
        {
            SimpsonFrame frame = stack[--sp];
            T c = (frame.A + frame.B) * _half;
            T d = (frame.A + c) * _half;
            T e = (c + frame.B) * _half;
            T fd = func(d);
            T fe = func(e);
            T left = SimpsonInterval(frame.A, c, frame.Fa, frame.Fc, fd);
            T right = SimpsonInterval(c, frame.B, frame.Fc, frame.Fb, fe);
            T sum = left + right;

            if (frame.Depth <= 0 || T.Abs(sum - frame.Whole) <= _fifteen * frame.Tol)
            {
                accumulator += sum + (sum - frame.Whole) / _fifteen;
                continue;
            }

            T halfTol = frame.Tol * _half;
            int nextDepth = frame.Depth - 1;

            stack[sp++] = new SimpsonFrame
            {
                A = c,
                B = frame.B,
                Fa = frame.Fc,
                Fb = frame.Fb,
                Fc = fe,
                Whole = right,
                Tol = halfTol,
                Depth = nextDepth
            };
            stack[sp++] = new SimpsonFrame
            {
                A = frame.A,
                B = c,
                Fa = frame.Fa,
                Fb = frame.Fc,
                Fc = fd,
                Whole = left,
                Tol = halfTol,
                Depth = nextDepth
            };
        }

        return accumulator;
    }

    private void EnsureSimpsonStack(int maxDepth)
    {
        int need = Math.Max(8, 2 * maxDepth + 4);
        if (_simpsonStack is not null && _simpsonStackCapacity >= need)
            return;

        _simpsonStack = new SimpsonFrame[need];
        _simpsonStackCapacity = need;
    }

    /// <summary>
    /// 反常积分 ∫<sub>a</sub><sup>∞</sup> f(x) dx（变量替换 + 自适应 Simpson）。
    /// </summary>
    public T IntegrateToInfinity(T a, SingleVariableFunction<T> func, int maxDepth = 24, T? tolerance = null)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        T tol = tolerance ?? _defaultH;
        return IntegrateImproperBySubstitution(a, func, maxDepth, tol);
    }

    /// <summary>
    /// 二重积分 ∫∫ f(x,y) dx dy，矩形区域。
    /// </summary>
    public T IntegrateDouble(
        T ax, T bx, T ay, T by,
        int nodesPerDim,
        Func<T, T, T> func,
        Method method = Method.GaussLegendre)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        if (nodesPerDim is not (4 or 8 or 16))
            throw new ArgumentException("每维节点数须为 4、8 或 16", nameof(nodesPerDim));

        EnsureGaussLegendreTable(nodesPerDim);
        T[] nodesT = _glNodesT!;
        T[] weightsT = _glWeightsT!;
        int nodeCount = nodesT.Length;

        T halfX = (bx - ax) * _half;
        T midX = (ax + bx) * _half;
        T halfY = (by - ay) * _half;
        T midY = (ay + by) * _half;
        T sum = T.Zero;

        for (int i = 0; i < nodeCount; i++)
        {
            T xi = nodesT[i];
            T wi = weightsT[i];
            T xPlus = midX + halfX * xi;
            T xMinus = midX - halfX * xi;

            for (int j = 0; j < nodeCount; j++)
            {
                T yj = nodesT[j];
                T w = wi * weightsT[j];
                T yPlus = midY + halfY * yj;
                T yMinus = midY - halfY * yj;
                sum += w * (func(xPlus, yPlus) + func(xMinus, yPlus)
                            + func(xPlus, yMinus) + func(xMinus, yMinus));
            }
        }

        return halfX * halfY * sum;
    }

    private T IntegrateImproperBySubstitution(T a, SingleVariableFunction<T> func, int maxDepth, T tolerance)
    {
        if (a <= T.Zero)
            throw new ArgumentException("反常积分下限须为正", nameof(a));

        _improperA = a;
        _improperScale = T.One / a;
        _improperSource = func;
        return IntegrateAdaptiveSimpson(T.Zero, T.One, maxDepth, MappedImproperIntegrand, tolerance);
    }

    private T MappedImproperIntegrand(T t)
    {
        T safeT = T.Max(t, _improperSafeFloor);
        T x = T.One / safeT;
        if (x < _improperA)
            return T.Zero;
        T denom = safeT * safeT;
        return _improperSource!(x) / denom * _improperScale;
    }

    public void ClearCache()
    {
        InvalidateIndefiniteTable();
        _glOrder = -1;
    }

    private void EnsureGaussLegendreTable(int order)
    {
        if (_glOrder == order && _glNodesT is not null)
            return;

        ReadOnlySpan<double> nodes = order switch
        {
            4 => GaussLegendreTables.Nodes4,
            8 => GaussLegendreTables.Nodes8,
            16 => GaussLegendreTables.Nodes16,
            _ => GaussLegendreTables.Nodes8
        };
        ReadOnlySpan<double> weights = order switch
        {
            4 => GaussLegendreTables.Weights4,
            8 => GaussLegendreTables.Weights8,
            16 => GaussLegendreTables.Weights16,
            _ => GaussLegendreTables.Weights8
        };

        if (_glNodesT is null || _glNodesT.Length < nodes.Length)
        {
            _glNodesT = new T[nodes.Length];
            _glWeightsT = new T[nodes.Length];
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            _glNodesT[i] = T.CreateChecked(nodes[i]);
            _glWeightsT[i] = T.CreateChecked(weights[i]);
        }

        _glOrder = order;
    }

    private void InvalidateIndefiniteTable()
    {
        _indefN = 0;
        _indefUpper = default;
    }

    private T EvaluateIndefiniteTrapezoidal(T x, int n)
    {
        T a = _lowerBound;
        if (x == a) return T.Zero;
        if (x < a)
            return -EvaluateIndefiniteTrapezoidal(a + (a - x), n);

        EnsureIndefiniteTable(a, x, n);
        return SampleIndefiniteAt(x);
    }

    private void EnsureIndefiniteTable(T a, T x, int n)
    {
        if (_indefN == n && _indefA == a && _indefNodes != null && x <= _indefUpper)
            return;

        BuildIndefiniteTable(a, x, n);
    }

    private void BuildIndefiniteTable(T a, T upper, int n)
    {
        int nodeCount = n + 1;
        if (_indefNodes == null || _indefNodes.Length < nodeCount)
        {
            _indefNodes = new T[nodeCount];
            _indefFx = new T[nodeCount];
            _indefCum = new T[nodeCount];
        }

        if (upper == a)
        {
            _indefA = a;
            _indefUpper = a;
            _indefN = n;
            return;
        }

        T h = (upper - a) / T.CreateChecked(n);
        _indefNodes[0] = a;
        _indefFx[0] = _boundFunc!(a);
        _indefCum[0] = T.Zero;

        for (int i = 1; i <= n; i++)
        {
            _indefNodes[i] = a + T.CreateChecked(i) * h;
            _indefFx[i] = _boundFunc!(_indefNodes[i]);
            _indefCum[i] = _indefCum[i - 1] + (_indefFx[i - 1] + _indefFx[i]) * h * _half;
        }

        _indefA = a;
        _indefUpper = upper;
        _indefN = n;
    }

    private T SampleIndefiniteAt(T x)
    {
        if (x <= _indefNodes![0]) return T.Zero;
        if (x >= _indefUpper)
        {
            if (x > _indefUpper)
            {
                BuildIndefiniteTable(_indefA, x, _indefN);
                return SampleIndefiniteAt(x);
            }
            return _indefCum![_indefN];
        }

        int lo = 0;
        int hi = _indefN;
        while (lo < hi)
        {
            int mid = (lo + hi + 1) >> 1;
            if (_indefNodes[mid] <= x) lo = mid;
            else hi = mid - 1;
        }

        T dx = x - _indefNodes[lo];
        T fx = _boundFunc!(x);
        return _indefCum![lo] + (_indefFx![lo] + fx) * dx * _half;
    }
}

file static class GaussLegendreTables
{
    // 4 点 Gauss-Legendre（正半轴节点与权重）
    public static ReadOnlySpan<double> Nodes4 => [0.8611363115940526, 0.33998104358485626];
    public static ReadOnlySpan<double> Weights4 => [0.3478548451374538, 0.6521451548625461];

    // 8 点
    public static ReadOnlySpan<double> Nodes8 =>
    [
        0.9602898564975363, 0.7966664774136267, 0.525532409916329, 0.1834346424956498
    ];
    public static ReadOnlySpan<double> Weights8 =>
    [
        0.10122853629037625, 0.2223810344533745, 0.31370664587788734, 0.362683783378362
    ];

    // 16 点
    public static ReadOnlySpan<double> Nodes16 =>
    [
        0.9894009349916499, 0.9445750230732326, 0.8656312023878318, 0.755404408355003,
        0.6178762444026448, 0.4580167776572274, 0.2816035507792589, 0.09501250983763744
    ];
    public static ReadOnlySpan<double> Weights16 =>
    [
        0.027152459411754094, 0.062253523938647894, 0.09515851168249278, 0.12462897125553387,
        0.14959598881657673, 0.1691565193950025, 0.18260341504492358, 0.1894506104550685
    ];
}
