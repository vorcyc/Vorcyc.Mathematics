namespace Vorcyc.Mathematics.MachineLearning.Serialization;

/// <summary>
/// StandardScaler 持久化快照。
/// </summary>
public sealed class StandardScalerSnapshot
{
    public double[] Mean { get; set; } = [];
    public double[] Std { get; set; } = [];
}

/// <summary>
/// SoftmaxRegression 持久化快照。
/// </summary>
public sealed class SoftmaxRegressionSnapshot
{
    public int NumClasses { get; set; }
    public double[][] Weights { get; set; } = [];
    public double[] Biases { get; set; } = [];
    public double LearningRate { get; set; }
    public int Epochs { get; set; }
    public double Lambda { get; set; }
}
