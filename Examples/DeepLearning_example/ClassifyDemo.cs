using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DeepLearning_example;

internal static class ClassifyDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 二分类 (ReLU + CrossEntropy) ---");

        var dataset = new List<TrainingSample<float>>
        {
            new(TensorUtilities.FromVector(0.1f, 0.2f), TensorUtilities.OneHot<float>(2, 0)),
            new(TensorUtilities.FromVector(0.2f, 0.1f), TensorUtilities.OneHot<float>(2, 0)),
            new(TensorUtilities.FromVector(0.8f, 0.9f), TensorUtilities.OneHot<float>(2, 1)),
            new(TensorUtilities.FromVector(0.9f, 0.8f), TensorUtilities.OneHot<float>(2, 1)),
            new(TensorUtilities.FromVector(0.15f, 0.25f), TensorUtilities.OneHot<float>(2, 0)),
            new(TensorUtilities.FromVector(0.85f, 0.75f), TensorUtilities.OneHot<float>(2, 1)),
        };

        var model = new Sequential<float>(
            new FullyConnectedLayer<float>(2, 6, null, new Random(7)),
            new ReLUActivation<float>(),
            new FullyConnectedLayer<float>(6, 2, null, new Random(7)));

        var trainer = new Trainer<float>();
        trainer.Fit(
            model,
            new CategoricalCrossEntropyLoss<float>(),
            new SgdOptimizer<float>(0.15f),
            dataset,
            epochs: 400,
            shuffle: true);

        int correct = 0;
        foreach (var sample in dataset)
        {
            var logits = model.Forward(sample.Input, training: false);
            int predicted = logits[0, 0, 0] > logits[0, 0, 1] ? 0 : 1;
            int expected = sample.Target[0, 0, 0] > sample.Target[0, 0, 1] ? 0 : 1;
            if (predicted == expected)
            {
                correct++;
            }
        }

        Console.WriteLine($"训练集准确率: {correct}/{dataset.Count} ({100.0 * correct / dataset.Count:F0}%)");
        return 0;
    }
}
