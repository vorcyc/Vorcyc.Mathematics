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
        // Durand-Kerner on the Bessel polynomial does not converge in single precision from order 7 up (wrong poles, NaN at 8-9).
        var poles = Poles<double>(order);
        var result = new ComplexFp32[poles.Length];
        for (var i = 0; i < result.Length; i++)
            result[i] = new ComplexFp32((float)poles[i].Real, (float)poles[i].Imaginary);
        return result;
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
        if (typeof(T) != typeof(double))
        {
            var wide = Poles<double>(order);
            var narrow = new Complex<T>[wide.Length];
            for (var i = 0; i < narrow.Length; i++)
                narrow[i] = new Complex<T>(T.CreateChecked(wide[i].Real), T.CreateChecked(wide[i].Imaginary));
            return narrow;
        }

        var a = new T[order + 1];
        for (var i = 0; i <= order; i++)
            a[i] = Reverse<T>(order - i, order);
        var poles = VMath.PolynomialRoots(a)
                    ?? throw new InvalidOperationException("Bessel polynomial root finder returned no roots.");
        // Durand-Kerner overflows to NaN from order 17 up; Aberth-Ehrlich converges there.
        if (poles.Any(p => !T.IsFinite(p.Real) || !T.IsFinite(p.Imaginary)))
            poles = ToGeneric<T>(AberthRoots(ReverseCoefficients(order)));
        var norm = T.Pow(T.CreateChecked(10), -T.Log10(a[order - 1]) / T.CreateChecked(order));
        for (var i = 0; i < poles.Length; i++)
            poles[i] *= norm;
        return poles;
    }

    // Monic reverse Bessel polynomial, highest power first, by the ratio a(k-1)/a(k) = (2n-k+1) k / (2 (n-k+1)); no huge factorials.
    private static double[] ReverseCoefficients(int order)
    {
        var a = new double[order + 1];
        a[0] = 1;
        for (var k = order; k >= 1; k--)
            a[order - k + 1] = a[order - k] * (2.0 * order - k + 1) * k / (2.0 * (order - k + 1));
        return a;
    }

    private static Complex[] AberthRoots(double[] a)
    {
        var n = a.Length - 1;
        var z = new Complex[n];
        for (var k = 0; k < n; k++)
            z[k] = Complex.FromPolarCoordinates((n + 1) / 2.0, 2 * Math.PI * k / n + 0.4);

        for (var iter = 0; iter < 500; iter++)
        {
            var worst = 0.0;
            for (var k = 0; k < n; k++)
            {
                Complex p = a[0], dp = Complex.Zero;
                for (var i = 1; i <= n; i++)
                {
                    dp = dp * z[k] + p;
                    p = p * z[k] + a[i];
                }

                if (p == Complex.Zero) continue;
                var ratio = p / dp;
                var sum = Complex.Zero;
                for (var j = 0; j < n; j++)
                    if (j != k) sum += 1 / (z[k] - z[j]);
                var step = ratio / (1 - ratio * sum);
                z[k] -= step;
                worst = Math.Max(worst, step.Magnitude / Math.Max(z[k].Magnitude, 1e-300));
            }

            if (worst < 1e-15) break;
        }

        return z;
    }

    private static Complex<T>[] ToGeneric<T>(Complex[] z)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var r = new Complex<T>[z.Length];
        for (var i = 0; i < r.Length; i++)
            r[i] = new Complex<T>(T.CreateChecked(z[i].Real), T.CreateChecked(z[i].Imaginary));
        return r;
    }
}
