namespace Vorcyc.Mathematics;

using ILGPU.Backends.IL;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics.Statistics;


/// <summary>
/// Advanced array opertions.
/// </summary>
public static partial class ArrayExtension
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<(int startIndex, int length)> SplitAsMarks<T>(this T[] array, int segmentLength)
    {
        //if (segmentLength <= 0)
        //    throw new ArgumentException("Segment length must be greater than zero.", nameof(segmentLength));

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
    public static IEnumerable<(int startIndex, int length)> SplitAsMarks<T>(this T[] array, int start, int length, int segmentLength)
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


    #region Zip



    /// <summary>
    /// Segments an array into smaller parts of a specified target length.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The original array to segment.</param>
    /// <param name="targetLength">The desired number of segments.</param>
    /// <returns>An enumerable of array segments.</returns>
    public static IEnumerable<ArraySegment<T>> Zip<T>(this T[] array, int targetLength)
    {
        double xStep = (double)array.Length / targetLength;
        var segmentLength = (int)xStep;

        for (int i = 0; i < array.Length; i += segmentLength)
        {
            yield return new ArraySegment<T>(array, i, segmentLength);
        }
    }

    /// <summary>
    /// Segments a specified range of an array into smaller parts of a specified target length.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The original array to segment.</param>
    /// <param name="startIndex">The starting index of the range to segment.</param>
    /// <param name="length">The number of elements to include in the range.</param>
    /// <param name="targetLength">The desired number of segments.</param>
    /// <returns>An enumerable of array segments.</returns>
    public static IEnumerable<ArraySegment<T>> Zip<T>(
        this T[] array,
        int startIndex, int length,
        int targetLength)
    {
        double xStep = (double)length / targetLength;
        var segmentLength = (int)xStep;

        for (int i = startIndex; i < startIndex + length - segmentLength; i += segmentLength)
        {
            yield return new ArraySegment<T>(array, i, segmentLength);
        }
    }

    /// <summary>
    /// Segments an array into smaller parts of a specified target length and returns the start index and length of each segment.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The original array to segment.</param>
    /// <param name="targetLength">The desired number of segments.</param>
    /// <returns>An enumerable of tuples containing the start index and length of each segment.</returns>
    public static IEnumerable<(int startIndex, int length)> ZipAsMarks<T>(this T[] array, int targetLength)
    {
        double xStep = (double)array.Length / targetLength;
        var segmentLength = (int)xStep;

        for (int i = 0; i < array.Length; i += segmentLength)
        {
            yield return (i, segmentLength);
        }
    }

    /// <summary>
    /// Segments a specified range of an array into smaller parts of a specified target length and returns the start index and length of each segment.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The original array to segment.</param>
    /// <param name="startIndex">The starting index of the range to segment.</param>
    /// <param name="length">The number of elements to include in the range.</param>
    /// <param name="targetLength">The desired number of segments.</param>
    /// <returns>An enumerable of tuples containing the start index and length of each segment.</returns>
    public static IEnumerable<(int startIndex, int length)> ZipAsMarks<T>(
        this T[] array,
        int startIndex, int length,
        int targetLength)
    {
        double xStep = (double)length / targetLength;
        var segmentLength = (int)xStep;

        for (int i = startIndex; i < startIndex + length - segmentLength; i += segmentLength)
        {
            yield return (i, segmentLength);
        }
    }



    #endregion







    /// <summary>
    /// Transforms an array of floats into a new array of a specified length by averaging segments of the original array.
    /// </summary>
    /// <param name="array">The original array of floats.</param>
    /// <param name="targetLength">The desired length of the transformed array.</param>
    /// <returns>An enumerable of floats representing the transformed array.</returns>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<float> TransformArray(this float[] array, int targetLength)
    {
        //四舍五入的，给TV用。往大了走，正好给TV多出余数的一帧
        var segmentLength = (int)Math.Ceiling((double)array.Length / targetLength);

        var splited = Split(array, segmentLength);

        foreach (var s in splited)
        {
            yield return SBasic.Average(s);
        }
    }

    /// <summary>
    /// Transforms an array of floats into a new array of a specified length by averaging segments of the original array.
    /// </summary>
    /// <param name="array">The original array of floats.</param>
    /// <param name="targetLength">The desired length of the transformed array.</param>
    /// <returns>A new array of floats representing the transformed array.</returns>
    public static float[] TransformToArray(float[] array, int targetLength)
    {
        return TransformToArray(array, 0, array.Length, targetLength);
    }

    /// <summary>
    /// Transforms a specified range of an array of floats into a new array of a specified length by averaging segments of the original array.
    /// </summary>
    /// <param name="array">The original array of floats.</param>
    /// <param name="start">The starting index of the range to transform.</param>
    /// <param name="length">The number of elements to include in the range.</param>
    /// <param name="targetLength">The desired length of the transformed array.</param>
    /// <returns>A new array of floats representing the transformed range of the array.</returns>
    public static float[] TransformToArray(float[] array, int start, int length, int targetLength)
    {
        if (targetLength <= 0) return null;

        var len = System.Math.Min(length, array.Length - start);

        var segmentLength = (int)Math.Round((double)len / targetLength);

        var splited = Split(array, start, length, segmentLength);

        var result = new float[targetLength];

        int i = 0;

        foreach (var s in splited)
        {
            if (i > targetLength - 1) break;
            result[i] = SBasic.Average(s);
            i++;
        }
        //剩余的一堆，再求次均值
        //Set the last one.
        var sub = splited.Skip(targetLength - 1);
        result[targetLength - 1] = SBasic.Average(sub);


        return result;
    }

}
