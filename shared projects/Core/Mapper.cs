namespace Vorcyc.Mathematics;

/// <summary>
/// Map a value from one range to another range.
/// </summary>
public static class Mapper
{


    #region mapper

    /// <summary>
    /// 将数字从一个范围映射至另一个范围
    /// </summary>
    /// <param name="number">输入的数字</param>
    /// <param name="rangeMin">数字所在区间的最小值，开区间（包含）</param>
    /// <param name="rangeMax">数字所在区间的最大值，开区间（包含）</param>
    /// <param name="mappingMin">映射至区间的最小值，开区间（包含）</param>
    /// <param name="mappingMax">映射至区间的最大值，开区间（包含）</param>
    /// <returns></returns>
    public static float Map(
        this float number,
        float rangeMin, float rangeMax,
        float mappingMin, float mappingMax)
    {
        if (number < rangeMin || number > rangeMax)
            throw new ArgumentException(nameof(number));

        if (rangeMin >= rangeMax)
            throw new ArgumentException("rangeMin must less than rangeMax.");

        if (mappingMin >= mappingMax)
            throw new ArgumentException("mappingMin must less than mappingMax.");

        var inputRange = rangeMax - rangeMin;
        var outputRange = mappingMax - mappingMin;
        var r = outputRange / inputRange;
        return mappingMin + r * (number - rangeMin);

    }



    /// <summary>
    /// 将数字从一个范围映射至另一个范围
    /// </summary>
    /// <param name="number">输入的数字</param>
    /// <param name="rangeMin">数字所在区间的最小值，开区间（包含）</param>
    /// <param name="rangeMax">数字所在区间的最大值，开区间（包含）</param>
    /// <param name="mappingMin">映射至区间的最小值，开区间（包含）</param>
    /// <param name="mappingMax">映射至区间的最大值，开区间（包含）</param>
    /// <returns></returns>
    public static double Map(
        this double number,
        double rangeMin, double rangeMax,
        double mappingMin, double mappingMax)
    {
        if (number < rangeMin || number > rangeMax)
            throw new ArgumentException(nameof(number));

        if (rangeMin >= rangeMax)
            throw new ArgumentException("rangeMin must less than rangeMax.");

        if (mappingMin >= mappingMax)
            throw new ArgumentException("mappingMin must less than mappingMax.");

        var inputRange = rangeMax - rangeMin;
        var outputRange = mappingMax - mappingMin;
        var r = outputRange / inputRange;
        return mappingMin + r * (number - rangeMin);
    }

    #endregion



    public static double Map(
        this double number,
        Vorcyc.Mathematics.Numerics.Range<double> from,
        Vorcyc.Mathematics.Numerics.Range<double> to)
    {
        if (number < from.Minimum || number > from.Maximum)
            throw new ArgumentOutOfRangeException("Argument 'number' must in range 'from'");

        var fromSize = from.Maximum - from.Minimum;
        var toSize = to.Maximum - to.Minimum;
        var r = toSize / fromSize;
        return to.Minimum + r * (number - from.Minimum);
    }



    public static float Map(
        this float number,
        Vorcyc.Mathematics.Numerics.Range<float> from,
        Vorcyc.Mathematics.Numerics.Range<float> to)
    {
        if (number < from.Minimum || number > from.Maximum)
            throw new ArgumentOutOfRangeException("Argument 'number' must in range 'from'");

        var fromSize = from.Maximum - from.Minimum;
        var toSize = to.Maximum - to.Minimum;
        var r = toSize / fromSize;
        return to.Minimum + r * (number - from.Minimum);
    }






    public static float Map(
        this float number,
        (float minimum, float maximum) from,
        (float minimum, float maximum) to)
    {
        if (number < from.minimum || number > from.maximum)
            throw new ArgumentOutOfRangeException("Argument 'number' must in range 'from'");

        var fromSize = from.maximum - from.minimum;
        var toSize = to.maximum - to.minimum;
        var r = toSize / fromSize;
        return to.minimum + r * (number - from.minimum);
    }





    public static double Map(
        this double number,
        (double minimum, double maximum) from,
        (double minimum, double maximum) to)
    {
        if (number < from.minimum || number > from.maximum)
            throw new ArgumentOutOfRangeException("Argument 'number' must in range 'from'");

        var fromSize = from.maximum - from.minimum;
        var toSize = to.maximum - to.minimum;
        var r = toSize / fromSize;
        return to.minimum + r * (number - from.minimum);
    }


}
