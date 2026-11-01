namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the multichannel NLMS Adaptive filter: one desired signal is estimated from several input channels, each filtered by its own FIR filter of the same order,
/// y(n) = Σ<sub>c</sub> Σ<sub>k</sub> w<sub>c</sub>[k]·x<sub>c</sub>(n−k). All taps of all channels are adapted together by one NLMS step normalized by the power of the whole multichannel regressor:
/// w<sub>c</sub>[k] += μ · e · x<sub>c</sub>(n−k) / (ε + Σ<sub>c</sub> Σ<sub>k</sub> x<sub>c</sub>(n−k)²).
/// </summary>
/// <remarks>
/// Typical uses are stereo / multichannel echo cancellation (the reference channels are the loudspeaker signals), multi-sensor noise cancellation
/// and identification of multiple-input single-output systems. The algorithm is stable for 0 &lt; μ &lt; 2 regardless of the level of the inputs.
/// If the channels are strongly correlated (as in stereo echo cancellation) the problem is ill-conditioned and the convergence is slow.
/// </remarks>
public sealed class MultichannelNlmsFilter
{
    private readonly float _mu;
    private readonly float _eps;

    /// <summary>
    /// Taps of all channels, channel after channel.
    /// </summary>
    private readonly float[] _weights;

    /// <summary>
    /// Delay lines of all channels, every sample stored twice, so that the regressor of a channel is contiguous.
    /// </summary>
    private readonly float[] _delayLines;
    private int _offset;

    /// <summary>
    /// Constructs <see cref="MultichannelNlmsFilter"/>.
    /// </summary>
    /// <param name="channels">Number of input channels</param>
    /// <param name="order">Order (number of taps) of the filter of every channel</param>
    /// <param name="mu">Step size, stable for 0 &lt; mu &lt; 2</param>
    /// <param name="eps">Epsilon, regularizes the normalization (added to the power of the multichannel regressor)</param>
    public MultichannelNlmsFilter(int channels, int order, float mu = 0.5f, float eps = 1e-3f)
    {
        Guard.AgainstNonPositive(channels, nameof(channels));
        Guard.AgainstNonPositive(order, nameof(order));
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.NonNegative(eps, nameof(eps));
        var weightsLength = AdaptiveArgument.Count((long)channels * order, nameof(order));
        var delayLinesLength = AdaptiveArgument.Count((long)channels * 2 * order, nameof(order));

        Channels = channels;
        Order = order;
        _mu = mu;
        _eps = eps;
        _weights = new float[weightsLength];
        _delayLines = new float[delayLinesLength];
    }

    /// <summary>
    /// Gets the number of input channels.
    /// </summary>
    public int Channels { get; }

    /// <summary>
    /// Gets the order (number of taps) of the filter of every channel.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets a copy of the taps of the filter of the given <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Channel index</param>
    public float[] GetWeights(int channel)
    {
        if ((uint)channel >= (uint)Channels)
            throw new ArgumentOutOfRangeException(nameof(channel));

        return _weights.AsSpan(channel * Order, Order).ToArray();
    }

    /// <summary>
    /// Inits the taps of all channels. The signal history is kept.
    /// </summary>
    /// <param name="weights">Taps of all channels, channel after channel (<see cref="Channels"/> · <see cref="Order"/> finite values)</param>
    /// <exception cref="ArgumentException"><paramref name="weights"/> does not have <see cref="Channels"/> · <see cref="Order"/> elements or holds NaN or infinity; the filter is not changed then.</exception>
    public void Init(ReadOnlySpan<float> weights)
    {
        if (weights.Length != _weights.Length)
            throw new ArgumentException("The number of weights must be equal to the number of channels times the filter order.", nameof(weights));
        AdaptiveArgument.AllFinite(weights, nameof(weights));

        weights.CopyTo(_weights);
    }

    /// <summary>
    /// Restarts the signal history. Current taps are preserved; use <see cref="Init(ReadOnlySpan{float})"/> to restart them as well.
    /// </summary>
    public void Reset()
    {
        _offset = 0;
        Array.Clear(_delayLines);
    }

