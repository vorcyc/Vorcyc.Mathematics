using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// Represents a frequency-domain signal interface.
/// </summary>
public interface IFrequencyDomain : IFrequencyDomainCharacteristics
{
    int Offset { get; }

    int TransformLength { get; }

    int ActualLength { get; }

    float Resolution { get; }

    WindowType? WindowApplied { get; }

    ComplexFp32[] Result { get; }

    ITimeDomainSignal Signal { get; }

    void Inverse();
}
