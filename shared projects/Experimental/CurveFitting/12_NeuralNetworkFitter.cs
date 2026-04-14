using System.Numerics;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Serialization;
using Vorcyc.Mathematics.DeepLearning.Training;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class NeuralNetworkFitter
{
    internal static FitResult<T> Fit_SingleColumn<T>(
        Span<T> xData, Span<T> yData, int epochs = 5000, int hiddenNodes = 10, T? learningRate = null,
        TrainingProgressHandler<T>? trainingProgressCallback = null,
        NeuralNetworkTrainingOptions? trainingOptions = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (typeof(T) == typeof(float))
        {
            var x = MemoryMarshal.Cast<T, float>(xData);
            var y = MemoryMarshal.Cast<T, float>(yData);
            TrainingProgressHandler<float>? callback = trainingProgressCallback is null
                ? null
                : (epoch, total, error) => trainingProgressCallback(epoch, total, T.CreateChecked(error));
            var options = BuildFloatOptions(learningRate, trainingOptions);
            var result = Fit_SingleColumnFloat(x, y, epochs, hiddenNodes, options, callback);
            return ToGenericFromFloat<T>(result);
        }

        if (typeof(T) == typeof(double))
        {
            var x = MemoryMarshal.Cast<T, double>(xData);
            var y = MemoryMarshal.Cast<T, double>(yData);
            TrainingProgressHandler<double>? callback = trainingProgressCallback is null
                ? null
                : (epoch, total, error) => trainingProgressCallback(epoch, total, T.CreateChecked(error));
            var options = BuildDoubleOptions(learningRate, trainingOptions);
            var result = Fit_SingleColumnDouble(x, y, epochs, hiddenNodes, options, callback);
            return ToGenericFromDouble<T>(result);
        }

        throw new NotSupportedException("Only float and double are supported for neural network fitting.");
    }

    internal static MultiColumnFitResult<T> Fit_MultiColumn<T>(
        DataRow<T>[] xData, Span<T> yData, int epochs = 5000, int hiddenNodes = 10, T? learningRate = null,
        TrainingProgressHandler<T>? trainingProgressCallback = null,
        NeuralNetworkTrainingOptions? trainingOptions = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (typeof(T) == typeof(float))
        {
            var rows = Array.ConvertAll(xData, row => new DataRow<float>(
                Array.ConvertAll(row.ToArray(), v => float.CreateTruncating(v))));
            var y = MemoryMarshal.Cast<T, float>(yData);
            TrainingProgressHandler<float>? callback = trainingProgressCallback is null
                ? null
                : (epoch, total, error) => trainingProgressCallback(epoch, total, T.CreateChecked(error));
            var options = BuildFloatOptions(learningRate, trainingOptions);
            var result = Fit_MultiColumnFloat(rows, y, epochs, hiddenNodes, options, callback);
            return ToGenericMultiFromFloat<T>(result);
        }

        if (typeof(T) == typeof(double))
        {
            var rows = Array.ConvertAll(xData, row => new DataRow<double>(
                Array.ConvertAll(row.ToArray(), v => double.CreateTruncating(v))));
            var y = MemoryMarshal.Cast<T, double>(yData);
            TrainingProgressHandler<double>? callback = trainingProgressCallback is null
                ? null
                : (epoch, total, error) => trainingProgressCallback(epoch, total, T.CreateChecked(error));
            var options = BuildDoubleOptions(learningRate, trainingOptions);
            var result = Fit_MultiColumnDouble(rows, y, epochs, hiddenNodes, options, callback);
            return ToGenericMultiFromDouble<T>(result);
        }

        throw new NotSupportedException("Only float and double are supported for neural network fitting.");
    }

    private static MlpTrainingOptions<float> BuildFloatOptions(float? learningRate, NeuralNetworkTrainingOptions? trainingOptions)
    {
        var options = trainingOptions?.ToFloatOptions() ?? new MlpTrainingOptions<float>();
        if (learningRate is not null)
        {
            options.InitialLearningRate = learningRate.Value;
            options.LearningRateScheduler = new ConstantLearningRateScheduler<float>(learningRate.Value);
        }

        return options;
    }

    private static MlpTrainingOptions<double> BuildDoubleOptions(double? learningRate, NeuralNetworkTrainingOptions? trainingOptions)
    {
        var options = trainingOptions?.ToDoubleOptions() ?? new MlpTrainingOptions<double>();
        if (learningRate is not null)
        {
            options.InitialLearningRate = learningRate.Value;
            options.LearningRateScheduler = new ConstantLearningRateScheduler<double>(learningRate.Value);
        }

        return options;
    }

    private static MlpTrainingOptions<float> BuildFloatOptions<T>(T? learningRate, NeuralNetworkTrainingOptions? trainingOptions)
        where T : unmanaged, IFloatingPointIeee754<T>
        => BuildFloatOptions(learningRate is null ? null : float.CreateTruncating(learningRate.Value), trainingOptions);

    private static MlpTrainingOptions<double> BuildDoubleOptions<T>(T? learningRate, NeuralNetworkTrainingOptions? trainingOptions)
        where T : unmanaged, IFloatingPointIeee754<T>
        => BuildDoubleOptions(learningRate is null ? null : double.CreateTruncating(learningRate.Value), trainingOptions);

    private static Random? CreateRandom<T>(MlpTrainingOptions<T> options)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => options.RandomSeed is int seed ? new Random(seed) : null;

    private static FitResult<float> Fit_SingleColumnFloat(
        Span<float> xData, Span<float> yData, int epochs, int hiddenNodes, MlpTrainingOptions<float> options,
        TrainingProgressHandler<float>? trainingProgressCallback)
    {
        if (xData.Length != yData.Length)
        {
            throw new ArgumentException("xData and yData must have the same length");
        }

        int sampleSize = xData.Length;
        var inputs = TensorUtilities.FromBatchVectors(xData, sampleSize, 1);
        var targets = TensorUtilities.FromBatchVectors(yData, sampleSize, 1);
        var model = MlpRegressor.CreateRegressionNetwork<float>(1, hiddenNodes, 1, CreateRandom(options));
        MlpRegressor.TrainBatched(model, inputs, targets, epochs, options,
            (epoch, total, error) => trainingProgressCallback?.Invoke(epoch, total, error));

        Func<float, float> predict = x => model.Forward(TensorUtilities.FromVector(x), training: false)[0, 0, 0];
        float mse = float.CreateTruncating(MlpRegressor.ComputeMeanSquaredError(model, inputs, targets));
        return new FitResult<float>(predict, ModelSerializer.FlattenParameters(model), mse);
    }

    private static FitResult<double> Fit_SingleColumnDouble(
        Span<double> xData, Span<double> yData, int epochs, int hiddenNodes, MlpTrainingOptions<double> options,
        TrainingProgressHandler<double>? trainingProgressCallback)
    {
        if (xData.Length != yData.Length)
        {
            throw new ArgumentException("xData and yData must have the same length");
        }

        int sampleSize = xData.Length;
        var inputs = TensorUtilities.FromBatchVectors(xData, sampleSize, 1);
        var targets = TensorUtilities.FromBatchVectors(yData, sampleSize, 1);
        var model = MlpRegressor.CreateRegressionNetwork<double>(1, hiddenNodes, 1, CreateRandom(options));
        MlpRegressor.TrainBatched(model, inputs, targets, epochs, options,
            (epoch, total, error) => trainingProgressCallback?.Invoke(epoch, total, error));

        Func<double, double> predict = x => double.CreateTruncating(model.Forward(TensorUtilities.FromVector(x), training: false)[0, 0, 0]);
        double mse = double.CreateTruncating(MlpRegressor.ComputeMeanSquaredError(model, inputs, targets));
        return new FitResult<double>(predict, ModelSerializer.FlattenParameters(model), mse);
    }

    private static MultiColumnFitResult<float> Fit_MultiColumnFloat(
        DataRow<float>[] xData, Span<float> yData, int epochs, int hiddenNodes, MlpTrainingOptions<float> options,
        TrainingProgressHandler<float>? trainingProgressCallback)
    {
        if (xData.Length != yData.Length)
        {
            throw new ArgumentException("xData and yData must have the same length");
        }

        int sampleSize = xData.Length;
        int features = xData[0].ColumnCount;
        var flatInputs = new float[sampleSize * features];
        for (int i = 0; i < sampleSize; i++)
        {
            for (int f = 0; f < features; f++)
            {
                flatInputs[i * features + f] = xData[i][f];
            }
        }

        var inputs = TensorUtilities.FromBatchVectors(flatInputs, sampleSize, features);
        var targets = TensorUtilities.FromBatchVectors(yData, sampleSize, 1);
        var model = MlpRegressor.CreateRegressionNetwork<float>(features, hiddenNodes, 1, CreateRandom(options));
        MlpRegressor.TrainBatched(model, inputs, targets, epochs, options,
            (epoch, total, error) => trainingProgressCallback?.Invoke(epoch, total, error));

        Func<DataRow<float>, float> multiPredict = row =>
        {
            var vector = new float[features];
            for (int f = 0; f < features; f++)
            {
                vector[f] = row[f];
            }

            return model.Forward(TensorUtilities.FromVector(vector), training: false)[0, 0, 0];
        };

        float mse = float.CreateTruncating(MlpRegressor.ComputeMeanSquaredError(model, inputs, targets));
        return new MultiColumnFitResult<float>(multiPredict, ModelSerializer.FlattenParameters(model), mse);
    }

    private static MultiColumnFitResult<double> Fit_MultiColumnDouble(
        DataRow<double>[] xData, Span<double> yData, int epochs, int hiddenNodes, MlpTrainingOptions<double> options,
        TrainingProgressHandler<double>? trainingProgressCallback)
    {
        if (xData.Length != yData.Length)
        {
            throw new ArgumentException("xData and yData must have the same length");
        }

        int sampleSize = xData.Length;
        int features = xData[0].ColumnCount;
        var flatInputs = new double[sampleSize * features];
        for (int i = 0; i < sampleSize; i++)
        {
            for (int f = 0; f < features; f++)
            {
                flatInputs[i * features + f] = xData[i][f];
            }
        }

        var inputs = TensorUtilities.FromBatchVectors(flatInputs, sampleSize, features);
        var targets = TensorUtilities.FromBatchVectors(yData, sampleSize, 1);
        var model = MlpRegressor.CreateRegressionNetwork<double>(features, hiddenNodes, 1, CreateRandom(options));
        MlpRegressor.TrainBatched(model, inputs, targets, epochs, options,
            (epoch, total, error) => trainingProgressCallback?.Invoke(epoch, total, error));

        Func<DataRow<double>, double> multiPredict = row =>
        {
            var vector = new double[features];
            for (int f = 0; f < features; f++)
            {
                vector[f] = row[f];
            }

            return double.CreateTruncating(model.Forward(TensorUtilities.FromVector(vector), training: false)[0, 0, 0]);
        };

        double mse = double.CreateTruncating(MlpRegressor.ComputeMeanSquaredError(model, inputs, targets));
        return new MultiColumnFitResult<double>(multiPredict, ModelSerializer.FlattenParameters(model), mse);
    }

    private static FitResult<T> ToGenericFromFloat<T>(FitResult<float> result)
        where T : unmanaged, IFloatingPointIeee754<T>
        => new(
            x => T.CreateChecked(result.Predict(float.CreateTruncating(x))),
            Array.ConvertAll(result.Parameters, p => T.CreateChecked(p)),
            T.CreateChecked(result.MeanSquaredError));

    private static FitResult<T> ToGenericFromDouble<T>(FitResult<double> result)
        where T : unmanaged, IFloatingPointIeee754<T>
        => new(
            x => T.CreateChecked(result.Predict(double.CreateTruncating(x))),
            Array.ConvertAll(result.Parameters, p => T.CreateChecked(p)),
            T.CreateChecked(result.MeanSquaredError));

    private static MultiColumnFitResult<T> ToGenericMultiFromFloat<T>(MultiColumnFitResult<float> result)
        where T : unmanaged, IFloatingPointIeee754<T>
        => new(
            row =>
            {
                var floatRow = new DataRow<float>(Array.ConvertAll(row.ToArray(), v => float.CreateTruncating(v)));
                return T.CreateChecked(result.Predict!(floatRow));
            },
            Array.ConvertAll(result.Parameters, p => T.CreateChecked(p)),
            T.CreateChecked(result.MeanSquaredError));

    private static MultiColumnFitResult<T> ToGenericMultiFromDouble<T>(MultiColumnFitResult<double> result)
        where T : unmanaged, IFloatingPointIeee754<T>
        => new(
            row =>
            {
                var doubleRow = new DataRow<double>(Array.ConvertAll(row.ToArray(), v => double.CreateTruncating(v)));
                return T.CreateChecked(result.Predict!(doubleRow));
            },
            Array.ConvertAll(result.Parameters, p => T.CreateChecked(p)),
            T.CreateChecked(result.MeanSquaredError));
}
