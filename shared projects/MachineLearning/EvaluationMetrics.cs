using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Common machine learning evaluation metrics.
/// </summary>
public static class EvaluationMetrics
{
    /// <summary>
    /// Computes the mean squared error (MSE).
    /// </summary>
    public static T MeanSquaredError<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> predicted)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("The actual and predicted values must have the same length.");
        if (actual.Length == 0)
            throw new ArgumentException("The input cannot be empty.");

        T sum = T.Zero;
        for (int i = 0; i < actual.Length; i++)
        {
            T diff = actual[i] - predicted[i];
            sum += diff * diff;
        }
        return sum / T.CreateChecked(actual.Length);
    }

    /// <summary>
    /// Computes the root mean squared error (RMSE).
    /// </summary>
    public static T RootMeanSquaredError<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> predicted)
        where T : struct, IFloatingPointIeee754<T>
        => T.Sqrt(MeanSquaredError(actual, predicted));

    /// <summary>
    /// Computes the mean absolute error (MAE).
    /// </summary>
    public static T MeanAbsoluteError<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> predicted)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("The actual and predicted values must have the same length.");
        if (actual.Length == 0)
            throw new ArgumentException("The input cannot be empty.");

        T sum = T.Zero;
        for (int i = 0; i < actual.Length; i++)
            sum += T.Abs(actual[i] - predicted[i]);
        return sum / T.CreateChecked(actual.Length);
    }

    /// <summary>
    /// Computes the classification accuracy.
    /// </summary>
    public static double Accuracy(ReadOnlySpan<string> actual, ReadOnlySpan<string> predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("The actual and predicted labels must have the same length.");
        if (actual.Length == 0)
            throw new ArgumentException("The input cannot be empty.");

        int correct = 0;
        for (int i = 0; i < actual.Length; i++)
        {
            if (actual[i] == predicted[i])
                correct++;
        }
        return (double)correct / actual.Length;
    }

    /// <summary>
    /// Computes the integer-label classification accuracy.
    /// </summary>
    public static double Accuracy(ReadOnlySpan<int> actual, ReadOnlySpan<int> predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("The actual and predicted labels must have the same length.");
        if (actual.Length == 0)
            throw new ArgumentException("The input cannot be empty.");

        int correct = 0;
        for (int i = 0; i < actual.Length; i++)
        {
            if (actual[i] == predicted[i])
                correct++;
        }
        return (double)correct / actual.Length;
    }
}
