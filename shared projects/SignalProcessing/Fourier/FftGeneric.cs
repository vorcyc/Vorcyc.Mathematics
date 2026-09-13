using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

/// <summary>
/// Generic complex FFT facade: <see cref="float"/> dispatches to <see cref="Fft"/>,
/// <see cref="double"/> to <see cref="Fft64"/>. Other <typeparamref name="T"/> types are rejected.
/// </summary>
/// <typeparam name="T">IEEE-754 sample type (<see cref="float"/> or <see cref="double"/>).</typeparam>
public sealed class Fft<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{
    private readonly Fft? _fft32;
    private readonly Fft64? _fft64;

    /// <summary>Gets the transform length (power of two).</summary>
    public int Size { get; }

    /// <summary>Constructs an FFT of the given power-of-two <paramref name="fftSize"/>.</summary>
    public Fft(int fftSize = 512)
    {
        Size = fftSize;
        if (typeof(T) == typeof(float))
            _fft32 = new Fft(fftSize);
        else if (typeof(T) == typeof(double))
            _fft64 = new Fft64(fftSize);
        else
            throw new NotSupportedException("Fft<T> supports float and double only.");
    }

    /// <summary>Forward FFT in place. Honors <paramref name="context"/> when provided.</summary>
    public void Direct(T[] re, T[] im, ComputingContext? context = null)
    {
        if (re is float[] rf && im is float[] ri)
        {
            if (context is null) _fft32!.Direct(rf, ri);
            else _fft32!.Direct(rf, ri, context);
            return;
        }

        if (re is double[] rd && im is double[] id)
        {
            if (context is null) _fft64!.Direct(rd, id);
            else _fft64!.Direct(rd, id, context);
            return;
        }

        throw new NotSupportedException("Fft<T> supports float and double only.");
    }

    /// <summary>Inverse FFT in place. Honors <paramref name="context"/> when provided.</summary>
    public void Inverse(T[] re, T[] im, ComputingContext? context = null)
    {
        if (re is float[] rf && im is float[] ri)
        {
            if (context is null) _fft32!.Inverse(rf, ri);
            else _fft32!.Inverse(rf, ri, context);
            return;
        }

        if (re is double[] rd && im is double[] id)
        {
            if (context is null) _fft64!.Inverse(rd, id);
            else _fft64!.Inverse(rd, id, context);
            return;
        }

        throw new NotSupportedException("Fft<T> supports float and double only.");
    }

    /// <summary>Normalized inverse FFT in place (÷ N). Honors <paramref name="context"/> when provided.</summary>
    public void InverseNorm(T[] re, T[] im, ComputingContext? context = null)
    {
        if (re is float[] rf && im is float[] ri)
        {
            if (context is null) _fft32!.InverseNorm(rf, ri);
            else _fft32!.InverseNorm(rf, ri, context);
            return;
        }

        if (re is double[] rd && im is double[] id)
        {
            if (context is null) _fft64!.InverseNorm(rd, id);
            else _fft64!.InverseNorm(rd, id, context);
            return;
        }

        throw new NotSupportedException("Fft<T> supports float and double only.");
    }
}
