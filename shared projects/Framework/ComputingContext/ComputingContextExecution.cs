namespace Vorcyc.Mathematics;

/// <summary>
/// Shared thresholds and helpers for <see cref="ComputingContext"/> dispatch.
/// </summary>
public static class ComputingContextExecution
{
    /// <summary>Minimum length before parallel reductions are considered.</summary>
    public const int ParallelReductionThreshold = 65_536;

    /// <summary>Minimum flop count before parallel matrix multiply is considered.</summary>
    public const int ParallelMatrixMultiplyThreshold = 262_144;

    /// <summary>
    /// Gets the worker count for parallel kernels.
    /// </summary>
    public static int ParallelWorkerCount(ComputingContext? context)
    {
        var resolved = ComputingContext.Resolve(context);
        return resolved.MaxParallelism ?? Environment.ProcessorCount;
    }

    /// <summary>
    /// Returns true when parallel execution should be used for the given problem size.
    /// </summary>
    public static bool UseParallel(ComputingContext? context, int problemSize, int threshold = ParallelReductionThreshold)
    {
        var mode = ComputingContext.Resolve(context).ResolveCpuMode(problemSize);
        return mode == CpuExecutionMode.Parallel
            && problemSize >= threshold
            && ParallelWorkerCount(context) > 1;
    }

    /// <summary>
    /// Returns true when parallel execution should be used over indexed work items.
    /// </summary>
    public static bool UseParallelIndexed(
        ComputingContext? context,
        int count,
        long workPerItem,
        int minCount = 2,
        int threshold = ParallelReductionThreshold)
    {
        if (count < minCount)
        {
            return false;
        }

        long totalWork = workPerItem * count;
        int problemSize = totalWork > int.MaxValue ? int.MaxValue : (int)totalWork;
        return UseParallel(context, problemSize, threshold);
    }

    /// <summary>
    /// Executes <paramref name="body"/> over [<paramref name="fromInclusive"/>, <paramref name="toExclusive"/>),
    /// honoring <see cref="ComputingContext"/> (including <see cref="ComputingScope"/>).
    /// </summary>
    public static void ForEach(
        ComputingContext? context,
        int fromInclusive,
        int toExclusive,
        Action<int> body,
        long workPerItem = 1,
        int minParallelCount = 2,
        int parallelThreshold = ParallelReductionThreshold,
        CancellationToken cancellationToken = default)
    {
        int count = toExclusive - fromInclusive;
        if (count <= 0)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (!UseParallelIndexed(context, count, workPerItem, minParallelCount, parallelThreshold))
        {
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                body(i);
            }

            return;
        }

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = ParallelWorkerCount(context),
            CancellationToken = cancellationToken
        };
        Parallel.For(fromInclusive, toExclusive, options, body);
    }
}
