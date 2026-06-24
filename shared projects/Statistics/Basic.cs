//基本统计函数
//1.	均值 (Mean): 计算一组数据的平均值。
//2.	中位数 (Median): 计算一组数据的中位数。
//3.	众数 (Mode): 计算一组数据中出现频率最高的值。
//4.	方差 (Variance): 计算一组数据的方差。
//5.	标准差 (Standard Deviation): 计算一组数据的标准差。
//6.	变异系数 (Coefficient of Variation): 衡量数据的离散程度。  
namespace Vorcyc.Mathematics.Statistics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides basic statistical functions, including methods for computing the mean, median, mode, variance, standard deviation, and coefficient of variation.
/// </summary>
/// <remarks>
/// This class contains computation methods for the following basic statistical functions:
/// <list type="bullet">
/// <item>
/// <description>Mean: Computes the average value of a set of data.</description>
/// </item>
/// <item>
/// <description>Median: Computes the median of a set of data.</description>
/// </item>
/// <item>
/// <description>Mode: Computes the most frequently occurring value in a set of data.</description>
/// </item>
/// <item>
/// <description>Variance: Computes the variance of a set of data.</description>
/// </item>
/// <item>
/// <description>Standard Deviation: Computes the standard deviation of a set of data.</description>
/// </item>
/// <item>
/// <description>Coefficient of Variation: Measures the dispersion of the data.</description>
/// </item>
/// </list>
/// </remarks>
public static partial class Basic
{

