using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Schölkopf one-class SVM with an RBF kernel. Higher <see cref="Score(T[])"/> means more anomalous.
/// </summary>
public sealed class OneClassSvm<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly double _nu;
    private readonly double _gamma;
    private readonly int _iterations;
    private double[][] _inputs = [];
    private double[] _alpha = [];
    private double _rho;
    private double _upper;
    private bool _isFitted;

    public OneClassSvm(T? nu = null, T? gamma = null, int iterations = 200)
    {
        _nu = nu.HasValue ? double.CreateChecked(nu.Value) : 0.1;
        if (_nu <= 0 || _nu > 1)
            throw new ArgumentOutOfRangeException(nameof(nu), "nu must lie in (0, 1].");
        _gamma = gamma.HasValue ? double.CreateChecked(gamma.Value) : double.NaN;
        if (iterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(iterations));
        _iterations = iterations;
    }

    public MachineLearningTask Task => MachineLearningTask.AnomalyDetection;

    public void Fit(T[,] x)
    {
        ArgumentNullException.ThrowIfNull(x);
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows < 2 || cols == 0)
            throw new ArgumentException("One-class SVM needs at least two samples.");

        _inputs = new double[rows][];
        for (int i = 0; i < rows; i++)
        {
            _inputs[i] = new double[cols];
            for (int j = 0; j < cols; j++)
                _inputs[i][j] = double.CreateChecked(x[i, j]);
        }

        double gamma = double.IsNaN(_gamma) ? 1.0 / cols : _gamma;
        var kernel = new double[rows, rows];
        for (int i = 0; i < rows; i++)
        {
            kernel[i, i] = 1;
            for (int j = i + 1; j < rows; j++)
            {
                double value = Math.Exp(-gamma * SquaredDistance(_inputs[i], _inputs[j]));
                kernel[i, j] = value;
                kernel[j, i] = value;
            }
        }

        _upper = 1.0 / (_nu * rows);
        _alpha = new double[rows];
        for (int i = 0; i < rows; i++)
            _alpha[i] = 1.0 / rows;

        for (int iteration = 0; iteration < _iterations; iteration++)
        {
            var gradient = Multiply(kernel, _alpha);
            double current = 0.5 * Dot(_alpha, gradient);
            double step = 0.5;
            bool improved = false;
            var trial = new double[rows];
            for (int attempt = 0; attempt < 12; attempt++)
            {
                for (int i = 0; i < rows; i++)
                    trial[i] = _alpha[i] - step * gradient[i];
                ProjectSimplex(trial, _upper);
                double next = 0.5 * Dot(trial, Multiply(kernel, trial));
                if (next <= current + 1e-12)
                {
                    trial.AsSpan().CopyTo(_alpha);
                    improved = true;
                    break;
                }

                step *= 0.5;
            }

            if (!improved)
                break;
        }

        var scores = Multiply(kernel, _alpha);
        double rho = 0;
        int count = 0;
        for (int i = 0; i < rows; i++)
        {
            if (_alpha[i] > 1e-8 && _alpha[i] < _upper - 1e-8)
            {
                rho += scores[i];
                count++;
            }
        }

        if (count == 0)
        {
            for (int i = 0; i < rows; i++)
            {
                if (_alpha[i] > 1e-8)
                {
                    rho += scores[i];
                    count++;
                }
            }
        }

        _rho = count == 0 ? 0 : rho / count;
        _gammaFitted = gamma;
        _isFitted = true;
    }

    private double _gammaFitted;

    /// <summary>Anomaly score. Larger values are farther outside the learned region.</summary>
    public T Score(T[] sample)
    {
        EnsureFitted();
        return T.CreateChecked(-Decision(ToDouble(sample)));
    }

    /// <summary>Labels samples at or above <paramref name="threshold"/> as anomalies (1).</summary>
    public int[] Predict(T[,] x, T threshold)
    {
        ArgumentNullException.ThrowIfNull(x);
        var scores = new T[x.GetLength(0)];
        Score(x, scores);
        double limit = double.CreateChecked(threshold);
        var labels = new int[scores.Length];
        for (int i = 0; i < scores.Length; i++)
            labels[i] = double.CreateChecked(scores[i]) >= limit ? 1 : 0;
        return labels;
    }

    public void Score(T[,] x, Span<T> scores)
    {
        EnsureFitted();
        ArgumentNullException.ThrowIfNull(x);
        int rows = x.GetLength(0);
        if (scores.Length < rows)
            throw new ArgumentException("The score buffer is shorter than the sample count.");

        var row = new double[x.GetLength(1)];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < row.Length; j++)
                row[j] = double.CreateChecked(x[i, j]);
            scores[i] = T.CreateChecked(-Decision(row));
        }
    }

    double Decision(double[] sample)
    {
        double value = 0;
        for (int i = 0; i < _inputs.Length; i++)
        {
            if (_alpha[i] == 0)
                continue;
            value += _alpha[i] * Math.Exp(-_gammaFitted * SquaredDistance(_inputs[i], sample));
        }

        return value - _rho;
    }

    static double[] ToDouble(T[] sample)
    {
        var values = new double[sample.Length];
        for (int i = 0; i < sample.Length; i++)
            values[i] = double.CreateChecked(sample[i]);
        return values;
    }

    void EnsureFitted()
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
    }

    static double SquaredDistance(double[] left, double[] right)
    {
        double sum = 0;
        for (int i = 0; i < left.Length; i++)
        {
            double delta = left[i] - right[i];
            sum += delta * delta;
        }

        return sum;
    }

    static double[] Multiply(double[,] matrix, double[] vector)
    {
        int rows = matrix.GetLength(0);
        var result = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double sum = 0;
            for (int j = 0; j < vector.Length; j++)
                sum += matrix[i, j] * vector[j];
            result[i] = sum;
        }

        return result;
    }

    static double Dot(double[] left, double[] right)
    {
        double sum = 0;
        for (int i = 0; i < left.Length; i++)
            sum += left[i] * right[i];
        return sum;
    }

    static void ProjectSimplex(double[] values, double upper)
    {
        double low = values[0] - 1;
        double high = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] - 1 < low)
                low = values[i] - 1;
            if (values[i] > high)
                high = values[i];
        }

        for (int iteration = 0; iteration < 60; iteration++)
        {
            double mid = 0.5 * (low + high);
            double sum = 0;
            for (int i = 0; i < values.Length; i++)
                sum += Math.Clamp(values[i] - mid, 0, upper);
            if (sum > 1)
                low = mid;
            else
                high = mid;
        }

        double tau = 0.5 * (low + high);
        for (int i = 0; i < values.Length; i++)
            values[i] = Math.Clamp(values[i] - tau, 0, upper);
    }
}
