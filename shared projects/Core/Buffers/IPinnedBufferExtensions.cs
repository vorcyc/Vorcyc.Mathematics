namespace Vorcyc.Mathematics.Buffers;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.Framework;

/// <summary>
/// Provides extension members for <see cref="IPinnedBuffer{T}"/>,
/// including common copy operations and buffer state validation helpers.
/// </summary>
public static class IPinnedBufferExtensions
{
    extension<T>(IPinnedBuffer<T> pinnedBuffer)
        where T : unmanaged, INumberBase<T>
    {
        /// <summary>
        /// Verifies that the underlying buffer has not been disposed and that its internal storage is still valid.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the buffer has been disposed or when its internal storage is no longer available.
        /// </exception>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal unsafe void ThrowIfDisposed()
        {
            if (pinnedBuffer is POHBuffer<T> pohBuffer)
            {
                ObjectDisposedException.ThrowIf(pohBuffer.IsDisposed, nameof(POHBuffer<>));
                ObjectDisposedException.ThrowIf(pohBuffer._buffer is null, nameof(POHBuffer<>));
            }
            else if (pinnedBuffer is NativeBuffer<T> nativeBuffer)
            {
                ObjectDisposedException.ThrowIf(nativeBuffer.IsDisposed, nameof(NativeBuffer<>));
                ObjectDisposedException.ThrowIf(nativeBuffer._pBuffer is null, nameof(NativeBuffer<>));
            }
        }

        /// <summary>
        /// Fills the entire buffer with random byte data.
        /// </summary>
        /// <remarks>
        /// This method overwrites the full contents of the buffer with newly generated random bytes.
        /// The buffer must not be disposed before the operation is performed.
        /// This method is not thread-safe and should be used only in a single-threaded context
        /// or with external synchronization.
        /// </remarks>
        public void FillWithRandomNumber()
        {
            pinnedBuffer.ThrowIfDisposed();
            Random.Shared.NextBytes(MemoryMarshal.AsBytes(pinnedBuffer.Span));
        }

        /// <summary>
        /// Gets the length of the current pinned buffer in bytes.
        /// </summary>
        public int ByteLength => pinnedBuffer.Length * Unsafe.SizeOf<T>();

        public static string Author => "cyclone_dll";
    }

    #region Copy From/To

