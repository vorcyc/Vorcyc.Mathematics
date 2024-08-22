namespace Vorcyc.Mathematics;

using System.Runtime.CompilerServices;
using static MathF;

/// <summary>
/// 三角函数
/// </summary>
public static class TrigonometryHelper
{

    /// <summary>
    /// 弧度转角度
    /// </summary>
    /// <param name="a"> An angle, measured in radians.</param>
    /// <returns></returns>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RadiansToDegrees(double a)
    {
        return 180.0 * a / Math.PI;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RadiansToDegrees(float radians)
    {
        return (radians * 57.29578f);
    }

    /// <summary>
    /// 角度转弧度
    /// </summary>
    /// <param name="a">An angle, measured in degress.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DegreesToRadians(double a)
    {
        return a * Math.PI / 180.0;
    }


    /// <summary>
    /// 角度转弧度
    /// </summary>
    /// <param name="a">An angle, measured in degress.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DegreesToRadians(float degrees)
    {
        return (degrees * 0.01745329f);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetAngleDifference(float radianAngle1, float radianAngle2)
    {
        radianAngle1 = RadianMin(radianAngle1);
        radianAngle2 = RadianMin(radianAngle2);
        float num = radianAngle1 - radianAngle2;
        if (Abs(num) > 3.1415926535897931f)
        {
            num -= (float)((2 * Math.Sign(num)) * 3.1415926535897931f);
        }
        return num;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RadianMin(float radianAngle)
    {
        if (Math.Abs(radianAngle) > 6.283185f)
        {
            while (Math.Abs(radianAngle) > 3.141593f)
            {
                radianAngle -= Math.Sign(radianAngle) * 6.283185f;
            }
            return radianAngle;
        }
        if (Math.Abs(radianAngle) > 3.141593f)
        {
            radianAngle -= Math.Sign(radianAngle) * 6.283185f;
            if (Math.Abs(radianAngle) > 3.141593f)
            {
                radianAngle -= Math.Sign(radianAngle) * 6.283185f;
            }
        }
        return radianAngle;
    }



    /// <summary>
    ///   Gets the angle formed by the vector [x,y].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Angle(float x, float y)
    {
        if (y >= 0)
        {
            if (x >= 0)
                return Atan2(y, x);
            return ConstantsFp32.PI - Atan(-y / x);
        }
        else
        {
            if (x >= 0)
                return 2.0f * ConstantsFp32.PI - Atan2(-y, x);
            return ConstantsFp32.PI + Atan(y / x);
        }
    }

    /// <summary>
    ///   Gets the angle formed by the vector [x,y].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Angle(double x, double y)
    {
        if (y >= 0)
        {
            if (x >= 0)
                return Math.Atan2(y, x);
            return Math.PI - Math.Atan(-y / x);
        }
        else
        {
            if (x >= 0)
                return 2.0 * Math.PI - Math.Atan2(-y, x);
            return Math.PI + Math.Atan(y / x);
        }
    }




    /// <summary>
    ///   Returns the hyperbolic arc cosine of the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Acosh(double x)
    {
        if (x < 1.0)
            throw new ArgumentOutOfRangeException("x");
        return System.Math.Log(x + System.Math.Sqrt(x * x - 1));
    }


    /// <summary>
    ///   Returns the hyperbolic arc cosine of the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Acosh(float x)
    {
        if (x < 1.0f)
            throw new ArgumentOutOfRangeException("x");
        return Log(x + Sqrt(x * x - 1));
    }


    /// <summary>
    /// Returns the hyperbolic arc sine of the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Asinh(double d)
    {
        double x;
        int sign;

        if (d == 0.0)
            return d;

        if (d < 0.0)
        {
            sign = -1;
            x = -d;
        }
        else
        {
            sign = 1;
            x = d;
        }
        return sign * System.Math.Log(x + System.Math.Sqrt(x * x + 1));
    }


    /// <summary>
    /// Returns the hyperbolic arc sine of the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Asinh(float d)
    {
        float x;
        int sign;

        if (d == 0.0f)
            return d;

        if (d < 0.0f)
        {
            sign = -1;
            x = -d;
        }
        else
        {
            sign = 1;
            x = d;
        }
        return sign * Log(x + Sqrt(x * x + 1));
    }


    /// <summary>
    /// Returns the hyperbolic arc tangent of the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Atanh(double d)
    {
        if (d > 1.0 || d < -1.0)
            throw new ArgumentOutOfRangeException("d");
        return 0.5 * System.Math.Log((1.0 + d) / (1.0 - d));
    }


    /// <summary>
    /// Returns the hyperbolic arc tangent of the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Atanh(float d)
    {
        if (d > 1.0f || d < -1.0f)
            throw new ArgumentOutOfRangeException("d");
        return 0.5f * Log((1.0f + d) / (1.0f - d));
    }


}
