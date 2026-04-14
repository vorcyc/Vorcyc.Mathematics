using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Vorcyc.Mathematics.SignalProcessing.Operations;

/// <summary>
/// Represents signal resampler (sampling rate converter).
/// </summary>
public class Resampler
{
    public int MinResamplingFilterOrder { get; set; } = 101;

    public Signal Interpolate(Signal signal, int factor, FirFilter? filter = null)
    {
        if (factor == 1)
        {
            return signal.Clone();
        }

        var output = new float[signal.Length * factor];
        var pos = 0;
        for (var i = 0; i < signal.Length; i++)
        {
            output[pos] = factor * signal[i];
            pos += factor;
        }

        var lpFilter = filter;
        if (filter is null)
        {
            var filterSize = factor > MinResamplingFilterOrder / 2 ? 2 * factor + 1 : MinResamplingFilterOrder;
            lpFilter = new FirFilter(DesignFilter.FirWinLp(filterSize, 0.5f / factor));
        }

        return lpFilter.ApplyTo(Signal.FromCopy(output, signal.SamplingRate * factor));
    }

    public Signal Decimate(Signal signal, int factor, FirFilter? filter = null)
    {
        if (factor == 1)
        {
            return signal.Clone();
        }

        var filterSize = factor > MinResamplingFilterOrder / 2 ? 2 * factor + 1 : MinResamplingFilterOrder;

        if (filter is null)
        {
            var lpFilter = new FirFilter(DesignFilter.FirWinLp(filterSize, 0.5f / factor));
            signal = lpFilter.ApplyTo(signal);
        }

        var output = new float[signal.Length / factor];
        var pos = 0;
        for (var i = 0; i < output.Length; i++)
        {
            output[i] = signal[pos];
            pos += factor;
        }

        return Signal.FromCopy(output, signal.SamplingRate / factor);
    }

    public Signal Resample(Signal signal, float newSamplingRate, FirFilter? filter = null, int order = 15)
    {
        if (MathF.Abs(signal.SamplingRate - newSamplingRate) < 1e-6f)
        {
            return signal.Clone();
        }

        var g = newSamplingRate / signal.SamplingRate;
        ReadOnlySpan<float> input = signal.Samples;
        var output = new float[(int)(input.Length * g)];

        if (g < 1 && filter is null)
        {
            filter = new FirFilter(DesignFilter.FirWinLp(MinResamplingFilterOrder, g / 2));
            input = filter.ApplyTo(signal).Samples;
        }

        var step = 1 / g;
        for (var n = 0; n < output.Length; n++)
        {
            var x = n * step;
            for (var i = -order; i < order; i++)
            {
                var j = (int)Math.Floor(x) - i;
                if (j < 0 || j >= input.Length)
                {
                    continue;
                }

                var t = x - j;
                var w = 0.5f * (1.0f + MathF.Cos(t / order * ConstantsFp32.PI));
                var sinc = TrigonometryHelper.Sinc(t);
                output[n] += w * sinc * input[j];
            }
        }

        return Signal.FromCopy(output, newSamplingRate);
    }

    public Signal ResampleUpDown(Signal signal, int up, int down, FirFilter? filter = null)
    {
        if (up == down)
        {
            return signal.Clone();
        }

        var newSamplingRate = signal.SamplingRate * up / down;

        if (up > 20 && down > 20)
        {
            return Resample(signal, newSamplingRate, filter);
        }

        var output = new float[signal.Length * up];
        var pos = 0;
        for (var i = 0; i < signal.Length; i++)
        {
            output[pos] = up * signal[i];
            pos += up;
        }

        var lpFilter = filter;
        if (filter is null)
        {
            var factor = Math.Max(up, down);
            var filterSize = factor > MinResamplingFilterOrder / 2 ? 8 * factor + 1 : MinResamplingFilterOrder;
            lpFilter = new FirFilter(DesignFilter.FirWinLp(filterSize, 0.5f / factor));
        }

        var upsampled = lpFilter.ApplyTo(Signal.FromCopy(output, signal.SamplingRate * up));

        output = new float[upsampled.Length / down];
        pos = 0;
        for (var i = 0; i < output.Length; i++)
        {
            output[i] = upsampled[pos];
            pos += down;
        }

        return Signal.FromCopy(output, newSamplingRate);
    }
}
