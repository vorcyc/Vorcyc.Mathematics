namespace Vorcyc.Mathematics.Helpers;

/// <summary>
/// Provides extension methods for <see cref="IComparable{T}"/> and <see cref="IComparable"/>
/// make it easy for human.
/// </summary>
public static class IComparableExtension
{
    //    在英文编程中，当命名一个方法表示 "小于或等于" 时，通常使用 "Equal" 而不是 "Equals"。这是因为 "Equal" 在这种情况下作为形容词，描述两个值的关系，而 "Equals" 通常是动词，用于指示执行比较的动作。因此，正确的方法名应该是 "LessThanOrEqual"。例如：

    //```csharp
    //public bool LessThanOrEqual(int a, int b)
    //    {
    //        return a <= b;
    //    }
    //```

    //这样的命名更符合编程中的习惯和逻辑表达。如果您有其他编程问题或需要帮助，请随时告诉我！
    
    /// <summary>
    /// Determines whether the first value is less than the second value.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns><c>true</c> if the first value is less than the second value; otherwise, <c>false</c>.</returns>
    public static bool LessThan<T>(this T value1, T value2)
        where T : struct, IComparable, IComparable<T>
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
        where T : struct, IComparable, IComparable<T>
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
        where T : struct, IComparable, IComparable<T>
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
        where T : struct, IComparable, IComparable<T>
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
        where T : struct, IComparable, IComparable<T>
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
        where T : struct, IComparable, IComparable<T>
        => value1.CompareTo(value2) != 0;

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
            if (values[i].CompareTo(result) == 1)
                result = values[i];
        }
        return result;
    }

    /// <summary>
    /// Returns the minimum value in an array.
    /// </summary>
    /// <typeparam name="T">The type of the values. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <returns>The minimum value in the array.</returns>
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
            if (values[i].CompareTo(max) == 1)
                max = values[i];
            if (values[i].CompareTo(min) == -1)
                min = values[i];
        }
        return (max, min);
    }

}