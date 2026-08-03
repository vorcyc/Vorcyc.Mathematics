using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.MachineLearning;
using Vorcyc.Mathematics.MachineLearning.Classfication;
using Vorcyc.Mathematics.MachineLearning.Preprocessing;
using Vorcyc.Mathematics.MachineLearning.Regression;
using Vorcyc.Mathematics.MachineLearning.Serialization;

namespace ML_module_test;

public static class InfrastructureSuite_test
{
    public static void Go()
    {
        var (x, y) = MLTestData.CreateThreeBlobDataset(pointsPerClass: 5);
        TestPipeline(x, y);
        TestOneHotAndLabelEncoder();
        TestGridSearch(x, y);
        TestOneVsRest(x, y);
        TestModelPersistence(x, y);
        TestRegressionNumerics();
        TestLuSolve();
        TestRegressionPipeline();
        TestRegressionCrossValidation();
    }

    static void TestPipeline(double[,] x, int[] y)
    {
        Console.WriteLine("Testing ClassificationPipeline...");
        var (xTrain, yTrain, xTest, yTest) = DataSplit.TrainTestSplit(x, y, 0.25, seed: 1);

        IClassifier<double> classifier = new SoftmaxRegression<double>(learningRate: 0.1, epochs: 2000);
        var pipeline = new ClassificationPipeline<double>()
            .AddPreprocessor(new StandardScaler<double>())
            .SetClassifier(classifier);
        pipeline.Fit(xTrain, yTrain);

        int[] preds = pipeline.PredictBatch(xTest);

        double accuracy = EvaluationMetrics.Accuracy(yTest, preds);
        TestAssert.True(accuracy >= 0.8, $"Pipeline accuracy {accuracy:P1}");
        Console.WriteLine($"Pipeline accuracy: {accuracy:P1}");
    }

    static void TestOneHotAndLabelEncoder()
    {
        Console.WriteLine("Testing LabelEncoder + OneHotEncoder...");
        string[] labels = ["cat", "dog", "cat", "fish"];
        var labelEncoder = new LabelEncoder();
        labelEncoder.Fit(labels);
        TestAssert.Equal(0, labelEncoder.Transform("cat"));
        TestAssert.Equal("fish", labelEncoder.InverseTransform(2));

        string[,] features =
        {
            { "red", "S" },
            { "blue", "M" },
            { "red", "L" }
        };
        var encoder = new OneHotEncoder();
        encoder.Fit(features);
        var encoded = encoder.Transform(features);
        TestAssert.Equal(1.0, encoded[0, 1]); // "red" is second in sorted [blue, red]
        TestAssert.Equal(1.0, encoded[2, 2]); // "L" in second feature block
        Console.WriteLine($"OneHot output shape: {encoded.GetLength(0)}x{encoded.GetLength(1)}");
    }

    static void TestGridSearch(double[,] x, int[] y)
    {
        Console.WriteLine("Testing GridSearch...");
        var candidates = new[]
        {
            new { Epochs = 1000, Lr = 0.05 },
            new { Epochs = 2000, Lr = 0.10 }
        };

        var result = GridSearch.SearchClassifier(
            x,
            y,
            candidates,
            opt => new SoftmaxRegression<double>(learningRate: (double)opt.Lr, epochs: opt.Epochs),
            folds: 3,
            seed: 5);

        TestAssert.True(result.BestScore.MeanMacroF1 > 0.8, "GridSearch macro-F1 too low");
        Console.WriteLine($"Best epochs={result.BestOptions.Epochs}, lr={result.BestOptions.Lr}");
        Console.WriteLine($"Best macro-F1: {result.BestScore.MeanMacroF1:F3}");
    }

    static void TestOneVsRest(double[,] x, int[] y)
    {
        Console.WriteLine("Testing OneVsRestClassifier...");
        var (xTrain, yTrain, xTest, yTest) = DataSplit.TrainTestSplit(x, y, 0.25, seed: 2);
        var model = new OneVsRestClassifier<double>(learningRate: 0.1, epochs: 2500);
        model.Fit(xTrain, yTrain);

        var preds = new int[yTest.Length];
        for (int i = 0; i < yTest.Length; i++)
            preds[i] = model.Predict(MLTestData.GetRow(xTest, i));

        double accuracy = EvaluationMetrics.Accuracy(yTest, preds);
        TestAssert.True(accuracy >= 0.8, $"OvR accuracy {accuracy:P1}");
        Console.WriteLine($"OvR accuracy: {accuracy:P1}");
    }

