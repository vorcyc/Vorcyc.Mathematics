using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

/// <summary>Lowpass Chebyshev-II in <typeparamref name="T"/>.</summary>
public class LowPassFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; private set; }
    public T Ripple { get; private set; }
    public int Order => _a.Length - 1;

    public LowPassFilter(T frequency, int order, T? ripple = null)
        : base(MakeTf(frequency, order, ripple ?? T.CreateChecked(20)))
    {
        Frequency = frequency;
        Ripple = ripple ?? T.CreateChecked(20);
    }

    public void Change(T frequency, T? ripple = null)
    {
        Frequency = frequency;
        Ripple = ripple ?? T.CreateChecked(20);
        Change(MakeTf(frequency, _a.Length - 1, Ripple));
    }

    private static TransferFunction<T> MakeTf(T frequency, int order, T ripple)
        => DesignFilter.IirLpTf(frequency, PrototypeChebyshevII.Poles<T>(order, ripple), PrototypeChebyshevII.Zeros<T>(order));
}

public sealed class LowPassFilter64 : LowPassFilter<double>
{
    public LowPassFilter64(double frequency, int order, double ripple = 20) : base(frequency, order, ripple) { }
}

/// <summary>Highpass Chebyshev-II in <typeparamref name="T"/>.</summary>
public class HighPassFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; private set; }
    public T Ripple { get; private set; }
    public int Order => _a.Length - 1;

    public HighPassFilter(T frequency, int order, T? ripple = null)
        : base(MakeTf(frequency, order, ripple ?? T.CreateChecked(20)))
    {
        Frequency = frequency;
        Ripple = ripple ?? T.CreateChecked(20);
    }

    public void Change(T frequency, T? ripple = null)
    {
        Frequency = frequency;
        Ripple = ripple ?? T.CreateChecked(20);
        Change(MakeTf(frequency, _a.Length - 1, Ripple));
    }

    private static TransferFunction<T> MakeTf(T frequency, int order, T ripple)
        => DesignFilter.IirHpTf(frequency, PrototypeChebyshevII.Poles<T>(order, ripple), PrototypeChebyshevII.Zeros<T>(order));
}

public sealed class HighPassFilter64 : HighPassFilter<double>
{
    public HighPassFilter64(double frequency, int order, double ripple = 20) : base(frequency, order, ripple) { }
}

/// <summary>Bandpass Chebyshev-II in <typeparamref name="T"/>.</summary>
public class BandPassFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T FrequencyLow { get; private set; }
    public T FrequencyHigh { get; private set; }
    public T Ripple { get; private set; }
    public int Order => (_a.Length - 1) / 2;

    public BandPassFilter(T frequencyLow, T frequencyHigh, int order, T? ripple = null)
        : base(MakeTf(frequencyLow, frequencyHigh, order, ripple ?? T.CreateChecked(20)))
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        Ripple = ripple ?? T.CreateChecked(20);
    }

    public void Change(T frequencyLow, T frequencyHigh, T? ripple = null)
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        Ripple = ripple ?? T.CreateChecked(20);
        Change(MakeTf(frequencyLow, frequencyHigh, (_a.Length - 1) / 2, Ripple));
    }

    private static TransferFunction<T> MakeTf(T frequencyLow, T frequencyHigh, int order, T ripple)
        => DesignFilter.IirBpTf(frequencyLow, frequencyHigh,
            PrototypeChebyshevII.Poles<T>(order, ripple), PrototypeChebyshevII.Zeros<T>(order));
}

public sealed class BandPassFilter64 : BandPassFilter<double>
{
    public BandPassFilter64(double frequencyLow, double frequencyHigh, int order, double ripple = 20)
        : base(frequencyLow, frequencyHigh, order, ripple) { }
}

/// <summary>Bandstop Chebyshev-II in <typeparamref name="T"/>.</summary>
public class BandStopFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T FrequencyLow { get; private set; }
    public T FrequencyHigh { get; private set; }
    public T Ripple { get; private set; }
    public int Order => (_a.Length - 1) / 2;

    public BandStopFilter(T frequencyLow, T frequencyHigh, int order, T? ripple = null)
        : base(MakeTf(frequencyLow, frequencyHigh, order, ripple ?? T.CreateChecked(20)))
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        Ripple = ripple ?? T.CreateChecked(20);
    }

    public void Change(T frequencyLow, T frequencyHigh, T? ripple = null)
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        Ripple = ripple ?? T.CreateChecked(20);
        Change(MakeTf(frequencyLow, frequencyHigh, (_a.Length - 1) / 2, Ripple));
    }

    private static TransferFunction<T> MakeTf(T frequencyLow, T frequencyHigh, int order, T ripple)
        => DesignFilter.IirBsTf(frequencyLow, frequencyHigh,
            PrototypeChebyshevII.Poles<T>(order, ripple), PrototypeChebyshevII.Zeros<T>(order));
}

public sealed class BandStopFilter64 : BandStopFilter<double>
{
    public BandStopFilter64(double frequencyLow, double frequencyHigh, int order, double ripple = 20)
        : base(frequencyLow, frequencyHigh, order, ripple) { }
}
