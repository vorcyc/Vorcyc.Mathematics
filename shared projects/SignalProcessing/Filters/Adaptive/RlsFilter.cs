namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents Adaptive RLS filter (Recursive-Least-Squares algorithm) with exponential forgetting.
/// The inverse correlation matrix is kept in double precision and updated in O(order²) per sample.
/// A regressor of zeros (digital silence) is skipped, so a gap leaves the filter untouched; the growth of the matrix is bounded,
/// so an input that never excites part of the input space (e.g. a constant or a pure tone) cannot overflow it.
/// For strongly colored inputs the numerically more robust <see cref="QrdRlsFilter"/> is the better choice.
/// </summary>
public class RlsFilter : AdaptiveFilter
{
    /// <summary>
    /// How many times the trace of the inverse corr matrix may exceed its initial value.
    /// </summary>
    private const double MaxGrowth = 1e16;

    /// <summary>
    /// Lambda.
    /// </summary>
    private readonly double _lambda;

    /// <summary>
    /// Value to initialize inverse corr matrix.
    /// </summary>
    private readonly double _initCorrMatrix;

    /// <summary>
    /// Safety bound of the trace of the inverse corr matrix. It is far above anything that a signal of a reasonable level produces
    /// (the matrix grows to about (1 - lambda) / input power per tap), so it does not change the result of the algorithm;
    /// it only keeps the matrix finite when part of the input space is never excited.
    /// </summary>
    private readonly double _maxTrace;

    /// <summary>
    /// Inverse corr matrix (row-major, symmetric).
    /// </summary>
    private readonly double[] _p;

    /// <summary>
    /// Matrix of gain coefficients.
    /// </summary>
    private readonly double[] _gains;

    /// <summary>
    /// Regressor in double precision.
    /// </summary>
    private readonly double[] _x;

    /// <summary>
    /// Constructs <see cref="RlsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="lambda">Lambda, forgetting factor in (0, 1]</param>
    /// <param name="initCorrMatrix">Value to initialize inverse corr matrix</param>
    public RlsFilter(int order, float lambda = 0.99f, float initCorrMatrix = 1e2f) : base(order)
    {
        AdaptiveArgument.Positive(lambda, nameof(lambda));
        AdaptiveArgument.InRange(lambda, 0f, 1f, nameof(lambda));
        AdaptiveArgument.Positive(initCorrMatrix, nameof(initCorrMatrix));
        var matrixLength = AdaptiveArgument.Count((long)_kernelSize * _kernelSize, nameof(order));

        _lambda = lambda;
        _initCorrMatrix = initCorrMatrix;
        _maxTrace = _kernelSize * _initCorrMatrix * MaxGrowth;

        _p = new double[matrixLength];
        _gains = new double[_kernelSize];
        _x = new double[_kernelSize];
        ResetState();
    }

    /// <summary>
    /// Restores the inverse corr matrix to its initial value.
    /// </summary>
    protected override void ResetState()
    {
        Array.Clear(_p);
        for (var i = 0; i < _kernelSize; i++)
        {
            _p[i * _kernelSize + i] = _initCorrMatrix;
        }
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

        // the output is accumulated in double precision as the weights are used together with the double precision matrix
        double y = 0;
        var silent = true;
        for (int i = 0; i < n; i++)
        {
            _x[i] = _delayLine[offset + i];
            y += _b[i] * _x[i];
            silent &= _x[i] == 0;
        }

        // a regressor of zeros carries no information: forgetting would only make the inverse corr matrix grow,
        // so a gap of silence leaves the filter (weights and matrix) untouched
        if (silent)
        {
            return 0f;
        }

        var e = desired - y;

        // a non-finite sample would end in every element of the inverse corr matrix for good: the sample is skipped
        if (!double.IsFinite(e))
        {
            return (float)y;
        }

        // ======================================================================
        // ============= lot of calculations before updating weights ============
        // ======================================================================

        // =========== calculate gain coefficients ===========
        // ===========   p*x / (lambda + xT*p*x)   ===========

        for (int i = 0; i < n; i++)    // calculate  p*x
        {
            double s = 0;
            var row = i * n;
            for (int k = 0; k < n; k++)
            {
                s += _p[row + k] * _x[k];
            }
            _gains[i] = s;
        }

        var g = _lambda;

        for (int k = 0; k < n; k++)      // calculate xT*p*x
        {
            g += _gains[k] * _x[k];
        }

        var invG = 1.0 / g;

        // ============ update inv corr matrix ================
        // ========== (p - gain*xT*p) / lambda ================
        // p is symmetric, so gain*xT*p = (p*x)(p*x)T / g: one triangle is calculated and mirrored

        var invLambda = 1.0 / _lambda;
        var trace = 0.0;

        for (int i = 0; i < n; i++)
        {
            var row = i * n;
            for (int j = i; j < n; j++)
            {
                var v = (_p[row + j] - _gains[i] * _gains[j] * invG) * invLambda;
                _p[row + j] = v;
                _p[j * n + i] = v;
            }
            trace += _p[row + i];
        }

        // forgetting without excitation of the whole input space (e.g. a constant or narrowband input) makes the matrix grow exponentially
        // in the directions that are never excited: keep it finite
        if (trace > _maxTrace)
        {
            var scale = _maxTrace / trace;
            for (int i = 0; i < _p.Length; i++)
            {
                _p[i] *= scale;
            }
        }

        // ======================================================================
        // ===================== finally, update weights: =======================
        // ======================================================================

        for (int i = 0; i < n; i++)
        {
            SetWeight(i, (float)(_b[i] + _gains[i] * invG * e));
        }

        return (float)y;
    }
}