    static void TestModelPersistence(double[,] x, int[] y)
    {
        Console.WriteLine("Testing ModelJsonPersistence...");
        string dir = Path.Combine(Path.GetTempPath(), "vorcyc_ml_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var scalerPath = Path.Combine(dir, "scaler.json");
            var chainPath = Path.Combine(dir, "scaler_chain.json");
            var modelPath = Path.Combine(dir, "softmax.json");
            var knnPath = Path.Combine(dir, "knn.json");
            var rfPath = Path.Combine(dir, "rf.json");
            var svmPath = Path.Combine(dir, "svm.json");

            var scaler = new StandardScaler<double>();
            scaler.Fit(x);
            ModelJsonPersistence.SaveStandardScaler(scaler, scalerPath);
            var loadedScaler = ModelJsonPersistence.LoadStandardScaler(scalerPath);

            // Second scaler on already-scaled data forms a chain (identity-ish but validates pipeline).
            var xScaledOnce = loadedScaler.Transform(x);
            var scaler2 = new StandardScaler<double>();
            scaler2.Fit(xScaledOnce);
            ModelJsonPersistence.SaveStandardScalerChain([loadedScaler, scaler2], chainPath);
            var loadedChain = ModelJsonPersistence.LoadStandardScalerChain(chainPath);
            TestAssert.Equal(2, loadedChain.Length);

            var xScaled = ModelJsonPersistence.TransformStandardScalerChain(loadedChain, x);
            var model = new SoftmaxRegression<double>(learningRate: 0.1, epochs: 1500, batchSize: 8, seed: 3);
            model.Fit(xScaled, y);
            ModelJsonPersistence.SaveSoftmaxRegression(model, modelPath);
            var loadedModel = ModelJsonPersistence.LoadSoftmaxRegression(modelPath);

            int original = model.Predict(MLTestData.GetRow(xScaled, 0));
            int restored = loadedModel.Predict(
                ModelJsonPersistence.TransformStandardScalerChain(loadedChain, MLTestData.GetRow(x, 0)));
            TestAssert.Equal(original, restored);

            var knn = new KnnClassifier<double>(k: 3);
            knn.Fit(xScaled, y);
            ModelJsonPersistence.SaveKnnClassifier(knn, knnPath);
            var loadedKnn = ModelJsonPersistence.LoadKnnClassifier(knnPath);
            TestAssert.Equal(
                knn.Predict(MLTestData.GetRow(xScaled, 0)),
                loadedKnn.Predict(MLTestData.GetRow(xScaled, 0)));

            var rf = new NumericRandomForest<double>(numTrees: 20, maxFeatures: 2, maxDepth: 8, seed: 9);
            rf.Fit(xScaled, y);
            ModelJsonPersistence.SaveNumericRandomForest(rf, rfPath);
            var loadedRf = ModelJsonPersistence.LoadNumericRandomForest(rfPath);
            TestAssert.Equal(
                rf.Predict(MLTestData.GetRow(xScaled, 0)),
                loadedRf.Predict(MLTestData.GetRow(xScaled, 0)));

            // Linear SVM expects labels ±1
            int rows = xScaled.GetLength(0);
            int cols = xScaled.GetLength(1);
            var svmInputs = new double[rows][];
            var svmLabels = new int[rows];
            for (int i = 0; i < rows; i++)
            {
                svmInputs[i] = MLTestData.GetRow(xScaled, i);
                svmLabels[i] = y[i] == 0 ? -1 : 1;
            }
            var svm = new SupportVectorMachine<double>(cols, learningRate: 0.05, epochs: 400);
            svm.Train(svmInputs, svmLabels);
            ModelJsonPersistence.SaveSupportVectorMachine(svm, svmPath);
            var loadedSvm = ModelJsonPersistence.LoadSupportVectorMachine(svmPath);
            TestAssert.Equal(svm.Predict(svmInputs[0]), loadedSvm.Predict(svmInputs[0]));

            Console.WriteLine("JSON save/load roundtrip OK (scaler/chain/softmax/knn/rf/svm)");
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    static void TestLuSolve()
    {
        Console.WriteLine("Testing LUSolve vs GaussianElimination...");
        var a = new Matrix<double>(new double[,]
        {
            { 5, 15, 15 },
            { 15, 55, 51 },
            { 15, 51, 55 }
        });
        var b = new double[] { 25, 91, 91 };
        var xGauss = LinearEquationSolver.GaussianEliminationSolve(a, b);
        var xLu = LinearEquationSolver.LUSolve(a, b);

        for (int i = 0; i < b.Length; i++)
            TestAssert.InRange(Math.Abs(xLu[i] - xGauss[i]), 0, 1e-6, $"LUSolve[{i}] mismatch");

        var a2 = new Matrix<double>(new double[,] { { 2, 1 }, { 1, 3 } });
        var b2 = new double[] { 5, 9 };
        var x2 = LinearEquationSolver.LUSolve(a2, b2);
        TestAssert.InRange(x2[0], 1.19, 1.21, "2x2 x[0]");
        TestAssert.InRange(x2[1], 2.59, 2.61, "2x2 x[1]");
        Console.WriteLine("LUSolve matches Gaussian elimination");
    }

    static void TestRegressionPipeline()
    {
        Console.WriteLine("Testing RegressionPipeline...");
        var x = new double[,]
        {
            { 1, 2 }, { 2, 3 }, { 3, 1 }, { 4, 5 }, { 5, 4 }, { 6, 6 }
        };
        var y = new double[] { 2, 4, 3, 8, 8, 11 };

        IRegressor<double> regressor = new MultipleLinearRegression<double>();
        var pipeline = new RegressionPipeline<double>()
            .AddPreprocessor(new StandardScaler<double>())
            .SetRegressor(regressor);
        pipeline.Fit(x, y);

        double pred = (double)pipeline.Predict([6.0, 7.0])!;
        TestAssert.InRange(pred, 11.0, 13.5, $"pipeline prediction={pred}");

        var batchPreds = pipeline.PredictBatch(x);
        TestAssert.InRange((double)batchPreds[5], 10.5, 11.5, $"batch prediction[5]={batchPreds[5]}");
        Console.WriteLine($"RegressionPipeline predict[6,7]={pred:F4}, batch[5]={batchPreds[5]:F4}");
    }

    static void TestRegressionCrossValidation()
    {
        Console.WriteLine("Testing regression CrossValidation...");
        var x = new double[,]
        {
            { 1, 2 }, { 2, 3 }, { 3, 1 }, { 4, 5 }, { 5, 4 }, { 6, 6 }
        };
        var y = new double[] { 2, 4, 3, 8, 8, 11 };

        var cv = CrossValidation.ValidateRegressor(
            x,
            y,
            folds: 3,
            seed: 9,
            buildPredictor: (xTrain, yTrain) =>
            {
                var model = new MultipleLinearRegression<double>();
                model.Fit(xTrain, yTrain);
                return sample => model.Predict(sample);
            });

        TestAssert.True(cv.MeanR2 > 0.5, $"CV R²={cv.MeanR2}");
        Console.WriteLine($"Regression CV RMSE={cv.MeanRMSE:F4}, R²={cv.MeanR2:F4}");
    }

    static void TestRegressionNumerics()
    {
        Console.WriteLine("Testing MultipleLinearRegression numerics...");
        var x = new double[,]
        {
            { 1, 2 },
            { 2, 3 },
            { 3, 1 },
            { 4, 5 },
            { 5, 4 }
        };
        var y = new double[] { 2, 4, 3, 8, 8 };
        var model = new Vorcyc.Mathematics.MachineLearning.Regression.MultipleLinearRegression<double>();
        model.Fit(x, y);

        double pred = (double)model.Predict([6.0, 7.0])!;
        TestAssert.True(model.RSquared > 0.9, $"R²={model.RSquared}");
        TestAssert.InRange(pred, 11.0, 13.5, $"prediction={pred}");

        double[] batchPreds = model.PredictBatch(x);
        for (int i = 0; i < y.Length; i++)
            TestAssert.InRange((double)batchPreds[i], y[i] - 0.5, y[i] + 0.5, $"batch[{i}]={batchPreds[i]}");
        Console.WriteLine($"MLR R²={model.RSquared:F4}, predict[6,7]={pred:F4}, batch OK");
    }

}

internal static class TestAssert
{
    public static void True(bool condition, string message) =>
        TestAssert.Equal(true, condition, message);

    public static void Equal<T>(T expected, T actual, string? message = null) where T : IEquatable<T>
    {
        if (!actual.Equals(expected))
            throw new InvalidOperationException(message ?? $"Expected {expected}, got {actual}");
    }

    public static void InRange(double value, double min, double max, string? message = null)
    {
        if (value < min || value > max)
            throw new InvalidOperationException(message ?? $"Value {value} not in [{min}, {max}]");
    }
}
