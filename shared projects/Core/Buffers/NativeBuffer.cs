#define use_unsafe_code

namespace Vorcyc.Mathematics.Buffers;

using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Provides a managed wrapper for a contiguous region of unmanaged memory, supporting efficient access, pinning,
/// and manipulation of elements of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// <see cref="NativeBuffer{T}"/> allocates and manages a block of native unmanaged memory for elements of type
/// <typeparamref name="T"/>, exposing .NET memory- and span-based APIs for efficient access.
/// The buffer supports pinning, slicing, and element-wise operations, and can be constructed from arrays, spans,
/// collections, or other memory sources.
/// The buffer must be explicitly disposed to release unmanaged resources.
/// Most operations throw <see cref="ObjectDisposedException"/> after disposal.
/// This type is intended for advanced scenarios such as interop, high-performance computing, or working with native libraries.
/// </remarks>
/// <typeparam name="T">
/// The element type stored in the buffer. Must be unmanaged and implement <see cref="INumberBase{T}"/>.
/// </typeparam>
public unsafe class NativeBuffer<T> : System.Buffers.MemoryManager<T>, IPinnedBuffer<T>
    where T : unmanaged, INumberBase<T>
{

    internal void* _pBuffer;

    private int _length;

    private Memory<T> _memory;


    #region constructors

    /// <summary>
    /// Initializes the internal buffer and memory structures for the specified number of elements.
    /// </summary>
    /// <param name="length">The number of elements to allocate. Must be greater than zero.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InitCore(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        var elementSize = (nuint)Unsafe.SizeOf<T>();
        var bufferSize = (nuint)length * elementSize;
        _pBuffer = NativeMemory.Alloc(bufferSize);
        _length = length;
        _memory = base.CreateMemory(length);
    }

    /// <summary>
    /// Initializes a new instance of the NativeBuffer class with the specified length.
    /// </summary>
    /// <param name="length">The number of elements to allocate in the buffer. Must be greater than zero.</param>
    public NativeBuffer(int length) => InitCore(length);

    /// <summary>
    /// Initializes a new instance of the NativeBuffer class using the specified array segment as the source of data.
    /// </summary>
    /// <param name="segment">The array segment containing the data to copy into the buffer. The segment's array must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if the array in the specified segment is null.</exception>
    public NativeBuffer(ArraySegment<T> segment)
        : this(segment.Count)
    {
        if (segment.Array is null)
            throw new ArgumentNullException(nameof(segment), "ArraySegment 的 Array 不能为空。");
        segment.AsSpan().CopyTo(new Span<T>(_pBuffer, segment.Count));
    }

    /// <summary>
    /// Initializes a new instance of the NativeBuffer class and copies the contents of the specified span into the buffer.
    /// </summary>
    /// <remarks>The buffer is allocated with a length equal to the length of the provided span. The contents of the
    /// span are copied into the buffer upon initialization.</remarks>
    /// <param name="span">The read-only span whose contents are copied to the newly allocated buffer.</param>
    public NativeBuffer(ReadOnlySpan<T> span)
        : this(span.Length)
    {
        span.CopyTo(new Span<T>(_pBuffer, span.Length));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeBuffer{T}"/> class by copying elements from the specified collection.
    /// </summary>
    /// <remarks>
    /// If <paramref name="collection"/> implements <see cref="ICollection{T}"/>, its element count is used to allocate
    /// the buffer directly. Otherwise, the sequence is materialized into a list before copying.
    /// The order of elements in the new buffer matches the enumeration order of <paramref name="collection"/>.
    /// </remarks>
    /// <param name="collection">The source collection whose elements are copied into the new buffer. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    public NativeBuffer(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        // 优化路径：ICollection<T> 可直接获取 Count 并一次性复制
        if (collection is ICollection<T> coll)
        {
            InitCore(coll.Count);

            var destSpan = new Span<T>(_pBuffer, coll.Count);
            // 不能直接 CopyTo Span，只能自己循环
            int i = 0;
            foreach (var item in coll)
            {
                destSpan[i++] = item;
            }
        }
        else
        {
            // 回退路径：先物化为 List<T> 再复制
            var list = new List<T>(collection);
            InitCore(list.Count);

            var destSpan = new Span<T>(_pBuffer, list.Count);
            list.CopyTo(destSpan);
        }
    }

    /// <summary>
    /// Initializes a new instance of the NativeBuffer class using the specified array as the initial contents.
    /// </summary>
    /// <remarks>The buffer is initialized to the same length as the provided array, and all elements are copied.
    /// Subsequent modifications to the original array do not affect the buffer.</remarks>
    /// <param name="array">The array whose elements are copied to the buffer. Cannot be null.</param>
    public NativeBuffer(T[] array)
    {
        ArgumentNullException.ThrowIfNull(array);

        InitCore(array.Length);
        array.CopyTo(new Span<T>(_pBuffer, array.Length));
    }

    /// <summary>
    /// Initializes a new instance of the NativeBuffer class using a segment of the specified array.
    /// </summary>
    /// <param name="array">The source array from which elements are copied to initialize the buffer. Cannot be null.</param>
    /// <param name="offset">The zero-based index in the array at which to begin copying elements.</param>
    /// <param name="count">The number of elements to copy from the array starting at the specified offset. Must be non-negative and the
    /// range defined by offset and count must not exceed the length of the array.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset or count specifies a range that is outside the bounds of the array.</exception>
    public NativeBuffer(T[] array, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(array);
        if ((uint)offset > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(offset), "偏移量超出数组范围。");
        if (count < 0 || offset + count > array.Length)
            throw new ArgumentOutOfRangeException(nameof(count), "计数超出数组范围。");

        InitCore(count);
        array.AsSpan(offset, count).CopyTo(new Span<T>(_pBuffer, count));
    }

    /// <summary>
    /// Initializes a new instance of the NativeBuffer class using the specified read-only memory as the initial contents.
    /// </summary>
    /// <param name="memory">The read-only memory region whose contents are copied into the buffer. The length of the memory determines the size
    /// of the buffer.</param>
    public NativeBuffer(ReadOnlyMemory<T> memory)
        : this(memory.Length)
    {
        memory.CopyTo(_memory);
    }

    #endregion


    #region Core Properties

    /// <inheritdoc cref="IPinnedBuffer{T}.Span" />
    public Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return _memory.Span;
        }
    }

    /// <inheritdoc cref="IPinnedBuffer{T}.Memory"/>
    public override Memory<T> Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return _memory;
        }
    }

    /// <inheritdoc cref="IPinnedBuffer{T}.Length" />
    public int Length
    {
        get
        {
            ThrowIfDisposed();
            return _length;
        }
    }

    /// <inheritdoc cref="IPinnedBuffer{T}.UnmanagedPointer" />
    public T* UnmanagedPointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return (T*)_pBuffer;
        }
    }

    /// <inheritdoc cref="IPinnedBuffer{T}.ManagedPointer" />
    public ref T ManagedPointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            //return ref UnmanagedPointer[0];
            return ref Unsafe.AsRef<T>(_pBuffer);
        }
    }

    #endregion


    #region MemoryManager<T>

    /// <summary>
    /// Returns a span that represents the memory managed by this instance.
    /// </summary>
    /// <returns>A span of type T that provides access to the underlying memory buffer.</returns>
    public override Span<T> GetSpan() => new(_pBuffer, _length);

    /// <summary>
    /// Pins the memory at the specified element index, preventing the garbage collector from relocating it, and returns a
    /// handle for accessing the memory.
    /// </summary>
    /// <remarks>The returned MemoryHandle must be disposed when no longer needed to ensure proper resource
    /// management. Pinning memory is typically used for interop scenarios or when passing memory to unmanaged
    /// code.</remarks>
    /// <param name="elementIndex">The zero-based index of the element to pin. Defaults to 0. Must be greater than or equal to 0 and less than or equal
    /// to the length of the memory region.</param>
    /// <returns>A MemoryHandle that provides access to the pinned memory at the specified element index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when elementIndex is less than 0 or greater than the length of the memory region.</exception>
    public override MemoryHandle Pin(int elementIndex = 0)
    {
        ThrowIfDisposed();
        if ((uint)elementIndex > (uint)_length)
            throw new ArgumentOutOfRangeException(nameof(elementIndex));

        // 计算偏移指针
        T* ptr = UnmanagedPointer + elementIndex;

        // 第二个参数为 GCHandle，这里不需要，传 default；第三个参数指定我们自己管理 lifetime
        return new MemoryHandle(ptr, default, this);
    }

    /// <summary>
    ///  <see cref="MemoryManager{T}.Unpin"/> member . Not used.
    /// </summary>
    public override void Unpin() { }


    #endregion


    #region Indexer

    /// <inheritdoc cref="IPinnedBuffer{T}.this[int]" />
    public T this[int index]
    {
        get
        {
            ThrowIfDisposed();
            if (index < 0 || index >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return UnmanagedPointer[index];
        }
        set
        {
            ThrowIfDisposed();
            if (index < 0 || index >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            UnmanagedPointer[index] = value;
        }
    }

    /// <inheritdoc cref="IPinnedBuffer{T}.this[int,int]" />
    public Span<T> this[int startIndex, int length]
    {
        get
        {
            ThrowIfDisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(length);

            if (startIndex < 0 || startIndex >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (startIndex + length > _length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return new(UnmanagedPointer + startIndex, length);
        }
    }


    //public Span<T> this[Range range]
    //{
    //    get
    //    {
    //        var (offset, length) = range.GetOffsetAndLength(_length);
    //        return new(UnmanagedPointer + offset, length);
    //    }
    //}

    /// <inheritdoc cref="IPinnedBuffer{T}.this[System.Range]" />
    public Span<T> this[Range range]
    {
        get
        {
            ThrowIfDisposed();
            return Span[range];
        }
    }

    #endregion


    #region Type Conversion

    /// <summary>
    /// Defines an implicit conversion from an array of <typeparamref name="T"/> to a new <see cref="NativeBuffer{T}"/> instance.
    /// </summary>
    /// <remarks>
    /// This operator allows a managed array to be used where a <see cref="NativeBuffer{T}"/> is expected.
    /// The resulting buffer contains a copy of the elements in <paramref name="array"/>.
    /// </remarks>
    /// <param name="array">The source array to convert. Cannot be <see langword="null"/>.</param>
    /// <returns>
    /// A new <see cref="NativeBuffer{T}"/> containing a copy of the elements from <paramref name="array"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    public static implicit operator NativeBuffer<T>(T[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        return new NativeBuffer<T>(array);
    }

    /// <summary>
    /// Defines an implicit conversion from a <see cref="NativeBuffer{T}"/> to a <see cref="Span{T}"/> that provides access to the buffer's
    /// elements.
    /// </summary>
    /// <remarks>This operator enables seamless use of <see cref="NativeBuffer{T}"/> instances in APIs that accept <see cref="Span{T}"/>
    /// parameters. The returned <see cref="Span{T}"/> reflects the current contents of the buffer.</remarks>
    /// <param name="buffer">The <see cref="NativeBuffer{T}"/> instance to convert to a <see cref="Span{T}"/>. Cannot be null.</param>
    public static implicit operator Span<T>(NativeBuffer<T> buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        return buffer.Span;
    }

    #endregion


    #region Concat & operators

    /// <summary>
    /// 连接两个缓冲区，返回一个包含两者全部元素的新 <see cref="NativeBuffer{T}"/>。
    /// </summary>
    /// <param name="left">左侧缓冲区，不能为空。</param>
    /// <param name="right">右侧缓冲区，不能为空。</param>
    /// <returns>新的缓冲区，其内容依次包含 <paramref name="left"/> 与 <paramref name="right"/> 的所有元素。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="left"/> 或 <paramref name="right"/> 为 null 时抛出。</exception>
    // Shared implementation used by both operator overloads.
    private static NativeBuffer<T> Concat(IPinnedBuffer<T> left, IPinnedBuffer<T> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        // concatenate: allocate sum of lengths and copy each buffer in order
        var result = new NativeBuffer<T>(left.Length + right.Length);

        var dest1 = new Span<T>(result._pBuffer, left.Length);
        var dest2 = new Span<T>(result.UnmanagedPointer + left.Length, right.Length);

        left.Span.CopyTo(dest1);
        right.Span.CopyTo(dest2);

        return result;
    }

    /// <summary>
    /// Concatenates two native buffers into a new buffer containing the elements of both operands.
    /// </summary>
    /// <remarks>The resulting buffer contains copies of the elements from both input buffers in order. The
    /// original buffers are not modified.</remarks>
    /// <param name="left">The first native buffer to concatenate.</param>
    /// <param name="right">The second native buffer to concatenate.</param>
    /// <returns>A new native buffer containing the elements of both the left and right buffers, with the elements of the right
    /// buffer appended to those of the left.</returns>
    public static NativeBuffer<T> operator +(NativeBuffer<T> left, NativeBuffer<T> right) => Concat(left, right);

    /// <inheritdoc cref="IPinnedBuffer{TNumber}.op_Addition(IPinnedBuffer{TNumber}, IPinnedBuffer{TNumber})" />
    //static IPinnedBuffer<T> IPinnedBuffer<T>.operator +(IPinnedBuffer<T> left, IPinnedBuffer<T> right) => Concat(left, right);

    #endregion


    #region Element-wise operations

    /// <inheritdoc cref="IPinnedBuffer{TNumber}.Clear" />
    public void Clear()
    {
        ThrowIfDisposed();
        //System.Runtime.CompilerServices.Unsafe.InitBlock(_buffer, 0, (uint)(_length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>()));
        Span.Clear();
    }

    /// <inheritdoc cref="IPinnedBuffer{TNumber}.Fill(TNumber)" />
    public void Fill(T value)
    {
        ThrowIfDisposed();
        Span.Fill(value);
    }

    /// <inheritdoc cref="IPinnedBuffer{TNumber}.Fill(TNumber,TNumber)" />
    public void Fill(T startValue, T step)
    {
        ThrowIfDisposed();
        var span = Span;
        T current = startValue;
        for (int i = 0; i < Span.Length; i++)
        {
            span[i] = current;
            current += step;
        }
    }

    /// <inheritdoc cref="IPinnedBuffer{TNumber}.FillWith{TOther}(TOther)" />
    public void FillWith<TOther>(TOther number)
        where TOther : unmanaged, INumberBase<TOther>
    {
        ThrowIfDisposed();
        /*
        * Marshal.SizeOf<bool>() == 4, Unsafe.SizeOf<bool>() == 1
        */

        var size = Unsafe.SizeOf<T>();
        var numberSize = Unsafe.SizeOf<TOther>();

        if (numberSize > size)
        {
            var count = size * _length / numberSize;
#if use_unsafe_code
            TOther* pDest = (TOther*)UnmanagedPointer;
            for (int i = 0; i < count; i++)
            {
                *(pDest + i) = number;
            }
#else
            ref var addr = ref Unsafe.As<T, TOther>(ref ManagedPointer);
            for (int i = 0; i < count; i++)
            {
                addr = number;
                addr = ref Unsafe.Add(ref addr, 1);
            }
#endif
        }
        else if (numberSize == size)
        {
            if (number is T n)
                Fill(n);
            else
            {
#if use_unsafe_code
                TOther* pDest = (TOther*)UnmanagedPointer;
                for (int i = 0; i < _length; i++)
                {
                    *(pDest + i) = number;
                }
#else
                ref var addr = ref Unsafe.As<T, TOther>(ref ManagedPointer);
                for (int i = 0; i < _length; i++)
                {
                    addr = number;
                    addr = ref Unsafe.Add(ref addr, 1);
                }
#endif
            }

        }
        else // numberSize < size
        {
#if use_unsafe_code
            TOther* pOtherNumber = &number;
            for (int i = 0; i < _length; i++)
            {
                *(UnmanagedPointer + i) = *(T*)pOtherNumber;
            }
#else
            var num = Unsafe.As<TOther, T>(ref number);
            for (int i = 0; i < _length; i++)
            {
                UnmanagedPointer[i] = num;
            }
#endif
        }
    }

    #endregion


    #region Reallocate    

    /// <inheritdoc cref="IPinnedBuffer{TNumber}.Reallocate(int)" />
    public void Reallocate(int newLength)
    {
        ThrowIfDisposed();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newLength);
        if (newLength == _length)
            return;

        var newSize = (nuint)newLength * (nuint)Unsafe.SizeOf<T>();
        var newBuffer = NativeMemory.Realloc(_pBuffer, newSize);
        _pBuffer = newBuffer;
        _length = newLength;
        _memory = base.CreateMemory(newLength);
    }

    #endregion


    #region Overrides

    /// <summary>
    /// Returns a string that represents the current collection, displaying up to the first 50 items followed by the total
    /// item count.
    /// </summary>
    /// <remarks>If the collection contains more than 50 items, only the first 50 are shown in the string, followed by
    /// an ellipsis and the total item count. This method is intended for debugging or display purposes and does not provide
    /// a complete serialization of the collection's contents.</remarks>
    /// <returns>A string representation of the collection, including up to 50 items and the total number of items.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        ThrowIfDisposed();
        const int Max = 50;
        var sb = new System.Text.StringBuilder("[");
        int count = Math.Min(_length, Max);
        for (int i = 0; i < count; i++)
        {
            sb.Append(this[i]);
            if (i < count - 1) sb.Append(", ");
        }
        if (_length > Max) sb.Append("... ");
        sb.Append($"({_length} items)]");
        return sb.ToString();
    }

    #endregion


    #region IDisposable

    private volatile bool _disposed;

    /// <summary>
    /// Releases the unmanaged memory held by this buffer.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources (called from finalizer).
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;

        NativeMemory.Free(_pBuffer);
        _pBuffer = null;
        _length = 0;
        _memory = default;
        _disposed = true;
    }

    /// <summary>
    /// Releases unmanaged resources if <see cref="Dispose()"/> was not called explicitly.
    /// </summary>
    ~NativeBuffer() => Dispose(disposing: false);

    /// <summary>
    /// Releases all resources used by this buffer.
    /// After calling this method, most operations will throw <see cref="ObjectDisposedException"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(NativeBuffer<>));
    }

    /// <inheritdoc cref="IPinnedBuffer{T}.IsDisposed" />
    public bool IsDisposed => _disposed;

    #endregion
}