
//// 示例数据集 (每行是一个样本，每列是一个特征)
//double[,] data =
//{
//            { 2.5, 2.4 },
//            { 0.5, 0.7 },
//            { 2.2, 2.9 },
//            { 1.9, 2.2 },
//            { 3.1, 3.0 },
//            { 2.3, 2.7 },
//            { 2.0, 1.6 },
//            { 1.0, 1.1 },
//    { 1.5, 1.6 },
//            { 1.1, 0.9 }
//        };

//PCA pca = new PCA(data);

//// 获取主成分
//double[,] components = pca.Transform();

//// 输出主成分
//Console.WriteLine("主成分:");
//        for (int i = 0; i<components.GetLength(0); i++)
//        {
//            for (int j = 0; j<components.GetLength(1); j++)
//            {
//                Console.Write($"{components[i, j]:F2} ");
//            }
//            Console.WriteLine();
//        }

//        // 输出解释的方差比例
//        double[] explainedVarianceRatio = pca.GetExplainedVarianceRatio();
//Console.WriteLine("\n解释的方差比例:");
//foreach (var proportion in explainedVarianceRatio)
//{
//    Console.WriteLine($"{proportion:F2}");
//}

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;


/// <summary>
/// 主成分分析 (PCA) 类，用于降维和特征提取。
/// </summary>
public class PCA<TSelf> : IMachineLearning
    where TSelf : IFloatingPointIeee754<TSelf>
{
    private readonly TSelf[,] _data;
    private TSelf[] _means;
    private TSelf[,] _covarianceMatrix;
    private TSelf[] _eigenValues;
    private TSelf[][] _eigenVectors;

    public MachineLearningTask Task => MachineLearningTask.DimensionalityReduction;

    /// <summary>
    /// 初始化 PCA 类的新实例。
    /// </summary>
    /// <param name="data">输入数据集，每行是一个样本，每列是一个特征。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PCA(TSelf[,] data)
    {
        _data = data;
        ComputeMeans();
        CenterData();
        ComputeCovarianceMatrix();
        ComputeEigenDecomposition();
    }

    /// <summary>
    /// 计算每个特征的均值。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeMeans()
    {
        int numFeatures = _data.GetLength(1);
        _means = new TSelf[numFeatures];
        for (int i = 0; i < numFeatures; i++)
        {
            _means[i] = Enumerable.Range(0, _data.GetLength(0))
                                  .Select(j => _data[j, i])
                                  .Aggregate(TSelf.Zero, (acc, val) => acc + val) / TSelf.CreateChecked(_data.GetLength(0));
        }
    }

    /// <summary>
    /// 对数据进行中心化处理。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CenterData()
    {
        for (int i = 0; i < _data.GetLength(0); i++)
        {
            for (int j = 0; j < _data.GetLength(1); j++)
            {
                _data[i, j] -= _means[j];
            }
        }
    }

    /// <summary>
    /// 计算协方差矩阵。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeCovarianceMatrix()
    {
        int numFeatures = _data.GetLength(1);
        _covarianceMatrix = new TSelf[numFeatures, numFeatures];
        for (int i = 0; i < numFeatures; i++)
        {
            for (int j = 0; j < numFeatures; j++)
            {
                _covarianceMatrix[i, j] = Enumerable.Range(0, _data.GetLength(0))
                                                    .Select(k => _data[k, i] * _data[k, j])
                                                    .Aggregate(TSelf.Zero, (acc, val) => acc + val) / TSelf.CreateChecked(_data.GetLength(0) - 1);
            }
        }
    }

    /// <summary>
    /// 计算协方差矩阵的特征值和特征向量。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeEigenDecomposition()
    {
        int n = _covarianceMatrix.GetLength(0);
        _eigenValues = new TSelf[n];
        _eigenVectors = new TSelf[n][];

        // 使用简单的幂迭代法计算特征值和特征向量
        for (int i = 0; i < n; i++)
        {
            TSelf[] b = Enumerable.Repeat(TSelf.One, n).ToArray();
            TSelf[] bOld;
            TSelf lambda = TSelf.Zero;

            for (int iter = 0; iter < 1000; iter++)
            {
                bOld = b;
                b = MatrixVectorMultiply(_covarianceMatrix, b);
                lambda = b.Max();
                b = b.Select(x => x / lambda).ToArray();

                if (b.Zip(bOld, (x, y) => TSelf.Abs(x - y)).Aggregate(TSelf.Zero, (acc, val) => acc + val) < TSelf.CreateChecked(1e-10))
                    break;
            }

            _eigenValues[i] = lambda;
            _eigenVectors[i] = b;
        }

        // 按特征值降序排序特征向量
        var eigenPairs = _eigenValues.Zip(_eigenVectors, (value, vector) => new { Value = value, Vector = vector })
                                     .OrderByDescending(pair => pair.Value)
                                     .ToArray();

        _eigenValues = eigenPairs.Select(pair => pair.Value).ToArray();
        _eigenVectors = eigenPairs.Select(pair => pair.Vector).ToArray();
    }

    /// <summary>
    /// 矩阵和向量相乘。
    /// </summary>
    /// <param name="matrix">输入矩阵。</param>
    /// <param name="vector">输入向量。</param>
    /// <returns>结果向量。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TSelf[] MatrixVectorMultiply(TSelf[,] matrix, TSelf[] vector)
    {
        int n = matrix.GetLength(0);
        TSelf[] result = new TSelf[n];

        for (int i = 0; i < n; i++)
        {
            result[i] = TSelf.Zero;
            for (int j = 0; j < n; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }

        return result;
    }

    /// <summary>
    /// 将原始数据转换为主成分。
    /// </summary>
    /// <returns>转换后的主成分数据。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSelf[,] Transform()
    {
        int numSamples = _data.GetLength(0);
        int numFeatures = _data.GetLength(1);
        TSelf[,] components = new TSelf[numSamples, numFeatures];

        for (int i = 0; i < numSamples; i++)
        {
            for (int j = 0; j < numFeatures; j++)
            {
                components[i, j] = _eigenVectors[j].Select((v, k) => v * _data[i, k]).Aggregate(TSelf.Zero, (acc, val) => acc + val);
            }
        }

        return components;
    }

    /// <summary>
    /// 获取解释的方差比例。
    /// </summary>
    /// <returns>解释的方差比例数组。</returns>
    public TSelf[] GetExplainedVarianceRatio()
    {
        TSelf totalVariance = _eigenValues.Aggregate(TSelf.Zero, (acc, val) => acc + val);
        return _eigenValues.Select(value => value / totalVariance).ToArray();
    }
}