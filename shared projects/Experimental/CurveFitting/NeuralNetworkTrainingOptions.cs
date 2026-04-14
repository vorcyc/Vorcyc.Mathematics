using Vorcyc.Mathematics.DeepLearning.Training;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

/// <summary>
/// Learning-rate schedule presets for curve-fitting neural networks.
/// </summary>
public enum NeuralNetworkSchedulerKind
{
  /// <summary>Constant learning rate.</summary>
    Constant,

    /// <summary>Step decay every N epochs.</summary>
    StepDecay,

    /// <summary>Exponential decay.</summary>
    ExponentialDecay,

    /// <summary>Cosine annealing to a minimum learning rate.</summary>
    CosineAnnealing
}

/// <summary>
/// Training options for <see cref="CurveFitter{T}.NeuralNetwork"/> APIs.
/// </summary>
public sealed class NeuralNetworkTrainingOptions
{
    /// <summary>Gets or sets the optimizer.</summary>
    public MlpOptimizerKind OptimizerKind { get; set; } = MlpOptimizerKind.Sgd;

    /// <summary>Gets or sets the initial learning rate.</summary>
    public double InitialLearningRate { get; set; } = 0.1;

    /// <summary>Gets or sets SGD momentum.</summary>
    public double SgdMomentum { get; set; } = 0.0;

    /// <summary>Gets or sets the scheduler preset.</summary>
    public NeuralNetworkSchedulerKind SchedulerKind { get; set; } = NeuralNetworkSchedulerKind.Constant;

    /// <summary>Gets or sets the multiplicative step-decay factor.</summary>
    public double DecayFactor { get; set; } = 0.5;

    /// <summary>Gets or sets the step-decay period in epochs.</summary>
    public int StepEpochs { get; set; } = 500;

    /// <summary>Gets or sets the exponential decay rate.</summary>
    public double ExponentialDecayRate { get; set; } = 0.01;

    /// <summary>Gets or sets the minimum learning rate for cosine annealing.</summary>
    public double MinimumLearningRate { get; set; } = 1e-5;

    /// <summary>Gets or sets an optional seed for reproducible weight initialization.</summary>
    public int? RandomSeed { get; set; }

    internal MlpTrainingOptions<float> ToFloatOptions()
    {
        return new MlpTrainingOptions<float>
        {
            OptimizerKind = OptimizerKind,
            InitialLearningRate = (float)InitialLearningRate,
            SgdMomentum = (float)SgdMomentum,
            LearningRateScheduler = CreateScheduler<float>(),
            RandomSeed = RandomSeed
        };
    }

    internal MlpTrainingOptions<double> ToDoubleOptions()
    {
        return new MlpTrainingOptions<double>
        {
            OptimizerKind = OptimizerKind,
            InitialLearningRate = InitialLearningRate,
            SgdMomentum = SgdMomentum,
            LearningRateScheduler = CreateScheduler<double>(),
            RandomSeed = RandomSeed
        };
    }

    private ILearningRateScheduler<T> CreateScheduler<T>()
        where T : unmanaged, System.Numerics.IBinaryFloatingPointIeee754<T>
    {
        var initial = T.CreateChecked(InitialLearningRate);
        return SchedulerKind switch
        {
            NeuralNetworkSchedulerKind.StepDecay => new StepDecayScheduler<T>(
                initial,
                T.CreateChecked(DecayFactor),
                StepEpochs),
            NeuralNetworkSchedulerKind.ExponentialDecay => new ExponentialDecayScheduler<T>(
                initial,
                T.CreateChecked(ExponentialDecayRate)),
            NeuralNetworkSchedulerKind.CosineAnnealing => new CosineAnnealingScheduler<T>(
                initial,
                T.CreateChecked(MinimumLearningRate)),
            _ => new ConstantLearningRateScheduler<T>(initial)
        };
    }
}
