using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;


/// <summary>
///   Square-Euclidean distance and similarity. Please note that this
///   distance is not a metric as it doesn't obey the triangle inequality.
/// </summary>
public class WeightedEuclidean<TSelf>
    : IDistance<TSelf>, ISimilarity<TSelf>
    where TSelf : INumber<TSelf>, IRootFunctions<TSelf>
{


    private static TSelf[] _weights;

    /// <summary>
    /// Gets or sets the weights for each dimension. Default is a vector of ones.
    /// </summary>
    public static TSelf[] Weights
    {
        get => _weights;
        set => _weights = value;
    }

    /// <summary>
    /// Sets <see cref="Weights"/> to ones with specified dim.
    /// </summary>
    /// <param name="dimensions">The number of dimensions (columns) in the dataset.</param>
    public static void SetDimensions(int dimensions)
    {
        _weights = new TSelf[dimensions];
        for (int i = 0; i < _weights.Length; i++)
            _weights[i] = TSelf.One;
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
    ///   A <see cref="INumber{TSelf}"/> value representing the distance <c>d(x,y)</c>
    ///   between <paramref name="x"/> and <paramref name="y"/> according 
    ///   to the distance function implemented by this class.
    /// </returns>
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Distance(TSelf[] x, TSelf[] y)
    {
        TSelf sum = TSelf.Zero;
        for (int i = 0; i < x.Length; i++)
        {
            TSelf u = x[i] - y[i];
            sum += u * u * _weights[i];
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf Similarity(TSelf[] x, TSelf[] y)
    {
        return TSelf.One / (TSelf.One + Distance(x, y));
    }
}
