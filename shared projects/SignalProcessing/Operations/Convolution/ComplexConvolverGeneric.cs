using System.Numerics;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;

/// <summary>
/// Fast (FFT) complex convolver in <typeparamref name="T"/>.
/// <see cref="float"/> matches <see cref="ComplexConvolver"/> on
/// <see cref="ComplexDiscreteSignal{T}"/>; <see cref="double"/> uses <see cref="Fft64"/>.
/// Existing <see cref="ComplexConvolver"/> / <see cref="ComplexDiscreteSignal"/> stay float.
/// </summary>
public class ComplexConvolver<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    /// <summary>Fast convolution via FFT. Result length is <c>N + M - 1</c>.</summary>
    public ComplexDiscreteSignal<T> Convolve(
        ComplexDiscreteSignal<T> signal,
        ComplexDiscreteSignal<T> kernel,
        int fftSize = 0,
        ComputingContext? context = null)
    {
        FilterGenericSupport.EnsureFloatOrDouble<T>();
        var length = signal.Length + kernel.Length - 1;
        if (fftSize == 0)
            fftSize = length.NextPowerOf2();

        var fft = new Fft<T>(fftSize);
        var a = Pad(signal, fftSize);
        var b = Pad(kernel, fftSize);

        fft.Direct(a.Real, a.Imag, context);
        fft.Direct(b.Real, b.Imag, context);

        var spectrum = a.Multiply(b);
        fft.Inverse(spectrum.Real, spectrum.Imag, context);

        var scale = T.One / T.CreateChecked(fftSize);
        for (var i = 0; i < spectrum.Length; i++)
        {
            spectrum.Real[i] *= scale;
            spectrum.Imag[i] *= scale;
        }

        return new ComplexDiscreteSignal<T>(signal.SamplingRate, spectrum.Real, spectrum.Imag).First(length);
    }

    /// <summary>Fast cross-correlation via FFT (time-reverse the kernel; no conjugate).</summary>
    public ComplexDiscreteSignal<T> CrossCorrelate(
        ComplexDiscreteSignal<T> signal,
        ComplexDiscreteSignal<T> kernel,
        int fftSize = 0,
        ComputingContext? context = null)
    {
        var reversedReal = new T[kernel.Length];
        var reversedImag = new T[kernel.Length];
        for (var i = 0; i < kernel.Length; i++)
        {
            reversedReal[i] = kernel.Real[kernel.Length - 1 - i];
            reversedImag[i] = kernel.Imag[kernel.Length - 1 - i];
        }

        return Convolve(signal, new ComplexDiscreteSignal<T>(kernel.SamplingRate, reversedReal, reversedImag),
            fftSize, context);
    }

    /// <summary>
    /// Deconvolution: exact polynomial division when the remainder is ~0, otherwise FFT spectral division.
    /// Result length is <c>N - M + 1</c> on the FFT path.
    /// </summary>
    public ComplexDiscreteSignal<T> Deconvolve(
        ComplexDiscreteSignal<T> signal,
        ComplexDiscreteSignal<T> kernel,
        int fftSize = 0,
        ComputingContext? context = null)
    {
        FilterGenericSupport.EnsureFloatOrDouble<T>();
        var div = VMath.DividePolynomial(ToComplex(signal.Real, signal.Imag), ToComplex(kernel.Real, kernel.Imag));
        var quotient = div[0];
        var remainder = div[1];
        var remTol = T.CreateChecked(1e-10);
        if (remainder.All(d => T.Abs(d.Real) < remTol && T.Abs(d.Imaginary) < remTol))
        {
            var qRe = new T[quotient.Length];
            var qIm = new T[quotient.Length];
            for (var i = 0; i < quotient.Length; i++)
            {
                qRe[i] = quotient[i].Real;
                qIm[i] = quotient[i].Imaginary;
            }
            return new ComplexDiscreteSignal<T>(signal.SamplingRate, qRe, qIm);
        }

        var length = signal.Length - kernel.Length + 1;
        if (fftSize == 0)
            fftSize = signal.Length.NextPowerOf2();

        var fft = new Fft<T>(fftSize);
        var a = Pad(signal, fftSize);
        var b = Pad(kernel, fftSize);
        fft.Direct(a.Real, a.Imag, context);
        fft.Direct(b.Real, b.Imag, context);

        var eps = T.CreateChecked(1e-10);
        for (var i = 0; i < fftSize; i++)
        {
            a.Real[i] += eps;
            a.Imag[i] += eps;
            b.Real[i] += eps;
            b.Imag[i] += eps;
        }

        var spectrum = a.Divide(b);
        fft.Inverse(spectrum.Real, spectrum.Imag, context);
        return new ComplexDiscreteSignal<T>(
            signal.SamplingRate,
            spectrum.Real.FastCopyFragment(length),
            spectrum.Imag.FastCopyFragment(length));
    }

    private static ComplexDiscreteSignal<T> Pad(ComplexDiscreteSignal<T> signal, int length)
        => new(signal.SamplingRate,
            FilterGenericSupport.PadTo(signal.Real, length),
            FilterGenericSupport.PadTo(signal.Imag, length));

    private static Complex<T>[] ToComplex(T[] real, T[] imag)
    {
        var result = new Complex<T>[real.Length];
        for (var i = 0; i < real.Length; i++)
            result[i] = new Complex<T>(real[i], imag[i]);
        return result;
    }
}

/// <summary>Double-precision complex FFT convolver.</summary>
public sealed class ComplexConvolver64 : ComplexConvolver<double>
{
}
