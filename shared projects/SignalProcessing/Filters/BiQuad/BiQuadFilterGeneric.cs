using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

/// <summary>
/// BiQuad IIR in <typeparamref name="T"/> (RBJ audio EQ cookbook).
/// </summary>
public class BiQuadFilter<T> : IirFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    private T _in1, _in2, _out1, _out2;

    protected BiQuadFilter() : base([T.One, T.Zero, T.Zero], [T.One, T.Zero, T.Zero])
    {
    }

    public BiQuadFilter(T b0, T b1, T b2, T a0, T a1, T a2)
        : base([b0, b1, b2], [a0, a1, a2])
    {
    }

    public override T Process(T sample)
    {
        var output = _b[0] * sample + _b[1] * _in1 + _b[2] * _in2 - _a[1] * _out1 - _a[2] * _out2;
        _in2 = _in1;
        _in1 = sample;
        _out2 = _out1;
        _out1 = output;
        return output;
    }

    public override void Reset() => _in1 = _in2 = _out1 = _out2 = T.Zero;

    public void Change(T b0, T b1, T b2, T a0, T a1, T a2)
    {
        if (T.Abs(a0) < T.CreateChecked(1e-30))
            throw new ArgumentException("The coefficient a0 can not be zero!");
        _b[0] = b0 / a0;
        _b[1] = b1 / a0;
        _b[2] = b2 / a0;
        _a[1] = a1 / a0;
        _a[2] = a2 / a0;
    }
}

public sealed class BiQuadFilter64 : BiQuadFilter<double>
{
    public BiQuadFilter64(double b0, double b1, double b2, double a0, double a1, double a2)
        : base(b0, b1, b2, a0, a1, a2)
    {
    }
}

/// <summary>BiQuad lowpass in <typeparamref name="T"/>.</summary>
public class LowPassFilter<T> : BiQuadFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; protected set; }
    public T Q { get; protected set; }

    public LowPassFilter(T frequency, T? q = null) => SetCoefficients(frequency, q ?? T.One);

    public void Change(T frequency, T? q = null) => SetCoefficients(frequency, q ?? T.One);

    private void SetCoefficients(T frequency, T q)
    {
        Frequency = frequency;
        Q = q;
        var omega = T.CreateChecked(2) * T.Pi * frequency;
        var alpha = T.Sin(omega) / (T.CreateChecked(2) * q);
        var cosw = T.Cos(omega);
        _b[0] = (T.One - cosw) / T.CreateChecked(2);
        _b[1] = T.One - cosw;
        _b[2] = _b[0];
        _a[0] = T.One + alpha;
        _a[1] = T.CreateChecked(-2) * cosw;
        _a[2] = T.One - alpha;
        Normalize();
    }
}

public sealed class LowPassFilter64 : LowPassFilter<double>
{
    public LowPassFilter64(double frequency, double q = 1) : base(frequency, q) { }
}

/// <summary>BiQuad highpass in <typeparamref name="T"/>.</summary>
public class HighPassFilter<T> : BiQuadFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; protected set; }
    public T Q { get; protected set; }

    public HighPassFilter(T frequency, T? q = null) => SetCoefficients(frequency, q ?? T.One);

    public void Change(T frequency, T? q = null) => SetCoefficients(frequency, q ?? T.One);

    private void SetCoefficients(T frequency, T q)
    {
        Frequency = frequency;
        Q = q;
        var omega = T.CreateChecked(2) * T.Pi * frequency;
        var alpha = T.Sin(omega) / (T.CreateChecked(2) * q);
        var cosw = T.Cos(omega);
        _b[0] = (T.One + cosw) / T.CreateChecked(2);
        _b[1] = -(T.One + cosw);
        _b[2] = _b[0];
        _a[0] = T.One + alpha;
        _a[1] = T.CreateChecked(-2) * cosw;
        _a[2] = T.One - alpha;
        Normalize();
    }
}

public sealed class HighPassFilter64 : HighPassFilter<double>
{
    public HighPassFilter64(double frequency, double q = 1) : base(frequency, q) { }
}

