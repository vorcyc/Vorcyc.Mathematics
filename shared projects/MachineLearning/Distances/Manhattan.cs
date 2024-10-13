using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   Manhattan (also known as Taxicab or L1) distance.
/// </summary>
/// 
/// <remarks>
/// <para>
///   Taxicab geometry, considered by Hermann Minkowski in 19th century Germany, 
///   is a form of geometry in which the usual distance function of metric or
///   Euclidean geometry is replaced by a new metric in which the distance between 
///   two points is the sum of the absolute differences of their Cartesian 
///   coordinates. The taxicab metric is also known as rectilinear distance, L1 
///   distance or L1 norm (see Lp space), city block distance, Manhattan distance,
///   or Manhattan length, with corresponding variations in the name of the geometry.
///   The latter names allude to the grid layout of most streets on the island of 
///   Manhattan, which causes the shortest path a car could take between two intersections
///   in the borough to have length equal to the intersections' distance in taxicab
///   geometry.</para>
///   
/// <para>
///   References:
///   <list type="bullet">
///     <item><description><a href="https://en.wikipedia.org/wiki/Taxicab_geometry">
///       https://en.wikipedia.org/wiki/Taxicab_geometry </a></description></item>
///   </list></para>  
/// </remarks>
public class Manhattan
#if NET7_0_OR_GREATER
<TSelf>
: IDistance<TSelf>
where TSelf : System.Numerics.INumber<TSelf>
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
        float sum = 0.0f;
        for (int i = 0; i < x.Length; i++)
            sum += System.Math.Abs(x[i] - y[i]);
        return sum;
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
        TSelf sum = TSelf.Zero;
        for (int i = 0; i < x.Length; i++)
            sum += TSelf.Abs(x[i] - y[i]);
        return sum;
    }

#endif
}