    /// <summary>
    /// Pushes one sample of every channel into the delay lines and returns the output of the current taps.
    /// </summary>
    private float Push(ReadOnlySpan<float> inputs, out int offset)
    {
        if (inputs.Length != Channels)
            throw new ArgumentException("Exactly one sample per channel is expected.", nameof(inputs));

        _offset = _offset == 0 ? Order - 1 : _offset - 1;
        offset = _offset;

        double y = 0;
        for (var c = 0; c < Channels; c++)
        {
            var line = c * 2 * Order;
            _delayLines[line + offset] = inputs[c];
            _delayLines[line + offset + Order] = inputs[c];

            var w = c * Order;
            for (var k = 0; k < Order; k++)
            {
                y += (double)_weights[w + k] * _delayLines[line + offset + k];
            }
        }
        return (float)y;
    }

    /// <summary>
    /// Processes one sample of every channel with the current taps and without adapting them.
    /// </summary>
    /// <param name="inputs">One sample of every channel (<see cref="Channels"/> values)</param>
    public float Process(ReadOnlySpan<float> inputs) => Push(inputs, out _);

    /// <summary>
    /// Processes one sample of every channel and the desired signal and adapts the taps.
    /// </summary>
    /// <param name="inputs">One sample of every channel (<see cref="Channels"/> values)</param>
    /// <param name="desired">Sample of desired signal</param>
    /// <returns>A priori filter output</returns>
    public float Process(ReadOnlySpan<float> inputs, float desired)
    {
        var y = Push(inputs, out var offset);
        var e = desired - y;

        // a non-finite sample (an input of any channel or the desired one) must not reach the taps
        if (!float.IsFinite(e))
        {
            return y;
        }

        var power = 0.0;
        for (var c = 0; c < Channels; c++)
        {
            var line = c * 2 * Order + offset;
            for (var k = 0; k < Order; k++)
            {
                var v = _delayLines[line + k];
                power += (double)v * v;
            }
        }

        // with eps = 0 a regressor of zeros would give 0 / 0
        var norm = _eps + power;
        if (!(norm > 0))
        {
            return y;
        }

        var step = _mu * e / norm;
        for (var c = 0; c < Channels; c++)
        {
            var line = c * 2 * Order + offset;
            var w = c * Order;
            for (var k = 0; k < Order; k++)
            {
                _weights[w + k] = (float)(_weights[w + k] + step * _delayLines[line + k]);
            }
        }

        return y;
    }

    /// <summary>
    /// Processes one sample of every channel and the desired signal, adapts the taps and returns the error as well.
    /// </summary>
    /// <param name="inputs">One sample of every channel (<see cref="Channels"/> values)</param>
    /// <param name="desired">Sample of desired signal</param>
    /// <param name="error">A priori error: <paramref name="desired"/> minus the returned output</param>
    /// <returns>A priori filter output</returns>
    public float Process(ReadOnlySpan<float> inputs, float desired, out float error)
    {
        var y = Process(inputs, desired);
        error = desired - y;
        return y;
    }

    /// <summary>
    /// Adaptively processes whole buffers.
    /// </summary>
    /// <param name="inputs">Input signals, one array per channel (<see cref="Channels"/> arrays of the same length)</param>
    /// <param name="desired">Desired signal (same length as the input signals)</param>
    /// <param name="output">Receives the filter output; may be empty if it is not needed</param>
    /// <param name="error">Receives the error (desired - output); may be empty if it is not needed</param>
    public void ProcessAll(float[][] inputs, ReadOnlySpan<float> desired, Span<float> output, Span<float> error = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        if (inputs.Length != Channels)
            throw new ArgumentException("Exactly one input signal per channel is expected.", nameof(inputs));
        foreach (var signal in inputs)
        {
            if (signal is null || signal.Length != desired.Length)
                throw new ArgumentException("Every input signal must have the same length as the desired signal.", nameof(inputs));
        }
        if (!output.IsEmpty && output.Length < desired.Length)
            throw new ArgumentException("Output buffer is too short.", nameof(output));
        if (!error.IsEmpty && error.Length < desired.Length)
            throw new ArgumentException("Error buffer is too short.", nameof(error));

        Span<float> frame = Channels <= 64 ? stackalloc float[Channels] : new float[Channels];
        for (var i = 0; i < desired.Length; i++)
        {
            for (var c = 0; c < Channels; c++)
            {
                frame[c] = inputs[c][i];
            }
            var y = Process(frame, desired[i]);
            var e = desired[i] - y;     // before the output is written: the buffers may overlap
            if (!output.IsEmpty) output[i] = y;
            if (!error.IsEmpty) error[i] = e;
        }
    }
}
