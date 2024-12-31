using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics;

/// <summary>Extension methods for <see cref="INumber{T}"/> and <see cref="ValueType"/> ('struct') with SIMD support.</summary>
public static partial class Statistics
{

    //public static double Max(double a ,double b)
    //{
    //    if (a > b)
    //        return a;
    //    else
    //        return b;            
    //}


    //public static double Max(double a ,double b)
    //{
    //    if (a >= b)
    //        return a;
    //    else
    //        return b;            
    //}


    //public static double Max(double a, double b)
    //{
    //    if (a > b) return a;
    //    return b;
    //}



    //public static double Max(double a, double b)
    //{
    //    return a > b ? a : b;
    //}


    //public static double Max(double a, double b) => a > b ? a : b;
    //public static double Max(float a, float b) => a > b ? a : b;
    //public static double Max(int a, int b) => a > b ? a : b;



    #region 针对 Span<T> and INumber<T>  的 MAX 方法

    /// <summary>
    /// Gets the max value in <see cref="Span{T}"/> of type <see cref="INumberBase{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(this Span<T> span)
           where T : struct, INumber<T>
    {
        if (span.Length == 0)
            throw new ArgumentException("Span cannot be empty.", nameof(span));

        if (Vector.IsHardwareAccelerated && span.Length >= Vector<T>.Count)
        {
            var maxVector = new Vector<T>(span.Slice(0, Vector<T>.Count).ToArray());
            int i;
            for (i = Vector<T>.Count; i <= span.Length - Vector<T>.Count; i += Vector<T>.Count)
            {
                var currentVector = new Vector<T>(span.Slice(i, Vector<T>.Count).ToArray());
                maxVector = Vector.Max(maxVector, currentVector);
            }

            T max = maxVector[0];
            for (int j = 1; j < Vector<T>.Count; j++)
            {
                if (maxVector[j] > max)
                    max = maxVector[j];
            }

            for (; i < span.Length; i++)
            {
                if (span[i] > max)
                    max = span[i];
            }

            return max;
        }
        else
        {
            T result = span[0];
            for (int i = 1; i < span.Length; i++)
            {
                if (span[i] > result)
                    result = span[i];
            }
            return result;
        }
    }

    /// <summary>
    /// Finds the maximum value in an array of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <returns>The maximum value in the array.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="values"/> array is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(this T[] values)
        where T : struct, INumber<T>
    {
        if (values.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(values));

        return Max(values.AsSpan());
    }

    /// <summary>
    /// Finds the maximum value in a subset of an array of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <param name="start">The starting index of the subset to search.</param>
    /// <param name="length">The length of the subset to search.</param>
    /// <returns>The maximum value in the specified subset of the array.</returns>
    /// <exception cref="ArgumentException">Thrown when the specified subset is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(this T[] values, int start, int length)
        where T : struct, INumber<T>
    {
        if (start < 0 || length < 0 || start + length > values.Length)
            throw new ArgumentException("Invalid subset specified.", nameof(values));

        return Max(values.AsSpan(start, length));
    }


    #endregion


    #region LocateMax

