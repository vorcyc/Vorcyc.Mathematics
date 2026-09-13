using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

/// <summary>
/// Generic real-input FFT facade: <see cref="float"/> dispatches to <see cref="RealFft"/>,
/// <see cref="double"/> to <see cref="RealFft64"/>.
/// </summary>
/// <typeparam name="T">IEEE-754 sample type (<see cref="float"/> or <see cref="double"/>).</typeparam>
public sealed class RealFft<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{
    private readonly RealFft? _fft32;
    private readonly RealFft64? _fft64;

    /// <summary>Gets the real-input transform length (power of two).</summary>
    public int Size { get; }

    /// <summary>Constructs a real FFT of the given power-of-two <paramref name="size"/>.</summary>
    public RealFft(int size)
    {
        Size = size;
        if (typeof(T) == typeof(float))
            _fft32 = new RealFft(size);
        else if (typeof(T) == typeof(double))
            _fft64 = new RealFft64(size);
        else
            throw new NotSupportedException("RealFft<T> supports float and double only.");
    }

    /// <summary>Forward: real <paramref name="input"/> → complex (<paramref name="re"/>, <paramref name="im"/>).</summary>
    public void Direct(T[] input, T[] re, T[] im, ComputingContext? context = null)
    {
        if (input is float[] inf && re is float[] rf && im is float[] ri)
        {
            _fft32!.Direct(inf, rf, ri, context);
            return;
        }

        if (input is double[] ind && re is double[] rd && im is double[] id)
        {
            _fft64!.Direct(ind, rd, id, context);
            return;
        }

        throw new NotSupportedException("RealFft<T> supports float and double only.");
    }

    /// <summary>Inverse: complex (<paramref name="re"/>, <paramref name="im"/>) → real <paramref name="output"/>.</summary>
    public void Inverse(T[] re, T[] im, T[] output, ComputingContext? context = null)
    {
        if (re is float[] rf && im is float[] ri && output is float[] of)
        {
            _fft32!.Inverse(rf, ri, of, context);
            return;
        }

        if (re is double[] rd && im is double[] id && output is double[] od)
        {
            _fft64!.Inverse(rd, id, od, context);
            return;
        }

        throw new NotSupportedException("RealFft<T> supports float and double only.");
    }

    /// <summary>Normalized inverse FFT.</summary>
    public void InverseNorm(T[] re, T[] im, T[] output, ComputingContext? context = null)
    {
        if (re is float[] rf && im is float[] ri && output is float[] of)
        {
            _fft32!.InverseNorm(rf, ri, of, context);
            return;
        }

        if (re is double[] rd && im is double[] id && output is double[] od)
        {
            _fft64!.InverseNorm(rd, id, od, context);
            return;
        }

        throw new NotSupportedException("RealFft<T> supports float and double only.");
    }
}
