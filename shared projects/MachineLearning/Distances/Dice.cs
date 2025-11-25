using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   Dice dissimilarity.
/// </summary>
public class Dice
#if NET7_0_OR_GREATER
    <TSelf>
    : IDistance<TSelf>
    where TSelf : INumber<TSelf>
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
    ///   A <see cref="INumber{TSelf}"/> value representing the distance <c>d(x,y)</c>
    ///   between <paramref name="x"/> and <paramref name="y"/> according 
    ///   to the distance function implemented by this class.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Distance(TSelf[] x, TSelf[] y)
    {
        TSelf tf = TSelf.Zero;
        TSelf ft = TSelf.Zero;
        TSelf tt = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != TSelf.Zero && y[i] == TSelf.Zero) tf++;
            if (x[i] == TSelf.Zero && y[i] != TSelf.Zero) ft++;
            if (x[i] != TSelf.Zero && y[i] != TSelf.Zero) tt++;
        }

        return (tf + ft) / (TSelf.CreateChecked(2) * tt + ft + tf);
    }
}
