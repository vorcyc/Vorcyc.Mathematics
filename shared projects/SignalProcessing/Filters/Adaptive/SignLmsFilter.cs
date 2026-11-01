using System;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents Sign-Sign LMS Adaptive filter (Sign Least-Mean-Squares algorithm):
/// every weight moves by ±<c>mu</c> per sample, w += mu · sign(e) · sign(x).
/// See also <see cref="SignErrorLmsFilter"/> and <see cref="SignDataLmsFilter"/>.
/// </summary>
public class SignLmsFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="SignLmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size: absolute change of every weight per sample, so it has to be small compared to the weight magnitudes</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public SignLmsFilter(int order, float mu = 0.001f, float leakage = 0) : base(order)
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
            _b[i] = _b[_kernelSize + i] = (1 - _leakage * _mu) * _b[i] + _mu * Sign(e) * Sign(_delayLine[offset]);
        }

        return y;
    }
}
