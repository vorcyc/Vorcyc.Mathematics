using Vorcyc.Mathematics.MachineLearning;
using Vorcyc.Mathematics.MachineLearning.Classfication;
using Vorcyc.Mathematics.MachineLearning.Preprocessing;

namespace ML_module_test;

public static class ClassificationSuite_test
{
    public static void Go()
    {
        var (x, y) = MLTestData.CreateThreeBlobDataset(pointsPerClass: 10);
        var (xTrain, yTrain, xTest, yTest) = DataSplit.TrainTestSplit(x, y, testRatio: 0.25, seed: 7);

        TestStandardScalerPipeline(xTrain, yTrain, xTest, yTest);
        TestSoftmaxRegression(xTrain, yTrain, xTest, yTest);
        TestNumericDecisionTree(xTrain, yTrain, xTest, yTest);
        TestNumericRandomForest(xTrain, yTrain, xTest, yTest);
        TestClassificationMetrics(yTest, PredictWithTree(xTrain, yTrain, xTest));
        TestGaussianNaiveBayes(x, y);
        TestCrossValidation(x, y);
        TestGradientBoosting(xTrain, yTrain, xTest, yTest);
        TestAdaBoost(xTrain, yTrain, xTest, yTest);
        TestKnnClassifier(xTrain, yTrain, xTest, yTest);
    }

    static void TestStandardScalerPipeline(double[,] xTrain, int[] yTrain, double[,] xTest, int[] yTest)
    {
        Console.WriteLine("Testing StandardScaler + Softmax pipeline...");
        var scaler = new StandardScaler<double>();
        var xTrainScaled = scaler.FitTransform(xTrain);
        var xTestScaled = scaler.Transform(ExtractRows(xTest));

        var model = new SoftmaxRegression<double>(learningRate: 0.1, epochs: 2500);
        model.Fit(xTrainScaled, yTrain);

        var predictions = PredictAll(model, xTestScaled);
        double accuracy = EvaluationMetrics.Accuracy(yTest, predictions);
        Console.WriteLine($"Scaler + Softmax accuracy: {accuracy:P1}");
    }

    static void TestSoftmaxRegression(double[,] xTrain, int[] yTrain, double[,] xTest, int[] yTest)
    {
        Console.WriteLine("Testing SoftmaxRegression...");
        var model = new SoftmaxRegression<double>(learningRate: 0.08, epochs: 3000);
        model.Fit(xTrain, yTrain);
        var predictions = PredictAll(model, xTest);
        double accuracy = EvaluationMetrics.Accuracy(yTest, predictions);
        Console.WriteLine($"Softmax accuracy: {accuracy:P1} (Expected high)");
    }

    static void TestNumericDecisionTree(double[,] xTrain, int[] yTrain, double[,] xTest, int[] yTest)
    {
        Console.WriteLine("Testing NumericDecisionTree...");
        var predictions = PredictWithTree(xTrain, yTrain, xTest);
        double accuracy = EvaluationMetrics.Accuracy(yTest, predictions);
        Console.WriteLine($"Decision tree accuracy: {accuracy:P1}");
    }

    static void TestNumericRandomForest(double[,] xTrain, int[] yTrain, double[,] xTest, int[] yTest)
    {
        Console.WriteLine("Testing NumericRandomForest...");
        var forest = new NumericRandomForest<double>(numTrees: 80, maxFeatures: 2, maxDepth: 10, seed: 11);
        forest.Fit(xTrain, yTrain);
        var predictions = new int[yTest.Length];
        for (int i = 0; i < yTest.Length; i++)
            predictions[i] = forest.Predict(MLTestData.GetRow(xTest, i));
        double accuracy = EvaluationMetrics.Accuracy(yTest, predictions);
        Console.WriteLine($"Random forest accuracy: {accuracy:P1}");
    }

    static void TestClassificationMetrics(int[] actual, int[] predicted)
    {
        Console.WriteLine("Testing ClassificationMetrics...");
        var cm = ClassificationMetrics.ConfusionMatrix(actual, predicted);
        double macroF1 = ClassificationMetrics.MacroF1(cm);
        double microF1 = ClassificationMetrics.MicroF1(cm);
        Console.WriteLine($"Confusion matrix size: {cm.NumClasses}x{cm.NumClasses}");
        Console.WriteLine($"Macro-F1: {macroF1:F3}, Micro-F1: {microF1:F3}");
        for (int label = 0; label < cm.NumClasses; label++)
        {
            Console.WriteLine(
                $"Class {label}: P={ClassificationMetrics.Precision(cm, label):F3}, " +
                $"R={ClassificationMetrics.Recall(cm, label):F3}, " +
                $"F1={ClassificationMetrics.F1Score(cm, label):F3}");
        }
    }

