using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;


/// <summary>
///   Herlinger distance.
/// </summary>
/// 
/// <remarks>
/// <para>
///   In probability and statistics, the Hellinger distance (also called 
///   Bhattacharyya distance as this was originally introduced by Anil Kumar
///   Bhattacharya) is used to quantify the similarity between two probability
///   distributions. It is a type of f-divergence. The Hellinger distance is 
///   defined in terms of the Hellinger integral, which was introduced by Ernst
///   Hellinger in 1909.</para>
///   
/// <para>
///   References:
///   <list type="bullet">
///     <item><description><a href="https://en.wikipedia.org/wiki/Hellinger_distance">
///       https://en.wikipedia.org/wiki/Hellinger_distance </a></description></item>
///   </list></para>  
/// </remarks>
/// 
public class Hellinger
#if NET7_0_OR_GREATER
    <TSelf>
    : IDistance<TSelf>
    where TSelf : INumber<TSelf>, IRootFunctions<TSelf>, IPowerFunctions<TSelf>
#else

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
        TSelf sum = TSelf.Zero;
        for (int i = 0; i < x.Length; i++)
            sum += TSelf.Pow(TSelf.Sqrt(x[i]) - TSelf.Sqrt(y[i]), TSelf.CreateChecked(2));

        return sum / TSelf.Sqrt(TSelf.CreateChecked(2));
    }
}
