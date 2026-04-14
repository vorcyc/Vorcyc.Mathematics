namespace Vorcyc.Mathematics.DeepLearning.Training;

using System.Numerics;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning.Optimizers;

/// <summary>
/// Selects the optimizer used by <see cref="MlpRegressor"/>.
/// </summary>
public enum MlpOptimizerKind
{
    /// <summary>Stochastic gradient descent.</summary>
    Sgd,

    /// <summary>Adam optimizer.</summary>
    Adam
}

/// <summary>
/// Training options for curve-fitting MLPs.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class MlpTrainingOptions<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>Gets or sets the optimizer kind.</summary>
    public MlpOptimizerKind OptimizerKind { get; set; } = MlpOptimizerKind.Sgd;

    /// <summary>Gets or sets the initial learning rate.</summary>
    public T InitialLearningRate { get; set; } = T.CreateChecked(0.1);

    /// <summary>Gets or sets the SGD momentum. Zero disables momentum.</summary>
    public T SgdMomentum { get; set; } = T.Zero;

    /// <summary>Gets or sets Adam beta1.</summary>
    public T AdamBeta1 { get; set; } = T.CreateChecked(0.9);

    /// <summary>Gets or sets Adam beta2.</summary>
    public T AdamBeta2 { get; set; } = T.CreateChecked(0.999);

    /// <summary>Gets or sets Adam epsilon.</summary>
    public T AdamEpsilon { get; set; } = T.CreateChecked(1e-8);

    /// <summary>Gets or sets an optional learning-rate scheduler.</summary>
    public ILearningRateScheduler<T>? LearningRateScheduler { get; set; }

    /// <summary>Gets or sets an optional seed for reproducible weight initialization.</summary>
    public int? RandomSeed { get; set; }

    /// <summary>Gets or sets an optional execution policy for batched numerical kernels during training.</summary>
    public ComputingContext? ComputingContext { get; set; }

    /// <summary>
    /// Creates an optimizer from the current options.
    /// </summary>
    public IOptimizer<T> CreateOptimizer()
    {
        return OptimizerKind switch
        {
            MlpOptimizerKind.Adam => new AdamOptimizer<T>(InitialLearningRate, AdamBeta1, AdamBeta2, AdamEpsilon),
            _ => new SgdOptimizer<T>(InitialLearningRate, SgdMomentum)
        };
    }
}
