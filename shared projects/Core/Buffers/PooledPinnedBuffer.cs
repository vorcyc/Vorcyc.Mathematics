using System.Runtime.InteropServices;

namespace Vorcyc.Mathematics.Buffers;

/// <summary>
/// A growable, high-performance pinned buffer backed by <see cref="PinnedArrayPool{T}"/>.
/// Designed for long-running real-time signal acquisition (audio, sensors, etc.).
/// <para>
/// <b>Thread safety:</b> This type is not thread-safe. External synchronization is required
/// if instances are accessed from multiple threads.
/// </para>
/// <para>
/// <b>Pointer stability:</b> The <see cref="Pointer"/> value may change after any call to
/// <see cref="Append"/> that triggers a capacity expansion. Callers must re-read the pointer
/// after such operations.
/// </para>
/// </summary>
/// <typeparam name="T">An unmanaged type (e.g., float, short, byte).</typeparam>
public sealed class PooledPinnedBuffer<T> : IDisposable where T : unmanaged
{
    #region Fields

    private T[] _array;
    private int _usedLength;
    private bool _disposed;

    private readonly int _initialCapacity;
    private readonly PinnedArrayPool<T> _pool;

    #endregion

    #region Properties

    /// <summary>
    /// Gets a <see cref="Span{T}"/> over the written portion of the buffer.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public Span<T> Span
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _array.AsSpan(0, _usedLength);
        }
    }

    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> over the written portion of the buffer.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public ReadOnlySpan<T> ReadOnlySpan
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _array.AsSpan(0, _usedLength);
        }
    }

    /// <summary>
    /// Gets the number of elements that have been written to the buffer.
    /// </summary>
    public int Length => _usedLength;

    /// <summary>
    /// Gets the total number of elements the internal array can hold before resizing.
    /// Returns <c>0</c> if the buffer has been disposed.
    /// </summary>
    public int Capacity => _disposed ? 0 : _array.Length;

    /// <summary>
    /// Gets the pinned memory address of the first element in the internal array.
    /// This value may change after a call to <see cref="Append"/> that triggers a capacity expansion.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public nint Pointer
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return Marshal.UnsafeAddrOfPinnedArrayElement(_array, 0);
        }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a buffer with the specified initial capacity.
    /// </summary>
    /// <param name="initialCapacity">The minimum initial capacity of the internal array. Must be greater than zero.</param>
    /// <param name="pool">
    /// An optional <see cref="PinnedArrayPool{T}"/> instance to rent arrays from.
    /// If <see langword="null"/>, <see cref="PinnedArrayPool{T}.Shared"/> is used.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialCapacity"/> is less than or equal to zero.</exception>
    public PooledPinnedBuffer(int initialCapacity = 4096, PinnedArrayPool<T>? pool = null)
    {
        if (initialCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));

        _initialCapacity = initialCapacity;
        _pool = pool ?? PinnedArrayPool<T>.Shared;
        _array = _pool.Rent(initialCapacity);
        _usedLength = 0;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Appends data to the buffer, expanding capacity if necessary.
    /// </summary>
    /// <param name="source">The data to append. If empty, the call is a no-op.</param>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Append(ReadOnlySpan<T> source)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (source.Length == 0) return;

        int newLength = _usedLength + source.Length;

        // 如果当前数组不够大，扩容
        if (newLength > _array.Length)
        {
            // 成倍增长，至少满足新长度
            int newCapacity = Math.Max(newLength, _array.Length * 2);
            if (newCapacity < 64) newCapacity = 64;

            T[] newArray = _pool.Rent(newCapacity);

            // 拷贝已有数据
            _array.AsSpan(0, _usedLength).CopyTo(newArray);

            // 归还旧数组（池会决定是否缓存）
            _pool.Return(_array, clearArray: false);

            _array = newArray;
        }

        // 写入新数据
        source.CopyTo(_array.AsSpan(_usedLength));
        _usedLength = newLength;
    }

    /// <summary>
    /// Clears the buffer contents while retaining the current array capacity for reuse.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _usedLength = 0;
    }

    /// <summary>
    /// Returns the current array to the pool and rents a fresh small array,
    /// allowing early memory reclamation.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(_array, clearArray: false);
        _array = _pool.Rent(_initialCapacity);
        _usedLength = 0;
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Returns the internal array to the pool and marks this instance as disposed.
    /// Subsequent calls are no-ops.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _pool.Return(_array, clearArray: false);
        _array = null!;
        _usedLength = 0;
    }

    #endregion
}