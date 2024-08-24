using System.Runtime.CompilerServices;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<ArraySegment<T>> Split<T>(this T[] array, int segmentLength)
    {
        if (segmentLength <= 0)
            throw new ArgumentException("Segment length must be greater than zero.", nameof(segmentLength));

        // 计算余数
        var remainder = array.Length % segmentLength;

        // 将数组分割成段
        for (int i = 0; i < array.Length - remainder; i += segmentLength)
        {
            yield return new ArraySegment<T>(array, i, segmentLength);
        }

        // 处理剩余元素
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<ArraySegment<T>> Split<T>(this T[] array, int start, int length, int segmentLength)
    {
        if (start < 0 || start >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(start), "Start index must be within the bounds of the array.");
        if (length <= 0)
            throw new ArgumentException("Length must be greater than zero.", nameof(length));
        if (segmentLength <= 0)
            throw new ArgumentException("Segment length must be greater than zero.", nameof(segmentLength));

        // 计算有效处理长度
        var len = System.Math.Min(length, array.Length - start);
        // 计算余数
        var remainder = len % segmentLength;

        // 将数组分割成段
        for (int i = start; i < start + len - remainder; i += segmentLength)
        {
            yield return new ArraySegment<T>(array, i, segmentLength);
        }

        // 处理剩余元素
        if (remainder != 0)
            yield return new ArraySegment<T>(array, start + len - remainder, remainder);
    }



    /// <summary>
    /// Splits the array into segments of specified length and returns the start index and length of each segment.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to be split.</param>
    /// <param name="segmentLength">The length of each segment.</param>
    /// <returns>An enumerable of tuples containing the start index and length of each segment.</returns>
    /// <exception cref="ArgumentException">Thrown when segment length is less than or equal to zero.</exception>
    /// <remarks>
    /// This method is special provided for <see cref="Span{T}"/> instead of <see cref="ArraySegment{T}"/>.
    /// </remarks>
    [MethodImpl (MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<(int startIndex, int length)> SplitAsMarks<T>(this T[] array, int segmentLength)
    {
        if (segmentLength <= 0)
            throw new ArgumentException("Segment length must be greater than zero.", nameof(segmentLength));

        // 计算余数
        var remainder = array.Length % segmentLength;

        // 将数组分割成段
        for (int i = 0; i < array.Length - remainder; i += segmentLength)
        {
            yield return (i, segmentLength);
        }

        // 处理剩余元素
        if (remainder != 0)
            yield return (array.Length - remainder, remainder);
    }



    /// <summary>
    /// Splits the array into segments of specified length starting from a given index and returns the start index and length of each segment.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to be split.</param>
    /// <param name="start">The starting index from where to begin splitting.</param>
    /// <param name="length">The length of the array to be processed from the starting index.</param>
    /// <param name="segmentLength">The length of each segment.</param>
    /// <returns>An enumerable of tuples containing the start index and length of each segment.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the start index is out of the bounds of the array.</exception>
    /// <exception cref="ArgumentException">Thrown when length or segment length is less than or equal to zero.</exception>
    /// <remarks>
    /// This method is special provided for <see cref="Span{T}"/> instead of <see cref="ArraySegment{T}"/>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<(int startIndex,int length)> SplitAsMarks<T>(this T[] array, int start, int length, int segmentLength)
    {
        if (start < 0 || start >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(start), "Start index must be within the bounds of the array.");
        if (length <= 0)
            throw new ArgumentException("Length must be greater than zero.", nameof(length));
        if (segmentLength <= 0)
            throw new ArgumentException("Segment length must be greater than zero.", nameof(segmentLength));

        // 计算有效处理长度
        var len = System.Math.Min(length, array.Length - start);
        // 计算余数
        var remainder = len % segmentLength;

        // 将数组分割成段
        for (int i = start; i < start + len - remainder; i += segmentLength)
        {
            yield return (i, segmentLength);
        }

        // 处理剩余元素
        if (remainder != 0)
            yield return (start + len - remainder, remainder);
    }



}
