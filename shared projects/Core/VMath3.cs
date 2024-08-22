namespace Vorcyc.Mathematics;

public static partial class VMath
{




    #region Max Min

    /// <summary>
    /// 找最大值和最小值，并返回TupleValue&lt;T1,T2&gt;
    /// </summary>
    /// <param name="array"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
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



    public static (float max, float min) MaxMin(
        this ArraySegment<float> arraySegment)
    {
        var returnMin = float.MaxValue;
        var returnMax = float.MinValue;

        for (int i = arraySegment.Offset; i < (arraySegment.Offset + arraySegment.Count); i++)
        {
            float value = arraySegment.Array[i];
            returnMin = (value < returnMin) ? value : returnMin;
            returnMax = (value > returnMax) ? value : returnMax;
        }

        return (returnMax, returnMin);
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
