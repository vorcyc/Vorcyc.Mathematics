using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;


/// <summary>
///   Sokal-Michener dissimilarity.
/// </summary>
public class SokalMichener<TSelf>
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
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Distance(TSelf[] x, TSelf[] y)
    {
        TSelf tf = TSelf.Zero;
        TSelf ft = TSelf.Zero;
        TSelf tt = TSelf.Zero;
        TSelf ff = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] == TSelf.One && y[i] == TSelf.Zero) tf++;
            if (x[i] == TSelf.Zero && y[i] == TSelf.One) ft++;
            if (x[i] == TSelf.Zero && y[i] == TSelf.Zero) tt++;
            if (x[i] == TSelf.Zero && y[i] == TSelf.Zero) ff++;
        }

        TSelf r = TSelf.CreateTruncating(2) * (tf + ft);
        return r / (ff + tt + r);
    }



}
