namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// Represents the second-order Volterra NLMS Adaptive filter: a truncated Volterra series, i.e. a linear FIR part and a quadratic part,
/// y(n) = Σ<sub>i</sub> h[i]·x(n−i) + Σ<sub>i≤j</sub> q[i,j]·x(n−i)·x(n−j),
/// so it can learn weakly nonlinear systems (loudspeaker and amplifier distortion, nonlinear channels) that a linear filter cannot.
/// The filter is linear in its coefficients: both blocks are adapted by the NLMS algorithm, each normalized by the power of its own regressor.
/// </summary>
/// <remarks>
/// <para>
/// The linear part has <see cref="AdaptiveFilter.Order"/> taps (<c>Kernel</c>), the quadratic part uses the newest <see cref="QuadraticOrder"/> samples and has
/// QuadraticOrder·(QuadraticOrder + 1)/2 coefficients (<see cref="QuadraticKernel"/>, see <see cref="QuadraticIndex(int, int)"/> for the layout).
/// After an adaptation step the a posteriori error is e·(1 − μ − μ<sub>q</sub>), so the algorithm is stable for μ + μ<sub>q</sub> &lt; 2.
/// </para>
/// <para>
/// The quadratic coefficients are part of the filter output, but the inherited <c>Tf</c> and frequency response describe the linear part only.
/// The quadratic coefficients are not touched by <see cref="AdaptiveFilter.Reset"/> or by <see cref="AdaptiveFilter.Init(float[])"/>, use <see cref="InitQuadratic(ReadOnlySpan{float})"/>.
/// The quadratic regressor is the product of the input samples, so its power depends on the 4th power of the level of the input:
/// for a nearly constant (DC) input the quadratic and linear parts cannot be told apart and the quadratic block is not identifiable.
/// </para>
/// </remarks>
public class VolterraNlmsFilter : AdaptiveFilter
{
    private readonly float _mu;
    private readonly float _quadraticMu;
    private readonly float _eps;
    private readonly float[] _q;

    /// <summary>
    /// Constructs <see cref="VolterraNlmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Number of taps of the linear part</param>
    /// <param name="quadraticOrder">Memory of the quadratic part (number of the newest samples that form the products), from 1 to <paramref name="order"/></param>
    /// <param name="mu">Step size of the linear part</param>
    /// <param name="quadraticMu">Step size of the quadratic part; <paramref name="mu"/> + <paramref name="quadraticMu"/> must be less than 2</param>
    /// <param name="eps">Epsilon, regularizes the normalization of both blocks (added to the power of the regressor of the block)</param>
    public VolterraNlmsFilter(int order, int quadraticOrder, float mu = 0.3f, float quadraticMu = 0.3f, float eps = 1e-3f) : base(order)
    {
        Guard.AgainstNonPositive(quadraticOrder, nameof(quadraticOrder));
        if (quadraticOrder > order)
            throw new ArgumentOutOfRangeException(nameof(quadraticOrder), "quadraticOrder must not exceed the order of the linear part.");
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.Positive(quadraticMu, nameof(quadraticMu));
        if (!(mu + quadraticMu < 2))
            throw new ArgumentOutOfRangeException(nameof(quadraticMu), "mu + quadraticMu must be less than 2.");
        AdaptiveArgument.NonNegative(eps, nameof(eps));
        var terms = AdaptiveArgument.Count((long)quadraticOrder * (quadraticOrder + 1) / 2, nameof(quadraticOrder));

        QuadraticOrder = quadraticOrder;
        _mu = mu;
        _quadraticMu = quadraticMu;
        _eps = eps;
        _q = new float[terms];
    }

    /// <summary>
    /// Gets the memory of the quadratic part (number of the newest samples that form the products).
    /// </summary>
    public int QuadraticOrder { get; }

    /// <summary>
    /// Gets a copy of the coefficients q[i, j] (i ≤ j &lt; <see cref="QuadraticOrder"/>) of the quadratic part, in the order given by <see cref="QuadraticIndex(int, int)"/>.
    /// </summary>
    public float[] QuadraticKernel => (float[])_q.Clone();

