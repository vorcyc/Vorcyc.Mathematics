namespace Vorcyc.Mathematics;

using System.ComponentModel.Design;
using System.Numerics;
using System.Runtime.InteropServices;

public static partial class ArrayExtension
{

    #region Generic

    public static T[] Copy<T>(this T[] source)
    {
        var result = new T[source.Length];
        Array.Copy(source, result, source.Length);
        return result;
    }

    public static T[] Copy<T>(this T[] source, int length)
    {
        var result = new T[length];
        Array.Copy(source, result, length);
        return result;
    }


    public static T[]? InitializeArray<T>(int length, T initialValue = default)
    {
        if (length < 0)
        {
            return default;
        }

        var array = new T[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = initialValue;
        }
        return array;
    }



    /// <summary>
    /// Fills the entire array with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="value">The value to fill the array with.</param>
    public static void Fill<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }

    /// <summary>
    /// Fills a specified range of the array with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="start">The start index of the range to fill.</param>
    /// <param name="end">The end index of the range to fill.</param>
    /// <param name="value">The value to fill the array with.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the start or end index is out of range.</exception>
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
    /// Fills a specified range of the array with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="range">The range to fill.</param>
    /// <param name="value">The value to fill the array with.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the range is out of bounds.</exception>
    public static void Fill<T>(this Span<T> array, Range range, T value)
    {
        if (array.IsEmpty) throw new ArgumentNullException(nameof(array));

        var (offset, length) = range.GetOffsetAndLength(array.Length);
        for (int i = offset; i < offset + length; i++)
        {
            array[i] = value;
        }
    }

    /// <summary>
    /// Fills the span with random float numbers.
    /// </summary>
    /// <param name="span">The span to fill.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the span length is less than 1.</exception>
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
    /// <typeparam name="T">The type of the elements in the array, which must implement IFloatingPointIeee754.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    public static void FillWithRandomNumber<T>(this T[] array)
        where T : IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));

        for (int i = 0; i < array.Length; i++)
            array[i] = T.CreateTruncating(Random.Shared.NextDouble());
    }

    /// <summary>
    /// Fills a specified range of the array with random numbers.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array, which must implement IFloatingPointIeee754.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="range">The range to fill.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the range is out of bounds.</exception>
    public static void FillWithRandomNumber<T>(this T[] array, Range range)
        where T : IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));

        var (offset, length) = range.GetOffsetAndLength(array.Length);
        for (int i = offset; i < offset + length; i++)
            array[i] = T.CreateTruncating(Random.Shared.NextDouble());
    }



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



    public static void FillWithRandomNumber(this int[] array, Range range)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        var (offset, length) = range.GetOffsetAndLength(array.Length);
        for (int i = offset; i < offset + length; i++)
            array[i] = Random.Shared.Next();
    }


    public static void FillWithRandomNumber(this long[] array)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        for (int i = 0; i < array.Length; i++)
            array[i] = Random.Shared.NextInt64();
    }



    public static void FillWithRandomNumber(this long[] array, Range range)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        var (offset, length) = range.GetOffsetAndLength(array.Length);
        for (int i = offset; i < offset + length; i++)
            array[i] = Random.Shared.NextInt64();
    }

























    /// <summary>
    /// 取内部一段，并返回迭代集。
    /// </summary>
    /// <remarks>
    /// 可以使用LINQ提供的扩展方法 System.Linq.Enumerable.Skip(start).Take(length)）实现同样功能。
    /// 但本版本性能更高。
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">源数组</param>
    /// <param name="start">起始索引</param>
    /// <param name="length">长度</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
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
    /// 取一个数组的内部片段，并返回片段。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">源数组</param>
    /// <param name="start">起始索引</param>
    /// <param name="length">长度</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static T[] GetInnerArray<T>(this T[] array, int start, int length)
    /*where T : struct , IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>*/
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
    /// 移出数组中的一部分，并返回移出后的新数组。
    /// </summary>
    /// <remarks>
    /// 本方法会进行安全检查，因此即使索引越界也不会抛出异常。
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">源数组</param>
    /// <param name="start">待移出部分的起始索引</param>
    /// <param name="length">待移出部分的长度</param>
    /// <returns>返回移出指定段后的数组</returns>
    public static T[] RemoveSegment<T>(this T[] array, int start, int length)
    {
        var result = new T[array.Length - length];
        Array.Copy(array, 0, result, 0, start);
        Array.Copy(array, start + length, result, start, array.Length - start - length);
        return result;
    }


    /// <summary>
    /// 联接两个数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="leading">前置数组</param>
    /// <param name="following">后置数组</param>
    /// <returns>返回联接后的新数组</returns>
    public static T[] Merge<T>(this T[] leading, T[] following)
    {
        var result = new T[leading.Length + following.Length];
        Array.Copy(leading, result, leading.Length);
        Array.Copy(following, 0, result, leading.Length, following.Length);
        return result;
    }



    public static string ToString<T>(this IEnumerable<T> collection)
    {
        return "[" + string.Join(",", collection) + "]";
    }


    public static T[] FastCopyFragment<T>(this T[] source, int size, int sourceOffset = 0, int destinationOffset = 0)
        where T : unmanaged
    {
        //var totalSize = size + destinationOffset;
        //var destination = new T[totalSize];
        //Buffer.BlockCopy(source, sourceOffset * Marshal.SizeOf<T>(), destination, destinationOffset * Marshal.SizeOf<T>(), size * Marshal.SizeOf<T>());
        //return destination;

        var totalSize = size + destinationOffset;
        var destination = new T[totalSize];
        Array.Copy(source, sourceOffset, destination, destinationOffset, size);
        return destination;
    }

    public static void FastCopyTo<T>(this T[] source, T[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)
    {
        //Buffer.BlockCopy(source, sourceOffset * _32Bits, destination, destinationOffset * _32Bits, size * _32Bits);
        Array.Copy(source, sourceOffset, destination, destinationOffset, size);
    }


    /// <summary>
    /// Creates new array containing given array repeated <paramref name="n"/> times.
    /// </summary>
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
    public static float[] ToFloats(this IEnumerable<double> values)
    {
        return values.Select(v => (float)v).ToArray();
    }

    /// <summary>
    /// Creates array of double-precision values from enumerable of single-precision values.
    /// </summary>
    public static double[] ToDoubles(this IEnumerable<float> values)
    {
        return values.Select(v => (double)v).ToArray();
    }



    #region single precision


    private const byte _32Bits = sizeof(float);

    /// <summary>
    /// Creates fast copy of array.
    /// </summary>
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
    public static float[] PadZeros(this float[] source, int size)
    {
        var zeroPadded = new float[size];
        Buffer.BlockCopy(source, 0, zeroPadded, 0, source.Length * _32Bits);
        return zeroPadded;
    }


    /// Creates new zero-padded array of given <paramref name="size"/> from given array.
    /// </summary>
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
    public static double[] PadZeros(this double[] source, int size)
    {
        var zeroPadded = new double[size];
        Buffer.BlockCopy(source, 0, zeroPadded, 0, source.Length * _64Bits);
        return zeroPadded;
    }



    #endregion




    public static T Last<T>(this T[] array) => array[array.Length - 1];// array[^1];



    public static T First<T>(this T[] array) => array[0];



















}