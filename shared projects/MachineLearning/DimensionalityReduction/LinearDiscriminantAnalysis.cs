using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;

/// <summary>
/// 线性判别分析 (LDA)，用于有监督降维与分类。
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
    /// 初始化 LDA。
    /// </summary>
    /// <param name="numComponents">投影维度，默认为 1。</param>
    public LinearDiscriminantAnalysis(int numComponents = 1)
    {
        if (numComponents <= 0)
            throw new ArgumentException("投影维度必须大于 0。", nameof(numComponents));
        _numComponents = numComponents;
    }

    /// <inheritdoc />
    public MachineLearningTask Task =>
        MachineLearningTask.DimensionalityReduction | MachineLearningTask.Classification;

    /// <summary>判别投影矩阵，每行一个方向。</summary>
    public T[][] ProjectionMatrix => _projectionMatrix;

    /// <summary>
    /// 拟合 LDA 模型。
    /// </summary>
    public void Fit(T[,] x, int[] labels)
    {
        if (x == null || labels == null)
            throw new ArgumentException("输入不能为 null。");
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || labels.Length == 0)
            throw new ArgumentException("训练数据不能为空。");
        if (rows != labels.Length)
            throw new ArgumentException("样本数与标签数不匹配。");

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
    /// 将样本投影到 LDA 子空间。
    /// </summary>
    public T[] Transform(T[] sample)
    {
        if (_projectionMatrix.Length == 0)
            throw new InvalidOperationException("模型尚未拟合。");
        if (sample.Length != _overallMean.Length)
            throw new ArgumentException("特征维度不匹配。", nameof(sample));

        return ProjectCentered(CenterSample(sample));
    }

    /// <summary>
    /// 投影整个矩阵。
    /// </summary>
    public T[,] Transform(T[,] x)
    {
        if (_projectionMatrix.Length == 0)
            throw new InvalidOperationException("模型尚未拟合。");
        return ProjectMatrix(x);
    }

    /// <summary>
    /// 预测类别（在 LDA 空间中取最近类中心）。
    /// </summary>
    public int Predict(T[] sample)
    {
        if (_classMeans.Count == 0)
            throw new InvalidOperationException("模型尚未拟合。");

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
        int components = _projectionMatrix.Length;
        var result = new T[rows, components];
        for (int i = 0; i < rows; i++)
        {
            var row = new T[x.GetLength(1)];
            for (int j = 0; j < row.Length; j++)
                row[j] = x[i, j];
            var projected = Transform(row);
            for (int k = 0; k < components; k++)
                result[i, k] = projected[k];
        }
        return result;
    }
}
