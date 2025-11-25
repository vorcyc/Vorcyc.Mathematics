namespace Vorcyc.Mathematics.Helpers;

/// <summary>
/// 为 <see cref="string"/> 提供扩展成员的工具类。
/// 注意：本文件使用“extension”扩展成员语法（假定 C# 未来版本支持），
/// 以便像实例方法一样扩展字符串以及定义运算符。
/// </summary>
public static class StringExtension
{
    // 该块为对 string 的扩展成员定义区。
    // 在此块内可将 string 当作“实例”（这里用名为 str 的接收者）来访问。
    extension(string str)
    {
        /// <summary>
        /// 截断当前字符串到指定的最大长度。
        /// </summary>
        /// <param name="maxLength">最大长度（小于等于 0 时返回空字符串）。</param>
        /// <returns>
        /// 当 <c>str</c> 为空或 <paramref name="maxLength"/> 小于等于 0 时返回空字符串；
        /// 否则返回不超过指定长度的子串。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Truncate(int maxLength)
        {
            // 空或无效长度，直接返回空
            if (string.IsNullOrEmpty(str) || maxLength <= 0)
            {
                return string.Empty;
            }
            // 如果长度已不超过限制，原样返回；否则截断
            return str.Length <= maxLength ? str : str.Substring(0, maxLength);
        }

        /// <summary>
        /// 使用系统路径分隔符将两个字符串进行路径样式的拼接。
        /// </summary>
        /// <param name="str1">左操作数（路径/片段）。</param>
        /// <param name="str2">右操作数（路径/片段）。</param>
        /// <returns>基于当前平台的路径合并结果。</returns>
        /// <remarks>
        /// 本运算符等价于 <see cref="System.IO.Path.Combine(string, string)"/>。
        /// 若用于一般字符串拼接，请注意其会插入路径分隔符。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string operator /(string str1, string str2)
        {
            // 使用 Path.Combine 执行跨平台路径拼接
            return System.IO.Path.Combine(str1, str2);
        }

        ///// <summary>
        ///// 统计当前字符串中指定字符的出现次数。
        ///// </summary>
        ///// <param name="c">要统计的字符。</param>
        ///// <returns>出现次数；当 <c>str</c> 为空时返回 0。</returns>
        //public int GetCharCount(char c)
        //{
        //    if (string.IsNullOrEmpty(str))
        //    {
        //        return 0;
        //    }
        //    int count = 0;
        //    // 逐字符扫描并计数
        //    foreach (char ch in str)
        //    {
        //        if (ch == c)
        //        {
        //            count++;
        //        }
        //    }
        //    return count;
        //}

        /// <summary>
        /// 统计当前字符串中指定字符的出现次数。
        /// </summary>
        /// <param name="c">要统计的字符。</param>
        /// <returns>出现次数；当 <c>str</c> 为空时返回 0。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCharCount(char c)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            return str.AsSpan().Count(c); // .NET 8+ System.MemoryExtensions.Count 使用 SIMD，远超手动循环
        }


        ///// <summary>
        ///// 在当前字符串中，从指定起始索引开始查找某字符的第 <paramref name="occurrence"/> 次出现位置。
        ///// </summary>
        ///// <param name="c">要查找的字符。</param>
        ///// <param name="startIndex">起始索引（包含）。</param>
        ///// <param name="occurrence">要查找的出现序号（1 表示第一次出现）。</param>
        ///// <returns>
        ///// 找到则返回索引；否则返回 -1。
        ///// 当 <c>str</c> 为空、<paramref name="occurrence"/> ≤ 0、
        ///// 或 <paramref name="startIndex"/> 越界时，直接返回 -1。
        ///// </returns>
        //public int GetIndexOf(char c, int startIndex, int occurrence)
        //{
        //    if (string.IsNullOrEmpty(str) || occurrence <= 0 || startIndex < 0 || startIndex >= str.Length)
        //    {
        //        return -1;
        //    }
        //    int count = 0;
        //    // 从起始位置向后寻找第 occurrence 次匹配
        //    for (int i = startIndex; i < str.Length; i++)
        //    {
        //        if (str[i] == c)
        //        {
        //            count++;
        //            if (count == occurrence)
        //            {
        //                return i;
        //            }
        //        }
        //    }
        //    return -1;
        //}


