namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the variable step-size LMS Adaptive filter of Kwong and Johnston: the step size follows
/// the instantaneous error power, mu(n+1) = alpha · mu(n) + gamma · e(n)², and is kept within [muMin, muMax].
/// A large error gives a large step (fast convergence), a small error gives a small step (low misadjustment).
/// </summary>
public class VssLmsFilter : AdaptiveFilter
{
    private readonly float _muInitial;
    private readonly float _alpha;
    private readonly float _gamma;
    private readonly float _muMin;
    private readonly float _muMax;
    private readonly float _leakage;
    private float _mu;

    /// <summary>
    /// Constructs <see cref="VssLmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Initial step size; <paramref name="muMax"/> / 2 if omitted</param>
    /// <param name="alpha">Forgetting factor of the step size, in (0, 1]</param>
    /// <param name="gamma">Gain of the error power in the step size recursion</param>
    /// <param name="muMin">Lower bound of the step size; <paramref name="muMax"/> / 10 if omitted</param>
    /// <param name="muMax">Upper bound of the step size; it has to be small compared to 2 / (<paramref name="order"/> · input power), otherwise the filter diverges. 0.1 / <paramref name="order"/> (unit input power) if omitted</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="muMax"/> must not exceed 1</param>
    public VssLmsFilter(int order, float? mu = null, float alpha = 0.97f, float gamma = 4.8e-4f, float? muMin = null, float? muMax = null, float leakage = 0) : base(order)
    {
        var max = muMax ?? 0.1f / order;
        var min = muMin ?? max / 10;

        AdaptiveArgument.Positive(max, nameof(muMax));
        AdaptiveArgument.Positive(min, nameof(muMin));
        AdaptiveArgument.Positive(alpha, nameof(alpha));
        AdaptiveArgument.InRange(alpha, 0f, 1f, nameof(alpha));
        AdaptiveArgument.Finite(gamma, nameof(gamma));
        if (mu is { } initial)
            AdaptiveArgument.Finite(initial, nameof(mu));
        Guard.AgainstExceedance(min, max, nameof(muMin), nameof(muMax));
        AdaptiveArgument.Leakage(leakage, max);

        _muInitial = Math.Clamp(mu ?? max / 2, min, max);
        _alpha = alpha;
        _gamma = gamma;
        _muMin = min;
        _muMax = max;
        _leakage = leakage;
        _mu = _muInitial;
    }

    /// <summary>
    /// Gets the current step size.
    /// </summary>
    public float Mu => _mu;

    /// <summary>
    /// Restores the initial step size.
    /// </summary>
    protected override void ResetState() => _mu = _muInitial;

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
        var step = _mu * e;

        for (var i = 0; i < x.Length; i++)
        {
            SetWeight(i, decay * _b[i] + step * x[i]);
        }

        _mu = Math.Clamp(_alpha * _mu + _gamma * e * e, _muMin, _muMax);

        return y;
    }
}
