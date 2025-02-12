using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.Statistics;

namespace core_module_test;

internal class Statistics_test
{

    public static void go()
    {
        //Span<float> values = stackalloc float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };

        for (int i = 0; i < 10; i++)
        {
            //var values = new float[Random.Shared.Next(50,5000000)];
            var size = Random.Shared.Next(50, 5000000);
            var values = new PinnableArray<double>(size, false);
            values.FillWithRandomNumber();


            $"length : {values.Values.Length}".PrintLine(ConsoleColor.Green);
            var average = values.AsSpan().Variance<double>();
            average.PrintLine();

            $"length : {values.AsSpan().Length}".PrintLine(ConsoleColor.Green);
            average = values.AsSpan().Variance();
            average.PrintLine();

            "----------".PrintLine(ConsoleColor.Red);
        }


    }


    public static void go2()
    {

        for (int i = 0; i < 20; i++)
        {
            var a = new float[1000];
            a.FillWithRandomNumber();

            Vorcyc.Mathematics.Statistics.Basic.CalculateAllStatistics<float>(a).PrintLine();
            //Statistics.CalculateAllStatistics_SIMD<float>(a).PrintLine();
            new string('-', 50).PrintLine(ConsoleColor.Red);

        }

    }


    public static void advanced()
    {
        double[] data = { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };
        Span<double> dataSpan = data;

        // Test Percentile
        double percentileGeneric = dataSpan.Percentile(0.9);
        double percentileNonGeneric = Advanced2.Percentile(data, 0.9);
        Console.WriteLine($"Percentile (Generic): {percentileGeneric}, Percentile (Non-Generic): {percentileNonGeneric}");

        // Test Quartiles
        var quartilesGeneric = dataSpan.Quartiles();
        var quartilesNonGeneric = Advanced2.Quartiles(data);
        Console.WriteLine($"Quartiles (Generic): {quartilesGeneric}, Quartiles (Non-Generic): {quartilesNonGeneric}");

        // Test Skewness
        double skewnessGeneric = dataSpan.Skewness();
        double skewnessNonGeneric = Advanced2.Skewness(data);
        Console.WriteLine($"Skewness (Generic): {skewnessGeneric}, Skewness (Non-Generic): {skewnessNonGeneric}");

        // Test Kurtosis
        double kurtosisGeneric = dataSpan.Kurtosis();
        double kurtosisNonGeneric = Advanced2.Kurtosis(data);
        Console.WriteLine($"Kurtosis (Generic): {kurtosisGeneric}, Kurtosis (Non-Generic): {kurtosisNonGeneric}");

        // Test Confidence Interval
        var confidenceIntervalGeneric = dataSpan.ConfidenceInterval(0.95);
        var confidenceIntervalNonGeneric = Advanced2.ConfidenceInterval(data, 0.95);
        Console.WriteLine($"Confidence Interval (Generic): {confidenceIntervalGeneric}, Confidence Interval (Non-Generic): {confidenceIntervalNonGeneric}");

        // Test T-Test
        double tTestGeneric = dataSpan.TTest(5.5);
        double tTestNonGeneric = Advanced2.TTest(data, 5.5);
        Console.WriteLine($"T-Test (Generic): {tTestGeneric}, T-Test (Non-Generic): {tTestNonGeneric}");

        // Test ANOVA
        var groups = new List<double[]>
            {
                new double[] { 1.0, 2.0, 3.0 },
                new double[] { 4.0, 5.0, 6.0 },
                new double[] { 7.0, 8.0, 9.0 }
            };
        var groupsSpan = groups.Select(g => new ArraySegment<double>(g));
        double anovaGeneric = groupsSpan.Anova();
        double anovaNonGeneric = Advanced2.Anova(groups);
        Console.WriteLine($"ANOVA (Generic): {anovaGeneric}, ANOVA (Non-Generic): {anovaNonGeneric}");

        // Test Chi-Squared Test
        int[] observed = { 10, 20, 30 };
        int[] expected = { 15, 25, 35 };
        Span<double> observedSpan = observed.Select(x => (double)x).ToArray();
        Span<double> expectedSpan = expected.Select(x => (double)x).ToArray();
        double chiSquaredGeneric = observedSpan.ChiSquaredTest(expectedSpan);
        double chiSquaredNonGeneric = Advanced2.ChiSquaredTest(observed, expected);
        Console.WriteLine($"Chi-Squared Test (Generic): {chiSquaredGeneric}, Chi-Squared Test (Non-Generic): {chiSquaredNonGeneric}");

        // Test Mann-Whitney U Test
        double[] sample1 = { 1.0, 2.0, 3.0 };
        double[] sample2 = { 4.0, 5.0, 6.0 };
        Span<double> sample1Span = sample1;
        Span<double> sample2Span = sample2;
        double mannWhitneyGeneric = sample1Span.MannWhitneyUTest(sample2Span);
        double mannWhitneyNonGeneric = Advanced2.MannWhitneyUTest(sample1, sample2);
        Console.WriteLine($"Mann-Whitney U Test (Generic): {mannWhitneyGeneric}, Mann-Whitney U Test (Non-Generic): {mannWhitneyNonGeneric}");
    }



