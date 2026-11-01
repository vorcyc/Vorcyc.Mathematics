namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the adaptive IIR notch filter (Nehorai 1985) that tracks the frequency of a sinusoid: the constrained second-order section
/// H(z) = (1 + a z<sup>-1</sup> + z<sup>-2</sup>) / (1 + r a z<sup>-1</sup> + r² z<sup>-2</sup>), a = −2 cos(2π f),
/// has its zeros on the unit circle at the frequency f and its poles at the radius <c>r</c>. The coefficient <c>a</c> follows a normalized
/// gradient descent of the output power, so the notch moves to the frequency of the strongest sinusoid and removes it while the rest
/// of the spectrum passes with almost unit gain.
/// </summary>
/// <remarks>
/// The pole radius <c>r</c> sets the width of the notch, about 2π(1 − r) in radians per sample at −3 dB: a radius close to 1 gives a narrow notch
/// that removes little more than the sinusoid, but adapts slowly and may miss a sinusoid whose frequency is far from the starting frequency.
/// For a known frequency use <see cref="SinusoidalInterferenceCanceller"/>.
/// </remarks>
public sealed class AdaptiveNotchFilter
{
    private readonly double _radius;
    private readonly double _mu;
    private readonly double _beta;
    private readonly double _initialA;
    private double _a;
    private double _x1, _x2, _y1, _y2, _s1, _s2;
    private double _power;
    private double _powerWeight;
    private bool _started;

    /// <summary>
    /// Constructs <see cref="AdaptiveNotchFilter"/>.
    /// </summary>
    /// <param name="initialFrequency">Starting frequency of the notch in cycles per sample, in (0, 0.5)</param>
    /// <param name="poleRadius">Pole radius in (0, 1): the closer to 1 the narrower the notch</param>
    /// <param name="mu">Normalized step of the frequency adaptation, usually 0.001 … 0.05: larger values track faster and jitter more</param>
    /// <param name="averaging">Forgetting factor of the power estimate used for the normalization, in [0, 1)</param>
    public AdaptiveNotchFilter(double initialFrequency = 0.1, double poleRadius = 0.95, double mu = 0.01, double averaging = 0.99)
    {
        if (!(initialFrequency > 0 && initialFrequency < 0.5))
            throw new ArgumentOutOfRangeException(nameof(initialFrequency), "initialFrequency must be in (0, 0.5).");
        if (!(poleRadius > 0 && poleRadius < 1))
            throw new ArgumentOutOfRangeException(nameof(poleRadius), "poleRadius must be in (0, 1).");
        if (!(mu > 0 && double.IsFinite(mu)))
            throw new ArgumentOutOfRangeException(nameof(mu), mu, "mu must be a positive finite number.");
        if (!(averaging >= 0 && averaging < 1))
            throw new ArgumentOutOfRangeException(nameof(averaging), "averaging must be in [0, 1).");

        _radius = poleRadius;
        _mu = mu;
        _beta = averaging;
        _initialA = -2 * Math.Cos(2 * Math.PI * initialFrequency);
        _a = _initialA;
    }

    /// <summary>
    /// Gets the current frequency of the notch in cycles per sample (multiply by the sampling rate to get Hz).
    /// </summary>
    public double Frequency => Math.Acos(Math.Clamp(-_a / 2, -1, 1)) / (2 * Math.PI);

    /// <summary>
    /// Gets the pole radius.
    /// </summary>
    public double PoleRadius => _radius;

    /// <summary>
    /// Processes one sample and returns the notch-filtered sample.
    /// </summary>
    /// <param name="input">Input sample</param>
    public float Process(float input)
    {
        // a non-finite sample would stay in the recursive state for good: it passes through, the filter goes on as if it had not happened
        if (!float.IsFinite(input))
        {
            return input;
        }

        var r = _radius;

        var y = input + _a * _x1 + _x2 - r * _a * _y1 - r * r * _y2;

        // sensitivity of the output to the coefficient: s = dy / da
        var s = _x1 - r * _y1 - r * _a * _s1 - r * r * _s2;

        // Normalized gradient step of the output power. The very first sample has no history to adapt from (and s is zero there), so the
        // average of s² starts with the second sample; it is divided by the share of the samples it has seen so that its first values are not
        // biased towards zero (which would make the first steps 1 / (1 − averaging) times too large).
        if (_started)
        {
            _power = _beta * _power + (1 - _beta) * s * s;
            _powerWeight = _beta * _powerWeight + (1 - _beta);

            var power = _power / _powerWeight;
            if (power > 1e-300)
            {
                var next = _a - _mu * y * s / power;
                if (double.IsFinite(next))
                {
                    _a = Math.Clamp(next, -2, 2);
                }
            }
        }
        _started = true;

        _x2 = _x1;
        _x1 = input;
        _y2 = _y1;
        _y1 = y;
        _s2 = _s1;
        _s1 = s;

        return (float)y;
    }

    /// <summary>
    /// Processes a whole buffer.
    /// </summary>
    /// <param name="input">Input samples</param>
    /// <param name="output">Receives the filtered samples (at least as long as <paramref name="input"/>)</param>
    public void ProcessAll(ReadOnlySpan<float> input, Span<float> output)
    {
        if (output.Length < input.Length)
            throw new ArgumentException("Output buffer is too short.", nameof(output));

        for (var i = 0; i < input.Length; i++)
        {
            output[i] = Process(input[i]);
        }
    }

    /// <summary>
    /// Restores the starting frequency and clears the signal history.
    /// </summary>
    public void Reset()
    {
        _a = _initialA;
        _x1 = _x2 = _y1 = _y2 = _s1 = _s2 = 0;
        _power = 0;
        _powerWeight = 0;
        _started = false;
    }
}
