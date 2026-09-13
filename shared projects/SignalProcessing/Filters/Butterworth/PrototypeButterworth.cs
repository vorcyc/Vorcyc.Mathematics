using System.Numerics;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;

/// <summary>
/// Butterworth filter prototype.
/// </summary>
public static class PrototypeButterworth
{
    /// <summary>
    /// Evaluates analog poles of Butterworth filter of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    public static ComplexFp32[] Poles(int order)
    {
        var poles = new ComplexFp32[order];

        for (var k = 0; k < order; k++)
        {
            var theta = ConstantsFp32.PI * (2 * k + 1) / (2 * order);

            poles[k] = new ComplexFp32(-MathF.Sin(theta), MathF.Cos(theta));
        }

        return poles;
    }

    /// <summary>
    /// Analog Butterworth poles in <typeparamref name="T"/>.
    /// </summary>
    public static Complex<T>[] Poles<T>(int order)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var poles = new Complex<T>[order];
        var two = T.CreateChecked(2);
        for (var k = 0; k < order; k++)
        {
            var theta = T.Pi * T.CreateChecked(2 * k + 1) / (two * T.CreateChecked(order));
            poles[k] = new Complex<T>(-T.Sin(theta), T.Cos(theta));
        }
        return poles;
    }
}
