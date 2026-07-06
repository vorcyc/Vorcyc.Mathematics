using Vorcyc.Mathematics.SignalProcessing.Effects.Base;
using Vorcyc.Mathematics.SignalProcessing.Operations;

namespace Vorcyc.Mathematics.SignalProcessing.Effects;

/// <summary>
/// Represents AutoWah audio effect (envelope follower + Wah-Wah effect).
/// </summary>
public class AutowahEffect : AudioEffect
{
    // Stored parameters for deferred initialization
    private int _fs;
    private float _minFrequencyHz;
    private float _maxFrequencyHz;
    private float _attackTimeSeconds;
    private float _releaseTimeSeconds;

    /// <summary>
    /// Gets or sets Q factor (a.k.a. Quality Factor, resonance).
    /// </summary>
    public float Q { get; set; }

    /// <summary>
    /// Gets or sets minimal LFO frequency (in Hz).
    /// </summary>
    public float MinFrequency
    {
        get => _minFrequencyHz;
        set => _minFrequencyHz = value;
    }

    /// <summary>
    /// Gets or sets maximal LFO frequency (in Hz).
    /// </summary>
    public float MaxFrequency
    {
        get => _maxFrequencyHz;
        set => _maxFrequencyHz = value;
    }

    /// <summary>
    /// Gets or sets attack time (in seconds).
    /// </summary>
    public float AttackTime
    {
        get => _envelopeFollower?.AttackTime ?? _attackTimeSeconds;
        set
        {
            _attackTimeSeconds = value;
            if (_envelopeFollower != null)
                _envelopeFollower.AttackTime = value;
        }
    }

    /// <summary>
    /// Gets or sets release time (in seconds).
    /// </summary>
    public float ReleaseTime
    {
        get => _envelopeFollower?.ReleaseTime ?? _releaseTimeSeconds;
        set
        {
            _releaseTimeSeconds = value;
            if (_envelopeFollower != null)
                _envelopeFollower.ReleaseTime = value;
        }
    }

    /// <summary>
    /// Internal envelope follower.
    /// </summary>
    private EnvelopeFollower? _envelopeFollower;

    /// <summary>
    /// Constructs <see cref="AutowahEffect"/> with deferred sampling rate initialization.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
    /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
    /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
    /// <param name="attackTime">Attack time (in seconds)</param>
    /// <param name="releaseTime">Release time (in seconds)</param>
    public AutowahEffect(float minFrequency = 30,
                         float maxFrequency = 2000,
                         float q = 0.5f,
                         float attackTime = 0.01f,
                         float releaseTime = 0.05f)
    {
        _minFrequencyHz = minFrequency;
        _maxFrequencyHz = maxFrequency;
        Q = q;
        _attackTimeSeconds = attackTime;
        _releaseTimeSeconds = releaseTime;
        _fs = 0;
    }

    /// <summary>
    /// Constructs <see cref="AutowahEffect"/> with immediate sampling rate.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
    /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
    /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
    /// <param name="attackTime">Attack time (in seconds)</param>
    /// <param name="releaseTime">Release time (in seconds)</param>
    public AutowahEffect(int samplingRate,
                         float minFrequency = 30,
                         float maxFrequency = 2000,
                         float q = 0.5f,
                         float attackTime = 0.01f,
                         float releaseTime = 0.05f)
        : this(minFrequency, maxFrequency, q, attackTime, releaseTime)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Sets sampling rate and initializes envelope follower.
    /// </summary>
    public override void SetSamplingRate(int samplingRate)
    {
        _fs = samplingRate;
        _envelopeFollower = new EnvelopeFollower(samplingRate, _attackTimeSeconds, _releaseTimeSeconds);
    }

    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        if (_fs == 0 || _envelopeFollower == null)
            throw new InvalidOperationException("Sampling rate not set. Call SetSamplingRate first.");

        var env = _envelopeFollower.Process(sample) * MathF.Sqrt(Q);

        var frequencyRange = ConstantsFp32.PI * (MaxFrequency - MinFrequency) / _fs;
        var minFreq = ConstantsFp32.PI * MinFrequency / _fs;

        var centerFrequency = env * frequencyRange + minFreq;

        var f = (2 * MathF.Sin(centerFrequency));

        _yh = sample - _yl - Q * _yb;
        _yb += f * _yh;
        _yl += f * _yb;

        return Wet * _yb + Dry * sample;
    }

    /// <summary>
    /// Resets effect.
    /// </summary>
    public override void Reset()
    {
        _yh = _yl = _yb = 0;
        _envelopeFollower?.Reset();
    }

    private float _yh, _yb, _yl;
}