        /// <summary>
        /// 在当前字符串中，从指定起始索引开始查找某字符的第 <paramref name="occurrence"/> 次出现位置。
        /// </summary>
        /// <param name="c">要查找的字符。</param>
        /// <param name="startIndex">起始索引（包含）。</param>
        /// <param name="occurrence">要查找的出现序号（1 表示第一次出现）。</param>
        /// <returns>
        /// 找到则返回索引；否则返回 -1。
        /// 当 <c>str</c> 为空、<paramref name="occurrence"/> ≤ 0、
        /// 或 <paramref name="startIndex"/> 越界时，直接返回 -1。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndexOf(char c, int startIndex, int occurrence)
        {
            if (string.IsNullOrEmpty(str) || occurrence <= 0 || startIndex < 0 || startIndex >= str.Length)
                return -1;

            var span = str.AsSpan(startIndex);
            int found = 0;

            while (true)
            {
                int idx = span.IndexOf(c);
                if (idx == -1)
                    return -1;

                if (++found == occurrence)
                    return startIndex + idx;

                // 跳过当前匹配的字符，继续查找
                span = span.Slice(idx + 1);
                startIndex += idx + 1;
            }
        }


        ///// <summary>
        ///// 统计当前字符串中子串的非重叠出现次数。
        ///// </summary>
        ///// <param name="substring">要统计的子串。</param>
        ///// <param name="comparisonType">比较方式（默认区分大小写，Ordinal）。</param>
        ///// <returns>非重叠出现次数；当任一为空时返回 0。</returns>
        //public int GetSubStringCount(string substring, StringComparison comparisonType = StringComparison.Ordinal)
        //{
        //    if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(substring))
        //    {
        //        return 0;
        //    }
        //    int count = 0;
        //    int index = 0;
        //    // 每次从上次匹配末尾继续，确保不重叠
        //    while ((index = str.IndexOf(substring, index, comparisonType)) != -1)
        //    {
        //        count++;
        //        index += substring.Length;
        //    }
        //    return count;
        //}


        /// <summary>
        /// 统计当前字符串中子串的非重叠出现次数。
        /// </summary>
        /// <param name="substring">要统计的子串。</param>
        /// <param name="comparisonType">比较方式（默认区分大小写，Ordinal）。</param>
        /// <returns>非重叠出现次数；当任一为空时返回 0。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSubStringCount(string substring, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(substring))
                return 0;

            var source = str.AsSpan();
            var target = substring.AsSpan();
            int count = 0;
            int index = 0;

            while (true)
            {
                int pos = source[index..].IndexOf(target, comparisonType);
                if (pos == -1)
                    break;

                count++;
                index += pos + target.Length;
            }

            return count;
        }

        /// <summary>
        /// 获取当前字符串中，首个由起止分隔符包围的子串。
        /// </summary>
        /// <param name="startDelimiter">起始分隔符。</param>
        /// <param name="endDelimiter">结束分隔符。</param>
        /// <param name="comparisonType">比较方式（默认区分大小写，Ordinal）。</param>
        /// <returns>
        /// 成功则返回位于两个分隔符之间的子串；否则返回空字符串。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetSubStringByDelimiter(string startDelimiter, string endDelimiter, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(startDelimiter) || string.IsNullOrEmpty(endDelimiter))
            {
                return string.Empty;
            }
            // 查找起始分隔符
            int startIndex = str.IndexOf(startDelimiter, comparisonType);
            if (startIndex == -1)
            {
                return string.Empty;
            }
            startIndex += startDelimiter.Length;
            // 从起始之后查找结束分隔符
            int endIndex = str.IndexOf(endDelimiter, startIndex, comparisonType);
            if (endIndex == -1)
            {
                return string.Empty;
            }
            return str.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// 获取当前字符串中，第 <paramref name="occurrence"/> 次由起止分隔符包围的子串。
        /// </summary>
        /// <param name="startDelimiter">起始分隔符。</param>
        /// <param name="endDelimiter">结束分隔符。</param>
        /// <param name="occurrence">目标出现序号（1 表示第一次出现）。</param>
        /// <param name="comparisonType">比较方式（默认区分大小写，Ordinal）。</param>
        /// <returns>
        /// 成功则返回对应的子串；否则返回空字符串。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetSubStringByDelimiter(string startDelimiter, string endDelimiter, int occurrence, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(startDelimiter) || string.IsNullOrEmpty(endDelimiter) || occurrence <= 0)
            {
                return string.Empty;
            }
            int count = 0;
            int searchIndex = 0;
            // 循环查找每一段由分隔符包围的片段，直到达到指定序号
            while (count < occurrence)
            {
                int startIndex = str.IndexOf(startDelimiter, searchIndex, comparisonType);
                if (startIndex == -1)
                {
                    return string.Empty;
                }
                startIndex += startDelimiter.Length;

                int endIndex = str.IndexOf(endDelimiter, startIndex, comparisonType);
                if (endIndex == -1)
                {
                    return string.Empty;
                }

                count++;
                if (count == occurrence)
                {
                    return str.Substring(startIndex, endIndex - startIndex);
                }

                // 从当前结束分隔符之后继续搜索
                searchIndex = endIndex + endDelimiter.Length;
            }
            return string.Empty;
        }
    }
}