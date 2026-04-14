using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

public static partial class Advanced
{
    /// <summary>
    /// One-sample t-test with two-tailed p-value.
    /// </summary>
    public static HypothesisTestResult<T> TTestResult<T>(this Span<T> sample, T populationMean)
        where T : IFloatingPointIeee754<T>
    {
        T statistic = sample.TTest(populationMean);
        int df = sample.Length - 1;
        double p = StatisticalMath.StudentTTwoTailP(double.CreateChecked(statistic), df);
        return new HypothesisTestResult<T>(statistic, T.CreateChecked(p), df);
    }

    /// <summary>
    /// One-way ANOVA with F-test p-value.
    /// </summary>
    public static HypothesisTestResult<T> AnovaResult<T>(this IEnumerable<ArraySegment<T>> groups)
        where T : IFloatingPointIeee754<T>
    {
        int k = groups.Count();
        int n = groups.Sum(g => g.Count);
        T fStatistic = groups.Anova();
        int df1 = k - 1;
        int df2 = n - k;
        double p = StatisticalMath.FSurvival(double.CreateChecked(fStatistic), df1, df2);
        return new HypothesisTestResult<T>(fStatistic, T.CreateChecked(p), df1);
    }

    /// <summary>
    /// Chi-squared goodness-of-fit test with upper-tail p-value.
    /// </summary>
    public static HypothesisTestResult<T> ChiSquaredTestResult<T>(this Span<T> observed, Span<T> expected)
        where T : IFloatingPointIeee754<T>
    {
        T statistic = observed.ChiSquaredTest(expected);
        int df = observed.Length - 1;
        double p = StatisticalMath.ChiSquaredSurvival(double.CreateChecked(statistic), df);
        return new HypothesisTestResult<T>(statistic, T.CreateChecked(p), df);
    }

    /// <summary>
    /// Mann-Whitney U test with normal-approximation p-value.
    /// </summary>
    public static HypothesisTestResult<T> MannWhitneyUTestResult<T>(this Span<T> sample1, Span<T> sample2)
        where T : IFloatingPointIeee754<T>
    {
        T u = sample1.MannWhitneyUTest(sample2);
        double p = StatisticalMath.MannWhitneyPValue(
            double.CreateChecked(u), sample1.Length, sample2.Length);
        return new HypothesisTestResult<T>(u, T.CreateChecked(p), 0);
    }

    /// <summary>
    /// Non-destructive percentile: copies data unless <paramref name="sortInPlace"/> is true.
    /// </summary>
    public static T Percentile<T>(
        this ReadOnlySpan<T> sequence,
        double percentile,
        bool sortInPlace = false)
        where T : IFloatingPointIeee754<T>
    {
        if (sequence.IsEmpty)
            throw new ArgumentException("Sequence cannot be empty.", nameof(sequence));

        if (sortInPlace)
            return sequence.ToArray().AsSpan().Percentile(percentile);

        var copy = sequence.ToArray();
        return copy.AsSpan().Percentile(percentile);
    }
}
