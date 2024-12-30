using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// 表示高斯混合模型（GMM）的实现。
/// </summary>
/// <typeparam name="T">元素类型，必须实现 <see cref="System.Numerics.IFloatingPointIeee754{T}"/>、<see cref="System.Numerics.IFloatingPointConstants{T}"/> 和 <see cref="System.Numerics.IMinMaxValue{T}"/> 接口。</typeparam>
/// <remarks>
/// 高斯混合模型（GMM）是一种概率模型，用于表示具有多个高斯分布的混合分布。它主要用于聚类分析和密度估计。
/// 
/// GMM算法包括两个主要步骤：
/// 1. 期望步骤（E步）：计算每个数据点属于每个高斯分布的责任值。
/// 2. 最大化步骤（M步）：根据责任值更新模型参数（均值、协方差矩阵和权重）。
/// 
/// 该实现假设数据点服从多元高斯分布，并使用EM算法进行参数估计。
/// </remarks>
public class GMM<T> : EMBase<T>, IMachineLearning
    where T : struct, System.Numerics.IFloatingPointIeee754<T>, System.Numerics.IFloatingPointConstants<T>, System.Numerics.IMinMaxValue<T>
{
    private int _maxIterations;
    private double _tolerance;

    /// <summary>
    /// 初始化 <see cref="GMM{T}"/> 类的新实例。
    /// </summary>
    /// <param name="numComponents">高斯分布的数量。</param>
    /// <param name="maxIterations">最大迭代次数。</param>
    /// <param name="tolerance">对数似然函数的收敛容差。</param>
    public GMM(int numComponents, int maxIterations = 100, double tolerance = 1e-6)
        : base(numComponents)
    {
        _maxIterations = maxIterations;
        _tolerance = tolerance;
    }

    /// <summary>
    /// 获取聚类中心（均值）。
    /// </summary>
    public IReadOnlyList<Vector<T>> Means => _means;

    /// <summary>
    /// 获取协方差矩阵。
    /// </summary>
    public IReadOnlyList<Matrix<T>> Covariances => _covariances;

    /// <summary>
    /// 获取权重。
    /// </summary>
    public IReadOnlyList<T> Weights => _weights;


    public MachineLearningTask Task => MachineLearningTask.Clustering;

    /// <summary>
    /// 使用高斯混合模型拟合数据。
    /// </summary>
    /// <param name="data">要拟合的数据，表示为向量的列表。</param>
    public void Fit(List<Vector<T>> data)
    {
        _data = data;
        _numDimensions = data[0].Dimension;
        InitializeParameters(data);

        T logLikelihood = T.MinValue;
        for (int iter = 0; iter < _maxIterations; iter++)
        {
            // 期望步骤
            ExpectationStep(data);
            // 最大化步骤
            MaximizationStep(data);

            // 检查收敛性
            T newLogLikelihood = ComputeLogLikelihood(data);
            if (T.Abs(newLogLikelihood - logLikelihood) < T.CreateChecked(_tolerance))
            {
                break;
            }
            logLikelihood = newLogLikelihood;
        }
    }

    /// <summary>
    /// 计算对数似然函数值。
    /// </summary>
    /// <param name="data">要拟合的数据，表示为向量的列表。</param>
    /// <returns>对数似然函数值。</returns>
    private T ComputeLogLikelihood(List<Vector<T>> data)
    {
        int n = data.Count;
        T logLikelihood = T.Zero;

        for (int i = 0; i < n; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < _numClusters; j++)
            {
                sum += _weights[j] * MultivariateGaussian(data[i], _means[j], _covariances[j]);
            }
            logLikelihood += T.Log(sum);
        }

        return logLikelihood;
    }
}
