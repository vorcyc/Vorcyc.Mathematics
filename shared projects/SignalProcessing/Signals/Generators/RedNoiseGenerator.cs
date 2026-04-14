namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Red (Brownian) noise generator.
/// </summary>
public sealed class RedNoiseGenerator : ISampleGenerator
{
    private float _prev;

    /// <summary>
    /// Lower output level.
    /// </summary>
    public float Min { get; set; } = -1f;

    /// <summary>
    /// Upper output level.
    /// </summary>
    public float Max { get; set; } = 1f;

    /// <inheritdoc />
    public float NextSample()
    {
        var mean = (Min + Max) / 2;
        var low = Min - mean;
        var high = Max - mean;

        var white = Random.Shared.NextSingle() * (high - low) + low;
        var red = (_prev + 0.02f * white) / 1.02f;
        _prev = red;
        return red * 3.5f + mean;
    }

    /// <inheritdoc />
    public void Reset() => _prev = 0;
}
