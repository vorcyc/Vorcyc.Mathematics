namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the gradient adaptive lattice (GAL) Adaptive filter in the lattice-ladder form (joint-process estimator):
/// a lattice of <c>stages</c> stages decorrelates the input into the backward prediction errors, and an
/// adaptive ladder combines them to estimate the desired signal. The reflection coefficients and the ladder coefficients are
/// adapted by normalized gradient steps. Because the backward errors are (nearly) uncorrelated, the convergence does not depend on the
/// eigenvalue spread of the input, so it converges much faster than <see cref="LmsFilter"/> for colored input at O(stages) cost per sample.
/// The noise of the gradient steps of the reflection coefficients limits the accuracy: for very clean (high SNR) system identification
/// prefer <see cref="RlsFilter"/> or <see cref="QrdRlsFilter"/>.
/// </summary>
public sealed class GalLatticeFilter
{
    private readonly int _stages;
    private readonly float _muLattice;
    private readonly float _muLadder;
    private readonly float _beta;
    private readonly float _eps;

    private readonly float[] _reflection;
    private readonly float[] _ladder;
    private readonly float[] _forward;
    private readonly float[] _backward;
    private readonly float[] _backwardPrevious;
    private readonly float[] _latticeEnergy;
    private readonly float[] _ladderEnergy;

    /// <summary>
    /// beta^n: the energy estimates start from zero, so they are divided by 1 - beta^n to be unbiased from the very first sample.
    /// </summary>
    private float _betaPower = 1;

    /// <summary>
    /// Reflection coefficients stay inside (-limit, limit) to keep the lattice stable.
    /// </summary>
    private const float ReflectionLimit = 0.9999f;

    /// <summary>
    /// Constructs <see cref="GalLatticeFilter"/> of given number of lattice stages; the equivalent transversal filter has <paramref name="stages"/> + 1 taps.
    /// </summary>
    /// <param name="stages">Number of lattice stages</param>
    /// <param name="muLattice">Step size of the reflection coefficients, in (0, 1). The noise of the coefficients in the steady state grows with its square root, so keep it small (0.0005 .. 0.01)</param>
    /// <param name="muLadder">Step size of the ladder coefficients with the same meaning as for <see cref="NlmsFilter"/>: stable for 0 &lt; muLadder &lt; 2</param>
    /// <param name="beta">Forgetting factor of the energy estimates, in [0, 0.999999]</param>
    /// <param name="eps">Regularization of the normalization; keep it small compared to the expected power of the input</param>
    public GalLatticeFilter(int stages, float muLattice = 0.001f, float muLadder = 0.3f, float beta = 0.99f, float eps = 1e-6f)
    {
        Guard.AgainstNonPositive(stages, nameof(stages));
        AdaptiveArgument.Positive(muLattice, nameof(muLattice));
        if (muLattice >= 1)
            throw new ArgumentOutOfRangeException(nameof(muLattice), "muLattice must be in (0, 1).");
        AdaptiveArgument.Positive(muLadder, nameof(muLadder));
        AdaptiveArgument.InRange(beta, 0f, 0.999999f, nameof(beta));
        AdaptiveArgument.NonNegative(eps, nameof(eps));
        var taps = AdaptiveArgument.Count(stages + 1L, nameof(stages));

        _stages = stages;
        _muLattice = muLattice;
        _muLadder = muLadder;
        _beta = beta;
        _eps = eps;

        _reflection = new float[stages];
        _ladder = new float[taps];
        _forward = new float[taps];
        _backward = new float[taps];
        _backwardPrevious = new float[taps];
        _latticeEnergy = new float[stages];
        _ladderEnergy = new float[taps];
    }

    /// <summary>
    /// Gets the number of lattice stages.
    /// </summary>
    public int Stages => _stages;

    /// <summary>
    /// Gets a copy of the reflection coefficients of the lattice (one per stage).
    /// </summary>
    public float[] ReflectionCoefficients => (float[])_reflection.Clone();

    /// <summary>
    /// Gets a copy of the ladder coefficients (one per stage plus one for the input).
    /// </summary>
    public float[] LadderCoefficients => (float[])_ladder.Clone();

    /// <summary>
    /// Restarts the filter: all coefficients, energy estimates and the signal history are zero again.
    /// </summary>
    public void Reset()
    {
        Array.Clear(_reflection);
        Array.Clear(_ladder);
        Array.Clear(_forward);
        Array.Clear(_backward);
        Array.Clear(_backwardPrevious);
        Array.Clear(_latticeEnergy);
        Array.Clear(_ladderEnergy);
        _betaPower = 1;
    }

    /// <summary>
    /// Processes one sample of input and desired signals and adapts the coefficients.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    /// <returns>A priori filter output (estimate of the desired signal); NaN for a sample that is skipped because the input or the desired sample is not finite</returns>
    public float Process(float input, float desired)
    {
        // A non-finite sample would end in the lattice and in the energy estimates for good: the sample is skipped as a whole,
        // the filter goes on as if it had not happened.
        if (!float.IsFinite(input) || !float.IsFinite(desired))
        {
            return float.NaN;
        }

        _forward[0] = input;
        _backward[0] = input;

        for (var m = 1; m <= _stages; m++)
        {
            var k = _reflection[m - 1];
            _forward[m] = _forward[m - 1] - k * _backwardPrevious[m - 1];
            _backward[m] = _backwardPrevious[m - 1] - k * _forward[m - 1];
        }

        var y = 0f;
        for (var m = 0; m <= _stages; m++)
        {
            y += _ladder[m] * _backward[m];
        }

        var e = desired - y;

        _betaPower *= _beta;
        var unbias = 1 / (1 - _betaPower);

        for (var m = 1; m <= _stages; m++)
        {
            var energy = _beta * _latticeEnergy[m - 1] +
                         (1 - _beta) * (_forward[m - 1] * _forward[m - 1] + _backwardPrevious[m - 1] * _backwardPrevious[m - 1]);
            _latticeEnergy[m - 1] = energy;

            var gradient = _forward[m] * _backwardPrevious[m - 1] + _backward[m] * _forward[m - 1];
            var denominator = _eps + energy * unbias;
            if (denominator > 0)
            {
                var k = _reflection[m - 1] + _muLattice * gradient / denominator;
                _reflection[m - 1] = Math.Clamp(k, -ReflectionLimit, ReflectionLimit);
            }
        }

        // every ladder coefficient is normalized by the energy of its own (uncorrelated) input, so the step is shared between the stages
        var ladderStep = _muLadder / (_stages + 1);
        for (var m = 0; m <= _stages; m++)
        {
            var energy = _beta * _ladderEnergy[m] + (1 - _beta) * _backward[m] * _backward[m];
            _ladderEnergy[m] = energy;

            var denominator = _eps + energy * unbias;
            if (denominator > 0)
            {
                _ladder[m] += ladderStep * e * _backward[m] / denominator;
            }
        }

        Array.Copy(_backward, _backwardPrevious, _backward.Length);

        return y;
    }
}
