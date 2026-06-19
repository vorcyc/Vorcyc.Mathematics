using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;

/// <summary>
/// Factor analysis class used to perform factor analysis.
/// </summary>
/// <typeparam name="T">The numeric type, which must implement the INumber interface.</typeparam>
public class FactorAnalysis<T> :IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>
    /// The factor loading matrix.
    /// </summary>
    public Matrix<T> Loadings { get; private set; }

    /// <summary>
    /// The communalities array.
    /// </summary>
    public T[] Communalities { get; private set; }

    /// <summary>
    /// The specific variances array.
    /// </summary>
    public T[] SpecificVariances { get; private set; }


    public MachineLearningTask Task => MachineLearningTask.DimensionalityReduction;

    /// <summary>
    /// Execution policy honored by this estimator. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FactorAnalysis{T}"/> class.
    /// </summary>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    public FactorAnalysis(ComputingContext? context = null)
    {
        Context = context;
    }

    /// <summary>
    /// Performs factor analysis.
    /// </summary>
    /// <param name="data">The data matrix, where each row represents a sample and each column represents a variable.</param>
    /// <param name="numFactors">The number of factors.</param>
    public void Analyze(Matrix<T> data, int numFactors)
    {
        // Standardize the data
        var standardizedData = StandardizeData(data);

        // Compute the covariance matrix
        var covarianceMatrix = CalculateCovarianceMatrix(standardizedData);

        // Compute eigenvalues and eigenvectors
        var eig = MatrixDecomposition.SymmetricEigendecomposition(covarianceMatrix);
        var eigenvalues = eig.Eigenvalues;
        var eigenvectors = eig.Eigenvectors;

        // Select the top numFactors eigenvectors
        var selectedEigenvectors = SelectTopEigenvectors(eigenvectors, numFactors);

        // Compute the factor loading matrix
        Loadings = CalculateLoadings(selectedEigenvectors, eigenvalues, numFactors);

        // Compute communalities and specific variances
        Communalities = CalculateCommunalities(Loadings);
        SpecificVariances = CalculateSpecificVariances(Communalities, data.Columns);
    }

    /// <summary>
    /// Standardizes the data matrix.
    /// </summary>
    /// <param name="data">The data matrix.</param>
    /// <returns>The standardized data matrix.</returns>
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
    /// Computes the covariance matrix.
    /// </summary>
    /// <param name="data">The standardized data matrix.</param>
    /// <returns>The covariance matrix.</returns>
    private Matrix<T> CalculateCovarianceMatrix(Matrix<T> data)
    {
        int rows = data.Rows;
        int cols = data.Columns;
        Matrix<T> covarianceMatrix = new(cols, cols);

        ComputingContextExecution.ForEach(
            Context,
            0,
            cols,
            i =>
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
            },
            workPerItem: (long)cols * rows);

        return covarianceMatrix;
    }

    /// <summary>
    /// Selects the top numFactors eigenvectors.
    /// </summary>
    /// <param name="eigenvectors">The eigenvector matrix.</param>
    /// <param name="numFactors">The number of factors.</param>
    /// <returns>The selected eigenvector matrix.</returns>
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
    /// Computes the factor loading matrix.
    /// </summary>
    /// <param name="eigenvectors">The eigenvector matrix.</param>
    /// <param name="eigenvalues">The eigenvalues array.</param>
    /// <param name="numFactors">The number of factors.</param>
    /// <returns>The factor loading matrix.</returns>
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
    /// Computes the communalities array.
    /// </summary>
    /// <param name="loadings">The factor loading matrix.</param>
    /// <returns>The communalities array.</returns>
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
    /// Computes the specific variances array.
    /// </summary>
    /// <param name="communalities">The communalities array.</param>
    /// <param name="numVariables">The number of variables.</param>
    /// <returns>The specific variances array.</returns>
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
