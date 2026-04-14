namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Uniform white noise generator.
/// </summary>
public sealed class WhiteNoiseGenerator : ISampleGenerator
{
    /// <summary>
    /// Lower output level.
    /// </summary>
    public float Min { get; set; } = -1f;

    /// <summary>
    /// Upper output level.
    /// </summary>
    public float Max { get; set; } = 1f;

    /// <inheritdoc />
    public float NextSample() => Random.Shared.NextSingle() * (Max - Min) + Min;

    /// <inheritdoc />
    public void Reset() { }
}
