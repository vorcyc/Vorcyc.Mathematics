namespace Vorcyc.Mathematics.Numerics;

using System.Numerics;

/// <summary>
/// Represents a structure for rational numbers.
/// </summary>
/// <typeparam name="T">A generic type that must implement the <see cref="IBinaryInteger{T}"/> interface.</typeparam>
/// <remarks>
/// A rational number is a number that can be expressed as the ratio of two integers, that is, a number of the form a/b, where a and b are integers and b is not equal to zero.
/// Characteristics of rational numbers include:
/// <list type="bullet">
/// <item><description>The set of rational numbers includes all integers and all numbers that can be expressed as the ratio of two integers.</description></item>
/// <item><description>A rational number can be represented as a finite decimal or an infinite repeating decimal. For example, 1/2 = 0.5 is a finite decimal, while 1/3 = 0.333... is an infinite repeating decimal.</description></item>
/// <item><description>Rational numbers are closed under addition, subtraction, multiplication, and division (with a nonzero divisor), which means that the result of these operations on rational numbers is still a rational number.</description></item>
/// </list>
/// Common operations on rational numbers include:
/// <list type="bullet">
/// <item><description>Addition: adding two rational numbers requires finding a common denominator and then adding the numerators.</description></item>
/// <item><description>Subtraction: subtracting two rational numbers requires finding a common denominator and then subtracting the numerators.</description></item>
/// <item><description>Multiplication: multiplying two rational numbers simply requires multiplying the numerators and multiplying the denominators.</description></item>
/// <item><description>Division: dividing one rational number by another simply requires multiplying the first rational number by the reciprocal of the second.</description></item>
/// <item><description>Simplification: simplifying a rational number requires dividing the numerator and denominator by their greatest common divisor (GCD).</description></item>
/// <item><description>Comparison: two rational numbers can be compared by cross-multiplication to avoid division.</description></item>
/// <item><description>Negation: negating a rational number is done by negating the numerator.</description></item>
/// <item><description>Reciprocal: the reciprocal of a rational number is obtained by swapping the numerator and denominator.</description></item>
/// <item><description>Absolute value: the absolute value of a rational number is obtained by taking the absolute values of the numerator and denominator.</description></item>
/// <item><description>Conversion to decimal: a rational number can be converted to decimal form by dividing the numerator by the denominator.</description></item>
/// </list>
/// </remarks>
public readonly struct Rational<T> : IComparable<Rational<T>>, IEquatable<Rational<T>>
    where T : IBinaryInteger<T>
{

    #region Properties

    /// <summary>
    /// Gets the numerator of the rational number.
    /// </summary>
    public T Numerator { get; }

    /// <summary>
    /// Gets the denominator of the rational number.
    /// </summary>
    public T Denominator { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Rational{T}"/> structure.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <exception cref="DivideByZeroException">Thrown when the denominator is zero.</exception>
    public Rational(T numerator, T denominator)
    {
        if (denominator == T.Zero)
            throw new DivideByZeroException("Denominator cannot be zero.");

        // 简化分数
        var gcd = VMath.Gcd(numerator, denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;

        // 确保分母为正
        if (Denominator < T.Zero)
        {
            Numerator = -Numerator;
            Denominator = -Denominator;
        }
    }

    #endregion

    #region Operator Overloads

    /// <summary>
    /// Implements the addition operation of two <see cref="Rational{T}"/> instances.
    /// </summary>
    /// <param name="a">The first <see cref="Rational{T}"/> instance.</param>
    /// <param name="b">The second <see cref="Rational{T}"/> instance.</param>
    /// <returns>The sum of the two <see cref="Rational{T}"/> instances.</returns>
    public static Rational<T> operator +(Rational<T> a, Rational<T> b)
    {
        return new Rational<T>(
            a.Numerator * b.Denominator + b.Numerator * a.Denominator,
            a.Denominator * b.Denominator
        );
    }

    /// <summary>
    /// Implements the subtraction operation of two <see cref="Rational{T}"/> instances.
    /// </summary>
    /// <param name="a">The first <see cref="Rational{T}"/> instance.</param>
    /// <param name="b">The second <see cref="Rational{T}"/> instance.</param>
    /// <returns>The difference of the two <see cref="Rational{T}"/> instances.</returns>
    public static Rational<T> operator -(Rational<T> a, Rational<T> b)
    {
        return new Rational<T>(
            a.Numerator * b.Denominator - b.Numerator * a.Denominator,
            a.Denominator * b.Denominator
        );
    }

    /// <summary>
    /// Implements the multiplication operation of two <see cref="Rational{T}"/> instances.
    /// </summary>
    /// <param name="a">The first <see cref="Rational{T}"/> instance.</param>
    /// <param name="b">The second <see cref="Rational{T}"/> instance.</param>
    /// <returns>The product of the two <see cref="Rational{T}"/> instances.</returns>
    public static Rational<T> operator *(Rational<T> a, Rational<T> b)
    {
        return new Rational<T>(
            a.Numerator * b.Numerator,
            a.Denominator * b.Denominator
        );
    }

    /// <summary>
    /// Implements the division operation of two <see cref="Rational{T}"/> instances.
    /// </summary>
    /// <param name="a">The first <see cref="Rational{T}"/> instance.</param>
    /// <param name="b">The second <see cref="Rational{T}"/> instance.</param>
    /// <returns>The quotient of the two <see cref="Rational{T}"/> instances.</returns>
    /// <exception cref="DivideByZeroException">Thrown when the denominator is zero.</exception>
    public static Rational<T> operator /(Rational<T> a, Rational<T> b)
    {
        if (b.Numerator == T.Zero)
            throw new DivideByZeroException("Cannot divide by zero.");

        return new Rational<T>(
            a.Numerator * b.Denominator,
            a.Denominator * b.Numerator
        );
    }

    /// <summary>
    /// Determines whether two <see cref="Rational{T}"/> instances are equal.
    /// </summary>
    /// <param name="a">The first <see cref="Rational{T}"/> instance.</param>
    /// <param name="b">The second <see cref="Rational{T}"/> instance.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Rational<T> a, Rational<T> b)
    {
        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether two <see cref="Rational{T}"/> instances are not equal.
    /// </summary>
    /// <param name="a">The first <see cref="Rational{T}"/> instance.</param>
    /// <param name="b">The second <see cref="Rational{T}"/> instance.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Rational<T> a, Rational<T> b)
    {
        return !a.Equals(b);
    }

    /// <summary>
    /// Determines whether the first <see cref="Rational{T}"/> instance is less than the second instance.
    /// </summary>
    /// <param name="a">The first <see cref="Rational{T}"/> instance.</param>
    /// <param name="b">The second <see cref="Rational{T}"/> instance.</param>
    /// <returns><c>true</c> if the first instance is less than the second instance; otherwise, <c>false</c>.</returns>
    public static bool operator <(Rational<T> a, Rational<T> b)
    {
        return a.CompareTo(b) < 0;
    }

    /// <summary>
    /// Determines whether the first <see cref="Rational{T}"/> instance is greater than the second instance.
    /// </summary>
    /// <param name="a">The first <see cref="Rational{T}"/> instance.</param>
    /// <param name="b">The second <see cref="Rational{T}"/> instance.</param>
    /// <returns><c>true</c> if the first instance is greater than the second instance; otherwise, <c>false</c>.</returns>
    public static bool operator >(Rational<T> a, Rational<T> b)
    {
        return a.CompareTo(b) > 0;
    }

    /// <summary>
    /// Determines whether the first <see cref="Rational{T}"/> instance is less than or equal to the second instance.
    /// </summary>
    /// <param name="a">The first <see cref="Rational{T}"/> instance.</param>
    /// <param name="b">The second <see cref="Rational{T}"/> instance.</param>
    /// <returns><c>true</c> if the first instance is less than or equal to the second instance; otherwise, <c>false</c>.</returns>
    public static bool operator <=(Rational<T> a, Rational<T> b)
    {
        return a.CompareTo(b) <= 0;
    }

    /// <summary>
    /// Determines whether the first <see cref="Rational{T}"/> instance is greater than or equal to the second instance.
    /// </summary>
    /// <param name="a">The first <see cref="Rational{T}"/> instance.</param>
    /// <param name="b">The second <see cref="Rational{T}"/> instance.</param>
    /// <returns><c>true</c> if the first instance is greater than or equal to the second instance; otherwise, <c>false</c>.</returns>
    public static bool operator >=(Rational<T> a, Rational<T> b)
    {
        return a.CompareTo(b) >= 0;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Compares the current instance with another <see cref="Rational{T}"/> instance.
    /// </summary>
    /// <param name="other">Another <see cref="Rational{T}"/> instance.</param>
    /// <returns>A value indicating whether the current instance is less than, equal to, or greater than the other instance.</returns>
    public int CompareTo(Rational<T> other)
    {
        return (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);
    }

    /// <summary>
    /// Determines whether the current instance is equal to another <see cref="Rational{T}"/> instance.
    /// </summary>
    /// <param name="other">Another <see cref="Rational{T}"/> instance.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(Rational<T> other)
    {
        return Numerator == other.Numerator && Denominator == other.Denominator;
    }

    /// <summary>
    /// Determines whether the current instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the object is a <see cref="Rational{T}"/> and is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Rational<T> other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for the current instance.
    /// </summary>
    /// <returns>The hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Numerator, Denominator);
    }

    /// <summary>
    /// Returns the string representation of the current instance.
    /// </summary>
    /// <returns>The string representation of the current instance.</returns>
    public override string ToString()
    {
        return $"{Numerator}/{Denominator}";
    }

    /// <summary>
    /// Parses a string representation into a <see cref="Rational{T}"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>The parsed <see cref="Rational{T}"/> instance.</returns>
    /// <exception cref="FormatException">Thrown when the string format is invalid.</exception>
    public static Rational<T> Parse(string s)
    {
        var parts = s.Split('/');
        if (parts.Length != 2)
            throw new FormatException("Invalid rational number format.");

        var numerator = T.Parse(parts[0], null);
        var denominator = T.Parse(parts[1], null);

        return new Rational<T>(numerator, denominator);
    }

    /// <summary>
    /// Tries to parse a string representation into a <see cref="Rational{T}"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="result">The parsed <see cref="Rational{T}"/> instance.</param>
    /// <returns><c>true</c> if parsing succeeds; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string s, out Rational<T> result)
    {
        result = default;
        var parts = s.Split('/');
        if (parts.Length != 2)
            return false;

        if (T.TryParse(parts[0], null, out var numerator) &&
            T.TryParse(parts[1], null, out var denominator))
        {
            result = new Rational<T>(numerator, denominator);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the negation of the current rational number.
    /// </summary>
    /// <returns>The negation of the current rational number.</returns>
    public Rational<T> Negate()
    {
        return new Rational<T>(-Numerator, Denominator);
    }

    /// <summary>
    /// Returns the reciprocal of the current rational number.
    /// </summary>
    /// <returns>The reciprocal of the current rational number.</returns>
    /// <exception cref="DivideByZeroException">Thrown when the numerator is zero.</exception>
    public Rational<T> Reciprocal()
    {
        if (Numerator == T.Zero)
            throw new DivideByZeroException("Cannot take reciprocal of zero.");

        return new Rational<T>(Denominator, Numerator);
    }

    /// <summary>
    /// Returns the absolute value of the current rational number.
    /// </summary>
    /// <returns>The absolute value of the current rational number.</returns>
    public Rational<T> Abs()
    {
        return new Rational<T>(T.Abs(Numerator), T.Abs(Denominator));
    }

    /// <summary>
    /// Converts the current rational number to its decimal representation.
    /// </summary>
    /// <returns>The decimal representation of the current rational number.</returns>
    public double ToDouble()
    {
        return (double)(dynamic)Numerator / (double)(dynamic)Denominator;
    }

    /// <summary>
    /// Converts the current rational number to the specified floating-point type.
    /// </summary>
    /// <typeparam name="TFloatingNumber">The floating-point type to convert to.</typeparam>
    /// <returns>The floating-point representation of the current rational number.</returns>
    public TFloatingNumber ToFloatingPointNumber<TFloatingNumber>()
        where TFloatingNumber : IFloatingPointIeee754<TFloatingNumber>
    {
        return TFloatingNumber.CreateChecked(Numerator) / TFloatingNumber.CreateChecked(Denominator);
    }

    #endregion
}
