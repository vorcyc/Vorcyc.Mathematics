using System.Numerics;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics;


/// <summary>
/// Provides extension members for <see cref="PinnableArray{T}"/> to compute aggregate values
/// such as minimum and maximum, including span-based and asynchronous variants.
/// </summary>
/// <remarks>
/// - Designed for numeric types implementing <see cref="INumber{TSelf}"/>.
/// - Methods operate directly on the underlying storage via <see cref="PinnableArray{T}.AsSpan()"/> to avoid extra allocations.
/// - Async methods can utilize a configurable worker count or TPL-based strategy.
/// </remarks>
public static class PinnableArrayExtension
{
    extension<T>(PinnableArray<T> array)
        where T : unmanaged, INumber<T>
    {
        #region Max

        /// <summary>
        /// Returns the maximum value in the current <see cref="PinnableArray{T}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Max() => Statistics.INumberExtension.Max(array.AsSpan());

        /// <summary>
        /// Returns the maximum value within the specified sub-range of the current <see cref="PinnableArray{T}"/>.
        /// </summary>
        /// <param name="start">Start index of the sub-range.</param>
        /// <param name="length">Number of elements in the sub-range.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Max(int start, int length) => Statistics.INumberExtension.Max(array.AsSpan(start, length));

        /// <summary>
        /// Asynchronously computes the maximum value in the current <see cref="PinnableArray{T}"/>.
        /// </summary>
        /// <param name="numberOfWorkers">
        /// Optional worker count (≤ <see cref="Environment.ProcessorCount"/>). Ignored when <paramref name="useTPL"/> is true.
        /// </param>
        /// <param name="useTPL">
        /// If true, uses the Task Parallel Library strategy; otherwise a custom partitioned approach.
        /// </param>
        /// <returns>A task producing the maximum value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> MaxAsync(int? numberOfWorkers = null, bool useTPL = false)
            => await IComparableExtension.CompareMaxAsync((T[])array, numberOfWorkers, useTPL);

        /// <summary>
        /// Asynchronously computes the maximum value within a specified sub-range of the current <see cref="PinnableArray{T}"/>.
        /// </summary>
        /// <param name="start">Start index of the sub-range.</param>
        /// <param name="length">Number of elements in the sub-range.</param>
        /// <param name="numberOfWorkers">
        /// Optional worker count (≤ <see cref="Environment.ProcessorCount"/>). Ignored when <paramref name="useTPL"/> is true.
        /// </param>
        /// <param name="useTPL">
        /// If true, uses the Task Parallel Library strategy; otherwise a custom partitioned approach.
        /// </param>
        /// <returns>A task producing the maximum value for the sub-range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> MaxAsync(int start, int length, int? numberOfWorkers = null, bool useTPL = false)
            => await IComparableExtension.CompareMaxAsync((T[])array, start, length, numberOfWorkers, useTPL);

        #endregion

        #region Min

        /// <summary>
        /// Returns the minimum value in the current <see cref="PinnableArray{T}"/>.
        /// </summary>
        /// <returns>The smallest element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Min() => Statistics.INumberExtension.Min(array.AsSpan());

        /// <summary>
        /// Returns the minimum value within the specified sub-range of the current <see cref="PinnableArray{T}"/>.
        /// </summary>
        /// <param name="start">Start index of the sub-range.</param>
        /// <param name="length">Number of elements in the sub-range.</param>
        /// <returns>The smallest element in the sub-range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Min(int start, int length) => Statistics.INumberExtension.Min(array.AsSpan(start, length));

        /// <summary>
        /// Asynchronously computes the minimum value in the current <see cref="PinnableArray{T}"/>.
        /// </summary>
        /// <param name="numberOfWorkers">
        /// Optional worker count (≤ <see cref="Environment.ProcessorCount"/>). Ignored when <paramref name="useTPL"/> is true.
        /// </param>
        /// <param name="useTPL">
        /// If true, uses the Task Parallel Library; otherwise a custom partitioned approach.
        /// </param>
        /// <returns>A task producing the minimum value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> MinAsync(int? numberOfWorkers = null, bool useTPL = false)
            => await IComparableExtension.CompareMinAsync((T[])array, numberOfWorkers, useTPL);

        /// <summary>
        /// Asynchronously computes the minimum value within a specified sub-range of the current <see cref="PinnableArray{T}"/>.
        /// </summary>
        /// <param name="start">Start index of the sub-range.</param>
        /// <param name="length">Number of elements in the sub-range.</param>
        /// <param name="numberOfWorkers">
        /// Optional worker count (≤ <see cref="Environment.ProcessorCount"/>). Ignored when <paramref name="useTPL"/> is true.
        /// </param>
        /// <param name="useTPL">
        /// If true, uses the Task Parallel Library; otherwise a custom partitioned approach.
        /// </param>
        /// <returns>A task producing the minimum value for the sub-range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> MinAsync(int start, int length, int? numberOfWorkers = null, bool useTPL = false)
            => await IComparableExtension.CompareMinAsync((T[])array, start, length, numberOfWorkers, useTPL);

        #endregion
    }
}