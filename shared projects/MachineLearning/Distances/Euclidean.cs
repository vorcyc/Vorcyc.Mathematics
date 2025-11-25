using System.Numerics;
using System.Runtime.CompilerServices;
using static System.MathF;

namespace Vorcyc.Mathematics.MachineLearning.Distances;


/// <summary>
///   Euclidean distance metric.
/// </summary>
public class Euclidean
#if NET7_0_OR_GREATER
<TSelf>
: IDistance<TSelf>, ISimilarity<TSelf>
where TSelf : INumber<TSelf>, IRootFunctions<TSelf>
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
    ///   A <see cref="INumber{TSelf}"/> value representing the distance <c>d(x,y)</c>
    ///   between <paramref name="x"/> and <paramref name="y"/> according 
    ///   to the distance function implemented by this class.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Distance(TSelf x, TSelf y)
    {
        return TSelf.Abs(x - y);
    }


    /// <summary>
    ///   Gets the Euclidean distance between two points. Note: this function 
    ///   is dangerous as it is too easy to invert its arguments by mistake. 
    ///   Please consider using the Tuple&lt;double, double> overload instead.
    /// </summary>
    /// 
    /// <param name="vector1x">The first coordinate of first point in space.</param>
    /// <param name="vector1y">The second coordinate of first point in space.</param>
    /// <param name="vector2x">The first coordinate of second point in space.</param>
    /// <param name="vector2y">The second coordinate of second point in space.</param>
    /// 
    /// <returns>The Euclidean distance between x and y.</returns>
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Distance(TSelf vector1x, TSelf vector1y, TSelf vector2x, TSelf vector2y)
    {
        TSelf dx = vector1x - vector2x;
        TSelf dy = vector1y - vector2y;
        return TSelf.Sqrt(dx * dx + dy * dy);
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
    public static TSelf Distance(TSelf[] x, TSelf[] y)
    {
        TSelf sum = TSelf.Zero;
        for (int i = 0; i < x.Length; i++)
        {
            TSelf u = x[i] - y[i];
            sum += u * u;
        }
        return TSelf.Sqrt(sum);
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
    public static TSelf Similarity(TSelf x, TSelf y)
    {
        return TSelf.One / (TSelf.One + TSelf.Abs(x - y));
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
        TSelf sum = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            TSelf u = x[i] - y[i];
            sum += u * u;
        }

        return TSelf.One / (TSelf.One * TSelf.Sqrt(sum));
    }


#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(float x, float y)
    {
        return Abs(x - y);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(float[] x, float[] y)
    {
        float sum = 0.0f;
        for (int i = 0; i < x.Length; i++)
        {
            float u = x[i] - y[i];
            sum += u * u;
        }
        return Sqrt(sum);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(float vector1x, float vector1y, float vector2x, float vector2y)
    {
        float dx = vector1x - vector2x;
        float dy = vector1y - vector2y;
        return Sqrt(dx * dx + dy * dy);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Similarity(float x, float y)
    {
        return 1.0f / (1.0f + Abs(x - y));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    /// <remarks>
    /// The length of x must be equals to length of y.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Similarity(float[] x, float[] y)
    {
        float sum = 0.0f;

        for (int i = 0; i < x.Length; i++)
        {
            float u = x[i] - y[i];
            sum += u * u;
        }

        return 1.0f / (1.0f + Sqrt(sum));
    }


}
