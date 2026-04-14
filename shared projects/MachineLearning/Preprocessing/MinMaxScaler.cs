using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// 最小-最大缩放：x' = (x - min) / (max - min)。
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
    /// 拟合缩放参数。
    /// </summary>
    public void Fit(T[,] x)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0)
            throw new ArgumentException("输入矩阵不能为空。");

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
    /// 拟合并变换。
    /// </summary>
    public T[,] FitTransform(T[,] x)
    {
        Fit(x);
        return Transform(x);
    }

    /// <summary>
    /// 变换矩阵。
    /// </summary>
    public T[,] Transform(T[,] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("缩放器尚未拟合。");
        if (x.GetLength(1) != _min.Length)
            throw new ArgumentException("特征维度不匹配。");

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
            throw new InvalidOperationException("缩放器尚未拟合。");
        if (source.GetLength(1) != _min.Length)
            throw new ArgumentException("特征维度不匹配。");

        int rows = source.GetLength(0);
        int cols = source.GetLength(1);
        if (destination.GetLength(0) != rows || destination.GetLength(1) != cols)
            throw new ArgumentException("destination 形状须与 source 一致。", nameof(destination));

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                destination[i, j] = (source[i, j] - _min[j]) / _range[j];
        }
    }

    /// <summary>
    /// 变换单向量。
    /// </summary>
    public T[] Transform(T[] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("缩放器尚未拟合。");
        if (x == null || x.Length != _min.Length)
            throw new ArgumentException("特征维度不匹配。", nameof(x));

        var result = new T[x.Length];
        for (int j = 0; j < x.Length; j++)
            result[j] = (x[j] - _min[j]) / _range[j];
        return result;
    }
}
