/* duan linli aka cyclone_dll
 * 19.11.5
 * VORCYC CO,.LTD
 */

//tex:
//Formula 1: $$(a+b)^2 = a^2 + 2ab + b^2$$
//Formula 2: $$a^2-b^2 = (a+b)(a-b)$$

//! 8.0f * Atan(1.0f)  = 2* Constants.M_PI

namespace Vorcyc.Mathematics.SignalProcessing.Windowing;

using static System.MathF;
using static Vorcyc.Mathematics.VMath;

public static partial class Windowing
{


    //tex:
    //Formula 1: $$(a+b)^2 = a^2 + 2ab + b^2$$
    //Formula 2: $$a^2-b^2 = (a+b)(a-b)$$


    #region Rectangular

    public static void Rectangular(float[] data, int offset, int length)
    {
        //for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        //{
        //    data[index] *= 1;
        //}
        //! 实际上就是啥都不做
    }


    public static unsafe void Rectangular(float* pData, int length)
    {
        //for (int i = 0; i < length; i++)
        //{
        //    pData[i] *= 1;
        //}
        //! 实际上就是啥都不做
    }

    #endregion


    #region Triangular

    //tex: $$ w[n]=1 -  |\frac{n - \frac{N}{2}} {\frac{L}{2}}| ,  0 \le n \le N $$
    
    /// <summary>
    /// Performs triangle window on real numbers.
    /// </summary>
    /// <param name="pData"></param>
    /// <param name="length"></param>    
    public static unsafe void Triangular(float* pData, int length)
    {
        float factor = 2.0f / (length - 1);
        for (int i = 0; i < (length - 1) / 2; i++)
        {
            float tri = factor * i;
            pData[i] *= tri;
        }
        for (int i = 0; i < length; i++)
        {
            float tri = 2.0f - (factor * i);
            pData[i] *= tri;
        }
    }

    public static void Triangular(float[] data, int offset, int length)
    {
        float factor = 2.0f / (length - 1);
        for (int i = offset, increment = 0; i < (offset + length - 1) / 2; i++, increment++)
        {
            float tri = factor * increment;
            data[i] *= tri;
        }
        for (int i = offset, increment = 0; i < offset + length; i++, increment++)
        {
            float tri = 2.0f - (factor * increment);
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
    public static unsafe void Hamming(float* pData, int length)
    {
        //float factor = 8.0f * Atan(1.0f) / (length - 1);
        float factor = ConstantsFp32.TWO_PI / (length - 1);
        for (int i = 0; i < length; i++)
        {
            float ham = 0.54f - 0.46f * Cos(factor * i);
            pData[i] *= ham;
        }
    }

    public static void Hamming(float[] data, int offset, int length)
    {
        //float factor = 2 * ConstantsFP32.M_PI / (length - 1);
        float factor = ConstantsFp32.TWO_PI / (length - 1);
        for (int i = offset, increment = 0; i < length + offset; i++, increment++)
        {
            float ham = 0.54f - 0.46f * Cos(factor * increment);
            data[i] *= ham;
        }
    }

    #endregion


    #region Blackman

    public static void Blackman(float[] data, int offset, int length)
    {
        float factor = ConstantsFp32.TWO_PI / (length - 1);
        for (int i = offset, increment = 0; i < offset + length; i++, increment++)
        {
            float black =
                 0.42f -
                 (0.5f * Cos(factor * increment)) +
                 (0.08f * Cos(2 * factor * increment));

            data[i] *= black;
        }
    }

    /// <summary>
    /// Performs Blackman window on complex sequence.
    /// </summary>
    /// <param name="pData"></param>
    /// <param name="length"></param>
    public static unsafe void Blackman(float* pData, int length)
    {
        //float factor = 8.0f * Atan(1.0f) / (length - 1);       
        float factor = ConstantsFp32.TWO_PI / (length - 1);
        for (int i = 0; i < length; i++)
        {
            float black =
                 0.42f -
                 (0.5f * Cos(factor * i)) +
                 (0.08f * Cos(2 * factor * i));

            pData[i] *= black;
        }
    }


    #endregion


    #region Hann

    //百度百科的 https://baike.baidu.com/item/%E6%B1%89%E5%AE%81%E7%AA%97/10378703?fr=ge_ala
    // 是错的！！！那么重要的公式竟然是错的！！！！

    public static void Hann(float[] data, int offset, int length)
    {
        float factor = ConstantsFp32.TWO_PI / (length - 1);
        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            float han = 0.5f * (1f - Cos(factor * increment));
            data[index] *= han;
        }
    }


    public static unsafe void Hann(float* pData, int length)
    {
        float factor = ConstantsFp32.TWO_PI / (length - 1);
        for (int i = 0; i < length; i++)
        {
            float han = 0.5f * (1f - Cos(factor * i));
            pData[i] *= han;
        }
    }

    #endregion


    #region Gaussian

    public static void Gaussian(float[] data, int offset, int length)
    {
        float factor = (length - 1) * .5f;
        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            float gaussian = Exp(-0.5f * Pow((increment - factor) / (0.4f * factor), 2.0f));
            data[index] *= gaussian;
        }
    }

