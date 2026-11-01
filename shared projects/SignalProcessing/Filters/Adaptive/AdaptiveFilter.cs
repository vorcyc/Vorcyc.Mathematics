namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// Abstract class for adaptive filters.
/// </summary>
/// <remarks>
/// <para>
/// Numeric arguments (step sizes, regularization, leakage, forgetting factors, the weights of <see cref="Init(float[])"/>) must be finite: NaN and infinity are rejected with an
/// <see cref="ArgumentException"/> (<see cref="ArgumentOutOfRangeException"/> for the scalars of the constructors), as is a size that the filter could not allocate (for example an order whose
/// <c>order · order</c> matrix has more elements than an array can hold).
/// </para>
/// <para>
/// <b>Non-finite samples.</b> A sample of the input or of the desired signal that is NaN or infinite does not adapt the filter and never poisons the weights or the state of the algorithm;
/// the filter goes on as soon as the sample has left it. While such an input sample is inside the delay line the output of the filter is not finite
/// (the output is the dot product with the regressor), and so is the error; the filter itself is not affected.
/// </para>
/// </remarks>
public abstract class AdaptiveFilter : FirFilter
{
    /// <summary>
    /// Constructs <see cref="AdaptiveFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="order"/> is not positive or too large (the kernel and the delay line hold <c>2 · order</c> values).</exception>
    public AdaptiveFilter(int order) : base(new float[ValidateOrder(order)])
    {
        Array.Resize(ref _delayLine, _kernelSize * 2);  // trick for better performance
    }

    private static int ValidateOrder(int order)
    {
        Guard.AgainstNonPositive(order, nameof(order));
        AdaptiveArgument.Count(2L * order, nameof(order));
        return order;
    }

    /// <summary>
    /// Gets the filter order (number of adaptive taps).
    /// </summary>
    public int Order => _kernelSize;

    /// <summary>
    /// Gets the number of samples that have gone through the delay line, adapted or not (the signal history that <see cref="Reset"/> clears does not reset it).
    /// An algorithm that needs the regressor of the previous adapted sample to be the predecessor of the current one compares it with the count of its previous step.
    /// </summary>
    protected long SampleCount { get; private set; }

    /// <summary>
    /// Inits weights of adaptive filter. The adaptation state of the algorithm
    /// (e.g. inverse correlation matrix, running step sizes) is restarted as well; the signal history is kept.
    /// </summary>
    /// <param name="weights">Weights (filter kernel), <see cref="Order"/> finite values</param>
    /// <exception cref="ArgumentException"><paramref name="weights"/> does not have <see cref="Order"/> elements or holds NaN or infinity; the filter is not changed then.</exception>
    public void Init(float[] weights)
    {
        Guard.AgainstInequality(_kernelSize, weights.Length, "Filter order", "Weights array size");
        AdaptiveArgument.AllFinite(weights, nameof(weights));
        ChangeKernel(weights);
        ResetState();
    }

    /// <summary>
    /// Restarts the signal history: clears the whole (duplicated) delay line and the history kept by the algorithm,
    /// and restarts the adaptation state. Current weights are preserved; use <see cref="Init(float[])"/> to restart them as well.
    /// </summary>
    public override void Reset()
    {
        _delayLineOffset = _kernelSize - 1;
        Array.Clear(_delayLine, 0, _delayLine.Length);
        ResetHistory();
        ResetState();
    }

    /// <summary>
    /// Restores the algorithm-specific adaptation state (e.g. inverse correlation matrix, running step sizes) to its initial value.
    /// Called by <see cref="Reset"/> and <see cref="Init(float[])"/>; the current weights are already stored in the kernel when it is called.
    /// </summary>
    protected virtual void ResetState()
    {
    }

    /// <summary>
    /// Clears the signal history that the algorithm keeps beside the delay line. Called by <see cref="Reset"/> only:
    /// <see cref="Init(float[])"/> keeps the signal history, which has to stay consistent with the delay line.
    /// </summary>
    protected virtual void ResetHistory()
    {
    }

