namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Cosinusoidal oscillator with amplitude mapping to [<see cref="Min"/>, <see cref="Max"/>].
/// </summary>
public sealed class CosineOscillator : IAmplitudeOscillator
{
    private int _n;

    /// <inheritdoc />
    public float SamplingRate { get; set; } = 44100f;

    /// <inheritdoc />
    public float Frequency { get; set; } = 100f;

    /// <inheritdoc />
    public float Min { get; set; } = -1f;

    /// <inheritdoc />
    public float Max { get; set; } = 1f;

    /// <inheritdoc />
    public float Phase { get; set; }

    /// <inheritdoc />
    public float NextSample()
    {
        var sample = MathF.Cos(2 * MathF.PI * Frequency / SamplingRate * _n + Phase);
        sample = Min + (Max - Min) * (1 + sample) / 2;
        _n++;
        return sample;
    }

    /// <inheritdoc />
    public void Reset() => _n = 0;
}
