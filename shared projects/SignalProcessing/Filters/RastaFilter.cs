using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Filters;

/// <summary>
/// Represents RASTA filter (used for robust speech processing).
/// </summary>
public class RastaFilter : IirFilter
{
    /// <summary>
    /// Constructs <see cref="RastaFilter"/>.
    /// </summary>
    /// <param name="pole">Pole</param>
    public RastaFilter(float pole = 0.98f) : base(new[] { 0.2f, 0.1f, 0, -0.1f, -0.2f }, new[] { 1, -pole })
    {
    }
}
