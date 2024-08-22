using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base
{
    /// <summary>
    /// Interface for offline filters.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Applies filter to entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto);


        ///// <summary>
        ///// Applies effect to entire <paramref name="signal"/> (in-place).
        ///// </summary>
        ///// <param name="signal">Signal</param>
        ///// <param name="method">Filtering method</param>
        //void Apply(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto);
    }
}
