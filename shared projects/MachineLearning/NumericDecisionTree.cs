using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// CART classification decision tree for numeric features (Gini impurity).
/// </summary>
public class NumericDecisionTree<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private TreeNode? _root;

    /// <summary>
    /// Initializes the decision tree.
    /// </summary>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <param name="minSamplesSplit">The minimum number of samples required to split.</param>
    public NumericDecisionTree(int maxDepth = 12, int minSamplesSplit = 2, ComputingContext? context = null)
    {
        if (maxDepth <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxDepth));
        if (minSamplesSplit < 2)
            throw new ArgumentOutOfRangeException(nameof(minSamplesSplit));

        MaxDepth = maxDepth;
        MinSamplesSplit = minSamplesSplit;
        Context = context;
    }

    /// <summary>The maximum depth.</summary>
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

    /// <inheritdoc />
    public void Fit(T[,] x, int[] y) => Fit(x, y, sampleIndices: null);
    /// <summary>
    /// Fits the classification tree. Labels must be non-negative integers.
    /// </summary>
    /// <param name="sampleIndices">
    /// The sample row indices used for training (may contain duplicates, for bootstrapping). When <see langword="null"/>, all rows are used.
    /// </param>
    public void Fit(T[,] x, int[] y, int[]? sampleIndices)
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");
        int rows = x.GetLength(0);
        if (rows == 0 || y.Length == 0 || rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");
        var indices = sampleIndices ?? Enumerable.Range(0, rows).ToArray();
        if (sampleIndices != null)
        {
            foreach (int index in sampleIndices)
            {
                if (index < 0 || index >= rows)
                    throw new ArgumentOutOfRangeException(nameof(sampleIndices), "Sample index is out of range.");
            }
        }
        int numClasses = y.Max() + 1;
        _root = BuildNode(x, y, indices, depth: 0, numClasses, featureMask: null);
    }

    /// <summary>
    /// Predicts the class.
    /// </summary>
    public int Predict(T[] sample)
    {
        if (_root == null)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (sample == null)
            throw new ArgumentNullException(nameof(sample));

        return PredictNode(_root, sample);
    }

    /// <inheritdoc />
    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        if (_root == null)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        if (ComputingContextExecution.UseParallelIndexed(Context, rows, MaxDepth))
        {
            var root = _root!;
            var buffer = new int[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i => buffer[i] = PredictNodeFromRow(root, x, i),
                workPerItem: MaxDepth);
            new ReadOnlySpan<int>(buffer).CopyTo(predictions);
            return;
        }

        for (int i = 0; i < rows; i++)
            predictions[i] = PredictNodeFromRow(_root, x, i);
    }

    private static int PredictNode(TreeNode root, T[] sample)
    {
        var node = root;
        while (!node.IsLeaf)
            node = sample[node.FeatureIndex] <= node.Threshold ? node.Left! : node.Right!;
        return node.PredictedClass;
    }

    private static int PredictNodeFromRow(TreeNode root, T[,] x, int row)
    {
        var node = root;
        while (!node.IsLeaf)
            node = x[row, node.FeatureIndex] <= node.Threshold ? node.Left! : node.Right!;
        return node.PredictedClass;
    }

    private TreeNode BuildNode(
        T[,] x,
        int[] y,
        int[] indices,
        int depth,
        int numClasses,
        HashSet<int>? featureMask)
    {
        int predictedClass = ClassificationMath.MajorityClass(y, indices);
        if (depth >= MaxDepth || indices.Length < MinSamplesSplit || IsPure(y, indices))
            return TreeNode.Leaf(predictedClass);

        if (!TryFindBestSplit(x, y, indices, numClasses, featureMask, out int feature, out T threshold, out int[] left, out int[] right))
            return TreeNode.Leaf(predictedClass);

        if (left.Length == 0 || right.Length == 0)
            return TreeNode.Leaf(predictedClass);

        return new TreeNode
        {
            IsLeaf = false,
            FeatureIndex = feature,
            Threshold = threshold,
            Left = BuildNode(x, y, left, depth + 1, numClasses, featureMask),
            Right = BuildNode(x, y, right, depth + 1, numClasses, featureMask)
        };
    }

    private static bool TryFindBestSplit(
        T[,] x,
        int[] y,
        int[] indices,
        int numClasses,
        HashSet<int>? featureMask,
        out int bestFeature,
        out T bestThreshold,
        out int[] leftIndices,
        out int[] rightIndices)
    {
        int totalCount = indices.Length;
        return CartThresholdSearch.TryFindBestSplit(
            x,
            indices,
            featureMask,
            (_, _, left, right) =>
                WeightedGini(y, left, numClasses, totalCount)
                + WeightedGini(y, right, numClasses, totalCount),
            out bestFeature,
            out bestThreshold,
            out leftIndices,
            out rightIndices);
    }

    private static T WeightedGini(int[] y, ReadOnlySpan<int> indices, int numClasses, int totalCount) =>
        T.CreateChecked(indices.Length) / T.CreateChecked(totalCount) * Gini(y, indices, numClasses);

    private static T Gini(int[] y, ReadOnlySpan<int> indices, int numClasses)
    {
        var counts = new int[numClasses];
        foreach (int i in indices)
            counts[y[i]]++;

        T impurity = T.One;
        T n = T.CreateChecked(indices.Length);
        for (int c = 0; c < numClasses; c++)
        {
            T p = T.CreateChecked(counts[c]) / n;
            impurity -= p * p;
        }
        return impurity;
    }

    private static bool IsPure(int[] y, int[] indices)
    {
        int first = y[indices[0]];
        for (int i = 1; i < indices.Length; i++)
        {
            if (y[indices[i]] != first)
                return false;
        }
        return true;
    }

    private sealed class TreeNode
    {
        public bool IsLeaf;
        public int PredictedClass;
        public int FeatureIndex;
        public T Threshold = T.Zero;
        public TreeNode? Left;
        public TreeNode? Right;

        public static TreeNode Leaf(int predictedClass) =>
            new() { IsLeaf = true, PredictedClass = predictedClass };
    }
}
