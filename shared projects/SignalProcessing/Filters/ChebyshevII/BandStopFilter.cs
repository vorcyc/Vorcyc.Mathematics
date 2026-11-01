using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

/// <summary>
/// Represents bandstop Chebyshev-II filter.
/// </summary>
/// <remarks>
/// <para>
/// The frequencies are the stopband edges: the gain first reaches <c>-Ripple</c> dB there (like in SciPy's <c>cheby2</c>);
/// the stopband lies between them and the passbands outside.
/// </para>
/// <para>
/// The order is the order of the lowpass prototype, so the transfer function has degree <c>2 * Order</c>.
/// It is built in single precision, which limits the usable order and attenuation
/// (higher orders, large attenuations and narrow or low bands lose the response in the coefficients).
/// Use <see cref="BandStopFilter64"/> or <c>BandStopFilter&lt;double&gt;</c> then.
/// </para>
/// </remarks>
public class BandStopFilter : ZiFilter
{
    /// <summary>
    /// Gets low stopband edge frequency (normalized): the gain first reaches <c>-<see cref="Ripple"/></c> dB there.
    /// </summary>
    public float FrequencyLow { get; private set; }

    /// <summary>
    /// Gets high stopband edge frequency (normalized): the gain first reaches <c>-<see cref="Ripple"/></c> dB there.
    /// </summary>
    public float FrequencyHigh { get; private set; }

    /// <summary>
    /// Gets stopband attenuation (in dB).
    /// </summary>
    public float Ripple { get; private set; }

    /// <summary>
    /// Gets filter order (the order of the lowpass prototype, the transfer function has degree 2 * order).
    /// </summary>
    public int Order => (_a.Length - 1) / 2;

    /// <summary>
    /// Constructs <see cref="BandStopFilter"/> of given <paramref name="order"/> 
    /// with given stopband edge frequencies <paramref name="frequencyLow"/> and <paramref name="frequencyHigh"/>.
    /// </summary>
    /// <param name="frequencyLow">Normalized low stopband edge frequency in range (0..0.5), the end points excluded</param>
    /// <param name="frequencyHigh">Normalized high stopband edge frequency in range (0..0.5), the end points excluded</param>
    /// <param name="order">Order of the lowpass prototype (at least 1), the transfer function has degree 2 * order</param>
    /// <param name="ripple">Stopband attenuation (in dB)</param>
    /// <exception cref="ArgumentException">
    /// A frequency is not in range (0..0.5) or <paramref name="frequencyLow"/> is not less than <paramref name="frequencyHigh"/>,
    /// <paramref name="order"/> is less than 1, or <paramref name="ripple"/> is not a positive finite number.
    /// </exception>
    public BandStopFilter(float frequencyLow, float frequencyHigh, int order, float ripple = 20f)
        : base(MakeTf(frequencyLow, frequencyHigh, order, ripple))
    {
        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        Ripple = ripple;
    }

    /// <summary>
    /// Generates transfer function.
    /// </summary>
    /// <param name="frequencyLow">Normalized low stopband edge frequency in range (0..0.5), the end points excluded</param>
    /// <param name="frequencyHigh">Normalized high stopband edge frequency in range (0..0.5), the end points excluded</param>
    /// <param name="order">Order of the lowpass prototype</param>
    /// <param name="ripple">Stopband attenuation (in dB)</param>
    private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order, float ripple = 20f)
    {
        return DesignFilter.IirBsTf(frequencyLow,
                                    frequencyHigh,
                                    PrototypeChebyshevII.Poles(order, ripple),
                                    PrototypeChebyshevII.Zeros(order));
    }

    /// <summary>
    /// Changes filter coefficients online (preserving the state of the filter).
    /// </summary>
    /// <param name="frequencyLow">Normalized low stopband edge frequency in range (0..0.5), the end points excluded</param>
    /// <param name="frequencyHigh">Normalized high stopband edge frequency in range (0..0.5), the end points excluded</param>
    /// <param name="ripple">Stopband attenuation (in dB)</param>
    public void Change(float frequencyLow, float frequencyHigh, float ripple = 20f)
    {
        Change(MakeTf(frequencyLow, frequencyHigh, (_a.Length - 1) / 2, ripple));

        FrequencyLow = frequencyLow;
        FrequencyHigh = frequencyHigh;
        Ripple = ripple;
    }
}
