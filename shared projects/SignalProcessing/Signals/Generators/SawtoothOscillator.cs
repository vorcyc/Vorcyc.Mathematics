namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Sawtooth wave oscillator.
/// </summary>
public sealed class SawtoothOscillator : IAmplitudeOscillator
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
            UpdateCycles(resetPhase: true);
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
            UpdateCycles(resetPhase: true);
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
        var sample = Min + (Max - Min) * (_n % _cycles) / _cycles;
        _n++;
        return sample;
    }

    /// <inheritdoc />
    public void Reset() => _n = (int)(_cycles / 2);

    private void UpdateCycles(bool resetPhase)
    {
        _cycles = _samplingRate / _frequency;
        if (resetPhase)
        {
            _n = (int)(_cycles / 2);
        }
    }
}
