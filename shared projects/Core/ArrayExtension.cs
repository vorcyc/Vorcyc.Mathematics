namespace Vorcyc.Mathematics;

using System.Numerics;
using System.Runtime.InteropServices;

public static partial class ArrayExtension
{

    #region Generic

    /// <summary>
    /// 复制数组。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="source">源数组。</param>
    /// <returns>返回复制后的新数组。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Copy<T>(this T[] source)
    {
        var result = new T[source.Length];
        Array.Copy(source, result, source.Length);
        return result;
    }

    /// <summary>
    /// 复制数组的指定长度。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="source">源数组。</param>
    /// <param name="length">要复制的长度。</param>
    /// <returns>返回复制后的新数组。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Copy<T>(this T[] source, int length)
    {
        var result = new T[length];
        Array.Copy(source, result, length);
        return result;
    }

    /// <summary>
    /// 初始化一个指定长度的数组，并用初始值填充。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="length">数组的长度。</param>
    /// <param name="initialValue">初始值。</param>
    /// <returns>返回初始化后的数组。</returns>
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
    /// 用指定值填充整个数组。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="array">要填充的数组。</param>
    /// <param name="value">填充的值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }

    /// <summary>
    /// 用指定值填充数组的指定范围。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="array">要填充的数组。</param>
    /// <param name="start">起始索引。</param>
    /// <param name="end">结束索引。</param>
    /// <param name="value">填充的值。</param>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
    /// <exception cref="ArgumentOutOfRangeException">当起始或结束索引超出范围时抛出。</exception>
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
    /// 用指定值填充数组的指定范围。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="array">要填充的数组。</param>
    /// <param name="range">要填充的范围。</param>
    /// <param name="value">填充的值。</param>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
    /// <exception cref="ArgumentOutOfRangeException">当范围超出数组边界时抛出。</exception>
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
    /// 用指定值填充数组的指定范围。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="values">指定的<see cref="Span{T}"/></param>
    /// <param name="value">填充的值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(this Span<T> values, T value)
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = value;
        }
    }

    /// <summary>
    /// 用随机浮点数填充数组。
    /// </summary>
    /// <param name="span">要填充的数组。</param>
    /// <exception cref="ArgumentOutOfRangeException">当数组长度小于1时抛出。</exception>
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
    /// 用随机数填充数组。
    /// </summary>
    /// <typeparam name="T">数组元素的类型，必须实现 IFloatingPointIeee754 接口。</typeparam>
    /// <param name="array">要填充的数组。</param>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber<T>(this T[] array)
        where T : IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));

        for (int i = 0; i < array.Length; i++)
            array[i] = T.CreateTruncating(Random.Shared.NextDouble());
    }

    /// <summary>
    /// 用随机数填充数组的指定范围。
    /// </summary>
    /// <typeparam name="T">数组元素的类型，必须实现 IFloatingPointIeee754 接口。</typeparam>
    /// <param name="array">要填充的数组。</param>
    /// <param name="range">要填充的范围。</param>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
    /// <exception cref="ArgumentOutOfRangeException">当范围超出数组边界时抛出。</exception>
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
    /// 用随机数填充数组。
    /// </summary>
    /// <param name="array">要填充的数组。</param>
    /// <param name="limit">随机数的范围。</param>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
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
    /// 用随机数填充数组的指定范围。
    /// </summary>
    /// <param name="array">要填充的数组。</param>
    /// <param name="range">要填充的范围。</param>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber(this int[] array, Range range)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        var (offset, length) = range.GetOffsetAndLength(array.Length);
        for (int i = offset; i < offset + length; i++)
            array[i] = Random.Shared.Next();
    }

    /// <summary>
    /// 用随机数填充数组。
    /// </summary>
    /// <param name="array">要填充的数组。</param>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber(this long[] array)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        for (int i = 0; i < array.Length; i++)
            array[i] = Random.Shared.NextInt64();
    }

    /// <summary>
    /// 用随机数填充数组的指定范围。
    /// </summary>
    /// <param name="array">要填充的数组。</param>
    /// <param name="range">要填充的范围。</param>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillWithRandomNumber(this long[] array, Range range)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(array));
        var (offset, length) = range.GetOffsetAndLength(array.Length);
        for (int i = offset; i < offset + length; i++)
            array[i] = Random.Shared.NextInt64();
    }

    /// <summary>
    /// 用指定的起始值和步长填充数组。
    /// </summary>
    /// <typeparam name="T">数组元素的类型，必须实现 INumber 接口。</typeparam>
    /// <param name="array">要填充的数组。</param>
    /// <param name="startValue">起始值。</param>
    /// <param name="step">步长。</param>
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
    /// 用指定的起始值和步长填充数组。
    /// </summary>
    /// <typeparam name="T">数组元素的类型，必须实现 INumber 接口。</typeparam>
    /// <param name="span">要填充的数组。</param>
    /// <param name="startValue">起始值。</param>
    /// <param name="step">步长。</param>
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
    /// 取内部一段，并返回迭代集。
    /// </summary>
    /// <remarks>
    /// 可以使用LINQ提供的扩展方法 System.Linq.Enumerable.Skip(start).Take(length)）实现同样功能。
    /// 但本版本性能更高。
    /// </remarks>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="array">源数组。</param>
    /// <param name="start">起始索引。</param>
    /// <param name="length">长度。</param>
    /// <returns>返回内部片段的迭代集。</returns>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
    /// <exception cref="ArgumentOutOfRangeException">当起始索引或长度超出范围时抛出。</exception>
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
    /// 取一个数组的内部片段，并返回片段。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="array">源数组。</param>
    /// <param name="start">起始索引。</param>
    /// <param name="length">长度。</param>
    /// <returns>返回内部片段的数组。</returns>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
    /// <exception cref="ArgumentOutOfRangeException">当起始索引或长度超出范围时抛出。</exception>
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
    /// 移出数组中的一部分，并返回移出后的新数组。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="array">源数组。</param>
    /// <param name="start">待移出部分的起始索引。</param>
    /// <param name="length">待移出部分的长度。</param>
    /// <returns>返回移出指定段后的数组。</returns>
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
    /// 联接两个数组。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="leading">前置数组，不能为 null。</param>
    /// <param name="following">后置数组，不能为 null。</param>
    /// <returns>返回联接后的新数组。如果任一输入为 null，则抛出 <see cref="ArgumentNullException"/>。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="leading"/> 或 <paramref name="following"/> 为 null 时抛出。</exception>
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
    /// 将集合转换为字符串。
    /// </summary>
    /// <typeparam name="T">集合元素的类型。</typeparam>
    /// <param name="collection">要转换的集合。</param>
    /// <returns>返回表示集合的字符串。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToString<T>(this IEnumerable<T> collection)
    {
        return "[" + string.Join(",", collection) + "]";
    }

    /// <summary>
    /// 快速复制数组的片段。
    /// </summary>
    /// <typeparam name="T">数组元素的类型，必须是非托管类型。</typeparam>
    /// <param name="source">源数组。</param>
    /// <param name="size">要复制的大小。</param>
    /// <param name="sourceOffset">源数组的偏移量。</param>
    /// <param name="destinationOffset">目标数组的偏移量。</param>
    /// <returns>返回复制后的新数组。</returns>
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
    /// 快速复制数组到目标数组。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="source">源数组。</param>
    /// <param name="destination">目标数组。</param>
    /// <param name="size">要复制的大小。</param>
    /// <param name="sourceOffset">源数组的偏移量。</param>
    /// <param name="destinationOffset">目标数组的偏移量。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FastCopyTo<T>(this T[] source, T[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)
    {
        Array.Copy(source, sourceOffset, destination, destinationOffset, size);
    }

    /// <summary>
    /// 创建一个包含给定数组重复 <paramref name="n"/> 次的新数组。
    /// </summary>
    /// <typeparam name="T">数组元素的类型。</typeparam>
    /// <param name="source">源数组。</param>
    /// <param name="n">重复次数。</param>
    /// <returns>返回重复后的新数组。</returns>
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