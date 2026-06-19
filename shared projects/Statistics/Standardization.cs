// Standardization (feature scaling) primitives
// Provides T[]/ReadOnlySpan<T>-friendly scaling transforms such as z-score (standard score)
// and min-max normalization. These are stateless, allocation-returning helpers intended for
// general statistical use (distinct from the stateful Fit/Transform scalers in MachineLearning).

using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Provides feature-scaling transforms, including z-score (standard score) standardization
/// and min-max normalization.
/// </summary>
/// <remarks>
/// Unlike the stateful scalers in the machine learning module, these methods are stateless
/// and return a newly allocated array containing the transformed values.
/// </remarks>
public static class Standardization
{
    /// <summary>
    /// Standardizes a data set to zero mean and unit variance (z-score / standard score).
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="values">The data set to standardize.</param>
    /// <param name="sample">When <see langword="true"/>, uses the sample standard deviation (divides by n-1); otherwise uses the population standard deviation (divides by n). The default is <see langword="false"/>.</param>
    /// <returns>A new array where each element is transformed to <c>(value - mean) / standardDeviation</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty, or when <paramref name="sample"/> is <see langword="true"/> and fewer than two values are provided.</exception>
    /// <remarks>
    /// When the standard deviation is zero (all values are identical), the result is an array of zeros.
    /// </remarks>
    public static T[] ZScore<T>(ReadOnlySpan<T> values, bool sample = false)
        where T : IFloatingPointIeee754<T>, IRootFunctions<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("The data set cannot be empty.", nameof(values));
        if (sample && values.Length < 2)
            throw new ArgumentException("At least two values are required to compute the sample standard deviation.", nameof(values));

        int n = values.Length;
        T count = T.CreateChecked(n);

        T sum = T.Zero;
        for (int i = 0; i < n; i++)
            sum += values[i];
        T mean = sum / count;

