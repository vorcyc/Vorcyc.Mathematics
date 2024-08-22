#if NET7_0_OR_GREATER

using System.Numerics;
using static Vorcyc.Mathematics.VMath;

namespace Vorcyc.Mathematics.SignalProcessing.Windowing;

public static partial class Windowing
{

    /*
     * CreateChecked - 从值创建当前类型的实例，对超出当前类型的可表示范围的任何值引发溢出异常
     * CreateSaturating - 从值创建当前类型的实例，使属于当前类型的可表示范围之外的任何值饱和
     * CreateTruncating - 从值创建当前类型的实例，截断当前类型的可表示范围之外的任何值。
     */


    #region Rectangular


    public static void Rectangular<T>(T[] data, int offset, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    { }

    public static unsafe void Rectangular<T>(T* pData, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    { }


    #endregion


    #region Triangular

    /// <summary>
    /// Performs triangle window on complex sequence.
    /// </summary>
    /// <param name="pData"></param>
    /// <param name="length"></param>
    public static unsafe void Triangular<T>(T* pData, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(2.0 / (length - 1));
        for (int i = 0; i < (length - 1) / 2; i++)
        {
            T tri = factor * T.CreateChecked(i);
            pData[i] *= tri;
        }
        for (int i = 0; i < length; i++)
        {
            T tri = T.CreateTruncating(2.0) - factor * T.CreateChecked(i);
            pData[i] *= tri;
        }
    }


    public static void Triangular<T>(T[] data, int offset, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(2.0f / (length - 1));
        for (int i = offset, increment = 0; i < (offset + length - 1) / 2; i++, increment++)
        {
            T tri = factor * T.CreateChecked(increment);
            data[i] *= tri;
        }
        for (int i = offset, increment = 0; i < offset + length; i++, increment++)
        {
            T tri = T.CreateTruncating(2.0) - (factor * T.CreateChecked(increment));
            data[i] *= tri;
        }
    }

    #endregion


    #region Hamming

    /// <summary>
    /// Performs Hamming window on complex sequence.
    /// </summary>
    /// <param name="pData"></param>
    /// <param name="length"></param>
    public static unsafe void Hamming<T>(T* pData, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T const_0p54 = T.CreateTruncating(0.54);
        T const_0p46 = T.CreateTruncating(0.46);

        T factor = T.CreateTruncating(8.0) * T.Atan(T.One) / (T.CreateChecked(length - 1));
        for (int i = 0; i < length; i++)
        {
            T ham = const_0p54 - const_0p46 * T.Cos(factor * T.CreateChecked(i));
            pData[i] *= ham;
        }
    }

    public static void Hamming<T>(T[] data, int offset, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T const_0p54 = T.CreateTruncating(0.54);
        T const_0p46 = T.CreateTruncating(0.46);

        T factor = T.CreateTruncating(2) * T.Pi / T.CreateChecked(length - 1);
        for (int i = offset, increment = 0; i < length + offset; i++, increment++)
        {
            T ham = const_0p54 - const_0p46 * T.Cos(factor * T.CreateChecked(increment));
            data[i] *= ham;
        }
    }

    #endregion


    #region Blackman

    /// <summary>
    /// Performs Blackman window on complex sequence.
    /// </summary>
    /// <param name="pData"></param>
    /// <param name="length"></param>
    public static unsafe void Blackman<T>(T* pData, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {

        T const_0p42 = T.CreateTruncating(0.42);
        T const_0p5 = T.CreateTruncating(0.5);
        T const_0p08 = T.CreateChecked(0.08);

        T factor = T.CreateTruncating(8.0) * T.Atan(T.One) / T.CreateChecked(length - 1);
        for (int i = 0; i < length; i++)
        {
            T black =
                 const_0p42 -
                 (const_0p5 * T.Cos(factor * T.CreateChecked(i))) +
                 (const_0p08 * T.Cos(T.CreateChecked(2 * i) * factor));

            pData[i] *= black;
        }
    }


    public static void Blackman<T>(T[] data, int offset, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {

        T const_0p42 = T.CreateTruncating(0.42);
        T const_0p5 = T.CreateTruncating(0.5);
        T const_0p08 = T.CreateChecked(0.08);

        T factor = T.CreateTruncating(2) * T.Pi / T.CreateChecked(length - 1);
        for (int i = offset, increment = 0; i < offset + length; i++, increment++)
        {
            T black =
                 const_0p42 -
                 (const_0p5 * T.Cos(factor * T.CreateChecked(increment))) +
                 (const_0p08 * T.Cos(T.CreateChecked(2 * increment) * factor));

            data[i] *= black;
        }
    }

    #endregion


    #region Hann

    public static void Hann<T>(T[] data, int offset, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {

        T zero_point_five = T.CreateTruncating(0.5);

        T factor = T.CreateChecked(2) * T.Pi / (T.CreateChecked(length - 1));
        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            T han = zero_point_five - zero_point_five * T.Cos(factor * T.CreateChecked(increment));
            data[index] *= han;
        }
    }


    public static unsafe void Hann<T>(T* pData, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T zero_point_five = T.CreateTruncating(0.5);

        T factor = T.CreateTruncating(2) * T.Pi / (T.CreateChecked(length - 1));
        for (int i = 0; i < length; i++)
        {
            T han = zero_point_five - zero_point_five * T.Cos(factor * T.CreateChecked(i));
            pData[i] *= han;
        }
    }

    #endregion


    #region Gaussian

    public static void Gaussian<T>(T[] data, int offset, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T zero_point_five = T.CreateTruncating(0.5);
        T factor = T.CreateTruncating(length - 1) * zero_point_five;

        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            T gaussian = T.Exp(-zero_point_five * T.Pow(T.CreateChecked(increment) - factor / (T.CreateTruncating(0.4) * factor), T.CreateTruncating(2)));
            data[index] *= gaussian;
        }
    }

    public static unsafe void Gaussian<T>(T* pData, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T zero_point_five = T.CreateTruncating(0.5);
        T factor = T.CreateTruncating(length - 1) * zero_point_five;

        for (int i = 0; i < length; i++)
        {
            T gaussian = T.Exp(-zero_point_five * T.Pow(T.CreateChecked(i) - factor / (T.CreateTruncating(0.4) * factor), T.CreateTruncating(2)));
            pData[i] *= gaussian;
        }
    }

    #endregion


    #region Kaiser

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <param name="alpha">The default value is 12</param>
    public static void Kaiser<T>(T[] data, int offset, int length, T? alpha = null)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T factor = T.CreateTruncating(2.0 / (length - 1));
        alpha ??= T.CreateTruncating(12);

        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            T kaiser = I0(alpha.Value * T.Sqrt(T.One - (T.CreateTruncating(increment) * factor - T.One) * (T.CreateTruncating(increment) * factor - T.One)))
                / I0(alpha.Value);

            data[index] *= kaiser;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pData"></param>
    /// <param name="length"></param>
    /// <param name="alpha">The default value is 12</param>
    public static unsafe void Kaiser<T>(T* pData, int length, T? alpha = null)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T factor = T.CreateTruncating(2.0f / (length - 1));
        alpha ??= T.CreateTruncating(12);

        for (int i = 0; i < length; i++)
        {
            T kaiser = I0(alpha.Value * T.Sqrt(T.One - (T.CreateTruncating(i) * factor - T.One) * (T.CreateTruncating(i) * factor - T.One))) / I0(alpha.Value);
            pData[i] *= kaiser;
        }
    }

    #endregion


    #region Kaiser-Bessel Derived

    /// <summary>
    ///  Generates Kaiser-Bessel Derived window .
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <param name="alpha">The default value is 4</param>
    public static void Kbd<T>(T[] data, int offset, int length, T? alpha = null)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        alpha ??= T.CreateTruncating(4);

        //x var window = new float[length ];
        // 优化数据量 :
        var window = new T[length / 2 + 1];

        T factor = T.CreateChecked(4.0 / length);
        T sum = T.Zero;

        for (int index = offset, increment = 0; index <= (length + offset) / 2; index++, increment++)
        {
            sum += I0(Constants<T>.Pi * alpha.Value * T.Sqrt(T.One - (T.CreateTruncating(increment) * factor - T.One) * (T.CreateTruncating(increment) * factor - T.One)));
            window[increment] = sum;
        }

        for (int index = offset, increment = 0; index < (length + offset) / 2; index++, increment++)
        {
            var v = T.Sqrt(window[increment] / sum);
            data[index] *= v;

            var backwardIndex = length - 1 - increment + offset;
            data[backwardIndex] *= v;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pData"></param>
    /// <param name="length"></param>
    /// <param name="alpha">The default value is 4</param>
    public static unsafe void Kbd<T>(T* pData, int length, T? alpha = null)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        alpha ??= T.CreateTruncating(4);
        //x var window = new float[length ];
        // 优化数据量 :
        var window = new T[length / 2 + 1];

        T factor = T.CreateChecked(4.0f / length);
        T sum = T.Zero;

        for (int i = 0; i <= length / 2; i++)
        {
            sum += I0(Constants<T>.Pi * alpha.Value * T.Sqrt(T.One - (T.CreateTruncating(i) * factor - T.One) * (T.CreateTruncating(i) * factor - T.One)));
            window[i] = sum;
        }

        for (int i = 0; i < length / 2; i++)
        {
            var v = T.Sqrt(window[i] / sum);
            pData[i] *= v;

            var backwardIndex = length - 1 - i;
            pData[backwardIndex] *= v;
        }
    }


    //public static void Kbd2(float[] data, int offset, int length, float alpha = 4f)
    //{
    //    //var window = new float[length ];
    //    // 优化数据量 :
    //    var window = new float[length / 2 + 1];

    //    float factor = 4.0f / length;
    //    float sum = 0f;

    //    for (int index = offset, increment = 0; index <= (length + offset) / 2; index++, increment++)
    //    {
    //        sum += MathUtils.I0(ConstantsFP32.M_PI * alpha * Sqrt(1 - (increment * factor - 1) * (increment * factor - 1)));
    //        window[increment] = sum;
    //    }

    //    for (int index = offset, increment = 0; index < (length + offset) / 2; index++, increment++)
    //    {
    //        var v = Sqrt(window[increment] / sum);
    //        data[index] *= v;

    //        var backwardIndex = length - 1 - increment + offset;
    //        data[backwardIndex] *= v;

    //    }
    //}



    #endregion


    #region Bartlett-Hann

    public static void Bartlett_Hann<T>(T[] data, int offset, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T factor = T.CreateTruncating(1.0 / (length - 1));
        T zero_point_five = T.CreateTruncating(0.5);
        T zero_point_38 = T.CreateTruncating(0.38);
        T M_0_62 = T.CreateChecked(0.62);
        T M_0_48 = T.CreateChecked(0.48);

        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            var bh = M_0_62 - M_0_48 * T.Abs(T.CreateTruncating(increment) * factor - zero_point_five) - zero_point_38 * T.Cos(Constants<T>.Two_Pi * T.CreateTruncating(increment) * factor);
            data[index] *= bh;
        }
    }

    public static unsafe void Bartlett_Hann<T>(T* pData, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T factor = T.CreateTruncating(1.0 / (length - 1));
        T zero_point_five = T.CreateTruncating(0.5);
        T zero_point_38 = T.CreateTruncating(0.38);
        T M_0_62 = T.CreateChecked(0.62);
        T M_0_48 = T.CreateChecked(0.48);

        for (int i = 0; i < length; i++)
        {
            var bh = M_0_62 - M_0_48 * T.Abs(T.CreateTruncating(i) * factor - zero_point_five) - zero_point_38 * T.Cos(Constants<T>.Two_Pi * T.CreateTruncating(i) * factor);
            pData[i] *= bh;
        }
    }

    #endregion


    #region Lanczos

    public static void Lanczos<T>(T[] data, int offset, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T factor = T.CreateTruncating(2.0 / (length - 1));

        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            var lanczos = Sinc(T.CreateTruncating(increment) * factor - T.One);
            data[index] *= lanczos;
        }
    }


