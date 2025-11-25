using Vorcyc.Mathematics.SignalProcessing.Transforms;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

/// <summary>
/// Provides methods for filter design and analysis.
/// </summary>
public static partial class DesignFilter
{
    #region fractional delay FIR filter design

    /// <summary>
    /// Designs ideal lowpass fractional-delay FIR filter using sinc-window method.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
    /// <param name="delay">Fractional delay</param>
    /// <param name="window">Window</param>
    public static float[] FirWinFdLp(int order, float frequency, float delay, WindowType window = WindowType.Blackman)
    {
        Guard.AgainstInvalidRange(frequency, 0, 0.5f, "Cutoff frequency");

        var kernel = new float[order];

        var middle = (order - 1) / 2;
        var freq2Pi = 2 * ConstantsFp32.PI * frequency;

        for (var i = 0; i < order; i++)
        {
            var d = i - delay - middle;

            kernel[i] = d == 0 ? 2 * frequency : MathF.Sin(freq2Pi * d) / (ConstantsFp32.PI * d);
        }

        kernel.ApplyWindow(window);

        NormalizeKernel(kernel);

        return kernel;
    }

    /// <summary>
    /// Designs ideal highpass fractional-delay FIR filter using sinc-window method.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
    /// <param name="delay">Fractional delay</param>
    /// <param name="window">Window</param>
    public static float[] FirWinFdHp(int order, float frequency, float delay, WindowType window = WindowType.Blackman)
    {
        Guard.AgainstInvalidRange(frequency, 0, 0.5f, "Cutoff frequency");

        var kernel = new float[order];

        var middle = (order - 1) / 2;
        var freq2Pi = 2 * ConstantsFp32.PI * (0.5f - frequency);

        var sign = -1;

        for (var i = 0; i < order; i++)
        {
            var d = i - delay - middle;

            kernel[i] = d == 0 ? 2 * (0.5f - frequency) : sign * MathF.Sin(freq2Pi * d) / (ConstantsFp32.PI * d);

            sign = -sign;
        }

        kernel.ApplyWindow(window);

        NormalizeKernel(kernel, ConstantsFp32.PI);

        return kernel;
    }

    /// <summary>
    /// Designs ideal bandpass fractional-delay FIR filter using sinc-window method.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
    /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
    /// <param name="delay">Fractional delay</param>
    /// <param name="window">Window</param>
    public static float[] FirWinFdBp(int order, float frequencyLow, float frequencyHigh, float delay, WindowType window = WindowType.Blackman)
    {
        Guard.AgainstInvalidRange(frequencyLow, 0, 0.5f, "low cutoff frequency");
        Guard.AgainstInvalidRange(frequencyHigh, 0, 0.5f, "high cutoff frequency");
        Guard.AgainstInvalidRange(frequencyLow, frequencyHigh, "low cutoff frequency", "high cutoff frequency");

        var kernel = new float[order];

        var middle = (order - 1) / 2;
        var freq12Pi = 2 * ConstantsFp32.PI * frequencyLow;
        var freq22Pi = 2 * ConstantsFp32.PI * frequencyHigh;

        for (var i = 0; i < order; i++)
        {
            var d = i - delay - middle;

            kernel[i] = d == 0 ? 2 * (frequencyHigh - frequencyLow) : (MathF.Sin(freq22Pi * d) - MathF.Sin(freq12Pi * d)) / (ConstantsFp32.PI * d);
        }

        kernel.ApplyWindow(window);

        NormalizeKernel(kernel, 2 * ConstantsFp32.PI * (frequencyLow + frequencyHigh) / 2);

        return kernel;
    }

