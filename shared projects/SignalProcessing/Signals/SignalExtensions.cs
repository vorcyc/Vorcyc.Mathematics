namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// Extension methods for <see cref="Signal"/> (DSP operations migrated from legacy <see cref="DiscreteSignal"/> helpers).
/// </summary>
public static class SignalExtensions
{
    /// <summary>
    /// Creates a new signal from the first <paramref name="count"/> samples.
    /// </summary>
    public static Signal First(this Signal signal, int count)
    {
        Guard.AgainstNonPositive(count, "Number of samples");
        Guard.AgainstExceedance(count, signal.Length, "Number of samples", "signal length");
        return signal.CloneRange(0, count);
    }

    /// <summary>
    /// Creates a new signal from the last <paramref name="count"/> samples.
    /// </summary>
    public static Signal Last(this Signal signal, int count)
    {
        Guard.AgainstNonPositive(count, "Number of samples");
        Guard.AgainstExceedance(count, signal.Length, "Number of samples", "signal length");
        return signal.CloneRange(signal.Length - count, count);
    }

    /// <summary>
    /// Amplifies signal samples in-place.
    /// </summary>
    public static void Amplify(this Signal signal, float coeff)
    {
        var samples = signal.Samples;
        for (var i = 0; i < signal.Length; i++)
        {
            samples[i] *= coeff;
        }

        signal.NotifySamplesModified();
    }

    /// <summary>
    /// Attenuates signal samples in-place.
    /// </summary>
    public static void Attenuate(this Signal signal, float coeff)
    {
        Guard.AgainstNonPositive(coeff, "Attenuation coefficient");
        signal.Amplify(1 / coeff);
    }

    /// <summary>
    /// Reverses signal samples in-place.
    /// </summary>
    public static void Reverse(this Signal signal)
    {
        var samples = signal.Samples;
        for (int i = 0, j = signal.Length - 1; i < signal.Length / 2; i++, j--)
        {
            var tmp = samples[i];
            samples[i] = samples[j];
            samples[j] = tmp;
        }

        signal.NotifySamplesModified();
    }

    /// <summary>
    /// Normalizes signal by its max absolute value (to range [-1, 1]).
    /// </summary>
    public static void NormalizeMax(this Signal signal, int bitsPerSample = 0)
    {
        var samples = signal.Samples;
        var max = 0f;
        for (var i = 0; i < signal.Length; i++)
        {
            var abs = MathF.Abs(samples[i]);
            if (abs > max)
            {
                max = abs;
            }
        }

        if (max < 1e-10f)
        {
            return;
        }

        var norm = 1 / max;
        if (bitsPerSample > 0)
        {
            norm *= (float)(1 - 1 / Math.Pow(2, bitsPerSample));
        }

        signal.Amplify(norm);
    }

    /// <summary>
    /// Creates a delayed copy by shifting right (positive delay) or left (negative delay).
    /// </summary>
    public static Signal Delay(this Signal signal, int delay)
    {
        var length = signal.Length;

        if (delay <= 0)
        {
            delay = -delay;
            Guard.AgainstInvalidRange(delay, length, "Delay", "signal length");

            var output = new float[length - delay];
            signal.Samples.Slice(delay, length - delay).CopyTo(output);
            return Signal.FromCopy(output, signal.SamplingRate);
        }

        var shifted = new float[length + delay];
        signal.Samples.CopyTo(shifted.AsSpan(delay));
        return Signal.FromCopy(shifted, signal.SamplingRate);
    }

