namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

using Vorcyc.Mathematics.Numerics;

/// <summary>
/// Represents the complex LMS Adaptive filter (Widrow–Hoff algorithm for complex-valued signals):
/// h += μ · e · conj(x), where e = d − Σ h[k]·x(n−k).
/// </summary>
/// <remarks>
/// The algorithm is stable for 0 &lt; μ &lt; 2 / (order · σ²), where σ² is the power |x|² of the input; if the level of the input is not known,
/// use <see cref="ComplexNlmsFilter"/>.
/// </remarks>
public class ComplexLmsFilter : ComplexAdaptiveFilter
{
    private readonly float _mu;
    private readonly float _leakage;

    /// <summary>
    /// Constructs <see cref="ComplexLmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order (number of complex taps)</param>
    /// <param name="mu">Step size</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public ComplexLmsFilter(int order, float mu = 0.001f, float leakage = 0) : base(order)
    {
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.Leakage(leakage, mu);

        _mu = mu;
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
        var keep = 1 - _leakage * _mu;

        // e · conj(x) = (er·xr + ei·xi) + i(ei·xr − er·xi)
        for (var k = 0; k < x.Length; k++)
        {
            _h[k].Real = keep * _h[k].Real + _mu * (er * x[k].Real + ei * x[k].Imaginary);
            _h[k].Imaginary = keep * _h[k].Imaginary + _mu * (ei * x[k].Real - er * x[k].Imaginary);
        }

        return y;
    }
}
