using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// 标准化缩放：x' = (x - mean) / std。
/// </summary>
public class StandardScaler<T> : IMatrixTransformInto<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T[] _mean = [];
    private T[] _std = [];
    private bool _isFitted;

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.None;

    /// <summary>各特征均值。</summary>
    public IReadOnlyList<T> Mean =>
        _isFitted ? _mean : throw new InvalidOperationException("缩放器尚未拟合。");

    /// <summary>各特征标准差。</summary>
    public IReadOnlyList<T> Std =>
        _isFitted ? _std : throw new InvalidOperationException("缩放器尚未拟合。");

    /// <summary>
    /// 根据数据估计均值与标准差。
    /// </summary>
    public void Fit(T[,] x)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0)
            throw new ArgumentException("输入矩阵不能为空。");

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
            throw new ArgumentException("destination 形状须与 source 一致。", nameof(destination));

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                destination[i, j] = (source[i, j] - _mean[j]) / _std[j];
        }
    }

    /// <summary>
    /// 变换单向量。
    /// </summary>
    public T[] Transform(T[] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("缩放器尚未拟合。");
        if (x == null || x.Length != _mean.Length)
            throw new ArgumentException("特征维度不匹配。", nameof(x));

        var result = new T[x.Length];
        for (int j = 0; j < x.Length; j++)
            result[j] = (x[j] - _mean[j]) / _std[j];
        return result;
    }

    private void EnsureFitted(T[,] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("缩放器尚未拟合。");
        if (x.GetLength(1) != _mean.Length)
            throw new ArgumentException("特征维度不匹配。");
    }

    /// <summary>
    /// 从已保存的均值与标准差恢复缩放器。
    /// </summary>
    public void LoadState(T[] mean, T[] std)
    {
        if (mean == null || std == null)
            throw new ArgumentException("均值与标准差不能为 null。");
        if (mean.Length == 0 || mean.Length != std.Length)
            throw new ArgumentException("均值与标准差长度必须相同且非空。");

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
