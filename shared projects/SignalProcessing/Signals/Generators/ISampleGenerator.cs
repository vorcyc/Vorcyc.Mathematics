namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Online sample-by-sample signal generator (streaming / LFO use).
/// </summary>
public interface ISampleGenerator
{
    /// <summary>
    /// Generates the next sample.
    /// </summary>
    float NextSample();

    /// <summary>
    /// Resets generator state.
    /// </summary>
    void Reset();
}
