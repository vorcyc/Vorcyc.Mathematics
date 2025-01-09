namespace Vorcyc.Mathematics.Numerics;

using System.Numerics;

/// <summary>
/// 表示有理数的结构体。
/// </summary>
/// <typeparam name="T">必须实现 <see cref="IBinaryInteger{T}"/> 接口的泛型类型。</typeparam>
/// <remarks>
/// 有理数是可以表示为两个整数之比的数，即形如 a/b 的数，其中 a 和 b 是整数，且 b 不等于零。
/// 有理数的特点包括：
/// <list type="bullet">
/// <item><description>有理数的集合包括所有整数和所有可以表示为两个整数之比的数。</description></item>
/// <item><description>有理数可以表示为有限小数或无限循环小数。例如，1/2 = 0.5 是有限小数，而 1/3 = 0.333... 是无限循环小数。</description></item>
/// <item><description>有理数在加法、减法、乘法和除法（除数不为零）下是封闭的，这意味着对有理数进行这些运算的结果仍然是有理数。</description></item>
/// </list>
/// 有理数的常见操作包括：
/// <list type="bullet">
/// <item><description>加法：两个有理数相加需要找到一个公分母，然后将分子相加。</description></item>
/// <item><description>减法：两个有理数相减需要找到一个公分母，然后将分子相减。</description></item>
/// <item><description>乘法：两个有理数相乘只需将分子相乘，分母相乘。</description></item>
/// <item><description>除法：将一个有理数除以另一个有理数只需将第一个有理数乘以第二个有理数的倒数。</description></item>
/// <item><description>简化：简化有理数需要将分子和分母除以它们的最大公约数（GCD）。</description></item>
/// <item><description>比较：可以通过交叉相乘来比较两个有理数，以避免除法。</description></item>
/// <item><description>取反：有理数的取反通过将分子取反来实现。</description></item>
/// <item><description>倒数：有理数的倒数通过交换分子和分母来实现。</description></item>
/// <item><description>绝对值：有理数的绝对值通过取分子和分母的绝对值来实现。</description></item>
/// <item><description>转换为小数：有理数可以通过将分子除以分母来转换为小数形式。</description></item>
/// </list>
/// </remarks>
public readonly struct Rational<T> : IComparable<Rational<T>>, IEquatable<Rational<T>>
    where T : IBinaryInteger<T>
{
    #region Properties

    /// <summary>
    /// 获取有理数的分子。
    /// </summary>
    public T Numerator { get; }

    /// <summary>
    /// 获取有理数的分母。
    /// </summary>
    public T Denominator { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// 初始化 <see cref="Rational{T}"/> 结构的新实例。
    /// </summary>
    /// <param name="numerator">分子。</param>
    /// <param name="denominator">分母。</param>
    /// <exception cref="DivideByZeroException">当分母为零时引发。</exception>
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
    /// 实现两个 <see cref="Rational{T}"/> 实例的加法运算。
    /// </summary>
    /// <param name="a">第一个 <see cref="Rational{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>两个 <see cref="Rational{T}"/> 实例的和。</returns>
    public static Rational<T> operator +(Rational<T> a, Rational<T> b)
    {
        return new Rational<T>(
            a.Numerator * b.Denominator + b.Numerator * a.Denominator,
            a.Denominator * b.Denominator
        );
    }

    /// <summary>
    /// 实现两个 <see cref="Rational{T}"/> 实例的减法运算。
    /// </summary>
    /// <param name="a">第一个 <see cref="Rational{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>两个 <see cref="Rational{T}"/> 实例的差。</returns>
    public static Rational<T> operator -(Rational<T> a, Rational<T> b)
    {
        return new Rational<T>(
            a.Numerator * b.Denominator - b.Numerator * a.Denominator,
            a.Denominator * b.Denominator
        );
    }

    /// <summary>
    /// 实现两个 <see cref="Rational{T}"/> 实例的乘法运算。
    /// </summary>
    /// <param name="a">第一个 <see cref="Rational{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>两个 <see cref="Rational{T}"/> 实例的积。</returns>
    public static Rational<T> operator *(Rational<T> a, Rational<T> b)
    {
        return new Rational<T>(
            a.Numerator * b.Numerator,
            a.Denominator * b.Denominator
        );
    }

    /// <summary>
    /// 实现两个 <see cref="Rational{T}"/> 实例的除法运算。
    /// </summary>
    /// <param name="a">第一个 <see cref="Rational{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>两个 <see cref="Rational{T}"/> 实例的商。</returns>
    /// <exception cref="DivideByZeroException">当分母为零时引发。</exception>
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
    /// 判断两个 <see cref="Rational{T}"/> 实例是否相等。
    /// </summary>
    /// <param name="a">第一个 <see cref="Rational{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>如果两个实例相等，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    public static bool operator ==(Rational<T> a, Rational<T> b)
    {
        return a.Equals(b);
    }

    /// <summary>
    /// 判断两个 <see cref="Rational{T}"/> 实例是否不相等。
    /// </summary>
    /// <param name="a">第一个 <see cref="Rational{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>如果两个实例不相等，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    public static bool operator !=(Rational<T> a, Rational<T> b)
    {
        return !a.Equals(b);
    }

    /// <summary>
    /// 判断第一个 <see cref="Rational{T}"/> 实例是否小于第二个实例。
    /// </summary>
    /// <param name="a">第一个 <see cref="Rational{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>如果第一个实例小于第二个实例，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    public static bool operator <(Rational<T> a, Rational<T> b)
    {
        return a.CompareTo(b) < 0;
    }

    /// <summary>
    /// 判断第一个 <see cref="Rational{T}"/> 实例是否大于第二个实例。
    /// </summary>
    /// <param name="a">第一个 <see cref="Rational{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>如果第一个实例大于第二个实例，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    public static bool operator >(Rational<T> a, Rational<T> b)
    {
        return a.CompareTo(b) > 0;
    }

    /// <summary>
    /// 判断第一个 <see cref="Rational{T}"/> 实例是否小于或等于第二个实例。
    /// </summary>
    /// <param name="a">第一个 <see cref="Rational{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>如果第一个实例小于或等于第二个实例，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    public static bool operator <=(Rational<T> a, Rational<T> b)
    {
        return a.CompareTo(b) <= 0;
    }

    /// <summary>
    /// 判断第一个 <see cref="Rational{T}"/> 实例是否大于或等于第二个实例。
    /// </summary>
    /// <param name="a">第一个 <see cref="Rational{T}"/> 实例。</param>
    /// <param name="b">第二个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>如果第一个实例大于或等于第二个实例，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    public static bool operator >=(Rational<T> a, Rational<T> b)
    {
        return a.CompareTo(b) >= 0;
    }

    #endregion

    #region Methods

    /// <summary>
    /// 比较当前实例与另一个 <see cref="Rational{T}"/> 实例。
    /// </summary>
    /// <param name="other">另一个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>一个值，指示当前实例是否小于、等于或大于另一个实例。</returns>
    public int CompareTo(Rational<T> other)
    {
        return (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);
    }

    /// <summary>
    /// 判断当前实例是否与另一个 <see cref="Rational{T}"/> 实例相等。
    /// </summary>
    /// <param name="other">另一个 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>如果两个实例相等，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    public bool Equals(Rational<T> other)
    {
        return Numerator == other.Numerator && Denominator == other.Denominator;
    }

    /// <summary>
    /// 判断当前实例是否与另一个对象相等。
    /// </summary>
    /// <param name="obj">要与当前实例进行比较的对象。</param>
    /// <returns>如果对象是 <see cref="Rational{T}"/> 并且与当前实例相等，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    public override bool Equals(object? obj)
    {
        return obj is Rational<T> other && Equals(other);
    }

    /// <summary>
    /// 返回当前实例的哈希代码。
    /// </summary>
    /// <returns>当前实例的哈希代码。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Numerator, Denominator);
    }

    /// <summary>
    /// 返回当前实例的字符串表示形式。
    /// </summary>
    /// <returns>当前实例的字符串表示形式。</returns>
    public override string ToString()
    {
        return $"{Numerator}/{Denominator}";
    }

    /// <summary>
    /// 将字符串表示形式解析为 <see cref="Rational{T}"/> 实例。
    /// </summary>
    /// <param name="s">要解析的字符串。</param>
    /// <returns>解析后的 <see cref="Rational{T}"/> 实例。</returns>
    /// <exception cref="FormatException">当字符串格式无效时引发。</exception>
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
    /// 尝试将字符串表示形式解析为 <see cref="Rational{T}"/> 实例。
    /// </summary>
    /// <param name="s">要解析的字符串。</param>
    /// <param name="result">解析后的 <see cref="Rational{T}"/> 实例。</param>
    /// <returns>如果解析成功，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
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
    /// 返回当前有理数的取反数。
    /// </summary>
    /// <returns>当前有理数的取反数。</returns>
    public Rational<T> Negate()
    {
        return new Rational<T>(-Numerator, Denominator);
    }

    /// <summary>
    /// 返回当前有理数的倒数。
    /// </summary>
    /// <returns>当前有理数的倒数。</returns>
    /// <exception cref="DivideByZeroException">当分子为零时引发。</exception>
    public Rational<T> Reciprocal()
    {
        if (Numerator == T.Zero)
            throw new DivideByZeroException("Cannot take reciprocal of zero.");

        return new Rational<T>(Denominator, Numerator);
    }

    /// <summary>
    /// 返回当前有理数的绝对值。
    /// </summary>
    /// <returns>当前有理数的绝对值。</returns>
    public Rational<T> Abs()
    {
        return new Rational<T>(T.Abs(Numerator), T.Abs(Denominator));
    }

    /// <summary>
    /// 将当前有理数转换为小数表示形式。
    /// </summary>
    /// <returns>当前有理数的小数表示形式。</returns>
    public double ToDouble()
    {
        return (double)(dynamic)Numerator / (double)(dynamic)Denominator;
    }

    /// <summary>
    /// 将当前有理数转换为指定的浮点数类型。
    /// </summary>
    /// <typeparam name="TFloatingNumber">要转换的浮点数类型。</typeparam>
    /// <returns>当前有理数的浮点数表示形式。</returns>
    public TFloatingNumber ToFloatingNumber<TFloatingNumber>()
        where TFloatingNumber : IFloatingPointIeee754<TFloatingNumber>
    {
        return TFloatingNumber.CreateChecked(Numerator) / TFloatingNumber.CreateChecked(Denominator);
    }

    #endregion
}
