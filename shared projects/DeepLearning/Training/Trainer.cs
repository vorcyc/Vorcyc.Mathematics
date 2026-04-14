namespace Vorcyc.Mathematics.DeepLearning.Training;



using System.Numerics;

using Vorcyc.Mathematics;

using Vorcyc.Mathematics.DeepLearning.Losses;

using Vorcyc.Mathematics.DeepLearning.Optimizers;

using Vorcyc.Mathematics.DeepLearning;



/// <summary>

/// High-level training loop for <see cref="Sequential{T}"/> models.

/// </summary>

/// <typeparam name="T">Element type.</typeparam>

public sealed class Trainer<T>

    where T : unmanaged, IBinaryFloatingPointIeee754<T>

{

    private readonly Random _random = new();



    /// <summary>

    /// Trains a model for the specified number of epochs using individual samples.

    /// </summary>

    public void Fit(

        Sequential<T> model,

        ILoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<TrainingSample<T>> dataset,

        int epochs,

        bool shuffle = true,

        Action<int, T>? onEpochEnd = null,

        ILearningRateScheduler<T>? learningRateScheduler = null,

        ComputingContext? computingContext = null)

    {

        ValidateFitArguments(model, loss, optimizer, dataset, epochs);

        using var _ = CreateComputingScope(computingContext);

        var order = new int[dataset.Count];

        for (int epoch = 1; epoch <= epochs; epoch++)

        {

            ApplyLearningRate(optimizer, learningRateScheduler, epoch, epochs);

            for (int i = 0; i < order.Length; i++)

            {

                order[i] = i;

            }



            if (shuffle)

            {

                Shuffle(order);

            }



            T epochLoss = T.Zero;

            foreach (var index in order)

            {

                epochLoss += TrainStep(model, loss, optimizer, dataset[index]);

            }



            var averageLoss = epochLoss / T.CreateTruncating(dataset.Count);

            onEpochEnd?.Invoke(epoch, averageLoss);

        }

    }



    /// <summary>

    /// Trains a model using batched tensors (1×N×F layout) for each step.

    /// </summary>

    public void FitBatched(

        Sequential<T> model,

        ILoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<TrainingSample<T>> batches,

        int epochs,

        Action<int, T>? onEpochEnd = null,

        ILearningRateScheduler<T>? learningRateScheduler = null,

        ComputingContext? computingContext = null)

    {

        ValidateFitArguments(model, loss, optimizer, batches, epochs);

        using var _ = CreateComputingScope(computingContext);

        for (int epoch = 1; epoch <= epochs; epoch++)

        {

            ApplyLearningRate(optimizer, learningRateScheduler, epoch, epochs);

            T epochLoss = T.Zero;

            foreach (var batch in batches)

            {

                epochLoss += TrainBatchStep(model, loss, optimizer, batch);

            }



            var averageLoss = epochLoss / T.CreateTruncating(batches.Count);

            onEpochEnd?.Invoke(epoch, averageLoss);

        }

    }



    /// <inheritdoc cref="Fit"/>

    public T TrainStep(

        Sequential<T> model,

        ILoss<T> loss,

        IOptimizer<T> optimizer,

        TrainingSample<T> sample)

    {

        optimizer.ZeroGrad(model.Parameters);

        var prediction = model.Forward(sample.Input, training: true);

        var lossValue = loss.Compute(prediction, sample.Target);

        var grad = loss.Backward(prediction, sample.Target);

        model.Backward(grad);

        optimizer.Step(model.Parameters);

        return lossValue;

    }



    /// <summary>

    /// Runs a single forward / backward / optimizer step on a batch tensor.

    /// </summary>

    public T TrainBatchStep(

        Sequential<T> model,

        ILoss<T> loss,

        IOptimizer<T> optimizer,

        TrainingSample<T> batch)

        => TrainStep(model, loss, optimizer, batch);



    /// <summary>

    /// Trains a <see cref="CnnMlpModel{T}"/> using NHWC batch inputs.

    /// </summary>

    public void FitCnnMlp(

        CnnMlpModel<T> model,

        ILoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<BatchTrainingSample<T>> batches,

        int epochs,

        Action<int, T>? onEpochEnd = null,

        ILearningRateScheduler<T>? learningRateScheduler = null,

        ComputingContext? computingContext = null)

    {

        ValidateCnnMlpFitArguments(model, loss, optimizer, batches, epochs);

        using var _ = CreateComputingScope(computingContext);

        for (int epoch = 1; epoch <= epochs; epoch++)

        {

            ApplyLearningRate(optimizer, learningRateScheduler, epoch, epochs);

            T epochLoss = T.Zero;

            foreach (var batch in batches)

            {

                epochLoss += TrainCnnMlpStep(model, loss, optimizer, batch);

            }



            var averageLoss = epochLoss / T.CreateTruncating(batches.Count);

            onEpochEnd?.Invoke(epoch, averageLoss);

        }

    }



    /// <summary>

    /// Runs one training step on a CNN+MLP hybrid model.

    /// </summary>

    public T TrainCnnMlpStep(

        CnnMlpModel<T> model,

        ILoss<T> loss,

        IOptimizer<T> optimizer,

        BatchTrainingSample<T> sample)

    {

        optimizer.ZeroGrad(model.Parameters);

        var prediction = model.Forward(sample.Input, training: true);

        var lossValue = loss.Compute(prediction, sample.Target);

        var grad = loss.Backward(prediction, sample.Target);

        model.Backward(grad);

        optimizer.Step(model.Parameters);

        return lossValue;

    }



    /// <summary>

    /// Trains a <see cref="BatchSequential{T}"/> end-to-end on NHWC batches.

    /// </summary>

    public void FitBatchSequential(

        BatchSequential<T> model,

        IBatchLoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<BatchLabelSample<T>> batches,

        int epochs,

        Action<int, T>? onEpochEnd = null,

        ILearningRateScheduler<T>? learningRateScheduler = null,

        ComputingContext? computingContext = null)

    {

        ValidateBatchSequentialFitArguments(model, loss, optimizer, batches, epochs);

        using var _ = CreateComputingScope(computingContext);

        for (int epoch = 1; epoch <= epochs; epoch++)

        {

            ApplyLearningRate(optimizer, learningRateScheduler, epoch, epochs);

            T epochLoss = T.Zero;

            foreach (var batch in batches)

            {

                epochLoss += TrainBatchSequentialStep(model, loss, optimizer, batch);

            }



            var averageLoss = epochLoss / T.CreateTruncating(batches.Count);

            onEpochEnd?.Invoke(epoch, averageLoss);

        }

    }



    /// <summary>

    /// Runs one training step on a pure NHWC batch model.

    /// </summary>

    public T TrainBatchSequentialStep(

        BatchSequential<T> model,

        IBatchLoss<T> loss,

        IOptimizer<T> optimizer,

        BatchLabelSample<T> sample)

    {

        optimizer.ZeroGrad(model.Parameters);

        var prediction = model.Forward(sample.Input, training: true);

        var lossValue = loss.Compute(prediction, sample.Target);

        var grad = loss.Backward(prediction, sample.Target);

        model.Backward(grad);

        optimizer.Step(model.Parameters);

        return lossValue;

    }



    /// <summary>Trains a <see cref="BatchParallelConcatModel{T}"/> on NHWC batches.</summary>

    public void FitBatchParallelConcat(

        BatchParallelConcatModel<T> model,

        IBatchLoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<BatchLabelSample<T>> batches,

        int epochs,

        Action<int, T>? onEpochEnd = null,

        ILearningRateScheduler<T>? learningRateScheduler = null,

        ComputingContext? computingContext = null)

    {

        ValidateBatchParallelConcatFitArguments(model, loss, optimizer, batches, epochs);

        using var _ = CreateComputingScope(computingContext);

        for (int epoch = 1; epoch <= epochs; epoch++)

        {

            ApplyLearningRate(optimizer, learningRateScheduler, epoch, epochs);

            T epochLoss = T.Zero;

            foreach (var batch in batches)

            {

                epochLoss += TrainBatchParallelConcatStep(model, loss, optimizer, batch);

            }

            onEpochEnd?.Invoke(epoch, epochLoss / T.CreateTruncating(batches.Count));

        }

    }



    /// <summary>Trains a batch model using integer class labels.</summary>

    public void FitBatchSequential(

        BatchSequential<T> model,

        ISparseBatchLoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<BatchClassLabelSample<T>> batches,

        int epochs,

        Action<int, T>? onEpochEnd = null,

        ILearningRateScheduler<T>? learningRateScheduler = null,

        ComputingContext? computingContext = null)

    {

        ValidateBatchClassFitArguments(model, loss, optimizer, batches, epochs);

        using var _ = CreateComputingScope(computingContext);

        for (int epoch = 1; epoch <= epochs; epoch++)

        {

            ApplyLearningRate(optimizer, learningRateScheduler, epoch, epochs);

            T epochLoss = T.Zero;

            foreach (var batch in batches)

            {

                epochLoss += TrainBatchSequentialClassStep(model, loss, optimizer, batch);

            }

            onEpochEnd?.Invoke(epoch, epochLoss / T.CreateTruncating(batches.Count));

        }

    }



    /// <summary>Runs one training step with integer class labels.</summary>

    public T TrainBatchSequentialClassStep(

        BatchSequential<T> model,

        ISparseBatchLoss<T> loss,

        IOptimizer<T> optimizer,

        BatchClassLabelSample<T> sample)

    {

        optimizer.ZeroGrad(model.Parameters);

        var prediction = model.Forward(sample.Input, training: true);

        var lossValue = loss.ComputeFromClassIndices(prediction, sample.ClassIndices);

        var grad = loss.BackwardFromClassIndices(prediction, sample.ClassIndices);

        model.Backward(grad);

        optimizer.Step(model.Parameters);

        return lossValue;

    }



    /// <summary>Runs one training step on a parallel-concat model.</summary>

    public T TrainBatchParallelConcatStep(

        BatchParallelConcatModel<T> model,

        IBatchLoss<T> loss,

        IOptimizer<T> optimizer,

        BatchLabelSample<T> sample)

    {

        optimizer.ZeroGrad(model.Parameters);

        var prediction = model.Forward(sample.Input, training: true);

        var lossValue = loss.Compute(prediction, sample.Target);

        var grad = loss.Backward(prediction, sample.Target);

        model.Backward(grad);

        optimizer.Step(model.Parameters);

        return lossValue;

    }



    private static void ApplyLearningRate(

        IOptimizer<T> optimizer,

        ILearningRateScheduler<T>? scheduler,

        int epoch,

        int totalEpochs)

    {

        if (scheduler is not null)

        {

            optimizer.SetLearningRate(scheduler.GetLearningRate(epoch, totalEpochs));

        }

    }



    private static void ValidateFitArguments(

        Sequential<T> model,

        ILoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<TrainingSample<T>> dataset,

        int epochs)

    {

        ArgumentNullException.ThrowIfNull(model);

        ArgumentNullException.ThrowIfNull(loss);

        ArgumentNullException.ThrowIfNull(optimizer);

        ArgumentNullException.ThrowIfNull(dataset);



        if (epochs <= 0)

        {

            throw new ArgumentOutOfRangeException(nameof(epochs));

        }



        if (dataset.Count == 0)

        {

            throw new ArgumentException("Dataset must contain at least one sample.", nameof(dataset));

        }

    }



    private static void ValidateBatchSequentialFitArguments(

        BatchSequential<T> model,

        IBatchLoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<BatchLabelSample<T>> batches,

        int epochs)

    {

        ArgumentNullException.ThrowIfNull(model);

        ArgumentNullException.ThrowIfNull(loss);

        ArgumentNullException.ThrowIfNull(optimizer);

        ArgumentNullException.ThrowIfNull(batches);



        if (epochs <= 0)

        {

            throw new ArgumentOutOfRangeException(nameof(epochs));

        }



        if (batches.Count == 0)

        {

            throw new ArgumentException("Batches must contain at least one item.", nameof(batches));

        }

    }



    private static void ValidateBatchParallelConcatFitArguments(

        BatchParallelConcatModel<T> model,

        IBatchLoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<BatchLabelSample<T>> batches,

        int epochs)

    {

        ArgumentNullException.ThrowIfNull(model);

        ArgumentNullException.ThrowIfNull(loss);

        ArgumentNullException.ThrowIfNull(optimizer);

        ArgumentNullException.ThrowIfNull(batches);



        if (epochs <= 0)

        {

            throw new ArgumentOutOfRangeException(nameof(epochs));

        }



        if (batches.Count == 0)

        {

            throw new ArgumentException("Batches must contain at least one item.", nameof(batches));

        }

    }



    private static void ValidateBatchClassFitArguments(

        BatchSequential<T> model,

        ISparseBatchLoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<BatchClassLabelSample<T>> batches,

        int epochs)

    {

        ArgumentNullException.ThrowIfNull(model);

        ArgumentNullException.ThrowIfNull(loss);

        ArgumentNullException.ThrowIfNull(optimizer);

        ArgumentNullException.ThrowIfNull(batches);



        if (epochs <= 0)

        {

            throw new ArgumentOutOfRangeException(nameof(epochs));

        }



        if (batches.Count == 0)

        {

            throw new ArgumentException("Batches must contain at least one item.", nameof(batches));

        }

    }



    private static void ValidateCnnMlpFitArguments(

        CnnMlpModel<T> model,

        ILoss<T> loss,

        IOptimizer<T> optimizer,

        IReadOnlyList<BatchTrainingSample<T>> batches,

        int epochs)

    {

        ArgumentNullException.ThrowIfNull(model);

        ArgumentNullException.ThrowIfNull(loss);

        ArgumentNullException.ThrowIfNull(optimizer);

        ArgumentNullException.ThrowIfNull(batches);



        if (epochs <= 0)

        {

            throw new ArgumentOutOfRangeException(nameof(epochs));

        }



        if (batches.Count == 0)

        {

            throw new ArgumentException("Batches must contain at least one item.", nameof(batches));

        }

    }



    private void Shuffle(int[] indices)

    {

        for (int i = indices.Length - 1; i > 0; i--)

        {

            int j = _random.Next(i + 1);

            (indices[i], indices[j]) = (indices[j], indices[i]);

        }

    }

    private static IDisposable? CreateComputingScope(ComputingContext? context)
        => context is not null ? ComputingScope.Enter(context) : null;

}


