using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// Represents an implementation of the Expectation-Maximization algorithm.
/// </summary>
/// <typeparam name="T">The element type, which must implement the <see cref="IFloatingPointIeee754{T}"/> and <see cref="IFloatingPointConstants{T}"/> interfaces.</typeparam>
/// <remarks>
/// The Expectation-Maximization (EM) algorithm is an iterative algorithm for finding maximum-likelihood or maximum a posteriori estimates of parameters in the presence of latent variables. It is mainly used for cluster analysis and density estimation.
/// 
/// The EM algorithm consists of two main steps:
/// 1. Expectation step (E-step): computes the responsibility value of each data point for each cluster.
/// 2. Maximization step (M-step): updates the model parameters (means, covariance matrices, and weights) based on the responsibility values.
/// 
/// This implementation assumes that the data points follow a multivariate Gaussian distribution and uses a Gaussian Mixture Model (GMM) for clustering. The optimized version adds convergence checks and performance improvements.
/// </remarks>
public class ExpectationMaximization<T> : EMBase<T>, IMachineLearning
    where T : unmanaged, IFloatingPointIeee754<T>, IFloatingPointConstants<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpectationMaximization{T}"/> class.
    /// </summary>
    /// <param name="numClusters">The number of clusters, which must be a positive integer.</param>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    public ExpectationMaximization(int numClusters, ComputingContext? context = null) : base(numClusters, context) { }

    /// <summary>
    /// Gets the cluster centers (means).
    /// </summary>
    public IReadOnlyList<T[]> Means => _means;

    /// <summary>
    /// Gets the covariance matrices.
    /// </summary>
    public IReadOnlyList<Matrix<T>> Covariances => _covariances;

    /// <summary>
    /// Gets the weights.
    /// </summary>
    public IReadOnlyList<T> Weights => _weights;

    /// <summary>
    /// Gets the machine learning task type.
    /// </summary>
    public MachineLearningTask Task => MachineLearningTask.Clustering;

    /// <summary>
    /// Fits the data using the Expectation-Maximization algorithm.
    /// </summary>
    /// <param name="data">The data to fit, represented as a list of arrays.</param>
    /// <param name="maxIterations">The maximum number of iterations; the default is 100.</param>
    /// <param name="tolerance">The convergence tolerance; the default is 1e-4.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="data"/> is empty or its dimension is invalid.</exception>
    public void Fit(List<T[]> data, int maxIterations = 100, T tolerance = default)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "The data cannot be null.");
        if (data.Count == 0)
            throw new ArgumentException("The data list cannot be empty.", nameof(data));
        if (data[0].Length == 0)
            throw new ArgumentException("The data dimension must be greater than 0.", nameof(data));

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
    /// Predicts the cluster to which a data point belongs.
    /// </summary>
    /// <param name="dataPoint">The data point to predict.</param>
    /// <returns>The index of the cluster to which the data point belongs.</returns>
    /// <exception cref="ArgumentException">Thrown when the dimension of <paramref name="dataPoint"/> does not match the model.</exception>
    public int Predict(T[] dataPoint)
    {
        if (dataPoint == null || dataPoint.Length != _numDimensions)
            throw new ArgumentException("The dimension of the input data point does not match the model.", nameof(dataPoint));

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
    /// Computes the log-likelihood of the data.
    /// </summary>
    /// <param name="data">The input data.</param>
    /// <returns>The log-likelihood value.</returns>
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