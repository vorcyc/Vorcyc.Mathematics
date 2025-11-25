using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

/// <summary>
///   The Minkowski distance is a metric in a normed vector space which can be 
///   considered as a generalization of both the <see cref="Euclidean">Euclidean 
///   distance</see> and the <see cref="Manhattan">Manhattan distance</see>.
/// </summary>
/// 
/// <remarks>
/// <para>
///   The framework distinguishes between metrics and distances by using different
///   types for them. This makes it possible to let the compiler figure out logic
///   problems such as the specification of a non-metric for a method that requires
///   a proper metric (i.e. that respects the triangle inequality).</para>
///   
/// <para>
///   The objective of this technique is to make it harder to make some mistakes.
///   However, it is possible to bypass this mechanism by using the named constructors
///   such as <see cref="Nonmetric"/> to create distances implementing the <see cref="IMetric{T}"/>
///   interface that are not really metrics. Use at your own risk.</para>
/// </remarks>
public class Minkowski<TNumber> : IDistance<TNumber>
    where TNumber : INumber<TNumber>, IPowerFunctions<TNumber>
{

    private static TNumber _p;

    /// <summary>
    ///   Gets the order <c>p</c> of this Minkowski distance.
    /// </summary>
    public static TNumber Order
    {
        get => _p;
        set
        {
            if (value < TNumber.One)
                throw new ArgumentOutOfRangeException("The Minkowski distance is not a metric for p < 1.");
            _p = value;
        }
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
    public static TNumber Distance(TNumber[] x, TNumber[] y)
    {
        TNumber sum = TNumber.Zero;
        for (int i = 0; i < x.Length; i++)
            sum += TNumber.Pow(TNumber.Abs(x[i] - y[i]), _p);
        return TNumber.Pow(sum, TNumber.One / _p);
    }



}
