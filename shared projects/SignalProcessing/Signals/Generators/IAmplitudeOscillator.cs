namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Oscillator with configurable amplitude range and frequency (used by audio effects for LFO tuning).
/// </summary>
public interface IAmplitudeOscillator : ISampleGenerator
{
    /// <summary>
    /// Sampling rate in Hz.
    /// </summary>
    float SamplingRate { get; set; }

    /// <summary>
    /// Oscillator frequency in Hz.
    /// </summary>
    float Frequency { get; set; }

    /// <summary>
    /// Lower output level.
    /// </summary>
    float Min { get; set; }

    /// <summary>
    /// Upper output level.
    /// </summary>
    float Max { get; set; }

    /// <summary>
    /// Initial phase in radians.
    /// </summary>
    float Phase { get; set; }
}
