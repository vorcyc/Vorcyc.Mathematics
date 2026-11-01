namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents Sign-Error LMS Adaptive filter (Sign-Error Least-Mean-Squares algorithm): w += mu · sign(e) · x.
/// The update does not depend on the magnitude of the error, which makes the filter robust against
/// impulsive noise in the desired signal.
/// </summary>
public class SignErrorLmsFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="SignErrorLmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size: every weight changes by up to mu · |x| per sample, so it has to be small compared to the weight magnitudes divided by the input amplitude</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public SignErrorLmsFilter(int order, float mu = 0.001f, float leakage = 0) : base(order)
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
        var step = _mu * Sign(e);

        for (var i = 0; i < x.Length; i++)
        {
            SetWeight(i, decay * _b[i] + step * x[i]);
        }

        return y;
    }
}
