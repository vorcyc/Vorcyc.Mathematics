using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class SpanHelper
{
    // 对比测试方法
    internal static void CompareMethods()
    {
        // 生成测试数据
        int dataSize = 1000;
        var random = new Random();
        var data = new double[dataSize];
        for (int i = 0; i < dataSize; i++)
        {
            data[i] = random.NextDouble() * 1000;
        }

        var span = new Span<double>(data);

        // 输出 Sum 方法的结果
        Console.WriteLine($"Sum_Normal\t: {span.Sum_Normal()}");
        Console.WriteLine($"Sum_SIMD\t: {span.Sum_SIMD()}");

        // 输出 Average 方法的结果
        Console.WriteLine($"Average_Normal\t: {span.Average_Normal()}");
        Console.WriteLine($"Average_SIMD\t: {span.Average_SIMD()}");

        // 输出 Max 方法的结果
        Console.WriteLine($"Max_Normal\t: {span.Max_Normal()}");
        Console.WriteLine($"Max_SIMD\t: {span.Max_SIMD()}");

        // 输出 Min 方法的结果
        Console.WriteLine($"Min_Normal\t: {span.Min_Normal()}");
        Console.WriteLine($"Min_SIMD\t: {span.Min_SIMD()}");
    }
    /// <summary>
    /// 计算一组值中元素的总和，使用SIMD进行优化。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="INumber{T}"/>接口。</typeparam>
    /// <param name="values">要计算总和的值的一组值。</param>
    /// <returns>一组值中元素的总和。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>总和 (Sum): 返回一组数据中所有元素的总和，用于表示数据的整体大小。</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Sum_SIMD<T>(this Span<T> values)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Average_SIMD<T>(this Span<T> values)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        return values.Sum_SIMD() / T.CreateChecked(values.Length);
    }


    /// <summary>
    /// 计算一组值中元素的最大值，使用SIMD进行优化。
    /// </summary>
    /// <typeparam name="T">一组值中元素的类型，必须实现<see cref="INumber{T}"/>接口。</typeparam>
    /// <param name="values">要计算最大值的值的一组值。</param>
    /// <returns>一组值中元素的最大值。</returns>
    /// <exception cref="ArgumentException">当<paramref name="values"/>为空时抛出。</exception>
    /// <remarks>最大值 (Max): 返回一组数据中所有元素的最大值，用于表示数据的最大值。</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Max_SIMD<T>(this Span<T> values)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));

        int length = values.Length;
        int simdLength = Vector<T>.Count;
        int remainder = length % simdLength;

        Vector<T> maxVector = new Vector<T>(values.Slice(0, simdLength));
        int i = simdLength;

        // 以Vector<T>.Count为单位处理数据
        for (; i < length - remainder; i += simdLength)
        {
            Vector<T> vector = new Vector<T>(values.Slice(i, simdLength));
            maxVector = Vector.Max(maxVector, vector);
        }

        // 求maxVector的元素的最大值
        T max = maxVector[0];
        for (int j = 1; j < simdLength; j++)
        {
            if (maxVector[j] > max)
                max = maxVector[j];
        }

        // 处理剩余的元素
        for (; i < length; i++)
        {
            if (values[i] > max)
                max = values[i];
        }

        return max;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Min_SIMD<T>(this Span<T> values)
    where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));

        int length = values.Length;
        int simdLength = Vector<T>.Count;
        int remainder = length % simdLength;

        Vector<T> minVector = new Vector<T>(values.Slice(0, simdLength));
        int i = simdLength;

        // 以Vector<T>.Count为单位处理数据
        for (; i < length - remainder; i += simdLength)
        {
            Vector<T> vector = new Vector<T>(values.Slice(i, simdLength));
            minVector = Vector.Min(minVector, vector);
        }

        // 求minVector的元素的最小值
        T min = minVector[0];
        for (int j = 1; j < simdLength; j++)
        {
            if (minVector[j] < min)
                min = minVector[j];
        }

        // 处理剩余的元素
        for (; i < length; i++)
        {
            if (values[i] < min)
                min = values[i];
        }

        return min;
    }













    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Max_Normal<T>(this Span<T> values)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        T max = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > max)
                max = values[i];
        }
        return max;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Min_Normal<T>(this Span<T> values)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        T min = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < min)
                min = values[i];
        }
        return min;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Sum_Normal<T>(this Span<T> values)
        where T : INumber<T>
    {
        var result = T.Zero;
        for (int i = 0; i < values.Length; i++)
        {
            result += values[i];
        }
        return result;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Average_Normal<T>(this Span<T> values)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));
        return values.Sum_Normal() / T.CreateChecked(values.Length);
    }











}
