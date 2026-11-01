namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the normalized sign-sign LMS Adaptive filter: w += mu / (eps + ‖x‖²) · sign(e) · sign(x).
/// It is the sign-sign LMS (<see cref="SignLmsFilter"/>) with a step size that is divided by the power of the regressor,
/// as defined in the padasip library. The sign-sign update itself does not depend on the scale of the signals,
/// the division does: the step per tap is about <c>mu / (order · input power)</c>, so a large input slows the filter down and a
/// very small input (below <c>sqrt(eps / order)</c>) makes the steps large, therefore choose <c>eps</c> small compared to the
/// power of the input over <c>order</c> samples. See <see cref="NlmsFilter"/> for a normalized filter that is independent of the input scale.
/// </summary>
public class NssLmsFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _eps;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="NssLmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size: the weights move by up to <paramref name="mu"/> / (<paramref name="eps"/> + ‖x‖²) per sample, so it has to be small compared to the weight magnitudes times the power of the regressor</param>
    /// <param name="eps">Epsilon, regularizes the normalization (added to the regressor power)</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public NssLmsFilter(int order, float mu = 0.1f, float eps = 1e-3f, float leakage = 0) : base(order)
    {
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.NonNegative(eps, nameof(eps));
        AdaptiveArgument.Leakage(leakage, mu);

        _mu = mu;
        _eps = eps;
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

        var power = 0f;
        for (var i = 0; i < x.Length; i++)
        {
            power += x[i] * x[i];
        }

        // With eps = 0 a silent regressor would give 0 / 0, and a regressor of a tiny power (below about 1e-39) a step that overflows single precision
        // (the weights would become infinite): such a sample is skipped.
        var denominator = _eps + power;
        var step = _mu / (double)denominator;
        if (denominator > 0 && step < float.MaxValue / 4)
        {
            var decay = 1 - _leakage * _mu;
            var signedStep = (float)(step * Sign(e));

            for (var i = 0; i < x.Length; i++)
            {
                SetWeight(i, decay * _b[i] + signedStep * Sign(x[i]));
            }
        }

        return y;
    }
}
