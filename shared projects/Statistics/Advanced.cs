//高级统计函数
//14.	百分位数 (Percentiles): 计算数据集的指定百分位数。
//15.	四分位数 (Quartiles): 计算数据集的四分位数。
//16.	偏度 (Skewness): 计算数据集的偏度，衡量数据分布的对称性。
//17.	峰度 (Kurtosis): 计算数据集的峰度，衡量数据分布的尖锐程度。
//18.	置信区间 (Confidence Intervals): 计算均值或比例的置信区间。
//19.	假设检验 (Hypothesis Testing): 实现各种假设检验，如z检验、t检验、卡方检验等。
//20.	方差分析 (ANOVA): 实现单因素和多因素方差分析。
//21.	卡方检验 (Chi-Squared Test): 实现卡方独立性检验和拟合优度检验。
//22.	非参数检验 (Non-parametric Tests): 实现如曼 - 惠特尼U检验、克鲁斯卡尔 - 沃利斯检验等非参数检验。  


namespace Vorcyc.Mathematics.Statistics;

using System.Numerics;
using System.Linq;

/// <summary>
/// Provides advanced statistical functions, including methods for computing percentiles, quartiles, skewness, kurtosis, confidence intervals, hypothesis testing, ANOVA, the chi-squared test, and non-parametric tests.
/// </summary>
/// <remarks>
/// This class contains computation methods for the following advanced statistical functions:
/// <list type="bullet">
/// <item>
/// <description>Percentiles: Computes the specified percentile of a data set.</description>
/// </item>
/// <item>
/// <description>Quartiles: Computes the quartiles of a data set.</description>
/// </item>
/// <item>
/// <description>Skewness: Computes the skewness of a data set, measuring the asymmetry of the data distribution.</description>
/// </item>
/// <item>
/// <description>Kurtosis: Computes the kurtosis of a data set, measuring the sharpness of the data distribution.</description>
/// </item>
/// <item>
/// <description>Confidence Intervals: Computes the confidence interval for a mean or proportion.</description>
/// </item>
/// <item>
/// <description>Hypothesis Testing: Implements various hypothesis tests, such as the z-test, t-test, and chi-squared test.</description>
/// </item>
/// <item>
/// <description>ANOVA: Implements one-way and multi-way analysis of variance.</description>
/// </item>
/// <item>
/// <description>Chi-Squared Test: Implements the chi-squared test of independence and the goodness-of-fit test.</description>
/// </item>
/// <item>
/// <description>Non-parametric Tests: Implements non-parametric tests such as the Mann-Whitney U test and the Kruskal-Wallis test.</description>
/// </item>
/// </list>
/// </remarks>
public static partial class Advanced
{
    /// <summary>
    /// Computes the specified percentile of a data set.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sequence">The data set.</param>
    /// <param name="percentile">The percentile (between 0 and 1).</param>
    /// <returns>The value at the specified percentile.</returns>
    /// <remarks>
    /// Percentiles: Used to determine the relative position of a value within a data set.
    /// For example, the 90th percentile indicates that 90% of the values in the data set are less than or equal to that value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Percentile<T>(this Span<T> sequence, double percentile) where T : IFloatingPointIeee754<T>
    {
        sequence.Sort();
        int N = sequence.Length;
        double n = (N - 1) * percentile + 1;
        if (n == 1d) return sequence[0];
        else if (n == N) return sequence[N - 1];
        else
        {
            int k = (int)n;
            double d = n - k;
            return sequence[k - 1] + T.CreateChecked(d) * (sequence[k] - sequence[k - 1]);
        }
    }

    /// <summary>
    /// Computes the quartiles of a data set.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sequence">The data set.</param>
    /// <returns>A tuple containing the first, second, and third quartiles.</returns>
    /// <remarks>
    /// Quartiles: The three values that divide a data set into four equal parts.
    /// The first quartile (Q1) is the value below or equal to which 25% of the data lies, the second quartile (Q2) is the median, and the third quartile (Q3) is the value below or equal to which 75% of the data lies.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T Q1, T Q2, T Q3) Quartiles<T>(this Span<T> sequence) where T : IFloatingPointIeee754<T>
    {
        return (
            sequence.Percentile(0.25),
            sequence.Percentile(0.50),
            sequence.Percentile(0.75)
        );
    }

