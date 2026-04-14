using System.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Internal distribution and rank helpers for inferential statistics.
/// </summary>
internal static class StatisticalMath
{
    public static double[] AssignRanks(ReadOnlySpan<double> values, int[] groupIndices, int groupCount)
    {
        int n = values.Length;
        var indexed = new (double Value, int Index)[n];
        for (int i = 0; i < n; i++)
            indexed[i] = (values[i], i);

        Array.Sort(indexed, (a, b) => a.Value.CompareTo(b.Value));

        var ranks = new double[n];
        int i0 = 0;
        while (i0 < n)
        {
            int i1 = i0;
            while (i1 + 1 < n && indexed[i1 + 1].Value == indexed[i0].Value)
                i1++;

            double averageRank = 0;
            for (int k = i0; k <= i1; k++)
                averageRank += k + 1;
            averageRank /= i1 - i0 + 1;

            for (int k = i0; k <= i1; k++)
                ranks[indexed[k].Index] = averageRank;

            i0 = i1 + 1;
        }

        return ranks;
    }

    public static double NormalInverseCdf(double p)
    {
        if (p <= 0 || p >= 1)
            throw new ArgumentOutOfRangeException(nameof(p), "Probability must be in (0, 1).");

        return InverseByBisection(NormalCdf, p, -10, 10);
    }

    public static double StudentTCdf(double t, double df)
    {
        double x = df / (df + t * t);
        double beta = VMath.RegularizedIncompleteBeta(double.CreateChecked(x), df / 2, 0.5);
        double cdf = 1 - 0.5 * beta;
        return t >= 0 ? cdf : 1 - cdf;
    }

    public static double StudentTTwoTailP(double t, double df)
    {
        t = Math.Abs(t);
        return 2 * (1 - StudentTCdf(t, df));
    }

    public static double StudentTCriticalValue(double confidenceLevel, double df)
    {
        double alpha = 1 - confidenceLevel;
        double p = 1 - alpha / 2;
        return InverseByBisection(x => StudentTCdf(x, df), p, 0, 100);
    }

    public static double ChiSquaredCdf(double x, double df)
    {
        if (x <= 0)
            return 0;

        if (Math.Abs(df - 1) < 1e-12)
            return NormalCdf(Math.Sqrt(x)) - NormalCdf(-Math.Sqrt(x));

        double shape = df / 2;
        double scale = 0.5;
        return VMath.LowerIncompleteGamma(double.CreateChecked(shape), double.CreateChecked(x * scale))
            / VMath.Gamma(double.CreateChecked(shape));
    }

    public static double ChiSquaredSurvival(double x, double df)
    {
        if (x <= 0)
            return 1;

        if (Math.Abs(df - 1) < 1e-12)
            return NormalTwoTailP(Math.Sqrt(x));

        double cdf = ChiSquaredCdf(x, df);
        return Math.Clamp(1 - cdf, 0, 1);
    }

    public static double FCdf(double f, double df1, double df2)
    {
        if (f <= 0)
            return 0;
        double x = df1 * f / (df1 * f + df2);
        return VMath.RegularizedIncompleteBeta(double.CreateChecked(x), df1 / 2, df2 / 2);
    }

    public static double FSurvival(double f, double df1, double df2) =>
        1 - FCdf(f, df1, df2);

    public static double NormalCdf(double z) =>
        0.5 * (1 + double.CreateChecked(VMath.Erf(z / Math.Sqrt(2))));

    public static double NormalTwoTailP(double z) =>
        2 * (1 - NormalCdf(Math.Abs(z)));

    public static double MannWhitneyPValue(double u, int n1, int n2)
    {
        double mu = n1 * n2 / 2.0;
        double sigma = Math.Sqrt(n1 * n2 * (n1 + n2 + 1) / 12.0);
        double z = (u - mu + 0.5) / sigma;
        return NormalTwoTailP(z);
    }

    public static double KruskalWallisPValue(double h, int groupCount)
    {
        int df = groupCount - 1;
        return ChiSquaredSurvival(h, df);
    }

    public static double WilcoxonPValue(double w, int n)
    {
        double mu = n * (n + 1) / 4.0;
        double sigma = Math.Sqrt(n * (n + 1) * (2 * n + 1) / 24.0);
        double z = (w - mu - 0.5) / sigma;
        return NormalTwoTailP(z);
    }

    public static double InverseByBisection(Func<double, double> cdf, double p, double low, double high, int maxIter = 80)
    {
        for (int i = 0; i < maxIter; i++)
        {
            double mid = (low + high) / 2;
            double value = cdf(mid);
            if (value < p)
                low = mid;
            else
                high = mid;
        }

        return (low + high) / 2;
    }
}
