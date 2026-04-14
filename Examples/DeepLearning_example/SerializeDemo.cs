using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Serialization;

namespace DeepLearning_example;

internal static class SerializeDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 模型序列化 (ModelSerializer v3) ---");

        var model = new Sequential<float>(
            new FullyConnectedLayer<float>(2, 4, "fc1", random: new Random(1)),
            new ReLUActivation<float>("relu1"),
            new FullyConnectedLayer<float>(4, 1, "fc2", random: new Random(1)));

        var sample = TensorUtilities.FromVector(0.3f, 0.7f);
        float before = model.Forward(sample, training: false)[0, 0, 0];

        string path = Path.Combine(Path.GetTempPath(), $"vmath_dl_example_{Guid.NewGuid():N}.vmdl");
        try
        {
            ModelSerializer.SaveToFile(model, path);
            long bytes = new FileInfo(path).Length;

            TensorUtilities.FillUniformRandom(model.Parameters[0].Value, 1f);
            ModelSerializer.LoadFromFile(model, path);

            float after = model.Forward(sample, training: false)[0, 0, 0];
            Console.WriteLine($"文件: {path}");
            Console.WriteLine($"大小: {bytes} 字节, 参数张量数: {model.Parameters.Count}");
            Console.WriteLine($"推理一致性: {before:F6} → 加载后 {after:F6} (Δ={MathF.Abs(before - after):E2})");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        return 0;
    }
}
