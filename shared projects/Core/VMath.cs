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




    /// <summary>  
    /// 快速整数版本的以2为底的对数
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Log2_2(int x)
    {
        // Validate parameters
        if (x <= 0)
        {
            // Cannot have the log of 0
            throw new Exception("Log2 of zero.");
        }

        // Get the max index --- x - 1
        x--;
        int i = 0;
        for (i = 0; x != 0; i++)
            x >>= 1;
        return i;
    }


    /// <summary>
    /// 求阶乘
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Factorial(int n)
    {
        int num = n;
        if (num == 0)
        {
            return 1;
        }
        return (num * Factorial(num - 1));
    }




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
