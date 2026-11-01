namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the sliding-window RLS Adaptive filter: after every adapted sample the weights are the exact least-squares solution over the last
/// <c>window</c> adapted samples only (a rectangular window, no exponential forgetting):
/// w = argmin Σ (d(i) − wT x(i))² + delta · ‖w − w<sub>0</sub>‖².
/// A sample that leaves the window is removed from the solution again (a rank-one downdate), so a change of the system is tracked exactly
/// <c>window</c> samples later and a burst of outliers disappears completely once it has left the window; with an exponential
/// window (<see cref="RlsFilter"/>) it fades out only gradually.
/// </summary>
/// <remarks>
/// <para>
/// The inverse correlation matrix is updated in double precision in O(order²) per sample; the window of the last <c>window</c> regressors and
/// targets is stored (<c>window · order</c> values). A downdate is numerically delicate (it is the inverse of an update),
/// so the matrix and the weights are recomputed from the window every <c>refreshInterval</c> samples (a Cholesky solve, O(window · order² + order³)).
/// <c>w<sub>0</sub></c> are the weights at the last <see cref="AdaptiveFilter.Init(float[])"/> or <see cref="AdaptiveFilter.Reset"/> (zero for a new filter): the small regularization <c>delta</c> pulls the solution
/// towards them when the window does not excite all directions.
/// </para>
/// <para>
/// Only adapted samples enter the window. Frozen filtering (<see cref="AdaptiveFilter.Process(float)"/>, <c>ProcessAllSamples</c>) does not change the weights
/// and the window, it only extends the signal history.
/// </para>
/// </remarks>
public class SlidingWindowRlsFilter : AdaptiveFilter
{
    private readonly int _window;
    private readonly double _delta;
    private readonly int _refreshInterval;

    // the last <window> regressors and targets, a ring whose oldest entry is at _head
    private readonly float[] _windowRegressors;
    private readonly float[] _windowTargets;
    private int _head;
    private int _count;
    private long _sinceRefresh;

    private readonly double[] _w;           // weights in double precision
    private readonly double[] _prior;       // w0
    private readonly double[] _p;           // inverse correlation matrix (row-major)
    private readonly double[] _x;           // regressor of the new sample
    private readonly double[] _o;           // regressor of the sample that leaves the window
    private readonly double[] _px;          // P x

    /// <summary>
    /// Constructs <see cref="SlidingWindowRlsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="window">Length of the window in samples; at least <paramref name="order"/> to determine all weights</param>
    /// <param name="delta">Regularization: the matrix is initialized with delta · I and keeps this share, a small positive value compared to the energy of the regressor over the window</param>
    /// <param name="refreshInterval">The matrix and the weights are recomputed from the window every this number of samples (0 for never); <paramref name="window"/> if omitted</param>
    public SlidingWindowRlsFilter(int order, int window, float delta = 1e-3f, int? refreshInterval = null) : base(order)
    {
        Guard.AgainstNonPositive(window, nameof(window));
        AdaptiveArgument.Positive(delta, nameof(delta));
        if (window < order)
            throw new ArgumentOutOfRangeException(nameof(window), "window must not be shorter than order.");
        if (refreshInterval is < 0)
            throw new ArgumentOutOfRangeException(nameof(refreshInterval), "refreshInterval must not be negative.");
        var windowLength = AdaptiveArgument.Count((long)window * order, nameof(window));
        var matrixLength = AdaptiveArgument.Count((long)order * order, nameof(order));

        _window = window;
        _delta = delta;
        _refreshInterval = refreshInterval ?? window;

        _windowRegressors = new float[windowLength];
        _windowTargets = new float[window];
        _w = new double[order];
        _prior = new double[order];
        _p = new double[matrixLength];
        _x = new double[order];
        _o = new double[order];
        _px = new double[order];
        ResetState();
    }

    /// <summary>
    /// Gets the length of the window in samples.
    /// </summary>
    public int Window => _window;

