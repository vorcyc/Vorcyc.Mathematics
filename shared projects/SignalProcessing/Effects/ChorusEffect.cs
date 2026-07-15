using Vorcyc.Mathematics.Framework;
using Vorcyc.Mathematics.SignalProcessing.Effects.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

namespace Vorcyc.Mathematics.SignalProcessing.Effects;

// Currently, the implementation is not very efficient:
// it's just a set of vibrato effects.

/// <summary>
/// Represents Chorus audio effect.
/// </summary>
public class ChorusEffect : AudioEffect
{
    // Stored parameters for deferred initialization
    private float[] _lfoFrequenciesHz;
    private float[] _widthsSeconds;
    private ISampleGenerator[]? _customLfos;
    private bool _useCustomLfos;

    /// <summary>
    /// Gets or sets widths for each voice (max delays in seconds).
    /// </summary>
    public float[] Widths
    {
        get => _voices?.Select(v => v.Width).ToArray() ?? _widthsSeconds;
        set
        {
            // 始终缓存配置值；仅在采样率已绑定（各 voice 已建）后转发到每个声部。
            _widthsSeconds = value;
            if (_voices != null)
            {
                for (var i = 0; i < _voices.Length; i++)
                {
                    _voices[i].Width = value[i];
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets LFO frequencies for each voice.
    /// </summary>
    public float[] LfoFrequencies
    {
        get => _lfoFrequenciesHz;
        set
        {
            _lfoFrequenciesHz = value;

            if (_voices != null)
            {
                for (var i = 0; i < _voices.Length; i++)
                {
                    _voices[i].LfoFrequency = value[i];
                }
            }
        }
    }

    /// <summary>
    /// Chorus voices.
    /// </summary>
    private VibratoEffect[]? _voices;

    /// <summary>
    /// Constructs <see cref="ChorusEffect"/> with deferred sampling rate initialization.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="lfoFrequencies">LFO frequencies for each voice</param>
    /// <param name="widths">Widths (max delays, in seconds) for each voice</param>
    public ChorusEffect(float[] lfoFrequencies, float[] widths)
    {
        Guard.AgainstInequality(lfoFrequencies.Length, widths.Length, "Size of frequency array", "size of widths array");

        _lfoFrequenciesHz = lfoFrequencies;
        _widthsSeconds = widths;
        _useCustomLfos = false;
    }

    /// <summary>
    /// Constructs <see cref="ChorusEffect"/> with deferred sampling rate initialization from custom LFOs.
    /// Call <see cref="SetSamplingRate"/> before using.
    /// </summary>
    /// <param name="lfos">LFOs (in the form of signal generators)</param>
    /// <param name="widths">Widths (max delays, in seconds) for each voice</param>
    public ChorusEffect(ISampleGenerator[] lfos, float[] widths)
    {
        Guard.AgainstInequality(lfos.Length, widths.Length, "Number of LFOs", "size of widths array");

        _customLfos = lfos;
        _widthsSeconds = widths;
        _lfoFrequenciesHz = new float[lfos.Length];
        _useCustomLfos = true;
    }

    /// <summary>
    /// Constructs <see cref="ChorusEffect"/> with immediate sampling rate.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="lfoFrequencies">LFO frequencies for each voice</param>
    /// <param name="widths">Widths (max delays, in seconds) for each voice</param>
    public ChorusEffect(int samplingRate, float[] lfoFrequencies, float[] widths)
        : this(lfoFrequencies, widths)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Constructs <see cref="ChorusEffect"/> with immediate sampling rate from custom LFOs.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="lfos">LFOs (in the form of signal generators)</param>
    /// <param name="widths">Widths (max delays, in seconds) for each voice</param>
    public ChorusEffect(int samplingRate, ISampleGenerator[] lfos, float[] widths)
        : this(lfos, widths)
    {
        SetSamplingRate(samplingRate);
    }

    /// <summary>
    /// Sets sampling rate and initializes all chorus voices.
    /// </summary>
    public override void SetSamplingRate(int samplingRate)
    {
        _voices = new VibratoEffect[_widthsSeconds.Length];

        if (_useCustomLfos && _customLfos != null)
        {
            for (var i = 0; i < _voices.Length; i++)
            {
                _voices[i] = new VibratoEffect(_customLfos[i], _widthsSeconds[i]);
                _voices[i].SetSamplingRate(samplingRate);
            }
        }
        else
        {
            for (var i = 0; i < _voices.Length; i++)
            {
                _voices[i] = new VibratoEffect(_lfoFrequenciesHz[i], _widthsSeconds[i]);
                _voices[i].SetSamplingRate(samplingRate);
            }
        }
    }

    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        if (_voices == null)
            throw new InvalidOperationException("Sampling rate not set. Call SetSamplingRate first.");

        var chorus = _voices.Sum(v => v.Process(sample)) / _voices.Length;

        return sample * Dry + chorus * Wet;
    }

    /// <summary>
    /// Resets effect.
    /// </summary>
    public override void Reset()
    {
        if (_voices != null)
        {
            foreach (var voice in _voices)
            {
                voice.Reset();
            }
        }
    }
}
