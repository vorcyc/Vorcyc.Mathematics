using System.Numerics;
using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.Extensions.FFTW;

public static class ArgumentNullExceptionExtension
{

    extension(ArgumentNullException)
    {

        /// <summary>
        /// Throws an exception if the specified pointer is zero.
        /// </summary>
        /// <param name="ptr">The pointer value to validate. If <paramref name="ptr"/> is <see cref="nint.Zero"/>, an exception is thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ptr"/> is <see cref="nint.Zero"/>.</exception>
        public static void ThrowIfZero(nint ptr)
        {
            if (ptr == nint.Zero)
            {
                throw new ArgumentNullException(nameof(ptr), "Pointer is null.");
            }
        }

        /// <summary>
        /// Throws an exception if the specified object is null.
        /// </summary>
        /// <remarks>Use this method to enforce non-null arguments for reference types at runtime. This
        /// method is typically used at the start of a method to validate input parameters.</remarks>
        /// <typeparam name="T">The reference type of the object to check for null.</typeparam>
        /// <param name="obj">The object to validate. If <paramref name="obj"/> is null, an exception is thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        public static void ThrowIfNull<T>(T? obj) where T : class
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj), "Object is null.");
            }
        }

        /// <summary>
        /// Throws an exception if the specified string is null or empty.
        /// </summary>
        /// <param name="str">The string to validate. If null or empty, an exception is thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null or an empty string.</exception>
        public static void ThrowIfNullOrEmpty(string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str), "String is null or empty.");
            }
        }

        /// <summary>
        /// Throws an exception if the specified unmanaged pointer is null.
        /// </summary>
        /// <param name="ptr">A pointer to validate. If <paramref name="ptr"/> is null, an exception is thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ptr"/> is null.</exception>
        public unsafe static void ThrowIfNull(void* ptr)
        {
            if (ptr == null)
            {
                throw new ArgumentNullException(nameof(ptr), "Pointer is null.");
            }
        }

        /// <summary>
        /// Throws an exception if the specified span is empty.
        /// </summary>
        /// <remarks>Use this method to enforce that a span contains at least one element before
        /// proceeding with operations that require non-empty data.</remarks>
        /// <typeparam name="T">The type of elements contained in the span.</typeparam>
        /// <param name="span">The span to check for emptiness. Must not be empty.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="span"/> is empty.</exception>
        public static void ThrowIfEmpty<T>(Span<T> span)
        {
            if (span.IsEmpty)
            {
                throw new ArgumentNullException(nameof(span), "Span is empty.");
            }
        }

    }
}


public static class InvalidOperationExceptionExtension
{

    extension(InvalidOperationException)
    {

        /// <summary>
        /// Throws an exception if the specified pointer is zero.
        /// </summary>
        /// <param name="ptr">The pointer value to validate. If <paramref name="ptr"/> is <see cref="nint.Zero"/>, an exception is thrown.</param>
        /// <param name="message">An optional message to include in the exception.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="ptr"/> is <see cref="nint.Zero"/>.</exception>
        public static void ThrowIfZero(nint ptr, string? message = null)
        {
            if (ptr == nint.Zero)
            {
                throw new InvalidOperationException(message ?? "Pointer is null.");
            }
        }


        /// <summary>
        /// Throws an exception if the specified <see cref="PinnableArray{T}"/> instance is not pinned.
        /// </summary>
        /// <remarks>Use this method to ensure that the array is pinned before performing operations that
        /// require a fixed memory address, such as interop or unsafe code.</remarks>
        /// <typeparam name="T">The type of elements in the array. Must be unmanaged and implement <see cref="INumberBase{T}"/>.</typeparam>
        /// <param name="array">The <see cref="PinnableArray{T}"/> to check for a pinned state.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="array"/> is not pinned.</exception>
        public static void ThrowIfUnpinned<T>(PinnableArray<T> array) where T : unmanaged, INumberBase<T>
        {
            if (!array.IsPinned)
            {
                throw new InvalidOperationException("PinnableArray is not pinned.");
            }
        }

    }
}