        T sumSquares = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T diff = values[i] - mean;
            sumSquares += diff * diff;
        }

        T denominator = sample ? T.CreateChecked(n - 1) : count;
        T stdDev = T.Sqrt(sumSquares / denominator);

        T[] result = new T[n];
        if (stdDev == T.Zero)
            return result;

        for (int i = 0; i < n; i++)
            result[i] = (values[i] - mean) / stdDev;

        return result;
    }

    /// <summary>
    /// Standardizes a data set to zero mean and unit variance (z-score / standard score),
    /// honoring the supplied <see cref="ComputingContext"/> execution policy.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="values">The data set to standardize.</param>
    /// <param name="sample">When <see langword="true"/>, uses the sample standard deviation (divides by n-1); otherwise uses the population standard deviation (divides by n). The default is <see langword="false"/>.</param>
    /// <param name="context">
    /// The execution policy. When <see langword="null"/>, the ambient or default policy is used.
    /// Parallel execution is only applied for sufficiently large data sets; otherwise the scalar
    /// implementation is used and the result is identical.
    /// </param>
    /// <returns>A new array where each element is transformed to <c>(value - mean) / standardDeviation</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty, or when <paramref name="sample"/> is <see langword="true"/> and fewer than two values are provided.</exception>
    /// <remarks>
    /// When the standard deviation is zero (all values are identical), the result is an array of zeros.
    /// </remarks>
    public static T[] ZScore<T>(ReadOnlySpan<T> values, bool sample, ComputingContext? context)
        where T : IFloatingPointIeee754<T>, IRootFunctions<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("The data set cannot be empty.", nameof(values));
        if (sample && values.Length < 2)
            throw new ArgumentException("At least two values are required to compute the sample standard deviation.", nameof(values));

        if (!ComputingContextExecution.UseParallel(context, values.Length))
            return ZScore<T>(values, sample);

        T[] data = values.ToArray();
        int n = data.Length;
        T count = T.CreateChecked(n);

        T sum = StatisticsParallel.ReduceParallel(n, context, data, static (array, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
                local += array[i];
            return local;
        });
        T mean = sum / count;

        T sumSquares = StatisticsParallel.ReduceParallel(n, context, data, (array, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
            {
                T diff = array[i] - mean;
                local += diff * diff;
            }
            return local;
        });

        T denominator = sample ? T.CreateChecked(n - 1) : count;
        T stdDev = T.Sqrt(sumSquares / denominator);

        T[] result = new T[n];
        if (stdDev == T.Zero)
            return result;

        ComputingContextExecution.ForEach(context, 0, n, i => result[i] = (data[i] - mean) / stdDev);

        return result;
    }

    /// <summary>
    /// Standardizes a data set to zero mean and unit variance (z-score / standard score),
    /// honoring the supplied <see cref="ComputingContext"/> execution policy.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="values">The data set to standardize.</param>
    /// <param name="sample">When <see langword="true"/>, uses the sample standard deviation (divides by n-1); otherwise uses the population standard deviation (divides by n). The default is <see langword="false"/>.</param>
    /// <param name="context">The execution policy. When <see langword="null"/>, the ambient or default policy is used.</param>
    /// <returns>A new array where each element is transformed to <c>(value - mean) / standardDeviation</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty, or when <paramref name="sample"/> is <see langword="true"/> and fewer than two values are provided.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ZScore<T>(T[] values, bool sample, ComputingContext? context)
        where T : IFloatingPointIeee754<T>, IRootFunctions<T>
        => ZScore<T>((ReadOnlySpan<T>)values, sample, context);

    /// <summary>
    /// Standardizes a data set to zero mean and unit variance (z-score / standard score).
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="values">The data set to standardize.</param>
    /// <param name="sample">When <see langword="true"/>, uses the sample standard deviation (divides by n-1); otherwise uses the population standard deviation (divides by n). The default is <see langword="false"/>.</param>
    /// <returns>A new array where each element is transformed to <c>(value - mean) / standardDeviation</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty, or when <paramref name="sample"/> is <see langword="true"/> and fewer than two values are provided.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ZScore<T>(T[] values, bool sample = false)
        where T : IFloatingPointIeee754<T>, IRootFunctions<T>
        => ZScore<T>((ReadOnlySpan<T>)values, sample);

    /// <summary>
    /// Scales a data set to a target range using min-max normalization.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <param name="values">The data set to scale.</param>
    /// <param name="min">The lower bound of the target range. The default is <see cref="INumberBase{T}.Zero"/>.</param>
    /// <param name="max">The upper bound of the target range. The default is <see cref="INumberBase{T}.One"/>.</param>
    /// <returns>A new array where each element is linearly scaled into the range <c>[min, max]</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty or when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    /// <remarks>
    /// When all values are identical (the source range has zero width), every element is mapped to <paramref name="min"/>.
    /// </remarks>
    public static T[] MinMaxScale<T>(ReadOnlySpan<T> values, T? min = null, T? max = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("The data set cannot be empty.", nameof(values));

        T targetMin = min ?? T.Zero;
        T targetMax = max ?? T.One;
        if (targetMax < targetMin)
            throw new ArgumentException("The target range maximum must be greater than or equal to the minimum.", nameof(max));

        T sourceMin = values[0];
        T sourceMax = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < sourceMin) sourceMin = values[i];
            if (values[i] > sourceMax) sourceMax = values[i];
        }

        int n = values.Length;
        T[] result = new T[n];
        T sourceRange = sourceMax - sourceMin;
        if (sourceRange == T.Zero)
        {
            for (int i = 0; i < n; i++)
                result[i] = targetMin;
            return result;
        }

        T targetRange = targetMax - targetMin;
        for (int i = 0; i < n; i++)
            result[i] = targetMin + (values[i] - sourceMin) / sourceRange * targetRange;

        return result;
    }

    /// <summary>
    /// Scales a data set to a target range using min-max normalization.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <param name="values">The data set to scale.</param>
    /// <param name="min">The lower bound of the target range. The default is <see cref="INumberBase{T}.Zero"/>.</param>
    /// <param name="max">The upper bound of the target range. The default is <see cref="INumberBase{T}.One"/>.</param>
    /// <returns>A new array where each element is linearly scaled into the range <c>[min, max]</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty or when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] MinMaxScale<T>(T[] values, T? min = null, T? max = null)
        where T : struct, IFloatingPointIeee754<T>
        => MinMaxScale<T>((ReadOnlySpan<T>)values, min, max);
}
