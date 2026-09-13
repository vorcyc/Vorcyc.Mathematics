using System.Numerics;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// Shared helpers for generic filter kernels and FDA (float / double only at call sites).
/// </summary>
internal static class FilterGenericSupport
{
    public static void EnsureFloatOrDouble<T>()
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Generic filters support float and double only.");
    }

    public static void GuardNormFreq<T>(T frequency, string name)
        where T : IFloatingPointIeee754<T>
        => Guard.AgainstInvalidRange(double.CreateChecked(frequency), 0, 0.5, name);

    public static void GuardBand<T>(T frequencyLow, T frequencyHigh)
        where T : IFloatingPointIeee754<T>
    {
        GuardNormFreq(frequencyLow, "lower frequency");
        GuardNormFreq(frequencyHigh, "upper frequency");
        Guard.AgainstInvalidRange(double.CreateChecked(frequencyLow), double.CreateChecked(frequencyHigh),
            "lower frequency", "upper frequency");
    }

    public static T[] PadTo<T>(T[] source, int size)
    {
        var dst = new T[size];
        Array.Copy(source, dst, Math.Min(source.Length, size));
        return dst;
    }

    public static T[] Clone<T>(T[] source)
    {
        var dst = new T[source.Length];
        source.AsSpan().CopyTo(dst);
        return dst;
    }

    public static T[] Convolve<T>(T[] a, T[] b)
        where T : IFloatingPointIeee754<T>
    {
        var c = new T[a.Length + b.Length - 1];
        for (int i = 0; i < a.Length; i++)
        {
            for (int j = 0; j < b.Length; j++)
                c[i + j] += a[i] * b[j];
        }
        return c;
    }

    public static T[] CrossCorrelate<T>(T[] a, T[] b)
        where T : IFloatingPointIeee754<T>
    {
        var rev = new T[b.Length];
        for (int i = 0; i < b.Length; i++)
            rev[i] = b[b.Length - 1 - i];
        return Convolve(a, rev);
    }

    public static T[] Reverse<T>(T[] a)
    {
        var r = new T[a.Length];
        for (int i = 0; i < a.Length; i++)
            r[i] = a[a.Length - 1 - i];
        return r;
    }

    public static T Sum<T>(T[] a)
        where T : IFloatingPointIeee754<T>
    {
        var s = T.Zero;
        for (int i = 0; i < a.Length; i++)
            s += a[i];
        return s;
    }

    public static T[] Widen<T>(float[] src)
        where T : IFloatingPointIeee754<T>
    {
        var dst = new T[src.Length];
        for (int i = 0; i < src.Length; i++)
            dst[i] = T.CreateChecked(src[i]);
        return dst;
    }

    public static float[] Narrow<T>(T[] src)
        where T : IFloatingPointIeee754<T>
    {
        var dst = new float[src.Length];
        for (int i = 0; i < src.Length; i++)
            dst[i] = float.CreateChecked(src[i]);
        return dst;
    }

    public static void ApplyWindow<T>(T[] kernel, WindowType window)
        where T : IFloatingPointIeee754<T>
    {
        var w = WindowBuilder.OfType(window, kernel.Length);
        for (int i = 0; i < kernel.Length; i++)
            kernel[i] *= T.CreateChecked(w[i]);
    }

    public static void SplitReIm<T>(Complex<T>[] z, out T[] re, out T[] im)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        re = new T[z.Length];
        im = new T[z.Length];
        for (int i = 0; i < z.Length; i++)
        {
            re[i] = z[i].Real;
            im[i] = z[i].Imaginary;
        }
    }
}
