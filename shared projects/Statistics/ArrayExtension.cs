namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Advanced array opertions.
/// </summary>
public static class ArrayExtension
{


    /// <summary>
    /// Splits an array into segments of specified length.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to split.</param>
    /// <param name="segmentLength">The length of each segment.</param>
    /// <returns>An enumerable of array segments.</returns>
    public static IEnumerable<ArraySegment<T>> Split<T>(T[] array, int segmentLength)
    {
        // Calculate the remainder
        var remainder = array.Length % segmentLength;

        // Split the array into segments
        for (int i = 0; i < array.Length - remainder; i += segmentLength)
        {
            yield return new ArraySegment<T>(array, i, segmentLength);
        }

        // Handle the remaining elements
        if (remainder != 0)
            yield return new ArraySegment<T>(array, array.Length - remainder, remainder);
    }


    /// <summary>
    /// Splits a portion of an array into segments of specified length.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to split.</param>
    /// <param name="start">The starting index from which to begin splitting.</param>
    /// <param name="length">The length of the portion to split.</param>
    /// <param name="segmentLength">The length of each segment.</param>
    /// <returns>An enumerable of array segments.</returns>
    public static IEnumerable<ArraySegment<T>> Split<T>(T[] array, int start, int length, int segmentLength)
    {
        // Calculate the effective length to process
        var len = System.Math.Min(length, array.Length - start);
        // Calculate the remainder
        var remainder = len % segmentLength;

        // Split the array into segments
        for (int i = start; i < start + len - remainder; i += segmentLength)
        {
            yield return new ArraySegment<T>(array, i, segmentLength);
        }

        // Handle the remaining elements
        if (remainder != 0)
            yield return new ArraySegment<T>(array, array.Length - remainder, remainder);
    }


}
