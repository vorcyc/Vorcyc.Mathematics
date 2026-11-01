namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the Affine Projection Algorithm (APA) Adaptive filter: the weights are updated so that the a posteriori errors of the
/// last <c>projectionOrder</c> regressors are reduced at once, w += mu · U (UT U + delta I)^-1 e.
/// It converges much faster than <see cref="NlmsFilter"/> for colored (correlated) inputs; with <c>projectionOrder = 1</c> it is NLMS.
/// The cost is O(order · projectionOrder²) per sample.
/// </summary>
public class ApaFilter : AdaptiveFilter
{
    private readonly int _projection;
    private readonly float _mu;
    private readonly double _delta;

    /// <summary>
    /// Inputs x(n), x(n-1), ..., newest first (order + projectionOrder - 1 samples).
    /// </summary>
    private readonly float[] _history;

    /// <summary>
    /// Desired samples d(n), d(n-1), ..., newest first (projectionOrder samples).
    /// </summary>
    private readonly float[] _desiredHistory;

    private readonly double[] _gram;
    private readonly double[] _errors;
    private readonly double[] _solution;

    /// <summary>
    /// Constructs <see cref="ApaFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="projectionOrder">Number of regressors used by one update, usually 2..16</param>
    /// <param name="mu">Step size, stable for 0 &lt; mu &lt; 2 regardless of the input scale</param>
    /// <param name="delta">Regularization of the projection matrix; keep it small compared to the expected power of the input over <paramref name="order"/> samples</param>
    public ApaFilter(int order, int projectionOrder = 4, float mu = 0.5f, float delta = 1e-3f) : base(order)
    {
        Guard.AgainstNonPositive(projectionOrder, nameof(projectionOrder));
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.Positive(delta, nameof(delta));
        var historyLength = AdaptiveArgument.Count(order + (long)projectionOrder - 1, nameof(projectionOrder));
        var gramLength = AdaptiveArgument.Count((long)projectionOrder * projectionOrder, nameof(projectionOrder));

        _projection = projectionOrder;
        _mu = mu;
        _delta = delta;
        _history = new float[historyLength];
        _desiredHistory = new float[projectionOrder];
        _gram = new double[gramLength];
        _errors = new double[projectionOrder];
        _solution = new double[projectionOrder];
    }

    /// <summary>
    /// Gets the number of regressors used by one update.
    /// </summary>
    public int ProjectionOrder => _projection;

    /// <summary>
    /// Clears the history of the input and desired signals.
    /// </summary>
    protected override void ResetHistory()
    {
        Array.Clear(_history);
        Array.Clear(_desiredHistory);
    }

    /// <summary>
    /// Processes one sample with the current weights and without adapting them. The sample is kept in the history
    /// (its own output stands for the unknown desired value, so it does not influence the adaptation that may follow).
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        Array.Copy(_history, 0, _history, 1, _history.Length - 1);
        _history[0] = sample;

        var y = base.Process(sample);

        Array.Copy(_desiredHistory, 0, _desiredHistory, 1, _projection - 1);
        _desiredHistory[0] = y;

        return y;
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts filter coefficients.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override float Process(float input, float desired)
    {
        var y = Push(input, out _);

        var n = _kernelSize;
        var k = _projection;

        _desiredHistory[0] = desired;

        // a priori errors of the last k regressors: regressor j is _history[j .. j + n - 1]
        for (var j = 0; j < k; j++)
        {
            double estimate = 0;
            for (var i = 0; i < n; i++)
            {
                estimate += _b[i] * _history[j + i];
            }
            _errors[j] = _desiredHistory[j] - estimate;

            // a non-finite input or desired sample (they stay in the histories for a while) would end in every weight
            if (!double.IsFinite(_errors[j]))
            {
                return y;
            }
        }

        // Gram matrix UT U + delta I
        for (var j = 0; j < k; j++)
        {
            for (var l = j; l < k; l++)
            {
                double sum = 0;
                for (var i = 0; i < n; i++)
                {
                    sum += _history[j + i] * _history[l + i];
                }
                _gram[j * k + l] = sum;
                _gram[l * k + j] = sum;
            }
            _gram[j * k + j] += _delta;
        }

        if (!SolveSymmetricPositiveDefinite(_gram, _errors, _solution, k))
        {
            return y;
        }

        for (var i = 0; i < n; i++)
        {
            double correction = 0;
            for (var j = 0; j < k; j++)
            {
                correction += _solution[j] * _history[j + i];
            }
            SetWeight(i, (float)(_b[i] + _mu * correction));
        }

        return y;
    }

    /// <summary>
    /// Solves a x = b for a symmetric positive definite matrix with the Cholesky decomposition (the matrix is overwritten).
    /// </summary>
    private static bool SolveSymmetricPositiveDefinite(double[] a, double[] b, double[] x, int k)
    {
        for (var j = 0; j < k; j++)
        {
            var d = a[j * k + j];
            for (var p = 0; p < j; p++)
            {
                d -= a[j * k + p] * a[j * k + p];
            }

            if (!(d > 0))
            {
                return false;
            }

            var l = Math.Sqrt(d);
            a[j * k + j] = l;

            for (var i = j + 1; i < k; i++)
            {
                var s = a[i * k + j];
                for (var p = 0; p < j; p++)
                {
                    s -= a[i * k + p] * a[j * k + p];
                }
                a[i * k + j] = s / l;
            }
        }

        for (var i = 0; i < k; i++)
        {
            var s = b[i];
            for (var p = 0; p < i; p++)
            {
                s -= a[i * k + p] * x[p];
            }
            x[i] = s / a[i * k + i];
        }

        for (var i = k - 1; i >= 0; i--)
        {
            var s = x[i];
            for (var p = i + 1; p < k; p++)
            {
                s -= a[p * k + i] * x[p];
            }
            x[i] = s / a[i * k + i];
        }

        return true;
    }
}
