namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Linear frequency chirp oscillator. When <see cref="Length"/> &gt; 0, wraps at the end of the cycle.
/// </summary>
public sealed class ChirpOscillator : ISampleGenerator
{
    private int _n;

    /// <summary>
    /// Sampling rate in Hz.
    /// </summary>
    public float SamplingRate { get; set; } = 44100f;

    /// <summary>
    /// Number of samples per chirp cycle (0 = no wrap).
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Lower output level.
    /// </summary>
    public float Min { get; set; } = -1f;

    /// <summary>
    /// Upper output level.
    /// </summary>
    public float Max { get; set; } = 1f;

    /// <summary>
    /// Start frequency in Hz.
    /// </summary>
    public float StartFrequency { get; set; } = 100f;

    /// <summary>
    /// End frequency in Hz.
    /// </summary>
    public float EndFrequency { get; set; } = 1000f;

    /// <inheritdoc />
    public float NextSample()
    {
        var cycleLength = Length > 0 ? Length : 1;
        var k = (EndFrequency - StartFrequency) / cycleLength;
        var fs = SamplingRate;

        var sample = MathF.Cos(2 * MathF.PI * (StartFrequency / fs + k * _n) * _n / fs);
        sample = Min + (Max - Min) * (1 + sample) / 2;

        if (Length > 0 && ++_n == Length)
        {
            _n = 0;
        }
        else if (Length <= 0)
        {
            _n++;
        }

        return sample;
    }

    /// <inheritdoc />
    public void Reset() => _n = 0;
}
