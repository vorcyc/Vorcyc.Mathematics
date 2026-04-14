using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DeepLearning_example;

internal static class XorDemo
{
    public static int Run()
    {
        Console.WriteLine("--- XOR 训练 (Sequential + MSE) ---");

        var dataset = new[]
        {
            new TrainingSample<float>(TensorUtilities.FromVector(0f, 0f), TensorUtilities.FromVector(0f)),
            new TrainingSample<float>(TensorUtilities.FromVector(0f, 1f), TensorUtilities.FromVector(1f)),
            new TrainingSample<float>(TensorUtilities.FromVector(1f, 0f), TensorUtilities.FromVector(1f)),
            new TrainingSample<float>(TensorUtilities.FromVector(1f, 1f), TensorUtilities.FromVector(0f)),
        };

        var model = new Sequential<float>(
            new FullyConnectedLayer<float>(2, 8, null, new Random(42)),
            new SigmoidActivation<float>(),
            new FullyConnectedLayer<float>(8, 1, null, new Random(42)),
            new SigmoidActivation<float>());

        var trainer = new Trainer<float>();
        trainer.Fit(
            model,
            new MeanSquaredErrorLoss<float>(),
            new SgdOptimizer<float>(0.5f),
            dataset,
            epochs: 5000,
            shuffle: true,
            onEpochEnd: (epoch, loss) =>
            {
                if (epoch is 1000 or 3000 or 5000)
                {
                    Console.WriteLine($"  epoch {epoch,4}: loss = {float.CreateTruncating(loss):F4}");
                }
            });

        Console.WriteLine("预测 (输入 → 输出, 期望):");
        foreach (var sample in dataset)
        {
            float x0 = sample.Input[0, 0, 0];
            float x1 = sample.Input[0, 0, 1];
            float y = model.Forward(sample.Input, training: false)[0, 0, 0];
            float target = sample.Target[0, 0, 0];
            Console.WriteLine($"  ({x0:F0}, {x1:F0}) → {y:F3} (期望 {target:F0})");
        }

        return 0;
    }
}
