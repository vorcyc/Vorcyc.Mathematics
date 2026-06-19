using System.Text.Json;
using Vorcyc.Mathematics.MachineLearning.Classfication;
using Vorcyc.Mathematics.MachineLearning.Preprocessing;

namespace Vorcyc.Mathematics.MachineLearning.Serialization;

/// <summary>
/// JSON persistence utility for machine learning models.
/// </summary>
public static class ModelJsonPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Serializes a <see cref="StandardScaler{T}"/> to a JSON file at the specified path.
    /// </summary>
    /// <param name="scaler">The scaler instance to save.</param>
    /// <param name="path">The destination file path.</param>
    public static void SaveStandardScaler(StandardScaler<double> scaler, string path)
    {
        var snapshot = new StandardScalerSnapshot
        {
            Mean = scaler.Mean.Select(v => (double)v).ToArray(),
            Std = scaler.Std.Select(v => (double)v).ToArray()
        };
        File.WriteAllText(path, JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    /// <summary>
    /// Deserializes a <see cref="StandardScaler{T}"/> from a JSON file at the specified path.
    /// </summary>
    /// <param name="path">The source file path.</param>
    /// <returns>A <see cref="StandardScaler{T}"/> restored from the saved state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be deserialized.</exception>
    public static StandardScaler<double> LoadStandardScaler(string path)
    {
        var snapshot = JsonSerializer.Deserialize<StandardScalerSnapshot>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Unable to deserialize StandardScaler.");
        var scaler = new StandardScaler<double>();
        scaler.LoadState(snapshot.Mean, snapshot.Std);
        return scaler;
    }

    /// <summary>
    /// Serializes a <see cref="SoftmaxRegression{T}"/> model to a JSON file at the specified path.
    /// </summary>
    /// <param name="model">The model instance to save.</param>
    /// <param name="path">The destination file path.</param>
    public static void SaveSoftmaxRegression(SoftmaxRegression<double> model, string path)
    {
        var snapshot = model.CaptureSnapshot();
        File.WriteAllText(path, JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    /// <summary>
    /// Deserializes a <see cref="SoftmaxRegression{T}"/> model from a JSON file at the specified path.
    /// </summary>
    /// <param name="path">The source file path.</param>
    /// <returns>A <see cref="SoftmaxRegression{T}"/> restored from the saved state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be deserialized.</exception>
    public static SoftmaxRegression<double> LoadSoftmaxRegression(string path)
    {
        var snapshot = JsonSerializer.Deserialize<SoftmaxRegressionSnapshot>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Unable to deserialize SoftmaxRegression.");
        var model = new SoftmaxRegression<double>();
        model.RestoreFromSnapshot(snapshot);
        return model;
    }
}
