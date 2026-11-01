using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

/// <summary>
/// Represents lowpass Chebyshev-II filter.
/// </summary>
/// <remarks>
/// <para>
/// The frequency is the stopband edge: the gain first reaches <c>-Ripple</c> dB there (like in SciPy's <c>cheby2</c>);
/// the passband below it is monotonic.
/// </para>
/// <para>
/// The transfer function is built in single precision, which limits the usable order and attenuation
/// (high orders, large attenuations and frequencies close to 0 lose the response in the coefficients).
/// Use <see cref="LowPassFilter64"/> or <c>LowPassFilter&lt;double&gt;</c> then.
/// </para>
/// </remarks>
public class LowPassFilter : ZiFilter
{
    /// <summary>
    /// Gets stopband edge frequency (normalized): the gain first reaches <c>-<see cref="Ripple"/></c> dB there.
    /// </summary>
    public float Frequency { get; private set; }

    /// <summary>
    /// Gets stopband attenuation (in dB).
    /// </summary>
    public float Ripple { get; private set; }

    /// <summary>
    /// Gets filter order.
    /// </summary>
    public int Order => _a.Length - 1;

    /// <summary>
    /// Constructs <see cref="LowPassFilter"/> of given <paramref name="order"/> with given stopband edge <paramref name="frequency"/>.
    /// </summary>
    /// <param name="frequency">Normalized stopband edge frequency in range (0..0.5), the end points excluded</param>
    /// <param name="order">Filter order (at least 1)</param>
    /// <param name="ripple">Stopband attenuation (in dB)</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="frequency"/> is not in range (0..0.5), <paramref name="order"/> is less than 1,
    /// or <paramref name="ripple"/> is not a positive finite number.
    /// </exception>
    public LowPassFilter(float frequency, int order, float ripple = 20f) : base(MakeTf(frequency, order, ripple))
    {
        Frequency = frequency;
        Ripple = ripple;
    }

    /// <summary>
    /// Generates transfer function.
    /// </summary>
    /// <param name="frequency">Normalized stopband edge frequency in range (0..0.5), the end points excluded</param>
    /// <param name="order">Filter order</param>
    /// <param name="ripple">Stopband attenuation (in dB)</param>
    private static TransferFunction MakeTf(float frequency, int order, float ripple = 20f)
    {
        return DesignFilter.IirLpTf(frequency, PrototypeChebyshevII.Poles(order, ripple), PrototypeChebyshevII.Zeros(order));
    }

    /// <summary>
    /// Changes filter coefficients online (preserving the state of the filter).
    /// </summary>
    /// <param name="frequency">Normalized stopband edge frequency in range (0..0.5), the end points excluded</param>
    /// <param name="ripple">Stopband attenuation (in dB)</param>
    public void Change(float frequency, float ripple = 20f)
    {
        Change(MakeTf(frequency, _a.Length - 1, ripple));

        Frequency = frequency;
        Ripple = ripple;
    }
}
