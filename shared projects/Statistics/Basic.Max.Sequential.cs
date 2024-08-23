using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;


public static partial class SBasic
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

    #region single version


    /// <summary>
    /// 返回一组数字中的最大值
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static float Max(this float[] array)
    {
        var result = array[0];
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > result) result = array[i];
        }
        return result;
    }



    public static float Max(this float[] values, int start, int length)
    {
        float result = values[start];
        for (int i = start; i < start + length; i++)
        {
            if (values[i] > result) result = values[i];
        }
        return result;
    }



    /// <summary>
    /// 返回最大值和其索引
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static (int index, float max) LocateMax(this float[] array)
    {
        ref float retMax = ref array[0];//= float.MinValue;
        var retIndex = 0;

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > retMax)
            {
                retMax = ref array[i];
                retIndex = i;
            }
        }

        return (retIndex, retMax);
    }




    public static (int index, float max) LocateMax(this float[] array, int start, int length)
    {
        ref var retMax = ref array[0];
        var retIndex = 0;

        var end = System.Math.Min(start + length, array.Length);

        for (int i = start; i < end; i++)
        {
            if (array[i] > retMax)
            {
                retMax = ref array[i];
                retIndex = i;
            }
        }

        return (retIndex, retMax);
    }




    #endregion

    #region Span<T> and INumber<T>

    /// <summary>
    /// Gets the max value in <see cref="Span{T}"/> of type <see cref="INumberBase{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(this Span<T> span)
        where T : INumber<T>
    {
        var result = span[0];
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] > result)
                result = span[i];
        }

        return result;
    }



    /// <summary>
    /// Gets the max value and it's index in <see cref="Span{T}"/> of type <see cref="INumberBase{TSelf}"/>.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="span"></param>
    /// <returns></returns>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int index, TValue value) LocateMax<TValue>(this Span<TValue> span)
        where TValue : INumber<TValue>
    {
        TValue result = span[0];
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


    #endregion


    /// <summary>
    /// Sequential version
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="values"></param>
    /// <returns></returns>
    public static TValue Max<TValue>(params TValue[] values)
        where TValue : IComparable, IComparable<TValue>
    {
        if (values.Length == 1)
            return values[0];

        TValue result = values[0];
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].CompareTo(result) == 1)
                result = values[i];
        }
        return result;
    }


    public static (int, TValue) LocateMax<TValue>(this TValue[] values)
        where TValue : IComparable, IComparable<TValue>
    {
        TValue result = values[0];
        int resultIndex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].CompareTo(result) == 1)
            {
                result = values[i];
                resultIndex = i;
            }
        }
        return (resultIndex, result);
    }


    public static TValue Max<TValue>(this TValue[] values, int start, int length)
        where TValue : IComparable, IComparable<TValue>
    {
        TValue result = values[start];
        for (int i = start; i < start + length; i++)
        {
            if (values[i].CompareTo(result) == 1)
                result = values[i];
        }

        return result;
    }


    /// <summary>
    /// Returns the maximum value and its index in a sequential sequence of values.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="values"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static (int, TValue) LocateMax<TValue>(this TValue[] values, int start, int length)
        where TValue : IComparable, IComparable<TValue>
    {
        TValue result = values[start];
        int resultIndex = 0;
        for (int i = start; i < start + length; i++)
        {
            if (values[i].CompareTo(result) == 1)
            {
                result = values[i];
                resultIndex = i;
            }
        }

        return (resultIndex, result);
    }


}
