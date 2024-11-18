using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Helpers;

//public static class SpanMathExtension
//{
///// <summary>
///// 将指定的数值加到 Span 的每个元素上。
///// </summary>
///// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
///// <param name="left">要进行加法操作的 Span。</param>
///// <param name="right">要加到每个元素上的数值。</param>
//[MethodImpl(MethodImplOptions.AggressiveInlining)]
//public static void Add<T>(this Span<T> left, T right) where T : struct, INumber<T>
//{
//    for (int i = 0; i < left.Length; i++)
//    {
//        left[i] += right;
//    }
//}

///// <summary>
///// 将两个 Span 的对应元素相加。
///// </summary>
///// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
///// <param name="left">要进行加法操作的第一个 Span。</param>
///// <param name="right">要加到第一个 Span 上的第二个 Span。</param>
//[MethodImpl(MethodImplOptions.AggressiveInlining)]
//public static void Add<T>(this Span<T> left, Span<T> right) where T : struct, INumber<T>
//{
//    for (int i = 0; i < left.Length; i++)
//    {
//        left[i] += right[i];
//    }
//}

///// <summary>
///// 从 Span 的每个元素中减去指定的数值。
///// </summary>
///// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
///// <param name="left">要进行减法操作的 Span。</param>
///// <param name="right">要从每个元素中减去的数值。</param>
//[MethodImpl(MethodImplOptions.AggressiveInlining)]
//public static void Subtract<T>(this Span<T> left, T right) where T : struct, INumber<T>
//{
//    for (int i = 0; i < left.Length; i++)
//    {
//        left[i] -= right;
//    }
//}

///// <summary>
///// 将两个 Span 的对应元素相减。
///// </summary>
///// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
///// <param name="left">要进行减法操作的第一个 Span。</param>
///// <param name="right">要从第一个 Span 中减去的第二个 Span。</param>
//[MethodImpl(MethodImplOptions.AggressiveInlining)]
//public static void Subtract<T>(this Span<T> left, Span<T> right) where T : struct, INumber<T>
//{
//    for (int i = 0; i < left.Length; i++)
//    {
//        left[i] -= right[i];
//    }
//}

///// <summary>
///// 将 Span 的每个元素乘以指定的数值。
///// </summary>
///// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
///// <param name="left">要进行乘法操作的 Span。</param>
///// <param name="right">要乘以每个元素的数值。</param>
//[MethodImpl(MethodImplOptions.AggressiveInlining)]
//public static void Multiply<T>(this Span<T> left, T right) where T : struct, INumber<T>
//{
//    for (int i = 0; i < left.Length; i++)
//    {
//        left[i] *= right;
//    }
//}

///// <summary>
///// 将两个 Span 的对应元素相乘。
///// </summary>
///// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
///// <param name="left">要进行乘法操作的第一个 Span。</param>
///// <param name="right">要乘以第一个 Span 的第二个 Span。</param>
//[MethodImpl(MethodImplOptions.AggressiveInlining)]
//public static void Multiply<T>(this Span<T> left, Span<T> right) where T : struct, INumber<T>
//{
//    for (int i = 0; i < left.Length; i++)
//    {
//        left[i] *= right[i];
//    }
//}

///// <summary>
///// 将 Span 的每个元素除以指定的数值。
///// </summary>
///// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
///// <param name="left">要进行除法操作的 Span。</param>
///// <param name="right">要除以每个元素的数值。</param>
//[MethodImpl(MethodImplOptions.AggressiveInlining)]
//public static void Divide<T>(this Span<T> left, T right) where T : struct, INumber<T>
//{
//    for (int i = 0; i < left.Length; i++)
//    {
//        left[i] /= right;
//    }
//}

///// <summary>
///// 将两个 Span 的对应元素相除。
///// </summary>
///// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
///// <param name="left">要进行除法操作的第一个 Span。</param>
///// <param name="right">要除以第一个 Span 的第二个 Span。</param>
//[MethodImpl(MethodImplOptions.AggressiveInlining)]
//public static void Divide<T>(this Span<T> left, Span<T> right) where T : struct, INumber<T>
//{
//    for (int i = 0; i < left.Length; i++)
//    {
//        left[i] /= right[i];
//    }
//}

//}