/// <summary>BiQuad bandpass in <typeparamref name="T"/>.</summary>
public class BandPassFilter<T> : BiQuadFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; protected set; }
    public T Q { get; protected set; }

    public BandPassFilter(T frequency, T? q = null) => SetCoefficients(frequency, q ?? T.One);

    public void Change(T frequency, T? q = null) => SetCoefficients(frequency, q ?? T.One);

    private void SetCoefficients(T frequency, T q)
    {
        Frequency = frequency;
        Q = q;
        var omega = T.CreateChecked(2) * T.Pi * frequency;
        var alpha = T.Sin(omega) / (T.CreateChecked(2) * q);
        var cosw = T.Cos(omega);
        _b[0] = alpha;
        _b[1] = T.Zero;
        _b[2] = -_b[0];
        _a[0] = T.One + alpha;
        _a[1] = T.CreateChecked(-2) * cosw;
        _a[2] = T.One - alpha;
        Normalize();
    }
}

public sealed class BandPassFilter64 : BandPassFilter<double>
{
    public BandPassFilter64(double frequency, double q = 1) : base(frequency, q) { }
}

/// <summary>BiQuad notch in <typeparamref name="T"/>.</summary>
public class NotchFilter<T> : BiQuadFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; protected set; }
    public T Q { get; protected set; }

    public NotchFilter(T frequency, T? q = null) => SetCoefficients(frequency, q ?? T.One);

    public void Change(T frequency, T? q = null) => SetCoefficients(frequency, q ?? T.One);

    private void SetCoefficients(T frequency, T q)
    {
        Frequency = frequency;
        Q = q;
        var omega = T.CreateChecked(2) * T.Pi * frequency;
        var alpha = T.Sin(omega) / (T.CreateChecked(2) * q);
        var cosw = T.Cos(omega);
        _b[0] = T.One;
        _b[1] = T.CreateChecked(-2) * cosw;
        _b[2] = T.One;
        _a[0] = T.One + alpha;
        _a[1] = T.CreateChecked(-2) * cosw;
        _a[2] = T.One - alpha;
        Normalize();
    }
}

public sealed class NotchFilter64 : NotchFilter<double>
{
    public NotchFilter64(double frequency, double q = 1) : base(frequency, q) { }
}

/// <summary>BiQuad allpass in <typeparamref name="T"/>.</summary>
public class AllPassFilter<T> : BiQuadFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; protected set; }
    public T Q { get; protected set; }

    public AllPassFilter(T frequency, T? q = null) => SetCoefficients(frequency, q ?? T.One);

    public void Change(T frequency, T? q = null) => SetCoefficients(frequency, q ?? T.One);

    private void SetCoefficients(T frequency, T q)
    {
        Frequency = frequency;
        Q = q;
        var omega = T.CreateChecked(2) * T.Pi * frequency;
        var alpha = T.Sin(omega) / (T.CreateChecked(2) * q);
        var cosw = T.Cos(omega);
        _b[0] = T.One - alpha;
        _b[1] = T.CreateChecked(-2) * cosw;
        _b[2] = T.One + alpha;
        _a[0] = _b[2];
        _a[1] = _b[1];
        _a[2] = _b[0];
        Normalize();
    }
}

public sealed class AllPassFilter64 : AllPassFilter<double>
{
    public AllPassFilter64(double frequency, double q = 1) : base(frequency, q) { }
}

/// <summary>BiQuad peaking EQ in <typeparamref name="T"/>.</summary>
public class PeakFilter<T> : BiQuadFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; protected set; }
    public T Q { get; protected set; }
    public T Gain { get; protected set; }

    public PeakFilter(T frequency, T? q = null, T? gain = null)
        => SetCoefficients(frequency, q ?? T.One, gain ?? T.One);

    public void Change(T frequency, T? q = null, T? gain = null)
        => SetCoefficients(frequency, q ?? T.One, gain ?? T.One);

    private void SetCoefficients(T frequency, T q, T gain)
    {
        Frequency = frequency;
        Q = q;
        Gain = gain;
        var ga = T.Pow(T.CreateChecked(10), gain / T.CreateChecked(40));
        var omega = T.CreateChecked(2) * T.Pi * frequency;
        var alpha = T.Sin(omega) / (T.CreateChecked(2) * q);
        var cosw = T.Cos(omega);
        _b[0] = T.One + alpha * ga;
        _b[1] = T.CreateChecked(-2) * cosw;
        _b[2] = T.One - alpha * ga;
        _a[0] = T.One + alpha / ga;
        _a[1] = T.CreateChecked(-2) * cosw;
        _a[2] = T.One - alpha / ga;
        Normalize();
    }
}

