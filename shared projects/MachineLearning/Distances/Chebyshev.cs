using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   Chebyshev distance.
/// </summary>
public class Chebyshev
#if NET7_0_OR_GREATER
    <TSelf>
    : IDistance<TSelf>
    where TSelf : INumber<TSelf>
#else
    : IDistance<float[]>
#endif
{

    /// <summary>
    ///   Computes the distance <c>d(x,y)</c> between points
    ///   <paramref name="x"/> and <paramref name="y"/>.
    /// </summary>
    /// 
    /// <param name="x">The first point <c>x</c>.</param>
    /// <param name="y">The second point <c>y</c>.</param>
    /// 
    /// <returns>
    ///   A <see cref="INumber{TSelf}"/> value representing the distance <c>d(x,y)</c>
    ///   between <paramref name="x"/> and <paramref name="y"/> according 
    ///   to the distance function implemented by this class.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Distance(TSelf[] x, TSelf[] y)
    {
        TSelf max = TSelf.Abs(x[0] - y[0]);

        for (int i = 1; i < x.Length; i++)
        {
            TSelf abs = TSelf.Abs(x[i] - y[i]);
            if (abs > max)
                max = abs;
        }

        return max;
    }
}