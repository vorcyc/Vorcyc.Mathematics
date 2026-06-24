using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

public static partial class Basic
{
    /// <summary>
    /// Computes mean and variance with explicit <see cref="DescriptiveStatisticsOptions"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T average, T variance) Variance<T>(
        this Span<T> values,
        DescriptiveStatisticsOptions options)
        where T : INumber<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));

        int divisor = options.VarianceDivisor(values.Length);
        var mean = Average(values);
        T result = T.Zero;

        int length = values.Length;
        int simdLength = Vector<T>.Count;
        int remainder = length % simdLength;
        Vector<T> varianceVector = Vector<T>.Zero;
        int i = 0;

        for (; i < length - remainder; i += simdLength)
        {
            Vector<T> vector = new(values.Slice(i, simdLength));
            Vector<T> diff = vector - new Vector<T>(mean);
            varianceVector += diff * diff;
        }

        for (int j = 0; j < simdLength; j++)
            result += varianceVector[j];

        for (; i < length; i++)
        {
            var v = values[i];
            result += (v - mean) * (v - mean);
        }

        result /= T.CreateChecked(divisor);
        return (mean, result);
    }

    /// <summary>
    /// Computes standard deviation with explicit options.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T StandardDeviation<T>(
        this Span<T> values,
        DescriptiveStatisticsOptions options)
        where T : IFloatingPointIeee754<T>
    {
        var (_, variance) = Variance(values, options);
        return T.Sqrt(variance);
    }

    /// <summary>
    /// Computes all descriptive statistics with explicit options.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T Mean, T Median, T Mode, T Variance, T StandardDeviation, T CoefficientOfVariation)
        CalculateAllStatistics<T>(
            this Span<T> values,
            DescriptiveStatisticsOptions options)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));

        T sum = T.Zero;
        T sumOfSquares = T.Zero;
        var frequency = new Dictionary<T, int>();
        var sortedValues = new T[values.Length];
        values.CopyTo(sortedValues);
        Array.Sort(sortedValues);

        for (int i = 0; i < values.Length; i++)
        {
            var value = values[i];
            sum += value;
            sumOfSquares += value * value;
            frequency[value] = frequency.GetValueOrDefault(value) + 1;
        }

        T mean = sum / T.CreateChecked(values.Length);
        T median = values.Length % 2 == 0
            ? (sortedValues[values.Length / 2 - 1] + sortedValues[values.Length / 2]) / T.CreateChecked(2)
            : sortedValues[values.Length / 2];

        T mode = frequency.OrderByDescending(kvp => kvp.Value).First().Key;
        int divisor = options.VarianceDivisor(values.Length);
        T variance = (sumOfSquares - sum * sum / T.CreateChecked(values.Length)) / T.CreateChecked(divisor);
        T standardDeviation = T.Sqrt(variance);
        T coefficientOfVariation = standardDeviation / mean;

        return (mean, median, mode, variance, standardDeviation, coefficientOfVariation);
    }

    /// <summary>
    /// Geometric mean for positive values.
    /// </summary>
    public static T GeometricMean<T>(this ReadOnlySpan<T> values)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));

        T logSum = T.Zero;
        foreach (var v in values)
        {
            if (v <= T.Zero)
                throw new ArgumentException("Geometric mean requires positive values.");
            logSum += T.Log(v);
        }

        return T.Exp(logSum / T.CreateChecked(values.Length));
    }

    /// <summary>
    /// Harmonic mean for non-zero values.
    /// </summary>
    public static T HarmonicMean<T>(this ReadOnlySpan<T> values)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Span cannot be empty.", nameof(values));

        T reciprocalSum = T.Zero;
        foreach (var v in values)
        {
            if (v == T.Zero)
                throw new ArgumentException("Harmonic mean requires non-zero values.");
            reciprocalSum += T.One / v;
        }

        return T.CreateChecked(values.Length) / reciprocalSum;
    }
}