internal static class SpanMathExtension
{
    /// <summary>
    /// 将指定的数值加到 Span 的每个元素上。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
    /// <param name="left">要进行加法操作的 Span。</param>
    /// <param name="right">要加到每个元素上的数值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<T>(this Span<T> left, T right) where T : struct, INumber<T>
    {
        if (Vector.IsHardwareAccelerated && left.Length >= Vector<T>.Count)
        {
            // 使用 SIMD 进行批量加法操作
            var vectorRight = new Vector<T>(right);
            int i;
            for (i = 0; i <= left.Length - Vector<T>.Count; i += Vector<T>.Count)
            {
                var vectorLeft = new Vector<T>(left.Slice(i, Vector<T>.Count));
                (vectorLeft + vectorRight).CopyTo(left.Slice(i, Vector<T>.Count));
            }
            // 处理剩余的元素
            for (; i < left.Length; i++)
            {
                left[i] += right;
            }
        }
        else
        {
            // 使用常规方法进行加法操作
            for (int i = 0; i < left.Length; i++)
            {
                left[i] += right;
            }
        }
    }

    /// <summary>
    /// 将两个 Span 的对应元素相加。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
    /// <param name="left">要进行加法操作的第一个 Span。</param>
    /// <param name="right">要加到第一个 Span 上的第二个 Span。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<T>(this Span<T> left, Span<T> right) where T : struct, INumber<T>
    {
        if (Vector.IsHardwareAccelerated && left.Length >= Vector<T>.Count)
        {
            // 使用 SIMD 进行批量加法操作
            int i;
            for (i = 0; i <= left.Length - Vector<T>.Count; i += Vector<T>.Count)
            {
                var vectorLeft = new Vector<T>(left.Slice(i, Vector<T>.Count));
                var vectorRight = new Vector<T>(right.Slice(i, Vector<T>.Count));
                (vectorLeft + vectorRight).CopyTo(left.Slice(i, Vector<T>.Count));
            }
            // 处理剩余的元素
            for (; i < left.Length; i++)
            {
                left[i] += right[i];
            }
        }
        else
        {
            // 使用常规方法进行加法操作
            for (int i = 0; i < left.Length; i++)
            {
                left[i] += right[i];
            }
        }
    }

    /// <summary>
    /// 从 Span 的每个元素中减去指定的数值。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
    /// <param name="left">要进行减法操作的 Span。</param>
    /// <param name="right">要从每个元素中减去的数值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Subtract<T>(this Span<T> left, T right) where T : struct, INumber<T>
    {
        if (Vector.IsHardwareAccelerated && left.Length >= Vector<T>.Count)
        {
            // 使用 SIMD 进行批量减法操作
            var vectorRight = new Vector<T>(right);
            int i;
            for (i = 0; i <= left.Length - Vector<T>.Count; i += Vector<T>.Count)
            {
                var vectorLeft = new Vector<T>(left.Slice(i, Vector<T>.Count));
                (vectorLeft - vectorRight).CopyTo(left.Slice(i, Vector<T>.Count));
            }
            // 处理剩余的元素
            for (; i < left.Length; i++)
            {
                left[i] -= right;
            }
        }
        else
        {
            // 使用常规方法进行减法操作
            for (int i = 0; i < left.Length; i++)
            {
                left[i] -= right;
            }
        }
    }

    /// <summary>
    /// 将两个 Span 的对应元素相减。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
    /// <param name="left">要进行减法操作的第一个 Span。</param>
    /// <param name="right">要从第一个 Span 中减去的第二个 Span。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Subtract<T>(this Span<T> left, Span<T> right) where T : struct, INumber<T>
    {
        if (Vector.IsHardwareAccelerated && left.Length >= Vector<T>.Count)
        {
            // 使用 SIMD 进行批量减法操作
            int i;
            for (i = 0; i <= left.Length - Vector<T>.Count; i += Vector<T>.Count)
            {
                var vectorLeft = new Vector<T>(left.Slice(i, Vector<T>.Count));
                var vectorRight = new Vector<T>(right.Slice(i, Vector<T>.Count));
                (vectorLeft - vectorRight).CopyTo(left.Slice(i, Vector<T>.Count));
            }
            // 处理剩余的元素
            for (; i < left.Length; i++)
            {
                left[i] -= right[i];
            }
        }
        else
        {
            // 使用常规方法进行减法操作
            for (int i = 0; i < left.Length; i++)
            {
                left[i] -= right[i];
            }
        }
    }

