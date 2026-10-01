namespace Vorcyc.Mathematics;
using System.Numerics;
using System.Runtime.InteropServices;
public static partial class ArrayExtension
{
    #region Generic
    /// <summary>
    /// Copies the array.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <returns>The newly copied array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Copy<T>(this T[] source)
    {
        var result = new T[source.Length];
        Array.Copy(source, result, source.Length);
        return result;
    }
    /// <summary>
    /// Copies the specified length of the array.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="length">The length to copy.</param>
    /// <returns>The newly copied array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Copy<T>(this T[] source, int length)
    {
        var result = new T[length];
        Array.Copy(source, result, length);
        return result;
    }
    /// <summary>
    /// Initializes an array of the specified length and fills it with an initial value.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="length">The length of the array.</param>
    /// <param name="initialValue">The initial value.</param>
    /// <returns>The initialized array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[]? InitializeArray<T>(int length, T initialValue = default!)
    {
        if (length < 0)
        {
            return null;
        }
        var array = new T[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = initialValue!;
        }
        return array;
    }
    /// <summary>
    /// Fills the entire array with the specified value.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="value">The value to fill with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }
    /// <summary>
    /// Fills the specified range of the array with the specified value.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="start">The start index.</param>
    /// <param name="end">The end index.</param>
    /// <param name="value">The value to fill with.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the start or end index is out of range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(this T[] array, int start, int end, T value)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (start < 0 || start > end)
            throw new ArgumentOutOfRangeException(nameof(start));
        if (end >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(end));
        for (int i = start; i < end; i++)
        {
            array[i] = value;
        }
    }
    /// <summary>
    /// Fills the specified range of the array with the specified value.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="range">The range to fill.</param>
    /// <param name="value">The value to fill with.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the range is out of the array bounds.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(this T[] array, Range range, T value)
    {
        if (array is null) throw new ArgumentNullException(nameof(array));
        var (offset, length) = range.GetOffsetAndLength(array.Length);
        for (int i = offset; i < offset + length; i++)
        {
            array[i] = value;
        }
    }
    /// <summary>
    /// Fills the specified <see cref="Span{T}"/> with the specified value.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="values">The specified <see cref="Span{T}"/>.</param>
    /// <param name="value">The value to fill with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(this Span<T> values, T value)
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = value;
        }
    }
    /// <summary>
    /// Fills the array with random floating-point numbers.
    /// </summary>
    /// <param name="span">The array to fill.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the array length is less than 1.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber(this Span<float> span)
    {
        if (span.Length < 1)
            throw new ArgumentOutOfRangeException(nameof(span));
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = Random.Shared.NextSingle();
        }
    }
    /// <summary>
    /// Fills the array with random numbers.
    /// </summary>
    /// <typeparam name="T">The array element type, must implement the IFloatingPointIeee754 interface.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber<T>(this T[] array)
        where T : IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        for (int i = 0; i < array.Length; i++)
            array[i] = T.CreateTruncating(Random.Shared.NextDouble());
    }
    /// <summary>
    /// Fills the specified range of the array with random numbers.
    /// </summary>
    /// <typeparam name="T">The array element type, must implement the IFloatingPointIeee754 interface.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="range">The range to fill.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the range is out of the array bounds.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber<T>(this T[] array, Range range)
        where T : IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        var (offset, length) = range.GetOffsetAndLength(array.Length);
        for (int i = offset; i < offset + length; i++)
            array[i] = T.CreateTruncating(Random.Shared.NextDouble());
    }
    /// <summary>
    /// Fills the array with random numbers.
    /// </summary>
    /// <param name="array">The array to fill.</param>
    /// <param name="limit">The range of the random numbers.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber(this int[] array, (int max, int min)? limit = null)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        if (limit is null)
            for (int i = 0; i < array.Length; i++)
                array[i] = Random.Shared.Next();
        else
            for (int i = 0; i < array.Length; i++)
                array[i] = Random.Shared.Next(limit.Value.min, limit.Value.max);
    }
    /// <summary>
    /// Fills the specified range of the array with random numbers.
    /// </summary>
    /// <param name="array">The array to fill.</param>
    /// <param name="range">The range to fill.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber(this int[] array, Range range)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        var (offset, length) = range.GetOffsetAndLength(array.Length);
        for (int i = offset; i < offset + length; i++)
            array[i] = Random.Shared.Next();
    }
    /// <summary>
    /// Fills the array with random numbers.
    /// </summary>
    /// <param name="array">The array to fill.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber(this long[] array)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        for (int i = 0; i < array.Length; i++)
            array[i] = Random.Shared.NextInt64();
    }
    /// <summary>
    /// Fills the specified range of the array with random numbers.
    /// </summary>
    /// <param name="array">The array to fill.</param>
    /// <param name="range">The range to fill.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber(this long[] array, Range range)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        var (offset, length) = range.GetOffsetAndLength(array.Length);
        for (int i = offset; i < offset + length; i++)
            array[i] = Random.Shared.NextInt64();
    }
    /// <summary>
    /// Fills the array with the specified starting value and step.
    /// </summary>
    /// <typeparam name="T">The array element type, must implement the INumber interface.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="startValue">The starting value.</param>
    /// <param name="step">The step.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(this T[] array, T startValue, T step)
        where T : INumber<T>
    {
        var value = startValue;
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = startValue;
            startValue += step;
        }
    }
    /// <summary>
    /// Fills the array with the specified starting value and step.
    /// </summary>
    /// <typeparam name="T">The array element type, must implement the INumber interface.</typeparam>
    /// <param name="span">The array to fill.</param>
    /// <param name="startValue">The starting value.</param>
    /// <param name="step">The step.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(this Span<T> span, T startValue, T step)
        where T : INumber<T>
    {
        var value = startValue;
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = startValue;
            startValue += step;
        }
    }
    /// <summary>
    /// Gets an inner segment and returns it as an enumerable sequence.
    /// </summary>
    /// <remarks>
    /// The same effect can be achieved using the LINQ extension method System.Linq.Enumerable.Skip(start).Take(length).
    /// However, this version has better performance.
    /// </remarks>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="start">The start index.</param>
    /// <param name="length">The length.</param>
    /// <returns>An enumerable sequence of the inner segment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the start index or length is out of range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> GetInner<T>(this T[] array, int start, int length)
    {
        if (array is null)
            throw new ArgumentNullException();
        if (start < 0 || start > array.Length)
            throw new ArgumentOutOfRangeException(nameof(start));
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));
        int len = System.Math.Min(array.Length, length) + start;
        for (int i = start; i < len; i++)
        {
            yield return array[i];
        }
    }
    /// <summary>
    /// Gets an inner segment of an array and returns it as a new array.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="start">The start index.</param>
    /// <param name="length">The length.</param>
    /// <returns>A new array containing the inner segment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the start index or length is out of range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] GetInnerArray<T>(this T[] array, int start, int length)
    {
        if (array == null)
            throw new ArgumentNullException();
        if (start < 0 || start > array.Length)
            throw new ArgumentOutOfRangeException(nameof(start));
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));
        int arrayBound = System.Math.Min(array.Length - start, length);
        T[] result = new T[arrayBound];
        Array.Copy(array, start, result, 0, arrayBound);
        return result;
    }
    /// <summary>
    /// Removes a segment from the array and returns the resulting new array.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="start">The start index of the segment to remove.</param>
    /// <param name="length">The length of the segment to remove.</param>
    /// <returns>The array with the specified segment removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] RemoveSegment<T>(this T[] array, int start, int length)
    {
        var result = new T[array.Length - length];
        Array.Copy(array, 0, result, 0, start);
        Array.Copy(array, start + length, result, start, array.Length - start - length);
        return result;
    }
    ///// <summary>
    ///// 联接两个数组。
    ///// </summary>
    ///// <typeparam name="T">数组元素的类型。</typeparam>
    ///// <param name="leading">前置数组。</param>
    ///// <param name="following">后置数组。</param>
    ///// <returns>返回联接后的新数组。</returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static T[] Merge<T>(this T[] leading, T[] following)
    //{
    //    var result = new T[leading.Length + following.Length];
    //    Array.Copy(leading, result, leading.Length);
    //    Array.Copy(following, 0, result, leading.Length, following.Length);
    //    return result;
    //}
    /// <summary>
    /// Merges two arrays.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="leading">The leading array, cannot be null.</param>
    /// <param name="following">The following array, cannot be null.</param>
    /// <returns>The merged new array. Throws <see cref="ArgumentNullException"/> if either input is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="leading"/> or <paramref name="following"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Merge<T>(this T[] leading, T[] following)
    {
        // 显式检查 null，更友好（尤其在调试时）
        ArgumentNullException.ThrowIfNull(leading);
        ArgumentNullException.ThrowIfNull(following);
        // 如果任一数组为空，直接返回另一个的副本（避免分配 0 长数组）
        if (leading.Length == 0) return following.ToArray();  // ToArray() 会返回克隆
        if (following.Length == 0) return leading.ToArray();
        var result = new T[leading.Length + following.Length];
        Array.Copy(leading, result, leading.Length);
        Array.Copy(following, 0, result, leading.Length, following.Length);
        return result;
    }
    /// <summary>
    /// Converts a collection to a string.
    /// </summary>
    /// <typeparam name="T">The collection element type.</typeparam>
    /// <param name="collection">The collection to convert.</param>
    /// <returns>A string representing the collection.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToString<T>(this IEnumerable<T> collection)
    {
        return "[" + string.Join(",", collection) + "]";
    }
    /// <summary>
    /// Quickly copies a fragment of the array.
    /// </summary>
    /// <typeparam name="T">The array element type, must be an unmanaged type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="size">The size to copy.</param>
    /// <param name="sourceOffset">The offset in the source array.</param>
    /// <param name="destinationOffset">The offset in the destination array.</param>
    /// <returns>The newly copied array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] FastCopyFragment<T>(this T[] source, int size, int sourceOffset = 0, int destinationOffset = 0)
        where T : unmanaged
    {
        var totalSize = size + destinationOffset;
        var destination = new T[totalSize];
        Array.Copy(source, sourceOffset, destination, destinationOffset, size);
        return destination;
    }
    /// <summary>
    /// Quickly copies the array to a destination array.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="size">The size to copy.</param>
    /// <param name="sourceOffset">The offset in the source array.</param>
    /// <param name="destinationOffset">The offset in the destination array.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FastCopyTo<T>(this T[] source, T[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)
    {
        Array.Copy(source, sourceOffset, destination, destinationOffset, size);
    }
    /// <summary>
    /// Creates a new array containing the given array repeated <paramref name="n"/> times.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="n">The number of repetitions.</param>
    /// <returns>The newly repeated array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Repeat<T>(this T[] source, int n)
    {
        var repeated = new T[source.Length * n];
        var elementSize = Marshal.SizeOf<T>();
        var offset = 0;
        for (var i = 0; i < n; i++)
        {
            Buffer.BlockCopy(source, 0, repeated, offset * elementSize, source.Length * elementSize);
            offset += source.Length;
        }
        return repeated;
    }
    #endregion
    /// <summary>
    /// Creates array of single-precision values from enumerable of double-precision values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] ToFloats(this IEnumerable<double> values)
    {
        return values.Select(v => (float)v).ToArray();
    }
    /// <summary>
    /// Creates array of double-precision values from enumerable of single-precision values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] ToDoubles(this IEnumerable<float> values)
    {
        return values.Select(v => (double)v).ToArray();
    }

    #region single precision
    private const byte _32Bits = sizeof(float);
    /// <summary>
    /// Creates fast copy of array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] FastCopy(this float[] source)
    {
        var destination = new float[source.Length];
        Buffer.BlockCopy(source, 0, destination, 0, source.Length * _32Bits);
        return destination;
    }
    /// <summary>
    /// Makes fast copy of array (or its part) to existing <paramref name="destination"/> array (or its part).
    /// </summary>
    /// <param name="source">Source array</param>
    /// <param name="destination">Destination array</param>
    /// <param name="size">Number of elements to copy</param>
    /// <param name="sourceOffset">Offset in source array</param>
    /// <param name="destinationOffset">Offset in destination array</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FastCopyTo(this float[] source, float[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)
    {
        Buffer.BlockCopy(source, sourceOffset * _32Bits, destination, destinationOffset * _32Bits, size * _32Bits);
    }
    /// <summary>
    /// Makes fast copy of array fragment starting at specified offset.
    /// </summary>
    /// <param name="source">Source array</param>
    /// <param name="size">Number of elements to copy</param>
    /// <param name="sourceOffset">Offset in source array</param>
    /// <param name="destinationOffset">Offset in destination array</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] FastCopyFragment(this float[] source, int size, int sourceOffset = 0, int destinationOffset = 0)
    {
        var totalSize = size + destinationOffset;
        var destination = new float[totalSize];
        Buffer.BlockCopy(source, sourceOffset * _32Bits, destination, destinationOffset * _32Bits, size * _32Bits);
        return destination;
    }
    /// <summary>
    /// Performs fast merging of array with <paramref name="another"/> array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] Merge(this float[] source, float[] another)
    {
        var merged = new float[source.Length + another.Length];
        Buffer.BlockCopy(source, 0, merged, 0, source.Length * _32Bits);
        Buffer.BlockCopy(another, 0, merged, source.Length * _32Bits, another.Length * _32Bits);
        return merged;
    }
    /// <summary>
    /// Creates new array containing given array repeated <paramref name="n"/> times.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] Repeat(this float[] source, int n)
    {
        var repeated = new float[source.Length * n];
        var offset = 0;
        for (var i = 0; i < n; i++)
        {
            Buffer.BlockCopy(source, 0, repeated, offset * _32Bits, source.Length * _32Bits);
            offset += source.Length;
        }
        return repeated;
    }

    /// <summary>
    /// Creates new zero-padded array of given <paramref name="size"/> from given array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] PadZeros(this float[] source, int size)
    {
        var zeroPadded = new float[size];
        Buffer.BlockCopy(source, 0, zeroPadded, 0, source.Length * _32Bits);
        return zeroPadded;
    }
    /// <summary>
    /// Creates new zero-padded array of given <paramref name="size"/> from given array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] PadZeros<T>(this T[] source, int size)
    {
        var zeroPadded = new T[size];
        Array.Copy(source, zeroPadded, size);
        return zeroPadded;
    }
    #endregion

    #region double precision
    private const byte _64Bits = sizeof(double);
    /// <summary>
    /// Creates fast copy of array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] FastCopy(this double[] source)
    {
        var destination = new double[source.Length];
        Buffer.BlockCopy(source, 0, destination, 0, source.Length * _64Bits);
        return destination;
    }
    /// <summary>
    /// Makes fast copy of array (or its part) to existing <paramref name="destination"/> array (or its part).
    /// </summary>
    /// <param name="source">Source array</param>
    /// <param name="destination">Destination array</param>
    /// <param name="size">Number of elements to copy</param>
    /// <param name="sourceOffset">Offset in source array</param>
    /// <param name="destinationOffset">Offset in destination array</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FastCopyTo(this double[] source, double[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)
    {
        Buffer.BlockCopy(source, sourceOffset * _64Bits, destination, destinationOffset * _64Bits, size * _64Bits);
    }
    /// <summary>
    /// Makes fast copy of array fragment starting at specified offset.
    /// </summary>
    /// <param name="source">Source array</param>
    /// <param name="size">Number of elements to copy</param>
    /// <param name="sourceOffset">Offset in source array</param>
    /// <param name="destinationOffset">Offset in destination array</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] FastCopyFragment(this double[] source, int size, int sourceOffset = 0, int destinationOffset = 0)
    {
        var totalSize = size + destinationOffset;
        var destination = new double[totalSize];
        Buffer.BlockCopy(source, sourceOffset * _64Bits, destination, destinationOffset * _64Bits, size * _64Bits);
        return destination;
    }
    /// <summary>
    /// Performs fast merging of array with <paramref name="another"/> array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] Merge(this double[] source, double[] another)
    {
        var merged = new double[source.Length + another.Length];
        Buffer.BlockCopy(source, 0, merged, 0, source.Length * _64Bits);
        Buffer.BlockCopy(another, 0, merged, source.Length * _64Bits, another.Length * _64Bits);
        return merged;
    }
    /// <summary>
    /// Creates new array containing given array repeated <paramref name="n"/> times.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] Repeat(this double[] source, int n)
    {
        var repeated = new double[source.Length * n];
        var offset = 0;
        for (var i = 0; i < n; i++)
        {
            Buffer.BlockCopy(source, 0, repeated, offset * _64Bits, source.Length * _64Bits);
            offset += source.Length;
        }
        return repeated;
    }
    /// <summary>
    /// Creates new zero-padded array of given <paramref name="size"/> from given array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] PadZeros(this double[] source, int size)
    {
        var zeroPadded = new double[size];
        Buffer.BlockCopy(source, 0, zeroPadded, 0, source.Length * _64Bits);
        return zeroPadded;
    }

    #endregion
    /// <summary>
    /// Gets the last element of the array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Last<T>(this T[] array) => array[array.Length - 1];// array[^1];
    /// <summary>
    /// Gets the first element of the array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T First<T>(this T[] array) => array[0];

}
