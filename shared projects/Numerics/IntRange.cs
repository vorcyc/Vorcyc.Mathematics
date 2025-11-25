namespace Vorcyc.Mathematics.Numerics;

using System;
using System.Collections.Generic;

/// <summary>
/// 表示一个整数范围，具有最小值和最大值。
/// </summary>
/// <remarks>
/// 该类表示一个包含最小值和最大值的整数范围，其中范围的最小值和最大值都包含在内。
/// 这种范围的数学表示法为 <b>[min, max]</b>。
/// </remarks>
[Serializable]
public struct IntRange : IEquatable<IntRange>, IEnumerable<int>
{
    /// <summary>
    /// 获取或设置范围的最小值。
    /// </summary>
    public int Min { get; set; }

    /// <summary>
    /// 获取或设置范围的最大值。
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    /// 获取范围的长度，定义为 (max - min)。
    /// </summary>
    public int Length => Max - Min;

    /// <summary>
    /// 初始化 <see cref="IntRange"/> 类的新实例。
    /// </summary>
    /// <param name="min">范围的最小值。</param>
    /// <param name="max">范围的最大值。</param>
    public IntRange(int min, int max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// 检查指定值是否在范围内。
    /// </summary>
    /// <param name="x">要检查的值。</param>
    /// <returns>如果指定值在范围内，则为 <b>true</b>；否则为 <b>false</b>。</returns>
    public bool IsInside(int x) => x >= Min && x <= Max;

    /// <summary>
    /// 计算两个范围之间的交集。
    /// </summary>
    /// <param name="range">要计算交集的第二个范围。</param>
    /// <returns>包含此范围与作为参数给定的 <paramref name="range"/> 之间交集的新 <see cref="IntRange"/> 结构。</returns>
    public IntRange Intersection(IntRange range) => new IntRange(Math.Max(Min, range.Min), Math.Min(Max, range.Max));

    /// <summary>
    /// 检查指定范围是否在范围内。
    /// </summary>
    /// <param name="range">要检查的范围。</param>
    /// <returns>如果指定范围在范围内，则为 <b>true</b>；否则为 <b>false</b>。</returns>
    public bool IsInside(IntRange range) => IsInside(range.Min) && IsInside(range.Max);

    /// <summary>
    /// 检查指定范围是否与范围重叠。
    /// </summary>
    /// <param name="range">要检查重叠的范围。</param>
    /// <returns>如果指定范围与范围重叠，则为 <b>true</b>；否则为 <b>false</b>。</returns>
    public bool IsOverlapping(IntRange range) => IsInside(range.Min) || IsInside(range.Max) || range.IsInside(Min) || range.IsInside(Max);

    /// <summary>
    /// 确定两个实例是否相等。
    /// </summary>
    public static bool operator ==(IntRange range1, IntRange range2) => range1.Min == range2.Min && range1.Max == range2.Max;

    /// <summary>
    /// 确定两个实例是否不相等。
    /// </summary>
    public static bool operator !=(IntRange range1, IntRange range2) => !(range1 == range2);

    /// <summary>
    /// 指示当前对象是否等于同一类型的另一个对象。
    /// </summary>
    /// <param name="other">要与此对象进行比较的对象。</param>
    /// <returns>如果当前对象等于 <paramref name="other"/> 参数，则为 true；否则为 false。</returns>
    public bool Equals(IntRange other) => this == other;

    /// <summary>
    /// 确定指定的 <see cref="System.Object"/> 是否等于此实例。
    /// </summary>
    /// <param name="obj">要与此实例进行比较的 <see cref="System.Object"/>。</param>
    /// <returns>如果指定的 <see cref="System.Object"/> 等于此实例，则为 true；否则为 false。</returns>
    public override bool Equals(object obj) => obj is IntRange range && this == range;

    /// <summary>
    /// 返回此实例的哈希代码。
    /// </summary>
    /// <returns>此实例的哈希代码，适用于哈希算法和数据结构（如哈希表）。</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Min.GetHashCode();
            hash = hash * 31 + Max.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// 返回表示此实例的 <see cref="System.String"/>。
    /// </summary>
    /// <returns>表示此实例的 <see cref="System.String"/>。</returns>
    public override string ToString() => $"[{Min}, {Max}]";

    /// <summary>
    /// 返回表示此实例的 <see cref="System.String"/>。
    /// </summary>
    /// <param name="format">格式字符串。</param>
    /// <param name="formatProvider">格式提供程序。</param>
    /// <returns>表示此实例的 <see cref="System.String"/>。</returns>
    public string ToString(string format, IFormatProvider formatProvider) => $"[{Min.ToString(format, formatProvider)}, {Max.ToString(format, formatProvider)}]";

    /// <summary>
    /// 返回一个枚举器，该枚举器遍历集合。
    /// </summary>
    /// <returns>可用于遍历集合的 <see cref="T:System.Collections.IEnumerator"/> 对象。</returns>
    public IEnumerator<int> GetEnumerator()
    {
        for (int i = Min; i <= Max; i++)
            yield return i;
    }

    /// <summary>
    /// 返回一个枚举器，该枚举器遍历集合。
    /// </summary>
    /// <returns>可用于遍历集合的 <see cref="T:System.Collections.IEnumerator"/> 对象。</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
