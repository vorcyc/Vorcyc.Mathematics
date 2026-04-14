namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Applies fade-in / fade-out to an underlying sample generator.
/// </summary>
public sealed class FadeInOutGenerator : ISampleGenerator
{
    private readonly ISampleGenerator _inner;
    private int _fadeInSampleCount;
    private int _fadeOutSampleCount;
    private int _fadeInIndex;
    private int _fadeOutIndex;
    private int _index;

    /// <summary>
    /// Total length in samples.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Sampling rate in Hz.
    /// </summary>
    public float SamplingRate { get; }

    /// <summary>
    /// Is fade-out in progress.
    /// </summary>
    public bool FadeStarted { get; private set; }

    /// <summary>
    /// Is fade-out finished.
    /// </summary>
    public bool FadeFinished => _fadeOutIndex <= 0;

    /// <summary>
    /// Wraps <paramref name="inner"/> generator.
    /// </summary>
    public FadeInOutGenerator(ISampleGenerator inner, int length, float samplingRate)
    {
        _inner = inner;
        Length = length;
        SamplingRate = samplingRate;
        _fadeOutIndex = _fadeOutSampleCount - 1;
    }

    /// <summary>
    /// Sets fade-in duration in seconds.
    /// </summary>
    public FadeInOutGenerator FadeIn(double seconds)
    {
        _fadeInSampleCount = (int)(seconds * SamplingRate);
        return this;
    }

    /// <summary>
    /// Sets fade-out duration in seconds.
    /// </summary>
    public FadeInOutGenerator FadeOut(double seconds)
    {
        _fadeOutSampleCount = (int)(seconds * SamplingRate);
        _fadeOutIndex = _fadeOutSampleCount - 1;
        return this;
    }

    /// <summary>
    /// Starts fade-out immediately.
    /// </summary>
    public void BeginFadeOut()
    {
        if (_fadeOutSampleCount > 0)
        {
            _fadeOutIndex = _fadeOutSampleCount - 1;
            FadeStarted = true;
        }
    }

    /// <inheritdoc />
    public float NextSample()
    {
        var sample = _inner.NextSample();

        if (FadeStarted || _index++ > Length - _fadeOutSampleCount)
        {
            sample *= (float)_fadeOutIndex-- / _fadeOutSampleCount;
            FadeStarted = !FadeFinished;
        }

        if (_fadeInIndex < _fadeInSampleCount)
        {
            sample *= (float)_fadeInIndex++ / _fadeInSampleCount;
        }

        return sample;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _inner.Reset();
        _index = 0;
        _fadeInIndex = 0;
        _fadeOutIndex = _fadeOutSampleCount - 1;
        FadeStarted = false;
    }
}
