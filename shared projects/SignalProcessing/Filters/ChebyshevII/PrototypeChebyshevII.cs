using System.Numerics;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

/// <summary>
/// Chebyshev-II filter prototype.
/// </summary>
public static class PrototypeChebyshevII
{
    /// <summary>
    /// Evaluates analog poles of Chebyshev-II filter of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="ripple">Ripple (in dB)</param>
    public static ComplexFp32[] Poles(int order, float ripple = 0.1f)
    {
        var eps = MathF.Sqrt(MathF.Pow(10, ripple / 10) - 1);
        var s = TrigonometryHelper.Asinh(1 / eps) / order;
        var sinh = MathF.Sinh(s);
        var cosh = MathF.Cosh(s);

        var poles = new ComplexFp32[order];

        for (var k = 0; k < order; k++)
        {
            var theta = ConstantsFp32.PI * (2 * k + 1) / (2 * order);
            var re = -sinh * MathF.Sin(theta);
            var im = cosh * MathF.Cos(theta);
            poles[k] = 1 / new ComplexFp32(re, im);
        }

        return poles;
    }

    /// <summary>
    /// Evaluates analog zeros of Chebyshev-II filter of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    public static ComplexFp32[] Zeros(int order)
    {
        var zeros = new ComplexFp32[order];

        for (var k = 0; k < order; k++)
        {
            var theta = ConstantsFp32.PI * (2 * k + 1) / (2 * order);
            zeros[k] = new ComplexFp32(0, -1 / MathF.Cos(theta));
        }

        return zeros;
    }

    /// <summary>
    /// Analog Chebyshev-II poles in <typeparamref name="T"/>.
    /// </summary>
    public static Complex<T>[] Poles<T>(int order, T? ripple = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
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
            poles[k] = T.One / new Complex<T>(-sinh * T.Sin(theta), cosh * T.Cos(theta));
        }
        return poles;
    }

    /// <summary>
    /// Analog Chebyshev-II zeros in <typeparamref name="T"/>.
    /// </summary>
    public static Complex<T>[] Zeros<T>(int order)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var zeros = new Complex<T>[order];
        var two = T.CreateChecked(2);
        for (var k = 0; k < order; k++)
        {
            var theta = T.Pi * T.CreateChecked(2 * k + 1) / (two * T.CreateChecked(order));
            zeros[k] = new Complex<T>(T.Zero, -T.One / T.Cos(theta));
        }
        return zeros;
    }
}
