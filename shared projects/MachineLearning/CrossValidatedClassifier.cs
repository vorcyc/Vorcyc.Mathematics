using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Classifier that keeps fold scores and out-of-fold predictions, then refits on every sample so <see cref="Predict"/> is available.
/// </summary>
public sealed class CrossValidatedClassifier<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly Func<IClassifier<T>> _factory;
    private readonly int _folds;
    private readonly int? _seed;
    private IClassifier<T>? _model;

    public CrossValidatedClassifier(Func<IClassifier<T>> factory, int folds = 5, int? seed = null)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        if (folds < 2)
            throw new ArgumentOutOfRangeException(nameof(folds));
        _folds = folds;
        _seed = seed;
    }

    public MachineLearningTask Task => MachineLearningTask.Classification;

    public ComputingContext? Context { get; set; }

    public CrossValidationResult? Validation { get; private set; }

    public int[]? OutOfFoldPredictions { get; private set; }

    public void Fit(T[,] x, int[] y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        int rows = x.GetLength(0);
        if (rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");

        var foldIndices = CrossValidation.KFold(rows, _folds, _seed);
        var outOfFold = new int[rows];
        var accuracies = new double[_folds];
        var macroF1 = new double[_folds];

        for (int fold = 0; fold < _folds; fold++)
        {
            var testSet = new HashSet<int>(foldIndices[fold]);
            var trainIndex = Enumerable.Range(0, rows).Where(i => !testSet.Contains(i)).ToArray();
            var testIndex = foldIndices[fold];
            var model = _factory();
            model.Fit(Array2DHelpers.CopyRows(x, trainIndex), Array2DHelpers.CopyIntLabels(y, trainIndex));

            var truth = new int[testIndex.Length];
            var predicted = new int[testIndex.Length];
            for (int i = 0; i < testIndex.Length; i++)
            {
                int row = testIndex[i];
                truth[i] = y[row];
                predicted[i] = model.Predict(Array2DHelpers.GetRow(x, row));
                outOfFold[row] = predicted[i];
            }

            accuracies[fold] = EvaluationMetrics.Accuracy(truth, predicted);
            macroF1[fold] = ClassificationMetrics.MacroF1(truth, predicted);
        }

        Validation = new CrossValidationResult
        {
            MeanAccuracy = accuracies.Average(),
            StdAccuracy = StandardDeviation(accuracies),
            MeanMacroF1 = macroF1.Average(),
            StdMacroF1 = StandardDeviation(macroF1),
            FoldAccuracies = accuracies,
            FoldMacroF1 = macroF1
        };
        OutOfFoldPredictions = outOfFold;

        _model = _factory();
        _model.Fit(x, y);
    }

    public int Predict(T[] sample)
    {
        EnsureFitted();
        return _model!.Predict(sample);
    }

    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        EnsureFitted();
        ArgumentNullException.ThrowIfNull(x);
        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The prediction buffer is shorter than the sample count.");

        for (int i = 0; i < rows; i++)
            predictions[i] = _model!.Predict(Array2DHelpers.GetRow(x, i));
    }

    void EnsureFitted()
    {
        if (_model == null)
            throw new InvalidOperationException("The model has not been fitted yet.");
    }

    static double StandardDeviation(double[] values)
    {
        if (values.Length <= 1)
            return 0;
        double mean = values.Average();
        double sum = 0;
        for (int i = 0; i < values.Length; i++)
        {
            double delta = values[i] - mean;
            sum += delta * delta;
        }

        return Math.Sqrt(sum / (values.Length - 1));
    }
}
