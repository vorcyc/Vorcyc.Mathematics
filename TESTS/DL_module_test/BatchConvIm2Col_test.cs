using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;

namespace DL_module_test;

internal static class BatchConvIm2Col_test
{
    public static bool Run()
    {
        var input = new BatchTensor<float>(2, 6, 6, 2);
        for (int i = 0; i < input.Values.Length; i++)
        {
            input.Values[i] = (float)(i % 11) * 0.05f;
        }

        var layer = new BatchConvolution2DLayer<float>(2, 4, kernelSize: 5);
        foreach (var parameter in layer.Parameters)
        {
            parameter.Value.Fill(0.05f);
        }

        var output = layer.Forward(input, training: true);
        var gradOutput = new BatchTensor<float>(
            output.Batch,
            output.Height,
            output.Width,
            output.Channels);
        gradOutput.Values.Fill(0.02f);

        var gradInput = layer.Backward(gradOutput);
        float filterGradSum = 0f;
        foreach (var parameter in layer.Parameters)
        {
            foreach (var value in parameter.Gradient.Values)
            {
                filterGradSum += MathF.Abs(value);
            }
        }

        return filterGradSum > 0f
            && !float.IsNaN(output[0, 0, 0, 0])
            && gradInput.Values.Length == input.Values.Length;
    }
}
