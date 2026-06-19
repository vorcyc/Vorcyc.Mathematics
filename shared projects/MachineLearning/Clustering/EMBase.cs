namespace Vorcyc.Mathematics.MachineLearning;

using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics.LinearAlgebra;


/// <summary>
/// Represents the base class for the Expectation-Maximization (EM) algorithm.
/// </summary>
/// <typeparam name="T">The element type, which must implement the <see cref="IFloatingPointIeee754{T}"/> and <see cref="IFloatingPointConstants{T}"/> interfaces.</typeparam>
/// <remarks>
/// The Expectation-Maximization (EM) algorithm is an iterative method for finding maximum-likelihood or maximum a posteriori estimates of parameters in statistical models, where the model depends on unobserved latent variables.
/// 
/// The EM algorithm consists of two main steps:
/// 1. Expectation step (E-step): computes the expected value of the log-likelihood of the conditional distribution of the observed data under the current parameter estimates.
/// 2. Maximization step (M-step): finds the parameters that maximize the expected log-likelihood computed in the E-step.
/// 
/// This base class provides general EM-based functionality, such as parameter initialization, the E-step, and the M-step, supporting implementations such as Gaussian Mixture Models.
/// </remarks>
public abstract class EMBase<T> where T : unmanaged, IFloatingPointIeee754<T>, IFloatingPointConstants<T>
{
    protected readonly int _numClusters;      // Number of clusters
    protected int _numDimensions;             // Data dimension
    protected List<T[]> _data;                // Input data
    protected List<T[]> _means;               // Mean vectors
    protected List<Matrix<T>> _covariances;   // Covariance matrices
    protected List<T> _weights;               // Cluster weights
    protected List<T[]> _responsibilities;    // Responsibility matrix

    /// <summary>
    /// Execution policy honored by the EM loops. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EMBase{T}"/> class.
    /// </summary>
    /// <param name="numClusters">The number of clusters, which must be a positive integer.</param>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="numClusters"/> is less than or equal to 0.</exception>
    protected EMBase(int numClusters, ComputingContext? context = null)
    {
        if (numClusters <= 0)
            throw new ArgumentException("The number of clusters must be a positive integer.", nameof(numClusters));
        _numClusters = numClusters;
        Context = context;
    }

    /// <summary>
    /// Initializes the parameters of the EM algorithm.
    /// </summary>
    /// <param name="data">The data to fit, represented as a list of arrays.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="data"/> is empty or its dimension is invalid.</exception>
    protected void InitializeParameters(List<T[]> data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "The data cannot be null.");
        if (data.Count == 0)
            throw new ArgumentException("The data list cannot be empty.", nameof(data));
        if (data[0].Length == 0)
            throw new ArgumentException("The data dimension must be greater than 0.", nameof(data));

        _data = data;
        _numDimensions = data[0].Length;
        Random rand = new Random();

        _means = new List<T[]>(_numClusters);
        _covariances = new List<Matrix<T>>(_numClusters);
        _weights = new List<T>(_numClusters);
        _responsibilities = new List<T[]>(_numClusters);

