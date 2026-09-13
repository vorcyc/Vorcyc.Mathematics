using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.OnePole;

/// <summary>
/// One-pole IIR in <typeparamref name="T"/>.
/// </summary>
public class OnePoleFilter<T> : IirFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    private T _prev;

    protected OnePoleFilter() : base([T.One], [T.One, T.Zero])
    {
    }

    public OnePoleFilter(T b, T a) : base([b], [T.One, a])
    {
    }

    public override T Process(T sample)
    {
        var output = _b[0] * sample - _a[1] * _prev;
        _prev = output;
        return output;
    }

    public override void Reset() => _prev = T.Zero;
}

public sealed class OnePoleFilter64 : OnePoleFilter<double>
{
    public OnePoleFilter64(double b, double a) : base(b, a) { }
}

/// <summary>One-pole lowpass in <typeparamref name="T"/>.</summary>
public class LowPassFilter<T> : OnePoleFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; protected set; }

    public LowPassFilter(T frequency) => SetCoefficients(frequency);

    public void Change(T frequency) => SetCoefficients(frequency);

    private void SetCoefficients(T frequency)
    {
        Frequency = frequency;
        _a[0] = T.One;
        _a[1] = -T.Exp(T.CreateChecked(-2) * T.Pi * frequency);
        _b[0] = T.One + _a[1];
    }
}

public sealed class LowPassFilter64 : LowPassFilter<double>
{
    public LowPassFilter64(double frequency) : base(frequency) { }
}

/// <summary>One-pole highpass in <typeparamref name="T"/>.</summary>
public class HighPassFilter<T> : OnePoleFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; protected set; }

    public HighPassFilter(T frequency) => SetCoefficients(frequency);

    public void Change(T frequency) => SetCoefficients(frequency);

    private void SetCoefficients(T frequency)
    {
        Frequency = frequency;
        _a[0] = T.One;
        _a[1] = T.Exp(T.CreateChecked(-2) * T.Pi * (T.CreateChecked(0.5) - frequency));
        _b[0] = T.One - _a[1];
    }
}

public sealed class HighPassFilter64 : HighPassFilter<double>
{
    public HighPassFilter64(double frequency) : base(frequency) { }
}
