using System.Numerics;

namespace Vorcyc.Mathematics.Calculus.Series;

/// <summary>
/// 可选拟合工作区，复用采样缓冲以避免重复分配。
/// </summary>
public sealed class ChebyshevFitWorkspace<T> where T : struct, IFloatingPointIeee754<T>
{
    private T[]? _functionSamples;
    private T[]? _theta;
    private T[]? _cosTable;
    private int _sampleCount;
    private int _order;

    internal void Ensure(int sampleCount)
    {
        if (_functionSamples is not null && _functionSamples.Length >= sampleCount)
            return;

        _functionSamples = new T[sampleCount];
        _theta = new T[sampleCount];
        _cosTable = null;
        _sampleCount = 0;
        _order = -1;
    }

    internal Span<T> FunctionSamples(int sampleCount)
    {
        Ensure(sampleCount);
        return _functionSamples!.AsSpan(0, sampleCount);
    }

    internal Span<T> Theta(int sampleCount)
    {
        Ensure(sampleCount);
        return _theta!.AsSpan(0, sampleCount);
    }

    internal ReadOnlySpan<T> CosTable(int order, int sampleCount, ReadOnlySpan<T> theta)
    {
        if (_cosTable is not null && _order == order && _sampleCount == sampleCount)
            return _cosTable;

        int rows = order + 1;
        int len = rows * sampleCount;
        if (_cosTable is null || _cosTable.Length < len)
            _cosTable = new T[len];

        for (int k = 0; k <= order; k++)
        {
            T kT = T.CreateChecked(k);
            int row = k * sampleCount;
            for (int j = 0; j < sampleCount; j++)
                _cosTable[row + j] = T.Cos(kT * theta[j]);
        }

        _order = order;
        _sampleCount = sampleCount;
        return _cosTable.AsSpan(0, len);
    }
}

/// <summary>
/// 第一类 Chebyshev 级数：f(x) ≈ Σ aₖ Tₖ(x)，x ∈ [-1,1]。
/// </summary>
public sealed class ChebyshevSeries<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly T[] _coefficients;
    private readonly T _two;

    /// <summary>用 Chebyshev 系数初始化。</summary>
    public ChebyshevSeries(ReadOnlySpan<T> coefficients)
    {
        if (coefficients.IsEmpty) throw new ArgumentException("系数不能为空", nameof(coefficients));
        _coefficients = coefficients.ToArray();
        _two = T.CreateChecked(2);
    }

    /// <summary>
    /// 在区间 [a,b] 上对函数采样并估算 Chebyshev 系数（离散余弦型求和）。
    /// </summary>
    public static ChebyshevSeries<T> FromFunction(
        SingleVariableFunction<T> func,
        int order,
        T a,
        T b,
        int sampleCount) =>
        FromFunction(func, order, a, b, sampleCount, workspace: null);

    /// <summary>
    /// 拟合 Chebyshev 级数；传入 <paramref name="workspace"/> 可复用采样缓冲。
    /// </summary>
    public static ChebyshevSeries<T> FromFunction(
        SingleVariableFunction<T> func,
        int order,
        T a,
        T b,
        int sampleCount,
        ChebyshevFitWorkspace<T>? workspace)
    {
        if (order < 0) throw new ArgumentException("阶数必须非负", nameof(order));
        if (sampleCount < order + 1) throw new ArgumentException("采样数不足", nameof(sampleCount));

        var coeffs = new T[order + 1];
        T half = (a + b) * T.CreateChecked(0.5);
        T halfLen = (b - a) * T.CreateChecked(0.5);
        T pi = T.Pi;
        T two = T.CreateChecked(2);
        T invN = T.One / T.CreateChecked(sampleCount);
        T halfSample = T.CreateChecked(0.5);

        T[]? rentedF = null;
        T[]? rentedTheta = null;
        T[]? rentedCos = null;
        Span<T> fSamples = workspace is not null
            ? workspace.FunctionSamples(sampleCount)
            : (rentedF = new T[sampleCount]);
        Span<T> theta = workspace is not null
            ? workspace.Theta(sampleCount)
            : (rentedTheta = new T[sampleCount]);

        for (int j = 0; j < sampleCount; j++)
        {
            theta[j] = pi * (T.CreateChecked(j) + halfSample) * invN;
            T x = half + halfLen * T.Cos(theta[j]);
            fSamples[j] = func(x);
        }

        ReadOnlySpan<T> cosTable = workspace is not null
            ? workspace.CosTable(order, sampleCount, theta)
            : BuildCosTable(order, sampleCount, theta, rentedCos = new T[(order + 1) * sampleCount]);

        _ = rentedF;
        _ = rentedTheta;
        _ = rentedCos;

        for (int k = 0; k <= order; k++)
        {
            T sum = T.Zero;
            int row = k * sampleCount;
            for (int j = 0; j < sampleCount; j++)
                sum += fSamples[j] * cosTable[row + j];

            coeffs[k] = (k == 0 ? invN : two * invN) * sum;
        }

        return new ChebyshevSeries<T>(coeffs);
    }

    private static ReadOnlySpan<T> BuildCosTable(int order, int sampleCount, ReadOnlySpan<T> theta, T[] buffer)
    {
        for (int k = 0; k <= order; k++)
        {
            T kT = T.CreateChecked(k);
            int row = k * sampleCount;
            for (int j = 0; j < sampleCount; j++)
                buffer[row + j] = T.Cos(kT * theta[j]);
        }
        return buffer;
    }

    /// <summary>在 [-1,1] 上求值。</summary>
    public T Evaluate(T x)
    {
        if (_coefficients.Length == 1)
            return _coefficients[0];

        T b0 = _coefficients[^1];
        T b1 = T.Zero;
        for (int k = _coefficients.Length - 2; k >= 1; k--)
        {
            T b2 = b1;
            b1 = b0;
            b0 = _coefficients[k] + _two * x * b1 - b2;
        }

        return _coefficients[0] + x * b0 - b1;
    }

    /// <summary>在物理区间 [a,b] 上求值（内部映射到 [-1,1]）。</summary>
    public T EvaluateMapped(T x, T a, T b)
    {
        T invLen = T.One / (b - a);
        T mapped = (_two * x - a - b) * invLen;
        return Evaluate(mapped);
    }
}
