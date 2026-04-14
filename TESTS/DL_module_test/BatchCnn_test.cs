using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.LinearAlgebra;

namespace DL_module_test;

internal static class BatchCnn_test
{
    public static bool Run()
    {
        var batch = new BatchTensor<float>(2, 2, 2, 1);
        batch[0, 0, 0, 0] = 1f;
        batch[0, 0, 1, 0] = 2f;
        batch[0, 1, 0, 0] = 3f;
        batch[0, 1, 1, 0] = 4f;
        batch[1, 0, 0, 0] = 4f;
        batch[1, 0, 1, 0] = 3f;
        batch[1, 1, 0, 0] = 2f;
        batch[1, 1, 1, 0] = 1f;

        var model = new BatchSequential<float>(
            new BatchConvolution2DLayer<float>(1, 1, kernelSize: 2, stride: 1, name: "conv"),
            new BatchBatchNormLayer<float>(1, "bn"),
            new BatchReLUActivation<float>(),
            new BatchMaxPool2DLayer<float>(),
            new BatchFlattenLayer<float>());

        var output = model.Forward(batch, training: true);
        if (output.Shape.Batch != 2 || output.Shape.Height != 1 || output.Shape.Width != 1 || output.Shape.Channels != 1)
        {
            return false;
        }

        var sample = batch.GetSample(0);
        if (sample.Width != 2 || sample.Height != 2 || sample.Depth != 1)
        {
            return false;
        }

        var feature = output.ToFeatureTensor();
        return feature.Height == 2 && feature.Depth == 1;
    }
}
