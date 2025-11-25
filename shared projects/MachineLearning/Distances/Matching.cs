using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   Matching dissimilarity.
/// </summary>
public class Matching
#if NET7_0_OR_GREATER
    <TSelf>
    : IDistance<TSelf>
    where TSelf : INumber<TSelf>
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
    public static float Distance(float[] x, float[] y)
    {
        int tf = 0;
        int ft = 0;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] == 1 && y[i] == 0) tf++;
            if (x[i] == 0 && y[i] == 1) ft++;
        }

        return (tf + ft) / (float)(x.Length);
    }

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
        TSelf tf = TSelf.Zero;
        TSelf ft = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] == TSelf.One && y[i] == TSelf.Zero) tf++;
            if (x[i] == TSelf.Zero && y[i] == TSelf.One) ft++;
        }


        return (tf + ft) / (TSelf)Convert.ChangeType(x.Length, typeof(TSelf));
    }

#endif
}
