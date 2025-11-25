using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;

/// <summary>
/// 因子分析类，用于执行因子分析。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
public class FactorAnalysis<T> :IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>
    /// 因子载荷矩阵。
    /// </summary>
    public Matrix<T> Loadings { get; private set; }

    /// <summary>
    /// 共同性数组。
    /// </summary>
    public T[] Communalities { get; private set; }

    /// <summary>
    /// 特异性方差数组。
    /// </summary>
    public T[] SpecificVariances { get; private set; }


    public MachineLearningTask Task => MachineLearningTask.DimensionalityReduction;

    /// <summary>
    /// 执行因子分析。
    /// </summary>
    /// <param name="data">数据矩阵，每行代表一个样本，每列代表一个变量。</param>
    /// <param name="numFactors">因子数量。</param>
    public void Analyze(Matrix<T> data, int numFactors)
    {
        // 标准化数据
        var standardizedData = StandardizeData(data);

        // 计算协方差矩阵
        var covarianceMatrix = CalculateCovarianceMatrix(standardizedData);

        // 计算特征值和特征向量
        var (eigenvalues, eigenvectors) = EigenDecomposition(covarianceMatrix);

        // 选择前 numFactors 个特征向量
        var selectedEigenvectors = SelectTopEigenvectors(eigenvectors, numFactors);

        // 计算因子载荷矩阵
        Loadings = CalculateLoadings(selectedEigenvectors, eigenvalues, numFactors);

        // 计算共同性和特异性方差
        Communalities = CalculateCommunalities(Loadings);
        SpecificVariances = CalculateSpecificVariances(Communalities, data.Columns);
    }

    /// <summary>
    /// 标准化数据矩阵。
    /// </summary>
    /// <param name="data">数据矩阵。</param>
    /// <returns>标准化后的数据矩阵。</returns>
    private Matrix<T> StandardizeData(Matrix<T> data)
    {
        int rows = data.Rows;
        int cols = data.Columns;
        Matrix<T> standardizedData = new(rows, cols);

        for (int j = 0; j < cols; j++)
        {
            T mean = T.Zero;
            T stdDev = T.Zero;

            for (int i = 0; i < rows; i++)
            {
                mean += data[i, j];
            }
            mean /= T.CreateChecked(rows);

            for (int i = 0; i < rows; i++)
            {
                stdDev += (data[i, j] - mean) * (data[i, j] - mean);
            }
            stdDev = T.Sqrt(stdDev / T.CreateChecked(rows));

            for (int i = 0; i < rows; i++)
            {
                standardizedData[i, j] = (data[i, j] - mean) / stdDev;
            }
        }

        return standardizedData;
    }

    /// <summary>
    /// 计算协方差矩阵。
    /// </summary>
    /// <param name="data">标准化后的数据矩阵。</param>
    /// <returns>协方差矩阵。</returns>
    private Matrix<T> CalculateCovarianceMatrix(Matrix<T> data)
    {
        int rows = data.Rows;
        int cols = data.Columns;
        Matrix<T> covarianceMatrix = new(cols, cols);

        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                T covariance = T.Zero;
                for (int k = 0; k < rows; k++)
                {
                    covariance += data[k, i] * data[k, j];
                }
                covarianceMatrix[i, j] = covariance / T.CreateChecked(rows - 1);
            }
        }

        return covarianceMatrix;
    }

    /// <summary>
    /// 计算特征值和特征向量。
    /// </summary>
    /// <param name="matrix">协方差矩阵。</param>
    /// <returns>特征值和特征向量的元组。</returns>
    private (T[], Matrix<T>) EigenDecomposition(Matrix<T> matrix)
    {
        // 这里我们使用一个简单的特征值分解算法（如幂迭代法），但在实际应用中，建议使用更高效和稳定的算法。
        int n = matrix.Rows;
        T[] eigenvalues = new T[n];
        Matrix<T> eigenvectors = new(n, n);

        // 初始化特征向量为单位矩阵
        for (int i = 0; i < n; i++)
        {
            eigenvectors[i, i] = T.One;
        }

        // 简单的幂迭代法
        for (int k = 0; k < n; k++)
        {
            T[] b = new T[n];
            b[k] = T.One;
            T[] bNext = new T[n];
            T lambda = T.Zero;

            for (int iter = 0; iter < 100; iter++)
            {
                // 计算 bNext = matrix * b
                for (int i = 0; i < n; i++)
                {
                    bNext[i] = T.Zero;
                    for (int j = 0; j < n; j++)
                    {
                        bNext[i] += matrix[i, j] * b[j];
                    }
                }

                // 归一化 bNext
                T norm = T.Zero;
                for (int i = 0; i < n; i++)
                {
                    norm += bNext[i] * bNext[i];
                }
                norm = T.Sqrt(norm);
                for (int i = 0; i < n; i++)
                {
                    bNext[i] /= norm;
                }

                // 计算特征值
                lambda = T.Zero;
                for (int i = 0; i < n; i++)
                {
                    lambda += bNext[i] * b[i];
                }

                // 更新 b
                Array.Copy(bNext, b, n);
            }

            eigenvalues[k] = lambda;
            for (int i = 0; i < n; i++)
            {
                eigenvectors[i, k] = b[i];
            }
        }

        return (eigenvalues, eigenvectors);
    }

    /// <summary>
    /// 选择前 numFactors 个特征向量。
    /// </summary>
    /// <param name="eigenvectors">特征向量矩阵。</param>
    /// <param name="numFactors">因子数量。</param>
    /// <returns>选择的特征向量矩阵。</returns>
    private Matrix<T> SelectTopEigenvectors(Matrix<T> eigenvectors, int numFactors)
    {
        int rows = eigenvectors.Rows;
        Matrix<T> selectedEigenvectors = new(rows, numFactors);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < numFactors; j++)
            {
                selectedEigenvectors[i, j] = eigenvectors[i, j];
            }
        }

        return selectedEigenvectors;
    }

    /// <summary>
    /// 计算因子载荷矩阵。
    /// </summary>
    /// <param name="eigenvectors">特征向量矩阵。</param>
    /// <param name="eigenvalues">特征值数组。</param>
    /// <param name="numFactors">因子数量。</param>
    /// <returns>因子载荷矩阵。</returns>
    private Matrix<T> CalculateLoadings(Matrix<T> eigenvectors, T[] eigenvalues, int numFactors)
    {
        int rows = eigenvectors.Rows;
        Matrix<T> loadings = new(rows, numFactors);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < numFactors; j++)
            {
                loadings[i, j] = eigenvectors[i, j] * T.Sqrt(eigenvalues[j]);
            }
        }

        return loadings;
    }

    /// <summary>
    /// 计算共同性数组。
    /// </summary>
    /// <param name="loadings">因子载荷矩阵。</param>
    /// <returns>共同性数组。</returns>
    private T[] CalculateCommunalities(Matrix<T> loadings)
    {
        int rows = loadings.Rows;
        int cols = loadings.Columns;
        T[] communalities = new T[rows];

        for (int i = 0; i < rows; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < cols; j++)
            {
                sum += loadings[i, j] * loadings[i, j];
            }
            communalities[i] = sum;
        }

        return communalities;
    }

    /// <summary>
    /// 计算特异性方差数组。
    /// </summary>
    /// <param name="communalities">共同性数组。</param>
    /// <param name="numVariables">变量数量。</param>
    /// <returns>特异性方差数组。</returns>
    private T[] CalculateSpecificVariances(T[] communalities, int numVariables)
    {
        T[] specificVariances = new T[numVariables];

        for (int i = 0; i < numVariables; i++)
        {
            specificVariances[i] = T.One - communalities[i];
        }

        return specificVariances;
    }
}
