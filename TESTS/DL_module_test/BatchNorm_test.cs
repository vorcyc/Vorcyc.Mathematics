using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DL_module_test;

internal static class BatchNorm_test
{
    public static bool Run()
    {
        var model = new Sequential<float>(
            new FullyConnectedLayer<float>(2, 4),
            new BatchNormLayer<float>(4),
            new ReLUActivation<float>(),
            new FullyConnectedLayer<float>(4, 1));

        var inputs = TensorUtilities.FromBatchVectors(
            [0.1f, 0.2f, 0.9f, 0.8f, 0.2f, 0.1f, 0.7f, 0.9f],
            batchSize: 4,
            features: 2);
        var targets = TensorUtilities.FromBatchVectors(
            [0.2f, 0.9f, 0.3f, 0.8f],
            batchSize: 4,
            features: 1);

        new Trainer<float>().FitBatched(
            model,
            new MeanSquaredErrorLoss<float>(),
            new AdamOptimizer<float>(0.05f),
            [new TrainingSample<float>(inputs, targets)],
            epochs: 200);

        var bn = (BatchNormLayer<float>)model.Layers[1];
        if (bn.RunningMean[0, 0, 0] == 0f && bn.RunningVariance[0, 0, 0] == 1f)
        {
            return false;
        }

        var eval = model.Forward(inputs, training: false);
        return eval.Height == 4;
    }
}
