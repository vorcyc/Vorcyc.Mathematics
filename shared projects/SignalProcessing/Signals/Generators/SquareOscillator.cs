namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Square wave oscillator.
/// </summary>
public sealed class SquareOscillator : IAmplitudeOscillator
{
    private int _n;
    private float _cycles;

    /// <inheritdoc />
    public float SamplingRate
    {
        get => _samplingRate;
        set
        {
            _samplingRate = value;
            UpdateCycles();
        }
    }
    private float _samplingRate = 44100f;

    /// <inheritdoc />
    public float Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            UpdateCycles();
        }
    }
    private float _frequency = 100f;

    /// <inheritdoc />
    public float Min { get; set; } = -1f;

    /// <inheritdoc />
    public float Max { get; set; } = 1f;

    /// <inheritdoc />
    public float Phase { get; set; }

    /// <inheritdoc />
    public float NextSample()
    {
        var x = _n % _cycles;
        var sample = x < _cycles / 2 ? Max : Min;
        _n++;
        return sample;
    }

    /// <inheritdoc />
    public void Reset() => _n = 0;

    private void UpdateCycles() => _cycles = _samplingRate / _frequency;
}
