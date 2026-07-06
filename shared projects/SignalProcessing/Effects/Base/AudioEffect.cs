using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Vorcyc.Mathematics.SignalProcessing.Effects.Base;

/// <summary>
/// Abstract class for audio effects.
/// </summary>
public abstract class AudioEffect : WetDryMixer, IFilter, IOnlineFilter
{
    /// <summary>
    /// Processes one sample.
    /// </summary>
    public abstract float Process(float sample);

    /// <summary>
    /// Resets effect.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Applies effect to entire <paramref name="signal"/> and returns new processed signal.
    /// </summary>
    public virtual Signal ApplyTo(Signal signal, FilteringMethod method = FilteringMethod.Auto)
        => this.FilterOnline(signal);

    /// <summary>
    /// Applies effect to entire <paramref name="signal"/> in-place.
    /// </summary>
    public virtual void Apply(Signal signal, FilteringMethod method = FilteringMethod.Auto)
        => this.FilterOnline_Inplace(signal);

    /// <summary>
    /// Sets sampling rate for this effect (deferred initialization).
    /// Default implementation does nothing; override in effects that depend on sampling rate.
    /// </summary>
    /// <param name="samplingRate">Sampling rate in Hz</param>
    public virtual void SetSamplingRate(int samplingRate) { }
}
