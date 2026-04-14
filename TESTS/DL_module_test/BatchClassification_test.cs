using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DL_module_test;

internal static class BatchClassification_test
{
    public static bool Run()
    {
        var batch = new BatchTensor<float>(2, 4, 4, 1);
        FillPattern(batch, 0, leftOnes: true);
        FillPattern(batch, 1, leftOnes: false);

        var targets = new BatchTensor<float>(2, 1, 1, 2);
        targets[0, 0, 0, 0] = 1f;
        targets[0, 0, 0, 1] = 0f;
        targets[1, 0, 0, 0] = 0f;
        targets[1, 0, 0, 1] = 1f;

        var model = new BatchSequential<float>(
            new BatchConvolution2DLayer<float>(1, 4, kernelSize: 3, name: "conv"),
            new BatchReLUActivation<float>(),
            new BatchGlobalAveragePool2DLayer<float>(),
            new BatchFullyConnectedLayer<float>(4, 2, name: "fc"));

        var trainer = new Trainer<float>();
        trainer.FitBatchSequential(
            model,
            new BatchCategoricalCrossEntropyLoss<float>(),
            new AdamOptimizer<float>(0.05f),
            [new BatchLabelSample<float>(batch, targets)],
            epochs: 3000);

        var logits = model.Forward(batch, training: false);
        return logits[0, 0, 0, 0] > logits[0, 0, 0, 1]
            && logits[1, 0, 0, 1] > logits[1, 0, 0, 0];
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
