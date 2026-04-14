using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;

namespace DL_module_test;

internal static class BatchFc_test
{
    public static bool Run()
    {
        var inputs = TensorUtilities.FromBatchVectors(
            [0f, 0f, 0f, 1f, 1f, 0f, 1f, 1f],
            batchSize: 4,
            features: 2);
        var batchInput = BatchTensor<float>.FromFeatureTensor(inputs);
        var targets = TensorUtilities.FromBatchVectors([0f, 1f, 1f, 0f], batchSize: 4, features: 1);

        var model = new BatchSequential<float>(
            new BatchFullyConnectedLayer<float>(2, 8),
            new BatchSigmoidActivation<float>(),
            new BatchFullyConnectedLayer<float>(8, 1),
            new BatchSigmoidActivation<float>());

        var loss = new MeanSquaredErrorLoss<float>();
        var optimizer = new AdamOptimizer<float>(0.1f);

        for (int epoch = 0; epoch < 2500; epoch++)
        {
            optimizer.ZeroGrad(model.Parameters);
            var features = model.Forward(batchInput, training: true);
            var prediction = features.ToFeatureTensor();
            var lossValue = loss.Compute(prediction, targets);
            var grad = loss.Backward(prediction, targets);
            model.Backward(BatchTensor<float>.FromFeatureTensor(grad));
            optimizer.Step(model.Parameters);
            _ = lossValue;
        }

        var output = model.Forward(batchInput, training: false).ToFeatureTensor();
        for (int i = 0; i < 4; i++)
        {
            if (MathF.Abs(output[0, i, 0] - targets[0, i, 0]) > 0.3f)
            {
                return false;
            }
        }

        return true;
    }
}
