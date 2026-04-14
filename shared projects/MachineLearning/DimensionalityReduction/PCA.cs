

using System.Numerics;

using System.Runtime.CompilerServices;

using Vorcyc.Mathematics.LinearAlgebra;



namespace Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;





/// <summary>

/// 主成分分析 (PCA) 类，用于降维和特征提取。

/// </summary>

public class PCA<TSelf> : IMachineLearning

    where TSelf : struct, IFloatingPointIeee754<TSelf>

{

    private readonly Matrix<TSelf> _data;

    private TSelf[] _means;

    private Matrix<TSelf> _covarianceMatrix;

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

        _data = new Matrix<TSelf>(data);

        ComputeMeans();

        CenterData();

        ComputeCovarianceMatrix();

        ComputeEigenDecomposition();

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    private void ComputeMeans()

    {

        int numFeatures = _data.Columns;

        int numSamples = _data.Rows;

        _means = new TSelf[numFeatures];

        for (int j = 0; j < numFeatures; j++)

        {

            TSelf sum = TSelf.Zero;

            for (int i = 0; i < numSamples; i++)

                sum += _data[i, j];

            _means[j] = sum / TSelf.CreateChecked(numSamples);

        }

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    private void CenterData()

    {

        for (int i = 0; i < _data.Rows; i++)

        {

            for (int j = 0; j < _data.Columns; j++)

                _data[i, j] -= _means[j];

        }

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    private void ComputeCovarianceMatrix()

    {

        int numFeatures = _data.Columns;

        int numSamples = _data.Rows;

        TSelf scale = TSelf.One / TSelf.CreateChecked(numSamples - 1);

        _covarianceMatrix = new Matrix<TSelf>(numFeatures, numFeatures);



        for (int i = 0; i < numFeatures; i++)

        {

            for (int j = 0; j < numFeatures; j++)

            {

                TSelf sum = TSelf.Zero;

                for (int k = 0; k < numSamples; k++)

                    sum += _data[k, i] * _data[k, j];

                _covarianceMatrix[i, j] = sum * scale;

            }

        }

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    private void ComputeEigenDecomposition()

    {

        var result = MatrixDecomposition.SymmetricEigendecomposition(_covarianceMatrix);

        _eigenValues = result.Eigenvalues;

        int n = _eigenValues.Length;

        _eigenVectors = new TSelf[n][];

        for (int j = 0; j < n; j++)

            _eigenVectors[j] = result.Eigenvectors.GetColumn(j);

    }



    /// <summary>

    /// 将原始数据转换为主成分。

    /// </summary>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public TSelf[,] Transform()

    {

        int numSamples = _data.Rows;

        int numFeatures = _data.Columns;

        TSelf[,] components = new TSelf[numSamples, numFeatures];

        var row = new TSelf[numFeatures];



        for (int i = 0; i < numSamples; i++)

        {

            _data.GetRow(i).CopyTo(row);

            for (int j = 0; j < numFeatures; j++)

                components[i, j] = VectorSpan.Dot(_eigenVectors[j], row);

        }



        return components;

    }



    /// <summary>

    /// 获取解释的方差比例。

    /// </summary>

    public TSelf[] GetExplainedVarianceRatio()

    {

        TSelf totalVariance = VectorSpan.Sum(_eigenValues);

        var ratios = new TSelf[_eigenValues.Length];

        for (int i = 0; i < ratios.Length; i++)

            ratios[i] = _eigenValues[i] / totalVariance;

        return ratios;

    }

}


