// Divergence and distribution-distance primitives
// Provides T[]/ReadOnlySpan<T>-friendly measures between two probability distributions,
// such as the Hellinger and Bhattacharyya distances. These act as the single source of
// truth so other modules (e.g. MachineLearning distances) can reuse them.

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Provides distance and divergence measures between two probability distributions,
/// including the Hellinger distance and the Bhattacharyya distance.
/// </summary>
/// <remarks>
/// The inputs are expected to be discrete probability distributions (non-negative values
/// that sum to one). These methods are low-level primitives reused by higher-level
/// statistical APIs and by the machine learning distance layer.
/// </remarks>
public static partial class Divergence
{
    /// <summary>
    /// Computes the Hellinger distance between two discrete probability distributions.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/>, <see cref="IRootFunctions{T}"/> and <see cref="IPowerFunctions{T}"/>.</typeparam>
    /// <param name="x">The first probability distribution.</param>
    /// <param name="y">The second probability distribution.</param>
    /// <returns>The Hellinger distance between <paramref name="x"/> and <paramref name="y"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the two distributions have different lengths or are empty.</exception>
    /// <remarks>
    /// The Hellinger distance is defined as the sum of squared differences between the square
    /// roots of the corresponding probabilities, scaled by the reciprocal of the square root of two.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T HellingerDistance<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y)
        where T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two distributions must be equal.");
        if (x.IsEmpty)
            throw new ArgumentException("The distributions cannot be empty.", nameof(x));

        T sum = T.Zero;
        T two = T.CreateChecked(2);
        for (int i = 0; i < x.Length; i++)
            sum += T.Pow(T.Sqrt(x[i]) - T.Sqrt(y[i]), two);

        return sum / T.Sqrt(two);
    }

    /// <summary>
    /// Computes the Hellinger distance between two discrete probability distributions.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/>, <see cref="IRootFunctions{T}"/> and <see cref="IPowerFunctions{T}"/>.</typeparam>
    /// <param name="x">The first probability distribution.</param>
    /// <param name="y">The second probability distribution.</param>
    /// <returns>The Hellinger distance between <paramref name="x"/> and <paramref name="y"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the two distributions have different lengths or are empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T HellingerDistance<T>(T[] x, T[] y)
        where T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>
        => HellingerDistance<T>((ReadOnlySpan<T>)x, (ReadOnlySpan<T>)y);

    /// <summary>
    /// Computes the Bhattacharyya distance between two discrete probability distributions.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/>, <see cref="IRootFunctions{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="x">The first probability distribution.</param>
    /// <param name="y">The second probability distribution.</param>
    /// <returns>
    /// The Bhattacharyya distance, defined as the negative natural logarithm of the
    /// Bhattacharyya coefficient between the two distributions.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the two distributions have different lengths or are empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BhattacharyyaDistance<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y)
        where T : IFloatingPointIeee754<T>, IRootFunctions<T>, ILogarithmicFunctions<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two distributions must be equal.");
        if (x.IsEmpty)
            throw new ArgumentException("The distributions cannot be empty.", nameof(x));

        T coefficient = T.Zero;
        for (int i = 0; i < x.Length; i++)
            coefficient += T.Sqrt(x[i] * y[i]);

        return coefficient == T.Zero ? T.PositiveInfinity : -T.Log(coefficient);
    }

    /// <summary>
    /// Computes the Bhattacharyya distance between two discrete probability distributions.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/>, <see cref="IRootFunctions{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="x">The first probability distribution.</param>
    /// <param name="y">The second probability distribution.</param>
    /// <returns>The Bhattacharyya distance between <paramref name="x"/> and <paramref name="y"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the two distributions have different lengths or are empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BhattacharyyaDistance<T>(T[] x, T[] y)
        where T : IFloatingPointIeee754<T>, IRootFunctions<T>, ILogarithmicFunctions<T>
        => BhattacharyyaDistance<T>((ReadOnlySpan<T>)x, (ReadOnlySpan<T>)y);
}
