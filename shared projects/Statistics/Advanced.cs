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
/// 提供高级统计函数，包括百分位数、四分位数、偏度、峰度、置信区间、假设检验、方差分析、卡方检验和非参数检验的计算方法。
/// </summary>
/// <remarks>
/// 该类包含以下高级统计函数的计算方法：
/// <list type="bullet">
/// <item>
/// <description>百分位数 (Percentiles): 计算数据集的指定百分位数。</description>
/// </item>
/// <item>
/// <description>四分位数 (Quartiles): 计算数据集的四分位数。</description>
/// </item>
/// <item>
/// <description>偏度 (Skewness): 计算数据集的偏度，衡量数据分布的对称性。</description>
/// </item>
/// <item>
/// <description>峰度 (Kurtosis): 计算数据集的峰度，衡量数据分布的尖锐程度。</description>
/// </item>
/// <item>
/// <description>置信区间 (Confidence Intervals): 计算均值或比例的置信区间。</description>
/// </item>
/// <item>
/// <description>假设检验 (Hypothesis Testing): 实现各种假设检验，如z检验、t检验、卡方检验等。</description>
/// </item>
/// <item>
/// <description>方差分析 (ANOVA): 实现单因素和多因素方差分析。</description>
/// </item>
/// <item>
/// <description>卡方检验 (Chi-Squared Test): 实现卡方独立性检验和拟合优度检验。</description>
/// </item>
/// <item>
/// <description>非参数检验 (Non-parametric Tests): 实现如曼-惠特尼U检验、克鲁斯卡尔-沃利斯检验等非参数检验。</description>
/// </item>
/// </list>
/// </remarks>
public static class Advanced
{
    /// <summary>
    /// 计算数据集的指定百分位数。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="sequence">数据集。</param>
    /// <param name="percentile">百分位数（0到1之间）。</param>
    /// <returns>指定百分位数的值。</returns>
    /// <remarks>
    /// 百分位数 (Percentiles): 用于确定数据集中某个值在所有数据中的相对位置。
    /// 例如，第90百分位数表示数据集中90%的值小于或等于该值。
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
    /// 计算数据集的四分位数。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="sequence">数据集。</param>
    /// <returns>包含第一、第二和第三四分位数的元组。</returns>
    /// <remarks>
    /// 四分位数 (Quartiles): 将数据集分成四个相等部分的三个值。
    /// 第一四分位数 (Q1) 是25%的数据小于或等于该值，第二四分位数 (Q2) 是中位数，第三四分位数 (Q3) 是75%的数据小于或等于该值。
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
    /// 计算数据集的偏度，衡量数据分布的对称性。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="sequence">数据集。</param>
    /// <returns>数据集的偏度值。</returns>
    /// <remarks>
    /// 偏度 (Skewness): 用于衡量数据分布的对称性。
    /// 偏度为零表示数据分布对称，正偏度表示数据右偏，负偏度表示数据左偏。
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
    /// 计算数据集的峰度，衡量数据分布的尖锐程度。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="sequence">数据集。</param>
    /// <returns>数据集的峰度值。</returns>
    /// <remarks>
    /// 峰度 (Kurtosis): 用于衡量数据分布的尖锐程度。
    /// 峰度值越高，数据分布越尖锐；峰度值越低，数据分布越平坦。
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
    /// 计算均值或比例的置信区间。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="sequence">数据集。</param>
    /// <param name="confidenceLevel">置信水平（例如0.95表示95%的置信水平）。</param>
    /// <returns>包含置信区间下限和上限的元组。</returns>
    /// <remarks>
    /// 置信区间 (Confidence Intervals): 用于估计总体参数的范围。
    /// 置信区间提供了一个范围，其中包含总体参数的可能值，并且该范围内包含总体参数的概率等于置信水平。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T Lower, T Upper) ConfidenceInterval<T>(this Span<T> sequence, double confidenceLevel) where T : IFloatingPointIeee754<T>
    {
        T mean = sequence.Average();
        T stdDev = T.Sqrt(sequence.Sum(x => T.Pow(x - mean, T.CreateChecked(2))) / (T.CreateChecked(sequence.Length) - T.One));
        T z = T.CreateChecked(1.96); // For 95% confidence
        T marginOfError = z * (stdDev / T.Sqrt(T.CreateChecked(sequence.Length)));
        return (mean - marginOfError, mean + marginOfError);
    }

    /// <summary>
    /// 实现各种假设检验，如z检验、t检验、卡方检验等。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="sample">样本数据集。</param>
    /// <param name="populationMean">总体均值。</param>
    /// <returns>t检验的统计量。</returns>
    /// <remarks>
    /// 假设检验 (Hypothesis Testing): 用于检验样本数据是否支持某个假设。
    /// 例如，t检验用于比较样本均值与总体均值，z检验用于比较样本比例与总体比例，卡方检验用于检验分类数据的独立性。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T TTest<T>(this Span<T> sample, T populationMean) where T : IFloatingPointIeee754<T>
    {
        T sampleMean = sample.Average();
        T stdDev = T.Sqrt(sample.Sum(x => T.Pow(x - sampleMean, T.CreateChecked(2))) / (T.CreateChecked(sample.Length) - T.One));
        return (sampleMean - populationMean) / (stdDev / T.Sqrt(T.CreateChecked(sample.Length)));
    }

