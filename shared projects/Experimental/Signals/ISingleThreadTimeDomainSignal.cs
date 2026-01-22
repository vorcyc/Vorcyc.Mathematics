using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// Represents a time-domain signal that supports single-threaded access to its sample data and resampling operations.
/// </summary>
/// <remarks>This interface provides direct access to the signal's sample array and methods for resampling the
/// signal to different sampling rates. Implementations are intended for use in single-threaded scenarios; concurrent
/// access is not supported.</remarks>
public interface ISingleThreadTimeDomainSignal : ITimeDomainSignal
{

    /// <summary>
    /// Gets the signal sample array.
    /// </summary>
    Span<float> Samples { get; }


    #region Resampling

    /// <summary>
    /// Resamples the signal to a new sampling rate.
    /// </summary>
    /// <param name="destnationSamplingRate">The target sampling rate.</param>
    /// <param name="filter">An optional FIR filter.</param>
    /// <param name="order">The filter order; defaults to 15.</param>
    /// <returns>A resampled <see cref="ITimeDomainSignal"/> object.</returns>
    Signal Resample(int destnationSamplingRate, FirFilter? filter = null, int order = 15);



    #endregion

}