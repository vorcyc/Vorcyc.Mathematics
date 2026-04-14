namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Reads samples from a wave table with optional interpolation.
/// </summary>
public class WaveTableGenerator : ISampleGenerator
{
    protected float[] _samples = Array.Empty<float>();
    protected float _stride = 1f;
    protected bool _interpolate;
    protected float _n;

    /// <summary>
    /// Stride through the wave table.
    /// </summary>
    public float Stride
    {
        get => _stride;
        set
        {
            _stride = value;
            _interpolate = MathF.Abs(MathF.Round(value) - value) > 1e-5f;
        }
    }

    /// <summary>
    /// Constructs generator from wave table samples.
    /// </summary>
    public WaveTableGenerator(float[] samples)
    {
        _samples = samples;
    }

    /// <summary>
    /// Parameterless constructor for subclasses that build their own table.
    /// </summary>
    protected WaveTableGenerator() { }

    /// <inheritdoc />
    public virtual float NextSample()
    {
        var idx = ((int)_n) % _samples.Length;

        if (_interpolate)
        {
            var frac = _n - (int)_n;
            _n += _stride;
            return _samples[idx] + frac * (_samples[(idx + 1) % _samples.Length] - _samples[idx]);
        }

        _n += _stride;
        return _samples[idx];
    }

    /// <inheritdoc />
    public virtual void Reset() => _n = 0;
}
