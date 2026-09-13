using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;

/// <summary>
/// Fast (FFT) convolver / cross-correlator in <typeparamref name="T"/>.
/// <see cref="float"/> matches <see cref="Convolver"/> numerically; <see cref="double"/>
/// stays on <see cref="RealFft64"/>. Existing <see cref="Convolver"/> remains float-only.
/// </summary>
/// <typeparam name="T">IEEE-754 sample type (<see cref="float"/> or <see cref="double"/>).</typeparam>
public class Convolver<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{
    private int _fftSize;
    private RealFft<T>? _fft;
    private T[]? _real1;
    private T[]? _imag1;
    private T[]? _real2;
    private T[]? _imag2;
    private T[]? _ifftOut;

    /// <summary>
    /// Constructs a convolver. When <paramref name="fftSize"/> is 0, the first
    /// call sizes the FFT to the next power of two of <c>N + M - 1</c>.
    /// </summary>
    public Convolver(int fftSize = 0)
    {
        FilterGenericSupport.EnsureFloatOrDouble<T>();
        if (fftSize > 0)
            PrepareMemory(fftSize);
    }

    /// <summary>Gets the prepared FFT length (0 before the first operation when constructed with 0).</summary>
    public int FftSize => _fftSize;

    private void PrepareMemory(int fftSize)
    {
        _fftSize = fftSize;
        _fft = new RealFft<T>(_fftSize);
        _real1 = new T[_fftSize];
        _imag1 = new T[_fftSize];
        _real2 = new T[_fftSize];
        _imag2 = new T[_fftSize];
        _ifftOut = new T[_fftSize];
    }

    private void EnsurePrepared(int convLength)
    {
        if (_fft is not null)
            return;
        PrepareMemory(convLength.NextPowerOf2());
    }

    /// <summary>
    /// Fast convolution via FFT. Writes the first <c>input.Length + kernel.Length - 1</c>
    /// samples (or <paramref name="output"/>.Length, whichever is smaller).
    /// </summary>
    public void Convolve(T[] input, T[] kernel, T[] output, ComputingContext? context = null)
        => Convolve(input.AsSpan(), kernel.AsSpan(), output, context);

    /// <summary>
    /// Fast convolution via FFT from sample spans. Result length is
    /// <c>input.Length + kernel.Length - 1</c>.
    /// </summary>
    public void Convolve(ReadOnlySpan<T> input, ReadOnlySpan<T> kernel, T[] output, ComputingContext? context = null)
    {
        ArgumentOutOfRangeException.ThrowIfZero(input.Length);
        ArgumentOutOfRangeException.ThrowIfZero(kernel.Length);
        ArgumentNullException.ThrowIfNull(output);

        var convLength = input.Length + kernel.Length - 1;
        EnsurePrepared(convLength);

        Array.Clear(_real1!, 0, _fftSize);
        Array.Clear(_imag1!, 0, _fftSize);
        Array.Clear(_real2!, 0, _fftSize);
        Array.Clear(_imag2!, 0, _fftSize);

        input.CopyTo(_real1.AsSpan(0, input.Length));
        kernel.CopyTo(_real2.AsSpan(0, kernel.Length));

        _fft!.Direct(_real1!, _real1!, _imag1!, context);
        _fft.Direct(_real2!, _real2!, _imag2!, context);

        var scale = T.One / T.CreateChecked(_fftSize);
        var bins = _fftSize / 2;
        for (var i = 0; i <= bins; i++)
        {
            var re = _real1![i] * _real2![i] - _imag1![i] * _imag2![i];
            var im = _real1[i] * _imag2[i] + _imag1[i] * _real2[i];
            _real1[i] = re * scale;
            _imag1[i] = im * scale;
        }

        _fft.Inverse(_real1!, _imag1!, _ifftOut!, context);

        var n = Math.Min(output.Length, convLength);
        _ifftOut.AsSpan(0, n).CopyTo(output.AsSpan(0, n));
    }

    /// <summary>
    /// Fast convolution; returns a new array of length <c>input.Length + kernel.Length - 1</c>.
    /// </summary>
    public T[] Convolve(T[] input, T[] kernel, ComputingContext? context = null)
    {
        var output = new T[input.Length + kernel.Length - 1];
        Convolve(input, kernel, output, context);
        return output;
    }

    /// <summary>
    /// Fast cross-correlation via FFT (does not mutate the inputs).
    /// Result length is <c>input1.Length + input2.Length - 1</c>.
    /// </summary>
    public void CrossCorrelate(T[] input1, T[] input2, T[] output, ComputingContext? context = null)
        => CrossCorrelate(input1.AsSpan(), input2.AsSpan(), output, context);

    /// <summary>
    /// Fast cross-correlation via FFT from sample spans (does not mutate the inputs).
    /// </summary>
    public void CrossCorrelate(ReadOnlySpan<T> input1, ReadOnlySpan<T> input2, T[] output, ComputingContext? context = null)
    {
        var reversed = new T[input2.Length];
        for (var i = 0; i < input2.Length; i++)
            reversed[i] = input2[input2.Length - 1 - i];
        Convolve(input1, reversed, output, context);
    }

    /// <summary>
    /// Fast cross-correlation; returns a new array of length <c>input1.Length + input2.Length - 1</c>.
    /// </summary>
    public T[] CrossCorrelate(T[] input1, T[] input2, ComputingContext? context = null)
    {
        var output = new T[input1.Length + input2.Length - 1];
        CrossCorrelate(input1, input2, output, context);
        return output;
    }
}

/// <summary>Double-precision FFT convolver (same as <see cref="Convolver{T}"/> of <see cref="double"/>).</summary>
public sealed class Convolver64 : Convolver<double>
{
    /// <inheritdoc />
    public Convolver64(int fftSize = 0) : base(fftSize)
    {
    }
}