    static int[] PredictWithTree(double[,] xTrain, int[] yTrain, double[,] xTest)
    {
        var tree = new NumericDecisionTree<double>(maxDepth: 8);
        tree.Fit(xTrain, yTrain);
        var predictions = new int[xTest.GetLength(0)];
        for (int i = 0; i < predictions.Length; i++)
            predictions[i] = tree.Predict(MLTestData.GetRow(xTest, i));
        return predictions;
    }

    static int[] PredictAll(SoftmaxRegression<double> model, double[,] x)
    {
        var predictions = new int[x.GetLength(0)];
        for (int i = 0; i < predictions.Length; i++)
            predictions[i] = model.Predict(MLTestData.GetRow(x, i));
        return predictions;
    }

    static double[,] ExtractRows(double[,] x)
    {
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        var copy = new double[rows, cols];
        Array.Copy(x, copy, x.Length);
        return copy;
    }

    static void TestKnnClassifier(double[,] xTrain, int[] yTrain, double[,] xTest, int[] yTest)
    {
        Console.WriteLine("Testing KnnClassifier...");
        var model = new KnnClassifier<double>(k: 3);
        model.Fit(xTrain, yTrain);

        var predictions = new int[yTest.Length];
        for (int i = 0; i < yTest.Length; i++)
            predictions[i] = model.Predict(MLTestData.GetRow(xTest, i));

        double accuracy = EvaluationMetrics.Accuracy(yTest, predictions);
        Console.WriteLine($"KnnClassifier accuracy: {accuracy:P1}");
    }

    static void TestGaussianNaiveBayes(double[,] x, int[] y)
    {
        Console.WriteLine("Testing GaussianNaiveBayes...");
        var (xTrain, yTrain, xTest, yTest) = DataSplit.TrainTestSplit(x, y, testRatio: 0.25, seed: 3);
        var model = new GaussianNaiveBayes<double>();
        model.Fit(xTrain, yTrain);

        var predictions = new int[yTest.Length];
        for (int i = 0; i < yTest.Length; i++)
            predictions[i] = model.Predict(MLTestData.GetRow(xTest, i));

        var probs = model.PredictProbabilities(MLTestData.GetRow(xTest, 0));
        double accuracy = EvaluationMetrics.Accuracy(yTest, predictions);
        Console.WriteLine($"Gaussian NB accuracy: {accuracy:P1}");
        Console.WriteLine($"Posterior classes: {string.Join(", ", probs.Select(kv => $"{kv.Key}:{(double)kv.Value:F3}"))}");
    }

    static void TestAdaBoost(double[,] xTrain, int[] yTrain, double[,] xTest, int[] yTest)
    {
        Console.WriteLine("Testing AdaBoostClassifier...");
        var model = new AdaBoostClassifier<double>(nEstimators: 60);
        model.Fit(xTrain, yTrain);

        var predictions = new int[yTest.Length];
        for (int i = 0; i < yTest.Length; i++)
            predictions[i] = model.Predict(MLTestData.GetRow(xTest, i));

        double accuracy = EvaluationMetrics.Accuracy(yTest, predictions);
        Console.WriteLine($"AdaBoost accuracy: {accuracy:P1}");
    }

    static void TestGradientBoosting(double[,] xTrain, int[] yTrain, double[,] xTest, int[] yTest)
    {
        Console.WriteLine("Testing GradientBoostingClassifier...");
        var model = new GradientBoostingClassifier<double>(nEstimators: 40, learningRate: 0.15, maxDepth: 3);
        model.Fit(xTrain, yTrain);

        var predictions = new int[yTest.Length];
        for (int i = 0; i < yTest.Length; i++)
            predictions[i] = model.Predict(MLTestData.GetRow(xTest, i));

        double accuracy = EvaluationMetrics.Accuracy(yTest, predictions);
        Console.WriteLine($"Gradient boosting accuracy: {accuracy:P1}");
    }

    static void TestCrossValidation(double[,] x, int[] y)
    {
        Console.WriteLine("Testing CrossValidation...");
        var cv = CrossValidation.Validate(
            x,
            y,
            folds: 5,
            seed: 21,
            buildPredictor: (xTrain, yTrain) =>
            {
                var model = new GaussianNaiveBayes<double>();
                model.Fit(xTrain, yTrain);
                return sample => model.Predict(sample);
            });

        Console.WriteLine($"CV accuracy: {cv.MeanAccuracy:P1} ± {cv.StdAccuracy:P1}");
        Console.WriteLine($"CV macro-F1: {cv.MeanMacroF1:F3} ± {cv.StdMacroF1:F3}");
        Console.WriteLine($"Fold accuracies: {string.Join(", ", cv.FoldAccuracies.Select(a => a.ToString("P0")))}");
    }

}
