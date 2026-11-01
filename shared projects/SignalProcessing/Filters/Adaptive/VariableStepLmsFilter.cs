namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents LMS Adaptive filter (Least-Mean-Squares) with an individual, fixed step size for every tap
/// (w[i] += mu[i] · e · x[i]). The steps do not change over time; for step sizes that adapt to the error
/// see <see cref="VssLmsFilter"/> and <see cref="GassLmsFilter"/>.
/// </summary>
public class VariableStepLmsFilter : AdaptiveFilter
{
    private readonly float[] _mu;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="VariableStepLmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size of every tap (<paramref name="order"/> values, each positive and finite); 0.001 for every tap if omitted. The values are copied</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> times the largest step size must not exceed 1</param>
    public VariableStepLmsFilter(int order, float[]? mu = null, float leakage = 0) : base(order)
    {
        // an own copy: the caller could change the array behind the checks
        _mu = mu is null ? Enumerable.Repeat(0.001f, order).ToArray() : (float[])mu.Clone();
        Guard.AgainstInequality(order, _mu.Length, "Filter order", "Steps array size");

        var largest = 0f;
        foreach (var step in _mu)
        {
            AdaptiveArgument.Positive(step, nameof(mu));
            largest = Math.Max(largest, step);
        }
        AdaptiveArgument.Leakage(leakage, largest);

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
            _b[i] = _b[_kernelSize + i] = (1 - _leakage * _mu[i]) * _b[i] + _mu[i] * e * _delayLine[offset];
        }

        return y;
    }
}
