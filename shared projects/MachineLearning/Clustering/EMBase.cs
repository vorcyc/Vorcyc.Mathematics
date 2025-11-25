namespace Vorcyc.Mathematics.MachineLearning;

using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics.LinearAlgebra;


/// <summary>
/// 表示期望最大化（EM）算法的基类。
/// </summary>
/// <typeparam name="T">元素类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 和 <see cref="IFloatingPointConstants{T}"/> 接口。</typeparam>
/// <remarks>
/// 期望最大化（EM）算法是一种迭代方法，用于在统计模型中寻找参数的最大似然估计或最大后验估计，其中模型依赖于未观察到的潜在变量。
/// 
/// EM算法包括两个主要步骤：
/// 1. 期望步骤（E步）：计算在当前参数估计下，给定观测数据的条件分布的对数似然函数的期望值。
/// 2. 最大化步骤（M步）：找到使E步中计算的期望对数似然函数最大化的参数。
/// 
/// 该基类提供了基于EM算法的通用功能，如参数初始化、E步和M步，支持高斯混合模型等实现。
/// </remarks>
public abstract class EMBase<T> where T : unmanaged, IFloatingPointIeee754<T>, IFloatingPointConstants<T>
{
    protected readonly int _numClusters;      // 聚类数量
    protected int _numDimensions;             // 数据维度
    protected List<T[]> _data;                // 输入数据
    protected List<T[]> _means;               // 均值向量
    protected List<Matrix<T>> _covariances;   // 协方差矩阵
    protected List<T> _weights;               // 聚类权重
    protected List<T[]> _responsibilities;    // 责任矩阵

    /// <summary>
    /// 初始化 <see cref="EMBase{T}"/> 类的新实例。
    /// </summary>
    /// <param name="numClusters">聚类的数量，必须为正整数。</param>
    /// <exception cref="ArgumentException">当 <paramref name="numClusters"/> 小于等于 0 时抛出。</exception>
    protected EMBase(int numClusters)
    {
        if (numClusters <= 0)
            throw new ArgumentException("聚类数量必须为正整数。", nameof(numClusters));
        _numClusters = numClusters;
    }

    /// <summary>
    /// 初始化EM算法的参数。
    /// </summary>
    /// <param name="data">要拟合的数据，表示为数组的列表。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="data"/> 为 null 时抛出。</exception>
    /// <exception cref="ArgumentException">当 <paramref name="data"/> 为空或维度无效时抛出。</exception>
    protected void InitializeParameters(List<T[]> data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "数据不能为 null。");
        if (data.Count == 0)
            throw new ArgumentException("数据列表不能为空。", nameof(data));
        if (data[0].Length == 0)
            throw new ArgumentException("数据维度必须大于 0。", nameof(data));

        _data = data;
        _numDimensions = data[0].Length;
        Random rand = new Random();

        _means = new List<T[]>(_numClusters);
        _covariances = new List<Matrix<T>>(_numClusters);
        _weights = new List<T>(_numClusters);
        _responsibilities = new List<T[]>(_numClusters);

