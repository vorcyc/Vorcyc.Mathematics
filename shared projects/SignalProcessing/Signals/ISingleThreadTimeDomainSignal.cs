using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// Represents a time-domain signal that supports single-threaded access to its sample data and resampling operations.
/// </summary>
public interface ISingleThreadTimeDomainSignal : ITimeDomainSignal
{
    /// <summary>
    /// Gets the signal sample array.
    /// </summary>
    Span<float> Samples { get; }

    /// <summary>
    /// Resamples the signal to a new sampling rate.
    /// </summary>
    /// <param name="destinationSamplingRate">The target sampling rate in Hz.</param>
    /// <param name="filter">An optional FIR filter.</param>
    /// <param name="order">The filter order; defaults to 15.</param>
    Signal Resample(float destinationSamplingRate, FirFilter? filter = null, int order = 15);
}
