using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;

/// <summary>
/// Represents fast (FFT) convolver.
/// </summary>
public class Convolver
{
    private int _fftSize;
    private RealFft _fft;
    private float[] _real1;
    private float[] _imag1;
    private float[] _real2;
    private float[] _imag2;

    public Convolver(int fftSize = 0)
    {
        if (fftSize > 0)
        {
            PrepareMemory(fftSize);
        }
    }

    private void PrepareMemory(int fftSize)
    {
        _fftSize = fftSize;
        _fft = new RealFft(_fftSize);

        _real1 = new float[_fftSize];
        _imag1 = new float[_fftSize];
        _real2 = new float[_fftSize];
        _imag2 = new float[_fftSize];
    }

    /// <summary>
    /// Does fast convolution via FFT. Returns length signal.Length + kernel.Length - 1.
    /// </summary>
    public Signal Convolve(Signal signal, Signal kernel)
    {
        var length = signal.Length + kernel.Length - 1;

        if (_fft is null)
        {
            PrepareMemory(length.NextPowerOf2());
        }

        var output = new float[_fftSize];
        Convolve(signal.Samples, kernel.Samples, output);

        return Signal.FromCopy(output.AsSpan(0, length), signal.SamplingRate);
    }

    public void Convolve(float[] input, float[] kernel, float[] output)
        => Convolve(input.AsSpan(), kernel.AsSpan(), output);

    /// <summary>
    /// Does fast convolution via FFT from sample spans.
    /// </summary>
    public void Convolve(ReadOnlySpan<float> input, ReadOnlySpan<float> kernel, float[] output)
    {
        Array.Clear(_real1, 0, _fftSize);
        Array.Clear(_real2, 0, _fftSize);

        input.Slice(0, input.Length).CopyTo(_real1.AsSpan(0, input.Length));
        kernel.Slice(0, kernel.Length).CopyTo(_real2.AsSpan(0, kernel.Length));

        _fft.Direct(_real1, _real1, _imag1);
        _fft.Direct(_real2, _real2, _imag2);

        for (var i = 0; i <= _fftSize / 2; i++)
        {
            var re = _real1[i] * _real2[i] - _imag1[i] * _imag2[i];
            var im = _real1[i] * _imag2[i] + _imag1[i] * _real2[i];
            _real1[i] = re / _fftSize;
            _imag1[i] = im / _fftSize;
        }

        _fft.Inverse(_real1, _imag1, output);
    }

    public Signal CrossCorrelate(Signal signal1, Signal signal2)
    {
        var length = signal1.Length + signal2.Length - 1;

        if (_fft is null)
        {
            PrepareMemory(length.NextPowerOf2());
        }

        var output = new float[_fftSize];
        CrossCorrelate(signal1.Samples, signal2.Samples, output);

        return Signal.FromCopy(output.AsSpan(0, length), signal1.SamplingRate);
    }

    public void CrossCorrelate(float[] input1, float[] input2, float[] output)
    {
        var kernelLength = input2.Length - 1;

        for (var i = 0; i < kernelLength / 2; i++)
        {
            var tmp = input2[i];
            input2[i] = input2[kernelLength - i];
            input2[kernelLength - i] = tmp;
        }

        Convolve(input1, input2, output);
    }

    /// <summary>
    /// Cross-correlates sample spans without mutating the inputs.
    /// </summary>
    public void CrossCorrelate(ReadOnlySpan<float> input1, ReadOnlySpan<float> input2, float[] output)
    {
        var reversed = new float[input2.Length];
        for (var i = 0; i < input2.Length; i++)
        {
            reversed[i] = input2[input2.Length - 1 - i];
        }

        Convolve(input1, reversed, output);
    }
}
