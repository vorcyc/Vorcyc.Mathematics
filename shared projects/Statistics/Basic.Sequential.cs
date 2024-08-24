﻿using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

public static partial class SBasic
{


    #region Max Min

    /// <summary>
    /// 找最大值和最小值，并返回TupleValue&lt;T1,T2&gt;
    /// </summary>
    /// <param name="array"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [method:MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float max, float min) MaxMin(
        this float[] array,
        int start, int length)
    {
        var returnMin = float.MaxValue;
        var returnMax = float.MinValue;

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
    /// Finds the maximum and minimum values in the given array segment.
    /// </summary>
    /// <param name="arraySegment">The segment of the array to search.</param>
    /// <returns>A tuple containing the maximum and minimum values in the array segment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array in the segment is null.</exception>
    [method:MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float max, float min) MaxMin(
        this ArraySegment<float> arraySegment)
    {
        if (arraySegment.Array == null)
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
    /// Finds the maximum and minimum values in the given span.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span. Must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="span">The span of elements to search.</param>
    /// <returns>A tuple containing the maximum and minimum values in the span.</returns>
    /// <exception cref="ArgumentException">Thrown when the span is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T max, T min) MaxMin<T>(this Span<T> span)
        where T : INumber<T>
    {
        var max = span[0];
        var min = span[0];

        for (int i = 1; i < span.Length; i++)
        {
            if (span[i] > max) max = span[i];
            if (span[i] < min) min = span[i];
        }

        return (max, min);
    }



    #endregion



    #region Average

    /// <summary>
    /// Calculates the average of the elements in an array of floats.
    /// </summary>
    /// <param name="array">The array of floats to calculate the average of.</param>
    /// <returns>The average of the elements in the array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="array"/> is empty.</exception>    
    public static float Average(this float[] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (array.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(array));

        float result = 0.0f;

        for (int i = 0; i < array.Length; i++)
        {
            result += array[i];
        }
        return result / array.Length;
    }



    /// <summary>
    /// Calculates the average of the elements in a subarray of floats.
    /// </summary>
    /// <param name="array">The array of floats to calculate the average of.</param>
    /// <param name="start">The starting index of the subarray.</param>
    /// <param name="length">The length of the subarray.</param>
    /// <returns>The average of the elements in the specified subarray.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> or <paramref name="length"/> is out of range.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining)]
    public static float Average(this float[] array, int start, int length)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (start < 0 || length < 0 || start + length > array.Length)
            throw new ArgumentOutOfRangeException("Start or length is out of range.");

        float result = 0.0f;

        var end = System.Math.Min(start + length, array.Length);

        for (int i = start; i < end; i++)
        {
            result += array[i];
        }
        return result / length;
    }



    /// <summary>
    /// Calculates the average of the elements in an array of bytes.
    /// </summary>
    /// <param name="array">The array of bytes to calculate the average of.</param>
    /// <returns>The average of the elements in the array as a double.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="array"/> is empty.</exception>
    [method:MethodImpl( MethodImplOptions.AggressiveInlining)]
    public static double Average(this byte[] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (array.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(array));

        long sum = 0;
        for (int i = 0; i < array.Length; i++)
        {
            sum += array[i];
        }
        return (double)sum / array.Length;
    }



    /// <summary>
    /// Calculates the average of the elements in a span.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="span">The span of values to calculate the average of.</param>
    /// <returns>The average of the elements in the span.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="span"/> is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Average<T>(this Span<T> span)
        where T : INumber<T>
    {
        if (span.Length == 0)
            throw new ArgumentException("Span cannot be empty.", nameof(span));

        T sum = T.Zero;
        for (int i = 0; i < span.Length; i++)
        {
            sum += span[i];
        }
        return sum / T.CreateChecked(span.Length);
    }


    #endregion


    #region Variance

    /// <summary>
    /// Calculates the average and variance of the elements in an array of floats.
    /// </summary>
    /// <param name="array">The array of floats to calculate the average and variance of.</param>
    /// <returns>A tuple containing the average and variance of the elements in the array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="array"/> is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float average, float variance) Variance(this float[] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (array.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(array));

        var mean = Average(array);
        var result = 0.0f;

        for (int i = 0; i < array.Length; i++)
        {
            var v = array[i];
            result += (v - mean) * (v - mean);
        }

        result /= array.Length - 1;
        return (mean, result);
    }



    /// <summary>
    /// Calculates the average and variance of the elements in a subarray of floats.
    /// </summary>
    /// <param name="array">The array of floats to calculate the average and variance of.</param>
    /// <param name="start">The starting index of the subarray.</param>
    /// <param name="length">The length of the subarray.</param>
    /// <returns>A tuple containing the average and variance of the elements in the specified subarray.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> or <paramref name="length"/> is out of range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float average, float variance) Variance(this float[] array, int start, int length)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (start < 0 || length < 0 || start + length > array.Length)
            throw new ArgumentOutOfRangeException("Start or length is out of range.");

        var mean = Average(array, start, length);
        var result = 0.0f;

        var end = System.Math.Min(start + length, array.Length);

        for (int i = start; i < end; i++)
        {
            var v = array[i];
            result += (v - mean) * (v - mean);
        }

        result /= length - 1;
        return (mean, result);
    }



    /// <summary>
    /// Calculates the average and variance of the elements in a span.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="array">The span of values to calculate the average and variance of.</param>
    /// <returns>A tuple containing the average and variance of the elements in the span.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="array"/> is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T average, T variance) Variance<T>(this Span<T> array)
        where T : INumber<T>
    {
        if (array.Length == 0)
            throw new ArgumentException("Span cannot be empty.", nameof(array));

        var mean = Average(array);
        var result = T.Zero;

        for (int i = 0; i < array.Length; i++)
        {
            var v = array[i];
            result += (v - mean) * (v - mean);
        }

        result /= T.CreateChecked(array.Length - 1);
        return (mean, result);
    }



    #endregion


    #region Max min median

    /// <summary>
    /// Finds the maximum, minimum, and median values in an array of floats.
    /// </summary>
    /// <param name="array">The array of floats to analyze.</param>
    /// <returns>A tuple containing the maximum, minimum, and median values in the array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="array"/> is empty.</exception>
    public static (float max, float min, float median) GetMaximumMinimumMedian(this float[] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (array.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(array));

        var t = (float[])array.Clone();
        Array.Sort(t);
        return (t[array.Length - 1], t[0], t[array.Length / 2]);
    }


    /// <summary>
    /// Gets Max Min median
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static (float max, float min, float median) Get3M(this float[] array)
    {
        var temp = (float[])array.Clone();
        Array.Sort(temp);
        return (temp[array.Length - 1], temp[0], temp[array.Length / 2]);
    }

    #endregion




}
