using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Isolation Forest anomaly detector (Liu et al.): random isolation trees score how quickly a sample is isolated.
/// Higher scores indicate more anomalous samples (range approximately 0…1).
/// </summary>
public sealed class IsolationForest<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly int _numTrees;
    private readonly int _subsampleSize;
    private readonly int _maxDepth;
    private readonly int? _seed;
    private readonly List<IsolationTree> _trees = [];
    private int _featureCount;
    private int _fitSampleCount;
    private bool _isFitted;

    /// <summary>
    /// Initializes an Isolation Forest.
    /// </summary>
    /// <param name="numTrees">Number of isolation trees.</param>
    /// <param name="subsampleSize">Bootstrap subsample size per tree; &lt;=0 uses min(256, n).</param>
    /// <param name="maxDepth">Maximum tree depth; &lt;=0 uses ceil(log2(subsampleSize)).</param>
    /// <param name="seed">Optional RNG seed for reproducibility.</param>
    /// <param name="context">Execution policy for batch scoring.</param>
    public IsolationForest(
        int numTrees = 100,
        int subsampleSize = 256,
        int maxDepth = 0,
        int? seed = null,
        ComputingContext? context = null)
    {
        if (numTrees <= 0)
            throw new ArgumentOutOfRangeException(nameof(numTrees));
        _numTrees = numTrees;
        _subsampleSize = subsampleSize;
        _maxDepth = maxDepth;
        _seed = seed;
        Context = context;
    }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.AnomalyDetection;

    /// <summary>
    /// Execution policy honored by batch scoring. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>Number of trees.</summary>
    public int NumTrees => _numTrees;

    /// <summary>Whether the model has been fitted.</summary>
    public bool IsFitted => _isFitted;

    /// <summary>
    /// Fits the isolation forest on the feature matrix (rows = samples).
    /// </summary>
    public void Fit(T[,] x)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0)
            throw new ArgumentException("Training data cannot be empty.");

        _featureCount = cols;
        _fitSampleCount = rows;
        int psi = _subsampleSize <= 0 ? Math.Min(256, rows) : Math.Min(_subsampleSize, rows);
        int depthLimit = _maxDepth <= 0 ? (int)Math.Ceiling(Math.Log2(Math.Max(2, psi))) : _maxDepth;

        var random = _seed.HasValue ? new Random(_seed.Value) : Random.Shared;
        var treeSeeds = new int[_numTrees];
        for (int t = 0; t < _numTrees; t++)
            treeSeeds[t] = random.Next();

        var built = new IsolationTree[_numTrees];
        ComputingContextExecution.ForEach(
            Context,
            0,
            _numTrees,
            t =>
            {
                var localRandom = new Random(treeSeeds[t]);
                var indices = SampleIndices(rows, psi, localRandom);
                built[t] = IsolationTree.Build(x, indices, depthLimit, localRandom);
            },
            workPerItem: (long)psi * Math.Max(1, cols));

        _trees.Clear();
        _trees.AddRange(built);
        _isFitted = true;
    }

    /// <summary>
    /// Returns the anomaly score for a single sample (higher ≈ more anomalous).
    /// </summary>
    public T Score(T[] sample)
    {
        EnsureFitted();
        if (sample == null)
            throw new ArgumentNullException(nameof(sample));
        if (sample.Length != _featureCount)
            throw new ArgumentException("Feature dimension does not match the model.", nameof(sample));

        return ScoreCore(sample);
    }

    /// <summary>
    /// Returns anomaly scores for each row of <paramref name="x"/>.
    /// </summary>
    public T[] Score(T[,] x)
    {
        EnsureFitted();
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        if (x.GetLength(1) != _featureCount)
            throw new ArgumentException("Feature dimension does not match the model.");

        int rows = x.GetLength(0);
        var scores = new T[rows];
        Score(x, scores);
        return scores;
    }

    /// <summary>
    /// Writes anomaly scores for each row of <paramref name="x"/> into <paramref name="scores"/>.
    /// </summary>
    public void Score(T[,] x, Span<T> scores)
    {
        EnsureFitted();
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        int rows = x.GetLength(0);
        if (x.GetLength(1) != _featureCount)
            throw new ArgumentException("Feature dimension does not match the model.");
        if (scores.Length < rows)
            throw new ArgumentException("The scores span is too short.", nameof(scores));

        if (ComputingContextExecution.UseParallelIndexed(Context, rows, _trees.Count * _featureCount))
        {
            var buffer = new T[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i =>
                {
                    var sample = new T[_featureCount];
                    for (int j = 0; j < _featureCount; j++)
                        sample[j] = x[i, j];
                    buffer[i] = ScoreCore(sample);
                },
                workPerItem: (long)_trees.Count * _featureCount);
            new ReadOnlySpan<T>(buffer).CopyTo(scores);
            return;
        }

        var reusable = new T[_featureCount];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < _featureCount; j++)
                reusable[j] = x[i, j];
            scores[i] = ScoreCore(reusable);
        }
    }

    /// <summary>
    /// Labels samples whose score is at or above <paramref name="threshold"/> as anomalies (1), else normal (0).
    /// </summary>
    public int[] Predict(T[,] x, T threshold)
    {
        var scores = Score(x);
        var labels = new int[scores.Length];
        for (int i = 0; i < scores.Length; i++)
            labels[i] = scores[i] >= threshold ? 1 : 0;
        return labels;
    }

    private T ScoreCore(ReadOnlySpan<T> sample)
    {
        double pathSum = 0;
        for (int t = 0; t < _trees.Count; t++)
            pathSum += _trees[t].PathLength(sample);
        double avgPath = pathSum / _trees.Count;
        double c = AveragePathLength(_fitSampleCount);
        if (c <= 1e-12)
            return T.CreateChecked(0.5);
        double score = Math.Pow(2.0, -avgPath / c);
        return T.CreateChecked(score);
    }

    private void EnsureFitted()
    {
        if (!_isFitted || _trees.Count == 0)
            throw new InvalidOperationException("The model has not been fitted yet.");
    }

    private static int[] SampleIndices(int population, int sampleSize, Random random)
    {
        var indices = new int[sampleSize];
        for (int i = 0; i < sampleSize; i++)
            indices[i] = random.Next(population);
        return indices;
    }

    /// <summary>
    /// Expected path length of an unsuccessful BST search, used to normalize isolation depth.
    /// </summary>
    internal static double AveragePathLength(int n)
    {
        if (n <= 1)
            return 0;
        if (n == 2)
            return 1;
        return 2.0 * (Harmonic(n - 1) - 1.0) + (2.0 * (n - 1) / n);
    }

    private static double Harmonic(int i)
    {
        // H(i) ≈ ln(i) + γ
        return Math.Log(i) + 0.5772156649;
    }

    private sealed class IsolationTree
    {
        private readonly IsolationNode _root;

        private IsolationTree(IsolationNode root) => _root = root;

        public static IsolationTree Build(T[,] x, int[] indices, int maxDepth, Random random) =>
            new(BuildNode(x, indices, depth: 0, maxDepth, random));

        public double PathLength(ReadOnlySpan<T> sample) => PathLengthNode(_root, sample, depth: 0);

        private static IsolationNode BuildNode(T[,] x, int[] indices, int depth, int maxDepth, Random random)
        {
            int cols = x.GetLength(1);
            if (indices.Length <= 1 || depth >= maxDepth)
                return IsolationNode.External(indices.Length);

            int feature = random.Next(cols);
            GetMinMax(x, indices, feature, out T min, out T max);
            if (min == max)
                return IsolationNode.External(indices.Length);

            // Uniform split in [min, max)
            double u = random.NextDouble();
            T threshold = min + T.CreateChecked(u) * (max - min);
            if (threshold >= max)
                threshold = min; // ensure at least one side can receive samples when max is exclusive-ish

            var leftList = new List<int>(indices.Length);
            var rightList = new List<int>(indices.Length);
            foreach (int i in indices)
            {
                if (x[i, feature] <= threshold)
                    leftList.Add(i);
                else
                    rightList.Add(i);
            }

            if (leftList.Count == 0 || rightList.Count == 0)
                return IsolationNode.External(indices.Length);

            return IsolationNode.Internal(
                feature,
                threshold,
                BuildNode(x, leftList.ToArray(), depth + 1, maxDepth, random),
                BuildNode(x, rightList.ToArray(), depth + 1, maxDepth, random));
        }

        private static void GetMinMax(T[,] x, int[] indices, int feature, out T min, out T max)
        {
            min = x[indices[0], feature];
            max = min;
            for (int k = 1; k < indices.Length; k++)
            {
                T v = x[indices[k], feature];
                if (v < min) min = v;
                if (v > max) max = v;
            }
        }

        private static double PathLengthNode(IsolationNode node, ReadOnlySpan<T> sample, int depth)
        {
            if (node.IsExternal)
                return depth + AveragePathLength(node.Size);

            return sample[node.FeatureIndex] <= node.Threshold
                ? PathLengthNode(node.Left!, sample, depth + 1)
                : PathLengthNode(node.Right!, sample, depth + 1);
        }
    }

    private sealed class IsolationNode
    {
        public bool IsExternal;
        public int Size;
        public int FeatureIndex;
        public T Threshold = T.Zero;
        public IsolationNode? Left;
        public IsolationNode? Right;

        public static IsolationNode External(int size) =>
            new() { IsExternal = true, Size = size };

        public static IsolationNode Internal(int feature, T threshold, IsolationNode left, IsolationNode right) =>
            new()
            {
                IsExternal = false,
                FeatureIndex = feature,
                Threshold = threshold,
                Left = left,
                Right = right
            };
    }
}
