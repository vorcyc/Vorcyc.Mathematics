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
        => Statistics.Correlation.PearsonCorrelation<TSelf>(x, y);

}