    /// <summary>
    /// 将 Span 的每个元素乘以指定的数值。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
    /// <param name="left">要进行乘法操作的 Span。</param>
    /// <param name="right">要乘以每个元素的数值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Multiply<T>(this Span<T> left, T right) where T : struct, INumber<T>
    {
        if (Vector.IsHardwareAccelerated && left.Length >= Vector<T>.Count)
        {
            // 使用 SIMD 进行批量乘法操作
            var vectorRight = new Vector<T>(right);
            int i;
            for (i = 0; i <= left.Length - Vector<T>.Count; i += Vector<T>.Count)
            {
                var vectorLeft = new Vector<T>(left.Slice(i, Vector<T>.Count));
                (vectorLeft * vectorRight).CopyTo(left.Slice(i, Vector<T>.Count));
            }
            // 处理剩余的元素
            for (; i < left.Length; i++)
            {
                left[i] *= right;
            }
        }
        else
        {
            // 使用常规方法进行乘法操作
            for (int i = 0; i < left.Length; i++)
            {
                left[i] *= right;
            }
        }
    }

    /// <summary>
    /// 将两个 Span 的对应元素相乘。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
    /// <param name="left">要进行乘法操作的第一个 Span。</param>
    /// <param name="right">要乘以第一个 Span 的第二个 Span。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Multiply<T>(this Span<T> left, Span<T> right) where T : struct, INumber<T>
    {
        if (Vector.IsHardwareAccelerated && left.Length >= Vector<T>.Count)
        {
            // 使用 SIMD 进行批量乘法操作
            int i;
            for (i = 0; i <= left.Length - Vector<T>.Count; i += Vector<T>.Count)
            {
                var vectorLeft = new Vector<T>(left.Slice(i, Vector<T>.Count));
                var vectorRight = new Vector<T>(right.Slice(i, Vector<T>.Count));
                (vectorLeft * vectorRight).CopyTo(left.Slice(i, Vector<T>.Count));
            }
            // 处理剩余的元素
            for (; i < left.Length; i++)
            {
                left[i] *= right[i];
            }
        }
        else
        {
            // 使用常规方法进行乘法操作
            for (int i = 0; i < left.Length; i++)
            {
                left[i] *= right[i];
            }
        }
    }

    /// <summary>
    /// 将 Span 的每个元素除以指定的数值。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
    /// <param name="left">要进行除法操作的 Span。</param>
    /// <param name="right">要除以每个元素的数值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Divide<T>(this Span<T> left, T right) where T : struct, INumber<T>
    {
        if (Vector.IsHardwareAccelerated && left.Length >= Vector<T>.Count)
        {
            // 使用 SIMD 进行批量除法操作
            var vectorRight = new Vector<T>(right);
            int i;
            for (i = 0; i <= left.Length - Vector<T>.Count; i += Vector<T>.Count)
            {
                var vectorLeft = new Vector<T>(left.Slice(i, Vector<T>.Count));
                (vectorLeft / vectorRight).CopyTo(left.Slice(i, Vector<T>.Count));
            }
            // 处理剩余的元素
            for (; i < left.Length; i++)
            {
                left[i] /= right;
            }
        }
        else
        {
            // 使用常规方法进行除法操作
            for (int i = 0; i < left.Length; i++)
            {
                left[i] /= right;
            }
        }
    }

    /// <summary>
    /// 将两个 Span 的对应元素相除。
    /// </summary>
    /// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
    /// <param name="left">要进行除法操作的第一个 Span。</param>
    /// <param name="right">要除以第一个 Span 的第二个 Span。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Divide<T>(this Span<T> left, Span<T> right) where T : struct, INumber<T>
    {
        if (Vector.IsHardwareAccelerated && left.Length >= Vector<T>.Count)
        {
            // 使用 SIMD 进行批量除法操作
            int i;
            for (i = 0; i <= left.Length - Vector<T>.Count; i += Vector<T>.Count)
            {
                var vectorLeft = new Vector<T>(left.Slice(i, Vector<T>.Count));
                var vectorRight = new Vector<T>(right.Slice(i, Vector<T>.Count));
                (vectorLeft / vectorRight).CopyTo(left.Slice(i, Vector<T>.Count));
            }
            // 处理剩余的元素
            for (; i < left.Length; i++)
            {
                left[i] /= right[i];
            }
        }
        else
        {
            // 使用常规方法进行除法操作
            for (int i = 0; i < left.Length; i++)
            {
                left[i] /= right[i];
            }
        }
    }
}

