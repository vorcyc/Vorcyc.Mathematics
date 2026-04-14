using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// Abstract class for Linear Time-Invariant (LTI) filters.
/// </summary>
public abstract class LtiFilter : IFilter, IOnlineFilter
{
    /// <summary>
    /// Gets transfer function of LTI filter.
    /// </summary>
    public abstract TransferFunction Tf { get; protected set; }

    /// <summary>
    /// Applies LTI filter to entire <paramref name="signal"/> and returns new filtered signal.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="method">Filtering method</param>
    public abstract Signal ApplyTo(Signal signal, FilteringMethod method = FilteringMethod.Auto);

    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public abstract float Process(float sample);

    /// <summary>
    /// Resets LTI filter.
    /// </summary>
    public abstract void Reset();
}
