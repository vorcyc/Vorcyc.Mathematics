using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// 表示期望最大化（EM）算法的基类。
/// </summary>
/// <typeparam name="T">元素类型，必须实现 <see cref="System.Numerics.IFloatingPointIeee754{T}"/> 和 <see cref="System.Numerics.IFloatingPointConstants{T}"/> 接口。</typeparam>
/// <remarks>
/// 期望最大化（EM）算法是一种迭代方法，用于在统计模型中寻找参数的最大似然估计或最大后验估计，其中模型依赖于未观察到的潜在变量。
/// 
/// EM算法包括两个主要步骤：
/// 1. 期望步骤（E步）：计算在当前参数估计下，给定观测数据的条件分布的对数似然函数的期望值。
/// 2. 最大化步骤（M步）：找到使E步中计算的期望对数似然函数最大化的参数。
/// 
/// 该基类提供了基于EM算法的通用功能，如参数初始化、E步和M步。
/// </remarks>
public abstract class EMBase<T>
    where T : struct, System.Numerics.IFloatingPointIeee754<T>, System.Numerics.IFloatingPointConstants<T>
{
    protected int _numClusters;
    protected int _numDimensions;
    protected List<Vector<T>> _data;
    protected List<Vector<T>> _means;
    protected List<Matrix<T>> _covariances;
    protected List<T> _weights;
    protected List<T[]> _responsibilities;

    /// <summary>
    /// 初始化 <see cref="EMBase{T}"/> 类的新实例。
    /// </summary>
    /// <param name="numClusters">聚类的数量。</param>
    protected EMBase(int numClusters)
    {
        _numClusters = numClusters;
    }

    /// <summary>
    /// 初始化EM算法的参数。
    /// </summary>
    /// <param name="data">要拟合的数据，表示为向量的列表。</param>
    protected void InitializeParameters(List<Vector<T>> data)
    {
        Random rand = new Random();
        _means = new List<Vector<T>>();
        _covariances = new List<Matrix<T>>();
        _weights = new List<T>();
        _responsibilities = new List<T[]>();

        for (int i = 0; i < _numClusters; i++)
        {
            // 随机选择初始均值
            _means.Add(data[rand.Next(data.Count)]);
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
    /// <param name="data">要拟合的数据，表示为向量的列表。</param>
    protected void ExpectationStep(List<Vector<T>> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < _numClusters; j++)
            {
                // 计算每个数据点和聚类的责任值
                _responsibilities[j][i] = _weights[j] * MultivariateGaussian(data[i], _means[j], _covariances[j]);
                sum += _responsibilities[j][i];
            }
            for (int j = 0; j < _numClusters; j++)
            {
                // 归一化责任值
                _responsibilities[j][i] /= sum;
            }
        }
    }

    /// <summary>
    /// 执行EM算法的最大化步骤（M步）。
    /// </summary>
    /// <param name="data">要拟合的数据，表示为向量的列表。</param>
    protected void MaximizationStep(List<Vector<T>> data)
    {
        for (int j = 0; j < _numClusters; j++)
        {
            // 当前聚类的责任值之和
            T responsibilitySum = Statistics.Basic.Sum(_responsibilities[j].AsSpan());
            // 更新权重
            _weights[j] = responsibilitySum / T.CreateChecked(data.Count);

            // 更新均值
            T[] newMeanElements = new T[_numDimensions];
            for (int i = 0; i < data.Count; i++)
            {
                for (int d = 0; d < _numDimensions; d++)
                {
                    newMeanElements[d] += _responsibilities[j][i] * data[i].Elements[d];
                }
            }
            for (int d = 0; d < _numDimensions; d++)
            {
                newMeanElements[d] /= responsibilitySum;
            }
            _means[j] = new Vector<T>(newMeanElements);

            // 更新协方差矩阵
            T[,] newCovarianceElements = new T[_numDimensions, _numDimensions];
            for (int i = 0; i < data.Count; i++)
            {
                Vector<T> diff = data[i] - _means[j];
                for (int d1 = 0; d1 < _numDimensions; d1++)
                {
                    for (int d2 = 0; d2 < _numDimensions; d2++)
                    {
                        newCovarianceElements[d1, d2] += _responsibilities[j][i] * diff.Elements[d1] * diff.Elements[d2];
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
    protected T MultivariateGaussian(Vector<T> x, Vector<T> mean, Matrix<T> covariance)
    {
        int d = x.Dimension;
        T det = covariance.Determinant();
        Vector<T> diff = x - mean;
        Matrix<T> invCov = covariance.Inverse();
        T exponent = T.Zero;
        for (int i = 0; i < d; i++)
        {
            for (int j = 0; j < d; j++)
            {
                exponent += diff.Elements[i] * invCov[i, j] * diff.Elements[j];
            }
        }
        return T.Exp(T.CreateChecked(-0.5) * exponent) / T.Sqrt(T.Pow(T.CreateChecked(2) * T.Pi, T.CreateChecked(d)) * det);
    }

    /// <summary>
    /// 创建指定大小的单位矩阵。
    /// </summary>
    /// <param name="size">矩阵的大小。</param>
    /// <returns>单位矩阵。</returns>
    protected Matrix<T> CreateIdentityMatrix(int size)
    {
        T[,] matrix = new T[size, size];
        for (int i = 0; i < size; i++)
        {
            matrix[i, i] = T.One;
        }
        return new Matrix<T>(matrix);
    }
}
