using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace DeepLearning_example;

internal static class CnnMlpDemo
{
    public static int Run()
    {
        Console.WriteLine("--- CNN + MLP (CnnMlpModel + FitCnnMlp) ---");

        var batch = new BatchTensor<float>(2, 4, 4, 1);
        FillPattern(batch, sampleIndex: 0, leftOnes: true);
        FillPattern(batch, sampleIndex: 1, leftOnes: false);
        var targets = TensorUtilities.FromBatchVectors([1f, 0f], batchSize: 2, features: 1);

        var backbone = new BatchSequential<float>(
            new BatchConvolution2DLayer<float>(1, 4, kernelSize: 3),
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
            epochs: 2000,
            onEpochEnd: (epoch, loss) =>
            {
                if (epoch is 1000 or 2000)
                {
                    Console.WriteLine($"  epoch {epoch,4}: loss = {float.CreateTruncating(loss):F4}");
                }
            });

        var output = model.Forward(batch, training: false);
        Console.WriteLine($"样本 0 (左半为 1): {output[0, 0, 0]:F3} (期望 ≈1)");
        Console.WriteLine($"样本 1 (右半为 1): {output[0, 1, 0]:F3} (期望 ≈0)");
        return 0;
    }

    static void FillPattern(BatchTensor<float> batch, int sampleIndex, bool leftOnes)
    {
        for (var h = 0; h < 4; h++)
        {
            for (var w = 0; w < 4; w++)
            {
                bool isLeft = w < 2;
                batch[sampleIndex, h, w, 0] = (leftOnes ? isLeft : !isLeft) ? 1f : 0f;
            }
        }
    }
}
