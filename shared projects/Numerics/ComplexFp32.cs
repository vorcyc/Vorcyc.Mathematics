namespace Vorcyc.Mathematics.Numerics;

/*
 * 
 * 23.9.26 从泛型版本改过来 ，之前的那个版本放弃了
 * 不用属性而是直接用字段，并且是可读可写的字段
 */

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


/// <summary>
/// A complex number z is a number of the form z = x + yi, where x and y
/// are real numbers, and i is the imaginary unit, with the property i2= -1.
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public /*readonly*/ struct ComplexFp32
    : IEquatable<ComplexFp32>,
      IFormattable,
      INumberBase<ComplexFp32>,
      ISignedNumber<ComplexFp32>
{

    private const NumberStyles DefaultNumberStyle = NumberStyles.Float | NumberStyles.AllowThousands;

    private const NumberStyles InvalidNumberStyles = ~(NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite
                                                     | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign
                                                     | NumberStyles.AllowParentheses | NumberStyles.AllowDecimalPoint
                                                     | NumberStyles.AllowThousands | NumberStyles.AllowExponent
                                                     | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowHexSpecifier);


    public static readonly ComplexFp32 Zero = new(0f, 0f);
    public static readonly ComplexFp32 One = new(1f, 0f);
    public static readonly ComplexFp32 ImaginaryOne = new(0f, 1f);
    public static readonly ComplexFp32 NaN = new(float.NaN, float.NaN);
    public static readonly ComplexFp32 Infinity = new(float.PositiveInfinity, float.PositiveInfinity);

    private static float InverseOfLog10 = 0.43429448190325f; // 1 / Log(10)

    // This is the largest x for which (Hypot(x,x) + x) will not overflow. It is used for branching inside Sqrt.
    private static readonly float s_sqrtRescaleThreshold = float.MaxValue / (float.Sqrt(2f) + 1f);

    // This is the largest x for which 2 x^2 will not overflow. It is used for branching inside Asin and Acos.
    private static readonly float s_asinOverflowThreshold = float.Sqrt(float.MaxValue) / 2f;

    // This value is used inside Asin and Acos.
    private static readonly float s_log2 = float.Log(2f);

    // Do not rename, these fields are needed for binary serialization
    //private readonly float m_real; // Do not rename (binary serialization)
    //private readonly float m_imaginary; // Do not rename (binary serialization)

    public ComplexFp32(float real = 0f, float imaginary = 0f)
    {
        Real = real;
        Imaginary = imaginary;
    }

    //public float Real => m_real;


    //public float Imaginary => m_imaginary;


    /// <summary>
    /// The real part.
    /// </summary>
    public float Real = 0f;

    /// <summary>
    /// The imaginary part.
    /// </summary>
    public float Imaginary = 0f;

    /// <summary>
    /// Gets the magnitude (or absolute value) of a complex number.
    /// </summary>
    public float Magnitude => Abs(this);

    /// <summary>
    /// Gets the phase of a complex number.
    /// </summary>
    public float Phase => float.Atan2(Imaginary, Real);


    #region 类型转换

    /// <summary>
    /// Deconstruct to <see cref="ValueTuple{T1,T2}"/>.
    /// </summary>
    /// <param name="real"></param>
    /// <param name="imaginary"></param>
    public void Deconstruct(out float real, out float imaginary)
    {
        real = Real;
        imaginary = Imaginary;
    }

    /// <summary>
    /// Implicitly converts a <see cref="Complex{T}"/> to a tuple containing its real and imaginary parts.
    /// </summary>
    public static implicit operator (float Real, float Imaginary)(ComplexFp32 value)
    {
        return (value.Real, value.Imaginary);
    }

    /// <summary>
    /// Implicitly converts a tuple of real and imaginary parts to a <see cref="Complex{T}"/>.
    /// </summary>
    public static implicit operator ComplexFp32((float Real, float Imaginary) value)
    {
        return new ComplexFp32(value.Real, value.Imaginary);
    }

    /// <summary>
    /// Converts a <see cref="Span{Single}"/> containing at least two floats into a <see cref="ComplexFp32"/> instance.
    /// The first element represents the real part, and the second element represents the imaginary part.
    /// </summary>
    /// <param name="span">A <see cref="Span{Single}"/> with at least two elements, where span[0] is the real part and span[1] is the imaginary part.</param>
    /// <returns>A <see cref="ComplexFp32"/> instance initialized with the real and imaginary parts from the input span.</returns>
    /// <exception cref="ArgumentException">Thrown when the input span has fewer than two elements.</exception>
    public static ComplexFp32 FromSpan(Span<float> span)
    {
        if (span.Length < 2)
            throw new ArgumentException("Span must have at least 2 elements.");

        //return new ComplexF(span[0], span[1]);
        return Unsafe.As<float, ComplexFp32>(ref span[0]);
        //return MemoryMarshal.Cast<float, ComplexF>(span)[0];

        //使用内联数组：
        //[InlineArray(2)]
        //public struct ComplexF
        //{
        //    private float _element0; // 实部
        //                             // 第二个元素隐式为虚部

        //    public float Real => this[0];
        //    public float Imaginary => this[1];
        //}

        //public static ComplexF ToComplexF(Span<float> span)
        //{
        //    if (span.Length < 2)
        //        throw new ArgumentException("Span must have at least 2 elements.");

        //    return MemoryMarshal.AsRef<ComplexF>(span);
        //}
    }

    #endregion
    /// <summary>
    /// Creates a complex number from polar coordinates using single-precision floating-point numbers.
    /// </summary>
    /// <param name="magnitude">The magnitude, which is the distance from the origin (the absolute value).</param>
    /// <param name="phase">The phase, which is the angle from the real axis, in radians.</param>
    /// <returns>A complex number with the specified magnitude and phase.</returns>
    public static ComplexFp32 FromPolarCoordinates(float magnitude, float phase)
    {
        return new ComplexFp32(magnitude * float.Cos(phase), magnitude * float.Sin(phase));
    }

    /// <summary>
    /// Negates a complex number.
    /// </summary>
    /// <param name="value">The complex number to negate.</param>
    /// <returns>The result of negating the real and imaginary parts of <paramref name="value"/>.</returns>
    public static ComplexFp32 Negate(ComplexFp32 value)
    {
        return -value;
    }

    /// <summary>
    /// Adds two complex numbers.
    /// </summary>
    /// <param name="left">The first complex number to add.</param>
    /// <param name="right">The second complex number to add.</param>
    /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
    public static ComplexFp32 Add(ComplexFp32 left, ComplexFp32 right)
    {
        return left + right;
    }

    /// <summary>
    /// Adds a complex number and a real number.
    /// </summary>
    /// <param name="left">The complex number to add.</param>
    /// <param name="right">The real number to add.</param>
    /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/> treated as a complex number with zero imaginary part.</returns>
    public static ComplexFp32 Add(ComplexFp32 left, float right)
    {
        return left + right;
    }

    /// <summary>
    /// Adds a real number and a complex number.
    /// </summary>
    /// <param name="left">The real number to add.</param>
    /// <param name="right">The complex number to add.</param>
    /// <returns>The sum of <paramref name="left"/> treated as a complex number with zero imaginary part and <paramref name="right"/>.</returns>
    public static ComplexFp32 Add(float left, ComplexFp32 right)
    {
        return left + right;
    }

    /// <summary>
    /// Subtracts one complex number from another.
    /// </summary>
    /// <param name="left">The complex number to subtract from.</param>
    /// <param name="right">The complex number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/>.</returns>
    public static ComplexFp32 Subtract(ComplexFp32 left, ComplexFp32 right)
    {
        return left - right;
    }

    /// <summary>
    /// Subtracts a real number from a complex number.
    /// </summary>
    /// <param name="left">The complex number to subtract from.</param>
    /// <param name="right">The real number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> treated as a complex number with zero imaginary part from <paramref name="left"/>.</returns>
    public static ComplexFp32 Subtract(ComplexFp32 left, float right)
    {
        return left - right;
    }

    /// <summary>
    /// Subtracts a complex number from a real number.
    /// </summary>
    /// <param name="left">The real number to subtract from.</param>
    /// <param name="right">The complex number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/> treated as a complex number with zero imaginary part.</returns>
    public static ComplexFp32 Subtract(float left, ComplexFp32 right)
    {
        return left - right;
    }

    /// <summary>
    /// Multiplies two complex numbers.
    /// </summary>
    /// <param name="left">The first complex number to multiply.</param>
    /// <param name="right">The second complex number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> and <paramref name="right"/>.</returns>
    public static ComplexFp32 Multiply(ComplexFp32 left, ComplexFp32 right)
    {
        return left * right;
    }

    /// <summary>
    /// Multiplies a complex number by a real number.
    /// </summary>
    /// <param name="left">The complex number to multiply.</param>
    /// <param name="right">The real number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> and <paramref name="right"/> treated as a complex number with zero imaginary part.</returns>
    public static ComplexFp32 Multiply(ComplexFp32 left, float right)
    {
        return left * right;
    }

    /// <summary>
    /// Multiplies a real number by a complex number.
    /// </summary>
    /// <param name="left">The real number to multiply.</param>
    /// <param name="right">The complex number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> treated as a complex number with zero imaginary part and <paramref name="right"/>.</returns>
    public static ComplexFp32 Multiply(float left, ComplexFp32 right)
    {
        return left * right;
    }

    /// <summary>
    /// Divides one complex number by another.
    /// </summary>
    /// <param name="dividend">The complex number to divide.</param>
    /// <param name="divisor">The complex number to divide by.</param>
    /// <returns>The result of dividing <paramref name="dividend"/> by <paramref name="divisor"/>.</returns>
    public static ComplexFp32 Divide(ComplexFp32 dividend, ComplexFp32 divisor)
    {
        return dividend / divisor;
    }

    /// <summary>
    /// Divides a complex number by a real number.
    /// </summary>
    /// <param name="dividend">The complex number to divide.</param>
    /// <param name="divisor">The real number to divide by.</param>
    /// <returns>The result of dividing <paramref name="dividend"/> by <paramref name="divisor"/> treated as a complex number with zero imaginary part.</returns>
    public static ComplexFp32 Divide(ComplexFp32 dividend, float divisor)
    {
        return dividend / divisor;
    }

    /// <summary>
    /// Divides a real number by a complex number.
    /// </summary>
    /// <param name="dividend">The real number to divide.</param>
    /// <param name="divisor">The complex number to divide by.</param>
    /// <returns>The result of dividing <paramref name="dividend"/> treated as a complex number with zero imaginary part by <paramref name="divisor"/>.</returns>
    public static ComplexFp32 Divide(float dividend, ComplexFp32 divisor)
    {
        return dividend / divisor;
    }

    /// <summary>
    /// Computes the unary negation of a complex number.
    /// </summary>
    /// <param name="value">The complex number to negate.</param>
    /// <returns>The complex number with negated real and imaginary parts.</returns>
    public static ComplexFp32 operator -(ComplexFp32 value)  /* Unary negation of a complex number */
    {
        return new ComplexFp32(-value.Real, -value.Imaginary);
    }

    /// <summary>
    /// Adds two complex numbers.
    /// </summary>
    /// <param name="left">The first complex number to add.</param>
    /// <param name="right">The second complex number to add.</param>
    /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
    public static ComplexFp32 operator +(ComplexFp32 left, ComplexFp32 right)
    {
        return new ComplexFp32(left.Real + right.Real, left.Imaginary + right.Imaginary);
    }

    /// <summary>
    /// Adds a complex number and a real number.
    /// </summary>
    /// <param name="left">The complex number to add.</param>
    /// <param name="right">The real number to add.</param>
    /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/> treated as a complex number with zero imaginary part.</returns>
    public static ComplexFp32 operator +(ComplexFp32 left, float right)
    {
        return new ComplexFp32(left.Real + right, left.Imaginary);
    }

    /// <summary>
    /// Adds a real number and a complex number.
    /// </summary>
    /// <param name="left">The real number to add.</param>
    /// <param name="right">The complex number to add.</param>
    /// <returns>The sum of <paramref name="left"/> treated as a complex number with zero imaginary part and <paramref name="right"/>.</returns>
    public static ComplexFp32 operator +(float left, ComplexFp32 right)
    {
        return new ComplexFp32(left + right.Real, right.Imaginary);
    }

    /// <summary>
    /// Subtracts one complex number from another.
    /// </summary>
    /// <param name="left">The complex number to subtract from.</param>
    /// <param name="right">The complex number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/>.</returns>
    public static ComplexFp32 operator -(ComplexFp32 left, ComplexFp32 right)
    {
        return new ComplexFp32(left.Real - right.Real, left.Imaginary - right.Imaginary);
    }

    /// <summary>
    /// Subtracts a real number from a complex number.
    /// </summary>
    /// <param name="left">The complex number to subtract from.</param>
    /// <param name="right">The real number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> treated as a complex number with zero imaginary part from <paramref name="left"/>.</returns>
    public static ComplexFp32 operator -(ComplexFp32 left, float right)
    {
        return new ComplexFp32(left.Real - right, left.Imaginary);
    }

    /// <summary>
    /// Subtracts a complex number from a real number.
    /// </summary>
    /// <param name="left">The real number to subtract from.</param>
    /// <param name="right">The complex number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/> treated as a complex number with zero imaginary part.</returns>
    public static ComplexFp32 operator -(float left, ComplexFp32 right)
    {
        return new ComplexFp32(left - right.Real, -right.Imaginary);
    }

    /// <summary>
    /// Multiplies two complex numbers.
    /// </summary>
    /// <param name="left">The first complex number to multiply.</param>
    /// <param name="right">The second complex number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> and <paramref name="right"/>, computed as (ac - bd) + (bc + ad)i where a and b are the real and imaginary parts of <paramref name="left"/>, and c and d are the real and imaginary parts of <paramref name="right"/>.</returns>
    public static ComplexFp32 operator *(ComplexFp32 left, ComplexFp32 right)
    {
        // Multiplication:  (a + bi)(c + di) = (ac -bd) + (bc + ad)i
        var result_realpart = (left.Real * right.Real) - (left.Imaginary * right.Imaginary);
        var result_imaginarypart = (left.Imaginary * right.Real) + (left.Real * right.Imaginary);
        return new ComplexFp32(result_realpart, result_imaginarypart);
    }

    /// <summary>
    /// Multiplies a complex number by a real number.
    /// </summary>
    /// <param name="left">The complex number to multiply.</param>
    /// <param name="right">The real number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> and <paramref name="right"/> treated as a complex number with zero imaginary part.</returns>
    public static ComplexFp32 operator *(ComplexFp32 left, float right)
    {
        if (!float.IsFinite(left.Real))
        {
            if (!float.IsFinite(left.Imaginary))
            {
                return new ComplexFp32(float.NaN, float.NaN);
            }

            return new ComplexFp32(left.Real * right, float.NaN);
        }

        if (!float.IsFinite(left.Imaginary))
        {
            return new ComplexFp32(float.NaN, left.Imaginary * right);
        }

        return new ComplexFp32(left.Real * right, left.Imaginary * right);
    }

    /// <summary>
    /// Multiplies a real number by a complex number.
    /// </summary>
    /// <param name="left">The real number to multiply.</param>
    /// <param name="right">The complex number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> treated as a complex number with zero imaginary part and <paramref name="right"/>.</returns>
    public static ComplexFp32 operator *(float left, ComplexFp32 right)
    {
        if (!float.IsFinite(right.Real))
        {
            if (!float.IsFinite(right.Imaginary))
            {
                return new ComplexFp32(float.NaN, float.NaN);
            }

            return new ComplexFp32(left * right.Real, float.NaN);
        }

        if (!float.IsFinite(right.Imaginary))
        {
            return new ComplexFp32(float.NaN, left * right.Imaginary);
        }

        return new ComplexFp32(left * right.Real, left * right.Imaginary);
    }

    /// <summary>
    /// Divides one complex number by another using Smith's formula.
    /// </summary>
    /// <param name="left">The complex number to divide.</param>
    /// <param name="right">The complex number to divide by.</param>
    /// <returns>The result of dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
    public static ComplexFp32 operator /(ComplexFp32 left, ComplexFp32 right)
    {
        // Division : Smith's formula.
        var a = left.Real;
        var b = left.Imaginary;
        var c = right.Real;
        var d = right.Imaginary;

        // Computing c * c + d * d will overflow even in cases where the actual result of the division does not overflow.
        if (float.Abs(d) < float.Abs(c))
        {
            float doc = d / c;
            return new ComplexFp32((a + b * doc) / (c + d * doc), (b - a * doc) / (c + d * doc));
        }
        else
        {
            float cod = c / d;
            return new ComplexFp32((b + a * cod) / (d + c * cod), (-a + b * cod) / (d + c * cod));
        }
    }

    /// <summary>
    /// Divides a complex number by a real number.
    /// </summary>
    /// <param name="left">The complex number to divide.</param>
    /// <param name="right">The real number to divide by.</param>
    /// <returns>The result of dividing <paramref name="left"/> by <paramref name="right"/> treated as a complex number with zero imaginary part.</returns>
    public static ComplexFp32 operator /(ComplexFp32 left, float right)
    {
        // IEEE prohibit optimizations which are value changing
        // so we make sure that behaviour for the simplified version exactly match
        // full version.
        if (right == 0f)
        {
            return new ComplexFp32(float.NaN, float.NaN);
        }

        if (!float.IsFinite(left.Real))
        {
            if (!float.IsFinite(left.Imaginary))
            {
                return new ComplexFp32(float.NaN, float.NaN);
            }

            return new ComplexFp32(left.Real / right, float.NaN);
        }

        if (!float.IsFinite(left.Imaginary))
        {
            return new ComplexFp32(float.NaN, left.Imaginary / right);
        }

        // Here the actual optimized version of code.
        return new ComplexFp32(left.Real / right, left.Imaginary / right);
    }

    /// <summary>
    /// Divides a real number by a complex number using Smith's formula.
    /// </summary>
    /// <param name="left">The real number to divide.</param>
    /// <param name="right">The complex number to divide by.</param>
    /// <returns>The result of dividing <paramref name="left"/> treated as a complex number with zero imaginary part by <paramref name="right"/>.</returns>
    public static ComplexFp32 operator /(float left, ComplexFp32 right)
    {
        // Division : Smith's formula.
        var a = left;
        var c = right.Real;
        var d = right.Imaginary;

        // Computing c * c + d * d will overflow even in cases where the actual result of the division does not overflow.
        if (float.Abs(d) < float.Abs(c))
        {
            var doc = d / c;
            return new ComplexFp32(a / (c + d * doc), (-a * doc) / (c + d * doc));
        }
        else
        {
            var cod = c / d;
            return new ComplexFp32(a * cod / (d + c * cod), -a / (d + c * cod));
        }
    }

    /// <summary>
    /// Computes the absolute value (or magnitude) of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the absolute value of.</param>
    /// <returns>The magnitude of <paramref name="value"/>, computed as sqrt(real^2 + imaginary^2) using single-precision floating-point arithmetic.</returns>
    public static float Abs(ComplexFp32 value)
    {
        return Hypot(value.Real, value.Imaginary);
    }

    private static float Hypot(float a, float b)
    {
        // Using
        //   sqrt(a^2 + b^2) = |a| * sqrt(1 + (b/a)^2)
        // we can factor out the larger component to dodge overflow even when a * a would overflow.

        a = float.Abs(a);
        b = float.Abs(b);

        float small, large;
        if (a < b)
        {
            small = a;
            large = b;
        }
        else
        {
            small = b;
            large = a;
        }

        if (small == 0f)
        {
            return (large);
        }
        else if (float.IsPositiveInfinity(large) && !float.IsNaN(small))
        {
            // The NaN test is necessary so we don't return +inf when small=NaN and large=+inf.
            // NaN in any other place returns NaN without any special handling.
            return (float.PositiveInfinity);
        }
        else
        {
            float ratio = small / large;
            return (large * float.Sqrt(1f + ratio * ratio));
        }

    }

    private static float Log1P(float x)
    {
        // Compute log(1 + x) without loss of accuracy when x is small.

        // Our only use case so far is for positive values, so this isn't coded to handle negative values.
        Debug.Assert((x >= 0f) || float.IsNaN(x));

        var xp1 = 1f + x;
        if (xp1 == 1f)
        {
            return x;
        }
        else if (x < 0.75f)
        {
            // This is accurate to within 5 ulp with any floating-point system that uses a guard digit,
            // as proven in Theorem 4 of "What Every Computer Scientist Should Know About Floating-Point
            // Arithmetic" (https://docs.oracle.com/cd/E19957-01/806-3568/ncg_goldberg.html)
            return x * float.Log(xp1) / (xp1 - 1f);
        }
        else
        {
            return float.Log(xp1);
        }

    }
    /// <summary>
    /// Returns the complex conjugate of a complex number.
    /// </summary>
    /// <param name="value">The complex number to conjugate.</param>
    /// <returns>The complex conjugate of <paramref name="value"/>, where the real part remains unchanged and the imaginary part is negated, using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Conjugate(ComplexFp32 value)
    {
        // Conjugate of a Complex number: the conjugate of x+i*y is x-i*y
        return new ComplexFp32(value.Real, -value.Imaginary);
    }

    /// <summary>
    /// Returns the reciprocal of a complex number.
    /// </summary>
    /// <param name="value">The complex number to find the reciprocal of.</param>
    /// <returns>The reciprocal of <paramref name="value"/>, computed as 1 divided by <paramref name="value"/>, or <see cref="Zero"/> if <paramref name="value"/> is zero, using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Reciprocal(ComplexFp32 value)
    {
        // Reciprocal of a Complex number : the reciprocal of x+i*y is 1/(x+i*y)
        if (value.Real == 0f && value.Imaginary == 0f)
        {
            return Zero;
        }
        return One / value;
    }

    /// <summary>
    /// Determines whether two complex numbers are equal.
    /// </summary>
    /// <param name="left">The first complex number to compare.</param>
    /// <param name="right">The second complex number to compare.</param>
    /// <returns><see langword="true"/> if the real and imaginary parts of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ComplexFp32 left, ComplexFp32 right)
    {
        return left.Real == right.Real && left.Imaginary == right.Imaginary;
    }

    /// <summary>
    /// Determines whether two complex numbers are not equal.
    /// </summary>
    /// <param name="left">The first complex number to compare.</param>
    /// <param name="right">The second complex number to compare.</param>
    /// <returns><see langword="true"/> if the real or imaginary parts of <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ComplexFp32 left, ComplexFp32 right)
    {
        return left.Real != right.Real || left.Imaginary != right.Imaginary;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current complex number.
    /// </summary>
    /// <param name="obj">The object to compare with the current complex number.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="ComplexFp32"/> and its real and imaginary parts are equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ComplexFp32 other && Equals(other);
    }

    /// <summary>
    /// Determines whether the specified complex number is equal to the current complex number.
    /// </summary>
    /// <param name="value">The complex number to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the real and imaginary parts of <paramref name="value"/> are equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ComplexFp32 value)
    {
        return Real.Equals(value.Real) && Imaginary.Equals(value.Imaginary);
    }

    /// <summary>
    /// Returns a hash code for the current complex number.
    /// </summary>
    /// <returns>A hash code based on the real and imaginary parts of the complex number.</returns>
    public override int GetHashCode() => HashCode.Combine(Real, Imaginary);

    /// <summary>
    /// Returns a string representation of the complex number using the default format.
    /// </summary>
    /// <returns>A string in the format "&lt;real; imaginary&gt;" using single-precision floating-point numbers.</returns>
    public override string ToString() => $"<{Real}; {Imaginary}>";

    /// <summary>
    /// Returns a string representation of the complex number with the specified format for its real and imaginary parts.
    /// </summary>
    /// <param name="format">The format string for the real and imaginary parts.</param>
    /// <returns>A string in the format "&lt;real; imaginary&gt;" with the specified format applied to the real and imaginary parts.</returns>
    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format) => ToString(format, null);

    /// <summary>
    /// Returns a string representation of the complex number using the specified format provider.
    /// </summary>
    /// <param name="provider">The format provider to use for formatting the real and imaginary parts.</param>
    /// <returns>A string in the format "&lt;real; imaginary&gt;" using the specified format provider.</returns>
    public string ToString(IFormatProvider? provider) => ToString(null, provider);

    /// <summary>
    /// Returns a string representation of the complex number with the specified format and format provider.
    /// </summary>
    /// <param name="format">The format string for the real and imaginary parts.</param>
    /// <param name="provider">The format provider to use for formatting the real and imaginary parts.</param>
    /// <returns>A string in the format "&lt;real; imaginary&gt;" with the specified format and format provider applied to the real and imaginary parts.</returns>
    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? provider)
    {
        return string.Format(provider, "<{0}; {1}>", Real.ToString(format, provider), Imaginary.ToString(format, provider));
    }

    /// <summary>
    /// Computes the sine of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the sine of.</param>
    /// <returns>The sine of <paramref name="value"/>, computed as sin(x)cosh(y) + i cos(x)sinh(y), where x is the real part and y is the imaginary part, using single-precision floating-point numbers.</returns>
    /// <remarks>
    /// There is a known limitation with this algorithm: inputs that cause sinh and cosh to overflow, but for
    /// which sin or cos are small enough that sin * cosh or cos * sinh are still representable, nonetheless
    /// produce overflow. For example, Sin((0.01, 711.0)) should produce (~3.0E306, PositiveInfinity), but
    /// instead produces (PositiveInfinity, PositiveInfinity).
    /// </remarks>
    public static ComplexFp32 Sin(ComplexFp32 value)
    {
        // We need both sinh and cosh of imaginary part. To avoid multiple calls to Math.Exp with the same value,
        // we compute them both here from a single call to Math.Exp.
        var p = float.Exp(value.Imaginary);
        var q = 1f / p;
        var sinh = (p - q) * .5f;
        var cosh = (p + q) * .5f;
        return new ComplexFp32(float.Sin(value.Real) * cosh, float.Cos(value.Real) * sinh);
        // There is a known limitation with this algorithm: inputs that cause sinh and cosh to overflow, but for
        // which sin or cos are small enough that sin * cosh or cos * sinh are still representable, nonetheless
        // produce overflow. For example, Sin((0.01, 711.0)) should produce (~3.0E306, PositiveInfinity), but
        // instead produces (PositiveInfinity, PositiveInfinity).
    }

    /// <summary>
    /// Computes the hyperbolic sine of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the hyperbolic sine of.</param>
    /// <returns>The hyperbolic sine of <paramref name="value"/>, computed via the relation sinh(z) = -i sin(iz), using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Sinh(ComplexFp32 value)
    {
        // Use sinh(z) = -i sin(iz) to compute via sin(z).
        ComplexFp32 sin = Sin(new ComplexFp32(-value.Imaginary, value.Real));
        return new ComplexFp32(sin.Imaginary, -sin.Real);
    }

    /// <summary>
    /// Computes the arcsine of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the arcsine of.</param>
    /// <returns>The arcsine of <paramref name="value"/>, using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Asin(ComplexFp32 value)
    {
        float b, bPrime, v;
        Asin_Internal(float.Abs(value.Real), float.Abs(value.Imaginary), out b, out bPrime, out v);

        float u;
        if (bPrime < 0f)
        {
            u = float.Asin(b);
        }
        else
        {
            u = float.Atan(bPrime);
        }

        if (value.Real < 0f) u = -u;
        if (value.Imaginary < 0f) v = -v;

        return new ComplexFp32(u, v);
    }

    /// <summary>
    /// Computes the cosine of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the cosine of.</param>
    /// <returns>The cosine of <paramref name="value"/>, computed as cos(x)cosh(y) - i sin(x)sinh(y), where x is the real part and y is the imaginary part, using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Cos(ComplexFp32 value)
    {
        var p = float.Exp(value.Imaginary);
        var q = 1f / p;
        var sinh = (p - q) * .5f;
        var cosh = (p + q) * .5f;
        return new ComplexFp32(float.Cos(value.Real) * cosh, -float.Sin(value.Real) * sinh);
    }

    /// <summary>
    /// Computes the hyperbolic cosine of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the hyperbolic cosine of.</param>
    /// <returns>The hyperbolic cosine of <paramref name="value"/>, computed via the relation cosh(z) = cos(iz), using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Cosh(ComplexFp32 value)
    {
        // Use cosh(z) = cos(iz) to compute via cos(z).
        return Cos(new ComplexFp32(-value.Imaginary, value.Real));
    }

    /// <summary>
    /// Computes the arccosine of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the arccosine of.</param>
    /// <returns>The arccosine of <paramref name="value"/>, using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Acos(ComplexFp32 value)
    {
        float b, bPrime, v;
        Asin_Internal(float.Abs(value.Real), float.Abs(value.Imaginary), out b, out bPrime, out v);

        float u;
        if (bPrime < 1f)
        {
            u = float.Acos(b);
        }
        else
        {
            u = float.Atan(1f / bPrime);
        }

        if (value.Real < 0f) u = float.Pi - u;
        if (value.Imaginary > 0f) v = -v;

        return new ComplexFp32(u, v);
    }

    /// <summary>
    /// Computes the tangent of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the tangent of.</param>
    /// <returns>The tangent of <paramref name="value"/>, computed as (sin(2x) + i sinh(2y)) / (cos(2x) + cosh(2y)) for small imaginary parts, or an equivalent form to avoid overflow, using single-precision floating-point numbers.</returns>
    /// <remarks>
    /// This approach does not work for |y| > ~355, because sinh(2y) and cosh(2y) overflow,
    /// even though their ratio does not. In that case, the method divides through by cosh to compute
    /// tan z = (sin(2x) / cosh(2y) + i tanh(2y)) / (1 + cos(2x) / cosh(2y)).
    /// </remarks>
    public static ComplexFp32 Tan(ComplexFp32 value)
    {
        // tan z = sin z / cos z, but to avoid unnecessary repeated trig computations, use
        //   tan z = (sin(2x) + i sinh(2y)) / (cos(2x) + cosh(2y))
        // (see Abramowitz & Stegun 4.3.57 or derive by hand), and compute trig functions here.

        // This approach does not work for |y| > ~355, because sinh(2y) and cosh(2y) overflow,
        // even though their ratio does not. In that case, divide through by cosh to get:
        //   tan z = (sin(2x) / cosh(2y) + i \tanh(2y)) / (1 + cos(2x) / cosh(2y))
        // which correctly computes the (tiny) real part and the (normal-sized) imaginary part.

        var x2 = 2f * value.Real;
        var y2 = 2f * value.Imaginary;
        var p = float.Exp(y2);
        var q = 1f / p;
        var cosh = (p + q) * .5f;
        if (float.Abs(value.Imaginary) <= 4f)
        {
            var sinh = (p - q) * .5f;
            var D = float.Cos(x2) + cosh;
            return new ComplexFp32(float.Sin(x2) / D, sinh / D);
        }
        else
        {
            var D = 1f + float.Cos(x2) / cosh;
            return new ComplexFp32(float.Sin(x2) / cosh / D, float.Tanh(y2) / D);
        }
    }

    /// <summary>
    /// Computes the hyperbolic tangent of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the hyperbolic tangent of.</param>
    /// <returns>The hyperbolic tangent of <paramref name="value"/>, computed via the relation tanh(z) = -i tan(iz), using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Tanh(ComplexFp32 value)
    {
        // Use tanh(z) = -i tan(iz) to compute via tan(z).
        ComplexFp32 tan = Tan(new ComplexFp32(-value.Imaginary, value.Real));
        return new ComplexFp32(tan.Imaginary, -tan.Real);
    }

    /// <summary>
    /// Computes the arctangent of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the arctangent of.</param>
    /// <returns>The arctangent of <paramref name="value"/>, computed as (i/2) * (log(1 - iz) - log(1 + iz)), using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Atan(ComplexFp32 value)
    {
        ComplexFp32 two = new(2f, 0f);
        return (ImaginaryOne / two) * (Log(One - ImaginaryOne * value) - Log(One + ImaginaryOne * value));
    }

    private static void Asin_Internal(float x, float y, out float b, out float bPrime, out float v)
    {

        // This method for the inverse complex sine (and cosine) is described in Hull, Fairgrieve,
        // and Tang, "Implementing the Complex Arcsine and Arccosine Functions Using Exception Handling",
        // ACM Transactions on Mathematical Software (1997)
        // (https://www.researchgate.net/profile/Ping_Tang3/publication/220493330_Implementing_the_Complex_Arcsine_and_Arccosine_Functions_Using_Exception_Handling/links/55b244b208ae9289a085245d.pdf)

        // First, the basics: start with sin(w) = (e^{iw} - e^{-iw}) / (2i) = z. Here z is the input
        // and w is the output. To solve for w, define t = e^{i w} and multiply through by t to
        // get the quadratic equation t^2 - 2 i z t - 1 = 0. The solution is t = i z + sqrt(1 - z^2), so
        //   w = arcsin(z) = - i log( i z + sqrt(1 - z^2) )
        // Decompose z = x + i y, multiply out i z + sqrt(1 - z^2), use log(s) = |s| + i arg(s), and do a
        // bunch of algebra to get the components of w = arcsin(z) = u + i v
        //   u = arcsin(beta)  v = sign(y) log(alpha + sqrt(alpha^2 - 1))
        // where
        //   alpha = (rho + sigma) / 2      beta = (rho - sigma) / 2
        //   rho = sqrt((x + 1)^2 + y^2)    sigma = sqrt((x - 1)^2 + y^2)
        // These formulas appear in DLMF section 4.23. (http://dlmf.nist.gov/4.23), along with the analogous
        //   arccos(w) = arccos(beta) - i sign(y) log(alpha + sqrt(alpha^2 - 1))
        // So alpha and beta together give us arcsin(w) and arccos(w).

        // As written, alpha is not susceptible to cancelation errors, but beta is. To avoid cancelation, note
        //   beta = (rho^2 - sigma^2) / (rho + sigma) / 2 = (2 x) / (rho + sigma) = x / alpha
        // which is not subject to cancelation. Note alpha >= 1 and |beta| <= 1.

        // For alpha ~ 1, the argument of the log is near unity, so we compute (alpha - 1) instead,
        // write the argument as 1 + (alpha - 1) + sqrt((alpha - 1)(alpha + 1)), and use the log1p function
        // to compute the log without loss of accuracy.
        // For beta ~ 1, arccos does not accurately resolve small angles, so we compute the tangent of the angle
        // instead.
        // Hull, Fairgrieve, and Tang derive formulas for (alpha - 1) and beta' = tan(u) that do not suffer
        // from cancelation in these cases.

        // For simplicity, we assume all positive inputs and return all positive outputs. The caller should
        // assign signs appropriate to the desired cut conventions. We return v directly since its magnitude
        // is the same for both arcsin and arccos. Instead of u, we usually return beta and sometimes beta'.
        // If beta' is not computed, it is set to -1; if it is computed, it should be used instead of beta
        // to determine u. Compute u = arcsin(beta) or u = arctan(beta') for arcsin, u = arccos(beta)
        // or arctan(1/beta') for arccos.

        Debug.Assert((x >= 0f) || float.IsNaN(x));
        Debug.Assert((y >= 0f) || float.IsNaN(y));

        // For x or y large enough to overflow alpha^2, we can simplify our formulas and avoid overflow.
        if ((x > s_asinOverflowThreshold) || (y > s_asinOverflowThreshold))
        {
            b = -1f;
            bPrime = x / y;

            float small, big;
            if (x < y)
            {
                small = x;
                big = y;
            }
            else
            {
                small = y;
                big = x;
            }
            var ratio = small / big;
            v = s_log2 + float.Log(big) + 0.5f * Log1P(ratio * ratio);
        }
        else
        {
            var r = Hypot((x + 1f), y);
            var s = Hypot((x - 1f), y);

            var a = (r + s) * 0.5f;
            b = x / a;

            if (b > 0.75f)
            {
                if (x <= 1f)
                {
                    var amx = (y * y / (r + (x + 1f)) + (s + (1f - x))) * 0.5f;
                    bPrime = x / float.Sqrt((a + x) * amx);
                }
                else
                {
                    // In this case, amx ~ y^2. Since we take the square root of amx, we should
                    // pull y out from under the square root so we don't lose its contribution
                    // when y^2 underflows.
                    var t = (1f / (r + (x + 1f)) + 1f / (s + (x - 1f))) * 0.5f;
                    bPrime = x / y / float.Sqrt((a + x) * t);
                }
            }
            else
            {
                bPrime = -1f;
            }

            if (a < 1.5f)
            {
                if (x < 1f)
                {
                    // This is another case where our expression is proportional to y^2 and
                    // we take its square root, so again we pull out a factor of y from
                    // under the square root.
                    var t = (1f / (r + (x + 1f)) + 1f / (s + (1f - x))) * .5f;
                    var am1 = y * y * t;
                    v = Log1P(am1 + y * float.Sqrt(t * (a + 1f)));
                }
                else
                {
                    float am1 = (y * y / (r + (x + 1f)) + (s + (x - 1f))) * float.CreateChecked(0.5);
                    v = Log1P(am1 + float.Sqrt(am1 * (a + 1f)));
                }
            }
            else
            {
                // Because of the test above, we can be sure that a * a will not overflow.
                v = float.Log(a + float.Sqrt((a - 1f) * (a + 1f)));
            }
        }
    }

    /// <summary>
    /// Determines whether a complex number is finite.
    /// </summary>
    /// <param name="value">The complex number to check.</param>
    /// <returns><see langword="true"/> if both the real and imaginary parts of <paramref name="value"/> are finite; otherwise, <see langword="false"/>.</returns>
    public static bool IsFinite(ComplexFp32 value) => float.IsFinite(value.Real) && float.IsFinite(value.Imaginary);

    /// <summary>
    /// Determines whether a complex number is infinite.
    /// </summary>
    /// <param name="value">The complex number to check.</param>
    /// <returns><see langword="true"/> if either the real or imaginary part of <paramref name="value"/> is infinite; otherwise, <see langword="false"/>.</returns>
    public static bool IsInfinity(ComplexFp32 value) => float.IsInfinity(value.Real) || float.IsInfinity(value.Imaginary);

    /// <summary>
    /// Determines whether a complex number is NaN (Not a Number).
    /// </summary>
    /// <param name="value">The complex number to check.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is neither finite nor infinite (i.e., NaN); otherwise, <see langword="false"/>.</returns>
    public static bool IsNaN(ComplexFp32 value) => !IsInfinity(value) && !IsFinite(value);

    /// <summary>
    /// Computes the natural logarithm of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the logarithm of.</param>
    /// <returns>The natural logarithm of <paramref name="value"/>, computed as ln(|z|) + i*arg(z), where |z| is the magnitude and arg(z) is the phase, using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Log(ComplexFp32 value)
    {
        return new ComplexFp32(float.Log(Abs(value)), float.Atan2(value.Imaginary, value.Real));
    }

    /// <summary>
    /// Computes the logarithm of a complex number with a specified base.
    /// </summary>
    /// <param name="value">The complex number to compute the logarithm of.</param>
    /// <param name="baseValue">The base of the logarithm.</param>
    /// <returns>The logarithm of <paramref name="value"/> with base <paramref name="baseValue"/>, computed as log(value)/log(baseValue), using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Log(ComplexFp32 value, float baseValue)
    {
        return Log(value) / Log(baseValue);
    }

    /// <summary>
    /// Computes the base-10 logarithm of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the base-10 logarithm of.</param>
    /// <returns>The base-10 logarithm of <paramref name="value"/>, computed as log(value)/log(10), using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Log10(ComplexFp32 value)
    {
        ComplexFp32 tempLog = Log(value);
        return Scale(tempLog, InverseOfLog10);
    }

    /// <summary>
    /// Computes the exponential of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the exponential of.</param>
    /// <returns>The exponential of <paramref name="value"/>, computed as e^x * (cos(y) + i*sin(y)), where x is the real part and y is the imaginary part, using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Exp(ComplexFp32 value)
    {
        var expReal = float.Exp(value.Real);
        var cosImaginary = expReal * float.Cos(value.Imaginary);
        var sinImaginary = expReal * float.Sin(value.Imaginary);
        return new ComplexFp32(cosImaginary, sinImaginary);
    }

    /// <summary>
    /// Computes the square root of a complex number.
    /// </summary>
    /// <param name="value">The complex number to compute the square root of.</param>
    /// <returns>The principal square root of <paramref name="value"/> with non-negative real part, using single-precision floating-point numbers.</returns>
    /// <remarks>
    /// If the components are too large, Hypot will overflow, even though the subsequent sqrt would
    /// make the result representable. To avoid this, we re-scale (by exact powers of 2 for accuracy)
    /// when we encounter very large components to avoid intermediate infinities.
    /// </remarks>
    public static ComplexFp32 Sqrt(ComplexFp32 value)
    {
        if (value.Imaginary == 0f)
        {
            // Handle the trivial case quickly.
            if (value.Real < 0f)
            {
                return new ComplexFp32(0f, float.Sqrt(-value.Real));
            }

            return new ComplexFp32(float.Sqrt(value.Real), 0f);
        }

        // One way to compute Sqrt(z) is just to call Pow(z, 0.5), which coverts to polar coordinates
        // (sqrt + atan), halves the phase, and reconverts to cartesian coordinates (cos + sin).
        // Not only is this more expensive than necessary, it also fails to preserve certain expected
        // symmetries, such as that the square root of a pure negative is a pure imaginary, and that the
        // square root of a pure imaginary has exactly equal real and imaginary parts. This all goes
        // back to the fact that Math.PI is not stored with infinite precision, so taking half of Math.PI
        // does not land us on an argument with cosine exactly equal to zero.

        // To find a fast and symmetry-respecting formula for complex square root,
        // note x + i y = \sqrt{a + i b} implies x^2 + 2 i x y - y^2 = a + i b,
        // so x^2 - y^2 = a and 2 x y = b. Cross-substitute and use the quadratic formula to obtain
        //   x = \sqrt{\frac{\sqrt{a^2 + b^2} + a}{2}}  y = \pm \sqrt{\frac{\sqrt{a^2 + b^2} - a}{2}}
        // There is just one complication: depending on the sign on a, either x or y suffers from
        // cancelation when |b| << |a|. We can get around this by noting that our formulas imply
        // x^2 y^2 = b^2 / 4, so |x| |y| = |b| / 2. So after computing the one that doesn't suffer
        // from cancelation, we can compute the other with just a division. This is basically just
        // the right way to evaluate the quadratic formula without cancelation.

        // All this reduces our total cost to two sqrts and a few flops, and it respects the desired
        // symmetries. Much better than atan + cos + sin!

        // The signs are a matter of choice of branch cut, which is traditionally taken so x > 0 and sign(y) = sign(b).

        // If the components are too large, Hypot will overflow, even though the subsequent sqrt would
        // make the result representable. To avoid this, we re-scale (by exact powers of 2 for accuracy)
        // when we encounter very large components to avoid intermediate infinities.
        bool rescale = false;
        var realCopy = value.Real;
        var imaginaryCopy = value.Imaginary;
        if ((float.Abs(realCopy) >= s_sqrtRescaleThreshold) || (float.Abs(imaginaryCopy) >= s_sqrtRescaleThreshold))
        {
            if (float.IsInfinity(value.Imaginary) && !float.IsNaN(value.Real))
            {
                // We need to handle infinite imaginary parts specially because otherwise
                // our formulas below produce inf/inf = NaN. The NaN test is necessary
                // so that we return NaN rather than (+inf,inf) for (NaN,inf).
                return (new ComplexFp32(float.PositiveInfinity, imaginaryCopy));
            }

            realCopy *= 0.25f;
            imaginaryCopy *= 0.25f;
            rescale = true;
        }

        // This is the core of the algorithm. Everything else is special case handling.
        float x, y;
        if (realCopy >= 0f)
        {
            x = float.Sqrt((Hypot(realCopy, imaginaryCopy) + realCopy) * 0.5f);
            y = imaginaryCopy / (2f * x);
        }
        else
        {
            y = float.Sqrt((Hypot(realCopy, imaginaryCopy) - realCopy) * 0.5f);
            if (imaginaryCopy < 0f) y = -y;
            x = imaginaryCopy / (2f * y);
        }

        if (rescale)
        {
            x *= 2f;
            y *= 2f;
        }

        return new ComplexFp32(x, y);
    }

    /// <summary>
    /// Computes a complex number raised to a complex power.
    /// </summary>
    /// <param name="value">The complex number to raise to the power.</param>
    /// <param name="power">The complex power to raise the number to.</param>
    /// <returns>The result of raising <paramref name="value"/> to the power of <paramref name="power"/>, using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Pow(ComplexFp32 value, ComplexFp32 power)
    {
        if (power == Zero)
        {
            return One;
        }

        if (value == Zero)
        {
            return Zero;
        }

        var valueReal = value.Real;
        var valueImaginary = value.Imaginary;
        var powerReal = power.Real;
        var powerImaginary = power.Imaginary;

        var rho = Abs(value);
        var theta = float.Atan2(valueImaginary, valueReal);
        var newRho = powerReal * theta + powerImaginary * float.Log(rho);

        float t = float.Pow(rho, powerReal) * float.Pow(float.E, -powerImaginary * theta);

        return new ComplexFp32(t * float.Cos(newRho), t * float.Sin(newRho));
    }

    /// <summary>
    /// Computes a complex number raised to a real power.
    /// </summary>
    /// <param name="value">The complex number to raise to the power.</param>
    /// <param name="power">The real power to raise the number to.</param>
    /// <returns>The result of raising <paramref name="value"/> to the power of <paramref name="power"/>, using single-precision floating-point numbers.</returns>
    public static ComplexFp32 Pow(ComplexFp32 value, float power)
    {
        return Pow(value, new ComplexFp32(power, 0f));
    }

    private static ComplexFp32 Scale(ComplexFp32 value, float factor)
    {
        var realResult = factor * value.Real;
        var imaginaryResuilt = factor * value.Imaginary;
        return new ComplexFp32(realResult, imaginaryResuilt);
    }

    //
    // Explicit Conversions To Complex
    //

    public static explicit operator ComplexFp32(decimal value)
    {
        return new ComplexFp32((float)value, 0f);
    }

    /// <summary>Explicitly converts a <see cref="Int128" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    public static explicit operator ComplexFp32(Int128 value)
    {
        return new ComplexFp32((float)value, 0f);
    }

    public static explicit operator ComplexFp32(BigInteger value)
    {
        return new ComplexFp32((float)value, 0f);
    }

    /// <summary>Explicitly converts a <see cref="UInt128" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    [CLSCompliant(false)]
    public static explicit operator ComplexFp32(UInt128 value)
    {
        return new ComplexFp32((float)value, 0f);
    }

    //
    // Implicit Conversions To Complex
    //

    public static implicit operator ComplexFp32(byte value)
    {
        return new ComplexFp32(value, 0f);
    }

    /// <summary>Implicitly converts a <see cref="char" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    public static implicit operator ComplexFp32(char value)
    {
        return new ComplexFp32(value, 0f);
    }

    public static implicit operator ComplexFp32(double value)
    {
        return new ComplexFp32((float)value, 0f);
    }

    /// <summary>Implicitly converts a <see cref="Half" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    public static implicit operator ComplexFp32(Half value)
    {
        return new ComplexFp32((float)value, 0f);
    }

    public static implicit operator ComplexFp32(short value)
    {
        return new ComplexFp32(value, 0f);
    }

    public static implicit operator ComplexFp32(int value)
    {
        return new ComplexFp32(value, 0f);
    }

    public static implicit operator ComplexFp32(long value)
    {
        return new ComplexFp32(value, 0f);
    }

    /// <summary>Implicitly converts a <see cref="IntPtr" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    public static implicit operator ComplexFp32(nint value)
    {
        return new ComplexFp32(value, 0f);
    }

    [CLSCompliant(false)]
    public static implicit operator ComplexFp32(sbyte value)
    {
        return new ComplexFp32(value, 0f);
    }

    public static implicit operator ComplexFp32(float value)
    {
        return new ComplexFp32(value, 0f);
    }

    [CLSCompliant(false)]
    public static implicit operator ComplexFp32(ushort value)
    {
        return new ComplexFp32(value, 0f);
    }

    [CLSCompliant(false)]
    public static implicit operator ComplexFp32(uint value)
    {
        return new ComplexFp32(value, 0f);
    }

    [CLSCompliant(false)]
    public static implicit operator ComplexFp32(ulong value)
    {
        return new ComplexFp32(value, 0f);
    }

    /// <summary>Implicitly converts a <see cref="UIntPtr" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    [CLSCompliant(false)]
    public static implicit operator ComplexFp32(nuint value)
    {
        return new ComplexFp32(value, 0f);
    }

    //
    // IAdditiveIdentity
    //

    /// <inheritdoc cref="IAdditiveIdentity{TSelf, TResult}.AdditiveIdentity" />
    static ComplexFp32 IAdditiveIdentity<ComplexFp32, ComplexFp32>.AdditiveIdentity => new(0f, 0f);

    //
    // IDecrementOperators
    //

    /// <inheritdoc cref="IDecrementOperators{TSelf}.op_Decrement(TSelf)" />
    public static ComplexFp32 operator --(ComplexFp32 value) => value - One;

    //
    // IIncrementOperators
    //

    /// <inheritdoc cref="IIncrementOperators{TSelf}.op_Increment(TSelf)" />
    public static ComplexFp32 operator ++(ComplexFp32 value) => value + One;

    //
    // IMultiplicativeIdentity
    //

    /// <inheritdoc cref="IMultiplicativeIdentity{TSelf, TResult}.MultiplicativeIdentity" />
    static ComplexFp32 IMultiplicativeIdentity<ComplexFp32, ComplexFp32>.MultiplicativeIdentity => new ComplexFp32(1f, 0f);

    //
    // INumberBase
    //

    /// <inheritdoc cref="INumberBase{TSelf}.One" />
    static ComplexFp32 INumberBase<ComplexFp32>.One => new ComplexFp32(1f, 0f);

    /// <inheritdoc cref="INumberBase{TSelf}.Radix" />
    static int INumberBase<ComplexFp32>.Radix => 2;

    /// <inheritdoc cref="INumberBase{TSelf}.Zero" />
    static ComplexFp32 INumberBase<ComplexFp32>.Zero => new ComplexFp32(0f, 0f);

    /// <inheritdoc cref="INumberBase{TSelf}.Abs(TSelf)" />
    static ComplexFp32 INumberBase<ComplexFp32>.Abs(ComplexFp32 value) => Abs(value);

    /// <inheritdoc cref="INumberBase{TSelf}.CreateChecked{TOther}(TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComplexFp32 CreateChecked<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        ComplexFp32 result;

        if (typeof(TOther) == typeof(ComplexFp32))
        {
            result = (ComplexFp32)(object)value;
        }
        else if (!TryConvertFrom(value, out result) && !TOther.TryConvertToChecked(value, out result))
        {
            throw new NotSupportedException();
        }

        return result;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.CreateSaturating{TOther}(TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComplexFp32 CreateSaturating<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        ComplexFp32 result;

        if (typeof(TOther) == typeof(ComplexFp32))
        {
            result = (ComplexFp32)(object)value;
        }
        else if (!TryConvertFrom(value, out result) && !TOther.TryConvertToSaturating(value, out result))
        {
            throw new NotSupportedException();
        }

        return result;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.CreateTruncating{TOther}(TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComplexFp32 CreateTruncating<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        ComplexFp32 result;

        if (typeof(TOther) == typeof(ComplexFp32))
        {
            result = (ComplexFp32)(object)value;
        }
        else if (!TryConvertFrom(value, out result) && !TOther.TryConvertToTruncating(value, out result))
        {
            throw new NotSupportedException();
        }

        return result;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsCanonical(TSelf)" />
    static bool INumberBase<ComplexFp32>.IsCanonical(ComplexFp32 value) => true;

    /// <inheritdoc cref="INumberBase{TSelf}.IsComplexNumber(TSelf)" />
    public static bool IsComplexNumber(ComplexFp32 value) => (value.Real != 0f) && (value.Imaginary != 0f);

    /// <inheritdoc cref="INumberBase{TSelf}.IsEvenInteger(TSelf)" />
    public static bool IsEvenInteger(ComplexFp32 value) => (value.Imaginary == 0f) && float.IsEvenInteger(value.Real);

    /// <inheritdoc cref="INumberBase{TSelf}.IsImaginaryNumber(TSelf)" />
    public static bool IsImaginaryNumber(ComplexFp32 value) => (value.Real == 0f) && float.IsRealNumber(value.Imaginary);

    /// <inheritdoc cref="INumberBase{TSelf}.IsInteger(TSelf)" />
    public static bool IsInteger(ComplexFp32 value) => (value.Imaginary == 0f) && float.IsInteger(value.Real);

    /// <inheritdoc cref="INumberBase{TSelf}.IsNegative(TSelf)" />
    public static bool IsNegative(ComplexFp32 value)
    {
        // since complex numbers do not have a well-defined concept of
        // negative we report false if this value has an imaginary part

        return (value.Imaginary == 0f) && float.IsNegative(value.Real);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsNegativeInfinity(TSelf)" />
    public static bool IsNegativeInfinity(ComplexFp32 value)
    {
        // since complex numbers do not have a well-defined concept of
        // negative we report false if this value has an imaginary part

        return (value.Imaginary == 0f) && float.IsNegativeInfinity(value.Real);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsNormal(TSelf)" />
    public static bool IsNormal(ComplexFp32 value)
    {
        // much as IsFinite requires both part to be finite, we require both
        // part to be "normal" (finite, non-zero, and non-subnormal) to be true

        return float.IsNormal(value.Real)
            && ((value.Imaginary == 0f) || float.IsNormal(value.Imaginary));
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsOddInteger(TSelf)" />
    public static bool IsOddInteger(ComplexFp32 value) => (value.Imaginary == 0f) && float.IsOddInteger(value.Real);

    /// <inheritdoc cref="INumberBase{TSelf}.IsPositive(TSelf)" />
    public static bool IsPositive(ComplexFp32 value)
    {
        // since complex numbers do not have a well-defined concept of
        // negative we report false if this value has an imaginary part

        return (value.Imaginary == 0f) && float.IsPositive(value.Real);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsPositiveInfinity(TSelf)" />
    public static bool IsPositiveInfinity(ComplexFp32 value)
    {
        // since complex numbers do not have a well-defined concept of
        // positive we report false if this value has an imaginary part

        return (value.Imaginary == 0f) && float.IsPositiveInfinity(value.Real);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsRealNumber(TSelf)" />
    public static bool IsRealNumber(ComplexFp32 value) => (value.Imaginary == 0f) && float.IsRealNumber(value.Real);

    /// <inheritdoc cref="INumberBase{TSelf}.IsSubnormal(TSelf)" />
    public static bool IsSubnormal(ComplexFp32 value)
    {
        // much as IsInfinite allows either part to be infinite, we allow either
        // part to be "subnormal" (finite, non-zero, and non-normal) to be true

        return float.IsSubnormal(value.Real) || float.IsSubnormal(value.Imaginary);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsZero(TSelf)" />
    static bool INumberBase<ComplexFp32>.IsZero(ComplexFp32 value) => (value.Real == 0f) && (value.Imaginary == 0f);

    /// <inheritdoc cref="INumberBase{TSelf}.MaxMagnitude(TSelf, TSelf)" />
    public static ComplexFp32 MaxMagnitude(ComplexFp32 x, ComplexFp32 y)
    {
        // complex numbers are not normally comparable, however every complex
        // number has a real magnitude (absolute value) and so we can provide
        // an implementation for MaxMagnitude

        // This matches the IEEE 754:2019 `maximumMagnitude` function
        //
        // It propagates NaN inputs back to the caller and
        // otherwise returns the input with a larger magnitude.
        // It treats +0 as larger than -0 as per the specification.

        var ax = Abs(x);
        var ay = Abs(y);

        if ((ax > ay) || float.IsNaN(ax))
        {
            return x;
        }

        if (ax == ay)
        {
            // We have two equal magnitudes which means we have two of the following
            //   `+a + ib`
            //   `-a + ib`
            //   `+a - ib`
            //   `-a - ib`
            //
            // We want to treat `+a + ib` as greater than everything and `-a - ib` as
            // lesser. For `-a + ib` and `+a - ib` its "ambiguous" which should be preferred
            // so we will just preference `+a - ib` since that's the most correct choice
            // in the face of something like `+a - i0.0` vs `-a + i0.0`. This is the "most
            // correct" choice because both represent real numbers and `+a` is preferred
            // over `-a`.

            if (float.IsNegative(y.Real))
            {
                if (float.IsNegative(y.Imaginary))
                {
                    // when `y` is `-a - ib` we always prefer `x` (its either the same as
                    // `x` or some part of `x` is positive).

                    return x;
                }
                else
                {
                    if (float.IsNegative(x.Real))
                    {
                        // when `y` is `-a + ib` and `x` is `-a + ib` or `-a - ib` then
                        // we either have same value or both parts of `x` are negative
                        // and we want to prefer `y`.

                        return y;
                    }
                    else
                    {
                        // when `y` is `-a + ib` and `x` is `+a + ib` or `+a - ib` then
                        // we want to prefer `x` because either both parts are positive
                        // or we want to prefer `+a - ib` due to how it handles when `x`
                        // represents a real number.

                        return x;
                    }
                }
            }
            else if (float.IsNegative(y.Imaginary))
            {
                if (float.IsNegative(x.Real))
                {
                    // when `y` is `+a - ib` and `x` is `-a + ib` or `-a - ib` then
                    // we either both parts of `x` are negative or we want to prefer
                    // `+a - ib` due to how it handles when `y` represents a real number.

                    return y;
                }
                else
                {
                    // when `y` is `+a - ib` and `x` is `+a + ib` or `+a - ib` then
                    // we want to prefer `x` because either both parts are positive
                    // or they represent the same value.

                    return x;
                }
            }
        }

        return y;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.MaxMagnitudeNumber(TSelf, TSelf)" />
    static ComplexFp32 INumberBase<ComplexFp32>.MaxMagnitudeNumber(ComplexFp32 x, ComplexFp32 y)
    {
        // complex numbers are not normally comparable, however every complex
        // number has a real magnitude (absolute value) and so we can provide
        // an implementation for MaxMagnitudeNumber

        // This matches the IEEE 754:2019 `maximumMagnitudeNumber` function
        //
        // It does not propagate NaN inputs back to the caller and
        // otherwise returns the input with a larger magnitude.
        // It treats +0 as larger than -0 as per the specification.

        var ax = Abs(x);
        var ay = Abs(y);

        if ((ax > ay) || float.IsNaN(ay))
        {
            return x;
        }

        if (ax == ay)
        {
            // We have two equal magnitudes which means we have two of the following
            //   `+a + ib`
            //   `-a + ib`
            //   `+a - ib`
            //   `-a - ib`
            //
            // We want to treat `+a + ib` as greater than everything and `-a - ib` as
            // lesser. For `-a + ib` and `+a - ib` its "ambiguous" which should be preferred
            // so we will just preference `+a - ib` since that's the most correct choice
            // in the face of something like `+a - i0.0` vs `-a + i0.0`. This is the "most
            // correct" choice because both represent real numbers and `+a` is preferred
            // over `-a`.

            if (float.IsNegative(y.Real))
            {
                if (float.IsNegative(y.Imaginary))
                {
                    // when `y` is `-a - ib` we always prefer `x` (its either the same as
                    // `x` or some part of `x` is positive).

                    return x;
                }
                else
                {
                    if (float.IsNegative(x.Real))
                    {
                        // when `y` is `-a + ib` and `x` is `-a + ib` or `-a - ib` then
                        // we either have same value or both parts of `x` are negative
                        // and we want to prefer `y`.

                        return y;
                    }
                    else
                    {
                        // when `y` is `-a + ib` and `x` is `+a + ib` or `+a - ib` then
                        // we want to prefer `x` because either both parts are positive
                        // or we want to prefer `+a - ib` due to how it handles when `x`
                        // represents a real number.

                        return x;
                    }
                }
            }
            else if (float.IsNegative(y.Imaginary))
            {
                if (float.IsNegative(x.Real))
                {
                    // when `y` is `+a - ib` and `x` is `-a + ib` or `-a - ib` then
                    // we either both parts of `x` are negative or we want to prefer
                    // `+a - ib` due to how it handles when `y` represents a real number.

                    return y;
                }
                else
                {
                    // when `y` is `+a - ib` and `x` is `+a + ib` or `+a - ib` then
                    // we want to prefer `x` because either both parts are positive
                    // or they represent the same value.

                    return x;
                }
            }
        }

        return y;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.MinMagnitude(TSelf, TSelf)" />
    public static ComplexFp32 MinMagnitude(ComplexFp32 x, ComplexFp32 y)
    {
        // complex numbers are not normally comparable, however every complex
        // number has a real magnitude (absolute value) and so we can provide
        // an implementation for MaxMagnitude

        // This matches the IEEE 754:2019 `minimumMagnitude` function
        //
        // It propagates NaN inputs back to the caller and
        // otherwise returns the input with a smaller magnitude.
        // It treats -0 as smaller than +0 as per the specification.

        var ax = Abs(x);
        var ay = Abs(y);

        if ((ax < ay) || float.IsNaN(ax))
        {
            return x;
        }

        if (ax == ay)
        {
            // We have two equal magnitudes which means we have two of the following
            //   `+a + ib`
            //   `-a + ib`
            //   `+a - ib`
            //   `-a - ib`
            //
            // We want to treat `+a + ib` as greater than everything and `-a - ib` as
            // lesser. For `-a + ib` and `+a - ib` its "ambiguous" which should be preferred
            // so we will just preference `-a + ib` since that's the most correct choice
            // in the face of something like `+a - i0.0` vs `-a + i0.0`. This is the "most
            // correct" choice because both represent real numbers and `-a` is preferred
            // over `+a`.

            if (float.IsNegative(y.Real))
            {
                if (float.IsNegative(y.Imaginary))
                {
                    // when `y` is `-a - ib` we always prefer `y` as both parts are negative
                    return y;
                }
                else
                {
                    if (float.IsNegative(x.Real))
                    {
                        // when `y` is `-a + ib` and `x` is `-a + ib` or `-a - ib` then
                        // we either have same value or both parts of `x` are negative
                        // and we want to prefer it.

                        return x;
                    }
                    else
                    {
                        // when `y` is `-a + ib` and `x` is `+a + ib` or `+a - ib` then
                        // we want to prefer `y` because either both parts of 'x' are positive
                        // or we want to prefer `-a - ib` due to how it handles when `y`
                        // represents a real number.

                        return y;
                    }
                }
            }
            else if (float.IsNegative(y.Imaginary))
            {
                if (float.IsNegative(x.Real))
                {
                    // when `y` is `+a - ib` and `x` is `-a + ib` or `-a - ib` then
                    // either both parts of `x` are negative or we want to prefer
                    // `-a - ib` due to how it handles when `x` represents a real number.

                    return x;
                }
                else
                {
                    // when `y` is `+a - ib` and `x` is `+a + ib` or `+a - ib` then
                    // we want to prefer `y` because either both parts of x are positive
                    // or they represent the same value.

                    return y;
                }
            }
            else
            {
                return x;
            }
        }

        return y;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.MinMagnitudeNumber(TSelf, TSelf)" />
    static ComplexFp32 INumberBase<ComplexFp32>.MinMagnitudeNumber(ComplexFp32 x, ComplexFp32 y)
    {
        // complex numbers are not normally comparable, however every complex
        // number has a real magnitude (absolute value) and so we can provide
        // an implementation for MinMagnitudeNumber

        // This matches the IEEE 754:2019 `minimumMagnitudeNumber` function
        //
        // It does not propagate NaN inputs back to the caller and
        // otherwise returns the input with a smaller magnitude.
        // It treats -0 as smaller than +0 as per the specification.

        var ax = Abs(x);
        var ay = Abs(y);

        if ((ax < ay) || float.IsNaN(ay))
        {
            return x;
        }

        if (ax == ay)
        {
            // We have two equal magnitudes which means we have two of the following
            //   `+a + ib`
            //   `-a + ib`
            //   `+a - ib`
            //   `-a - ib`
            //
            // We want to treat `+a + ib` as greater than everything and `-a - ib` as
            // lesser. For `-a + ib` and `+a - ib` its "ambiguous" which should be preferred
            // so we will just preference `-a + ib` since that's the most correct choice
            // in the face of something like `+a - i0.0` vs `-a + i0.0`. This is the "most
            // correct" choice because both represent real numbers and `-a` is preferred
            // over `+a`.

            if (float.IsNegative(y.Real))
            {
                if (float.IsNegative(y.Imaginary))
                {
                    // when `y` is `-a - ib` we always prefer `y` as both parts are negative
                    return y;
                }
                else
                {
                    if (float.IsNegative(x.Real))
                    {
                        // when `y` is `-a + ib` and `x` is `-a + ib` or `-a - ib` then
                        // we either have same value or both parts of `x` are negative
                        // and we want to prefer it.

                        return x;
                    }
                    else
                    {
                        // when `y` is `-a + ib` and `x` is `+a + ib` or `+a - ib` then
                        // we want to prefer `y` because either both parts of 'x' are positive
                        // or we want to prefer `-a - ib` due to how it handles when `y`
                        // represents a real number.

                        return y;
                    }
                }
            }
            else if (float.IsNegative(y.Imaginary))
            {
                if (float.IsNegative(x.Real))
                {
                    // when `y` is `+a - ib` and `x` is `-a + ib` or `-a - ib` then
                    // either both parts of `x` are negative or we want to prefer
                    // `-a - ib` due to how it handles when `x` represents a real number.

                    return x;
                }
                else
                {
                    // when `y` is `+a - ib` and `x` is `+a + ib` or `+a - ib` then
                    // we want to prefer `y` because either both parts of x are positive
                    // or they represent the same value.

                    return y;
                }
            }
            else
            {
                return x;
            }
        }

        return y;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.Parse(ReadOnlySpan{char}, NumberStyles, IFormatProvider?)" />
    public static ComplexFp32 Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        if (!TryParse(s, style, provider, out ComplexFp32 result))
        {
            throw new OverflowException();
        }
        return result;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.Parse(string, NumberStyles, IFormatProvider?)" />
    public static ComplexFp32 Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), style, provider);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromChecked{TOther}(TOther, out TSelf)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<ComplexFp32>.TryConvertFromChecked<TOther>(TOther value, out ComplexFp32 result)
    {
        return TryConvertFrom<TOther>(value, out result);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromSaturating{TOther}(TOther, out TSelf)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<ComplexFp32>.TryConvertFromSaturating<TOther>(TOther value, out ComplexFp32 result)
    {
        return TryConvertFrom<TOther>(value, out result);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromTruncating{TOther}(TOther, out TSelf)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<ComplexFp32>.TryConvertFromTruncating<TOther>(TOther value, out ComplexFp32 result)
    {
        return TryConvertFrom<TOther>(value, out result);
    }

    private static bool TryConvertFrom<TOther>(TOther value, out ComplexFp32 result)
        where TOther : INumberBase<TOther>
    {
        // We don't want to defer to `double.Create*(value)` because some type might have its own
        // `TOther.ConvertTo*(value, out Complex result)` handling that would end up bypassed.

        if (typeof(TOther) == typeof(byte))
        {
            byte actualValue = (byte)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(char))
        {
            char actualValue = (char)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(decimal))
        {
            decimal actualValue = (decimal)(object)value;
            result = (ComplexFp32)actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(double))
        {
            double actualValue = (double)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(Half))
        {
            Half actualValue = (Half)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(short))
        {
            short actualValue = (short)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(int))
        {
            int actualValue = (int)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(long))
        {
            long actualValue = (long)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(Int128))
        {
            Int128 actualValue = (Int128)(object)value;
            result = (ComplexFp32)actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(nint))
        {
            nint actualValue = (nint)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(sbyte))
        {
            sbyte actualValue = (sbyte)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(float))
        {
            float actualValue = (float)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(ushort))
        {
            ushort actualValue = (ushort)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(uint))
        {
            uint actualValue = (uint)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(ulong))
        {
            ulong actualValue = (ulong)(object)value;
            result = actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(UInt128))
        {
            UInt128 actualValue = (UInt128)(object)value;
            result = (ComplexFp32)actualValue;
            return true;
        }
        else if (typeof(TOther) == typeof(nuint))
        {
            nuint actualValue = (nuint)(object)value;
            result = actualValue;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertToChecked{TOther}(TSelf, out TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<ComplexFp32>.TryConvertToChecked<TOther>(ComplexFp32 value, [MaybeNullWhen(false)] out TOther result)
    {
        // Complex numbers with an imaginary part can't be represented as a "real number"
        // so we'll throw an OverflowException for this scenario for integer types and
        // for decimal. However, we will convert it to NaN for the floating-point types,
        // since that's what Sqrt(-1) (which is `new Complex(0, 1)`) results in.
        result = TOther.CreateChecked(value);
        return true;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertToSaturating{TOther}(TSelf, out TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<ComplexFp32>.TryConvertToSaturating<TOther>(ComplexFp32 value, [MaybeNullWhen(false)] out TOther result)
    {
        // Complex numbers with an imaginary part can't be represented as a "real number"
        // and there isn't really a well-defined way to "saturate" to just a real value.
        //
        // The two potential options are that we either treat complex numbers with a non-
        // zero imaginary part as NaN and then convert that to 0 -or- we ignore the imaginary
        // part and only consider the real part.
        //
        // We use the latter below since that is "more useful" given an unknown number type.
        // Users who want 0 instead can always check `IsComplexNumber` and special-case the
        // handling.
        result = TOther.CreateSaturating(value.Real);
        return true;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertToTruncating{TOther}(TSelf, out TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<ComplexFp32>.TryConvertToTruncating<TOther>(ComplexFp32 value, [MaybeNullWhen(false)] out TOther result)
    {
        // Complex numbers with an imaginary part can't be represented as a "real number"
        // so we'll only consider the real part for the purposes of truncation.
        result = TOther.CreateTruncating(value.Real);
        return true;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryParse(ReadOnlySpan{char}, NumberStyles, IFormatProvider?, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out ComplexFp32 result)
    {
        ValidateParseStyleFloatingPoint(style);

        int openBracket = s.IndexOf('<');
        int semicolon = s.IndexOf(';');
        int closeBracket = s.IndexOf('>');

        if ((s.Length < 5) || (openBracket == -1) || (semicolon == -1) || (closeBracket == -1) || (openBracket > semicolon) || (openBracket > closeBracket) || (semicolon > closeBracket))
        {
            // We need at least 5 characters for `<0;0>`
            // We also expect a to find an open bracket, a semicolon, and a closing bracket in that order

            result = default;
            return false;
        }

        if ((openBracket != 0) && (((style & NumberStyles.AllowLeadingWhite) == 0) || !s.Slice(0, openBracket).IsWhiteSpace()))
        {
            // The opening bracket wasn't the first and we either didn't allow leading whitespace
            // or one of the leading characters wasn't whitespace at all.

            result = default;
            return false;
        }

        if (!float.TryParse(s.Slice(openBracket + 1, semicolon), style, provider, out float real))
        {
            result = default;
            return false;
        }

        if (char.IsWhiteSpace(s[semicolon + 1]))
        {
            // We allow a single whitespace after the semicolon regardless of style, this is so that
            // the output of `ToString` can be correctly parsed by default and values will roundtrip.
            semicolon += 1;
        }

        if (!float.TryParse(s.Slice(semicolon + 1, closeBracket - semicolon), style, provider, out float imaginary))
        {
            result = default;
            return false;
        }

        if ((closeBracket != (s.Length - 1)) && (((style & NumberStyles.AllowTrailingWhite) == 0) || !s.Slice(closeBracket).IsWhiteSpace()))
        {
            // The closing bracket wasn't the last and we either didn't allow trailing whitespace
            // or one of the trailing characters wasn't whitespace at all.

            result = default;
            return false;
        }

        result = new ComplexFp32(real, imaginary);
        return true;

        static void ValidateParseStyleFloatingPoint(NumberStyles style)
        {
            // Check for undefined flags or hex number
            if ((style & (InvalidNumberStyles | NumberStyles.AllowHexSpecifier)) != 0)
            {
                ThrowInvalid(style);

                static void ThrowInvalid(NumberStyles value)
                {
                    if ((value & InvalidNumberStyles) != 0)
                    {
                        throw new ArgumentException("Invalid Number Styles.", nameof(style));
                    }

                    throw new ArgumentException("Hex Style Not Supported.");
                }
            }
        }
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryParse(string, NumberStyles, IFormatProvider?, out TSelf)" />
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out ComplexFp32 result)
    {
        if (s is null)
        {
            result = default;
            return false;
        }
        return TryParse(s.AsSpan(), style, provider, out result);
    }

    //
    // IParsable
    //

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)" />
    public static ComplexFp32 Parse(string s, IFormatProvider? provider) => Parse(s, DefaultNumberStyle, provider);

    /// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)" />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out ComplexFp32 result) => TryParse(s, DefaultNumberStyle, provider, out result);

    //
    // ISignedNumber
    //

    /// <inheritdoc cref="ISignedNumber{TSelf}.NegativeOne" />
    static ComplexFp32 ISignedNumber<ComplexFp32>.NegativeOne => new(-1f, 0f);

    //
    // ISpanFormattable
    //

    /// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)" />
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        int charsWrittenSoFar = 0;

        // We have at least 6 more characters for: <0; 0>
        if (destination.Length < 6)
        {
            charsWritten = charsWrittenSoFar;
            return false;
        }

        destination[charsWrittenSoFar++] = '<';

        bool tryFormatSucceeded = Real.TryFormat(destination.Slice(charsWrittenSoFar), out int tryFormatCharsWritten, format, provider);
        charsWrittenSoFar += tryFormatCharsWritten;

        // We have at least 4 more characters for: ; 0>
        if (!tryFormatSucceeded || (destination.Length < (charsWrittenSoFar + 4)))
        {
            charsWritten = charsWrittenSoFar;
            return false;
        }

        destination[charsWrittenSoFar++] = ';';
        destination[charsWrittenSoFar++] = ' ';

        tryFormatSucceeded = Imaginary.TryFormat(destination.Slice(charsWrittenSoFar), out tryFormatCharsWritten, format, provider);
        charsWrittenSoFar += tryFormatCharsWritten;

        // We have at least 1 more character for: >
        if (!tryFormatSucceeded || (destination.Length < (charsWrittenSoFar + 1)))
        {
            charsWritten = charsWrittenSoFar;
            return false;
        }

        destination[charsWrittenSoFar++] = '>';

        charsWritten = charsWrittenSoFar;
        return true;
    }

    //
    // ISpanParsable
    //

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)" />
    public static ComplexFp32 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, DefaultNumberStyle, provider);

    /// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ComplexFp32 result) => TryParse(s, DefaultNumberStyle, provider, out result);

    //
    // IUnaryPlusOperators
    //

    /// <inheritdoc cref="IUnaryPlusOperators{TSelf, TResult}.op_UnaryPlus(TSelf)" />
    public static ComplexFp32 operator +(ComplexFp32 value) => value;
}