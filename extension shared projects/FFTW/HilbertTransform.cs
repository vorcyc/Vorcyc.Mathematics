using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics.Buffers;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// Provides static methods for computing the Hilbert transform of real-valued signals using FFT-based analytic signal
/// construction.
/// </summary>
/// <remarks>The Hilbert transform is commonly used in signal processing to generate the analytic signal, extract
/// instantaneous phase and amplitude, or perform envelope detection. All methods in this class are thread-safe and do
/// not modify the input data.</remarks>
public static class HilbertTransform
{

    #region Hilbert Transform Single

    /// <summary>
    /// Computes the Hilbert transform of a real-valued signal using an FFT-based analytic signal construction.
    /// </summary>
    /// <param name="realSignal">The input real-valued signal.</param>
    /// <returns>
    /// A new array containing the Hilbert-transformed signal, which is the imaginary part
    /// of the corresponding analytic signal.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] ComputeHilbertTransform(ReadOnlySpan<float> realSignal)
    {
        int n = realSignal.Length;
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(n, 0);

        using PinnableArray<ComplexFp32> freqDomain = new(n, true);
        using PinnableArray<ComplexFp32> analyticSignal = new(n, true);

        Dft1D.Forward(realSignal, freqDomain);
        ApplyAnalyticSignalSpectrumMask(freqDomain.Span);
        Dft1D.Inverse(freqDomain, analyticSignal);

        float[] hilbert = new float[n];
        for (int i = 0; i < n; i++)
            hilbert[i] = analyticSignal[i].Imaginary / n;

        return hilbert;
    }

    /// <summary>
    /// Applies the analytic-signal spectrum mask in place.
    /// </summary>
    /// <param name="freqDomain">The frequency-domain buffer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyAnalyticSignalSpectrumMask(Span<ComplexFp32> freqDomain)
    {
        int n = freqDomain.Length;
        int half = n / 2;

        freqDomain[0] = new ComplexFp32(freqDomain[0].Real, 0f);

        for (int i = 1; i < half; i++)
            freqDomain[i] *= 2f;

        if (n % 2 == 0)
            freqDomain[half] = new ComplexFp32(freqDomain[half].Real, 0f);

        for (int i = half + 1; i < n; i++)
            freqDomain[i] = ComplexFp32.Zero;
    }

    #endregion

    #region Hilbert Transform Double

    /// <summary>
    /// Computes the Hilbert transform of a real-valued signal using an FFT-based analytic signal construction.
    /// </summary>
    /// <param name="realSignal">The input real-valued signal.</param>
    /// <returns>
    /// A new array containing the Hilbert-transformed signal, which is the imaginary part
    /// of the corresponding analytic signal.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] ComputeHilbertTransform(ReadOnlySpan<double> realSignal)
    {
        int n = realSignal.Length;
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(n, 0);

        using PinnableArray<Complex> freqDomain = new(n, true);
        using PinnableArray<Complex> analyticSignal = new(n, true);

        Dft1D.Forward(realSignal, freqDomain);
        ApplyAnalyticSignalSpectrumMask(freqDomain.Span);
        Dft1D.Inverse(freqDomain, analyticSignal);

        double[] hilbert = new double[n];
        for (int i = 0; i < n; i++)
            hilbert[i] = analyticSignal[i].Imaginary / n;

        return hilbert;
    }

    /// <summary>
    /// Applies the analytic-signal spectrum mask in place.
    /// </summary>
    /// <param name="freqDomain">The frequency-domain buffer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyAnalyticSignalSpectrumMask(Span<Complex> freqDomain)
    {
        int n = freqDomain.Length;
        int half = n / 2;

        freqDomain[0] = new Complex(freqDomain[0].Real, 0);

        for (int i = 1; i < half; i++)
            freqDomain[i] *= 2;

        if (n % 2 == 0)
            freqDomain[half] = new Complex(freqDomain[half].Real, 0);

        for (int i = half + 1; i < n; i++)
            freqDomain[i] = Complex.Zero;
    }

    #endregion

}
