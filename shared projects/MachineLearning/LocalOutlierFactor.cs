using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Local outlier factor. Scores near 1 are inliers; larger scores are more isolated.
/// </summary>
public sealed class LocalOutlierFactor<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly int _neighbors;
    private double[][] _points = [];
    private double[] _localDensity = [];
    private double[] _kDistance = [];
    private int[][] _neighborIndex = [];
    private bool _isFitted;

    public LocalOutlierFactor(int neighbors = 20)
    {
        if (neighbors < 1)
            throw new ArgumentOutOfRangeException(nameof(neighbors));
        _neighbors = neighbors;
    }

    public MachineLearningTask Task => MachineLearningTask.AnomalyDetection;

    public int NeighborCount => _neighborIndex.Length == 0 ? _neighbors : _neighborIndex[0].Length;

    public void Fit(T[,] x)
    {
        ArgumentNullException.ThrowIfNull(x);
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows < 2 || cols == 0)
            throw new ArgumentException("Local outlier factor needs at least two samples.");

        int k = Math.Min(_neighbors, rows - 1);
        _points = new double[rows][];
        for (int i = 0; i < rows; i++)
        {
            _points[i] = new double[cols];
            for (int j = 0; j < cols; j++)
                _points[i][j] = double.CreateChecked(x[i, j]);
        }

        _neighborIndex = new int[rows][];
        _kDistance = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            var order = OrderByDistance(i, excludeSelf: true);
            _neighborIndex[i] = order[..k];
            _kDistance[i] = Distance(_points[i], _points[order[k - 1]]);
        }

        _localDensity = new double[rows];
        for (int i = 0; i < rows; i++)
            _localDensity[i] = LocalDensity(_points[i], _neighborIndex[i]);
        _isFitted = true;
    }

    public T Score(T[] sample)
    {
        EnsureFitted();
        return T.CreateChecked(ScoreCore(ToDouble(sample)));
    }

    public void Score(T[,] x, Span<T> scores)
    {
        EnsureFitted();
        ArgumentNullException.ThrowIfNull(x);
        int rows = x.GetLength(0);
        if (scores.Length < rows)
            throw new ArgumentException("The score buffer is shorter than the sample count.");
        if (x.GetLength(1) != _points[0].Length)
            throw new ArgumentException("Feature dimension does not match the model.");

        bool sameTraining = rows == _points.Length && SameMatrix(x);
        for (int i = 0; i < rows; i++)
        {
            if (sameTraining)
                scores[i] = T.CreateChecked(TrainingScore(i));
            else
            {
                var row = new double[_points[0].Length];
                for (int j = 0; j < row.Length; j++)
                    row[j] = double.CreateChecked(x[i, j]);
                scores[i] = T.CreateChecked(ScoreCore(row));
            }
        }
    }

    /// <summary>Labels samples at or above <paramref name="threshold"/> as anomalies (1).</summary>
    public int[] Predict(T[,] x, T threshold)
    {
        var scores = new T[x.GetLength(0)];
        Score(x, scores);
        double limit = double.CreateChecked(threshold);
        var labels = new int[scores.Length];
        for (int i = 0; i < scores.Length; i++)
            labels[i] = double.CreateChecked(scores[i]) >= limit ? 1 : 0;
        return labels;
    }

    double TrainingScore(int index)
    {
        double density = _localDensity[index];
        if (density <= 0)
            return double.PositiveInfinity;

        double sum = 0;
        int[] neighbors = _neighborIndex[index];
        for (int n = 0; n < neighbors.Length; n++)
            sum += _localDensity[neighbors[n]] / density;
        return sum / neighbors.Length;
    }

    double ScoreCore(double[] sample)
    {
        int k = _neighborIndex[0].Length;
        var neighbors = Nearest(sample, k);
        double density = LocalDensity(sample, neighbors);
        if (density <= 0)
            return double.PositiveInfinity;

        double sum = 0;
        for (int i = 0; i < neighbors.Length; i++)
            sum += _localDensity[neighbors[i]] / density;
        return sum / neighbors.Length;
    }

    double LocalDensity(double[] sample, int[] neighbors)
    {
        double sum = 0;
        for (int i = 0; i < neighbors.Length; i++)
        {
            int neighbor = neighbors[i];
            double reach = Math.Max(_kDistance[neighbor], Distance(sample, _points[neighbor]));
            sum += reach;
        }

        double mean = sum / neighbors.Length;
        return mean <= 1e-15 ? double.PositiveInfinity : 1.0 / mean;
    }

    int[] Nearest(double[] sample, int k)
    {
        var order = new int[_points.Length];
        for (int i = 0; i < order.Length; i++)
            order[i] = i;
        Array.Sort(order, (a, b) =>
        {
            int compare = Distance(sample, _points[a]).CompareTo(Distance(sample, _points[b]));
            return compare != 0 ? compare : a.CompareTo(b);
        });
        return order[..k];
    }

    int[] OrderByDistance(int index, bool excludeSelf)
    {
        int n = _points.Length;
        var order = new int[excludeSelf ? n - 1 : n];
        int cursor = 0;
        for (int i = 0; i < n; i++)
        {
            if (excludeSelf && i == index)
                continue;
            order[cursor++] = i;
        }

        Array.Sort(order, (a, b) =>
        {
            int compare = Distance(_points[index], _points[a]).CompareTo(Distance(_points[index], _points[b]));
            return compare != 0 ? compare : a.CompareTo(b);
        });
        return order;
    }

    bool SameMatrix(T[,] x)
    {
        for (int i = 0; i < _points.Length; i++)
        {
            for (int j = 0; j < _points[i].Length; j++)
            {
                if (double.CreateChecked(x[i, j]) != _points[i][j])
                    return false;
            }
        }

        return true;
    }

    static double[] ToDouble(T[] sample)
    {
        var values = new double[sample.Length];
        for (int i = 0; i < sample.Length; i++)
            values[i] = double.CreateChecked(sample[i]);
        return values;
    }

    static double Distance(double[] left, double[] right)
    {
        double sum = 0;
        for (int i = 0; i < left.Length; i++)
        {
            double delta = left[i] - right[i];
            sum += delta * delta;
        }

        return Math.Sqrt(sum);
    }

    void EnsureFitted()
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
    }
}
