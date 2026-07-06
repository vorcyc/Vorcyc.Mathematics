using Vorcyc.Mathematics.SignalProcessing.Effects.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;
using Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

namespace Vorcyc.Mathematics.SignalProcessing.Effects;

/// <summary>
/// Represents Phaser audio effect.
/// </summary>
public class PhaserEffect : AudioEffect
{
    // Stored parameters for deferred initialization
    private int _fs;
    private float _lfoFrequencyHz;
    private float _minFrequencyHz;
    private float _maxFrequencyHz;
    private bool _useCustomLfo;
    private ISampleGenerator? _customLfo;

    /// <summary>
    /// Gets or sets Q factor (a.k.a. Quality Factor, resonance).
    /// </summary>
    public float Q { get; set; }

    /// <summary>
    /// Gets or sets LFO frequency (in Hz).
    /// </summary>
    public float LfoFrequency
    {
        get => _lfoFrequencyHz;
        set
        {
            _lfoFrequencyHz = value;
            Lfo?.SetLfoFrequency(value);
        }
    }

    /// <summary>
    /// Gets or sets minimal LFO frequency (in Hz).
    /// </summary>
    public float MinFrequency
    {
        get => _minFrequencyHz;
        set
        {
            _minFrequencyHz = value;
            Lfo?.SetLfoRange(value, MaxFrequency);
        }
    }

    /// <summary>
    /// Gets or sets maximal LFO frequency (in Hz).
    /// </summary>
    public float MaxFrequency
    {
        get => _maxFrequencyHz;
        set
        {
            _maxFrequencyHz = value;
            Lfo?.SetLfoRange(MinFrequency, value);
        }
    }

    /// <summary>
    /// Get or sets LFO signal generator.
    /// </summary>
    public ISampleGenerator? Lfo { get; set; }

    /// <summary>
    /// Notch filter with varying center frequency.
    /// </summary>
    private NotchFilter? _filter;

    /// <summary>
    /// Constructs <see cref="PhaserEffect"/> with deferred sampling rate initialization.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
    /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
    /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
    /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
    public PhaserEffect(float lfoFrequency = 1.0f,
                        float minFrequency = 300,
                        float maxFrequency = 3000,
                        float q = 0.5f)
    {
        _lfoFrequencyHz = lfoFrequency;
        _minFrequencyHz = minFrequency;
        _maxFrequencyHz = maxFrequency;
        Q = q;
        _useCustomLfo = false;
        _fs = 0;
    }

    /// <summary>
    /// Constructs <see cref="PhaserEffect"/> with deferred sampling rate initialization from custom LFO.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="lfo">LFO signal generator</param>
    /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
    public PhaserEffect(ISampleGenerator lfo, float q = 0.5f)
    {
        _customLfo = lfo;
        Q = q;
        _useCustomLfo = true;
        _fs = 0;
    }

    /// <summary>
    /// Constructs <see cref="PhaserEffect"/> with immediate sampling rate.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
    /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
    /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
    /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
    public PhaserEffect(int samplingRate,
                        float lfoFrequency = 1.0f,
                        float minFrequency = 300,
                        float maxFrequency = 3000,
                        float q = 0.5f)
        : this(lfoFrequency, minFrequency, maxFrequency, q)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Constructs <see cref="PhaserEffect"/> with immediate sampling rate from custom LFO.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="lfo">LFO signal generator</param>
    /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
    public PhaserEffect(int samplingRate, ISampleGenerator lfo, float q = 0.5f)
        : this(lfo, q)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Sets sampling rate and initializes LFO and filter.
    /// </summary>
    public override void SetSamplingRate(int samplingRate)
    {
        _fs = samplingRate;

        if (_useCustomLfo && _customLfo != null)
        {
            Lfo = _customLfo;
            if (Lfo is TriangleOscillator tri)
                tri.SamplingRate = samplingRate;
        }
        else
        {
            Lfo = new TriangleOscillator { SamplingRate = samplingRate };
            Lfo.SetLfoFrequency(_lfoFrequencyHz);
            Lfo.SetLfoRange(_minFrequencyHz, _maxFrequencyHz);
        }

        _filter = new NotchFilter(Lfo.NextSample() / _fs, Q);
    }

    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        if (_filter == null || Lfo == null)
            throw new InvalidOperationException("Sampling rate not set. Call SetSamplingRate first.");

        var output = _filter.Process(sample);

        _filter.Change(Lfo.NextSample() / _fs, Q);     // vary notch filter coefficients

        return output * Wet + sample * Dry;
    }

    /// <summary>
    /// Resets effect.
    /// </summary>
    public override void Reset()
    {
        _filter?.Reset();
        Lfo?.Reset();
    }
}
