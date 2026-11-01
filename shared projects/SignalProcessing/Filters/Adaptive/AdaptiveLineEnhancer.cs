namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the adaptive line enhancer (ALE): an adaptive predictor that separates the narrowband (periodic) part of a signal from its
/// broadband (noise) part. The adaptive filter predicts the current sample from the samples that are at least <c>delay</c> samples old;
/// a sinusoid stays predictable over such a gap, while a noise that is uncorrelated over <c>delay</c> samples does not. The output of the filter is the
/// enhanced narrowband signal, the prediction error is the broadband residual.
/// </summary>
/// <remarks>
/// The delay has to be longer than the correlation length of the noise (1 for white noise) and the filter has to be longer than the period
/// of the lowest frequency that has to be enhanced; a longer filter gives a narrower pass band. The sample-by-sample algorithm is any
/// <see cref="AdaptiveFilter"/>, <see cref="NlmsFilter"/> by default.
/// </remarks>
public sealed class AdaptiveLineEnhancer
{
    private readonly AdaptiveFilter _filter;
    private readonly float[] _delayLine;
    private int _position;

    /// <summary>
    /// Constructs <see cref="AdaptiveLineEnhancer"/> with an <see cref="NlmsFilter"/> of the given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Order of the adaptive filter</param>
    /// <param name="delay">Decorrelation delay in samples, at least 1</param>
    /// <param name="mu">Step size of the NLMS, stable for 0 &lt; mu &lt; 2; a small step gives a narrow pass band and a slow start</param>
    /// <param name="eps">Epsilon of the NLMS</param>
    public AdaptiveLineEnhancer(int order, int delay = 1, float mu = 0.05f, float eps = 1e-3f)
        : this(new NlmsFilter(order, mu, eps), delay)
    {
    }

    /// <summary>
    /// Constructs <see cref="AdaptiveLineEnhancer"/> around the given adaptive <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">Adaptive filter that predicts the signal from its delayed samples</param>
    /// <param name="delay">Decorrelation delay in samples, at least 1</param>
    public AdaptiveLineEnhancer(AdaptiveFilter filter, int delay = 1)
    {
        ArgumentNullException.ThrowIfNull(filter);
        Guard.AgainstNonPositive(delay, nameof(delay));

        _filter = filter;
        _delayLine = new float[delay];
    }

    /// <summary>
    /// Gets the adaptive filter (its <c>Kernel</c> is the predictor).
    /// </summary>
    public AdaptiveFilter Filter => _filter;

    /// <summary>
    /// Gets the decorrelation delay in samples.
    /// </summary>
    public int Delay => _delayLine.Length;

    /// <summary>
    /// Processes one sample and returns the enhanced (narrowband) signal.
    /// </summary>
    /// <param name="input">Input sample</param>
    public float Process(float input) => Process(input, out _);

    /// <summary>
    /// Processes one sample and returns the enhanced (narrowband) signal and the broadband residual.
    /// </summary>
    /// <param name="input">Input sample</param>
    /// <param name="residual">Prediction error: <paramref name="input"/> minus the returned enhanced sample</param>
    public float Process(float input, out float residual)
    {
        var delayed = _delayLine[_position];
        _delayLine[_position] = input;
        if (++_position == _delayLine.Length)
        {
            _position = 0;
        }

        return _filter.Process(delayed, input, out residual);
    }

    /// <summary>
    /// Processes a whole buffer.
    /// </summary>
    /// <param name="input">Input samples</param>
    /// <param name="enhanced">Receives the enhanced signal (at least as long as <paramref name="input"/>)</param>
    /// <param name="residual">Receives the broadband residual; may be empty if it is not needed</param>
    public void ProcessAll(ReadOnlySpan<float> input, Span<float> enhanced, Span<float> residual = default)
    {
        if (enhanced.Length < input.Length)
            throw new ArgumentException("Output buffer is too short.", nameof(enhanced));
        if (!residual.IsEmpty && residual.Length < input.Length)
            throw new ArgumentException("Residual buffer is too short.", nameof(residual));

        for (var i = 0; i < input.Length; i++)
        {
            enhanced[i] = Process(input[i], out var r);
            if (!residual.IsEmpty)
            {
                residual[i] = r;
            }
        }
    }

    /// <summary>
    /// Restarts the signal history and the adaptation state; the weights of the predictor are preserved.
    /// </summary>
    public void Reset()
    {
        Array.Clear(_delayLine);
        _position = 0;
        _filter.Reset();
    }
}
