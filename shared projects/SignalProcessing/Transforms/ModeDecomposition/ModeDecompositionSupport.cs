using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

internal static class ModeDecompositionSupport
{
    public static void EnsureFloatOrDouble<T>()
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Only float and double are supported.");
    }

    public static int NextPow2(int n)
    {
        if (n <= 1) return 1;
        int p = 1;
        while (p < n) p <<= 1;
        return p;
    }

    public static void FftShiftInPlace<T>(T[] re, T[] im)
        where T : unmanaged
    {
        int n = re.Length;
        int half = n / 2;
        for (int i = 0; i < half; i++)
        {
            int j = i + half;
            (re[i], re[j]) = (re[j], re[i]);
            (im[i], im[j]) = (im[j], im[i]);
        }
    }
}
