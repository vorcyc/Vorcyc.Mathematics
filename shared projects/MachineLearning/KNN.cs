using System.Numerics;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// K-nearest-neighbors regression.
/// </summary>
/// <typeparam name="T">Numeric type of the features and targets.</typeparam>
public class KNN<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly List<T[]> _featureRows = [];
    private readonly List<T> _targets = [];

    /// <summary>
    /// Initializes a new KNN regressor.
    /// </summary>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public KNN(ComputingContext? context = null)
    {
        Context = context;
    }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>
    /// Execution policy honored by batch regression. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Adds a 2D regression sample.
    /// </summary>
    public void Add(Point<T> point, T target) =>
        AddRegression([point.X, point.Y], target);

    /// <summary>
    /// Adds an arbitrary-dimensional regression sample.
    /// </summary>
    public void Add(ReadOnlySpan<T> features, T target) =>
        AddRegression(features, target);

    /// <summary>
    /// Performs regression for a 2D point.
    /// </summary>
    public T Regress(Point<T> point, int k) =>
        Regress([point.X, point.Y], k);

    /// <summary>
    /// Performs regression for a feature vector.
    /// </summary>
    public T Regress(ReadOnlySpan<T> features, int k, bool distanceWeighted = false)
    {
        ValidateK(k);
        if (_featureRows.Count == 0)
            throw new InvalidOperationException("The training set is empty.");
        if (features.Length != _featureRows[0].Length)
            throw new ArgumentException("Feature dimension does not match the training samples.");
        if (_featureRows.Count < k)
            throw new ArgumentException($"The number of available samples ({_featureRows.Count}) is less than k ({k}).");

        return KnnNeighborSearch.MeanTargetFromVectors(
            CollectionsMarshal.AsSpan(_featureRows),
            CollectionsMarshal.AsSpan(_targets),
            features,
            k,
            distanceWeighted);
    }

    /// <summary>
    /// Performs batch regression prediction for each row of the feature matrix.
    /// </summary>
    public T[] RegressBatch(T[,] x, int k, bool distanceWeighted = false)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        int rows = x.GetLength(0);
        var predictions = new T[rows];
        RegressBatch(x, k, predictions, distanceWeighted);
        return predictions;
    }

    /// <summary>
    /// Writes batch regression predictions into <paramref name="predictions"/>.
    /// </summary>
    public void RegressBatch(T[,] x, int k, Span<T> predictions, bool distanceWeighted = false)
    {
        ValidateK(k);
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        if (_featureRows.Count == 0)
            throw new InvalidOperationException("The training set is empty.");
        if (_featureRows.Count < k)
            throw new ArgumentException($"The number of available samples ({_featureRows.Count}) is less than k ({k}).");

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (cols != _featureRows[0].Length)
            throw new ArgumentException("Feature dimension does not match the training samples.");
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        var storedFeatures = CollectionsMarshal.AsSpan(_featureRows);
        var storedTargets = CollectionsMarshal.AsSpan(_targets);
        if (ComputingContextExecution.UseParallelIndexed(Context, rows, (long)_featureRows.Count * cols))
        {
            var stored = _featureRows.ToArray();
            var targets = _targets.ToArray();
            var buffer = new T[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i =>
                {
                    var localSample = new T[cols];
                    for (int j = 0; j < cols; j++)
                        localSample[j] = x[i, j];
                    buffer[i] = KnnNeighborSearch.MeanTargetFromVectors(
                        stored,
                        targets,
                        localSample,
                        k,
                        distanceWeighted);
                },
                workPerItem: (long)_featureRows.Count * cols);
            new ReadOnlySpan<T>(buffer).CopyTo(predictions);
            return;
        }

        var sample = new T[cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                sample[j] = x[i, j];
            predictions[i] = KnnNeighborSearch.MeanTargetFromVectors(
                storedFeatures,
                storedTargets,
                sample,
                k,
                distanceWeighted);
        }
    }

    private void AddRegression(ReadOnlySpan<T> features, T target)
    {
        var row = new T[features.Length];
        features.CopyTo(row);
        _featureRows.Add(row);
        _targets.Add(target);
    }

    private static void ValidateK(int k)
    {
        if (k <= 0)
            throw new ArgumentOutOfRangeException(nameof(k), "k must be greater than 0.");
    }
}
