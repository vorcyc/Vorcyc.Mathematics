using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   Hamming distance.
/// </summary>
public class Hamming
#if NET7_0_OR_GREATER
<TSelf>
: IDistance<TSelf>
where TSelf : INumber<TSelf>
#endif
{
    public static bool ToBool(int x) => x != 0;

#if NET7_0_OR_GREATER

    /// <summary>
    ///   Computes the distance <c>d(x,y)</c> between points
    ///   <paramref name="x"/> and <paramref name="y"/>.
    /// </summary>
    /// 
    /// <param name="x">The first point <c>x</c>.</param>
    /// <param name="y">The second point <c>y</c>.</param>
    /// 
    /// <returns>
    ///   A double-precision value representing the distance <c>d(x,y)</c>
    ///   between <paramref name="x"/> and <paramref name="y"/> according 
    ///   to the distance function implemented by this class.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Distance(TSelf[] x, TSelf[] y)
    {
        TSelf sum = TSelf.Zero;
        for (int i = 0; i < x.Length; i++)
            if (x[i] != y[i])
                sum++;
        return sum;
    }


#endif


    /// <summary>
    /// Computes the hamming distance of two signed 32-bit intergers.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int Distance(int x, int y)
    {
        int dist = 0;
        var val = x ^ y;

        // Count the number of bits set
        while (val != 0)
        {
            // A bit is set, so increment the count and clear the bit
            dist++;
            val &= val - 1;
        }

        // Return the number of differing bits
        return dist;
    }

    /// <summary>
    /// Computes the hamming distance of two unsigned 32-bit intergers.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int Distance(uint x, uint y)
    {
        int dist = 0;
        uint val = x ^ y;

        // Count the number of bits set
        while (val != 0)
        {
            // A bit is set, so increment the count and clear the bit
            dist++;
            val &= val - 1;
        }

        // Return the number of differing bits
        return dist;
    }

    /// <summary>
    /// Computes the hamming distance of two signed 64-bit intergers.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int Distance(long x, long y)
    {
        int dist = 0;
        var val = x ^ y;

        // Count the number of bits set
        while (val != 0)
        {
            // A bit is set, so increment the count and clear the bit
            dist++;
            val &= val - 1;
        }

        // Return the number of differing bits
        return dist;
    }

    /// <summary>
    /// Computes the hamming distance of two unsigned 64-bit intergers.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int Distance(ulong x, ulong y)
    {
        int dist = 0;
        var val = x ^ y;

        // Count the number of bits set
        while (val != 0)
        {
            // A bit is set, so increment the count and clear the bit
            dist++;
            val &= val - 1;
        }

        // Return the number of differing bits
        return dist;
    }



    /// <summary>
    /// Computes the hamming distance of two strings.
    /// </summary>
    /// <remarks>
    /// The length of x and y must be equal.
    /// </remarks>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int Distance(string x, string y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("Undefined for sequences of unequal length");

        int dist = 0;
        var i = x.Length;
        //while (i-- > 0)
        while (ToBool(i--))
        {
            if (x[i] != y[i])
            {
                dist++;
            }
        }

        return dist;
    }





    public static int Distance(byte[] a, byte[] b)
    {
        int distance = 0;
        for (int i = 0, n = a.Length; i < n; i++)
        {
            if (a[i] != b[i])
            {
                distance++;
            }
        }

        return distance;
    }



    public static int Similarity(byte[] a, byte[] b)
    {
        return a.Length - Distance(a, b);
    }



    public static int Similarity(int[] expected, int[] actual, int setBytesPerLong)
    {
        int mask = 0xFF;
        int sameBytes = 0;

        for (int i = 0; i < expected.Length; ++i)
        {
            long a = expected[i];
            long b = actual[i];

            for (int j = 0; j < setBytesPerLong; ++j)
            {
                if ((a & mask) == (b & mask))
                {
                    sameBytes++;
                }

                a >>= 8;
                b >>= 8;
            }
        }

        return sameBytes;
    }


}
