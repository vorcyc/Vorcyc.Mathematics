using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Filters;

/// <summary>
/// Represents N-th order Thiran allpass interpolation filter for Delta delay samples.
/// <code>
/// Example:
/// <br/>
///     N = 13           <br/>
///     Delta = 13 + 0.4 <br/>
/// </code>
/// </summary>
public class ThiranFilter : IirFilter
{
    /// <summary>
    /// Constructs <see cref="ThiranFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="delta">Delta (fractional delay)</param>
    public ThiranFilter(int order, float delta) : base(MakeTf(order, delta))
    {
    }

    /// <summary>
    /// Generates transfer function.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="delta">Delta (fractional delay)</param>
    private static TransferFunction MakeTf(int order, float delta)
    {
        var a = Enumerable.Range(0, order + 1).Select(i => ThiranCoefficient(i, order, delta));
        var b = a.Reverse();

        return new TransferFunction(b.ToArray(), a.ToArray());
    }

    /// <summary>
    /// Computes <paramref name="k"/>-th coefficient of denominator of transfer function.
    /// </summary>
    /// <param name="k">k</param>
    /// <param name="n">n</param>
    /// <param name="delta">Delta</param>
    private static float ThiranCoefficient(int k, int n, float delta)
    {
        var a = 1.0f;

        for (var i = 0; i <= n; i++)
        {
            a *= (delta - n + i) / (delta - n + k + i);
        }

        a *= MathF.Pow(-1, k) * VMath.BinomialCoefficient(k, n);

        return a;
    }
}