public sealed class PeakFilter64 : PeakFilter<double>
{
    public PeakFilter64(double frequency, double q = 1, double gain = 1) : base(frequency, q, gain) { }
}

/// <summary>BiQuad low shelf in <typeparamref name="T"/>.</summary>
public class LowShelfFilter<T> : BiQuadFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; protected set; }
    public T Q { get; protected set; }
    public T Gain { get; protected set; }

    public LowShelfFilter(T frequency, T? q = null, T? gain = null)
        => SetCoefficients(frequency, q ?? T.One, gain ?? T.One);

    public void Change(T frequency, T? q = null, T? gain = null)
        => SetCoefficients(frequency, q ?? T.One, gain ?? T.One);

    private void SetCoefficients(T frequency, T q, T gain)
    {
        Frequency = frequency;
        Q = q;
        Gain = gain;
        var ga = T.Pow(T.CreateChecked(10), gain / T.CreateChecked(40));
        var asqrt = T.Sqrt(ga);
        var omega = T.CreateChecked(2) * T.Pi * frequency;
        var alpha = T.Sin(omega) / T.CreateChecked(2) * T.Sqrt((ga + T.One / ga) * (T.One / q - T.One) + T.CreateChecked(2));
        var cosw = T.Cos(omega);
        _b[0] = ga * (ga + T.One - (ga - T.One) * cosw + T.CreateChecked(2) * asqrt * alpha);
        _b[1] = T.CreateChecked(2) * ga * (ga - T.One - (ga + T.One) * cosw);
        _b[2] = ga * (ga + T.One - (ga - T.One) * cosw - T.CreateChecked(2) * asqrt * alpha);
        _a[0] = ga + T.One + (ga - T.One) * cosw + T.CreateChecked(2) * asqrt * alpha;
        _a[1] = T.CreateChecked(-2) * (ga - T.One + (ga + T.One) * cosw);
        _a[2] = ga + T.One + (ga - T.One) * cosw - T.CreateChecked(2) * asqrt * alpha;
        Normalize();
    }
}

public sealed class LowShelfFilter64 : LowShelfFilter<double>
{
    public LowShelfFilter64(double frequency, double q = 1, double gain = 1) : base(frequency, q, gain) { }
}

/// <summary>BiQuad high shelf in <typeparamref name="T"/>.</summary>
public class HighShelfFilter<T> : BiQuadFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; protected set; }
    public T Q { get; protected set; }
    public T Gain { get; protected set; }

    public HighShelfFilter(T frequency, T? q = null, T? gain = null)
        => SetCoefficients(frequency, q ?? T.One, gain ?? T.One);

    public void Change(T frequency, T? q = null, T? gain = null)
        => SetCoefficients(frequency, q ?? T.One, gain ?? T.One);

    private void SetCoefficients(T frequency, T q, T gain)
    {
        Frequency = frequency;
        Q = q;
        Gain = gain;
        var ga = T.Pow(T.CreateChecked(10), gain / T.CreateChecked(40));
        var asqrt = T.Sqrt(ga);
        var omega = T.CreateChecked(2) * T.Pi * frequency;
        var alpha = T.Sin(omega) / T.CreateChecked(2) * T.Sqrt((ga + T.One / ga) * (T.One / q - T.One) + T.CreateChecked(2));
        var cosw = T.Cos(omega);
        _b[0] = ga * (ga + T.One + (ga - T.One) * cosw + T.CreateChecked(2) * asqrt * alpha);
        _b[1] = T.CreateChecked(-2) * ga * (ga - T.One + (ga + T.One) * cosw);
        _b[2] = ga * (ga + T.One + (ga - T.One) * cosw - T.CreateChecked(2) * asqrt * alpha);
        _a[0] = ga + T.One - (ga - T.One) * cosw + T.CreateChecked(2) * asqrt * alpha;
        _a[1] = T.CreateChecked(2) * (ga - T.One - (ga + T.One) * cosw);
        _a[2] = ga + T.One - (ga - T.One) * cosw - T.CreateChecked(2) * asqrt * alpha;
        Normalize();
    }
}

public sealed class HighShelfFilter64 : HighShelfFilter<double>
{
    public HighShelfFilter64(double frequency, double q = 1, double gain = 1) : base(frequency, q, gain) { }
}
