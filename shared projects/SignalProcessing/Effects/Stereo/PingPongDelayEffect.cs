using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.SignalProcessing.Effects.Stereo;

/// <summary>
/// Represents stereo ping-pong delay audio effect.
/// </summary>
public class PingPongDelayEffect : StereoEffect
{
    /// <summary>
    /// Left channel delay line.
    /// </summary>
    private FractionalDelayLine? _delayLineLeft;

    /// <summary>
    /// Righ channel delay line.
    /// </summary>
    private FractionalDelayLine? _delayLineRight;

    /// <summary>
    /// Sampling rate.
    /// </summary>
    private int _fs;

    // Stored parameters for deferred initialization
    private float _delaySeconds;
    private float _reserveDelaySeconds;
    private InterpolationMode _interpolationMode;

    /// <summary>
    /// Gets or sets pan.
    /// </summary>
    public float Pan { get; set; }

    /// <summary>
    /// Gets or sets delay (in seconds).
    /// </summary>
    public float Delay
    {
        // 未绑定采样率时返回已缓存的秒数（配置值），绑定后按当前 _fs 反算。
        get => _fs == 0 ? _delaySeconds : _delay / _fs;
        set
        {
            // 始终缓存配置值；仅在采样率已绑定后做依赖采样率的实时更新（左右延迟线/采样计数）。
            _delaySeconds = value;
            if (_fs != 0)
            {
                _delayLineLeft!.Ensure(_fs, value);
                _delayLineRight!.Ensure(_fs, value);
                _delay = _fs * value;
            }
        }
    }
    private float _delay;

    /// <summary>
    /// Gets or sets feedback coefficient.
    /// </summary>
    public float Feedback { get; set; }

    /// <summary>
    /// Constructs <see cref="PingPongDelayEffect"/> with deferred sampling rate initialization.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="pan">Pan</param>
    /// <param name="delay">Delay (in seconds)</param>
    /// <param name="feedback">Feedback</param>
    /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
    /// <param name="reserveDelay">Max delay for reserving the size of delay line</param>
    public PingPongDelayEffect(float pan,
                               float delay,
                               float feedback = 0.5f,
                               InterpolationMode interpolationMode = InterpolationMode.Nearest,
                               float reserveDelay = 0/*sec*/)
    {
        _delaySeconds = delay;
        _reserveDelaySeconds = reserveDelay;
        _interpolationMode = interpolationMode;
        Feedback = feedback;
        Pan = pan;
        _fs = 0;  // Mark as uninitialized
    }

    /// <summary>
    /// Constructs <see cref="PingPongDelayEffect"/> with immediate sampling rate.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="pan">Pan</param>
    /// <param name="delay">Delay (in seconds)</param>
    /// <param name="feedback">Feedback</param>
    /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
    /// <param name="reserveDelay">Max delay for reserving the size of delay line</param>
    public PingPongDelayEffect(int samplingRate,
                               float pan,
                               float delay,
                               float feedback = 0.5f,
                               InterpolationMode interpolationMode = InterpolationMode.Nearest,
                               float reserveDelay = 0/*sec*/)
        : this(pan, delay, feedback, interpolationMode, reserveDelay)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Sets sampling rate and initializes delay lines.
    /// </summary>
    public override void SetSamplingRate(int samplingRate)
    {
        _fs = samplingRate;

        var effectiveReserve = _reserveDelaySeconds < _delaySeconds ? _delaySeconds : _reserveDelaySeconds;
        _delayLineLeft = new FractionalDelayLine(samplingRate, effectiveReserve, _interpolationMode);
        _delayLineRight = new FractionalDelayLine(samplingRate, effectiveReserve, _interpolationMode);
        _delay = samplingRate * _delaySeconds;
    }

    /// <summary>
    /// Processes one sample in each of two channels : [ input left , input right ] -> [ output left , output right ].
    /// </summary>
    /// <param name="left">Input sample in left channel</param>
    /// <param name="right">Input sample in right channel</param>
    public override void Process(ref float left, ref float right)
    {
        if (_delayLineLeft == null || _delayLineRight == null)
            throw new InvalidOperationException("Sampling rate not set. Call SetSamplingRate first.");

        var delayedLeft = _delayLineLeft.Read(_delay);
        var delayedRight = _delayLineRight.Read(_delay);

        var processedLeft = left * (1 - Pan) + delayedRight * Feedback;
        var processedRight = right * Pan + delayedLeft * Feedback;

        _delayLineLeft.Write(processedLeft);
        _delayLineRight.Write(processedRight);

        left = left * Dry + processedLeft * Wet;
        right = right * Dry + processedRight * Wet;
    }

    /// <summary>
    /// Resets effect.
    /// </summary>
    public override void Reset()
    {
        _delayLineLeft?.Reset();
        _delayLineRight?.Reset();
    }
}
