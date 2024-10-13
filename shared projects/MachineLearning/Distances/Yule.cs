using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

public class Yule
#if NET7_0_OR_GREATER
    <TSelf>
    : IDistance<TSelf>
    where TSelf : INumber<TSelf>
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
    ///   A generic type value representing the distance <c>d(x,y)</c>
    ///   between <paramref name="x"/> and <paramref name="y"/> according 
    ///   to the distance function implemented by this class.
    /// </returns>
    public static TSelf Distance(TSelf[] x, TSelf[] y)
    {
        TSelf tf = TSelf.Zero;
        TSelf ft = TSelf.Zero;
        TSelf tt = TSelf.Zero;
        TSelf ff = TSelf.Zero;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != TSelf.Zero && y[i] == TSelf.Zero) tf++;
            if (x[i] == TSelf.Zero && y[i] != TSelf.Zero) ft++;
            if (x[i] != TSelf.Zero && y[i] != TSelf.Zero) tt++;
            if (x[i] == TSelf.Zero && y[i] == TSelf.Zero) ff++;
        }

        var two = TSelf.One + TSelf.One;

        TSelf r = two * (tf + ft);
        return r / (tt + ff * r / two);
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
    public static double Distance(int[] x, int[] y)
    {
        int tf = 0;
        int ft = 0;
        int tt = 0;
        int ff = 0;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] == 1 && y[i] == 0) tf++;
            if (x[i] == 0 && y[i] == 1) ft++;
            if (x[i] == 1 && y[i] == 1) tt++;
            if (x[i] == 0 && y[i] == 0) ff++;
        }

        double r = 2 * (tf + ft);
        return r / (tt + ff + r / 2);
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
    ///   A single-precision value representing the distance <c>d(x,y)</c>
    ///   between <paramref name="x"/> and <paramref name="y"/> according 
    ///   to the distance function implemented by this class.
    /// </returns>
    public static float Distance(float[] x, float[] y)
    {
        int tf = 0;
        int ft = 0;
        int tt = 0;
        int ff = 0;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != 0 && y[i] == 0) tf++;
            if (x[i] == 0 && y[i] != 0) ft++;
            if (x[i] != 0 && y[i] != 0) tt++;
            if (x[i] == 0 && y[i] == 0) ff++;
        }

        float r = 2.0f * (tf + ft);
        return r / (tt + ff + r / 2.0f);
    }

}