    /// <summary>
    /// Computes the skewness of a data set, measuring the asymmetry of the data distribution.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sequence">The data set.</param>
    /// <returns>The skewness value of the data set.</returns>
    /// <remarks>
    /// Skewness: Used to measure the asymmetry of a data distribution.
    /// A skewness of zero indicates a symmetric distribution, a positive skewness indicates a right-skewed distribution, and a negative skewness indicates a left-skewed distribution.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Skewness<T>(this Span<T> sequence) where T : IFloatingPointIeee754<T>
    {
        T mean = sequence.Average();
        T n = T.CreateChecked(sequence.Length);
        T m3 = sequence.Sum(x => T.Pow(x - mean, T.CreateChecked(3))) / n;
        T m2 = sequence.Sum(x => T.Pow(x - mean, T.CreateChecked(2))) / n;
        return m3 / T.Pow(m2, T.CreateChecked(1.5));
    }

    /// <summary>
    /// Computes the kurtosis of a data set, measuring the sharpness of the data distribution.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sequence">The data set.</param>
    /// <returns>The kurtosis value of the data set.</returns>
    /// <remarks>
    /// Kurtosis: Used to measure the sharpness of a data distribution.
    /// The higher the kurtosis value, the sharper the distribution; the lower the kurtosis value, the flatter the distribution.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Kurtosis<T>(this Span<T> sequence) where T : IFloatingPointIeee754<T>
    {
        T mean = sequence.Average();
        T n = T.CreateChecked(sequence.Length);
        T m4 = sequence.Sum(x => T.Pow(x - mean, T.CreateChecked(4))) / n;
        T m2 = sequence.Sum(x => T.Pow(x - mean, T.CreateChecked(2))) / n;
        return m4 / T.Pow(m2, T.CreateChecked(2)) - T.CreateChecked(3);
    }

    /// <summary>
    /// Computes the confidence interval for a mean or proportion.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sequence">The data set.</param>
    /// <param name="confidenceLevel">The confidence level (for example, 0.95 represents a 95% confidence level).</param>
    /// <returns>A tuple containing the lower and upper bounds of the confidence interval.</returns>
    /// <remarks>
    /// Confidence Intervals: Used to estimate the range of a population parameter.
    /// A confidence interval provides a range that contains the likely values of the population parameter, and the probability that the range contains the population parameter equals the confidence level.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T Lower, T Upper) ConfidenceInterval<T>(this Span<T> sequence, double confidenceLevel) where T : IFloatingPointIeee754<T>
    {
        T mean = sequence.Average();
        T stdDev = T.Sqrt(sequence.Sum(x => T.Pow(x - mean, T.CreateChecked(2))) / (T.CreateChecked(sequence.Length) - T.One));
        double df = sequence.Length - 1;
        double critical = StatisticalMath.StudentTCriticalValue(confidenceLevel, df);
        T marginOfError = T.CreateChecked(critical) * (stdDev / T.Sqrt(T.CreateChecked(sequence.Length)));
        return (mean - marginOfError, mean + marginOfError);
    }

    /// <summary>
    /// Implements various hypothesis tests, such as the z-test, t-test, and chi-squared test.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sample">The sample data set.</param>
    /// <param name="populationMean">The population mean.</param>
    /// <returns>The test statistic of the t-test.</returns>
    /// <remarks>
    /// Hypothesis Testing: Used to test whether sample data supports a given hypothesis.
    /// For example, the t-test is used to compare a sample mean with a population mean, the z-test is used to compare a sample proportion with a population proportion, and the chi-squared test is used to test the independence of categorical data.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T TTest<T>(this Span<T> sample, T populationMean) where T : IFloatingPointIeee754<T>
    {
        T sampleMean = sample.Average();
        T stdDev = T.Sqrt(sample.Sum(x => T.Pow(x - sampleMean, T.CreateChecked(2))) / (T.CreateChecked(sample.Length) - T.One));
        return (sampleMean - populationMean) / (stdDev / T.Sqrt(T.CreateChecked(sample.Length)));
    }

