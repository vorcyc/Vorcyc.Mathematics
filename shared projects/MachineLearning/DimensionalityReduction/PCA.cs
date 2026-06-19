using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;

/// <summary>
/// Principal Component Analysis (PCA) class for dimensionality reduction and feature extraction.
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
    /// Execution policy honored by this estimator. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Initializes a new instance of the PCA class.
    /// </summary>
    /// <param name="data">The input data set; each row is a sample and each column is a feature.</param>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    public PCA(TSelf[,] data, ComputingContext? context = null)
    {
        Context = context;
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

        ComputingContextExecution.ForEach(
            Context,
            0,
            numFeatures,
            i =>
            {
                for (int j = 0; j < numFeatures; j++)
                {
                    TSelf sum = TSelf.Zero;
                    for (int k = 0; k < numSamples; k++)
                        sum += _data[k, i] * _data[k, j];
                    _covarianceMatrix[i, j] = sum * scale;
                }
            },
            workPerItem: (long)numFeatures * numSamples);
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
    /// Transforms the original data into principal components.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSelf[,] Transform()
    {
        int numSamples = _data.Rows;
        int numFeatures = _data.Columns;
        TSelf[,] components = new TSelf[numSamples, numFeatures];

        ComputingContextExecution.ForEach(
            Context,
            0,
            numSamples,
            i =>
            {
                var row = new TSelf[numFeatures];
                _data.GetRow(i).CopyTo(row);
                for (int j = 0; j < numFeatures; j++)
                    components[i, j] = VectorSpan.Dot(_eigenVectors[j], row);
            },
            workPerItem: (long)numFeatures * numFeatures);
        return components;
    }

    /// <summary>
    /// Gets the explained variance ratios.
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


