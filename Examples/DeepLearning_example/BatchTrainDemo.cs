using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DeepLearning_example;

internal static class BatchTrainDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 批训练 XOR (FitBatched + ComputingContext) ---");

        var inputs = TensorUtilities.FromBatchVectors(
            [0f, 0f, 0f, 1f, 1f, 0f, 1f, 1f],
            batchSize: 4,
            features: 2);
        var targets = TensorUtilities.FromBatchVectors(
            [0f, 1f, 1f, 0f],
            batchSize: 4,
            features: 1);

        var model = new Sequential<float>(
            new FullyConnectedLayer<float>(2, 8, null, new Random(42)),
            new SigmoidActivation<float>(),
            new FullyConnectedLayer<float>(8, 1, null, new Random(42)),
            new SigmoidActivation<float>());

        var trainer = new Trainer<float>();
        trainer.FitBatched(
            model,
            new MeanSquaredErrorLoss<float>(),
            new AdamOptimizer<float>(0.1f),
            [new TrainingSample<float>(inputs, targets)],
            epochs: 2500,
            computingContext: ComputingContext.Parallel,
            onEpochEnd: (epoch, loss) =>
            {
                if (epoch is 1000 or 2500)
                {
                    Console.WriteLine($"  epoch {epoch,4}: loss = {float.CreateTruncating(loss):F4}");
                }
            });

        var output = model.Forward(inputs, training: false);
        Console.WriteLine("批预测 (输入 → 输出, 期望):");
        for (var i = 0; i < 4; i++)
        {
            float x0 = inputs[0, i, 0];
            float x1 = inputs[0, i, 1];
            float y = output[0, i, 0];
            float t = targets[0, i, 0];
            Console.WriteLine($"  ({x0:F0}, {x1:F0}) → {y:F3} (期望 {t:F0})");
        }

        return 0;
    }
}