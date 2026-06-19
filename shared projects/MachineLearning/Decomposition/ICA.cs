using System.Numerics;

using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.Decomposition;

/// <summary>
/// Independent Component Analysis (ICA) using the FastICA fixed-point algorithm.
/// Supports the log-cosh, exp, and cube contrast functions and both the symmetric
/// (parallel) and deflation estimation strategies.
/// </summary>
/// <remarks>
/// ICA performs blind source separation: given observations that are assumed to be
/// linear mixtures of statistically independent, non-Gaussian sources, it recovers
/// an unmixing matrix that restores the original sources up to scale and permutation.
/// <para>
/// The input is laid out the same way as <c>PCA</c>: each row is a sample (observation
/// over time) and each column is one observed mixed signal/feature.
/// </para>
/// </remarks>
/// <typeparam name="TSelf">The floating-point element type.</typeparam>
public class ICA<TSelf> : IMachineLearning
    where TSelf : struct, IFloatingPointIeee754<TSelf>
{
    private readonly int _numSamples;
    private readonly int _numFeatures;
    private readonly int _numComponents;
    private readonly int _maxIterations;
    private readonly TSelf _tolerance;
    private readonly int? _randomSeed;
    private readonly ICANonlinearity _nonlinearity;
    private readonly ICAAlgorithm _algorithm;
    private readonly TSelf _alpha;

    private TSelf[] _mean;          // per-feature mean, length = _numFeatures
    private TSelf[][] _whitened;    // whitened signals Z, [_numComponents][_numSamples]
    private TSelf[][] _whitening;   // whitening matrix K, [_numComponents][_numFeatures]
    private TSelf[][] _dewhitening; // de-whitening matrix K+, [_numFeatures][_numComponents]
    private TSelf[][] _unmixing;    // unmixing on whitened data W, [_numComponents][_numComponents]
    private TSelf[][] _components;  // unmixing on centered data W*K, [_numComponents][_numFeatures]
    private TSelf[][] _mixing;      // mixing matrix A, [_numFeatures][_numComponents]
    private TSelf[][] _sources;     // estimated sources S = W*Z, [_numComponents][_numSamples]
    private bool _hasConverged;
    private int _iterations;

    /// <inheritdoc/>
    public MachineLearningTask Task => MachineLearningTask.SourceSeparation;

    /// <summary>
    /// Initializes a new instance of the <see cref="ICA{TSelf}"/> class and runs FastICA.
    /// </summary>
    /// <param name="data">The input data set; each row is a sample and each column is an observed signal.</param>
    /// <param name="numberOfComponents">
    /// The number of independent components to extract. Defaults to the number of input signals (columns).
    /// </param>
    /// <param name="nonlinearity">The contrast function used to measure non-Gaussianity.</param>
    /// <param name="algorithm">The estimation strategy: symmetric (parallel) or deflation.</param>
    /// <param name="maxIterations">The maximum number of fixed-point iterations.</param>
    /// <param name="tolerance">The convergence tolerance on the change of the unmixing matrix.</param>
    /// <param name="alpha">The slope of the log-cosh contrast function (typically in [1, 2]); ignored by other nonlinearities.</param>
    /// <param name="randomSeed">An optional seed used to initialize the unmixing matrix for reproducible results.</param>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    public ICA(
        TSelf[,] data,
        int? numberOfComponents = null,
        ICANonlinearity nonlinearity = ICANonlinearity.LogCosh,
        ICAAlgorithm algorithm = ICAAlgorithm.Symmetric,
        int maxIterations = 200,
        double tolerance = 1e-4,
        double alpha = 1.0,
        int? randomSeed = null,
        ComputingContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        _numSamples = data.GetLength(0);
        _numFeatures = data.GetLength(1);

        if (_numSamples < 2)
            throw new ArgumentException("The input data must contain at least two samples.", nameof(data));

        _numComponents = numberOfComponents ?? _numFeatures;
        if (_numComponents < 1 || _numComponents > _numFeatures)
            throw new ArgumentOutOfRangeException(nameof(numberOfComponents), "The number of components must be between 1 and the number of features.");

        if (maxIterations < 1)
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "The maximum number of iterations must be a positive integer.");

        if (alpha <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(alpha), "The contrast slope alpha must be positive.");

        _maxIterations = maxIterations;
        _tolerance = TSelf.CreateChecked(tolerance);
        _randomSeed = randomSeed;
        _nonlinearity = nonlinearity;
        _algorithm = algorithm;
        _alpha = TSelf.CreateChecked(alpha);
        Context = context;

        var centered = CenterData(data);
        Whiten(centered);
        RunFastICA();
        ComputeSources();
        ComputeMixingAndComponents();
    }

    /// <summary>
    /// Execution policy honored by this estimator. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Gets the number of independent components extracted.
    /// </summary>
    public int NumberOfComponents => _numComponents;

    /// <summary>
    /// Gets a value indicating whether the fixed-point iteration converged within the iteration budget.
    /// </summary>
    public bool HasConverged => _hasConverged;

    /// <summary>
    /// Gets the number of iterations that were actually performed.
    /// </summary>
    public int Iterations => _iterations;

    /// <summary>
    /// Gets the contrast (nonlinearity) function used during estimation.
    /// </summary>
    public ICANonlinearity Nonlinearity => _nonlinearity;

    /// <summary>
    /// Gets the estimation strategy (symmetric or deflation) used.
    /// </summary>
    public ICAAlgorithm Algorithm => _algorithm;

    /// <summary>
    /// Gets a copy of the per-feature mean that was removed during centering.
    /// </summary>
    public TSelf[] Mean => (TSelf[])_mean.Clone();

    /// <summary>
    /// Transforms the original data into the estimated independent sources.
    /// </summary>
    /// <returns>A matrix of shape [samples, components] containing the recovered sources.</returns>
    public TSelf[,] Transform()
    {
        var result = new TSelf[_numSamples, _numComponents];
        for (int k = 0; k < _numComponents; k++)
        {
            var source = _sources[k];
            for (int s = 0; s < _numSamples; s++)
                result[s, k] = source[s];
        }
        return result;
    }

    /// <summary>
    /// Reconstructs the observed mixed signals from independent sources.
    /// </summary>
    /// <param name="sources">A matrix of shape [samples, components].</param>
    /// <returns>A matrix of shape [samples, features] containing the reconstructed observations.</returns>
    public TSelf[,] InverseTransform(TSelf[,] sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        int rows = sources.GetLength(0);
        int cols = sources.GetLength(1);
        if (cols != _numComponents)
            throw new ArgumentException("The number of source columns must equal the number of components.", nameof(sources));

        var result = new TSelf[rows, _numFeatures];
        for (int s = 0; s < rows; s++)
        {
            for (int i = 0; i < _numFeatures; i++)
            {
                TSelf sum = _mean[i];
                var mixingRow = _mixing[i];
                for (int k = 0; k < cols; k++)
                    sum += mixingRow[k] * sources[s, k];
                result[s, i] = sum;
            }
        }
        return result;
    }

    /// <summary>
    /// Gets a copy of the estimated mixing matrix A (shape [features, components]) where observations ≈ A · sources.
    /// </summary>
    public TSelf[,] GetMixingMatrix()
    {
        var result = new TSelf[_numFeatures, _numComponents];
        for (int i = 0; i < _numFeatures; i++)
        {
            var row = _mixing[i];
            for (int k = 0; k < _numComponents; k++)
                result[i, k] = row[k];
        }
        return result;
    }

    /// <summary>
    /// Gets a copy of the estimated unmixing matrix W (shape [components, features]) where sources = W · (observations - mean).
    /// </summary>
    public TSelf[,] GetUnmixingMatrix()
    {
        var result = new TSelf[_numComponents, _numFeatures];
        for (int k = 0; k < _numComponents; k++)
        {
            var row = _components[k];
            for (int i = 0; i < _numFeatures; i++)
                result[k, i] = row[i];
        }
        return result;
    }

    private TSelf[][] CenterData(TSelf[,] data)
    {
        _mean = new TSelf[_numFeatures];
        var centered = new TSelf[_numFeatures][];
        TSelf sampleCount = TSelf.CreateChecked(_numSamples);

        for (int f = 0; f < _numFeatures; f++)
        {
            TSelf sum = TSelf.Zero;
            for (int s = 0; s < _numSamples; s++)
                sum += data[s, f];

            TSelf mean = sum / sampleCount;
            _mean[f] = mean;

            var column = new TSelf[_numSamples];
            for (int s = 0; s < _numSamples; s++)
                column[s] = data[s, f] - mean;
            centered[f] = column;
        }

        return centered;
    }

    private void Whiten(TSelf[][] centered)
    {
        // Covariance matrix of the centered signals: C = (1 / numSamples) * Xc * Xc^T.
        var covariance = new Matrix<TSelf>(_numFeatures, _numFeatures);
        TSelf scale = TSelf.One / TSelf.CreateChecked(_numSamples);

        for (int i = 0; i < _numFeatures; i++)
        {
            var xi = centered[i];
            for (int j = i; j < _numFeatures; j++)
            {
                var xj = centered[j];
                TSelf sum = TSelf.Zero;
                for (int s = 0; s < _numSamples; s++)
                    sum += xi[s] * xj[s];

                TSelf value = sum * scale;
                covariance[i, j] = value;
                covariance[j, i] = value;
            }
        }

        var eigen = MatrixDecomposition.SymmetricEigendecomposition(covariance);
        TSelf epsilon = TSelf.CreateChecked(1e-12);

        _whitening = new TSelf[_numComponents][];
        _dewhitening = new TSelf[_numFeatures][];
        for (int i = 0; i < _numFeatures; i++)
            _dewhitening[i] = new TSelf[_numComponents];

        // Eigenpairs are sorted by descending eigenvalue; keep the leading components.
        for (int k = 0; k < _numComponents; k++)
        {
            TSelf lambda = eigen.Eigenvalues[k];
            if (lambda < epsilon)
                lambda = epsilon;

            TSelf sqrt = TSelf.Sqrt(lambda);
            TSelf invSqrt = TSelf.One / sqrt;
            var eigenVector = eigen.Eigenvectors.GetColumn(k);

            var whiteningRow = new TSelf[_numFeatures];
            for (int i = 0; i < _numFeatures; i++)
            {
                TSelf component = eigenVector[i];
                whiteningRow[i] = component * invSqrt;
                _dewhitening[i][k] = component * sqrt;
            }
            _whitening[k] = whiteningRow;
        }

        // Whitened signals Z = K * Xc.
        _whitened = new TSelf[_numComponents][];
        ComputingContextExecution.ForEach(
            Context,
            0,
            _numComponents,
            k =>
            {
                var z = new TSelf[_numSamples];
                var whiteningRow = _whitening[k];
                for (int i = 0; i < _numFeatures; i++)
                {
                    TSelf weight = whiteningRow[i];
                    var xi = centered[i];
                    for (int s = 0; s < _numSamples; s++)
                        z[s] += weight * xi[s];
                }
                _whitened[k] = z;
            },
            workPerItem: (long)_numFeatures * _numSamples);
    }

    private void Contrast(TSelf u, out TSelf g, out TSelf gPrime)
    {
        switch (_nonlinearity)
        {
            case ICANonlinearity.Exp:
            {
                // g(u) = u * exp(-u^2 / 2), g'(u) = (1 - u^2) * exp(-u^2 / 2).
                TSelf squared = u * u;
                TSelf exp = TSelf.Exp(-squared / TSelf.CreateChecked(2));
                g = u * exp;
                gPrime = (TSelf.One - squared) * exp;
                break;
            }
            case ICANonlinearity.Cube:
            {
                // g(u) = u^3, g'(u) = 3 * u^2.
                TSelf squared = u * u;
                g = squared * u;
                gPrime = TSelf.CreateChecked(3) * squared;
                break;
            }
            default:
            {
                // Log-cosh: g(u) = tanh(alpha * u), g'(u) = alpha * (1 - tanh^2).
                TSelf tanh = TSelf.Tanh(_alpha * u);
                g = tanh;
                gPrime = _alpha * (TSelf.One - tanh * tanh);
                break;
            }
        }
    }

    private static void Normalize(TSelf[] vector)
    {
        TSelf norm = TSelf.Zero;
        for (int j = 0; j < vector.Length; j++)
            norm += vector[j] * vector[j];

        norm = TSelf.Sqrt(norm);
        if (norm > TSelf.Zero)
        {
            TSelf inv = TSelf.One / norm;
            for (int j = 0; j < vector.Length; j++)
                vector[j] *= inv;
        }
    }

    private void RunFastICA()
    {
        int nc = _numComponents;
        var random = _randomSeed.HasValue ? new Random(_randomSeed.Value) : Random.Shared;

        var w = new TSelf[nc][];
        for (int k = 0; k < nc; k++)
        {
            var row = new TSelf[nc];
            for (int j = 0; j < nc; j++)
                row[j] = TSelf.CreateChecked(random.NextDouble() * 2.0 - 1.0);
            w[k] = row;
        }

        _hasConverged = false;
        _iterations = 0;

        _unmixing = _algorithm == ICAAlgorithm.Deflation
            ? RunDeflation(w)
            : RunSymmetric(w);
    }

    private TSelf[][] RunSymmetric(TSelf[][] w)
    {
        int nc = _numComponents;
        int m = _numSamples;
        TSelf invSamples = TSelf.One / TSelf.CreateChecked(m);

        SymmetricDecorrelation(w);

        var wNext = new TSelf[nc][];
        for (int k = 0; k < nc; k++)
            wNext[k] = new TSelf[nc];

        for (int iteration = 1; iteration <= _maxIterations; iteration++)
        {
            _iterations = iteration;

            for (int k = 0; k < nc; k++)
            {
                var wk = w[k];
                var next = wNext[k];
                for (int i = 0; i < nc; i++)
                    next[i] = TSelf.Zero;

                // next accumulates E[Z * g(W_k · Z)]; derivativeSum accumulates g'(W_k · Z).
                TSelf derivativeSum = TSelf.Zero;
                for (int s = 0; s < m; s++)
                {
                    TSelf projection = TSelf.Zero;
                    for (int j = 0; j < nc; j++)
                        projection += wk[j] * _whitened[j][s];

                    Contrast(projection, out TSelf g, out TSelf gPrime);
                    derivativeSum += gPrime;
                    for (int i = 0; i < nc; i++)
                        next[i] += g * _whitened[i][s];
                }

                // W_k+ = E[Z * g] - E[g'] * W_k.
                TSelf derivativeMean = derivativeSum * invSamples;
                for (int i = 0; i < nc; i++)
                    next[i] = next[i] * invSamples - derivativeMean * wk[i];
            }

            SymmetricDecorrelation(wNext);

            // Convergence: how close each new row is to the previous one (up to sign).
            TSelf maxChange = TSelf.Zero;
            for (int k = 0; k < nc; k++)
            {
                var a = wNext[k];
                var b = w[k];
                TSelf dot = TSelf.Zero;
                for (int j = 0; j < nc; j++)
                    dot += a[j] * b[j];

                TSelf change = TSelf.Abs(TSelf.Abs(dot) - TSelf.One);
                if (change > maxChange)
                    maxChange = change;
            }

            (w, wNext) = (wNext, w);

            if (maxChange < _tolerance)
            {
                _hasConverged = true;
                break;
            }
        }

        return w;
    }

    private TSelf[][] RunDeflation(TSelf[][] w)
    {
        int nc = _numComponents;
        int m = _numSamples;
        TSelf invSamples = TSelf.One / TSelf.CreateChecked(m);

        var wNew = new TSelf[nc];
        bool allConverged = true;

        for (int k = 0; k < nc; k++)
        {
            var wk = w[k];
            Normalize(wk);

            bool converged = false;
            for (int iteration = 1; iteration <= _maxIterations; iteration++)
            {
                if (iteration > _iterations)
                    _iterations = iteration;

                for (int i = 0; i < nc; i++)
                    wNew[i] = TSelf.Zero;

                // wNew accumulates E[Z * g(W_k · Z)]; derivativeSum accumulates g'(W_k · Z).
                TSelf derivativeSum = TSelf.Zero;
                for (int s = 0; s < m; s++)
                {
                    TSelf projection = TSelf.Zero;
                    for (int j = 0; j < nc; j++)
                        projection += wk[j] * _whitened[j][s];

                    Contrast(projection, out TSelf g, out TSelf gPrime);
                    derivativeSum += gPrime;
                    for (int i = 0; i < nc; i++)
                        wNew[i] += g * _whitened[i][s];
                }

                TSelf derivativeMean = derivativeSum * invSamples;
                for (int i = 0; i < nc; i++)
                    wNew[i] = wNew[i] * invSamples - derivativeMean * wk[i];

                // Gram-Schmidt deflation: remove projections onto already-found components.
                for (int p = 0; p < k; p++)
                {
                    var wp = w[p];
                    TSelf dot = TSelf.Zero;
                    for (int j = 0; j < nc; j++)
                        dot += wNew[j] * wp[j];
                    for (int j = 0; j < nc; j++)
                        wNew[j] -= dot * wp[j];
                }

                Normalize(wNew);

                // Convergence: how close the new vector is to the previous one (up to sign).
                TSelf dotOld = TSelf.Zero;
                for (int j = 0; j < nc; j++)
                    dotOld += wNew[j] * wk[j];
                TSelf change = TSelf.Abs(TSelf.Abs(dotOld) - TSelf.One);

                for (int j = 0; j < nc; j++)
                    wk[j] = wNew[j];

                if (change < _tolerance)
                {
                    converged = true;
                    break;
                }
            }

            if (!converged)
                allConverged = false;
        }

        _hasConverged = allConverged;
        return w;
    }

    private void SymmetricDecorrelation(TSelf[][] w)
    {
        int nc = _numComponents;

        // A = W * W^T (symmetric, positive definite).
        var a = new Matrix<TSelf>(nc, nc);
        for (int p = 0; p < nc; p++)
        {
            var wp = w[p];
            for (int q = p; q < nc; q++)
            {
                var wq = w[q];
                TSelf sum = TSelf.Zero;
                for (int j = 0; j < nc; j++)
                    sum += wp[j] * wq[j];
                a[p, q] = sum;
                a[q, p] = sum;
            }
        }

        var eigen = MatrixDecomposition.SymmetricEigendecomposition(a);
        TSelf epsilon = TSelf.CreateChecked(1e-12);

        var eigenVectors = new TSelf[nc][];
        var invSqrt = new TSelf[nc];
        for (int k = 0; k < nc; k++)
        {
            TSelf lambda = eigen.Eigenvalues[k];
            if (lambda < epsilon)
                lambda = epsilon;
            invSqrt[k] = TSelf.One / TSelf.Sqrt(lambda);
            eigenVectors[k] = eigen.Eigenvectors.GetColumn(k);
        }

        // B = A^(-1/2) = U * diag(1/sqrt(lambda)) * U^T.
        var b = new TSelf[nc][];
        for (int p = 0; p < nc; p++)
        {
            var row = new TSelf[nc];
            for (int q = 0; q < nc; q++)
            {
                TSelf sum = TSelf.Zero;
                for (int k = 0; k < nc; k++)
                    sum += eigenVectors[k][p] * invSqrt[k] * eigenVectors[k][q];
                row[q] = sum;
            }
            b[p] = row;
        }

        // W <- B * W.
        var result = new TSelf[nc][];
        for (int p = 0; p < nc; p++)
        {
            var bp = b[p];
            var row = new TSelf[nc];
            for (int i = 0; i < nc; i++)
            {
                TSelf sum = TSelf.Zero;
                for (int q = 0; q < nc; q++)
                    sum += bp[q] * w[q][i];
                row[i] = sum;
            }
            result[p] = row;
        }

        for (int p = 0; p < nc; p++)
            result[p].CopyTo(w[p], 0);
    }

    private void ComputeSources()
    {
        int nc = _numComponents;
        int m = _numSamples;

        _sources = new TSelf[nc][];
        ComputingContextExecution.ForEach(
            Context,
            0,
            nc,
            k =>
            {
                var source = new TSelf[m];
                var wk = _unmixing[k];
                for (int j = 0; j < nc; j++)
                {
                    TSelf weight = wk[j];
                    var zj = _whitened[j];
                    for (int s = 0; s < m; s++)
                        source[s] += weight * zj[s];
                }
                _sources[k] = source;
            },
            workPerItem: (long)nc * m);
    }

    private void ComputeMixingAndComponents()
    {
        int nc = _numComponents;
        int nf = _numFeatures;

        // Unmixing on centered data: components = W * K.
        _components = new TSelf[nc][];
        ComputingContextExecution.ForEach(
            Context,
            0,
            nc,
            k =>
            {
                var wk = _unmixing[k];
                var row = new TSelf[nf];
                for (int i = 0; i < nf; i++)
                {
                    TSelf sum = TSelf.Zero;
                    for (int j = 0; j < nc; j++)
                        sum += wk[j] * _whitening[j][i];
                    row[i] = sum;
                }
                _components[k] = row;
            },
            workPerItem: (long)nf * nc);

        // Mixing matrix: A = K+ * W^T.
        _mixing = new TSelf[nf][];
        ComputingContextExecution.ForEach(
            Context,
            0,
            nf,
            i =>
            {
                var dewhiteningRow = _dewhitening[i];
                var row = new TSelf[nc];
                for (int k = 0; k < nc; k++)
                {
                    var wk = _unmixing[k];
                    TSelf sum = TSelf.Zero;
                    for (int j = 0; j < nc; j++)
                        sum += dewhiteningRow[j] * wk[j];
                    row[k] = sum;
                }
                _mixing[i] = row;
            },
            workPerItem: (long)nc * nc);
    }
}

/// <summary>
/// The contrast (nonlinearity) function used by FastICA to measure non-Gaussianity.
/// </summary>
public enum ICANonlinearity
{
    /// <summary>
    /// The log-cosh function: g(u) = tanh(alpha * u). A robust general-purpose default.
    /// </summary>
    LogCosh,

    /// <summary>
    /// The Gaussian-derived function: g(u) = u * exp(-u^2 / 2). More robust when sources are highly super-Gaussian or contain outliers.
    /// </summary>
    Exp,

    /// <summary>
    /// The cubic function: g(u) = u^3. Fast, but more sensitive to outliers.
    /// </summary>
    Cube
}

/// <summary>
/// The strategy used to estimate the independent components.
/// </summary>
public enum ICAAlgorithm
{
    /// <summary>
    /// Estimates all components in parallel and orthogonalizes them with symmetric decorrelation.
    /// </summary>
    Symmetric,

    /// <summary>
    /// Estimates the components one at a time, removing the previously found ones with Gram-Schmidt deflation.
    /// </summary>
    Deflation
}
