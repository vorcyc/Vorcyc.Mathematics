namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Triangle wave oscillator.
/// </summary>
public sealed class TriangleOscillator : IAmplitudeOscillator
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
        var x = _n % _cycles;
        var sample = x < _cycles / 2
            ? Min + 2 * x * (Max - Min) / _cycles
            : Max + 2 * (x - _cycles / 2) * (Min - Max) / _cycles;
        _n++;
        return sample;
    }

    /// <inheritdoc />
    public void Reset() => _n = (int)(_cycles / 4);

    private void UpdateCycles(bool resetPhase)
    {
        _cycles = _samplingRate / _frequency;
        if (resetPhase)
        {
            _n = (int)(_cycles / 4);
        }
    }
}
