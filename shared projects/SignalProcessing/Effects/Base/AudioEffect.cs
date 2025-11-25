using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Effects.Base;

/// <summary>
/// Abstract class for audio effects.
/// </summary>
public abstract class AudioEffect : WetDryMixer, IFilter, IOnlineFilter
{
    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public abstract float Process(float sample);

    /// <summary>
    /// Resets effect.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Applies effect to entire <paramref name="signal"/> and returns new processed signal.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="method">Filtering method</param>
    public virtual DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) 
        => this.FilterOnline(signal);


    /// <summary>
    /// Applies effect to entire <paramref name="signal"/> (in-place).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="method">Filtering method</param>
    public virtual void Apply(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) 
        => this.FilterOnline_Inplace(signal);
}