    /// <summary>
    /// Computes the sum of the elements in a set of values, optimized using SIMD.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set of values, which must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="values">The set of values whose sum is to be computed.</param>
    /// <returns>The sum of the elements in the set of values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <remarks>Sum: Returns the sum of all elements in a set of data, used to represent the overall magnitude of the data.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(this Span<T> values)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        int length = values.Length;
        int simdLength = Vector<T>.Count;
        int remainder = length % simdLength;
        Vector<T> sumVector = Vector<T>.Zero;
        int i = 0;
        // 以Vector<T>.Count为单位处理数据
        for (; i < length - remainder; i += simdLength)
        {
            Vector<T> vector = new Vector<T>(values.Slice(i, simdLength));
            sumVector += vector;
        }
        // 求和sumVector的元素
        T sum = T.Zero;
        for (int j = 0; j < simdLength; j++)
        {
            sum += sumVector[j];
        }
        // 处理剩余的元素
        for (; i < length; i++)
        {
            sum += values[i];
        }
        return sum;
    }

    /// <summary>
    /// Computes the sum of the elements in a set of values, using the specified selector function to select each value.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set of values, which must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="values">The set of values whose sum is to be computed.</param>
    /// <param name="selector">The function used to select each value.</param>
    /// <returns>The sum of the elements in the set of values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <remarks>Sum: Returns the sum of all elements in a set of data, used to represent the overall magnitude of the data.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(this Span<T> values, Func<T, T> selector)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        T sum = T.Zero;
        for (int i = 0; i < values.Length; i++)
        {
            sum += selector(values[i]);
        }
        return sum;
    }

    /// <summary>
    /// Computes the average value of the elements in a set of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set of values, which must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="values">The set of values whose average is to be computed.</param>
    /// <returns>The average value of the elements in the set of values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <remarks>Mean: Returns the average value of a set of data, used to represent the central tendency of the data.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Average<T>(this Span<T> values)
    where T : INumber<T>
    {
        if (values.Length == 0)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        T sum = Sum(values);
        return sum / T.CreateChecked(values.Length);
    }

    /// <summary>
    /// Computes the median of the elements in a set of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set of values, which must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="values">The set of values whose median is to be computed.</param>
    /// <returns>The median of the elements in the set of values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <remarks>Median: Returns the median of a set of data, used to represent the middle value of the data, which effectively reflects the distribution of the data.</remarks>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Median<T>(this Span<T> values)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        int length = values.Length;
        var sortedArray = new T[length];
        values.CopyTo(sortedArray);
        Array.Sort(sortedArray);
        if (length % 2 == 0)
        {
            var mid = length / 2;
            return (sortedArray[mid - 1] + sortedArray[mid]) / T.CreateChecked(2);
        }
        else
        {
            return sortedArray[length / 2];
        }
    }

    /// <summary>
    /// Computes the mode of the elements in a set of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set of values, which must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="values">The set of values whose mode is to be computed.</param>
    /// <returns>The most frequently occurring element in the set of values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <remarks>Mode: Returns the most frequently occurring value in a set of data, used to represent the most common value in the data.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Mode<T>(this Span<T> values)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        var frequency = new Dictionary<T, int>();
        foreach (var value in values)
        {
            if (frequency.ContainsKey(value))
            {
                frequency[value]++;
            }
            else
            {
                frequency[value] = 1;
            }
        }
        var maxFrequency = frequency.Values.Max();
        return frequency.First(kvp => kvp.Value == maxFrequency).Key;
    }

    /// <summary>
    /// Computes the average value and variance of the elements in a set of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set of values, which must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="values">The set of values whose average and variance are to be computed.</param>
    /// <returns>A tuple containing the average value and variance of the elements in the set of values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <remarks>Variance: Returns the variance of a set of data, used to represent the dispersion of the data.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T average, T variance) Variance<T>(this Span<T> values)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        var mean = Average(values);
        var result = T.Zero;
        int length = values.Length;
        int simdLength = Vector<T>.Count;
        int remainder = length % simdLength;
        Vector<T> varianceVector = Vector<T>.Zero;
        int i = 0;
        // 以Vector<T>.Count为单位处理数据
        for (; i < length - remainder; i += simdLength)
        {
            Vector<T> vector = new Vector<T>(values.Slice(i, simdLength));
            Vector<T> diff = vector - new Vector<T>(mean);
            varianceVector += diff * diff;
        }
        // 求和varianceVector的元素
        for (int j = 0; j < simdLength; j++)
        {
            result += varianceVector[j];
        }
        // 处理剩余的元素
        for (; i < length; i++)
        {
            var v = values[i];
            result += (v - mean) * (v - mean);
        }
        result /= T.CreateChecked(values.Length - 1);
        return (mean, result);
    }

    /// <summary>
    /// Computes the standard deviation of the elements in a set of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set of values, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="values">The set of values whose standard deviation is to be computed.</param>
    /// <returns>The standard deviation of the elements in the set of values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <remarks>Standard Deviation: Returns the standard deviation of a set of data, used to represent the dispersion of the data.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T StandardDeviation<T>(this Span<T> values)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        var (_, variance) = Variance(values);
        return T.Sqrt(variance);
    }

    /// <summary>
    /// Computes the coefficient of variation of the elements in a set of values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set of values, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="values">The set of values whose coefficient of variation is to be computed.</param>
    /// <returns>The coefficient of variation of the elements in the set of values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <remarks>Coefficient of Variation: Measures the dispersion of the data, representing the ratio of the standard deviation to the mean.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CoefficientOfVariation<T>(this Span<T> values)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        var standardDeviation = StandardDeviation(values);
        var mean = Average(values);
        return standardDeviation / mean;
    }

    /// <summary>
    /// Computes all statistics of a set of values, including the mean, median, mode, variance, standard deviation, and coefficient of variation.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set of values, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="values">The set of values whose statistics are to be computed.</param>
    /// <returns>A tuple containing the mean, median, mode, variance, standard deviation, and coefficient of variation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <remarks>Returns a tuple containing all statistics.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T Mean, T Median, T Mode, T Variance, T StandardDeviation, T CoefficientOfVariation)
        CalculateAllStatistics<T>(this Span<T> values)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        // 初始化变量
        T sum = T.Zero;
        T sumOfSquares = T.Zero;
        var frequency = new Dictionary<T, int>();
        var sortedValues = new T[values.Length];
        values.CopyTo(sortedValues);
        Array.Sort(sortedValues);
        // 遍历 values 计算总和、总和的平方和频率
        for (int i = 0; i < values.Length; i++)
        {
            var value = values[i];
            sum += value;
            sumOfSquares += value * value;
            if (frequency.ContainsKey(value))
            {
                frequency[value]++;
            }
            else
            {
                frequency[value] = 1;
            }
        }
        // 计算均值
        T mean = sum / T.CreateChecked(values.Length);
        // 计算中位数
        T median;
        if (values.Length % 2 == 0)
        {
            median = (sortedValues[values.Length / 2 - 1] + sortedValues[values.Length / 2]) / T.CreateChecked(2);
        }
        else
        {
            median = sortedValues[values.Length / 2];
        }
        // 计算众数
        T mode = frequency.OrderByDescending(kvp => kvp.Value).First().Key;
        // 计算方差
        T variance = (sumOfSquares - sum * sum / T.CreateChecked(values.Length)) / T.CreateChecked(values.Length - 1);
        // 计算标准差
        T standardDeviation = T.Sqrt(variance);
        // 计算变异系数
        T coefficientOfVariation = standardDeviation / mean;
        return (mean, median, mode, variance, standardDeviation, coefficientOfVariation);
    }
}
