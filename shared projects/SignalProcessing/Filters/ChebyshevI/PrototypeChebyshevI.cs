using System.Numerics;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;

/// <summary>
/// Chebyshev-I filter prototype.
/// </summary>
public static class PrototypeChebyshevI
{
    /// <summary>
    /// Evaluates analog poles of Chebyshev-I filter of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order, at least 1</param>
    /// <param name="ripple">Ripple (in dB)</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="order"/> is less than 1.</exception>
    public static ComplexFp32[] Poles(int order, float ripple = 0.1f)
    {
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), order, "Filter order must be at least 1.");

        var eps = MathF.Sqrt(MathF.Pow(10, ripple / 10) - 1);
        var s = TrigonometryHelper.Asinh(1 / eps) / order;
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

    /// <summary>
    /// Analog Chebyshev-I poles in <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="order"/> is less than 1.</exception>
    public static Complex<T>[] Poles<T>(int order, T? ripple = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), order, "Filter order must be at least 1.");

        var rip = ripple ?? T.CreateChecked(0.1);
        var ten = T.CreateChecked(10);
        var eps = T.Sqrt(T.Pow(ten, rip / ten) - T.One);
        var s = TrigonometryHelper.Asinh(T.One / eps) / T.CreateChecked(order);
        var sinh = T.Sinh(s);
        var cosh = T.Cosh(s);
        var two = T.CreateChecked(2);
        var poles = new Complex<T>[order];
        for (var k = 0; k < order; k++)
        {
            var theta = T.Pi * T.CreateChecked(2 * k + 1) / (two * T.CreateChecked(order));
            poles[k] = new Complex<T>(-sinh * T.Sin(theta), cosh * T.Cos(theta));
        }
        return poles;
    }
}
