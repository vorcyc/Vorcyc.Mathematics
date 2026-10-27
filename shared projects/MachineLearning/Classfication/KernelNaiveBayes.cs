using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning.Classfication;

/// <summary>
/// Naive Bayes with an independent Gaussian-kernel density on each feature.
/// </summary>
public sealed class KernelNaiveBayes<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly double _bandwidthScale;
    private int[] _labels = [];
    private double[] _priors = [];
    private double[][][] _values = [];
    private double[][] _bandwidth = [];
    private bool _isFitted;

    public KernelNaiveBayes(T bandwidthScale = default, ComputingContext? context = null)
    {
        _bandwidthScale = bandwidthScale.Equals(default) ? 1.0 : double.CreateChecked(bandwidthScale);
        if (_bandwidthScale <= 0)
            throw new ArgumentOutOfRangeException(nameof(bandwidthScale));
        Context = context;
    }

    public ComputingContext? Context { get; set; }

    public MachineLearningTask Task => MachineLearningTask.Classification;

    public void Fit(T[,] x, int[] y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || rows != y.Length)
            throw new ArgumentException("Training data is empty or the labels do not match.");

        var groups = new SortedDictionary<int, List<int>>();
        for (int i = 0; i < rows; i++)
        {
            if (!groups.TryGetValue(y[i], out var list))
            {
                list = [];
                groups[y[i]] = list;
            }

            list.Add(i);
        }

        _labels = groups.Keys.ToArray();
        _priors = new double[_labels.Length];
        _values = new double[_labels.Length][][];
        _bandwidth = new double[_labels.Length][];

        for (int classIndex = 0; classIndex < _labels.Length; classIndex++)
        {
            var rowsInClass = groups[_labels[classIndex]];
            _priors[classIndex] = rowsInClass.Count / (double)rows;
            _values[classIndex] = new double[cols][];
            _bandwidth[classIndex] = new double[cols];
            for (int feature = 0; feature < cols; feature++)
            {
                var column = new double[rowsInClass.Count];
                double mean = 0;
                for (int i = 0; i < rowsInClass.Count; i++)
                {
                    column[i] = double.CreateChecked(x[rowsInClass[i], feature]);
                    mean += column[i];
                }

                mean /= column.Length;
                double variance = 0;
                for (int i = 0; i < column.Length; i++)
                {
                    double delta = column[i] - mean;
                    variance += delta * delta;
                }

                variance = column.Length > 1 ? variance / (column.Length - 1) : 0;
                double sigma = Math.Sqrt(variance);
                double scott = sigma * Math.Pow(column.Length, -0.2);
                _bandwidth[classIndex][feature] = Math.Max(1e-3, _bandwidthScale * (scott == 0 ? 1e-3 : scott));
                _values[classIndex][feature] = column;
            }
        }

        _isFitted = true;
    }

    public int Predict(T[] sample)
    {
        EnsureFitted();
        return _labels[ArgMax(LogPosteriors(sample))];
    }

    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        EnsureFitted();
        ArgumentNullException.ThrowIfNull(x);
        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The prediction buffer is shorter than the sample count.");

        for (int i = 0; i < rows; i++)
            predictions[i] = Predict(Array2DHelpers.GetRow(x, i));
    }

    double[] LogPosteriors(T[] sample)
    {
        if (sample.Length != _bandwidth[0].Length)
            throw new ArgumentException("Feature dimension does not match the model.");

        var logPosterior = new double[_labels.Length];
        for (int classIndex = 0; classIndex < _labels.Length; classIndex++)
        {
            double logDensity = Math.Log(_priors[classIndex]);
            for (int feature = 0; feature < sample.Length; feature++)
            {
                double value = double.CreateChecked(sample[feature]);
                double bandwidth = _bandwidth[classIndex][feature];
                double[] column = _values[classIndex][feature];
                double density = 0;
                double inv = 1.0 / (column.Length * bandwidth * Math.Sqrt(2.0 * Math.PI));
                for (int i = 0; i < column.Length; i++)
                {
                    double z = (value - column[i]) / bandwidth;
                    density += Math.Exp(-0.5 * z * z);
                }

                logDensity += Math.Log(Math.Max(density * inv, 1e-300));
            }

            logPosterior[classIndex] = logDensity;
        }

        return logPosterior;
    }

    static int ArgMax(double[] values)
    {
        int best = 0;
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > values[best])
                best = i;
        }

        return best;
    }

    void EnsureFitted()
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
    }
}
