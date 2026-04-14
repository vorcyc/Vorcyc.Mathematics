namespace Vorcyc.Mathematics.Calculus.Series;

using System.Numerics;


/// <summary>
/// 提供傅里叶级数展开计算的实例类，支持泛型浮点类型。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public sealed class FourierSeries<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly SingleVariableFunction<T> _func;
    private readonly T _period;
    private readonly T _twoOverT;
    private readonly T _twoPiOverT;
    private readonly T _oneThird;
    private readonly T _two;
    private readonly T _four;

    private T[]? _aCoeffs;
    private T[]? _bCoeffs;
    private bool[]? _aValid;
    private bool[]? _bValid;
    private int _coeffSegments = -1;

    private T[]? _fSamples;
    private T[]? _sampleNodes;
    private int _sampleCount = -1;

    public FourierSeries(SingleVariableFunction<T> func, T period, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _period = period;
        _ = defaultH;
        _twoOverT = T.CreateChecked(2) / period;
        _twoPiOverT = T.CreateChecked(2 * Math.PI) / period;
        _oneThird = T.One / T.CreateChecked(3);
        _two = T.CreateChecked(2);
        _four = T.CreateChecked(4);
    }

    public T Calculate(T x, int order = 5, int segments = 1000)
    {
        if (order < 0) throw new ArgumentException("阶数必须大于等于 0", nameof(order));

        EnsureSamples(segments);

        T sum = GetFourierCoefficient(true, 0, segments) / _two;

        for (int n = 1; n <= order; n++)
        {
            T a_n = GetFourierCoefficient(true, n, segments);
            T b_n = GetFourierCoefficient(false, n, segments);
            T nOmegaX = T.CreateChecked(n) * _twoPiOverT * x;
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

        EnsureSamples(segments);
        InvalidateCoeffCacheIfSegmentsChanged(segments);

        ref T slot = ref GetCoeffSlot(isCosine, n);
        ref bool valid = ref GetValidFlag(isCosine, n);
        if (!valid)
        {
            T coeff = IntegrateTrigProduct(n, isCosine);
            if (T.IsNaN(coeff) || T.IsInfinity(coeff))
                throw new InvalidOperationException($"傅里叶系数 a_{n} 或 b_{n} 计算结果无效");

            slot = coeff;
            valid = true;
        }
        return slot;
    }

    public SingleVariableFunction<T> GetSeries(int order = 5, int segments = 1000)
    {
        return x => Calculate(x, order, segments);
    }

    public void ClearCache()
    {
        _coeffSegments = -1;
        _aValid?.AsSpan().Clear();
        _bValid?.AsSpan().Clear();
        _sampleCount = -1;
    }

    private void InvalidateCoeffCacheIfSegmentsChanged(int segments)
    {
        if (_coeffSegments == segments)
            return;

        _coeffSegments = segments;
        _aValid?.AsSpan().Clear();
        _bValid?.AsSpan().Clear();
    }

    private ref T GetCoeffSlot(bool isCosine, int n)
    {
        int need = n + 1;
        if (isCosine)
        {
            if (_aCoeffs is null || _aCoeffs.Length < need)
            {
                int newLen = Math.Max(need, (_aCoeffs?.Length ?? 0) * 2);
                Array.Resize(ref _aCoeffs, newLen);
                Array.Resize(ref _aValid, newLen);
            }
            return ref _aCoeffs[n];
        }

        if (_bCoeffs is null || _bCoeffs.Length < need)
        {
            int newLen = Math.Max(need, (_bCoeffs?.Length ?? 0) * 2);
            Array.Resize(ref _bCoeffs, newLen);
            Array.Resize(ref _bValid, newLen);
        }
        return ref _bCoeffs[n];
    }

    private ref bool GetValidFlag(bool isCosine, int n) =>
        ref (isCosine ? _aValid! : _bValid!)[n];

    private void EnsureSamples(int segments)
    {
        int n = segments % 2 == 0 ? segments : segments + 1;
        if (_fSamples != null && _sampleCount == n)
            return;

        _sampleCount = n;
        int nodeCount = n + 1;
        if (_fSamples == null || _fSamples.Length < nodeCount)
        {
            _fSamples = new T[nodeCount];
            _sampleNodes = new T[nodeCount];
        }

        T h = _period / T.CreateChecked(n);
        for (int j = 0; j <= n; j++)
        {
            T x = T.CreateChecked(j) * h;
            _sampleNodes![j] = x;
            _fSamples[j] = _func(x);
        }
    }

    private T IntegrateTrigProduct(int n, bool isCosine)
    {
        int nSeg = _sampleCount;
        T h = _period / T.CreateChecked(nSeg);
        T nOmega = T.CreateChecked(n) * _twoPiOverT;

        T EvalTrig(T x) => isCosine ? T.Cos(nOmega * x) : T.Sin(nOmega * x);

        T g0 = _fSamples![0] * EvalTrig(_sampleNodes![0]);
        T gn = _fSamples[nSeg] * EvalTrig(_sampleNodes[nSeg]);
        T sum = g0 + gn;

        for (int j = 1; j < nSeg; j++)
        {
            T g = _fSamples[j] * EvalTrig(_sampleNodes[j]);
            sum += (j & 1) == 1 ? _four * g : _two * g;
        }

        return sum * h * _oneThird * _twoOverT;
    }
}
