namespace Vorcyc.Mathematics.Statistics;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

/// <summary>
/// Find the max and min value in sequence.
/// </summary>
public static class ExtremeValueFinder
{

    #region Max Min

    /// <summary>
    /// Finds the maximum and minimum values in a subarray of floats.
    /// </summary>
    /// <param name="array">The array of floats to search.</param>
    /// <param name="start">The starting index of the subarray.</param>
    /// <param name="length">The length of the subarray.</param>
    /// <returns>A tuple containing the maximum and minimum values in the specified subarray.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> or <paramref name="length"/> is out of range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float max, float min) FindExtremeValue(
        this float[] array,
        int start, int length)
    {
        //if (array == null)
        //    throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        //if (start < 0 || length < 0 || start + length > array.Length)
        //    throw new ArgumentOutOfRangeException("Start or length is out of range.");

        var returnMin = array[start];
        var returnMax = array[start];

        var end = System.Math.Min(start + length, array.Length);

        for (int i = start; i < end; i++)
        {
            float value = array[i];
            returnMin = (value < returnMin) ? value : returnMin;
            returnMax = (value > returnMax) ? value : returnMax;
        }

        return (returnMax, returnMin);
    }

    /// <summary>
    /// Finds the maximum and minimum values in a specified range of an array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array. Must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="array">The array to search for extreme values.</param>
    /// <param name="start">The starting index of the range to search.</param>
    /// <param name="length">The number of elements to include in the range.</param>
    /// <returns>A tuple containing the maximum and minimum values in the specified range of the array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the start or length is out of range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T max, T min) FindExtremeValue<T>(
        this T[] array,
        int start, int length)
        where T : INumber<T>
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (start < 0 || length < 0 || start + length > array.Length)
            throw new ArgumentOutOfRangeException("Start or length is out of range.");

        var returnMin = array[start];
        var returnMax = array[start];

        var end = System.Math.Min(start + length, array.Length);

        for (int i = start; i < end; i++)
        {
            T value = array[i];
            returnMin = (value < returnMin) ? value : returnMin;
            returnMax = (value > returnMax) ? value : returnMax;
        }

        return (returnMax, returnMin);
    }


    /// <summary>
    /// Retrieves the maximum and minimum values from an array of floats.
    /// </summary>
    /// <param name="array">An array containing float values.</param>
    /// <returns>A tuple containing the maximum and minimum values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float max, float min) FindExtremeValue(this float[] array)
    {
        // Check if the array is null
        if (array is null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        // Initialize the minimum and maximum values
        var returnMin = array[0];
        var returnMax = array[0];

        // Iterate through the array to find the minimum and maximum values
        for (int i = 1; i < array.Length; i++)
        {
            float value = array[i];
            returnMin = (value < returnMin) ? value : returnMin;
            returnMax = (value > returnMax) ? value : returnMax;
        }

        // Return the maximum and minimum values
        return (returnMax, returnMin);
    }


    /// <summary>
    /// Finds the maximum and minimum values in an array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array. Must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="array">The array to search for extreme values.</param>
    /// <returns>A tuple containing the maximum and minimum values in the array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T max, T min) FindExtremeValue<T>(this T[] array)
        where T : INumber<T>
    {
        // Check if the array is null
        if (array is null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        // Initialize the minimum and maximum values
        var returnMin = array[0];
        var returnMax = array[0];

        // Iterate through the array to find the minimum and maximum values
        for (int i = 1; i < array.Length; i++)
        {
            T value = array[i];
            returnMin = (value < returnMin) ? value : returnMin;
            returnMax = (value > returnMax) ? value : returnMax;
        }

        // Return the maximum and minimum values
        return (returnMax, returnMin);
    }




    /// <summary>
    /// Finds the maximum and minimum values in the given array segment.
    /// </summary>
    /// <param name="arraySegment">The segment of the array to search.</param>
    /// <returns>A tuple containing the maximum and minimum values in the array segment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array in the segment is null.</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float max, float min) FindExtremeValue(
        this ArraySegment<float> arraySegment)
    {
        if (arraySegment.Array is null)
            throw new ArgumentNullException(nameof(arraySegment.Array), "Array cannot be null.");

        float returnMin = arraySegment[0];
        float returnMax = arraySegment[0];

        for (int i = 1; i < arraySegment.Count; i++)
        {
            float value = arraySegment[i];
            if (value < returnMin) returnMin = value;
            if (value > returnMax) returnMax = value;
        }

        return (returnMax, returnMin);
    }


    /// <summary>
    /// Finds the maximum and minimum values in an <see cref="ArraySegment{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array segment. Must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="arraySegment">The array segment to search for extreme values.</param>
    /// <returns>A tuple containing the maximum and minimum values in the array segment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array segment's array is null.</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T max, T min) FindExtremeValue<T>(
        this ArraySegment<T> arraySegment)
        where T : INumber<T>
    {
        if (arraySegment.Array is null)
            throw new ArgumentNullException(nameof(arraySegment.Array), "Array cannot be null.");

        T returnMin = arraySegment[0];
        T returnMax = arraySegment[0];

        for (int i = 1; i < arraySegment.Count; i++)
        {
            T value = arraySegment[i];
            if (value < returnMin) returnMin = value;
            if (value > returnMax) returnMax = value;
        }

        return (returnMax, returnMin);
    }



    #endregion





    #region 硬件加速的 GetExtremeValue

    /// <summary>
    /// Finds the maximum and minimum values in the given span.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span. Must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="span">The span of elements to search.</param>
    /// <returns>A tuple containing the maximum and minimum values in the span.</returns>
    /// <exception cref="ArgumentException">Thrown when the span is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (T max, T min) FindExtremeValue_Normal<T>(this Span<T> span)
        where T : unmanaged, INumber<T>
    {
        var max = span[0];
        var min = span[0];

        for (int i = 1; i < span.Length; i++)
        {
            var current = span[i];
            if (current > max) max = current;
            if (current < min) min = current;
        }

        return (max, min);
    }


    /// <summary>
    /// 获取一段数据中的最大值和最小值，使用 SSE2 指令集进行优化。
    /// </summary>
    /// <param name="segment">包含浮点数的 Span。</param>
    /// <returns>包含最大值和最小值的元组。</returns>
    /// <exception cref="ArgumentException">当数组为空时抛出。</exception>
    /// <exception cref="PlatformNotSupportedException">当平台不支持 SSE2 时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (float max, float min) FindExtremeValue_Vector128(this Span<float> span)
    {
        if (span.IsEmpty)
            throw new ArgumentException("Span 不能为空", nameof(span));

        if (!Sse.IsSupported)
            throw new PlatformNotSupportedException("SSE is not supported on this platform.");

        var vectorSize = Vector128<float>.Count;
        var maxVector = Vector128.Create(float.MinValue);
        var minVector = Vector128.Create(float.MaxValue);

        int i = 0;
        for (; i <= span.Length - vectorSize; i += vectorSize)
        {
            //var currentVector = Vector128.Create(span[i], span[i + 1], span[i + 2], span[i + 3]);
            var currentVector = Vector128.LoadUnsafe(ref span[i]);
            maxVector = Sse.Max(currentVector, maxVector);
            minVector = Sse.Min(currentVector, minVector);
        }

        // 提取向量中的最大值和最小值
        //float max = maxVector.ToScalar();
        //float min = minVector.ToScalar();
        float max = maxVector[0];
        float min = minVector[0];
        for (int j = 1; j < vectorSize; j++)
        {
            //max = Math.Max(max, maxVector.GetElement(j));
            //min = Math.Min(min, minVector.GetElement(j));   
            max = Math.Max(max, maxVector[j]);
            min = Math.Min(min, minVector[j]);
        }

        // 处理剩余的数据
        for (; i < span.Length; i++)
        {
            max = Math.Max(span[i], max);
            min = Math.Min(span[i], min);
        }

        return (max, min);
    }


    /// <summary>
    /// 使用 AVX2 指令集优化，从浮点数的 Span 中获取最大值和最小值。
    /// </summary>
    /// <param name="span">包含浮点数的 Span。</param>
    /// <returns>包含最大值和最小值的元组。</returns>
    /// <exception cref="ArgumentException">当数组为空时抛出。</exception>
    /// <exception cref="PlatformNotSupportedException">当平台不支持 AVX2 时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (float max, float min) FindExtremeValue_Vector256(this Span<float> span)
    {
        // 检查数组是否为空
        if (span == null || span.Length == 0)
            throw new ArgumentException("Array is null or empty");

        // 检查平台是否支持 AVX2
        if (!Avx2.IsSupported)
            throw new PlatformNotSupportedException("Avx2 is not supported on this platform.");

        var vectorSize = Vector256<float>.Count;
        // 初始化最大值和最小值向量
        var maxVector = Vector256.Create(float.MinValue); // 不要用 CreateScalar()，否则只会填充第一个元素，其余为0
        var minVector = Vector256.Create(float.MaxValue); // 不要用 CreateScalar()，否则只会填充第一个元素，其余为0

        int i = 0;
        // 使用 AVX2 指令集处理数据
        for (; i <= span.Length - vectorSize; i += vectorSize)
        {
            //var currentVector = Vector256.Create(segment[i], segment[i + 1], segment[i + 2], segment[i + 3],
            //                                     segment[i + 4], segment[i + 5], segment[i + 6], segment[i + 7]);
            var currentVector = Vector256.LoadUnsafe(ref span[i]);
            maxVector = Avx2.Max(currentVector, maxVector);
            minVector = Avx2.Min(currentVector, minVector);
        }

        // 提取向量中的最大值和最小值
        //float max = maxVector.ToScalar();
        //float min = minVector.ToScalar();
        float max = maxVector[0];
        float min = minVector[0];
        for (int j = 1; j < vectorSize; j++)
        {  
            //max = Math.Max(max, maxVector.GetElement(j));
            //min = Math.Min(min, minVector.GetElement(j));   
            max = Math.Max(max, maxVector[j]);
            min = Math.Min(min, minVector[j]);
        }

        // 处理剩余的数据
        for (; i < span.Length; i++)
        {
            max = Math.Max(span[i], max);
            min = Math.Min(span[i], min);
        }

        return (max, min);
    }


    /// <summary>
    /// 使用 AVX512 指令集优化，从浮点数的 Span 中获取最大值和最小值。
    /// </summary>
    /// <param name="segment">包含浮点数的 Span。</param>
    /// <returns>包含最大值和最小值的元组。</returns>
    /// <exception cref="ArgumentException">当数组为空时抛出。</exception>
    /// <exception cref="PlatformNotSupportedException">当平台不支持 AVX512 时抛出。</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (float max, float min) FindExtremeValue_Vector512(Span<float> segment)
    {
        // 检查数组是否为空
        if (segment == null || segment.Length == 0)
            throw new ArgumentException("Array is null or empty");

        // 检查平台是否支持 AVX512
        if (!Avx512F.IsSupported)
            throw new PlatformNotSupportedException("Avx512F is not supported on this platform.");

        var vectorSize = Vector512<float>.Count;
        var maxVector = Vector512.Create(float.MinValue);
        var minVector = Vector512.Create(float.MaxValue);

        int i = 0;
        // 使用 AVX512 指令集处理数据
        for (; i <= segment.Length - vectorSize; i += vectorSize)
        {
            //var currentVector = Vector512.Create(segment[i], segment[i + 1], segment[i + 2], segment[i + 3],
            //                                     segment[i + 4], segment[i + 5], segment[i + 6], segment[i + 7],
            //                                     segment[i + 8], segment[i + 9], segment[i + 10], segment[i + 11],
            //                                     segment[i + 12], segment[i + 13], segment[i + 14], segment[i + 15]);
            var currentVector = Vector512.LoadUnsafe(ref segment[i]);
            maxVector = Avx512F.Max(currentVector, maxVector);
            minVector = Avx512F.Min(currentVector, minVector);
        }

        // 提取向量中的最大值和最小值
        float max = maxVector[0];
        float min = minVector[0];
        for (int j = 1; j < vectorSize; j++)
        {
            max = Math.Max(max, maxVector[j]);
            min = Math.Min(min, minVector[j]);
        }

        // 处理剩余的数据
        for (; i < segment.Length; i++)
        {
            max = Math.Max(segment[i], max);
            min = Math.Min(segment[i], min);
        }

        return (max, min);
    }



    /// <summary>
    /// Retrieves the maximum and minimum values from a span of floats, selecting the optimal vectorized method based on hardware support.
    /// </summary>
    /// <param name="segment">A span containing float values.</param>
    /// <returns>A tuple containing the maximum and minimum values.</returns>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float max, float min) FindExtremeValue(this Span<float> segment)
    {
        // 如果硬件支持 AVX512，则使用 AVX512 方法
        if (Vector512.IsHardwareAccelerated)
            return FindExtremeValue_Vector512(segment);
        // 如果硬件支持 AVX2，则使用 AVX2 方法
        else if (Vector256.IsHardwareAccelerated)
            return FindExtremeValue_Vector256(segment);
        // 如果硬件支持 SSE2，则使用 SSE2 方法
        else if (Vector128.IsHardwareAccelerated)
            return FindExtremeValue_Vector128(segment);
        // 否则，使用普通方法
        else
            return FindExtremeValue_Normal(segment);
    }

    #endregion





































}
