namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the least-lncosh Adaptive filter: w += mu · tanh(lambda · e) · x, the gradient step of the cost
/// ln(cosh(lambda · e)) / lambda. For small errors it behaves like <see cref="LmsFilter"/> (with the step mu · lambda),
/// for large errors like <see cref="SignErrorLmsFilter"/>, which makes it robust against impulsive noise.
/// </summary>
public class LlncoshFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _lambda;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="LlncoshFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size, the same rules as for <see cref="LmsFilter"/> apply to mu · lambda</param>
    /// <param name="lambda">Scale of the error: errors well above 1 / lambda are treated like outliers</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public LlncoshFilter(int order, float mu = 0.001f, float lambda = 1, float leakage = 0) : base(order)
    {
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.Positive(lambda, nameof(lambda));
        AdaptiveArgument.Leakage(leakage, mu);
        _mu = mu;
        _lambda = lambda;
        _leakage = leakage;
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts filter coefficients.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override float Process(float input, float desired)
    {
        var y = Push(input, out var offset);

        var e = desired - y;

        if (!float.IsFinite(e))
        {
            return y;
        }

        var x = Regressor(offset);
        var decay = 1 - _leakage * _mu;
        var step = _mu * MathF.Tanh(_lambda * e);

        for (var i = 0; i < x.Length; i++)
        {
            SetWeight(i, decay * _b[i] + step * x[i]);
        }

        return y;
    }
}
