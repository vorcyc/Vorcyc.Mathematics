//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Vorcyc.Mathematics;

//public static class ArrayExtension2
//{


//#if NET10_0_OR_GREATER



//    #region For [source] instance array

//    extension<T>(T[] source)
//    {

//        /// <summary>
//        /// Creates a new array that is a copy of the source array.
//        /// </summary>
//        /// <returns>A new array containing the elements of the source array in the same order.</returns>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public T[] Copy()
//        {
//            ArgumentNullException.ThrowIfNull(source);
//            T[] copy = new T[source.Length];
//            Array.Copy(source, copy, source.Length);
//            return copy;
//        }

//        /// <summary>
//        /// Creates a new array containing a copy of the first specified number of elements from the source array.
//        /// </summary>
//        /// <param name="length">The number of elements to copy from the beginning of the source array. Must be non-negative and less than or
//        /// equal to the length of the source array.</param>
//        /// <returns>A new array containing the copied elements. The length of the returned array is equal to the specified
//        /// length.</returns>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public T[] Copy(int length)
//        {
//            var result = new T[length];
//            Array.Copy(source, result, length);
//            return result;
//        }


//        /// <summary>
//        /// 用指定值填充数组的指定范围。
//        /// </summary>
//        /// <typeparam name="T">数组元素的类型。</typeparam>
//        /// <param name="array">要填充的数组。</param>
//        /// <param name="start">起始索引。</param>
//        /// <param name="end">结束索引。</param>
//        /// <param name="value">填充的值。</param>
//        /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
//        /// <exception cref="ArgumentOutOfRangeException">当起始或结束索引超出范围时抛出。</exception>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public void Fill(int start, int end, T value)
//        {
//            ArgumentNullException.ThrowIfNull(source);

//            if (start < 0 || start > end)
//                throw new ArgumentOutOfRangeException(nameof(start));

//            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(end, source.Length);

//            for (int i = start; i < end; i++)
//            {
//                source[i] = value;
//            }
//        }

//    }

//    #endregion






//#endif
//}
