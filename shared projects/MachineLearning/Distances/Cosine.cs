using System.Numerics;
using System.Runtime.CompilerServices;
using static System.MathF;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   Cosine distance. For a proper distance metric, see <see cref="Angular{TSelf}"/>.
/// </summary>
public class Cosine
#if NET7_0_OR_GREATER
    <TSelf>
    : IDistance<TSelf>, ISimilarity<TSelf>
    where TSelf : INumber<TSelf>, IRootFunctions<TSelf>
#else
    //: IDistance<float[]>
#endif
{

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
        TSelf p = TSelf.Zero;
        TSelf q = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            sum += x[i] * y[i];
            p += x[i] * x[i];
            q += y[i] * y[i];
        }

        TSelf den = TSelf.Sqrt(p) * TSelf.Sqrt(q);
        return sum == TSelf.Zero ? TSelf.One : TSelf.One - (sum / den);
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
        TSelf sum = TSelf.Zero;
        TSelf p = TSelf.Zero;
        TSelf q = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            sum += x[i] * y[i];
            p += x[i] * x[i];
            q += y[i] * y[i];
        }

        TSelf den = TSelf.Sqrt(p) * TSelf.Sqrt(q);
        return (sum == TSelf.Zero) ? TSelf.Zero : sum / den;
    }


#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(float[] x, float[] y)
    {
        float sum = 0.0f;
        float p = 0.0f;
        float q = 0.0f;

        for (int i = 0; i < x.Length; i++)
        {
            sum += x[i] * y[i];
            p += x[i] * x[i];
            q += y[i] * y[i];
        }

        float den = Sqrt(p) * Sqrt(q);
        return sum == 0.0f ? 1.0f : 1.0f - (sum / den);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Similarity(float[] x, float[] y)
    {
        float sum = 0;
        float p = 0;
        float q = 0;

        for (int i = 0; i < x.Length; i++)
        {
            sum += x[i] * y[i];
            p += x[i] * x[i];
            q += y[i] * y[i];
        }

        float den = Sqrt(p) * Sqrt(q);
        return (sum == 0.0f) ? 0.0f : sum / den;
    }


}