    /// <summary>
    /// Designs ideal bandstop fractional-delay FIR filter using sinc-window method.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
    /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
    /// <param name="delay">Fractional delay</param>
    /// <param name="window">Window</param>
    public static float[] FirWinFdBs(int order, float frequencyLow, float frequencyHigh, float delay, WindowType window = WindowType.Blackman)
    {
        Guard.AgainstInvalidRange(frequencyLow, 0, 0.5f, "low cutoff frequency");
        Guard.AgainstInvalidRange(frequencyHigh, 0, 0.5f, "high cutoff frequency");
        Guard.AgainstInvalidRange(frequencyLow, frequencyHigh, "low cutoff frequency", "high cutoff frequency");

        var kernel = new float[order];

        var middle = (order - 1) / 2;
        var freq12Pi = 2 * ConstantsFp32.PI * frequencyLow;
        var freq22Pi = 2 * ConstantsFp32.PI * (0.5f - frequencyHigh);

        var sign = 1;

        for (var i = 0; i < order; i++)
        {
            var d = i - delay - middle;

            kernel[i] = d == 0 ? 2 * (0.5f - frequencyHigh + frequencyLow) : (MathF.Sin(freq12Pi * d) + sign * MathF.Sin(freq22Pi * d)) / (ConstantsFp32.PI * d);

            sign = -sign;
        }

        kernel.ApplyWindow(window);

        NormalizeKernel(kernel);

        return kernel;
    }

    /// <summary>
    /// Designs ideal allpass fractional-delay FIR filter using sinc-window method.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="delay">Fractional delay</param>
    /// <param name="window">Window</param>
    public static float[] FirWinFdAp(int order, float delay, WindowType window = WindowType.Blackman)
    {
        var kernel = new float[order];

        var middle = (order - 1) / 2;

        for (var i = 0; i < order; i++)
        {
            kernel[i] = TrigonometryHelper.Sinc(i - delay - middle);
        }

        kernel.ApplyWindow(window);

        NormalizeKernel(kernel);

        return kernel;
    }

    /// <summary>
    /// Normalizes frequency response at given frequency 
    /// (normalizes kernel coefficients to map frequency response onto [0, 1])
    /// </summary>
    /// <param name="kernel">Kernel</param>
    /// <param name="frequency">Frequency</param>
    public static void NormalizeKernel(float[] kernel, float frequency = 0)
    {
        var w = ComplexFp32.FromPolarCoordinates(1, frequency);

        var gain = ComplexFp32.Abs(1 / VMath.EvaluatePolynomial(kernel, w));

        for (var i = 0; i < kernel.Length; i++)
        {
            kernel[i] *= gain;
        }
    }

    #endregion


    #region FirWin functions

    //
    // FirWin(Lp|Hp|Bp|Bs) functions:
    // 
    // as of ver.0.9.5,
    // they're coded as the special case of fractional-delay FIR filter design
    // with either delay=0 (odd order) or delay=0.5 (even order)
    // 

    /// <summary>
    /// Designs ideal lowpass FIR filter using sinc-window method.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
    /// <param name="window">Window</param>
    public static float[] FirWinLp(int order, float frequency, WindowType window = WindowType.Blackman)
    {
        return FirWinFdLp(order, frequency, (order + 1) % 2 * 0.5f, window);
    }

    /// <summary>
    /// Designs ideal highpass FIR filter using sinc-window method.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
    /// <param name="window">Window</param>
    public static float[] FirWinHp(int order, float frequency, WindowType window = WindowType.Blackman)
    {
        return FirWinFdHp(order, frequency, (order + 1) % 2 * 0.5f, window);
    }

    /// <summary>
    /// Designs ideal bandpass FIR filter using sinc-window method.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
    /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
    /// <param name="window">Window</param>
    public static float[] FirWinBp(int order, float frequencyLow, float frequencyHigh, WindowType window = WindowType.Blackman)
    {
        return FirWinFdBp(order, frequencyLow, frequencyHigh, (order + 1) % 2 * 0.5f, window);
    }

    /// <summary>
    /// Designs ideal bandstop FIR filter using sinc-window method.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
    /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
    /// <param name="window">Window</param>
    public static float[] FirWinBs(int order, float frequencyLow, float frequencyHigh, WindowType window = WindowType.Blackman)
    {
        return FirWinFdBs(order, frequencyLow, frequencyHigh, (order + 1) % 2 * 0.5f, window);
    }

