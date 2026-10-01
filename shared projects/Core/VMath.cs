namespace Vorcyc.Mathematics;
using System.Numerics;
using System.Runtime.CompilerServices;
/// <summary>
/// Extra math functions for Offlet and 32-bit floating-point number.
/// </summary>
/// <remarks>
/// <strong><em>Includes :</em></strong>
/// <list type="bullet">
/// <item>Trigonometric Functions.</item>
/// <item>Bit-based operations.</item>
/// <item>32-bit floating-point number constants.</item>
/// <item>Basic statistical functions with sequential and parallel versions.</item>
/// <item>etc.</item>
/// </list>
/// </remarks>
public static partial class VMath
{

    #region GCD or LCM
    /// <summary>
    /// Computes the greatest common divisor (GCD) of two integers using the Euclidean algorithm.
    /// </summary>
    /// <param name="n">The first integer.</param>
    /// <param name="m">The second integer.</param>
    /// <returns>The greatest common divisor of the two integers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Gcd(int n, int m)
    {
        // 继续循环直到余数为零
        while (m != 0)
        {
            // 更新 m 为 n 除以 m 的余数
            m = n % (n = m);
        }
        // 返回最大公约数
        return n;
    }
    /// <summary>
    /// Computes the greatest common divisor (GCD) of two generic integers using the Euclidean algorithm.
    /// </summary>
    /// <typeparam name="T">The generic type, must implement <see cref="IBinaryInteger{T}"/>.</typeparam>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <returns>The greatest common divisor of the two integers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Gcd<T>(this T a, T b) where T : IBinaryInteger<T>
    {
        while (b != T.Zero)
        {
            T temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
    /// <summary>
    /// Computes the highest common factor (HCF) of two integers using recursion.
    /// </summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <returns>The highest common factor of the two integers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Hcf(int a, int b)
    {
        // 如果两个数相等，返回其中一个数
        if (a == b)
        {
            return b;
        }
        // 如果 a 小于 b，递归调用 Hcf(a, b - a)
        if (a < b)
        {
            return Hcf(a, b - a);
        }
        // 否则，递归调用 Hcf(a - b, b)
        return Hcf(a - b, b);
    }

    ////另一种写法
    //public static int f(int a, int b)//最大公约数 
    //{
    //    if (a < b) { a = a + b; b = a - b; a = a - b; }
    //    return (a % b == 0) ? b : f(a % b, b);
    //}
    //Accord.net Tools.cs里边的另一种写法
    ///// <summary>
    /////   Gets the greatest common divisor between two integers.
    ///// </summary>
    ///// 
    ///// <param name="a">First value.</param>
    ///// <param name="b">Second value.</param>
    ///// 
    ///// <returns>The greatest common divisor.</returns>
    ///// 
    //public static int GreatestCommonDivisor(int a, int b)
    //{
    //    int x = a - b * (int)Math.Floor(a / (double)b);
    //    while (x != 0)
    //    {
    //        a = b;
    //        b = x;
    //        x = a - b * (int)Math.Floor(a / (double)b);
    //    }
    //    return b;
    //}
    /// <summary>
    /// Computes the least common multiple (LCM).
    /// The least common multiple of several numbers is the smallest positive number (other than 0) that is a multiple of all of them.
    /// Least Common Multiple 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Lcm(int a, int b)
    {
        return a * b / Hcf(a, b);
    }
    //Fraction
    /// <summary>
    /// Reduces a fraction to its simplest integer ratio.
    /// </summary>
    /// <param name="numerator"></param>
    /// <param name="denominator"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int numerator, int denominator) SimplestIntegerRatioOfFraction(int numerator, int denominator)
    {
        var hcf = Hcf(numerator, denominator);
        return (numerator / hcf, denominator / hcf);
    }
    #endregion

    /// <summary>
    /// Computes the hypotenuse of a right triangle.
    ///   Hypotenuse calculus without overflow/underflow
    /// </summary>
    /// <param name="a">First value</param>
    /// <param name="b">Second value</param>
    /// <returns>The hypotenuse Sqrt(a^2 + b^2)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Hypotenuse(double a, double b)
    {
        double r = 0.0;
        double absA = System.Math.Abs(a);
        double absB = System.Math.Abs(b);
        if (absA > absB)
        {
            r = b / a;
            r = absA * System.Math.Sqrt(1 + r * r);
        }
        else if (b != 0)
        {
            r = a / b;
            r = absB * System.Math.Sqrt(1 + r * r);
        }
        return r;
    }

    /// <summary>
    ///   Hypotenuse calculus without overflow/underflow
    /// </summary>
    /// <param name="a">first value</param>
    /// <param name="b">second value</param>
    /// <returns>The hypotenuse Sqrt(a^2 + b^2)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Hypotenuse(float a, float b)
    {
        float r = 0f;
        float absA = System.Math.Abs(a);
        float absB = System.Math.Abs(b);
        if (absA > absB)
        {
            r = b / a;
            r = absA * MathF.Sqrt(1 + r * r);
        }
        else if (b != 0)
        {
            r = a / b;
            r = absB * MathF.Sqrt(1 + r * r);
        }
        return r;
    }
    /// <summary>
    ///   Gets the proper modulus operation for
    ///   an integer value x and modulo m.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Mod(int x, int m)
    {
        if (m < 0)
            m = -m;
        int r = x % m;
        return r < 0 ? r + m : r;
    }
    /// <summary>
    ///   Gets the proper modulus operation for
    ///   a real value x and modulo m.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Mod(double x, double m)
    {
        if (m < 0)
            m = -m;
        double r = x % m;
        return r < 0 ? r + m : r;
    }
    /// <summary>
    ///   Gets the proper modulus operation for
    ///   a real value x and modulo m.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Mod(float x, float m)
    {
        if (m < 0)
            m = -m;
        float r = x % m;
        return r < 0 ? r + m : r;
    }

    /// <summary>
    ///   Returns the factorial falling power of the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FactorialPower(int value, int degree)
    {
        int t = value;
        for (int i = 0; i < degree; i++)
            t *= degree--;
        return t;
    }
    /// <summary>
    ///   Truncated power function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double TruncatedPower(double value, double degree)
    {
        double x = System.Math.Pow(value, degree);
        return (x > 0) ? x : 0.0;
    }

    /// <summary>
    ///   Fast inverse floating-point square root.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InvSqrt(float f)
    {
        unsafe
        {
            float xhalf = 0.5f * f;
            int i = *(int*)&f;
            i = 0x5f375a86 - (i >> 1);
            f = *(float*)&i;
            f *= (1.5f - xhalf * f * f);
            return f;
        }
    }
    /// <summary>
    ///   Fast inverse floating-point square root.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double InvSqrt(double f)
    {
        unsafe
        {
            double xhalf = 0.5 * f;
            long i = *(long*)&f;
            i = 0x5f375a86 - (i >> 1);
            f = *(double*)&i;
            f *= (1.5 - xhalf * f * f);
            return f;
        }
    }

    /// <summary>
    ///   Gets the maximum value among three values.
    /// </summary>
    /// 
    /// <param name="a">The first value <c>a</c>.</param>
    /// <param name="b">The second value <c>b</c>.</param>
    /// <param name="c">The third value <c>c</c>.</param>
    /// 
    /// <returns>The maximum value among <paramref name="a"/>, 
    ///   <paramref name="b"/> and <paramref name="c"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max(float a, float b, float c)
    {
        if (a > b)
        {
            if (c > a)
                return c;
            return a;
        }
        else
        {
            if (c > b)
                return c;
            return b;
        }
    }
    /// <summary>
    ///   Gets the maximum value among three values.
    /// </summary>
    /// 
    /// <param name="a">The first value <c>a</c>.</param>
    /// <param name="b">The second value <c>b</c>.</param>
    /// <param name="c">The third value <c>c</c>.</param>
    /// 
    /// <returns>The maximum value among <paramref name="a"/>, 
    /// <paramref name="b"/> and <paramref name="c"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Max(double a, double b, double c)
    {
        if (a > b)
        {
            if (c > a)
                return c;
            return a;
        }
        else
        {
            if (c > b)
                return c;
            return b;
        }
    }
    /// <summary>
    ///   Gets the minimum value among three values.
    /// </summary>
    /// 
    /// <param name="a">The first value <c>a</c>.</param>
    /// <param name="b">The second value <c>b</c>.</param>
    /// <param name="c">The third value <c>c</c>.</param>
    /// 
    /// <returns>The minimum value among <paramref name="a"/>, 
    /// <paramref name="b"/> and <paramref name="c"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min(float a, float b, float c)
    {
        if (a < b)
        {
            if (c < a)
                return c;
            return a;
        }
        else
        {
            if (c < b)
                return c;
            return b;
        }
    }
    /// <summary>
    ///   Gets the minimum value among three values.
    /// </summary>
    /// 
    /// <param name="a">The first value <c>a</c>.</param>
    /// <param name="b">The second value <c>b</c>.</param>
    /// <param name="c">The third value <c>c</c>.</param>
    /// 
    /// <returns>The minimum value among <paramref name="a"/>, 
    ///   <paramref name="b"/> and <paramref name="c"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Min(double a, double b, double c)
    {
        if (a < b)
        {
            if (c < a)
                return c;
            return a;
        }
        else
        {
            if (c < b)
                return c;
            return b;
        }
    }
    /// <summary>
    /// Calculates power of 2.
    /// </summary>
    /// 
    /// <param name="power">Power to raise in.</param>
    /// 
    /// <returns>Returns specified power of 2 in the case if power is in the range of
    /// [0, 30]. Otherwise returns 0.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Pow2(int power)
    {
        return ((power >= 0) && (power <= 30)) ? (1 << power) : 0;
    }
    /// <summary>
    /// Get base of binary logarithm.
    /// </summary>
    /// <param name="x">Source integer number.</param>
    /// <returns>Power of the number (base of binary logarithm).</returns>
    /// <remarks>
    /// <para><strong>This is the fastest version.</strong></para>
    /// <para><em>Measured on AMD 5950X: for (int i = 1; i &lt; 50000000; i++) </em></para>
    /// <list type="bullet">
    /// <item><description><para><em> This version took: 00:00:00.4058618 </em></para></description></item>
    /// <item><description><para><em> The second (shift) version took: 00:00:02.1504855 </em></para></description></item>
    /// </list>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Log2(int x)
    {
        if (x <= 65536)
        {
            if (x <= 256)
            {
                if (x <= 16)
                {
                    if (x <= 4)
                    {
                        if (x <= 2)
                        {
                            if (x <= 1)
                                return 0;
                            return 1;
                        }
                        return 2;
                    }
                    if (x <= 8)
                        return 3;
                    return 4;
                }
                if (x <= 64)
                {
                    if (x <= 32)
                        return 5;
                    return 6;
                }
                if (x <= 128)
                    return 7;
                return 8;
            }
            if (x <= 4096)
            {
                if (x <= 1024)
                {
                    if (x <= 512)
                        return 9;
                    return 10;
                }
                if (x <= 2048)
                    return 11;
                return 12;
            }
            if (x <= 16384)
            {
                if (x <= 8192)
                    return 13;
                return 14;
            }
            if (x <= 32768)
                return 15;
            return 16;
        }
        if (x <= 16777216)
        {
            if (x <= 1048576)
            {
                if (x <= 262144)
                {
                    if (x <= 131072)
                        return 17;
                    return 18;
                }
                if (x <= 524288)
                    return 19;
                return 20;
            }
            if (x <= 4194304)
            {
                if (x <= 2097152)
                    return 21;
                return 22;
            }
            if (x <= 8388608)
                return 23;
            return 24;
        }
        if (x <= 268435456)
        {
            if (x <= 67108864)
            {
                if (x <= 33554432)
                    return 25;
                return 26;
            }
            if (x <= 134217728)
                return 27;
            return 28;
        }
        if (x <= 1073741824)
        {
            if (x <= 536870912)
                return 29;
            return 30;
        }
        return 31;
    }

    ///// <summary>  
    ///// Fast integer version of base-2 logarithm
    ///// </summary>
    ///// <param name="x"></param>
    ///// <returns></returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static int Log2_2(int x)
    //{
    //    // Validate parameters
    //    if (x <= 0)
    //    {
    //        // Cannot have the log of 0
    //        throw new Exception("Log2 of zero.");
    //    }
    //    // Get the max index --- x - 1
    //    x--;
    //    int i = 0;
    //    for (i = 0; x != 0; i++)
    //        x >>= 1;
    //    return i;
    //}
    /* Commented-out version removed; keep only the fastest int version, plus a generic version. */
    /// <summary>
    /// Computes the base-2 logarithm of a generic integer.
    /// </summary>
    /// <typeparam name="T">The generic type, must implement <see cref="IBinaryInteger{T}"/>.</typeparam>
    /// <param name="x">The integer to compute the logarithm of.</param>
    /// <returns>The base-2 logarithm of the input value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the input value is less than or equal to zero.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Log2<T>(T x)
        where T : IBinaryInteger<T>
    {
        // Validate parameters
        if (x <= T.Zero)
        {
            // Cannot have the log of 0
            throw new ArgumentOutOfRangeException("Log2 of zero.");
        }
        // Get the max index --- x - 1
        x--;
        T i = T.Zero;
        for (i = T.Zero; x != T.Zero; i++)
            x >>= 1;
        return i;
    }
    #region Factorial
    /// <summary>
    /// Computes the factorial of an integer.
    /// </summary>
    /// <param name="n">The integer to compute the factorial of.</param>
    /// <returns>The factorial of the input integer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when n is negative.</exception>
    /// <remarks>
    /// The factorial is the product of all positive integers less than or equal to n, denoted n!.
    /// For n = 0, the factorial is defined as 1.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Factorial(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Factorial is not defined for negative numbers.");
        }
        if (n == 0)
        {
            return 1;
        }
        return n * Factorial(n - 1);
    }
    /// <summary>
    /// Computes the factorial of a generic integer.
    /// </summary>
    /// <typeparam name="T">The integer type.</typeparam>
    /// <param name="n">The integer to compute the factorial of.</param>
    /// <returns>The factorial of the input integer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when n is negative.</exception>
    /// <remarks>
    /// The factorial is the product of all positive integers less than or equal to n, denoted n!.
    /// For n = 0, the factorial is defined as 1.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Factorial<T>(T n) where T : INumber<T>
    {
        if (n < T.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Factorial is not defined for negative numbers.");
        }
        if (n == T.Zero)
        {
            return T.One;
        }
        return n * Factorial(n - T.One);
    }
    #endregion
    #region Gamma Function
    /// <summary>
    /// Computes the Gamma function using an approximation formula.
    /// </summary>
    /// <param name="x">The input value.</param>
    /// <returns>The value of the Gamma function.</returns>
    /// <remarks>
    /// The Gamma function is an important function in mathematics, widely used in probability theory, statistics, and combinatorics.
    /// It extends the factorial function: for a positive integer n, Gamma(n) = (n-1)!.
    /// This function is computed using the Lanczos approximation, which extends to real and complex numbers on the complex plane.
    /// <para>
    /// Implementation steps:
    /// <list type="number">
    /// <item>A set of constants p is defined for the approximation.</item>
    /// <item>If x is less than 0.5, the reflection formula is used to compute the Gamma value.</item>
    /// <item>Otherwise, the approximation formula is used to compute the Gamma value.</item>
    /// </list>
    /// </para>
    /// This implementation provides high precision over a wide range of values.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Gamma(double x)
    {
        // Computes the Gamma function using an approximation formula
        double[] p =
        [
            0.99999999999980993,
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.13857109526572012,
            9.9843695780195716e-6,
            1.5056327351493116e-7
        ];
        const double TWO_PI = 2 * Math.PI;
        int g = 7;
        if (x < 0.5) return Math.PI / (Math.Sin(Math.PI * x) * Gamma(1 - x));
        x -= 1;
        double a = p[0];
        double t = x + g + 0.5;
        for (int i = 1; i < p.Length; i++)
        {
            a += p[i] / (x + i);
        }
        return Math.Sqrt(TWO_PI) * Math.Pow(t, x + 0.5) * Math.Exp(-t) * a;
    }
    /// <summary>
    /// Computes the natural logarithm of the Gamma function.
    /// </summary>
    /// <param name="x">The input value</param>
    /// <returns>The natural logarithm of the value of the Gamma function</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GammaLog(double x) => Math.Log(Gamma(x));
    /// <summary>
    /// Computes the Gamma function using an approximation formula.
    /// </summary>
    /// <param name="x">The input value.</param>
    /// <returns>The value of the Gamma function.</returns>
    /// <remarks>
    /// The Gamma function is an important function in mathematics, widely used in probability theory, statistics, and combinatorics.
    /// It extends the factorial function: for a positive integer n, Gamma(n) = (n-1)!.
    /// This function is computed using the Lanczos approximation, which extends to real and complex numbers on the complex plane.
    /// <para>
    /// Implementation steps:
    /// <list type="number">
    /// <item>A set of constants p is defined for the approximation.</item>
    /// <item>If x is less than 0.5, the reflection formula is used to compute the Gamma value.</item>
    /// <item>Otherwise, the approximation formula is used to compute the Gamma value.</item>
    /// </list>
    /// </para>
    /// This implementation provides high precision over a wide range of values.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Gamma(float x)
    {
        // Computes the Gamma function using an approximation formula
        float[] p =
        [
            0.99999999999980993f,
            676.5203681218851f,
            -1259.1392167224028f,
            771.32342877765313f,
            -176.61502916214059f,
            12.507343278686905f,
            -0.13857109526572012f,
            9.9843695780195716e-6f,
            1.5056327351493116e-7f
        ];
        int g = 7;
        if (x < 0.5f) return ConstantsFp32.PI / (MathF.Sin(ConstantsFp32.PI * x) * Gamma(1f - x));
        x -= 1;
        float a = p[0];
        float t = x + g + 0.5f;
        for (int i = 1; i < p.Length; i++)
        {
            a += p[i] / (x + i);
        }
        return MathF.Sqrt(ConstantsFp32.TWO_PI) * MathF.Pow(t, x + 0.5f) * MathF.Exp(-t) * a;
    }
    /// <summary>
    /// Computes the natural logarithm of the Gamma function.
    /// </summary>
    /// <param name="x">The input value</param>
    /// <returns>The natural logarithm of the value of the Gamma function</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GammaLog(float x) => MathF.Log(Gamma(x));
    /// <summary>
    /// Computes the Gamma function using an approximation formula.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="x">The input value.</param>
    /// <returns>The value of the Gamma function.</returns>
    /// <remarks>
    /// The Gamma function is an important function in mathematics, widely used in probability theory, statistics, and combinatorics.
    /// It extends the factorial function: for a positive integer n, Gamma(n) = (n-1)!.
    /// This function is computed using the Lanczos approximation, which extends to real and complex numbers on the complex plane.
    /// <para>
    /// Implementation steps:
    /// <list type="number">
    /// <item>A set of constants p is defined for the approximation.</item>
    /// <item>If x is less than 0.5, the reflection formula is used to compute the Gamma value.</item>
    /// <item>Otherwise, the approximation formula is used to compute the Gamma value.</item>
    /// </list>
    /// </para>
    /// This implementation provides high precision over a wide range of values.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Gamma<T>(T x) where T : IFloatingPointIeee754<T>
    {
        // Computes the Gamma function using an approximation formula
        T[] p =
        [
            T.CreateChecked(0.99999999999980993),
            T.CreateChecked(676.5203681218851),
            T.CreateChecked(-1259.1392167224028),
            T.CreateChecked(771.32342877765313),
            T.CreateChecked(-176.61502916214059),
            T.CreateChecked(12.507343278686905),
            T.CreateChecked(-0.13857109526572012),
            T.CreateChecked(9.9843695780195716e-6),
            T.CreateChecked(1.5056327351493116e-7)
        ];
        T g = T.CreateChecked(7);
        T half = T.One / T.CreateChecked(2);
        T one = T.One;
        T pi = T.CreateChecked(Math.PI);
        T twoPi = T.CreateChecked(2 * Math.PI);
        if (x < half)
        {
            return pi / (T.Sin(pi * x) * Gamma(one - x));
        }
        x -= one;
        T a = p[0];
        T t = x + g + half;
        for (int i = 1; i < p.Length; i++)
        {
            a += p[i] / (x + T.CreateChecked(i));
        }
        return T.Sqrt(twoPi) * T.Pow(t, x + half) * T.Exp(-t) * a;
    }
    /// <summary>
    /// Computes the natural logarithm of the Gamma function.
    /// </summary>
    /// <typeparam name="T">The numeric type</typeparam>
    /// <param name="x">The input value</param>
    /// <returns>The natural logarithm of the value of the Gamma function</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GammaLog<T>(T x) where T : IFloatingPointIeee754<T>
    {
        return T.Log(Gamma(x));
    }
    #endregion
    #region Error Function
    /// <summary>
    /// Computes the error function (erf).
    /// </summary>
    /// <typeparam name="T">The generic type, must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <param name="x">The input value.</param>
    /// <returns>The value of the error function.</returns>
    /// <remarks>
    /// The error function (erf) has important applications in statistics and probability theory. It is mainly used to compute the cumulative distribution function (CDF) of the normal distribution.
    /// The error function is defined as:
    /// <code>
    /// erf(x) = (2 / √π) ∫[0, x] e^(-t^2) dt
    /// </code>
    /// where e is the base of the natural logarithm and π is pi.
    /// 
    /// <para>
    /// Main uses of the error function include:
    /// <list type="number">
    /// <item>Cumulative distribution function of the normal distribution: the error function is used to compute the CDF of the standard normal distribution. In a normal distribution, the CDF represents the probability that a random variable is less than or equal to a given value.
    /// Formula: Φ(x) = 0.5 * (1 + erf(x / √2)), where Φ(x) is the CDF of the normal distribution and erf is the error function.
    /// </item>
    /// <item>
    /// Probability computation: the error function is used to compute probability values under the normal distribution. For example, it simplifies computing the probability within a given range.
    /// </item>
    /// <item>
    /// Numerical analysis: the error function is widely used in numerical analysis, especially when dealing with Gaussian integrals and other normal-distribution-related computations.
    /// </item>
    /// <item>
    /// Engineering and physics: in engineering and physics, the error function is used to solve problems involving normal distributions and Gaussian functions, such as signal processing and heat conduction.
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Erf<T>(T x) where T : IFloatingPointIeee754<T>
    {
        // Computes the error function using an approximation formula
        T sign = x < T.Zero ? T.NegativeOne : T.One;
        x = T.Abs(x);
        T a1 = T.CreateChecked(0.254829592);
        T a2 = T.CreateChecked(-0.284496736);
        T a3 = T.CreateChecked(1.421413741);
        T a4 = T.CreateChecked(-1.453152027);
        T a5 = T.CreateChecked(1.061405429);
        T p = T.CreateChecked(0.3275911);
        T t = T.One / (T.One + p * x);
        T y = T.One - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * T.Exp(-x * x);
        return sign * y;
    }
    #endregion
    #region Lower Incomplete Gamma Function
    /// <summary>
    /// Computes the lower incomplete Gamma function using a convergent series.
    /// </summary>
    /// <typeparam name="T">The generic type, must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <param name="s">The shape parameter.</param>
    /// <param name="x">The variable value.</param>
    /// <returns>The value of the lower incomplete Gamma function.</returns>
    /// <remarks>
    /// The lower incomplete Gamma function extends the Gamma function and is defined as:
    /// <code>
    /// γ(s, x) = ∫[0, x] t^(s-1) * e^(-t) dt
    /// </code>
    /// where s is the shape parameter and x is the variable value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T LowerIncompleteGamma<T>(T s, T x) where T : IFloatingPointIeee754<T>
    {
        // γ(s,x) = x^s * e^(-x) * Σ_{k=0..∞} x^k / (s(s+1)...(s+k))
        T term = T.One / s;
        T sum = term;
        for (int k = 1; k < 100; k++)
        {
            term *= x / (s + T.CreateChecked(k));
            sum += term;
            if (T.Abs(term) < T.Abs(sum) * T.CreateChecked(1e-15)) break;
        }
        return T.Pow(x, s) * T.Exp(-x) * sum;
    }
    #endregion
    #region Beta
    /// <summary>
    /// Computes the value of the Beta function.
    /// </summary>
    /// <typeparam name="T">The generic type, must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <param name="alpha">The shape parameter α.</param>
    /// <param name="beta">The shape parameter β.</param>
    /// <returns>The value of the Beta function.</returns>
    /// <remarks>
    /// The Beta function formula is:
    /// <code>
    /// B(α, β) = Γ(α) * Γ(β) / Γ(α + β)
    /// </code>
    /// where Γ is the Gamma function.
    /// The Beta function is widely used in probability theory and statistics, especially when dealing with the Beta distribution.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Beta<T>(T alpha, T beta) where T : IFloatingPointIeee754<T>
    {
        return VMath.Gamma(alpha) * VMath.Gamma(beta) / VMath.Gamma(alpha + beta);
    }
    /// <summary>
    /// Computes the value of the regularized incomplete Beta function.
    /// </summary>
    /// <typeparam name="T">The generic type, must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="alpha">The shape parameter α.</param>
    /// <param name="beta">The shape parameter β.</param>
    /// <returns>The value of the regularized incomplete Beta function.</returns>
    /// <remarks>
    /// The regularized incomplete Beta function formula is:
    /// <code>
    /// I_x(α, β) = (1 / B(α, β)) * ∫[0, x] t^(α-1) * (1-t)^(β-1) dt
    /// </code>
    /// where B(α, β) is the Beta function.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T RegularizedIncompleteBeta<T>(T x, T alpha, T beta) where T : IFloatingPointIeee754<T>
    {
        T bt = (x == T.Zero || x == T.One) ? T.Zero :
            T.Exp(VMath.GammaLog(alpha + beta) - VMath.GammaLog(alpha) - VMath.GammaLog(beta) + alpha * T.Log(x) + beta * T.Log(T.One - x));
        if (x < (alpha + T.One) / (alpha + beta + T.CreateChecked(2)))
        {
            return bt * BetaContinuedFraction(x, alpha, beta) / alpha;
        }
        else
        {
            return T.One - bt * BetaContinuedFraction(T.One - x, beta, alpha) / beta;
        }
    }
    /// <summary>
    /// Computes the Beta function value using a continued-fraction expansion.
    /// </summary>
    /// <typeparam name="T">The generic type, must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="alpha">The shape parameter α.</param>
    /// <param name="beta">The shape parameter β.</param>
    /// <returns>The value of the Beta function.</returns>
    /// <remarks>
    /// Computes the Beta function value using a continued-fraction expansion, with formula:
    /// <code>
    /// B(x; α, β) = ∑[m=0, ∞] (m * (β - m) * x) / ((α + 2m - 1) * (α + 2m))
    /// </code>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BetaContinuedFraction<T>(T x, T alpha, T beta) where T : IFloatingPointIeee754<T>
    {
        int maxIterations = 100;
        T epsilon = T.CreateChecked(3.0e-7);
        T b = T.One - (alpha + beta) * x / (alpha + T.One);
        T c = T.One;
        T d = T.One / b;
        T h = d;
        for (int m = 1; m <= maxIterations; m++)
        {
            int m2 = 2 * m;
            T aa = T.CreateChecked(m) * (beta - T.CreateChecked(m)) * x / ((alpha - T.One + T.CreateChecked(m2)) * (alpha + T.CreateChecked(m2)));
            d = T.One + aa * d;
            if (T.Abs(d) < epsilon) d = epsilon;
            c = T.One + aa / c;
            if (T.Abs(c) < epsilon) c = epsilon;
            d = T.One / d;
            h *= d * c;
            aa = -(alpha + T.CreateChecked(m)) * (alpha + beta + T.CreateChecked(m)) * x / ((alpha + T.CreateChecked(m2)) * (alpha + T.One + T.CreateChecked(m2)));
            d = T.One + aa * d;
            if (T.Abs(d) < epsilon) d = epsilon;
            c = T.One + aa / c;
            if (T.Abs(c) < epsilon) c = epsilon;
            d = T.One / d;
            h *= d * c;
        }
        return h;
    }

