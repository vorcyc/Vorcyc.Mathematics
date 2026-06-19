using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// Represents an implementation of the Gaussian Mixture Model (GMM).
/// </summary>
/// <typeparam name="T">The element type, which must implement the <see cref="IFloatingPointIeee754{T}"/>, <see cref="IFloatingPointConstants{T}"/>, and <see cref="IMinMaxValue{T}"/> interfaces.</typeparam>
/// <remarks>
/// The Gaussian Mixture Model (GMM) is a probabilistic model used to represent a mixture distribution composed of multiple Gaussian distributions. It is mainly used for cluster analysis and density estimation.
/// 
/// The GMM algorithm consists of two main steps:
/// 1. Expectation step (E-step): computes the responsibility value of each data point for each Gaussian distribution.
/// 2. Maximization step (M-step): updates the model parameters (means, covariance matrices, and weights) based on the responsibility values.
/// 
/// This implementation assumes that the data points follow a multivariate Gaussian distribution and uses the EM algorithm for parameter estimation. The optimized version adds convergence checks, prediction functionality, and performance improvements.
/// </remarks>
public class GMM<T> : EMBase<T>, IMachineLearning
    where T : unmanaged, IFloatingPointIeee754<T>, IFloatingPointConstants<T>, IMinMaxValue<T>
{
    private readonly int _maxIterations; // Maximum number of iterations
    private readonly T _tolerance;       // Convergence tolerance

    /// <summary>
    /// Initializes a new instance of the <see cref="GMM{T}"/> class.
    /// </summary>
    /// <param name="numComponents">The number of Gaussian distributions, which must be a positive integer.</param>
    /// <param name="maxIterations">The maximum number of iterations; the default is 100.</param>
    /// <param name="tolerance">The convergence tolerance for the log-likelihood function; the default is 1e-6.</param>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="numComponents"/> or <paramref name="maxIterations"/> is less than or equal to 0.</exception>
    public GMM(int numComponents, int maxIterations = 100, T tolerance = default, ComputingContext? context = null)
        : base(numComponents, context)
    {
        if (maxIterations <= 0)
            throw new ArgumentException("The maximum number of iterations must be a positive integer.", nameof(maxIterations));

        _maxIterations = maxIterations;
        _tolerance = tolerance == default ? T.CreateChecked(1e-6) : tolerance;
    }

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
    /// Fits the data using the Gaussian Mixture Model.
    /// </summary>
    /// <param name="data">The data to fit, represented as a list of arrays.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="data"/> is empty or its dimension is invalid.</exception>
    public void Fit(List<T[]> data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "The data cannot be null.");
        if (data.Count == 0)
            throw new ArgumentException("The data list cannot be empty.", nameof(data));
        if (data[0].Length == 0)
            throw new ArgumentException("The data dimension must be greater than 0.", nameof(data));

        _data = data;
        _numDimensions = data[0].Length;
        InitializeParameters(data);

        T logLikelihood = T.MinValue;
        for (int iter = 0; iter < _maxIterations; iter++)
        {
            ExpectationStep(data);
            MaximizationStep(data);

            T newLogLikelihood = ComputeLogLikelihood(data);
            if (iter > 0 && T.Abs(newLogLikelihood - logLikelihood) < _tolerance)
                break;

            logLikelihood = newLogLikelihood;
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
    /// Computes the log-likelihood function value.
    /// </summary>
    /// <param name="data">The data to fit, represented as a list of arrays.</param>
    /// <returns>The log-likelihood function value.</returns>
    private T ComputeLogLikelihood(List<T[]> data)
    {
        T logLikelihood = T.Zero;
        for (int i = 0; i < data.Count; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < _numClusters; j++)
            {
                sum += _weights[j] * MultivariateGaussian(data[i], _means[j], _covariances[j]);
            }
            logLikelihood += T.Log(sum > T.Zero ? sum : T.CreateChecked(1e-10)); // Avoid log(0)
        }
        return logLikelihood;
    }
}