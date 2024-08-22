using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;

/// <summary>
/// Represents highpass Butterworth filter.
/// </summary>
public class HighPassFilter : ZiFilter
{
    /// <summary>
    /// Gets cutoff frequency.
    /// </summary>
    public float Frequency { get; private set; }

    /// <summary>
    /// Gets filter order.
    /// </summary>
    public int Order => _a.Length - 1;

    /// <summary>
    /// Constructs <see cref="HighPassFilter"/> of given <paramref name="order"/> with given cutoff <paramref name="frequency"/>.
    /// </summary>
    /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
    /// <param name="order">Filter order</param>
    public HighPassFilter(float frequency, int order) : base(MakeTf(frequency, order))
    {
        Frequency = frequency;
    }

    /// <summary>
    /// Generates transfer function.
    /// </summary>
    /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
    /// <param name="order">Filter order</param>
    private static TransferFunction MakeTf(float frequency, int order)
    {
        return DesignFilter.IirHpTf(frequency, PrototypeButterworth.Poles(order));
    }

    /// <summary>
    /// Changes filter coefficients online (preserving the state of the filter).
    /// </summary>
    /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
    public void Change(float frequency)
    {
        Frequency = frequency;

        Change(MakeTf(frequency, _a.Length - 1));
    }
}