    public static unsafe void Gaussian(float* pData, int length)
    {
        float factor = (length - 1) * .5f;
        for (int i = 0; i < length; i++)
        {
            float gaussian = Exp(-0.5f * Pow((i - factor) / (0.4f * factor), 2.0f));
            pData[i] *= gaussian;
        }
    }

    #endregion


    #region Kaiser

    public static void Kaiser(float[] data, int offset, int length, float alpha = 12f)
    {
        float factor = 2.0f / (length - 1);
        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            float kaiser = I0(alpha * Sqrt(1 - (increment * factor - 1) * (increment * factor - 1)))
                / I0(alpha);

            data[index] *= kaiser;
        }
    }


    public static unsafe void Kaiser(float* pData, int length, float alpha = 12f)
    {
        float factor = 2.0f / (length - 1);
        for (int i = 0; i < length; i++)
        {
            float kaiser = I0(alpha * Sqrt(1 - (i * factor - 1) * (i * factor - 1))) / I0(alpha);
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
    /// <param name="alpha"></param>
    public static void Kbd(float[] data, int offset, int length, float alpha = 4f)
    {
        //x var window = new float[length ];
        // 优化数据量 :
        var window = new float[length / 2 + 1];

        float factor = 4.0f / length;
        float sum = 0f;

        for (int index = offset, increment = 0; index <= (length + offset) / 2; index++, increment++)
        {
            sum += I0(ConstantsFp32.PI * alpha * Sqrt(1 - (increment * factor - 1) * (increment * factor - 1)));
            window[increment] = sum;
        }

        for (int index = offset, increment = 0; index < (length + offset) / 2; index++, increment++)
        {
            var v = Sqrt(window[increment] / sum);
            data[index] *= v;

            var backwardIndex = length - 1 - increment + offset;
            data[backwardIndex] *= v;
        }
    }

    public static unsafe void Kbd(float* pData, int length, float alpha = 4f)
    {
        //x var window = new float[length ];
        // 优化数据量 :
        var window = new float[length / 2 + 1];

        float factor = 4.0f / length;
        float sum = 0f;

        for (int i = 0; i <= length / 2; i++)
        {
            sum += I0(ConstantsFp32.PI * alpha * Sqrt(1 - (i * factor - 1) * (i * factor - 1)));
            window[i] = sum;
        }

        for (int i = 0; i < length / 2; i++)
        {
            var v = Sqrt(window[i] / sum);
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

    public static void Bartlett_Hann(float[] data, int offset, int length)
    {
        float factor = 1.0f / (length - 1);
        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            var bh = 0.62f - 0.48f * Abs(increment * factor - 0.5f) - 0.38f * Cos(ConstantsFp32.TWO_PI * increment * factor);
            data[index] *= bh;
        }
    }

    public static unsafe void Bartlett_Hann(float* pData, int length)
    {
        float factor = 1.0f / (length - 1);
        for (int i = 0; i < length; i++)
        {
            var bh = 0.62f - 0.48f * Abs(i * factor - 0.5f) - 0.38f * Cos(ConstantsFp32.TWO_PI * i * factor);
            pData[i] *= bh;
        }
    }

    #endregion


    #region Lanczos

    public static void Lanczos(float[] data, int offset, int length)
    {
        float factor = 2.0f / (length - 1);
        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            var lanczos = Sinc(increment * factor - 1);
            data[index] *= lanczos;
        }
    }


    public static unsafe void Lanczos(float* pData, int length)
    {
        float factor = 2.0f / (length - 1);
        for (int i = 0; i < length; i++)
        {
            var lanczos = Sinc(i * factor - 1);
            pData[i] *= lanczos;
        }
    }

    #endregion


    #region PowerOfSine

    public static void PowerOfSine(float[] data, int offset, int length, float alpha = 1.5f)
    {
        float factor = ConstantsFp32.PI / length;
        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            var v = Pow(Sin(increment * factor), alpha);
            data[index] *= v;
        }
    }


    public static unsafe void PowerOfSine(float* pData, int length, float alpha = 1.5f)
    {
        float factor = ConstantsFp32.PI / length;
        for (int i = 0; i < length; i++)
        {
            var v = Pow(Sin(i * factor), alpha);
            pData[i] *= v;
        }
    }

    #endregion


    #region Flattop

    public static void Flattop(float[] data, int offset, int length)
    {
        float factor = ConstantsFp32.TWO_PI / (length - 1);

        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            var v = 0.216f - 0.417f * Cos(increment * factor) + 0.278f * Cos(2 * increment * factor) - 0.084f * Cos(3 * increment * factor) + 0.007f * Cos(4 * increment * factor);
            data[index] *= v;
        }
    }


    public static unsafe void Flattop(float* pData, int length)
    {
        float factor = ConstantsFp32.TWO_PI / (length - 1);

        for (int i = 0; i < length; i++)
        {
            var v = 0.216f - 0.417f * Cos(i * factor) + 0.278f * Cos(2 * i * factor) - 0.084f * Cos(3 * i * factor) + 0.007f * Cos(4 * i * factor);
            pData[i] *= v;
        }
    }

    #endregion


    #region Liftering

    public static void Liftering(float[] data, int offset, int length, int l = 22)
    {
        if (l <= 0)
        {
            //Rectangular(data,offset, length);
            return;
        }

        for (int index = offset, increment = 0; index < length + offset; index++, increment++)
        {
            var v = 1 + l * Sin(ConstantsFp32.PI * increment / l) / 2;
            data[index] *= v;
        }
    }


    public static unsafe void Liftering(float* pData, int length, int l = 22)
    {
        if (l <= 0)
        {
            //Rectangular(data,offset, length);
            return;
        }

        for (int i = 0; i < length; i++)
        {
            var v = 1 + l * Sin(ConstantsFp32.PI * i / l) / 2;
            pData[i] *= v;
        }
    }

    #endregion


    #region Blackman Harris


    public static void Blackman_Harris(float[] data, int offset, int length)
    {
        float factor = ConstantsFp32.TWO_PI / length;
        for (int i = offset, increment = 0; i < offset + length; i++, increment++)
        {
            float arg = factor * increment;
            float harris =
                0.35875f -
                0.48829f * Cos(arg) +
                0.14128f * Cos(2 * arg) -
                0.01168f * Cos(3 * arg);

            data[i] *= harris;
        }
    }


    /// <summary>
    /// Performs 4 term Blackman-Harris window on complex sequence.
    /// </summary>
    /// <param name="pData"></param>
    /// <param name="length"></param>
    public static unsafe void Blackman_Harris(float* pData, int length)
    {
        float factor = ConstantsFp32.TWO_PI / length;
        for (int i = 0; i < length; i++)
        {
            float arg = factor * i;
            float harris =
                0.35875f -
                0.48829f * Cos(arg) +
                0.14128f * Cos(2 * arg) -
                0.01168f * Cos(3 * arg);

            pData[i] *= harris;
        }
    }

    #endregion



