using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Serialization;

namespace DL_module_test;

internal static class ModelArchitectureV3_test
{
    public static bool Run()
    {
        var model = new BatchSequential<float>(
            new BatchConvolution2DLayer<float>(1, 2, kernelSize: 3, name: "conv"),
            new BatchBatchNormLayer<float>(2, "bn"),
            new BatchDropoutLayer<float>(0.1, "drop"),
            new BatchGlobalAveragePool2DLayer<float>(),
            new BatchFullyConnectedLayer<float>(2, 2, "fc"));

        var before = ModelSerializer.FlattenParameters(model);
        var path = Path.Combine(Path.GetTempPath(), $"vmath_arch_{Guid.NewGuid():N}.bin");

        try
        {
            ModelSerializer.SaveToFile(model, path);
            TensorUtilities.FillUniformRandom(model.Parameters[0].Value, 1f);
            ModelSerializer.LoadFromFile(model, path);

            var after = ModelSerializer.FlattenParameters(model);
            if (before.Length != after.Length)
            {
                return false;
            }

            for (int i = 0; i < before.Length; i++)
            {
                if (before[i] != after[i])
                {
                    return false;
                }
            }

            return true;
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
