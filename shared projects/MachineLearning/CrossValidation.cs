using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;
using Vorcyc.Mathematics.MachineLearning.Regression;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Regression K-fold cross-validation result.
/// </summary>
public sealed class RegressionCrossValidationResult
{
    public required double MeanRMSE { get; init; }
    public required double StdRMSE { get; init; }
    public required double MeanR2 { get; init; }
    public required double StdR2 { get; init; }
    public required double[] FoldRmse { get; init; }
    public required double[] FoldR2 { get; init; }
}

/// <summary>
/// K-fold cross-validation result.
/// </summary>
public sealed class CrossValidationResult
{
    public required double MeanAccuracy { get; init; }
    public required double StdAccuracy { get; init; }
    public required double MeanMacroF1 { get; init; }
    public required double StdMacroF1 { get; init; }
    public required double[] FoldAccuracies { get; init; }
    public required double[] FoldMacroF1 { get; init; }
}

/// <summary>
/// K-fold cross-validation utilities.
/// </summary>
public static class CrossValidation
{
    /// <summary>
    /// Generates K-fold indices. Each fold is the set of test-set indices.
    /// </summary>
    public static int[][] KFold(int count, int folds, int? seed = null)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (folds < 2 || folds > count)
            throw new ArgumentOutOfRangeException(nameof(folds), "The number of folds must be within [2, sample count].");

        var shuffled = DataSplit.CreateShuffledIndices(count, seed);
        var result = new int[folds][];
        int baseSize = count / folds;
        int remainder = count % folds;
        int offset = 0;

        for (int fold = 0; fold < folds; fold++)
        {
            int foldSize = baseSize + (fold < remainder ? 1 : 0);
            result[fold] = new int[foldSize];
            Array.Copy(shuffled, offset, result[fold], 0, foldSize);
            offset += foldSize;
        }
        return result;
    }

    /// <summary>
    /// Performs K-fold cross-validation for an integer-label classifier.
    /// </summary>
    public static CrossValidationResult Validate<T>(
        T[,] x,
        int[] y,
        int folds,
        Func<T[,], int[], Func<T[], int>> buildPredictor,
        int? seed = null)
        where T : struct
    {
        if (x == null || y == null || buildPredictor == null)
            throw new ArgumentException("Input cannot be null.");
        int rows = x.GetLength(0);
        if (rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");

        var foldIndices = KFold(rows, folds, seed);
        var accuracies = new double[folds];
        var macroF1 = new double[folds];

        for (int fold = 0; fold < folds; fold++)
        {
            var testSet = new HashSet<int>(foldIndices[fold]);
            var trainIdx = Enumerable.Range(0, rows).Where(i => !testSet.Contains(i)).ToArray();
            var testIdx = foldIndices[fold];

            var xTrain = Array2DHelpers.CopyRows(x, trainIdx);
            var yTrain = Array2DHelpers.CopyIntLabels(y, trainIdx);
            var predictor = buildPredictor(xTrain, yTrain);

            var yTrue = new int[testIdx.Length];
            var yPred = new int[testIdx.Length];
            for (int i = 0; i < testIdx.Length; i++)
            {
                yTrue[i] = y[testIdx[i]];
                yPred[i] = predictor(Array2DHelpers.GetRow(x, testIdx[i]));
            }

            accuracies[fold] = EvaluationMetrics.Accuracy(yTrue, yPred);
            macroF1[fold] = ClassificationMetrics.MacroF1(yTrue, yPred);
        }

        return new CrossValidationResult
        {
            MeanAccuracy = Mean(accuracies),
            StdAccuracy = Std(accuracies),
            MeanMacroF1 = Mean(macroF1),
            StdMacroF1 = Std(macroF1),
            FoldAccuracies = accuracies,
            FoldMacroF1 = macroF1
        };
    }

    /// <summary>
    /// Performs K-fold cross-validation for a regressor.
    /// </summary>
    public static RegressionCrossValidationResult ValidateRegressor<T>(
        T[,] x,
        T[] y,
        int folds,
        Func<T[,], T[], Func<T[], T>> buildPredictor,
        int? seed = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (x == null || y == null || buildPredictor == null)
            throw new ArgumentException("Input cannot be null.");
        int rows = x.GetLength(0);
        if (rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");

        var foldIndices = KFold(rows, folds, seed);
        var rmse = new double[folds];
        var r2 = new double[folds];

        for (int fold = 0; fold < folds; fold++)
        {
            var testSet = new HashSet<int>(foldIndices[fold]);
            var trainIdx = Enumerable.Range(0, rows).Where(i => !testSet.Contains(i)).ToArray();
            var testIdx = foldIndices[fold];

            var xTrain = Array2DHelpers.CopyRows(x, trainIdx);
            var yTrain = Array2DHelpers.CopyLabels(y, trainIdx);
            var predictor = buildPredictor(xTrain, yTrain);

            var yTrue = new T[testIdx.Length];
            var yPred = new T[testIdx.Length];
            for (int i = 0; i < testIdx.Length; i++)
            {
                yTrue[i] = y[testIdx[i]];
                yPred[i] = predictor(Array2DHelpers.GetRow(x, testIdx[i]));
            }

            rmse[fold] = double.CreateTruncating(EvaluationMetrics.RootMeanSquaredError(yTrue, yPred));
            r2[fold] = double.CreateTruncating(
                RegressionMathHelper.ComputeRSquared(yTrue, i => yPred[i]));
        }

        return new RegressionCrossValidationResult
        {
            MeanRMSE = Mean(rmse),
            StdRMSE = Std(rmse),
            MeanR2 = Mean(r2),
            StdR2 = Std(r2),
            FoldRmse = rmse,
            FoldR2 = r2
        };
    }

    private static double Mean(double[] values) => values.Length == 0 ? 0.0 : values.Average();

    private static double Std(double[] values)
    {
        if (values.Length <= 1)
            return 0.0;
        double mean = Mean(values);
        double sum = 0.0;
        foreach (double value in values)
        {
            double diff = value - mean;
            sum += diff * diff;
        }
        return Math.Sqrt(sum / values.Length);
    }
}
