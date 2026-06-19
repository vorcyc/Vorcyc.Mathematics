using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Gradient boosting classifier (multiclass, log loss + regression-tree weak learners).
/// </summary>
public class GradientBoostingClassifier<T> : IClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T[]? _baseScores;
    private List<RegressionTree[]>? _stages;
    private int _numClasses;

    /// <summary>
    /// Initializes the gradient boosting classifier.
    /// </summary>
    /// <param name="nEstimators">The number of boosting rounds.</param>
    /// <param name="learningRate">The learning rate (shrinkage factor).</param>
    /// <param name="maxDepth">The maximum depth of the weak learners.</param>
    /// <param name="minSamplesSplit">The minimum number of samples required to split.</param>
    public GradientBoostingClassifier(
        int nEstimators = 50,
        T learningRate = default,
        int maxDepth = 3,
        int minSamplesSplit = 2,
        ComputingContext? context = null)
    {
        if (nEstimators <= 0)
            throw new ArgumentOutOfRangeException(nameof(nEstimators));
        if (maxDepth <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxDepth));
        if (minSamplesSplit < 2)
            throw new ArgumentOutOfRangeException(nameof(minSamplesSplit));

        NEstimators = nEstimators;
        LearningRate = learningRate.Equals(default) ? T.CreateChecked(0.1) : learningRate;
        MaxDepth = maxDepth;
        MinSamplesSplit = minSamplesSplit;
        Context = context;
    }

    /// <summary>The number of boosting rounds.</summary>
    public int NEstimators { get; }

    /// <summary>The learning rate.</summary>
    public T LearningRate { get; }

    /// <summary>The maximum depth of the weak learners.</summary>
    public int MaxDepth { get; }

    /// <summary>The minimum number of samples to split.</summary>
    public int MinSamplesSplit { get; }

    /// <summary>
    /// Execution policy honored by this estimator. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <summary>
    /// Fits the multiclass model. Labels must be non-negative integers.
    /// </summary>
    public void Fit(T[,] x, int[] y)
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");

        int rows = x.GetLength(0);
        if (rows == 0 || y.Length == 0 || rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");
        if (y.Min() < 0)
            throw new ArgumentException("Labels must be non-negative integers.");

        _numClasses = y.Max() + 1;
        _baseScores = ComputeBaseScores(y, _numClasses);
        _stages = new List<RegressionTree[]>(NEstimators);

        var scores = new T[rows][];
        for (int i = 0; i < rows; i++)
        {
            scores[i] = (T[])_baseScores.Clone();
        }

        for (int stage = 0; stage < NEstimators; stage++)
        {
            var trees = new RegressionTree[_numClasses];
            ComputingContextExecution.ForEach(
                Context,
                0,
                _numClasses,
                c =>
                {
                    // Per-class scratch buffers keep this loop safe to run in parallel.
                    var localProbabilities = new T[_numClasses];
                    var localResiduals = new T[rows];
                    for (int i = 0; i < rows; i++)
                    {
                        StableProbabilities.Softmax(scores[i], localProbabilities);
                        T target = y[i] == c ? T.One : T.Zero;
                        localResiduals[i] = target - localProbabilities[c];
                    }

                    trees[c] = RegressionTree.Fit(
                        x,
                        localResiduals,
                        MaxDepth,
                        MinSamplesSplit);
                },
                workPerItem: (long)rows * MaxDepth);

            _stages.Add(trees);

            for (int i = 0; i < rows; i++)
            {
                for (int c = 0; c < _numClasses; c++)
                {
                    scores[i][c] += LearningRate * trees[c].Predict(Array2DHelpers.GetRow(x, i));
                }
            }
        }
    }

    /// <summary>
    /// Predicts the class.
    /// </summary>
    public int Predict(T[] sample)
    {
        if (_baseScores == null || _stages == null)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (sample == null)
            throw new ArgumentNullException(nameof(sample));

        var scores = (T[])_baseScores.Clone();
        foreach (var stage in _stages)
        {
            for (int c = 0; c < _numClasses; c++)
                scores[c] += LearningRate * stage[c].Predict(sample);
        }

        return ClassificationMath.ArgMax(scores);
    }

    private static T[] ComputeBaseScores(int[] y, int numClasses)
    {
        var counts = new int[numClasses];
        foreach (int label in y)
            counts[label]++;

        T n = T.CreateChecked(y.Length);
        var baseScores = new T[numClasses];
        for (int c = 0; c < numClasses; c++)
        {
            T prior = T.CreateChecked(counts[c]) / n;
            prior = T.Max(prior, T.CreateChecked(1e-6));
            baseScores[c] = T.Log(prior);
        }
        return baseScores;
    }

    private sealed class RegressionTree
    {
        private Node? _root;

        public static RegressionTree Fit(T[,] x, T[] targets, int maxDepth, int minSamplesSplit)
        {
            int rows = x.GetLength(0);
            var indices = Enumerable.Range(0, rows).ToArray();
            var tree = new RegressionTree();
            tree._root = BuildNode(x, targets, indices, depth: 0, maxDepth, minSamplesSplit);
            return tree;
        }

        public T Predict(T[] sample)
        {
            var node = _root ?? throw new InvalidOperationException("The tree has not been built.");
            while (!node.IsLeaf)
                node = sample[node.FeatureIndex] <= node.Threshold ? node.Left! : node.Right!;
            return node.Value;
        }

        private static Node BuildNode(
            T[,] x,
            T[] targets,
            int[] indices,
            int depth,
            int maxDepth,
            int minSamplesSplit)
        {
            T mean = Mean(targets, indices);
            if (depth >= maxDepth || indices.Length < minSamplesSplit)
                return Node.Leaf(mean);

            if (!TryFindBestSplit(x, targets, indices, out int feature, out T threshold, out int[] left, out int[] right))
                return Node.Leaf(mean);

            if (left.Length == 0 || right.Length == 0)
                return Node.Leaf(mean);

            return new Node
            {
                IsLeaf = false,
                FeatureIndex = feature,
                Threshold = threshold,
                Left = BuildNode(x, targets, left, depth + 1, maxDepth, minSamplesSplit),
                Right = BuildNode(x, targets, right, depth + 1, maxDepth, minSamplesSplit)
            };
        }

        private static bool TryFindBestSplit(
            T[,] x,
            T[] targets,
            int[] indices,
            out int bestFeature,
            out T bestThreshold,
            out int[] leftIndices,
            out int[] rightIndices) =>
            CartThresholdSearch.TryFindBestSplit(
                x,
                indices,
                allowedFeatures: null,
                (_, _, left, right) => WeightedVariance(targets, left) + WeightedVariance(targets, right),
                out bestFeature,
                out bestThreshold,
                out leftIndices,
                out rightIndices);

        private static T WeightedVariance(T[] targets, ReadOnlySpan<int> indices)
        {
            if (indices.Length == 0)
                return T.Zero;

            T mean = Mean(targets, indices);
            T variance = T.Zero;
            foreach (int i in indices)
            {
                T diff = targets[i] - mean;
                variance += diff * diff;
            }
            return variance;
        }

        private static T Mean(T[] targets, ReadOnlySpan<int> indices)
        {
            T sum = T.Zero;
            foreach (int i in indices)
                sum += targets[i];
            return sum / T.CreateChecked(indices.Length);
        }

        private sealed class Node
        {
            public bool IsLeaf;
            public T Value;
            public int FeatureIndex;
            public T Threshold = T.Zero;
            public Node? Left;
            public Node? Right;

            public static Node Leaf(T value) =>
                new() { IsLeaf = true, Value = value };
        }
    }
}
