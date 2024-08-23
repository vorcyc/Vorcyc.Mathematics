using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

public static partial class SBasic
{


    #region float version

    /// <summary>
    /// 返回一组数字中的最小值
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static float Min(this float[] array)
    {
        var result = array[0];
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] < result) result = array[i];
        }
        return result;
    }




    public static float Min(this float[] array, int start, int length)
    {
        float result = array[start];
        for (int i = start; i < start + length; i++)
        {
            if (array[i] < result) result = array[i];
        }
        return result;
    }

    #endregion





    #region generic version

    #region Span<T> and INumber<T>

    /// <summary>
    /// Gets the min value in <see cref="Span{T}"/> of type <see cref="INumberBase{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(this Span<T> span)
        where T : INumber<T>
    {
        var result = span[0];
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] < result)
                result = span[i];
        }
        return result;
    }

    /// <summary>
    /// Gets the min value and it's index in <see cref="Span{T}"/> of type <see cref="INumberBase{TSelf}"/>.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="span"></param>
    /// <returns></returns>
    [method:MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, TValue value) LocateMin<TValue>(this Span<TValue> span)
        where TValue : INumber<TValue>
    {
        TValue result = span[0];
        int resultIndex = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] < result)
            {
                result = span[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }

    #endregion


    #region IComparable

    /// <summary>
    /// Sequential version
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="values"></param>
    /// <returns></returns>
    public static TValue Min<TValue>(params TValue[] values)
        where TValue : IComparable, IComparable<TValue>
    {
        TValue result = values[0];
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].CompareTo(result) == -1)
                result = values[i];
        }
        return result;
    }


    public static (int, TValue) LocateMin<TValue>(this TValue[] values)
        where TValue : IComparable, IComparable<TValue>
    {
        TValue result = values[0];
        int resultIndex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].CompareTo(result) == -1)
            {
                result = values[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }


    public static TValue Min<TValue>(this TValue[] values, int start, int length)
        where TValue : IComparable, IComparable<TValue>
    {
        TValue result = values[start];
        for (int i = start; i < start + length; i++)
        {
            if (values[i].CompareTo(result) == -1)
                result = values[i];
        }

        return result;
    }


    #endregion



    #endregion





    #region LocateMin


    #region float version


    /// <summary>
    /// 返回最大值和其索引
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, float min) LocateMin(this float[] array)
    {
        ref float retMin = ref array[0];//= float.MinValue;
        var retIndex = 0;

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] < retMin)
            {
                retMin = ref array[i];
                retIndex = i;
            }
        }

        return (retIndex, retMin);
    }


    /// <summary>
    /// Finds the minimum value and its index in the specified range of the array.
    /// </summary>
    /// <param name="array">The array to search.</param>
    /// <param name="start">The starting index of the range to search.</param>
    /// <param name="length">The length of the range to search.</param>
    /// <returns>A tuple containing the index of the minimum value and the minimum value itself.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the start index is out of range or the length is out of range.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, float min) LocateMin(this float[] array, int start, int length)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");
        if (start < 0 || start >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(start), "Start index is out of range.");
        if (length < 0 || start + length > array.Length)
            throw new ArgumentOutOfRangeException(nameof(length), "Length is out of range.");

        ref var retMin = ref array[start];
        var retIndex = start;

        var end = System.Math.Min(start + length, array.Length);

        for (int i = start + 1; i < end; i++)
        {
            if (array[i] < retMin)
            {
                retMin = ref array[i];
                retIndex = i;
            }
        }

        return (retIndex, retMin);
    }

    #endregion


    #region generic version

    /// <summary>
    /// Returns the maximum value and its index in a sequential sequence of values.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="values"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, TValue) LocateMin<TValue>(this TValue[] values, int start, int length)
        where TValue : IComparable, IComparable<TValue>
    {
        TValue result = values[start];
        int resultIndex = 0;
        for (int i = start; i < start + length; i++)
        {
            if (values[i].CompareTo(result) == -1)
            {
                result = values[i];
                resultIndex = i;
            }
        }

        return (resultIndex, result);
    }


    #endregion



    #endregion







}
