using Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;
using Vorcyc.Mathematics.Statistics;

namespace ML_module_test;

/// <summary>0.10.18: Holt terminal state, ForecastHolt recurrence, PCA out-of-sample Transform.</summary>
internal static class HoltPca_018_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing Holt/PCA 0.10.18 APIs...");

        AssertHoltStateAndForecast();
        AssertHoltWintersState();
        AssertPcaOutOfSample();

        if (_failures != 0)
            throw new InvalidOperationException($"Holt/PCA 0.10.18: {_failures} assertion(s) failed.");

        Console.WriteLine("HoltPca_018_test: PASS");
    }

    static void AssertHoltStateAndForecast()
    {
        double[] y = [1, 2, 3, 4, 5, 6, 7, 8];
        const double alpha = 0.8;
        const double beta = 0.3;

        var (fitted, state) = y.AsSpan().HoltWithState(alpha, beta);
        Expect("fitted len", fitted.Length == y.Length);

        double level = y[0];
        double trend = y[1] - y[0];
        for (int i = 1; i < y.Length; i++)
        {
            double prev = level;
            level = alpha * y[i] + (1 - alpha) * (level + trend);
            trend = beta * (level - prev) + (1 - beta) * trend;
        }

        Expect("last level", Math.Abs(state.LastLevel - level) < 1e-12, $"L={state.LastLevel} exp={level}");
        Expect("last trend", Math.Abs(state.LastTrend - trend) < 1e-12, $"T={state.LastTrend} exp={trend}");

        var forecast = y.AsSpan().ForecastHolt(3, alpha, beta);
        Expect("h1", Math.Abs(forecast[0] - (level + trend)) < 1e-12);
        Expect("h2", Math.Abs(forecast[1] - (level + 2 * trend)) < 1e-12);
        Expect("h3", Math.Abs(forecast[2] - (level + 3 * trend)) < 1e-12);
    }

    static void AssertHoltWintersState()
    {
        double[] y = [10, 12, 9, 11, 13, 15, 12, 14];
        var (fitted, forecast, state) = y.AsSpan().HoltWintersWithState(4, 3, 0.4, 0.1, 0.2);
        Expect("hw fitted", fitted.Length == y.Length);
        Expect("hw forecast", forecast.Length == 3);
        Expect("season len", state.LastSeasonal.Length == 4);
        Expect("h1 uses S0", Math.Abs(forecast[0] - (state.LastLevel + state.LastTrend + state.LastSeasonal[0])) < 1e-12);
        Expect("h2 uses S1", Math.Abs(forecast[1] - (state.LastLevel + 2 * state.LastTrend + state.LastSeasonal[1])) < 1e-12);
    }

    static void AssertPcaOutOfSample()
    {
        double[,] train =
        {
            { 2.5, 2.4 },
            { 0.5, 0.7 },
            { 2.2, 2.9 },
            { 1.9, 2.2 },
            { 3.1, 3.0 },
            { 2.3, 2.7 },
            { 2.0, 1.6 },
            { 1.0, 1.1 },
            { 1.5, 1.6 },
            { 1.1, 0.9 },
        };

        var pca = new PCA<double>(train);
        var inSample = pca.Transform();
        var oosFull = pca.Transform(train);
        Expect("oos rows", oosFull.GetLength(0) == train.GetLength(0));
        Expect("oos cols", oosFull.GetLength(1) == train.GetLength(1));

        double max = 0;
        for (int i = 0; i < inSample.GetLength(0); i++)
        {
            for (int j = 0; j < inSample.GetLength(1); j++)
                max = Math.Max(max, Math.Abs(inSample[i, j] - oosFull[i, j]));
        }
        Expect("oos matches in-sample", max < 1e-10, $"maxΔ={max}");

        var one = pca.Transform(train, components: 1);
        Expect("k=1 cols", one.GetLength(1) == 1);
        Expect("k=1 first col", Math.Abs(one[0, 0] - oosFull[0, 0]) < 1e-12);
        Expect("means len", pca.Means.Length == 2);
        Expect("eigs desc", pca.Eigenvalues[0] >= pca.Eigenvalues[1]);
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }
}