    public static void time_series()
    {
        double[] data = { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };
        Span<double> dataSpan = data;

        // Test Smooth
        double[] smoothGeneric = dataSpan.Smooth(3);
        double[] smoothNonGeneric = TimeSeriesAnalysis2.Smooth(data, 3);
        Console.WriteLine($"Smooth (Generic): {string.Join(", ", smoothGeneric)}, Smooth (Non-Generic): {string.Join(", ", smoothNonGeneric)}");

        // Test Decompose
        var decomposeGeneric = dataSpan.Decompose(3);
        var decomposeNonGeneric = TimeSeriesAnalysis2.Decompose(data, 3);
        Console.WriteLine($"Decompose (Generic): Trend: {string.Join(", ", decomposeGeneric.Trend)}, Seasonal: {string.Join(", ", decomposeGeneric.Seasonal)}, Residual: {string.Join(", ", decomposeGeneric.Residual)}");
        Console.WriteLine($"Decompose (Non-Generic): Trend: {string.Join(", ", decomposeNonGeneric.Trend)}, Seasonal: {string.Join(", ", decomposeNonGeneric.Seasonal)}, Residual: {string.Join(", ", decomposeNonGeneric.Residual)}");

        // Test Forecast
        double[] forecastGeneric = dataSpan.Forecast(5);
        double[] forecastNonGeneric = TimeSeriesAnalysis2.Forecast(data, 5);
        Console.WriteLine($"Forecast (Generic): {string.Join(", ", forecastGeneric)}, Forecast (Non-Generic): {string.Join(", ", forecastNonGeneric)}");

        // Test Autocorrelation
        double[] acfGeneric = dataSpan.Autocorrelation(3);
        double[] acfNonGeneric = TimeSeriesAnalysis2.Autocorrelation(data, 3);
        Console.WriteLine($"Autocorrelation (Generic): {string.Join(", ", acfGeneric)}, Autocorrelation (Non-Generic): {string.Join(", ", acfNonGeneric)}");

        // Test PartialAutocorrelation
        double[] pacfGeneric = dataSpan.PartialAutocorrelation(3);
        double[] pacfNonGeneric = TimeSeriesAnalysis2.PartialAutocorrelation(data, 3);
        Console.WriteLine($"PartialAutocorrelation (Generic): {string.Join(", ", pacfGeneric)}, PartialAutocorrelation (Non-Generic): {string.Join(", ", pacfNonGeneric)}");

        // Test MovingAverage
        double[] movingAverageGeneric = dataSpan.MovingAverage(3);
        double[] movingAverageNonGeneric = TimeSeriesAnalysis2.MovingAverage(data, 3);
        Console.WriteLine($"MovingAverage (Generic): {string.Join(", ", movingAverageGeneric)}, MovingAverage (Non-Generic): {string.Join(", ", movingAverageNonGeneric)}");

        // Test ExponentialSmoothing
        double[] exponentialSmoothingGeneric = dataSpan.ExponentialSmoothing(0.5);
        double[] exponentialSmoothingNonGeneric = TimeSeriesAnalysis2.ExponentialSmoothing(data, 0.5);
        Console.WriteLine($"ExponentialSmoothing (Generic): {string.Join(", ", exponentialSmoothingGeneric)}, ExponentialSmoothing (Non-Generic): {string.Join(", ", exponentialSmoothingNonGeneric)}");

    }



    public static void others()
    {
        // 生成更大的数据集
        int dataSize = 10000;
        float[] dataX = new float[dataSize];
        float[] dataY = new float[dataSize];
        Random random = new Random();

        for (int i = 0; i < dataSize; i++)
        {
            dataX[i] = random.NextSingle() * 100;
            dataY[i] = 2 * dataX[i] + random.NextSingle() * 10; // y = 2x + noise
        }

        Span<float> dataXSpan = dataX;
        Span<float> dataYSpan = dataY;

        // Test Covariance
        float covarianceGeneric = dataXSpan.Covariance(dataYSpan);
        float covarianceNonGeneric = Others2.Covariance(dataX, dataY);
        Console.WriteLine($"Covariance (Generic): {covarianceGeneric}, Covariance (Non-Generic): {covarianceNonGeneric}");

        // Test CorrelationCoefficient
        float correlationGeneric = dataXSpan.CorrelationCoefficient(dataYSpan);
        float correlationNonGeneric = Others2.CorrelationCoefficient(dataX, dataY);
        Console.WriteLine($"CorrelationCoefficient (Generic): {correlationGeneric}, CorrelationCoefficient (Non-Generic): {correlationNonGeneric}");

        // Test LinearRegression
        var regressionGeneric = dataXSpan.LinearRegression(dataYSpan);
        var regressionNonGeneric = Others2.LinearRegression(dataX, dataY);
        Console.WriteLine($"LinearRegression (Generic): Slope: {regressionGeneric.Slope}, Intercept: {regressionGeneric.Intercept}");
        Console.WriteLine($"LinearRegression (Non-Generic): Slope: {regressionNonGeneric.Slope}, Intercept: {regressionNonGeneric.Intercept}");

    }
}