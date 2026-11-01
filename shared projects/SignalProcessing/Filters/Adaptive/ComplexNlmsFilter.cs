namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

using Vorcyc.Mathematics.Numerics;

/// <summary>
/// Represents the complex NLMS Adaptive filter (Normalized Least-Mean-Squares algorithm for complex-valued signals):
/// h += μ · e · conj(x) / (ε + Σ|x|²), where e = d − Σ h[k]·x(n−k).
/// </summary>
/// <remarks>
/// The step is normalized by the power of the whole regressor, so the algorithm is stable for 0 &lt; μ &lt; 2 regardless of the level of the input.
/// </remarks>
public class ComplexNlmsFilter : ComplexAdaptiveFilter
{
    private readonly float _mu;
    private readonly float _eps;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="ComplexNlmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order (number of complex taps)</param>
    /// <param name="mu">Step size, stable for 0 &lt; mu &lt; 2</param>
    /// <param name="eps">Epsilon, regularizes the normalization (added to the regressor power); keep it small compared to the expected power of the input over <paramref name="order"/> samples</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public ComplexNlmsFilter(int order, float mu = 0.5f, float eps = 1e-3f, float leakage = 0) : base(order)
    {
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.NonNegative(eps, nameof(eps));
        AdaptiveArgument.Leakage(leakage, mu);

        _mu = mu;
        _eps = eps;
        _leakage = leakage;
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts the taps.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override ComplexFp32 Process(ComplexFp32 input, ComplexFp32 desired)
    {
        var offset = Push(input);
        var y = Output(offset);

        var er = desired.Real - y.Real;
        var ei = desired.Imaginary - y.Imaginary;

        if (!float.IsFinite(er) || !float.IsFinite(ei))
        {
            return y;
        }

        var x = Regressor(offset);

        var power = 0.0;
        for (var k = 0; k < x.Length; k++)
        {
            power += (double)x[k].Real * x[k].Real + (double)x[k].Imaginary * x[k].Imaginary;
        }

        // with eps = 0 a regressor of zeros would give 0 / 0
        var norm = _eps + power;
        if (!(norm > 0))
        {
            return y;
        }

        // In double precision: mu / norm overflows single precision for a regressor of a tiny power (below about 1e-39, eps = 0). The change of a tap is
        // at most step · |e| · ‖x‖: when even that cannot be held, the sample is skipped.
        var step = _mu / norm;
        if (!(step * (Math.Abs(er) + Math.Abs(ei)) * Math.Sqrt(power) < float.MaxValue / 4))
        {
            return y;
        }

        var keep = 1 - _leakage * _mu;

        for (var k = 0; k < x.Length; k++)
        {
            _h[k].Real = (float)(keep * _h[k].Real + step * (er * x[k].Real + ei * x[k].Imaginary));
            _h[k].Imaginary = (float)(keep * _h[k].Imaginary + step * (ei * x[k].Real - er * x[k].Imaginary));
        }

        return y;
    }
}
