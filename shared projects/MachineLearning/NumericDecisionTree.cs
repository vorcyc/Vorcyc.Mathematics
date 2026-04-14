using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;



namespace Vorcyc.Mathematics.MachineLearning;



/// <summary>

/// 数值特征 CART 分类决策树（Gini 不纯度）。

/// </summary>

public class NumericDecisionTree<T> : IBatchClassifier<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private TreeNode? _root;



    /// <summary>

    /// 初始化决策树。

    /// </summary>

    /// <param name="maxDepth">最大深度。</param>

    /// <param name="minSamplesSplit">分裂所需最小样本数。</param>

    public NumericDecisionTree(int maxDepth = 12, int minSamplesSplit = 2)

    {

        if (maxDepth <= 0)

            throw new ArgumentOutOfRangeException(nameof(maxDepth));

        if (minSamplesSplit < 2)

            throw new ArgumentOutOfRangeException(nameof(minSamplesSplit));



        MaxDepth = maxDepth;

        MinSamplesSplit = minSamplesSplit;

    }



    /// <summary>最大深度。</summary>

    public int MaxDepth { get; }



    /// <summary>分裂最小样本数。</summary>

    public int MinSamplesSplit { get; }



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Classification;



    /// <inheritdoc />
    public void Fit(T[,] x, int[] y) => Fit(x, y, sampleIndices: null);

    /// <summary>
    /// 拟合分类树。标签为非负整数。
    /// </summary>
    /// <param name="sampleIndices">
    /// 参与训练的样本行索引（可含重复，用于自助法）。为 <see langword="null"/> 时使用全部行。
    /// </param>
    public void Fit(T[,] x, int[] y, int[]? sampleIndices)
    {
        if (x == null || y == null)
            throw new ArgumentException("输入不能为 null。");
        int rows = x.GetLength(0);
        if (rows == 0 || y.Length == 0 || rows != y.Length)
            throw new ArgumentException("样本数与标签数不匹配。");

        var indices = sampleIndices ?? Enumerable.Range(0, rows).ToArray();
        if (sampleIndices != null)
        {
            foreach (int index in sampleIndices)
            {
                if (index < 0 || index >= rows)
                    throw new ArgumentOutOfRangeException(nameof(sampleIndices), "样本索引超出范围。");
            }
        }

        int numClasses = y.Max() + 1;
        _root = BuildNode(x, y, indices, depth: 0, numClasses, featureMask: null);
    }



    /// <summary>

    /// 预测类别。

    /// </summary>

    public int Predict(T[] sample)

    {

        if (_root == null)

            throw new InvalidOperationException("模型尚未拟合。");

        if (sample == null)

            throw new ArgumentNullException(nameof(sample));



        return PredictNode(_root, sample);

    }



    /// <inheritdoc />

    public void PredictBatch(T[,] x, Span<int> predictions)

    {

        if (_root == null)

            throw new InvalidOperationException("模型尚未拟合。");

        if (x == null)

            throw new ArgumentNullException(nameof(x));



        int rows = x.GetLength(0);

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



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


