using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;

namespace DL_module_test;

internal static class BatchConvBackward_test
{
    public static bool Run()
    {
        var batch = new BatchTensor<float>(2, 3, 3, 1);
        batch[0, 0, 0, 0] = 1f;
        batch[0, 1, 1, 0] = 2f;
        batch[1, 0, 0, 0] = 3f;
        batch[1, 2, 2, 0] = 4f;

        var layer = new BatchConvolution2DLayer<float>(1, 2, kernelSize: 2);
        foreach (var parameter in layer.Parameters)
        {
            parameter.Value.Fill(0.1f);
        }

        var output = layer.Forward(batch, training: true);
        var gradOutput = new BatchTensor<float>(output.Batch, output.Height, output.Width, output.Channels);
        gradOutput[0, 0, 0, 0] = 1f;
        gradOutput[0, 0, 0, 1] = 0.5f;
        gradOutput[1, 0, 0, 0] = 2f;
        gradOutput[1, 0, 0, 1] = -1f;

        var gradInput = layer.Backward(gradOutput);

        float filterGradSum = 0f;
        foreach (var parameter in layer.Parameters)
        {
            foreach (var value in parameter.Gradient.Values)
            {
                filterGradSum += MathF.Abs(value);
            }
        }

        return gradInput.Batch == 2
            && filterGradSum > 0f
            && !float.IsNaN(gradInput[0, 0, 0, 0]);
    }
}
