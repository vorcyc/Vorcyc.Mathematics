namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents LMF Adaptive filter (Least-Mean-Fourth algorithm).
/// </summary>
public class LmfFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="LmfFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size (the update is proportional to the cube of the error, so keep it small; use <see cref="NlmfFilter"/> when the input scale is unknown)</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public LmfFilter(int order, float mu = 0.001f, float leakage = 0) : base(order)
    {
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.Leakage(leakage, mu);

        _mu = mu;
        _leakage = leakage;
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts filter coefficients.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override float Process(float input, float desired)
    {
        var offset = _delayLineOffset;

        _delayLine[offset + _kernelSize] = input;   // duplicate it for better loop performance


        var y = Process(input);

        var e = desired - y;

        if (!float.IsFinite(e))
        {
            return y;
        }

        for (var i = 0; i < _kernelSize; i++, offset++)
        {
            _b[i] = _b[_kernelSize + i] = (1 - _leakage * _mu) * _b[i] + 4 * _mu * e * e * e * _delayLine[offset];
        }

        return y;
    }
}
