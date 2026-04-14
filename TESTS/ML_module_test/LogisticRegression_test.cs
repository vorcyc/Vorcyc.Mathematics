using Vorcyc.Mathematics.MachineLearning;
using Vorcyc.Mathematics.MachineLearning.Classfication;

namespace ML_module_test;

public static class LogisticRegression_test
{
    public static void Go()
    {
        Console.WriteLine("Testing LogisticRegression...");
        var x = new double[,]
        {
            { 0.0, 0.0 },
            { 0.0, 1.0 },
            { 1.0, 0.0 },
            { 1.0, 1.0 }
        };
        int[] y = [0, 0, 1, 1];

        var model = new LogisticRegression<double>(epochs: 3000, learningRate: 0.1);
        model.Fit(x, y);

        int p00 = model.Predict([0.0, 0.0]);
        int p11 = model.Predict([1.0, 1.0]);
        double prob11 = (double)model.PredictProbability([1.0, 1.0]);

        Console.WriteLine($"Predict [0,0]: {p00} (Expected 0)");
        Console.WriteLine($"Predict [1,1]: {p11} (Expected 1)");
        Console.WriteLine($"Probability [1,1]: {prob11:F4} (Expected > 0.5)");

        var actual = new[] { 0, 0, 1, 1 };
        var predicted = new[]
        {
            model.Predict([0.0, 0.0]),
            model.Predict([0.0, 1.0]),
            model.Predict([1.0, 0.0]),
            model.Predict([1.0, 1.0])
        };
        Console.WriteLine($"Accuracy: {EvaluationMetrics.Accuracy(actual, predicted):P0}");
    }
}
