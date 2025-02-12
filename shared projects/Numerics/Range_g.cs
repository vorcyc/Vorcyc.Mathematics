using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.Numerics;

/// <summary>
/// 表示一个范围的开区间。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 <see cref="IComparable{T}"/>、<see cref="IFormattable"/>、<see cref="IConvertible"/>、<see cref="IEquatable{T}"/> 接口。</typeparam>
public sealed class Range<T> : IEquatable<Range<T>>
    where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
{
    private T _value;

    /// <summary>
    /// 初始化 <see cref="Range{T}"/> 类的新实例。
    /// </summary>
    /// <param name="minimum">指定区间的最小值。</param>
    /// <param name="maximum">指定区间的最大值。</param>
    public Range(T minimum, T maximum)
    {
        if (maximum.LessThanOrEqual(minimum)) throw new ArgumentException("Maximum must be greater than minimum.");

        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// 初始化 <see cref="Range{T}"/> 类的新实例。
    /// </summary>
    /// <param name="minimum">指定区间的最小值。</param>
    /// <param name="maximum">指定区间的最大值。</param>
    /// <param name="value">指定的值。</param>
    public Range(T minimum, T maximum, T value)
    {
        if (maximum.LessThanOrEqual(minimum)) throw new ArgumentException("Maximum must be greater than minimum.");

        Minimum = minimum;
        Maximum = maximum;
        _value = value;
    }

    /// <summary>
    /// 获取或设置值，设置值时会限定在依极值所限定的区间内。
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            if (value.CompareTo(Maximum) > 0)
                _value = Maximum;
            else if (value.CompareTo(Minimum) <= 0)
                _value = Minimum;
            else
                _value = value;

            ValueChanged?.Invoke(this, new RangeValueChangedEventArgs<T>(_value));
            ValueChangedCallback?.Invoke(_value);
        }
    }

    /// <summary>
    /// 获取在构造时设置的最大值。
    /// </summary>
    public T Maximum { get; }

    /// <summary>
    /// 获取在构造时设置的最小值。
    /// </summary>
    public T Minimum { get; }

    /// <summary>
    /// 检查指定值是否在范围内。
    /// </summary>
    /// <param name="x">要检查的值。</param>
    /// <returns>如果指定值在范围内，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public bool IsInside(T x) => x.GreaterThanOrEqual(Minimum) && x.LessThanOrEqual(Maximum);

    /// <summary>
    /// 计算两个范围之间的交集。
    /// </summary>
    /// <param name="range">要计算交集的第二个范围。</param>
    /// <returns>包含此范围与作为参数给定的 <paramref name="range"/> 之间交集的新 <see cref="Range{T}"/> 结构。</returns>
    public Range<T> Intersection(Range<T> range)
    {
        if (Maximum.LessThan(range.Minimum) || Minimum.GreaterThan(range.Maximum))
            return null;

        return new Range<T>(Minimum.CompareMax(range.Minimum), Maximum.CompareMin(range.Maximum));
    }

    /// <summary>
    /// 计算两个范围之间的并集。
    /// </summary>
    /// <param name="range">要计算并集的第二个范围。</param>
    /// <returns>包含此范围与作为参数给定的 <paramref name="range"/> 之间并集的新 <see cref="Range{T}"/> 结构。</returns>
    public Range<T> Union(Range<T> range) => new Range<T>(Minimum.CompareMin(range.Minimum), Maximum.CompareMax(range.Maximum));

    /// <summary>
    /// 检查指定范围是否在当前范围内。
    /// </summary>
    /// <param name="range">要检查的范围。</param>
    /// <returns>如果指定范围在当前范围内，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public bool IsInside(Range<T> range) => IsInside(range.Minimum) && IsInside(range.Maximum);

    /// <summary>
    /// 检查指定范围是否与当前范围重叠。
    /// </summary>
    /// <param name="range">要检查重叠的范围。</param>
    /// <returns>如果指定范围与当前范围重叠，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    public bool IsOverlapping(Range<T> range) => IsInside(range.Minimum) || IsInside(range.Maximum) || range.IsInside(Minimum) || range.IsInside(Maximum);

    /// <summary>
    /// 当设置值时传递有效值的回调函数。
    /// </summary>
    public event EventHandler<RangeValueChangedEventArgs<T>> ValueChanged;

    internal Action<T> ValueChangedCallback;

    /// <summary>
    /// 确定两个实例是否相等。
    /// </summary>
    public static bool operator ==(Range<T> range1, Range<T> range2) => range1.Minimum.Equals(range2.Minimum) && range1.Maximum.Equals(range2.Maximum);

    /// <summary>
    /// 确定两个实例是否不相等。
    /// </summary>
    public static bool operator !=(Range<T> range1, Range<T> range2) => !(range1 == range2);

    /// <summary>
    /// 指示当前对象是否等于同一类型的另一个对象。
    /// </summary>
    /// <param name="other">要与此对象进行比较的对象。</param>
    /// <returns>如果当前对象等于 <paramref name="other"/> 参数，则为 true；否则为 false。</returns>
    public bool Equals(Range<T> other) => this == other;

    /// <summary>
    /// 确定指定的 <see cref="System.Object"/> 是否等于此实例。
    /// </summary>
    /// <param name="obj">要与此实例进行比较的 <see cref="System.Object"/>。</param>
    /// <returns>如果指定的 <see cref="System.Object"/> 等于此实例，则为 true；否则为 false。</returns>
    public override bool Equals(object obj) => obj is Range<T> range && this == range;

    /// <summary>
    /// 返回此实例的哈希代码。
    /// </summary>
    /// <returns>此实例的哈希代码，适用于哈希算法和数据结构（如哈希表）。</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 11;
            hash = hash * 31 + Minimum.GetHashCode();
            hash = hash * 31 + Maximum.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// 返回表示此实例的 <see cref="System.String"/>。
    /// </summary>
    /// <returns>表示此实例的 <see cref="System.String"/>。</returns>
    public override string ToString() => $"Range<{typeof(T)}> : [{Minimum},{Maximum}]";

    /// <summary>
    /// 返回表示此实例的 <see cref="System.String"/>。
    /// </summary>
    /// <param name="format">格式字符串。</param>
    /// <param name="formatProvider">格式提供程序。</param>
    /// <returns>表示此实例的 <see cref="System.String"/>。</returns>
    public string ToString(string format, IFormatProvider formatProvider) => $"Range<{typeof(T)}> : [{Minimum.ToString(format, formatProvider)}, {Maximum.ToString(format, formatProvider)}]";

    /// <summary>
    /// 表示值更改事件参数的类。
    /// </summary>
    /// <typeparam name="TV">数值类型，必须实现 <see cref="IComparable{TV}"/>、<see cref="IFormattable"/>、<see cref="IConvertible"/>、<see cref="IEquatable{TV}"/> 接口。</typeparam>
    public class RangeValueChangedEventArgs<TV> : EventArgs
        where TV : struct, IComparable, IFormattable, IConvertible, IComparable<TV>, IEquatable<TV>
    {
        /// <summary>
        /// 初始化 <see cref="RangeValueChangedEventArgs{TV}"/> 类的新实例。
        /// </summary>
        /// <param name="value">更改的值。</param>
        public RangeValueChangedEventArgs(TV value) => Value = value;

        /// <summary>
        /// 获取更改的值。
        /// </summary>
        public TV Value { get; }
    }
}