    #endregion
    /// <summary>
    ///   Returns the square root of the specified <see cref="decimal"/> number.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Sqrt(decimal x, decimal epsilon = 0.0M)
    {
        if (x < 0)
            throw new OverflowException("Cannot calculate square root from a negative number.");
        decimal current = (decimal)Math.Sqrt((double)x), previous;
        do
        {
            previous = current;
            if (previous == 0.0M) return 0;
            current = (previous + x / previous) / 2;
        }
        while (Math.Abs(previous - current) > epsilon);
        return current;
    }
    /// <summary>
    /// Computes the unit in the last place (ULP) of the input value, i.e., the smallest possible difference between two adjacent floating-point numbers.
    /// </summary>
    /// <param name="value">The double-precision floating-point value to compute the ULP of.</param>
    /// <returns>The unit in the last place (ULP) of the input value.</returns>
    /// <remarks>
    /// This method first converts the input value to a 64-bit integer using <see cref="BitConverter.DoubleToInt64Bits(double)"/>.
    /// It then adds 1 to the integer value to get the next adjacent integer value, and converts it back to a double using <see cref="BitConverter.Int64BitsToDouble(long)"/>.
    /// Finally, it computes the difference between the next value and the input value, and returns it as the ULP of the input value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Ulp(double value)
    {
        var bits = BitConverter.DoubleToInt64Bits(value);
        var nextValue = BitConverter.Int64BitsToDouble(bits + 1);
        var result = nextValue - value;
        return result;
    }
    /// <summary>
    /// Computes the unit in the last place (ULP) of the input value, i.e., the smallest possible difference between two adjacent floating-point numbers.
    /// </summary>
    /// <param name="value">The single-precision floating-point value to compute the ULP of.</param>
    /// <returns>The unit in the last place (ULP) of the input value.</returns>
    /// <remarks>
    /// This method first converts the input value to a 32-bit integer using <see cref="BitConverter.SingleToInt32Bits(float)"/>.
    /// It then adds 1 to the integer value to get the next adjacent integer value, and converts it back to a float using <see cref="BitConverter.Int32BitsToSingle(int)"/>.
    /// Finally, it computes the difference between the next value and the input value, and returns it as the ULP of the input value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Ulp(float value)
    {
        var bits = BitConverter.SingleToInt32Bits(value);
        var nextValue = BitConverter.Int32BitsToSingle(bits + 1);
        var result = nextValue - value;
        return result;
    }
    /// <summary>
    /// Computes the unit in the last place (ULP) of the input value, i.e., the smallest possible difference between two adjacent half-precision floating-point numbers.
    /// </summary>
    /// <param name="value">The half-precision floating-point value to compute the ULP of.</param>
    /// <returns>The unit in the last place (ULP) of the input value.</returns>
    /// <remarks>
    /// This method first converts the input value to a 16-bit integer using <see cref="BitConverter.HalfToInt16Bits(Half)"/>.
    /// It then adds 1 to the integer value to get the next adjacent integer value, and converts it back to a Half using <see cref="BitConverter.Int16BitsToHalf(short)"/>.
    /// Finally, it computes the difference between the next value and the input value, and returns it as the ULP of the input value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Half Ulp(Half value)
    {
        var bits = BitConverter.HalfToInt16Bits(value);
        var nextValue = BitConverter.Int16BitsToHalf((short)(bits + 1));
        var result = nextValue - value;
        return result;
    }
}
