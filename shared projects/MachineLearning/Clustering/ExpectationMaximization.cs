using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// 表示期望最大化算法的实现。
/// </summary>
/// <typeparam name="T">元素类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 和 <see cref="IFloatingPointConstants{T}"/> 接口。</typeparam>
/// <remarks>
/// 期望最大化（EM）算法是一种迭代算法，用于在存在隐变量的情况下寻找参数的最大似然估计或最大后验估计。它主要用于聚类分析和密度估计。
/// 
/// EM算法包括两个主要步骤：
/// 1. 期望步骤（E步）：计算每个数据点属于每个聚类的责任值。
/// 2. 最大化步骤（M步）：根据责任值更新模型参数（均值、协方差矩阵和权重）。
/// 
/// 该实现假设数据点服从多元高斯分布，并使用高斯混合模型（GMM）进行聚类。优化版本添加了收敛检查和性能改进。
/// </remarks>
public class ExpectationMaximization<T> : EMBase<T>, IMachineLearning
    where T : unmanaged, IFloatingPointIeee754<T>, IFloatingPointConstants<T>
{
    /// <summary>
    /// 初始化 <see cref="ExpectationMaximization{T}"/> 类的新实例。
    /// </summary>
    /// <param name="numClusters">聚类的数量，必须为正整数。</param>
    public ExpectationMaximization(int numClusters) : base(numClusters) { }

    /// <summary>
    /// 获取聚类中心（均值）。
    /// </summary>
    public IReadOnlyList<T[]> Means => _means;

    /// <summary>
    /// 获取协方差矩阵。
    /// </summary>
    public IReadOnlyList<Matrix<T>> Covariances => _covariances;

    /// <summary>
    /// 获取权重。
    /// </summary>
    public IReadOnlyList<T> Weights => _weights;

    /// <summary>
    /// 获取机器学习任务类型。
    /// </summary>
    public MachineLearningTask Task => MachineLearningTask.Clustering;

    /// <summary>
    /// 使用期望最大化算法拟合数据。
    /// </summary>
    /// <param name="data">要拟合的数据，表示为数组的列表。</param>
    /// <param name="maxIterations">最大迭代次数，默认值为 100。</param>
    /// <param name="tolerance">收敛容差，默认值为 1e-4。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="data"/> 为 null 时抛出。</exception>
    /// <exception cref="ArgumentException">当 <paramref name="data"/> 为空或维度无效时抛出。</exception>
    public void Fit(List<T[]> data, int maxIterations = 100, T tolerance = default)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "数据不能为 null。");
        if (data.Count == 0)
            throw new ArgumentException("数据列表不能为空。", nameof(data));
        if (data[0].Length == 0)
            throw new ArgumentException("数据维度必须大于 0。", nameof(data));

        InitializeParameters(data);

        T lastLikelihood = T.NegativeInfinity;
        for (int i = 0; i < maxIterations; i++)
        {
            ExpectationStep(data);
            MaximizationStep(data);

            T likelihood = CalculateLogLikelihood(data);
            if (i > 0 && T.Abs(likelihood - lastLikelihood) < (tolerance == default ? T.CreateChecked(1e-4) : tolerance))
                break;

            lastLikelihood = likelihood;
        }
    }

    /// <summary>
    /// 预测数据点所属的聚类。
    /// </summary>
    /// <param name="dataPoint">要预测的数据点。</param>
    /// <returns>数据点所属聚类的索引。</returns>
    /// <exception cref="ArgumentException">当 <paramref name="dataPoint"/> 的维度与模型不匹配时抛出。</exception>
    public int Predict(T[] dataPoint)
    {
        if (dataPoint == null || dataPoint.Length != _numDimensions)
            throw new ArgumentException("输入数据点的维度与模型不匹配。", nameof(dataPoint));

        T maxProb = T.NegativeInfinity;
        int bestCluster = 0;

        for (int j = 0; j < _numClusters; j++)
        {
            T prob = _weights[j] * MultivariateGaussian(dataPoint, _means[j], _covariances[j]);
            if (prob > maxProb)
            {
                maxProb = prob;
                bestCluster = j;
            }
        }

        return bestCluster;
    }

    /// <summary>
    /// 计算数据的对数似然值。
    /// </summary>
    /// <param name="data">输入数据。</param>
    /// <returns>对数似然值。</returns>
    private T CalculateLogLikelihood(List<T[]> data)
    {
        T likelihood = T.Zero;
        for (int i = 0; i < data.Count; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < _numClusters; j++)
            {
                sum += _weights[j] * MultivariateGaussian(data[i], _means[j], _covariances[j]);
            }
            likelihood += T.Log(sum);
        }
        return likelihood;
    }
}