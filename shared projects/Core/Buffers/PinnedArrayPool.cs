using System.Numerics;

namespace Vorcyc.Mathematics.Buffers;

/// <summary>
/// Provides a high-performance pool of pinned arrays allocated on the pinned object heap (POH),
/// organized by power-of-two bucket sizes.
/// <list type="bullet">
/// <item>Suitable for scenarios that require stable memory addresses and reduced GC allocation pressure.</item>
/// <item>Follows an <c><see cref="System.Buffers.ArrayPool{T}"/> </c>-style <see cref="Rent(int)"/>/<see cref="Return(T[], bool)"/> pattern with power-of-two bucketing.</item>
/// <item>Only arrays whose length exactly matches a bucket size are cached, ensuring rented arrays always satisfy the requested minimum length.</item>
/// <item>Thread-safe for concurrent use through lock-free bucket operations.</item>
/// </list>
/// </summary>
/// <typeparam name="T">An unmanaged element type so arrays can be allocated as pinned arrays on the POH.</typeparam>
public sealed class PinnedArrayPool<T> where T : unmanaged
{

    #region Fields & Shared Instance

    /// <summary>
    /// Gets the shared global instance.
    /// </summary>
    public static readonly PinnedArrayPool<T> Shared = new();

    /// <summary>
    /// The maximum number of arrays cached per bucket.
    /// </summary>
    private const int MaxArraysPerBucket = 50;

    /// <summary>
    /// The minimum bucket size. Arrays smaller than this are not cached.
    /// </summary>
    private const int MinimumBucketSize = 16;

    /// <summary>
    /// The maximum number of larger buckets probed during rent operations.
    /// </summary>
    private const int ProbeWidth = 2;

    /// <summary>
    /// Stores buckets indexed by their power-of-two exponent.
    /// </summary>
    private readonly Bucket?[] _buckets = new Bucket?[32];

    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PinnedArrayPool{T}"/> class.
    /// </summary>
    private PinnedArrayPool() { }

    #endregion


    #region Public Methods

    /// <summary>
    /// Rents a pinned array whose length is at least <paramref name="minLength"/>.
    /// </summary>
    /// <param name="minLength">The minimum required array length. Must be greater than zero.</param>
    /// <returns>
    /// A pinned array whose length is equal to the target bucket size or a probed larger bucket size.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="minLength"/> is less than or equal to zero.
    /// </exception>
    public T[] Rent(int minLength)
    {
        if (minLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(minLength));

        int bucketIndex = GetBucketIndex(minLength);
        int bucketSize = 1 << bucketIndex;

        // 探测当前桶及少量更大桶，避免大范围扫描导致超配
        int maxIdx = Math.Min(bucketIndex + ProbeWidth, 31);
        for (int i = bucketIndex; i <= maxIdx; i++)
        {
            int size = 1 << i;
            if (_buckets[i] is { } bucket && bucket.TryTake(out var array))
            {
                // 桶中只存储"长度恰好等于桶容量"的数组
                return array;
            }
        }

        // 没有可复用的缓存 → 分配恰好为桶大小的新 pinned 数组
        return GC.AllocateUninitializedArray<T>(bucketSize, pinned: true);
    }

    /// <summary>
    /// Returns an array to the pool.
    /// </summary>
    /// <param name="array">The array to return.</param>
    /// <param name="clearArray">
    /// <see langword="true"/> to clear the array contents before caching it; otherwise, <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
    public void Return(T[] array, bool clearArray = false)
    {
        if (array is null) throw new ArgumentNullException(nameof(array));

        int length = array.Length;
        if (length < MinimumBucketSize) return;

        int bucketIndex = GetBucketIndex(length);
        int bucketSize = 1 << bucketIndex;

        // 仅缓存长度"恰好等于桶容量"的数组，保證 Rent 保證的長度不小於請求
        if (length != bucketSize) return;

        if (clearArray)
        {
            array.AsSpan().Clear();
        }

        ref var slot = ref _buckets[bucketIndex];
        var bucket = Volatile.Read(ref slot)
            ?? Interlocked.CompareExchange(ref slot, new Bucket(MaxArraysPerBucket, bucketSize), null)
            ?? Volatile.Read(ref slot)!;

        // 桶满则丢弃引用（让 GC 回收），避免内存膨胀
        bucket.TryAdd(array);
    }

    /// <summary>
    /// Removes all cached arrays from all buckets.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _buckets.Length; i++)
        {
            Volatile.Write(ref _buckets[i], null);
        }
    }

    #endregion


    #region Private Helpers

    /// <summary>
    /// Computes the bucket index for the smallest power-of-two bucket that can hold
    /// the specified <paramref name="length"/>.
    /// </summary>
    /// <param name="length">The requested length.</param>
    /// <returns>The bucket index representing a power-of-two capacity.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBucketIndex(int length)
    {
        if (length <= MinimumBucketSize)
            return 4; // 2^4 = 16

        return BitOperations.Log2(BitOperations.RoundUpToPowerOf2((uint)length));
    }

    #endregion


    #region Nested Types

    /// <summary>
    /// Represents a single bucket that caches arrays of one exact capacity.
    /// </summary>
    private sealed class Bucket
    {
        #region Fields

        /// <summary>
        /// Stores cached array references for this bucket.
        /// </summary>
        private readonly T[][] _arrays;

        /// <summary>
        /// The exact array length supported by this bucket.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        /// The current number of reserved slots.
        /// </summary>
        private int _count;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Bucket"/> class.
        /// </summary>
        /// <param name="maxCount">The maximum number of arrays that can be cached in this bucket.</param>
        /// <param name="capacity">The exact array length accepted by this bucket.</param>
        public Bucket(int maxCount, int capacity)
        {
            _arrays = new T[maxCount][];
            _capacity = capacity;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempts to add an array to this bucket.
        /// </summary>
        /// <param name="array">The array to add.</param>
        /// <returns><see langword="true"/> if the array was cached; otherwise, <see langword="false"/>.</returns>
        public bool TryAdd(T[] array)
        {
            if (array.Length != _capacity)
                return false;

            while (true)
            {
                int count = Volatile.Read(ref _count);
                if (count >= _arrays.Length) return false;

                if (Interlocked.CompareExchange(ref _count, count + 1, count) == count)
                {
                    // 预留到索引 'count'
                    Volatile.Write(ref _arrays[count], array);
                    return true;
                }
            }
        }

        /// <summary>
        /// Attempts to remove an array from this bucket.
        /// </summary>
        /// <param name="array">When this method returns, contains the rented array if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if an array was removed; otherwise, <see langword="false"/>.</returns>
        public bool TryTake(out T[] array)
        {
            while (true)
            {
                int count = Volatile.Read(ref _count);
                if (count == 0)
                {
                    array = null!;
                    return false;
                }

                int newCount = count - 1;
                if (Interlocked.CompareExchange(ref _count, newCount, count) == count)
                {
                    // Spin-wait until the slot is filled by a concurrent TryAdd
                    SpinWait spinner = default;
                    while (true)
                    {
                        array = Interlocked.Exchange(ref _arrays[newCount], null!)!;
                        if (array != null)
                            return true;
                        spinner.SpinOnce();
                    }
                }
            }
        }

        #endregion
    }

    #endregion

}