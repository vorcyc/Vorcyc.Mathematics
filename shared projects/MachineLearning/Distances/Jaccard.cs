using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   Jaccard (Index) distance.
/// </summary>
/// 
/// <remarks>
/// <para>
///   The Jaccard index, also known as the Jaccard similarity coefficient (originally
///   coined coefficient de communauté by Paul Jaccard), is a statistic used for comparing
///   the similarity and diversity of sample sets. The Jaccard coefficient measures 
///   similarity between finite sample sets, and is defined as the size of the intersection
///   divided by the size of the union of the sample sets.</para>
///   
/// <para>
///   References:
///   <list type="bullet">
///     <item><description><a href="https://en.wikipedia.org/wiki/Jaccard_index">
///       https://en.wikipedia.org/wiki/Jaccard_index </a></description></item>
///   </list></para>  
/// </remarks>
public class Jaccard
#if NET7_0_OR_GREATER
    <TSelf>
    : IDistance<TSelf>, ISimilarity<TSelf>
    where TSelf : INumber<TSelf>, IRootFunctions<TSelf>
#endif
{


    ///// <summary>
    ///// Jaccard index aka Jaccard simlarity coefficient.
    ///// It’s a measure of similarity for the two sets of data, with a range from 0% to 100%.
    ///// The higher the percentage, the more similar the two populations. 
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="A"></param>
    ///// <param name="B"></param>
    ///// <returns></returns>
    //public static double Similarity<T>(IEnumerable<T> A, IEnumerable<T> B)
    //{
    //    var t1 = (double)A.Intersect(B).Count();
    //    var t2 = (double)A.Union(B).Count();
    //    return t1 / t2;
    //}

    ///// <summary>
    ///// 1 - Index(A,B)
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="A"></param>
    ///// <param name="B"></param>
    ///// <returns></returns>
    //public static double Distance<T>(IEnumerable<T> A, IEnumerable<T> B)
    //{
    //    return 1.0 - Similarity(A, B);
    //}

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
        TSelf inter = TSelf.Zero;
        TSelf union = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != TSelf.Zero || y[i] != TSelf.Zero)
            {
                if (x[i] == y[i])
                    inter++;
                union++;
            }
        }

        return (union == TSelf.Zero) ? TSelf.Zero : TSelf.One - (inter / union);
    }


    /// <summary>
    ///   Gets a similarity measure between two points.
    /// </summary>
    /// 
    /// <param name="x">The first point to be compared.</param>
    /// <param name="y">The second point to be compared.</param>
    /// 
    /// <returns>A similarity measure between x and y.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Similarity(TSelf[] x, TSelf[] y)
    {
        TSelf inter = TSelf.Zero;
        TSelf union = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != TSelf.Zero || y[i] != TSelf.Zero)
            {
                if (x[i] == y[i])
                    inter++;
                union++;
            }
        }

        return (inter == TSelf.Zero) ? TSelf.Zero : inter / union;
    }


#endif

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
        int inter = 0;
        int union = 0;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != 0 || y[i] != 0)
            {
                if (x[i] == y[i])
                    inter++;
                union++;
            }
        }

        return (union == 0) ? 0f : 1.0f - (inter / (float)union);
    }


    /// <summary>
    ///   Gets a similarity measure between two points.
    /// </summary>
    /// 
    /// <param name="x">The first point to be compared.</param>
    /// <param name="y">The second point to be compared.</param>
    /// 
    /// <returns>A similarity measure between x and y.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Similarity(float[] x, float[] y)
    {
        int inter = 0;
        float union = 0;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != 0 || y[i] != 0)
            {
                if (x[i] == y[i])
                    inter++;
                union++;
            }
        }

        return (inter == 0) ? 0f : inter / (float)union;
    }


}
