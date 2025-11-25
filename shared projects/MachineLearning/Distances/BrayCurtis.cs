using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   Bray-Curtis distance.
/// </summary>
public class BrayCurtis
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
    ///   A double-precision value representing the distance <c>d(x,y)</c>
    ///   between <paramref name="x"/> and <paramref name="y"/> according 
    ///   to the distance function implemented by this class.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Distance(TSelf[] x, TSelf[] y)
    {
        TSelf sumP = TSelf.Zero;
        TSelf sumN = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            sumN += TSelf.Abs(x[i] - y[i]);
            sumP += TSelf.Abs(x[i] + y[i]);
        }

        return sumN / sumP;
    }
}
