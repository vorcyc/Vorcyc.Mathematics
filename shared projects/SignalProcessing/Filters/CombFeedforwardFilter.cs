using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Filters;

/// <summary>
/// Represents feedforward comb filter:
/// <code>
///     y[n] = b0 * x[n] + bm * x[n - m]
/// </code>
/// </summary>
public class CombFeedforwardFilter : FirFilter
{
    /// <summary>
    /// Delay (m).
    /// </summary>
    private readonly int _delay;

    /// <summary>
    /// Constructs <see cref="CombFeedforwardFilter"/>.
    /// </summary>
    /// <param name="m">Delay</param>
    /// <param name="b0">Coefficient b0</param>
    /// <param name="bm">Coefficient bm</param>
    /// <param name="normalize"></param>
    public CombFeedforwardFilter(int m, float b0 = 1f, float bm = 0.5f, bool normalize = true)
        : base(MakeKernel(m, b0, bm, normalize))
    {
        _delay = m;
    }

    /// <summary>
    /// Generates filter kernel.
    /// </summary>
    /// <param name="m">Delay</param>
    /// <param name="b0">Coefficient b0</param>
    /// <param name="bm">Coefficient bm</param>
    /// <param name="normalize">Normalize freq response to unit gain</param>
    private static float[] MakeKernel(int m, float b0, float bm, bool normalize)
    {
        var kernel = new float[m + 1];
        kernel[0] = b0;
        kernel[m] = bm;

        if (normalize)
        {
            var sum = (b0 + bm);
            kernel[0] /= sum;
            kernel[m] /= sum;
        }

        return kernel;
    }

    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public override float Process(float sample)
    {
        var b0 = _b[0];
        var bm = _b[_delay];

        var output = b0 * sample + bm * _delayLine[_delayLineOffset];

        _delayLine[_delayLineOffset] = sample;

        if (--_delayLineOffset < 1)
        {
            _delayLineOffset = _kernelSize - 1;
        }

        return output;
    }

    /// <summary>
    /// Applies filter to entire <paramref name="signal"/> and returns new filtered signal.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="method">Filtering method</param>
    public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
    {
        if (method != FilteringMethod.Auto)
        {
            return base.ApplyTo(signal, method);
        }

        var input = signal.Samples;
        var output = new float[input.Length + _kernelSize - 1];

        var b0 = _b[0];
        var bm = _b[_delay];

        int i = 0, j = 0;

        for (; i < _delay; i++)
        {
            output[i] = b0 * input[i];
        }
        for (; i < signal.SampleCount; i++, j++)
        {
            output[i] = b0 * input[i] + bm * input[j];
        }
        for (; i < output.Length; i++, j++)
        {
            output[i] = bm * input[j];
        }

        return new DiscreteSignal(signal.SamplingRate, output);
    }

    /// <summary>
    /// Changes coefficients (preserving the state of the filter).
    /// </summary>
    /// <param name="b0">Coefficient b0</param>
    /// <param name="bm">Coefficient bm</param>
    public void Change(float b0, float bm)
    {
        _b[0] = b0;
        _b[_delay] = bm;
    }
}
