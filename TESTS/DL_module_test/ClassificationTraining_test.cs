using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DL_module_test;

internal static class ClassificationTraining_test
{
    public static bool Run()
    {
        // Simple 2-class linearly separable points in 2D.
        var dataset = new List<TrainingSample<float>>
        {
            new(TensorUtilities.FromVector(0.1f, 0.2f), TensorUtilities.OneHot<float>(2, 0)),
            new(TensorUtilities.FromVector(0.2f, 0.1f), TensorUtilities.OneHot<float>(2, 0)),
            new(TensorUtilities.FromVector(0.9f, 0.8f), TensorUtilities.OneHot<float>(2, 1)),
            new(TensorUtilities.FromVector(0.8f, 0.9f), TensorUtilities.OneHot<float>(2, 1)),
        };

        var model = new Sequential<float>(
            new FullyConnectedLayer<float>(2, 4),
            new ReLUActivation<float>(),
            new FullyConnectedLayer<float>(4, 2));

        var trainer = new Trainer<float>();
        var loss = new CategoricalCrossEntropyLoss<float>();
        var optimizer = new SgdOptimizer<float>(0.1f);

        trainer.Fit(model, loss, optimizer, dataset, epochs: 300, shuffle: true);

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

        return correct == dataset.Count;
    }
}
