using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics;

/// <summary>
/// Map a value from one range to another range.
/// </summary>
public static class NumberMapper
{


    /// <summary>
    /// Defines the behavior when the input value is out of the specified range.
    /// </summary>
    public enum InputValueOutOfRangeHandleBehavior
    {
        /// <summary>
        /// Clamps the input value to the nearest boundary value.
        /// </summary>
        Saturating,

        /// <summary>
        /// Throws an exception when the input value is out of range.
        /// </summary>
        ThrowException,
    }

    /// <summary>
    /// Maps an input value from one range to another.
    /// </summary>
    /// <typeparam name="TNumber">The type of the number, must be unmanaged and implement <see cref="INumber{TNumber}"/>.</typeparam>
    /// <param name="number">The input value to be mapped.</param>
    /// <param name="inMin">The minimum value of the input range.</param>
    /// <param name="inMax">The maximum value of the input range.</param>
    /// <param name="outMin">The minimum value of the output range.</param>
    /// <param name="outMax">The maximum value of the output range.</param>
    /// <param name="handleBehavior">Specifies the behavior when the input value is out of range. Default is <see cref="InputValueOutOfRangeHandleBehavior.Saturating"/>.</param>
    /// <returns>The mapped value in the output range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="inMin"/> is greater than or equal to <paramref name="inMax"/>, or <paramref name="outMin"/> is greater than or equal to <paramref name="outMax"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="number"/> is out of the input range and <paramref name="handleBehavior"/> is <see cref="InputValueOutOfRangeHandleBehavior.ThrowException"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TNumber Map<TNumber>(
        this TNumber number,
        TNumber inMin, TNumber inMax,
        TNumber outMin, TNumber outMax, InputValueOutOfRangeHandleBehavior handleBehavior = InputValueOutOfRangeHandleBehavior.Saturating)
        where TNumber : unmanaged, INumber<TNumber>
    {

        if (inMin >= inMax)
            throw new ArgumentOutOfRangeException(nameof(inMin), "inMin must be less than inMax.");

        if (outMin >= outMax)
            throw new ArgumentOutOfRangeException(nameof(outMin), "outMin must be less than outMax.");

        if (handleBehavior == InputValueOutOfRangeHandleBehavior.ThrowException)
        {
            if (number < inMin || number > inMax)
                throw new ArgumentException($"{nameof(number)} must be between {nameof(inMin)} and {nameof(inMax)}.");
        }
        else if (handleBehavior == InputValueOutOfRangeHandleBehavior.Saturating)
        {
            if (number < inMin)
                number = inMin;
            else if (number > inMax)
                number = inMax;
        }

        var inputRange = inMax - inMin;
        var outputRange = outMax - outMin;
        var ratio = outputRange / inputRange;
        return outMin + ratio * (number - inMin);
    }



    //#region normal form


    ///// <summary>
    ///// Maps a double-precision floating-point number from one range to another.
    ///// </summary>
    ///// <param name="number">The double-precision floating-point number to map.</param>
    ///// <param name="inMin">The minimum value of the input range.</param>
    ///// <param name="inMax">The maximum value of the input range.</param>
    ///// <param name="outMin">The minimum value of the output range.</param>
    ///// <param name="outMax">The maximum value of the output range.</param>
    ///// <returns>The double-precision floating-point number mapped to the target range.</returns>
    ///// <exception cref="ArgumentException">
    ///// Thrown when <paramref name="number"/> is not within the <paramref name="inMin"/> and <paramref name="inMax"/> range,
    ///// or when <paramref name="inMin"/> is greater than or equal to <paramref name="inMax"/>,
    ///// or when <paramref name="outMin"/> is greater than or equal to <paramref name="outMax"/>.
    ///// </exception>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static float Map(
    //    this float number,
    //    float inMin, float inMax,
    //    float outMin, float outMax)
    //{
    //    if (number < inMin || number > inMax) return 0.0f;
    //    //throw new ArgumentException(nameof(number));

    //    if (inMin >= inMax)
    //        return 0.0f;//float.NaN;// ; throw new ArgumentException("rangeMin must less than rangeMax.");

    //    if (outMin >= outMax)
    //        return 0.0f;// float.NaN;// throw new ArgumentException("mappingMin must less than mappingMax.");


    //    var inputRange = inMax - inMin;
    //    var outputRange = outMax - outMin;
    //    var ratio = outputRange / inputRange;
    //    return outMin + ratio * (number - inMin);

    //}




    ///// <summary>
    ///// Maps a double-precision floating-point number from one range to another.
    ///// </summary>
    ///// <param name="number">The double-precision floating-point number to map.</param>
    ///// <param name="inMin">The minimum value of the input range.</param>
    ///// <param name="inMax">The maximum value of the input range.</param>
    ///// <param name="outMin">The minimum value of the output range.</param>
    ///// <param name="outMax">The maximum value of the output range.</param>
    ///// <returns>The double-precision floating-point number mapped to the target range.</returns>
    ///// <exception cref="ArgumentException">
    ///// Thrown when <paramref name="number"/> is not within the <paramref name="inMin"/> and <paramref name="inMax"/> range,
    ///// or when <paramref name="inMin"/> is greater than or equal to <paramref name="inMax"/>,
    ///// or when <paramref name="outMin"/> is greater than or equal to <paramref name="outMax"/>.
    ///// </exception>
    //[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static double Map(
    //    this double number,
    //    double inMin, double inMax,
    //    double outMin, double outMax)
    //{
    //    if (number < inMin || number > inMax)
    //        throw new ArgumentException(nameof(number));

    //    if (inMin >= inMax)
    //        throw new ArgumentException("rangeMin must less than rangeMax.");

    //    if (outMin >= outMax)
    //        throw new ArgumentException("mappingMin must less than mappingMax.");

    //    var inputRange = inMax - inMin;
    //    var outputRange = outMax - outMin;
    //    var r = outputRange / inputRange;
    //    return outMin + r * (number - inMin);
    //}


    ///// <summary>
    ///// Maps a floating-point number from one range to another.
    ///// </summary>
    ///// <typeparam name="TFloatingNumber">The floating-point number type, which must implement the <see cref="IFloatingPointIeee754{TFloatingNumber}"/> interface.</typeparam>
    ///// <param name="number">The floating-point number to map.</param>
    ///// <param name="inMin">The minimum value of the input range.</param>
    ///// <param name="inMax">The maximum value of the input range.</param>
    ///// <param name="outMin">The minimum value of the output range.</param>
    ///// <param name="outMax">The maximum value of the output range.</param>
    ///// <returns>The floating-point number mapped to the target range, or <see cref="TFloatingNumber.Zero"/> if the input is out of range or the ranges are invalid.</returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static TFloatingNumber Map<TFloatingNumber>(
    //    this TFloatingNumber number,
    //    TFloatingNumber inMin, TFloatingNumber inMax,
    //    TFloatingNumber outMin, TFloatingNumber outMax)
    //    where TFloatingNumber : unmanaged, IFloatingPointIeee754<TFloatingNumber>
    //{
    //    if (number < inMin || number > inMax) return TFloatingNumber.Zero;
    //    //throw new ArgumentException(nameof(number));

    //    if (inMin >= inMax)
    //        return TFloatingNumber.Zero;//float.NaN;// ; throw new ArgumentException("rangeMin must less than rangeMax.");

    //    if (outMin >= outMax)
    //        return TFloatingNumber.Zero;// float.NaN;// throw new ArgumentException("mappingMin must less than mappingMax.");


    //    var inputRange = inMax - inMin;
    //    var outputRange = outMax - outMin;
    //    var ratio = outputRange / inputRange;
    //    return outMin + ratio * (number - inMin);

    //}


    //#endregion


    //#region with check


    ///// <summary>
    ///// Maps a single-precision floating-point number from one range to another and returns a boolean indicating success.
    ///// </summary>
    ///// <param name="input">The single-precision floating-point number to map.</param>
    ///// <param name="inputMin">The minimum value of the input range.</param>
    ///// <param name="inputMax">The maximum value of the input range.</param>
    ///// <param name="outputMin">The minimum value of the output range.</param>
    ///// <param name="outputMax">The maximum value of the output range.</param>
    ///// <param name="result">The mapped single-precision floating-point number if the mapping is successful; otherwise, NaN.</param>
    ///// <returns>True if the mapping is successful; otherwise, false.</returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static bool Map(
    //    float input,
    //    float inputMin, float inputMax,
    //    float outputMin, float outputMax,
    //    out float result)
    //{
    //    if (input < inputMin || input > inputMax)
    //    {
    //        result = float.NaN;
    //        return false;
    //    }

    //    if (inputMin >= inputMax)
    //    {
    //        result = float.NaN;
    //        return false;
    //    }

    //    if (outputMin >= outputMax)
    //    {
    //        result = float.NaN;
    //        return false;
    //    }

    //    var inputRange = inputMax - inputMin;
    //    var outputRange = outputMax - outputMin;
    //    var ratio = outputRange / inputRange;
    //    result = outputMin + ratio * (input - inputMin);
    //    return true;
    //}

    ///// <summary>
    ///// Maps a floating-point number from one range to another and returns a boolean indicating success.
    ///// </summary>
    ///// <typeparam name="TFloatingNumber">The floating-point number type, which must implement the <see cref="IFloatingPointIeee754{TFloatingNumber}"/> interface.</typeparam>
    ///// <param name="input">The floating-point number to map.</param>
    ///// <param name="inputMin">The minimum value of the input range.</param>
    ///// <param name="inputMax">The maximum value of the input range.</param>
    ///// <param name="outputMin">The minimum value of the output range.</param>
    ///// <param name="outputMax">The maximum value of the output range.</param>
    ///// <param name="result">The mapped floating-point number if the mapping is successful; otherwise, NaN.</param>
    ///// <returns>True if the mapping is successful; otherwise, false.</returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static bool Map<TFloatingNumber>(
    //    TFloatingNumber input,
    //    TFloatingNumber inputMin, TFloatingNumber inputMax,
    //    TFloatingNumber outputMin, TFloatingNumber outputMax,
    //    out TFloatingNumber result)
    //    where TFloatingNumber : unmanaged, IFloatingPointIeee754<TFloatingNumber>
    //{
    //    if (input < inputMin || input > inputMax)
    //    {
    //        result = TFloatingNumber.NaN;
    //        return false;
    //    }

    //    if (inputMin >= inputMax)
    //    {
    //        result = TFloatingNumber.NaN;
    //        return false;
    //    }

    //    if (outputMin >= outputMax)
    //    {
    //        result = TFloatingNumber.NaN;
    //        return false;
    //    }

    //    var inputRange = inputMax - inputMin;
    //    var outputRange = outputMax - outputMin;
    //    var ratio = outputRange / inputRange;
    //    result = outputMin + ratio * (input - inputMin);
    //    return true;
    //}

    //#endregion


    //#region with my Range<T>

    ///// <summary>
    ///// Maps a double-precision floating-point number from one range to another.
    ///// </summary>
    ///// <param name="number">The double-precision floating-point number to map.</param>
    ///// <param name="from">The source range, containing the minimum and maximum values.</param>
    ///// <param name="to">The target range, containing the minimum and maximum values.</param>
    ///// <returns>The double-precision floating-point number mapped to the target range.</returns>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="number"/> is not within the <paramref name="from"/> range.</exception>
    //[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static double Map(
    //    this double number,
    //    Vorcyc.Mathematics.Numerics.Range<double> from,
    //    Vorcyc.Mathematics.Numerics.Range<double> to)
    //{
    //    if (number < from.Minimum || number > from.Maximum)
    //        throw new ArgumentOutOfRangeException("Argument 'number' must in range 'from'");

    //    var fromSize = from.Maximum - from.Minimum;
    //    var toSize = to.Maximum - to.Minimum;
    //    var r = toSize / fromSize;
    //    return to.Minimum + r * (number - from.Minimum);
    //}

    ///// <summary>
    ///// Maps a single-precision floating-point number from one range to another.
    ///// </summary>
    ///// <param name="number">The single-precision floating-point number to map.</param>
    ///// <param name="from">The source range, containing the minimum and maximum values.</param>
    ///// <param name="to">The target range, containing the minimum and maximum values.</param>
    ///// <returns>The single-precision floating-point number mapped to the target range.</returns>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="number"/> is not within the <paramref name="from"/> range.</exception>
    //[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static float Map(
    //    this float number,
    //    Vorcyc.Mathematics.Numerics.Range<float> from,
    //    Vorcyc.Mathematics.Numerics.Range<float> to)
    //{
    //    if (number < from.Minimum || number > from.Maximum)
    //        throw new ArgumentOutOfRangeException("Argument 'number' must in range 'from'");

    //    var fromSize = from.Maximum - from.Minimum;
    //    var toSize = to.Maximum - to.Minimum;
    //    var r = toSize / fromSize;
    //    return to.Minimum + r * (number - from.Minimum);
    //}






    //#endregion




    //#region with ValueTuple<T1,T2>

    ///// <summary>
    ///// Maps a single-precision floating-point number from one range to another.
    ///// </summary>
    ///// <param name="number">The single-precision floating-point number to map.</param>
    ///// <param name="from">The source range, containing the minimum and maximum values.</param>
    ///// <param name="to">The target range, containing the minimum and maximum values.</param>
    ///// <returns>The single-precision floating-point number mapped to the target range.</returns>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="number"/> is not within the <paramref name="from"/> range.</exception>
    //[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static float Map(
    //    this float number,
    //    (float minimum, float maximum) from,
    //    (float minimum, float maximum) to)
    //{
    //    if (number < from.minimum || number > from.maximum)
    //        throw new ArgumentOutOfRangeException("Argument 'number' must in range 'from'");

    //    var fromSize = from.maximum - from.minimum;
    //    var toSize = to.maximum - to.minimum;
    //    var r = toSize / fromSize;
    //    return to.minimum + r * (number - from.minimum);
    //}


    ///// <summary>
    ///// Maps a double-precision floating-point number from one range to another.
    ///// </summary>
    ///// <param name="number">The double-precision floating-point number to map.</param>
    ///// <param name="from">The source range, containing the minimum and maximum values.</param>
    ///// <param name="to">The target range, containing the minimum and maximum values.</param>
    ///// <returns>The double-precision floating-point number mapped to the target range.</returns>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="number"/> is not within the <paramref name="from"/> range.</exception>
    //[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static double Map(
    //    this double number,
    //    (double minimum, double maximum) from,
    //    (double minimum, double maximum) to)
    //{
    //    if (number < from.minimum || number > from.maximum)
    //        throw new ArgumentOutOfRangeException("Argument 'number' must in range 'from'");

    //    var fromSize = from.maximum - from.minimum;
    //    var toSize = to.maximum - to.minimum;
    //    var r = toSize / fromSize;
    //    return to.minimum + r * (number - from.minimum);
    //}


    ///// <summary>
    ///// Maps a floating-point number from one range to another.
    ///// </summary>
    ///// <typeparam name="TFloatingNumber">The floating-point number type, which must implement the <see cref="IFloatingPointIeee754{TFloatingNumber}"/> interface.</typeparam>
    ///// <param name="number">The floating-point number to map.</param>
    ///// <param name="from">The source range, containing the minimum and maximum values.</param>
    ///// <param name="to">The target range, containing the minimum and maximum values.</param>
    ///// <returns>The floating-point number mapped to the target range.</returns>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="number"/> is not within the <paramref name="from"/> range.</exception>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static TFloatingNumber Map<TFloatingNumber>(
    //    this TFloatingNumber number,
    //    (TFloatingNumber minimum, TFloatingNumber maximum) from,
    //    (TFloatingNumber minimum, TFloatingNumber maximum) to)
    //    where TFloatingNumber : unmanaged, IFloatingPointIeee754<TFloatingNumber>
    //{
    //    // 确保数字在 'from' 范围内
    //    if (number < from.minimum || number > from.maximum)
    //        throw new ArgumentOutOfRangeException(nameof(number), "参数 'number' 必须在 'from' 范围内");

    //    // 计算 'from' 和 'to' 范围的大小
    //    var fromSize = from.maximum - from.minimum;
    //    var toSize = to.maximum - to.minimum;

    //    // 计算比例并将数字映射到 'to' 范围
    //    var r = toSize / fromSize;
    //    return to.minimum + r * (number - from.minimum);
    //}



    //#endregion




}
