using System.Linq;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents NLMS Adaptive filter (Normalized Least-Mean-Squares algorithm + Epsilon).
/// </summary>
public class NlmsFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _eps;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="NlmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size, stable for 0 &lt; mu &lt; 2 regardless of the input scale</param>
    /// <param name="eps">Epsilon, regularizes the normalization (added to the regressor power); keep it small compared to the expected power of the input over <paramref name="order"/> samples</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public NlmsFilter(int order, float mu = 0.75f, float eps = 1, float leakage = 0) : base(order)
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
        var offset = _delayLineOffset;

        _delayLine[offset + _kernelSize] = input;   // duplicate it for better loop performance


        var y = Process(input);

        var e = desired - y;

        if (!float.IsFinite(e))
        {
            return y;
        }

        // only the regressor must be counted: the second half of the delay line is its duplicate
        var power = 0f;
        for (var i = 0; i < _kernelSize; i++)
        {
            var v = _delayLine[offset + i];
            power += v * v;
        }

        var norm = _eps + power;

        // with eps = 0 a regressor of zeros (leading digital silence) would give 0 / 0
        if (!(norm > 0))
        {
            return y;
        }

        for (var i = 0; i < _kernelSize; i++, offset++)
        {
            _b[i] = _b[_kernelSize + i] = (1 - _leakage * _mu) * _b[i] + _mu * e * _delayLine[offset] / norm;
        }

        return y;
    }
}