    /// <summary>
    /// Defines an extension block that adds instance-style members to <see cref="IPinnedBuffer{T}"/>.
    /// </summary>
    /// <remarks>
    /// Members defined in this block behave like instance members of <see cref="IPinnedBuffer{T}"/>,
    /// while being implemented externally so that the interface itself does not need to be modified.
    /// </remarks>
    /// <typeparam name="T">
    /// The element type of the buffer. Must be unmanaged and implement <see cref="INumberBase{T}"/>.
    /// </typeparam>
    /// <param name="pinnedBuffer">The pinned buffer instance being extended.</param>
    extension<T>(IPinnedBuffer<T> pinnedBuffer)
        where T : unmanaged, INumberBase<T>
    {
        /// <summary>
        /// Copies data from an array into the buffer.
        /// Data that exceeds the destination buffer length is truncated.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        public void CopyFrom(T[] array)
        {
            pinnedBuffer.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(array);
            var minLength = Math.Min(pinnedBuffer.Length, array.Length);
            array.AsSpan(0, minLength).CopyTo(pinnedBuffer.Span);
        }

        /// <summary>
        /// Copies data from a specified segment of an array into the buffer.
        /// Data that exceeds the destination buffer length is truncated.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="offset">The starting offset in the source array.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="offset"/> or <paramref name="length"/> does not describe a valid range in <paramref name="array"/>.
        /// </exception>
        public void CopyFrom(T[] array, int offset, int length)
        {
            pinnedBuffer.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(array);
            if (offset < 0 || offset >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "起始索引超出数组范围。");
            if (length < 0 || offset + length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "长度超出数组范围。");
            var minLength = Math.Min(pinnedBuffer.Length, length);
            array.AsSpan(offset, minLength).CopyTo(pinnedBuffer.Span);
        }

        /// <summary>
        /// Copies data from a read-only span into the buffer.
        /// Data that exceeds the destination buffer length is truncated.
        /// </summary>
        /// <param name="span">The source read-only span.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="span"/> is empty.
        /// </exception>
        public void CopyFrom(ReadOnlySpan<T> span)
        {
            pinnedBuffer.ThrowIfDisposed();
            ArgumentNullException.ThrowIfEmpty(span);
            var minLength = Math.Min(pinnedBuffer.Length, span.Length);
            span.Slice(0, minLength).CopyTo(pinnedBuffer.Span);
        }

        /// <summary>
        /// Copies data from an <see cref="ArraySegment{T}"/> into the buffer.
        /// Data that exceeds the destination buffer length is truncated.
        /// </summary>
        /// <param name="segment">The source array segment.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the array of <paramref name="segment"/> is <see langword="null"/>.
        /// </exception>
        public void CopyFrom(ArraySegment<T> segment)
        {
            pinnedBuffer.ThrowIfDisposed();
            if (segment.Array is null || segment.Count == 0)
                throw new ArgumentNullException(nameof(segment), "ArraySegment 的 Array 不能为空。");
            var minLength = Math.Min(pinnedBuffer.Length, segment.Count);
            segment.AsSpan(0, minLength).CopyTo(pinnedBuffer.Span);
        }

        /// <summary>
        /// Copies data from an enumerable sequence into the buffer in enumeration order.
        /// Elements that do not fit in the destination buffer are ignored.
        /// </summary>
        /// <param name="collection">The source sequence.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public void CopyFrom(IEnumerable<T> collection)
        {
            pinnedBuffer.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(collection);
            int i = 0;
            foreach (var item in collection)
            {
                if (i >= pinnedBuffer.Length)
                    break;
                pinnedBuffer.Span[i++] = item;
            }
        }

        /// <summary>
        /// Copies data from another pinned buffer into the current buffer.
        /// The copy length is truncated to the smaller of the two buffers.
        /// </summary>
        /// <param name="other">The source pinned buffer.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="other"/> is <see langword="null"/>.
        /// </exception>
        public void CopyFrom(IPinnedBuffer<T> other)
        {
            pinnedBuffer.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(other);
            var minLength = Math.Min(pinnedBuffer.Length, other.Length);
            other.Span.Slice(0, minLength).CopyTo(pinnedBuffer.Span);
        }

        /// <summary>
        /// Copies data from a <see cref="ReadOnlyMemory{T}"/> instance into the buffer.
        /// Data that exceeds the destination buffer length is truncated.
        /// </summary>
        /// <param name="memory">The source read-only memory block.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="memory"/> is empty.
        /// </exception>
        public void CopyFrom(ReadOnlyMemory<T> memory)
        {
            pinnedBuffer.ThrowIfDisposed();
            if (memory.IsEmpty)
                throw new ArgumentNullException(nameof(memory), "Memory 不能为空。");
            var minLength = Math.Min(pinnedBuffer.Length, memory.Length);
            memory.Slice(0, minLength).CopyTo(pinnedBuffer.Memory);
        }

        /// <summary>
        /// Copies the contents of the buffer into a destination array.
        /// The copy length is truncated to the smaller of the buffer length and the array length.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        public void CopyTo(T[] array)
        {
            pinnedBuffer.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(array);
            var minLength = Math.Min(pinnedBuffer.Length, array.Length);
            pinnedBuffer.Span.Slice(0, minLength).CopyTo(array);
        }

        /// <summary>
        /// Copies the contents of the buffer into a specified segment of a destination array.
        /// The copy length is truncated to the smaller of the buffer length and the requested length.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="offset">The starting offset in the destination array.</param>
        /// <param name="length">The maximum number of elements to write.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="offset"/> or <paramref name="length"/> does not describe a valid range in <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int offset, int length)
        {
            pinnedBuffer.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(array);
            if (offset < 0 || offset >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "起始索引超出数组范围。");
            if (length < 0 || offset + length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "长度超出数组范围。");

            var minLength = Math.Min(pinnedBuffer.Length, length);
            pinnedBuffer.Span.Slice(0, minLength).CopyTo(array.AsSpan(offset, minLength));
        }

        /// <summary>
        /// Copies the contents of the buffer into a destination <see cref="Span{T}"/>.
        /// The copy length is truncated to the smaller of the two lengths.
        /// </summary>
        /// <param name="span">The destination span.</param>
        public void CopyTo(Span<T> span)
        {
            pinnedBuffer.ThrowIfDisposed();
            var minLength = Math.Min(pinnedBuffer.Length, span.Length);
            pinnedBuffer.Span.Slice(0, minLength).CopyTo(span);
        }

        /// <summary>
        /// Copies the contents of the buffer into a destination <see cref="Memory{T}"/>.
        /// The copy length is truncated to the smaller of the two lengths.
        /// </summary>
        /// <param name="memory">The destination memory block.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="memory"/> is empty.
        /// </exception>
        public void CopyTo(Memory<T> memory)
        {
            pinnedBuffer.ThrowIfDisposed();
            if (memory.IsEmpty)
                throw new ArgumentNullException(nameof(memory), "Memory 不能为空。");
            var minLength = Math.Min(pinnedBuffer.Length, memory.Length);
            pinnedBuffer.Span.Slice(0, minLength).CopyTo(memory.Span);
        }

        /// <summary>
        /// Copies the contents of the current buffer into another pinned buffer.
        /// The copy length is truncated to the smaller of the two buffers.
        /// </summary>
        /// <param name="other">The destination pinned buffer.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="other"/> is <see langword="null"/>.
        /// </exception>
        public void CopyTo(IPinnedBuffer<T> other)
        {
            pinnedBuffer.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(other);
            var minLength = Math.Min(pinnedBuffer.Length, other.Length);
            pinnedBuffer.Span.Slice(0, minLength).CopyTo(other.Span);
        }
    }

