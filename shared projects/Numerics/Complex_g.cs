namespace Vorcyc.Mathematics.Numerics;

/*
 *  23.8.25 创建 ：
 *  1. 反编译了 BCL 中的 Complex 结构改成.NET 7 支持的泛型数学版本
 *  
 *  23.9.23 RENEW 
 *  1. 修改了些许 bug
 *  2. 不再是 只读 的
 *  
 * 25.7.9
 * 1 改回只读结构体
 * 2 增加与值元祖互转
 * 3 增加从长度为2的 Span<T> 高性能转换为 ComplexF
 */

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Represents a complex number with generic floating-point real and imaginary components.
/// Implements interfaces for numerical operations, equality comparison, and formatting.
/// </summary>
/// <typeparam name="T">The floating-point type that implements <see cref="IFloatingPointIeee754{T}"/>, 
/// <see cref="IEquatable{T}"/>, <see cref="IFormattable"/>, <see cref="IComparable{T}"/>, and <see cref="IMinMaxValue{T}"/>.</typeparam>
public readonly struct Complex<T> : INumber<Complex<T>>, IEquatable<Complex<T>>, ISignedNumber<Complex<T>>, IUtf8SpanFormattable
    where T : struct, IFloatingPointIeee754<T>, IEquatable<T>, IFormattable, IComparable<T>, IMinMaxValue<T>
{

    private static readonly T s_two = T.One + T.One;

    // This is the largest x for which (Hypot(x,x) + x) will not overflow. It is used for branching inside Sqrt.
    private static readonly T s_sqrtRescaleThreshold = T.MaxValue / (T.Sqrt(s_two) + T.One);

    // This is the largest x for which 2 x^2 will not overflow. It is used for branching inside Asin and Acos.
    private static readonly T s_asinOverflowThreshold = T.Sqrt(T.MaxValue) / s_two;

    // This value is used inside Asin and Acos.
    private static readonly T s_log2 = T.Log(s_two);


    private readonly T _real;
    private readonly T _imaginary;

    /// <summary>
    /// Gets or sets the real part of the complex number.
    /// </summary>
    public T Real => _real;

    /// <summary>
    /// Gets or sets the imaginary part of the complex number.
    /// </summary>
    public T Imaginary => _imaginary;

    /// <summary>
    /// Initializes a new instance of <see cref="Complex{T}"/> with specified real and imaginary parts.
    /// </summary>
    /// <param name="real">The real part of the complex number.</param>
    /// <param name="imaginary">The imaginary part of the complex number.</param>
    public Complex(T real, T imaginary)
    {
        _real = real;
        _imaginary = imaginary;
    }

    #region 类型转换

    /// <summary>
    /// Deconstruct to <see cref="ValueTuple{T1,T2}"/>.
    /// </summary>
    /// <param name="real"></param>
    /// <param name="imaginary"></param>
    public void Deconstruct(out T real, out T imaginary)
    {
        real = _real;
        imaginary = _imaginary;
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
    /// Gets a complex number with real and imaginary parts equal to zero.
    /// </summary>
    public static Complex<T> Zero => new Complex<T>(T.Zero, T.Zero);

    /// <summary>
    /// Gets a complex number with real part equal to one and imaginary part equal to zero.
    /// </summary>
    public static Complex<T> One => new Complex<T>(T.One, T.Zero);

    /// <summary>
    /// Gets a complex number with real part equal to zero and imaginary part equal to one.
    /// </summary>
    public static Complex<T> ImaginaryOne => new Complex<T>(T.Zero, T.One);

    /// <summary>
    /// Gets a complex number with real part equal to negative one and imaginary part equal to zero.
    /// </summary>
    public static Complex<T> NegativeOne => new Complex<T>(T.NegativeOne, T.Zero);

    /// <summary>
    /// Gets the radix (base) for the number system, which is 2 for binary floating-point numbers.
    /// </summary>
    public static int Radix => 2;

    /// <summary>
    /// Gets the additive identity, which is <see cref="Zero"/>.
    /// </summary>
    public static Complex<T> AdditiveIdentity => Zero;

    /// <summary>
    /// Gets the multiplicative identity, which is <see cref="One"/>.
    /// </summary>
    public static Complex<T> MultiplicativeIdentity => One;

    /// <summary>
    /// Computes the absolute value (magnitude) of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>A <see cref="Complex{T}"/> with the magnitude as the real part and zero as the imaginary part.</returns>
    /// <remarks>
    /// The magnitude is calculated as sqrt(real^2 + imaginary^2). Returns NaN if either part is NaN.
    /// </remarks>
    public static Complex<T> Abs(Complex<T> value) => new Complex<T>(T.Sqrt(value.Real * value.Real + value.Imaginary * value.Imaginary), T.Zero);

    /// <summary>
    /// Computes the sine of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The sine of the complex number.</returns>
    /// <remarks>
    /// Uses the formula: sin(x + yi) = sin(x)cosh(y) + i*cos(x)sinh(y).
    /// </remarks>
    public static Complex<T> Sin(Complex<T> value)
    {
        T sin = T.Sin(value.Real), cos = T.Cos(value.Real);
        T sinh = T.Sinh(value.Imaginary), cosh = T.Cosh(value.Imaginary);
        return new Complex<T>(sin * cosh, cos * sinh);
    }

    /// <summary>
    /// Computes the hyperbolic sine of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The hyperbolic sine of the complex number.</returns>
    /// <remarks>
    /// Uses the identity: sinh(x + yi) = i*sin(-y + xi).
    /// </remarks>
    public static Complex<T> Sinh(Complex<T> value)
    {
        Complex<T> sin = Sin(new Complex<T>(T.NegativeOne * value.Imaginary, value.Real));
        return new Complex<T>(sin.Imaginary, T.NegativeOne * sin.Real);
    }

    /// <summary>
    /// Computes the arcsine of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The arcsine of the complex number.</returns>
    /// <remarks>
    /// Uses the formula: asin(z) = (i/2)*(log(1 - iz) - log(1 + iz)).
    /// Handles large values and ensures numerical stability.
    /// </remarks>
    public static Complex<T> Asin(Complex<T> value)
    {
        T one = T.One, two = one + one, negOne = T.NegativeOne;
        Complex<T> i = ImaginaryOne, negI = new Complex<T>(T.Zero, negOne);
        return (i / two) * (Log(One - i * value) - Log(One + i * value));
    }

    /// <summary>
    /// Computes the cosine of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The cosine of the complex number.</returns>
    /// <remarks>
    /// Uses the formula: cos(x + yi) = cos(x)cosh(y) - i*sin(x)sinh(y).
    /// </remarks>
    public static Complex<T> Cos(Complex<T> value)
    {
        T piOver2 = T.CreateChecked(Math.PI / 2);
        if (value.Imaginary == T.Zero && T.Abs(value.Real - piOver2) < T.CreateChecked(1e-6))
        {
            return Complex<T>.Zero;
        }
        T sin = T.Sin(value.Real), cos = T.Cos(value.Real);
        T sinh = T.Sinh(value.Imaginary), cosh = T.Cosh(value.Imaginary);
        return new Complex<T>(cos * cosh, T.NegativeOne * sin * sinh);
    }

    /// <summary>
    /// Computes the hyperbolic cosine of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The hyperbolic cosine of the complex number.</returns>
    /// <remarks>
    /// Uses the identity: cosh(x + yi) = cos(-y + xi).
    /// </remarks>
    public static Complex<T> Cosh(Complex<T> value) => Cos(new Complex<T>(T.NegativeOne * value.Imaginary, value.Real));

    /// <summary>
    /// Computes the arccosine of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The arccosine of the complex number.</returns>
    /// <remarks>
    /// Uses a numerically stable algorithm to handle large values and avoid overflow.
    /// For z = x + yi, computes u + vi where u is the real part and v is the imaginary part, 
    /// adjusting for the signs of x and y to ensure correct quadrant.
    /// </remarks>
    public static Complex<T> Acos(Complex<T> value)
    {
        T b, bPrime, v;
        Asin_Internal(T.Abs(value.Real), T.Abs(value.Imaginary), out b, out bPrime, out v);

        T u;
        if (bPrime < T.Zero)
        {
            u = T.Acos(b);
        }
        else
        {
            u = T.Atan(T.One / bPrime);
        }

        if (value.Real < T.Zero) u = T.Pi - u;
        if (value.Imaginary > T.Zero) v = -v;

        return new Complex<T>(u, v);
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

        Debug.Assert((x >= T.Zero) || T.IsNaN(x));
        Debug.Assert((y >= T.Zero) || T.IsNaN(y));

        var dot5 = T.CreateChecked(0.5);

        // For x or y large enough to overflow alpha^2, we can simplify our formulas and avoid overflow.
        if ((x > s_asinOverflowThreshold) || (y > s_asinOverflowThreshold))
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
            v = s_log2 + T.Log(big) + dot5 * Log1P(ratio * ratio);
        }
        else
        {
            T r = T.Hypot((x + T.One), y);
            T s = T.Hypot((x - T.One), y);

            T a = (r + s) * dot5;
            b = x / a;

            if (b > T.CreateChecked(0.75))
            {
                if (x <= T.One)
                {
                    T amx = (y * y / (r + (x + T.One)) + (s + (T.One - x))) * dot5;
                    bPrime = x / T.Sqrt((a + x) * amx);
                }
                else
                {
                    // In this case, amx ~ y^2. Since we take the square root of amx, we should
                    // pull y out from under the square root so we don't lose its contribution
                    // when y^2 underflows.
                    T t = (T.One / (r + (x + T.One)) + T.One / (s + (x - T.One))) * dot5;
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
                    T t = (T.One / (r + (x + T.One)) + T.One / (s + (T.One - x))) * dot5;
                    T am1 = y * y * t;
                    v = Log1P(am1 + y * T.Sqrt(t * (a + T.One)));
                }
                else
                {
                    T am1 = (y * y / (r + (x + T.One)) + (s + (x - T.One))) * dot5;
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

    private static T Log1P(T x)
    {
        // Compute log(1 + x) without loss of accuracy when x is small.

        // Our only use case so far is for positive values, so this isn't coded to handle negative values.
        Debug.Assert((x >= T.Zero) || T.IsNaN(x));

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
    /// <param name="value">The complex number to conjugate.</param>
    /// <returns>The complex conjugate of <paramref name="value"/>, where the real part remains unchanged and the imaginary part is negated, using single-precision floating-point numbers.</returns>
    public static Complex<T> Conjugate(Complex<T> value)
    {
        // Conjugate of a Complex number: the conjugate of x+i*y is x-i*y
        return new Complex<T>(value.Real, -value.Imaginary);
    }

    /// <summary>
    /// Returns the reciprocal of a complex number.
    /// </summary>
    /// <param name="value">The complex number to find the reciprocal of.</param>
    /// <returns>The reciprocal of <paramref name="value"/>, computed as 1 divided by <paramref name="value"/>, or <see cref="Zero"/> if <paramref name="value"/> is zero, using single-precision floating-point numbers.</returns>
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
    /// Computes the tangent of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The tangent of the complex number.</returns>
    /// <remarks>
    /// Uses the formula: tan(x + yi) = sin(2x)/(cos(2x) + cosh(2y)) + i*sinh(2y)/(cos(2x) + cosh(2y)).
    /// Switches to an alternative formula for large imaginary parts to avoid overflow.
    /// </remarks>
    public static Complex<T> Tan(Complex<T> value)
    {
        T two = T.One + T.One, x2 = two * value.Real, y2 = two * value.Imaginary;
        T sin = T.Sin(x2), cos = T.Cos(x2), cosh = T.Cosh(y2), sinh = T.Sinh(y2);
        T four = two + two;
        if (T.Abs(value.Imaginary) <= T.CreateChecked(4.0))
        {
            T D = cos + cosh;
            return new Complex<T>(sin / D, sinh / D);
        }
        else
        {
            T D = T.One + cos / cosh;
            return new Complex<T>(sin / cosh / D, T.Tanh(y2) / D);
        }
    }

    /// <summary>
    /// Computes the hyperbolic tangent of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The hyperbolic tangent of the complex number.</returns>
    /// <remarks>
    /// Uses the identity: tanh(x + yi) = i*tan(-y + xi).
    /// </remarks>
    public static Complex<T> Tanh(Complex<T> value)
    {
        Complex<T> tan = Tan(new Complex<T>(T.NegativeOne * value.Imaginary, value.Real));
        return new Complex<T>(tan.Imaginary, T.NegativeOne * tan.Real);
    }

    /// <summary>
    /// Computes the arctangent of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The arctangent of the complex number.</returns>
    /// <remarks>
    /// Uses the formula: atan(z) = (i/2)*(log(1 - iz) - log(1 + iz)).
    /// </remarks>
    public static Complex<T> Atan(Complex<T> value)
    {
        T two = T.One + T.One;
        Complex<T> i = ImaginaryOne;
        return (i / two) * (Log(One - i * value) - Log(One + i * value));
    }

    /// <summary>
    /// Computes the natural logarithm of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The natural logarithm of the complex number.</returns>
    /// <remarks>
    /// Uses the formula: log(z) = log(|z|) + i*atan2(imaginary, real).
    /// </remarks>
    public static Complex<T> Log(Complex<T> value)
    {
        T magnitude = Abs(value).Real;
        T theta = T.Atan2(value.Imaginary, value.Real);
        return new Complex<T>(T.Log(magnitude), theta);
    }

    /// <summary>
    /// Computes the base-10 logarithm of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The base-10 logarithm of the complex number.</returns>
    /// <remarks>
    /// Uses the formula: log10(z) = log(z)/log(10).
    /// </remarks>
    public static Complex<T> Log10(Complex<T> value) => Log(value) / T.CreateChecked(Math.Log(10.0));

    /// <summary>
    /// Computes the exponential of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The exponential of the complex number.</returns>
    /// <remarks>
    /// Uses the formula: exp(x + yi) = exp(x)*(cos(y) + i*sin(y)).
    /// </remarks>
    public static Complex<T> Exp(Complex<T> value)
    {
        T expReal = T.Exp(value.Real);
        T sin = T.Sin(value.Imaginary), cos = T.Cos(value.Imaginary);
        return new Complex<T>(expReal * cos, expReal * sin);
    }

    /// <summary>
    /// Computes the square root of a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The square root of the complex number.</returns>
    /// <remarks>
    /// Handles special cases (NaN, infinity) and uses a numerically stable algorithm.
    /// For z = x + yi, computes sqrt(|z| + x)/2 + i*(y/(2*sqrt(|z| + x))) for x >= 0,
    /// or adjusts for x < 0 to ensure correct quadrant.
    /// </remarks>
    public static Complex<T> Sqrt(Complex<T> value)
    {
        if (T.IsNaN(value.Real))
        {
            if (T.IsInfinity(value.Imaginary)) return new Complex<T>(T.PositiveInfinity, value.Imaginary);
            return new Complex<T>(T.NaN, T.NaN);
        }
        if (T.IsNaN(value.Imaginary))
        {
            if (T.IsPositiveInfinity(value.Real)) return new Complex<T>(T.NaN, T.PositiveInfinity);
            if (T.IsNegativeInfinity(value.Real)) return new Complex<T>(T.PositiveInfinity, T.NaN);
            return new Complex<T>(T.NaN, T.NaN);
        }
        if (value.Imaginary == T.Zero)
        {
            if (T.IsNegative(value.Real)) return new Complex<T>(T.Zero, T.Sqrt(T.NegativeOne * value.Real));
            return new Complex<T>(T.Sqrt(value.Real), T.Zero);
        }
        T x, y;
        T zero = T.Zero, one = T.One, two = one + one;
        T magnitude = T.Sqrt(value.Real * value.Real + value.Imaginary * value.Imaginary);
        if (T.IsPositive(value.Real) || value.Real == zero)
        {
            x = T.Sqrt((magnitude + value.Real) / two);
            y = value.Imaginary / (two * x);
        }
        else
        {
            y = T.Sqrt((magnitude - value.Real) / two);
            if (T.IsNegative(value.Imaginary)) y = T.NegativeOne * y;
            x = value.Imaginary / (two * y);
        }
        return new Complex<T>(x, y);
    }

    /// <summary>
    /// Raises a complex number to a complex power.
    /// </summary>
    /// <param name="value">The base complex number.</param>
    /// <param name="power">The exponent complex number.</param>
    /// <returns>The result of raising <paramref name="value"/> to <paramref name="power"/>.</returns>
    /// <remarks>
    /// Uses the formula: z^w = exp(w*log(z)) = |z|^w * exp(-w*theta*i) * (cos(w*theta) + i*sin(w*theta)).
    /// </remarks>
    public static Complex<T> Pow(Complex<T> value, Complex<T> power)
    {
        if (power == Zero) return One;
        if (value == Zero) return Zero;
        T rho = Abs(value).Real, theta = T.Atan2(value.Imaginary, value.Real);
        T newRho = power.Real * theta + power.Imaginary * T.Log(rho);
        T t = T.Pow(rho, power.Real) * T.Exp(T.NegativeOne * power.Imaginary * theta);
        return new Complex<T>(t * T.Cos(newRho), t * T.Sin(newRho));
    }

    /// <summary>
    /// Determines if the complex number is in canonical form.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>Always true, as complex numbers are always in canonical form.</returns>
    public static bool IsCanonical(Complex<T> value) => true;

    /// <summary>
    /// Determines if the complex number is a non-zero complex number (non-zero real and imaginary parts).
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if both real and imaginary parts are non-zero; otherwise, false.</returns>
    public static bool IsComplexNumber(Complex<T> value) => value.Real != T.Zero && value.Imaginary != T.Zero;

    /// <summary>
    /// Determines if the complex number represents an even integer.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the imaginary part is zero and the real part is an even integer; otherwise, false.</returns>
    public static bool IsEvenInteger(Complex<T> value) => value.Imaginary == T.Zero && T.IsEvenInteger(value.Real);

    /// <summary>
    /// Determines if the complex number is finite.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if both real and imaginary parts are finite; otherwise, false.</returns>
    public static bool IsFinite(Complex<T> value) => T.IsFinite(value.Real) && T.IsFinite(value.Imaginary);

    /// <summary>
    /// Determines if the complex number is purely imaginary (real part is zero).
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the real part is zero and the imaginary part is a real number; otherwise, false.</returns>
    public static bool IsImaginaryNumber(Complex<T> value) => value.Real == T.Zero && T.IsRealNumber(value.Imaginary);

    /// <summary>
    /// Determines if the complex number is infinite.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if either the real or imaginary part is infinite; otherwise, false.</returns>
    public static bool IsInfinity(Complex<T> value) => T.IsInfinity(value.Real) || T.IsInfinity(value.Imaginary);

    /// <summary>
    /// Determines if the complex number represents an integer.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the imaginary part is zero and the real part is an integer; otherwise, false.</returns>
    public static bool IsInteger(Complex<T> value) => value.Imaginary == T.Zero && T.IsInteger(value.Real);

    /// <summary>
    /// Determines if the complex number is NaN.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the number is neither finite nor infinite (i.e., NaN); otherwise, false.</returns>
    public static bool IsNaN(Complex<T> value) => !IsInfinity(value) && !IsFinite(value);

    /// <summary>
    /// Determines if the complex number is negative.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the imaginary part is zero and the real part is negative; otherwise, false.</returns>
    public static bool IsNegative(Complex<T> value) => value.Imaginary == T.Zero && T.IsNegative(value.Real);

    /// <summary>
    /// Determines if the complex number is negative infinity.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the imaginary part is zero and the real part is negative infinity; otherwise, false.</returns>
    public static bool IsNegativeInfinity(Complex<T> value) => value.Imaginary == T.Zero && T.IsNegativeInfinity(value.Real);

    /// <summary>
    /// Determines if the complex number is normal (not zero, subnormal, infinite, or NaN).
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the real part is normal and the imaginary part is either zero or normal; otherwise, false.</returns>
    public static bool IsNormal(Complex<T> value) => T.IsNormal(value.Real) && (value.Imaginary == T.Zero || T.IsNormal(value.Imaginary));

    /// <summary>
    /// Determines if the complex number represents an odd integer.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the imaginary part is zero and the real part is an odd integer; otherwise, false.</returns>
    public static bool IsOddInteger(Complex<T> value) => value.Imaginary == T.Zero && T.IsOddInteger(value.Real);

    /// <summary>
    /// Determines if the complex number is positive.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the imaginary part is zero and the real part is positive; otherwise, false.</returns>
    public static bool IsPositive(Complex<T> value) => value.Imaginary == T.Zero && T.IsPositive(value.Real);

    /// <summary>
    /// Determines if the complex number is positive infinity.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the imaginary part is zero and the real part is positive infinity; otherwise, false.</returns>
    public static bool IsPositiveInfinity(Complex<T> value) => value.Imaginary == T.Zero && T.IsPositiveInfinity(value.Real);

    /// <summary>
    /// Determines if the complex number is a real number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if the imaginary part is zero and the real part is a real number; otherwise, false.</returns>
    public static bool IsRealNumber(Complex<T> value) => value.Imaginary == T.Zero && T.IsRealNumber(value.Real);

    /// <summary>
    /// Determines if the complex number is subnormal.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if either the real or imaginary part is subnormal; otherwise, false.</returns>
    public static bool IsSubnormal(Complex<T> value) => T.IsSubnormal(value.Real) || T.IsSubnormal(value.Imaginary);

    /// <summary>
    /// Determines if the complex number is zero.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>True if both real and imaginary parts are zero; otherwise, false.</returns>
    public static bool IsZero(Complex<T> value) => value.Real == T.Zero && value.Imaginary == T.Zero;

    /// <summary>
    /// Returns the complex number with the larger magnitude.
    /// </summary>
    /// <param name="x">The first complex number.</param>
    /// <param name="y">The second complex number.</param>
    /// <returns>The complex number with the larger magnitude, with tie-breaking rules for equal magnitudes.</returns>
    /// <remarks>
    /// If magnitudes are equal, prefers the number with non-negative real and imaginary parts.
    /// </remarks>
    public static Complex<T> MaxMagnitude(Complex<T> x, Complex<T> y)
    {
        T ax = Abs(x).Real, ay = Abs(y).Real;
        if (T.IsNaN(ax) || ax > ay) return x;
        if (ax == ay)
        {
            if (T.IsNegative(y.Real))
            {
                if (T.IsNegative(y.Imaginary)) return x;
                else if (T.IsNegative(x.Real)) return y;
                else return x;
            }
            else if (T.IsNegative(y.Imaginary)) return T.IsNegative(x.Real) ? y : x;
        }
        return y;
    }

    /// <summary>
    /// Returns the complex number with the larger magnitude, treating NaN as the maximum.
    /// </summary>
    /// <param name="x">The first complex number.</param>
    /// <param name="y">The second complex number.</param>
    /// <returns>The complex number with the larger magnitude, with NaN taking precedence.</returns>
    public static Complex<T> MaxMagnitudeNumber(Complex<T> x, Complex<T> y)
    {
        T ax = Abs(x).Real, ay = Abs(y).Real;
        if (T.IsNaN(ay)) return x;
        if (ax > ay) return x;
        if (ax == ay)
        {
            if (T.IsNegative(y.Real))
            {
                if (T.IsNegative(y.Imaginary)) return x;
                else if (T.IsNegative(x.Real)) return y;
                else return x;
            }
            else if (T.IsNegative(y.Imaginary)) return T.IsNegative(x.Real) ? y : x;
        }
        return y;
    }

    /// <summary>
    /// Returns the complex number with the smaller magnitude.
    /// </summary>
    /// <param name="x">The first complex number.</param>
    /// <param name="y">The second complex number.</param>
    /// <returns>The complex number with the smaller magnitude, with tie-breaking rules for equal magnitudes.</returns>
    /// <remarks>
    /// If magnitudes are equal, prefers the number with non-negative real and imaginary parts.
    /// </remarks>
    public static Complex<T> MinMagnitude(Complex<T> x, Complex<T> y)
    {
        T ax = Abs(x).Real, ay = Abs(y).Real;
        if (T.IsNaN(ax) || ax < ay) return x;
        if (ax == ay)
        {
            if (T.IsNegative(y.Real))
            {
                if (T.IsNegative(y.Imaginary)) return y;
                else if (T.IsNegative(x.Real)) return x;
                else return y;
            }
            else if (T.IsNegative(y.Imaginary)) return T.IsNegative(x.Real) ? x : y;
            else return x;
        }
        return y;
    }

    /// <summary>
    /// Returns the complex number with the smaller magnitude, treating NaN as the minimum.
    /// </summary>
    /// <param name="x">The first complex number.</param>
    /// <param name="y">The second complex number.</param>
    /// <returns>The complex number with the smaller magnitude, with NaN taking precedence.</returns>
    public static Complex<T> MinMagnitudeNumber(Complex<T> x, Complex<T> y)
    {
        T ax = Abs(x).Real, ay = Abs(y).Real;
        if (T.IsNaN(ay) || ax < ay) return x;
        if (ax == ay)
        {
            if (T.IsNegative(y.Real))
            {
                if (T.IsNegative(y.Imaginary)) return y;
                else if (T.IsNegative(x.Real)) return x;
                else return y;
            }
            else if (T.IsNegative(y.Imaginary)) return T.IsNegative(x.Real) ? x : y;
            else return x;
        }
        return y;
    }

    /// <summary>
    /// Parses a string into a complex number.
    /// </summary>
    /// <param name="s">The string to parse, in the format "<real; imaginary>".</param>
    /// <param name="style">The number style to use for parsing.</param>
    /// <param name="provider">The format provider for parsing.</param>
    /// <returns>The parsed complex number.</returns>
    /// <exception cref="OverflowException">Thrown if parsing fails.</exception>
    public static Complex<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        if (!TryParse(s, style, provider, out Complex<T> result)) throw new OverflowException();
        return result;
    }

    /// <summary>
    /// Parses a string into a complex number.
    /// </summary>
    /// <param name="s">The string to parse, in the format "<real; imaginary>".</param>
    /// <param name="style">The number style to use for parsing.</param>
    /// <param name="provider">The format provider for parsing.</param>
    /// <returns>The parsed complex number.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="s"/> is null.</exception>
    /// <exception cref="OverflowException">Thrown if parsing fails.</exception>
    public static Complex<T> Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), style, provider);
    }

    /// <summary>
    /// Parses a string into a complex number using default styles.
    /// </summary>
    /// <param name="s">The string to parse, in the format "<real; imaginary>".</param>
    /// <param name="provider">The format provider for parsing.</param>
    /// <returns>The parsed complex number.</returns>
    public static Complex<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
    }

    /// <summary>
    /// Parses a string into a complex number using default styles.
    /// </summary>
    /// <param name="s">The string to parse, in the format "<real; imaginary>".</param>
    /// <param name="provider">The format provider for parsing.</param>
    /// <returns>The parsed complex number.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="s"/> is null.</exception>
    public static Complex<T> Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), provider);
    }

    /// <summary>
    /// Attempts to parse a string into a complex number.
    /// </summary>
    /// <param name="s">The string to parse, in the format "<real; imaginary>".</param>
    /// <param name="style">The number style to use for parsing.</param>
    /// <param name="provider">The format provider for parsing.</param>
    /// <param name="result">The parsed complex number, or default if parsing fails.</param>
    /// <returns>True if parsing succeeds; otherwise, false.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Complex<T> result)
    {
        result = default;
        if (s.Length < 3 || s[0] != '<' || s[^1] != '>') return false;
        int semicolonIndex = s.IndexOf(';');
        if (semicolonIndex == -1) return false;
        var realSpan = s.Slice(1, semicolonIndex - 1).Trim();
        var imagSpan = s.Slice(semicolonIndex + 1, s.Length - semicolonIndex - 2).Trim();
        if (!T.TryParse(realSpan, style, provider, out T real)) return false;
        if (!T.TryParse(imagSpan, style, provider, out T imag)) return false;
        result = new Complex<T>(real, imag);
        return true;
    }

    /// <summary>
    /// Attempts to parse a string into a complex number.
    /// </summary>
    /// <param name="s">The string to parse, in the format "<real; imaginary>".</param>
    /// <param name="style">The number style to use for parsing.</param>
    /// <param name="provider">The format provider for parsing.</param>
    /// <param name="result">The parsed complex number, or default if parsing fails.</param>
    /// <returns>True if parsing succeeds; otherwise, false.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Complex<T> result)
    {
        if (string.IsNullOrEmpty(s))
        {
            result = default;
            return false;
        }
        return TryParse(s.AsSpan(), style, provider, out result);
    }

    /// <summary>
    /// Attempts to parse a string into a complex number using default styles.
    /// </summary>
    /// <param name="s">The string to parse, in the format "<real; imaginary>".</param>
    /// <param name="provider">The format provider for parsing.</param>
    /// <param name="result">The parsed complex number, or default if parsing fails.</param>
    /// <returns>True if parsing succeeds; otherwise, false.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Complex<T> result)
    {
        return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);
    }

    /// <summary>
    /// Attempts to parse a string into a complex number using default styles.
    /// </summary>
    /// <param name="s">The string to parse, in the format "<real; imaginary>".</param>
    /// <param name="provider">The format provider for parsing.</param>
    /// <param name="result">The parsed complex number, or default if parsing fails.</param>
    /// <returns>True if parsing succeeds; otherwise, false.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Complex<T> result)
    {
        return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);
    }

    /// <summary>
    /// Converts a value to a complex number with checked conversion.
    /// </summary>
    /// <typeparam name="TOther">The type to convert from.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted complex number, or default if conversion fails.</param>
    /// <returns>True if conversion succeeds; otherwise, false.</returns>
    public static bool TryConvertFromChecked<TOther>(TOther value, [MaybeNullWhen(false)] out Complex<T> result) where TOther : INumberBase<TOther>
    {
        if (T.TryConvertFromChecked(value, out T real))
        {
            result = new Complex<T>(real, T.Zero);
            return true;
        }
        result = default;
        return false;
    }

    /// <summary>
    /// Converts a value to a complex number with saturating conversion.
    /// </summary>
    /// <typeparam name="TOther">The type to convert from.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted complex number, or default if conversion fails.</param>
    /// <returns>True if conversion succeeds; otherwise, false.</returns>
    public static bool TryConvertFromSaturating<TOther>(TOther value, [MaybeNullWhen(false)] out Complex<T> result) where TOther : INumberBase<TOther>
    {
        if (T.TryConvertFromSaturating(value, out T real))
        {
            result = new Complex<T>(real, T.Zero);
            return true;
        }
        result = default;
        return false;
    }

    /// <summary>
    /// Converts a value to a complex number with truncating conversion.
    /// </summary>
    /// <typeparam name="TOther">The type to convert from.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted complex number, or default if conversion fails.</param>
    /// <returns>True if conversion succeeds; otherwise, false.</returns>
    public static bool TryConvertFromTruncating<TOther>(TOther value, [MaybeNullWhen(false)] out Complex<T> result) where TOther : INumberBase<TOther>
    {
        if (T.TryConvertFromTruncating(value, out T real))
        {
            result = new Complex<T>(real, T.Zero);
            return true;
        }
        result = default;
        return false;
    }

    /// <summary>
    /// Converts a complex number to another numeric type with checked conversion.
    /// </summary>
    /// <typeparam name="TOther">The target type.</typeparam>
    /// <param name="value">The complex number to convert.</param>
    /// <param name="result">The converted value, or default if conversion fails.</param>
    /// <returns>True if conversion succeeds; otherwise, false.</returns>
    public static bool TryConvertToChecked<TOther>(Complex<T> value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        if (value.Imaginary != T.Zero)
        {
            result = default;
            return false;
        }
        return T.TryConvertToChecked(value.Real, out result);
    }

    /// <summary>
    /// Converts a complex number to another numeric type with saturating conversion.
    /// </summary>
    /// <typeparam name="TOther">The target type.</typeparam>
    /// <param name="value">The complex number to convert.</param>
    /// <param name="result">The converted value, or default if conversion fails.</param>
    /// <returns>True if conversion succeeds; otherwise, false.</returns>
    public static bool TryConvertToSaturating<TOther>(Complex<T> value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        if (value.Imaginary != T.Zero)
        {
            result = default;
            return false;
        }
        return T.TryConvertToSaturating(value.Real, out result);
    }

    /// <summary>
    /// Converts a complex number to another numeric type with truncating conversion.
    /// </summary>
    /// <typeparam name="TOther">The target type.</typeparam>
    /// <param name="value">The complex number to convert.</param>
    /// <param name="result">The converted value, or default if conversion fails.</param>
    /// <returns>True if conversion succeeds; otherwise, false.</returns>
    public static bool TryConvertToTruncating<TOther>(Complex<T> value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        if (value.Imaginary != T.Zero)
        {
            result = default;
            return false;
        }
        return T.TryConvertToTruncating(value.Real, out result);
    }

    /// <summary>
    /// Determines whether this complex number equals another complex number.
    /// </summary>
    /// <param name="other">The complex number to compare with.</param>
    /// <returns>True if the real and imaginary parts are equal; otherwise, false.</returns>
    public bool Equals(Complex<T> other) => Real.Equals(other.Real) && Imaginary.Equals(other.Imaginary);

    /// <summary>
    /// Determines whether this complex number equals another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the object is a <see cref="Complex{T}"/> and equals this instance; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Complex<T> other && Equals(other);

    /// <summary>
    /// Computes a hash code for the complex number.
    /// </summary>
    /// <returns>A hash code based on the real and imaginary parts.</returns>
    public override int GetHashCode() => HashCode.Combine(Real, Imaginary);

    /// <summary>
    /// Compares this complex number to another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>A value indicating the relative order of the objects.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="obj"/> is not a <see cref="Complex{T}"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is Complex<T> other) return CompareTo(other);
        throw new ArgumentException("Object must be of type Complex<T>.");
    }

    /// <summary>
    /// Compares this complex number to another complex number based on magnitude.
    /// </summary>
    /// <param name="other">The complex number to compare with.</param>
    /// <returns>A negative number if this magnitude is less, zero if equal, or a positive number if greater.</returns>
    /// <remarks>
    /// If magnitudes are equal, applies tie-breaking rules based on the signs of real and imaginary parts.
    /// </remarks>
    public int CompareTo(Complex<T> other)
    {
        if (IsNaN(this) || IsNaN(other)) return 0;
        T thisAbs = Abs(this).Real, otherAbs = Abs(other).Real;
        if (thisAbs < otherAbs) return -1;
        if (thisAbs > otherAbs) return 1;
        if (T.IsNegative(other.Real))
        {
            if (T.IsNegative(other.Imaginary)) return 1;
            else if (T.IsNegative(this.Real)) return -1;
            else return 1;
        }
        else if (T.IsNegative(other.Imaginary)) return T.IsNegative(this.Real) ? -1 : 1;
        return 0;
    }

    /// <summary>
    /// Formats the complex number as a string.
    /// </summary>
    /// <param name="format">The format to use for the real and imaginary parts.</param>
    /// <param name="formatProvider">The format provider to use.</param>
    /// <returns>A string in the format "<real; imaginary>".</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        var handler = new DefaultInterpolatedStringHandler(4, 2, formatProvider, stackalloc char[512]);
        handler.AppendLiteral("<");
        handler.AppendFormatted(Real, format);
        handler.AppendLiteral("; ");
        handler.AppendFormatted(Imaginary, format);
        handler.AppendLiteral(">");
        return handler.ToStringAndClear();
    }

    /// <summary>
    /// Attempts to format the complex number into a character span.
    /// </summary>
    /// <param name="destination">The destination span for the formatted string.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <param name="format">The format to use for the real and imaginary parts.</param>
    /// <param name="provider">The format provider to use.</param>
    /// <returns>True if formatting succeeds; otherwise, false.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        string s = ToString(format.ToString(), provider);
        charsWritten = s.Length;
        if (charsWritten > destination.Length)
        {
            charsWritten = 0;
            return false;
        }
        s.CopyTo(destination);
        return true;
    }

    /// <summary>
    /// Attempts to format the complex number into a UTF-8 byte span.
    /// </summary>
    /// <param name="utf8Destination">The destination span for the UTF-8 encoded string.</param>
    /// <param name="bytesWritten">The number of bytes written.</param>
    /// <param name="format">The format to use for the real and imaginary parts.</param>
    /// <param name="provider">The format provider to use.</param>
    /// <returns>True if formatting succeeds; otherwise, false.</returns>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        string s = ToString(format.ToString(), provider);
        int maxBytes = Encoding.UTF8.GetMaxByteCount(s.Length);
        if (maxBytes > utf8Destination.Length)
        {
            bytesWritten = 0;
            return false;
        }
        bytesWritten = Encoding.UTF8.GetBytes(s, utf8Destination);
        return true;
    }

    /// <summary>
    /// Determines if one complex number is greater than another based on magnitude.
    /// </summary>
    /// <param name="left">The first complex number.</param>
    /// <param name="right">The second complex number.</param>
    /// <returns>True if the magnitude of <paramref name="left"/> is greater than that of <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator >(Complex<T> left, Complex<T> right)
    {
        if (IsNaN(left) || IsNaN(right)) return false;
        return Abs(left).Real > Abs(right).Real;
    }

    /// <summary>
    /// Determines if one complex number is greater than or equal to another based on magnitude.
    /// </summary>
    /// <param name="left">The first complex number.</param>
    /// <param name="right">The second complex number.</param>
    /// <returns>True if the magnitude of <paramref name="left"/> is greater than or equal to that of <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator >=(Complex<T> left, Complex<T> right)
    {
        if (IsNaN(left) || IsNaN(right)) return false;
        return Abs(left).Real >= Abs(right).Real;
    }

    /// <summary>
    /// Determines if one complex number is less than another based on magnitude.
    /// </summary>
    /// <param name="left">The first complex number.</param>
    /// <param name="right">The second complex number.</param>
    /// <returns>True if the magnitude of <paramref name="left"/> is less than that of <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator <(Complex<T> left, Complex<T> right)
    {
        if (IsNaN(left) || IsNaN(right)) return false;
        return Abs(left).Real < Abs(right).Real;
    }

    /// <summary>
    /// Determines if one complex number is less than or equal to another based on magnitude.
    /// </summary>
    /// <param name="left">The first complex number.</param>
    /// <param name="right">The second complex number.</param>
    /// <returns>True if the magnitude of <paramref name="left"/> is less than or equal to that of <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator <=(Complex<T> left, Complex<T> right)
    {
        if (IsNaN(left) || IsNaN(right)) return false;
        return Abs(left).Real <= Abs(right).Real;
    }

    /// <summary>
    /// Determines if two complex numbers are equal.
    /// </summary>
    /// <param name="left">The first complex number.</param>
    /// <param name="right">The second complex number.</param>
    /// <returns>True if the real and imaginary parts are equal; otherwise, false.</returns>
    public static bool operator ==(Complex<T> left, Complex<T> right) => left.Real == right.Real && left.Imaginary == right.Imaginary;

    /// <summary>
    /// Determines if two complex numbers are not equal.
    /// </summary>
    /// <param name="left">The first complex number.</param>
    /// <param name="right">The second complex number.</param>
    /// <returns>True if the real or imaginary parts differ; otherwise, false.</returns>
    public static bool operator !=(Complex<T> left, Complex<T> right) => !(left == right);

    /// <summary>
    /// Adds two complex numbers.
    /// </summary>
    /// <param name="left">The first complex number.</param>
    /// <param name="right">The second complex number.</param>
    /// <returns>The sum of the two complex numbers.</returns>
    public static Complex<T> operator +(Complex<T> left, Complex<T> right) => new Complex<T>(left.Real + right.Real, left.Imaginary + right.Imaginary);

    /// <summary>
    /// Subtracts one complex number from another.
    /// </summary>
    /// <param name="left">The first complex number.</param>
    /// <param name="right">The second complex number.</param>
    /// <returns>The difference of the two complex numbers.</returns>
    public static Complex<T> operator -(Complex<T> left, Complex<T> right) => new Complex<T>(left.Real - right.Real, left.Imaginary - right.Imaginary);

    /// <summary>
    /// Multiplies two complex numbers.
    /// </summary>
    /// <param name="left">The first complex number.</param>
    /// <param name="right">The second complex number.</param>
    /// <returns>The product of the two complex numbers.</returns>
    /// <remarks>
    /// Uses the formula: (a + bi)*(c + di) = (ac - bd) + i(ad + bc).
    /// </remarks>
    public static Complex<T> operator *(Complex<T> left, Complex<T> right)
    {
        T real = left.Real * right.Real - left.Imaginary * right.Imaginary;
        T imag = left.Imaginary * right.Real + left.Real * right.Imaginary;
        return new Complex<T>(real, imag);
    }

    /// <summary>
    /// Divides one complex number by another.
    /// </summary>
    /// <param name="left">The numerator complex number.</param>
    /// <param name="right">The denominator complex number.</param>
    /// <returns>The quotient of the two complex numbers.</returns>
    /// <remarks>
    /// Uses Smith's formula to avoid overflow: (a + bi)/(c + di) = ((a + b*(d/c))/(c + d*(d/c)), (b - a*(d/c))/(c + d*(d/c))) if |d| < |c|,
    /// or ((b + a*(c/d))/(d + c*(c/d)), (-a + b*(c/d))/(d + c*(c/d))) otherwise.
    /// </remarks>
    public static Complex<T> operator /(Complex<T> left, Complex<T> right)
    {
        T a = left.Real, b = left.Imaginary, c = right.Real, d = right.Imaginary;
        T absC = T.Abs(c), absD = T.Abs(d);
        if (absD < absC)
        {
            T doc = d / c;
            T denom = c + d * doc;
            return new Complex<T>((a + b * doc) / denom, (b - a * doc) / denom);
        }
        else
        {
            T cod = c / d;
            T denom = d + c * cod;
            return new Complex<T>((b + a * cod) / denom, (T.NegativeOne * a + b * cod) / denom);
        }
    }

    /// <summary>
    /// Divides a complex number by a scalar.
    /// </summary>
    /// <param name="left">The complex number.</param>
    /// <param name="right">The scalar denominator.</param>
    /// <returns>The result of dividing the complex number by the scalar.</returns>
    /// <remarks>
    /// Handles special cases like division by zero or non-finite inputs, returning NaN where appropriate.
    /// </remarks>
    public static Complex<T> operator /(Complex<T> left, T right)
    {
        // IEEE prohibits optimizations that change values, so behavior matches full division.
        if (T.IsZero(right))
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

        // Optimized division for finite values.
        return new Complex<T>(left.Real / right, left.Imaginary / right);
    }

    /// <summary>
    /// Divides a scalar by a complex number.
    /// </summary>
    /// <param name="left">The scalar numerator.</param>
    /// <param name="right">The complex denominator.</param>
    /// <returns>The result of dividing the scalar by the complex number.</returns>
    /// <remarks>
    /// Uses Smith's formula to avoid overflow: a/(c + di) = (a/(c + d*(d/c)), -a*(d/c)/(c + d*(d/c))) if |d| < |c|,
    /// or (a*(c/d)/(d + c*(c/d)), -a/(d + c*(c/d))) otherwise.
    /// </remarks>
    public static Complex<T> operator /(T left, Complex<T> right)
    {
        T a = left;
        T c = right.Real;
        T d = right.Imaginary;

        if (T.Abs(d) < T.Abs(c))
        {
            T doc = d / c;
            return new Complex<T>(a / (c + d * doc), (T.NegativeOne * a * doc) / (c + d * doc));
        }
        else
        {
            T cod = c / d;
            return new Complex<T>(a * cod / (d + c * cod), T.NegativeOne * a / (d + c * cod));
        }
    }

    /// <summary>
    /// Computes the modulus of one complex number by another.
    /// </summary>
    /// <param name="left">The dividend complex number.</param>
    /// <param name="right">The divisor complex number.</param>
    /// <returns>The modulus of the division.</returns>
    /// <exception cref="DivideByZeroException">Thrown if the divisor is zero.</exception>
    /// <remarks>
    /// Computes z1 % z2 as z1 - z2*floor(z1/z2), applied component-wise to real and imaginary parts.
    /// </remarks>
    public static Complex<T> operator %(Complex<T> left, Complex<T> right)
    {
        if (right.Real == T.Zero && right.Imaginary == T.Zero) throw new DivideByZeroException();
        Complex<T> div = left / right;
        T real = left.Real - right.Real * T.Floor(div.Real);
        T imag = left.Imaginary - right.Imaginary * T.Floor(div.Imaginary);
        return new Complex<T>(real, imag);
    }

    /// <summary>
    /// Negates a complex number.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The negated complex number.</returns>
    public static Complex<T> operator -(Complex<T> value) => new Complex<T>(T.NegativeOne * value.Real, T.NegativeOne * value.Imaginary);

    /// <summary>
    /// Returns the complex number unchanged (unary plus).
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The same complex number.</returns>
    public static Complex<T> operator +(Complex<T> value) => value;

    /// <summary>
    /// Increments the real part of the complex number by one.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The incremented complex number.</returns>
    public static Complex<T> operator ++(Complex<T> value) => value + One;

    /// <summary>
    /// Decrements the real part of the complex number by one.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The decremented complex number.</returns>
    public static Complex<T> operator --(Complex<T> value) => value - One;
}