    /// <summary>
    /// <para>
    /// Designs FIR filter using frequency sampling method 
    /// (works identical to firwin2 in sciPy and fir2 in MATLAB).
    /// </para>
    /// <para>
    /// Note. By default, the FFT size is auto-computed. 
    ///       If it is set explicitly, then (fftSize/2 + 1) must exceed the filter order.
    /// </para>
    /// <para>
    /// Note. Array of frequencies can be null. 
    ///       In this case the <paramref name="fftSize"/> must be provided and size of gains array must be fftSize/2 + 1. 
    ///       Frequencies will be uniformly sampled on range [0..0.5].
    /// </para>
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="frequencies">Frequencies (frequency sampling points), each in range [0..0.5]</param>
    /// <param name="gain">Filter gains at the frequency sampling points</param>
    /// <param name="fftSize">FFT size</param>
    /// <param name="window">Window</param>
    public static float[] Fir(int order,
                               float[] frequencies,
                               float[] gain,
                               int fftSize = 0,
                               WindowType window = WindowType.Hamming)
    {
        if (fftSize == 0)
        {
            fftSize = 2 * order.NextPowerOf2(); //NextPowerOfTwo(order);
        }

        var freqCount = fftSize / 2 + 1;

        if (frequencies is null)
        {
            frequencies = Enumerable.Range(0, freqCount)
                                    .Select(i => (float)i / fftSize)
                                    .ToArray();
        }

        if (order >= freqCount)
        {
            throw new ArgumentException($"Given that filter order is {order} the FFT size must be at least {2 *  order.NextPowerOf2() /* MathUtils.NextPowerOfTwo(order)*/}");
        }

        Guard.AgainstInequality(frequencies.Length, gain.Length, "Length of frequencies array", "length of gain array");
        Guard.AgainstNotOrdered(frequencies, "Array of frequencies");


        // linear interpolation

        var step = 1.0f / fftSize;
        var grid = Enumerable.Range(0, freqCount)
                             .Select(f => f * step)
                             .ToArray();

        var response = new float[grid.Length];
        var x = frequencies;
        var y = gain;

        var left = 0;
        var right = 1;

        for (var i = 0; i < grid.Length; i++)
        {
            while (grid[i] > x[right] && right < x.Length - 1)
            {
                right++;
                left++;
            }

            response[i] = y[left] + (y[right] - y[left]) * (grid[i] - x[left]) / (x[right] - x[left]);
        }

        // prepare complex frequency response

        var complexResponse = new ComplexFp32[fftSize];

        for (var i = 0; i < response.Length; i++)
        {
            complexResponse[i] = response[i] * ComplexFp32.Exp(new ComplexFp32(0, -(order - 1) / 2.0f * 2 * ConstantsFp32.PI * i / fftSize));
        }

        var real = complexResponse.Select(c => c.Real).ToArray();
        var imag = complexResponse.Select(c => c.Imaginary).ToArray();

        // IFFT

        var fft = new RealFft(fftSize);
        fft.Inverse(real, imag, real);

        var kernel = real.Take(order).Select(s => s / fftSize).ToArray();

        kernel.ApplyWindow(window);

        return kernel;
    }

    #endregion


    #region equiripple FIR filter

    /// <summary>
    /// Designs equiripple lowpass FIR filter using Remez (Parks-McClellan) algorithm.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="fp">Passband edge frequency</param>
    /// <param name="fa">Stopband edge frequency</param>
    /// <param name="wp">Passband weight</param>
    /// <param name="wa">Stopband weight</param>
    public static float[] FirEquirippleLp(int order, float fp, float fa, float wp, float wa)
    {
        return new Remez(order, new[] { 0, fp, fa, 0.5f }, new[] { 1, 0.0f }, new[] { wp, wa }).Design();
    }

