using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Robust (outlier-resistant) descriptive statistics.
/// </summary>
public static class Robust
{
    /// <summary>
    /// Interquartile range (Q3 - Q1).
    /// </summary>
    public static T InterquartileRange<T>(this Span<T> values)
        where T : IFloatingPointIeee754<T>
    {
        var (q1, _, q3) = values.Quartiles();
        return q3 - q1;
    }

    /// <summary>
    /// Median absolute deviation from the median.
    /// </summary>
    public static T MedianAbsoluteDeviation<T>(this Span<T> values)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));

        T median = values.Median();
        var deviations = new T[values.Length];
        for (int i = 0; i < values.Length; i++)
            deviations[i] = T.Abs(values[i] - median);

        return deviations.AsSpan().Median();
    }

    /// <summary>
    /// Clamps values to [lower percentile, upper percentile] (inclusive).
    /// </summary>
    public static T[] Winsorize<T>(ReadOnlySpan<T> values, double lowerPercentile = 0.05, double upperPercentile = 0.95)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Values cannot be empty.", nameof(values));

        var sorted = values.ToArray();
        Array.Sort(sorted);
        T lower = sorted.AsSpan().Percentile(lowerPercentile);
        T upper = sorted.AsSpan().Percentile(upperPercentile);

        var result = new T[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            T v = values[i];
            if (v < lower) result[i] = lower;
            else if (v > upper) result[i] = upper;
            else result[i] = v;
        }

        return result;
    }

    /// <summary>
    /// Huber mean via iteratively reweighted least squares.
    /// </summary>
    public static T HuberMean<T>(this Span<T> values, T delta, int maxIterations = 50)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));

        T mean = values.Average();
        for (int iter = 0; iter < maxIterations; iter++)
        {
            T numerator = T.Zero;
            T denominator = T.Zero;
            foreach (var v in values)
            {
                T residual = v - mean;
                T abs = T.Abs(residual);
                T weight = abs <= delta ? T.One : delta / abs;
                numerator += weight * v;
                denominator += weight;
            }

            T next = numerator / denominator;
            if (T.Abs(next - mean) < T.CreateChecked(1e-12))
                break;
            mean = next;
        }

        return mean;
    }
}