public static void Apply(float[] data, int offset, int length, WindowType windowType)
    {
        switch (windowType)
        {
            case WindowType.Rectangular:
                Rectangular(data, offset, length);
                break;
            case WindowType.Triangular:
                Triangular(data, offset, length);
                break;
            case WindowType.Hamming:
                Hamming(data, offset, length);
                break;
            case WindowType.Blackman:
                Blackman(data, offset, length);
                break;
            case WindowType.Hann:
                Hann(data, offset, length);
                break;
            case WindowType.Gaussian:
                Gaussian(data, offset, length);
                break;
            case WindowType.Kaiser:
                Kaiser(data, offset, length);
                break;
            case WindowType.Kbd:
                Kaiser(data, offset, length);
                break;
            case WindowType.BartlettHann:
                Bartlett_Hann(data, offset, length);
                break;
            case WindowType.Lanczos:
                Lanczos(data, offset, length);
                break;
            case WindowType.PowerOfSine:
                PowerOfSine(data, offset, length);
                break;
            case WindowType.Flattop:
                Flattop(data, offset, length);
                break;
            case WindowType.Liftering:
                Liftering(data, offset, length);
                break;
            case WindowType.BlackmanHarris:
                Blackman_Harris(data, offset, length);
                break;
            default:
                break;
        }
    }
}