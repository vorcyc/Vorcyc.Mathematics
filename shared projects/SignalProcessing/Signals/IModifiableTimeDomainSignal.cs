using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// Represents a time-domain signal that supports modification operations such as appending, inserting, removing, and
/// resampling samples.
/// </summary>
public interface IModifiableTimeDomainSignal : ITimeDomainSignal
{
    ModifiableTimeDomainSignal.LockedSamplesView Samples { get; }

    ValueTask AppendAsync(float[] samples, CancellationToken cancellationToken = default);

    int FlushPendingAppends();

    void Insert(int index, float[] samples);

    void Insert(TimeSpan timePoint, float[] samples);

    void RemoveRange(int index, int count);

    void RemoveRange(TimeSpan startTimePoint, TimeSpan duration);

    ModifiableTimeDomainSignal Resample(float destinationSamplingRate, FirFilter? filter = null, int order = 15);
}