    /// <summary>
    /// Concatenates two signals with the same sampling rate.
    /// </summary>
    public static Signal Concatenate(this Signal signal1, Signal signal2)
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
            "Sampling rate of signal1", "sampling rate of signal2");

        var output = new float[signal1.Length + signal2.Length];
        signal1.Samples.CopyTo(output.AsSpan(0, signal1.Length));
        signal2.Samples.CopyTo(output.AsSpan(signal1.Length));
        return Signal.FromCopy(output, signal1.SamplingRate);
    }

    /// <summary>
    /// Creates a copy of <paramref name="signal"/> repeated <paramref name="n"/> times.
    /// </summary>
    public static Signal Repeat(this Signal signal, int n)
    {
        Guard.AgainstNonPositive(n, "Number of repeat times");

        var output = new float[signal.Length * n];
        for (var i = 0; i < n; i++)
        {
            signal.Samples.CopyTo(output.AsSpan(i * signal.Length, signal.Length));
        }

        return Signal.FromCopy(output, signal.SamplingRate);
    }

    /// <summary>
    /// Creates <see cref="ComplexDiscreteSignal"/> with zero imaginary parts.
    /// </summary>
    public static ComplexDiscreteSignal ToComplex(this Signal signal)
    {
        var real = new float[signal.Length];
        signal.Samples.CopyTo(real);
        return new ComplexDiscreteSignal((int)signal.SamplingRate, real);
    }

    /// <summary>
    /// Superimposes two signals. The shorter signal is broadcast to the length of the longer one.
    /// </summary>
    public static Signal Superimpose(this Signal signal1, Signal signal2)
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
            "Sampling rate of signal1", "sampling rate of signal2");

        if (signal1.Length >= signal2.Length)
        {
            var superimposed = signal1.Clone();
            var output = superimposed.Samples;
            var input = signal2.Samples;
            for (var i = 0; i < signal2.Length; i++)
            {
                output[i] += input[i];
            }

            superimposed.NotifySamplesModified();
            return superimposed;
        }

        var result = signal2.Clone();
        var resultSamples = result.Samples;
        var addend = signal1.Samples;
        for (var i = 0; i < signal1.Length; i++)
        {
            resultSamples[i] += addend[i];
        }

        result.NotifySamplesModified();
        return result;
    }

    /// <summary>
    /// Superimposes <paramref name="signal2"/> onto <paramref name="signal1"/> at given <paramref name="positions"/>.
    /// </summary>
    public static Signal SuperimposeMany(this Signal signal1, Signal signal2, int[] positions)
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
            "Sampling rate of signal1", "sampling rate of signal2");

        var totalLength = Math.Max(signal1.Length, signal2.Length + positions.Max());
        var output = new float[totalLength];
        signal1.Samples.CopyTo(output.AsSpan(0, signal1.Length));

        var input = signal2.Samples;
        for (var p = 0; p < positions.Length; p++)
        {
            var offset = positions[p];
            for (var i = 0; i < signal2.Length; i++)
            {
                output[offset + i] += input[i];
            }
        }

        return Signal.FromCopy(output, signal1.SamplingRate);
    }

    /// <summary>
    /// Subtracts <paramref name="signal2"/> from <paramref name="signal1"/>.
    /// The shorter signal is broadcast to the length of the longer one.
    /// </summary>
    public static Signal Subtract(this Signal signal1, Signal signal2)
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
            "Sampling rate of signal1", "sampling rate of signal2");

        if (signal1.Length >= signal2.Length)
        {
            var subtracted = signal1.Clone();
            var output = subtracted.Samples;
            var input = signal2.Samples;
            for (var i = 0; i < signal2.Length; i++)
            {
                output[i] -= input[i];
            }

            subtracted.NotifySamplesModified();
            return subtracted;
        }

        var result = new float[signal2.Length];
        var minuend = signal1.Samples;
        var subtrahend = signal2.Samples;
        for (var i = 0; i < signal1.Length; i++)
        {
            result[i] = minuend[i] - subtrahend[i];
        }

        for (var i = signal1.Length; i < signal2.Length; i++)
        {
            result[i] = -subtrahend[i];
        }

        return Signal.FromCopy(result, signal1.SamplingRate);
    }

    /// <summary>
    /// Full-rectifies signal samples in-place.
    /// </summary>
    public static void FullRectify(this Signal signal)
    {
        var samples = signal.Samples;
        for (var i = 0; i < signal.Length; i++)
        {
            if (samples[i] < 0)
            {
                samples[i] = -samples[i];
            }
        }

        signal.NotifySamplesModified();
    }

    /// <summary>
    /// Half-rectifies signal samples in-place.
    /// </summary>
    public static void HalfRectify(this Signal signal)
    {
        var samples = signal.Samples;
        for (var i = 0; i < signal.Length; i++)
        {
            if (samples[i] < 0)
            {
                samples[i] = 0;
            }
        }

        signal.NotifySamplesModified();
    }

    /// <summary>
    /// Fades signal in and out linearly (in-place).
    /// </summary>
    public static void FadeInFadeOut(this Signal signal, double fadeInDuration, double fadeOutDuration)
    {
        signal.FadeIn(fadeInDuration);
        signal.FadeOut(fadeOutDuration);
    }

    /// <summary>
    /// Fades signal in and out linearly (in-place).
    /// </summary>
    public static void FadeInFadeOut(this Signal signal, TimeSpan fadeInDuration, TimeSpan fadeOutDuration)
    {
        signal.FadeIn(fadeInDuration);
        signal.FadeOut(fadeOutDuration);
    }

    /// <summary>
    /// Fades signal in linearly (in-place).
    /// </summary>
    public static void FadeIn(this Signal signal, double duration)
    {
        Guard.AgainstNonPositive(duration, "Fade-in duration");

        var fadeSampleCount = Math.Min(signal.Length, (int)(signal.SamplingRate * duration));
        ApplyFadeIn(signal, fadeSampleCount);
    }

    /// <summary>
    /// Fades signal in linearly (in-place).
    /// </summary>
    public static void FadeIn(this Signal signal, TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentException("Fade-in duration must greater than zero.");
        }

        var fadeSampleCount = Math.Min(signal.Length, (int)(signal.SamplingRate * duration.TotalSeconds));
        ApplyFadeIn(signal, fadeSampleCount);
    }

    /// <summary>
    /// Fades signal out linearly (in-place).
    /// </summary>
    public static void FadeOut(this Signal signal, double duration)
    {
        Guard.AgainstNonPositive(duration, "Fade-out duration");

        var fadeSampleCount = Math.Min(signal.Length, (int)(signal.SamplingRate * duration));
        ApplyFadeOut(signal, fadeSampleCount);
    }

    /// <summary>
    /// Fades signal out linearly (in-place).
    /// </summary>
    public static void FadeOut(this Signal signal, TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentException("Fade-out duration must greater than zero.");
        }

        var fadeSampleCount = Math.Min(signal.Length, (int)(signal.SamplingRate * duration.TotalSeconds));
        ApplyFadeOut(signal, fadeSampleCount);
    }

    /// <summary>
    /// Crossfades linearly between two signals.
    /// </summary>
    public static Signal Crossfade(this Signal signal1, Signal signal2, double duration)
    {
        Guard.AgainstNonPositive(duration, "Crossfade duration");
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
            "Sampling rate of signal1", "sampling rate of signal2");

        return CrossfadeCore(signal1, signal2, (int)(signal1.SamplingRate * duration));
    }

    /// <summary>
    /// Crossfades linearly between two signals.
    /// </summary>
    public static Signal Crossfade(this Signal signal1, Signal signal2, TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentException("Crossfade duration must greater than zero.");
        }

        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
            "Sampling rate of signal1", "sampling rate of signal2");

        return CrossfadeCore(signal1, signal2, (int)(signal1.SamplingRate * duration.TotalSeconds));
    }

    /// <summary>
    /// Computes the average energy of a signal fragment via <see cref="SignalSegment"/>.
    /// </summary>
    public static float Energy(this Signal signal, int startPos, int endPos)
        => GetSegment(signal, startPos, endPos).AverageEnergy;

    /// <summary>
    /// Computes the RMS value of a signal fragment via <see cref="SignalSegment"/>.
    /// </summary>
    public static float Rms(this Signal signal, int startPos, int endPos)
        => GetSegment(signal, startPos, endPos).Rms;

    /// <summary>
    /// Computes the zero-crossing rate of a signal fragment via <see cref="SignalSegment"/>.
    /// </summary>
    public static float ZeroCrossingRate(this Signal signal, int startPos, int endPos)
        => GetSegment(signal, startPos, endPos).ZeroCrossingRate;

    /// <summary>
    /// Computes the Shannon entropy of a signal fragment via <see cref="SignalSegment"/>.
    /// </summary>
    public static float Entropy(this Signal signal, int startPos, int endPos, int binCount = 32)
        => GetSegment(signal, startPos, endPos).GetEntropy(binCount);

    /// <summary>
    /// Computes the average energy of a sample fragment.
    /// </summary>
    public static float Energy(ReadOnlySpan<float> samples, int startPos, int endPos)
    {
        var total = 0.0f;
        for (var i = startPos; i < endPos; i++)
        {
            total += samples[i] * samples[i];
        }

        return total / (endPos - startPos);
    }

    /// <summary>
    /// Computes the RMS value of a sample fragment.
    /// </summary>
    public static float Rms(ReadOnlySpan<float> samples, int startPos, int endPos)
        => MathF.Sqrt(Energy(samples, startPos, endPos));

    /// <summary>
    /// Computes the zero-crossing rate of a sample fragment.
    /// </summary>
    public static float ZeroCrossingRate(ReadOnlySpan<float> samples, int startPos, int endPos)
    {
        const float disbalance = 1e-4f;

        var prevSample = samples[startPos] + disbalance;

        var rate = 0;
        for (var i = startPos + 1; i < endPos; i++)
        {
            var sample = samples[i] + disbalance;

            if ((sample >= 0) != (prevSample >= 0))
            {
                rate++;
            }

            prevSample = sample;
        }

        return (float)rate / (endPos - startPos - 1);
    }

    /// <summary>
    /// Computes the Shannon entropy of a sample fragment using uniformly distributed bins.
    /// </summary>
    public static float Entropy(ReadOnlySpan<float> samples, int startPos, int endPos, int binCount = 32)
    {
        var len = endPos - startPos;

        if (len < binCount)
        {
            binCount = len;
        }

        if (binCount <= 0)
        {
            return 0;
        }

        var bins = new int[binCount + 1];

        var min = samples[startPos];
        var max = samples[startPos];
        for (var i = startPos; i < endPos; i++)
        {
            var sample = MathF.Abs(samples[i]);

            if (sample < min)
            {
                min = sample;
            }
            if (sample > max)
            {
                max = sample;
            }
        }

        if (max - min < 1e-8f)
        {
            return 0;
        }

        var binLength = (max - min) / binCount;

        for (var i = startPos; i < endPos; i++)
        {
            bins[(int)((MathF.Abs(samples[i]) - min) / binLength)]++;
        }

        var entropy = 0.0f;
        for (var i = 0; i < binCount; i++)
        {
            var p = (float)bins[i] / len;

            if (p > 1e-8f)
            {
                entropy += p * MathF.Log(p, 2);
            }
        }

        return -entropy / MathF.Log(binCount, 2);
    }

    private static void ApplyFadeIn(Signal signal, int fadeSampleCount)
    {
        if (fadeSampleCount <= 0)
        {
            return;
        }

        var samples = signal.Samples;
        for (var i = 0; i < fadeSampleCount; i++)
        {
            samples[i] *= (float)i / fadeSampleCount;
        }

        signal.NotifySamplesModified();
    }

    private static void ApplyFadeOut(Signal signal, int fadeSampleCount)
    {
        if (fadeSampleCount <= 0)
        {
            return;
        }

        var samples = signal.Samples;
        for (int i = signal.Length - fadeSampleCount, fadeIndex = fadeSampleCount - 1;
             i < signal.Length;
             i++, fadeIndex--)
        {
            samples[i] *= (float)fadeIndex / fadeSampleCount;
        }

        signal.NotifySamplesModified();
    }

    private static Signal CrossfadeCore(Signal signal1, Signal signal2, int crossfadeSampleCount)
    {
        var minSignalLength = Math.Min(signal1.Length, signal2.Length);
        crossfadeSampleCount = Math.Min(crossfadeSampleCount, minSignalLength);

        var outputLength = signal1.Length + signal2.Length - crossfadeSampleCount;
        var output = new float[outputLength];

        var s1 = signal1.Samples;
        var s2 = signal2.Samples;

        s1.Slice(0, signal1.Length - crossfadeSampleCount)
            .CopyTo(output.AsSpan(0, signal1.Length - crossfadeSampleCount));
        s2.Slice(crossfadeSampleCount)
            .CopyTo(output.AsSpan(signal1.Length, signal2.Length - crossfadeSampleCount));

        var startPos = signal1.Length - crossfadeSampleCount;
        for (int i = startPos, fadeIndex = 0; fadeIndex < crossfadeSampleCount; fadeIndex++, i++)
        {
            var frac = (float)fadeIndex / crossfadeSampleCount;
            output[i] = (1 - frac) * s1[i] + frac * s2[fadeIndex];
        }

        return Signal.FromCopy(output, signal1.SamplingRate);
    }

    private static SignalSegment GetSegment(Signal signal, int startPos, int endPos)
    {
        var length = endPos - startPos;
        return signal[startPos, length, throwException: true]!.Value;
    }
}
