using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms;

/// <summary>
/// Generic Hilbert / analytic-signal transform. <see cref="float"/> and <see cref="double"/>
/// use <see cref="Fft{T}"/> (fp32 or fp64 kernel).
/// </summary>
/// <typeparam name="T">IEEE-754 sample type (<see cref="float"/> or <see cref="double"/>).</typeparam>
public sealed class HilbertTransform<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{
    private readonly Fft<T> _fft;
    private readonly T[] _re;
    private readonly T[] _im;

    /// <summary>Gets the transform length (power of two).</summary>
    public int Size { get; }

    /// <summary>Constructs a Hilbert transformer. <paramref name="size"/> must be a power of two.</summary>
    public HilbertTransform(int size = 512)
    {
        Size = size;
        _fft = new Fft<T>(size);
        _re = new T[size];
        _im = new T[size];
    }

    /// <summary>
    /// In-place analytic signal: <paramref name="re"/> stays the (padded) real part,
    /// <paramref name="im"/> receives the Hilbert transform. Lengths must match; pads to
    /// the next power of two internally when needed.
    /// </summary>
    public static void AnalyticSignalInPlace(T[] re, T[] im, ComputingContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(re);
        ArgumentNullException.ThrowIfNull(im);
        if (re.Length != im.Length)
            throw new ArgumentException("Real and imaginary buffers must have the same length.");

        int n = re.Length;
        Array.Clear(im);
        if (n == 0) return;

        int fftSize = NextPow2(n);
        var pr = new T[fftSize];
        var pi = new T[fftSize];
        re.AsSpan().CopyTo(pr);

        var fft = new Fft<T>(fftSize);
        fft.Direct(pr, pi, context);

        T two = T.CreateChecked(2);
        for (int i = 1; i < fftSize / 2; i++)
        {
            pr[i] *= two;
            pi[i] *= two;
        }
        for (int i = fftSize / 2 + 1; i < fftSize; i++)
        {
            pr[i] = T.Zero;
            pi[i] = T.Zero;
        }

        fft.InverseNorm(pr, pi, context);
        pr.AsSpan(0, n).CopyTo(re);
        pi.AsSpan(0, n).CopyTo(im);
    }

    /// <summary>Fast Hilbert transform of <paramref name="input"/> into <paramref name="output"/> (imaginary part).</summary>
    public void Direct(ReadOnlySpan<T> input, T[] output, ComputingContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(output);
        Array.Clear(_re);
        Array.Clear(output);
        input.Slice(0, Math.Min(input.Length, Size)).CopyTo(_re);
        _re.AsSpan().CopyTo(_im); // placeholder; overwritten by FFT
        Array.Clear(_im);

        _fft.Direct(_re, _im, context);

        T two = T.CreateChecked(2);
        for (int i = 1; i < Size / 2; i++)
        {
            _re[i] *= two;
            _im[i] *= two;
        }
        for (int i = Size / 2 + 1; i < Size; i++)
        {
            _re[i] = T.Zero;
            _im[i] = T.Zero;
        }

        _fft.Inverse(_re, _im, context);
        int n = Math.Min(Size, output.Length);
        _im.AsSpan(0, n).CopyTo(output);
    }

    /// <summary>Analytic-signal magnitudes without allocating a complex signal.</summary>
    public void AnalyticMagnitude(ReadOnlySpan<T> input, Span<T> magnitude, ComputingContext? context = null)
    {
        var imag = new T[Size];
        Direct(input, imag, context);
        T inv = T.One / T.CreateChecked(Size);
        int n = Math.Min(Size, magnitude.Length);
        for (int i = 0; i < n; i++)
        {
            T re = _re[i] * inv;
            T im = imag[i] * inv;
            magnitude[i] = T.Sqrt(re * re + im * im);
        }
    }

    private static int NextPow2(int n)
    {
        if (n <= 1) return 1;
        int p = 1;
        while (p < n) p <<= 1;
        return p;
    }
}