        for (int i = 0; i < _numClusters; i++)
        {
            // 随机选择初始均值
            _means.Add((T[])data[rand.Next(data.Count)].Clone());
            // 初始化协方差矩阵为单位矩阵
            _covariances.Add(CreateIdentityMatrix(_numDimensions));
            // 初始化权重
            _weights.Add(T.One / T.CreateChecked(_numClusters));
            // 初始化责任矩阵
            _responsibilities.Add(new T[data.Count]);
        }
    }

    /// <summary>
    /// 执行EM算法的期望步骤（E步）。
    /// </summary>
    /// <param name="data">要拟合的数据，表示为数组的列表。</param>
    protected void ExpectationStep(List<T[]> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            T sum = T.Zero;
            Span<T> probs = stackalloc T[_numClusters]; // 临时存储概率

            // 计算每个数据点的责任值
            for (int j = 0; j < _numClusters; j++)
            {
                probs[j] = _weights[j] * MultivariateGaussian(data[i], _means[j], _covariances[j]);
                sum += probs[j];
            }

            // 归一化责任值
            for (int j = 0; j < _numClusters; j++)
            {
                _responsibilities[j][i] = sum != T.Zero ? probs[j] / sum : T.One / T.CreateChecked(_numClusters);
            }
        }
    }

    /// <summary>
    /// 执行EM算法的最大化步骤（M步）。
    /// </summary>
    /// <param name="data">要拟合的数据，表示为数组的列表。</param>
    protected void MaximizationStep(List<T[]> data)
    {
        for (int j = 0; j < _numClusters; j++)
        {
            // 当前聚类的责任值之和
            T responsibilitySum = T.Zero;
            Span<T> respSpan = _responsibilities[j].AsSpan();
            for (int i = 0; i < respSpan.Length; i++)
                responsibilitySum += respSpan[i];

            // 更新权重
            _weights[j] = responsibilitySum / T.CreateChecked(data.Count);

            // 更新均值
            var newMeanElements = new T[_numDimensions];
            for (int i = 0; i < data.Count; i++)
            {
                T resp = _responsibilities[j][i];
                for (int d = 0; d < _numDimensions; d++)
                {
                    newMeanElements[d] += resp * data[i][d];
                }
            }
            for (int d = 0; d < _numDimensions; d++)
            {
                newMeanElements[d] /= responsibilitySum;
            }
            _means[j] = newMeanElements;

            // 更新协方差矩阵
            var newCovarianceElements = new T[_numDimensions, _numDimensions];
            for (int i = 0; i < data.Count; i++)
            {
                T resp = _responsibilities[j][i];
                ReadOnlySpan<T> diff = ComputeDifference(data[i], _means[j]);
                for (int d1 = 0; d1 < _numDimensions; d1++)
                {
                    for (int d2 = 0; d2 < _numDimensions; d2++)
                    {
                        newCovarianceElements[d1, d2] += resp * diff[d1] * diff[d2];
                    }
                }
            }
            for (int d1 = 0; d1 < _numDimensions; d1++)
            {
                for (int d2 = 0; d2 < _numDimensions; d2++)
                {
                    newCovarianceElements[d1, d2] /= responsibilitySum;
                }
            }
            // 添加小的正则化项以确保协方差矩阵可逆
            for (int d = 0; d < _numDimensions; d++)
            {
                newCovarianceElements[d, d] += T.CreateChecked(1e-6); // 正则化
            }
            _covariances[j] = new Matrix<T>(newCovarianceElements);
        }
    }

    /// <summary>
    /// 计算多元高斯分布的概率密度函数值。
    /// </summary>
    /// <param name="x">数据点。</param>
    /// <param name="mean">均值向量。</param>
    /// <param name="covariance">协方差矩阵。</param>
    /// <returns>概率密度函数值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T MultivariateGaussian(ReadOnlySpan<T> x, ReadOnlySpan<T> mean, Matrix<T> covariance)
    {
        int d = x.Length;
        T det = covariance.Determinant();
        if (T.Abs(det) < T.CreateChecked(1e-10))
            return T.Zero; // 避免除零

        ReadOnlySpan<T> diff = ComputeDifference(x, mean);
        Matrix<T> invCov = covariance.Inverse();
        T exponent = ComputeExponent(diff, invCov);

        T normalization = T.Sqrt(T.Pow(T.CreateChecked(2) * T.Pi, T.CreateChecked(d)) * det);
        return T.Exp(T.CreateChecked(-0.5) * exponent) / normalization;
    }

    /// <summary>
    /// 创建指定大小的单位矩阵。
    /// </summary>
    /// <param name="size">矩阵的大小。</param>
    /// <returns>单位矩阵。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Matrix<T> CreateIdentityMatrix(int size)
    {
        return Matrix<T>.Eye(size); // 使用优化后的 Matrix<T>.Eye
    }

    /// <summary>
    /// 计算数据点与均值的差向量。
    /// </summary>
    /// <param name="x">数据点。</param>
    /// <param name="mean">均值向量。</param>
    /// <returns>差向量。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T[] ComputeDifference(ReadOnlySpan<T> x, ReadOnlySpan<T> mean)
    {
        var diff = new T[_numDimensions];
        for (int i = 0; i < _numDimensions; i++)
        {
            diff[i] = x[i] - mean[i];
        }
        return diff;
    }

    /// <summary>
    /// 计算高斯分布的指数项。
    /// </summary>
    /// <param name="diff">差向量。</param>
    /// <param name="invCov">协方差矩阵的逆。</param>
    /// <returns>指数值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T ComputeExponent(ReadOnlySpan<T> diff, Matrix<T> invCov)
    {
        T exponent = T.Zero;
        for (int i = 0; i < _numDimensions; i++)
        {
            T temp = T.Zero;
            for (int j = 0; j < _numDimensions; j++)
            {
                temp += diff[i] * invCov[i, j] * diff[j];
            }
            exponent += temp;
        }
        return exponent;
    }
}