    /// <summary>
    /// Implements one-way and multi-way analysis of variance.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="groups">The collection of data groups.</param>
    /// <returns>The F-value of the analysis of variance.</returns>
    /// <remarks>
    /// ANOVA: Used to compare whether there are significant differences among the means of multiple groups.
    /// One-way ANOVA is used to compare multiple levels of one factor, while multi-way ANOVA is used to compare the interactions of multiple factors.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Anova<T>(this IEnumerable<ArraySegment<T>> groups) where T : IFloatingPointIeee754<T>
    {
        int k = 0;
        int n = 0;
        T grandSum = T.Zero;
        int totalLength = 0;

        // 计算总数和总和
        foreach (var group in groups)
        {
            k++;
            n += group.Count;
            foreach (var value in group)
            {
                grandSum += value;
                totalLength++;
            }
        }

        T grandMean = grandSum / T.CreateChecked(totalLength);

        T ssBetween = T.Zero;
        T ssWithin = T.Zero;

        // 计算组间平方和和组内平方和
        foreach (var group in groups)
        {
            T groupSum = T.Zero;
            foreach (var value in group)
            {
                groupSum += value;
            }
            T groupMean = groupSum / T.CreateChecked(group.Count);
            ssBetween += T.CreateChecked(group.Count) * T.Pow(groupMean - grandMean, T.CreateChecked(2));

            foreach (var value in group)
            {
                ssWithin += T.Pow(value - groupMean, T.CreateChecked(2));
            }
        }

        T msBetween = ssBetween / T.CreateChecked(k - 1);
        T msWithin = ssWithin / T.CreateChecked(n - k);

        return msBetween / msWithin;
    }

    /// <summary>
    /// Implements the chi-squared test of independence and the goodness-of-fit test.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="observed">The observed-value data set.</param>
    /// <param name="expected">The expected-value data set.</param>
    /// <returns>The test statistic of the chi-squared test.</returns>
    /// <remarks>
    /// Chi-Squared Test: Used to test the independence and goodness of fit of categorical data.
    /// The test of independence is used to test whether two categorical variables are independent, while the goodness-of-fit test is used to test whether observed values match expected values.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ChiSquaredTest<T>(this Span<T> observed, Span<T> expected) where T : IFloatingPointIeee754<T>
    {
        if (observed.Length != expected.Length)
            throw new ArgumentException("Observed and expected arrays must have the same length.");

        T chiSquared = T.Zero;

        for (int i = 0; i < observed.Length; i++)
        {
            T difference = observed[i] - expected[i];
            chiSquared += T.Pow(difference, T.CreateChecked(2)) / expected[i];
        }

        return chiSquared;
    }