    /// <summary>
    /// Restarts the solution: the window becomes empty and the matrix is initialized again; the current weights become the prior weights w<sub>0</sub>.
    /// </summary>
    protected override void ResetState()
    {
        for (var i = 0; i < _kernelSize; i++)
        {
            _w[i] = _b[i];
            _prior[i] = _b[i];
        }

        Array.Clear(_p);
        for (var i = 0; i < _kernelSize; i++)
        {
            _p[i * _kernelSize + i] = 1.0 / _delta;
        }

        _head = 0;
        _count = 0;
        _sinceRefresh = 0;
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts filter coefficients.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override float Process(float input, float desired)
    {
        var n = _kernelSize;

        Push(input, out var offset);
        var regressor = Regressor(offset);
        for (var i = 0; i < n; i++)
        {
            _x[i] = regressor[i];
        }

        // the a priori output in double precision
        var y = 0.0;
        for (var i = 0; i < n; i++)
        {
            y += _w[i] * _x[i];
        }

        // A non-finite sample would enter the window and the matrix (and make every refresh fail until it has left the window): it is skipped,
        // the window, the matrix and the weights stay as they are.
        if (!double.IsFinite(y) || !float.IsFinite(desired))
        {
            return (float)y;
        }

        // the sample that leaves the window (taken out before its slot is reused)
        var leaves = _count == _window;
        var leavingTarget = 0.0;
        if (leaves)
        {
            var slot = _head * n;
            for (var i = 0; i < n; i++)
            {
                _o[i] = _windowRegressors[slot + i];
            }
            leavingTarget = _windowTargets[_head];
        }

        // add the new sample: the RLS update with λ = 1
        var denominator = 1.0;
        for (var i = 0; i < n; i++)
        {
            var s = 0.0;
            var row = i * n;
            for (var k = 0; k < n; k++)
            {
                s += _p[row + k] * _x[k];
            }
            _px[i] = s;
            denominator += s * _x[i];
        }

        var error = desired - y;
        var invDenominator = 1.0 / denominator;
        for (var i = 0; i < n; i++)
        {
            _w[i] += _px[i] * invDenominator * error;
        }
        RankOneUpdate(-invDenominator);

        // store the new sample
        var target = leaves ? _head : (_head + _count) % _window;
        for (var i = 0; i < n; i++)
        {
            _windowRegressors[target * n + i] = (float)_x[i];
        }
        _windowTargets[target] = desired;
        if (leaves)
        {
            _head = (_head + 1) % _window;
        }
        else
        {
            _count++;
        }

        // remove the sample that has left the window
        if (leaves)
        {
            var xPx = 0.0;
            var leavingError = leavingTarget;
            for (var i = 0; i < n; i++)
            {
                var s = 0.0;
                var row = i * n;
                for (var k = 0; k < n; k++)
                {
                    s += _p[row + k] * _o[k];
                }
                _px[i] = s;
                xPx += s * _o[i];
                leavingError -= _w[i] * _o[i];
            }

            var rest = 1.0 - xPx;
            if (rest > 1e-9)
            {
                var invRest = 1.0 / rest;
                for (var i = 0; i < n; i++)
                {
                    _w[i] -= leavingError * _px[i] * invRest;
                }
                RankOneUpdate(invRest);
            }
            else
            {
                // the downdate is ill-conditioned: solve the window from scratch
                Refresh();
            }
        }

        if (_refreshInterval > 0 && ++_sinceRefresh >= _refreshInterval)
        {
            Refresh();
        }

        for (var i = 0; i < n; i++)
        {
            SetWeight(i, (float)_w[i]);
        }

        return (float)y;
    }

    /// <summary>
    /// P += scale · (P x)(P x)T, with P x stored in <see cref="_px"/>.
    /// </summary>
    private void RankOneUpdate(double scale)
    {
        var n = _kernelSize;
        for (var i = 0; i < n; i++)
        {
            var row = i * n;
            var left = scale * _px[i];
            for (var k = i; k < n; k++)
            {
                var v = _p[row + k] + left * _px[k];
                _p[row + k] = v;
                _p[k * n + i] = v;
            }
        }
    }

    /// <summary>
    /// Recomputes the matrix and the weights from the samples inside the window. The attempt counts whether it succeeds or not: a window that cannot be solved
    /// (the matrix is not positive definite) is tried again after another <c>refreshInterval</c> samples, not at every sample.
    /// </summary>
    private void Refresh()
    {
        _sinceRefresh = 0;

        var n = _kernelSize;

        var a = new double[n * n];
        var rhs = new double[n];
        for (var i = 0; i < n; i++)
        {
            a[i * n + i] = _delta;
            rhs[i] = _delta * _prior[i];
        }

        for (var t = 0; t < _count; t++)
        {
            var slot = t * n;
            var target = _windowTargets[t];
            for (var i = 0; i < n; i++)
            {
                var xi = (double)_windowRegressors[slot + i];
                rhs[i] += target * xi;
                var row = i * n;
                for (var k = i; k < n; k++)
                {
                    a[row + k] += xi * _windowRegressors[slot + k];
                }
            }
        }

        for (var i = 0; i < n; i++)
        {
            for (var k = 0; k < i; k++)
            {
                a[i * n + k] = a[k * n + i];
            }
        }

        TryInvertAndSolve(a, rhs, n);
    }

    /// <summary>
    /// Inverts the symmetric positive definite matrix with a Cholesky factorization into the inverse correlation matrix and solves for the weights.
    /// Nothing is changed when the matrix is not positive definite.
    /// </summary>
    private bool TryInvertAndSolve(double[] a, double[] rhs, int n)
    {
        var l = new double[n * n];
        for (var j = 0; j < n; j++)
        {
            var d = a[j * n + j];
            for (var p = 0; p < j; p++)
            {
                d -= l[j * n + p] * l[j * n + p];
            }
            if (!(d > 0))
            {
                return false;
            }

            var ljj = Math.Sqrt(d);
            l[j * n + j] = ljj;
            for (var i = j + 1; i < n; i++)
            {
                var s = a[i * n + j];
                for (var p = 0; p < j; p++)
                {
                    s -= l[i * n + p] * l[j * n + p];
                }
                l[i * n + j] = s / ljj;
            }
        }

        // P = A^-1 column by column: solve L z = e_c, then L^T p = z
        var column = new double[n];
        for (var c = 0; c < n; c++)
        {
            for (var i = 0; i < n; i++)
            {
                var s = i == c ? 1.0 : 0.0;
                for (var p = 0; p < i; p++)
                {
                    s -= l[i * n + p] * column[p];
                }
                column[i] = s / l[i * n + i];
            }
            for (var i = n - 1; i >= 0; i--)
            {
                var s = column[i];
                for (var p = i + 1; p < n; p++)
                {
                    s -= l[p * n + i] * column[p];
                }
                column[i] = s / l[i * n + i];
            }
            for (var i = 0; i < n; i++)
            {
                _p[i * n + c] = column[i];
            }
        }

        for (var i = 0; i < n; i++)
        {
            var s = 0.0;
            for (var k = 0; k < n; k++)
            {
                s += _p[i * n + k] * rhs[k];
            }
            _w[i] = s;
        }

        return true;
    }
}
