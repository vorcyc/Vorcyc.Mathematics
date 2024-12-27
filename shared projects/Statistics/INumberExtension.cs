using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

public static partial class INumberExtension
{
    /// <summary>
    /// Calculates the sum of the elements in a span using SIMD for optimization.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="span">The span of values to calculate the sum of.</param>
    /// <returns>The sum of the elements in the span.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="span"/> is empty.</exception>
    [DeviceDependency(DeviceDependency.CPU)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(this Span<T> span)
        where T : INumber<T>
    {
        if (span.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(span));

        int length = span.Length;
        int simdLength = Vector<T>.Count;
        int remainder = length % simdLength;

        Vector<T> sumVector = Vector<T>.Zero;
        int i = 0;

        // Process data in chunks of Vector<T>.Count
        for (; i < length - remainder; i += simdLength)
        {
            Vector<T> vector = new Vector<T>(span.Slice(i, simdLength));
            sumVector += vector;
        }

        // Sum the elements of the sumVector
        T sum = T.Zero;
        for (int j = 0; j < simdLength; j++)
        {
            sum += sumVector[j];
        }

        // Process remaining elements
        for (; i < length; i++)
        {
            sum += span[i];
        }

        return sum;
    }

    /// <summary>
    /// Calculates the average of the elements in a span.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="span">The span of values to calculate the average of.</param>
    /// <returns>The average of the elements in the span.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="span"/> is empty.</exception>
    [DeviceDependency(DeviceDependency.CPU)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Average<T>(this Span<T> span)
        where T : INumber<T>
    {
        if (span.Length == 0)
            throw new ArgumentException("Span cannot be empty.", nameof(span));

        T sum = Sum(span);
        return sum / T.CreateChecked(span.Length);
    }

    /// <summary>
    /// Calculates the average and variance of the elements in a span.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="array">The span of values to calculate the average and variance of.</param>
    /// <returns>A tuple containing the average and variance of the elements in the span.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="array"/> is empty.</exception>
    [DeviceDependency(DeviceDependency.CPU)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T average, T variance) Variance<T>(this Span<T> array)
        where T : INumber<T>
    {
        if (array.Length == 0)
            throw new ArgumentException("Span cannot be empty.", nameof(array));

        var mean = Average(array);
        var result = T.Zero;

        int length = array.Length;
        int simdLength = Vector<T>.Count;
        int remainder = length % simdLength;

        Vector<T> varianceVector = Vector<T>.Zero;
        int i = 0;

        // Process data in chunks of Vector<T>.Count
        for (; i < length - remainder; i += simdLength)
        {
            Vector<T> vector = new Vector<T>(array.Slice(i, simdLength));
            Vector<T> diff = vector - new Vector<T>(mean);
            varianceVector += diff * diff;
        }

        // Sum the elements of the varianceVector
        for (int j = 0; j < simdLength; j++)
        {
            result += varianceVector[j];
        }

        // Process remaining elements
        for (; i < length; i++)
        {
            var v = array[i];
            result += (v - mean) * (v - mean);
        }

        result /= T.CreateChecked(array.Length - 1);
        return (mean, result);
    }
}
