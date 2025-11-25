using Vorcyc.Mathematics.SignalProcessing.Filters.OnePole;

namespace Vorcyc.Mathematics.SignalProcessing.Filters;

/// <summary>
/// Represents de-emphasis IIR filter.
/// </summary>
public class DeEmphasisFilter : OnePoleFilter
{
    /// <summary>
    /// Constructs <see cref="DeEmphasisFilter"/>.
    /// </summary>
    /// <param name="a">De-emphasis coefficient</param>
    /// <param name="normalize">Normalize freq response to unit gain</param>
    public DeEmphasisFilter(float a = 0.97f, bool normalize = false) : base(1.0f, -a)
    {
        if (normalize)
        {
            _b[0] = 1 - a;
        }
    }
}
