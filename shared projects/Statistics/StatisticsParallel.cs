// Internal parallel-reduction helpers for ComputingContext-aware statistics.
// These mirror the chunked partial-sum pattern used elsewhere in the code base
// (e.g. LinearAlgebra.VectorSpanComputingContext) so that the generic statistical
// hot paths can share a single, consistent parallel reduction strategy instead of
// duplicating the chunking logic in each method.

using System.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Provides internal, generic parallel reduction helpers used by the
/// <see cref="ComputingContext"/>-aware statistical APIs.
/// </summary>
/// <remarks>
/// The work is partitioned into one chunk per worker (see
/// <see cref="ComputingContextExecution.ParallelWorkerCount(ComputingContext?)"/>),
/// each chunk produces a partial result, and the partials are combined by summation.
/// Callers are responsible for deciding whether parallel execution is worthwhile via
/// <see cref="ComputingContextExecution.UseParallel(ComputingContext?, int, int)"/>.
/// </remarks>
internal static class StatisticsParallel
{
    /// <summary>
    /// Performs a chunked parallel reduction over a single sequence of <paramref name="length"/> elements,
    /// summing the per-chunk partial results.
    /// </summary>
    /// <typeparam name="T">The numeric element type.</typeparam>
    /// <param name="length">The number of elements to reduce.</param>
    /// <param name="context">The execution policy controlling the worker count.</param>
    /// <param name="primary">The data backing the reduction.</param>
    /// <param name="accumulate">
    /// A function that accumulates a partial result over the half-open index range <c>[start, end)</c>
    /// of <paramref name="primary"/>.
    /// </param>
    /// <returns>The sum of all per-chunk partial results.</returns>
    public static T ReduceParallel<T>(
        int length,
        ComputingContext? context,
        T[] primary,
        Func<T[], int, int, T> accumulate)
        where T : INumberBase<T>
    {
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new T[workers];
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            partials[worker] = accumulate(primary, start, end);
        });

        T sum = T.Zero;
        for (int i = 0; i < partials.Length; i++)
        {
            sum += partials[i];
        }

        return sum;
    }

    /// <summary>
    /// Performs a chunked parallel reduction over two aligned sequences of <paramref name="length"/> elements,
    /// summing the per-chunk partial results.
    /// </summary>
    /// <typeparam name="T">The numeric element type.</typeparam>
    /// <param name="length">The number of elements to reduce.</param>
    /// <param name="context">The execution policy controlling the worker count.</param>
    /// <param name="primary">The first data sequence backing the reduction.</param>
    /// <param name="secondary">The second data sequence backing the reduction.</param>
    /// <param name="accumulate">
    /// A function that accumulates a partial result over the half-open index range <c>[start, end)</c>
    /// of <paramref name="primary"/> and <paramref name="secondary"/>.
    /// </param>
    /// <returns>The sum of all per-chunk partial results.</returns>
    public static T ReduceParallel<T>(
        int length,
        ComputingContext? context,
        T[] primary,
        T[] secondary,
        Func<T[], T[], int, int, T> accumulate)
        where T : INumberBase<T>
    {
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new T[workers];
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            partials[worker] = accumulate(primary, secondary, start, end);
        });

        T sum = T.Zero;
        for (int i = 0; i < partials.Length; i++)
        {
            sum += partials[i];
        }

        return sum;
    }
}
