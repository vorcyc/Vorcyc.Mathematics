using Vorcyc.Mathematics.MachineLearning;
using Vorcyc.Mathematics.MachineLearning.CurveFitting;

namespace ML_module_test;

public static class GprAndIsolationForest_test
{
    public static void Go()
    {
        TestGaussianProcessPredictStd();
        TestIsolationForest();
    }

    static void TestGaussianProcessPredictStd()
    {
        Console.WriteLine("Testing GPR PredictStd / PredictVariance...");
        double[] xData = { 0, 1, 2, 3, 4 };
        double[] yData = xData.Select(Math.Sin).ToArray();

        var result = CurveFitter<double>.GaussianProcess(
            xData, yData, lengthScale: 1.0, signalVariance: 1.0, noiseVariance: 0.01);

        double mean = result.PredictMean(1.5);
        double std = result.PredictStd(1.5);
        double variance = result.PredictVariance(1.5);
        TestAssert.True(std >= 0, "PredictStd must be non-negative");
        TestAssert.InRange(Math.Abs(variance - std * std), 0, 1e-9, "Std^2 should match variance");
        TestAssert.True(Math.Abs(mean - Math.Sin(1.5)) < 0.2, $"Mean at 1.5={mean}");

        double stdNear = result.PredictStd(2.0);
        double stdFar = result.PredictStd(20.0);
        TestAssert.True(stdFar >= stdNear, $"Far std ({stdFar}) should be >= near ({stdNear})");
        Console.WriteLine($"GPR mean={mean:F4}, std={std:F4}, farStd={stdFar:F4}");
    }

    static void TestIsolationForest()
    {
        Console.WriteLine("Testing IsolationForest...");
        // Dense normal cluster around origin + a few far outliers
        int nNormal = 80;
        var x = new double[nNormal + 4, 2];
        var rng = new Random(7);
        for (int i = 0; i < nNormal; i++)
        {
            x[i, 0] = rng.NextDouble() * 0.4 - 0.2;
            x[i, 1] = rng.NextDouble() * 0.4 - 0.2;
        }
        x[nNormal, 0] = 5; x[nNormal, 1] = 5;
        x[nNormal + 1, 0] = -6; x[nNormal + 1, 1] = 4;
        x[nNormal + 2, 0] = 7; x[nNormal + 2, 1] = -5;
        x[nNormal + 3, 0] = -4; x[nNormal + 3, 1] = -7;

        var forest = new IsolationForest<double>(numTrees: 50, subsampleSize: 64, seed: 11);
        TestAssert.True(forest.Task == MachineLearningTask.AnomalyDetection, "Task should be AnomalyDetection");
        forest.Fit(x);

        var scores = forest.Score(x);
        double meanNormal = scores.Take(nNormal).Average();
        double meanOutlier = scores.Skip(nNormal).Average();
        TestAssert.True(meanOutlier > meanNormal,
            $"Outlier mean score ({meanOutlier:F3}) should exceed normal ({meanNormal:F3})");

        double threshold = (meanNormal + meanOutlier) * 0.5;
        var labels = forest.Predict(x, threshold);
        int flaggedOutliers = labels.Skip(nNormal).Count(l => l == 1);
        TestAssert.True(flaggedOutliers >= 3, $"Expected most outliers flagged, got {flaggedOutliers}");
        Console.WriteLine($"IsolationForest normal={meanNormal:F3}, outlier={meanOutlier:F3}, flagged={flaggedOutliers}/4");
    }
}
