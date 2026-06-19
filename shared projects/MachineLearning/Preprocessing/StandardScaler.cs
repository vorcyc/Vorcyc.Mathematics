using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// Standardization scaling: x' = (x - mean) / std.
/// </summary>
public class StandardScaler<T> : IMatrixTransformInto<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T[] _mean = [];
    private T[] _std = [];
    private bool _isFitted;

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.None;

    /// <summary>The per-feature means.</summary>
    public IReadOnlyList<T> Mean =>
        _isFitted ? _mean : throw new InvalidOperationException("The scaler has not been fitted yet.");

    /// <summary>The per-feature standard deviations.</summary>
    public IReadOnlyList<T> Std =>
        _isFitted ? _std : throw new InvalidOperationException("The scaler has not been fitted yet.");

    /// <summary>
    /// Estimates the mean and standard deviation from the data.
    /// </summary>
    public void Fit(T[,] x)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0)
            throw new ArgumentException("The input matrix cannot be empty.");

        _mean = new T[cols];
        _std = new T[cols];

        for (int j = 0; j < cols; j++)
        {
            T sum = T.Zero;
            for (int i = 0; i < rows; i++)
                sum += x[i, j];
            _mean[j] = sum / T.CreateChecked(rows);

            T varSum = T.Zero;
            for (int i = 0; i < rows; i++)
            {
                T diff = x[i, j] - _mean[j];
                varSum += diff * diff;
            }
            T variance = varSum / T.CreateChecked(rows);
            _std[j] = T.Sqrt(variance);
            if (_std[j] == T.Zero)
                _std[j] = T.One;
        }

        _isFitted = true;
    }

    /// <summary>
    /// Fits and transforms.
    /// </summary>
    public T[,] FitTransform(T[,] x)
    {
        Fit(x);
        return Transform(x);
    }

    /// <summary>
    /// Transforms a matrix.
    /// </summary>
    public T[,] Transform(T[,] x)
    {
        EnsureFitted(x);
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        var result = new T[rows, cols];
        TransformInto(x, result);
        return result;
    }

    /// <inheritdoc />
    public void TransformInto(T[,] source, T[,] destination)
    {
        EnsureFitted(source);
        int rows = source.GetLength(0);
        int cols = source.GetLength(1);
        if (destination.GetLength(0) != rows || destination.GetLength(1) != cols)
            throw new ArgumentException("The destination shape must match the source.", nameof(destination));

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                destination[i, j] = (source[i, j] - _mean[j]) / _std[j];
        }
    }

    /// <summary>
    /// Transforms a single vector.
    /// </summary>
    public T[] Transform(T[] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The scaler has not been fitted yet.");
        if (x == null || x.Length != _mean.Length)
            throw new ArgumentException("The feature dimensionality does not match.", nameof(x));

        var result = new T[x.Length];
        for (int j = 0; j < x.Length; j++)
            result[j] = (x[j] - _mean[j]) / _std[j];
        return result;
    }

    private void EnsureFitted(T[,] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The scaler has not been fitted yet.");
        if (x.GetLength(1) != _mean.Length)
            throw new ArgumentException("The feature dimensionality does not match.");
    }

    /// <summary>
    /// Restores the scaler from saved means and standard deviations.
    /// </summary>
    public void LoadState(T[] mean, T[] std)
    {
        if (mean == null || std == null)
            throw new ArgumentException("The mean and standard deviation cannot be null.");
        if (mean.Length == 0 || mean.Length != std.Length)
            throw new ArgumentException("The mean and standard deviation must have the same length and be non-empty.");

        _mean = (T[])mean.Clone();
        _std = (T[])std.Clone();
        for (int i = 0; i < _std.Length; i++)
        {
            if (_std[i] == T.Zero)
                _std[i] = T.One;
        }
        _isFitted = true;
    }
}
