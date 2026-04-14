namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Linear ramp: y[n] = slope * n + intercept.
/// </summary>
public sealed class RampGenerator : ISampleGenerator
{
    private int _n;

    /// <summary>
    /// Slope.
    /// </summary>
    public float Slope { get; set; }

    /// <summary>
    /// Intercept.
    /// </summary>
    public float Intercept { get; set; }

    /// <inheritdoc />
    public float NextSample()
    {
        var sample = Slope * _n + Intercept;
        _n++;
        return sample;
    }

    /// <inheritdoc />
    public void Reset() => _n = 0;
}
