using System.Numerics;
using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// <see cref="ComputingContext"/>-aware statistics entry points.
/// </summary>
public static partial class Basic
{
    /// <summary>
    /// Sum with optional execution policy.
    /// </summary>
    public static float Sum(this Span<float> values, ComputingContext? context)
    {
        if (values.IsEmpty)
            throw new ArgumentException("Data span cannot be empty.");

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(values.Length);
        return mode switch
        {
            CpuExecutionMode.Normal => SumScalar(values),
            CpuExecutionMode.Parallel when ComputingContextExecution.UseParallel(context, values.Length)
                => SumParallel(values, context),
            _ => Sum(values)
        };
    }

    /// <summary>
    /// Sum with optional execution policy.
    /// </summary>
    public static double Sum(this Span<double> values, ComputingContext? context)
    {
        if (values.IsEmpty)
            throw new ArgumentException("Data span cannot be empty.");

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(values.Length);
        return mode switch
        {
            CpuExecutionMode.Normal => SumScalar(values),
            CpuExecutionMode.Parallel when ComputingContextExecution.UseParallel(context, values.Length)
                => SumParallel(values, context),
            _ => Sum(values)
        };
    }

    /// <summary>
    /// Average with optional execution policy.
    /// </summary>
    public static float Average(this Span<float> values, ComputingContext? context)
        => values.Sum(context) / values.Length;

    /// <summary>
    /// Average with optional execution policy.
    /// </summary>
    public static double Average(this Span<double> values, ComputingContext? context)
        => values.Sum(context) / values.Length;

    /// <summary>
    /// Sample variance with optional execution policy.
    /// </summary>
    public static (float average, float variance) Variance(this Span<float> values, ComputingContext? context)
    {
        if (values.IsEmpty)
        {
            throw new ArgumentException("Data span cannot be empty.");
        }

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(values.Length);
        if (mode == CpuExecutionMode.Normal)
        {
            float mean = SumScalar(values) / values.Length;
            return (mean, SumSquaredDeviationsScalar(values, mean) / (values.Length - 1));
        }

        if (ComputingContextExecution.UseParallel(context, values.Length))
        {
            float mean = values.Average(context);
            float variance = SumSquaredDeviationsParallel(values, mean, context) / (values.Length - 1);
            return (mean, variance);
        }

        return values.Variance();
    }

    /// <summary>
    /// Sample variance with optional execution policy.
    /// </summary>
    public static (double average, double variance) Variance(this Span<double> values, ComputingContext? context)
    {
        if (values.IsEmpty)
        {
            throw new ArgumentException("Data span cannot be empty.");
        }

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(values.Length);
        if (mode == CpuExecutionMode.Normal)
        {
            double mean = SumScalar(values) / values.Length;
            return (mean, SumSquaredDeviationsScalar(values, mean) / (values.Length - 1));
        }

        if (ComputingContextExecution.UseParallel(context, values.Length))
        {
            double mean = values.Average(context);
            double variance = SumSquaredDeviationsParallel(values, mean, context) / (values.Length - 1);
            return (mean, variance);
        }

        return values.Variance();
    }

    /// <summary>
    /// Standard deviation with optional execution policy.
    /// </summary>
    public static float StandardDeviation(this Span<float> values, ComputingContext? context)
    {
        var (_, variance) = values.Variance(context);
        return MathF.Sqrt(variance);
    }

    /// <summary>
    /// Standard deviation with optional execution policy.
    /// </summary>
    public static double StandardDeviation(this Span<double> values, ComputingContext? context)
    {
        var (_, variance) = values.Variance(context);
        return Math.Sqrt(variance);
    }

    private static float SumScalar(ReadOnlySpan<float> values)
    {
        float sum = 0f;
        for (var i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    private static double SumScalar(ReadOnlySpan<double> values)
    {
        double sum = 0d;
        for (var i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    private static float SumParallel(ReadOnlySpan<float> values, ComputingContext? context)
    {
        var data = Materialize(values);
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new float[workers];
        int length = data.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            float local = 0f;
            for (int i = start; i < end; i++)
            {
                local += data[i];
            }

            partials[worker] = local;
        });

        float sum = 0f;
        for (var i = 0; i < partials.Length; i++)
        {
            sum += partials[i];
        }

        return sum;
    }

    private static double SumParallel(ReadOnlySpan<double> values, ComputingContext? context)
    {
        var data = Materialize(values);
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new double[workers];
        int length = data.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            double local = 0d;
            for (int i = start; i < end; i++)
            {
                local += data[i];
            }

            partials[worker] = local;
        });

        double sum = 0d;
        for (var i = 0; i < partials.Length; i++)
        {
            sum += partials[i];
        }

        return sum;
    }

