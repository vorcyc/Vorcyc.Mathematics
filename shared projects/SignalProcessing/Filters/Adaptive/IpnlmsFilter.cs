namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the IPNLMS Adaptive filter (Improved Proportionate Normalized LMS, Benesty and Gay 2002):
/// the step size of every tap is a mix of an equal share (as in <see cref="NlmsFilter"/>) and a share proportional to the
/// magnitude of its weight (as in <see cref="PnlmsFilter"/>), which makes it fast for sparse and robust for dispersive impulse responses.
/// </summary>
public class IpnlmsFilter : AdaptiveFilter
{
    private const float L1Epsilon = 1e-6f;

    private readonly float _mu;
    private readonly float _alpha;
    private readonly float _eps;
    private readonly float[] _gains;

    /// <summary>
    /// Constructs <see cref="IpnlmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size, stable for 0 &lt; mu &lt; 2 regardless of the input scale</param>
    /// <param name="alpha">Proportionality in [-1, 1): -1 gives <see cref="NlmsFilter"/>, values near 1 give a PNLMS-like behavior, 0 is a good compromise (at 1 every gain would be proportional to the weight, which is zero at the start, so the filter could never move)</param>
    /// <param name="eps">Epsilon, regularizes the normalization; keep it small compared to the expected power of the input per tap</param>
    public IpnlmsFilter(int order, float mu = 0.5f, float alpha = 0, float eps = 1e-3f) : base(order)
    {
        AdaptiveArgument.Positive(mu, nameof(mu));
        if (!(alpha >= -1f && alpha < 1f))
            throw new ArgumentOutOfRangeException(nameof(alpha), "alpha must be in [-1, 1).");
        AdaptiveArgument.NonNegative(eps, nameof(eps));

        _mu = mu;
        _alpha = alpha;
        _eps = eps;
        _gains = new float[_kernelSize];
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
        var n = x.Length;

        var l1 = 0f;
        for (var i = 0; i < n; i++)
        {
            l1 += Math.Abs(_b[i]);
        }

        var equal = (1 - _alpha) / (2 * n);
        var proportional = (1 + _alpha) / (2 * l1 + L1Epsilon);
        var power = 0f;
        for (var i = 0; i < n; i++)
        {
            _gains[i] = equal + proportional * Math.Abs(_b[i]);
            power += _gains[i] * x[i] * x[i];
        }

        // with eps = 0 a silent regressor would give 0 / 0
        var denominator = _eps + power;
        if (denominator > 0)
        {
            var step = _mu * e / denominator;
            for (var i = 0; i < n; i++)
            {
                SetWeight(i, _b[i] + step * _gains[i] * x[i]);
            }
        }

        return y;
    }
}
