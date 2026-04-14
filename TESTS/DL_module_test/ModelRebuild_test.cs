using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Serialization;

namespace DL_module_test;

internal static class ModelRebuild_test
{
    public static bool Run()
    {
        var original = new BatchSequential<float>(
            new BatchConvolution2DLayer<float>(1, 2, kernelSize: 3, name: "conv"),
            new BatchBatchNormLayer<float>(2, "bn"),
            new BatchGlobalAveragePool2DLayer<float>(),
            new BatchFullyConnectedLayer<float>(2, 2, "fc"));

        var before = ModelSerializer.FlattenParameters(original);
        var path = Path.Combine(Path.GetTempPath(), $"vmath_rebuild_{Guid.NewGuid():N}.bin");

        try
        {
            ModelSerializer.SaveToFile(original, path);
            var rebuilt = ModelSerializer.LoadBatchSequentialFromFile<float>(path);
            var after = ModelSerializer.FlattenParameters(rebuilt);

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

            return rebuilt.Layers.Count == original.Layers.Count;
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
