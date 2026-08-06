using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier
{
    /// <summary>
    /// Represents Short-Time Fourier Transform.
    /// </summary>
    public class Stft
    {
        /// <summary>
        /// Gets FFT size.
        /// </summary>
        public int Size => _fftSize;
        private readonly int _fftSize;

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        private readonly RealFft _fft;

        /// <summary>
        /// Overlap size (number of samples).
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Window size (number of samples).
        /// </summary>
        private readonly int _windowSize;

        /// <summary>
        /// Window type.
        /// </summary>
        private readonly WindowType _window;

        /// <summary>
        /// Pre-computed samples of the window function.
        /// </summary>
        private readonly float[] _windowSamples;

        /// <summary>
        /// Constructs STFT transformer.
        /// </summary>
        /// <param name="windowSize">Size of analysis window</param>
        /// <param name="hopSize">Hop (overlap) size</param>
        /// <param name="window">Type of the window function to apply</param>
        /// <param name="fftSize">Size of FFT</param>
        public Stft(int windowSize = 1024, int hopSize = 256, WindowType window = WindowType.Hann, int fftSize = 0)
        {
            _fftSize = fftSize >= windowSize ? fftSize : windowSize.NextPowerOf2();// MathUtils.NextPowerOfTwo(windowSize);
            _fft = new RealFft(_fftSize);

            _hopSize = hopSize;
            _windowSize = windowSize;
            _window = window;
            _windowSamples = WindowBuilder.OfType(_window, _windowSize);
        }

        /// <summary>
        /// Does STFT of an <paramref name="input"/>.
        /// Returns list of computed spectra (real and imaginary parts) in time.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="context">Execution context driving Normal/SIMD/Parallel dispatch (optional)</param>
        public List<(float[], float[])> Direct(float[] input, ComputingContext? context = null)
            => Direct(input.AsSpan(), context);

        /// <summary>
        /// Does STFT of sample data.
        /// </summary>
        public List<(float[], float[])> Direct(ReadOnlySpan<float> input, ComputingContext? context = null)
        {
            var len = input.Length >= _windowSize ? (input.Length - _windowSize) / _hopSize + 1 : 0;

            var stft = new List<(float[], float[])>(len + 1);

            for (var i = 0; i < len; i++)
            {
                stft.Add((new float[_fftSize], new float[_fftSize]));
            }

            bool frameParallel = ComputingContextExecution.UseParallelIndexed(context, len, _fftSize);
            var frameFft = frameParallel ? NestedFrameFftContext(context) : context;

            // stft:

            if (frameParallel)
            {
                // Frame-level parallelism: each frame writes its own pre-allocated (re, im)
                // slot, so writes never overlap. One RealFft + scratch buffer per worker keeps
                // the internal FFT buffers thread-safe. Frame FFT uses SIMD when outer is Parallel.
                var inputArray = input.ToArray();
                using var fftLocal = new ThreadLocal<RealFft>(() => new RealFft(_fftSize));
                using var bufLocal = new ThreadLocal<float[]>(() => new float[_fftSize]);

                ComputingContextExecution.ForEach(context, 0, len, i =>
                {
                    var buf = bufLocal.Value!;
                    CopyFrame(inputArray, i * _hopSize, _windowSize, buf);
                    buf.ApplyWindow(_windowSamples);
                    var (re, im) = stft[i];
                    fftLocal.Value!.Direct(buf, re, im, frameFft);
                }, workPerItem: _fftSize);
            }
            else
            {
                var windowedBuffer = new float[_fftSize];
                var pos = 0;
                for (var i = 0; i < len; pos += _hopSize, i++)
                {
                    CopyFrame(input, pos, _windowSize, windowedBuffer);
                    windowedBuffer.ApplyWindow(_windowSamples);
                    var (re, im) = stft[i];
                    _fft.Direct(windowedBuffer, re, im, frameFft);
                }
            }

            // last (incomplete) frame (always serial):

            stft.Add((new float[_fftSize], new float[_fftSize]));

            var lastBuffer = new float[_fftSize];
            var lastPos = len * _hopSize;
            CopyFrame(input, lastPos, input.Length - lastPos, lastBuffer);
            lastBuffer.ApplyWindow(_windowSamples);

            var (lre, lim) = stft.Last();

            _fft.Direct(lastBuffer, lre, lim, context);

            return stft;
        }

        /// <summary>
        /// Does STFT of a <paramref name="signal"/>.
        /// Returns list of computed spectra (real and imaginary parts) in time.
        /// </summary>
        /// <param name="signal">Input signal</param>
        public List<(float[], float[])> Direct(Signal signal, ComputingContext? context = null)
            => Direct(signal.Samples, context);

        /// <summary>
        /// Does Inverse STFT from list of spectra <paramref name="stft"/>.
        /// </summary>
        /// <param name="stft">List of spectra (real and imaginary parts)</param>
        /// <param name="perfectReconstruction">Perfect reconstruction mode</param>
        public float[] Inverse(List<(float[], float[])> stft, bool perfectReconstruction = true, ComputingContext? context = null)
        {
            var spectraCount = stft.Count;
            var output = new float[spectraCount * _hopSize + _fftSize];

            float gain;

            if (perfectReconstruction)
            {
                Guard.AgainstExceedance(_hopSize, _windowSize, "Hop size for perfect reconstruction", "window size");

                gain = 1f / _windowSize;
            }
            // simpler reconstruction of the signal
            // (with insignificant discrepancies in the beginning and in the end)
            else
            {
                gain = 1 / (_fftSize * _windowSamples.Select(w => w * w).Sum() / _hopSize);
            }

            // Two-phase: parallel per-frame IFFT into disjoint buffers, then serial overlap-add
            // (the overlap-add writes into shared, overlapping output regions so it stays serial).
            if (ComputingContextExecution.UseParallelIndexed(context, spectraCount, _fftSize))
            {
                var frameBufs = new float[spectraCount][];
                using var fftLocal = new ThreadLocal<RealFft>(() => new RealFft(_fftSize));
                var frameFft = NestedFrameFftContext(context);

                ComputingContextExecution.ForEach(context, 0, spectraCount, i =>
                {
                    var (re, im) = stft[i];
                    var b = new float[_fftSize];
                    fftLocal.Value!.Inverse(re, im, b, frameFft);
                    frameBufs[i] = b;
                }, workPerItem: _fftSize);

                var p = 0;
                for (var i = 0; i < spectraCount; i++)
                {
                    var b = frameBufs[i];
                    for (var j = 0; j < _windowSize; j++)
                    {
                        output[p + j] += b[j] * _windowSamples[j];
                    }
                    for (var j = 0; j < _hopSize; j++)
                    {
                        output[p + j] *= gain;
                    }
                    p += _hopSize;
                }

                return InverseFinalize(output, gain, perfectReconstruction, spectraCount * _hopSize);
            }

            var buf = new float[_fftSize];

            var pos = 0;

            for (var i = 0; i < spectraCount; i++)
            {
                var (re, im) = stft[i];

                _fft.Inverse(re, im, buf, context);

                // windowing and reconstruction

                for (var j = 0; j < _windowSize; j++)
                {
                    output[pos + j] += buf[j] * _windowSamples[j];
                }

                for (var j = 0; j < _hopSize; j++)
                {
                    output[pos + j] *= gain;
                }

                pos += _hopSize;
            }

            return InverseFinalize(output, gain, perfectReconstruction, pos);
        }

        /// <summary>
        /// Applies the trailing-frame gain and (optionally) the perfect-reconstruction
        /// window normalization shared by <see cref="Inverse"/> and
        /// <see cref="ReconstructMagnitudePhase"/>.
        /// </summary>
        /// <param name="output">Overlap-added output buffer</param>
        /// <param name="gain">Reconstruction gain</param>
        /// <param name="perfectReconstruction">Perfect reconstruction mode</param>
        /// <param name="tailPos">Position where the last frame's window begins</param>
        private float[] InverseFinalize(float[] output, float gain, bool perfectReconstruction, int tailPos)
        {
            for (var j = 0; j < _windowSize; j++)
            {
                output[tailPos + j] *= gain;
            }

            if (perfectReconstruction)      // additional normalization
            {
                float[] windowSummed = ComputeWindowSummed();

                var offset = _windowSize - _hopSize;

                for (int j = 0, k = output.Length - _hopSize - 1; j < offset; j++, k--)
                {
                    if (Math.Abs(windowSummed[j]) > 1e-30)
                    {
                        output[j] /= windowSummed[j];   // leftmost part of the signal
                        output[k] /= windowSummed[j];   // rightmost part of the signal
                    }
                }

                // main central part of the signal

                for (int j = offset, k = offset; j < output.Length - _windowSize; j++, k++)
                {
                    if (k == _windowSize) k = offset;

                    output[j] /= windowSummed[k];
                }
            }

            return output;
        }

        /// <summary>
        /// Helper method for ISTFT in 'perfect reconstruction' mode.
        /// </summary>
        /// <returns>Summed window coefficients</returns>
        private float[] ComputeWindowSummed()
        {
            var windowSummed = new float[_windowSize];

            for (var pos = 0; pos < _windowSize; pos += _hopSize)
            {
                for (var j = 0; pos + j < _windowSize; j++)
                {
                    windowSummed[pos + j] += _windowSamples[j] * _windowSamples[j];
                }
            }

            return windowSummed;
        }

        /// <summary>
        /// Computes spectrogram. 
        /// The spectrogram is essentially a list of power spectra in time.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="normalize">Normalize each spectrum</param>
        public List<float[]> Spectrogram(float[] input, bool normalize = true, ComputingContext? context = null)
            => Spectrogram(input.AsSpan(), normalize, context);

        /// <summary>
        /// Computes spectrogram from sample data.
        /// </summary>
        public List<float[]> Spectrogram(ReadOnlySpan<float> input, bool normalize = true, ComputingContext? context = null)
        {
            var len = input.Length >= _windowSize ? (input.Length - _windowSize) / _hopSize + 1 : 0;

            var spectrogram = new List<float[]>(len + 1);

            for (var i = 0; i < len; i++)
            {
                spectrogram.Add(new float[_fftSize / 2 + 1]);
            }

            // spectrogram:

            bool frameParallel = ComputingContextExecution.UseParallelIndexed(context, len, _fftSize);
            var frameFft = frameParallel ? NestedFrameFftContext(context) : context;

            if (frameParallel)
            {
                var inputArray = input.ToArray();
                using var fftLocal = new ThreadLocal<RealFft>(() => new RealFft(_fftSize));
                using var bufLocal = new ThreadLocal<float[]>(() => new float[_fftSize]);

                ComputingContextExecution.ForEach(context, 0, len, i =>
                {
                    var buf = bufLocal.Value!;
                    CopyFrame(inputArray, i * _hopSize, _windowSize, buf);
                    if (_window != WindowType.Rectangular)
                    {
                        buf.ApplyWindow(_windowSamples);
                    }
                    fftLocal.Value!.PowerSpectrum(buf, spectrogram[i], normalize, frameFft);
                }, workPerItem: _fftSize);
            }
            else
            {
                var windowedBuffer = new float[_fftSize];
                var pos = 0;
                for (int i = 0; i < len; pos += _hopSize, i++)
                {
                    CopyFrame(input, pos, _windowSize, windowedBuffer);
                    if (_window != WindowType.Rectangular)
                    {
                        windowedBuffer.ApplyWindow(_windowSamples);
                    }
                    _fft.PowerSpectrum(windowedBuffer, spectrogram[i], normalize, frameFft);
                }
            }

            // last (incomplete) frame (always serial):

            var lastBuffer = new float[_fftSize];
            var lastPos = len * _hopSize;
            CopyFrame(input, lastPos, input.Length - lastPos, lastBuffer);
            lastBuffer.ApplyWindow(_windowSamples);

            spectrogram.Add(new float[_fftSize / 2 + 1]);

            _fft.PowerSpectrum(lastBuffer, spectrogram.Last(), normalize, context);

            return spectrogram;
        }

        /// <summary>
        /// Computes spectrogram from <paramref name="signal"/>.
        /// </summary>
        public List<float[]> Spectrogram(Signal signal, bool normalize = true, ComputingContext? context = null)
            => Spectrogram(signal.Samples, normalize, context);

        /// <summary>
        /// Computes averaged periodogram (Welch-style): mean of per-frame power spectra over
        /// <b>complete</b> windows only (trailing samples that do not fill a window are discarded).
        /// Memory-efficient — does not store all spectra. Honors <paramref name="context"/> for
        /// frame-level parallel dispatch; when frames are parallelized, each frame FFT uses SIMD
        /// (not nested Parallel) to avoid oversubscription.
        /// </summary>
        /// <param name="input">Input data</param>
        public float[] AveragePeriodogram(float[] input, ComputingContext? context = null)
            => AveragePeriodogram(input.AsSpan(), context);

        /// <summary>
        /// Computes averaged periodogram from sample data (complete frames only; see overload remarks).
        /// </summary>
        public float[] AveragePeriodogram(ReadOnlySpan<float> input, ComputingContext? context = null)
        {
            var len = input.Length >= _windowSize ? (input.Length - _windowSize) / _hopSize + 1 : 0;

            var binCount = _fftSize / 2 + 1;
            var periodogram = new float[binCount];
            if (len < 1)
                return periodogram;

            // Frame-level parallelism; when active, downgrade inner FFT to SIMD to avoid nested Parallel.For.
            bool frameParallel = ComputingContextExecution.UseParallelIndexed(context, len, _fftSize);
            var frameFftContext = frameParallel ? NestedFrameFftContext(context) : context;

            if (frameParallel)
            {
                var inputArray = input.ToArray();
                var locals = new System.Collections.Concurrent.ConcurrentBag<float[]>();
                using var fftLocal = new ThreadLocal<RealFft>(() => new RealFft(_fftSize));
                using var bufLocal = new ThreadLocal<float[]>(() => new float[_fftSize]);
                using var specLocal = new ThreadLocal<float[]>(() => new float[binCount]);
                using var accLocal = new ThreadLocal<float[]>(() =>
                {
                    var acc = new float[binCount];
                    locals.Add(acc);
                    return acc;
                }, trackAllValues: false);

                ComputingContextExecution.ForEach(context, 0, len, i =>
                {
                    var buf = bufLocal.Value!;
                    CopyFrame(inputArray, i * _hopSize, _windowSize, buf);
                    if (_window != WindowType.Rectangular)
                    {
                        buf.ApplyWindow(_windowSamples);
                    }
                    var spectrum = specLocal.Value!;
                    fftLocal.Value!.PowerSpectrum(buf, spectrum, false, frameFftContext);
                    var acc = accLocal.Value!;
                    for (var j = 0; j < binCount; j++)
                    {
                        acc[j] += spectrum[j];
                    }
                }, workPerItem: _fftSize);

                foreach (var acc in locals)
                {
                    for (var j = 0; j < binCount; j++)
                    {
                        periodogram[j] += acc[j];
                    }
                }
            }
            else
            {
                var spectrum = new float[binCount];
                var windowedBuffer = new float[_fftSize];
                var pos = 0;
                for (var i = 0; i < len; pos += _hopSize, i++)
                {
                    CopyFrame(input, pos, _windowSize, windowedBuffer);
                    if (_window != WindowType.Rectangular)
                    {
                        windowedBuffer.ApplyWindow(_windowSamples);
                    }
                    _fft.PowerSpectrum(windowedBuffer, spectrum, false, frameFftContext);
                    for (var j = 0; j < binCount; j++)
                    {
                        periodogram[j] += spectrum[j];
                    }
                }
            }

            float inv = 1f / len;
            for (var j = 0; j < binCount; j++)
                periodogram[j] *= inv;

            return periodogram;
        }

        /// <summary>
        /// When the outer loop already uses <see cref="CpuExecutionMode.Parallel"/>, prefer SIMD
        /// (or Normal) for per-frame FFTs so workers are not nested Parallel.For calls.
        /// </summary>
        static ComputingContext NestedFrameFftContext(ComputingContext? context)
        {
            var mode = ComputingContext.Resolve(context).CpuMode;
            return mode == CpuExecutionMode.Normal
                ? ComputingContext.Normal
                : ComputingContext.Simd;
        }

        /// <summary>
        /// Computes averaged periodogram from <paramref name="signal"/>.
        /// </summary>
        public float[] AveragePeriodogram(Signal signal, ComputingContext? context = null)
            => AveragePeriodogram(signal.Samples, context);

        /// <summary>
        /// Computes spectrogram in the form of list of magnitudes and phases from <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Input data</param>
        public MagnitudePhaseList MagnitudePhaseSpectrogram(float[] input, ComputingContext? context = null)
            => MagnitudePhaseSpectrogram(input.AsSpan(), context);

        /// <summary>
        /// Computes magnitude-phase spectrogram from sample data.
        /// </summary>
        public MagnitudePhaseList MagnitudePhaseSpectrogram(ReadOnlySpan<float> input, ComputingContext? context = null)
        {
            var len = input.Length >= _windowSize ? (input.Length - _windowSize) / _hopSize + 1 : 0;

            var binCount = _fftSize / 2 + 1;

            var mag = new List<float[]>(len + 1);
            var phase = new List<float[]>(len + 1);

            for (var i = 0; i < len; i++)
            {
                mag.Add(new float[binCount]);
                phase.Add(new float[binCount]);
            }

            // magnitude-phase spectrogram:

            bool frameParallel = ComputingContextExecution.UseParallelIndexed(context, len, _fftSize);
            var frameFft = frameParallel ? NestedFrameFftContext(context) : context;

            if (frameParallel)
            {
                var inputArray = input.ToArray();
                using var fftLocal = new ThreadLocal<RealFft>(() => new RealFft(_fftSize));
                using var bufLocal = new ThreadLocal<float[]>(() => new float[_fftSize]);
                using var reLocal = new ThreadLocal<float[]>(() => new float[binCount]);
                using var imLocal = new ThreadLocal<float[]>(() => new float[binCount]);

                ComputingContextExecution.ForEach(context, 0, len, i =>
                {
                    var buf = bufLocal.Value!;
                    CopyFrame(inputArray, i * _hopSize, _windowSize, buf);
                    buf.ApplyWindow(_windowSamples);
                    var re = reLocal.Value!;
                    var im = imLocal.Value!;
                    fftLocal.Value!.Direct(buf, re, im, frameFft);
                    var mi = mag[i];
                    var pi = phase[i];
                    for (var j = 0; j < binCount; j++)
                    {
                        mi[j] = MathF.Sqrt(re[j] * re[j] + im[j] * im[j]);
                        pi[j] = MathF.Atan2(im[j], re[j]);
                    }
                }, workPerItem: _fftSize);
            }
            else
            {
                var windowedBuffer = new float[_fftSize];
                var re = new float[binCount];
                var im = new float[binCount];
                var pos = 0;
                for (var i = 0; i < len; pos += _hopSize, i++)
                {
                    CopyFrame(input, pos, _windowSize, windowedBuffer);
                    windowedBuffer.ApplyWindow(_windowSamples);
                    _fft.Direct(windowedBuffer, re, im, frameFft);
                    for (var j = 0; j < binCount; j++)
                    {
                        mag[i][j] = MathF.Sqrt(re[j] * re[j] + im[j] * im[j]);
                        phase[i][j] = MathF.Atan2(im[j], re[j]);
                    }
                }
            }

            // last (incomplete) frame (always serial):

            var lastBuffer = new float[_fftSize];
            var lastRe = new float[binCount];
            var lastIm = new float[binCount];
            var lastPos = len * _hopSize;
            CopyFrame(input, lastPos, input.Length - lastPos, lastBuffer);
            lastBuffer.ApplyWindow(_windowSamples);

            mag.Add(new float[binCount]);
            phase.Add(new float[binCount]);

            _fft.Direct(lastBuffer, lastRe, lastIm, context);

            var m = mag.Last();
            var p = phase.Last();

            for (var j = 0; j < binCount; j++)
            {
                m[j] = MathF.Sqrt(lastRe[j] * lastRe[j] + lastIm[j] * lastIm[j]);
                p[j] = MathF.Atan2(lastIm[j], lastRe[j]);
            }

            return new MagnitudePhaseList { Magnitudes = mag, Phases = phase };
        }

        /// <summary>
        /// Computes spectrogram in the form of list of magnitudes and phases from <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Input signal</param>
        public MagnitudePhaseList MagnitudePhaseSpectrogram(Signal signal, ComputingContext? context = null)
            => MagnitudePhaseSpectrogram(signal.Samples, context);

        /// <summary>
        /// Reconstructs samples from <paramref name="spectrogram"/> in the form of list of magnitudes and phases.
        /// </summary>
        /// <param name="spectrogram">Spectrogram in the form of list of magnitudes and phases</param>
        /// <param name="perfectReconstruction">Perfect reconstruction mode</param>
        public float[] ReconstructMagnitudePhase(MagnitudePhaseList spectrogram, bool perfectReconstruction = true, ComputingContext? context = null)
        {
            var spectraCount = spectrogram.Magnitudes.Count;
            var output = new float[spectraCount * _hopSize + _windowSize];

            var mag = spectrogram.Magnitudes;
            var phase = spectrogram.Phases;

            var binCount = _fftSize / 2 + 1;

            float gain;

            if (perfectReconstruction)
            {
                Guard.AgainstExceedance(_hopSize, _windowSize, "Hop size for perfect reconstruction", "window size");

                gain = 1f / _windowSize;
            }
            // simpler reconstruction of the signal
            // (with insignificant discrepancies in the beginning and in the end)
            else
            {
                gain = 1 / (_fftSize * _windowSamples.Select(w => w * w).Sum() / _hopSize);
            }

            // Two-phase: parallel per-frame IFFT (mag/phase -> re/im -> buffer) into disjoint
            // buffers, then serial overlap-add into the shared, overlapping output regions.
            if (ComputingContextExecution.UseParallelIndexed(context, spectraCount, _fftSize))
            {
                var frameBufs = new float[spectraCount][];
                using var fftLocal = new ThreadLocal<RealFft>(() => new RealFft(_fftSize));
                using var reLocal = new ThreadLocal<float[]>(() => new float[binCount]);
                using var imLocal = new ThreadLocal<float[]>(() => new float[binCount]);
                var frameFft = NestedFrameFftContext(context);

                ComputingContextExecution.ForEach(context, 0, spectraCount, i =>
                {
                    var re = reLocal.Value!;
                    var im = imLocal.Value!;
                    var mi = mag[i];
                    var pi = phase[i];
                    for (var j = 0; j < binCount; j++)
                    {
                        re[j] = mi[j] * MathF.Cos(pi[j]);
                        im[j] = mi[j] * MathF.Sin(pi[j]);
                    }
                    var b = new float[_fftSize];
                    fftLocal.Value!.Inverse(re, im, b, frameFft);
                    frameBufs[i] = b;
                }, workPerItem: _fftSize);

                var p = 0;
                for (var i = 0; i < spectraCount; i++)
                {
                    var b = frameBufs[i];
                    for (var j = 0; j < _windowSize; j++)
                    {
                        output[p + j] += b[j] * _windowSamples[j];
                    }
                    for (var j = 0; j < _hopSize; j++)
                    {
                        output[p + j] *= gain;
                    }
                    p += _hopSize;
                }

                return InverseFinalize(output, gain, perfectReconstruction, spectraCount * _hopSize);
            }

            var buf = new float[_fftSize];
            var re0 = new float[binCount];
            var im0 = new float[binCount];

            var pos = 0;

            for (var i = 0; i < spectraCount; i++)
            {
                for (var j = 0; j < binCount; j++)
                {
                    re0[j] = mag[i][j] * MathF.Cos(phase[i][j]);
                    im0[j] = mag[i][j] * MathF.Sin(phase[i][j]);
                }

                _fft.Inverse(re0, im0, buf, context);

                // windowing and reconstruction

                for (var j = 0; j < _windowSize; j++)
                {
                    output[pos + j] += buf[j] * _windowSamples[j];
                }

                for (var j = 0; j < _hopSize; j++)
                {
                    output[pos + j] *= gain;
                }

                pos += _hopSize;
            }

            return InverseFinalize(output, gain, perfectReconstruction, pos);
        }

        private static void CopyFrame(ReadOnlySpan<float> input, int position, int count, Span<float> destination)
        {
            input.Slice(position, count).CopyTo(destination.Slice(0, count));
        }
    }

    /// <summary>
    /// Represents spectrogram in the form of list of magnitudes and phases.
    /// </summary>
    public struct MagnitudePhaseList
    {
        /// <summary>
        /// Gets or sets list of magnitudes.
        /// </summary>
        public List<float[]> Magnitudes { get; set; }

        /// <summary>
        /// Gets or sets list of phases.
        /// </summary>
        public List<float[]> Phases { get; set; }
    }
}
