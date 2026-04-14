using Vorcyc.Mathematics.Buffers;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.Statistics;

namespace core_module_test;

internal static class Statistics_Extensions_test
{
    public static void RunAll()
    {
        TestVarianceOptions();
        TestWeightedAndRunningMoments();
        TestHypothesisResults();
        TestNonParametric();
        TestDistributionSampling();
        TestRobust();
        TestTimeSeriesExtended();
        TestTensorStatistics();
        TestPinnableDescriptive();
        Console.WriteLine("Statistics_Extensions_test: all passed.");
    }

    static void TestVarianceOptions()
    {
        double[] data = [2, 4, 4, 4, 5, 5, 7, 9];
        var sample = data.AsSpan().Variance(DescriptiveStatisticsOptions.Default);
        var pop = data.AsSpan().Variance(new DescriptiveStatisticsOptions { VarianceKind = VarianceKind.Population });
        if (Math.Abs(sample.variance - 4.571428571428571) > 1e-6)
            throw new Exception("Sample variance mismatch.");
        if (Math.Abs(pop.variance - 4.0) > 1e-6)
            throw new Exception("Population variance mismatch.");
    }

    static void TestWeightedAndRunningMoments()
    {
        float[] values = [1, 2, 3];
        float[] weights = [1, 1, 2];
        float wmean = Basic.WeightedAverage(values, weights);
        if (Math.Abs(wmean - 2.25f) > 1e-5f)
            throw new Exception("Weighted average mismatch.");

        var moments = new RunningMoments<double>();
        foreach (var v in new[] { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 })
            moments.Push(v);
        if (Math.Abs(moments.Mean - 5.0) > 1e-9)
            throw new Exception("Running mean mismatch.");
        if (Math.Abs(moments.Variance - sampleVariance(8)) > 0.1)
            throw new Exception("Running variance mismatch.");
    }

    static double sampleVariance(int n) => 4.571428571428571;

    static void TestHypothesisResults()
    {
        double[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        var tResult = data.AsSpan().TTestResult(0.0);
        if (tResult.PValue <= 0 || tResult.PValue > 1)
            throw new Exception("T-test p-value out of range.");
        if (!tResult.RejectsNullHypothesisAt(0.05))
            throw new Exception("Expected significant t-test against mean 0.");

        var groups = new List<ArraySegment<double>>
        {
            new([1.0, 2.0, 3.0]),
            new([4.0, 5.0, 6.0]),
            new([7.0, 8.0, 9.0])
        };
        var anova = groups.AnovaResult();
        if (anova.PValue >= 0.05)
            throw new Exception("Expected significant ANOVA.");
    }

    static void TestNonParametric()
    {
        var groups = new List<ArraySegment<double>>
        {
            new([1.0, 2.0, 3.0]),
            new([10.0, 11.0, 12.0])
        };
        var kw = groups.KruskalWallisTestResult();
        if (kw.PValue >= 0.05)
            throw new Exception("Expected significant Kruskal-Wallis.");

        double[] before = [100, 110, 90];
        double[] after = [95, 105, 88];
        var wilcoxon = Advanced.WilcoxonSignedRankTestResult(before, after);
        if (wilcoxon.PValue < 0 || wilcoxon.PValue > 1)
            throw new Exception("Wilcoxon p-value out of range.");
    }

    static void TestDistributionSampling()
    {
        double p = 0.975;
        double z = Distribution.NormalInverseCDF(p);
        if (Math.Abs(z - 1.959963984540054) > 1e-3)
            throw new Exception("Normal inverse CDF mismatch.");

        var samples = new double[1000];
        Distribution.FillNormal(samples.AsSpan(), 0, 1);
        double mean = samples.AsSpan().Average();
        if (Math.Abs(mean) > 0.15)
            throw new Exception("Normal samples mean too far from 0.");
    }

    static void TestRobust()
    {
        double[] data = [1, 2, 3, 4, 100];
        double mad = data.AsSpan().MedianAbsoluteDeviation();
        if (mad <= 0)
            throw new Exception("MAD should be positive.");
        var winsorized = Robust.Winsorize(data, 0.1, 0.9);
        if (winsorized.Max() >= 100)
            throw new Exception("Winsorize should cap outliers.");
    }

    static void TestTimeSeriesExtended()
    {
        double[] series = [1, 2, 3, 4, 5, 6, 7, 8];
        var forecast = series.ForecastHolt(3, 0.8, 0.2);
        if (forecast.Length != 3 || forecast[^1] <= forecast[0])
            throw new Exception("Holt forecast should be increasing for this series.");

        var rolling = series.RollingMean(3);
        if (rolling.Length != series.Length)
            throw new Exception("Rolling mean length mismatch.");
    }

    static void TestTensorStatistics()
    {
        var tensor = new Tensor4D<float>(2, 2, 2, 1);
        for (int i = 0; i < tensor.Values.Length; i++)
            tensor.Values[i] = i + 1;

        var means = TensorStatistics.MeanAlongAxis(tensor, TensorStatistics.Tensor4DAxis.Dim3);
        if (means.Length != 2 * 2 * 2)
            throw new Exception("MeanAlongAxis outer size mismatch.");
    }

    static void TestPinnableDescriptive()
    {
        using var array = new PinnableArray<double>([1.0, 2.0, 3.0, 4.0, 5.0], false);
        var stats = array.DescriptiveStatistics();
        if (Math.Abs(stats.Mean - 3.0) > 1e-9)
            throw new Exception("Pinnable descriptive mean mismatch.");
    }
}
