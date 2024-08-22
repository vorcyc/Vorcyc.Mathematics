namespace Vorcyc.Mathematics.SignalProcessing.Signals;

using System.Numerics;
using Vorcyc.Mathematics.Numerics;

public class ComplexDiscreteSignal<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{


    /// <summary>
    /// Gets sampling rate (number of samples per one second).
    /// </summary>
    public int SamplingRate { get; }

    /// <summary>
    /// Gets the real parts of complex-valued samples.
    /// </summary>
    public T[] Real { get; }

    /// <summary>
    /// Gets the imaginary parts of complex-valued samples.
    /// </summary>
    public T[] Imag { get; }

    /// <summary>
    /// Gets the length of the signal.
    /// </summary>
    public int Length => Real.Length;

    /// <summary>
    /// The most efficient constructor for initializing complex discrete signals. 
    /// By default, it just wraps <see cref="ComplexDiscreteSignal"/> 
    /// around arrays <paramref name="real"/> and <paramref name="imag"/> (without copying).
    /// If a new memory should be allocated for signal data, set <paramref name="allocateNew"/> to true.
    /// </summary>
    /// <param name="samplingRate">Sampling rate of the signal</param>
    /// <param name="real">Array of real parts of the complex-valued signal</param>
    /// <param name="imag">Array of imaginary parts of the complex-valued signal</param>
    /// <param name="allocateNew">Set to true if new memory should be allocated for data</param>
    public ComplexDiscreteSignal(int samplingRate, T[] real, T[]? imag = null, bool allocateNew = false)
    {
        Guard.AgainstNonPositive(samplingRate, "Sampling rate");

        SamplingRate = samplingRate;
        Real = allocateNew ? real.Copy() : real;

        // additional logic for imaginary part initialization

        if (imag != null)
        {
            Guard.AgainstInequality(real.Length, imag.Length, "Number of real parts", "number of imaginary parts");

            Imag = allocateNew ? imag.Copy() : imag;
        }
        else
        {
            Imag = new T[real.Length];
        }
    }

    /// <summary>
    /// Constructs complex signal from collections of <paramref name="real"/> and <paramref name="imag"/> parts.
    /// </summary>
    /// <param name="samplingRate">Sampling rate of the signal</param>
    /// <param name="real">Array of real parts of the complex-valued signal</param>
    /// <param name="imag">Array of imaginary parts of the complex-valued signal</param>
    public ComplexDiscreteSignal(int samplingRate, IEnumerable<T> real, IEnumerable<T>? imag = null)
        : this(samplingRate, real.ToArray(), imag?.ToArray())
    {
    }

    /// <summary>
    /// Constructs signal from collection of <paramref name="samples"/> sampled at <paramref name="samplingRate"/>.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="samples">Collection of complex-valued samples</param>
    public ComplexDiscreteSignal(int samplingRate, IEnumerable<Complex<T>> samples)
        : this(samplingRate, samples.Select(s => s.Real), samples.Select(s => s.Imaginary))
    {
    }

    /// <summary>
    /// Constructs signal of given <paramref name="length"/> filled with specified values.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="length">Number of samples</param>
    /// <param name="real">Value of each sample</param>
    /// <param name="imag">Value of each sample</param>
    public ComplexDiscreteSignal(int samplingRate, int length, T real = default, T imag = default)
    {
        Guard.AgainstNonPositive(samplingRate, "Sampling rate");

        SamplingRate = samplingRate;

        var reals = new T[length];
        var imags = new T[length];
        for (var i = 0; i < length; i++)
        {
            reals[i] = real;
            imags[i] = imag;
        }
        Real = reals;
        Imag = imags;
    }

    /// <summary>
    /// Constructs signal from collection of integer <paramref name="samples"/> sampled at given <paramref name="samplingRate"/>.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    /// <param name="samples">Collection of integer samples</param>
    /// <param name="normalizeFactor">Each sample will be divided by this value , the default value is 1</param>
    public ComplexDiscreteSignal(int samplingRate, IEnumerable<T> samples, T? normalizeFactor = null)
    {
        Guard.AgainstNonPositive(samplingRate, "Sampling rate");

        normalizeFactor ??= T.One;

        SamplingRate = samplingRate;

        var intSamples = samples.ToArray();
        var realSamples = new T[intSamples.Length];

        for (var i = 0; i < intSamples.Length; i++)
        {
            realSamples[i] = intSamples[i] / normalizeFactor.Value;
        }

        Real = realSamples;
        Imag = new T[intSamples.Length];
    }

