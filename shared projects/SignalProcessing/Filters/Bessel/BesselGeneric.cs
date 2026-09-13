using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Bessel;

/// <summary>Lowpass Bessel in <typeparamref name="T"/>.</summary>
public class LowPassFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; private set; }
    public int Order => _a.Length - 1;

    public LowPassFilter(T frequency, int order) : base(MakeTf(frequency, order)) => Frequency = frequency;

    public void Change(T frequency)
    {
        Frequency = frequency;
        Change(MakeTf(frequency, _a.Length - 1));
    }

    private static TransferFunction<T> MakeTf(T frequency, int order)
        => DesignFilter.IirLpTf(frequency, PrototypeBessel.Poles<T>(order));
}

public sealed class LowPassFilter64 : LowPassFilter<double>
{
    public LowPassFilter64(double frequency, int order) : base(frequency, order) { }
}

/// <summary>Highpass Bessel in <typeparamref name="T"/>.</summary>
public class HighPassFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; private set; }
    public int Order => _a.Length - 1;

    public HighPassFilter(T frequency, int order) : base(MakeTf(frequency, order)) => Frequency = frequency;

    public void Change(T frequency)
    {
        Frequency = frequency;
        Change(MakeTf(frequency, _a.Length - 1));
    }

    private static TransferFunction<T> MakeTf(T frequency, int order)
        => DesignFilter.IirHpTf(frequency, PrototypeBessel.Poles<T>(order));
}

public sealed class HighPassFilter64 : HighPassFilter<double>
{
    public HighPassFilter64(double frequency, int order) : base(frequency, order) { }
}

/// <summary>Bandpass Bessel in <typeparamref name="T"/>.</summary>
public class BandPassFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T FrequencyLow { get; private set; }
    public T FrequencyHigh { get; private set; }
    public int Order => (_a.Length - 1) / 2;

    public BandPassFilter(T frequencyLow, T frequencyHigh, int order) : base(MakeTf(frequencyLow, frequencyHigh, order))
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
    }

    public void Change(T frequencyLow, T frequencyHigh)
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        Change(MakeTf(frequencyLow, frequencyHigh, (_a.Length - 1) / 2));
    }

    private static TransferFunction<T> MakeTf(T frequencyLow, T frequencyHigh, int order)
        => DesignFilter.IirBpTf(frequencyLow, frequencyHigh, PrototypeBessel.Poles<T>(order));
}

public sealed class BandPassFilter64 : BandPassFilter<double>
{
    public BandPassFilter64(double frequencyLow, double frequencyHigh, int order) : base(frequencyLow, frequencyHigh, order) { }
}

/// <summary>Bandstop Bessel in <typeparamref name="T"/>.</summary>
public class BandStopFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T FrequencyLow { get; private set; }
    public T FrequencyHigh { get; private set; }
    public int Order => (_a.Length - 1) / 2;

    public BandStopFilter(T frequencyLow, T frequencyHigh, int order) : base(MakeTf(frequencyLow, frequencyHigh, order))
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
    }

    public void Change(T frequencyLow, T frequencyHigh)
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        Change(MakeTf(frequencyLow, frequencyHigh, (_a.Length - 1) / 2));
    }

    private static TransferFunction<T> MakeTf(T frequencyLow, T frequencyHigh, int order)
        => DesignFilter.IirBsTf(frequencyLow, frequencyHigh, PrototypeBessel.Poles<T>(order));
}

public sealed class BandStopFilter64 : BandStopFilter<double>
{
    public BandStopFilter64(double frequencyLow, double frequencyHigh, int order) : base(frequencyLow, frequencyHigh, order) { }
}
