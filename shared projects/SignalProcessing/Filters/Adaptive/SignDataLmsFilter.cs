namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents Sign-Data LMS Adaptive filter (Sign-Data Least-Mean-Squares algorithm): w += mu · e · sign(x).
/// The regressor enters the update only through its sign, which removes the multiplications by the input samples.
/// </summary>
public class SignDataLmsFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="SignDataLmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size, it has to be small compared to 2 / (<paramref name="order"/> · mean |x|)</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public SignDataLmsFilter(int order, float mu = 0.001f, float leakage = 0) : base(order)
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
            SetWeight(i, decay * _b[i] + step * Sign(x[i]));
        }

        return y;
    }
}
