namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the filtered-x LMS (FxLMS) Adaptive filter, the standard algorithm of active noise control (ANC):
/// the controller output <c>y</c> reaches the error sensor through a secondary path S, so the reference signal has to be filtered by
/// an estimate of S before it is used in the LMS update. Terms: <c>x</c> is the reference (noise pickup), <c>y</c> the controller output
/// that has to be <b>subtracted</b> from the disturbance by the secondary source, and the error <c>e = d - S * y</c> is the residual noise
/// measured by the error sensor.
/// </summary>
/// <remarks>
/// A typical control loop: <c>e[n] = d[n] - (s * y)[n]</c> is measured (it only depends on the outputs of previous samples when the
/// secondary path has a delay of at least one sample), then <c>y[n] = filter.Process(x[n], e[n])</c> is sent to the secondary source.
/// </remarks>
public sealed class FxLmsFilter
{
    private readonly int _order;
    private readonly float _mu;
    private readonly float _leakage;
    private readonly float _eps;
    private readonly bool _normalized;
    private readonly float[] _secondaryPath;
    private readonly float[] _weights;

    /// <summary>
    /// Reference history, newest first (controller order samples).
    /// </summary>
    private readonly float[] _reference;

    /// <summary>
    /// Reference history for the secondary path model, newest first.
    /// </summary>
    private readonly float[] _secondaryInput;

    /// <summary>
    /// Filtered reference history, newest first (controller order samples).
    /// </summary>
    private readonly float[] _filtered;

    /// <summary>
    /// Constructs <see cref="FxLmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Order (number of taps) of the controller</param>
    /// <param name="secondaryPath">Impulse response of the estimate of the secondary path (controller output to error sensor)</param>
    /// <param name="mu">Step size; for the normalized update stable for 0 &lt; mu &lt; 2 with an accurate secondary path model, in practice well below that (a secondary path with a delay needs a smaller step)</param>
    /// <param name="normalized">Normalizes the step by the power of the filtered reference (FxNLMS)</param>
    /// <param name="eps">Regularization of the normalization</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public FxLmsFilter(int order, ReadOnlySpan<float> secondaryPath, float mu = 0.1f, bool normalized = true, float eps = 1e-3f, float leakage = 0)
    {
        Guard.AgainstNonPositive(order, nameof(order));
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.NonNegative(eps, nameof(eps));
        AdaptiveArgument.Leakage(leakage, mu);
        if (secondaryPath.IsEmpty)
            throw new ArgumentException("The secondary path estimate must not be empty.", nameof(secondaryPath));
        foreach (var tap in secondaryPath)
        {
            if (!float.IsFinite(tap))
                throw new ArgumentException("The secondary path estimate must be finite.", nameof(secondaryPath));
        }

        _order = order;
        _mu = mu;
        _normalized = normalized;
        _eps = eps;
        _leakage = leakage;
        _secondaryPath = secondaryPath.ToArray();
        _weights = new float[order];
        _reference = new float[order];
        _secondaryInput = new float[secondaryPath.Length];
        _filtered = new float[order];
    }

    /// <summary>
    /// Gets the order (number of taps) of the controller.
    /// </summary>
    public int Order => _order;

    /// <summary>
    /// Gets a copy of the weights of the controller.
    /// </summary>
    public float[] Kernel => (float[])_weights.Clone();

    /// <summary>
    /// Inits the weights of the controller.
    /// </summary>
    /// <param name="weights">Weights, <see cref="Order"/> finite values</param>
    /// <exception cref="ArgumentException"><paramref name="weights"/> does not have <see cref="Order"/> elements or holds NaN or infinity; the filter is not changed then.</exception>
    public void Init(ReadOnlySpan<float> weights)
    {
        if (weights.Length != _order)
            throw new ArgumentException("Weights array size must be equal to the filter order.", nameof(weights));
        AdaptiveArgument.AllFinite(weights, nameof(weights));

        weights.CopyTo(_weights);
    }

    /// <summary>
    /// Clears the signal history; the weights are preserved.
    /// </summary>
    public void Reset()
    {
        Array.Clear(_reference);
        Array.Clear(_secondaryInput);
        Array.Clear(_filtered);
    }

    /// <summary>
    /// Processes one sample: adapts the weights with the error sensor sample and returns the controller output for the new reference sample.
    /// </summary>
    /// <param name="reference">Sample of the reference signal x(n)</param>
    /// <param name="error">Sample of the error sensor signal e(n) = d(n) - (S * y)(n)</param>
    /// <returns>Controller output y(n) that has to be subtracted from the disturbance by the secondary source</returns>
    public float Process(float reference, float error)
    {
        Array.Copy(_reference, 0, _reference, 1, _order - 1);
        _reference[0] = reference;

        Array.Copy(_secondaryInput, 0, _secondaryInput, 1, _secondaryInput.Length - 1);
        _secondaryInput[0] = reference;

        var filteredSample = 0f;
        for (var j = 0; j < _secondaryPath.Length; j++)
        {
            filteredSample += _secondaryPath[j] * _secondaryInput[j];
        }

        Array.Copy(_filtered, 0, _filtered, 1, _order - 1);
        _filtered[0] = filteredSample;

        var step = _mu * error;

        var power = 0f;
        for (var i = 0; i < _order; i++)
        {
            power += _filtered[i] * _filtered[i];
        }

        // A non-finite error, or a non-finite sample of the reference that is still inside the filtered reference, must not reach the weights
        // (0 · NaN is NaN as well): the weights are left as they are, the output of the controller is calculated all the same.
        var adapt = float.IsFinite(step) && float.IsFinite(power);

        if (_normalized)
        {
            // with eps = 0 a silent filtered reference would give 0 / 0
            var denominator = _eps + power;
            step = denominator > 0 ? step / denominator : 0;
        }

        var decay = 1 - _leakage * _mu;
        var y = 0f;
        for (var i = 0; i < _order; i++)
        {
            if (adapt)
            {
                _weights[i] = decay * _weights[i] + step * _filtered[i];
            }
            y += _weights[i] * _reference[i];
        }

        return y;
    }
}
