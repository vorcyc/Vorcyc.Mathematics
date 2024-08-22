namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// Provides extension methods for working with <see cref="DiscreteSignal"/> objects.
/// </summary>
public static partial class DiscreteSignalExtensions
{
    // Note.
    // Method implementations are LINQ-less and leverage FastCopy() for better performance.



    /// <summary>
    /// Creates the delayed copy of <paramref name="signal"/> 
    /// by shifting it either to the right (positive <paramref name="delay"/>, e.g. Delay(1000)) 
    /// or to the left (negative <paramref name="delay"/>, e.g. Delay(-1000)).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="delay">Delay (positive or negative number of delay samples)</param>
    public static DiscreteSignal Delay(this DiscreteSignal signal, int delay)
    {
        var length = signal.SampleCount;

        if (delay <= 0)
        {
            delay = -delay;

            Guard.AgainstInvalidRange(delay, length, "Delay", "signal length");

            return new DiscreteSignal(
                            signal.SamplingRate,
                            signal.Samples.Values.FastCopyFragment(length - delay, delay),
                            MemoryStrategy.Immediate);
        }

        return new DiscreteSignal(
                        signal.SamplingRate,
                        signal.Samples.Values.FastCopyFragment(length, destinationOffset: delay),
                        MemoryStrategy.Immediate);
    }




    /// <summary>
    /// Superimposes signals <paramref name="signal1"/> and <paramref name="signal2"/>. 
    /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    public static DiscreteSignal Superimpose(this DiscreteSignal signal1, DiscreteSignal signal2)
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        DiscreteSignal superimposed;

        if (signal1.SampleCount >= signal2.SampleCount)
        {
            superimposed = signal1.Clone();

            for (var i = 0; i < signal2.SampleCount; i++)
            {
                superimposed[i] += signal2.Samples[i];
            }
        }
        else
        {
            superimposed = signal2.Clone();

            for (var i = 0; i < signal1.SampleCount; i++)
            {
                superimposed[i] += signal1.Samples[i];
            }
        }