    private static float SumSquaredDeviationsScalar(ReadOnlySpan<float> values, float mean)
    {
        float sum = 0f;
        for (var i = 0; i < values.Length; i++)
        {
            float diff = values[i] - mean;
            sum += diff * diff;
        }

        return sum;
    }

    private static double SumSquaredDeviationsScalar(ReadOnlySpan<double> values, double mean)
    {
        double sum = 0d;
        for (var i = 0; i < values.Length; i++)
        {
            double diff = values[i] - mean;
            sum += diff * diff;
        }

        return sum;
    }

    private static float SumSquaredDeviationsParallel(ReadOnlySpan<float> values, float mean, ComputingContext? context)
    {
        var data = Materialize(values);
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new float[workers];
        int length = data.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            float local = 0f;
            for (int i = start; i < end; i++)
            {
                float diff = data[i] - mean;
                local += diff * diff;
            }

            partials[worker] = local;
        });

        float sum = 0f;
        for (var i = 0; i < partials.Length; i++)
        {
            sum += partials[i];
        }

        return sum;
    }

    private static double SumSquaredDeviationsParallel(ReadOnlySpan<double> values, double mean, ComputingContext? context)
    {
        var data = Materialize(values);
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new double[workers];
        int length = data.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            double local = 0d;
            for (int i = start; i < end; i++)
            {
                double diff = data[i] - mean;
                local += diff * diff;
            }

            partials[worker] = local;
        });

        double sum = 0d;
        for (var i = 0; i < partials.Length; i++)
        {
            sum += partials[i];
        }

        return sum;
    }

    /// <summary>
    /// Average with optional execution policy (floating-point types).
    /// </summary>
    public static T Average<T>(this Span<T> values, ComputingContext? context)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
        {
            throw new ArgumentException("Data span cannot be empty.");
        }

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(values.Length);
        if (mode == CpuExecutionMode.Normal)
        {
            return AverageScalar(values);
        }

        if (ComputingContextExecution.UseParallel(context, values.Length))
        {
            return AverageParallel(values, context);
        }

        return Average(values);
    }

    /// <summary>
    /// Sample variance with optional execution policy (floating-point types).
    /// </summary>
    public static (T average, T variance) Variance<T>(this Span<T> values, ComputingContext? context)
        where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty)
        {
            throw new ArgumentException("Data span cannot be empty.");
        }

        var mode = ComputingContext.Resolve(context).ResolveCpuMode(values.Length);
        if (mode == CpuExecutionMode.Normal)
        {
            T mean = AverageScalar(values);
            return (mean, SumSquaredDeviationsScalar(values, mean) / T.CreateChecked(values.Length - 1));
        }

        if (ComputingContextExecution.UseParallel(context, values.Length))
        {
            T mean = values.Average(context);
            T variance = SumSquaredDeviationsParallel(values, mean, context) / T.CreateChecked(values.Length - 1);
            return (mean, variance);
        }

        return Variance(values);
    }

    /// <summary>
    /// Standard deviation with optional execution policy (floating-point types).
    /// </summary>
    public static T StandardDeviation<T>(this Span<T> values, ComputingContext? context)
        where T : IFloatingPointIeee754<T>
    {
        var (_, variance) = values.Variance(context);
        return T.Sqrt(variance);
    }

    private static T AverageScalar<T>(ReadOnlySpan<T> values)
        where T : IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (var i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum / T.CreateChecked(values.Length);
    }

    private static T AverageParallel<T>(ReadOnlySpan<T> values, ComputingContext? context)
        where T : IFloatingPointIeee754<T>
    {
        var data = Materialize(values);
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new T[workers];
        int length = data.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            T local = T.Zero;
            for (int i = start; i < end; i++)
            {
                local += data[i];
            }

            partials[worker] = local;
        });

        T sum = T.Zero;
        for (var i = 0; i < partials.Length; i++)
        {
            sum += partials[i];
        }

        return sum / T.CreateChecked(values.Length);
    }

    private static T SumSquaredDeviationsScalar<T>(ReadOnlySpan<T> values, T mean)
        where T : IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (var i = 0; i < values.Length; i++)
        {
            T diff = values[i] - mean;
            sum += diff * diff;
        }

        return sum;
    }

    private static T SumSquaredDeviationsParallel<T>(ReadOnlySpan<T> values, T mean, ComputingContext? context)
        where T : IFloatingPointIeee754<T>
    {
        var data = Materialize(values);
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new T[workers];
        int length = data.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            T local = T.Zero;
            for (int i = start; i < end; i++)
            {
                T diff = data[i] - mean;
                local += diff * diff;
            }

            partials[worker] = local;
        });

        T sum = T.Zero;
        for (var i = 0; i < partials.Length; i++)
        {
            sum += partials[i];
        }

        return sum;
    }

    /// <summary>
    /// Weighted average with optional execution policy (floating-point types).
    /// </summary>
    public static T WeightedAverage<T>(ReadOnlySpan<T> values, ReadOnlySpan<T> weights, ComputingContext? context)
        where T : IFloatingPointIeee754<T>
    {
        if (values.Length != weights.Length || values.IsEmpty)
        {
            throw new ArgumentException("Values and weights must have the same non-zero length.");
        }

        T weightedSum = T.Zero;
        T weightTotal = T.Zero;

        if (ComputingContextExecution.UseParallel(context, values.Length))
        {
            var valueData = values.ToArray();
            var weightData = weights.ToArray();
            int workers = ComputingContextExecution.ParallelWorkerCount(context);
            var partialWeighted = new T[workers];
            var partialWeights = new T[workers];
            int length = valueData.Length;
            int chunk = (length + workers - 1) / workers;

            Parallel.For(0, workers, worker =>
            {
                int start = worker * chunk;
                if (start >= length)
                {
                    return;
                }

                int end = Math.Min(start + chunk, length);
                T localWeighted = T.Zero;
                T localWeight = T.Zero;
                for (int i = start; i < end; i++)
                {
                    localWeighted += valueData[i] * weightData[i];
                    localWeight += weightData[i];
                }

                partialWeighted[worker] = localWeighted;
                partialWeights[worker] = localWeight;
            });

            for (var i = 0; i < workers; i++)
            {
                weightedSum += partialWeighted[i];
                weightTotal += partialWeights[i];
            }
        }
        else if (ComputingContext.Resolve(context).ResolveCpuMode(values.Length) == CpuExecutionMode.Normal)
        {
            for (var i = 0; i < values.Length; i++)
            {
                weightedSum += values[i] * weights[i];
                weightTotal += weights[i];
            }
        }
        else
        {
            return WeightedAverage(values, weights);
        }

        if (weightTotal == T.Zero)
        {
            throw new ArgumentException("Sum of weights must be non-zero.");
        }

        return weightedSum / weightTotal;
    }

    private static T[] Materialize<T>(ReadOnlySpan<T> span) => span.ToArray();
}

