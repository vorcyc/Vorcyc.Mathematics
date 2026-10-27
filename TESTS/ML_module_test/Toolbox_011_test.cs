using Vorcyc.Mathematics.MachineLearning;
using Vorcyc.Mathematics.MachineLearning.Classfication;
using Vorcyc.Mathematics.Statistics;

namespace ML_module_test;

internal static class Toolbox_011_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing 0.11.0 classifiers, anomaly detectors, and time-series models...");
        Classifiers();
        AnomalyDetectors();
        HoltAndWinters();
        Arima();
        Garch();
        Var();
        if (_failures != 0)
            throw new InvalidOperationException($"Toolbox 0.11.0: {_failures} assertion(s) failed.");
        Console.WriteLine("Toolbox_011_test: PASS");
    }

    static void Classifiers()
    {
        var (x, y) = Blobs();
        var ecoc = new ErrorCorrectingOutputCode<double>();
        ecoc.Fit(x, y);
        Expect(Accuracy(ecoc, x, y) > 0.9, "ecoc accuracy");

        var bayes = new KernelNaiveBayes<double>();
        bayes.Fit(x, y);
        Expect(Accuracy(bayes, x, y) > 0.9, "kernel nb accuracy");

        var network = new NeuralNetworkClassifier<double>(hiddenUnits: 8, epochs: 500, seed: 3);
        network.Fit(x, y);
        Expect(Accuracy(network, x, y) > 0.9, "neural net accuracy");

        var validated = new CrossValidatedClassifier<double>(() => new GaussianNaiveBayes<double>(), folds: 4, seed: 1);
        validated.Fit(x, y);
        Expect(validated.Validation != null && validated.Validation.MeanAccuracy > 0.8, "cv accuracy");
        Expect(validated.OutOfFoldPredictions != null && validated.OutOfFoldPredictions.Length == y.Length, "oof length");
        Expect(validated.Predict([0.1, 0.1]) == 0, $"cv predict {validated.Predict([0.1, 0.1])}");
    }

    static void AnomalyDetectors()
    {
        var rng = new Random(5);
        var samples = new double[31, 2];
        for (int i = 0; i < 30; i++)
        {
            samples[i, 0] = rng.NextDouble() - 0.5;
            samples[i, 1] = rng.NextDouble() - 0.5;
        }

        samples[30, 0] = 8;
        samples[30, 1] = 8;
        var lof = new LocalOutlierFactor<double>(neighbors: 5);
        lof.Fit(samples);
        double inlier = double.CreateChecked(lof.Score([0.0, 0.0]));
        double outlier = double.CreateChecked(lof.Score([8.0, 8.0]));
        Expect(outlier > inlier * 1.5, $"lof inlier {inlier} outlier {outlier}");

        var svm = new OneClassSvm<double>(nu: 0.05, iterations: 80);
        var cloud = new double[30, 2];
        for (int i = 0; i < 30; i++)
        {
            cloud[i, 0] = samples[i, 0];
            cloud[i, 1] = samples[i, 1];
        }

        svm.Fit(cloud);
        double inside = double.CreateChecked(svm.Score([0.0, 0.0]));
        double outside = double.CreateChecked(svm.Score([8.0, 8.0]));
        Expect(outside > inside, $"ocsvm inside {inside} outside {outside}");
        Expect(svm.Predict(new double[,] { { 8, 8 } }, threshold: 0)[0] == 1, "ocsvm predict");
    }

    static void HoltAndWinters()
    {
        var line = new double[20];
        for (int i = 0; i < line.Length; i++)
            line[i] = i;
        var holt = HoltModel<double>.Estimate(line);
        var forecast = holt.Forecast(3);
        Expect(Math.Abs(forecast[0] - 20) < 1e-8, $"holt h1 {forecast[0]}");
        Expect(Math.Abs(forecast[1] - 21) < 1e-8, $"holt h2 {forecast[1]}");
        Expect(holt.Alpha > 0 && holt.Alpha < 1, "holt alpha");

        int period = 4;
        var seasonal = new double[40];
        double[] shape = [0.7, 1.3, 0.9, 1.1];
        for (int t = 0; t < seasonal.Length; t++)
            seasonal[t] = (8 + 0.15 * t) * shape[t % period];
        var (fitted, seasonalForecast) = seasonal.AsSpan().HoltWintersMultiplicative(period, 4, 0.4, 0.2, 0.3);
        Expect(fitted.Length == seasonal.Length && seasonalForecast.Length == 4, "hw lengths");
        Expect(seasonalForecast.All(value => value > 0 && double.IsFinite(value)), "hw forecast positive");
        var estimated = HoltWintersModel<double>.Estimate(seasonal, period, HoltWintersSeasonality.Multiplicative);
        Expect(estimated.Forecast(2).Length == 2, "hw model forecast");
    }

    static void Arima()
    {
        var rng = new Random(6);
        var walk = new double[30];
        for (int i = 1; i < walk.Length; i++)
            walk[i] = walk[i - 1] + 0.2 * Gaussian(rng);
        var integrated = ArimaModel<double>.Estimate(walk, 0, 1, 0, includeConstant: false);
        var flat = integrated.Forecast(3);
        Expect(Math.Abs(flat[0] - walk[^1]) < 1e-6 && Math.Abs(flat[2] - walk[^1]) < 1e-6, $"arima random walk {flat[0]}");

        var ar = new double[2500];
        for (int i = 1; i < ar.Length; i++)
            ar[i] = 0.7 * ar[i - 1] + Gaussian(rng);
        var ar1 = ArimaModel<double>.Estimate(ar, 1, 0, 0, includeConstant: false);
        Expect(Math.Abs(ar1.Ar[0] - 0.7) < 0.08, $"ar1 {ar1.Ar[0]}");

        var seasonal = new double[800];
        for (int i = 4; i < seasonal.Length; i++)
            seasonal[i] = 0.55 * seasonal[i - 4] + 0.4 * Gaussian(rng);
        var sar = ArimaModel<double>.Estimate(seasonal, 0, 0, 0, seasonLength: 4, seasonalP: 1, includeConstant: false);
        Expect(Math.Abs(sar.SeasonalAr[0] - 0.55) < 0.1, $"seasonal ar {sar.SeasonalAr[0]}");
    }

    static void Garch()
    {
        var rng = new Random(7);
        const double omega = 0.05;
        const double alpha = 0.15;
        const double beta = 0.75;
        int n = 3000;
        var series = new double[n];
        double variance = omega / (1 - alpha - beta);
        for (int t = 1; t < n; t++)
        {
            double shock = Math.Sqrt(variance) * Gaussian(rng);
            series[t] = shock;
            variance = omega + alpha * shock * shock + beta * variance;
        }

        var model = GarchModel<double>.Estimate(series, garchOrder: 1, archOrder: 1);
        Expect(model.Omega > 0, "garch omega");
        Expect(model.Alpha[0] > 0 && model.Beta[0] > 0 && model.Alpha[0] + model.Beta[0] < 1, $"garch persistence {model.Alpha[0]} {model.Beta[0]}");
        Expect(Math.Abs(model.Alpha[0] - alpha) < 0.12, $"garch alpha {model.Alpha[0]}");
        Expect(Math.Abs(model.Beta[0] - beta) < 0.2, $"garch beta {model.Beta[0]}");
        Expect(model.ForecastVariance(3).All(value => value > 0), "garch forecast");
    }

    static void Var()
    {
        var rng = new Random(8);
        int n = 2000;
        var series = new double[n, 2];
        for (int t = 1; t < n; t++)
        {
            series[t, 0] = 0.5 * series[t - 1, 0] + 0.2 * series[t - 1, 1] + 0.3 * Gaussian(rng);
            series[t, 1] = -0.3 * series[t - 1, 0] + 0.4 * series[t - 1, 1] + 0.3 * Gaussian(rng);
        }

        var model = VarModel<double>.Estimate(series, order: 1, includeConstant: true);
        Expect(Math.Abs(model.Coefficients[0][0, 0] - 0.5) < 0.08, $"var a00 {model.Coefficients[0][0, 0]}");
        Expect(Math.Abs(model.Coefficients[0][0, 1] - 0.2) < 0.08, $"var a01 {model.Coefficients[0][0, 1]}");
        Expect(Math.Abs(model.Coefficients[0][1, 0] + 0.3) < 0.08, $"var a10 {model.Coefficients[0][1, 0]}");
        Expect(Math.Abs(model.Coefficients[0][1, 1] - 0.4) < 0.08, $"var a11 {model.Coefficients[0][1, 1]}");
        Expect(model.Forecast(2).GetLength(0) == 2, "var forecast rows");
    }

    static (double[,] X, int[] Y) Blobs()
    {
        var rng = new Random(9);
        var x = new double[60, 2];
        var y = new int[60];
        double[,] centers = { { 0, 0 }, { 5, 0 }, { 0, 5 } };
        for (int i = 0; i < 60; i++)
        {
            int label = i / 20;
            y[i] = label;
            x[i, 0] = centers[label, 0] + 0.15 * Gaussian(rng);
            x[i, 1] = centers[label, 1] + 0.15 * Gaussian(rng);
        }

        return (x, y);
    }

    static double Accuracy(IClassifier<double> model, double[,] x, int[] y)
    {
        int correct = 0;
        for (int i = 0; i < y.Length; i++)
        {
            if (model.Predict([x[i, 0], x[i, 1]]) == y[i])
                correct++;
        }

        return correct / (double)y.Length;
    }

    static double Gaussian(Random random)
    {
        double u1 = 1 - random.NextDouble();
        double u2 = 1 - random.NextDouble();
        return Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(2 * Math.PI * u2);
    }

    static void Expect(bool condition, string message)
    {
        if (condition)
            return;
        _failures++;
        Console.WriteLine("FAIL: " + message);
    }
}
