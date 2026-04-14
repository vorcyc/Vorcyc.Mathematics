namespace Vorcyc.Mathematics.DeepLearning.Training;



using System.Numerics;

using Vorcyc.Mathematics.DeepLearning.Losses;

using Vorcyc.Mathematics.DeepLearning.Modules;

using Vorcyc.Mathematics.DeepLearning.Optimizers;

using Vorcyc.Mathematics.LinearAlgebra;



/// <summary>

/// MLP training helpers used by curve fitting and regression tasks.

/// </summary>

public static class MlpRegressor

{

    /// <summary>

    /// Creates a two-hidden-layer-style regression MLP: FC → Sigmoid → FC → Sigmoid.

    /// </summary>

    public static Sequential<T> CreateRegressionNetwork<T>(int inputSize, int hiddenSize, int outputSize = 1)

        where T : unmanaged, IBinaryFloatingPointIeee754<T>

        => CreateRegressionNetwork<T>(inputSize, hiddenSize, outputSize, random: null);

    /// <summary>
    /// Creates a two-hidden-layer-style regression MLP with optional deterministic initialization.
    /// </summary>
    public static Sequential<T> CreateRegressionNetwork<T>(
        int inputSize,
        int hiddenSize,
        int outputSize,
        Random? random)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        return new Sequential<T>(
            new FullyConnectedLayer<T>(inputSize, hiddenSize, "fc1", random),
            new SigmoidActivation<T>("sigmoid1"),
            new FullyConnectedLayer<T>(hiddenSize, outputSize, "fc2", random),
            new SigmoidActivation<T>("sigmoid2"));
    }



    /// <summary>

    /// Trains a regression network on batched input/target tensors using full-batch gradient descent.

    /// </summary>

    public static void TrainBatched<T>(

        Sequential<T> model,

        Tensor<T> inputs,

        Tensor<T> targets,

        T learningRate,

        int epochs,

        IOptimizer<T>? optimizer = null,

        Action<int, int, T>? progressCallback = null)

        where T : unmanaged, IBinaryFloatingPointIeee754<T>

    {

        var options = new MlpTrainingOptions<T> { InitialLearningRate = learningRate };

        TrainBatched(model, inputs, targets, epochs, options, progressCallback);

    }



    /// <summary>

    /// Trains a regression network with optimizer and learning-rate scheduling options.

    /// </summary>

    public static void TrainBatched<T>(

        Sequential<T> model,

        Tensor<T> inputs,

        Tensor<T> targets,

        int epochs,

        MlpTrainingOptions<T>? options = null,

        Action<int, int, T>? progressCallback = null)

        where T : unmanaged, IBinaryFloatingPointIeee754<T>

    {

        options ??= new MlpTrainingOptions<T>();

        var optimizer = options.CreateOptimizer();

        var scheduler = options.LearningRateScheduler ?? new ConstantLearningRateScheduler<T>(options.InitialLearningRate);

        var trainer = new Trainer<T>();

        var loss = new MeanSquaredErrorLoss<T>();

        var batch = new TrainingSample<T>(inputs, targets);



        trainer.FitBatched(

            model,

            loss,

            optimizer,

            [batch],

            epochs,

            learningRateScheduler: scheduler,

            computingContext: options.ComputingContext,

            onEpochEnd: (epoch, avgLoss) => progressCallback?.Invoke(epoch, epochs, avgLoss));

    }



    /// <summary>

    /// Computes mean squared error over batched predictions.

    /// </summary>

    public static T ComputeMeanSquaredError<T>(Sequential<T> model, Tensor<T> inputs, Tensor<T> targets)

        where T : unmanaged, IBinaryFloatingPointIeee754<T>

    {

        var predictions = model.Forward(inputs, training: false);

        return new MeanSquaredErrorLoss<T>().Compute(predictions, targets);

    }

}


