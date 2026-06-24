using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DeepLearning_example;

internal static class CurveFitDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 曲线拟合 (MlpRegressor) ---");

        float[] x = [0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f];
        float[] y = [0f, 0.04f, 0.16f, 0.36f, 0.64f, 1f];

        int sampleSize = x.Length;
        const int hiddenNodes = 12;

        var inputs = TensorUtilities.FromBatchVectors(x, sampleSize, features: 1);
        var targets = TensorUtilities.FromBatchVectors(y, sampleSize, features: 1);

        var options = new MlpTrainingOptions<float>
        {
            RandomSeed = 42,
            OptimizerKind = MlpOptimizerKind.Adam,
            InitialLearningRate = 0.08f
        };

        var model = MlpRegressor.CreateRegressionNetwork<float>(
            inputSize: 1,
            hiddenSize: hiddenNodes,
            outputSize: 1,
            random: options.RandomSeed is int seed ? new Random(seed) : null);

        MlpRegressor.TrainBatched(model, inputs, targets, epochs: 4000, options);

        float mse = MlpRegressor.ComputeMeanSquaredError(model, inputs, targets);

        float[] probes = [0.25f, 0.5f, 0.75f];
        Console.WriteLine($"训练 MSE: {mse:E3}");
        Console.WriteLine("y = x² 插值:");
        foreach (float probe in probes)
        {
            float predicted = model.Forward(TensorUtilities.FromVector(probe), training: false)[0, 0, 0];
            float expected = probe * probe;
            Console.WriteLine($"  x={probe:F2} → 预测 {predicted:F4}, 真值 {expected:F4}");
        }

        return 0;
    }
}