    /// <summary>
    /// Designs equiripple highpass FIR filter using Remez (Parks-McClellan) algorithm.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="fa">Stopband edge frequency</param>
    /// <param name="fp">Passband edge frequency</param>
    /// <param name="wa">Stopband weight</param>
    /// <param name="wp">Passband weight</param>
    public static float[] FirEquirippleHp(int order, float fa, float fp, float wa, float wp)
    {
        return new Remez(order, new[] { 0, fa, fp, 0.5f }, new[] { 0, 1.0f}, new[] { wa, wp }).Design();
    }

    /// <summary>
    /// Designs equiripple bandpass FIR filter using Remez (Parks-McClellan) algorithm.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="fa1">Left stopband edge frequency</param>
    /// <param name="fp1">Passband left edge frequency</param>
    /// <param name="fp2">Passband right edge frequency</param>
    /// <param name="fa2">Right stopband edge frequency</param>
    /// <param name="wa1">Left stopband weight</param>
    /// <param name="wp">Passband weight</param>
    /// <param name="wa2">Right stopband weight</param>
    public static float[] FirEquirippleBp(int order, float fa1, float fp1, float fp2, float fa2, float wa1, float wp, float wa2)
    {
        return new Remez(order, new[] { 0, fa1, fp1, fp2, fa2, 0.5f }, new[] { 0, 1.0f, 0 }, new[] { wa1, wp, wa2 }).Design();
    }

    /// <summary>
    /// Designs equiripple bandstop FIR filter using Remez (Parks-McClellan) algorithm.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="fp1">Left passband edge frequency</param>
    /// <param name="fa1">Stopband left edge frequency</param>
    /// <param name="fa2">Stopband right edge frequency</param>
    /// <param name="fp2">Right passband edge frequency</param>
    /// <param name="wp1">Left passband weight</param>
    /// <param name="wa">Stopband weight</param>
    /// <param name="wp2">Right passband weight</param>
    public static float[] FirEquirippleBs(int order, float fp1, float fa1, float fa2, float fp2, float wp1, float wa, float wp2)
    {
        return new Remez(order, new[] { 0, fp1, fa1, fa2, fp2, 0.5f }, new[] { 1, 0.0f, 1 }, new[] { wp1, wa, wp2 }).Design();
    }

    #endregion


    #region convert LowPass FIR filter kernel between band forms

    /// <summary>
    /// Converts linear-phase lowpass filter to highpass filter with the same cutoff frequency. 
    /// This method works only for odd-sized kernels.
    /// </summary>
    /// <param name="kernel">Lowpass filter kernel</param>
    public static float[] FirLpToHp(float[] kernel)
    {
        Guard.AgainstEvenNumber(kernel.Length, "The order of the filter");

        var kernelHp = kernel.Select(k => -k).ToArray();
        kernelHp[kernelHp.Length / 2] += 1.0f;
        return kernelHp;
    }

    /// <summary>
    /// Converts linear-phase highpass filter to lowpass filter with the same cutoff frequency. 
    /// This method works only for odd-sized kernels.
    /// </summary>
    /// <param name="kernel">Highpass filter kernel</param>
    public static float[] FirHpToLp(float[] kernel) => FirLpToHp(kernel);

    /// <summary>
    /// Converts linear-phase bandpass filter to bandstop filter with the same cutoff frequencies. 
    /// This method works only for odd-sized kernels.
    /// </summary>
    /// <param name="kernel">Bandpass filter kernel</param>
    public static float[] FirBpToBs(float[] kernel) => FirLpToHp(kernel);

    /// <summary>
    /// Converts linear-phase bandstop filter to bandpass filter with the same cutoff frequencies. 
    /// This method works only for odd-sized kernels.
    /// </summary>
    /// <param name="kernel">Bandstop filter kernel</param>
    public static float[] FirBsToBp(float[] kernel) => FirLpToHp(kernel);

    #endregion
}