    public static unsafe void Lanczos<T>(T* pData, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T factor = T.CreateTruncating(2.0 / (length - 1));
        for (int i = 0; i < length; i++)
        {
            var lanczos = Sinc(T.CreateTruncating(i) * factor - T.One);
            pData[i] *= lanczos;
        }
    }

    #endregion


    #region PowerOfSine

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <param name="alpha">The default value is 1.5</param>
    public static void PowerOfSine<T>(T[] data, int offset, int length, T? alpha = null)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {

        alpha ??= T.CreateTruncating(1.5);
        T factor = T.CreateChecked(ConstantsFp32.PI / length);

        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            var v = T.Pow(T.Sin(T.CreateTruncating(increment) * factor), alpha.Value);
            data[index] *= v;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pData"></param>
    /// <param name="length"></param>
    /// <param name="alpha">The default value is 1.5</param>
    public static unsafe void PowerOfSine<T>(T* pData, int length, T? alpha = null)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        alpha ??= T.CreateTruncating(1.5);
        T factor = T.CreateChecked(ConstantsFp32.PI / length);

        for (int i = 0; i < length; i++)
        {
            var v = T.Pow(T.Sin(T.CreateTruncating(i) * factor), alpha.Value);
            pData[i] *= v;
        }
    }

    #endregion


