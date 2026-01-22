using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// Represents a frequency-domain signal interface, inheriting from <see cref="IFrequencyDomainCharacteristics"/>.
/// </summary>
public interface IFrequencyDomain : IFrequencyDomainCharacteristics
{
    /// <summary>
    /// Gets the offset of the frequency-domain transform within the time-domain signal.
    /// </summary>
    int Offset { get; }

    /// <summary>
    /// Gets the length of the frequency-domain transform.
    /// </summary>
    /// <remarks>
    /// The length is a power of 2. It may include zero-padding for FFT purposes, so it is >= <see cref="ActualLength"/>.
    /// </remarks>
    int TransformLength { get; }

    /// <summary>
    /// Gets the actual length of the signal.
    /// </summary>
    int ActualLength { get; }

    /// <summary>
    /// Gets the frequency resolution.
    /// </summary>
    float Resolution { get; }

    /// <summary>
    /// Gets the window type applied to the signal.
    /// </summary>
    WindowType? WindowApplied { get; }

    /// <summary>
    /// Gets the complex number array of the FFT result.
    /// </summary>
    ComplexFp32[] Result { get; }

    /// <summary>
    /// Gets the original time-domain signal.
    /// </summary>
    ITimeDomainSignal Signal { get; }

    /// <summary>
    /// Performs an inverse transform on the FFT result and writes the output back to the time-domain signal.
    /// This method modifies the sample values of the original time-domain signal.
    /// </summary>
    void Inverse();
}