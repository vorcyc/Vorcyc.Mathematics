using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// K-nearest-neighbors classifier for integer labels, implementing <see cref="IClassifier{T}"/>.
/// </summary>
public sealed class KnnClassifier<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T[,]? _features;
    private int[]? _labels;
    private readonly int _k;

    /// <summary>
    /// Initializes a new KNN classifier.
    /// </summary>
    /// <param name="k">Number of neighbors.</param>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public KnnClassifier(int k = 3, ComputingContext? context = null)
    {
        if (k <= 0)
            throw new ArgumentOutOfRangeException(nameof(k));
        _k = k;
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
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <inheritdoc />
    public void Fit(T[,] x, int[] y)
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");
        int rows = x.GetLength(0);
        if (rows == 0 || y.Length == 0 || rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");
        if (y.Min() < 0)
            throw new ArgumentException("Labels must be non-negative integers.");

        _features = x;
        _labels = y;
    }

    /// <inheritdoc />
    public int Predict(T[] sample)
    {
        ValidateSample(sample);
        return KnnNeighborSearch.MajorityLabelFromRows(_features!, _labels!, sample, _k);
    }

    /// <inheritdoc />
    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        if (_features == null || _labels == null)
            throw new InvalidOperationException("The model has not been fitted yet.");

        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        int cols = x.GetLength(1);
        if (cols != _features.GetLength(1))
            throw new ArgumentException("Feature dimension does not match the training samples.");

        var features = _features;
        var labels = _labels;
        int k = _k;
        if (ComputingContextExecution.UseParallelIndexed(Context, rows, features.GetLength(0)))
        {
            var buffer = new int[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i => buffer[i] = KnnNeighborSearch.MajorityLabelFromQueryRow(features, labels, x, i, k),
                workPerItem: features.GetLength(0));
            new ReadOnlySpan<int>(buffer).CopyTo(predictions);
            return;
        }

        for (int i = 0; i < rows; i++)
            predictions[i] = KnnNeighborSearch.MajorityLabelFromQueryRow(_features, _labels, x, i, _k);
    }

    private void ValidateSample(T[] sample)
    {
        if (_features == null || _labels == null)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (sample == null)
            throw new ArgumentNullException(nameof(sample));
        if (sample.Length != _features.GetLength(1))
            throw new ArgumentException("Feature dimension does not match the training samples.", nameof(sample));
        if (_labels.Length < _k)
            throw new InvalidOperationException($"The number of training samples ({_labels.Length}) is less than k ({_k}).");
    }

    /// <summary>
    /// Exports a prototype-set snapshot (double precision).
    /// </summary>
    public Serialization.KnnClassifierSnapshot CaptureSnapshot()
    {
        if (_features == null || _labels == null)
            throw new InvalidOperationException("The model has not been fitted yet.");

        int rows = _features.GetLength(0);
        int cols = _features.GetLength(1);
        var features = new double[rows][];
        for (int i = 0; i < rows; i++)
        {
            features[i] = new double[cols];
            for (int j = 0; j < cols; j++)
                features[i][j] = double.CreateChecked(_features[i, j]);
        }

        return new Serialization.KnnClassifierSnapshot
        {
            K = _k,
            Features = features,
            Labels = (int[])_labels.Clone()
        };
    }

    /// <summary>
    /// Restores the prototype set from a snapshot (for inference).
    /// </summary>
    public void RestoreFromSnapshot(Serialization.KnnClassifierSnapshot snapshot)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));
        if (snapshot.Features == null || snapshot.Features.Length == 0)
            throw new ArgumentException("Snapshot features cannot be empty.");
        if (snapshot.Labels == null || snapshot.Labels.Length != snapshot.Features.Length)
            throw new ArgumentException("Snapshot labels must align with features.");
        if (snapshot.K != _k)
            throw new ArgumentException($"Snapshot k ({snapshot.K}) does not match this classifier's k ({_k}).");

        int rows = snapshot.Features.Length;
        int cols = snapshot.Features[0].Length;
        var features = new T[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            if (snapshot.Features[i].Length != cols)
                throw new ArgumentException("All feature rows must share the same dimension.");
            for (int j = 0; j < cols; j++)
                features[i, j] = T.CreateChecked(snapshot.Features[i][j]);
        }
        _features = features;
        _labels = (int[])snapshot.Labels.Clone();
    }
}
