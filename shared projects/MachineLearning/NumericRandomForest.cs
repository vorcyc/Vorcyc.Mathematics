using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Random forest based on numeric decision trees.
/// </summary>
public class NumericRandomForest<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly List<ForestTree> _trees = [];
    private readonly int _numTrees;
    private readonly int _maxFeatures;
    private readonly int _maxDepth;
    private readonly int _minSamplesSplit;
    private readonly int? _seed;

    /// <summary>
    /// Initializes the random forest.
    /// </summary>
    public NumericRandomForest(
        int numTrees = 25,
        int maxFeatures = 0,
        int maxDepth = 12,
        int minSamplesSplit = 2,
        int? seed = null,
        ComputingContext? context = null)
    {
        if (numTrees <= 0)
            throw new ArgumentOutOfRangeException(nameof(numTrees));
        _numTrees = numTrees;
        _maxFeatures = maxFeatures;
        _maxDepth = maxDepth;
        _minSamplesSplit = minSamplesSplit;
        _seed = seed;
        Context = context;
    }

    /// <summary>
    /// Execution policy honored by this estimator. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <summary>
    /// Trains the random forest.
    /// </summary>
    public void Fit(T[,] x, int[] y)
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || y.Length == 0 || rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");

        _trees.Clear();
        var random = _seed.HasValue ? new Random(_seed.Value) : Random.Shared;
        int featureCount = _maxFeatures > 0 ? _maxFeatures : Math.Max(1, (int)Math.Sqrt(cols));

        // Derive a deterministic per-tree seed sequence so trees can be built in parallel.
        var treeSeeds = new int[_numTrees];
        for (int t = 0; t < _numTrees; t++)
            treeSeeds[t] = random.Next();

        var built = new ForestTree[_numTrees];
        ComputingContextExecution.ForEach(
            Context,
            0,
            _numTrees,
            t =>
            {
                var localRandom = new Random(treeSeeds[t]);
                var bootstrapIndices = EnsembleHelpers.CreateBootstrapIndices(rows, localRandom);
                var selectedFeatures = SelectFeatures(cols, featureCount, localRandom);
                var maskedX = ProjectFeatures(x, selectedFeatures);
                var tree = new NumericDecisionTree<T>(_maxDepth, _minSamplesSplit);
                tree.Fit(maskedX, y, bootstrapIndices);
                built[t] = new ForestTree(tree, selectedFeatures);
            },
            workPerItem: (long)rows * Math.Max(1, featureCount));

        _trees.AddRange(built);
    }

    /// <summary>
    /// Predicts the class.
    /// </summary>
    public int Predict(T[] sample)
    {
        if (_trees.Count == 0)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (sample == null)
            throw new ArgumentNullException(nameof(sample));

        var votes = new int[_trees.Count];
        for (int t = 0; t < _trees.Count; t++)
            votes[t] = _trees[t].Predict(sample);
        return ClassificationMath.MajorityVote(votes);
    }

    /// <inheritdoc />
    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        if (_trees.Count == 0)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        if (ComputingContextExecution.UseParallelIndexed(Context, rows, _trees.Count))
        {
            var buffer = new int[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i =>
                {
                    var localVotes = new int[_trees.Count];
                    for (int t = 0; t < _trees.Count; t++)
                        localVotes[t] = _trees[t].PredictFromRow(x, i);
                    buffer[i] = ClassificationMath.MajorityVote(localVotes);
                },
                workPerItem: _trees.Count);
            new ReadOnlySpan<int>(buffer).CopyTo(predictions);
            return;
        }
        var votes = new int[_trees.Count];
        for (int i = 0; i < rows; i++)
        {
            for (int t = 0; t < _trees.Count; t++)
                votes[t] = _trees[t].PredictFromRow(x, i);
            predictions[i] = ClassificationMath.MajorityVote(votes);
        }
    }

    private static int[] SelectFeatures(int totalFeatures, int count, Random random)
    {
        count = Math.Min(count, totalFeatures);
        var indices = Enumerable.Range(0, totalFeatures).ToList();
        var selected = new int[count];
        for (int i = 0; i < count; i++)
        {
            int pick = random.Next(indices.Count);
            selected[i] = indices[pick];
            indices.RemoveAt(pick);
        }
        Array.Sort(selected);
        return selected;
    }

    private static T[,] ProjectFeatures(T[,] x, int[] featureIndices)
    {
        int rows = x.GetLength(0);
        var result = new T[rows, featureIndices.Length];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < featureIndices.Length; j++)
                result[i, j] = x[i, featureIndices[j]];
        }
        return result;
    }

    private sealed class ForestTree
    {
        private readonly NumericDecisionTree<T> _tree;
        private readonly int[] _featureIndices;
        private readonly T[] _projected;

        public ForestTree(NumericDecisionTree<T> tree, int[] featureIndices)
        {
            _tree = tree;
            _featureIndices = featureIndices;
            _projected = new T[featureIndices.Length];
        }

        public int Predict(T[] sample)
        {
            for (int j = 0; j < _featureIndices.Length; j++)
                _projected[j] = sample[_featureIndices[j]];
            return _tree.Predict(_projected);
        }

        public int PredictFromRow(T[,] x, int row)
        {
            // Use a local buffer so rows can be predicted concurrently.
            var projected = new T[_featureIndices.Length];
            for (int j = 0; j < _featureIndices.Length; j++)
                projected[j] = x[row, _featureIndices[j]];
            return _tree.Predict(projected);
        }
    }
}
