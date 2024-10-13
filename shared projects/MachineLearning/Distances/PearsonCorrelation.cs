using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   Pearson Correlation similarity.
/// </summary>
public class PearsonCorrelation<TSelf>
    : ISimilarity<TSelf>
    where TSelf : INumber<TSelf>, IRootFunctions<TSelf>
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
    public static TSelf Similarity(TSelf[] x, TSelf[] y)
    {
        TSelf p = TSelf.Zero;
        TSelf q = TSelf.Zero;
        TSelf p2 = TSelf.Zero;
        TSelf q2 = TSelf.Zero;
        TSelf sum = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            p += x[i];
            q += y[i];
            p2 += x[i] * x[i];
            q2 += y[i] * y[i];
            sum += x[i] * y[i];
        }

        TSelf n = TSelf.CreateTruncating(x.Length);
        TSelf num = sum - (p * q) / n;
        TSelf den = TSelf.Sqrt((p2 - (p * p) / n) * (q2 - (q * q) / n));

        return (den == TSelf.Zero) ? TSelf.Zero : num / den;
    }

}
