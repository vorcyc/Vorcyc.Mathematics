namespace Vorcyc.Mathematics.MachineLearning;

public interface IMachineLearning
{


    MachineLearningTask Task { get; }

}

/// <summary>
/// Represents the kind of machine learning task.
/// </summary>
[Flags]
public enum MachineLearningTask
{
    /// <summary>
    /// No task.
    /// </summary>
    None = 0,

    /// <summary>
    /// Classification task.
    /// </summary>
    Classification = 1 << 0, // 1

    /// <summary>
    /// Clustering task.
    /// </summary>
    Clustering = 1 << 1,     // 2

    /// <summary>
    /// Regression task.
    /// </summary>
    Regression = 1 << 2,     // 4

    /// <summary>
    /// Anomaly detection task.
    /// </summary>
    AnomalyDetection = 1 << 3, // 8

    /// <summary>
    /// Dimensionality reduction task.
    /// </summary>
    DimensionalityReduction = 1 << 4, // 16

    /// <summary>
    /// Blind source separation task (e.g. Independent Component Analysis).
    /// </summary>
    SourceSeparation = 1 << 5 // 32
}
