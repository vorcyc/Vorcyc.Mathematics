using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.SignalProcessing.Effects.Stereo;

/// <summary>
/// Represents stereo delay audio effect.
/// </summary>
public class StereoDelayEffect : StereoEffect
{
    /// <summary>
    /// Left channel delay effect.
    /// </summary>
    private DelayEffect? _delayEffectLeft;

    /// <summary>
    /// Right channel delay effect.
    /// </summary>
    private DelayEffect? _delayEffectRight;

    // Stored parameters for deferred initialization
    private float _delayLeftSeconds;
    private float _delayRightSeconds;
    private float _feedbackLeft;
    private float _feedbackRight;
    private InterpolationMode _interpolationMode;
    private float _reserveDelaySeconds;

    /// <summary>
    /// Gets or sets left channel delay (in seconds).
    /// </summary>
    public float DelayLeft
    {
        get => _delayEffectLeft?.Delay ?? _delayLeftSeconds;
        set
        {
            _delayLeftSeconds = value;
            if (_delayEffectLeft != null)
                _delayEffectLeft.Delay = value;
        }
    }

    /// <summary>
    /// Gets or sets right channel delay (in seconds).
    /// </summary>
    public float DelayRight
    {
        get => _delayEffectRight?.Delay ?? _delayRightSeconds;
        set
        {
            _delayRightSeconds = value;
            if (_delayEffectRight != null)
                _delayEffectRight.Delay = value;
        }
    }

    /// <summary>
    /// Gets or sets left channel feedback.
    /// </summary>
    public float FeedbackLeft
    {
        get => _delayEffectLeft?.Feedback ?? _feedbackLeft;
        set
        {
            _feedbackLeft = value;
            if (_delayEffectLeft != null)
                _delayEffectLeft.Feedback = value;
        }
    }

    /// <summary>
    /// Gets or sets right channel feedback.
    /// </summary>
    public float FeedbackRight
    {
        get => _delayEffectRight?.Feedback ?? _feedbackRight;
        set
        {
            _feedbackRight = value;
            if (_delayEffectRight != null)
                _delayEffectRight.Feedback = value;
        }
    }

    /// <summary>
    /// Gets or sets pan.
    /// </summary>
    public float Pan { get; set; }

    /// <summary>
    /// Constructs <see cref="StereoDelayEffect"/> with deferred sampling rate initialization.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="pan">Pan</param>
    /// <param name="delayLeft">Left channel delay (in seconds)</param>
    /// <param name="delayRight">Right channel delay (in seconds)</param>
    /// <param name="feedbackLeft">Left channel feedback</param>
    /// <param name="feedbackRight">Right channel feedback</param>
    /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
    /// <param name="reserveDelay">Max delay for reserving the size of delay line</param>
    public StereoDelayEffect(float pan,
                             float delayLeft,
                             float delayRight,
                             float feedbackLeft = 0.5f,
                             float feedbackRight = 0.5f,
                             InterpolationMode interpolationMode = InterpolationMode.Nearest,
                             float reserveDelay = 0/*sec*/)
    {
        _delayLeftSeconds = delayLeft;
        _delayRightSeconds = delayRight;
        _feedbackLeft = feedbackLeft;
        _feedbackRight = feedbackRight;
        _interpolationMode = interpolationMode;
        _reserveDelaySeconds = reserveDelay;
        Pan = pan;
    }

    /// <summary>
    /// Constructs <see cref="StereoDelayEffect"/> with immediate sampling rate.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="pan">Pan</param>
    /// <param name="delayLeft">Left channel delay (in seconds)</param>
    /// <param name="delayRight">Right channel delay (in seconds)</param>
    /// <param name="feedbackLeft">Left channel feedback</param>
    /// <param name="feedbackRight">Right channel feedback</param>
    /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
    /// <param name="reserveDelay">Max delay for reserving the size of delay line</param>
    public StereoDelayEffect(int samplingRate,
                             float pan,
                             float delayLeft,
                             float delayRight,
                             float feedbackLeft = 0.5f,
                             float feedbackRight = 0.5f,
                             InterpolationMode interpolationMode = InterpolationMode.Nearest,
                             float reserveDelay = 0/*sec*/)
        : this(pan, delayLeft, delayRight, feedbackLeft, feedbackRight, interpolationMode, reserveDelay)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Sets sampling rate and initializes delay effects.
    /// </summary>
    public override void SetSamplingRate(int samplingRate)
    {
        _delayEffectLeft = new DelayEffect(_delayLeftSeconds, _feedbackLeft, _interpolationMode, _reserveDelaySeconds);
        _delayEffectLeft.SetSamplingRate(samplingRate);

        _delayEffectRight = new DelayEffect(_delayRightSeconds, _feedbackRight, _interpolationMode, _reserveDelaySeconds);
        _delayEffectRight.SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Processes one sample in each of two channels : [ input left , input right ] -> [ output left , output right ].
    /// </summary>
    /// <param name="left">Input sample in left channel</param>
    /// <param name="right">Input sample in right channel</param>
    public override void Process(ref float left, ref float right)
    {
        if (_delayEffectLeft == null || _delayEffectRight == null)
            throw new InvalidOperationException("Sampling rate not set. Call SetSamplingRate first.");

        var delayedLeft = _delayEffectLeft.Process(left);
        var delayedRight = _delayEffectRight.Process(right);

        delayedLeft *= 1 - Pan;
        delayedRight *= Pan;

        left = left * Dry + delayedLeft * Wet;
        right = right * Dry + delayedRight * Wet;
    }

    /// <summary>
    /// Resets effect.
    /// </summary>
    public override void Reset()
    {
        _delayEffectLeft?.Reset();
        _delayEffectRight?.Reset();
    }
}
