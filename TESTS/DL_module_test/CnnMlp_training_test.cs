using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DL_module_test;

internal static class CnnMlp_training_test
{
    public static bool Run()
    {
        var batch = new BatchTensor<float>(2, 4, 4, 1);
        FillPattern(batch, sampleIndex: 0, leftOnes: true);
        FillPattern(batch, sampleIndex: 1, leftOnes: false);

        var targets = TensorUtilities.FromBatchVectors([1f, 0f], batchSize: 2, features: 1);

        var backbone = new BatchSequential<float>(
            new BatchConvolution2DLayer<float>(1, 4, kernelSize: 3, name: "conv"),
            new BatchReLUActivation<float>(),
            new BatchMaxPool2DLayer<float>(),
            new BatchFlattenLayer<float>());

        var head = new Sequential<float>(
            new FullyConnectedLayer<float>(16, 8),
            new SigmoidActivation<float>(),
            new FullyConnectedLayer<float>(8, 1),
            new SigmoidActivation<float>());

        var model = new CnnMlpModel<float>(backbone, head);
        var trainer = new Trainer<float>();
        trainer.FitCnnMlp(
            model,
            new MeanSquaredErrorLoss<float>(),
            new AdamOptimizer<float>(0.05f),
            [new BatchTrainingSample<float>(batch, targets)],
            epochs: 2500);

        var output = model.Forward(batch, training: false);
        return MathF.Abs(output[0, 0, 0] - 1f) < 0.35f
            && MathF.Abs(output[0, 1, 0] - 0f) < 0.35f;
    }

    private static void FillPattern(BatchTensor<float> batch, int sampleIndex, bool leftOnes)
    {
        for (int h = 0; h < 4; h++)
        {
            for (int w = 0; w < 4; w++)
            {
                bool isLeft = w < 2;
                batch[sampleIndex, h, w, 0] = (leftOnes ? isLeft : !isLeft) ? 1f : 0f;
            }
        }
    }
}
