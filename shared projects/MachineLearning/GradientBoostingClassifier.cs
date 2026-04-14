using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 梯度提升分类器（多分类，对数损失 + 回归树弱学习器）。
/// </summary>
public class GradientBoostingClassifier<T> : IClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T[]? _baseScores;
    private List<RegressionTree[]>? _stages;
    private int _numClasses;

    /// <summary>
    /// 初始化梯度提升分类器。
    /// </summary>
    /// <param name="nEstimators">提升轮数。</param>
    /// <param name="learningRate">学习率（收缩因子）。</param>
    /// <param name="maxDepth">弱学习器最大深度。</param>
    /// <param name="minSamplesSplit">分裂所需最小样本数。</param>
    public GradientBoostingClassifier(
        int nEstimators = 50,
        T learningRate = default,
        int maxDepth = 3,
        int minSamplesSplit = 2)
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
    }

    /// <summary>提升轮数。</summary>
    public int NEstimators { get; }

    /// <summary>学习率。</summary>
    public T LearningRate { get; }

    /// <summary>弱学习器最大深度。</summary>
    public int MaxDepth { get; }

    /// <summary>分裂最小样本数。</summary>
    public int MinSamplesSplit { get; }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <summary>
    /// 拟合多分类模型。标签为非负整数。
    /// </summary>
    public void Fit(T[,] x, int[] y)
    {
        if (x == null || y == null)
            throw new ArgumentException("输入不能为 null。");

        int rows = x.GetLength(0);
        if (rows == 0 || y.Length == 0 || rows != y.Length)
            throw new ArgumentException("样本数与标签数不匹配。");
        if (y.Min() < 0)
            throw new ArgumentException("标签必须为非负整数。");

        _numClasses = y.Max() + 1;
        _baseScores = ComputeBaseScores(y, _numClasses);
        _stages = new List<RegressionTree[]>(NEstimators);

        var scores = new T[rows][];
        for (int i = 0; i < rows; i++)
        {
            scores[i] = (T[])_baseScores.Clone();
        }

        var probabilities = new T[_numClasses];
        var residuals = new T[rows];

        for (int stage = 0; stage < NEstimators; stage++)
        {
            var trees = new RegressionTree[_numClasses];
            for (int c = 0; c < _numClasses; c++)
            {
                for (int i = 0; i < rows; i++)
                {
                    StableProbabilities.Softmax(scores[i], probabilities);
                    T target = y[i] == c ? T.One : T.Zero;
                    residuals[i] = target - probabilities[c];
                }

                trees[c] = RegressionTree.Fit(
                    x,
                    residuals,
                    MaxDepth,
                    MinSamplesSplit);
            }

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
    /// 预测类别。
    /// </summary>
    public int Predict(T[] sample)
    {
        if (_baseScores == null || _stages == null)
            throw new InvalidOperationException("模型尚未拟合。");
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
            var node = _root ?? throw new InvalidOperationException("树未构建。");
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
