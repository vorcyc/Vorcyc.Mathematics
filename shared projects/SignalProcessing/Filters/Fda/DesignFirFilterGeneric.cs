using System.Numerics;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

public static partial class DesignFilter
{
    /// <summary>Ideal lowpass fractional-delay FIR (sinc-window) in <typeparamref name="T"/>.</summary>
    public static T[] FirWinFdLp<T>(int order, T frequency, T delay, WindowType window = WindowType.Blackman)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardNormFreq(frequency, "Cutoff frequency");
        var kernel = new T[order];
        var middle = T.CreateChecked((order - 1) / 2);
        var freq2Pi = T.CreateChecked(2) * T.Pi * frequency;
        for (var i = 0; i < order; i++)
        {
            var d = T.CreateChecked(i) - delay - middle;
            kernel[i] = d == T.Zero ? T.CreateChecked(2) * frequency : T.Sin(freq2Pi * d) / (T.Pi * d);
        }
        FilterGenericSupport.ApplyWindow(kernel, window);
        NormalizeKernel(kernel);
        return kernel;
    }

    /// <summary>Ideal highpass fractional-delay FIR in <typeparamref name="T"/>.</summary>
    public static T[] FirWinFdHp<T>(int order, T frequency, T delay, WindowType window = WindowType.Blackman)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardNormFreq(frequency, "Cutoff frequency");
        var kernel = new T[order];
        var middle = T.CreateChecked((order - 1) / 2);
        var half = T.CreateChecked(0.5);
        var freq2Pi = T.CreateChecked(2) * T.Pi * (half - frequency);
        var sign = T.CreateChecked(-1);
        for (var i = 0; i < order; i++)
        {
            var d = T.CreateChecked(i) - delay - middle;
            kernel[i] = d == T.Zero
                ? T.CreateChecked(2) * (half - frequency)
                : sign * T.Sin(freq2Pi * d) / (T.Pi * d);
            sign = -sign;
        }
        FilterGenericSupport.ApplyWindow(kernel, window);
        NormalizeKernel(kernel, T.Pi);
        return kernel;
    }

    /// <summary>Ideal bandpass fractional-delay FIR in <typeparamref name="T"/>.</summary>
    public static T[] FirWinFdBp<T>(int order, T frequencyLow, T frequencyHigh, T delay, WindowType window = WindowType.Blackman)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardBand(frequencyLow, frequencyHigh);
        var kernel = new T[order];
        var middle = T.CreateChecked((order - 1) / 2);
        var freq12Pi = T.CreateChecked(2) * T.Pi * frequencyLow;
        var freq22Pi = T.CreateChecked(2) * T.Pi * frequencyHigh;
        for (var i = 0; i < order; i++)
        {
            var d = T.CreateChecked(i) - delay - middle;
            kernel[i] = d == T.Zero
                ? T.CreateChecked(2) * (frequencyHigh - frequencyLow)
                : (T.Sin(freq22Pi * d) - T.Sin(freq12Pi * d)) / (T.Pi * d);
        }
        FilterGenericSupport.ApplyWindow(kernel, window);
        NormalizeKernel(kernel, T.CreateChecked(2) * T.Pi * (frequencyLow + frequencyHigh) / T.CreateChecked(2));
        return kernel;
    }

    /// <summary>Ideal bandstop fractional-delay FIR in <typeparamref name="T"/>.</summary>
    public static T[] FirWinFdBs<T>(int order, T frequencyLow, T frequencyHigh, T delay, WindowType window = WindowType.Blackman)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardBand(frequencyLow, frequencyHigh);
        var kernel = new T[order];
        var middle = T.CreateChecked((order - 1) / 2);
        var half = T.CreateChecked(0.5);
        var freq12Pi = T.CreateChecked(2) * T.Pi * frequencyLow;
        var freq22Pi = T.CreateChecked(2) * T.Pi * (half - frequencyHigh);
        var sign = T.One;
        for (var i = 0; i < order; i++)
        {
            var d = T.CreateChecked(i) - delay - middle;
            kernel[i] = d == T.Zero
                ? T.CreateChecked(2) * (half - frequencyHigh + frequencyLow)
                : (T.Sin(freq12Pi * d) + sign * T.Sin(freq22Pi * d)) / (T.Pi * d);
            sign = -sign;
        }
        FilterGenericSupport.ApplyWindow(kernel, window);
        NormalizeKernel(kernel);
        return kernel;
    }

    /// <summary>Ideal allpass fractional-delay FIR in <typeparamref name="T"/>.</summary>
    public static T[] FirWinFdAp<T>(int order, T delay, WindowType window = WindowType.Blackman)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var kernel = new T[order];
        var middle = T.CreateChecked((order - 1) / 2);
        for (var i = 0; i < order; i++)
            kernel[i] = TrigonometryHelper.Sinc(T.CreateChecked(i) - delay - middle);
        FilterGenericSupport.ApplyWindow(kernel, window);
        NormalizeKernel(kernel);
        return kernel;
    }

    /// <summary>Normalizes a FIR kernel so |H(e^{jω})| = 1 at <paramref name="frequency"/> (radians).</summary>
    public static void NormalizeKernel<T>(T[] kernel, T? frequency = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var freq = frequency ?? T.Zero;
        var w = Complex<T>.FromPolarCoordinates(T.One, freq);
        var gain = Complex<T>.Abs(T.One / VMath.EvaluatePolynomial(kernel, w));
        for (var i = 0; i < kernel.Length; i++)
            kernel[i] *= gain;
    }

    /// <summary>Ideal lowpass FIR (sinc-window) in <typeparamref name="T"/>.</summary>
    public static T[] FirWinLp<T>(int order, T frequency, WindowType window = WindowType.Blackman)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
        => FirWinFdLp(order, frequency, T.CreateChecked((order + 1) % 2) * T.CreateChecked(0.5), window);

    /// <summary>Ideal highpass FIR (sinc-window) in <typeparamref name="T"/>.</summary>
    public static T[] FirWinHp<T>(int order, T frequency, WindowType window = WindowType.Blackman)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
        => FirWinFdHp(order, frequency, T.CreateChecked((order + 1) % 2) * T.CreateChecked(0.5), window);

    /// <summary>Ideal bandpass FIR (sinc-window) in <typeparamref name="T"/>.</summary>
    public static T[] FirWinBp<T>(int order, T frequencyLow, T frequencyHigh, WindowType window = WindowType.Blackman)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
        => FirWinFdBp(order, frequencyLow, frequencyHigh, T.CreateChecked((order + 1) % 2) * T.CreateChecked(0.5), window);

    /// <summary>Ideal bandstop FIR (sinc-window) in <typeparamref name="T"/>.</summary>
    public static T[] FirWinBs<T>(int order, T frequencyLow, T frequencyHigh, WindowType window = WindowType.Blackman)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
        => FirWinFdBs(order, frequencyLow, frequencyHigh, T.CreateChecked((order + 1) % 2) * T.CreateChecked(0.5), window);

    /// <summary>Frequency-sampling FIR design (firwin2 / fir2) in <typeparamref name="T"/>.</summary>
    public static T[] Fir<T>(int order, T[]? frequencies, T[] gain, int fftSize = 0, WindowType window = WindowType.Hamming)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (fftSize == 0)
            fftSize = 2 * order.NextPowerOf2();

        var freqCount = fftSize / 2 + 1;
        if (frequencies is null)
        {
            frequencies = new T[freqCount];
            for (var i = 0; i < freqCount; i++)
                frequencies[i] = T.CreateChecked(i) / T.CreateChecked(fftSize);
        }

        if (order >= freqCount)
            throw new ArgumentException($"Given that filter order is {order} the FFT size must be at least {2 * order.NextPowerOf2()}");

        Guard.AgainstInequality(frequencies.Length, gain.Length, "Length of frequencies array", "length of gain array");
        for (var i = 1; i < frequencies.Length; i++)
        {
            if (frequencies[i] < frequencies[i - 1])
                throw new ArgumentException("Array of frequencies must be ordered!");
        }

        var step = T.One / T.CreateChecked(fftSize);
        var response = new T[freqCount];
        var left = 0;
        var right = 1;
        for (var i = 0; i < freqCount; i++)
        {
            var grid = T.CreateChecked(i) * step;
            while (grid > frequencies[right] && right < frequencies.Length - 1)
            {
                right++;
                left++;
            }
            response[i] = gain[left] + (gain[right] - gain[left]) * (grid - frequencies[left]) / (frequencies[right] - frequencies[left]);
        }

        var real = new T[fftSize];
        var imag = new T[fftSize];
        var halfOrder = T.CreateChecked(order - 1) / T.CreateChecked(2);
        for (var i = 0; i < response.Length; i++)
        {
            var phase = -halfOrder * T.CreateChecked(2) * T.Pi * T.CreateChecked(i) / T.CreateChecked(fftSize);
            var c = response[i] * Complex<T>.Exp(new Complex<T>(T.Zero, phase));
            real[i] = c.Real;
            imag[i] = c.Imaginary;
        }

        var fft = new RealFft<T>(fftSize);
        fft.Inverse(real, imag, real);

        var kernel = new T[order];
        var scale = T.CreateChecked(fftSize);
        for (var i = 0; i < order; i++)
            kernel[i] = real[i] / scale;
        FilterGenericSupport.ApplyWindow(kernel, window);
        return kernel;
    }

    /// <summary>Equiripple lowpass FIR (Remez) in <typeparamref name="T"/>.</summary>
    public static T[] FirEquirippleLp<T>(int order, T fp, T fa, T wp, T wa)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
        => new Remez<T>(order, [T.Zero, fp, fa, T.CreateChecked(0.5)], [T.One, T.Zero], [wp, wa]).Design();

    /// <summary>Equiripple highpass FIR (Remez) in <typeparamref name="T"/>.</summary>
    public static T[] FirEquirippleHp<T>(int order, T fa, T fp, T wa, T wp)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
        => new Remez<T>(order, [T.Zero, fa, fp, T.CreateChecked(0.5)], [T.Zero, T.One], [wa, wp]).Design();

    /// <summary>Equiripple bandpass FIR (Remez) in <typeparamref name="T"/>.</summary>
    public static T[] FirEquirippleBp<T>(int order, T fa1, T fp1, T fp2, T fa2, T wa1, T wp, T wa2)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
        => new Remez<T>(order, [T.Zero, fa1, fp1, fp2, fa2, T.CreateChecked(0.5)], [T.Zero, T.One, T.Zero], [wa1, wp, wa2]).Design();

    /// <summary>Equiripple bandstop FIR (Remez) in <typeparamref name="T"/>.</summary>
    public static T[] FirEquirippleBs<T>(int order, T fp1, T fa1, T fa2, T fp2, T wp1, T wa, T wp2)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
        => new Remez<T>(order, [T.Zero, fp1, fa1, fa2, fp2, T.CreateChecked(0.5)], [T.One, T.Zero, T.One], [wp1, wa, wp2]).Design();

    /// <summary>Converts an odd-length linear-phase lowpass kernel to highpass (or BP↔BS).</summary>
    public static T[] FirLpToHp<T>(T[] kernel)
        where T : IFloatingPointIeee754<T>
    {
        Guard.AgainstEvenNumber(kernel.Length, "The order of the filter");
        var hp = new T[kernel.Length];
        for (var i = 0; i < kernel.Length; i++)
            hp[i] = -kernel[i];
        hp[hp.Length / 2] += T.One;
        return hp;
    }

    /// <summary>Converts an odd-length linear-phase highpass kernel to lowpass.</summary>
    public static T[] FirHpToLp<T>(T[] kernel) where T : IFloatingPointIeee754<T> => FirLpToHp(kernel);

    /// <summary>Converts an odd-length linear-phase bandpass kernel to bandstop.</summary>
    public static T[] FirBpToBs<T>(T[] kernel) where T : IFloatingPointIeee754<T> => FirLpToHp(kernel);

    /// <summary>Converts an odd-length linear-phase bandstop kernel to bandpass.</summary>
    public static T[] FirBsToBp<T>(T[] kernel) where T : IFloatingPointIeee754<T> => FirLpToHp(kernel);
}
