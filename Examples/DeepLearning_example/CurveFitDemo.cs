using Vorcyc.Mathematics.DeepLearning.Training;
using Vorcyc.Mathematics.Experimental.CurveFitting;

namespace DeepLearning_example;

internal static class CurveFitDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 曲线拟合 (CurveFitter.NeuralNetwork) ---");

        Span<float> x = [0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f];
        Span<float> y = [0f, 0.04f, 0.16f, 0.36f, 0.64f, 1f];

        var trainingOptions = new NeuralNetworkTrainingOptions
        {
            RandomSeed = 42,
            OptimizerKind = MlpOptimizerKind.Adam,
            InitialLearningRate = 0.08,
            SchedulerKind = NeuralNetworkSchedulerKind.CosineAnnealing,
            MinimumLearningRate = 0.001
        };

        var result = CurveFitter<float>.NeuralNetwork(
            x, y,
            epochs: 4000,
            hiddenNodes: 12,
            trainingOptions: trainingOptions);

        float[] probes = [0.25f, 0.5f, 0.75f];
        Console.WriteLine($"训练 MSE: {float.CreateTruncating(result.MeanSquaredError):E3}");
        Console.WriteLine("y = x² 插值:");
        foreach (float probe in probes)
        {
            float predicted = float.CreateTruncating(result.Predict(probe));
            float expected = probe * probe;
            Console.WriteLine($"  x={probe:F2} → 预测 {predicted:F4}, 真值 {expected:F4}");
        }

        return 0;
    }
}
