namespace Vorcyc.Mathematics;

using System.Runtime.CompilerServices;


/// <summary>
/// Advanced array opertions.
/// </summary>
public static partial class ArrayExtension
{

    //    主要区别
    //分割方式：

    //Split<T> 按固定长度 segmentLength 分割，并处理剩余元素。

    //Zip<T> 通过计算每段的步长 xStep 分割，每段的长度尽量相等。

    //应用场景：

    //Split<T> 适用于需要按固定长度分割数组的场景，适合精确控制每段的长度。

    //Zip<T> 适用于需要将数组分割成尽量相等长度的段的场景，更适合用于对数组进行均匀分割。

    #region Split


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

    #endregion


    #region Zip


    /// <summary>
    /// 对数组进行压缩并生成数组片段序列，确保没有数据被遗漏。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="array">要进行压缩的数组。</param>
    /// <param name="targetLength">目标长度。</param>
    /// <returns>一个包含数组片段的序列。</returns>
    /// <remarks>
    /// 使用 <see cref="MethodImplOptions.AggressiveInlining"/> 指示编译器进行内联优化。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<ArraySegment<T>> Zip<T>(this T[] array, int targetLength)
    {
        double xStep = (double)array.Length / targetLength;
        int currentStart = 0;
        for (int i = 0; i < targetLength; i++)
        {
            int nextStart = (int)Math.Round(xStep * (i + 1));
            int segmentLength = nextStart - currentStart;
            if (currentStart + segmentLength > array.Length)
            {
                segmentLength = array.Length - currentStart;
            }
            yield return new ArraySegment<T>(array, currentStart, segmentLength);
            currentStart = nextStart;
        }
    }


    /// <summary>
    /// 对数组进行压缩并生成数组片段序列，确保没有数据被遗漏。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="array">要进行压缩的数组。</param>
    /// <param name="startIndex">数组的起始索引。</param>
    /// <param name="length">数组的长度。</param>
    /// <param name="targetLength">目标长度。</param>
    /// <returns>一个包含数组片段的序列。</returns>
    /// <remarks>
    /// 使用 <see cref="MethodImplOptions.AggressiveInlining"/> 指示编译器进行内联优化。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<ArraySegment<T>> Zip<T>(
        this T[] array,
        int startIndex, int length,
        int targetLength)
    {
        double xStep = (double)length / targetLength;
        int currentStart = startIndex;
        for (int i = 0; i < targetLength; i++)
        {
            int nextStart = (int)Math.Round(xStep * (i + 1)) + startIndex;
            int segmentLength = nextStart - currentStart;
            if (currentStart + segmentLength > startIndex + length)
            {
                segmentLength = startIndex + length - currentStart;
            }
            yield return new ArraySegment<T>(array, currentStart, segmentLength);
            currentStart = nextStart;
        }
    }


    /// <summary>
    /// 对数组进行压缩并生成标记序列，确保没有数据被遗漏。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="array">要进行压缩的数组。</param>
    /// <param name="targetLength">目标长度。</param>
    /// <returns>一个包含起始索引和长度的标记序列。</returns>
    /// <remarks>
    /// 使用 <see cref="MethodImplOptions.AggressiveInlining"/> 指示编译器进行内联优化。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<(int startIndex, int length)> ZipAsMarks<T>(this T[] array, int targetLength)
    {
        double xStep = (double)array.Length / targetLength;
        int segmentStart = 0;
        for (int i = 0; i < targetLength; i++)
        {
            int nextStart = (int)Math.Round(xStep * (i + 1));
            int segmentLength = nextStart - segmentStart;
            if (segmentStart + segmentLength > array.Length)
            {
                segmentLength = array.Length - segmentStart;
            }
            yield return (segmentStart, segmentLength);
            segmentStart = nextStart;
        }
    }


    /// <summary>
    /// 对数组进行压缩并生成标记序列，确保没有数据被遗漏。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="array">要进行压缩的数组。</param>
    /// <param name="startIndex">数组的起始索引。</param>
    /// <param name="length">数组的长度。</param>
    /// <param name="targetLength">目标长度。</param>
    /// <returns>一个包含起始索引和长度的标记序列。</returns>
    /// <remarks>
    /// 使用 <see cref="MethodImplOptions.AggressiveInlining"/> 指示编译器进行内联优化。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<(int startIndex, int length)> ZipAsMarks<T>(
        this T[] array,
        int startIndex, int length,
        int targetLength)
    {
        double xStep = (double)length / targetLength;
        int currentStart = startIndex;
        for (int i = 0; i < targetLength; i++)
        {
            int nextStart = (int)Math.Round(xStep * (i + 1)) + startIndex;
            int segmentLength = nextStart - currentStart;
            if (currentStart + segmentLength > startIndex + length)
            {
                segmentLength = startIndex + length - currentStart;
            }
            yield return (currentStart, segmentLength);
            currentStart = nextStart;
        }
    }




    #endregion




    #region TransformArray



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
            yield return Statistics.Basic.Average(s);
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
            result[i] = Statistics.Basic.Average(s);
            i++;
        }
        //剩余的一堆，再求次均值
        //Set the last one.
        var sub = splited.Skip(targetLength - 1);
        result[targetLength - 1] = Statistics.Basic.Average(sub);


        return result;
    }

    #endregion

}
