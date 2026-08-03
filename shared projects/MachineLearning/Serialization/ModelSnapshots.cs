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
/// Ordered chain of fitted <see cref="StandardScalerSnapshot"/> stages (pipeline preprocessing).
/// </summary>
public sealed class StandardScalerChainSnapshot
{
    /// <summary>Ordered scaler stages applied left-to-right.</summary>
    public StandardScalerSnapshot[] Scalers { get; set; } = [];
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

    /// <summary>
    /// Gets or sets the mini-batch size used during training (&lt;=0 = full batch).
    /// </summary>
    public int BatchSize { get; set; }

    /// <summary>
    /// Gets or sets the mini-batch shuffle seed (null when non-deterministic).
    /// </summary>
    public int? Seed { get; set; }
}

/// <summary>
/// Serializable snapshot of a fitted k-nearest-neighbors classifier (prototype set).
/// </summary>
public sealed class KnnClassifierSnapshot
{
    /// <summary>Number of neighbors k.</summary>
    public int K { get; set; }

    /// <summary>Training feature rows (prototypes).</summary>
    public double[][] Features { get; set; } = [];

    /// <summary>Training labels aligned with <see cref="Features"/>.</summary>
    public int[] Labels { get; set; } = [];
}

/// <summary>
/// Serializable node in a numeric CART decision tree.
/// </summary>
public sealed class DecisionTreeNodeSnapshot
{
    /// <summary>Whether this node is a leaf.</summary>
    public bool IsLeaf { get; set; }

    /// <summary>Predicted class at a leaf.</summary>
    public int PredictedClass { get; set; }

    /// <summary>Split feature index for an internal node.</summary>
    public int FeatureIndex { get; set; }

    /// <summary>Split threshold for an internal node.</summary>
    public double Threshold { get; set; }

    /// <summary>Left child (values ≤ threshold).</summary>
    public DecisionTreeNodeSnapshot? Left { get; set; }

    /// <summary>Right child (values &gt; threshold).</summary>
    public DecisionTreeNodeSnapshot? Right { get; set; }
}

/// <summary>
/// Serializable snapshot of a fitted <c>NumericDecisionTree</c>.
/// </summary>
public sealed class NumericDecisionTreeSnapshot
{
    /// <summary>Maximum tree depth hyperparameter.</summary>
    public int MaxDepth { get; set; }

    /// <summary>Minimum samples to split hyperparameter.</summary>
    public int MinSamplesSplit { get; set; }

    /// <summary>Root node of the fitted tree.</summary>
    public DecisionTreeNodeSnapshot? Root { get; set; }
}

/// <summary>
/// One tree in a random forest, including the projected feature indices.
/// </summary>
public sealed class ForestTreeSnapshot
{
    /// <summary>Feature indices selected for this tree (into the original feature space).</summary>
    public int[] FeatureIndices { get; set; } = [];

    /// <summary>The fitted decision tree on the projected features.</summary>
    public NumericDecisionTreeSnapshot Tree { get; set; } = new();
}

/// <summary>
/// Serializable snapshot of a fitted <c>NumericRandomForest</c>.
/// </summary>
public sealed class NumericRandomForestSnapshot
{
    /// <summary>Number of trees hyperparameter.</summary>
    public int NumTrees { get; set; }

    /// <summary>Max features per tree hyperparameter (0 = √d default at fit time).</summary>
    public int MaxFeatures { get; set; }

    /// <summary>Maximum tree depth hyperparameter.</summary>
    public int MaxDepth { get; set; }

    /// <summary>Minimum samples to split hyperparameter.</summary>
    public int MinSamplesSplit { get; set; }

    /// <summary>Optional seed used at construction.</summary>
    public int? Seed { get; set; }

    /// <summary>Fitted trees.</summary>
    public ForestTreeSnapshot[] Trees { get; set; } = [];
}

/// <summary>
/// Serializable snapshot of a fitted support vector machine (weights / dual coefficients + support vectors).
/// </summary>
public sealed class SupportVectorMachineSnapshot
{
    /// <summary>Feature dimensionality.</summary>
    public int FeatureCount { get; set; }

    /// <summary>Learning rate.</summary>
    public double LearningRate { get; set; }

    /// <summary>Training epochs.</summary>
    public int Epochs { get; set; }

    /// <summary>Kernel type.</summary>
    public SupportVectorMachineKernelType KernelType { get; set; }

    /// <summary>RBF / polynomial gamma.</summary>
    public double Gamma { get; set; }

    /// <summary>Polynomial degree.</summary>
    public int PolynomialDegree { get; set; }

    /// <summary>Sigmoid kernel alpha.</summary>
    public double SigmoidAlpha { get; set; }

    /// <summary>Sigmoid kernel constant.</summary>
    public double SigmoidConstant { get; set; }

    /// <summary>Linear-kernel weight vector.</summary>
    public double[] Weights { get; set; } = [];

    /// <summary>Bias term.</summary>
    public double Bias { get; set; }

    /// <summary>Training inputs retained for dual (non-linear) prediction; may be empty for linear.</summary>
    public double[][] TrainingInputs { get; set; } = [];

    /// <summary>Training labels (−1 / +1) aligned with <see cref="TrainingInputs"/>.</summary>
    public int[] TrainingLabels { get; set; } = [];

    /// <summary>Dual coefficients (α) aligned with training samples.</summary>
    public double[] Alphas { get; set; } = [];
}
