namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the least-squares lattice (LSL) Adaptive filter in the joint-process form, the recursive least-squares lattice algorithm with
/// a posteriori errors (Haykin): the lattice computes the exact exponentially weighted least-squares forward and backward predictions of
/// every order, and a ladder of the cross-correlations of the backward errors with the desired signal gives the least-squares estimate of the desired signal.
/// It solves the same problem as <see cref="RlsFilter"/> with the same convergence, but needs only O(stages) operations per sample (RLS: O(order²))
/// and no matrix; the equivalent transversal filter has <c>stages + 1</c> taps.
/// </summary>
/// <remarks>
/// <para>
/// All recursions run in double precision. A regressor of zeros (digital silence, the last <c>stages + 1</c> input samples are zero) is skipped, so a gap leaves the
/// filter untouched, like <see cref="RlsFilter"/>; the prediction error energies are kept above a tiny share (1e-12) of the input energy, so a constant or
/// noise-free sinusoidal input (whose higher-order errors decay below the rounding noise) cannot turn the recursions into NaN.
/// The initial energies <c>initialEnergy</c> play the role of the regularization of RLS and fade with the forgetting.
/// </para>
/// <para>
/// This class is not an <see cref="AdaptiveFilter"/>: the weights of the transversal form are not stored, call <see cref="GetKernel"/> to compute them
/// (O(stages³), for inspection and system identification, not for every sample). It needs the reflection coefficients of the last <c>stages</c> samples,
/// 2 · stages² values of memory.
/// </para>
/// </remarks>
public sealed class LslLatticeFilter
{
    /// <summary>
    /// Floor of the prediction error energies as a share of the energy of the input, and of the conversion factors.
    /// </summary>
    private const double EnergyFloor = 1e-12;
    private const double GammaFloor = 1e-12;

    private readonly int _stages;
    private readonly double _lambda;
    private readonly double _initialEnergy;

    // quantities of the current time n, orders 0 .. stages
    private readonly double[] _f;
    private readonly double[] _b;
    private readonly double[] _forwardEnergy;
    private readonly double[] _backwardEnergy;
    private readonly double[] _gamma;

    // quantities of the previous time n-1
    private readonly double[] _bPrevious;
    private readonly double[] _backwardEnergyPrevious;
    private readonly double[] _gammaPrevious;

    // time-recursive statistics and the coefficients of the current time
    private readonly double[] _correlation;      // Δ_m
    private readonly double[] _reflectionForward;    // Γf,m (m = 1 .. stages stored at m - 1)
    private readonly double[] _reflectionBackward;   // Γb,m
    private readonly double[] _ladderCorrelation;    // ρ_m
    private readonly double[] _ladder;               // κ_m
    private double _energy0;
    private int _zeros;

    // reflection coefficients of the last <stages> samples (ring, slot _head is the newest) for the reconstruction of the transversal weights
    private readonly double[] _historyForward;
    private readonly double[] _historyBackward;
    private int _head;

    /// <summary>
    /// Constructs <see cref="LslLatticeFilter"/> of given number of lattice stages; the equivalent transversal filter has <paramref name="stages"/> + 1 taps.
    /// </summary>
    /// <param name="stages">Number of lattice stages</param>
    /// <param name="lambda">Forgetting factor in (0, 1]</param>
    /// <param name="initialEnergy">Initial value of the prediction error energies (the regularization), a small positive value compared to the energy of the input</param>
    public LslLatticeFilter(int stages, float lambda = 0.99f, float initialEnergy = 1e-2f)
    {
        Guard.AgainstNonPositive(stages, nameof(stages));
        AdaptiveArgument.Positive(lambda, nameof(lambda));
        AdaptiveArgument.InRange(lambda, 0f, 1f, nameof(lambda));
        AdaptiveArgument.Positive(initialEnergy, nameof(initialEnergy));
        var n = AdaptiveArgument.Count(stages + 1L, nameof(stages));
        var historyLength = AdaptiveArgument.Count((long)stages * stages, nameof(stages));

        _stages = stages;
        _lambda = lambda;
        _initialEnergy = initialEnergy;

        _f = new double[n];
        _b = new double[n];
        _forwardEnergy = new double[n];
        _backwardEnergy = new double[n];
        _gamma = new double[n];
        _bPrevious = new double[n];
        _backwardEnergyPrevious = new double[n];
        _gammaPrevious = new double[n];
        _correlation = new double[n];
        _reflectionForward = new double[stages];
        _reflectionBackward = new double[stages];
        _ladderCorrelation = new double[n];
        _ladder = new double[n];
        _historyForward = new double[historyLength];
        _historyBackward = new double[historyLength];
        Reset();
    }

