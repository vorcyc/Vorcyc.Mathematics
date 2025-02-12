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



    #region 最大公约数 或 最小公倍数


    /// <summary>
    /// 使用欧几里得算法计算两个整数的最大公约数（GCD）。
    /// </summary>
    /// <param name="n">第一个整数。</param>
    /// <param name="m">第二个整数。</param>
    /// <returns>两个整数的最大公约数。</returns>
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
    /// 使用欧几里得算法计算两个泛型整数的最大公约数（GCD）。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IBinaryInteger{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="a">第一个整数。</param>
    /// <param name="b">第二个整数。</param>
    /// <returns>两个整数的最大公约数。</returns>
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
    /// 计算两个整数的最大公约数（HCF），使用递归方法。
    /// </summary>
    /// <param name="a">第一个整数。</param>
    /// <param name="b">第二个整数。</param>
    /// <returns>两个整数的最大公约数。</returns>
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
    /// 求最小公倍数.
    /// 几个数共有的倍数叫做这几个数的公倍数，其中除0以外最小的一个公倍数，叫做这几个数的最小公倍数。
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
    /// 取分数的最简整数比
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
    /// 算三角形斜边
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
            Int32 i = *(Int32*)&f;
            i = 0x5f375a86 - (i >> 1);
            f = *(float*)&i;
            f *= (1.5f - xhalf * f * f);
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
    /// <para><strong>这是最快版本的</strong></para>
    /// <para><em>在AMD 5950X 上执行 : for (int i = 1; i &lt; 50000000; i++) </em></para>
    /// <list type="bullet">
    /// <item><description><para><em> 本版本耗时  ：00:00:00.4058618 </em></para></description></item>
    /// <item><description><para><em> 第二个(移位)版本耗时 ：00:00:02.1504855 </em></para></description></item>
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
    ///// 快速整数版本的以2为底的对数
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

    /* 被注释的版本不要了，只要最快的int那个版本，另外再提供一个泛型版本的 */

    /// <summary>
    /// 计算泛型整数的以2为底的对数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IBinaryInteger{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">要计算对数的整数。</param>
    /// <returns>输入值的以2为底的对数。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当输入值小于或等于零时抛出。</exception>
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

    #region 阶乘


    /// <summary>
    /// 计算一个整数的阶乘。
    /// </summary>
    /// <param name="n">要计算阶乘的整数。</param>
    /// <returns>输入整数的阶乘值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当 n 为负数时抛出。</exception>
    /// <remarks>
    /// 阶乘是所有小于或等于 n 的正整数的乘积，记作 n!。
    /// 对于 n = 0，阶乘定义为 1。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Factorial(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "阶乘未定义负数。");
        }

        if (n == 0)
        {
            return 1;
        }
        return n * Factorial(n - 1);
    }

    /// <summary>
    /// 计算一个泛型整数的阶乘。
    /// </summary>
    /// <typeparam name="T">整数类型。</typeparam>
    /// <param name="n">要计算阶乘的整数。</param>
    /// <returns>输入整数的阶乘值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当 n 为负数时抛出。</exception>
    /// <remarks>
    /// 阶乘是所有小于或等于 n 的正整数的乘积，记作 n!。
    /// 对于 n = 0，阶乘定义为 1。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Factorial<T>(T n) where T : INumber<T>
    {
        if (n < T.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "阶乘未定义负数。");
        }

        if (n == T.Zero)
        {
            return T.One;
        }
        return n * Factorial(n - T.One);
    }


    #endregion


    #region gamma函数


    /// <summary>
    /// 使用近似公式计算Gamma函数。
    /// </summary>
    /// <param name="x">输入值。</param>
    /// <returns>Gamma函数的值。</returns>
    /// <remarks>
    /// Gamma函数 (Gamma Function) 是数学中的一个重要函数，广泛应用于概率论、统计学和组合数学等领域。
    /// 它是阶乘函数的扩展，对于正整数 n，Gamma 函数满足 Gamma(n) = (n-1)!。
    /// 该函数使用Lanczos近似公式进行计算，能够在复数平面上对实数和复数进行扩展。
    /// <para>
    /// 具体实现步骤如下：
    /// <list type="number">
    /// <item>定义了一组常数 p，用于近似计算。</item>
    /// <item>如果 x 小于 0.5，使用反射公式计算 Gamma 函数值。</item>
    /// <item>否则，使用近似公式计算 Gamma 函数值。</item>
    /// </list>
    /// </para>
    /// 该实现能够在较大范围内提供高精度的 Gamma 函数值。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Gamma(double x)
    {
        // 使用近似公式计算Gamma函数
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
    /// 计算Gamma函数的自然对数。
    /// </summary>
    /// <param name="x">输入的值</param>
    /// <returns>Gamma函数的值的自然对数</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GammaLog(double x) => Math.Log(Gamma(x));


    /// <summary>
    /// 使用近似公式计算Gamma函数。
    /// </summary>
    /// <param name="x">输入值。</param>
    /// <returns>Gamma函数的值。</returns>
    /// <remarks>
    /// Gamma函数 (Gamma Function) 是数学中的一个重要函数，广泛应用于概率论、统计学和组合数学等领域。
    /// 它是阶乘函数的扩展，对于正整数 n，Gamma 函数满足 Gamma(n) = (n-1)!。
    /// 该函数使用Lanczos近似公式进行计算，能够在复数平面上对实数和复数进行扩展。
    /// <para>
    /// 具体实现步骤如下：
    /// <list type="number">
    /// <item>定义了一组常数 p，用于近似计算。</item>
    /// <item>如果 x 小于 0.5，使用反射公式计算 Gamma 函数值。</item>
    /// <item>否则，使用近似公式计算 Gamma 函数值。</item>
    /// </list>
    /// </para>
    /// 该实现能够在较大范围内提供高精度的 Gamma 函数值。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Gamma(float x)
    {
        // 使用近似公式计算Gamma函数
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
    /// 计算Gamma函数的自然对数。
    /// </summary>
    /// <param name="x">输入的值</param>
    /// <returns>Gamma函数的值的自然对数</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GammaLog(float x) => MathF.Log(Gamma(x));

    /// <summary>
    /// 使用近似公式计算Gamma函数。
    /// </summary>
    /// <typeparam name="T">数值类型。</typeparam>
    /// <param name="x">输入值。</param>
    /// <returns>Gamma函数的值。</returns>
    /// <remarks>
    /// Gamma函数 (Gamma Function) 是数学中的一个重要函数，广泛应用于概率论、统计学和组合数学等领域。
    /// 它是阶乘函数的扩展，对于正整数 n，Gamma 函数满足 Gamma(n) = (n-1)!。
    /// 该函数使用Lanczos近似公式进行计算，能够在复数平面上对实数和复数进行扩展。
    /// <para>
    /// 具体实现步骤如下：
    /// <list type="number">
    /// <item>定义了一组常数 p，用于近似计算。</item>
    /// <item>如果 x 小于 0.5，使用反射公式计算 Gamma 函数值。</item>
    /// <item>否则，使用近似公式计算 Gamma 函数值。</item>
    /// </list>
    /// </para>
    /// 该实现能够在较大范围内提供高精度的 Gamma 函数值。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Gamma<T>(T x) where T : IFloatingPointIeee754<T>
    {
        // 使用近似公式计算Gamma函数
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
    /// 计算Gamma函数的自然对数。
    /// </summary>
    /// <typeparam name="T">数值类型</typeparam>
    /// <param name="x">输入的值</param>
    /// <returns>Gamma函数的值的自然对数</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GammaLog<T>(T x) where T : IFloatingPointIeee754<T>
    {
        return T.Log(Gamma(x));
    }
    #endregion


    #region 误差函数

    /// <summary>
    /// 计算误差函数（Error Function）。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">输入值。</param>
    /// <returns>误差函数的值。</returns>
    /// <remarks>
    /// 误差函数（Error Function，简称 erf）在统计学和概率论中有重要的应用。它主要用于计算正态分布的累积分布函数（CDF）。
    /// 误差函数的定义为：
    /// <code>
    /// erf(x) = (2 / √π) ∫[0, x] e^(-t^2) dt
    /// </code>
    /// 其中，e 是自然对数的底数，π 是圆周率。
    /// 
    /// <para>
    /// 误差函数的主要用途包括：
    /// <list type="number">
    /// <item>正态分布的累积分布函数 ：误差函数用于计算标准正态分布的累积分布函数（CDF）。在正态分布中，累积分布函数表示随机变量小于或等于某个值的概率。
    /// 公式：Φ(x) = 0.5 * (1 + erf(x / √2))，其中 Φ(x) 是正态分布的累积分布函数，erf 是误差函数。
    /// </item>
    /// <item>
    /// 概率计算 ：误差函数用于计算正态分布下的概率值。例如，计算某个范围内的概率时，可以使用误差函数来简化计算。
    /// </item>
    /// <item>
    /// 数值分析 ：误差函数在数值分析中也有广泛应用，特别是在处理高斯积分和其他涉及正态分布的计算时。
    /// </item>
    /// <item>
    /// 工程和物理学 ：在工程和物理学中，误差函数用于解决涉及正态分布和高斯函数的问题，如信号处理、热传导等领域。
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Erf<T>(T x) where T : IFloatingPointIeee754<T>
    {
        // 使用近似公式计算误差函数
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


    #region 不完全Gamma函数

    /// <summary>
    /// 使用近似公式计算下不完全Gamma函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="s">形状参数。</param>
    /// <param name="x">变量值。</param>
    /// <returns>下不完全Gamma函数的值。</returns>
    /// <remarks>
    /// 下不完全Gamma函数（Lower Incomplete Gamma Function）是Gamma函数的扩展，定义为：
    /// <code>
    /// γ(s, x) = ∫[0, x] t^(s-1) * e^(-t) dt
    /// </code>
    /// 其中，s 是形状参数，x 是变量值。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T LowerIncompleteGamma<T>(T s, T x) where T : IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (int k = 0; k < 100; k++)
        {
            sum += T.Pow(x, s + T.CreateChecked(k)) * T.Exp(-x) / VMath.Factorial<T>(T.CreateChecked(k));
        }
        return sum;
    }


    #endregion


    #region Beta

    /// <summary>
    /// 计算 Beta 函数的值。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="alpha">形状参数 α。</param>
    /// <param name="beta">形状参数 β。</param>
    /// <returns>Beta 函数的值。</returns>
    /// <remarks>
    /// Beta 函数的公式为：
    /// <code>
    /// B(α, β) = Γ(α) * Γ(β) / Γ(α + β)
    /// </code>
    /// 其中，Γ 是 Gamma 函数。
    /// Beta 函数在概率论和统计学中有广泛的应用，特别是在处理 Beta 分布时。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Beta<T>(T alpha, T beta) where T : IFloatingPointIeee754<T>
    {
        return VMath.Gamma(alpha) * VMath.Gamma(beta) / VMath.Gamma(alpha + beta);
    }

    /// <summary>
    /// 计算正则化不完全 Beta 函数的值。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">变量值。</param>
    /// <param name="alpha">形状参数 α。</param>
    /// <param name="beta">形状参数 β。</param>
    /// <returns>正则化不完全 Beta 函数的值。</returns>
    /// <remarks>
    /// 正则化不完全 Beta 函数的公式为：
    /// <code>
    /// I_x(α, β) = (1 / B(α, β)) * ∫[0, x] t^(α-1) * (1-t)^(β-1) dt
    /// </code>
    /// 其中，B(α, β) 是 Beta 函数。
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
    /// 使用连分数展开计算 Beta 函数的值。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">变量值。</param>
    /// <param name="alpha">形状参数 α。</param>
    /// <param name="beta">形状参数 β。</param>
    /// <returns>Beta 函数的值。</returns>
    /// <remarks>
    /// 使用连分数展开计算 Beta 函数的值，公式为：
    /// <code>
    /// B(x; α, β) = ∑[m=0, ∞] (m * (β - m) * x) / ((α + 2m - 1) * (α + 2m))
    /// </code>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BetaContinuedFraction<T>(T x, T alpha, T beta) where T : IFloatingPointIeee754<T>
    {
        int maxIterations = 100;
        T epsilon = T.CreateChecked(3.0e-7);
        T a = T.One;
        T b = T.One - (alpha + beta) * x / (alpha + T.One);
        T c = T.One / epsilon;
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
            h *= d * c;
            aa = -(alpha + T.CreateChecked(m)) * (alpha + beta + T.CreateChecked(m)) * x / ((alpha + T.CreateChecked(m2)) * (alpha + T.One + T.CreateChecked(m2)));
            d = T.One + aa * d;
            if (T.Abs(d) < epsilon) d = epsilon;
            c = T.One + aa / c;
            if (T.Abs(c) < epsilon) c = epsilon;
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
    /// 计算输入值的最后一位单位（ULP），即相邻两个浮点数之间可能的最小差异。
    /// </summary>
    /// <param name="value">要计算ULP的双精度浮点数。</param>
    /// <returns>输入值的最后一位单位（ULP）。</returns>
    /// <remarks>
    /// 该方法首先使用 <see cref="BitConverter.DoubleToInt64Bits(double)"/> 方法将输入值转换为长整型整数。
    /// 然后将1添加到整数值以获取下一个相邻的整数值，并使用 <see cref="BitConverter.Int64BitsToDouble(long)"/> 方法将其转换回双精度浮点数。
    /// 最后计算下一个值与输入值之间的差异，并将其作为输入值的ULP返回。
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
    /// 计算输入值的最后一位单位（ULP），即相邻两个浮点数之间可能的最小差异。
    /// </summary>
    /// <param name="value">要计算ULP的单精度浮点数。</param>
    /// <returns>输入值的最后一位单位（ULP）。</returns>
    /// <remarks>
    /// 该方法首先使用 <see cref="BitConverter.SingleToInt32Bits(float)"/> 方法将输入值转换为整型整数。
    /// 然后将1添加到整数值以获取下一个相邻的整数值，并使用 <see cref="BitConverter.Int32BitsToSingle(int)"/> 方法将其转换回单精度浮点数。
    /// 最后计算下一个值与输入值之间的差异，并将其作为输入值的ULP返回。
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
    /// 计算输入值的最后一位单位（ULP），即相邻两个半精度浮点数之间可能的最小差异。
    /// </summary>
    /// <param name="value">要计算ULP的半精度浮点数。</param>
    /// <returns>输入值的最后一位单位（ULP）。</returns>
    /// <remarks>
    /// 该方法首先使用 <see cref="BitConverter.HalfToInt16Bits(Half)"/> 方法将输入值转换为短整型整数。
    /// 然后将1添加到整数值以获取下一个相邻的整数值，并使用 <see cref="BitConverter.Int16BitsToHalf(short)"/> 方法将其转换回半精度浮点数。
    /// 最后计算下一个值与输入值之间的差异，并将其作为输入值的ULP返回。
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