    #endregion

    #region Each helpers      

    /// <summary>
    /// Represents a delegate that performs an indexed element-wise operation by reference.
    /// </summary>
    /// <remarks>
    /// The referenced element can be modified in place inside the callback.
    /// </remarks>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <param name="index">The index of the current element.</param>
    /// <param name="current">A reference to the current element.</param>
    public delegate void ElementWiseActionDelegate<TElement>(int index, ref TElement current)
        where TElement : unmanaged, INumberBase<TElement>;

    extension<T>(IPinnedBuffer<T> pinnedBuffer)
        where T : unmanaged, INumberBase<T>
    {
        /// <summary>
        /// Transforms each element by using the specified function and writes the result back to the buffer.
        /// </summary>
        /// <param name="func">
        /// A transformation function that receives the element index and current value,
        /// and returns the new value to store at the same position.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="func"/> is <see langword="null"/>.
        /// </exception>
        public void Each(Func<int, T, T> func)
        {
            ArgumentNullException.ThrowIfNull(func);
            pinnedBuffer.ThrowIfDisposed();
            if (pinnedBuffer.Length == 0) return;

            for (int i = 0; i < pinnedBuffer.Length; i++)
                pinnedBuffer[i] = func(i, pinnedBuffer[i]);
        }

        /// <summary>
        /// Transforms each element by using a function that can inspect the previous and next elements.
        /// </summary>
        /// <param name="func">
        /// A transformation function whose parameters are:
        /// <c>index</c>, <c>prev</c>, <c>current</c>, and <c>next</c>.
        /// The returned value is written back to the current position.
        /// For the first element, <c>prev</c> is <see langword="null"/>.
        /// For the last element, <c>next</c> is <see langword="null"/>.
        /// </param>
        /// <param name="direction">
        /// The traversal direction.
        /// Use <see cref="Direction.Forward"/> for front-to-back traversal,
        /// or <see cref="Direction.Inverse"/> for back-to-front traversal.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="func"/> is <see langword="null"/>.
        /// </exception>
        public void Each(Func<int, T?, T, T?, T> func, Direction direction = Direction.Forward)
        {
            ArgumentNullException.ThrowIfNull(func);
            pinnedBuffer.ThrowIfDisposed();
            if (pinnedBuffer.Length == 0) return;

            if (direction == Direction.Forward)
            {
                for (int i = 0; i < pinnedBuffer.Length; i++)
                {
                    if (i == 0)
                    {
                        pinnedBuffer[i] = func(i, null, pinnedBuffer[i], pinnedBuffer[i + 1]);
                    }
                    else if (i == pinnedBuffer.Length - 1)
                    {
                        pinnedBuffer[i] = func(i, pinnedBuffer[i - 1], pinnedBuffer[i], null);
                    }
                    else
                    {
                        pinnedBuffer[i] = func(i, pinnedBuffer[i - 1], pinnedBuffer[i], pinnedBuffer[i + 1]);
                    }
                }
            }
            else if (direction == Direction.Inverse)
            {
                for (int i = pinnedBuffer.Length - 1; i > 0; i--)
                {
                    if (i == 0)
                    {
                        pinnedBuffer[i] = func(i, null, pinnedBuffer[i], pinnedBuffer[i + 1]);
                    }
                    else if (i == pinnedBuffer.Length - 1)
                    {
                        pinnedBuffer[i] = func(i, pinnedBuffer[i - 1], pinnedBuffer[i], null);
                    }
                    else
                    {
                        pinnedBuffer[i] = func(i, pinnedBuffer[i - 1], pinnedBuffer[i], pinnedBuffer[i + 1]);
                    }
                }
            }
        }

        /// <summary>
        /// Performs an in-place element-wise operation by using a callback that receives each element by reference.
        /// </summary>
        /// <param name="action">
        /// A delegate that receives the current index and a reference to the element,
        /// and may modify the element in place.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public void Each(ElementWiseActionDelegate<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            pinnedBuffer.ThrowIfDisposed();
            if (pinnedBuffer.Length == 0) return;

            //ref var addr = ref MemoryMarshal.GetReference(pinnedBuffer.Values);
            ref var addr = ref pinnedBuffer.ManagedPointer;
            for (int i = 0; i < pinnedBuffer.Length; i++)
            {
                action(i, ref addr);
                addr = ref Unsafe.Add(ref addr, 1);
            }
        }
    }

    #endregion
}

