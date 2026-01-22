#define use_unsafe_code

namespace Vorcyc.Mathematics.Buffers;

/* duan linli aka cyclone_dll
 * 
 * 2026 年 2 月 28 日
 * 于昆明初次完成修订
 * 最早叫 PinnedNumbers 
 * 现在改进了。还加入了租赁模式（待实现）
 */

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Provides a pinned, heap-allocated buffer for unmanaged values that supports direct memory access, efficient element
/// operations, and safe disposal. The buffer is allocated in the .NET pinned object heap (POH) and exposes both managed
/// and unmanaged pointers for advanced scenarios.
/// </summary>
/// <remarks>  <see cref="POHBuffer{T}"/> is designed for scenarios that require a fixed, non-relocating buffer for interop,
/// high-performance memory operations, or advanced numerical processing. The buffer is pinned for its lifetime,
/// ensuring that its memory address remains constant and suitable for native interop or unsafe code. The class
/// implements <see cref="IPinnedBuffer{T}"/> and provides span-based access, pointer access, and element-wise operations. Call
/// Dispose when the buffer is no longer needed to release resources. After disposal, accessing members will throw an
/// ObjectDisposedException.</remarks>
/// <typeparam name="T">The type of elements stored in the buffer. Must be unmanaged and implement <see cref="INumberBase{T}"/> .</typeparam>
public unsafe class POHBuffer<T> : IPinnedBuffer<T>
    where T : unmanaged, INumberBase<T>
{

    internal T[]? _buffer;

    private int _length;

    private Memory<T> _memory;


    #region constructors

    /// <summary>
    /// Initializes the internal buffer and memory structures with the specified length.
    /// </summary>
    /// <param name="length">The number of elements to allocate for the buffer. Must be greater than zero.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InitCore(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        _buffer = GC.AllocateUninitializedArray<T>(length, pinned: true);
        _length = length;
        _memory = new Memory<T>(_buffer, 0, length);
    }

    /// <summary>
    /// Initializes a new instance of the POHBuffer class with the specified buffer length.
    /// </summary>
    /// <param name="length">The length of the buffer to allocate. Must be a non-negative integer.</param>
    public POHBuffer(int length) => InitCore(length);

    /// <summary>
    /// Initializes a new instance of the POHBuffer class using the specified array segment as the initial buffer contents.
    /// </summary>
    /// <param name="segment">The array segment whose contents are copied into the buffer. The segment's array must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if the array in the specified segment is null.</exception>
    public POHBuffer(ArraySegment<T> segment)
        : this(segment.Count)
    {
        if (segment.Array is null)
            throw new ArgumentNullException(nameof(segment), "ArraySegment 的 Array 不能为空。");
        segment.CopyTo(_buffer!);
    }

    /// <summary>
    /// Initializes a new instance of the POHBuffer class using the specified span as the initial contents.
    /// </summary>
    /// <remarks>The length of the buffer is set to match the length of the provided span. The contents of the span
    /// are copied into the buffer upon initialization.</remarks>
    /// <param name="span">The span whose contents are copied to initialize the buffer.</param>
    public POHBuffer(ReadOnlySpan<T> span)
    {
        InitCore(span.Length);
        span.CopyTo(_buffer);
    }

    /// <summary>
    /// Initializes a new instance of the POHBuffer class that contains elements copied from the specified collection.
    /// </summary>
    /// <remarks>If the specified collection implements <see cref="ICollection{T}"/>, the elements are copied directly for improved
    /// performance. Otherwise, the collection is first materialized as a list before copying. The resulting buffer will
    /// have the same number of elements as the source collection.</remarks>
    /// <param name="collection">The collection whose elements are copied to the new buffer. Cannot be null.</param>
    public POHBuffer(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        // 优化路径：ICollection<T> 可直接获取 Count 并一次性复制
        if (collection is ICollection<T> coll)
        {
            InitCore(coll.Count);
            coll.CopyTo(_buffer!, 0);
        }
        else
        {
            // 回退路径：先物化为 List<T> 再复制
            var list = new List<T>(collection);
            InitCore(list.Count);
            list.CopyTo(_buffer!);
        }
    }

    /// <summary>
    /// Initializes a new instance of the POHBuffer class using the specified array as the initial contents.
    /// </summary>
    /// <param name="array">The array whose elements are copied to initialize the buffer. Cannot be null.</param>
    public POHBuffer(T[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        InitCore(array.Length);
        array.AsSpan().CopyTo(_buffer);
    }

    /// <summary>
    /// Initializes a new instance of the POHBuffer class using a segment of the specified array.
    /// </summary>
    /// <param name="array">The array from which elements are copied to initialize the buffer. Cannot be null.</param>
    /// <param name="offset">The zero-based index in the array at which the segment to copy begins. Must be within the bounds of the array.</param>
    /// <param name="count">The number of elements to copy from the array. Must be non-negative and the range defined by offset and count
    /// must not exceed the length of the array.</param>
    /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if offset is outside the bounds of the array, or if count is negative or the range defined by offset and
    /// count exceeds the length of the array.</exception>
    public POHBuffer(T[] array, int offset, int count)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array));
        if ((uint)offset > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(offset), "偏移量超出数组范围。");
        if (count < 0 || offset + count > array.Length)
            throw new ArgumentOutOfRangeException(nameof(count), "计数超出数组范围。");

        InitCore(count);
        array.AsSpan(offset, count).CopyTo(_buffer);
    }

    /// <summary>
    /// Initializes a new instance of the POHBuffer class using the specified read-only memory segment.
    /// </summary>
    /// <param name="memory">The read-only memory segment containing the data to be used for the buffer. The segment must not be empty.</param>
    public POHBuffer(ReadOnlyMemory<T> memory)
        : this(memory.Span)
    {
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

    /// <inheritdoc cref="IPinnedBuffer{T}.Memory" />
    public Memory<T> Memory
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
            //return (T*)Marshal.UnsafeAddrOfPinnedArrayElement<T>(_buffer!, 0);
            //据说上面这个因为 P/Invoke 开销很大
            ref T first = ref MemoryMarshal.GetArrayDataReference(_buffer!);
            return (T*)Unsafe.AsPointer(ref first);
        }
    }

    /// <inheritdoc cref="IPinnedBuffer{T}.ManagedPointer" />
    public ref T ManagedPointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            //return ref UnmanagedPointer[0]; 最早写法！不对！不要交错依赖
            return ref MemoryMarshal.GetArrayDataReference<T>(_buffer!);
            //return ref _buffer![0];
        }
    }

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
            return _memory.Span[range];
        }
    }

    #endregion


    #region Type Conversion

    /// <summary>
    /// Defines an implicit conversion from an array of type <typeparamref name="T"/> to a new <see cref="POHBuffer{T}"/> instance.
    /// </summary>
    /// <param name="array">The source array to convert. Cannot be <see langword="null"/>.</param>
    /// <returns>
    /// A new <see cref="POHBuffer{T}"/> containing a copy of the elements in <paramref name="array"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="array"/> is <see langword="null"/>.
    public static implicit operator POHBuffer<T>(T[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        return new POHBuffer<T>(array);
    }


    /// <summary>
    /// Defines an implicit conversion from a <see cref="POHBuffer{T}"/> to a <see cref="Span{T}"/> that provides access to the buffer's values.
    /// </summary>
    /// <remarks>This operator enables seamless use of <see cref="POHBuffer{T}"/> instances in APIs that accept <see cref="Span{T}"/>
    /// parameters. The returned span reflects the current contents of the buffer.</remarks>
    /// <param name="buffer">The <see cref="POHBuffer{T}"/> instance to convert to a <see cref="Span{T}"/>. Cannot be null.</param>
    public static implicit operator Span<T> (POHBuffer<T> buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        return buffer.Span;
    }

    #endregion


    #region Concat & operators

    /// <summary>
    /// 连接两个固定缓冲区，生成一个新的 <see cref="POHBuffer{T}"/>，
    /// 其内容依次包含左缓冲区和右缓冲区的所有元素。
    /// </summary>
    /// <param name="left">左侧缓冲区，不能为空。</param>
    /// <param name="right">右侧缓冲区，不能为空。</param>
    /// <returns>新的缓冲区实例，包含拼接后的数据。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="left"/> 或 <paramref name="right"/> 为 null 时抛出。</exception>
    // Shared implementation used by both operator overloads.
    private static POHBuffer<T> Concat(IPinnedBuffer<T> left, IPinnedBuffer<T> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        // concatenate: allocate sum of lengths and copy each buffer in order
        var result = new POHBuffer<T>(left.Length + right.Length);

        var dest1 = new Span<T>(result._buffer, 0, left.Length);
        var dest2 = new Span<T>(result.UnmanagedPointer + left.Length, right.Length);

        left.Span.CopyTo(dest1);
        right.Span.CopyTo(dest2);

        return result;
    }

    /// <summary>
    /// Combines two <see cref="POHBuffer{T}"/> instances into a single buffer containing the elements of both.
    /// </summary>
    /// <remarks>The resulting buffer contains all elements from the left buffer followed by all elements from the
    /// right buffer. Neither input buffer is modified.</remarks>
    /// <param name="left">The first buffer whose elements will appear first in the resulting buffer.</param>
    /// <param name="right">The second buffer whose elements will appear after those of the first buffer in the resulting buffer.</param>
    /// <returns>A new <see cref="POHBuffer{T}"/> containing the concatenated elements of the two input buffers.</returns>
    public static POHBuffer<T> operator +(POHBuffer<T> left, POHBuffer<T> right) => Concat(left, right);

    ///// <inheritdoc cref="IPinnedBuffer{TNumber}.op_Addition(IPinnedBuffer{TNumber}, IPinnedBuffer{TNumber})" />
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

        var newBuffer = GC.AllocateUninitializedArray<T>(newLength, pinned: true);

        var copyLength = Math.Min(_length, newLength);
        new Span<T>(_buffer, 0, copyLength).CopyTo(new Span<T>(newBuffer, 0, copyLength));

        _buffer = newBuffer;
        _length = newLength;
        _memory = new Memory<T>(_buffer, 0, newLength);
    }

    #endregion


    #region Overrides

    /// <summary>
    /// 返回缓冲区内容的字符串表示形式。
    /// 最多显示前 50 个元素，并在结尾附带总元素数量。
    /// </summary>
    /// <returns>表示缓冲区内容的字符串。</returns>
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


    private bool _disposed;

    /// <summary>
    /// Releases all resources used by the current instance of the class.
    /// </summary>
    /// <remarks>Call this method when you are finished using the object to free unmanaged resources and
    /// perform other cleanup operations. After calling this method, the object should not be used.</remarks>
    public void Dispose()
    {
        if (_disposed) return;
        _buffer = null;
        _length = 0;
        _memory = default;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 当缓冲区已被释放或内部数组为 null 时抛出 <see cref="ObjectDisposedException"/>。
    /// 在公开成员访问前用于防御性检查对象状态。
    /// </summary>
    /// <exception cref="ObjectDisposedException">当缓冲区已释放或内部缓冲数组为 null 时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(this._disposed, nameof(POHBuffer<>));
        ObjectDisposedException.ThrowIf(this._buffer is null, nameof(POHBuffer<>));
    }

    /// <inheritdoc cref="IPinnedBuffer{T}.IsDisposed" />
    public bool IsDisposed => _disposed;

    #endregion

}