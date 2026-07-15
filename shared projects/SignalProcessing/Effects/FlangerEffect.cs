using Vorcyc.Mathematics.SignalProcessing.Effects.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

namespace Vorcyc.Mathematics.SignalProcessing.Effects;

/// <summary>
/// Represents Flanger audio effect.
/// </summary>
public class FlangerEffect : AudioEffect
{
    /// <summary>
    /// Internal fractional delay line.
    /// </summary>
    private FractionalDelayLine? _delayLine;

    /// <summary>
    /// Sampling rate.
    /// </summary>
    private int _fs;

    // Stored parameters for deferred initialization
    private float _widthSeconds;
    private float _reserveWidthSeconds;
    private InterpolationMode _interpolationMode;
    private float _lfoFrequencyHz;
    private bool _useCustomLfo;
    private ISampleGenerator? _customLfo;

    /// <summary>
    /// Gets or sets width (in seconds).
    /// </summary>
    public float Width
    {
        get => _widthSeconds;
        set
        {
            // 始终缓存配置值；仅在采样率已绑定后重配延迟线。
            _widthSeconds = value;
            if (_fs != 0)
                _delayLine!.Ensure(_fs, value);
        }
    }

    /// <summary>
    /// Gets or sets LFO frequency (in Hz).
    /// </summary>
    public float LfoFrequency
    {
        get => _lfoFrequencyHz;
        set
        {
            _lfoFrequencyHz = value;
            _lfo?.SetLfoFrequency(value);
        }
    }

    /// <summary>
    /// Gets or sets LFO signal generator.
    /// </summary>
    public ISampleGenerator Lfo
    {
        get => _lfo ?? throw new InvalidOperationException("Sampling rate not set. Call SetSamplingRate first.");
        set
        {
            _lfo = value;
            _lfo.SetLfoRange(0f, 1f);
        }
    }
    private ISampleGenerator? _lfo;

    /// <summary>
    /// Gets or sets depth.
    /// </summary>
    public float Depth { get; set; }

    /// <summary>
    /// Gets or sets feedback parameter.
    /// </summary>
    public float Feedback { get; set; }

    /// <summary>
    /// Gets or sets Inverted mode flag.
    /// </summary>
    public bool Inverted { get; set; }

    /// <summary>
    /// Gets or sets interpolation mode.
    /// </summary>
    public InterpolationMode InterpolationMode
    {
        get => _delayLine?.InterpolationMode ?? _interpolationMode;
        set
        {
            if (_delayLine != null)
                _delayLine.InterpolationMode = value;
            _interpolationMode = value;
        }
    }

    /// <summary>
    /// Constructs <see cref="FlangerEffect"/> with deferred sampling rate initialization.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
    /// <param name="width">Width (in seconds)</param>
    /// <param name="depth">Depth</param>
    /// <param name="feedback">Feedback</param>
    /// <param name="inverted">Inverted mode</param>
    /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
    /// <param name="reserveWidth">Max width (in seconds) for reserving the size of delay line</param>
    public FlangerEffect(float lfoFrequency = 1/*Hz*/,
                         float width = 0.003f/*sec*/,
                         float depth = 0.5f,
                         float feedback = 0,
                         bool inverted = false,
                         InterpolationMode interpolationMode = InterpolationMode.Linear,
                         float reserveWidth = 0/*sec*/)
    {
        _lfoFrequencyHz = lfoFrequency;
        _widthSeconds = width;
        _reserveWidthSeconds = reserveWidth;
        _interpolationMode = interpolationMode;
        Depth = depth;
        Feedback = feedback;
        Inverted = inverted;
        _useCustomLfo = false;
        _fs = 0;
    }

