using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;

/// <summary>
/// Represents highpass Chebyshev-I filter.
/// </summary>
public class HighPassFilter : ZiFilter
{
    /// <summary>
    /// Gets cutoff frequency.
    /// </summary>
    public float Frequency { get; private set; }

    /// <summary>
    /// Gets ripple (in dB).
    /// </summary>
    public float Ripple { get; private set; }

    /// <summary>
    /// Gets filter order.
    /// </summary>
    public int Order => _a.Length - 1;

    /// <summary>
    /// Constructs <see cref="HighPassFilter"/> of given <paramref name="order"/> with given cutoff <paramref name="frequency"/>.
    /// </summary>
    /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
    /// <param name="order">Filter order</param>
    /// <param name="ripple">Ripple (in dB)</param>
    public HighPassFilter(float frequency, int order, float ripple = 0.1f) : base(MakeTf(frequency, order, ripple))
    {
        Frequency = frequency;
        Ripple = ripple;
    }

    /// <summary>
    /// Generates transfer function.
    /// </summary>
    /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
    /// <param name="order">Filter order</param>
    /// <param name="ripple">Ripple (in dB)</param>
    private static TransferFunction MakeTf(float frequency, int order, float ripple = 0.1f)
    {
        return DesignFilter.IirHpTf(frequency, PrototypeChebyshevI.Poles(order, ripple));
    }

    /// <summary>
    /// Changes filter coefficients online (preserving the state of the filter).
    /// </summary>
    /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
    /// <param name="ripple">Ripple (in dB)</param>
    public void Change(float frequency, float ripple = 0.1f)
    {
        Frequency = frequency;
        Ripple = ripple;

        Change(MakeTf(frequency, _a.Length - 1, ripple));
    }
}
