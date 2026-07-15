using Vorcyc.Mathematics.SignalProcessing.Effects.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Effects;

/// <summary>
/// Represents Delay audio effect.
/// </summary>
public class DelayEffect : AudioEffect
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
    private float _delaySeconds;
    private float _reserveDelaySeconds;
    private InterpolationMode _interpolationMode;

    /// <summary>
    /// Gets or sets delay (in seconds).
    /// </summary>
    public float Delay
    {
        // 未绑定采样率时返回已缓存的秒数（配置值），绑定后按当前 _fs 反算。
        get => _fs == 0 ? _delaySeconds : _delay / _fs;
        set
        {
            // 始终缓存配置值；仅在采样率已绑定后做依赖采样率的实时更新（延迟线/采样计数）。
            _delaySeconds = value;
            if (_fs != 0)
            {
                _delayLine!.Ensure(_fs, value);
                _delay = _fs * value;
            }
        }
    }
    private float _delay;

    /// <summary>
    /// Gets or sets feedback parameter.
    /// </summary>
    public float Feedback { get; set; }

    /// <summary>
    /// Constructs <see cref="DelayEffect"/> with deferred sampling rate initialization.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="delay">Delay (in seconds)</param>
    /// <param name="feedback">Feedback</param>
    /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
    /// <param name="reserveDelay">Max delay for reserving the size of delay line</param>
    public DelayEffect(float delay,
                       float feedback = 0.5f,
                       InterpolationMode interpolationMode = InterpolationMode.Nearest,
                       float reserveDelay = 0f)
    {
        _delaySeconds = delay;
        _reserveDelaySeconds = reserveDelay;
        _interpolationMode = interpolationMode;
        Feedback = feedback;
        _fs = 0;
    }

    /// <summary>
    /// Constructs <see cref="DelayEffect"/> with immediate sampling rate.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="delay">Delay (in seconds)</param>
    /// <param name="feedback">Feedback</param>
    /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
    /// <param name="reserveDelay">Max delay for reserving the size of delay line</param>
    public DelayEffect(int samplingRate,
                       float delay,
                       float feedback = 0.5f,
                       InterpolationMode interpolationMode = InterpolationMode.Nearest,
                       float reserveDelay = 0f)
        : this(delay, feedback, interpolationMode, reserveDelay)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Sets sampling rate and initializes delay line.
    /// </summary>
    public override void SetSamplingRate(int samplingRate)
    {
        _fs = samplingRate;

        var effectiveReserve = _reserveDelaySeconds < _delaySeconds ? _delaySeconds : _reserveDelaySeconds;
        _delayLine = new FractionalDelayLine(samplingRate, effectiveReserve, _interpolationMode);
        _delay = samplingRate * _delaySeconds;
    }

    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        if (_delayLine == null)
            throw new InvalidOperationException("Sampling rate not set. Call SetSamplingRate first.");

        var delayed = _delayLine.Read(_delay);

        var output = sample + delayed * Feedback;

        _delayLine.Write(sample);

        return sample * Dry + output * Wet;
    }

    /// <summary>
    /// Resets effect.
    /// </summary>
    public override void Reset()
    {
        _delayLine?.Reset();
    }
}
