namespace Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;

/// <summary>
/// Chebyshev-I filter prototype.
/// </summary>
public static class PrototypeChebyshevI
{
    /// <summary>
    /// Evaluates analog poles of Chebyshev-I filter of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="ripple">Ripple (in dB)</param>
    public static ComplexFp32[] Poles(int order, float ripple = 0.1f)
    {
        var eps = MathF.Sqrt(MathF.Pow(10, ripple / 10) - 1);
        var s = VMath.Asinh(1 / eps) / order;
        var sinh = MathF.Sinh(s);
        var cosh = MathF.Cosh(s);

        var poles = new ComplexFp32[order];

        for (var k = 0; k < order; k++)
        {
            var theta = ConstantsFp32.PI * (2 * k + 1) / (2 * order);
            var re = -sinh * MathF.Sin(theta);
            var im =  cosh * MathF.Cos(theta);
            poles[k] = new ComplexFp32(re, im);
        }

        return poles;
    }
}
