using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// Min-max scaling: x' = (x - min) / (max - min).
/// </summary>
public class MinMaxScaler<T> : IMatrixTransformInto<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T[] _min = [];
    private T[] _range = [];
    private bool _isFitted;

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.None;

    /// <summary>
    /// Fits the scaling parameters.
    /// </summary>
    public void Fit(T[,] x)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0)
            throw new ArgumentException("The input matrix cannot be empty.");

        _min = new T[cols];
        _range = new T[cols];

        for (int j = 0; j < cols; j++)
        {
            T min = x[0, j];
            T max = x[0, j];
            for (int i = 1; i < rows; i++)
            {
                if (x[i, j] < min) min = x[i, j];
                if (x[i, j] > max) max = x[i, j];
            }
            _min[j] = min;
            T range = max - min;
            _range[j] = range == T.Zero ? T.One : range;
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
        if (!_isFitted)
            throw new InvalidOperationException("The scaler has not been fitted yet.");
        if (x.GetLength(1) != _min.Length)
            throw new ArgumentException("The feature dimensionality does not match.");

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        var result = new T[rows, cols];
        TransformInto(x, result);
        return result;
    }

    /// <inheritdoc />
    public void TransformInto(T[,] source, T[,] destination)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The scaler has not been fitted yet.");
        if (source.GetLength(1) != _min.Length)
            throw new ArgumentException("The feature dimensionality does not match.");

        int rows = source.GetLength(0);
        int cols = source.GetLength(1);
        if (destination.GetLength(0) != rows || destination.GetLength(1) != cols)
            throw new ArgumentException("The destination shape must match the source.", nameof(destination));

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                destination[i, j] = (source[i, j] - _min[j]) / _range[j];
        }
    }

    /// <summary>
    /// Transforms a single vector.
    /// </summary>
    public T[] Transform(T[] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The scaler has not been fitted yet.");
        if (x == null || x.Length != _min.Length)
            throw new ArgumentException("The feature dimensionality does not match.", nameof(x));

        var result = new T[x.Length];
        for (int j = 0; j < x.Length; j++)
            result[j] = (x[j] - _min[j]) / _range[j];
        return result;
    }
}