    /// <summary>
    /// Gets the number of lattice stages.
    /// </summary>
    public int Stages => _stages;

    /// <summary>
    /// Gets the number of taps of the equivalent transversal filter.
    /// </summary>
    public int Order => _stages + 1;

    /// <summary>
    /// Restarts the filter from scratch: all statistics, coefficients and the signal history are reset.
    /// </summary>
    public void Reset()
    {
        Array.Clear(_f);
        Array.Clear(_b);
        Array.Clear(_bPrevious);
        Array.Clear(_correlation);
        Array.Clear(_reflectionForward);
        Array.Clear(_reflectionBackward);
        Array.Clear(_ladderCorrelation);
        Array.Clear(_ladder);
        Array.Clear(_historyForward);
        Array.Clear(_historyBackward);
        _head = 0;

        Array.Fill(_forwardEnergy, _initialEnergy);
        Array.Fill(_backwardEnergy, _initialEnergy);
        Array.Fill(_backwardEnergyPrevious, _initialEnergy);
        Array.Fill(_gamma, 1.0);
        Array.Fill(_gammaPrevious, 1.0);
        _energy0 = _initialEnergy;
        _zeros = _stages + 1;       // an empty history is a silent one, the first non-zero sample ends it
    }

    /// <summary>
    /// Gets the weights of the equivalent transversal filter y(n) = Σ w[i] · x(n − i), the least-squares solution after the last processed sample.
    /// </summary>
    public float[] GetKernel()
    {
        // The least-squares predictors of consecutive orders are linked in a time-shifted way:
        //   A_j(n) = A_{j-1}(n) + Γf,j(n) z^-1 C_{j-1}(n-1),   C_j(n) = z^-1 C_{j-1}(n-1) + Γb,j(n) A_{j-1}(n)   (A_0 = C_0 = 1),
        // so C_m(n) needs the coefficients of the m last samples. Before the first samples the coefficients are zero, which is the initial state.
        var m = _stages;
        var older = new double[m + 1][];
        older[0] = [1.0];

        for (var age = m - 1; age >= 0; age--)
        {
            var top = m - age;                              // orders needed at this age: C_0 .. C_top
            var a = new double[top + 1][];
            var c = new double[top + 1][];
            a[0] = [1.0];
            c[0] = [1.0];

            var slot = (_head - age + m) % m;
            for (var j = 1; j <= top; j++)
            {
                var gf = _historyForward[slot * m + j - 1];
                var gb = _historyBackward[slot * m + j - 1];
                var cOlder = older[j - 1];                  // C_{j-1} of the previous sample, j coefficients
                var aSame = a[j - 1];                       // A_{j-1} of this sample, j coefficients

                var aj = new double[j + 1];
                var cj = new double[j + 1];
                for (var k = 0; k <= j; k++)
                {
                    var shifted = k >= 1 ? cOlder[k - 1] : 0;
                    var same = k <= j - 1 ? aSame[k] : 0;
                    aj[k] = same + gf * shifted;
                    cj[k] = shifted + gb * same;
                }

                a[j] = aj;
                c[j] = cj;
            }

            older = c;
        }

        var kernel = new double[m + 1];
        for (var j = 0; j <= m; j++)
        {
            var cj = older[j];
            for (var k = 0; k <= j; k++)
            {
                kernel[k] += _ladder[j] * cj[k];
            }
        }

        return kernel.Select(v => (float)v).ToArray();
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts the coefficients.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    /// <returns>A priori filter output: the estimate of the desired signal by the least-squares solution before this sample; NaN for a sample that is skipped because the input or the desired sample is not finite</returns>
    public float Process(float input, float desired)
    {
        // A non-finite sample would end in every recursive statistic for good (and the filter would return the desired sample itself, as a NaN prediction error counts as zero):
        // the sample is skipped as a whole, the filter goes on as if it had not happened.
        if (!float.IsFinite(input) || !float.IsFinite(desired))
        {
            return float.NaN;
        }

        var m1 = _stages;

        // a regressor of zeros carries no information: forgetting would only make the energies decay
        if (input == 0)
        {
            if (_zeros <= m1)
            {
                _zeros++;
            }
        }
        else
        {
            _zeros = 0;
        }

        if (_zeros > m1)
        {
            return 0f;
        }

        var u = (double)input;
        var d = (double)desired;

        // the a priori output is the a posteriori error of the desired signal converted back (it does not depend on d itself)
        _f[0] = u;
        _b[0] = u;
        _energy0 = _lambda * _energy0 + u * u;
        _forwardEnergy[0] = _energy0;
        _backwardEnergy[0] = _energy0;

        // The error energies of a (nearly) constant or noise-free sinusoidal input decay to zero, below the rounding noise of the energy of the
        // input: their order updates are noise then, and the divisions by them (and by the conversion factors, which lie in (0, 1]) would poison
        // every statistic for good. Keeping them above a tiny share of the input energy has no effect on a signal that excites all stages.
        var floor = EnergyFloor * _energy0;

        for (var m = 1; m <= m1; m++)
        {
            var delta = _lambda * _correlation[m - 1] + _bPrevious[m - 1] * _f[m - 1] / _gammaPrevious[m - 1];
            _correlation[m - 1] = delta;

            var gammaF = -delta / _backwardEnergyPrevious[m - 1];
            var gammaB = -delta / _forwardEnergy[m - 1];
            _reflectionForward[m - 1] = gammaF;
            _reflectionBackward[m - 1] = gammaB;

            _f[m] = _f[m - 1] + gammaF * _bPrevious[m - 1];
            _b[m] = _bPrevious[m - 1] + gammaB * _f[m - 1];

            var deltaSquare = delta * delta;
            _forwardEnergy[m] = Math.Max(_forwardEnergy[m - 1] - deltaSquare / _backwardEnergyPrevious[m - 1], floor);
            _backwardEnergy[m] = Math.Max(_backwardEnergyPrevious[m - 1] - deltaSquare / _forwardEnergy[m - 1], floor);
        }

        // conversion factors of the current time: γ_m = γ_{m-1} − b_{m-1}² / B_{m-1}
        _gamma[0] = 1;
        for (var m = 1; m <= m1 + 1; m++)
        {
            var next = Math.Max(_gamma[m - 1] - _b[m - 1] * _b[m - 1] / _backwardEnergy[m - 1], GammaFloor);
            if (m <= m1)
            {
                _gamma[m] = next;
            }
            else
            {
                _gammaFinal = next;
            }
        }

        // joint process estimation with the a posteriori errors
        var e = d;
        for (var m = 0; m <= m1; m++)
        {
            _ladderCorrelation[m] = _lambda * _ladderCorrelation[m] + _b[m] * e / _gamma[m];
            _ladder[m] = _ladderCorrelation[m] / _backwardEnergy[m];
            e -= _ladder[m] * _b[m];
        }

        // e is the a posteriori error of the (stages + 1)-tap filter: the a priori error is e / γ
        var priorError = _gammaFinal > 0 ? e / _gammaFinal : 0.0;

        // the reflection coefficients of this sample enter the history of the last <stages> samples
        _head = (_head + 1) % m1;
        Array.Copy(_reflectionForward, 0, _historyForward, _head * m1, m1);
        Array.Copy(_reflectionBackward, 0, _historyBackward, _head * m1, m1);

        // the quantities of this time become the previous ones
        Array.Copy(_b, _bPrevious, _b.Length);
        Array.Copy(_backwardEnergy, _backwardEnergyPrevious, _backwardEnergy.Length);
        Array.Copy(_gamma, _gammaPrevious, _gamma.Length);

        return (float)(d - priorError);
    }

    private double _gammaFinal = 1;

    /// <summary>
    /// Processes one sample of input and desired signals, adapts the coefficients and returns the error as well.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    /// <param name="error">A priori error: <paramref name="desired"/> minus the returned output</param>
    /// <returns>A priori filter output</returns>
    public float Process(float input, float desired, out float error)
    {
        var y = Process(input, desired);
        error = desired - y;
        return y;
    }
}
