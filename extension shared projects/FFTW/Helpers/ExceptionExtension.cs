using System.Numerics;

namespace Vorcyc.Mathematics.Extensions.FFTW.Helpers;

/// <summary>
/// Provides extension methods for validating arguments and throwing <see cref="ArgumentNullException"/> when null,
/// empty, or invalid values are detected.
/// </summary>
/// <remarks>Use the methods in this class to enforce argument validation for reference types, strings, pointers,
/// and spans. These methods help ensure that method parameters meet required preconditions and throw standardized
/// exceptions when validation fails.</remarks>
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

/// <summary>
/// Provides extension methods for validating argument values and throwing exceptions when argument conditions are not
/// met.
/// </summary>
/// <remarks>This static class contains methods that assist in argument validation, enabling more expressive and
/// concise error handling for method parameters. The extension methods are designed to be used with <see
/// cref="ArgumentException"/> to enforce argument constraints and improve code readability.</remarks>
public static class ArgumentExceptionExtension
{

    extension(ArgumentException)
    {

        /// <summary>
        /// Throws an exception if the lengths of the specified arrays are not equal.
        /// </summary>
        /// <typeparam name="T1">The type of elements in the first array. Must be unmanaged and implement <see cref="INumberBase{T1}"/>.</typeparam>
        /// <typeparam name="T2">The type of elements in the second array. Must be unmanaged and implement <see cref="INumberBase{T2}"/>.</typeparam>
        /// <param name="arr1">The first array to compare for length equality.</param>
        /// <param name="arr2">The second array to compare for length equality.</param>
        /// <param name="message">An optional custom error message to include in the exception. If <see langword="null"/>, a default message
        /// is used.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="arr1"/> and <paramref name="arr2"/> do not have the same length.</exception>
        public static void ThrowIfArrayLengthNotEqual<T1,T2>(PinnableArray<T1> arr1, PinnableArray<T2> arr2, string? message = null) 
            where T1 : unmanaged, INumberBase<T1>
            where T2 : unmanaged, INumberBase<T2>
        {
            if (arr1.Length != arr2.Length)
            {
                throw new ArgumentException(message ?? $"Array lengths are not equal. {nameof(arr1)}.Length: {arr1.Length}, {nameof(arr2)}.Length: {arr2.Length}");
            }
        }



        /// <summary>
        /// Throws an exception if the specified arrays do not have equal lengths.
        /// </summary>
        /// <typeparam name="T1">The type of elements in the first array.</typeparam>
        /// <typeparam name="T2">The type of elements in the second array.</typeparam>
        /// <param name="arr1">The first array to compare for length equality. Cannot be null.</param>
        /// <param name="arr2">The second array to compare for length equality. Cannot be null.</param>
        /// <param name="message">An optional custom error message to include in the exception. If null, a default message is used.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="arr1"/> and <paramref name="arr2"/> do not have equal lengths.</exception>
        public static void ThrowIfArrayLengthNotEqual<T1,T2>(T1[] arr1, T2[] arr2, string? message = null)
        {
            if (arr1.Length != arr2.Length)
            {
                throw new ArgumentException(message ?? $"Array lengths are not equal. {nameof(arr1)}.Length: {arr1.Length}, {nameof(arr2)}.Length: {arr2.Length}");
            }
        }



        /// <summary>
        /// Throws an exception if the lengths of the specified spans are not equal.
        /// </summary>
        /// <typeparam name="T1">The type of elements in the first span.</typeparam>
        /// <typeparam name="T2">The type of elements in the second span.</typeparam>
        /// <param name="arr1">The first span to compare for length equality.</param>
        /// <param name="arr2">The second span to compare for length equality.</param>
        /// <param name="message">An optional custom error message to include in the exception. If null, a default message is used.</param>
        /// <exception cref="ArgumentException">Thrown if the lengths of <paramref name="arr1"/> and <paramref name="arr2"/> are not equal.</exception>
        public static void ThrowIfArrayLengthNotEqual<T1, T2>(Span<T1> arr1, Span<T2> arr2, string? message = null)
        {
            if (arr1.Length != arr2.Length)
            {
                throw new ArgumentException(message ?? $"Array lengths are not equal. {nameof(arr1)}.Length: {arr1.Length}, {nameof(arr2)}.Length: {arr2.Length}");
            }
        }

    }
}

/// <summary>
/// Provides extension methods for validating operation conditions and throwing an <see
/// cref="InvalidOperationException"/> when required.
/// </summary>
/// <remarks>Use these methods to enforce preconditions related to pointer validity and array pinning,
/// particularly in scenarios involving interop or unsafe code. These extensions help ensure that invalid states are
/// detected early and reported with meaningful exceptions.</remarks>
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

