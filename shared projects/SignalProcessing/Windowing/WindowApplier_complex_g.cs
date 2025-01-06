
/*
* CreateChecked - 从值创建当前类型的实例，对超出当前类型的可表示范围的任何值引发溢出异常
* CreateSaturating - 从值创建当前类型的实例，使属于当前类型的可表示范围之外的任何值饱和
* CreateTruncating - 从值创建当前类型的实例，截断当前类型的可表示范围之外的任何值。
*/


namespace Vorcyc.Mathematics.SignalProcessing.Windowing;


using System.Numerics;
using static System.MathF;
using static Vorcyc.Mathematics.VMath;




public static partial class WindowApplier
{



    #region Rectangular

    //tex:$$ w(n) = 1 $$

    /// <summary>
    /// 计算矩形窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Rectangular<T>(Span<Complex<T>> values)
       where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    { }

    #endregion


    #region Triangular

    //tex:$$ w(n) = 1 - \left| \frac{n - (N-1)/2}{(N-1)/2} \right| $$


    /// <summary>
    /// 计算三角窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Triangular<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = 2.0f / (values.Length - 1);
        for (int i = 0; i < (values.Length - 1) / 2; i++)
        {
            float tri = factor * i;
            values[i] *= tri;
        }
        for (int i = 0; i < values.Length; i++)
        {
            float tri = 2.0f - (factor * i);
            values[i] *= tri;
        }
    }
    #endregion


    #region Hamming

    //tex:$$ w(n) = 0.54 - 0.46 \cos\left( \frac{2\pi n}{N-1} \right) $$

    /// <summary>
    /// 计算汉明窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hamming<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = ConstantsFp32.TWO_PI / (values.Length - 1);
        for (int n = 0; n < values.Length; n++)
        {
            float ham = 0.54f - 0.46f * Cos(factor * n);
            values[n] *= ham;
        }
    }
    #endregion


    #region Blackman

    //tex:$$ w(n) = 0.42 - 0.5 \cos\left( \frac{2\pi n}{N-1} \right) + 0.08 \cos\left( \frac{4\pi n}{N-1} \right) $$


    /// <summary>
    /// 计算布莱克曼窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param> 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = ConstantsFp32.TWO_PI / (values.Length - 1);
        for (int i = 0; i < values.Length; i++)
        {
            float black =
                 0.42f -
                 (0.5f * Cos(factor * i)) +
                 (0.08f * Cos(2 * factor * i));

            values[i] *= black;
        }
    }
    #endregion


    #region Hann


    //tex:$$ w(n) = 0.5 \left( 1 - \cos\left( \frac{2\pi n}{N-1} \right) \right) $$


    /// <summary>
    /// 计算汉宁窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hann<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = ConstantsFp32.TWO_PI / (values.Length - 1);
        for (int n = 0; n < values.Length; n++)
        {
            float han = 0.5f * (1f - Cos(factor * n));
            values[n] *= han;
        }
    }
    #endregion


    #region Gaussian


    //tex:$$ w(n) = \exp\left( -0.5 \left( \frac{n - (N-1)/2}{\sigma (N-1)/2} \right)^2 \right) $$

    /// <summary>
    /// 计算高斯窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Gaussian<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = (values.Length - 1) * .5f;
        for (int i = 0; i < values.Length; i++)
        {
            float gaussian = Exp(-0.5f * Pow((i - factor) / (0.4f * factor), 2.0f));
            values[i] *= gaussian;
        }
    }
    #endregion


    #region Kaiser


    //tex:$$ w(n) = \frac{I_0\left( \alpha \sqrt{1 - \left( \frac{2n}{N-1} - 1 \right)^2} \right)}{I_0(\alpha)} $$


    /// <summary>
    /// 计算凯撒窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    /// <param name="alpha">Alpha参数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kaiser<T>(Span<Complex<T>> values, float alpha = 12f)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = 2.0f / (values.Length - 1);
        for (int i = 0; i < values.Length; i++)
        {
            float kaiser = I0(alpha * Sqrt(1 - (i * factor - 1) * (i * factor - 1))) / I0(alpha);
            values[i] *= kaiser;
        }
    }
    #endregion


    #region Kbd


    //tex:$$ w(n) = \sqrt{\frac{\sum_{k=0}^{n} I_0\left( \pi \alpha \sqrt{1 - \left( \frac{2k}{N} - 1 \right)^2} \right)}{\sum_{k=0}^{N/2} I_0\left( \pi \alpha \sqrt{1 - \left( \frac{2k}{N} - 1 \right)^2} \right)}} $$


    /// <summary>
    /// 计算凯撒-贝塞尔派生窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    /// <param name="alpha">Alpha参数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kbd<T>(Span<Complex<T>> values, float alpha = 4f)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var window = new float[values.Length / 2 + 1];

        float factor = 4.0f / values.Length;
        float sum = 0f;

        for (int i = 0; i <= values.Length / 2; i++)
        {
            sum += I0(ConstantsFp32.PI * alpha * Sqrt(1 - (i * factor - 1) * (i * factor - 1)));
            window[i] = sum;
        }

        for (int i = 0; i < values.Length / 2; i++)
        {
            var v = Sqrt(window[i] / sum);
            values[i] *= v;

            var backwardIndex = values.Length - 1 - i;
            values[backwardIndex] *= v;
        }
    }
    #endregion


    #region Bartlett_Hann


    //tex:$$ w(n) = 0.62 - 0.48 \left| \frac{n}{N-1} - 0.5 \right| - 0.38 \cos\left( \frac{2\pi n}{N-1} \right) $$

    /// <summary>
    /// 计算巴特利特-汉宁窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bartlett_Hann<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = 1.0f / (values.Length - 1);
        for (int i = 0; i < values.Length; i++)
        {
            var bh = 0.62f - 0.48f * Abs(i * factor - 0.5f) - 0.38f * Cos(ConstantsFp32.TWO_PI * i * factor);
            values[i] *= bh;
        }
    }
    #endregion


    #region Lanczos


    //tex:$$ w(n) = \text{sinc}\left( \frac{2n}{N-1} - 1 \right) $$

    /// <summary>
    /// 计算兰索斯窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Lanczos<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = 2.0f / (values.Length - 1);
        for (int i = 0; i < values.Length; i++)
        {
            var lanczos = Sinc(i * factor - 1);
            values[i] *= lanczos;
        }
    }
    #endregion


    #region PowerOfSine


    //tex:$$ w(n) = \sin^\alpha\left( \frac{\pi n}{N} \right) $$


    /// <summary>
    /// 计算幂正弦窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    /// <param name="alpha">Alpha参数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PowerOfSine<T>(Span<Complex<T>> values, float alpha = 1.5f)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = ConstantsFp32.PI / values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            var v = Pow(Sin(i * factor), alpha);
            values[i] *= v;
        }
    }
    #endregion


    #region Flattop


    //tex:$$ w(n) = 0.216 - 0.417 \cos\left( \frac{2\pi n}{N-1} \right) + 0.278 \cos\left( \frac{4\pi n}{N-1} \right) - 0.084 \cos\left( \frac{6\pi n}{N-1} \right) + 0.007 \cos\left( \frac{8\pi n}{N-1} \right) $$


    /// <summary>
    /// 计算平顶窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Flattop<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = ConstantsFp32.TWO_PI / (values.Length - 1);

        for (int i = 0; i < values.Length; i++)
        {
            var v = 0.216f - 0.417f * Cos(i * factor) + 0.278f * Cos(2 * i * factor) - 0.084f * Cos(3 * i * factor) + 0.007f * Cos(4 * i * factor);
            values[i] *= v;
        }
    }
    #endregion


    #region Liftering


    //tex:$$ w(n) = 1 + \frac{L}{2} \sin\left( \frac{\pi n}{L} \right) $$

    /// <summary>
    /// 计算升降窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    /// <param name="l">L参数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Liftering<T>(Span<Complex<T>> values, int l = 22)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (l <= 0)
        {
            return;
        }
        for (int i = 0; i < values.Length; i++)
        {
            var v = 1 + l * Sin(ConstantsFp32.PI * i / l) / 2;
            values[i] *= v;
        }
    }
    #endregion


    #region Blackman_Harris


    //tex:$$ w(n) = 0.35875 - 0.48829 \cos\left( \frac{2\pi n}{N} \right) + 0.14128 \cos\left( \frac{4\pi n}{N} \right) - 0.01168 \cos\left( \frac{6\pi n}{N} \right) $$


    /// <summary>
    /// 计算布莱克曼-哈里斯窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman_Harris<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        float factor = ConstantsFp32.TWO_PI / values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            float arg = factor * i;
            float harris =
                0.35875f -
                0.48829f * Cos(arg) +
                0.14128f * Cos(2 * arg) -
                0.01168f * Cos(3 * arg);

            values[i] *= harris;
        }
    }
    #endregion


    #region Apply

    /// <summary>
    /// 应用指定的窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    /// <param name="windowType">窗函数类型。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Apply<T>(Span<Complex<T>> values, WindowType windowType)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        switch (windowType)
        {
            case WindowType.Rectangular:
                Rectangular(values);
                break;
            case WindowType.Triangular:
                Triangular(values);
                break;
            case WindowType.Hamming:
                Hamming(values);
                break;
            case WindowType.Blackman:
                Blackman(values);
                break;
            case WindowType.Hann:
                Hann(values);
                break;
            case WindowType.Gaussian:
                Gaussian(values);
                break;
            case WindowType.Kaiser:
                Kaiser(values);
                break;
            case WindowType.Kbd:
                Kbd(values);
                break;
            case WindowType.BartlettHann:
                Bartlett_Hann(values);
                break;
            case WindowType.Lanczos:
                Lanczos(values);
                break;
            case WindowType.PowerOfSine:
                PowerOfSine(values);
                break;
            case WindowType.Flattop:
                Flattop(values);
                break;
            case WindowType.Liftering:
                Liftering(values);
                break;
            case WindowType.BlackmanHarris:
                Blackman_Harris(values);
                break;
            default:
                break;
        }
    }
    #endregion














}
