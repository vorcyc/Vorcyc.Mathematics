using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Provides extension methods for <see cref="IComparable{T}"/> and <see cref="IComparable"/>
/// make it easy for human.
/// </summary>
public static partial class IComparableExtension
{
    //    在英文编程中，当命名一个方法表示 "小于或等于" 时，通常使用 "Equal" 而不是 "Equals"。这是因为 "Equal" 在这种情况下作为形容词，描述两个值的关系，而 "Equals" 通常是动词，用于指示执行比较的动作。因此，正确的方法名应该是 "LessThanOrEqual"。例如：

    //```csharp
    //public bool LessThanOrEqual(int a, int b)
    //    {
    //        return a <= b;
    //    }
    //```

    //这样的命名更符合编程中的习惯和逻辑表达。如果您有其他编程问题或需要帮助，请随时告诉我！


    #region 简化 IComparable 


    /// <summary>
    /// Determines whether the first value is less than the second value.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns><c>true</c> if the first value is less than the second value; otherwise, <c>false</c>.</returns>
    public static bool LessThan<T>(this T value1, T value2)
        where T : IComparable, IComparable<T>
    {
        return value1.CompareTo(value2) == -1;
    }

    /// <summary>
    /// Determines whether the first value is less than or equal to the second value.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns><c>true</c> if the first value is less than or equal to the second value; otherwise, <c>false</c>.</returns>
    public static bool LessThanOrEqual<T>(this T value1, T value2)
        where T : IComparable, IComparable<T>
    {
        return (value1.CompareTo(value2) == -1) || (value1.CompareTo(value2) == 0);
    }

    /// <summary>
    /// Determines whether the first value is greater than the second value.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns><c>true</c> if the first value is greater than the second value; otherwise, <c>false</c>.</returns>
    public static bool GreaterThan<T>(this T value1, T value2)
        where T : IComparable, IComparable<T>
    {
        return value1.CompareTo(value2) == 1;
    }

    /// <summary>
    /// Determines whether the first value is greater than or equal to the second value.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns><c>true</c> if the first value is greater than or equal to the second value; otherwise, <c>false</c>.</returns>
    public static bool GreaterThanOrEqual<T>(this T value1, T value2)
        where T : IComparable, IComparable<T>
    {
        return (value1.CompareTo(value2) == 1) || (value1.CompareTo(value2) == 0);
    }

    /// <summary>
    /// Determines whether the first value is equal to the second value.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns><c>true</c> if the first value is equal to the second value; otherwise, <c>false</c>.</returns>
    public static bool Equal<T>(this T value1, T value2)
        where T : IComparable, IComparable<T>
    {
        return value1.CompareTo(value2) == 0;
    }

    /// <summary>
    /// Determines whether the first value is not equal to the second value.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns><c>true</c> if the first value is not equal to the second value; otherwise, <c>false</c>.</returns>
    public static bool NotEqual<T>(this T value1, T value2)
        where T : IComparable, IComparable<T>
        => value1.CompareTo(value2) != 0;



    #endregion


    #region 双参数 Max 和 Min

    /// <summary>
    /// Returns the maximum of two values.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns>The maximum of the two values.</returns>
    public static T Max<T>(this T value1, T value2)
        where T : IComparable, IComparable<T>
        => value1.CompareTo(value2) == 1 ? value1 : value2;

    /// <summary>
    /// Returns the minimum of two values.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns>The minimum of the two values.</returns>
    public static T Min<T>(this T value1, T value2)
        where T : IComparable, IComparable<T>
        => value1.CompareTo(value2) == 1 ? value2 : value1;


    #endregion


    #region 数组和 Span 的 Max 和 Min

