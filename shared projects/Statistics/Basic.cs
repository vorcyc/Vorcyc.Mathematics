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
/// 提供基本的统计函数，包括均值、中位数、众数、方差、标准差和变异系数的计算方法。
/// </summary>
/// <remarks>
/// 该类包含以下基本统计函数的计算方法：
/// <list type="bullet">
/// <item>
/// <description>均值 (Mean): 计算一组数据的平均值。</description>
/// </item>
/// <item>
/// <description>中位数 (Median): 计算一组数据的中位数。</description>
/// </item>
/// <item>
/// <description>众数 (Mode): 计算一组数据中出现频率最高的值。</description>
/// </item>
/// <item>
/// <description>方差 (Variance): 计算一组数据的方差。</description>
/// </item>
/// <item>
/// <description>标准差 (Standard Deviation): 计算一组数据的标准差。</description>
/// </item>
/// <item>
/// <description>变异系数 (Coefficient of Variation): 衡量数据的离散程度。</description>
/// </item>
/// </list>
/// </remarks>
public static partial class Basic
{

    /// <summary>
    /// 计算一组值中元素的总和，使用SIMD进行优化。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="INumber{T}"/>接口。</typeparam>
    /// <param name="values">要计算总和的值的一组值。</param>
    /// <returns>一组值中元素的总和。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>总和 (Sum): 返回一组数据中所有元素的总和，用于表示数据的整体大小。</remarks>
    [DeviceDependency(DeviceDependency.CPU)]
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
    /// 计算一组值中元素的总和，使用指定的选择器函数进行选择。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="INumber{T}"/>接口。</typeparam>
    /// <param name="values">要计算总和的值的一组值。</param>
    /// <param name="selector">用于选择值的函数。</param>
    /// <returns>一组值中元素的总和。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>总和 (Sum): 返回一组数据中所有元素的总和，用于表示数据的整体大小。</remarks>
    [DeviceDependency(DeviceDependency.CPU)]
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
    /// 计算一组值中元素的平均值。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="INumber{T}"/>接口。</typeparam>
    /// <param name="values">要计算平均值的值的一组值。</param>
    /// <returns>一组值中元素的平均值。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>均值 (Mean): 返回一组数据的平均值，用于表示数据的中心趋势。</remarks>
    [DeviceDependency(DeviceDependency.CPU)]
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
    /// 计算一组值中元素的中位数。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="INumber{T}"/>接口。</typeparam>
    /// <param name="values">要计算中位数的值的一组值。</param>
    /// <returns>一组值中元素的中位数。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>中位数 (Median): 返回一组数据的中位数，用于表示数据的中间值，能有效反映数据的分布情况。</remarks>
    [DeviceDependency(DeviceDependency.CPU)]
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
    /// 计算一组值中元素的众数。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="INumber{T}"/>接口。</typeparam>
    /// <param name="values">要计算众数的值的一组值。</param>
    /// <returns>一组值中出现频率最高的元素。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>众数 (Mode): 返回一组数据中出现频率最高的值，用于表示数据中最常见的值。</remarks>
    [DeviceDependency(DeviceDependency.CPU)]
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
    /// 计算一组值中元素的平均值和方差。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="INumber{T}"/>接口。</typeparam>
    /// <param name="values">要计算平均值和方差的值的一组值。</param>
    /// <returns>包含一组值中元素的平均值和方差的元组。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>方差 (Variance): 返回一组数据的方差，用于表示数据的离散程度。</remarks>
    [DeviceDependency(DeviceDependency.CPU)]
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
    /// 计算一组值中元素的标准差。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="values">要计算标准差的值的一组值。</param>
    /// <returns>一组值中元素的标准差。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>标准差 (Standard Deviation): 返回一组数据的标准差，用于表示数据的离散程度。</remarks>
    [DeviceDependency(DeviceDependency.CPU)]
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
    /// 计算一组值中元素的变异系数。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <paramname="values">要计算变异系数的值的一组值。</param>
    /// <returns>一组值中元素的变异系数。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>变异系数 (Coefficient of Variation): 衡量数据的离散程度，表示标准差与均值的比值。</remarks>
    [DeviceDependency(DeviceDependency.CPU)]
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
    /// 计算一组值的所有统计值，包括均值、中位数、众数、方差、标准差和变异系数。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="values">要计算统计值的一组值。</param>
    /// <returns>包含均值、中位数、众数、方差、标准差和变异系数的元组。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>返回一个包含所有统计值的元组。</remarks>
    [DeviceDependency(DeviceDependency.CPU)]
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


