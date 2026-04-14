using System.Text.Json;
using Vorcyc.Mathematics.MachineLearning.Classfication;
using Vorcyc.Mathematics.MachineLearning.Preprocessing;

namespace Vorcyc.Mathematics.MachineLearning.Serialization;

/// <summary>
/// 机器学习模型 JSON 持久化工具。
/// </summary>
public static class ModelJsonPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void SaveStandardScaler(StandardScaler<double> scaler, string path)
    {
        var snapshot = new StandardScalerSnapshot
        {
            Mean = scaler.Mean.Select(v => (double)v).ToArray(),
            Std = scaler.Std.Select(v => (double)v).ToArray()
        };
        File.WriteAllText(path, JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    public static StandardScaler<double> LoadStandardScaler(string path)
    {
        var snapshot = JsonSerializer.Deserialize<StandardScalerSnapshot>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("无法反序列化 StandardScaler。");
        var scaler = new StandardScaler<double>();
        scaler.LoadState(snapshot.Mean, snapshot.Std);
        return scaler;
    }

    public static void SaveSoftmaxRegression(SoftmaxRegression<double> model, string path)
    {
        var snapshot = model.CaptureSnapshot();
        File.WriteAllText(path, JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    public static SoftmaxRegression<double> LoadSoftmaxRegression(string path)
    {
        var snapshot = JsonSerializer.Deserialize<SoftmaxRegressionSnapshot>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("无法反序列化 SoftmaxRegression。");
        var model = new SoftmaxRegression<double>();
        model.RestoreFromSnapshot(snapshot);
        return model;
    }
}