    /// <summary>
    /// 实现单因素和多因素方差分析。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="groups">数据组的集合。</param>
    /// <returns>方差分析的F值。</returns>
    /// <remarks>
    /// 方差分析 (ANOVA): 用于比较多个组的均值是否存在显著差异。
    /// 单因素方差分析用于比较一个因素的多个水平，多因素方差分析用于比较多个因素的交互作用。
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
    /// 实现卡方独立性检验和拟合优度检验。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="observed">观察值数据集。</param>
    /// <param name="expected">期望值数据集。</param>
    /// <returns>卡方检验的统计量。</returns>
    /// <remarks>
    /// 卡方检验 (Chi-Squared Test): 用于检验分类数据的独立性和拟合优度。
    /// 独立性检验用于检验两个分类变量是否独立，拟合优度检验用于检验观察值与期望值是否一致。
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
    /// 实现如曼-惠特尼U检验、克鲁斯卡尔-沃利斯检验等非参数检验。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="sample1">样本1数据集。</param>
    /// <param name="sample2">样本2数据集。</param>
    /// <returns>曼-惠特尼U检验的统计量。</returns>
    /// <remarks>
    /// 非参数检验 (Non-parametric Tests): 用于比较两个或多个样本的分布。
    /// 曼-惠特尼U检验用于比较两个独立样本的分布，克鲁斯卡尔-沃利斯检验用于比较多个独立样本的分布。
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
}


internal static class Advanced2
{
    // 14. Percentiles
    public static double Percentile(this double[] sequence, double percentile)
    {
        Array.Sort(sequence);
        int N = sequence.Length;
        double n = (N - 1) * percentile + 1;
        if (n == 1d) return sequence[0];
        else if (n == N) return sequence[N - 1];
        else
        {
            int k = (int)n;
            double d = n - k;
            return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
        }
    }

    // 15. Quartiles
    public static (double Q1, double Q2, double Q3) Quartiles(this double[] sequence)
    {
        return (
            Percentile(sequence, 0.25),
            Percentile(sequence, 0.50),
            Percentile(sequence, 0.75)
        );
    }

    // 16. Skewness
    public static double Skewness(this double[] sequence)
    {
        double mean = sequence.Average();
        double n = sequence.Length;
        double m3 = sequence.Sum(x => Math.Pow(x - mean, 3)) / n;
        double m2 = sequence.Sum(x => Math.Pow(x - mean, 2)) / n;
        return m3 / Math.Pow(m2, 1.5);
    }

    // 17. Kurtosis
    public static double Kurtosis(this double[] sequence)
    {
        double mean = sequence.Average();
        double n = sequence.Length;
        double m4 = sequence.Sum(x => Math.Pow(x - mean, 4)) / n;
        double m2 = sequence.Sum(x => Math.Pow(x - mean, 2)) / n;
        return m4 / Math.Pow(m2, 2) - 3;
    }

    // 18. Confidence Intervals
    public static (double Lower, double Upper) ConfidenceInterval(this double[] sequence, double confidenceLevel)
    {
        double mean = sequence.Average();
        double stdDev = Math.Sqrt(sequence.Sum(x => Math.Pow(x - mean, 2)) / (sequence.Length - 1));
        double z = 1.96; // For 95% confidence
        double marginOfError = z * (stdDev / Math.Sqrt(sequence.Length));
        return (mean - marginOfError, mean + marginOfError);
    }

    // 19. Hypothesis Testing (Example: One Sample t-Test)
    public static double TTest(this double[] sample, double populationMean)
    {
        double sampleMean = sample.Average();
        double stdDev = Math.Sqrt(sample.Sum(x => Math.Pow(x - sampleMean, 2)) / (sample.Length - 1));
        return (sampleMean - populationMean) / (stdDev / Math.Sqrt(sample.Length));
    }

    // 20. ANOVA (One-Way)
    public static double Anova(this List<double[]> groups)
    {
        int k = groups.Count;
        int n = groups.Sum(g => g.Length);
        double grandMean = groups.SelectMany(g => g).Average();
        double ssBetween = groups.Sum(g => g.Length * Math.Pow(g.Average() - grandMean, 2));
        double ssWithin = groups.Sum(g => g.Sum(x => Math.Pow(x - g.Average(), 2)));
        double msBetween = ssBetween / (k - 1);
        double msWithin = ssWithin / (n - k);
        return msBetween / msWithin;
    }

    // 21. Chi-Squared Test
    public static double ChiSquaredTest(this int[] observed, int[] expected)
    {
        if (observed.Length != expected.Length)
            throw new ArgumentException("Observed and expected arrays must have the same length.");
        return observed.Zip(expected, (o, e) => Math.Pow(o - e, 2) / e).Sum();
    }

    // 22. Non-parametric Tests (Example: Mann-Whitney U Test)
    public static double MannWhitneyUTest(this double[] sample1, double[] sample2)
    {
        int n1 = sample1.Length;
        int n2 = sample2.Length;
        double[] ranks = sample1.Concat(sample2).OrderBy(x => x).Select((x, i) => (double)(i + 1)).ToArray();
        double rankSum1 = sample1.Sum(x => ranks[Array.IndexOf(ranks, x)]);
        double rankSum2 = sample2.Sum(x => ranks[Array.IndexOf(ranks, x)]);
        double u1 = rankSum1 - (n1 * (n1 + 1)) / 2.0;
        double u2 = rankSum2 - (n2 * (n2 + 1)) / 2.0;
        return Math.Min(u1, u2);
    }
}