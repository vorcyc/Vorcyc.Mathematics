using System.Text.Json;
using Vorcyc.Mathematics.MachineLearning;
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
        File.WriteAllText(path, JsonSerializer.Serialize(scaler.CaptureSnapshot(), JsonOptions));
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
        scaler.RestoreFromSnapshot(snapshot);
        return scaler;
    }

    /// <summary>
    /// Serializes an ordered chain of fitted <see cref="StandardScaler{T}"/> stages.
    /// </summary>
    public static void SaveStandardScalerChain(IReadOnlyList<StandardScaler<double>> scalers, string path)
    {
        ArgumentNullException.ThrowIfNull(scalers);
        var snapshot = new StandardScalerChainSnapshot
        {
            Scalers = scalers.Select(s => s.CaptureSnapshot()).ToArray()
        };
        File.WriteAllText(path, JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    /// <summary>
    /// Deserializes an ordered chain of <see cref="StandardScaler{T}"/> stages.
    /// </summary>
    public static StandardScaler<double>[] LoadStandardScalerChain(string path)
    {
        var snapshot = JsonSerializer.Deserialize<StandardScalerChainSnapshot>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Unable to deserialize StandardScaler chain.");
        if (snapshot.Scalers == null || snapshot.Scalers.Length == 0)
            throw new InvalidOperationException("Scaler chain snapshot is empty.");

        var result = new StandardScaler<double>[snapshot.Scalers.Length];
        for (int i = 0; i < snapshot.Scalers.Length; i++)
        {
            var scaler = new StandardScaler<double>();
            scaler.RestoreFromSnapshot(snapshot.Scalers[i]);
            result[i] = scaler;
        }
        return result;
    }

    /// <summary>
    /// Applies a fitted scaler chain to a feature matrix (left-to-right).
    /// </summary>
    public static double[,] TransformStandardScalerChain(
        IReadOnlyList<StandardScaler<double>> scalers,
        double[,] x)
    {
        ArgumentNullException.ThrowIfNull(scalers);
        ArgumentNullException.ThrowIfNull(x);
        var current = x;
        foreach (var scaler in scalers)
            current = scaler.Transform(current);
        return current;
    }

    /// <summary>
    /// Applies a fitted scaler chain to a single feature vector (left-to-right).
    /// </summary>
    public static double[] TransformStandardScalerChain(
        IReadOnlyList<StandardScaler<double>> scalers,
        double[] sample)
    {
        ArgumentNullException.ThrowIfNull(scalers);
        ArgumentNullException.ThrowIfNull(sample);
        var current = sample;
        foreach (var scaler in scalers)
            current = scaler.Transform(current);
        return current;
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
        var model = new SoftmaxRegression<double>(
            learningRate: snapshot.LearningRate,
            epochs: snapshot.Epochs,
            lambda: snapshot.Lambda,
            batchSize: snapshot.BatchSize,
            seed: snapshot.Seed);
        model.RestoreFromSnapshot(snapshot);
        return model;
    }

    /// <summary>
    /// Serializes a fitted <see cref="KnnClassifier{T}"/> prototype set.
    /// </summary>
    public static void SaveKnnClassifier(KnnClassifier<double> model, string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(model.CaptureSnapshot(), JsonOptions));
    }

    /// <summary>
    /// Deserializes a <see cref="KnnClassifier{T}"/> from a prototype-set snapshot.
    /// </summary>
    public static KnnClassifier<double> LoadKnnClassifier(string path)
    {
        var snapshot = JsonSerializer.Deserialize<KnnClassifierSnapshot>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Unable to deserialize KnnClassifier.");
        var model = new KnnClassifier<double>(snapshot.K);
        model.RestoreFromSnapshot(snapshot);
        return model;
    }

    /// <summary>
    /// Serializes a fitted <see cref="NumericRandomForest{T}"/>.
    /// </summary>
    public static void SaveNumericRandomForest(NumericRandomForest<double> model, string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(model.CaptureSnapshot(), JsonOptions));
    }

    /// <summary>
    /// Deserializes a <see cref="NumericRandomForest{T}"/>.
    /// </summary>
    public static NumericRandomForest<double> LoadNumericRandomForest(string path)
    {
        var snapshot = JsonSerializer.Deserialize<NumericRandomForestSnapshot>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Unable to deserialize NumericRandomForest.");
        var model = new NumericRandomForest<double>(
            numTrees: Math.Max(1, snapshot.NumTrees),
            maxFeatures: snapshot.MaxFeatures,
            maxDepth: snapshot.MaxDepth > 0 ? snapshot.MaxDepth : 12,
            minSamplesSplit: snapshot.MinSamplesSplit >= 2 ? snapshot.MinSamplesSplit : 2,
            seed: snapshot.Seed);
        model.RestoreFromSnapshot(snapshot);
        return model;
    }

    /// <summary>
    /// Serializes a fitted <see cref="SupportVectorMachine{T}"/> (params + support vectors / dual coefficients).
    /// </summary>
    public static void SaveSupportVectorMachine(SupportVectorMachine<double> model, string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(model.CaptureSnapshot(), JsonOptions));
    }

    /// <summary>
    /// Deserializes a <see cref="SupportVectorMachine{T}"/>.
    /// </summary>
    public static SupportVectorMachine<double> LoadSupportVectorMachine(string path)
    {
        var snapshot = JsonSerializer.Deserialize<SupportVectorMachineSnapshot>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Unable to deserialize SupportVectorMachine.");
        var model = new SupportVectorMachine<double>(
            featureCount: snapshot.FeatureCount,
            learningRate: snapshot.LearningRate,
            epochs: snapshot.Epochs,
            kernelType: snapshot.KernelType,
            gamma: snapshot.Gamma,
            polynomialDegree: snapshot.PolynomialDegree,
            sigmoidAlpha: snapshot.SigmoidAlpha,
            sigmoidConstant: snapshot.SigmoidConstant);
        model.RestoreFromSnapshot(snapshot);
        return model;
    }

    /// <summary>
    /// Serializes a fitted <see cref="IsolationForest{T}"/>.
    /// </summary>
    public static void SaveIsolationForest(IsolationForest<double> model, string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(model.CaptureSnapshot(), JsonOptions));
    }

    /// <summary>
    /// Deserializes a fitted <see cref="IsolationForest{T}"/>.
    /// </summary>
    public static IsolationForest<double> LoadIsolationForest(string path)
    {
        var snapshot = JsonSerializer.Deserialize<IsolationForestSnapshot>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Unable to deserialize IsolationForest.");
        var model = new IsolationForest<double>(
            numTrees: Math.Max(1, snapshot.NumTrees),
            subsampleSize: snapshot.SubsampleSize,
            maxDepth: snapshot.MaxDepth,
            seed: snapshot.Seed);
        model.RestoreFromSnapshot(snapshot);
        return model;
    }
}
