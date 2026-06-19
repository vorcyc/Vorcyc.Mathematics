using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;

/// <summary>
/// t-SNE algorithm class, used for dimensionality reduction and data visualization.
/// </summary>
/// <typeparam name="T">The numeric type, which must implement the IFloatingPointIeee754 interface.</typeparam>
public class TSNE<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly int _perplexity;
    private readonly int _maxIter;
    private readonly T _learningRate;
    private readonly Action<int, T>? _progressCallback;

    /// <summary>
    /// Execution policy honored by this estimator. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Constructs a t-SNE algorithm instance.
    /// </summary>
    /// <param name="perplexity">The perplexity parameter.</param>
    /// <param name="maxIter">The maximum number of iterations.</param>
    /// <param name="learningRate">The learning rate.</param>
    /// <param name="progressCallback">An optional progress callback (iteration, cost).</param>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    public TSNE(int perplexity = 30, int maxIter = 1000, T learningRate = default, Action<int, T>? progressCallback = null, ComputingContext? context = null)
    {
        _perplexity = perplexity;
        _maxIter = maxIter;
        _learningRate = learningRate.Equals(default) ? T.CreateChecked(200.0) : learningRate;
        _progressCallback = progressCallback;
        Context = context;
    }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.DimensionalityReduction;

    /// <summary>
    /// Runs the t-SNE algorithm and returns the dimensionality-reduced matrix.
    /// </summary>
    /// <param name="data">The input high-dimensional data matrix.</param>
    /// <returns>The dimensionality-reduced matrix.</returns>
    public Matrix<T> FitTransform(Matrix<T> data)
    {
        int n = data.Rows;
        int d = data.Columns;
        int outputDims = 2;

        // Step 1: compute the pairwise affinities in the high-dimensional space
        Matrix<T> P = ComputePairwiseAffinities(data, _perplexity);

        // Step 2: randomly initialize the low-dimensional space
        Random rand = new Random();
        Matrix<T> Y = new Matrix<T>(n, outputDims);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < outputDims; j++)
            {
                Y[i, j] = T.CreateChecked(rand.NextDouble() * 1e-4);
            }
        }

        // Step 3: gradient descent
        for (int iter = 0; iter < _maxIter; iter++)
        {
            Matrix<T> Q = ComputeLowDimensionalAffinities(Y);
            Matrix<T> grads = ComputeGradients(P, Q, Y);

            // Update Y
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < outputDims; j++)
                {
                    Y[i, j] -= _learningRate * grads[i, j];
                }
            }

            if (iter % 100 == 0)
                _progressCallback?.Invoke(iter, ComputeCost(P, Q));
        }

        return Y;
    }

    /// <summary>
    /// Computes the pairwise affinities in the high-dimensional space.
    /// </summary>
    /// <param name="data">The input high-dimensional data matrix.</param>
    /// <param name="perplexity">The perplexity parameter.</param>
    /// <returns>The pairwise affinity matrix.</returns>
    private Matrix<T> ComputePairwiseAffinities(Matrix<T> data, int perplexity)
    {
        int n = data.Rows;
        Matrix<T> P = new Matrix<T>(n, n);

        ComputingContextExecution.ForEach(
            Context,
            0,
            n,
            i =>
            {
                T[] distances = new T[n];
                for (int j = 0; j < n; j++)
                {
                    distances[j] = EuclideanDistance(data, i, j);
                }

                T[] affinities = ComputeAffinities(distances, perplexity);
                for (int j = 0; j < n; j++)
                {
                    P[i, j] = affinities[j];
                }
            },
            workPerItem: (long)n * data.Columns);

        return P;
    }

    /// <summary>
    /// Computes the affinities for pairwise distances.
    /// </summary>
    /// <param name="distances">The pairwise distance array.</param>
    /// <param name="perplexity">The perplexity parameter.</param>
    /// <returns>The affinity array.</returns>
    private T[] ComputeAffinities(T[] distances, int perplexity)
    {
        int n = distances.Length;
        T[] affinities = new T[n];
        T beta = T.One;
        T logPerplexity = T.CreateChecked(Math.Log(perplexity));

        for (int i = 0; i < 50; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < n; j++)
            {
                affinities[j] = T.Exp(-distances[j] * beta);
                sum += affinities[j];
            }

            T entropy = T.Zero;
            for (int j = 0; j < n; j++)
            {
                affinities[j] /= sum;
                entropy -= affinities[j] * T.Log(affinities[j]);
            }

            if (T.Abs(entropy - logPerplexity) < T.CreateChecked(1e-5))
            {
                break;
            }

            if (entropy > logPerplexity)
            {
                beta *= T.CreateChecked(1.1);
            }
            else
            {
                beta /= T.CreateChecked(1.1);
            }
        }

        return affinities;
    }

    /// <summary>
    /// Computes the Euclidean distance between two data points.
    /// </summary>
    /// <param name="data">The data matrix.</param>
    /// <param name="i">The index of the first data point.</param>
    /// <param name="j">The index of the second data point.</param>
    /// <returns>The Euclidean distance.</returns>
    private T EuclideanDistance(Matrix<T> data, int i, int j)
    {
        T sum = T.Zero;
        for (int k = 0; k < data.Columns; k++)
        {
            T diff = data[i, k] - data[j, k];
            sum += diff * diff;
        }
        return T.Sqrt(sum);
    }

    /// <summary>
    /// Computes the pairwise affinities in the low-dimensional space.
    /// </summary>
    /// <param name="Y">The low-dimensional data matrix.</param>
    /// <returns>The pairwise affinity matrix.</returns>
    private Matrix<T> ComputeLowDimensionalAffinities(Matrix<T> Y)
    {
        int n = Y.Rows;
        Matrix<T> Q = new Matrix<T>(n, n);
        T sum = T.Zero;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i != j)
                {
                    T dist = EuclideanDistance(Y, i, j);
                    Q[i, j] = T.One / (T.One + dist * dist);
                    sum += Q[i, j];
                }
            }
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Q[i, j] /= sum;
            }
        }

        return Q;
    }

    /// <summary>
    /// Computes the gradients.
    /// </summary>
    /// <param name="P">The pairwise affinity matrix in the high-dimensional space.</param>
    /// <param name="Q">The pairwise affinity matrix in the low-dimensional space.</param>
    /// <param name="Y">The low-dimensional data matrix.</param>
    /// <returns>The gradient matrix.</returns>
    private Matrix<T> ComputeGradients(Matrix<T> P, Matrix<T> Q, Matrix<T> Y)
    {
        int n = Y.Rows;
        int d = Y.Columns;
        Matrix<T> grads = new Matrix<T>(n, d);

        ComputingContextExecution.ForEach(
            Context,
            0,
            n,
            i =>
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        T dist = EuclideanDistance(Y, i, j);
                        T coeff = T.CreateChecked(4) * (P[i, j] - Q[i, j]) * Q[i, j] / (T.One + dist * dist);
                        for (int k = 0; k < d; k++)
                        {
                            grads[i, k] += coeff * (Y[i, k] - Y[j, k]);
                        }
                    }
                }
            },
            workPerItem: (long)n * d);

        return grads;
    }

    /// <summary>
    /// Computes the cost function value.
    /// </summary>
    /// <param name="P">The pairwise affinity matrix in the high-dimensional space.</param>
    /// <param name="Q">The pairwise affinity matrix in the low-dimensional space.</param>
    /// <returns>The cost function value.</returns>
    private T ComputeCost(Matrix<T> P, Matrix<T> Q)
    {
        int n = P.Rows;
        T cost = T.Zero;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (P[i, j] > T.Zero)
                {
                    cost += P[i, j] * T.Log(P[i, j] / Q[i, j]);
                }
            }
        }

        return cost;
    }
}
