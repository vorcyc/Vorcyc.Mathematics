using Vorcyc.Mathematics.SignalProcessing.Effects.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

namespace Vorcyc.Mathematics.SignalProcessing.Effects;

/// <summary>
/// Represents Wah-Wah audio effect.
/// </summary>
public class WahwahEffect : AudioEffect
{
    // Stored parameters for deferred initialization
    private int _fs;
    private float _lfoFrequencyHz;
    private float _minFrequencyHz;
    private float _maxFrequencyHz;
    private bool _useCustomLfo;
    private ISampleGenerator? _customLfo;

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
    /// Gets or sets Q factor (a.k.a. Quality Factor, resonance).
    /// </summary>
    public float Q { get; set; }

    /// <summary>
    /// Gets or sets LFO signal generator.
    /// </summary>
    public ISampleGenerator? Lfo { get; set; }

    /// <summary>
    /// Constructs <see cref="WahwahEffect"/> with deferred sampling rate initialization.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
    /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
    /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
    /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
    public WahwahEffect(float lfoFrequency = 1.0f,
                        float minFrequency = 300,
                        float maxFrequency = 1500,
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
    /// Constructs <see cref="WahwahEffect"/> with deferred sampling rate initialization from custom LFO.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="lfo">LFO signal generator</param>
    /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
    public WahwahEffect(ISampleGenerator lfo, float q = 0.5f)
    {
        _customLfo = lfo;
        Q = q;
        _useCustomLfo = true;
        _fs = 0;
    }

    /// <summary>
    /// Constructs <see cref="WahwahEffect"/> with immediate sampling rate.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
    /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
    /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
    /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
    public WahwahEffect(int samplingRate,
                        float lfoFrequency = 1.0f,
                        float minFrequency = 300,
                        float maxFrequency = 1500,
                        float q = 0.5f)
        : this(lfoFrequency, minFrequency, maxFrequency, q)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Constructs <see cref="WahwahEffect"/> with immediate sampling rate from custom LFO.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="lfo">LFO signal generator</param>
    /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
    public WahwahEffect(int samplingRate, ISampleGenerator lfo, float q = 0.5f)
        : this(lfo, q)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Sets sampling rate and initializes LFO.
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
    }

    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        if (_fs == 0 || Lfo == null)
            throw new InvalidOperationException("Sampling rate not set. Call SetSamplingRate first.");

        var fs2pi = 2 * ConstantsFp32.PI / _fs;

        var f = (2 * MathF.Sin(Lfo.NextSample() * fs2pi));

        _yh = sample - _yl - Q * _yb;
        _yb += f * _yh;
        _yl += f * _yb;

        return _yb * Wet + sample * Dry;
    }

    /// <summary>
    /// Resets effect.
    /// </summary>
    public override void Reset()
    {
        _yh = _yb = _yl = 0;
        Lfo?.Reset();
    }

    private float _yh, _yb, _yl;
}
