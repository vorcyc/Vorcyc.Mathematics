namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the QRD-RLS Adaptive filter (QR-decomposition based Recursive-Least-Squares algorithm): the Cholesky factor of the
/// correlation matrix is updated with Givens rotations and the weights are obtained by back substitution.
/// It solves the same problem as <see cref="RlsFilter"/> but never forms the inverse correlation matrix, so it stays numerically stable
/// for ill-conditioned (strongly colored) inputs. The cost is O(order²) per sample. A regressor of zeros (digital silence) is skipped,
/// so a gap leaves the filter untouched.
/// </summary>
public class QrdRlsFilter : AdaptiveFilter
{
    /// <summary>
    /// A row of the factor is not allowed to decay below this value (it would underflow after a very long input that never excites it).
    /// </summary>
    private const double Floor = 1e-100;

    private readonly double _sqrtLambda;
    private readonly double _sqrtDelta;

    /// <summary>
    /// Upper triangular Cholesky factor (row-major).
    /// </summary>
    private readonly double[] _r;

    /// <summary>
    /// Right-hand side: R w = p.
    /// </summary>
    private readonly double[] _p;

    /// <summary>
    /// The new data row [x, d] that is rotated into the factor.
    /// </summary>
    private readonly double[] _row;

    private readonly double[] _w;

    /// <summary>
    /// Constructs <see cref="QrdRlsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="lambda">Lambda, forgetting factor in (0, 1]</param>
    /// <param name="initCorrMatrix">The correlation matrix is initialized with I / initCorrMatrix (the same meaning as for <see cref="RlsFilter"/>)</param>
    public QrdRlsFilter(int order, float lambda = 0.99f, float initCorrMatrix = 1e2f) : base(order)
    {
        AdaptiveArgument.Positive(lambda, nameof(lambda));
        AdaptiveArgument.InRange(lambda, 0f, 1f, nameof(lambda));
        AdaptiveArgument.Positive(initCorrMatrix, nameof(initCorrMatrix));
        var factorLength = AdaptiveArgument.Count((long)_kernelSize * _kernelSize, nameof(order));

        _sqrtLambda = Math.Sqrt(lambda);
        _sqrtDelta = 1.0 / Math.Sqrt(initCorrMatrix);

        _r = new double[factorLength];
        _p = new double[_kernelSize];
        _row = new double[_kernelSize + 1];
        _w = new double[_kernelSize];
        ResetState();
    }

    /// <summary>
    /// Restores the initial factor, regularized towards the current weights.
    /// </summary>
    protected override void ResetState()
    {
        Array.Clear(_r);
        for (var i = 0; i < _kernelSize; i++)
        {
            _r[i * _kernelSize + i] = _sqrtDelta;
            _p[i] = _sqrtDelta * _b[i];
            _w[i] = _b[i];
        }
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts filter coefficients.
    /// The weights are the solution of R w = p, so use <see cref="AdaptiveFilter.Init(float[])"/> (or ChangeKernel) to replace them.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override float Process(float input, float desired)
    {
        var n = _kernelSize;

        var y = Push(input, out var offset);

        // the weights were replaced from outside (ChangeKernel): keep the factor and make it fit the new weights, p = R w
        for (var i = 0; i < n; i++)
        {
            if (_b[i] != (float)_w[i])
            {
                for (var k = 0; k < n; k++)
                {
                    _w[k] = _b[k];
                }
                for (var k = 0; k < n; k++)
                {
                    var rk = k * n;
                    var sum = 0.0;
                    for (var j = k; j < n; j++)
                    {
                        sum += _r[rk + j] * _w[j];
                    }
                    _p[k] = sum;
                }
                break;
            }
        }

        var x = Regressor(offset);
        var silent = true;
        var finite = true;
        for (var i = 0; i < n; i++)
        {
            _row[i] = x[i];
            silent &= x[i] == 0;
            finite &= float.IsFinite(x[i]);
        }

        // a regressor of zeros carries no information: forgetting would only make the factor decay,
        // so a gap of silence leaves the filter untouched
        if (silent)
        {
            return y;
        }

        // a non-finite sample would end in every element of the factor for good: the sample is skipped
        if (!finite || !float.IsFinite(desired))
        {
            return y;
        }

        _row[n] = desired;

        for (var i = 0; i < n; i++)
        {
            var ri = i * n;

            // forgetting: a row of the factor that is never excited keeps decaying, stop it long before it underflows
            var scale = _sqrtLambda;
            if (_r[ri + i] * scale < Floor)
            {
                scale = 1;
            }

            for (var j = i; j < n; j++)
            {
                _r[ri + j] *= scale;
            }
            _p[i] *= scale;

            // Givens rotation that annihilates the i-th element of the new row
            var diagonal = _r[ri + i];
            var element = _row[i];
            if (element == 0)
            {
                continue;
            }

            var radius = Math.Sqrt(diagonal * diagonal + element * element);
            var c = diagonal / radius;
            var s = element / radius;

            for (var j = i; j < n; j++)
            {
                var rij = _r[ri + j];
                var vj = _row[j];
                _r[ri + j] = c * rij + s * vj;
                _row[j] = c * vj - s * rij;
            }

            var pi = _p[i];
            var vd = _row[n];
            _p[i] = c * pi + s * vd;
            _row[n] = c * vd - s * pi;
        }

        // back substitution: R w = p
        for (var i = n - 1; i >= 0; i--)
        {
            var ri = i * n;
            var sum = _p[i];
            for (var j = i + 1; j < n; j++)
            {
                sum -= _r[ri + j] * _w[j];
            }
            _w[i] = sum / _r[ri + i];
            SetWeight(i, (float)_w[i]);
        }

        return y;
    }
}
