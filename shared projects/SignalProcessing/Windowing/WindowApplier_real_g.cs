namespace Vorcyc.Mathematics.SignalProcessing.Windowing;

/* duan linli aka cyclone_dll
 * 19.11.5  , 25.1.8 基于Span<T> 重构
 * VORCYC CO,.LTD
 */

using System.Numerics;
using static Vorcyc.Mathematics.VMath;
using static Vorcyc.Mathematics.TrigonometryHelper;

/// <summary>
/// Provides methods to apply various window functions to data. All methods modify the input span in place.
/// </summary>
public static partial class WindowApplier
{

    #region Rectangular

    //tex:$$ w(n) = 1 $$

    /// <summary>
    /// Applies a rectangular window (no-op).
    /// </summary>
    /// <typeparam name="T">Numeric sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Rectangular<T>(Span<T> values) where T : INumber<T>
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
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Triangular<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T half = T.CreateChecked(n - 1) / T.CreateChecked(2);
        for (int i = 0; i < n; i++)
        {
            T w = T.CreateChecked(1) - T.Abs((T.CreateChecked(i) - half) / half);
            values[i] *= w;
        }
    }

    #endregion

    #region Hamming

    //tex:$$ w(n) = 0.54 - 0.46 \cos\left( \frac{2\pi n}{N-1} \right) $$

    /// <summary>
    /// Applies a symmetric Hamming window (denominator N-1).
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hamming<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(n - 1);
        for (int i = 0; i < n; i++)
        {
            T w = T.CreateChecked(0.54) - T.CreateChecked(0.46) * T.Cos(factor * T.CreateChecked(i));
            values[i] *= w;
        }
    }

    /// <summary>
    /// Applies a periodic Hamming window (denominator N).
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hamming_Periodic<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(n);
        for (int i = 0; i < n; i++)
        {
            T w = T.CreateChecked(0.54) - T.CreateChecked(0.46) * T.Cos(factor * T.CreateChecked(i));
            values[i] *= w;
        }
    }

    #endregion

    #region Blackman

    //tex:$$ w(n) = 0.42 - 0.5 \cos\left( \frac{2\pi n}{N-1} \right) + 0.08 \cos\left( \frac{4\pi n}{N-1} \right) $$

    /// <summary>
    /// Applies a symmetric Blackman window (denominator N-1).
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(n - 1);
        for (int i = 0; i < n; i++)
        {
            T w =
                 T.CreateChecked(0.42) -
                 (T.CreateChecked(0.5) * T.Cos(factor * T.CreateChecked(i))) +
                 (T.CreateChecked(0.08) * T.Cos(T.CreateChecked(2) * factor * T.CreateChecked(i)));
            values[i] *= w;
        }
    }

    /// <summary>
    /// Applies a periodic Blackman window (denominator N).
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman_Periodic<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(n);
        for (int i = 0; i < n; i++)
        {
            T w =
                 T.CreateChecked(0.42) -
                 (T.CreateChecked(0.5) * T.Cos(factor * T.CreateChecked(i))) +
                 (T.CreateChecked(0.08) * T.Cos(T.CreateChecked(2) * factor * T.CreateChecked(i)));
            values[i] *= w;
        }
    }

    #endregion

    #region Hann

    //tex:$$ w(n) = 0.5 \left( 1 - \cos\left( \frac{2\pi n}{N-1} \right) \right) $$

    /// <summary>
    /// Applies a symmetric Hann window (denominator N-1).
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hann<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(n - 1);
        for (int i = 0; i < n; i++)
        {
            T w = T.CreateChecked(0.5) * (T.CreateChecked(1) - T.Cos(factor * T.CreateChecked(i)));
            values[i] *= w;
        }
    }

    /// <summary>
    /// Applies a periodic Hann window (denominator N).
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hann_Periodic<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(n);
        for (int i = 0; i < n; i++)
        {
            T w = T.CreateChecked(0.5) * (T.CreateChecked(1) - T.Cos(factor * T.CreateChecked(i)));
            values[i] *= w;
        }
    }

    #endregion

    #region Gaussian

    //tex:$$ w(n) = \exp\left( -0.5 \left( \frac{n - (N-1)/2}{\sigma (N-1)/2} \right)^2 \right) $$

    /// <summary>
    /// Applies a Gaussian window with fixed σ = 0.4.
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Gaussian<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T half = T.CreateChecked(n - 1) * T.CreateChecked(0.5);
        for (int i = 0; i < n; i++)
        {
            T w = T.Exp(T.CreateChecked(-0.5) * T.Pow((T.CreateChecked(i) - half) / (T.CreateChecked(0.4) * half), T.CreateChecked(2)));
            values[i] *= w;
        }
    }

    #endregion

    #region Kaiser

    //tex:$$ w(n) = \frac{I_0\left( \alpha \sqrt{1 - \left( \frac{2n}{N-1} - 1 \right)^2} \right)}{I_0(\alpha)} $$

    /// <summary>
    /// Applies a Kaiser window with shape parameter alpha.
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    /// <param name="alpha">Shape parameter α.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kaiser<T>(Span<T> values, T alpha = default) where T : struct, IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(2) / T.CreateChecked(n - 1);
        T denom = I0(alpha);
        for (int i = 0; i < n; i++)
        {
            T x = T.CreateChecked(i) * factor - T.CreateChecked(1);
            T w = I0(alpha * T.Sqrt(T.CreateChecked(1) - x * x)) / denom;
            values[i] *= w;
        }
    }

    #endregion

    #region Kbd

    //tex:$$ w(n) = \sqrt{\frac{\sum_{k=0}^{n} I_0\left( \pi \alpha \sqrt{1 - \left( \frac{2k}{N} - 1 \right)^2} \right)}{\sum_{k=0}^{N/2} I_0\left( \pi \alpha \sqrt{1 - \left( \frac{2k}{N} - 1 \right)^2} \right)}} $$

    /// <summary>
    /// Applies a KBD (Kaiser–Bessel derived) window with shape parameter alpha.
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    /// <param name="alpha">Shape parameter α.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kbd<T>(Span<T> values, T alpha = default) where T : struct, IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        var window = new T[n / 2 + 1];

        T factor = T.CreateChecked(4) / T.CreateChecked(n);
        T sum = T.Zero;

        for (int i = 0; i <= n / 2; i++)
        {
            T x = T.CreateChecked(i) * factor - T.CreateChecked(1);
            sum += I0(T.CreateChecked(ConstantsFp32.PI) * alpha * T.Sqrt(T.CreateChecked(1) - x * x));
            window[i] = sum;
        }

        for (int i = 0; i < n / 2; i++)
        {
            var v = T.Sqrt(window[i] / sum);
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
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bartlett_Hann<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(1) / T.CreateChecked(n - 1);
        for (int i = 0; i < n; i++)
        {
            var w = T.CreateChecked(0.62) - T.CreateChecked(0.48) * T.Abs(T.CreateChecked(i) * factor - T.CreateChecked(0.5)) - T.CreateChecked(0.38) * T.Cos(T.CreateChecked(ConstantsFp32.TWO_PI) * T.CreateChecked(i) * factor);
            values[i] *= w;
        }
    }

    #endregion

    #region Lanczos

    //tex:$$ w(n) = \text{sinc}\left( \frac{2n}{N-1} - 1 \right) $$

    /// <summary>
    /// Applies a Lanczos window.
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Lanczos<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(2) / T.CreateChecked(n - 1);
        for (int i = 0; i < n; i++)
        {
            var w = Sinc(T.CreateChecked(i) * factor - T.CreateChecked(1));
            values[i] *= w;
        }
    }

    #endregion

    #region PowerOfSine

    //tex:$$ w(n) = \sin^\alpha\left( \frac{\pi n}{N} \right) $$

    /// <summary>
    /// Applies a power-of-sine window with exponent alpha.
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    /// <param name="alpha">Exponent α.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PowerOfSine<T>(Span<T> values, T alpha = default) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(ConstantsFp32.PI) / T.CreateChecked(n);
        for (int i = 0; i < n; i++)
        {
            var w = T.Pow(T.Sin(T.CreateChecked(i) * factor), alpha);
            values[i] *= w;
        }
    }

    #endregion

    #region Flattop

    //tex:$$ w(n) = 0.216 - 0.417 \cos\left( \frac{2\pi n}{N-1} \right) + 0.278 \cos\left( \frac{4\pi n}{N-1} \right) - 0.084 \cos\left( \frac{6\pi n}{N-1} \right) + 0.007 \cos\left( \frac{8\pi n}{N-1} \right) $$

    /// <summary>
    /// Applies a flat-top window (five-term approximation, symmetric).
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Flattop<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(n - 1);

        for (int i = 0; i < n; i++)
        {
            var w = T.CreateChecked(0.216) - T.CreateChecked(0.417) * T.Cos(T.CreateChecked(i) * factor) + T.CreateChecked(0.278) * T.Cos(T.CreateChecked(2) * T.CreateChecked(i) * factor) - T.CreateChecked(0.084) * T.Cos(T.CreateChecked(3) * T.CreateChecked(i) * factor) + T.CreateChecked(0.007) * T.Cos(T.CreateChecked(4) * T.CreateChecked(i) * factor);
            values[i] *= w;
        }
    }

    #endregion

    #region Liftering

    //tex:$$ w(n) = 1 + \frac{L}{2} \sin\left( \frac{\pi n}{L} \right) $$

    /// <summary>
    /// Applies a cepstral liftering window with parameter L.
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    /// <param name="l">Lifter parameter L (must be &gt; 0).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Liftering<T>(Span<T> values, int l = 22) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;
        if (l <= 0) return;

        for (int i = 0; i < n; i++)
        {
            var w = T.CreateChecked(1) + T.CreateChecked(l) * T.Sin(T.CreateChecked(ConstantsFp32.PI) * T.CreateChecked(i) / T.CreateChecked(l)) / T.CreateChecked(2);
            values[i] *= w;
        }
    }

    #endregion

    #region Blackman_Harris

    //tex:$$ w(n) = 0.35875 - 0.48829 \cos\left( \frac{2\pi n}{N} \right) + 0.14128 \cos\left( \frac{4\pi n}{N} \right) - 0.01168 \cos\left( \frac{6\pi n}{N} \right) $$

    /// <summary>
    /// Applies a 4-term Blackman–Harris window (denominator N).
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman_Harris<T>(Span<T> values) where T : IFloatingPointIeee754<T>
    {
        int n = values.Length;
        if (n <= 1) return;

        T factor = T.CreateChecked(ConstantsFp32.TWO_PI) / T.CreateChecked(n);
        for (int i = 0; i < n; i++)
        {
            T arg = factor * T.CreateChecked(i);
            T w =
                T.CreateChecked(0.35875) -
                T.CreateChecked(0.48829) * T.Cos(arg) +
                T.CreateChecked(0.14128) * T.Cos(T.CreateChecked(2) * arg) -
                T.CreateChecked(0.01168) * T.Cos(T.CreateChecked(3) * arg);

            values[i] *= w;
        }
    }

    #endregion

    #region Apply

    /// <summary>
    /// Applies the selected window function to the input span in place.
    /// </summary>
    /// <typeparam name="T">Floating-point sample type.</typeparam>
    /// <param name="values">Input data to be windowed in place.</param>
    /// <param name="windowType">Window function type.</param>
    /// <param name="alpha">Optional shape/exponent parameter (for Kaiser/PowerOfSine/KBD).</param>
    /// <param name="l">Optional lifter parameter L for cepstral liftering.</param>
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