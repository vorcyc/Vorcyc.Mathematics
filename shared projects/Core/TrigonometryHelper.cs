namespace Vorcyc.Mathematics;

using System.Numerics;
using System.Runtime.CompilerServices;
using static MathF;

/// <summary>
/// Provides helper methods for trigonometric operations.
/// </summary>
public static class TrigonometryHelper
{
    /// <summary>
    /// Converts an angle from radians to degrees.
    /// </summary>
    /// <param name="a">An angle, measured in radians.</param>
    /// <returns>The angle in degrees.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RadiansToDegrees(double a)
    {
        return 180.0 * a / Math.PI;
    }

    /// <summary>
    /// Converts an angle from radians to degrees.
    /// </summary>
    /// <param name="radians">An angle, measured in radians.</param>
    /// <returns>The angle in degrees.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RadiansToDegrees(float radians)
    {
        return (radians * 57.29578f);
    }

    /// <summary>
    /// Converts an angle from degrees to radians.
    /// </summary>
    /// <param name="a">An angle, measured in degrees.</param>
    /// <returns>The angle in radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DegreesToRadians(double a)
    {
        return a * Math.PI / 180.0;
    }

    /// <summary>
    /// Converts an angle from degrees to radians.
    /// </summary>
    /// <param name="degrees">An angle, measured in degrees.</param>
    /// <returns>The angle in radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DegreesToRadians(float degrees)
    {
        return (degrees * 0.01745329f);
    }

    /// <summary>
    /// Gets the difference between two angles, measured in radians.
    /// </summary>
    /// <param name="radianAngle1">The first angle, measured in radians.</param>
    /// <param name="radianAngle2">The second angle, measured in radians.</param>
    /// <returns>The difference between the two angles, measured in radians.</returns>
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

    /// <summary>
    /// Reduces an angle to its equivalent between -π and π.
    /// </summary>
    /// <param name="radianAngle">An angle, measured in radians.</param>
    /// <returns>The equivalent angle between -π and π.</returns>
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
    /// Reduces an angle to its equivalent between -π and π.
    /// </summary>
    /// <param name="radianAngle">An angle, measured in radians.</param>
    /// <returns>The equivalent angle between -π and π.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RadianMin(double radianAngle)
    {
        if (Math.Abs(radianAngle) > 6.283185)
        {
            while (Math.Abs(radianAngle) > 3.141593)
            {
                radianAngle -= Math.Sign(radianAngle) * 6.283185;
            }
            return radianAngle;
        }
        if (Math.Abs(radianAngle) > 3.141593f)
        {
            radianAngle -= Math.Sign(radianAngle) * 6.283185;
            if (Math.Abs(radianAngle) > 3.141593)
            {
                radianAngle -= Math.Sign(radianAngle) * 6.283185;
            }
        }
        return radianAngle;
    }




    /// <summary>
    /// Gets the angle formed by the vector [x,y].
    /// </summary>
    /// <param name="x">The x-coordinate of the vector.</param>
    /// <param name="y">The y-coordinate of the vector.</param>
    /// <returns>The angle formed by the vector, measured in radians.</returns>
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
    /// Gets the angle formed by the vector [x,y].
    /// </summary>
    /// <param name="x">The x-coordinate of the vector.</param>
    /// <param name="y">The y-coordinate of the vector.</param>
    /// <returns>The angle formed by the vector, measured in radians.</returns>
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
    /// Returns the hyperbolic arc cosine of the specified value.
    /// </summary>
    /// <param name="x">A value.</param>
    /// <returns>The hyperbolic arc cosine of the specified value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Acosh(double x)
    {
        if (x < 1.0)
            throw new ArgumentOutOfRangeException("x");
        return System.Math.Log(x + System.Math.Sqrt(x * x - 1));
    }

    /// <summary>
    /// Returns the hyperbolic arc cosine of the specified value.
    /// </summary>
    /// <param name="x">A value.</param>
    /// <returns>The hyperbolic arc cosine of the specified value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Acosh(float x)
    {
        if (x < 1.0f)
            throw new ArgumentOutOfRangeException("x");
        return Log(x + Sqrt(x * x - 1));
    }

   

    /// <summary>
    /// Returns the hyperbolic arc tangent of the specified value.
    /// </summary>
    /// <param name="d">A value.</param>
    /// <returns>The hyperbolic arc tangent of the specified value.</returns>
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
    /// <param name="d">A value.</param>
    /// <returns>The hyperbolic arc tangent of the specified value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Atanh(float d)
    {
        if (d > 1.0f || d < -1.0f)
            throw new ArgumentOutOfRangeException("d");
        return 0.5f * Log((1.0f + d) / (1.0f - d));
    }


    #region Sinc

    /// <summary>
    /// Returns Sinc of <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Sinc(double x)
    {
        return Math.Abs(x) > 1e-20 ? Math.Sin(Math.PI * x) / (Math.PI * x) : 1.0;
    }


    /// <summary>
    /// Returns Sinc of <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sinc(float x)
    {
        return MathF.Abs(x) > 1e-20 ? MathF.Sin(ConstantsFp32.PI * x) / (ConstantsFp32.PI * x) : 1.0f;
    }


    /// <summary>
    /// Returns Sinc of <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sinc<T>(T x)
        where T : IFloatingPointIeee754<T>
    {
        return T.Abs(x) > T.CreateTruncating(1e-20) ? T.Sin(T.Pi * x) / (T.Pi * x) : T.One;
    }

    #endregion


    #region Asinh


    ///// <summary>
    ///// Computes Inverse Sinh of <paramref name="x"/>.
    ///// </summary>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static double Asinh(double x)
    //{
    //    return Math.Log(x + Math.Sqrt(x * x + 1));
    //}


    ///// <summary>
    ///// Computes Inverse Sinh of <paramref name="x"/>.
    ///// </summary>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static float Asinh(float x)
    //{
    //    return MathF.Log(x + MathF.Sqrt(x * x + 1));
    //}

    /// <summary>
    /// Returns the hyperbolic arc sine of the specified value.
    /// </summary>
    /// <param name="d">A value.</param>
    /// <returns>The hyperbolic arc sine of the specified value.</returns>
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
    /// <param name="d">A value.</param>
    /// <returns>The hyperbolic arc sine of the specified value.</returns>
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
    /// Computes Inverse Sinh of <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Asinh<T>(T x)
        where T : struct, IFloatingPointIeee754<T>
    {
        return T.Log(x + T.Sqrt(x * x + T.One));
    }

    #endregion

}
