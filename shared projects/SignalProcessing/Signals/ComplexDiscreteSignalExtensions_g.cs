namespace Vorcyc.Mathematics.SignalProcessing.Signals;

using System.Numerics;
using Vorcyc.Mathematics.Numerics;

public static partial class ComplexDiscreteSignalExtensions
{

    /// <summary>
    /// Creates the delayed copy of <paramref name="signal"/> 
    /// by shifting it either to the right (positive <paramref name="delay"/>, e.g. Delay(1000)) 
    /// or to the left (negative <paramref name="delay"/>, e.g. Delay(-1000)).
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="delay">Delay (positive or negative number of delay samples)</param>
    public static ComplexDiscreteSignal<T> Delay<T>(this ComplexDiscreteSignal<T> signal, int delay)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var length = signal.Length;

        if (delay <= 0)
        {
            delay = -delay;

            Guard.AgainstInvalidRange(delay, length, "Delay", "signal length");

            return new ComplexDiscreteSignal<T>(
                            signal.SamplingRate,
                            signal.Real.FastCopyFragment(length - delay, delay),
                            signal.Imag.FastCopyFragment(length - delay, delay));
        }

        return new ComplexDiscreteSignal<T>(
                        signal.SamplingRate,
                        signal.Real.FastCopyFragment(length, destinationOffset: delay),
                        signal.Imag.FastCopyFragment(length, destinationOffset: delay));
    }

    /// <summary>
    /// Superimposes signals <paramref name="signal1"/> and <paramref name="signal2"/>. 
    /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    public static ComplexDiscreteSignal<T> Superimpose<T>(this ComplexDiscreteSignal<T> signal1, ComplexDiscreteSignal<T> signal2)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        ComplexDiscreteSignal<T> superimposed;

        if (signal1.Length > signal2.Length)
        {
            superimposed = signal1.Copy();

            for (var i = 0; i < signal2.Length; i++)
            {
                superimposed.Real[i] += signal2.Real[i];
                superimposed.Imag[i] += signal2.Imag[i];
            }
        }
        else
        {
            superimposed = signal2.Copy();

            for (var i = 0; i < signal1.Length; i++)
            {
                superimposed.Real[i] += signal1.Real[i];
                superimposed.Imag[i] += signal1.Imag[i];
            }
        }

        return superimposed;
    }

    /// <summary>
    /// Concatenates <paramref name="signal1"/> and <paramref name="signal2"/>.
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    public static ComplexDiscreteSignal<T> Concatenate<T>(this ComplexDiscreteSignal<T> signal1, ComplexDiscreteSignal<T> signal2)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        return new ComplexDiscreteSignal<T>(
                        signal1.SamplingRate,
                        signal1.Real.Merge(signal2.Real),
                        signal1.Imag.Merge(signal2.Imag));
    }

    /// <summary>
    /// Creates the copy of <paramref name="signal"/> repeated <paramref name="n"/> times.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="n">Number of times to repeat <paramref name="signal"/></param>
    public static ComplexDiscreteSignal<T> Repeat<T>(this ComplexDiscreteSignal<T> signal, int n)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        Guard.AgainstNonPositive(n, "Number of repeat times");

        return new ComplexDiscreteSignal<T>(
                        signal.SamplingRate,
                        signal.Real.Repeat(n),
                        signal.Imag.Repeat(n));
    }

    /// <summary>
    /// Amplifies <paramref name="signal"/> by <paramref name="coeff"/> in-place.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="coeff">Amplification coefficient</param>
    public static void Amplify<T>(this ComplexDiscreteSignal<T> signal, T coeff)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        for (var i = 0; i < signal.Length; i++)
        {
            signal.Real[i] *= coeff;
            signal.Imag[i] *= coeff;
        }
    }

    /// <summary>
    /// Attenuates <paramref name="signal"/> by <paramref name="coeff"/> in-place.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="coeff">Attenuation coefficient</param>
    public static void Attenuate<T>(this ComplexDiscreteSignal<T> signal, T coeff)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        Guard.AgainstNonPositive(coeff, "Attenuation coefficient");

        signal.Amplify(T.One / coeff);
    }

    /// <summary>
    /// Creates new signal from first <paramref name="n"/> samples of <paramref name="signal"/>.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="n">Number of samples to copy</param>
    public static ComplexDiscreteSignal<T> First<T>(this ComplexDiscreteSignal<T> signal, int n)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        Guard.AgainstNonPositive(n, "Number of samples");
        Guard.AgainstExceedance(n, signal.Length, "Number of samples", "signal length");

        return new ComplexDiscreteSignal<T>(
                        signal.SamplingRate,
                        signal.Real.FastCopyFragment(n),
                        signal.Imag.FastCopyFragment(n));
    }

    /// <summary>
    /// Creates new signal from last <paramref name="n"/> samples of <paramref name="signal"/>.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="n">Number of samples to copy</param>
    public static ComplexDiscreteSignal<T> Last<T>(this ComplexDiscreteSignal<T> signal, int n)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        Guard.AgainstNonPositive(n, "Number of samples");
        Guard.AgainstExceedance(n, signal.Length, "Number of samples", "signal length");

        return new ComplexDiscreteSignal<T>(
                        signal.SamplingRate,
                        signal.Real.FastCopyFragment(n, signal.Length - n),
                        signal.Imag.FastCopyFragment(n, signal.Imag.Length - n));
    }

    /// <summary>
    /// Creates new zero-padded complex discrete signal of <paramref name="length"/> from <paramref name="signal"/>.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="length">The length of a zero-padded signal.</param>
    public static ComplexDiscreteSignal<T> ZeroPadded<T>(this ComplexDiscreteSignal<T> signal, int length)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (length <= 0)
        {
            length = signal.Length.NextPowerOf2();
        }

        return new ComplexDiscreteSignal<T>(
                        signal.SamplingRate,
                        signal.Real.PadZeros(length),
                        signal.Imag.PadZeros(length));
    }

    /// <summary>
    /// Performs the complex multiplication of <paramref name="signal1"/> and <paramref name="signal2"/> (with normalization by length).
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    public static ComplexDiscreteSignal<T> Multiply<T>(
        this ComplexDiscreteSignal<T> signal1, ComplexDiscreteSignal<T> signal2)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        var length = signal1.Length;

        var real = new T[length];
        var imag = new T[length];

        var real1 = signal1.Real;
        var imag1 = signal1.Imag;
        var real2 = signal2.Real;
        var imag2 = signal2.Imag;

        for (var i = 0; i < length; i++)
        {
            real[i] = real1[i] * real2[i] - imag1[i] * imag2[i];
            imag[i] = real1[i] * imag2[i] + imag1[i] * real2[i];
        }

        return new ComplexDiscreteSignal<T>(signal1.SamplingRate, real, imag);
    }

    /// <summary>
    /// Performs the complex division of <paramref name="signal1"/> and <paramref name="signal2"/> (with normalization by length).
    /// </summary>
    /// <param name="signal1">First signal</param>
    /// <param name="signal2">Second signal</param>
    public static ComplexDiscreteSignal<T> Divide<T>(
        this ComplexDiscreteSignal<T> signal1, ComplexDiscreteSignal<T> signal2)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                    "Sampling rate of signal1", "sampling rate of signal2");

        var length = signal1.Length;

        var real = new T[length];
        var imag = new T[length];

        var real1 = signal1.Real;
        var imag1 = signal1.Imag;
        var real2 = signal2.Real;
        var imag2 = signal2.Imag;

        for (var i = 0; i < length; i++)
        {
            var den = imag1[i] * imag1[i] + imag2[i] * imag2[i];
            real[i] = (real1[i] * real2[i] + imag1[i] * imag2[i]) / den;
            imag[i] = (real2[i] * imag1[i] - imag2[i] * real1[i]) / den;
        }

        return new ComplexDiscreteSignal<T>(signal1.SamplingRate, real, imag);
    }

    /// <summary>
    /// Unwraps phases of complex-valued samples.
    /// </summary>
    /// <param name="phase">Phases</param>
    /// <param name="tolerance">Jump size , the default value PI</param>
    public static T[] Unwrap<T>(this T[] phase, T? tolerance = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (tolerance is null)
            tolerance = T.Pi;

        return VMath.Unwrap(phase, tolerance);
    }

    /// <summary>
    /// Yields complex numbers as type <see cref="Complex"/> from <paramref name="signal"/> samples.
    /// </summary>
    /// <param name="signal">Complex discrete signal</param>
    public static IEnumerable<Complex<T>> ToComplexNumbers<T>(this ComplexDiscreteSignal<T> signal)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        for (var i = 0; i < signal.Length; i++)
        {
            yield return new Complex<T>(signal.Real[i], signal.Imag[i]);
        }
    }













}
