namespace Vorcyc.Mathematics.DeepLearning.Training;

using System.Numerics;

/// <summary>
/// Computes the learning rate for a training epoch.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface ILearningRateScheduler<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Returns the learning rate for the given epoch.
    /// </summary>
    /// <param name="epoch">Current epoch (1-based).</param>
    /// <param name="totalEpochs">Total number of epochs.</param>
    T GetLearningRate(int epoch, int totalEpochs);
}

/// <summary>
/// Keeps the learning rate constant.
/// </summary>
public sealed class ConstantLearningRateScheduler<T> : ILearningRateScheduler<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    public ConstantLearningRateScheduler(T learningRate) => LearningRate = learningRate;

    /// <summary>Gets the constant learning rate.</summary>
    public T LearningRate { get; }

    /// <inheritdoc/>
    public T GetLearningRate(int epoch, int totalEpochs) => LearningRate;
}

/// <summary>
/// Multiplies the learning rate by a factor every fixed number of epochs.
/// </summary>
public sealed class StepDecayScheduler<T> : ILearningRateScheduler<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    public StepDecayScheduler(T initialLearningRate, T decayFactor, int stepEpochs)
    {
        InitialLearningRate = initialLearningRate;
        DecayFactor = decayFactor;
        StepEpochs = stepEpochs;
    }

    public T InitialLearningRate { get; }
    public T DecayFactor { get; }
    public int StepEpochs { get; }

    /// <inheritdoc/>
    public T GetLearningRate(int epoch, int totalEpochs)
    {
        int steps = Math.Max(0, (epoch - 1) / StepEpochs);
        return InitialLearningRate * T.Pow(DecayFactor, T.CreateTruncating(steps));
    }
}

/// <summary>
/// Exponential decay: lr = initial * exp(-decayRate * epoch).
/// </summary>
public sealed class ExponentialDecayScheduler<T> : ILearningRateScheduler<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    public ExponentialDecayScheduler(T initialLearningRate, T decayRate)
    {
        InitialLearningRate = initialLearningRate;
        DecayRate = decayRate;
    }

    public T InitialLearningRate { get; }
    public T DecayRate { get; }

    /// <inheritdoc/>
    public T GetLearningRate(int epoch, int totalEpochs)
        => InitialLearningRate * T.Exp(-DecayRate * T.CreateTruncating(epoch - 1));
}

/// <summary>
/// Cosine annealing from the initial learning rate down to a minimum value.
/// </summary>
public sealed class CosineAnnealingScheduler<T> : ILearningRateScheduler<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    public CosineAnnealingScheduler(T initialLearningRate, T minimumLearningRate)
    {
        InitialLearningRate = initialLearningRate;
        MinimumLearningRate = minimumLearningRate;
    }

    public T InitialLearningRate { get; }
    public T MinimumLearningRate { get; }

    /// <inheritdoc/>
    public T GetLearningRate(int epoch, int totalEpochs)
    {
        if (totalEpochs <= 1)
        {
            return InitialLearningRate;
        }

        var progress = T.CreateTruncating((epoch - 1) / (double)(totalEpochs - 1));
        var cosine = T.Cos(progress * T.Pi);
        return MinimumLearningRate + (InitialLearningRate - MinimumLearningRate) * (T.One + cosine) / T.CreateChecked(2.0);
    }
}