    /// <summary>
    /// Creates deep copy of the signal.
    /// </summary>
    public ComplexDiscreteSignal<T> Copy()
    {
        return new ComplexDiscreteSignal<T>(SamplingRate, Real, Imag, allocateNew: true);
    }

    /// <summary>
    /// Sample indexer. Works only with array of real parts of samples. Use it with caution.
    /// </summary>
    public T this[int index]
    {
        get => Real[index];
        set => Real[index] = value;
    }

    /// <summary>
    /// Creates the slice of the signal: 
    /// <code>
    ///     var middle = signal[900, 1200];
    /// </code>
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive)</param>
    /// <param name="endPos">Index of the last sample (exclusive)</param>
    public ComplexDiscreteSignal<T> this[int startPos, int endPos]
    {
        get
        {
            Guard.AgainstInvalidRange(startPos, endPos, "Left index", "Right index");

            var rangeLength = endPos - startPos;

            return new ComplexDiscreteSignal<T>(SamplingRate,
                                Real.FastCopyFragment(rangeLength, startPos),
                                Imag.FastCopyFragment(rangeLength, startPos));
        }
    }

    /// <summary>
    /// Gets the magnitudes of complex-valued samples.
    /// </summary>
    public T[] Magnitude
    {
        get
        {
            var real = Real;
            var imag = Imag;

            var magnitude = new T[real.Length];
            for (var i = 0; i < magnitude.Length; i++)
            {
                magnitude[i] = T.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
            }

            return magnitude;
        }
    }

    /// <summary>
    /// Gets the power (squared magnitudes) of complex-valued samples.
    /// </summary>
    public T[] Power
    {
        get
        {
            var real = Real;
            var imag = Imag;

            var magnitude = new T[real.Length];
            for (var i = 0; i < magnitude.Length; i++)
            {
                magnitude[i] = real[i] * real[i] + imag[i] * imag[i];
            }

            return magnitude;
        }
    }

    /// <summary>
    /// Gets the phases of complex-valued samples.
    /// </summary>
    public T[] Phase
    {
        get
        {
            var real = Real;
            var imag = Imag;

            var phase = new T[real.Length];
            for (var i = 0; i < phase.Length; i++)
            {
                phase[i] = T.Atan2(imag[i], real[i]);
            }

            return phase;
        }
    }

    /// <summary>
    /// Gets the unwrapped phases of complex-valued samples.
    /// </summary>
    public T[] PhaseUnwrapped => ComplexDiscreteSignalExtensions.Unwrap<T>(Phase);// Phase.Unwrap();


    #region overloaded operators

    /// <summary>
    /// Creates new signal by superimposing signals <paramref name="s1"/> and <paramref name="s2"/>. 
    /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
    /// </summary>
    /// <param name="s1">First signal</param>
    /// <param name="s2">Second signal</param>
    public static ComplexDiscreteSignal<T> operator +(ComplexDiscreteSignal<T> s1, ComplexDiscreteSignal<T> s2)
    {
        return s1.Superimpose(s2);
    }

    /// <summary>
    /// Creates new signal by adding <paramref name="constant"/> to signal <paramref name="s"/>.
    /// </summary>
    /// <param name="s">Signal</param>
    /// <param name="constant">Constant to add to each sample</param>
    public static ComplexDiscreteSignal<T> operator +(ComplexDiscreteSignal<T> s, T constant)
    {
        return new ComplexDiscreteSignal<T>(s.SamplingRate, s.Real.Select(x => x + constant), imag: null);
    }

    /// <summary>
    /// Creates new signal by subtracting <paramref name="constant"/> from signal <paramref name="s"/>.
    /// </summary>
    /// <param name="s">Signal</param>
    /// <param name="constant">Constant to subtract from each sample</param>
    public static ComplexDiscreteSignal<T> operator -(ComplexDiscreteSignal<T> s, T constant)
    {
        return new ComplexDiscreteSignal<T>(s.SamplingRate, s.Real.Select(x => x - constant), imag: null);
    }

    /// <summary>
    /// Creates new signal by multiplying <paramref name="s"/> by <paramref name="coeff"/> (amplification/attenuation).
    /// </summary>
    /// <param name="s">Signal</param>
    /// <param name="coeff">Amplification/attenuation coefficient</param>
    public static ComplexDiscreteSignal<T> operator *(ComplexDiscreteSignal<T> s, T coeff)
    {
        var signal = s.Copy();
        signal.Amplify(coeff);
        return signal;
    }

    #endregion





}
