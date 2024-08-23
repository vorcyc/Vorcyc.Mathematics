using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

public static partial class SBasic
{


    #region float version

    /// <summary>
    /// Finds the minimum value in an array of floats.
    /// </summary>
    /// <param name="array">The array of floats to search.</param>
    /// <returns>The minimum value in the array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="array"/> is empty.</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min(this float[] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (array.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(array));

        var result = array[0];
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] < result)
                result = array[i];
        }
        return result;
    }


    /// <summary>
    /// Finds the minimum value in a subarray of floats.
    /// </summary>
    /// <param name="array">The array of floats to search.</param>
    /// <param name="start">The starting index of the subarray.</param>
    /// <param name="length">The length of the subarray.</param>
    /// <returns>The minimum value in the specified subarray.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> or <paramref name="length"/> is out of range.</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min(this float[] array, int start, int length)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (start < 0 || length < 0 || start + length > array.Length)
            throw new ArgumentOutOfRangeException("Start or length is out of range.");

        float result = array[start];
        for (int i = start + 1; i < start + length; i++)
        {
            if (array[i] < result)
                result = array[i];
        }
        return result;
    }


    #endregion





    #region generic version

    #region Span<T> and INumber<T>

    /// <summary>
    /// Finds the minimum value in a span of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="span">The span of values to search.</param>
    /// <returns>The minimum value in the span.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="span"/> is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(this Span<T> span)
        where T : INumber<T>
    {
        if (span.Length == 0)
            throw new ArgumentException("Span cannot be empty.", nameof(span));

        var result = span[0];
        for (int i = 1; i < span.Length; i++)
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
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    /// Finds the minimum value in a parameter array of values.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the array, which must implement <see cref="IComparable"/> and <see cref="IComparable{TValue}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <returns>The minimum value in the array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="values"/> array is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="values"/> array is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue Min<TValue>(params TValue[] values)
        where TValue : IComparable, IComparable<TValue>
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values), "Array cannot be null.");

        if (values.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(values));

        TValue result = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i].CompareTo(result) < 0)
                result = values[i];
        }
        return result;
    }


    /// <summary>
    /// Locates the minimum value in an array and returns its index and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the array, which must implement <see cref="IComparable"/> and <see cref="IComparable{TValue}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <returns>A tuple containing the index of the minimum value and the minimum value itself.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="values"/> array is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="values"/> array is empty.</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, TValue) LocateMin<TValue>(this TValue[] values)
        where TValue : IComparable, IComparable<TValue>
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values), "Array cannot be null.");

        if (values.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(values));

        TValue result = values[0];
        int resultIndex = 0;

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i].CompareTo(result) < 0)
            {
                result = values[i];
                resultIndex = i;
            }
        }

        return (resultIndex, result);
    }



    /// <summary>
    /// Finds the minimum value in a subarray of values.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the array, which must implement <see cref="IComparable"/> and <see cref="IComparable{TValue}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <param name="start">The starting index of the subarray.</param>
    /// <param name="length">The length of the subarray.</param>
    /// <returns>The minimum value in the specified subarray.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="values"/> array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> or <paramref name="length"/> is out of range.</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue Min<TValue>(this TValue[] values, int start, int length)
        where TValue : IComparable, IComparable<TValue>
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values), "Array cannot be null.");

        if (start < 0 || length < 0 || start + length > values.Length)
            throw new ArgumentOutOfRangeException("Start or length is out of range.");

        TValue result = values[start];
        for (int i = start + 1; i < start + length; i++)
        {
            if (values[i].CompareTo(result) < 0)
                result = values[i];
        }

        return result;
    }



    #endregion



    #endregion





    #region LocateMin


    #region float version


    /// <summary>
    /// Locates the minimum value in a float array and returns its index and value.
    /// </summary>
    /// <param name="array">The array of floats to search.</param>
    /// <returns>A tuple containing the index of the minimum value and the minimum value itself.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="array"/> is empty.</exception>
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
    /// Locates the minimum value in a subarray and returns its index and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the array, which must implement <see cref="IComparable"/> and <see cref="IComparable{TValue}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <param name="start">The starting index of the subarray.</param>
    /// <param name="length">The length of the subarray.</param>
    /// <returns>A tuple containing the index of the minimum value and the minimum value itself.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="values"/> array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> or <paramref name="length"/> is out of range.</exception>
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
