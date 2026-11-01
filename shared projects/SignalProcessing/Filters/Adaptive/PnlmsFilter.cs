namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the PNLMS Adaptive filter (Proportionate Normalized LMS, Duttweiler 2000): every tap gets a step size
/// proportional to the magnitude of its current weight, so the few large taps of a sparse impulse response (e.g. an echo path)
/// converge much faster than with <see cref="NlmsFilter"/>.
/// </summary>
public class PnlmsFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _rho;
    private readonly float _delta;
    private readonly float _eps;
    private readonly float[] _gains;

    /// <summary>
    /// Constructs <see cref="PnlmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size, stable for 0 &lt; mu &lt; 2 regardless of the input scale</param>
    /// <param name="rho">Minimal share of the largest weight that every tap gets, in (0, 1]; 1 turns the algorithm into <see cref="NlmsFilter"/></param>
    /// <param name="delta">Floor of the largest weight, it lets the taps start adapting when all weights are still near zero</param>
    /// <param name="eps">Epsilon, regularizes the normalization; keep it small compared to the expected power of the input over <paramref name="order"/> samples</param>
    public PnlmsFilter(int order, float mu = 0.5f, float rho = 0.01f, float delta = 0.01f, float eps = 1e-3f) : base(order)
    {
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.Positive(rho, nameof(rho));
        AdaptiveArgument.InRange(rho, 0f, 1f, nameof(rho));
        AdaptiveArgument.Positive(delta, nameof(delta));
        AdaptiveArgument.NonNegative(eps, nameof(eps));

        _mu = mu;
        _rho = rho;
        _delta = delta;
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

        var largest = _delta;
        for (var i = 0; i < n; i++)
        {
            largest = Math.Max(largest, Math.Abs(_b[i]));
        }

        var floor = _rho * largest;
        var sum = 0f;
        for (var i = 0; i < n; i++)
        {
            var g = Math.Max(floor, Math.Abs(_b[i]));
            _gains[i] = g;
            sum += g;
        }

        // gains are scaled to have the mean 1: equal weights give exactly NLMS
        var scale = n / sum;
        var power = 0f;
        for (var i = 0; i < n; i++)
        {
            _gains[i] *= scale;
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
