namespace Vorcyc.Mathematics.SignalProcessing.Signals;

using System.Numerics;

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
    public static DiscreteSignal<T> Delay<T>(this DiscreteSignal<T> signal, int delay)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        var length = signal.SampleCount;

        if (delay <= 0)
        {
            delay = -delay;

            Guard.AgainstInvalidRange(delay, length, "Delay", "signal length");

            return new DiscreteSignal<T>(
                            signal.SamplingRate,
                            signal.Samples.Values.FastCopyFragment(length - delay, delay));
        }

        return new DiscreteSignal<T>(
                        signal.SamplingRate,
                        signal.Samples.Values.FastCopyFragment(length, destinationOffset: delay));
    }

    /// <summary>
    /// Superimposes signals <paramref name="signal1"/> and <paramref name="signal2"/>. 
    /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    public static DiscreteSignal<T> Superimpose<T>(this DiscreteSignal<T> signal1, DiscreteSignal<T> signal2)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        DiscreteSignal<T> superimposed;

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

    /// <summary>
    /// Superimposes <paramref name="signal2"/> and <paramref name="signal1"/> multiple times at given <paramref name="positions"/>.
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    /// <param name="positions">Positions (indices) where to insert <paramref name="signal2"/></param>
    public static DiscreteSignal<T> SuperimposeMany<T>(this DiscreteSignal<T> signal1, DiscreteSignal<T> signal2, int[] positions)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        var totalLength = Math.Max(signal1.SampleCount, signal2.SampleCount + positions.Max());

        DiscreteSignal<T> superimposed = new(signal1.SamplingRate, totalLength);
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

    /// <summary>
    /// Subtracts <paramref name="signal2"/> from <paramref name="signal1"/>. 
    /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    public static DiscreteSignal<T> Subtract<T>(this DiscreteSignal<T> signal1, DiscreteSignal<T> signal2)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        DiscreteSignal<T> subtracted;

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
            subtracted = new DiscreteSignal<T>(signal2.SamplingRate, signal2.SampleCount);

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
    public static DiscreteSignal<T> Concatenate<T>(this DiscreteSignal<T> signal1, DiscreteSignal<T> signal2)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        return new DiscreteSignal<T>(
                        signal1.SamplingRate,
                        signal1.Samples.Values.Merge(signal2.Samples));
    }

    /// <summary>
    /// Creates the copy of <paramref name="signal"/> repeated <paramref name="n"/> times.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="n">Number of times to repeat <paramref name="signal"/></param>
    public static DiscreteSignal<T> Repeat<T>(this DiscreteSignal<T> signal, int n)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        Guard.AgainstNonPositive(n, "Number of repeat times");

        return new DiscreteSignal<T>(
                        signal.SamplingRate,
                        signal.Samples.Values.Repeat(n));
    }


    /// <summary>
    /// Amplifies <paramref name="signal"/> by <paramref name="coeff"/> in-place.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="coeff">Amplification coefficient</param>
    public static void Amplify<T>(this DiscreteSignal<T> signal, T coeff)
       where T : unmanaged, IFloatingPointIeee754<T>
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
    public static void Attenuate<T>(this DiscreteSignal<T> signal, T coeff)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        //Guard.AgainstNonPositive(coeff, "Attenuation coefficient");
        if (T.IsNegative(coeff) || T.IsZero(coeff))
            throw new ArgumentException($"{nameof(coeff)} must be positive!");

        signal.Amplify(T.One / coeff);
    }

    /// <summary>
    /// Reverses <paramref name="signal"/> in-place.
    /// </summary>
    public static void Reverse<T>(this DiscreteSignal<T> signal)
        where T : unmanaged, IFloatingPointIeee754<T>
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
    public static DiscreteSignal<T> First<T>(this DiscreteSignal<T> signal, int n)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        Guard.AgainstNonPositive(n, "Number of samples");
        Guard.AgainstExceedance(n, signal.SampleCount, "Number of samples", "signal length");

        return new DiscreteSignal<T>(
                        signal.SamplingRate,
                        signal.Samples.Values.FastCopyFragment(n));
    }

    /// <summary>
    /// Creates new signal from last <paramref name="n"/> samples of <paramref name="signal"/>.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="n">Number of samples to copy</param>
    public static DiscreteSignal<T> Last<T>(this DiscreteSignal<T> signal, int n)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        Guard.AgainstNonPositive(n, "Number of samples");
        Guard.AgainstExceedance(n, signal.SampleCount, "Number of samples", "signal length");

        return new DiscreteSignal<T>(
                        signal.SamplingRate,
                        signal.Samples.Values.FastCopyFragment(n, signal.SampleCount - n));
    }

    /// <summary>
    /// Full-rectifies <paramref name="signal"/> in-place.
    /// </summary>
    /// <param name="signal">Signal</param>
    public static void FullRectify<T>(this DiscreteSignal<T> signal)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        for (var i = 0; i < signal.SampleCount; i++)
        {
            if (T.IsNegative(signal[i]))
            {
                signal[i] = -signal[i];
            }
        }
    }

    /// <summary>
    /// Half-rectifies <paramref name="signal"/> in-place.
    /// </summary>
    /// <param name="signal">Signal</param>
    public static void HalfRectify<T>(this DiscreteSignal<T> signal)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        for (var i = 0; i < signal.SampleCount; i++)
        {
            if (T.IsNegative(signal[i]))
            {
                signal[i] = T.Zero;
            }
        }
    }

    /// <summary>
    /// Normalizes <paramref name="signal"/> by its max absolute value (to range [-1, 1]).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="bitsPerSample">Bit depth</param>
    public static void NormalizeMax<T>(this DiscreteSignal<T> signal, int bitsPerSample = 0)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        var norm = T.One / signal.Samples.Max(T.Abs);

        if (bitsPerSample > 0)
        {
            //norm *= (float)(1 - 1 / Math.Pow(2, bitsPerSample));
            norm *= (T.One - T.One / T.Pow(T.CreateChecked(2), T.CreateChecked(bitsPerSample)));
        }

        signal.Amplify(norm);
    }

    ///// <summary>
    ///// Creates <see cref="ComplexDiscreteSignal"/> from <see cref="DiscreteSignal"/>. 
    ///// Imaginary parts will be filled with zeros.
    ///// </summary>
    ///// <param name="signal">Real-valued signal</param>
    //public static ComplexDiscreteSignal ToComplex(this DiscreteSignal signal)
    //{
    //    return new ComplexDiscreteSignal(signal.SamplingRate, signal.Samples.ToDoubles());
    //}

    /// <summary>
    /// Fades signal in and out linearly (in-place).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="fadeInDuration">Fade-in duration</param>
    /// <param name="fadeOutDuration">Fade-out duration</param>
    public static void FadeInFadeOut<T>(this DiscreteSignal<T> signal, double fadeInDuration, double fadeOutDuration)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        signal.FadeIn(fadeInDuration);
        signal.FadeOut(fadeOutDuration);
    }

    /// <summary>
    /// Fades signal in linearly (in-place).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="duration">Fade-in duration (in seconds)</param>
    public static void FadeIn<T>(this DiscreteSignal<T> signal, double duration)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        Guard.AgainstNonPositive(duration, "Fade-in duration");

        var fadeSampleCount = Math.Min(signal.SampleCount, (int)(signal.SamplingRate * duration));
        T fadeSampleCount_T = T.CreateTruncating(fadeSampleCount);

        for (var i = 0; i < fadeSampleCount; i++)
        {
            signal[i] *= T.CreateChecked(i) / fadeSampleCount_T;
        }
    }

    /// <summary>
    /// Fades signal out linearly (in-place).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="duration">Fade-out duration (in seconds)</param>
    public static void FadeOut<T>(this DiscreteSignal<T> signal, double duration)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        Guard.AgainstNonPositive(duration, "Fade-out duration");

        var fadeSampleCount = Math.Min(signal.SampleCount, (int)(signal.SamplingRate * duration));
        T fadeSampleCount_T = T.CreateTruncating(fadeSampleCount);

        for (int i = signal.SampleCount - fadeSampleCount, fadeIndex = fadeSampleCount - 1; i < signal.SampleCount; i++, fadeIndex--)
        {
            signal[i] *= T.CreateChecked(fadeIndex) / fadeSampleCount_T;
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
    public static DiscreteSignal<T> Crossfade<T>(this DiscreteSignal<T> signal1, DiscreteSignal<T> signal2, double duration)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        Guard.AgainstNonPositive(duration, "Crossfade duration");

        var minSignalLength = Math.Min(signal1.SampleCount, signal2.SampleCount);
        var crossfadeSampleCount = Math.Min((int)(signal1.SamplingRate * duration), minSignalLength);
        T crossfadeSampleCount_T = T.CreateTruncating(crossfadeSampleCount);

        var crossfaded = new DiscreteSignal<T>(signal1.SamplingRate, signal1.SamplingRate + signal2.SamplingRate - crossfadeSampleCount);

        Array.Copy(signal1.Samples, crossfaded.Samples, signal1.SamplingRate - crossfadeSampleCount);
        Array.Copy(signal2.Samples, crossfadeSampleCount, crossfaded.Samples, signal1.SamplingRate, signal2.SamplingRate - crossfadeSampleCount);

        var startPos = signal1.SamplingRate - crossfadeSampleCount;

        for (int i = startPos, fadeIndex = 0; fadeIndex < crossfadeSampleCount; fadeIndex++, i++)
        {
            var frac = T.CreateChecked(fadeIndex) / crossfadeSampleCount_T;

            crossfaded[i] = (T.One - frac) * signal1[i] + frac * signal2[fadeIndex];
        }

        return crossfaded;
    }





















}