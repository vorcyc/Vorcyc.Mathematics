using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// AdaBoost multiclass classifier (SAMME), with numeric decision stumps as weak learners.
/// </summary>
public class AdaBoostClassifier<T> : IClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private DecisionStump[]? _stumps;
    private T[]? _alphas;
    private int _numClasses;

    /// <summary>
    /// Initializes the AdaBoost classifier.
    /// </summary>
    /// <param name="nEstimators">The number of weak learners.</param>
    public AdaBoostClassifier(int nEstimators = 50, ComputingContext? context = null)
    {
        if (nEstimators <= 0)
            throw new ArgumentOutOfRangeException(nameof(nEstimators));
        NEstimators = nEstimators;
        Context = context;
    }

    /// <summary>The number of weak learners.</summary>
    public int NEstimators { get; }

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
        _stumps = new DecisionStump[NEstimators];
        _alphas = new T[NEstimators];

        var weights = new T[rows];
        T uniform = T.One / T.CreateChecked(rows);
        for (int i = 0; i < rows; i++)
            weights[i] = uniform;

        var allIndices = Enumerable.Range(0, rows).ToArray();

        for (int m = 0; m < NEstimators; m++)
        {
            var stump = DecisionStump.FitBest(x, y, weights, allIndices, _numClasses)
                ?? throw new InvalidOperationException("Unable to train the weak learner; please check the data.");

            T error = T.Zero;
            for (int i = 0; i < rows; i++)
            {
                if (stump.Predict(Array2DHelpers.GetRow(x, i)) != y[i])
                    error += weights[i];
            }

            if (error >= T.One - T.CreateChecked(1e-10))
                error = T.One - T.CreateChecked(1e-10);
            if (error <= T.CreateChecked(1e-10))
                error = T.CreateChecked(1e-10);

            T alpha = T.Log((T.One - error) / error);
            if (_numClasses > 2)
                alpha += T.Log(T.CreateChecked(_numClasses - 1));

            _stumps[m] = stump;
            _alphas[m] = alpha;

            T weightSum = T.Zero;
            for (int i = 0; i < rows; i++)
            {
                if (stump.Predict(Array2DHelpers.GetRow(x, i)) != y[i])
                    weights[i] *= T.Exp(alpha);
                weightSum += weights[i];
            }

            for (int i = 0; i < rows; i++)
                weights[i] /= weightSum;
        }
    }

    /// <summary>
    /// Predicts the class.
    /// </summary>
    public int Predict(T[] sample)
    {
        if (_stumps == null || _alphas == null)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (sample == null)
            throw new ArgumentNullException(nameof(sample));

        var votes = new T[_numClasses];
        for (int m = 0; m < _stumps.Length; m++)
        {
            int predicted = _stumps[m].Predict(sample);
            votes[predicted] += _alphas[m];
        }

        return ClassificationMath.ArgMax(votes);
    }

    private sealed class DecisionStump
    {
        public int FeatureIndex;
        public T Threshold = T.Zero;
        public int LeftClass;
        public int RightClass;

        public int Predict(T[] sample) =>
            sample[FeatureIndex] <= Threshold ? LeftClass : RightClass;

        public static DecisionStump? FitBest(
            T[,] x,
            int[] y,
            T[] weights,
            int[] indices,
            int numClasses)
        {
            DecisionStump? best = null;
            T bestError = T.CreateChecked(double.MaxValue);

            bool found = CartThresholdSearch.TryFindBestSplit(
                x,
                indices,
                allowedFeatures: null,
                (feature, threshold, left, right) =>
                {
                    T error = ComputeWeightedError(
                        x, y, weights, feature, threshold, left, right, numClasses,
                        out int leftClass, out int rightClass);

                    if (error < bestError)
                    {
                        bestError = error;
                        best = new DecisionStump
                        {
                            FeatureIndex = feature,
                            Threshold = threshold,
                            LeftClass = leftClass,
                            RightClass = rightClass
                        };
                    }

                    return error;
                },
                out _,
                out _,
                out _,
                out _);

            return found ? best : null;
        }

        private static T ComputeWeightedError(
            T[,] x,
            int[] y,
            T[] weights,
            int feature,
            T threshold,
            ReadOnlySpan<int> left,
            ReadOnlySpan<int> right,
            int numClasses,
            out int leftClass,
            out int rightClass)
        {
            var leftCounts = new T[numClasses];
            var rightCounts = new T[numClasses];

            foreach (int i in left)
                leftCounts[y[i]] += weights[i];
            foreach (int i in right)
                rightCounts[y[i]] += weights[i];

            leftClass = ClassificationMath.WeightedArgMax(leftCounts);
            rightClass = ClassificationMath.WeightedArgMax(rightCounts);

            T error = T.Zero;
            foreach (int i in left)
            {
                if (leftClass != y[i])
                    error += weights[i];
            }
            foreach (int i in right)
            {
                if (rightClass != y[i])
                    error += weights[i];
            }
            return error;
        }
    }
}