    /// <summary>
    /// Gets the max value and it's index in <see cref="Span{T}"/> of type <see cref="INumberBase{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span"></param>
    /// <returns></returns>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMax<T>(this Span<T> span)
        where T : INumber<T>
    {
        T result = span[0];
        int resultIndex = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] > result)
            {
                result = span[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }


    /// <summary>
    /// Gets the maximum value and its index in an array of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <returns>A tuple containing the index and the maximum value in the array.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="values"/> array is empty.</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMax<T>(this T[] values)
        where T : INumber<T>
    {
        if (values.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(values));

        T result = values[0];
        int resultIndex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] > result)
            {
                result = values[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }

    /// <summary>
    /// Gets the maximum value and its index in a subset of an array of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <param name="start">The starting index of the subset to search.</param>
    /// <param name="length">The length of the subset to search.</param>
    /// <returns>A tuple containing the index and the maximum value in the specified subset of the array.</returns>
    /// <exception cref="ArgumentException">Thrown when the specified subset is invalid.</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMax<T>(this T[] values, int start, int length)
        where T : INumber<T>
    {
        if (start < 0 || length < 0 || start + length > values.Length)
            throw new ArgumentException("Invalid subset specified.", nameof(values));

        T result = values[start];
        int resultIndex = start;
        for (int i = start; i < start + length; i++)
        {
            if (values[i] > result)
            {
                result = values[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }


    #endregion


    #region Min


    /// <summary>
    /// Finds the minimum value in a span of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="span">The span of values to search.</param>
    /// <returns>The minimum value in the span.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="span"/> is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(this Span<T> span)
        where T : struct, INumber<T>
    {
        if (span.Length == 0)
            throw new ArgumentException("Span cannot be empty.", nameof(span));

        if (Vector.IsHardwareAccelerated && span.Length >= Vector<T>.Count)
        {
            var minVector = new Vector<T>(span.Slice(0, Vector<T>.Count).ToArray());
            int i;
            for (i = Vector<T>.Count; i <= span.Length - Vector<T>.Count; i += Vector<T>.Count)
            {
                var currentVector = new Vector<T>(span.Slice(i, Vector<T>.Count).ToArray());
                minVector = Vector.Min(minVector, currentVector);
            }

            T min = minVector[0];
            for (int j = 1; j < Vector<T>.Count; j++)
            {
                if (minVector[j] < min)
                    min = minVector[j];
            }

            for (; i < span.Length; i++)
            {
                if (span[i] < min)
                    min = span[i];
            }

            return min;
        }
        else
        {
            T result = span[0];
            for (int i = 1; i < span.Length; i++)
            {
                if (span[i] < result)
                    result = span[i];
            }
            return result;
        }
    }

    /// <summary>
    /// Finds the minimum value in an array of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <returns>The minimum value in the array.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="values"/> array is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(this T[] values)
        where T : struct, INumber<T>
    {
        if (values.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(values));

        return Min(values.AsSpan());
    }

    /// <summary>
    /// Finds the minimum value in a subset of an array of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <param name="start">The starting index of the subset to search.</param>
    /// <param name="length">The length of the subset to search.</param>
    /// <returns>The minimum value in the specified subset of the array.</returns>
    /// <exception cref="ArgumentException">Thrown when the specified subset is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(this T[] values, int start, int length)
        where T : struct, INumber<T>
    {
        if (start < 0 || length < 0 || start + length > values.Length)
            throw new ArgumentException("Invalid subset specified.", nameof(values));

        return Min(values.AsSpan(start, length));
    }


    #endregion


    #region LocateMin

    /// <summary>
    /// Gets the min value and it's index in <see cref="Span{T}"/> of type <see cref="INumberBase{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span"></param>
    /// <returns></returns>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMin<T>(this Span<T> span)
        where T : INumber<T>
    {
        T result = span[0];
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

    /// <summary>
    /// Gets the minimum value and its index in an array of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <returns>A tuple containing the index and the minimum value in the array.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="values"/> array is empty.</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMin<T>(this T[] values)
        where T : INumber<T>
    {
        if (values.Length == 0)
            throw new ArgumentException("Array cannot be empty.", nameof(values));

        T result = values[0];
        int resultIndex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] < result)
            {
                result = values[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }

    /// <summary>
    /// Gets the minimum value and its index in a subset of an array of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="values">The array of values to search.</param>
    /// <param name="start">The starting index of the subset to search.</param>
    /// <param name="length">The length of the subset to search.</param>
    /// <returns>A tuple containing the index and the minimum value in the specified subset of the array.</returns>
    /// <exception cref="ArgumentException">Thrown when the specified subset is invalid.</exception>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMin<T>(this T[] values, int start, int length)
        where T : INumber<T>
    {
        if (start < 0 || length < 0 || start + length > values.Length)
            throw new ArgumentException("Invalid subset specified.", nameof(values));

        T result = values[start];
        int resultIndex = start;
        for (int i = start; i < start + length; i++)
        {
            if (values[i] < result)
            {
                result = values[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }

    #endregion


}
