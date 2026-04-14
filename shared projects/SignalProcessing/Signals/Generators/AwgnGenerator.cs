namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Additive white Gaussian noise generator (Box-Muller).
/// </summary>
public sealed class AwgnGenerator : ISampleGenerator
{
    private float _next;
    private bool _nextReady;
    private readonly Random _rand = new();

    /// <summary>
    /// Mean.
    /// </summary>
    public float Mean { get; set; }

    /// <summary>
    /// Standard deviation.
    /// </summary>
    public float Sigma { get; set; } = 1f;

    /// <inheritdoc />
    public float NextSample()
    {
        if (_nextReady)
        {
            _nextReady = false;
            return _next;
        }

        var u1 = _rand.NextDouble();
        var u2 = _rand.NextDouble();

        var r = Math.Sqrt(-2 * Math.Log(u1));
        var theta = 2 * Math.PI * u2;

        var sample = (float)(r * Math.Cos(theta) * Sigma + Mean);
        _next = (float)(r * Math.Sin(theta) * Sigma + Mean);
        _nextReady = true;

        return sample;
    }

    /// <inheritdoc />
    public void Reset() => _nextReady = false;
}