    /// <summary>
    /// Processes one sample with the current weights and without adapting them.
    /// Keeps the duplicated half of the delay line in sync, so that adaptation can be resumed afterwards.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        SampleCount++;
        _delayLine[_delayLineOffset + _kernelSize] = sample;   // keep the duplicate in sync
        return base.Process(sample);
    }

    /// <summary>
    /// Filters all <paramref name="samples"/> with the current weights without adapting them.
    /// The samples go through <see cref="Process(float)"/>, so the history kept by the algorithm stays in sync with the delay line.
    /// As for any <see cref="FirFilter"/> the filter is flushed with <c>order - 1</c> zeros (the result is that much longer than <paramref name="samples"/>),
    /// so the delay line holds zeros afterwards; to continue a stream filter it sample by sample with <see cref="Process(float)"/>.
    /// </summary>
    /// <param name="samples">Samples</param>
    public override float[] ProcessAllSamples(float[] samples) => ProcessAllSamples(samples.AsSpan());

    /// <summary>
    /// Filters all <paramref name="samples"/> with the current weights without adapting them.
    /// The samples go through <see cref="Process(float)"/>, so the history kept by the algorithm stays in sync with the delay line.
    /// As for any <see cref="FirFilter"/> the filter is flushed with <c>order - 1</c> zeros (the result is that much longer than <paramref name="samples"/>),
    /// so the delay line holds zeros afterwards; to continue a stream filter it sample by sample with <see cref="Process(float)"/>.
    /// </summary>
    /// <param name="samples">Samples</param>
    public override float[] ProcessAllSamples(Span<float> samples)
    {
        var filtered = new float[samples.Length + _kernelSize - 1];
        var k = 0;
        while (k < samples.Length)
        {
            filtered[k] = Process(samples[k]);
            k++;
        }
        while (k < filtered.Length)
        {
            filtered[k++] = Process(0);
        }
        return filtered;
    }

    /// <summary>
    /// Pushes <paramref name="input"/> into the delay line and returns the a priori output y(n) of the current weights.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="offset">Start of the regressor [x(n), x(n-1), ..., x(n-order+1)] inside the delay line, see <see cref="Regressor(int)"/></param>
    protected float Push(float input, out int offset)
    {
        offset = _delayLineOffset;
        return Process(input);
    }

    /// <summary>
    /// Gets the regressor [x(n), x(n-1), ..., x(n-order+1)] as a contiguous span (thanks to the duplicated delay line).
    /// </summary>
    /// <param name="offset">Offset returned by <see cref="Push(float, out int)"/></param>
    protected ReadOnlySpan<float> Regressor(int offset) => new(_delayLine, offset, _kernelSize);

    /// <summary>
    /// Sets weight number <paramref name="index"/> (both halves of the duplicated kernel).
    /// </summary>
    /// <param name="index">Weight index</param>
    /// <param name="value">New value</param>
    protected void SetWeight(int index, float value) => _b[index] = _b[_kernelSize + index] = value;

    /// <summary>
    /// Returns -1, 0 or 1 according to the sign of <paramref name="value"/> (0 for NaN, unlike <see cref="Math.Sign(float)"/> it never throws).
    /// </summary>
    /// <param name="value">Value</param>
    protected static float Sign(float value) => value > 0 ? 1f : value < 0 ? -1f : 0f;

    /// <summary>
    /// Processes one sample of input and desired signals, adapts filter coefficients and returns the error as well.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    /// <param name="error">A priori error: <paramref name="desired"/> minus the returned output</param>
    /// <returns>A priori filter output</returns>
    public float Process(float input, float desired, out float error)
    {
        var y = Process(input, desired);
        error = desired - y;
        return y;
    }

    /// <summary>
    /// Adaptively processes whole buffers (unlike <see cref="ProcessAllSamples(float[])"/>, the weights are adapted).
    /// </summary>
    /// <param name="input">Input signal</param>
    /// <param name="desired">Desired signal (same length as <paramref name="input"/>)</param>
    /// <param name="output">Receives the filter output; may be empty if it is not needed</param>
    /// <param name="error">Receives the error (desired - output); may be empty if it is not needed</param>
    public void ProcessAll(ReadOnlySpan<float> input, ReadOnlySpan<float> desired, Span<float> output, Span<float> error = default)
    {
        if (desired.Length != input.Length)
            throw new ArgumentException("Desired signal must have the same length as the input signal.", nameof(desired));
        if (!output.IsEmpty && output.Length < input.Length)
            throw new ArgumentException("Output buffer is too short.", nameof(output));
        if (!error.IsEmpty && error.Length < input.Length)
            throw new ArgumentException("Error buffer is too short.", nameof(error));

        for (var i = 0; i < input.Length; i++)
        {
            var y = Process(input[i], desired[i]);
            var e = desired[i] - y;     // before the output is written: the buffers may overlap
            if (!output.IsEmpty) output[i] = y;
            if (!error.IsEmpty) error[i] = e;
        }
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts filter coefficients.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public abstract float Process(float input, float desired);
}
