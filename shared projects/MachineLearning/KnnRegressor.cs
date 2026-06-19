using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// K-nearest-neighbors regressor implementing IRegressor and IBatchRegressor batch prediction.
/// </summary>
public sealed class KnnRegressor<T> : IBatchRegressor<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T[,]? _features;
    private T[]? _targets;
    private readonly int _k;
    private readonly bool _distanceWeighted;

    /// <summary>
    /// Initializes a new KNN regressor.
    /// </summary>
    /// <param name="k">Number of neighbors.</param>
    /// <param name="distanceWeighted">Whether to apply distance-weighted averaging over the neighbor targets.</param>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public KnnRegressor(int k = 3, bool distanceWeighted = false, ComputingContext? context = null)
    {
        if (k <= 0)
            throw new ArgumentOutOfRangeException(nameof(k));
        _k = k;
        _distanceWeighted = distanceWeighted;
        Context = context;
    }

    /// <summary>Number of neighbors k.</summary>
    public int K => _k;

    /// <summary>
    /// Execution policy honored by batch prediction. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <inheritdoc />
    public void Fit(T[,] x, T[] y)
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");

        int rows = x.GetLength(0);
        if (rows == 0 || y.Length == 0 || rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");
        if (rows < _k)
            throw new ArgumentException($"The number of training samples ({rows}) is less than k ({_k}).");

        _features = x;
        _targets = y;
    }

    /// <inheritdoc />
    public T Predict(T[] sample)
    {
        ValidateSample(sample);
        return KnnNeighborSearch.MeanTargetFromRows(
            _features!,
            _targets!,
            sample,
            _k,
            _distanceWeighted);
    }

    /// <inheritdoc />
    public void PredictBatch(T[,] x, Span<T> predictions)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        if (_features == null || _targets == null)
            throw new InvalidOperationException("The model has not been fitted yet.");

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (cols != _features.GetLength(1))
            throw new ArgumentException("Feature dimension does not match the training samples.");
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        var features = _features;
        var targets = _targets;
        int k = _k;
        bool distanceWeighted = _distanceWeighted;
        if (ComputingContextExecution.UseParallelIndexed(Context, rows, features.GetLength(0)))
        {
            var buffer = new T[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i => buffer[i] = KnnNeighborSearch.MeanTargetFromQueryRow(features, targets, x, i, k, distanceWeighted),
                workPerItem: features.GetLength(0));
            new ReadOnlySpan<T>(buffer).CopyTo(predictions);
            return;
        }

        for (int i = 0; i < rows; i++)
            predictions[i] = KnnNeighborSearch.MeanTargetFromQueryRow(
                _features,
                _targets!,
                x,
                i,
                _k,
                _distanceWeighted);
    }

    private void ValidateSample(T[] sample)
    {
        if (_features == null || _targets == null)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (sample == null)
            throw new ArgumentNullException(nameof(sample));
        if (sample.Length != _features.GetLength(1))
            throw new ArgumentException("Feature dimension does not match the training samples.", nameof(sample));
        if (_targets.Length < _k)
            throw new InvalidOperationException($"The number of training samples ({_targets.Length}) is less than k ({_k}).");
    }
}