    /// <summary>
    /// Implements non-parametric tests such as the Mann-Whitney U test and the Kruskal-Wallis test.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sample1">The first sample data set.</param>
    /// <param name="sample2">The second sample data set.</param>
    /// <returns>The test statistic of the Mann-Whitney U test.</returns>
    /// <remarks>
    /// Non-parametric Tests: Used to compare the distributions of two or more samples.
    /// The Mann-Whitney U test is used to compare the distributions of two independent samples, while the Kruskal-Wallis test is used to compare the distributions of multiple independent samples.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T MannWhitneyUTest<T>(this Span<T> sample1, Span<T> sample2) where T : IFloatingPointIeee754<T>
    {
        int n1 = sample1.Length;
        int n2 = sample2.Length;
        int totalLength = n1 + n2;

        // 合并样本并排序
        var combinedSamples = new (T Value, int SampleIndex)[totalLength];
        for (int i = 0; i < n1; i++)
        {
            combinedSamples[i] = (sample1[i], 1);
        }
        for (int i = 0; i < n2; i++)
        {
            combinedSamples[n1 + i] = (sample2[i], 2);
        }
        Array.Sort(combinedSamples, (a, b) => a.Value.CompareTo(b.Value));

        // 计算秩
        double[] ranks = new double[totalLength];
        for (int i = 0; i < totalLength; i++)
        {
            ranks[i] = i + 1;
        }

        // 处理相同值的秩
        for (int i = 0; i < totalLength; i++)
        {
            int j = i;
            while (j < totalLength - 1 && combinedSamples[j].Value.Equals(combinedSamples[j + 1].Value))
            {
                j++;
            }
            if (i != j)
            {
                double rankSum = 0;
                for (int k = i; k <= j; k++)
                {
                    rankSum += ranks[k];
                }
                double averageRank = rankSum / (j - i + 1);
                for (int k = i; k <= j; k++)
                {
                    ranks[k] = averageRank;
                }
                i = j;
            }
        }

        // 计算秩和
        T rankSum1 = T.Zero;
        T rankSum2 = T.Zero;
        for (int i = 0; i < totalLength; i++)
        {
            if (combinedSamples[i].SampleIndex == 1)
            {
                rankSum1 += T.CreateChecked(ranks[i]);
            }
            else
            {
                rankSum2 += T.CreateChecked(ranks[i]);
            }
        }

        // 计算 U 值
        T u1 = rankSum1 - T.CreateChecked(n1 * (n1 + 1)) / T.CreateChecked(2.0);
        T u2 = rankSum2 - T.CreateChecked(n2 * (n2 + 1)) / T.CreateChecked(2.0);

        return T.Min(u1, u2);
    }

    /// <summary>
    /// Performs an independent two-sample t-test assuming equal variances (pooled t-test).
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sample1">The first independent sample.</param>
    /// <param name="sample2">The second independent sample.</param>
    /// <returns>A <see cref="HypothesisTestResult{T}"/> containing the test statistic, two-tailed p-value, and degrees of freedom.</returns>
    /// <exception cref="ArgumentException">Thrown when either sample contains fewer than two values.</exception>
    /// <remarks>
    /// The pooled t-test assumes the two populations have equal variances. The pooled variance is
    /// estimated from both samples and the degrees of freedom are n1 + n2 - 2.
    /// </remarks>
    public static HypothesisTestResult<T> TTestIndependent<T>(ReadOnlySpan<T> sample1, ReadOnlySpan<T> sample2)
        where T : IFloatingPointIeee754<T>
    {
        if (sample1.Length < 2 || sample2.Length < 2)
            throw new ArgumentException("Each sample must contain at least two values.");

        int n1 = sample1.Length;
        int n2 = sample2.Length;

        T mean1 = Mean(sample1);
        T mean2 = Mean(sample2);
        T variance1 = SampleVariance(sample1, mean1);
        T variance2 = SampleVariance(sample2, mean2);

        T nT1 = T.CreateChecked(n1);
        T nT2 = T.CreateChecked(n2);
        int df = n1 + n2 - 2;

        T pooledVariance = ((nT1 - T.One) * variance1 + (nT2 - T.One) * variance2) / T.CreateChecked(df);
        T standardError = T.Sqrt(pooledVariance * (T.One / nT1 + T.One / nT2));
        T statistic = standardError == T.Zero ? T.Zero : (mean1 - mean2) / standardError;

        T pValue = T.CreateChecked(StatisticalMath.StudentTTwoTailP(double.CreateChecked(statistic), df));
        return new HypothesisTestResult<T>(statistic, pValue, df);
    }

    /// <summary>
    /// Performs an independent two-sample t-test assuming equal variances (pooled t-test).
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sample1">The first independent sample.</param>
    /// <param name="sample2">The second independent sample.</param>
    /// <returns>A <see cref="HypothesisTestResult{T}"/> containing the test statistic, two-tailed p-value, and degrees of freedom.</returns>
    /// <exception cref="ArgumentException">Thrown when either sample contains fewer than two values.</exception>
    public static HypothesisTestResult<T> TTestIndependent<T>(T[] sample1, T[] sample2)
        where T : IFloatingPointIeee754<T>
        => TTestIndependent<T>((ReadOnlySpan<T>)sample1, (ReadOnlySpan<T>)sample2);