    /// <summary>
    /// Constructs <see cref="FlangerEffect"/> with deferred sampling rate initialization from custom LFO.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="lfo">LFO signal generator</param>
    /// <param name="width">Width (in seconds)</param>
    /// <param name="depth">Depth</param>
    /// <param name="feedback">Feedback</param>
    /// <param name="inverted">Inverted mode</param>
    /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
    /// <param name="reserveWidth">Max width (in seconds) for reserving the size of delay line</param>
    public FlangerEffect(ISampleGenerator lfo,
                         float width = 0.003f/*sec*/,
                         float depth = 0.5f,
                         float feedback = 0,
                         bool inverted = false,
                         InterpolationMode interpolationMode = InterpolationMode.Linear,
                         float reserveWidth = 0/*sec*/)
    {
        _customLfo = lfo;
        _widthSeconds = width;
        _reserveWidthSeconds = reserveWidth;
        _interpolationMode = interpolationMode;
        Depth = depth;
        Feedback = feedback;
        Inverted = inverted;
        _useCustomLfo = true;
        _fs = 0;
    }

    /// <summary>
    /// Constructs <see cref="FlangerEffect"/> with immediate sampling rate.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
    /// <param name="width">Width (in seconds)</param>
    /// <param name="depth">Depth</param>
    /// <param name="feedback">Feedback</param>
    /// <param name="inverted">Inverted mode</param>
    /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
    /// <param name="reserveWidth">Max width (in seconds) for reserving the size of delay line</param>
    public FlangerEffect(int samplingRate,
                         float lfoFrequency = 1/*Hz*/,
                         float width = 0.003f/*sec*/,
                         float depth = 0.5f,
                         float feedback = 0,
                         bool inverted = false,
                         InterpolationMode interpolationMode = InterpolationMode.Linear,
                         float reserveWidth = 0/*sec*/)
        : this(lfoFrequency, width, depth, feedback, inverted, interpolationMode, reserveWidth)
    {
        SetSamplingRate(samplingRate);
        LfoFrequency = lfoFrequency;
    }

    /// <summary>
    /// Constructs <see cref="FlangerEffect"/> with immediate sampling rate from custom LFO.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="lfo">LFO signal generator</param>
    /// <param name="width">Width (in seconds)</param>
    /// <param name="depth">Depth</param>
    /// <param name="feedback">Feedback</param>
    /// <param name="inverted">Inverted mode</param>
    /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
    /// <param name="reserveWidth">Max width (in seconds) for reserving the size of delay line</param>
    public FlangerEffect(int samplingRate,
                         ISampleGenerator lfo,
                         float width = 0.003f/*sec*/,
                         float depth = 0.5f,
                         float feedback = 0,
                         bool inverted = false,
                         InterpolationMode interpolationMode = InterpolationMode.Linear,
                         float reserveWidth = 0/*sec*/)
        : this(lfo, width, depth, feedback, inverted, interpolationMode, reserveWidth)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Sets sampling rate and initializes delay line and LFO.
    /// </summary>
    public override void SetSamplingRate(int samplingRate)
    {
        _fs = samplingRate;

        var effectiveReserve = _reserveWidthSeconds < _widthSeconds ? _widthSeconds : _reserveWidthSeconds;
        _delayLine = new FractionalDelayLine(samplingRate, effectiveReserve, _interpolationMode);

        if (_useCustomLfo && _customLfo != null)
        {
            _lfo = _customLfo;
            if (_lfo is SineOscillator sine)
                sine.SamplingRate = samplingRate;
        }
        else
        {
            _lfo = new SineOscillator { SamplingRate = samplingRate, Min = 0, Max = 1 };
            _lfo.SetLfoFrequency(_lfoFrequencyHz);
        }

        _lfo.SetLfoRange(0f, 1f);
    }

    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        if (_delayLine == null || _lfo == null)
            throw new InvalidOperationException("Sampling rate not set. Call SetSamplingRate first.");

        var delay = _lfo.NextSample() * _widthSeconds * _fs;

        var delayedSample = _delayLine.Read(delay);

        _delayLine.Write(sample + Feedback * delayedSample);

        return Inverted ? Dry * sample - Wet * Depth * delayedSample
                        : Dry * sample + Wet * Depth * delayedSample;
    }

    /// <summary>
    /// Resets effect.
    /// </summary>
    public override void Reset()
    {
        _delayLine?.Reset();
        _lfo?.Reset();
    }
}
