using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

public static partial class Advanced
{
    /// <summary>
    /// Kruskal-Wallis H test for independent samples (one-way).
    /// </summary>
    public static HypothesisTestResult<T> KruskalWallisTestResult<T>(this IEnumerable<ArraySegment<T>> groups)
        where T : IFloatingPointIeee754<T>
    {
        var groupList = groups.ToList();
        if (groupList.Count < 2)
            throw new ArgumentException("At least two groups are required.");

        int total = groupList.Sum(g => g.Count);
        var allValues = new double[total];
        var groupIndex = new int[total];
        int offset = 0;
        for (int g = 0; g < groupList.Count; g++)
        {
            foreach (var value in groupList[g])
            {
                allValues[offset] = double.CreateChecked(value);
                groupIndex[offset] = g;
                offset++;
            }
        }

        var ranks = StatisticalMath.AssignRanks(allValues, groupIndex, groupList.Count);
        double[] rankSums = new double[groupList.Count];
        int[] counts = new int[groupList.Count];
        offset = 0;
        for (int g = 0; g < groupList.Count; g++)
        {
            counts[g] = groupList[g].Count;
            for (int i = 0; i < counts[g]; i++)
                rankSums[g] += ranks[offset++];
        }

        double h = 0;
        for (int g = 0; g < groupList.Count; g++)
            h += rankSums[g] * rankSums[g] / counts[g];

        h = 12.0 / (total * (total + 1)) * h - 3.0 * (total + 1);
        double p = StatisticalMath.KruskalWallisPValue(h, groupList.Count);
        int df = groupList.Count - 1;

        return new HypothesisTestResult<T>(
            T.CreateChecked(h),
            T.CreateChecked(p),
            df);
    }

    /// <summary>
    /// Wilcoxon signed-rank test for paired samples.
    /// </summary>
    public static HypothesisTestResult<T> WilcoxonSignedRankTestResult<T>(Span<T> sample1, Span<T> sample2)
        where T : IFloatingPointIeee754<T>
    {
        if (sample1.Length != sample2.Length || sample1.IsEmpty)
            throw new ArgumentException("Paired samples must have the same non-zero length.");

        var diffs = new List<(double Abs, double Signed)>();
        for (int i = 0; i < sample1.Length; i++)
        {
            double d = double.CreateChecked(sample1[i] - sample2[i]);
            if (d != 0)
                diffs.Add((Math.Abs(d), d));
        }

        int n = diffs.Count;
        if (n == 0)
            return new HypothesisTestResult<T>(T.Zero, T.One, 0);

        diffs.Sort((a, b) => a.Abs.CompareTo(b.Abs));

        double wPlus = 0;
        int i0 = 0;
        while (i0 < n)
        {
            int i1 = i0;
            while (i1 + 1 < n && diffs[i1 + 1].Abs == diffs[i0].Abs)
                i1++;

            double avgRank = 0;
            for (int k = i0; k <= i1; k++)
                avgRank += k + 1;
            avgRank /= i1 - i0 + 1;

            for (int k = i0; k <= i1; k++)
            {
                if (diffs[k].Signed > 0)
                    wPlus += avgRank;
            }

            i0 = i1 + 1;
        }

        double p = StatisticalMath.WilcoxonPValue(wPlus, n);
        return new HypothesisTestResult<T>(
            T.CreateChecked(wPlus),
            T.CreateChecked(p),
            n);
    }
}