    /// <summary>
    /// Performs Welch's t-test for two independent samples with unequal variances.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sample1">The first independent sample.</param>
    /// <param name="sample2">The second independent sample.</param>
    /// <returns>A <see cref="HypothesisTestResult{T}"/> containing the test statistic, two-tailed p-value, and the Welch-Satterthwaite degrees of freedom (rounded down to the nearest integer).</returns>
    /// <exception cref="ArgumentException">Thrown when either sample contains fewer than two values.</exception>
    /// <remarks>
    /// Welch's t-test does not assume equal population variances. The degrees of freedom are
    /// approximated using the Welch-Satterthwaite equation and truncated to an integer for the
    /// p-value lookup.
    /// </remarks>
    public static HypothesisTestResult<T> TTestWelch<T>(ReadOnlySpan<T> sample1, ReadOnlySpan<T> sample2)
        where T : IFloatingPointIeee754<T>
    {
        if (sample1.Length < 2 || sample2.Length < 2)
            throw new ArgumentException("Each sample must contain at least two values.");

        int n1 = sample1.Length;
        int n2 = sample2.Length;

        T mean1 = Mean(sample1);
        T mean2 = Mean(sample2);
        T variance1 = SampleVariance(sample1, mean1);
        T variance2 = SampleVariance(sample2, mean2);

        T nT1 = T.CreateChecked(n1);
        T nT2 = T.CreateChecked(n2);

        T term1 = variance1 / nT1;
        T term2 = variance2 / nT2;
        T standardError = T.Sqrt(term1 + term2);
        T statistic = standardError == T.Zero ? T.Zero : (mean1 - mean2) / standardError;

        // Welch-Satterthwaite degrees of freedom.
        T numerator = (term1 + term2) * (term1 + term2);
        T denominator = term1 * term1 / (nT1 - T.One) + term2 * term2 / (nT2 - T.One);
        T dfApprox = denominator == T.Zero ? T.One : numerator / denominator;
        int df = int.Max(1, int.CreateTruncating(dfApprox));

        T pValue = T.CreateChecked(StatisticalMath.StudentTTwoTailP(double.CreateChecked(statistic), df));
        return new HypothesisTestResult<T>(statistic, pValue, df);
    }

    /// <summary>
    /// Performs Welch's t-test for two independent samples with unequal variances.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="sample1">The first independent sample.</param>
    /// <param name="sample2">The second independent sample.</param>
    /// <returns>A <see cref="HypothesisTestResult{T}"/> containing the test statistic, two-tailed p-value, and degrees of freedom.</returns>
    /// <exception cref="ArgumentException">Thrown when either sample contains fewer than two values.</exception>
    public static HypothesisTestResult<T> TTestWelch<T>(T[] sample1, T[] sample2)
        where T : IFloatingPointIeee754<T>
        => TTestWelch<T>((ReadOnlySpan<T>)sample1, (ReadOnlySpan<T>)sample2);

    /// <summary>
    /// Computes the arithmetic mean of a sample.
    /// </summary>
    private static T Mean<T>(ReadOnlySpan<T> values)
        where T : IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (int i = 0; i < values.Length; i++)
            sum += values[i];
        return sum / T.CreateChecked(values.Length);
    }

    /// <summary>
    /// Computes the unbiased sample variance (divides by n-1) given a precomputed mean.
    /// </summary>
    private static T SampleVariance<T>(ReadOnlySpan<T> values, T mean)
        where T : IFloatingPointIeee754<T>
    {
        T sumSquares = T.Zero;
        for (int i = 0; i < values.Length; i++)
        {
            T diff = values[i] - mean;
            sumSquares += diff * diff;
        }
        return sumSquares / T.CreateChecked(values.Length - 1);
    }
}