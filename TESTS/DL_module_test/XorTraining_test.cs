using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DL_module_test;

internal static class XorTraining_test
{
    public static bool Run()
    {
        var dataset = new[]
        {
            new TrainingSample<float>(TensorUtilities.FromVector(0f, 0f), TensorUtilities.FromVector(0f)),
            new TrainingSample<float>(TensorUtilities.FromVector(0f, 1f), TensorUtilities.FromVector(1f)),
            new TrainingSample<float>(TensorUtilities.FromVector(1f, 0f), TensorUtilities.FromVector(1f)),
            new TrainingSample<float>(TensorUtilities.FromVector(1f, 1f), TensorUtilities.FromVector(0f)),
        };

        var model = new Sequential<float>(
            new FullyConnectedLayer<float>(2, 8),
            new SigmoidActivation<float>(),
            new FullyConnectedLayer<float>(8, 1),
            new SigmoidActivation<float>());

        var trainer = new Trainer<float>();
        var loss = new MeanSquaredErrorLoss<float>();
        var optimizer = new SgdOptimizer<float>(0.5f);

        trainer.Fit(model, loss, optimizer, dataset, epochs: 5000, shuffle: true);

        foreach (var sample in dataset)
        {
            var output = model.Forward(sample.Input, training: false);
            var error = MathF.Abs(output[0, 0, 0] - sample.Target[0, 0, 0]);
            if (error > 0.2f)
            {
                return false;
            }
        }

        return true;
    }
}
