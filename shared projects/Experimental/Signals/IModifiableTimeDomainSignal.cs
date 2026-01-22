using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// Represents a time-domain signal that supports modification operations such as appending, inserting, removing, and
/// resampling samples.
/// </summary>
/// <remarks>This interface extends the capabilities of a time-domain signal by allowing direct manipulation of
/// its sample data. Implementations may provide thread safety for specific operations, such as asynchronous appending.
/// Modification methods enable editing the signal in-place, which is useful for real-time processing, editing, or
/// analysis scenarios. Resampling allows changing the signal's sampling rate, optionally using a specified FIR filter
/// and order for quality control.</remarks>
public interface IModifiableTimeDomainSignal : ITimeDomainSignal
{

    ModifiableTimeDomainSignal.LockedSamplesView Samples { get; }

    /// <summary>
    /// Only this method may be called from other threads.
    /// </summary>
    /// <param name="samples"></param>
    ValueTask AppendAsync(float[] samples, CancellationToken cancellationToken = default);

    int FlushPendingAppends();

    void Insert(int index, float[] samples);

    void Insert(TimeSpan timePoint, float[] samples);

    void RemoveRange(int index, int count);

    void RemoveRange(TimeSpan startTimePoint, TimeSpan duration);

    //protected void OnSamplesModified();



    #region Resampling

    /// <summary>
    /// Resamples the signal to a new sampling rate.
    /// </summary>
    /// <param name="destnationSamplingRate">The target sampling rate.</param>
    /// <param name="filter">An optional FIR filter.</param>
    /// <param name="order">The filter order; defaults to 15.</param>
    /// <returns>A resampled <see cref="ITimeDomainSignal"/> object.</returns>
    ModifiableTimeDomainSignal Resample(int destnationSamplingRate, FirFilter? filter = null, int order = 15);



    #endregion

}