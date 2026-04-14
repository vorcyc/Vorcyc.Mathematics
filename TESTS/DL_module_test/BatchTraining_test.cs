using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DL_module_test;

internal static class BatchTraining_test
{
    public static bool Run()
    {
        var inputs = TensorUtilities.FromBatchVectors(
            [0f, 0f, 0f, 1f, 1f, 0f, 1f, 1f],
            batchSize: 4,
            features: 2);
        var targets = TensorUtilities.FromBatchVectors(
            [0f, 1f, 1f, 0f],
            batchSize: 4,
            features: 1);

        var model = new Sequential<float>(
            new FullyConnectedLayer<float>(2, 8),
            new SigmoidActivation<float>(),
            new FullyConnectedLayer<float>(8, 1),
            new SigmoidActivation<float>());

        var trainer = new Trainer<float>();
        trainer.FitBatched(
            model,
            new MeanSquaredErrorLoss<float>(),
            new AdamOptimizer<float>(0.1f),
            [new TrainingSample<float>(inputs, targets)],
            epochs: 3000);

        var output = model.Forward(inputs, training: false);
        for (int i = 0; i < 4; i++)
        {
            if (MathF.Abs(output[0, i, 0] - targets[0, i, 0]) > 0.25f)
            {
                return false;
            }
        }

        return true;
    }
}
