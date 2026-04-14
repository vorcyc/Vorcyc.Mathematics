namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Karplus-Strong plucked string synthesizer.
/// </summary>
public class KarplusStrongGenerator : WaveTableGenerator
{
    protected float _prev;
    protected readonly Random _rand = new();

    /// <summary>
    /// Sampling rate in Hz.
    /// </summary>
    public float SamplingRate { get; set; } = 44100f;

    /// <summary>
    /// Resonant frequency in Hz.
    /// </summary>
    public float Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            if (SamplingRate > 0 && value > 0)
            {
                GenerateWaveTable((int)(SamplingRate / value));
            }
        }
    }
    private float _frequency = 100f;

    /// <summary>
    /// Stretch factor.
    /// </summary>
    public float StretchFactor { get; set; } = 1f;

    /// <summary>
    /// Feedback coefficient in [0, 1].
    /// </summary>
    public float Feedback { get; set; } = 1f;

    /// <summary>
    /// Constructs generator with an auto-sized wave table.
    /// </summary>
    public KarplusStrongGenerator() : base() { }

    /// <summary>
    /// Constructs generator from an existing wave table.
    /// </summary>
    public KarplusStrongGenerator(float[] samples) : base(samples) { }

    /// <inheritdoc />
    public override float NextSample()
    {
        var idx = ((int)_n) % _samples.Length;

        if (_rand.NextDouble() < 1 / StretchFactor)
        {
            _samples[idx] = 0.5f * (_samples[idx] + _prev) * Feedback;
        }

        _prev = _samples[idx];
        _n++;

        return _prev;
    }

    /// <inheritdoc />
    public override void Reset()
    {
        var values = new[] { -1f, 1f };
        for (var i = 0; i < _samples.Length; i++)
        {
            _samples[i] = values[_rand.Next(2)];
        }

        base.Reset();
    }

    /// <summary>
    /// Fills the wave table with random ±1 values.
    /// </summary>
    protected void GenerateWaveTable(int sampleCount)
    {
        var values = new[] { -1f, 1f };
        _samples = Enumerable.Range(0, sampleCount).Select(_ => values[_rand.Next(2)]).ToArray();
    }
}
