using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
/// Angular distance, or the proper distance metric version of <see cref="Cosine{TSelf}" /> distance.
/// </summary>
public class Angular
#if NET7_0_OR_GREATER
    <TSelf>
    : IDistance<TSelf>, ISimilarity<TSelf>
    where TSelf : INumber<TSelf>, IRootFunctions<TSelf>, ITrigonometricFunctions<TSelf>
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
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Distance(TSelf[] x, TSelf[] y)
    {
        TSelf num = TSelf.Zero;
        TSelf p = TSelf.Zero;
        TSelf q = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            num += x[i] * y[i];
            p += x[i] * x[i];
            q += y[i] * y[i];
        }

        TSelf den = TSelf.Sqrt(p) * TSelf.Sqrt(q);
        TSelf similarity = num == TSelf.Zero ? TSelf.One : TSelf.One - (num / den);

        return TSelf.Acos(similarity);
    }



    /// <summary>
    ///   Gets a similarity measure between two points.
    /// </summary>
    /// 
    /// <param name="x">The first point to be compared.</param>
    /// <param name="y">The second point to be compared.</param>
    /// 
    /// <returns>A similarity measure between x and y.</returns>
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Similarity(TSelf[] x, TSelf[] y)
    {
        TSelf num = TSelf.Zero;
        TSelf p = TSelf.Zero;
        TSelf q = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            num += x[i] * y[i];
            p += x[i] * x[i];
            q += y[i] * y[i];
        }

        TSelf den = TSelf.Sqrt(p) * TSelf.Sqrt(q);
        TSelf similarity = num == TSelf.Zero ? TSelf.One : TSelf.One - (num / den);

        return TSelf.One - TSelf.Acos(similarity);
    }
}
