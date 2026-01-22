namespace Vorcyc.Mathematics.Buffers;

using System.Buffers;
using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


/* duan linli aka cyclone_dll
 * 
 * ‎2022‎年‎1‎月‎10‎日 
 * 于昆明初次完成修订
 * 最早叫 PinnedNumbers 
 * 现在改进了。还加入了租赁模式。
 * 2026.3.14
 * 完善、修复并参照 POHBuffer和NativeBuffer统一了　API设计
 * 
 */

/// <summary>
/// Provides a managed array of value type elements that can be optionally pinned in memory for interop or unsafe
/// operations, with support for leasing, pooling, and efficient element-wise operations.
/// </summary>
/// <remarks><see cref="PinnableArray{T}"/> enables scenarios that require stable memory addresses, such as interop with
/// unmanaged code or use in unsafe contexts. The array can be allocated from a pool or as a new array, depending on
/// configuration options. Pinning the array prevents the garbage collector from moving it, making the memory address
/// stable for the duration of the pin. The class supports a variety of initialization and filling patterns, as well as
/// efficient access to the underlying memory via spans, memory buffers, and pointers. Dispose the instance when
/// finished to release resources, especially when using leasing mode.</remarks>
/// <typeparam name="T">The value type of the elements in the array. Must be unmanaged and implement <see cref="INumberBase{TSelf}"/>.</typeparam>
public class PinnableArray<T> : IDisposable, IEnumerable<T>
    where T : unmanaged, INumberBase<T>
{

    private readonly T[] _array;

    private readonly int _length;

    private bool _isPinned;

    private Memory<T> _memory;

    private MemoryHandle _memoryHandle;

    private ArrayPool<T> _pool = ArrayPool<T>.Create();


    #region static prop/methods

    private static PinnableArrayOption s_options = new();

    /// <summary>
    /// Global options for all <see cref="PinnableArray{T}"/> instances of this element type.
    /// </summary>
    /// <remarks>
    /// Configure before allocating instances to affect allocation/leasing behavior.
    /// </remarks>
    public static PinnableArrayOption Options => s_options;


    /// <summary>
    /// Merges multiple <see cref="PinnableArray{T}"/> instances into a new one by concatenating them in order.
    /// </summary>
    /// <param name="arrays">Sequence of arrays to merge. Cannot be null, and cannot contain null elements.</param>
    /// <returns>
    /// A new <see cref="PinnableArray{T}"/> whose contents are all input arrays concatenated in enumeration order.
    /// If the sequence is empty, an empty-length instance is returned.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="arrays"/> is null or contains a null element.
    /// </exception>
    public static PinnableArray<T> Merge(IEnumerable<PinnableArray<T>> arrays)
    {
        ArgumentNullException.ThrowIfNull(arrays, nameof(arrays));

        // Materialize to avoid multiple enumeration and to know the count up-front.
        var list = arrays as IList<PinnableArray<T>> ?? arrays.ToList();
        if (list.Count == 0)
        {
            // Allow empty merge; caller can decide how to interpret it.
            return new PinnableArray<T>(0, pin: false);
        }

        var totalLength = 0;
        foreach (var arr in list)
        {
            if (arr is null)
                throw new ArgumentNullException(nameof(arrays), "Sequence must not contain null elements.");

            totalLength += arr.Length;
        }

        // Do not pin in constructor; we only need to pin if caller cares about UnmanagedPointer.
        var result = new PinnableArray<T>(totalLength, pin: false);
        var dest = result.Span;

        var offset = 0;
        foreach (var arr in list)
        {
            var src = arr.Span;
            src.CopyTo(dest.Slice(offset, src.Length));
            offset += src.Length;
        }

        // Keep behavior simple: do not auto-pin; caller can call result.Pin() when needed.
        return result;
    }


    #endregion


    #region pin and unpin

    /// <summary>
    /// Pins this <see cref="PinnableArray{T}"/> so you can get its stable memory address for interop.
    /// </summary>
    public void Pin()
    {
        ThrowIfDisposed();
        if (_isPinned) return;
        _memoryHandle = _memory.Pin();
        _isPinned = true;
    }

    /// <summary>
    /// Unpins this <see cref="PinnableArray{T}"/> so it can be moved or collected by the GC.
    /// </summary>
    public void Unpin()
    {
        ThrowIfDisposed();
        if (_isPinned)
        {
            _memoryHandle.Dispose();
            _isPinned = false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the item is pinned.
    /// </summary>
    public bool IsPinned => _isPinned;

    #endregion


    #region constructors

    //private void InitCore(int length, bool pin = true)
    //{
    //    _length = length;
    //    if (Options.UseLeasingMode)
    //    {
    //        _array = _pool.Rent(length);
    //    }
    //    else
    //    {
    //        _array = new T[length];
    //    }
    //    _memory = new Memory<T>(_array, 0, length);
    //}

    /// <summary>
    /// Initializes a new instance of the PinnableArray class with the specified length and optional pinning behavior.
    /// </summary>
    /// <remarks>If pinning is enabled, the array is pinned immediately after allocation, which can be useful
    /// for scenarios requiring fixed memory locations. The array may be allocated from a pool or as a new array,
    /// depending on configuration options.</remarks>
    /// <param name="length">The number of elements in the array. Must be greater than zero.</param>
    /// <param name="pin">A value indicating whether the array should be pinned in memory upon creation. The default is <see
    /// langword="true"/>.</param>
    public PinnableArray(int length, bool pin = true)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1, nameof(length));
        _length = length;
        if (Options.UseLeasingMode)
            _array = _pool.Rent(length);
        else
            _array = new T[length];
        _memory = new Memory<T>(_array, 0, length);
        if (pin) Pin();
    }

    /// <summary>
    /// Initializes a new instance of the PinnableArray class using the specified read-only memory segment.
    /// </summary>
    /// <param name="memory">The read-only memory segment whose contents are copied to the array.</param>
    /// <param name="pin">A value indicating whether the array should be pinned in memory. If <see langword="true"/>, the array is pinned;
    /// otherwise, it is not.</param>
    public PinnableArray(ReadOnlyMemory<T> memory, bool pin = true)
        : this(memory.Length, pin)
    {
        memory.CopyTo(_memory);
    }

    /// <summary>
    /// Initializes a new instance of the PinnableArray class using the specified read-only span and optionally pins the
    /// underlying memory.
    /// </summary>
    /// <remarks>If pinning is enabled, the underlying memory is pinned to prevent it from being moved by the
    /// garbage collector. This is useful for scenarios requiring stable memory addresses, such as interop or unsafe
    /// code.</remarks>
    /// <param name="span">A read-only span containing the elements to copy into the array.</param>
    /// <param name="pin">A value indicating whether the underlying memory should be pinned. The default is <see langword="true"/>.</param>
    public PinnableArray(ReadOnlySpan<T> span, bool pin = true)
        : this(span.Length, pin)
    {
        span.CopyTo(_memory.Span);
    }

    /// <summary>
    /// Initializes a new instance of the PinnableArray class using the specified array segment.
    /// </summary>
    /// <remarks>The contents of the provided segment are copied into the internal memory of the
    /// PinnableArray. Pinning the memory can be useful when interoperability with unmanaged code is required.</remarks>
    /// <param name="segment">The segment of the array to be used for the PinnableArray. The segment defines the portion of the array that
    /// will be copied into the internal memory. The segment's Count determines the size of the PinnableArray.</param>
    /// <param name="pin">A value indicating whether the internal memory should be pinned. If set to <see langword="true"/>, the memory
    /// will be pinned; otherwise, it will not be pinned.</param>
    public PinnableArray(ArraySegment<T> segment, bool pin = true)
        : this(segment.Count, pin)
    {
        segment.AsSpan().CopyTo(_memory.Span);
    }

    /// <summary>
    /// Initializes a new instance of the PinnableArray class using the specified array and optionally pins the
    /// underlying memory.
    /// </summary>
    /// <param name="array">The array whose contents are used to initialize the PinnableArray. Cannot be null.</param>
    /// <param name="pin">A value indicating whether the underlying memory should be pinned. The default is <see langword="true"/>.</param>
    public PinnableArray(T[] array, bool pin = true)
        : this(array.Length, pin)
    {
        ArgumentNullException.ThrowIfNull(array, nameof(array));
        array.CopyTo(_memory);
    }

    /// <summary>
    /// Initializes a new instance of the PinnableArray class by copying a specified range from the provided array,
    /// optionally pinning the memory for use in unsafe or interop scenarios.
    /// </summary>
    /// <remarks>If pinning is enabled, the memory is pinned immediately upon construction, which is useful
    /// for scenarios involving unmanaged code or fixed memory access. The constructor supports leasing mode for memory
    /// allocation if enabled in options, which may improve performance for repeated allocations.</remarks>
    /// <param name="array">The source array from which elements are copied.</param>
    /// <param name="offset">The zero-based index in the source array at which to begin copying elements. Must be greater than or equal to 0.</param>
    /// <param name="length">The number of elements to copy from the source array. Must be at least 1, and the range defined by offset and
    /// length must be valid within the array.</param>
    /// <param name="pin">A value indicating whether the underlying memory should be pinned. If <see langword="true"/>, the memory is
    /// pinned for use in scenarios requiring fixed memory addresses.</param>
    /// <exception cref="ArgumentException">Thrown if the combination of offset and length does not specify a valid range within the array.</exception>
    public PinnableArray(T[] array, int offset, int length, bool pin = true)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0, nameof(offset));
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1, nameof(length));
        if (offset + length > array.Length)
            throw new ArgumentException("Offset and length must specify a valid range within the array.", nameof(length));
        _length = length;
        if (Options.UseLeasingMode)
            _array = _pool.Rent(length);
        else
            _array = new T[length];
        _memory = new Memory<T>(_array, 0, length);
        new ReadOnlySpan<T>(array, offset, length).CopyTo(_memory.Span);
        if (pin) Pin();
    }


    #endregion


    #region properties    

    /// <summary>
    /// Gets the logical element count.
    /// </summary>
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return _length;
        }
    }

    /// <summary>
    /// Gets a pointer to the underlying unmanaged memory buffer, or null if the memory is not pinned.
    /// </summary>
    /// <remarks>Use this property to access the raw memory address for interop or unsafe operations. The
    /// returned pointer is valid only while the memory is pinned and the object is not disposed. Accessing this
    /// property after disposal may result in undefined behavior.</remarks>
    public unsafe T* UnmanagedPointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return _isPinned ? (T*)_memoryHandle.Pointer : null;
        }
    }

    /// <summary>
    /// Gets a managed reference to the first element of the underlying array.
    /// </summary>
    /// <remarks>Use this property to access or modify the array's contents directly. The reference is valid
    /// as long as the array remains allocated and unmodified. This property is intended for advanced scenarios where
    /// direct memory access is required.</remarks>
    public ref T ManagedPointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return ref MemoryMarshal.GetArrayDataReference(_array);
        }
    }

    /// <summary>
    /// Gets a span that provides direct access to the underlying values in the memory buffer.
    /// </summary>
    /// <remarks>The returned span reflects the current state of the buffer. Modifying the span will modify the
    /// underlying data. The span is only valid while the parent object is not disposed.</remarks>
    public Span<T> Span
    {
        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return _memory.Span;
        }
    }

    /// <summary>
    /// Gets the underlying array of values contained in the collection.
    /// </summary>
    public T[] Values
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get
        {
            ThrowIfDisposed();
            return _array;
        }
    }

    /// <summary>
    /// Gets a memory buffer that represents the underlying storage of this instance.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="Memory{T}"/> provides managed, bounds-checked access to the data.
    /// It is only valid while this object is not disposed. Modifying the memory buffer updates
    /// the underlying array.
    /// </remarks>
    public Memory<T> Memory
    {
        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return _memory;
        }
    }

    #endregion


    #region IEnumerable, IEnumerable<T> members

    /// <summary>
    /// Returns a non-generic enumerator for the underlying array.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();

    /// <summary>
    /// Returns a generic enumerator over the elements.
    /// </summary>
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

    #endregion


    #region Indexer

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Length"/>.
    /// </exception>
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            if (index < 0 || index >= _length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _array[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            ThrowIfDisposed();
            if (index < 0 || index >= _length)
                throw new ArgumentOutOfRangeException(nameof(index));
            _array[index] = value;
        }
    }

    /// <summary>
    /// Gets a contiguous region of the buffer as a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="startIndex">The zero-based starting index of the region.</param>
    /// <param name="length">The number of elements to include in the region.</param>
    /// <returns>
    /// A <see cref="Span{T}"/> that represents the specified range of elements within the buffer.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="startIndex"/> is less than 0, <paramref name="length"/> is less than 0,
    /// or <paramref name="startIndex"/> plus <paramref name="length"/> exceeds <see cref="Length"/>.
    /// </exception>
    public Span<T> this[int startIndex, int length]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            if (startIndex < 0 || length < 0 || startIndex + length > _length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            return _array.AsSpan(startIndex, length);
        }
    }

    /// <summary>
    /// Gets a contiguous region of the buffer as a <see cref="Span{T}"/> using a range.
    /// </summary>
    /// <param name="range">
    /// The range that specifies the start and end of the region to access.
    /// Indices are relative to <see cref="Length"/>, and may use the ^ operator from the end.
    /// </param>
    /// <returns>
    /// A <see cref="Span{T}"/> that represents the specified range of elements within the buffer.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the evaluated range falls outside the bounds of the buffer.
    /// </exception>
    public Span<T> this[Range range]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            var (offset, length) = range.GetOffsetAndLength(_length);
            return _array.AsSpan(offset, length);
        }
    }

    #endregion


    #region Type Conversion

    /// <summary>
    /// Defines an implicit conversion from an array of type <typeparamref name="T"/> to a new <see cref="PinnableArray{T}"/> instance.
    /// </summary>
    /// <param name="array">The source array to convert. Cannot be <see langword="null"/>.</param>
    /// <returns>
    /// A new <see cref="PinnableArray{T}"/> containing a copy of the elements in <paramref name="array"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="array"/> is <see langword="null"/>.
    public static implicit operator PinnableArray<T>(T[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        return new PinnableArray<T>(array);
    }

    /// <summary>
    /// Defines an implicit conversion from a <see cref="PinnableArray{T}"/> to a T[] array.
    /// </summary>
    /// <remarks>This operator enables seamless use of <see cref="PinnableArray{T}"/> instances where a T[] array is
    /// expected. If the <see cref="PinnableArray{T}"/> is null, an ArgumentNullException is thrown.</remarks>
    /// <param name="pinnableArray">The <see cref="PinnableArray{T}"/> instance to convert. Cannot be null.</param>
    public static implicit operator T[](PinnableArray<T> pinnableArray)
    {
        ArgumentNullException.ThrowIfNull(pinnableArray);
        pinnableArray.ThrowIfDisposed();
        return pinnableArray._array;
    }

    /// <summary>
    /// Defines an implicit conversion from a <see cref="PinnableArray{T}"/> to a <see cref="Span{T}"/> that provides access to the buffer's values.
    /// </summary>
    /// <remarks>This operator enables seamless use of <see cref="PinnableArray{T}"/> instances in APIs that accept <see cref="Span{T}"/>
    /// parameters. The returned span reflects the current contents of the buffer.</remarks>
    /// <param name="pinnableArray">The <see cref="PinnableArray{T}"/> instance to convert to a <see cref="Span{T}"/>. Cannot be null.</param>
    public static implicit operator Span<T>(PinnableArray<T> pinnableArray)
    {
        ArgumentNullException.ThrowIfNull(pinnableArray);
        pinnableArray.ThrowIfDisposed();
        return pinnableArray.Span;
    }

    /// <summary>
    /// Converts a specified pinned array to its native integer pointer representation.
    /// </summary>
    /// <remarks>This operator enables direct access to the unmanaged memory address of the pinned array. The
    /// array must be pinned before conversion; otherwise, an exception is thrown. Use caution when working with native
    /// pointers to avoid memory safety issues.</remarks>
    /// <param name="pinnableArray">The pinned array to convert. Must not be null, disposed, or unpinned.</param>
    public unsafe static implicit operator nint (PinnableArray<T> pinnableArray)
    {
        ArgumentNullException.ThrowIfNull(pinnableArray);
        pinnableArray.ThrowIfDisposed();
        InvalidOperationException.ThrowIfUnpinned(pinnableArray);
        return (nint)pinnableArray.UnmanagedPointer;
    }


    #endregion


    #region Element-wise operations

    /// <summary>
    /// Clears the contents of the buffer by setting all elements to their default value.
    /// </summary>
    /// <remarks>
    /// This method overwrites the entire logical range of the buffer (0 to <see cref="Length"/> - 1)
    /// with <c>default(T)</c>.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    public void Clear()
    {
        ThrowIfDisposed();
        _memory.Span.Clear();
    }

    /// <summary>
    /// Fills the buffer with the specified value.
    /// </summary>
    /// <param name="value">The value to assign to each element in the buffer.</param>
    /// <remarks>
    /// This method overwrites the entire logical range of the buffer (0 to <see cref="Length"/> - 1)
    /// with the given value.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    public void Fill(T value)
    {
        ThrowIfDisposed();
        _memory.Span.Fill(value);
    }

    /// <summary>
    /// Fills the buffer with a sequence of values starting from a given value and increasing by a fixed step.
    /// </summary>
    /// <param name="startValue">The initial value assigned to the first element.</param>
    /// <param name="step">The incremental step added to the previous value for each subsequent element.</param>
    /// <remarks>
    /// For index <c>i</c>, the value written is <c>startValue + step * i</c>.
    /// The entire logical range of the buffer (0 to <see cref="Length"/> - 1) is populated.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    public void Fill(T startValue, T step)
    {
        ThrowIfDisposed();
        var span = _memory.Span;
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = startValue;
            startValue += step;
        }
    }

    /// <summary>
    /// Fills the underlying buffer with the specified numeric value, interpreting the buffer as a sequence
    /// of elements of type <typeparamref name="TOther"/> when necessary.
    /// </summary>
    /// <typeparam name="TOther">
    /// The numeric type of the value to write into the buffer. Must be unmanaged and implement
    /// <see cref="INumberBase{TOther}"/>.
    /// </typeparam>
    /// <param name="number">The numeric value to write repeatedly into the buffer.</param>
    /// <remarks>
    /// <para>
    /// This method uses unsafe reinterpret casts to treat the underlying memory as a sequence of
    /// <typeparamref name="TOther"/> and writes <paramref name="number"/> repeatedly:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// If <c>sizeof(TOther) &gt; sizeof(T)</c>, only the largest multiple of <c>sizeof(TOther)</c> that fits
    /// in the buffer is written.
    /// </item>
    /// <item>
    /// If <c>sizeof(TOther) == sizeof(T)</c>, the buffer is filled element-wise, either directly (when
    /// <typeparamref name="TOther"/> is <typeparamref name="T"/>) or via reinterpretation.
    /// </item>
    /// <item>
    /// If <c>sizeof(TOther) &lt; sizeof(T)</c>, each destination element of type <typeparamref name="T"/>
    /// is formed by reinterpreting <paramref name="number"/> as <typeparamref name="T"/>.
    /// </item>
    /// </list>
    /// <para>
    /// This API is intended for advanced scenarios that require low-level memory manipulation or
    /// bulk initialization patterns.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
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
            ref var addr = ref Unsafe.As<T, TOther>(ref ManagedPointer);
            for (int i = 0; i < count; i++)
            {
                addr = number;
                addr = ref Unsafe.Add(ref addr, 1);
            }
        }
        else if (numberSize == size)
        {
            if (number is T n)
                Fill(n);
            else
            {
                ref var addr = ref Unsafe.As<T, TOther>(ref ManagedPointer);
                for (int i = 0; i < _length; i++)
                {
                    addr = number;
                    addr = ref Unsafe.Add(ref addr, 1);
                }
            }
        }
        else // numberSize < size
        {
            var num = Unsafe.As<TOther, T>(ref number);
            ref var addr = ref ManagedPointer;
            for (int i = 0; i < _length; i++)
            {
                addr = num;
                addr = ref Unsafe.Add(ref addr, 1);
            }
        }
    }

    #endregion


    #region Overrides

    /// <summary>
    /// Returns a string that represents the current contents of the buffer.
    /// </summary>
    /// <remarks>
    /// The returned string includes up to the first 50 elements of the buffer, separated by commas,
    /// followed by the total number of elements in parentheses. If the buffer contains more than
    /// 50 elements, an ellipsis (<c>... </c>) is appended before the total count to indicate that
    /// the output has been truncated.
    /// </remarks>
    /// <returns>
    /// A string representation of the buffer contents and the total number of items.
    /// </returns>
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

    private bool _isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                if (_isPinned) _memoryHandle.Dispose();
                if (Options.UseLeasingMode) _pool.Return(_array, false);
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _isDisposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~PinnableArray()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    /// <summary>
    /// Releases all resources used by the current instance of the class.
    /// </summary>
    /// <remarks>Call this method when you are finished using the object to release both managed and unmanaged
    /// resources. After calling this method, the object should not be used further. This method suppresses finalization to
    /// prevent the finalizer from running if it has already been called explicitly.</remarks>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public bool IsDisposed => _isDisposed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, nameof(PinnableArray<>));
    }

    #endregion

}
