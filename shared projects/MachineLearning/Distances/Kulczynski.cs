using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;


/// <summary>
///   Kulczynski dissimilarity.
/// </summary>
public class Kulczynski
#if NET7_0_OR_GREATER
    <TSelf>
    : IDistance<TSelf>
    where TSelf : INumber<TSelf>, IRootFunctions<TSelf>
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
    public double Distance(int[] x, int[] y)
    {
        int tf = 0;
        int ft = 0;
        int tt = 0;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != 0 && y[i] == 0) tf++;
            if (x[i] == 0 && y[i] != 0) ft++;
            if (x[i] != 0 && y[i] != 0) tt++;
        }

        double num = tf + ft - tt + x.Length;
        double den = ft + tf + x.Length;
        return num / den;
    }


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
        // TODO: Rewrite the integer dissimilarities (Yule, Russel-Rao,...)
        // using generics
        TSelf tf = TSelf.Zero;
        TSelf ft = TSelf.Zero;
        TSelf tt = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != TSelf.Zero && y[i] == TSelf.Zero) tf++;
            if (x[i] == TSelf.Zero && y[i] != TSelf.Zero) ft++;
            if (x[i] != TSelf.Zero && y[i] != TSelf.Zero) tt++;
        }

        TSelf num = tf + ft - tt + TSelf.CreateChecked(x.Length);
        TSelf den = ft + tf + TSelf.CreateChecked(x.Length);
        return num / den;
    }

}
