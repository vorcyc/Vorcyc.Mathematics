namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Specifies whether variance is computed for a population or a sample.
/// </summary>
public enum VarianceKind
{
    /// <summary>Divide by N (population variance).</summary>
    Population,

    /// <summary>Divide by N - 1 (Bessel-corrected sample variance).</summary>
    Sample
}

/// <summary>
/// Options for descriptive statistics calculations.
/// </summary>
public readonly struct DescriptiveStatisticsOptions
{
    /// <summary>Gets variance kind (sample by default).</summary>
    public VarianceKind VarianceKind { get; init; }

    /// <summary>
    /// When false, order-statistics methods copy data before sorting.
    /// </summary>
    public bool SortInPlace { get; init; }

    /// <summary>Default options: sample variance, non-destructive sort.</summary>
    public static DescriptiveStatisticsOptions Default { get; } = new()
    {
        VarianceKind = VarianceKind.Sample,
        SortInPlace = false
    };

    internal int VarianceDivisor(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be positive.", nameof(count));

        return VarianceKind == VarianceKind.Population ? count : count - 1;
    }
}
