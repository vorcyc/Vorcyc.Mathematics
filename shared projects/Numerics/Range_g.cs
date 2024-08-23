namespace Vorcyc.Mathematics.Numerics;

using Vorcyc.Mathematics.Helpers;
using Vorcyc.Mathematics.Statistics;


/// <summary>
/// 表示一个范围的开区间
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Range<T> : IEquatable<Range<T>>
    where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
{
    private T _value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="minimum">指定区间的最小值</param>
    /// <param name="maximum">指定区间的最大值</param>
    public Range(T minimum, T maximum)
    {
        if (maximum.IsLessThanOrEqual(minimum)) throw new ArgumentException("Maximum must greater than minimun.");

        Minimum = minimum;
        Maximum = maximum;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="minimun"></param>
    /// <param name="maximum"></param>
    /// <param name="value"></param>
    public Range(T minimun, T maximum, T value)
    {
        if (maximum.IsLessThanOrEqual(minimun)) throw new ArgumentException("Maximum must greater than minimun.");

        Minimum = minimun;
        Maximum = maximum;
        _value = value;
    }


    /// <summary>
    /// 获取或设置值，设置值时会限定在依极值所限定的区间内.
    /// </summary>
    public T Value
    {
        get { return _value; }
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
    /// 获取在构造时设置的最大值
    /// </summary>
    public T Maximum { get; }



    /// <summary>
    /// 获取在构造时设置的最小值
    /// </summary>
    public T Minimum { get; }




    /// <summary>
    ///   Check if the specified value is inside of the range.
    /// </summary>
    /// 
    /// <param name="x">Value to check.</param>
    /// 
    /// <returns>
    ///   <b>True</b> if the specified value is inside of the range or <b>false</b> otherwise.
    /// </returns>
    /// 
    public bool IsInside(T x)
    {
        return IComparableExtension.IsGreaterThanOrEqual(x, Minimum) && IComparableExtension.IsLessThanOrEqual(x, Maximum);
    }


    /// <summary>
    ///   Computes the intersection between two ranges.
    ///   交集
    /// </summary>
    /// 
    /// <param name="range">The second range for which the intersection should be calculated.</param>
    /// 
    /// <returns>An new <see cref="Range{T}"/> structure containing the intersection
    /// between this range and the <paramref name="range"/> given as argument.</returns>
    /// 
    public Range<T> Intersection(Range<T> range)
    {
        if (this.Maximum.IsLessThan(range.Minimum) || this.Minimum.IsGreaterThan(range.Maximum))
            return null;

        return new Range<T>(SBasic.Max(this.Minimum, range.Maximum), SBasic.Min(this.Maximum, range.Maximum));
    }


    /// <summary>
    ///  Computes the union between two ranges.
    ///  并集
    /// </summary>
    /// <param name="range">The second range for which the union should be calculated.</param>
    /// <returns>An new <see cref="Range{T}"/> structure containing the unino
    /// between this range and the <paramref name="range"/> given as argument.</returns>
    public Range<T> Union(Range<T> range)
    {
        return new Range<T>(SBasic.Min(this.Minimum, range.Minimum), SBasic.Max(this.Maximum, range.Maximum));
    }


    /// <summary>
    ///   Check if the specified range is inside of the range.
    /// </summary>
    /// 
    /// <param name="range">Range to check.</param>
    /// 
    /// <returns>
    ///   <b>True</b> if the specified range is inside of the range or <b>false</b> otherwise.
    /// </returns>
    /// 
    public bool IsInside(Range<T> range)
    {
        return ((IsInside(range.Minimum)) && (IsInside(range.Maximum)));
    }


    /// <summary>
    ///   Check if the specified range overlaps with the range.
    /// </summary>
    /// 
    /// <param name="range">Range to check for overlapping.</param>
    /// 
    /// <returns>
    ///   <b>True</b> if the specified range overlaps with the range or <b>false</b> otherwise.
    /// </returns>
    /// 
    public bool IsOverlapping(Range<T> range)
    {
        return ((IsInside(range.Minimum)) || (IsInside(range.Maximum)) ||
                 (range.IsInside(Minimum)) || (range.IsInside(Maximum)));
    }




    /// <summary>
    /// 当设置值时传递有效值的回调函数.
    /// </summary>
    public event EventHandler<RangeValueChangedEventArgs<T>> ValueChanged;


    internal Action<T> ValueChangedCallback;







    /// <summary>
    ///   Determines whether two instances are equal.
    /// </summary>
    /// 
    public static bool operator ==(Range<T> range1, Range<T> range2)
    {
        return (IComparableExtension.Equals(range1.Minimum, range2.Minimum) && IComparableExtension.Equals(range1.Maximum, range2.Maximum));
    }



    /// <summary>
    ///   Determines whether two instances are not equal.
    /// </summary>
    /// 
    public static bool operator !=(Range<T> range1, Range<T> range2)
    {
        return (range1.Minimum.NotEquals(range2.Minimum) || range1.Maximum.NotEquals(range2.Maximum));
    }




    #region interface implementation

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// 
    /// <param name="other">An object to compare with this object.</param>
    /// 
    /// <returns>
    ///   true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(Range<T> other)
    {
        return this == other;
    }

    #endregion


    #region overrides

    /// <summary>
    ///   Determines whether the specified <see cref="System.Object" />, is equal to this instance.
    /// </summary>
    /// 
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// 
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
        if (obj is Range<T> range)
        {
            return this == range;
        }
        return false;
    }


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

    #endregion



    #region ToString()

    /// <summary>
    ///   Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// 
    /// <returns>
    ///   A <see cref="System.String" /> that represents this instance.
    /// </returns>
    /// 
    public override string ToString()
    {
        return $"Range<{typeof(T)}> : [{Minimum},{Maximum}]";
    }


    /// <summary>
    ///   Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// 
    /// <param name="format">The format.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// 
    /// <returns>
    ///   A <see cref="System.String" /> that represents this instance.
    /// </returns>
    /// 
    public string ToString(string format, IFormatProvider formatProvider)
    {
        return $"Range<{typeof(T)}> : [{Minimum.ToString(format, formatProvider)}, {Maximum.ToString(format, formatProvider)}]";
    }

    #endregion








    public class RangeValueChangedEventArgs<TV>
        : EventArgs
        where TV : struct, IComparable, IFormattable, IConvertible, IComparable<TV>, IEquatable<TV>
    {
        private TV _val;

        public RangeValueChangedEventArgs(TV value)
        {
            _val = value;
        }

        public TV Value => _val;
    }
}