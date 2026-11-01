namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the generalized maximum correntropy criterion (GMCC) Adaptive filter:
/// w += mu · |e|^(alpha-1) · sign(e) · exp(-(|e| / beta)^alpha) · x.
/// Errors much larger than <c>beta</c> (outliers) get a vanishing weight, so the filter is robust against impulsive noise;
/// with <c>alpha = 2</c> it is the classic maximum correntropy criterion (MCC) algorithm.
/// </summary>
public class GmccFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _alpha;
    private readonly float _beta;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="GmccFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size, the same rules as for <see cref="LmsFilter"/> apply</param>
    /// <param name="alpha">Shape of the kernel, at least 1 (2 gives the Gaussian kernel)</param>
    /// <param name="beta">Width of the kernel: errors above it are suppressed</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public GmccFilter(int order, float mu = 0.001f, float alpha = 2, float beta = 1, float leakage = 0) : base(order)
    {
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.Positive(beta, nameof(beta));
        if (!(float.IsFinite(alpha) && alpha >= 1))
            throw new ArgumentOutOfRangeException(nameof(alpha), alpha, "alpha must be a finite number, at least 1.");
        AdaptiveArgument.Leakage(leakage, mu);

        _mu = mu;
        _alpha = alpha;
        _beta = beta;
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

        var magnitude = MathF.Abs(e);
        var gain = magnitude > 0
            ? MathF.Pow(magnitude, _alpha - 1) * MathF.Exp(-MathF.Pow(magnitude / _beta, _alpha))
            : 0f;
        var step = _mu * Sign(e) * gain;

        for (var i = 0; i < x.Length; i++)
        {
            SetWeight(i, decay * _b[i] + step * x[i]);
        }

        return y;
    }
}
