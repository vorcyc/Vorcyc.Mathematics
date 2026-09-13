using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// Provides extension methods for online filters.
/// </summary>
public static class IFilterExtensions
{
    /// <summary>
    /// Filters data frame-wise.
    /// </summary>
    public static void Process(this IOnlineFilter filter,
                               float[] input,
                               float[] output,
                               int count = 0,
                               int inputPos = 0,
                               int outputPos = 0)
    {
        if (count <= 0)
        {
            count = input.Length;
        }

        var endPos = inputPos + count;

        for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
        {
            output[m] = filter.Process(input[n]);
        }
    }

    /// <summary>
    /// Filters entire <paramref name="signal"/> by processing each signal sample in a loop.
    /// </summary>
    public static Signal FilterOnline(this IOnlineFilter filter, Signal signal)
    {
        var output = new float[signal.Length];
        var samples = signal.Samples;

        for (var i = 0; i < samples.Length; i++)
        {
            output[i] = filter.Process(samples[i]);
        }

        return Signal.FromCopy(output, signal.SamplingRate);
    }

    /// <summary>
    /// In-place online filtering.
    /// </summary>
    public static void FilterOnline_Inplace(this IOnlineFilter filter, Signal signal)
    {
        var samples = signal.Samples;
        for (var i = 0; i < signal.Length; i++)
        {
            samples[i] = filter.Process(samples[i]);
        }

        signal.NotifySamplesModified();
    }

    /// <summary>
    /// Calculates extra gain for filtering so that frequency response is normalized onto [0..1] range.
    /// </summary>
    public static float EstimateGain(this IOnlineFilter filter, int fftSize = 512)
    {
        var unit = Signal.Unit(fftSize);

        var unitSamples = unit.Samples;
        var response = new float[fftSize];
        for (var i = 0; i < fftSize; i++)
        {
            response[i] = filter.Process(unitSamples[i]);
        }

        var spectrum = new float[fftSize / 2 + 1];
        var fft = new RealFft(fftSize);
        fft.MagnitudeSpectrum(response, spectrum);

        return 1 / spectrum.Max(s => Math.Abs(s));
    }

    /// <summary>
    /// Filters entire <paramref name="signal"/> with extra <paramref name="gain"/>.
    /// </summary>
    public static Signal ApplyTo(this IOnlineFilter filter, Signal signal, float gain)
    {
        var samples = signal.Samples;
        var output = new float[signal.Length];
        for (var i = 0; i < output.Length; i++)
        {
            output[i] = gain * filter.Process(samples[i]);
        }

        return Signal.FromCopy(output, signal.SamplingRate);
    }

    /// <summary>
    /// Processes one sample with extra <paramref name="gain"/>.
    /// </summary>
    public static float Process(this IOnlineFilter filter, float sample, float gain)
        => gain * filter.Process(sample);

    /// <summary>Filters a frame through a generic online filter.</summary>
    public static void Process<T>(this IOnlineFilter<T> filter,
                                  ReadOnlySpan<T> input,
                                  Span<T> output)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (output.Length < input.Length)
            throw new ArgumentException("Output is shorter than input.", nameof(output));
        for (int i = 0; i < input.Length; i++)
            output[i] = filter.Process(input[i]);
    }

    /// <summary>Filters an entire buffer and returns a new array.</summary>
    public static T[] FilterOnline<T>(this IOnlineFilter<T> filter, ReadOnlySpan<T> samples)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        var output = new T[samples.Length];
        for (int i = 0; i < samples.Length; i++)
            output[i] = filter.Process(samples[i]);
        return output;
    }

#if DEBUG
    /// <summary>
    /// Offline filtering in time domain frame-by-frame (for tests).
    /// </summary>
    public static Signal ProcessChunks(this IOnlineFilter filter, Signal signal, int frameSize = 4096)
    {
        var input = new float[signal.Length];
        signal.Samples.CopyTo(input);
        var output = new float[signal.Length];

        var i = 0;
        for (; i + frameSize < signal.Length; i += frameSize)
        {
            filter.Process(input, output, frameSize, i, i);
        }

        filter.Process(input, output, signal.Length - i, i, i);

        return Signal.FromCopy(output, signal.SamplingRate);
    }
#endif
}
