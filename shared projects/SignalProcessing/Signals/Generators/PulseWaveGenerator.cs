namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Periodic pulse wave generator.
/// </summary>
public sealed class PulseWaveGenerator : ISampleGenerator
{
    private int _n;

    /// <summary>
    /// Sampling rate in Hz.
    /// </summary>
    public float SamplingRate { get; set; } = 44100f;

    /// <summary>
    /// Lower output level.
    /// </summary>
    public float Min { get; set; } = -1f;

    /// <summary>
    /// Upper output level.
    /// </summary>
    public float Max { get; set; } = 1f;

    /// <summary>
    /// Pulse duration in seconds.
    /// </summary>
    public float PulseDuration { get; set; } = 0.05f;

    /// <summary>
    /// Period in seconds.
    /// </summary>
    public float Period { get; set; } = 0.1f;

    /// <inheritdoc />
    public float NextSample()
    {
        var sample = _n <= (int)(PulseDuration * SamplingRate) ? Max : Min;

        if (++_n == (int)(Period * SamplingRate))
        {
            _n = 0;
        }

        return sample;
    }

    /// <inheritdoc />
    public void Reset() => _n = 0;
}