        for (int i = 0; i < _numClusters; i++)
        {
            // Randomly select an initial mean
            _means.Add((T[])data[rand.Next(data.Count)].Clone());
            // Initialize the covariance matrix as the identity matrix
            _covariances.Add(CreateIdentityMatrix(_numDimensions));
            // Initialize the weight
            _weights.Add(T.One / T.CreateChecked(_numClusters));
            // Initialize the responsibility matrix
            _responsibilities.Add(new T[data.Count]);
        }
    }

    /// <summary>
    /// Performs the expectation step (E-step) of the EM algorithm.
    /// </summary>
    /// <param name="data">The data to fit, represented as a list of arrays.</param>
    protected void ExpectationStep(List<T[]> data)
    {
        ComputingContextExecution.ForEach(
            Context,
            0,
            data.Count,
            i =>
            {
                T sum = T.Zero;
                Span<T> probs = stackalloc T[_numClusters]; // Temporarily stores probabilities

                // Compute the responsibility value of each data point
                for (int j = 0; j < _numClusters; j++)
                {
                    probs[j] = _weights[j] * MultivariateGaussian(data[i], _means[j], _covariances[j]);
                    sum += probs[j];
                }

                // Normalize the responsibility values
                for (int j = 0; j < _numClusters; j++)
                {
                    _responsibilities[j][i] = sum != T.Zero ? probs[j] / sum : T.One / T.CreateChecked(_numClusters);
                }
            },
            workPerItem: (long)_numClusters * _numDimensions * _numDimensions);
    }

    /// <summary>
    /// Performs the maximization step (M-step) of the EM algorithm.
    /// </summary>
    /// <param name="data">The data to fit, represented as a list of arrays.</param>
    protected void MaximizationStep(List<T[]> data)
    {
        ComputingContextExecution.ForEach(
            Context,
            0,
            _numClusters,
            j =>
            {
                // Sum of responsibility values for the current cluster
                T responsibilitySum = T.Zero;
                Span<T> respSpan = _responsibilities[j].AsSpan();
                for (int i = 0; i < respSpan.Length; i++)
                    responsibilitySum += respSpan[i];

                // Update the weight
                _weights[j] = responsibilitySum / T.CreateChecked(data.Count);

                // Update the mean
                var newMeanElements = new T[_numDimensions];
                for (int i = 0; i < data.Count; i++)
                {
                    T resp = _responsibilities[j][i];
                    for (int d = 0; d < _numDimensions; d++)
                    {
                        newMeanElements[d] += resp * data[i][d];
                    }
                }
                for (int d = 0; d < _numDimensions; d++)
                {
                    newMeanElements[d] /= responsibilitySum;
                }
                _means[j] = newMeanElements;

                // Update the covariance matrix
                var newCovarianceElements = new T[_numDimensions, _numDimensions];
                for (int i = 0; i < data.Count; i++)
                {
                    T resp = _responsibilities[j][i];
                    ReadOnlySpan<T> diff = ComputeDifference(data[i], _means[j]);
                    for (int d1 = 0; d1 < _numDimensions; d1++)
                    {
                        for (int d2 = 0; d2 < _numDimensions; d2++)
                        {
                            newCovarianceElements[d1, d2] += resp * diff[d1] * diff[d2];
                        }
                    }
                }
                for (int d1 = 0; d1 < _numDimensions; d1++)
                {
                    for (int d2 = 0; d2 < _numDimensions; d2++)
                    {
                        newCovarianceElements[d1, d2] /= responsibilitySum;
                    }
                }
                // Add a small regularization term to ensure the covariance matrix is invertible
                for (int d = 0; d < _numDimensions; d++)
                {
                    newCovarianceElements[d, d] += T.CreateChecked(1e-6); // Regularization
                }
                _covariances[j] = new Matrix<T>(newCovarianceElements);
            },
            workPerItem: (long)data.Count * _numDimensions * _numDimensions);
    }

    /// <summary>
    /// Computes the probability density function value of the multivariate Gaussian distribution.
    /// </summary>
    /// <param name="x">The data point.</param>
    /// <param name="mean">The mean vector.</param>
    /// <param name="covariance">The covariance matrix.</param>
    /// <returns>The probability density function value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T MultivariateGaussian(ReadOnlySpan<T> x, ReadOnlySpan<T> mean, Matrix<T> covariance)
    {
        int d = x.Length;
        T det = covariance.Determinant();
        if (T.Abs(det) < T.CreateChecked(1e-10))
            return T.Zero; // Avoid division by zero

        ReadOnlySpan<T> diff = ComputeDifference(x, mean);
        Matrix<T> invCov = covariance.Inverse();
        T exponent = ComputeExponent(diff, invCov);

        T normalization = T.Sqrt(T.Pow(T.CreateChecked(2) * T.Pi, T.CreateChecked(d)) * det);
        return T.Exp(T.CreateChecked(-0.5) * exponent) / normalization;
    }

    /// <summary>
    /// Creates an identity matrix of the specified size.
    /// </summary>
    /// <param name="size">The size of the matrix.</param>
    /// <returns>The identity matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Matrix<T> CreateIdentityMatrix(int size)
    {
        return Matrix<T>.Eye(size); // Uses the optimized Matrix<T>.Eye
    }

    /// <summary>
    /// Computes the difference vector between a data point and the mean.
    /// </summary>
    /// <param name="x">The data point.</param>
    /// <param name="mean">The mean vector.</param>
    /// <returns>The difference vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T[] ComputeDifference(ReadOnlySpan<T> x, ReadOnlySpan<T> mean)
    {
        var diff = new T[_numDimensions];
        for (int i = 0; i < _numDimensions; i++)
        {
            diff[i] = x[i] - mean[i];
        }
        return diff;
    }

    /// <summary>
    /// Computes the exponent term of the Gaussian distribution.
    /// </summary>
    /// <param name="diff">The difference vector.</param>
    /// <param name="invCov">The inverse of the covariance matrix.</param>
    /// <returns>The exponent value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T ComputeExponent(ReadOnlySpan<T> diff, Matrix<T> invCov)
    {
        T exponent = T.Zero;
        for (int i = 0; i < _numDimensions; i++)
        {
            T temp = T.Zero;
            for (int j = 0; j < _numDimensions; j++)
            {
                temp += diff[i] * invCov[i, j] * diff[j];
            }
            exponent += temp;
        }
        return exponent;
    }
}