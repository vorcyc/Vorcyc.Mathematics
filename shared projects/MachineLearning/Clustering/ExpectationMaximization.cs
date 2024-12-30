

using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// 表示期望最大化算法的实现。
/// </summary>
/// <typeparam name="T">元素类型，必须实现 <see cref="System.Numerics.IFloatingPointIeee754{T}"/> 和 <see cref="System.Numerics.IFloatingPointConstants{T}"/> 接口。</typeparam>
/// <remarks>
/// 期望最大化（EM）算法是一种迭代算法，用于在存在隐变量的情况下寻找参数的最大似然估计或最大后验估计。它主要用于聚类分析和密度估计。
/// 
/// EM算法包括两个主要步骤：
/// 1. 期望步骤（E步）：计算每个数据点属于每个聚类的责任值。
/// 2. 最大化步骤（M步）：根据责任值更新模型参数（均值、协方差矩阵和权重）。
/// 
/// 该实现假设数据点服从多元高斯分布，并使用高斯混合模型（GMM）进行聚类。
/// </remarks>
public class ExpectationMaximization<T> : EMBase<T> , IMachineLearning
    where T : struct, System.Numerics.IFloatingPointIeee754<T>, System.Numerics.IFloatingPointConstants<T>
{
    /// <summary>
    /// 初始化 <see cref="ExpectationMaximization{T}"/> 类的新实例。
    /// </summary>
    /// <param name="numClusters">聚类的数量。</param>
    public ExpectationMaximization(int numClusters) : base(numClusters) { }

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
    /// 使用期望最大化算法拟合数据。
    /// </summary>
    /// <param name="data">要拟合的数据，表示为向量的列表。</param>
    public void Fit(List<Vector<T>> data)
    {
        _data = data;
        _numDimensions = data[0].Dimension;
        InitializeParameters(data);

        for (int i = 0; i < 100; i++) // 迭代次数可以根据需要调整
        {
            ExpectationStep(data);
            MaximizationStep(data);
        }
    }
}
