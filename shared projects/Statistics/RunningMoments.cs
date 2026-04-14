using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Online mean and variance via Welford's algorithm (single-pass, numerically stable).
/// </summary>
/// <typeparam name="T">Floating-point numeric type.</typeparam>
public struct RunningMoments<T> where T : IFloatingPointIeee754<T>
{
    private T _mean;
    private T _m2;
    private int _count;

    /// <summary>Number of pushed values.</summary>
    public readonly int Count => _count;

    /// <summary>Current mean.</summary>
    public readonly T Mean => _count > 0 ? _mean : T.Zero;

    /// <summary>Sample variance (ddof = 1). Returns zero when count &lt; 2.</summary>
    public readonly T Variance
    {
        get
        {
            if (_count < 2)
                return T.Zero;
            return _m2 / T.CreateChecked(_count - 1);
        }
    }

    /// <summary>Population variance (ddof = 0).</summary>
    public readonly T PopulationVariance =>
        _count > 0 ? _m2 / T.CreateChecked(_count) : T.Zero;

    /// <summary>Sample standard deviation.</summary>
    public readonly T StandardDeviation => T.Sqrt(Variance);

    /// <summary>Incorporates one observation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T value)
    {
        _count++;
        T delta = value - _mean;
        _mean += delta / T.CreateChecked(_count);
        T delta2 = value - _mean;
        _m2 += delta * delta2;
    }

    /// <summary>Resets accumulated state.</summary>
    public void Reset()
    {
        _mean = T.Zero;
        _m2 = T.Zero;
        _count = 0;
    }
}
