namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// Represents the online-centered NLMS (OCNLMS) Adaptive filter (Cejnek and Vrba): the regressor and the target are centered by their running
/// means over the last <c>memory</c> samples before the NLMS update, so the filter becomes independent of the (possibly drifting)
/// non-zero mean of the signals:
/// y = wT (x − m<sub>x</sub>) + m<sub>d</sub>, w += mu · e · (x − m<sub>x</sub>) / (eps + ‖x − m<sub>x</sub>‖²).
/// </summary>
/// <remarks>
/// <para>
/// m<sub>x</sub> is the mean of the <c>memory</c> regressors and m<sub>d</sub> the mean of the <c>memory</c> desired samples that precede the current sample
/// (both over the same window, which keeps m<sub>d</sub> = wT m<sub>x</sub> for a linear system); as long as less samples have been seen the means are taken over the
/// available ones and are zero for the very first sample. <c>Kernel</c> is the weight vector w; the output of the filter for a given regressor
/// additionally contains the mean terms.
/// </para>
/// <para>
/// The running means of a delay-line regressor are the moving averages of the input at the lags 0 … order − 1, so they cost O(order) per sample.
/// </para>
/// </remarks>
public class OcNlmsFilter : AdaptiveFilter
{
    private readonly int _memory;
    private readonly float _mu;
    private readonly float _eps;

    /// <summary>
    /// The last <c>memory</c> input samples (ring buffer) and their running sum.
    /// </summary>
    private readonly float[] _inputs;
    private int _inputsPosition;
    private int _inputsCount;
    private double _inputsSum;

    /// <summary>
    /// Moving sums S(n-1), S(n-2), ..., S(n-order) (newest first) of the last <c>memory</c> inputs, stored twice for a contiguous view:
    /// at the time n they belong to the samples before the current one.
    /// </summary>
    private readonly double[] _sums;
    private int _sumsOffset;

    /// <summary>
    /// Mean of the regressor (the mean of the regressors that precede the current one) of the sample that is processed.
    /// </summary>
    private readonly double[] _mean;

    /// <summary>
    /// The last <c>memory</c> desired samples (ring buffer) and their running sum.
    /// </summary>
    private readonly float[] _targets;
    private int _targetsPosition;
    private int _targetsCount;
    private double _targetsSum;

    /// <summary>
    /// Constructs <see cref="OcNlmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="mu">Step size, stable for 0 &lt; mu &lt; 2 regardless of the input scale</param>
    /// <param name="eps">Epsilon, regularizes the normalization (added to the power of the centered regressor)</param>
    /// <param name="memory">Number of the preceding samples used for the centering, at least 2; a longer memory estimates a constant offset more accurately, a shorter one follows a drifting offset faster</param>
    public OcNlmsFilter(int order, float mu = 0.1f, float eps = 1, int memory = 100) : base(order)
    {
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.NonNegative(eps, nameof(eps));
        if (memory < 2)
            throw new ArgumentOutOfRangeException(nameof(memory), "memory must be at least 2.");

        _mu = mu;
        _eps = eps;
        _memory = memory;
        _inputs = new float[memory];
        _targets = new float[memory];
        _sums = new double[2 * _kernelSize];
        _sumsOffset = _kernelSize - 1;
        _mean = new double[_kernelSize];
    }

    /// <summary>
    /// Gets the number of the preceding samples used for the centering.
    /// </summary>
    public int Memory => _memory;

    /// <summary>
    /// Clears the running means.
    /// </summary>
    protected override void ResetHistory()
    {
        Array.Clear(_inputs);
        _inputsPosition = 0;
        _inputsCount = 0;
        _inputsSum = 0;

        Array.Clear(_sums);
        _sumsOffset = _kernelSize - 1;
        Array.Clear(_mean);

        Array.Clear(_targets);
        _targetsPosition = 0;
        _targetsCount = 0;
        _targetsSum = 0;
    }

    /// <summary>
    /// Processes one sample with the current weights and without adapting them; the running mean of the input follows the sample,
    /// the mean of the target stays as it was.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        var uncentered = base.Process(sample);

        // mean of the regressor at lag i: S(n-1-i) / count, where count is the number of the preceding samples inside the window
        var weighted = 0.0;
        if (_inputsCount > 0)
        {
            var scale = 1.0 / _inputsCount;
            for (var i = 0; i < _kernelSize; i++)
            {
                var m = _sums[_sumsOffset + i] * scale;
                _mean[i] = m;
                weighted += _b[i] * m;
            }
        }

        // the sample enters the running sum of the last <memory> inputs, S(n), and then the delay line of the sums;
        // a non-finite sample would never leave the sum again (NaN - NaN is NaN) and zero would bias every mean that holds it by the offset of the signal / memory,
        // so the mean of the preceding inputs stands for it (the signal has to have a large offset for this filter to be needed)
        var counted = float.IsFinite(sample) ? sample : _inputsCount > 0 ? (float)(_inputsSum / _inputsCount) : 0f;
        if (_inputsCount == _memory)
        {
            _inputsSum -= _inputs[_inputsPosition];
        }
        else
        {
            _inputsCount++;
        }
        _inputs[_inputsPosition] = counted;
        _inputsSum += counted;
        if (++_inputsPosition == _memory)
        {
            _inputsPosition = 0;
        }

        if (--_sumsOffset < 0)
        {
            _sumsOffset = _kernelSize - 1;
        }
        _sums[_sumsOffset] = _sums[_sumsOffset + _kernelSize] = _inputsSum;

        return (float)(uncentered - weighted + MeanTarget);
    }

    private double MeanTarget => _targetsCount == 0 ? 0 : _targetsSum / _targetsCount;

    /// <summary>
    /// Applies the filter with its current weights to the entire <paramref name="signal"/> (no adaptation). The mean terms of the filter
    /// depend on the history of the signal, so the samples always go through <see cref="Process(float)"/> and
    /// <paramref name="method"/> is ignored.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="method">Ignored</param>
    public override Signal ApplyTo(Signal signal, FilteringMethod method = FilteringMethod.Auto)
        => Signal.FromCopy(ProcessAllSamples(signal.Samples), signal.SamplingRate);

    /// <summary>
    /// Processes one sample of input and desired signals and adapts filter coefficients.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override float Process(float input, float desired)
    {
        var y = Push(input, out var offset);

        var e = desired - y;

        // a non-finite error (the input or the desired sample) must not reach the weights
        if (float.IsFinite(e))
        {
            var x = Regressor(offset);

            var power = 0.0;
            for (var i = 0; i < x.Length; i++)
            {
                var c = x[i] - _mean[i];
                power += c * c;
            }

            // with eps = 0 a regressor without any variation would give 0 / 0
            var denominator = _eps + power;
            if (denominator > 0)
            {
                var step = _mu * e / denominator;
                for (var i = 0; i < x.Length; i++)
                {
                    SetWeight(i, (float)(_b[i] + step * (x[i] - _mean[i])));
                }
            }
        }

        // the target enters the mean after it has been used; a non-finite one would never leave the running sum again, it is not counted
        if (!float.IsFinite(desired))
        {
            return y;
        }

        if (_targetsCount == _memory)
        {
            _targetsSum -= _targets[_targetsPosition];
        }
        else
        {
            _targetsCount++;
        }
        _targets[_targetsPosition] = desired;
        _targetsSum += desired;
        if (++_targetsPosition == _memory)
        {
            _targetsPosition = 0;
        }

        return y;
    }
}