    /// <summary>
    /// Returns the maximum value in an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <returns>The maximum value in the array.</returns>
    public static T Max<T>(this T[] values)
        where T : IComparable, IComparable<T>
    {
        T result = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i].GreaterThan(result))
                result = values[i];
        }
        return result;
    }



    /// <summary>
    /// Returns the maximum value in a specified range of an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <param name="start">The starting index of the range.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>The maximum value in the specified range of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(this T[] values, int start, int length)
        where T : IComparable, IComparable<T>
    {
        T result = values[start];
        for (int i = start; i < start + length; i++)
        {
            if (values[i].GreaterThan(result))
                result = values[i];
        }
        return result;
    }

    /// <summary>
    /// Returns the maximum value in a span.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="span">The span of values.</param>
    /// <returns>The maximum value in the span.</returns>
    public static T Max<T>(this Span<T> span)
        where T : IComparable, IComparable<T>
    {
        T result = span[0];
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].GreaterThan(result))
                result = span[i];
        }
        return result;
    }


    /// <summary>
    /// Returns the minimum value in an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <returns>The minimum value in the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(this T[] values)
        where T : IComparable, IComparable<T>
    {
        T result = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i].CompareTo(result) == -1)
                result = values[i];
        }
        return result;
    }


    /// <summary>
    /// Returns the minimum value in a specified range of an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <param name="start">The starting index of the range.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>The minimum value in the specified range of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(this T[] values, int start, int length)
        where T : IComparable, IComparable<T>
    {
        T result = values[start];
        for (int i = start; i < start + length; i++)
        {
            if (values[i].LessThan(result))
                result = values[i];
        }
        return result;
    }



    /// <summary>
    /// Returns the minimum value in a span.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="span">The span of values.</param>
    /// <returns>The minimum value in the span.</returns>
    public static T Min<T>(this Span<T> span)
        where T : IComparable, IComparable<T>
    {
        T result = span[0];
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].LessThan(result))
                result = span[i];
        }
        return result;
    }

    #endregion


    #region MaxMin

    /// <summary>
    /// Returns the maximum and minimum values in an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <returns>A tuple containing the maximum and minimum values in the array.</returns>
    public static (T max, T min) MaxMin<T>(this T[] values)
        where T : IComparable, IComparable<T>
    {
        T max = values[0], min = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i].GreaterThan(max))
                max = values[i];
            if (values[i].LessThan(min))
                min = values[i];
        }
        return (max, min);
    }


    /// <summary>
    /// Returns the maximum and minimum values in a specified range of an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <param name="start">The starting index of the range.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>A tuple containing the maximum and minimum values in the specified range of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T max, T min) MaxMin<T>(this T[] values, int start, int length)
        where T : IComparable, IComparable<T>
    {
        T max = values[start], min = values[start];
        for (int i = start; i < start + length; i++)
        {
            if (values[i].GreaterThan(max))
                max = values[i];
            if (values[i].LessThan(min))
                min = values[i];
        }
        return (max, min);
    }


    /// <summary>
    /// Returns the maximum and minimum values in a span.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="span">The span of values.</param>
    /// <returns>A tuple containing the maximum and minimum values in the span.</returns>
    public static (T max, T min) MaxMin<T>(this Span<T> span)
        where T : IComparable, IComparable<T>
    {
        T max = span[0], min = span[0];
        for (int i = 1; i < span.Length; i++)
        {
            if (span[i].GreaterThan(max))
                max = span[i];
            if (span[i].LessThan(min))
                min = span[i];
        }
        return (max, min);
    }

    #endregion


    #region LocateMax

    /// <summary>
    /// Returns the index and value of the maximum element in an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <returns>A tuple containing the index and value of the maximum element in the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMax<T>(this T[] values)
        where T : IComparable, IComparable<T>
    {
        T result = values[0];
        int resultIndex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].GreaterThan(result))
            {
                result = values[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }

    /// <summary>
    /// Returns the index and value of the maximum element in a specified range of an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <param name="start">The starting index of the range.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>A tuple containing the index and value of the maximum element in the specified range of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMax<T>(this T[] values, int start, int length)
        where T : IComparable, IComparable<T>
    {
        T result = values[start];
        int resultIndex = 0;
        for (int i = start; i < start + length; i++)
        {
            if (values[i].GreaterThan(result))
            {
                result = values[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }

    /// <summary>
    /// Returns the index and value of the maximum element in a span.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="span">The span of values.</param>
    /// <returns>A tuple containing the index and value of the maximum element in the span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMax<T>(this Span<T> span)
        where T : IComparable, IComparable<T>
    {
        T result = span[0];
        int resultIndex = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].GreaterThan(result))
            {
                result = span[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }


    #endregion


    #region LocateMin

    /// <summary>
    /// Returns the index and value of the minimum element in an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <returns>A tuple containing the index and value of the minimum element in the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMin<T>(this T[] values)
        where T : IComparable, IComparable<T>
    {
        T result = values[0];
        int resultIndex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].LessThan(result))
            {
                result = values[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }

    /// <summary>
    /// Returns the index and value of the minimum element in a specified range of an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <param name="start">The starting index of the range.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>A tuple containing the index and value of the minimum element in the specified range of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMin<T>(this T[] values, int start, int length)
        where T : IComparable, IComparable<T>
    {
        T result = values[start];
        int resultIndex = 0;
        for (int i = start; i < start + length; i++)
        {
            if (values[i].LessThan(result))
            {
                result = values[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }

    /// <summary>
    /// Returns the index and value of the minimum element in a span.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="span">The span of values.</param>
    /// <returns>A tuple containing the index and value of the minimum element in the span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, T value) LocateMin<T>(this Span<T> span)
        where T : IComparable, IComparable<T>
    {
        T result = span[0];
        int resultIndex = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].LessThan(result))
            {
                result = span[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }

    #endregion

}