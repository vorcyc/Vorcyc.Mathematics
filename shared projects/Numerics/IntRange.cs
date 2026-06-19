namespace Vorcyc.Mathematics.Numerics;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents an integer range with a minimum and a maximum value.
/// </summary>
/// <remarks>
/// This type represents an integer range that includes both the minimum and maximum values,
/// where both the minimum and maximum of the range are inclusive.
/// The mathematical notation for such a range is <b>[min, max]</b>.
/// </remarks>
[Serializable]
public struct IntRange : IEquatable<IntRange>, IEnumerable<int>
{
    /// <summary>
    /// Gets or sets the minimum value of the range.
    /// </summary>
    public int Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value of the range.
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    /// Gets the length of the range, defined as (max - min).
    /// </summary>
    public int Length => Max - Min;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntRange"/> class.
    /// </summary>
    /// <param name="min">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    public IntRange(int min, int max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Checks whether the specified value is inside the range.
    /// </summary>
    /// <param name="x">The value to check.</param>
    /// <returns><b>true</b> if the specified value is inside the range; otherwise, <b>false</b>.</returns>
    public bool IsInside(int x) => x >= Min && x <= Max;

    /// <summary>
    /// Computes the intersection between two ranges.
    /// </summary>
    /// <param name="range">The second range to compute the intersection with.</param>
    /// <returns>A new <see cref="IntRange"/> structure containing the intersection between this range and the <paramref name="range"/> given as a parameter.</returns>
    public IntRange Intersection(IntRange range) => new IntRange(Math.Max(Min, range.Min), Math.Min(Max, range.Max));

    /// <summary>
    /// Checks whether the specified range is inside this range.
    /// </summary>
    /// <param name="range">The range to check.</param>
    /// <returns><b>true</b> if the specified range is inside this range; otherwise, <b>false</b>.</returns>
    public bool IsInside(IntRange range) => IsInside(range.Min) && IsInside(range.Max);

    /// <summary>
    /// Checks whether the specified range overlaps with this range.
    /// </summary>
    /// <param name="range">The range to check for overlap.</param>
    /// <returns><b>true</b> if the specified range overlaps with this range; otherwise, <b>false</b>.</returns>
    public bool IsOverlapping(IntRange range) => IsInside(range.Min) || IsInside(range.Max) || range.IsInside(Min) || range.IsInside(Max);

    /// <summary>
    /// Determines whether two instances are equal.
    /// </summary>
    public static bool operator ==(IntRange range1, IntRange range2) => range1.Min == range2.Min && range1.Max == range2.Max;

    /// <summary>
    /// Determines whether two instances are not equal.
    /// </summary>
    public static bool operator !=(IntRange range1, IntRange range2) => !(range1 == range2);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">The object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
    public bool Equals(IntRange other) => this == other;

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
    /// <returns>true if the specified <see cref="System.Object"/> is equal to this instance; otherwise, false.</returns>
    public override bool Equals(object obj) => obj is IntRange range && this == range;

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code for this instance, suitable for use in hashing algorithms and data structures such as a hash table.</returns>
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
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
    public override string ToString() => $"[{Min}, {Max}]";

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
    public string ToString(string format, IFormatProvider formatProvider) => $"[{Min.ToString(format, formatProvider)}, {Max.ToString(format, formatProvider)}]";

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
    public IEnumerator<int> GetEnumerator()
    {
        for (int i = Min; i <= Max; i++)
            yield return i;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
