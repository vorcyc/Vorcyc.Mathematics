﻿using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;

/// <summary>
/// Represents signal processor that implements Overlap-Save algorithm of block convolution.
/// </summary>
public class OlsBlockConvolver : IFilter, IOnlineFilter
{
    /// <summary>
    /// Filter kernel.
    /// </summary>
    private readonly float[] _kernel;

    /// <summary>
    /// FFT size.
    /// </summary>
    private readonly int _fftSize;

    /// <summary>
    /// Internal FFT transformer.
    /// </summary>
    private readonly RealFft _fft;

    /// <summary>
    /// Offset in the input delay line.
    /// </summary>
    private int _bufferOffset;

    /// <summary>
    /// Offset in the delay line.
    /// </summary>
    private int _outputBufferOffset;

    //
    // internal buffers
    //
    private readonly float[] _kernelSpectrumRe;
    private readonly float[] _kernelSpectrumIm;
    private readonly float[] _blockRe;
    private readonly float[] _blockIm;
    private readonly float[] _convRe;
    private readonly float[] _convIm;
    private readonly float[] _lastSaved;

    /// <summary>
    /// Gets hop length: FFT size - kernel size + 1.
    /// </summary>
    public int HopSize => _fftSize - _kernel.Length + 1;

    /// <summary>
    /// Constructs <see cref="OlsBlockConvolver"/> with given <paramref name="kernel"/> and <paramref name="fftSize"/>.
    /// </summary>
    public OlsBlockConvolver(IEnumerable<float> kernel, int fftSize)
    {
        _kernel = kernel.ToArray();

        _fftSize = (fftSize).NextPowerOf2();

        Guard.AgainstExceedance(_kernel.Length, _fftSize, "Kernel length", "the size of FFT");

        _fft = new RealFft(_fftSize);

        _kernelSpectrumRe = _kernel.PadZeros(_fftSize);
        _kernelSpectrumIm = new float[_fftSize];
        _convRe = new float[_fftSize];
        _convIm = new float[_fftSize];
        _blockRe = new float[_fftSize];
        _blockIm = new float[_fftSize];
        _lastSaved = new float[_kernel.Length - 1];

        _fft.Direct(_kernelSpectrumRe, _kernelSpectrumRe, _kernelSpectrumIm);

        Reset();
    }

    /// <summary>
    /// Constructs <see cref="OlsBlockConvolver"/> with given <paramref name="kernel"/> and <paramref name="fftSize"/>.
    /// </summary>
    public OlsBlockConvolver(IEnumerable<double> kernel, int fftSize) : this(kernel.ToFloats(), fftSize)
    {
    }

    /// <summary>
    /// Constructs <see cref="OlsBlockConvolver"/> with given FIR <paramref name="filter"/> kernel and <paramref name="fftSize"/>.
    /// </summary>
    public static OlsBlockConvolver FromFilter(FirFilter filter, int fftSize)
    {
        fftSize = fftSize.NextPowerOf2();
        return new OlsBlockConvolver(filter.Kernel, fftSize);
    }

    /// <summary>
    /// Changes <paramref name="kernel"/> coefficients online.
    /// </summary>
    public void ChangeKernel(float[] kernel)
    {
        if (kernel.Length != _kernel.Length) return;
        
        Array.Clear(_kernelSpectrumRe, 0, _fftSize);
        kernel.FastCopyTo(_kernel, kernel.Length);
        kernel.FastCopyTo(_kernelSpectrumRe, kernel.Length);

        _fft.Direct(_kernelSpectrumRe, _kernelSpectrumRe, _kernelSpectrumIm);
    }

    /// <summary>
    /// Processes one sample.
    /// </summary>
    /// <param name="sample">Input sample</param>
    public float Process(float sample)
    {
        var offset = _bufferOffset + _kernel.Length - 1;

        _blockRe[offset] = sample;

        if (++_bufferOffset == HopSize)
        {
            ProcessFrame();
        }

        return _convRe[_outputBufferOffset++];
    }

    /// <summary>
    /// Process one frame (block).
    /// </summary>
    protected void ProcessFrame()
    {
        var M = _kernel.Length;

        var halfSize = _fftSize / 2;

        _lastSaved.FastCopyTo(_blockRe, M - 1);
        _blockRe.FastCopyTo(_lastSaved, M - 1, HopSize);

        _fft.Direct(_blockRe, _blockRe, _blockIm);

        for (var j = 0; j <= halfSize; j++)
        {
            _convRe[j] = (_blockRe[j] * _kernelSpectrumRe[j] - _blockIm[j] * _kernelSpectrumIm[j]) / _fftSize;
            _convIm[j] = (_blockRe[j] * _kernelSpectrumIm[j] + _blockIm[j] * _kernelSpectrumRe[j]) / _fftSize;
        }

        _fft.Inverse(_convRe, _convIm, _convRe);

        _outputBufferOffset = M - 1;
        _bufferOffset = 0;
    }

    /// <summary>
    /// Processes entire <paramref name="signal"/> and returns new filtered signal.
    /// </summary>
    /// <param name="signal">Input signal</param>
    /// <param name="method">Filtering method</param>
    public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
    {
        var firstCount = Math.Min(HopSize - 1, signal.SampleCount);

        int i = 0, j = 0;

        for (; i < firstCount; i++)    // first HopSize-1 samples are just placed in the delay line
        {
            Process(signal[i]);
        }

        var filtered = new float[signal.SampleCount + _kernel.Length - 1];

        for (; i < signal.SampleCount; i++, j++)    // process
        {
            filtered[j] = Process(signal[i]);
        }

        var lastCount = firstCount + _kernel.Length - 1;

        for (i = 0; i < lastCount; i++, j++)    // get last 'late' samples
        {
            filtered[j] = Process(0.0f);
        }

        return new DiscreteSignal(signal.SamplingRate, filtered);
    }

    /// <summary>
    /// Resets Overlap-Save convolver.
    /// </summary>
    public void Reset()
    {
        _bufferOffset = _kernel.Length - 1;
        _outputBufferOffset = 0;

        Array.Clear(_lastSaved, 0, _lastSaved.Length);
        Array.Clear(_blockRe, 0, _blockRe.Length);
        Array.Clear(_blockIm, 0, _blockIm.Length);
        Array.Clear(_convRe, 0, _convRe.Length);
        Array.Clear(_convIm, 0, _convIm.Length);
    }
}
