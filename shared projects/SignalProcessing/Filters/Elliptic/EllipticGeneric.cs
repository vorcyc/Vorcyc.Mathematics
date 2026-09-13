using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic;

/// <summary>Lowpass elliptic in <typeparamref name="T"/>.</summary>
public class LowPassFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; private set; }
    public T RipplePassband { get; private set; }
    public T RippleStopband { get; private set; }
    public int Order => _a.Length - 1;

    public LowPassFilter(T frequency, int order, T? ripplePass = null, T? rippleStop = null)
        : base(MakeTf(frequency, order, ripplePass ?? T.One, rippleStop ?? T.CreateChecked(20)))
    {
        Frequency = frequency;
        RipplePassband = ripplePass ?? T.One;
        RippleStopband = rippleStop ?? T.CreateChecked(20);
    }

    public void Change(T frequency, T? ripplePass = null, T? rippleStop = null)
    {
        Frequency = frequency;
        RipplePassband = ripplePass ?? T.One;
        RippleStopband = rippleStop ?? T.CreateChecked(20);
        Change(MakeTf(frequency, _a.Length - 1, RipplePassband, RippleStopband));
    }

    private static TransferFunction<T> MakeTf(T frequency, int order, T ripplePass, T rippleStop)
        => DesignFilter.IirLpTf(frequency,
            PrototypeElliptic.Poles<T>(order, ripplePass, rippleStop),
            PrototypeElliptic.Zeros<T>(order, ripplePass, rippleStop));
}

public sealed class LowPassFilter64 : LowPassFilter<double>
{
    public LowPassFilter64(double frequency, int order, double ripplePass = 1, double rippleStop = 20)
        : base(frequency, order, ripplePass, rippleStop) { }
}

/// <summary>Highpass elliptic in <typeparamref name="T"/>.</summary>
public class HighPassFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T Frequency { get; private set; }
    public T RipplePassband { get; private set; }
    public T RippleStopband { get; private set; }
    public int Order => _a.Length - 1;

    public HighPassFilter(T frequency, int order, T? ripplePass = null, T? rippleStop = null)
        : base(MakeTf(frequency, order, ripplePass ?? T.One, rippleStop ?? T.CreateChecked(20)))
    {
        Frequency = frequency;
        RipplePassband = ripplePass ?? T.One;
        RippleStopband = rippleStop ?? T.CreateChecked(20);
    }

    public void Change(T frequency, T? ripplePass = null, T? rippleStop = null)
    {
        Frequency = frequency;
        RipplePassband = ripplePass ?? T.One;
        RippleStopband = rippleStop ?? T.CreateChecked(20);
        Change(MakeTf(frequency, _a.Length - 1, RipplePassband, RippleStopband));
    }

    private static TransferFunction<T> MakeTf(T frequency, int order, T ripplePass, T rippleStop)
        => DesignFilter.IirHpTf(frequency,
            PrototypeElliptic.Poles<T>(order, ripplePass, rippleStop),
            PrototypeElliptic.Zeros<T>(order, ripplePass, rippleStop));
}

public sealed class HighPassFilter64 : HighPassFilter<double>
{
    public HighPassFilter64(double frequency, int order, double ripplePass = 1, double rippleStop = 20)
        : base(frequency, order, ripplePass, rippleStop) { }
}

/// <summary>Bandpass elliptic in <typeparamref name="T"/>.</summary>
public class BandPassFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T FrequencyLow { get; private set; }
    public T FrequencyHigh { get; private set; }
    public T RipplePassband { get; private set; }
    public T RippleStopband { get; private set; }
    public int Order => (_a.Length - 1) / 2;

    public BandPassFilter(T frequencyLow, T frequencyHigh, int order, T? ripplePass = null, T? rippleStop = null)
        : base(MakeTf(frequencyLow, frequencyHigh, order, ripplePass ?? T.One, rippleStop ?? T.CreateChecked(20)))
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        RipplePassband = ripplePass ?? T.One;
        RippleStopband = rippleStop ?? T.CreateChecked(20);
    }

    public void Change(T frequencyLow, T frequencyHigh, T? ripplePass = null, T? rippleStop = null)
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        RipplePassband = ripplePass ?? T.One;
        RippleStopband = rippleStop ?? T.CreateChecked(20);
        Change(MakeTf(frequencyLow, frequencyHigh, (_a.Length - 1) / 2, RipplePassband, RippleStopband));
    }

    private static TransferFunction<T> MakeTf(T frequencyLow, T frequencyHigh, int order, T ripplePass, T rippleStop)
        => DesignFilter.IirBpTf(frequencyLow, frequencyHigh,
            PrototypeElliptic.Poles<T>(order, ripplePass, rippleStop),
            PrototypeElliptic.Zeros<T>(order, ripplePass, rippleStop));
}

public sealed class BandPassFilter64 : BandPassFilter<double>
{
    public BandPassFilter64(double frequencyLow, double frequencyHigh, int order, double ripplePass = 1, double rippleStop = 20)
        : base(frequencyLow, frequencyHigh, order, ripplePass, rippleStop) { }
}

/// <summary>Bandstop elliptic in <typeparamref name="T"/>.</summary>
public class BandStopFilter<T> : ZiFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public T FrequencyLow { get; private set; }
    public T FrequencyHigh { get; private set; }
    public T RipplePassband { get; private set; }
    public T RippleStopband { get; private set; }
    public int Order => (_a.Length - 1) / 2;

    public BandStopFilter(T frequencyLow, T frequencyHigh, int order, T? ripplePass = null, T? rippleStop = null)
        : base(MakeTf(frequencyLow, frequencyHigh, order, ripplePass ?? T.One, rippleStop ?? T.CreateChecked(20)))
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        RipplePassband = ripplePass ?? T.One;
        RippleStopband = rippleStop ?? T.CreateChecked(20);
    }

    public void Change(T frequencyLow, T frequencyHigh, T? ripplePass = null, T? rippleStop = null)
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        RipplePassband = ripplePass ?? T.One;
        RippleStopband = rippleStop ?? T.CreateChecked(20);
        Change(MakeTf(frequencyLow, frequencyHigh, (_a.Length - 1) / 2, RipplePassband, RippleStopband));
    }

    private static TransferFunction<T> MakeTf(T frequencyLow, T frequencyHigh, int order, T ripplePass, T rippleStop)
        => DesignFilter.IirBsTf(frequencyLow, frequencyHigh,
            PrototypeElliptic.Poles<T>(order, ripplePass, rippleStop),
            PrototypeElliptic.Zeros<T>(order, ripplePass, rippleStop));
}

public sealed class BandStopFilter64 : BandStopFilter<double>
{
    public BandStopFilter64(double frequencyLow, double frequencyHigh, int order, double ripplePass = 1, double rippleStop = 20)
        : base(frequencyLow, frequencyHigh, order, ripplePass, rippleStop) { }
}
