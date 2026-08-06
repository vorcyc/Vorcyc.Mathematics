using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// Defines the contract for a time-domain signal.
/// </summary>
public interface ITimeDomainSignal : ITimeDomainCharacteristics
{
    TimeSpan Duration { get; }

    float SamplingRate { get; }

    int Length { get; }

    void NotifySamplesModified();

    /// <summary>
    /// Transforms the signal to the frequency domain using an optional <see cref="ComputingContext"/>.
    /// </summary>
    FrequencyDomain TransformToFrequencyDomain(ComputingContext? context = null, WindowType? window = null);

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

        float[] paddedArray = new float[paddedLength];
        new Span<float>(array, start, originalLength).CopyTo(paddedArray);
        return new Span<float>(paddedArray);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float[] PadZerosAndWindowing(
        ReadOnlySpan<float> samples,
        int desiredLen,
        WindowType? windowingType = null)
    {
        float[] tempSamples = new float[desiredLen];
        samples.CopyTo(tempSamples);

        if (windowingType is not null)
        {
            WindowApplier.Apply(tempSamples, windowingType.Value, true);
        }

        return tempSamples;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int TimeToArrayIndexOrLength(TimeSpan time, float samplingRate)
        => (int)(time.TotalSeconds * samplingRate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TimeSpan ArrayIndexOrLengthToTime(int indexOrLength, float samplingRate)
        => TimeSpan.FromSeconds(indexOrLength / samplingRate);
}