/// <summary>
/// Provides factory-style extension methods for creating <see cref="POHBuffer{T}"/> and <see cref="NativeBuffer{T}"/>
/// instances from arrays, spans, memory blocks, and enumerable sequences.
/// </summary>
public static class IPinnedBufferFactoryExtensions
{
    extension<T>(IPinnedBuffer<T> pinnedBuffer)
        where T : unmanaged, INumberBase<T>
    {
        /// <summary>
        /// Creates a new array containing a copy of the current pinned buffer contents.
        /// </summary>
        /// <returns>A new array containing the buffer contents.</returns>
        public T[] ToArray()
        {
            IPinnedBufferExtensions.ThrowIfDisposed(pinnedBuffer);
            var array = new T[pinnedBuffer.Length];
            pinnedBuffer.Span.CopyTo(array);
            return array;
        }
    }


    //extension<T>(T[]) where T : unmanaged, INumberBase<T>
    //{

    //    public  static T[] operator +(T[] array, T value)
    //    {
    //        ArgumentNullException.ThrowIfNull(array);
    //        var result = new T[array.Length];
    //        for (int i = 0; i < array.Length; i++)
    //            result[i] = array[i] + value;
    //        return result;
    //    }

    //不允许：
    //    public static implicit operator POHBuffer<T>(T[] array)
    //    {
    //        ArgumentNullException.ThrowIfNull(array);
    //        return new POHBuffer<T>(array);
    //    }
    //}


    //用传统语法写更方便

    /// <summary>
    /// Creates a new <see cref="POHBuffer{T}"/> from the contents of an entire managed array.
    /// </summary>
    /// <typeparam name="T">The element type. Must be unmanaged and implement <see cref="INumberBase{T}"/>.</typeparam>
    /// <param name="array">The source array. Cannot be <see langword="null"/>.</param>
    /// <returns>A new POH-backed pinned buffer containing a copy of the array contents.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
    public static POHBuffer<T> ToPohBuffer<T>(this T[] array) where T : unmanaged, INumberBase<T>
    {
        ArgumentNullException.ThrowIfNull(array);
        return new POHBuffer<T>(array);
    }

    /// <summary>
    /// Creates a new <see cref="POHBuffer{T}"/> from the contents of a read-only span.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="span">The source read-only span. Its length must be greater than zero.</param>
    /// <returns>A new POH-backed pinned buffer containing a copy of the span contents.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="span"/> is empty.</exception>
    public static POHBuffer<T> ToPohBuffer<T>(this ReadOnlySpan<T> span) where T : unmanaged, INumberBase<T>
    {
        ArgumentNullException.ThrowIfEmpty(span);
        return new POHBuffer<T>(span);
    }

    /// <summary>
    /// Creates a new <see cref="POHBuffer{T}"/> from the contents of an <see cref="ArraySegment{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="segment">The source array segment. Its underlying array cannot be <see langword="null"/> and its count must be greater than zero.</param>
    /// <returns>A new POH-backed pinned buffer containing a copy of the array segment contents.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <see cref="ArraySegment{T}.Array"/> is <see langword="null"/> or when the segment count is zero.
    /// </exception>
    public static POHBuffer<T> ToPohBuffer<T>(this ArraySegment<T> segment) where T : unmanaged, INumberBase<T>
    {
        if (segment.Array is null || segment.Count == 0)
            throw new ArgumentNullException(nameof(segment), "ArraySegment 的 Array 不能为空。");
        return new POHBuffer<T>(segment);
    }

