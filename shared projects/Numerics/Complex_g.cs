namespace Vorcyc.Mathematics.Numerics;

/*
 *  23.8.25 创建 ：
 *  1. 反编译了 BCL 中的 Complex 结构改成.NET 7 支持的泛型数学版本
 *  
 *  23.9.23 RENEW 
 *  1. 修改了些许 bug
 *  2. 不再是 只读 的
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
public /*readonly*/ struct Complex<T>
    : IEquatable<Complex<T>>,
      IFormattable,
      INumberBase<Complex<T>>,
      ISignedNumber<Complex<T>>
    where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
{

    private const NumberStyles DefaultNumberStyle = NumberStyles.Float | NumberStyles.AllowThousands;

    private const NumberStyles InvalidNumberStyles = ~(NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite
                                                     | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign
                                                     | NumberStyles.AllowParentheses | NumberStyles.AllowDecimalPoint
                                                     | NumberStyles.AllowThousands | NumberStyles.AllowExponent
                                                     | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowHexSpecifier);


    public static readonly Complex<T> Zero = new(T.Zero, T.Zero);
    public static readonly Complex<T> One = new(T.One, T.Zero);
    public static readonly Complex<T> ImaginaryOne = new(T.Zero, T.One);
    public static readonly Complex<T> NaN = new(T.NaN, T.NaN);
    public static readonly Complex<T> Infinity = new(T.PositiveInfinity, T.PositiveInfinity);

    private static T InverseOfLog10 = T.CreateChecked(0.43429448190325); // 1 / Log(10)

    // This is the largest x for which (Hypot(x,x) + x) will not overflow. It is used for branching inside Sqrt.
    private static readonly T s_sqrtRescaleThreshold = T.MaxValue / (T.Sqrt(T.CreateChecked(2)) + T.One);

    // This is the largest x for which 2 x^2 will not overflow. It is used for branching inside Asin and Acos.
    private static readonly T s_asinOverflowThreshold = T.Sqrt(T.MaxValue) / T.CreateSaturating(2);

    // This value is used inside Asin and Acos.
    private static readonly T s_log2 = T.Log(T.CreateChecked(2));

    // Do not rename, these fields are needed for binary serialization
    //private readonly T Real; // Do not rename (binary serialization)
    //private readonly T Imaginary; // Do not rename (binary serialization)

    public T Real;

    public T Imaginary;

    public Complex()
    {
        Real = T.Zero;
        Imaginary = T.Zero;
    }


    public Complex(T real, T imaginary)
    {
        Real = real;
        Imaginary = imaginary;
    }


    ///// <summary>
    ///// Gets the real part.
    ///// </summary>
    //public T Real { get { return m_real; } }

    ///// <summary>
    ///// Gets the imaginary part.
    ///// </summary>
    //public T Imaginary { get { return m_imaginary; } }

    #region 类型转换

    /// <summary>
    /// Deconstruct to <see cref="ValueTuple{T1,T2}"/>.
    /// </summary>
    /// <param name="real"></param>
    /// <param name="imaginary"></param>
    public void Deconstruct(out T real, out T imaginary)
    {
        real = Real;
        imaginary = Imaginary;
    }

    /// <summary>
    /// Implicitly converts a <see cref="Complex{T}"/> to a tuple containing its real and imaginary parts.
    /// </summary>
    public static implicit operator (T Real, T Imaginary)(Complex<T> value)
    {
        return (value.Real, value.Imaginary);
    }

    /// <summary>
    /// Implicitly converts a tuple of real and imaginary parts to a <see cref="Complex{T}"/>.
    /// </summary>
    public static implicit operator Complex<T>((T Real, T Imaginary) value)
    {
        return new Complex<T>(value.Real, value.Imaginary);
    }

    /// <summary>
    /// Converts a <see cref="Span{T}"/> containing at least two elements into a <see cref="Complex{T}"/> instance.
    /// The first element represents the real part, and the second element represents the imaginary part.
    /// </summary>
    /// <typeparam name="T">The numeric type of the real and imaginary parts, which must be an unmanaged type supporting arithmetic operations.</typeparam>
    /// <param name="span">A <see cref="Span{T}"/> with at least two elements, where span[0] is the real part and span[1] is the imaginary part.</param>
    /// <returns>A <see cref="Complex{T}"/> instance initialized with the real and imaginary parts from the input span.</returns>
    /// <exception cref="ArgumentException">Thrown when the input span has fewer than two elements.</exception>
    /// <remarks>
    /// This method uses <see cref="System.Runtime.CompilerServices.Unsafe.As{TFrom, TTo}(ref TFrom)"/> for efficient memory reinterpretation,
    /// assuming the memory layout of <see cref="Complex{T}"/> is compatible with two consecutive <typeparamref name="T"/> values.
    /// Ensure that <see cref="Complex{T}"/> is defined with <see cref="System.Runtime.InteropServices.StructLayoutAttribute"/>
    /// set to <see cref="System.Runtime.InteropServices.LayoutKind.Sequential"/> to guarantee correct memory alignment.
    /// The type <typeparamref name="T"/> must be an unmanaged type to ensure safe memory reinterpretation.
    /// </remarks>
    public static Complex<T> FromSpan(Span<T> span)
    {
        if (span.Length < 2)
            throw new ArgumentException("Span must have at least 2 elements.");

        //return new ComplexF(span[0], span[1]);
        return Unsafe.As<T, Complex<T>>(ref span[0]);
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
    /// Gets the magnitude (or absolute value) of a complex number.
    /// </summary>
    public T Magnitude => Abs(this);

    /// <summary>
    /// Gets the phase of a complex number.
    /// </summary>
    public T Phase => T.Atan2(Imaginary, Real);

    /// <summary>
    /// Creates a complex number from polar coordinates using the specified numeric type.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="magnitude">The magnitude, which is the distance from the origin (the absolute value).</param>
    /// <param name="phase">The phase, which is the angle from the real axis, in radians.</param>
    /// <returns>A complex number with the specified magnitude and phase.</returns>
    public static Complex<T> FromPolarCoordinates(T magnitude, T phase)
    {
        return new Complex<T>(magnitude * T.Cos(phase), magnitude * T.Sin(phase));
    }

    /// <summary>
    /// Negates a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to negate.</param>
    /// <returns>The result of negating the real and imaginary parts of <paramref name="value"/>.</returns>
    public static Complex<T> Negate(Complex<T> value)
    {
        return -value;
    }

    /// <summary>
    /// Adds two complex numbers.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The first complex number to add.</param>
    /// <param name="right">The second complex number to add.</param>
    /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
    public static Complex<T> Add(Complex<T> left, Complex<T> right)
    {
        return left + right;
    }

    /// <summary>
    /// Adds a complex number and a real number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The complex number to add.</param>
    /// <param name="right">The real number to add.</param>
    /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/> treated as a complex number with zero imaginary part.</returns>
    public static Complex<T> Add(Complex<T> left, T right)
    {
        return left + right;
    }

    /// <summary>
    /// Adds a real number and a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The real number to add.</param>
    /// <param name="right">The complex number to add.</param>
    /// <returns>The sum of <paramref name="left"/> treated as a complex number with zero imaginary part and <paramref name="right"/>.</returns>
    public static Complex<T> Add(T left, Complex<T> right)
    {
        return left + right;
    }

    /// <summary>
    /// Subtracts one complex number from another.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The complex number to subtract from.</param>
    /// <param name="right">The complex number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/>.</returns>
    public static Complex<T> Subtract(Complex<T> left, Complex<T> right)
    {
        return left - right;
    }

    /// <summary>
    /// Subtracts a real number from a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The complex number to subtract from.</param>
    /// <param name="right">The real number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> treated as a complex number with zero imaginary part from <paramref name="left"/>.</returns>
    public static Complex<T> Subtract(Complex<T> left, T right)
    {
        return left - right;
    }

    /// <summary>
    /// Subtracts a complex number from a real number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The real number to subtract from.</param>
    /// <param name="right">The complex number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/> treated as a complex number with zero imaginary part.</returns>
    public static Complex<T> Subtract(T left, Complex<T> right)
    {
        return left - right;
    }

    /// <summary>
    /// Multiplies two complex numbers.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The first complex number to multiply.</param>
    /// <param name="right">The second complex number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> and <paramref name="right"/>.</returns>
    public static Complex<T> Multiply(Complex<T> left, Complex<T> right)
    {
        return left * right;
    }

    /// <summary>
    /// Multiplies a complex number by a real number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The complex number to multiply.</param>
    /// <param name="right">The real number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> and <paramref name="right"/> treated as a complex number with zero imaginary part.</returns>
    public static Complex<T> Multiply(Complex<T> left, T right)
    {
        return left * right;
    }

    /// <summary>
    /// Multiplies a real number by a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The real number to multiply.</param>
    /// <param name="right">The complex number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> treated as a complex number with zero imaginary part and <paramref name="right"/>.</returns>
    public static Complex<T> Multiply(T left, Complex<T> right)
    {
        return left * right;
    }

    /// <summary>
    /// Divides one complex number by another.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="dividend">The complex number to divide.</param>
    /// <param name="divisor">The complex number to divide by.</param>
    /// <returns>The result of dividing <paramref name="dividend"/> by <paramref name="divisor"/>.</returns>
    public static Complex<T> Divide(Complex<T> dividend, Complex<T> divisor)
    {
        return dividend / divisor;
    }

    /// <summary>
    /// Divides a complex number by a real number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="dividend">The complex number to divide.</param>
    /// <param name="divisor">The real number to divide by.</param>
    /// <returns>The result of dividing <paramref name="dividend"/> by <paramref name="divisor"/> treated as a complex number with zero imaginary part.</returns>
    public static Complex<T> Divide(Complex<T> dividend, T divisor)
    {
        return dividend / divisor;
    }

    /// <summary>
    /// Divides a real number by a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="dividend">The real number to divide.</param>
    /// <param name="divisor">The complex number to divide by.</param>
    /// <returns>The result of dividing <paramref name="dividend"/> treated as a complex number with zero imaginary part by <paramref name="divisor"/>.</returns>
    public static Complex<T> Divide(T dividend, Complex<T> divisor)
    {
        return dividend / divisor;
    }

    /// <summary>
    /// Computes the unary negation of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to negate.</param>
    /// <returns>The complex number with negated real and imaginary parts.</returns>
    public static Complex<T> operator -(Complex<T> value)  /* Unary negation of a complex number */
    {
        return new Complex<T>(-value.Real, -value.Imaginary);
    }

    /// <summary>
    /// Adds two complex numbers.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The first complex number to add.</param>
    /// <param name="right">The second complex number to add.</param>
    /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
    public static Complex<T> operator +(Complex<T> left, Complex<T> right)
    {
        return new Complex<T>(left.Real + right.Real, left.Imaginary + right.Imaginary);
    }

    /// <summary>
    /// Adds a complex number and a real number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The complex number to add.</param>
    /// <param name="right">The real number to add.</param>
    /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/> treated as a complex number with zero imaginary part.</returns>
    public static Complex<T> operator +(Complex<T> left, T right)
    {
        return new Complex<T>(left.Real + right, left.Imaginary);
    }

    /// <summary>
    /// Adds a real number and a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The real number to add.</param>
    /// <param name="right">The complex number to add.</param>
    /// <returns>The sum of <paramref name="left"/> treated as a complex number with zero imaginary part and <paramref name="right"/>.</returns>
    public static Complex<T> operator +(T left, Complex<T> right)
    {
        return new Complex<T>(left + right.Real, right.Imaginary);
    }

    /// <summary>
    /// Subtracts one complex number from another.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The complex number to subtract from.</param>
    /// <param name="right">The complex number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/>.</returns>
    public static Complex<T> operator -(Complex<T> left, Complex<T> right)
    {
        return new Complex<T>(left.Real - right.Real, left.Imaginary - right.Imaginary);
    }

    /// <summary>
    /// Subtracts a real number from a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The complex number to subtract from.</param>
    /// <param name="right">The real number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> treated as a complex number with zero imaginary part from <paramref name="left"/>.</returns>
    public static Complex<T> operator -(Complex<T> left, T right)
    {
        return new Complex<T>(left.Real - right, left.Imaginary);
    }

    /// <summary>
    /// Subtracts a complex number from a real number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The real number to subtract from.</param>
    /// <param name="right">The complex number to subtract.</param>
    /// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/> treated as a complex number with zero imaginary part.</returns>
    public static Complex<T> operator -(T left, Complex<T> right)
    {
        return new Complex<T>(left - right.Real, -right.Imaginary);
    }

    /// <summary>
    /// Multiplies two complex numbers.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The first complex number to multiply.</param>
    /// <param name="right">The second complex number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> and <paramref name="right"/>, computed as (ac - bd) + (bc + ad)i where a and b are the real and imaginary parts of <paramref name="left"/>, and c and d are the real and imaginary parts of <paramref name="right"/>.</returns>
    public static Complex<T> operator *(Complex<T> left, Complex<T> right)
    {
        T result_realpart = left.Real * right.Real - left.Imaginary * right.Imaginary;
        T result_imaginarypart = left.Imaginary * right.Real + left.Real * right.Imaginary;
        return new Complex<T>(result_realpart, result_imaginarypart);
    }

    /// <summary>
    /// Multiplies a complex number by a real number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The complex number to multiply.</param>
    /// <param name="right">The real number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> and <paramref name="right"/> treated as a complex number with zero imaginary part.</returns>
    public static Complex<T> operator *(Complex<T> left, T right)
    {
        if (!T.IsFinite(left.Real))
        {
            if (!T.IsFinite(left.Imaginary))
            {
                return new Complex<T>(T.NaN, T.NaN);
            }

            return new Complex<T>(left.Real * right, T.NaN);
        }

        if (!T.IsFinite(left.Imaginary))
        {
            return new Complex<T>(T.NaN, left.Imaginary * right);
        }

        return new Complex<T>(left.Real * right, left.Imaginary * right);
    }

    /// <summary>
    /// Multiplies a real number by a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The real number to multiply.</param>
    /// <param name="right">The complex number to multiply.</param>
    /// <returns>The product of <paramref name="left"/> treated as a complex number with zero imaginary part and <paramref name="right"/>.</returns>
    public static Complex<T> operator *(T left, Complex<T> right)
    {
        if (!T.IsFinite(right.Real))
        {
            if (!T.IsFinite(right.Imaginary))
            {
                return new Complex<T>(T.NaN, T.NaN);
            }

            return new Complex<T>(left * right.Real, T.NaN);
        }

        if (!T.IsFinite(right.Imaginary))
        {
            return new Complex<T>(T.NaN, left * right.Imaginary);
        }

        return new Complex<T>(left * right.Real, left * right.Imaginary);
    }

    /// <summary>
    /// Divides one complex number by another using Smith's formula.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The complex number to divide.</param>
    /// <param name="right">The complex number to divide by.</param>
    /// <returns>The result of dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
    public static Complex<T> operator /(Complex<T> left, Complex<T> right)
    {
        // Division : Smith's formula.
        T a = left.Real;
        T b = left.Imaginary;
        T c = right.Real;
        T d = right.Imaginary;

        // Computing c * c + d * d will overflow even in cases where the actual result of the division does not overflow.
        if (T.Abs(d) < T.Abs(c))
        {
            T doc = d / c;
            return new Complex<T>((a + b * doc) / (c + d * doc), (b - a * doc) / (c + d * doc));
        }
        else
        {
            T cod = c / d;
            return new Complex<T>((b + a * cod) / (d + c * cod), (-a + b * cod) / (d + c * cod));
        }
    }

    /// <summary>
    /// Divides a complex number by a real number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The complex number to divide.</param>
    /// <param name="right">The real number to divide by.</param>
    /// <returns>The result of dividing <paramref name="left"/> by <paramref name="right"/> treated as a complex number with zero imaginary part.</returns>
    public static Complex<T> operator /(Complex<T> left, T right)
    {
        // IEEE prohibit optimizations which are value changing
        // so we make sure that behaviour for the simplified version exactly match
        // full version.
        if (right == T.Zero)
        {
            return new Complex<T>(T.NaN, T.NaN);
        }

        if (!T.IsFinite(left.Real))
        {
            if (!T.IsFinite(left.Imaginary))
            {
                return new Complex<T>(T.NaN, T.NaN);
            }

            return new Complex<T>(left.Real / right, T.NaN);
        }

        if (!T.IsFinite(left.Imaginary))
        {
            return new Complex<T>(T.NaN, left.Imaginary / right);
        }

        // Here the actual optimized version of code.
        return new Complex<T>(left.Real / right, left.Imaginary / right);
    }

    /// <summary>
    /// Divides a real number by a complex number using Smith's formula.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The real number to divide.</param>
    /// <param name="right">The complex number to divide by.</param>
    /// <returns>The result of dividing <paramref name="left"/> treated as a complex number with zero imaginary part by <paramref name="right"/>.</returns>
    public static Complex<T> operator /(T left, Complex<T> right)
    {
        // Division : Smith's formula.
        T a = left;
        T c = right.Real;
        T d = right.Imaginary;

        // Computing c * c + d * d will overflow even in cases where the actual result of the division does not overflow.
        if (T.Abs(d) < T.Abs(c))
        {
            T doc = d / c;
            return new Complex<T>(a / (c + d * doc), -a * doc / (c + d * doc));
        }
        else
        {
            T cod = c / d;
            return new Complex<T>(a * cod / (d + c * cod), -a / (d + c * cod));
        }
    }

    /// <summary>
    /// Computes the absolute value (or magnitude) of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the absolute value of.</param>
    /// <returns>The magnitude of <paramref name="value"/>, computed as sqrt(real^2 + imaginary^2) using the specified numeric type.</returns>
    public static T Abs(Complex<T> value)
    {
        return Hypot(value.Real, value.Imaginary);
    }

    private static T Hypot(T a, T b)
    {
        // Using
        //   sqrt(a^2 + b^2) = |a| * sqrt(1 + (b/a)^2)
        // we can factor out the larger component to dodge overflow even when a * a would overflow.

        a = T.Abs(a);
        b = T.Abs(b);

        T small, large;
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

        if (small == T.Zero)
        {
            return large;
        }
        else if (T.IsPositiveInfinity(large) && !T.IsNaN(small))
        {
            // The NaN test is necessary so we don't return +inf when small=NaN and large=+inf.
            // NaN in any other place returns NaN without any special handling.
            return T.PositiveInfinity;
        }
        else
        {
            T ratio = small / large;
            return large * T.Sqrt(T.One + ratio * ratio);
        }

    }

    private static T Log1P(T x)
    {
        // Compute log(1 + x) without loss of accuracy when x is small.

        // Our only use case so far is for positive values, so this isn't coded to handle negative values.
        Debug.Assert(x >= T.Zero || T.IsNaN(x));

        T xp1 = T.One + x;
        if (xp1 == T.One)
        {
            return x;
        }
        else if (x < T.CreateChecked(0.75))
        {
            // This is accurate to within 5 ulp with any floating-point system that uses a guard digit,
            // as proven in Theorem 4 of "What Every Computer Scientist Should Know About Floating-Point
            // Arithmetic" (https://docs.oracle.com/cd/E19957-01/806-3568/ncg_goldberg.html)
            return x * T.Log(xp1) / (xp1 - T.One);
        }
        else
        {
            return T.Log(xp1);
        }

    }
    /// <summary>
    /// Returns the complex conjugate of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to conjugate.</param>
    /// <returns>The complex conjugate of <paramref name="value"/>, where the real part remains unchanged and the imaginary part is negated.</returns>
    public static Complex<T> Conjugate(Complex<T> value)
    {
        // Conjugate of a Complex number: the conjugate of x+i*y is x-i*y
        return new Complex<T>(value.Real, -value.Imaginary);
    }

    /// <summary>
    /// Returns the reciprocal of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to find the reciprocal of.</param>
    /// <returns>The reciprocal of <paramref name="value"/>, computed as 1 divided by <paramref name="value"/>, or <see cref="Zero"/> if <paramref name="value"/> is zero.</returns>
    public static Complex<T> Reciprocal(Complex<T> value)
    {
        // Reciprocal of a Complex number : the reciprocal of x+i*y is 1/(x+i*y)
        if (value.Real == T.Zero && value.Imaginary == T.Zero)
        {
            return Zero;
        }
        return One / value;
    }

    /// <summary>
    /// Determines whether two complex numbers are equal.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The first complex number to compare.</param>
    /// <param name="right">The second complex number to compare.</param>
    /// <returns><see langword="true"/> if the real and imaginary parts of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Complex<T> left, Complex<T> right)
    {
        return left.Real == right.Real && left.Imaginary == right.Imaginary;
    }

    /// <summary>
    /// Determines whether two complex numbers are not equal.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="left">The first complex number to compare.</param>
    /// <param name="right">The second complex number to compare.</param>
    /// <returns><see langword="true"/> if the real or imaginary parts of <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Complex<T> left, Complex<T> right)
    {
        return left.Real != right.Real || left.Imaginary != right.Imaginary;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current complex number.
    /// </summary>
    /// <param name="obj">The object to compare with the current complex number.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Complex{T}"/> and its real and imaginary parts are equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Complex<T> other && Equals(other);
    }

    /// <summary>
    /// Determines whether the specified complex number is equal to the current complex number.
    /// </summary>
    /// <param name="value">The complex number to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the real and imaginary parts of <paramref name="value"/> are equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Complex<T> value)
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
    /// <returns>A string in the format "<real; imaginary>".</returns>
    public override string ToString() => $"<{Real}; {Imaginary}>";

    /// <summary>
    /// Returns a string representation of the complex number with the specified format for its real and imaginary parts.
    /// </summary>
    /// <param name="format">The format string for the real and imaginary parts.</param>
    /// <returns>A string in the format "<real; imaginary>" with the specified format applied to the real and imaginary parts.</returns>
    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format) => ToString(format, null);

    /// <summary>
    /// Returns a string representation of the complex number using the specified format provider.
    /// </summary>
    /// <param name="provider">The format provider to use for formatting the real and imaginary parts.</param>
    /// <returns>A string in the format "<real; imaginary>" using the specified format provider.</returns>
    public string ToString(IFormatProvider? provider) => ToString(null, provider);

    /// <summary>
    /// Returns a string representation of the complex number with the specified format and format provider.
    /// </summary>
    /// <param name="format">The format string for the real and imaginary parts.</param>
    /// <param name="provider">The format provider to use for formatting the real and imaginary parts.</param>
    /// <returns>A string in the format "<real; imaginary>" with the specified format and format provider applied to the real and imaginary parts.</returns>
    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? provider)
    {
        return string.Format(provider, "<{0}; {1}>", Real.ToString(format, provider), Imaginary.ToString(format, provider));
    }

    /// <summary>
    /// Computes the sine of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the sine of.</param>
    /// <returns>The sine of <paramref name="value"/>, computed as sin(x)cosh(y) + i cos(x)sinh(y), where x is the real part and y is the imaginary part.</returns>
    /// <remarks>
    /// There is a known limitation with this algorithm: inputs that cause sinh and cosh to overflow, but for
    /// which sin or cos are small enough that sin * cosh or cos * sinh are still representable, nonetheless
    /// produce overflow. For example, Sin((0.01, 711.0)) should produce (~3.0E306, PositiveInfinity), but
    /// instead produces (PositiveInfinity, PositiveInfinity).
    /// </remarks>
    public static Complex<T> Sin(Complex<T> value)
    {
        // We need both sinh and cosh of imaginary part. To avoid multiple calls to Math.Exp with the same value,
        // we compute them both here from a single call to Math.Exp.
        T p = T.Exp(value.Imaginary);
        T q = T.One / p;
        T sinh = (p - q) * T.CreateChecked(0.5);
        T cosh = (p + q) * T.CreateChecked(0.5);
        return new Complex<T>(T.Sin(value.Real) * cosh, T.Cos(value.Real) * sinh);
        // There is a known limitation with this algorithm: inputs that cause sinh and cosh to overflow, but for
        // which sin or cos are small enough that sin * cosh or cos * sinh are still representable, nonetheless
        // produce overflow. For example, Sin((0.01, 711.0)) should produce (~3.0E306, PositiveInfinity), but
        // instead produces (PositiveInfinity, PositiveInfinity).
    }

    /// <summary>
    /// Computes the hyperbolic sine of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the hyperbolic sine of.</param>
    /// <returns>The hyperbolic sine of <paramref name="value"/>, computed via the relation sinh(z) = -i sin(iz).</returns>
    public static Complex<T> Sinh(Complex<T> value)
    {
        // Use sinh(z) = -i sin(iz) to compute via sin(z).
        Complex<T> sin = Sin(new Complex<T>(-value.Imaginary, value.Real));
        return new Complex<T>(sin.Imaginary, -sin.Real);
    }

    /// <summary>
    /// Computes the arcsine of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the arcsine of.</param>
    /// <returns>The arcsine of <paramref name="value"/>.</returns>
    public static Complex<T> Asin(Complex<T> value)
    {
        T b, bPrime, v;
        Asin_Internal(T.Abs(value.Real), T.Abs(value.Imaginary), out b, out bPrime, out v);

        T u;
        if (bPrime < T.Zero)
        {
            u = T.Asin(b);
        }
        else
        {
            u = T.Atan(bPrime);
        }

        if (value.Real < T.Zero) u = -u;
        if (value.Imaginary < T.Zero) v = -v;

        return new Complex<T>(u, v);
    }

    /// <summary>
    /// Computes the cosine of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the cosine of.</param>
    /// <returns>The cosine of <paramref name="value"/>, computed as cos(x)cosh(y) - i sin(x)sinh(y), where x is the real part and y is the imaginary part.</returns>
    public static Complex<T> Cos(Complex<T> value)
    {
        T p = T.Exp(value.Imaginary);
        T q = T.One / p;
        T sinh = (p - q) * T.CreateChecked(0.5);
        T cosh = (p + q) * T.CreateChecked(0.5);
        return new Complex<T>(T.Cos(value.Real) * cosh, -T.Sin(value.Real) * sinh);
    }

    /// <summary>
    /// Computes the hyperbolic cosine of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the hyperbolic cosine of.</param>
    /// <returns>The hyperbolic cosine of <paramref name="value"/>, computed via the relation cosh(z) = cos(iz).</returns>
    public static Complex<T> Cosh(Complex<T> value)
    {
        // Use cosh(z) = cos(iz) to compute via cos(z).
        return Cos(new Complex<T>(-value.Imaginary, value.Real));
    }

    /// <summary>
    /// Computes the arccosine of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the arccosine of.</param>
    /// <returns>The arccosine of <paramref name="value"/>.</returns>
    public static Complex<T> Acos(Complex<T> value)
    {
        T b, bPrime, v;
        Asin_Internal(T.Abs(value.Real), T.Abs(value.Imaginary), out b, out bPrime, out v);

        T u;
        if (bPrime < T.One)
        {
            u = T.Acos(b);
        }
        else
        {
            u = T.Atan(T.One / bPrime);
        }

        if (value.Real < T.Zero) u = T.CreateChecked(Math.PI) - u;
        if (value.Imaginary > T.Zero) v = -v;

        return new Complex<T>(u, v);
    }

    /// <summary>
    /// Computes the tangent of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the tangent of.</param>
    /// <returns>The tangent of <paramref name="value"/>, computed as (sin(2x) + i sinh(2y)) / (cos(2x) + cosh(2y)) for small imaginary parts, or an equivalent form to avoid overflow.</returns>
    /// <remarks>
    /// This approach does not work for |y| > ~355, because sinh(2y) and cosh(2y) overflow,
    /// even though their ratio does not. In that case, the method divides through by cosh to compute
    /// tan z = (sin(2x) / cosh(2y) + i tanh(2y)) / (1 + cos(2x) / cosh(2y)).
    /// </remarks>
    public static Complex<T> Tan(Complex<T> value)
    {
        // tan z = sin z / cos z, but to avoid unnecessary repeated trig computations, use
        //   tan z = (sin(2x) + i sinh(2y)) / (cos(2x) + cosh(2y))
        // (see Abramowitz & Stegun 4.3.57 or derive by hand), and compute trig functions here.

        // This approach does not work for |y| > ~355, because sinh(2y) and cosh(2y) overflow,
        // even though their ratio does not. In that case, divide through by cosh to get:
        //   tan z = (sin(2x) / cosh(2y) + i \tanh(2y)) / (1 + cos(2x) / cosh(2y))
        // which correctly computes the (tiny) real part and the (normal-sized) imaginary part.

        T x2 = T.CreateChecked(2.0) * value.Real;
        T y2 = T.CreateChecked(2.0) * value.Imaginary;
        T p = T.Exp(y2);
        T q = T.One / p;
        T cosh = (p + q) * T.CreateChecked(0.5);
        if (T.Abs(value.Imaginary) <= T.CreateChecked(4.0))
        {
            T sinh = (p - q) * T.CreateChecked(0.5);
            T D = T.Cos(x2) + cosh;
            return new Complex<T>(T.Sin(x2) / D, sinh / D);
        }
        else
        {
            T D = T.One + T.Cos(x2) / cosh;
            return new Complex<T>(T.Sin(x2) / cosh / D, T.Tanh(y2) / D);
        }
    }

    /// <summary>
    /// Computes the hyperbolic tangent of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the hyperbolic tangent of.</param>
    /// <returns>The hyperbolic tangent of <paramref name="value"/>, computed via the relation tanh(z) = -i tan(iz).</returns>
    public static Complex<T> Tanh(Complex<T> value)
    {
        // Use tanh(z) = -i tan(iz) to compute via tan(z).
        Complex<T> tan = Tan(new Complex<T>(-value.Imaginary, value.Real));
        return new Complex<T>(tan.Imaginary, -tan.Real);
    }

    /// <summary>
    /// Computes the arctangent of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the arctangent of.</param>
    /// <returns>The arctangent of <paramref name="value"/>, computed as (i/2) * (log(1 - iz) - log(1 + iz)).</returns>
    public static Complex<T> Atan(Complex<T> value)
    {
        Complex<T> two = new Complex<T>(T.CreateChecked(2), T.Zero);
        return ImaginaryOne / two * (Log(One - ImaginaryOne * value) - Log(One + ImaginaryOne * value));
    }

    private static void Asin_Internal(T x, T y, out T b, out T bPrime, out T v)
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

        Debug.Assert(x >= T.Zero || T.IsNaN(x));
        Debug.Assert(y >= T.Zero || T.IsNaN(y));

        // For x or y large enough to overflow alpha^2, we can simplify our formulas and avoid overflow.
        if (x > s_asinOverflowThreshold || y > s_asinOverflowThreshold)
        {
            b = T.NegativeOne;
            bPrime = x / y;

            T small, big;
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
            T ratio = small / big;
            v = s_log2 + T.Log(big) + T.CreateChecked(0.5) * Log1P(ratio * ratio);
        }
        else
        {
            T r = Hypot(x + T.One, y);
            T s = Hypot(x - T.One, y);

            T a = (r + s) * T.CreateChecked(0.5);
            b = x / a;

            if (b > T.CreateChecked(0.75))
            {
                if (x <= T.One)
                {
                    T amx = (y * y / (r + (x + T.One)) + (s + (T.One - x))) * T.CreateChecked(0.5);
                    bPrime = x / T.Sqrt((a + x) * amx);
                }
                else
                {
                    // In this case, amx ~ y^2. Since we take the square root of amx, we should
                    // pull y out from under the square root so we don't lose its contribution
                    // when y^2 underflows.
                    T t = (T.One / (r + (x + T.One)) + T.One / (s + (x - T.One))) * T.CreateChecked(0.5);
                    bPrime = x / y / T.Sqrt((a + x) * t);
                }
            }
            else
            {
                bPrime = T.NegativeOne;
            }

            if (a < T.CreateChecked(1.5))
            {
                if (x < T.One)
                {
                    // This is another case where our expression is proportional to y^2 and
                    // we take its square root, so again we pull out a factor of y from
                    // under the square root.
                    T t = (T.One / (r + (x + T.One)) + T.One / (s + (T.One - x))) * T.CreateChecked(0.5);
                    T am1 = y * y * t;
                    v = Log1P(am1 + y * T.Sqrt(t * (a + T.One)));
                }
                else
                {
                    T am1 = (y * y / (r + (x + T.One)) + (s + (x - T.One))) * T.CreateChecked(0.5);
                    v = Log1P(am1 + T.Sqrt(am1 * (a + T.One)));
                }
            }
            else
            {
                // Because of the test above, we can be sure that a * a will not overflow.
                v = T.Log(a + T.Sqrt((a - T.One) * (a + T.One)));
            }
        }
    }
    /// <summary>
    /// Determines whether a complex number is finite.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to check.</param>
    /// <returns><see langword="true"/> if both the real and imaginary parts of <paramref name="value"/> are finite; otherwise, <see langword="false"/>.</returns>
    public static bool IsFinite(Complex<T> value) => T.IsFinite(value.Real) && T.IsFinite(value.Imaginary);

    /// <summary>
    /// Determines whether a complex number is infinite.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to check.</param>
    /// <returns><see langword="true"/> if either the real or imaginary part of <paramref name="value"/> is infinite; otherwise, <see langword="false"/>.</returns>
    public static bool IsInfinity(Complex<T> value) => T.IsInfinity(value.Real) || T.IsInfinity(value.Imaginary);

    /// <summary>
    /// Determines whether a complex number is NaN (Not a Number).
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to check.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is neither finite nor infinite (i.e., NaN); otherwise, <see langword="false"/>.</returns>
    public static bool IsNaN(Complex<T> value) => !IsInfinity(value) && !IsFinite(value);

    /// <summary>
    /// Computes the natural logarithm of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the logarithm of.</param>
    /// <returns>The natural logarithm of <paramref name="value"/>, computed as ln(|z|) + i*arg(z), where |z| is the magnitude and arg(z) is the phase.</returns>
    public static Complex<T> Log(Complex<T> value)
    {
        return new Complex<T>(T.Log(Abs(value)), T.Atan2(value.Imaginary, value.Real));
    }

    /// <summary>
    /// Computes the logarithm of a complex number with a specified base.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the logarithm of.</param>
    /// <param name="baseValue">The base of the logarithm.</param>
    /// <returns>The logarithm of <paramref name="value"/> with base <paramref name="baseValue"/>, computed as log(value)/log(baseValue).</returns>
    public static Complex<T> Log(Complex<T> value, T baseValue)
    {
        return Log(value) / Log(baseValue);
    }

    /// <summary>
    /// Computes the base-10 logarithm of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the base-10 logarithm of.</param>
    /// <returns>The base-10 logarithm of <paramref name="value"/>, computed as log(value)/log(10).</returns>
    public static Complex<T> Log10(Complex<T> value)
    {
        Complex<T> tempLog = Log(value);
        return Scale(tempLog, InverseOfLog10);
    }

    /// <summary>
    /// Computes the exponential of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the exponential of.</param>
    /// <returns>The exponential of <paramref name="value"/>, computed as e^x * (cos(y) + i*sin(y)), where x is the real part and y is the imaginary part.</returns>
    public static Complex<T> Exp(Complex<T> value)
    {
        T expReal = T.Exp(value.Real);
        T cosImaginary = expReal * T.Cos(value.Imaginary);
        T sinImaginary = expReal * T.Sin(value.Imaginary);
        return new Complex<T>(cosImaginary, sinImaginary);
    }

    /// <summary>
    /// Computes the square root of a complex number.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to compute the square root of.</param>
    /// <returns>The principal square root of <paramref name="value"/> with non-negative real part.</returns>
    /// <remarks>
    /// If the components are too large, Hypot will overflow, even though the subsequent sqrt would
    /// make the result representable. To avoid this, we re-scale (by exact powers of 2 for accuracy)
    /// when we encounter very large components to avoid intermediate infinities.
    /// </remarks>
    public static Complex<T> Sqrt(Complex<T> value)
    {
        if (value.Imaginary == T.Zero)
        {
            // Handle the trivial case quickly.
            if (value.Real < T.Zero)
            {
                return new Complex<T>(T.Zero, T.Sqrt(-value.Real));
            }

            return new Complex<T>(T.Sqrt(value.Real), T.Zero);
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
        T realCopy = value.Real;
        T imaginaryCopy = value.Imaginary;
        if (T.Abs(realCopy) >= s_sqrtRescaleThreshold || T.Abs(imaginaryCopy) >= s_sqrtRescaleThreshold)
        {
            if (T.IsInfinity(value.Imaginary) && !T.IsNaN(value.Real))
            {
                // We need to handle infinite imaginary parts specially because otherwise
                // our formulas below produce inf/inf = NaN. The NaN test is necessary
                // so that we return NaN rather than (+inf,inf) for (NaN,inf).
                return new Complex<T>(T.PositiveInfinity, imaginaryCopy);
            }

            realCopy *= T.CreateChecked(0.25);
            imaginaryCopy *= T.CreateChecked(0.25);
            rescale = true;
        }

        // This is the core of the algorithm. Everything else is special case handling.
        T x, y;
        if (realCopy >= T.Zero)
        {
            x = T.Sqrt((Hypot(realCopy, imaginaryCopy) + realCopy) * T.CreateChecked(0.5));
            y = imaginaryCopy / (T.CreateChecked(2.0) * x);
        }
        else
        {
            y = T.Sqrt((Hypot(realCopy, imaginaryCopy) - realCopy) * T.CreateChecked(0.5));
            if (imaginaryCopy < T.Zero) y = -y;
            x = imaginaryCopy / (T.CreateChecked(2.0) * y);
        }

        if (rescale)
        {
            x *= T.CreateChecked(2.0);
            y *= T.CreateChecked(2.0);
        }

        return new Complex<T>(x, y);
    }

    /// <summary>
    /// Computes a complex number raised to a complex power.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to raise to the power.</param>
    /// <param name="power">The complex power to raise the number to.</param>
    /// <returns>The result of raising <paramref name="value"/> to the power of <paramref name="power"/>.</returns>
    public static Complex<T> Pow(Complex<T> value, Complex<T> power)
    {
        if (power == Zero)
        {
            return One;
        }

        if (value == Zero)
        {
            return Zero;
        }

        T valueReal = value.Real;
        T valueImaginary = value.Imaginary;
        T powerReal = power.Real;
        T powerImaginary = power.Imaginary;

        T rho = Abs(value);
        T theta = T.Atan2(valueImaginary, valueReal);
        T newRho = powerReal * theta + powerImaginary * T.Log(rho);

        T t = T.Pow(rho, powerReal) * T.Pow(T.E, -powerImaginary * theta);

        return new Complex<T>(t * T.Cos(newRho), t * T.Sin(newRho));
    }

    /// <summary>
    /// Computes a complex number raised to a real power.
    /// </summary>
    /// <typeparam name="T">The numeric type supporting floating-point operations.</typeparam>
    /// <param name="value">The complex number to raise to the power.</param>
    /// <param name="power">The real power to raise the number to.</param>
    /// <returns>The result of raising <paramref name="value"/> to the power of <paramref name="power"/>.</returns>
    public static Complex<T> Pow(Complex<T> value, T power)
    {
        return Pow(value, new Complex<T>(power, T.Zero));
    }

    private static Complex<T> Scale(Complex<T> value, T factor)
    {
        T realResult = factor * value.Real;
        T imaginaryResuilt = factor * value.Imaginary;
        return new Complex<T>(realResult, imaginaryResuilt);
    }

    //
    // Explicit Conversions To Complex
    //

    public static explicit operator Complex<T>(decimal value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    /// <summary>Explicitly converts a <see cref="Int128" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    public static explicit operator Complex<T>(Int128 value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    public static explicit operator Complex<T>(BigInteger value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    /// <summary>Explicitly converts a <see cref="UInt128" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    [CLSCompliant(false)]
    public static explicit operator Complex<T>(UInt128 value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    //
    // Implicit Conversions To Complex
    //

    public static implicit operator Complex<T>(T value)
    {
        return new Complex<T>(value, T.Zero);
    }

    public static implicit operator Complex<T>(byte value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    /// <summary>Implicitly converts a <see cref="char" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    public static implicit operator Complex<T>(char value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    public static implicit operator Complex<T>(double value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    /// <summary>Implicitly converts a <see cref="Half" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    public static implicit operator Complex<T>(Half value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    public static implicit operator Complex<T>(short value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    public static implicit operator Complex<T>(int value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    public static implicit operator Complex<T>(long value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    /// <summary>Implicitly converts a <see cref="nint" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    public static implicit operator Complex<T>(nint value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    [CLSCompliant(false)]
    public static implicit operator Complex<T>(sbyte value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    public static implicit operator Complex<T>(float value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    [CLSCompliant(false)]
    public static implicit operator Complex<T>(ushort value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    [CLSCompliant(false)]
    public static implicit operator Complex<T>(uint value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    [CLSCompliant(false)]
    public static implicit operator Complex<T>(ulong value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    /// <summary>Implicitly converts a <see cref="nuint" /> value to a double-precision complex number.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a double-precision complex number.</returns>
    [CLSCompliant(false)]
    public static implicit operator Complex<T>(nuint value)
    {
        return new Complex<T>(T.CreateChecked(value), T.Zero);
    }

    //
    // IAdditiveIdentity
    //

    /// <inheritdoc cref="IAdditiveIdentity{TSelf, TResult}.AdditiveIdentity" />
    static Complex<T> IAdditiveIdentity<Complex<T>, Complex<T>>.AdditiveIdentity => new Complex<T>(T.Zero, T.Zero);

    //
    // IDecrementOperators
    //

    /// <inheritdoc cref="IDecrementOperators{TSelf}.op_Decrement(TSelf)" />
    public static Complex<T> operator --(Complex<T> value) => value - One;

    //
    // IIncrementOperators
    //

    /// <inheritdoc cref="IIncrementOperators{TSelf}.op_Increment(TSelf)" />
    public static Complex<T> operator ++(Complex<T> value) => value + One;

    //
    // IMultiplicativeIdentity
    //

    /// <inheritdoc cref="IMultiplicativeIdentity{TSelf, TResult}.MultiplicativeIdentity" />
    static Complex<T> IMultiplicativeIdentity<Complex<T>, Complex<T>>.MultiplicativeIdentity => new Complex<T>(T.One, T.Zero);

    //
    // INumberBase
    //

    /// <inheritdoc cref="INumberBase{TSelf}.One" />
    static Complex<T> INumberBase<Complex<T>>.One => new Complex<T>(T.One, T.Zero);

    /// <inheritdoc cref="INumberBase{TSelf}.Radix" />
    static int INumberBase<Complex<T>>.Radix => 2;

    /// <inheritdoc cref="INumberBase{TSelf}.Zero" />
    static Complex<T> INumberBase<Complex<T>>.Zero => new Complex<T>(T.Zero, T.Zero);

    /// <inheritdoc cref="INumberBase{TSelf}.Abs(TSelf)" />
    static Complex<T> INumberBase<Complex<T>>.Abs(Complex<T> value) => Abs(value);

    /// <inheritdoc cref="INumberBase{TSelf}.CreateChecked{TOther}(TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex<T> CreateChecked<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        Complex<T> result;

        if (typeof(TOther) == typeof(Complex<T>))
        {
            result = (Complex<T>)(object)value;
        }
        else if (!TryConvertFrom(value, out result) && !TOther.TryConvertToChecked(value, out result))
        {
            throw new NotSupportedException();
        }

        return result;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.CreateSaturating{TOther}(TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex<T> CreateSaturating<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        Complex<T> result;

        if (typeof(TOther) == typeof(Complex<T>))
        {
            result = (Complex<T>)(object)value;
        }
        else if (!TryConvertFrom(value, out result) && !TOther.TryConvertToSaturating(value, out result))
        {
            throw new NotSupportedException();
        }

        return result;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.CreateTruncating{TOther}(TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex<T> CreateTruncating<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        Complex<T> result;

        if (typeof(TOther) == typeof(Complex<T>))
        {
            result = (Complex<T>)(object)value;
        }
        else if (!TryConvertFrom(value, out result) && !TOther.TryConvertToTruncating(value, out result))
        {
            throw new NotSupportedException();
        }

        return result;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsCanonical(TSelf)" />
    static bool INumberBase<Complex<T>>.IsCanonical(Complex<T> value) => true;

    /// <inheritdoc cref="INumberBase{TSelf}.IsComplexNumber(TSelf)" />
    public static bool IsComplexNumber(Complex<T> value) => value.Real != T.Zero && value.Imaginary != T.Zero;

    /// <inheritdoc cref="INumberBase{TSelf}.IsEvenInteger(TSelf)" />
    public static bool IsEvenInteger(Complex<T> value) => value.Imaginary == T.Zero && T.IsEvenInteger(value.Real);

    /// <inheritdoc cref="INumberBase{TSelf}.IsImaginaryNumber(TSelf)" />
    public static bool IsImaginaryNumber(Complex<T> value) => value.Real == T.Zero && T.IsRealNumber(value.Imaginary);

    /// <inheritdoc cref="INumberBase{TSelf}.IsInteger(TSelf)" />
    public static bool IsInteger(Complex<T> value) => value.Imaginary == T.Zero && T.IsInteger(value.Real);

    /// <inheritdoc cref="INumberBase{TSelf}.IsNegative(TSelf)" />
    public static bool IsNegative(Complex<T> value)
    {
        // since complex numbers do not have a well-defined concept of
        // negative we report false if this value has an imaginary part

        return value.Imaginary == T.Zero && T.IsNegative(value.Real);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsNegativeInfinity(TSelf)" />
    public static bool IsNegativeInfinity(Complex<T> value)
    {
        // since complex numbers do not have a well-defined concept of
        // negative we report false if this value has an imaginary part

        return value.Imaginary == T.Zero && T.IsNegativeInfinity(value.Real);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsNormal(TSelf)" />
    public static bool IsNormal(Complex<T> value)
    {
        // much as IsFinite requires both part to be finite, we require both
        // part to be "normal" (finite, non-zero, and non-subnormal) to be true

        return T.IsNormal(value.Real)
            && (value.Imaginary == T.Zero || T.IsNormal(value.Imaginary));
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsOddInteger(TSelf)" />
    public static bool IsOddInteger(Complex<T> value) => value.Imaginary == T.Zero && T.IsOddInteger(value.Real);

    /// <inheritdoc cref="INumberBase{TSelf}.IsPositive(TSelf)" />
    public static bool IsPositive(Complex<T> value)
    {
        // since complex numbers do not have a well-defined concept of
        // negative we report false if this value has an imaginary part

        return value.Imaginary == T.Zero && T.IsPositive(value.Real);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsPositiveInfinity(TSelf)" />
    public static bool IsPositiveInfinity(Complex<T> value)
    {
        // since complex numbers do not have a well-defined concept of
        // positive we report false if this value has an imaginary part

        return value.Imaginary == T.Zero && T.IsPositiveInfinity(value.Real);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsRealNumber(TSelf)" />
    public static bool IsRealNumber(Complex<T> value) => value.Imaginary == T.Zero && T.IsRealNumber(value.Real);

    /// <inheritdoc cref="INumberBase{TSelf}.IsSubnormal(TSelf)" />
    public static bool IsSubnormal(Complex<T> value)
    {
        // much as IsInfinite allows either part to be infinite, we allow either
        // part to be "subnormal" (finite, non-zero, and non-normal) to be true

        return T.IsSubnormal(value.Real) || T.IsSubnormal(value.Imaginary);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsZero(TSelf)" />
    static bool INumberBase<Complex<T>>.IsZero(Complex<T> value) => value.Real == T.Zero && value.Imaginary == T.Zero;

    /// <inheritdoc cref="INumberBase{TSelf}.MaxMagnitude(TSelf, TSelf)" />
    public static Complex<T> MaxMagnitude(Complex<T> x, Complex<T> y)
    {
        // complex numbers are not normally comparable, however every complex
        // number has a real magnitude (absolute value) and so we can provide
        // an implementation for MaxMagnitude

        // This matches the IEEE 754:2019 `maximumMagnitude` function
        //
        // It propagates NaN inputs back to the caller and
        // otherwise returns the input with a larger magnitude.
        // It treats +0 as larger than -0 as per the specification.

        T ax = Abs(x);
        T ay = Abs(y);

        if (ax > ay || T.IsNaN(ax))
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

            if (T.IsNegative(y.Real))
            {
                if (T.IsNegative(y.Imaginary))
                {
                    // when `y` is `-a - ib` we always prefer `x` (its either the same as
                    // `x` or some part of `x` is positive).

                    return x;
                }
                else
                {
                    if (T.IsNegative(x.Real))
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
            else if (T.IsNegative(y.Imaginary))
            {
                if (T.IsNegative(x.Real))
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
    static Complex<T> INumberBase<Complex<T>>.MaxMagnitudeNumber(Complex<T> x, Complex<T> y)
    {
        // complex numbers are not normally comparable, however every complex
        // number has a real magnitude (absolute value) and so we can provide
        // an implementation for MaxMagnitudeNumber

        // This matches the IEEE 754:2019 `maximumMagnitudeNumber` function
        //
        // It does not propagate NaN inputs back to the caller and
        // otherwise returns the input with a larger magnitude.
        // It treats +0 as larger than -0 as per the specification.

        T ax = Abs(x);
        T ay = Abs(y);

        if (ax > ay || T.IsNaN(ay))
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

            if (T.IsNegative(y.Real))
            {
                if (T.IsNegative(y.Imaginary))
                {
                    // when `y` is `-a - ib` we always prefer `x` (its either the same as
                    // `x` or some part of `x` is positive).

                    return x;
                }
                else
                {
                    if (T.IsNegative(x.Real))
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
            else if (T.IsNegative(y.Imaginary))
            {
                if (T.IsNegative(x.Real))
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
    public static Complex<T> MinMagnitude(Complex<T> x, Complex<T> y)
    {
        // complex numbers are not normally comparable, however every complex
        // number has a real magnitude (absolute value) and so we can provide
        // an implementation for MaxMagnitude

        // This matches the IEEE 754:2019 `minimumMagnitude` function
        //
        // It propagates NaN inputs back to the caller and
        // otherwise returns the input with a smaller magnitude.
        // It treats -0 as smaller than +0 as per the specification.

        T ax = Abs(x);
        T ay = Abs(y);

        if (ax < ay || T.IsNaN(ax))
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

            if (T.IsNegative(y.Real))
            {
                if (T.IsNegative(y.Imaginary))
                {
                    // when `y` is `-a - ib` we always prefer `y` as both parts are negative
                    return y;
                }
                else
                {
                    if (T.IsNegative(x.Real))
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
            else if (T.IsNegative(y.Imaginary))
            {
                if (T.IsNegative(x.Real))
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
    static Complex<T> INumberBase<Complex<T>>.MinMagnitudeNumber(Complex<T> x, Complex<T> y)
    {
        // complex numbers are not normally comparable, however every complex
        // number has a real magnitude (absolute value) and so we can provide
        // an implementation for MinMagnitudeNumber

        // This matches the IEEE 754:2019 `minimumMagnitudeNumber` function
        //
        // It does not propagate NaN inputs back to the caller and
        // otherwise returns the input with a smaller magnitude.
        // It treats -0 as smaller than +0 as per the specification.

        T ax = Abs(x);
        T ay = Abs(y);

        if (ax < ay || T.IsNaN(ay))
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

            if (T.IsNegative(y.Real))
            {
                if (T.IsNegative(y.Imaginary))
                {
                    // when `y` is `-a - ib` we always prefer `y` as both parts are negative
                    return y;
                }
                else
                {
                    if (T.IsNegative(x.Real))
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
            else if (T.IsNegative(y.Imaginary))
            {
                if (T.IsNegative(x.Real))
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
    public static Complex<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        if (!TryParse(s, style, provider, out Complex<T> result))
        {
            throw new OverflowException();
        }
        return result;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.Parse(string, NumberStyles, IFormatProvider?)" />
    public static Complex<T> Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), style, provider);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromChecked{TOther}(TOther, out TSelf)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Complex<T>>.TryConvertFromChecked<TOther>(TOther value, out Complex<T> result)
    {
        return TryConvertFrom(value, out result);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromSaturating{TOther}(TOther, out TSelf)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Complex<T>>.TryConvertFromSaturating<TOther>(TOther value, out Complex<T> result)
    {
        return TryConvertFrom(value, out result);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromTruncating{TOther}(TOther, out TSelf)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Complex<T>>.TryConvertFromTruncating<TOther>(TOther value, out Complex<T> result)
    {
        return TryConvertFrom(value, out result);
    }

    private static bool TryConvertFrom<TOther>(TOther value, out Complex<T> result)
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
            result = (Complex<T>)actualValue;
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
            result = (Complex<T>)actualValue;
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
            result = (Complex<T>)actualValue;
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
    static bool INumberBase<Complex<T>>.TryConvertToChecked<TOther>(Complex<T> value, [MaybeNullWhen(false)] out TOther result)
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
    static bool INumberBase<Complex<T>>.TryConvertToSaturating<TOther>(Complex<T> value, [MaybeNullWhen(false)] out TOther result)
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
    static bool INumberBase<Complex<T>>.TryConvertToTruncating<TOther>(Complex<T> value, [MaybeNullWhen(false)] out TOther result)
    {
        // Complex numbers with an imaginary part can't be represented as a "real number"
        // so we'll only consider the real part for the purposes of truncation.
        result = TOther.CreateTruncating(value.Real);
        return true;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryParse(ReadOnlySpan{char}, NumberStyles, IFormatProvider?, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Complex<T> result)
    {
        ValidateParseStyleFloatingPoint(style);

        int openBracket = s.IndexOf('<');
        int semicolon = s.IndexOf(';');
        int closeBracket = s.IndexOf('>');

        if (s.Length < 5 || openBracket == -1 || semicolon == -1 || closeBracket == -1 || openBracket > semicolon || openBracket > closeBracket || semicolon > closeBracket)
        {
            // We need at least 5 characters for `<0;0>`
            // We also expect a to find an open bracket, a semicolon, and a closing bracket in that order

            result = default;
            return false;
        }

        if (openBracket != 0 && ((style & NumberStyles.AllowLeadingWhite) == 0 || !s.Slice(0, openBracket).IsWhiteSpace()))
        {
            // The opening bracket wasn't the first and we either didn't allow leading whitespace
            // or one of the leading characters wasn't whitespace at all.

            result = default;
            return false;
        }

        if (!T.TryParse(s.Slice(openBracket + 1, semicolon), style, provider, out T real))
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

        if (!T.TryParse(s.Slice(semicolon + 1, closeBracket - semicolon), style, provider, out T imaginary))
        {
            result = default;
            return false;
        }

        if (closeBracket != s.Length - 1 && ((style & NumberStyles.AllowTrailingWhite) == 0 || !s.Slice(closeBracket).IsWhiteSpace()))
        {
            // The closing bracket wasn't the last and we either didn't allow trailing whitespace
            // or one of the trailing characters wasn't whitespace at all.

            result = default;
            return false;
        }

        result = new Complex<T>(real, imaginary);
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
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out Complex<T> result)
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
    public static Complex<T> Parse(string s, IFormatProvider? provider) => Parse(s, DefaultNumberStyle, provider);

    /// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)" />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Complex<T> result) => TryParse(s, DefaultNumberStyle, provider, out result);

    //
    // ISignedNumber
    //

    /// <inheritdoc cref="ISignedNumber{TSelf}.NegativeOne" />
    static Complex<T> ISignedNumber<Complex<T>>.NegativeOne => new(T.NegativeOne, T.Zero);

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
        if (!tryFormatSucceeded || destination.Length < charsWrittenSoFar + 4)
        {
            charsWritten = charsWrittenSoFar;
            return false;
        }

        destination[charsWrittenSoFar++] = ';';
        destination[charsWrittenSoFar++] = ' ';

        tryFormatSucceeded = Imaginary.TryFormat(destination.Slice(charsWrittenSoFar), out tryFormatCharsWritten, format, provider);
        charsWrittenSoFar += tryFormatCharsWritten;

        // We have at least 1 more character for: >
        if (!tryFormatSucceeded || destination.Length < charsWrittenSoFar + 1)
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
    public static Complex<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, DefaultNumberStyle, provider);

    /// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Complex<T> result) => TryParse(s, DefaultNumberStyle, provider, out result);

    //
    // IUnaryPlusOperators
    //

    /// <inheritdoc cref="IUnaryPlusOperators{TSelf, TResult}.op_UnaryPlus(TSelf)" />
    public static Complex<T> operator +(Complex<T> value) => value;
}