        return superimposed;
    }

    ///// <summary>
    ///// Superimposes <paramref name="signal2"/> and <paramref name="signal1"/> multiple times at given <paramref name="positions"/>.
    ///// </summary>
    ///// <param name="signal1">First signal</param>
    ///// <param name="signal2">Second signal</param>
    ///// <param name="positions">Positions (indices) where to insert <paramref name="signal2"/></param>
    //public static DiscreteSignal SuperimposeMany(this DiscreteSignal signal1, DiscreteSignal signal2, int[] positions)
    //{
    //    Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
    //                                "Sampling rate of signal1", "sampling rate of signal2");

    //    var totalLength = Math.Max(signal1.Length, signal2.Length + positions.Max());

    //    DiscreteSignal superimposed = new DiscreteSignal(signal1.SamplingRate, totalLength);
    //    signal1.Samples.FastCopyTo(superimposed.Samples, signal1.Length);

    //    for (var p = 0; p < positions.Length; p++)
    //    {
    //        var offset = positions[p];

    //        for (var i = 0; i < signal2.Length; i++)
    //        {
    //            superimposed[offset + i] += signal2.Samples[i];
    //        }
    //    }

    //    return superimposed;
    //} 


    ///// <summary>
    ///// Superimposes <paramref name="signal2"/> and <paramref name="signal1"/> multiple times at given <paramref name="positions"/>.
    ///// </summary>
    ///// <param name="signal1">First signal</param>
    ///// <param name="signal2">Second signal</param>
    ///// <param name="positions">Positions (indices) where to insert <paramref name="signal2"/></param>
    //public static DiscreteSignal SuperimposeMany(this DiscreteSignal signal1, DiscreteSignal signal2, int[] positions)
    //{
    //    Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
    //                                "Sampling rate of signal1", "sampling rate of signal2");

    //    var totalLength = Math.Max(signal1.Length, signal2.Length + positions.Max());

    //    DiscreteSignal superimposed = new DiscreteSignal(signal1.SamplingRate, totalLength);
    //    signal1.Samples.FastCopyTo(superimposed.Samples, signal1.Length);

    //    for (var p = 0; p < positions.Length; p++)
    //    {
    //        var offset = positions[p];

    //        for (var i = 0; i < signal2.Length; i++)
    //        {
    //            superimposed[offset + i] += signal2.Samples[i];
    //        }
    //    }

    //    return superimposed;
    //}   



    /// <summary>
    /// Superimposes <paramref name="signal2"/> and <paramref name="signal1"/> multiple times at given <paramref name="positions"/>.
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    /// <param name="positions">Positions (indices) where to insert <paramref name="signal2"/></param>
    public static DiscreteSignal SuperimposeMany(this DiscreteSignal signal1, DiscreteSignal signal2, int[] positions)
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        var totalLength = Math.Max(signal1.SampleCount, signal2.SampleCount + positions.Max());

        DiscreteSignal superimposed = new DiscreteSignal(signal1.SamplingRate, totalLength, MemoryStrategy.Immediate);
        signal1.Samples.Values.FastCopyTo(superimposed.Samples, signal1.SampleCount);

        for (var p = 0; p < positions.Length; p++)
        {
            var offset = positions[p];

            for (var i = 0; i < signal2.SampleCount; i++)
            {
                superimposed[offset + i] += signal2.Samples[i];
            }
        }

        return superimposed;
    }

    ///// <summary>
    ///// Subtracts <paramref name="signal2"/> from <paramref name="signal1"/>. 
    ///// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
    ///// </summary>
    ///// <param name="signal1">First signal</param>
    ///// <param name="signal2">Second signal</param>
    //public static DiscreteSignal Subtract(this DiscreteSignal signal1, DiscreteSignal signal2)
    //{
    //    Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
    //                                "Sampling rate of signal1", "sampling rate of signal2");

    //    DiscreteSignal subtracted;

    //    if (signal1.Length >= signal2.Length)
    //    {
    //        subtracted = signal1.Copy();

    //        for (var i = 0; i < signal2.Length; i++)
    //        {
    //            subtracted[i] -= signal2.Samples[i];
    //        }
    //    }
    //    else
    //    {
    //        subtracted = new DiscreteSignal(signal2.SamplingRate, signal2.Length);

    //        for (var i = 0; i < signal1.Length; i++)
    //        {
    //            subtracted[i] = signal1.Samples[i] - signal2.Samples[i];
    //        }
    //        for (var i = signal1.Length; i < signal2.Length; i++)
    //        {
    //            subtracted[i] = -signal2.Samples[i];
    //        }
    //    }

    //    return subtracted;
    //}


    /// <summary>
    /// Subtracts <paramref name="signal2"/> from <paramref name="signal1"/>. 
    /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    public static DiscreteSignal Subtract(this DiscreteSignal signal1, DiscreteSignal signal2)
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        DiscreteSignal subtracted;

        if (signal1.SampleCount >= signal2.SampleCount)
        {
            subtracted = signal1.Clone();

            for (var i = 0; i < signal2.SampleCount; i++)
            {
                subtracted[i] -= signal2.Samples[i];
            }
        }
        else
        {
            subtracted = new DiscreteSignal(signal2.SamplingRate, signal2.SampleCount, MemoryStrategy.Immediate);

            for (var i = 0; i < signal1.SampleCount; i++)
            {
                subtracted[i] = signal1.Samples[i] - signal2.Samples[i];
            }
            for (var i = signal1.SampleCount; i < signal2.SampleCount; i++)
            {
                subtracted[i] = -signal2.Samples[i];
            }
        }

        return subtracted;
    }

    /// <summary>
    /// Concatenates <paramref name="signal1"/> and <paramref name="signal2"/>.
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    public static DiscreteSignal Concatenate(this DiscreteSignal signal1, DiscreteSignal signal2)
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        return new DiscreteSignal(
                        signal1.SamplingRate,
                        signal1.Samples.Values.Merge(signal2.Samples));
    }

    /// <summary>
    /// Creates the copy of <paramref name="signal"/> repeated <paramref name="n"/> times.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="n">Number of times to repeat <paramref name="signal"/></param>
    public static DiscreteSignal Repeat(this DiscreteSignal signal, int n)
    {
        Guard.AgainstNonPositive(n, "Number of repeat times");

        return new DiscreteSignal(
                        signal.SamplingRate,
                        signal.Samples.Values.Repeat(n));
    }



    /// <summary>
    /// Amplifies <paramref name="signal"/> by <paramref name="coeff"/> in-place.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="coeff">Amplification coefficient</param>
    public static void Amplify(this DiscreteSignal signal, float coeff)
    {
        for (var i = 0; i < signal.SampleCount; i++)
        {
            signal[i] *= coeff;
        }
    }



    /// <summary>
    /// Attenuates <paramref name="signal"/> by <paramref name="coeff"/> in-place.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="coeff">Attenuation coefficient</param>
    public static void Attenuate(this DiscreteSignal signal, float coeff)
    {
        Guard.AgainstNonPositive(coeff, "Attenuation coefficient");

        signal.Amplify(1 / coeff);
    }

    /// <summary>
    /// Reverses <paramref name="signal"/> in-place.
    /// </summary>
    public static void Reverse(this DiscreteSignal signal)
    {
        var samples = signal.Samples;

        for (int i = 0, j = samples.Length - 1; i < samples.Length / 2; i++, j--)
        {
            var tmp = samples[i];
            samples[i] = samples[j];
            samples[j] = tmp;
        }
    }

    /// <summary>
    /// Creates new signal from first <paramref name="n"/> samples of <paramref name="signal"/>.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="n">Number of samples to copy</param>
    public static DiscreteSignal First(this DiscreteSignal signal, int n)
    {
        Guard.AgainstNonPositive(n, "Number of samples");
        Guard.AgainstExceedance(n, signal.SampleCount, "Number of samples", "signal length");

        return new DiscreteSignal(
                        signal.SamplingRate,
                        signal.Samples.Values.FastCopyFragment(n),
                        MemoryStrategy.Immediate);
    }

    /// <summary>
    /// Creates new signal from last <paramref name="n"/> samples of <paramref name="signal"/>.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="n">Number of samples to copy</param>
    public static DiscreteSignal Last(this DiscreteSignal signal, int n)
    {
        Guard.AgainstNonPositive(n, "Number of samples");
        Guard.AgainstExceedance(n, signal.SampleCount, "Number of samples", "signal length");

        return new DiscreteSignal(
                        signal.SamplingRate,
                        signal.Samples.Values.FastCopyFragment(n, signal.SampleCount - n),
                        MemoryStrategy.Immediate);
    }

    /// <summary>
    /// Full-rectifies <paramref name="signal"/> in-place.
    /// </summary>
    /// <param name="signal">Signal</param>
    public static void FullRectify(this DiscreteSignal signal)
    {
        for (var i = 0; i < signal.SampleCount; i++)
        {
            if (signal[i] < 0)
            {
                signal[i] = -signal[i];
            }
        }
    }

    /// <summary>
    /// Half-rectifies <paramref name="signal"/> in-place.
    /// </summary>
    /// <param name="signal">Signal</param>
    public static void HalfRectify(this DiscreteSignal signal)
    {
        for (var i = 0; i < signal.SampleCount; i++)
        {
            if (signal[i] < 0)
            {
                signal[i] = 0;
            }
        }
    }

    /// <summary>
    /// Normalizes <paramref name="signal"/> by its max absolute value (to range [-1, 1]).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="bitsPerSample">Bit depth</param>
    public static void NormalizeMax(this DiscreteSignal signal, int bitsPerSample = 0)
    {
        var norm = 1 / signal.Samples.Max(s => Math.Abs(s));

        if (bitsPerSample > 0)
        {
            norm *= (float)(1 - 1 / Math.Pow(2, bitsPerSample));
        }

        signal.Amplify(norm);
    }

    /// <summary>
    /// Creates <see cref="ComplexDiscreteSignal"/> from <see cref="DiscreteSignal"/>. 
    /// Imaginary parts will be filled with zeros.
    /// </summary>
    /// <param name="signal">Real-valued signal</param>
    public static ComplexDiscreteSignal ToComplex(this DiscreteSignal signal)
    {
        return new ComplexDiscreteSignal(signal.SamplingRate, signal.Samples);
    }

    /// <summary>
    /// Fades signal in and out linearly (in-place).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="fadeInDuration">Fade-in duration</param>
    /// <param name="fadeOutDuration">Fade-out duration</param>
    public static void FadeInFadeOut(this DiscreteSignal signal, double fadeInDuration, double fadeOutDuration)
    {
        signal.FadeIn(fadeInDuration);
        signal.FadeOut(fadeOutDuration);
    }

    /// <summary>
    /// Fades signal in and out linearly (in-place).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="fadeInDuration">Fade-in duration</param>
    /// <param name="fadeOutDuration">Fade-out duration</param>
    public static void FadeInFadeOut(this DiscreteSignal signal, TimeSpan fadeInDuration, TimeSpan fadeOutDuration)
    {
        signal.FadeIn(fadeInDuration);
        signal.FadeOut(fadeOutDuration);
    }

    /// <summary>
    /// Fades signal in linearly (in-place).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="duration">Fade-in duration (in seconds)</param>
    public static void FadeIn(this DiscreteSignal signal, double duration)
    {
        Guard.AgainstNonPositive(duration, "Fade-in duration");

        var fadeSampleCount = Math.Min(signal.SampleCount, (int)(signal.SamplingRate * duration));

        for (var i = 0; i < fadeSampleCount; i++)
        {
            signal[i] *= (float)i / fadeSampleCount;
        }
    }

    /// <summary>
    /// Fades signal in linearly (in-place).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="duration">Fade-in duration (in seconds)</param>
    /// <exception cref="ArgumentException"></exception>
    public static void FadeIn(this DiscreteSignal signal, TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
            throw new ArgumentException("Fade-in duration must greater than zero.");

        var fadeSampleCount = Math.Min(signal.SampleCount, (int)(signal.SamplingRate * duration.TotalSeconds));

        for (var i = 0; i < fadeSampleCount; i++)
        {
            signal[i] *= (float)i / fadeSampleCount;
        }
    }

    /// <summary>
    /// Fades signal out linearly (in-place).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="duration">Fade-out duration (in seconds)</param>
    public static void FadeOut(this DiscreteSignal signal, double duration)
    {
        Guard.AgainstNonPositive(duration, "Fade-out duration");

        var fadeSampleCount = Math.Min(signal.SampleCount, (int)(signal.SamplingRate * duration));

        for (int i = signal.SampleCount - fadeSampleCount, fadeIndex = fadeSampleCount - 1; i < signal.SampleCount; i++, fadeIndex--)
        {
            signal[i] *= (float)fadeIndex / fadeSampleCount;
        }
    }

    /// <summary>
    /// Fades signal out linearly (in-place).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="duration">Fade-out duration</param>
    /// <exception cref="ArgumentException"></exception>
    public static void FadeOut(this DiscreteSignal signal, TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
            throw new ArgumentException("Fade-out duration must greater than zero.");

        var fadeSampleCount = Math.Min(signal.SampleCount, (int)(signal.SamplingRate * duration.TotalSeconds));

        for (int i = signal.SampleCount - fadeSampleCount, fadeIndex = fadeSampleCount - 1; i < signal.SampleCount; i++, fadeIndex--)
        {
            signal[i] *= (float)fadeIndex / fadeSampleCount;
        }
    }

    /// <summary>
    /// <para>
    /// Crossfades linearly between signals and returns crossfaded signal of length 
    /// equal to sum of signal lengths minus length of crossfade section.
    /// </para>
    /// <para>
    /// The length of crossfade section will be calculated 
    /// based on the sampling rate of the first signal.
    /// </para>
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    /// <param name="duration">Crossfade duration (in seconds)</param>
    public static DiscreteSignal Crossfade(this DiscreteSignal signal1, DiscreteSignal signal2, double duration)
    {
        Guard.AgainstNonPositive(duration, "Crossfade duration");

        var minSignalLength = Math.Min(signal1.SampleCount, signal2.SampleCount);
        var crossfadeSampleCount = Math.Min((int)(signal1.SamplingRate * duration), minSignalLength);

        var crossfaded = new DiscreteSignal(signal1.SamplingRate, signal1.SampleCount + signal2.SampleCount - crossfadeSampleCount, MemoryStrategy.Immediate);

        Array.Copy(signal1.Samples, crossfaded.Samples, signal1.SampleCount - crossfadeSampleCount);
        Array.Copy(signal2.Samples, crossfadeSampleCount, crossfaded.Samples, signal1.SampleCount, signal2.SampleCount - crossfadeSampleCount);

        var startPos = signal1.SampleCount - crossfadeSampleCount;

        for (int i = startPos, fadeIndex = 0; fadeIndex < crossfadeSampleCount; fadeIndex++, i++)
        {
            var frac = (float)fadeIndex / crossfadeSampleCount;

            crossfaded[i] = (1 - frac) * signal1[i] + frac * signal2[fadeIndex];
        }

        return crossfaded;
    }

    /// <summary>
    /// <para>
    /// Crossfades linearly between signals and returns crossfaded signal of length 
    /// equal to sum of signal lengths minus length of crossfade section.
    /// </para>
    /// <para>
    /// The length of crossfade section will be calculated 
    /// based on the sampling rate of the first signal.
    /// </para>
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    /// <param name="duration">Crossfade duration</param>
    /// <exception cref="ArgumentException"></exception>
    public static DiscreteSignal Crossfade(this DiscreteSignal signal1, DiscreteSignal signal2, TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
            throw new ArgumentException("Fade-out duration must greater than zero.");

        var minSignalLength = Math.Min(signal1.SampleCount, signal2.SampleCount);
        var crossfadeSampleCount = Math.Min((int)(signal1.SamplingRate * duration.TotalSeconds), minSignalLength);

        var crossfaded = new DiscreteSignal(signal1.SamplingRate, signal1.SampleCount + signal2.SampleCount - crossfadeSampleCount, MemoryStrategy.Immediate);

        Array.Copy(signal1.Samples, crossfaded.Samples, signal1.SampleCount - crossfadeSampleCount);
        Array.Copy(signal2.Samples, crossfadeSampleCount, crossfaded.Samples, signal1.SampleCount, signal2.SampleCount - crossfadeSampleCount);

        var startPos = signal1.SampleCount - crossfadeSampleCount;

        for (int i = startPos, fadeIndex = 0; fadeIndex < crossfadeSampleCount; fadeIndex++, i++)
        {
            var frac = (float)fadeIndex / crossfadeSampleCount;

            crossfaded[i] = (1 - frac) * signal1[i] + frac * signal2[fadeIndex];
        }

        return crossfaded;
    }
}
