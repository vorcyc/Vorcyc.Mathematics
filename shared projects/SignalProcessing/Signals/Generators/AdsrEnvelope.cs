namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// ADSR envelope generator (sample counts).
/// </summary>
public sealed class AdsrEnvelope : ISampleGenerator
{
    /// <summary>
    /// ADSR stage.
    /// </summary>
    public enum AdsrState
    {
        Attack,
        Decay,
        Sustain,
        Release
    }

    private AdsrState _state;
    private float _attack;
    private float _decay;
    private float _sustain;
    private float _release;

    private float _attackAmp = 1.5f;
    private float _attackSlope = 0.2f;
    private float _decaySlope = 0.2f;
    private float _sustainSlope = 0.2f;
    private float _releaseSlope = 0.2f;

    private float _a, _b;
    private int _n;
    private float _prev;

    /// <summary>
    /// Sampling rate in Hz (used when durations were specified in seconds).
    /// </summary>
    public float SamplingRate { get; set; } = 44100f;

    /// <summary>
    /// Gets current ADSR stage.
    /// </summary>
    public AdsrState State
    {
        get => _state;
        private set
        {
            _state = value;
            UpdateCoefficients();
        }
    }

    /// <summary>
    /// Attack peak amplitude.
    /// </summary>
    public float AttackAmplitude
    {
        get => _attackAmp;
        set => _attackAmp = value;
    }

    /// <summary>
    /// Constructs envelope from stage lengths in samples.
    /// </summary>
    public AdsrEnvelope(int attack, int decay, int sustain, int release)
    {
        _attack = attack;
        _decay = _attack + decay;
        _sustain = _decay + sustain;
        _release = _sustain + release;
        Reset();
    }

    /// <summary>
    /// Constructs envelope from stage durations in seconds (converted when <see cref="SamplingRate"/> is set).
    /// </summary>
    public AdsrEnvelope(float attackSeconds, float decaySeconds, float sustainSeconds, float releaseSeconds)
    {
        _attack = attackSeconds;
        _decay = _attack + decaySeconds;
        _sustain = _decay + sustainSeconds;
        _release = _sustain + releaseSeconds;
        Reset();
    }

    /// <summary>
    /// Converts second-based stage boundaries to samples using <see cref="SamplingRate"/>.
    /// </summary>
    public void ConfigureSamplingRate(float samplingRate)
    {
        SamplingRate = samplingRate;
        _attack *= samplingRate;
        _decay *= samplingRate;
        _sustain *= samplingRate;
        _release *= samplingRate;
        UpdateCoefficients();
    }

    /// <inheritdoc />
    public float NextSample()
    {
        float cur;

        if (_n > _sustain)
        {
            if (_state != AdsrState.Release)
            {
                State = AdsrState.Release;
            }
            cur = 0;
        }
        else if (_n > _decay)
        {
            if (_state != AdsrState.Sustain)
            {
                State = AdsrState.Sustain;
            }
            cur = 1;
        }
        else if (_n > _attack)
        {
            if (_state != AdsrState.Decay)
            {
                State = AdsrState.Decay;
            }
            cur = 1;
        }
        else
        {
            cur = _attackAmp;
        }

        _prev = _b * cur - _a * _prev;

        if (++_n == _release)
        {
            _n = 0;
        }

        return _prev;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _n = 0;
        _prev = 0;
        State = AdsrState.Attack;
    }

    private void UpdateCoefficients()
    {
        switch (_state)
        {
            case AdsrState.Release:
                _a = -MathF.Exp(-1 / ((_release - _sustain) * _releaseSlope));
                break;
            case AdsrState.Sustain:
                _a = -MathF.Exp(-1 / ((_sustain - _decay) * _sustainSlope));
                break;
            case AdsrState.Decay:
                _a = -MathF.Exp(-1 / ((_decay - _attack) * _decaySlope));
                break;
            default:
                _a = -MathF.Exp(-1 / (_attack * _attackSlope));
                break;
        }
        _b = 1 + _a;
    }
}
