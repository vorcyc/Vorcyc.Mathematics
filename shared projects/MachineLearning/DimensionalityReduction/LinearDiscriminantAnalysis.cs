using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;

/// <summary>
/// Linear Discriminant Analysis (LDA), used for supervised dimensionality reduction and classification.
/// </summary>
public class LinearDiscriminantAnalysis<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly int _numComponents;
    private T[] _overallMean = [];
    private Dictionary<int, T[]> _classMeans = [];
    private Dictionary<int, int> _classCounts = [];
    private T[][] _projectionMatrix = [];

    /// <summary>
    /// Initializes LDA.
    /// </summary>
    /// <param name="numComponents">The projection dimensionality, default is 1.</param>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    public LinearDiscriminantAnalysis(int numComponents = 1, ComputingContext? context = null)
    {
        if (numComponents <= 0)
            throw new ArgumentException("The projection dimensionality must be greater than 0.", nameof(numComponents));
        _numComponents = numComponents;
        Context = context;
    }

    /// <inheritdoc />
    public MachineLearningTask Task =>
        MachineLearningTask.DimensionalityReduction | MachineLearningTask.Classification;

    /// <summary>
    /// Execution policy honored by this estimator. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>The discriminant projection matrix, one direction per row.</summary>
    public T[][] ProjectionMatrix => _projectionMatrix;

    /// <summary>
    /// Fits the LDA model.
    /// </summary>
    public void Fit(T[,] x, int[] labels)
    {
        if (x == null || labels == null)
            throw new ArgumentException("Input cannot be null.");
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || labels.Length == 0)
            throw new ArgumentException("Training data cannot be empty.");
        if (rows != labels.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");

        ComputeClassStatistics(x, labels);
        var sw = ComputeWithinClassScatter(x, labels);
        var sb = ComputeBetweenClassScatter(cols);

        var swRegularized = sw.Clone();
        T epsilon = T.CreateChecked(1e-6);
        for (int i = 0; i < swRegularized.Rows; i++)
            swRegularized[i, i] += epsilon;

        var swInverse = swRegularized.Inverse();
        var discriminantMatrix = swInverse * sb;
        _projectionMatrix = ComputeTopEigenVectors(discriminantMatrix, _numComponents);
    }

    /// <summary>
    /// Projects a sample into the LDA subspace.
    /// </summary>
    public T[] Transform(T[] sample)
    {
        if (_projectionMatrix.Length == 0)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (sample.Length != _overallMean.Length)
            throw new ArgumentException("The feature dimensionality does not match.", nameof(sample));

        return ProjectCentered(CenterSample(sample));
    }

    /// <summary>
    /// Projects the entire matrix.
    /// </summary>
    public T[,] Transform(T[,] x)
    {
        if (_projectionMatrix.Length == 0)
            throw new InvalidOperationException("The model has not been fitted yet.");
        return ProjectMatrix(x);
    }

    /// <summary>
    /// Predicts the class (by taking the nearest class center in the LDA space).
    /// </summary>
    public int Predict(T[] sample)
    {
        if (_classMeans.Count == 0)
            throw new InvalidOperationException("The model has not been fitted yet.");

        var projected = Transform(sample);
        int bestClass = _classMeans.Keys.First();
        T bestDistance = T.CreateChecked(double.MaxValue);

        foreach (var (classId, mean) in _classMeans)
        {
            var classCenter = ProjectCentered(CenterSample(mean));
            T distance = T.Zero;
            for (int i = 0; i < projected.Length; i++)
            {
                T diff = projected[i] - classCenter[i];
                distance += diff * diff;
            }
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestClass = classId;
            }
        }
        return bestClass;
    }

    private T[] CenterSample(T[] sample)
    {
        var centered = new T[sample.Length];
        for (int i = 0; i < sample.Length; i++)
            centered[i] = sample[i] - _overallMean[i];
        return centered;
    }

    private T[] ProjectCentered(T[] centered)
    {
        var result = new T[_projectionMatrix.Length];
        for (int k = 0; k < _projectionMatrix.Length; k++)
        {
            T sum = T.Zero;
            for (int j = 0; j < centered.Length; j++)
                sum += centered[j] * _projectionMatrix[k][j];
            result[k] = sum;
        }
        return result;
    }

    private void ComputeClassStatistics(T[,] x, int[] labels)
    {
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        _overallMean = new T[cols];
        _classMeans.Clear();
        _classCounts.Clear();

        for (int i = 0; i < rows; i++)
        {
            int label = labels[i];
            if (!_classMeans.TryGetValue(label, out var mean))
            {
                mean = new T[cols];
                _classMeans[label] = mean;
                _classCounts[label] = 0;
            }
            _classCounts[label]++;
            for (int j = 0; j < cols; j++)
            {
                _overallMean[j] += x[i, j];
                mean[j] += x[i, j];
            }
        }

        for (int j = 0; j < cols; j++)
            _overallMean[j] /= T.CreateChecked(rows);

        foreach (var label in _classMeans.Keys.ToList())
        {
            var mean = _classMeans[label];
            T count = T.CreateChecked(_classCounts[label]);
            for (int j = 0; j < cols; j++)
                mean[j] /= count;
        }
    }

    private Matrix<T> ComputeWithinClassScatter(T[,] x, int[] labels)
    {
        int cols = x.GetLength(1);
        var sw = new Matrix<T>(cols, cols);
        int rows = x.GetLength(0);

        for (int i = 0; i < rows; i++)
        {
            var mean = _classMeans[labels[i]];
            for (int a = 0; a < cols; a++)
            {
                T da = x[i, a] - mean[a];
                for (int b = 0; b < cols; b++)
                {
                    T db = x[i, b] - mean[b];
                    sw[a, b] += da * db;
                }
            }
        }
        return sw;
    }

    private Matrix<T> ComputeBetweenClassScatter(int cols)
    {
        var sb = new Matrix<T>(cols, cols);
        foreach (var (classId, mean) in _classMeans)
        {
            T count = T.CreateChecked(_classCounts[classId]);
            for (int a = 0; a < cols; a++)
            {
                T da = mean[a] - _overallMean[a];
                for (int b = 0; b < cols; b++)
                {
                    T db = mean[b] - _overallMean[b];
                    sb[a, b] += count * da * db;
                }
            }
        }
        return sb;
    }

    private static T[][] ComputeTopEigenVectors(Matrix<T> matrix, int count)
    {
        int n = matrix.Rows;
        int k = Math.Min(count, n);
        var eigenVectors = new T[k][];
        var working = matrix.Clone();

        for (int c = 0; c < k; c++)
        {
            var vector = Enumerable.Repeat(T.One, n).Select(v => v / T.CreateChecked(n)).ToArray();
            T eigenValue = T.Zero;

            for (int iter = 0; iter < 1000; iter++)
            {
                var old = (T[])vector.Clone();
                vector = working.Multiply(vector);
                eigenValue = vector.Max();
                if (eigenValue == T.Zero)
                    break;
                for (int i = 0; i < n; i++)
                    vector[i] /= eigenValue;

                T diff = T.Zero;
                for (int i = 0; i < n; i++)
                    diff += T.Abs(vector[i] - old[i]);
                if (diff < T.CreateChecked(1e-10))
                    break;
            }

            eigenVectors[c] = vector;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    working[i, j] -= eigenValue * vector[i] * vector[j];
            }
        }

        return eigenVectors;
    }

    private T[,] ProjectMatrix(T[,] x)
    {
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        int components = _projectionMatrix.Length;
        var result = new T[rows, components];
        ComputingContextExecution.ForEach(
            Context,
            0,
            rows,
            i =>
            {
                var row = new T[cols];
                for (int j = 0; j < cols; j++)
                    row[j] = x[i, j];
                var projected = Transform(row);
                for (int k = 0; k < components; k++)
                    result[i, k] = projected[k];
            },
            workPerItem: (long)cols * components);
        return result;
    }
}
