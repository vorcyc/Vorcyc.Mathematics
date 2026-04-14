using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Serialization;

namespace DL_module_test;

internal static class ModelSerializer_test
{
    public static bool Run()
    {
        var model = new Sequential<float>(
            new FullyConnectedLayer<float>(2, 3, "fc1"),
            new SigmoidActivation<float>(),
            new FullyConnectedLayer<float>(3, 1, "fc2"));

        var before = ModelSerializer.FlattenParameters(model);
        var path = Path.Combine(Path.GetTempPath(), $"vmath_model_{Guid.NewGuid():N}.bin");
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
