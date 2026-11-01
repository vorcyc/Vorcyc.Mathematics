namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

using Vorcyc.Mathematics.Numerics;

/// <summary>
/// Abstract class for adaptive filters of complex-valued signals (equalizers of I/Q streams, beamformers, complex baseband processing).
/// The filter is the FIR filter y(n) = Σ h[k]·x(n−k) with complex taps h; the algorithms adapt the taps so that the output follows the desired signal d(n).
/// </summary>
/// <remarks>
/// With this convention the stochastic gradient algorithms move the taps along the conjugate of the regressor, <c>h += μ·e·conj(x)</c>,
/// where e = d − y is the a priori error. For signals with zero imaginary parts the filters reduce to their real-valued counterparts.
/// Numeric arguments (and the weights of <see cref="Init(ReadOnlySpan{ComplexFp32})"/>) must be finite (<see cref="ArgumentException"/> otherwise). A sample of the input or of the desired signal with a non-finite part does not adapt
/// the taps or the state of the algorithm; while such an input sample is inside the delay line the output is not finite, the taps are not affected.
/// </remarks>
public abstract class ComplexAdaptiveFilter
{
    private readonly ComplexFp32[] _delayLine;
    private int _offset;

    /// <summary>
    /// Taps h[0], h[1], ... of the filter.
    /// </summary>
    protected readonly ComplexFp32[] _h;

    /// <summary>
    /// Constructs <see cref="ComplexAdaptiveFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order (number of complex taps)</param>
    protected ComplexAdaptiveFilter(int order)
    {
        Guard.AgainstNonPositive(order, nameof(order));
        var delayLineLength = AdaptiveArgument.Count(2L * order, nameof(order));

        Order = order;
        _h = new ComplexFp32[order];
        _delayLine = new ComplexFp32[delayLineLength];   // every sample is stored twice, so that the regressor is always contiguous
    }

    /// <summary>
    /// Gets the filter order (number of complex taps).
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets a copy of the current taps h[0], h[1], ... of the filter.
    /// </summary>
    public ComplexFp32[] Weights => (ComplexFp32[])_h.Clone();

    /// <summary>
    /// Inits the taps of the filter. The adaptation state of the algorithm (e.g. the inverse correlation matrix) is restarted as well; the signal history is kept.
    /// </summary>
    /// <param name="weights">Taps (<see cref="Order"/> values with finite parts)</param>
    /// <exception cref="ArgumentException"><paramref name="weights"/> does not have <see cref="Order"/> elements or holds NaN or infinity; the filter is not changed then.</exception>
    public void Init(ReadOnlySpan<ComplexFp32> weights)
    {
        if (weights.Length != Order)
            throw new ArgumentException("The number of weights must be equal to the filter order.", nameof(weights));
        AdaptiveArgument.AllFinite(weights, nameof(weights));

        weights.CopyTo(_h);
        ResetState();
    }

    /// <summary>
    /// Restarts the signal history and the adaptation state. Current taps are preserved; use <see cref="Init(ReadOnlySpan{ComplexFp32})"/> to restart them as well.
    /// </summary>
    public void Reset()
    {
        _offset = 0;
        Array.Clear(_delayLine);
        ResetState();
    }

    /// <summary>
    /// Restores the algorithm-specific adaptation state (e.g. the inverse correlation matrix) to its initial value.
    /// </summary>
    protected virtual void ResetState()
    {
    }

    /// <summary>
    /// Pushes <paramref name="input"/> into the delay line.
    /// </summary>
    /// <param name="input">Input sample</param>
    /// <returns>Start of the regressor [x(n), x(n-1), ..., x(n-order+1)] inside the delay line, see <see cref="Regressor(int)"/></returns>
    protected int Push(ComplexFp32 input)
    {
        _offset = _offset == 0 ? Order - 1 : _offset - 1;
        _delayLine[_offset] = input;
        _delayLine[_offset + Order] = input;
        return _offset;
    }

    /// <summary>
    /// Gets the regressor [x(n), x(n-1), ..., x(n-order+1)] as a contiguous span.
    /// </summary>
    /// <param name="offset">Offset returned by <see cref="Push(ComplexFp32)"/></param>
    protected ReadOnlySpan<ComplexFp32> Regressor(int offset) => new(_delayLine, offset, Order);

    /// <summary>
    /// Calculates the output of the current taps for the regressor that starts at <paramref name="offset"/> (the sum is accumulated in double precision).
    /// </summary>
    /// <param name="offset">Offset returned by <see cref="Push(ComplexFp32)"/></param>
    protected ComplexFp32 Output(int offset)
    {
        double re = 0, im = 0;
        for (var k = 0; k < _h.Length; k++)
        {
            var x = _delayLine[offset + k];
            var h = _h[k];
            re += (double)h.Real * x.Real - (double)h.Imaginary * x.Imaginary;
            im += (double)h.Real * x.Imaginary + (double)h.Imaginary * x.Real;
        }
        return new ComplexFp32((float)re, (float)im);
    }

    /// <summary>
    /// Processes one sample with the current taps and without adapting them.
    /// </summary>
    /// <param name="input">Input sample</param>
    public ComplexFp32 Process(ComplexFp32 input) => Output(Push(input));

    /// <summary>
    /// Processes one sample of input and desired signals and adapts the taps.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    /// <returns>A priori filter output</returns>
    public abstract ComplexFp32 Process(ComplexFp32 input, ComplexFp32 desired);

    /// <summary>
    /// Processes one sample of input and desired signals, adapts the taps and returns the error as well.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    /// <param name="error">A priori error: <paramref name="desired"/> minus the returned output</param>
    /// <returns>A priori filter output</returns>
    public ComplexFp32 Process(ComplexFp32 input, ComplexFp32 desired, out ComplexFp32 error)
    {
        var y = Process(input, desired);
        error = new ComplexFp32(desired.Real - y.Real, desired.Imaginary - y.Imaginary);
        return y;
    }

    /// <summary>
    /// Adaptively processes whole buffers.
    /// </summary>
    /// <param name="input">Input signal</param>
    /// <param name="desired">Desired signal (same length as <paramref name="input"/>)</param>
    /// <param name="output">Receives the filter output; may be empty if it is not needed</param>
    /// <param name="error">Receives the error (desired - output); may be empty if it is not needed</param>
    public void ProcessAll(ReadOnlySpan<ComplexFp32> input, ReadOnlySpan<ComplexFp32> desired, Span<ComplexFp32> output, Span<ComplexFp32> error = default)
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
            var e = new ComplexFp32(desired[i].Real - y.Real, desired[i].Imaginary - y.Imaginary);     // before the output is written: the buffers may overlap
            if (!output.IsEmpty) output[i] = y;
            if (!error.IsEmpty) error[i] = e;
        }
    }
}
