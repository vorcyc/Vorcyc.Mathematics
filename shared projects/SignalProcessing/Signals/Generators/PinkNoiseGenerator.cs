namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Pink noise generator (Paul Kellet's algorithm).
/// </summary>
public sealed class PinkNoiseGenerator : ISampleGenerator
{
    private float _b0, _b1, _b2, _b3, _b4, _b5, _b6;
    private readonly Random _rand = new();

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

        var white = (float)(_rand.NextDouble() * (high - low) + low);

        _b0 = 0.99886f * _b0 + white * 0.0555179f;
        _b1 = 0.99332f * _b1 + white * 0.0750759f;
        _b2 = 0.96900f * _b2 + white * 0.1538520f;
        _b3 = 0.86650f * _b3 + white * 0.3104856f;
        _b4 = 0.55000f * _b4 + white * 0.5329522f;
        _b5 = -0.7616f * _b5 - white * 0.0168980f;
        var pink = (_b0 + _b1 + _b2 + _b3 + _b4 + _b5 + _b6 + white * 0.5362f) * 0.11f + mean;
        _b6 = white * 0.115926f;

        return pink;
    }

    /// <inheritdoc />
    public void Reset() => _b0 = _b1 = _b2 = _b3 = _b4 = _b5 = _b6 = 0;
}
