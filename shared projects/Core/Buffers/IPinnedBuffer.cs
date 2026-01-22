namespace Vorcyc.Mathematics.Buffers;

using System.Numerics;

/// <summary>
/// Defines a fixed or directly addressable buffer abstraction for unmanaged numeric values.
/// </summary>
/// <remarks>
/// Typical implementations include:
/// <list type="bullet">
///   <item>
///     <description>
///     <see cref="NativeBuffer{T}"/>, which stores data in unmanaged memory allocated through
///     <see cref="System.Runtime.InteropServices.NativeMemory"/>.
///     </description>
///   </item>
///   <item>
///     <description>
///     <see cref="POHBuffer{T}"/>, which stores data in a pinned managed array allocated on the
///     pinned object heap (POH).
///     </description>
///   </item>
/// </list>
/// Regardless of the underlying storage strategy, an implementation exposes:
/// <list type="bullet">
///   <item><description>A direct <see cref="Span{T}"/> view over the buffer contents.</description></item>
///   <item><description>An unmanaged pointer to the first element.</description></item>
///   <item><description>A managed by-reference view of the first element.</description></item>
/// </list>
/// Callers are responsible for ensuring that the buffer is not disposed or reallocated while any
/// previously acquired <see cref="Span{T}"/>, <c>ref</c>, or pointer is still in use.
/// </remarks>
/// <typeparam name="T">
/// The element type stored in the buffer. The type must be unmanaged and implement
/// <see cref="INumberBase{T}"/>.
/// </typeparam>
public interface IPinnedBuffer<T> : IDisposable
    where T : unmanaged, INumberBase<T>
{
    /// <summary>
    /// Gets a span over the logical contents of the buffer without copying.
    /// </summary>
    /// <remarks>
    /// For <see cref="NativeBuffer{T}"/>, the returned span maps directly to unmanaged memory.
    /// For <see cref="POHBuffer{T}"/>, the returned span maps directly to a pinned managed array.
    /// The returned span becomes invalid after the buffer is disposed or reallocated.
    /// </remarks>
    Span<T> Span { get; }

    /// <summary>
    /// Gets a contiguous <see cref="Memory{T}"/> view over the logical contents of the buffer.
    /// </summary>
    /// <remarks>
    /// This property is intended for integration with APIs that operate on <see cref="Memory{T}"/>.
    /// The returned memory represents the current buffer contents and becomes invalid after the
    /// buffer is disposed or reallocated.
    /// </remarks>
    Memory<T> Memory { get; }

    /// <summary>
    /// Gets an unmanaged pointer to the first element of the buffer.
    /// </summary>
    /// <remarks>
    /// For <see cref="NativeBuffer{T}"/>, this pointer refers to memory allocated by
    /// <see cref="System.Runtime.InteropServices.NativeMemory"/>.
    /// For <see cref="POHBuffer{T}"/>, this pointer refers to the first element of a pinned managed array.
    /// The pointer becomes invalid after the buffer is disposed or reallocated.
    /// </remarks>
    unsafe T* UnmanagedPointer { get; }

    /// <summary>
    /// Gets a managed reference to the first element of the buffer.
    /// </summary>
    /// <remarks>
    /// This member is intended for advanced scenarios that require <c>ref</c>-based access or
    /// low-level operations through <see cref="System.Runtime.CompilerServices.Unsafe"/>.
    /// Any reference obtained from this property becomes invalid after the buffer is disposed
    /// or reallocated.
    /// </remarks>
    ref T ManagedPointer { get; }

    /// <summary>
    /// Gets the logical number of elements in the buffer.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Gets or sets the element at the specified zero-based index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to access.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="index"/> is less than zero or greater than or equal to <see cref="Length"/>.
    /// </exception>
    T this[int index] { get; set; }

    /// <summary>
    /// Gets a contiguous region of the buffer as a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="startIndex">The zero-based index at which the region begins.</param>
    /// <param name="length">The number of elements in the region.</param>
    /// <returns>A span representing the requested region.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="startIndex"/> or <paramref name="length"/> does not describe a valid range within the buffer.
    /// </exception>
    Span<T> this[int startIndex, int length] { get; }

    /// <summary>
    /// Gets a range of the buffer as a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="range">The range of elements to access.</param>
    /// <returns>A span representing the specified range.</returns>
    Span<T> this[Range range] { get; }

    /// <summary>
    /// Resets all elements in the buffer to <c>default(T)</c>.
    /// </summary>
    void Clear();

    /// <summary>
    /// Fills the entire buffer with the specified value.
    /// </summary>
    /// <param name="value">The value assigned to every element.</param>
    void Fill(T value);

    /// <summary>
    /// Fills the buffer with an arithmetic sequence.
    /// </summary>
    /// <remarks>
    /// The value written to element <c>i</c> is equivalent to:
    /// <code>
    /// startValue + step * i
    /// </code>
    /// </remarks>
    /// <param name="startValue">The value assigned to the first element.</param>
    /// <param name="step">The increment applied between adjacent elements.</param>
    void Fill(T startValue, T step);

    /// <summary>
    /// Fills the buffer using a value of another unmanaged numeric type.
    /// </summary>
    /// <remarks>
    /// Implementations may reinterpret the underlying storage depending on the size relationship between
    /// <typeparamref name="TOther"/> and <typeparamref name="T"/>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     If <c>sizeof(TOther) &gt; sizeof(T)</c>, only the largest prefix that can be written as
    ///     <typeparamref name="TOther"/> elements is filled.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     If <c>sizeof(TOther) == sizeof(T)</c>, the buffer may be filled directly or through
    ///     reinterpretation, depending on the runtime types involved.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     If <c>sizeof(TOther) &lt; sizeof(T)</c>, the source value may be bitwise reinterpreted as
    ///     <typeparamref name="T"/> before filling each destination element.
    ///     </description>
    ///   </item>
    /// </list>
    /// This method is intended for advanced low-level scenarios and may rely on
    /// <see cref="System.Runtime.CompilerServices.Unsafe"/> or pointer-based writes.
    /// </remarks>
    /// <typeparam name="TOther">
    /// The source numeric type used to fill the buffer. The type must be unmanaged and implement
    /// <see cref="INumberBase{TOther}"/>.
    /// </typeparam>
    /// <param name="number">The value used to fill the buffer.</param>
    void FillWith<TOther>(TOther number) where TOther : unmanaged, INumberBase<TOther>;

    /// <summary>
    /// Reallocates the buffer to the specified logical length.
    /// </summary>
    /// <remarks>
    /// Implementations may use different strategies:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     <see cref="NativeBuffer{T}"/> typically resizes unmanaged memory by calling
    ///     <see cref="System.Runtime.InteropServices.NativeMemory.Realloc(void*, nuint)"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     <see cref="POHBuffer{T}"/> typically allocates a new pinned array and copies the first
    ///     <c>min(oldLength, newLength)</c> elements into it.
    ///     </description>
    ///   </item>
    /// </list>
    /// After a successful reallocation, any previously acquired <see cref="Span"/>,
    /// <see cref="Memory"/>, <see cref="UnmanagedPointer"/>, or <see cref="ManagedPointer"/>
    /// should be considered invalid and must be reacquired.
    /// </remarks>
    /// <param name="newLength">The new logical element count. Must be greater than zero.</param>
    void Reallocate(int newLength);

    /// <summary>
    /// Gets a value indicating whether the buffer has been disposed.
    /// </summary>
    bool IsDisposed { get; }
}