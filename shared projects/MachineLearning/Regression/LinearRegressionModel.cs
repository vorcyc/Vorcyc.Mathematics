using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// Shared logic for a multivariate affine regression model (intercept + coefficients).
/// </summary>
internal static class LinearRegressionModel
{
    public static void ValidateTrainingData<T>(T[,]? x, T[]? y)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || y.Length == 0)
            throw new ArgumentException("Training data cannot be empty.");
        if (rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");
    }

    public static void ApplyDesignSolution<T>(
        T[] solution,
        int featureCount,
        out T intercept,
        out T[] coefficients)
        where T : struct, IFloatingPointIeee754<T>
    {
        intercept = solution[0];
        coefficients = new T[featureCount];
        Array.Copy(solution, 1, coefficients, 0, featureCount);
    }

    public static T PredictAffine<T>(T intercept, ReadOnlySpan<T> coefficients, T[] x)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (x == null || x.Length != coefficients.Length)
            throw new ArgumentException("Feature dimension does not match the model.", nameof(x));

        return intercept + NumericKernels.Dot(coefficients, x);
    }

    public static void PredictAffineBatch<T>(
        T intercept,
        ReadOnlySpan<T> coefficients,
        T[,] x,
        Span<T> predictions,
        ComputingContext? context = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (cols != coefficients.Length)
            throw new ArgumentException("Feature dimension does not match the model.");
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        if (ComputingContextExecution.UseParallelIndexed(context, rows, cols))
        {
            var localCoefficients = coefficients.ToArray();
            var buffer = new T[rows];
            ComputingContextExecution.ForEach(
                context,
                0,
                rows,
                i => buffer[i] = intercept + NumericKernels.DotRow(x, i, localCoefficients),
                workPerItem: cols);
            new ReadOnlySpan<T>(buffer).CopyTo(predictions);
            return;
        }

        for (int i = 0; i < rows; i++)
            predictions[i] = intercept + NumericKernels.DotRow(x, i, coefficients);
    }
}