    /* SIMD版本，性能并没有提升，甚至更低*/

    ///// <summary>
    ///// 计算一组值的所有统计值，包括均值、中位数、众数、方差、标准差和变异系数。
    ///// </summary>
    ///// <typeparam name="T">一组值中元素的类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    ///// <param name="values">要计算统计值的一组值。</param>
    ///// <returns>包含均值、中位数、众数、方差、标准差和变异系数的元组。</returns>
    ///// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    ///// <remarks>返回一个包含所有统计值的元组。</remarks>
    //[DeviceDependency(DeviceDependency.CPU)]
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static (T Mean, T Median, T Mode, T Variance, T StandardDeviation, T CoefficientOfVariation)
    //    CalculateAllStatistics_SIMD<T>(this Span<T> values)
    //    where T : IFloatingPointIeee754<T>
    //{
    //    if (values.IsEmpty)
    //        throw new ArgumentException("Span cannot be empty.", nameof(values));

    //    // 初始化变量
    //    T sum = T.Zero;
    //    T sumOfSquares = T.Zero;
    //    var frequency = new Dictionary<T, int>();
    //    var sortedValues = new T[values.Length];
    //    values.CopyTo(sortedValues);
    //    Array.Sort(sortedValues);

    //    int length = values.Length;
    //    int simdLength = Vector<T>.Count;
    //    int remainder = length % simdLength;

    //    Vector<T> sumVector = Vector<T>.Zero;
    //    Vector<T> sumOfSquaresVector = Vector<T>.Zero;
    //    int i = 0;

    //    // 使用 SIMD 处理数据
    //    for (; i < length - remainder; i += simdLength)
    //    {
    //        Vector<T> vector = new Vector<T>(values.Slice(i, simdLength));
    //        sumVector += vector;
    //        sumOfSquaresVector += vector * vector;

    //        for (int j = 0; j < simdLength; j++)
    //        {
    //            var value = vector[j];
    //            if (frequency.ContainsKey(value))
    //            {
    //                frequency[value]++;
    //            }
    //            else
    //            {
    //                frequency[value] = 1;
    //            }
    //        }
    //    }

    //    // 求和 sumVector 和 sumOfSquaresVector 的元素
    //    for (int j = 0; j < simdLength; j++)
    //    {
    //        sum += sumVector[j];
    //        sumOfSquares += sumOfSquaresVector[j];
    //    }

    //    // 处理剩余的元素
    //    for (; i < length; i++)
    //    {
    //        var value = values[i];
    //        sum += value;
    //        sumOfSquares += value * value;

    //        if (frequency.ContainsKey(value))
    //        {
    //            frequency[value]++;
    //        }
    //        else
    //        {
    //            frequency[value] = 1;
    //        }
    //    }

    //    // 计算均值
    //    T mean = sum / T.CreateChecked(values.Length);

    //    // 计算中位数
    //    T median;
    //    if (values.Length % 2 == 0)
    //    {
    //        median = (sortedValues[values.Length / 2 - 1] + sortedValues[values.Length / 2]) / T.CreateChecked(2);
    //    }
    //    else
    //    {
    //        median = sortedValues[values.Length / 2];
    //    }

    //    // 计算众数
    //    T mode = frequency.OrderByDescending(kvp => kvp.Value).First().Key;

    //    // 计算方差
    //    T variance = (sumOfSquares - sum * sum / T.CreateChecked(values.Length)) / T.CreateChecked(values.Length - 1);

    //    // 计算标准差
    //    T standardDeviation = T.Sqrt(variance);

    //    // 计算变异系数
    //    T coefficientOfVariation = standardDeviation / mean;

    //    return (mean, median, mode, variance, standardDeviation, coefficientOfVariation);
    //}





}
