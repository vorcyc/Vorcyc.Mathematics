using Vorcyc.Mathematics.Experimental.Signals;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// Provides extension methods for transforming time-domain signals to the frequency domain using FFTW, with optional
/// windowing support.
/// </summary>
/// <remarks>These extension methods utilize FFTW's arbitrary-length discrete Fourier transform (DFT), so
/// zero-padding to a power-of-two length is not required. Windowing can be applied to the signal prior to
/// transformation by specifying a window type. The returned FrequencyDomain object represents the result of the
/// transformation and includes information about the original signal and windowing used.</remarks>
public static class SignalExtension
{

    extension(Signal signal)
    {

        /// <summary>
        /// Transforms the signal to the frequency domain using FFTW, with optional windowing.
        /// This implemention is based on FFTW's arbitrary-length DFT, so zero-padding is not required even if the length is not a power of 2.
        /// </summary>
        /// <param name="window">The window type to apply; <see langword="null"/> for no windowing.</param>
        /// <returns>A <see cref="FrequencyDomain"/> object representing the transformed signal.</returns>
        /// <remarks>Because FFTW supports arbitrary-length DFTs, zero-padding is not required.</remarks>
        public FrequencyDomain TransformToFrequencyDomainFFTW(WindowType? window = null)
        {
            signal.ThrowIfDisposed();
            if (window is null)
            {
                var result = new Vorcyc.Mathematics.Numerics.ComplexFp32[signal.Length];
                Vorcyc.Mathematics.Extensions.FFTW.Dft1D.Forward(signal.Samples, result);
                return new FrequencyDomain(0, signal.Length, signal.Length, result, signal, window);
            }
            else//由于窗函数需要修改样本值，所以只要使用窗函数时 都需要创建临时副本
            {
                var windowedSamples = new float[signal.Length];
                signal.Samples.CopyTo(windowedSamples);
                WindowApplier.Apply(windowedSamples, window.Value, useSIMD: false);
                var result = new Vorcyc.Mathematics.Numerics.ComplexFp32[windowedSamples.Length];
                Vorcyc.Mathematics.Extensions.FFTW.Dft1D.Forward(windowedSamples, result);
                return new FrequencyDomain(0, windowedSamples.Length, signal.Length, result, signal, window);
            }
        }
    }


    extension(SignalSegment signalSegment)
    {

        /// <summary>
        /// Transforms the signal to the frequency domain using FFTW, with optional windowing.
        /// This implemention is based on FFTW's arbitrary-length DFT, so zero-padding is not required even if the length is not a power of 2.
        /// </summary>
        /// <param name="window">The window type to apply; <see langword="null"/> for no windowing.</param>
        /// <returns>A <see cref="FrequencyDomain"/> object representing the transformed signal.</returns>
        /// <remarks>Because FFTW supports arbitrary-length DFTs, zero-padding is not required.</remarks>
        public FrequencyDomain TransformToFrequencyDomainFFTW(WindowType? window = null)
        {
            if (window is null)
            {
                var result = new Vorcyc.Mathematics.Numerics.ComplexFp32[signalSegment.Length];
                Vorcyc.Mathematics.Extensions.FFTW.Dft1D.Forward(signalSegment.Samples, result);
                return new FrequencyDomain(0, signalSegment.Length, signalSegment.Length, result, signalSegment, window);
            }
            else
            {
                var windowedSamples = new float[signalSegment.Length];
                WindowApplier.Apply(windowedSamples, window.Value, useSIMD: false);
                var result = new Vorcyc.Mathematics.Numerics.ComplexFp32[windowedSamples.Length];
                Vorcyc.Mathematics.Extensions.FFTW.Dft1D.Forward(windowedSamples, result);
                return new FrequencyDomain(0, windowedSamples.Length, signalSegment.Length, result, signalSegment, window);
            }
        }
    }


    extension(ModifiableTimeDomainSignal modifiableSignal)
    {

        /// <summary>
        /// Transforms the signal to the frequency domain using FFTW, with an optional window function.
        /// </summary>
        /// <param name="window">
        /// The window function type. When <see langword="null"/>, the raw samples are used directly.
        /// </param>
        /// <returns>A <see cref="FrequencyDomain"/> object representing the frequency-domain result.</returns>
        /// <remarks>
        /// Because FFTW performs a general DFT, zero-padding to a power-of-two length is not required.
        /// </remarks>
        public FrequencyDomain TransformToFrequencyDomainFFTW(WindowType? window = null)
        {
            using var view = modifiableSignal.Samples;

            if (window is null)
            {
                var result = new Vorcyc.Mathematics.Numerics.ComplexFp32[modifiableSignal.Length];
                Vorcyc.Mathematics.Extensions.FFTW.Dft1D.Forward(view.Span, result);
                return new FrequencyDomain(0, modifiableSignal.Length, modifiableSignal.Length, result, modifiableSignal, window);
            }
            else
            {
                var windowedSamples = new float[view.Span.Length];
                view.Span.CopyTo(windowedSamples);
                WindowApplier.Apply(windowedSamples, window.Value, useSIMD: false);
                var result = new Vorcyc.Mathematics.Numerics.ComplexFp32[windowedSamples.Length];
                Vorcyc.Mathematics.Extensions.FFTW.Dft1D.Forward(windowedSamples, result);
                return new FrequencyDomain(0, windowedSamples.Length, modifiableSignal.Length, result, modifiableSignal, window);
            }
        }
    }


    extension(FrequencyDomain frequencyDomain)
    {

        /// <summary>
        /// Performs an inverse Fast Fourier Transform (FFT) on the frequency domain data and writes the result to the
        /// associated time domain signal.
        /// </summary>
        /// <remarks>This method updates the underlying time domain signal with the result of the inverse
        /// FFT operation. The method supports both single-threaded and modifiable time domain signal types. The state
        /// of the frequency domain data is not modified.</remarks>
        public void InverseFFTW()
        {
            if (frequencyDomain.Signal is ISingleThreadTimeDomainSignal singleThreadSignal)
            {
                Vorcyc.Mathematics.Extensions.FFTW.Dft1D.Inverse(frequencyDomain.Result, singleThreadSignal.Samples.Slice(frequencyDomain.Offset, frequencyDomain.ActualLength));
            }
            else if (frequencyDomain.Signal is IModifiableTimeDomainSignal modifiableSignal)
            {
                using var lockedSamples = modifiableSignal.Samples;
                var samples = lockedSamples.Span;
                Vorcyc.Mathematics.Extensions.FFTW.Dft1D.Inverse(frequencyDomain.Result, samples);
            }
        }
    }

}
