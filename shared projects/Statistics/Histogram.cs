// Histogram (binning) primitives
// Provides allocation-returning helpers to bin a data set into a histogram, either with
// equal-width bins derived from the data range or with caller-supplied bin edges.

using System.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Provides histogram (binning) computations over a data set, supporting both equal-width
/// bins and explicit, caller-supplied bin edges.
/// </summary>
public static class Histogram
{
    /// <summary>
    /// Computes an equal-width histogram of a data set.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <param name="values">The data set to bin.</param>
    /// <param name="binCount">The number of equal-width bins. Must be greater than zero.</param>
    /// <returns>
    /// A tuple containing the <c>BinEdges</c> (an array of length <paramref name="binCount"/> + 1
    /// describing the inclusive lower and exclusive upper bounds of each bin, with the final
    /// upper bound inclusive) and the <c>Counts</c> (the number of values falling into each bin).
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="binCount"/> is less than one.</exception>
    /// <remarks>
    /// When all values are identical, a single populated bin centered on that value is produced
    /// and the remaining bins are empty.
    /// </remarks>
    public static (T[] BinEdges, int[] Counts) Compute<T>(ReadOnlySpan<T> values, int binCount)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("The data set cannot be empty.", nameof(values));
        if (binCount < 1)
            throw new ArgumentOutOfRangeException(nameof(binCount), "The number of bins must be at least one.");

        T min = values[0];
        T max = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < min) min = values[i];
            if (values[i] > max) max = values[i];
        }

        // Guard against a zero-width range by expanding it slightly around the single value.
        if (min == max)
        {
            T half = T.One / T.CreateChecked(2);
            min -= half;
            max += half;
        }

        T count = T.CreateChecked(binCount);
        T width = (max - min) / count;

        T[] edges = new T[binCount + 1];
        for (int i = 0; i <= binCount; i++)
            edges[i] = min + width * T.CreateChecked(i);
        edges[binCount] = max;

        int[] counts = new int[binCount];
        for (int i = 0; i < values.Length; i++)
        {
            int bin = int.CreateTruncating((values[i] - min) / width);
            if (bin < 0) bin = 0;
            if (bin >= binCount) bin = binCount - 1;
            counts[bin]++;
        }

        return (edges, counts);
    }

    /// <summary>
    /// Computes an equal-width histogram of a data set.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <param name="values">The data set to bin.</param>
    /// <param name="binCount">The number of equal-width bins. Must be greater than zero.</param>
    /// <returns>A tuple containing the bin edges and the per-bin counts.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="binCount"/> is less than one.</exception>
    public static (T[] BinEdges, int[] Counts) Compute<T>(T[] values, int binCount)
        where T : IFloatingPointIeee754<T>
        => Compute<T>((ReadOnlySpan<T>)values, binCount);

    /// <summary>
    /// Computes a histogram of a data set using explicit, monotonically increasing bin edges.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="values">The data set to bin.</param>
    /// <param name="edges">The bin edges in strictly increasing order. There are <c>edges.Length - 1</c> bins; bin <c>i</c> spans <c>[edges[i], edges[i + 1])</c>, with the final bin including its upper edge.</param>
    /// <returns>An array of length <c>edges.Length - 1</c> containing the number of values falling into each bin. Values outside the edge range are ignored.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty, when fewer than two edges are supplied, or when the edges are not strictly increasing.</exception>
    public static int[] Compute<T>(ReadOnlySpan<T> values, ReadOnlySpan<T> edges)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("The data set cannot be empty.", nameof(values));
        if (edges.Length < 2)
            throw new ArgumentException("At least two bin edges are required.", nameof(edges));
        for (int i = 1; i < edges.Length; i++)
        {
            if (edges[i] <= edges[i - 1])
                throw new ArgumentException("The bin edges must be in strictly increasing order.", nameof(edges));
        }

        int binCount = edges.Length - 1;
        int[] counts = new int[binCount];
        T lower = edges[0];
        T upper = edges[binCount];

        for (int i = 0; i < values.Length; i++)
        {
            T value = values[i];
            if (value < lower || value > upper)
                continue;

            int bin = FindBin(edges, value, binCount);
            if (bin >= 0)
                counts[bin]++;
        }

        return counts;
    }

    /// <summary>
    /// Computes a histogram of a data set using explicit, monotonically increasing bin edges.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="values">The data set to bin.</param>
    /// <param name="edges">The bin edges in strictly increasing order.</param>
    /// <returns>An array containing the number of values falling into each bin.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty, when fewer than two edges are supplied, or when the edges are not strictly increasing.</exception>
    public static int[] Compute<T>(T[] values, T[] edges)
        where T : INumber<T>
        => Compute<T>((ReadOnlySpan<T>)values, (ReadOnlySpan<T>)edges);

    /// <summary>
    /// Locates the bin index for a value within the supplied edges using a linear scan.
    /// The final bin includes its upper edge.
    /// </summary>
    private static int FindBin<T>(ReadOnlySpan<T> edges, T value, int binCount)
        where T : INumber<T>
    {
        for (int b = 0; b < binCount; b++)
        {
            bool isLastBin = b == binCount - 1;
            bool inRange = value >= edges[b] && (isLastBin ? value <= edges[b + 1] : value < edges[b + 1]);
            if (inRange)
                return b;
        }

        return -1;
    }
}
