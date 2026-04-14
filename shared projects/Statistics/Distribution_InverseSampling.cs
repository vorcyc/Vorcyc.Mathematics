using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

public static partial class Distribution
{
    #region Inverse CDF

    /// <summary>Inverse CDF (quantile) of the standard normal distribution.</summary>
    public static T NormalInverseCDF<T>(T p) where T : IFloatingPointIeee754<T>
    {
        double value = StatisticalMath.NormalInverseCdf(double.CreateChecked(p));
        return T.CreateChecked(value);
    }

    /// <summary>Inverse CDF of N(mean, stdDev^2).</summary>
    public static T NormalInverseCDF<T>(T p, T mean, T stdDev) where T : IFloatingPointIeee754<T>
        => mean + stdDev * NormalInverseCDF(p);

    /// <summary>Inverse CDF of Exp(lambda).</summary>
    public static T ExponentialInverseCDF<T>(T p, T lambda) where T : IFloatingPointIeee754<T>
    {
        if (p <= T.Zero || p >= T.One)
            throw new ArgumentOutOfRangeException(nameof(p));
        return -T.Log(T.One - p) / lambda;
    }

    /// <summary>Inverse CDF of Gamma(shape, scale) via bisection.</summary>
    public static T GammaInverseCDF<T>(T p, T shape, T scale) where T : IFloatingPointIeee754<T>
    {
        if (p <= T.Zero || p >= T.One)
            throw new ArgumentOutOfRangeException(nameof(p));

        double pd = double.CreateChecked(p);
        double high = double.CreateChecked(scale) * Math.Max(10, double.CreateChecked(shape) * 5);
        double x = StatisticalMath.InverseByBisection(
            v => double.CreateChecked(GammaCDF(T.CreateChecked(v), shape, scale)),
            pd,
            0,
            high);
        return T.CreateChecked(x);
    }

    /// <summary>Inverse CDF of Beta(alpha, beta) via bisection.</summary>
    public static T BetaInverseCDF<T>(T p, T alpha, T beta) where T : IFloatingPointIeee754<T>
    {
        if (p <= T.Zero || p >= T.One)
            throw new ArgumentOutOfRangeException(nameof(p));

        double pd = double.CreateChecked(p);
        double x = StatisticalMath.InverseByBisection(
            v => double.CreateChecked(BetaCDF(T.CreateChecked(v), alpha, beta)),
            pd,
            0,
            1);
        return T.CreateChecked(x);
    }

    #endregion

    #region Student / Chi / F

    /// <summary>Student t distribution CDF.</summary>
    public static T StudentTCDF<T>(T t, T degreesOfFreedom) where T : IFloatingPointIeee754<T>
        => T.CreateChecked(StatisticalMath.StudentTCdf(
            double.CreateChecked(t),
            double.CreateChecked(degreesOfFreedom)));

    /// <summary>Chi-squared distribution CDF.</summary>
    public static T ChiSquaredCDF<T>(T x, T degreesOfFreedom) where T : IFloatingPointIeee754<T>
        => T.CreateChecked(StatisticalMath.ChiSquaredCdf(
            double.CreateChecked(x),
            double.CreateChecked(degreesOfFreedom)));

    /// <summary>F distribution CDF.</summary>
    public static T FCDF<T>(T f, T df1, T df2) where T : IFloatingPointIeee754<T>
        => T.CreateChecked(StatisticalMath.FCdf(
            double.CreateChecked(f),
            double.CreateChecked(df1),
            double.CreateChecked(df2)));

    #endregion

    #region Sampling

    /// <summary>Draw one sample from N(mean, stdDev^2).</summary>
    public static T SampleNormal<T>(T mean, T stdDev, Random? random = null) where T : IFloatingPointIeee754<T>
    {
        random ??= Random.Shared;
        double u1 = 1.0 - random.NextDouble();
        double u2 = random.NextDouble();
        double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + stdDev * T.CreateChecked(z);
    }

    /// <summary>Draw one sample from Exp(lambda).</summary>
    public static T SampleExponential<T>(T lambda, Random? random = null) where T : IFloatingPointIeee754<T>
    {
        random ??= Random.Shared;
        double u = random.NextDouble();
        return -T.Log(T.CreateChecked(1 - u)) / lambda;
    }

    /// <summary>Draw one sample from Poisson(lambda).</summary>
    public static int SamplePoisson<T>(T lambda, Random? random = null) where T : IFloatingPointIeee754<T>
    {
        random ??= Random.Shared;
        double l = double.CreateChecked(lambda);
        double lExp = Math.Exp(-l);
        int k = 0;
        double p = 1.0;
        do
        {
            k++;
            p *= random.NextDouble();
        } while (p > lExp);
        return k - 1;
    }

    /// <summary>Draw one sample from Binomial(n, p).</summary>
    public static int SampleBinomial<T>(int n, T p, Random? random = null) where T : IFloatingPointIeee754<T>
    {
        random ??= Random.Shared;
        int successes = 0;
        double pd = double.CreateChecked(p);
        for (int i = 0; i < n; i++)
        {
            if (random.NextDouble() < pd)
                successes++;
        }
        return successes;
    }

    /// <summary>Draw one sample from Beta(alpha, beta) via gamma ratio.</summary>
    public static T SampleBeta<T>(T alpha, T beta, Random? random = null) where T : IFloatingPointIeee754<T>
    {
        T x = SampleGamma(alpha, T.One, random);
        T y = SampleGamma(beta, T.One, random);
        return x / (x + y);
    }

    /// <summary>Draw one sample from Gamma(shape, scale).</summary>
    public static T SampleGamma<T>(T shape, T scale, Random? random = null) where T : IFloatingPointIeee754<T>
    {
        random ??= Random.Shared;
        double k = double.CreateChecked(shape);
        double theta = double.CreateChecked(scale);

        if (k < 1)
        {
            double u = random.NextDouble();
            return T.CreateChecked(Math.Pow(u, 1 / k) * SampleGammaDouble(k + 1, theta, random));
        }

        return T.CreateChecked(SampleGammaDouble(k, theta, random));
    }

    private static double SampleGammaDouble(double shape, double scale, Random random)
    {
        double d = shape - 1.0 / 3.0;
        double c = 1.0 / Math.Sqrt(9.0 * d);
        while (true)
        {
            double x, v;
            do
            {
                x = StatisticalMath.NormalInverseCdf(random.NextDouble());
                v = 1.0 + c * x;
            } while (v <= 0);

            v = v * v * v;
            double u = random.NextDouble();
            if (u < 1 - 0.0331 * x * x * x * x)
                return d * v * scale;
            if (Math.Log(u) < 0.5 * x * x + d * (1 - v + Math.Log(v)))
                return d * v * scale;
        }
    }

    /// <summary>Fill span with independent N(mean, stdDev^2) samples.</summary>
    public static void FillNormal<T>(Span<T> destination, T mean, T stdDev, Random? random = null)
        where T : IFloatingPointIeee754<T>
    {
        for (int i = 0; i < destination.Length; i++)
            destination[i] = SampleNormal(mean, stdDev, random);
    }

    #endregion
}