    /// <summary>
    /// Creates a new <see cref="POHBuffer{T}"/> from the contents of a <see cref="ReadOnlyMemory{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="readOnlyMemory">The source read-only memory block. Its length must be greater than zero.</param>
    /// <returns>A new POH-backed pinned buffer containing a copy of the memory contents.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="readOnlyMemory"/> is empty.
    /// </exception>
    public static POHBuffer<T> ToPohBuffer<T>(this ReadOnlyMemory<T> readOnlyMemory) where T : unmanaged, INumberBase<T>
    {
        if (readOnlyMemory.Length == 0) throw new ArgumentNullException(nameof(readOnlyMemory), "ReadOnlyMemory 不能为空。");
        return new POHBuffer<T>(readOnlyMemory);
    }

    /// <summary>
    /// Creates a new <see cref="POHBuffer{T}"/> from the contents of an enumerable sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="collection">The source element sequence. Cannot be <see langword="null"/>.</param>
    /// <returns>A new POH-backed pinned buffer containing a copy of the sequence contents.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is <see langword="null"/>.</exception>
    public static POHBuffer<T> ToPohBuffer<T>(this IEnumerable<T> collection) where T : unmanaged, INumberBase<T>
    {
        ArgumentNullException.ThrowIfNull(collection);
        return new POHBuffer<T>(collection);
    }

    /// <summary>
    /// Creates a new <see cref="NativeBuffer{T}"/> from the contents of an entire managed array.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="array">The source array. Cannot be <see langword="null"/>.</param>
    /// <returns>A new native pinned buffer containing a copy of the array contents.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
    public static NativeBuffer<T> ToNativeBuffer<T>(this T[] array) where T : unmanaged, INumberBase<T>
    {
        ArgumentNullException.ThrowIfNull(array);
        return new NativeBuffer<T>(array);
    }

    /// <summary>
    /// Creates a new <see cref="NativeBuffer{T}"/> from the contents of a read-only span.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="span">The source read-only span. Its length must be greater than zero.</param>
    /// <returns>A new native pinned buffer containing a copy of the span contents.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="span"/> is empty.</exception>
    public static NativeBuffer<T> ToNativeBuffer<T>(this ReadOnlySpan<T> span) where T : unmanaged, INumberBase<T>
    {
        ArgumentNullException.ThrowIfEmpty(span);
        return new NativeBuffer<T>(span);
    }

    /// <summary>
    /// Creates a new <see cref="NativeBuffer{T}"/> from the contents of an <see cref="ArraySegment{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="segment">The source array segment. Its underlying array cannot be <see langword="null"/> and its count must be greater than zero.</param>
    /// <returns>A new native pinned buffer containing a copy of the array segment contents.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <see cref="ArraySegment{T}.Array"/> is <see langword="null"/> or when the segment count is zero.
    /// </exception>
    public static NativeBuffer<T> ToNativeBuffer<T>(this ArraySegment<T> segment) where T : unmanaged, INumberBase<T>
    {
        if (segment.Array is null || segment.Count == 0)
            throw new ArgumentNullException(nameof(segment), "ArraySegment 的 Array 不能为空。");
        return new NativeBuffer<T>(segment);
    }

    /// <summary>
    /// Creates a new <see cref="NativeBuffer{T}"/> from the contents of a <see cref="ReadOnlyMemory{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="readOnlyMemory">The source read-only memory block. Its length must be greater than zero.</param>
    /// <returns>A new native pinned buffer containing a copy of the memory contents.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="readOnlyMemory"/> is empty.
    /// </exception>
    public static NativeBuffer<T> ToNativeBuffer<T>(this ReadOnlyMemory<T> readOnlyMemory) where T : unmanaged, INumberBase<T>
    {
        if (readOnlyMemory.Length == 0) throw new ArgumentNullException(nameof(readOnlyMemory), "ReadOnlyMemory 不能为空。");
        return new NativeBuffer<T>(readOnlyMemory);
    }

    /// <summary>
    /// Creates a new <see cref="NativeBuffer{T}"/> from the contents of an enumerable sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="collection">The source element sequence. Cannot be <see langword="null"/>.</param>
    /// <returns>A new native pinned buffer containing a copy of the sequence contents.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is <see langword="null"/>.</exception>
    public static NativeBuffer<T> ToNativeBuffer<T>(this IEnumerable<T> collection) where T : unmanaged, INumberBase<T>
    {
        ArgumentNullException.ThrowIfNull(collection);
        return new NativeBuffer<T>(collection);
    }
}