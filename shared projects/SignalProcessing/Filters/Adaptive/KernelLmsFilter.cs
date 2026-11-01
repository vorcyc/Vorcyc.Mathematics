namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the quantized kernel LMS (QKLMS) Adaptive filter (Chen et al. 2012), a nonlinear adaptive filter that learns an arbitrary smooth
/// mapping from the newest <see cref="Order"/> input samples u(n) = [x(n), ..., x(n−order+1)] to the desired signal:
/// y(n) = Σ<sub>i</sub> α<sub>i</sub> · exp(−‖u(n) − c<sub>i</sub>‖² / (2σ²)), a growing sum of Gaussian kernels centered at the stored input vectors c<sub>i</sub>.
/// </summary>
/// <remarks>
/// <para>
/// The error e = d − y is learned by the LMS rule in the feature space of the kernel. The new input vector becomes a new center with the coefficient μ·e,
/// unless an existing center is closer than the quantization size: then the coefficient of the nearest center is updated instead. This keeps the dictionary small
/// (a quantization size of 0 gives the plain KLMS, which stores every input). When <see cref="MaxDictionarySize"/> is reached the dictionary stops growing and only the nearest center is updated.
/// </para>
/// <para>
/// The width σ of the kernel and the quantization size are in the units of the input signal (the distance is Euclidean in the space of the input vectors),
/// so the signal should be scaled to a level around 1. Every sample costs O(<see cref="DictionarySize"/> · <see cref="Order"/>).
/// With one-step prediction the desired sample is the next sample of the input: <c>Process(x[n], x[n + 1])</c>.
/// </para>
/// </remarks>
public sealed class KernelLmsFilter
{
    private readonly float _mu;
    private readonly double _inverseTwoSigmaSquared;
    private readonly double _quantizationSquared;
    private readonly float[] _delayLine;
    private int _offset;

    private float[] _centers = [];
    private float[] _coefficients = [];
    private int _count;

    /// <summary>
    /// Constructs <see cref="KernelLmsFilter"/>.
    /// </summary>
    /// <param name="order">Number of the newest input samples that form the input vector (embedding dimension)</param>
    /// <param name="mu">Step size, usually 0.1 … 1</param>
    /// <param name="kernelWidth">Width σ of the Gaussian kernel: the smaller the width the more detail the filter can learn and the slower it generalizes</param>
    /// <param name="quantizationSize">Distance below which an input vector is merged with its nearest center instead of creating a new one (typically 0.1σ … σ)</param>
    /// <param name="maxDictionarySize">Maximum number of the centers</param>
    public KernelLmsFilter(int order, float mu = 0.5f, float kernelWidth = 1f, float quantizationSize = 0.1f, int maxDictionarySize = 2000)
    {
        Guard.AgainstNonPositive(order, nameof(order));
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.Positive(kernelWidth, nameof(kernelWidth));
        AdaptiveArgument.NonNegative(quantizationSize, nameof(quantizationSize));
        Guard.AgainstNonPositive(maxDictionarySize, nameof(maxDictionarySize));
        var delayLineLength = AdaptiveArgument.Count(2L * order, nameof(order));
        AdaptiveArgument.Count((long)maxDictionarySize * order, nameof(maxDictionarySize));     // the centers of a full dictionary are stored in one array

        Order = order;
        MaxDictionarySize = maxDictionarySize;
        _mu = mu;
        _inverseTwoSigmaSquared = 1.0 / (2.0 * kernelWidth * kernelWidth);
        _quantizationSquared = (double)quantizationSize * quantizationSize;
        _delayLine = new float[delayLineLength];
    }

    /// <summary>
    /// Gets the number of the newest input samples that form the input vector.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets the maximum number of the centers.
    /// </summary>
    public int MaxDictionarySize { get; }

    /// <summary>
    /// Gets the current number of the centers.
    /// </summary>
    public int DictionarySize => _count;

    /// <summary>
    /// Restarts the signal history. The learned dictionary is preserved, use <see cref="ClearDictionary"/> to forget it.
    /// </summary>
    public void Reset()
    {
        _offset = 0;
        Array.Clear(_delayLine);
    }

    /// <summary>
    /// Removes all centers: forgets everything that has been learned. The signal history is kept.
    /// </summary>
    public void ClearDictionary()
    {
        _count = 0;
    }

    private int Push(float input)
    {
        _offset = _offset == 0 ? Order - 1 : _offset - 1;
        _delayLine[_offset] = input;
        _delayLine[_offset + Order] = input;
        return _offset;
    }

    /// <summary>
    /// Calculates the output for the input vector that starts at <paramref name="offset"/> and finds the nearest center.
    /// </summary>
    private double Evaluate(int offset, out int nearest, out double nearestDistance)
    {
        var u = new ReadOnlySpan<float>(_delayLine, offset, Order);

        double y = 0;
        nearest = -1;
        nearestDistance = double.PositiveInfinity;

        for (var i = 0; i < _count; i++)
        {
            var c = new ReadOnlySpan<float>(_centers, i * Order, Order);
            double distance = 0;
            for (var k = 0; k < c.Length; k++)
            {
                double d = u[k] - c[k];
                distance += d * d;
            }

            y += _coefficients[i] * Math.Exp(-distance * _inverseTwoSigmaSquared);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = i;
            }
        }
        return y;
    }

    /// <summary>
    /// Processes one sample with the current dictionary and without adapting it.
    /// </summary>
    /// <param name="input">Input sample</param>
    public float Process(float input) => (float)Evaluate(Push(input), out _, out _);

    /// <summary>
    /// Processes one sample of input and desired signals and adapts the dictionary.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    /// <returns>A priori filter output</returns>
    public float Process(float input, float desired)
    {
        var offset = Push(input);
        var y = Evaluate(offset, out var nearest, out var nearestDistance);
        var step = (float)(_mu * (desired - y));

        // a non-finite sample (or the memory of one in the input vector) must neither become a center nor poison a coefficient
        if (!float.IsFinite(step) || !RegressorIsFinite(offset))
        {
            return (float)y;
        }

        if ((nearest < 0 || nearestDistance > _quantizationSquared) && _count < MaxDictionarySize)
        {
            if ((_count + 1) * Order > _centers.Length)
            {
                var capacity = Math.Min(MaxDictionarySize, Math.Max(16, 2 * _count));
                Array.Resize(ref _centers, capacity * Order);
                Array.Resize(ref _coefficients, capacity);
            }

            Array.Copy(_delayLine, offset, _centers, _count * Order, Order);
            _coefficients[_count++] = step;
        }
        else if (nearest >= 0)
        {
            _coefficients[nearest] += step;
        }

        return (float)y;
    }

    private bool RegressorIsFinite(int offset)
    {
        for (var k = 0; k < Order; k++)
        {
            if (!float.IsFinite(_delayLine[offset + k]))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Processes one sample of input and desired signals, adapts the dictionary and returns the error as well.
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
    /// Adaptively processes whole buffers.
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
}
