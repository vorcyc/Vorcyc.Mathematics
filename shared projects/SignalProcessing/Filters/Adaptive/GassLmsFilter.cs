namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the gradient adaptive step-size LMS Adaptive filter: the step size is adapted by a gradient descent
/// on the squared error, mu(n) = mu(n-1) + rho · e(n) · x(n)T psi(n), where psi(n) tracks the derivative of the weights
/// with respect to the step size (see <see cref="GassVariant"/>); the new step size is used for the weight update of the same
/// sample, w(n+1) = w(n) + mu(n) · e(n) · x(n). The step size is kept within [muMin, muMax].
/// </summary>
public class GassLmsFilter : AdaptiveFilter
{
    private readonly GassVariant _variant;
    private readonly float _muInitial;
    private readonly float _rho;
    private readonly float _alpha;
    private readonly float _muMin;
    private readonly float _muMax;
    private readonly float _leakage;
    private readonly float[] _psi;
    private float _mu;

    /// <summary>
    /// Constructs <see cref="GassLmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="variant">Way of tracking the sensitivity of the weights to the step size</param>
    /// <param name="mu">Initial step size; <paramref name="muMax"/> / 2 if omitted</param>
    /// <param name="rho">Step of the step-size adaptation (meta step); 0.01 / <paramref name="order"/>² (unit input power) if omitted</param>
    /// <param name="alpha">Forgetting factor of the sensitivity vector in [0, 1), used by <see cref="GassVariant.Ang"/> only</param>
    /// <param name="muMin">Lower bound of the step size; <paramref name="muMax"/> / 10 if omitted</param>
    /// <param name="muMax">Upper bound of the step size; it has to be small compared to 2 / (<paramref name="order"/> · input power), otherwise the filter diverges. 0.1 / <paramref name="order"/> (unit input power) if omitted</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="muMax"/> must not exceed 1</param>
    public GassLmsFilter(int order, GassVariant variant = GassVariant.Benveniste, float? mu = null, float? rho = null, float alpha = 0.9f, float? muMin = null, float? muMax = null, float leakage = 0) : base(order)
    {
        var max = muMax ?? 0.1f / order;
        var min = muMin ?? max / 10;

        AdaptiveArgument.Positive(max, nameof(muMax));
        AdaptiveArgument.Positive(min, nameof(muMin));
        if (!(alpha >= 0f && alpha < 1f))
            throw new ArgumentOutOfRangeException(nameof(alpha), "alpha must be in [0, 1).");
        if (mu is { } initial)
            AdaptiveArgument.Finite(initial, nameof(mu));
        if (rho is { } metaStep)
            AdaptiveArgument.Finite(metaStep, nameof(rho));
        Guard.AgainstExceedance(min, max, nameof(muMin), nameof(muMax));
        AdaptiveArgument.Leakage(leakage, max);

        _variant = variant;
        _muInitial = Math.Clamp(mu ?? max / 2, min, max);
        _rho = rho ?? 0.01f / ((float)order * order);
        _alpha = alpha;
        _muMin = min;
        _muMax = max;
        _leakage = leakage;
        _psi = new float[_kernelSize];
        _mu = _muInitial;
    }

    /// <summary>
    /// Gets the current step size.
    /// </summary>
    public float Mu => _mu;

    /// <summary>
    /// Restores the initial step size and clears the sensitivity vector.
    /// </summary>
    protected override void ResetState()
    {
        _mu = _muInitial;
        Array.Clear(_psi);
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

        // x(n)T psi(n): the gradient of the squared error with respect to the step size is -e · this
        var xPsi = 0f;
        for (var i = 0; i < x.Length; i++)
        {
            xPsi += x[i] * _psi[i];
        }

        // the step size of this sample already contains its own error (Mathews and Xie)
        _mu = Math.Clamp(_mu + _rho * e * xPsi, _muMin, _muMax);

        var decay = 1 - _leakage * _mu;
        var step = _mu * e;

        for (var i = 0; i < x.Length; i++)
        {
            SetWeight(i, decay * _b[i] + step * x[i]);

            _psi[i] = _variant switch
            {
                GassVariant.Benveniste => _psi[i] - _mu * x[i] * xPsi + e * x[i],
                GassVariant.Ang => _alpha * _psi[i] + e * x[i],
                _ => e * x[i],
            };
        }

        return y;
    }
}
