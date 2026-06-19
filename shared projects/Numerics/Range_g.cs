using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.Numerics;

/// <summary>
/// Represents a range as an interval.
/// </summary>
/// <typeparam name="T">The numeric type, which must implement the <see cref="IComparable{T}"/>, <see cref="IFormattable"/>, <see cref="IConvertible"/>, and <see cref="IEquatable{T}"/> interfaces.</typeparam>
public sealed class Range<T> : IEquatable<Range<T>>
    where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
{
    private T _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Range{T}"/> class.
    /// </summary>
    /// <param name="minimum">The minimum value of the specified interval.</param>
    /// <param name="maximum">The maximum value of the specified interval.</param>
    public Range(T minimum, T maximum)
    {
        if (maximum.LessThanOrEqual(minimum)) throw new ArgumentException("Maximum must be greater than minimum.");

        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Range{T}"/> class.
    /// </summary>
    /// <param name="minimum">The minimum value of the specified interval.</param>
    /// <param name="maximum">The maximum value of the specified interval.</param>
    /// <param name="value">The specified value.</param>
    public Range(T minimum, T maximum, T value)
    {
        if (maximum.LessThanOrEqual(minimum)) throw new ArgumentException("Maximum must be greater than minimum.");

        Minimum = minimum;
        Maximum = maximum;
        _value = value;
    }

    /// <summary>
    /// Gets or sets the value. When setting the value, it is clamped to the interval defined by the bounds.
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
    /// Gets the maximum value set at construction time.
    /// </summary>
    public T Maximum { get; }

    /// <summary>
    /// Gets the minimum value set at construction time.
    /// </summary>
    public T Minimum { get; }

    /// <summary>
    /// Checks whether the specified value is inside the range.
    /// </summary>
    /// <param name="x">The value to check.</param>
    /// <returns><c>true</c> if the specified value is inside the range; otherwise, <c>false</c>.</returns>
    public bool IsInside(T x) => x.GreaterThanOrEqual(Minimum) && x.LessThanOrEqual(Maximum);

    /// <summary>
    /// Computes the intersection between two ranges.
    /// </summary>
    /// <param name="range">The second range to compute the intersection with.</param>
    /// <returns>A new <see cref="Range{T}"/> structure containing the intersection between this range and the <paramref name="range"/> given as a parameter.</returns>
    public Range<T> Intersection(Range<T> range)
    {
        if (Maximum.LessThan(range.Minimum) || Minimum.GreaterThan(range.Maximum))
            return null;

        return new Range<T>(Minimum.CompareMax(range.Minimum), Maximum.CompareMin(range.Maximum));
    }

    /// <summary>
    /// Computes the union between two ranges.
    /// </summary>
    /// <param name="range">The second range to compute the union with.</param>
    /// <returns>A new <see cref="Range{T}"/> structure containing the union between this range and the <paramref name="range"/> given as a parameter.</returns>
    public Range<T> Union(Range<T> range) => new Range<T>(Minimum.CompareMin(range.Minimum), Maximum.CompareMax(range.Maximum));

    /// <summary>
    /// Checks whether the specified range is inside the current range.
    /// </summary>
    /// <param name="range">The range to check.</param>
    /// <returns><c>true</c> if the specified range is inside the current range; otherwise, <c>false</c>.</returns>
    public bool IsInside(Range<T> range) => IsInside(range.Minimum) && IsInside(range.Maximum);

    /// <summary>
    /// Checks whether the specified range overlaps with the current range.
    /// </summary>
    /// <param name="range">The range to check for overlap.</param>
    /// <returns><c>true</c> if the specified range overlaps with the current range; otherwise, <c>false</c>.</returns>
    public bool IsOverlapping(Range<T> range) => IsInside(range.Minimum) || IsInside(range.Maximum) || range.IsInside(Minimum) || range.IsInside(Maximum);

    /// <summary>
    /// The callback function invoked with the valid value when the value is set.
    /// </summary>
    public event EventHandler<RangeValueChangedEventArgs<T>> ValueChanged;

    internal Action<T> ValueChangedCallback;

    /// <summary>
    /// Determines whether two instances are equal.
    /// </summary>
    public static bool operator ==(Range<T> range1, Range<T> range2) => range1.Minimum.Equals(range2.Minimum) && range1.Maximum.Equals(range2.Maximum);

    /// <summary>
    /// Determines whether two instances are not equal.
    /// </summary>
    public static bool operator !=(Range<T> range1, Range<T> range2) => !(range1 == range2);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">The object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
    public bool Equals(Range<T> other) => this == other;

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
    /// <returns>true if the specified <see cref="System.Object"/> is equal to this instance; otherwise, false.</returns>
    public override bool Equals(object obj) => obj is Range<T> range && this == range;

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code for this instance, suitable for use in hashing algorithms and data structures such as a hash table.</returns>
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
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
    public override string ToString() => $"Range<{typeof(T)}> : [{Minimum},{Maximum}]";

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
    public string ToString(string format, IFormatProvider formatProvider) => $"Range<{typeof(T)}> : [{Minimum.ToString(format, formatProvider)}, {Maximum.ToString(format, formatProvider)}]";

    /// <summary>
    /// Represents the class for the value changed event arguments.
    /// </summary>
    /// <typeparam name="TV">The numeric type, which must implement the <see cref="IComparable{TV}"/>, <see cref="IFormattable"/>, <see cref="IConvertible"/>, and <see cref="IEquatable{TV}"/> interfaces.</typeparam>
    public class RangeValueChangedEventArgs<TV> : EventArgs
        where TV : struct, IComparable, IFormattable, IConvertible, IComparable<TV>, IEquatable<TV>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeValueChangedEventArgs{TV}"/> class.
        /// </summary>
        /// <param name="value">The changed value.</param>
        public RangeValueChangedEventArgs(TV value) => Value = value;

        /// <summary>
        /// Gets the changed value.
        /// </summary>
        public TV Value { get; }
    }
}