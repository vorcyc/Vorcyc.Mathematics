/*
* CreateChecked - 从值创建当前类型的实例，对超出当前类型的可表示范围的任何值引发溢出异常
* CreateSaturating - 从值创建当前类型的实例，使属于当前类型的可表示范围之外的任何值饱和
* CreateTruncating - 从值创建当前类型的实例，截断当前类型的可表示范围之外的任何值。
*/

namespace Vorcyc.Mathematics.SignalProcessing.Windowing;

using System.Numerics;
using static System.MathF;
using static Vorcyc.Mathematics.VMath;
using static Vorcyc.Mathematics.TrigonometryHelper;

public static partial class WindowApplier
{
    #region Rectangular

    //tex:$$ w(n) = 1 $$

    /// <summary>
    /// Applies a rectangular window (no-op).
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Rectangular<T>(Span<Complex<T>> values)
       where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;
        // no-op
    }

    #endregion

    #region Triangular

    //tex:$$ w(n) = 1 - \left| \frac{n - (N-1)/2}{(N-1)/2} \right| $$

    /// <summary>
    /// Applies a triangular window.
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Triangular<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float half = (n - 1) * 0.5f;
        for (int i = 0; i < n; i++)
        {
            float w = 1f - Abs((i - half) / half);
            values[i] *= w;
        }
    }
    #endregion

    #region Hamming

    //tex:$$ w(n) = 0.54 - 0.46 \cos\left( \frac{2\pi n}{N-1} \right) $$

    /// <summary>
    /// Applies a symmetric Hamming window (denominator N-1).
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hamming<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / (n - 1);
        for (int i = 0; i < n; i++)
        {
            float w = 0.54f - 0.46f * Cos(factor * i);
            values[i] *= w;
        }
    }

    /// <summary>
    /// Applies a periodic Hamming window (denominator N).
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hamming_Periodic<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / n;
        for (int i = 0; i < n; i++)
        {
            float w = 0.54f - 0.46f * Cos(factor * i);
            values[i] *= w;
        }
    }
    #endregion

    #region Blackman

    //tex:$$ w(n) = 0.42 - 0.5 \cos\left( \frac{2\pi n}{N-1} \right) + 0.08 \cos\left( \frac{4\pi n}{N-1} \right) $$

    /// <summary>
    /// Applies a symmetric Blackman window (denominator N-1).
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param> 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / (n - 1);
        for (int i = 0; i < n; i++)
        {
            float w =
                 0.42f -
                 (0.5f * Cos(factor * i)) +
                 (0.08f * Cos(2 * factor * i));

            values[i] *= w;
        }
    }

    /// <summary>
    /// Applies a periodic Blackman window (denominator N).
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param> 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman_Periodic<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / n;
        for (int i = 0; i < n; i++)
        {
            float w =
                 0.42f -
                 (0.5f * Cos(factor * i)) +
                 (0.08f * Cos(2 * factor * i));

            values[i] *= w;
        }
    }
    #endregion

    #region Hann

    //tex:$$ w(n) = 0.5 \left( 1 - \cos\left( \frac{2\pi n}{N-1} \right) \right) $$

    /// <summary>
    /// Applies a symmetric Hann window (denominator N-1).
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hann<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / (n - 1);
        for (int i = 0; i < n; i++)
        {
            float w = 0.5f * (1f - Cos(factor * i));
            values[i] *= w;
        }
    }

    /// <summary>
    /// Applies a periodic Hann window (denominator N).
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hann_Periodic<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / n;
        for (int i = 0; i < n; i++)
        {
            float w = 0.5f * (1f - Cos(factor * i));
            values[i] *= w;
        }
    }
    #endregion

    #region Gaussian

    //tex:$$ w(n) = \exp\left( -0.5 \left( \frac{n - (N-1)/2}{\sigma (N-1)/2} \right)^2 \right) $$

    /// <summary>
    /// Applies a Gaussian window with fixed σ = 0.4.
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Gaussian<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float half = (n - 1) * .5f;
        for (int i = 0; i < n; i++)
        {
            float w = Exp(-0.5f * Pow((i - half) / (0.4f * half), 2.0f));
            values[i] *= w;
        }
    }
    #endregion

    #region Kaiser

    //tex:$$ w(n) = \frac{I_0\left( \alpha \sqrt{1 - \left( \frac{2n}{N-1} - 1 \right)^2} \right)}{I_0(\alpha)} $$

    /// <summary>
    /// Applies a Kaiser window with shape parameter alpha.
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    /// <param name="alpha">Shape parameter α.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kaiser<T>(Span<Complex<T>> values, float alpha = 12f)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = 2.0f / (n - 1);
        float denom = I0(alpha);
        for (int i = 0; i < n; i++)
        {
            float x = i * factor - 1f;
            float w = I0(alpha * Sqrt(1 - x * x)) / denom;
            values[i] *= w;
        }
    }
    #endregion

    #region Kbd

    //tex:$$ w(n) = \sqrt{\frac{\sum_{k=0}^{n} I_0\left( \pi \alpha \sqrt{1 - \left( \frac{2k}{N} - 1 \right)^2} \right)}{\sum_{k=0}^{N/2} I_0\left( \pi \alpha \sqrt{1 - \left( \frac{2k}{N} - 1 \right)^2} \right)}} $$

    /// <summary>
    /// Applies a KBD (Kaiser–Bessel derived) window with shape parameter alpha.
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    /// <param name="alpha">Shape parameter α.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kbd<T>(Span<Complex<T>> values, float alpha = 4f)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        var window = new float[n / 2 + 1];

        float factor = 4.0f / n;
        float sum = 0f;

        for (int i = 0; i <= n / 2; i++)
        {
            float x = i * factor - 1f;
            sum += I0(ConstantsFp32.PI * alpha * Sqrt(1 - x * x));
            window[i] = sum;
        }

        for (int i = 0; i < n / 2; i++)
        {
            var v = Sqrt(window[i] / sum);
            values[i] *= v;

            var backwardIndex = n - 1 - i;
            values[backwardIndex] *= v;
        }
    }
    #endregion

    #region Bartlett_Hann

    //tex:$$ w(n) = 0.62 - 0.48 \left| \frac{n}{N-1} - 0.5 \right| - 0.38 \cos\left( \frac{2\pi n}{N-1} \right) $$

    /// <summary>
    /// Applies a Bartlett–Hann window.
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bartlett_Hann<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = 1.0f / (n - 1);
        for (int i = 0; i < n; i++)
        {
            var w = 0.62f - 0.48f * Abs(i * factor - 0.5f) - 0.38f * Cos(ConstantsFp32.TWO_PI * i * factor);
            values[i] *= w;
        }
    }
    #endregion

    #region Lanczos

    //tex:$$ w(n) = \text{sinc}\left( \frac{2n}{N-1} - 1 \right) $$

    /// <summary>
    /// Applies a Lanczos window.
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Lanczos<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = 2.0f / (n - 1);
        for (int i = 0; i < n; i++)
        {
            var w = Sinc(i * factor - 1);
            values[i] *= w;
        }
    }
    #endregion

    #region PowerOfSine

    //tex:$$ w(n) = \sin^\alpha\left( \frac{\pi n}{N} \right) $$

    /// <summary>
    /// Applies a power-of-sine window with exponent alpha.
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    /// <param name="alpha">Exponent α.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PowerOfSine<T>(Span<Complex<T>> values, float alpha = 1.5f)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.PI / n;
        for (int i = 0; i < n; i++)
        {
            var w = Pow(Sin(i * factor), alpha);
            values[i] *= w;
        }
    }
    #endregion

    #region Flattop

    //tex:$$ w(n) = 0.216 - 0.417 \cos\left( \frac{2\pi n}{N-1} \right) + 0.278 \cos\left( \frac{4\pi n}{N-1} \right) - 0.084 \cos\left( \frac{6\pi n}{N-1} \right) + 0.007 \cos\left( \frac{8\pi n}{N-1} \right) $$

    /// <summary>
    /// Applies a flat-top window (five-term approximation, symmetric).
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Flattop<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / (n - 1);

        for (int i = 0; i < n; i++)
        {
            var w = 0.216f - 0.417f * Cos(i * factor) + 0.278f * Cos(2 * i * factor) - 0.084f * Cos(3 * i * factor) + 0.007f * Cos(4 * i * factor);
            values[i] *= w;
        }
    }
    #endregion

    #region Liftering

    //tex:$$ w(n) = 1 + \frac{L}{2} \sin\left( \frac{\pi n}{L} \right) $$

    /// <summary>
    /// Applies a cepstral liftering window with parameter L.
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    /// <param name="l">Lifter parameter L (must be &gt; 0).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Liftering<T>(Span<Complex<T>> values, int l = 22)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;
        if (l <= 0) return;

        for (int i = 0; i < n; i++)
        {
            var w = 1 + l * Sin(ConstantsFp32.PI * i / l) / 2;
            values[i] *= w;
        }
    }
    #endregion

    #region Blackman_Harris

    //tex:$$ w(n) = 0.35875 - 0.48829 \cos\left( \frac{2\pi n}{N} \right) + 0.14128 \cos\left( \frac{4\pi n}{N} \right) - 0.01168 \cos\left( \frac{6\pi n}{N} \right) $$

    /// <summary>
    /// Applies a 4-term Blackman–Harris window (denominator N).
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman_Harris<T>(Span<Complex<T>> values)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / n;
        for (int i = 0; i < n; i++)
        {
            float arg = factor * i;
            float w =
                0.35875f -
                0.48829f * Cos(arg) +
                0.14128f * Cos(2 * arg) -
                0.01168f * Cos(3 * arg);

            values[i] *= w;
        }
    }
    #endregion

    #region Apply

    /// <summary>
    /// Applies the specified window function to the input data in-place.
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    /// <param name="windowType">Window function type.</param>
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
            case WindowType.HammingPeriodic:
                Hamming_Periodic(values);
                break;
            case WindowType.Blackman:
                Blackman(values);
                break;
            case WindowType.BlackmanPeriodic:
                Blackman_Periodic(values);
                break;
            case WindowType.Hann:
                Hann(values);
                break;
            case WindowType.HannPeriodic:
                Hann_Periodic(values);
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