    #region Flattop

    public static void Flattop<T>(T[] data, int offset, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T factor = Constants<T>.Two_Pi / T.CreateTruncating(length - 1);
        T m0_216 = T.CreateChecked(0.216);
        T m0_417 = T.CreateChecked(0.417);
        T m0_278 = T.CreateChecked(0.278);
        T m0_084 = T.CreateChecked(0.084);
        T m0_007 = T.CreateChecked(0.007);

        T m2 = T.CreateTruncating(2);
        T m3 = T.CreateTruncating(3);
        T m4 = T.CreateTruncating(4);

        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            var t_inc = T.CreateTruncating(increment);

            var v = m0_216 - m0_417 * T.Cos(t_inc * factor) + m0_278 * T.Cos(m2 * t_inc * factor) - m0_084 * T.Cos(m3 * t_inc * factor) + m0_007 * T.Cos(m4 * t_inc * factor);
            data[index] *= v;
        }
    }


    public static unsafe void Flattop<T>(T* pData, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T factor = Constants<T>.Two_Pi / T.CreateTruncating(length - 1);
        T m0_216 = T.CreateChecked(0.216);
        T m0_417 = T.CreateChecked(0.417);
        T m0_278 = T.CreateChecked(0.278);
        T m0_084 = T.CreateChecked(0.084);
        T m0_007 = T.CreateChecked(0.007);

        T m2 = T.CreateTruncating(2);
        T m3 = T.CreateTruncating(3);
        T m4 = T.CreateTruncating(4);

        for (int i = 0; i < length; i++)
        {
            var t_i = T.CreateTruncating(i);

            var v = m0_216 - m0_417 * T.Cos(t_i * factor) + m0_278 * T.Cos(m2 * t_i * factor) - m0_084 * T.Cos(m3 * t_i * factor) + m0_007 * T.Cos(m4 * t_i * factor);
            pData[i] *= v;
        }
    }

    #endregion



    #region Blackman_Harris

    /// <summary>
    /// Performs 4 term Blackman-Harris window on complex sequence.
    /// </summary>
    /// <param name="pData"></param>
    /// <param name="length"></param>
    public static unsafe void Blackman_Harris<T>(T* pData, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {

        T const_0p35875 = T.CreateChecked(0.35875);
        T const_0p48829 = T.CreateChecked(0.48829);
        T const_0p14128 = T.CreateChecked(0.14128);
        T const_0p01168 = T.CreateChecked(0.01168);

        T factor = T.CreateTruncating(8.0) * T.Atan(T.One) / T.CreateChecked(length);
        for (int i = 0; i < length; i++)
        {
            T arg = factor * T.CreateChecked(i);
            T harris =
                const_0p35875 -
                const_0p48829 * T.Cos(arg) +
                const_0p14128 * T.Cos(T.CreateTruncating(2) * arg) -
                const_0p01168 * T.Cos(T.CreateTruncating(3) * arg);

            pData[i] *= harris;
        }
    }


    public static void Blackman_Harris<T>(T[] data, int offset, int length)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        T const_0p35875 = T.CreateChecked(0.35875);
        T const_0p48829 = T.CreateChecked(0.48829);
        T const_0p14128 = T.CreateChecked(0.14128);
        T const_0p01168 = T.CreateChecked(0.01168);

        T factor = T.CreateTruncating(2) * T.Pi / T.CreateChecked(length);
        for (int i = offset, increment = 0; i < offset + length; i++, increment++)
        {
            T arg = factor * T.CreateChecked(increment);
            T harris =
                const_0p35875 -
                const_0p48829 * T.Cos(arg) +
                const_0p14128 * T.Cos(T.CreateTruncating(2) * arg) -
                const_0p01168 * T.Cos(T.CreateTruncating(3) * arg);

            data[i] *= harris;
        }
    }

    #endregion


    #region Liftering

    public static void Liftering<T>(T[] data, int offset, int length, int l = 22)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        if (l <= 0)
        {
            //Rectangular(data,offset, length);
            return;
        }

        var t_l = T.CreateChecked(l);
        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            var v = T.One + t_l * T.Sin(T.Pi * T.CreateTruncating(increment) / t_l) / Constants<T>.Two;
            data[index] *= v;
        }
    }


    public static unsafe void Liftering<T>(T* pData, int length, int l = 22)
        where T : unmanaged, INumberBase<T>, IFloatingPointIeee754<T>
    {
        if (l <= 0)
        {
            //Rectangular(data,offset, length);
            return;
        }

        var t_l = T.CreateChecked(l);
        for (int i = 0; i < length; i++)
        {
            var v = T.One + t_l * T.Sin(T.Pi * T.CreateTruncating(i) / t_l) / Constants<T>.Two;
            pData[i] *= v;
        }
    }

    #endregion
}


#endif