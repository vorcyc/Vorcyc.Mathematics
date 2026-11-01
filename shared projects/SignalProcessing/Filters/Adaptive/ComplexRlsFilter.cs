namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

using Vorcyc.Mathematics.Numerics;

/// <summary>
/// Represents the complex RLS Adaptive filter (Recursive-Least-Squares algorithm with exponential forgetting for complex-valued signals).
/// The inverse correlation matrix is Hermitian; it is kept in double precision and updated in O(order²) per sample.
/// A regressor of zeros (digital silence) is skipped and the growth of the matrix is bounded, as in <see cref="RlsFilter"/>.
/// </summary>
public class ComplexRlsFilter : ComplexAdaptiveFilter
{
    /// <summary>
    /// How many times the trace of the inverse corr matrix may exceed its initial value.
    /// </summary>
    private const double MaxGrowth = 1e16;

    private readonly double _lambda;
    private readonly double _initCorrMatrix;
    private readonly double _maxTrace;

    /// <summary>
    /// Inverse corr matrix (row-major, Hermitian): real and imaginary parts.
    /// </summary>
    private readonly double[] _pRe;
    private readonly double[] _pIm;

    /// <summary>
    /// Regressor in double precision.
    /// </summary>
    private readonly double[] _xRe;
    private readonly double[] _xIm;

    /// <summary>
    /// Product of the inverse corr matrix and the conjugated regressor.
    /// </summary>
    private readonly double[] _gRe;
    private readonly double[] _gIm;

    /// <summary>
    /// Constructs <see cref="ComplexRlsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order (number of complex taps)</param>
    /// <param name="lambda">Lambda, forgetting factor in (0, 1]</param>
    /// <param name="initCorrMatrix">Value to initialize the diagonal of the inverse corr matrix</param>
    public ComplexRlsFilter(int order, float lambda = 0.99f, float initCorrMatrix = 1e2f) : base(order)
    {
        AdaptiveArgument.Positive(lambda, nameof(lambda));
        AdaptiveArgument.InRange(lambda, 0f, 1f, nameof(lambda));
        AdaptiveArgument.Positive(initCorrMatrix, nameof(initCorrMatrix));
        var matrixLength = AdaptiveArgument.Count((long)order * order, nameof(order));

        _lambda = lambda;
        _initCorrMatrix = initCorrMatrix;
        _maxTrace = order * _initCorrMatrix * MaxGrowth;

        _pRe = new double[matrixLength];
        _pIm = new double[matrixLength];
        _xRe = new double[order];
        _xIm = new double[order];
        _gRe = new double[order];
        _gIm = new double[order];
        ResetState();
    }

    /// <summary>
    /// Restores the inverse corr matrix to its initial value.
    /// </summary>
    protected override void ResetState()
    {
        Array.Clear(_pRe);
        Array.Clear(_pIm);
        for (var i = 0; i < Order; i++)
        {
            _pRe[i * Order + i] = _initCorrMatrix;
        }
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts the taps.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override ComplexFp32 Process(ComplexFp32 input, ComplexFp32 desired)
    {
        var n = Order;

        var offset = Push(input);
        var x = Regressor(offset);

        double yRe = 0, yIm = 0;
        var silent = true;
        for (var i = 0; i < n; i++)
        {
            double xr = x[i].Real, xi = x[i].Imaginary;
            _xRe[i] = xr;
            _xIm[i] = xi;
            yRe += _h[i].Real * xr - _h[i].Imaginary * xi;
            yIm += _h[i].Real * xi + _h[i].Imaginary * xr;
            silent &= xr == 0 && xi == 0;
        }

        var y = new ComplexFp32((float)yRe, (float)yIm);

        // a regressor of zeros carries no information: forgetting would only make the inverse corr matrix grow
        if (silent)
        {
            return y;
        }

        var eRe = desired.Real - yRe;
        var eIm = desired.Imaginary - yIm;

        // a non-finite sample would end in every element of the inverse corr matrix for good: the sample is skipped
        if (!double.IsFinite(eRe) || !double.IsFinite(eIm))
        {
            return y;
        }

        // The taps are the conjugate of the weights of the textbook form y = wH x, so the regressor of the recursion is phi = conj(x)
        // and the matrix is the conjugate of the textbook one. The gain is  g / (lambda + phiH g), g = P·phi.

        for (var i = 0; i < n; i++)
        {
            double sr = 0, si = 0;
            var row = i * n;
            for (var k = 0; k < n; k++)
            {
                double pr = _pRe[row + k], pi = _pIm[row + k];
                double fr = _xRe[k], fi = -_xIm[k];
                sr += pr * fr - pi * fi;
                si += pr * fi + pi * fr;
            }
            _gRe[i] = sr;
            _gIm[i] = si;
        }

        // phiH·g is real because the matrix is Hermitian
        var den = _lambda;
        for (var k = 0; k < n; k++)
        {
            den += _xRe[k] * _gRe[k] - _xIm[k] * _gIm[k];
        }

        var invDen = 1.0 / den;

        // P = (P - g gH / den) / lambda: the upper triangle is calculated and mirrored (conjugated)
        var invLambda = 1.0 / _lambda;
        var trace = 0.0;

        for (var i = 0; i < n; i++)
        {
            var row = i * n;
            for (var j = i; j < n; j++)
            {
                // g[i] · conj(g[j])
                var ar = _gRe[i] * _gRe[j] + _gIm[i] * _gIm[j];
                var ai = _gIm[i] * _gRe[j] - _gRe[i] * _gIm[j];

                var vr = (_pRe[row + j] - ar * invDen) * invLambda;
                var vi = j == i ? 0 : (_pIm[row + j] - ai * invDen) * invLambda;

                _pRe[row + j] = vr;
                _pIm[row + j] = vi;
                _pRe[j * n + i] = vr;
                _pIm[j * n + i] = -vi;
            }
            trace += _pRe[row + i];
        }

        // forgetting without excitation of the whole input space makes the matrix grow exponentially in the directions that are never excited: keep it finite
        if (trace > _maxTrace)
        {
            var scale = _maxTrace / trace;
            for (var i = 0; i < _pRe.Length; i++)
            {
                _pRe[i] *= scale;
                _pIm[i] *= scale;
            }
        }

        for (var i = 0; i < n; i++)
        {
            double kr = _gRe[i] * invDen, ki = _gIm[i] * invDen;
            _h[i].Real = (float)(_h[i].Real + kr * eRe - ki * eIm);
            _h[i].Imaginary = (float)(_h[i].Imaginary + kr * eIm + ki * eRe);
        }

        return y;
    }
}
