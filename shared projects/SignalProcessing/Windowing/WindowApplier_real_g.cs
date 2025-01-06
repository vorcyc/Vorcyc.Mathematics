namespace Vorcyc.Mathematics.SignalProcessing.Windowing;

/* duan linli aka cyclone_dll
 * 19.11.5  , 25.1.8 基于Span<T> 重构
 * VORCYC CO,.LTD
 */


using System.Numerics;
using static Vorcyc.Mathematics.VMath;

/// <summary>
/// Provides methods to apply various window functions to data.
/// </summary>
public static partial class WindowApplier
{

    #region Rectangular

    //tex:$$ w(n) = 1 $$

    /// <summary>
    /// 计算矩形窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Rectangular<T>(Span<T> values) where T : INumber<T>
    { }

    #endregion

    #region Triangular

    //tex:$$ w(n) = 1 - \left| \frac{n - (N-1)/2}{(N-1)/2} \right| $$

    /// <summary>
    /// 计算三角窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Triangular<T>(Span<T> values) where T : INumber<T>
    {
        T factor = T.CreateChecked(2) / T.CreateChecked(values.Length - 1);
        for (int i = 0; i < (values.Length - 1) / 2; i++)
        {
            T tri = factor * T.CreateChecked(i);
            values[i] *= tri;
        }
        for (int i = 0; i < values.Length; i++)
        {
            T tri = T.CreateChecked(2) - (factor * T.CreateChecked(i));
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
    public static void Hamming<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(values.Length - 1);
        for (int n = 0; n < values.Length; n++)
        {
            T ham = T.CreateChecked(0.54) - T.CreateChecked(0.46) * T.Cos(factor * T.CreateChecked(n));
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
    public static void Blackman<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(values.Length - 1);
        for (int i = 0; i < values.Length; i++)
        {
            T black =
                 T.CreateChecked(0.42) -
                 (T.CreateChecked(0.5) * T.Cos(factor * T.CreateChecked(i))) +
                 (T.CreateChecked(0.08) * T.Cos(T.CreateChecked(2) * factor * T.CreateChecked(i)));

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
    public static void Hann<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(values.Length - 1);
        for (int n = 0; n < values.Length; n++)
        {
            T han = T.CreateChecked(0.5) * (T.CreateChecked(1) - T.Cos(factor * T.CreateChecked(n)));
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
    public static void Gaussian<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(values.Length - 1) * T.CreateChecked(0.5);
        for (int i = 0; i < values.Length; i++)
        {
            T gaussian = T.Exp(T.CreateChecked(-0.5) * T.Pow((T.CreateChecked(i) - factor) / (T.CreateChecked(0.4) * factor), T.CreateChecked(2)));
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
    public static void Kaiser<T>(Span<T> values, T alpha = default) where T : struct, IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(2) / T.CreateChecked(values.Length - 1);
        for (int i = 0; i < values.Length; i++)
        {
            T kaiser = I0(alpha * T.Sqrt(T.CreateChecked(1) - (T.CreateChecked(i) * factor - T.CreateChecked(1)) * (T.CreateChecked(i) * factor - T.CreateChecked(1)))) / I0(alpha);
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
    [MethodImpl( MethodImplOptions.AggressiveInlining)]
    public static void Kbd<T>(Span<T> values, T alpha = default) where T : struct, IFloatingPointIeee754<T>
    {
        var window = new T[values.Length / 2 + 1];

        T factor = T.CreateChecked(4) / T.CreateChecked(values.Length);
        T sum = T.Zero;

        for (int i = 0; i <= values.Length / 2; i++)
        {
            sum += I0(T.CreateChecked(ConstantsFp32.PI) * alpha * T.Sqrt(T.CreateChecked(1) - (T.CreateChecked(i) * factor - T.CreateChecked(1)) * (T.CreateChecked(i) * factor - T.CreateChecked(1))));
            window[i] = sum;
        }

        for (int i = 0; i < values.Length / 2; i++)
        {
            var v = T.Sqrt(window[i] / sum);
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
    public static void Bartlett_Hann<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(1) / T.CreateChecked(values.Length - 1);
        for (int i = 0; i < values.Length; i++)
        {
            var bh = T.CreateChecked(0.62) - T.CreateChecked(0.48) * T.Abs(T.CreateChecked(i) * factor - T.CreateChecked(0.5)) - T.CreateChecked(0.38) * T.Cos(T.CreateChecked(ConstantsFp32.TWO_PI) * T.CreateChecked(i) * factor);
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
    public static void Lanczos<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(2) / T.CreateChecked(values.Length - 1);
        for (int i = 0; i < values.Length; i++)
        {
            var lanczos = Sinc(T.CreateChecked(i) * factor - T.CreateChecked(1));
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
    public static void PowerOfSine<T>(Span<T> values, T alpha = default) where T : IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(ConstantsFp32.PI) / T.CreateChecked(values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            var v = T.Pow(T.Sin(T.CreateChecked(i) * factor), alpha);
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
    public static void Flattop<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(values.Length - 1);

        for (int i = 0; i < values.Length; i++)
        {
            var v = T.CreateChecked(0.216) - T.CreateChecked(0.417) * T.Cos(T.CreateChecked(i) * factor) + T.CreateChecked(0.278) * T.Cos(T.CreateChecked(2) * T.CreateChecked(i) * factor) - T.CreateChecked(0.084) * T.Cos(T.CreateChecked(3) * T.CreateChecked(i) * factor) + T.CreateChecked(0.007) * T.Cos(T.CreateChecked(4) * T.CreateChecked(i) * factor);
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
    public static void Liftering<T>(Span<T> values, int l = 22) where T : IFloatingPointIeee754<T>
    {
        if (l <= 0)
        {
            return;
        }
        for (int i = 0; i < values.Length; i++)
        {
            var v = T.CreateChecked(1) + T.CreateChecked(l) * T.Sin(T.CreateChecked(ConstantsFp32.PI) * T.CreateChecked(i) / T.CreateChecked(l)) / T.CreateChecked(2);
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
    public static void Blackman_Harris<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            T arg = factor * T.CreateChecked(i);
            T harris =
                T.CreateChecked(0.35875) -
                T.CreateChecked(0.48829) * T.Cos(arg) +
                T.CreateChecked(0.14128) * T.Cos(T.CreateChecked(2) * arg) -
                T.CreateChecked(0.01168) * T.Cos(T.CreateChecked(3) * arg);

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
    /// <param name="alpha">Alpha参数。</param>
    /// <param name="l">L参数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Apply<T>(Span<T> values, WindowType windowType, T alpha = default, int l = 22) where T : struct, IFloatingPointIeee754<T>
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
                Kaiser(values, alpha);
                break;
            case WindowType.Kbd:
                Kbd(values, alpha);
                break;
            case WindowType.BartlettHann:
                Bartlett_Hann(values);
                break;
            case WindowType.Lanczos:
                Lanczos(values);
                break;
            case WindowType.PowerOfSine:
                PowerOfSine(values, alpha);
                break;
            case WindowType.Flattop:
                Flattop(values);
                break;
            case WindowType.Liftering:
                Liftering(values, l);
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