public static partial class IComparableExtension
{
    /// <summary>
    /// Returns the maximum value using an optional execution policy.
    /// </summary>
    public static T CompareMax<T>(this Span<T> span, ComputingContext? context)
        where T : IComparable, IComparable<T>
    {
        if (span.IsEmpty)
        {
            throw new ArgumentException("Span cannot be empty.");
        }

        if (ComputingContextExecution.UseParallel(context, span.Length))
        {
            return CompareMaxParallel(span, context);
        }

        return span.CompareMax();
    }

    /// <summary>
    /// Returns the minimum value using an optional execution policy.
    /// </summary>
    public static T CompareMin<T>(this Span<T> span, ComputingContext? context)
        where T : IComparable, IComparable<T>
    {
        if (span.IsEmpty)
        {
            throw new ArgumentException("Span cannot be empty.");
        }

        if (ComputingContextExecution.UseParallel(context, span.Length))
        {
            return CompareMinParallel(span, context);
        }

        return span.CompareMin();
    }

    private static T CompareMaxParallel<T>(ReadOnlySpan<T> span, ComputingContext? context)
        where T : IComparable, IComparable<T>
    {
        var data = span.ToArray();
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new T[workers];
        int length = data.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            T localMax = data[start];
            for (int i = start + 1; i < end; i++)
            {
                if (data[i].GreaterThan(localMax))
                {
                    localMax = data[i];
                }
            }

            partials[worker] = localMax;
        });

        T result = partials[0];
        for (var i = 1; i < partials.Length; i++)
        {
            if (partials[i].GreaterThan(result))
            {
                result = partials[i];
            }
        }

        return result;
    }

    private static T CompareMinParallel<T>(ReadOnlySpan<T> span, ComputingContext? context)
        where T : IComparable, IComparable<T>
    {
        var data = span.ToArray();
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new T[workers];
        int length = data.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            T localMin = data[start];
            for (int i = start + 1; i < end; i++)
            {
                if (data[i].LessThan(localMin))
                {
                    localMin = data[i];
                }
            }

            partials[worker] = localMin;
        });

        T result = partials[0];
        for (var i = 1; i < partials.Length; i++)
        {
            if (partials[i].LessThan(result))
            {
                result = partials[i];
            }
        }

        return result;
    }
}
