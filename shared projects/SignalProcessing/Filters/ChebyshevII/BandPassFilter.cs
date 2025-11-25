using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

/// <summary>
/// Represents bandpass Chebyshev-II filter.
/// </summary>
public class BandPassFilter : ZiFilter
{
    /// <summary>
    /// Gets low cutoff frequency.
    /// </summary>
    public float FrequencyLow { get; private set; }

    /// <summary>
    /// Gets high cutoff frequency.
    /// </summary>
    public float FrequencyHigh { get; private set; }

    /// <summary>
    /// Gets ripple (in dB).
    /// </summary>
    public float Ripple { get; private set; }

    /// <summary>
    /// Gets filter order.
    /// </summary>
    public int Order => (_a.Length - 1) / 2;

    /// <summary>
    /// Constructs <see cref="BandPassFilter"/> of given <paramref name="order"/> 
    /// with given cutoff frequencies <paramref name="frequencyLow"/> and <paramref name="frequencyHigh"/>.
    /// </summary>
    /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
    /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
    /// <param name="order">Filter order</param>
    /// <param name="ripple">Ripple (in dB)</param>
    public BandPassFilter(float frequencyLow, float frequencyHigh, int order, float ripple = 0.1f) 
        : base(MakeTf(frequencyLow, frequencyHigh, order, ripple))
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        Ripple = ripple;
    }

    /// <summary>
    /// Generates transfer function.
    /// </summary>
    /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
    /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
    /// <param name="order">Filter order</param>
    /// <param name="ripple">Ripple (in dB)</param>
    private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order, float ripple = 0.1f)
    {
        return DesignFilter.IirBpTf(frequencyLow,
                                    frequencyHigh,
                                    PrototypeChebyshevII.Poles(order, ripple),
                                    PrototypeChebyshevII.Zeros(order));
    }

    /// <summary>
    /// Changes filter coefficients online (preserving the state of the filter).
    /// </summary>
    /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
    /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
    /// <param name="ripple">Ripple (in dB)</param>
    public void Change(float frequencyLow, float frequencyHigh, float ripple = 0.1f)
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        Ripple = ripple;

        Change(MakeTf(frequencyLow, frequencyHigh, (_a.Length - 1) / 2, ripple));
    }
}
