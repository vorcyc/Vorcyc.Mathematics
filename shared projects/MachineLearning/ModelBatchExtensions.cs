using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Batch prediction extensions for classifiers and regressors.
/// </summary>
public static class ModelBatchExtensions
{
    /// <summary>
    /// Performs batch classification prediction for each row of the feature matrix.
    /// </summary>
    public static int[] PredictBatch<T>(this IClassifier<T> classifier, T[,] x, ComputingContext? context = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (classifier == null)
            throw new ArgumentNullException(nameof(classifier));
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        var predictions = new int[rows];
        PredictBatch(classifier, x, predictions, context);
        return predictions;
    }

    /// <summary>
    /// Writes the prediction results into <paramref name="predictions"/> (length &#8805; number of rows).
    /// </summary>
    public static void PredictBatch<T>(
        this IClassifier<T> classifier,
        T[,] x,
        Span<int> predictions,
        ComputingContext? context = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (classifier == null)
            throw new ArgumentNullException(nameof(classifier));
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        if (classifier is IBatchClassifier<T> batchClassifier)
        {
            batchClassifier.PredictBatch(x, predictions[..rows]);
            return;
        }

        int cols = x.GetLength(1);
        if (ComputingContextExecution.UseParallelIndexed(context, rows, cols))
        {
            var buffer = new int[rows];
            ComputingContextExecution.ForEach(
                context,
                0,
                rows,
                i =>
                {
                    var sample = new T[cols];
                    for (int j = 0; j < cols; j++)
                        sample[j] = x[i, j];
                    buffer[i] = classifier.Predict(sample);
                },
                workPerItem: cols);
            new ReadOnlySpan<int>(buffer).CopyTo(predictions);
            return;
        }

        var reusable = new T[cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                reusable[j] = x[i, j];
            predictions[i] = classifier.Predict(reusable);
        }
    }

    /// <summary>
    /// Performs batch regression prediction for each row of the feature matrix.
    /// </summary>
    public static T[] PredictBatch<T>(this IRegressor<T> regressor, T[,] x, ComputingContext? context = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (regressor == null)
            throw new ArgumentNullException(nameof(regressor));
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        var predictions = new T[rows];
        PredictBatch(regressor, x, predictions, context);
        return predictions;
    }

    /// <summary>
    /// Writes the regression predictions into <paramref name="predictions"/>.
    /// </summary>
    public static void PredictBatch<T>(
        this IRegressor<T> regressor,
        T[,] x,
        Span<T> predictions,
        ComputingContext? context = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (regressor == null)
            throw new ArgumentNullException(nameof(regressor));
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        if (regressor is IBatchRegressor<T> batchRegressor)
        {
            batchRegressor.PredictBatch(x, predictions[..rows]);
            return;
        }

        int cols = x.GetLength(1);
        if (ComputingContextExecution.UseParallelIndexed(context, rows, cols))
        {
            var buffer = new T[rows];
            ComputingContextExecution.ForEach(
                context,
                0,
                rows,
                i =>
                {
                    var sample = new T[cols];
                    for (int j = 0; j < cols; j++)
                        sample[j] = x[i, j];
                    buffer[i] = regressor.Predict(sample);
                },
                workPerItem: cols);
            new ReadOnlySpan<T>(buffer).CopyTo(predictions);
            return;
        }

        var reusable = new T[cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                reusable[j] = x[i, j];
            predictions[i] = regressor.Predict(reusable);
        }
    }
}

/// <summary>
/// Implementations that support efficient batch classification prediction can declare this interface.
/// </summary>
public interface IBatchClassifier<T> : IClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>Performs batch prediction, writing into <paramref name="predictions"/>.</summary>
    void PredictBatch(T[,] x, Span<int> predictions);
}

/// <summary>
/// Implementations that support efficient batch regression prediction can declare this interface.
/// </summary>
public interface IBatchRegressor<T> : IRegressor<T>
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>Performs batch prediction, writing into <paramref name="predictions"/>.</summary>
    void PredictBatch(T[,] x, Span<T> predictions);
}
