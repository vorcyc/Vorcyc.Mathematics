using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;

/// <summary>
/// Overlap-Save block convolver in <typeparamref name="T"/>.
/// <see cref="float"/> matches <see cref="OlsBlockConvolver"/>; <see cref="double"/> stays on
/// <see cref="RealFft64"/>. Existing <see cref="OlsBlockConvolver"/> remains float-only
/// (its <c>IEnumerable&lt;double&gt;</c> ctor still casts).
/// </summary>
public class OlsBlockConvolver<T> : IOnlineFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    private readonly T[] _kernel;
    private readonly int _fftSize;
    private readonly RealFft<T> _fft;
    private int _bufferOffset;
    private int _outputBufferOffset;
    private readonly T[] _kernelSpectrumRe;
    private readonly T[] _kernelSpectrumIm;
    private readonly T[] _blockRe;
    private readonly T[] _blockIm;
    private readonly T[] _convRe;
    private readonly T[] _convIm;
    private readonly T[] _lastSaved;

    /// <summary>Gets hop length: FFT size - kernel size + 1.</summary>
    public int HopSize => _fftSize - _kernel.Length + 1;

    /// <summary>Constructs an OLS convolver with <paramref name="kernel"/> and <paramref name="fftSize"/>.</summary>
    public OlsBlockConvolver(IEnumerable<T> kernel, int fftSize)
        : this(kernel is T[] array ? array.AsSpan() : kernel.ToArray().AsSpan(), fftSize)
    {
    }

    /// <summary>Constructs an OLS convolver from a kernel span.</summary>
    public OlsBlockConvolver(ReadOnlySpan<T> kernel, int fftSize)
    {
        FilterGenericSupport.EnsureFloatOrDouble<T>();
        _kernel = kernel.ToArray();
        _fftSize = fftSize.NextPowerOf2();
        Guard.AgainstExceedance(_kernel.Length, _fftSize, "Kernel length", "the size of FFT");

        _fft = new RealFft<T>(_fftSize);
        _kernelSpectrumRe = FilterGenericSupport.PadTo(_kernel, _fftSize);
        _kernelSpectrumIm = new T[_fftSize];
        _convRe = new T[_fftSize];
        _convIm = new T[_fftSize];
        _blockRe = new T[_fftSize];
        _blockIm = new T[_fftSize];
        _lastSaved = new T[Math.Max(0, _kernel.Length - 1)];

        _fft.Direct(_kernelSpectrumRe, _kernelSpectrumRe, _kernelSpectrumIm);
        Reset();
    }

    /// <summary>Constructs from an FIR kernel.</summary>
    public static OlsBlockConvolver<T> FromFilter(FirFilter<T> filter, int fftSize)
        => new(filter.Kernel, fftSize);

    /// <summary>Replaces kernel coefficients in place (same length only).</summary>
    public void ChangeKernel(T[] kernel)
    {
        if (kernel.Length != _kernel.Length)
            return;

        Array.Clear(_kernelSpectrumRe, 0, _fftSize);
        kernel.AsSpan().CopyTo(_kernel);
        kernel.AsSpan().CopyTo(_kernelSpectrumRe.AsSpan(0, kernel.Length));
        _fft.Direct(_kernelSpectrumRe, _kernelSpectrumRe, _kernelSpectrumIm);
    }

    /// <inheritdoc />
    public T Process(T sample)
    {
        var offset = _bufferOffset + _kernel.Length - 1;
        _blockRe[offset] = sample;
        if (++_bufferOffset == HopSize)
            ProcessFrame();
        return _convRe[_outputBufferOffset++];
    }

    private void ProcessFrame()
    {
        var m = _kernel.Length;
        var halfSize = _fftSize / 2;
        var scale = T.One / T.CreateChecked(_fftSize);

        _lastSaved.FastCopyTo(_blockRe, m - 1);
        _blockRe.FastCopyTo(_lastSaved, m - 1, HopSize);

        _fft.Direct(_blockRe, _blockRe, _blockIm);
        for (var j = 0; j <= halfSize; j++)
        {
            var re = _blockRe[j] * _kernelSpectrumRe[j] - _blockIm[j] * _kernelSpectrumIm[j];
            var im = _blockRe[j] * _kernelSpectrumIm[j] + _blockIm[j] * _kernelSpectrumRe[j];
            _convRe[j] = re * scale;
            _convIm[j] = im * scale;
        }
        _fft.Inverse(_convRe, _convIm, _convRe);

        _outputBufferOffset = m - 1;
        _bufferOffset = 0;
    }

    /// <summary>
    /// Processes an entire buffer. Result length is <c>signal.Length + kernel.Length - 1</c>.
    /// </summary>
    public T[] ApplyTo(ReadOnlySpan<T> signal)
    {
        Reset();
        var firstCount = Math.Min(HopSize - 1, signal.Length);
        int i = 0, j = 0;
        for (; i < firstCount; i++)
            Process(signal[i]);

        var filtered = new T[signal.Length + _kernel.Length - 1];
        for (; i < signal.Length; i++, j++)
            filtered[j] = Process(signal[i]);

        var lastCount = firstCount + _kernel.Length - 1;
        for (i = 0; i < lastCount; i++, j++)
            filtered[j] = Process(T.Zero);
        return filtered;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _bufferOffset = _kernel.Length - 1;
        _outputBufferOffset = 0;
        Array.Clear(_lastSaved);
        Array.Clear(_blockRe);
        Array.Clear(_blockIm);
        Array.Clear(_convRe);
        Array.Clear(_convIm);
    }
}

/// <summary>Double-precision OLS block convolver.</summary>
public sealed class OlsBlockConvolver64 : OlsBlockConvolver<double>
{
    /// <inheritdoc />
    public OlsBlockConvolver64(IEnumerable<double> kernel, int fftSize) : base(kernel, fftSize)
    {
    }

    /// <inheritdoc />
    public OlsBlockConvolver64(ReadOnlySpan<double> kernel, int fftSize) : base(kernel, fftSize)
    {
    }
}