    /// <summary>
    /// Gets the position of the coefficient of the product x(n−<paramref name="i"/>)·x(n−<paramref name="j"/>) in <see cref="QuadraticKernel"/> (the order of the indexes does not matter).
    /// The coefficients are stored row by row: (0,0), (0,1), ..., (0,M−1), (1,1), (1,2), ...
    /// </summary>
    /// <param name="i">First lag, from 0 to <see cref="QuadraticOrder"/> − 1</param>
    /// <param name="j">Second lag, from 0 to <see cref="QuadraticOrder"/> − 1</param>
    public int QuadraticIndex(int i, int j)
    {
        if ((uint)i >= (uint)QuadraticOrder)
            throw new ArgumentOutOfRangeException(nameof(i));
        if ((uint)j >= (uint)QuadraticOrder)
            throw new ArgumentOutOfRangeException(nameof(j));

        if (i > j)
        {
            (i, j) = (j, i);
        }
        return i * QuadraticOrder - i * (i - 1) / 2 + (j - i);
    }

    /// <summary>
    /// Inits the coefficients of the quadratic part. The linear part is initialized by <see cref="AdaptiveFilter.Init(float[])"/>.
    /// </summary>
    /// <param name="weights">Coefficients in the order given by <see cref="QuadraticIndex(int, int)"/> (QuadraticOrder·(QuadraticOrder + 1)/2 finite values)</param>
    /// <exception cref="ArgumentException"><paramref name="weights"/> has the wrong number of elements or holds NaN or infinity; the coefficients are not changed then.</exception>
    public void InitQuadratic(ReadOnlySpan<float> weights)
    {
        if (weights.Length != _q.Length)
            throw new ArgumentException("The number of weights must be equal to QuadraticOrder·(QuadraticOrder + 1)/2.", nameof(weights));
        AdaptiveArgument.AllFinite(weights, nameof(weights));

        weights.CopyTo(_q);
    }

    /// <summary>
    /// Processes one sample with the current coefficients and without adapting them.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        var offset = _delayLineOffset;
        var linear = base.Process(sample);

        var m = QuadraticOrder;
        var x = Regressor(offset);

        double quadratic = 0;
        var index = 0;
        for (var i = 0; i < m; i++)
        {
            double inner = 0;
            for (var j = i; j < m; j++)
            {
                inner += (double)_q[index++] * x[j];
            }
            quadratic += inner * x[i];
        }
        return (float)(linear + quadratic);
    }

    /// <summary>
    /// Applies the filter with its current coefficients to the entire <paramref name="signal"/> (no adaptation). The quadratic part is not an FIR filter,
    /// so the samples always go through <see cref="Process(float)"/> and <paramref name="method"/> is ignored.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="method">Ignored</param>
    public override Signal ApplyTo(Signal signal, FilteringMethod method = FilteringMethod.Auto)
        => Signal.FromCopy(ProcessAllSamples(signal.Samples), signal.SamplingRate);

    /// <summary>
    /// Processes one sample of input and desired signals and adapts the coefficients.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override float Process(float input, float desired)
    {
        var y = Push(input, out var offset);
        var e = desired - y;

        if (!float.IsFinite(e))
        {
            return y;
        }

        var m = QuadraticOrder;
        var x = Regressor(offset);

        // power of the linear regressor and of the regressor of the products x(n-i)·x(n-j), i ≤ j: ((Σ x²)² + Σ x⁴) / 2
        double linearPower = 0, sumSquares = 0, sumFourth = 0;
        for (var i = 0; i < x.Length; i++)
        {
            var s = (double)x[i] * x[i];
            linearPower += s;
            if (i < m)
            {
                sumSquares += s;
                sumFourth += s * s;
            }
        }
        var quadraticPower = (sumSquares * sumSquares + sumFourth) / 2;

        var linearNorm = _eps + linearPower;
        if (linearNorm > 0)
        {
            var step = _mu * e / linearNorm;
            for (var i = 0; i < x.Length; i++)
            {
                SetWeight(i, (float)(_b[i] + step * x[i]));
            }
        }

        var quadraticNorm = _eps + quadraticPower;
        if (quadraticNorm > 0)
        {
            var step = _quadraticMu * e / quadraticNorm;
            var index = 0;
            for (var i = 0; i < m; i++)
            {
                for (var j = i; j < m; j++, index++)
                {
                    _q[index] = (float)(_q[index] + step * x[i] * x[j]);
                }
            }
        }

        return y;
    }
}
