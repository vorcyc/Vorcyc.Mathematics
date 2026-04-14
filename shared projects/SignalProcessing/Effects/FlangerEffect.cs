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
    private readonly FractionalDelayLine _delayLine;

    /// <summary>
    /// Sampling rate.
    /// </summary>
    private readonly int _fs;

    /// <summary>
    /// Gets or sets width (in seconds).
    /// </summary>
    public float Width
    {
        get => _width;
        set
        {
            _delayLine.Ensure(_fs, value);
            _width = value;
        }
    }
    private float _width;

    /// <summary>
    /// Gets or sets LFO frequency (in Hz).
    /// </summary>
    public float LfoFrequency
    {
        get => _lfoFrequency;
        set
        {
            _lfoFrequency = value;
            _lfo.SetLfoFrequency(value);
        }
    }
    private float _lfoFrequency;

    /// <summary>
    /// Gets or sets LFO signal generator.
    /// </summary>
    public ISampleGenerator Lfo
    {
        get => _lfo;
        set
        {
            _lfo = value;
            _lfo.SetLfoRange(0f, 1f);
        }
    }
    private ISampleGenerator _lfo;

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
        get => _delayLine.InterpolationMode;
        set => _delayLine.InterpolationMode = value;
    }

    /// <summary>
    /// Constructs <see cref="FlangerEffect"/>.
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

        : this(samplingRate, new SineOscillator { SamplingRate = samplingRate, Min = 0, Max = 1 }, width, depth, feedback, inverted, interpolationMode, reserveWidth)
    {
        LfoFrequency = lfoFrequency;
    }

    /// <summary>
    /// Constructs <see cref="FlangerEffect"/> from <paramref name="lfo"/>.
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
    {
        _fs = samplingRate;
        _width = width;
        Depth = depth;
        Feedback = feedback;
        Inverted = inverted;

        Lfo = lfo;

        if (reserveWidth < width)
        {
            _delayLine = new FractionalDelayLine(samplingRate, width, interpolationMode);
        }
        else
        {
            _delayLine = new FractionalDelayLine(samplingRate, reserveWidth, interpolationMode);
        }
    }

    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        var delay = _lfo.NextSample() * _width * _fs;

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
        _delayLine.Reset();
        _lfo.Reset();
    }
}
