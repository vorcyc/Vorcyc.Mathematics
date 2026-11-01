namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the GNGD Adaptive filter (Generalized Normalized Gradient Descent, Mandic 2004):
/// an NLMS whose regularization term epsilon(n) is itself adapted by a gradient descent,
/// w += mu / (epsilon(n) + ||x||²) · e · x, which keeps the algorithm stable for non-stationary and nonlinear inputs.
/// </summary>
public class GngdFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _epsInitial;
    private readonly float _rho;
    private readonly float _leakage;
    private float _eps;
    private float _prevError;
    private float _prevNorm;
    private bool _hasPrevious;
    private long _previousCount;

    /// <summary>
    /// Constructs <see cref="GngdFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size, stable for 0 &lt; mu &lt; 2 regardless of the input scale</param>
    /// <param name="eps">Initial value of the regularization term epsilon(n)</param>
    /// <param name="rho">Step of the epsilon adaptation</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public GngdFilter(int order, float mu = 0.75f, float eps = 1, float rho = 0.1f, float leakage = 0) : base(order)
    {
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.NonNegative(eps, nameof(eps));
        AdaptiveArgument.Finite(rho, nameof(rho));
        AdaptiveArgument.Leakage(leakage, mu);

        _mu = mu;
        _epsInitial = eps;
        _rho = rho;
        _leakage = leakage;
        _eps = eps;
    }

    /// <summary>
    /// Gets the current regularization term epsilon(n).
    /// </summary>
    public float Epsilon => _eps;

    /// <summary>
    /// Restores the initial regularization term.
    /// </summary>
    protected override void ResetState() => _eps = _epsInitial;

    /// <summary>
    /// Forgets the previous step.
    /// </summary>
    protected override void ResetHistory()
    {
        _prevError = 0;
        _prevNorm = 0;
        _hasPrevious = false;
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts filter coefficients.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override float Process(float input, float desired)
    {
        var oldest = _delayLine[_delayLineOffset];      // x(n-order): it leaves the regressor with this sample

        var y = Push(input, out var offset);

        var e = desired - y;

        if (!float.IsFinite(e))
        {
            return y;
        }

        var x = Regressor(offset);

        var norm = 0f;
        for (var i = 0; i < x.Length; i++)
        {
            norm += x[i] * x[i];
        }

        // The previous step has to be the one of the sample just before this one (the previous regressor is the current one shifted by a sample):
        // frozen samples (Process(float)) or samples that were skipped in between break the link.
        if (_hasPrevious && _previousCount == SampleCount - 1)
        {
            // x(n)T x(n-1): the previous regressor is the current one shifted by a sample
            var dot = x[^1] * oldest;
            for (var i = 0; i < x.Length - 1; i++)
            {
                dot += x[i] * x[i + 1];
            }

            var previousDenominator = _eps + _prevNorm;
            if (previousDenominator > 0)
            {
                var nextEps = _eps - _rho * _mu * e * _prevError * dot / (previousDenominator * previousDenominator);
                if (float.IsFinite(nextEps))
                {
                    _eps = Math.Max(nextEps, 0f);
                }
            }
        }

        var denominator = _eps + norm;
        if (denominator > 0)
        {
            var decay = 1 - _leakage * _mu;
            var step = _mu * e / denominator;

            for (var i = 0; i < x.Length; i++)
            {
                SetWeight(i, decay * _b[i] + step * x[i]);
            }
        }

        _prevError = e;
        _prevNorm = norm;
        _hasPrevious = true;
        _previousCount = SampleCount;

        return y;
    }
}
