using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   Russel-Rao dissimilarity.
/// </summary>
public class RusselRao<TSelf>
    : IDistance<TSelf>
    where TSelf : INumber<TSelf>
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
        int tt = 0;
        for (int i = 0; i < x.Length; i++)
            if (x[i] != TSelf.Zero && y[i] != TSelf.Zero) tt++;

        return TSelf.CreateChecked((x.Length - tt) / (double)x.Length);
    }


}
