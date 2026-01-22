using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// Defines the contract for a time-domain signal, providing access to signal characteristics and methods for
/// transforming to the frequency domain.
/// </summary>
/// <remarks>Implementations of this interface represent sampled signals in the time domain and support conversion
/// to frequency-domain representations. The interface includes properties for duration, sampling rate, and length, as
/// well as methods for notifying when samples are modified and for performing frequency-domain transformations. Thread
/// safety and mutability depend on the specific implementation.</remarks>
public interface ITimeDomainSignal : ITimeDomainCharacteristics
{

    /// <summary>
    /// Gets the duration of the signal.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Gets the sampling rate of the signal.
    /// </summary>
    float SamplingRate { get; }

    /// <summary>
    /// Gets the length of the signal.
    /// </summary>
    int Length { get; }


    /// <summary>
    /// This method should be called after any operation that modifies sample values
    /// to notify the signal object that the samples have been modified.
    /// </summary>
    void NotifySamplesModified();


    #region To Frequency Domain

    /// <summary>
    /// Transforms the time-domain signal to a frequency-domain signal.
    /// The default implemention is based-on <see cref="Vorcyc.Mathematics.SignalProcessing.Fourier.FastFourierTransform"/>.
    /// </summary>
    /// <param name="window">The optional window type to apply.</param>
    /// <param name="fftVersion">The FFT execution mode. <see cref="FftVersion.Normal"/> is recommended for small-scale data, and <see cref="FftVersion.Parallel"/> for large-scale data. Defaults to <see cref="FftVersion.Normal"/>.</param>
    /// <returns>A <see cref="FrequencyDomain"/> object representing the frequency-domain signal.</returns>
    FrequencyDomain TransformToFrequencyDomain(WindowType? window = null, FftVersion fftVersion = FftVersion.Normal);

    #endregion

    #region Helpers

    /// <summary>
    /// Gets a segment of the array padded to the next power of 2 in length.
    /// </summary>
    /// <param name="array">The input array.</param>
    /// <param name="start">The start index.</param>
    /// <param name="length">The length of the segment.</param>
    /// <returns>A <see cref="Span{T}"/> of the array segment, zero-padded to a power-of-2 length if necessary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the start index or length is out of the valid range.</exception>
    internal static Span<float> GetLengthByPowerOf2(float[] array, int start, int length)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (start < 0 || start >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(start), "Start index is out of range.");

        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");

        if (start + length > array.Length)
            throw new ArgumentOutOfRangeException(nameof(length), "The segment exceeds the array bounds.");

        int originalLength = Math.Min(length, array.Length - start);
        int paddedLength = originalLength.NextPowerOf2();

        if (paddedLength == originalLength)
        {
            return new Span<float>(array, start, length);
        }
        else
        {
            float[] paddedArray = new float[paddedLength];
            new Span<float>(array, start, originalLength).CopyTo(paddedArray);
            return new Span<float>(paddedArray);
        }
    }

    /// <summary>
    /// Pads with zeros and optionally applies a window function, for use with FFT.
    /// </summary>
    /// <param name="samples">The input samples.</param>
    /// <param name="desiredLen">The desired length after zero-padding, which must be a power of 2. This length is used for FFT.</param>
    /// <param name="windowingType">The optional window type to apply.</param>
    /// <returns>A zero-padded (and optionally windowed) array of samples.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float[] PadZerosAndWindowing
        (
            ReadOnlySpan<float> samples,
            int desiredLen,
            WindowType? windowingType = null
        )
    {
        float[] tempSamples = new float[desiredLen];

        samples.CopyTo(tempSamples);

        //if (windowingType is not null)
        //    Vorcyc.Mathematics.SignalProcessing.Windowing.WindowApplier.Apply(tempSamples.AsSpan(), 0, desiredLen, windowingType.Value);  
        if (windowingType is not null)
            WindowApplier.Apply(tempSamples, windowingType.Value, true);

        return tempSamples;
    }


    /// <summary>
    /// Converts a time value to an array index or length.
    /// </summary>
    /// <param name="time">The time value to convert.</param>
    /// <param name="samplingRate">The sampling rate of the signal.</param>
    /// <returns>The corresponding array index or length.</returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    internal static int TimeToArrayIndexOrLength(TimeSpan time, float samplingRate)
        => (int)(time.TotalSeconds * samplingRate);


    /// <summary>
    /// Converts an array index or length to a time value.
    /// </summary>
    /// <param name="indexOrLength">The array index or length to convert.</param>
    /// <param name="samplingRate">The sampling rate of the signal.</param>
    /// <returns>The corresponding <see cref="TimeSpan"/>.</returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    internal static TimeSpan ArrayIndexOrLengthToTime(int indexOrLength, float samplingRate)
        => TimeSpan.FromSeconds(indexOrLength / samplingRate);


    #endregion

}