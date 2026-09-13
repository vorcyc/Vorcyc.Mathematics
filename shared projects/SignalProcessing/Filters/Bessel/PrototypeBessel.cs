using System.Numerics;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Bessel;
/// <summary>
/// Bessel filter prototype.
/// </summary>
public static class PrototypeBessel
{
    /// <summary>
    /// Gets <paramref name="k"/>-th coefficient of <paramref name="n"/>-th order Bessel polynomial.
    /// </summary>
    /// <param name="k">k</param>
    /// <param name="n">n</param>
    public static float Reverse(int k, int n)
    {
        return VMath.Factorial(2 * n - k) /
            (MathF.Pow(2, n - k) * VMath.Factorial(k) * VMath.Factorial(n - k));
    }

    ///// <summary>
    ///// Gets <paramref name="k"/>-th coefficient of <paramref name="n"/>-th order Bessel polynomial.
    ///// </summary>
    ///// <param name="k">k</param>
    ///// <param name="n">n</param>
    //public static float Reverse(int k, int n)
    //{
    //    return VMath.Factorial(2 * n - k) /
    //        (MathF.Pow(2, n - k) * VMath.Factorial(k) * VMath.Factorial(n - k));
    //}

    /// <summary>
    /// Evaluates analog poles of Bessel filter of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    public static ComplexFp32[] Poles(int order)
    {
        var a = Enumerable.Range(0, order + 1)
                          .Select(i => Reverse(order - i, order))
                          .ToArray();
        var poles = VMath.PolynomialRoots(a);
        // ...and normalize
        var norm = MathF.Pow(10, -MathF.Log10(a[order - 1]) / order);
        for (var i = 0; i < poles.Length; i++)
        {
            poles[i] *= norm;
        }
        return poles;
    }

    /// <summary>
    /// <paramref name="k"/>-th reverse Bessel polynomial coefficient of order <paramref name="n"/>, in <typeparamref name="T"/>.
    /// </summary>
    public static T Reverse<T>(int k, int n)
        where T : IFloatingPointIeee754<T>
    {
        return VMath.Factorial(T.CreateChecked(2 * n - k)) /
               (T.Pow(T.CreateChecked(2), T.CreateChecked(n - k)) *
                VMath.Factorial(T.CreateChecked(k)) *
                VMath.Factorial(T.CreateChecked(n - k)));
    }

    /// <summary>
    /// Analog Bessel poles in <typeparamref name="T"/>.
    /// </summary>
    public static Complex<T>[] Poles<T>(int order)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var a = new T[order + 1];
        for (var i = 0; i <= order; i++)
            a[i] = Reverse<T>(order - i, order);
        var poles = VMath.PolynomialRoots(a)
                    ?? throw new InvalidOperationException("Bessel polynomial root finder returned no roots.");
        var norm = T.Pow(T.CreateChecked(10), -T.Log10(a[order - 1]) / T.CreateChecked(order));
        for (var i = 0; i < poles.Length; i++)
            poles[i] *= norm;
        return poles;
    }
}
