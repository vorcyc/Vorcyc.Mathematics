namespace Vorcyc.Mathematics.MachineLearning.Serialization;

/// <summary>
/// Serializable persistence snapshot for a <c>StandardScaler</c> preprocessor,
/// capturing the per-feature statistics required to reproduce the standardization transform.
/// </summary>
public sealed class StandardScalerSnapshot
{
    /// <summary>
    /// Gets or sets the per-feature mean values learned during fitting, used to center each feature.
    /// </summary>
    public double[] Mean { get; set; } = [];

    /// <summary>
    /// Gets or sets the per-feature standard deviation values learned during fitting, used to scale each feature.
    /// </summary>
    public double[] Std { get; set; } = [];
}

/// <summary>
/// Serializable persistence snapshot for a <c>SoftmaxRegression</c> classifier,
/// capturing the trained parameters and hyperparameters required to restore the model.
/// </summary>
public sealed class SoftmaxRegressionSnapshot
{
    /// <summary>
    /// Gets or sets the number of output classes the model was trained to predict.
    /// </summary>
    public int NumClasses { get; set; }

    /// <summary>
    /// Gets or sets the trained weight matrix, organized as one weight vector per class (jagged array of feature weights).
    /// </summary>
    public double[][] Weights { get; set; } = [];

    /// <summary>
    /// Gets or sets the trained bias term for each class.
    /// </summary>
    public double[] Biases { get; set; } = [];

    /// <summary>
    /// Gets or sets the learning rate used during training.
    /// </summary>
    public double LearningRate { get; set; }

    /// <summary>
    /// Gets or sets the number of training epochs performed.
    /// </summary>
    public int Epochs { get; set; }

    /// <summary>
    /// Gets or sets the L2 regularization strength applied during training.
    /// </summary>
    public double Lambda { get; set; }
}
