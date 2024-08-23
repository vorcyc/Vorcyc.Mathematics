using System.Numerics;
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
    /// 求数组的均值
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static float Average(this float[] array)
    {
        float result = 0.0f;

        for (int i = 0; i < array.Length; i++)
        {
            result += array[i];
        }
        return result / array.Length;
    }


    public static float Average(this float[] array, int start, int length)
    {
        float result = 0.0f;

        var end = System.Math.Min(start + length, array.Length);

        for (int i = start; i < end; i++)
        {
            result += array[i];
        }
        return result / length;
    }



    public static double Average(this byte[] array)
    {
        long sum = 0;
        for (int i = 0; i < array.Length; i++)
        {
            sum += array[i];
        }
        return sum / array.Length;
    }


    #endregion


    #region Variance

    /// <summary>
    /// 求均值和方差
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static (float average, float variance) Variance(this float[] array)
    {
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
    /// 求均值和方差
    /// </summary>
    /// <param name="array"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static (float average, float variance)
        Variance(this float[] array, int start, int length)
    {
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

    #endregion


    #region Max min median

    /// <summary>
    /// 求最大值、最小值和中值
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static (float max, float min, float median) GetMaximumMinimumMedian(float[] array)
    {
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
