using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

public static partial class Basic
{
    /// <summary>
    /// Weighted arithmetic mean.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T WeightedAverage<T>(ReadOnlySpan<T> values, ReadOnlySpan<T> weights)
        where T : INumber<T>
    {
        if (values.Length != weights.Length || values.IsEmpty)
            throw new ArgumentException("Values and weights must have the same non-zero length.");

        T weightedSum = T.Zero;
        T weightTotal = T.Zero;
        for (int i = 0; i < values.Length; i++)
        {
            weightedSum += values[i] * weights[i];
            weightTotal += weights[i];
        }

        if (weightTotal == T.Zero)
            throw new ArgumentException("Sum of weights must be non-zero.");

        return weightedSum / weightTotal;
    }

    /// <summary>
    /// Weighted sample variance (Bessel-corrected when possible).
    /// </summary>
    public static (T Mean, T Variance) WeightedVariance<T>(
        ReadOnlySpan<T> values,
        ReadOnlySpan<T> weights,
        DescriptiveStatisticsOptions? options = null)
        where T : IFloatingPointIeee754<T>
    {
        if (values.Length != weights.Length || values.IsEmpty)
            throw new ArgumentException("Values and weights must have the same non-zero length.");

        var opts = options ?? DescriptiveStatisticsOptions.Default;

        T mean = WeightedAverage(values, weights);
        T numerator = T.Zero;
        T weightSum = T.Zero;

        for (int i = 0; i < values.Length; i++)
        {
            T diff = values[i] - mean;
            numerator += weights[i] * diff * diff;
            weightSum += weights[i];
        }

        T denominator = opts.VarianceKind == VarianceKind.Population
            ? weightSum
            : weightSum - T.One;

        if (double.CreateChecked(denominator) <= 0)
            return (mean, T.Zero);

        return (mean, numerator / denominator);
    }
}
