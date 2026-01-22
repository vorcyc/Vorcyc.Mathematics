using System.Numerics;

namespace Vorcyc.Mathematics;

/// SimpleRNG is a simple random number generator based on 
/// George Marsaglia's MWC (multiply with carry) generator.
/// Although it is very simple, it passes Marsaglia's DIEHARD
/// series of random number generator tests.
/// 
/// Written by John D. Cook 
/// http://www.johndcook.com
///

/*
 * 2022.9.4 搬运自 https://github.com/perivar/FindSimilar 的 SimpleRNG.cs 并进行了修改整合
 */

/// <summary>
/// Simple random number generator for 64-bit floating-point numbers.
/// </summary>
public static class SimpleRNG_Fp64
{
    private static uint m_w;
    private static uint m_z;

    static SimpleRNG_Fp64()
    {
        // These values are not magical, just the default values Marsaglia used.
        // Any pair of unsigned integers should be fine.
        m_w = 521288629;
        m_z = 362436069;
    }

    // The random generator seed can be set three ways:
    // 1) specifying two non-zero unsigned integers
    // 2) specifying one non-zero unsigned integer and taking a default value for the second
    // 3) setting the seed from the system time

    /// <summary>
    /// Sets the seed using two unsigned integer values.
    /// </summary>
    /// <param name="u">The first seed value.</param>
    /// <param name="v">The second seed value.</param>
    public static void SetSeed(uint u, uint v)
    {
        if (u != 0) m_w = u;
        if (v != 0) m_z = v;
    }

    /// <summary>
    /// Sets the seed using a single unsigned integer value.
    /// </summary>
    /// <param name="u">The seed value.</param>
    public static void SetSeed(uint u)
    {
        m_w = u;
    }

    /// <summary>
    /// Sets the seed from the current system time.
    /// </summary>
    public static void SetSeedFromSystemTime()
    {
        System.DateTime dt = System.DateTime.Now;
        long x = dt.ToFileTime();
        SetSeed((uint)(x >> 16), (uint)(x % 4294967296));
    }

    // Produce a uniform random sample from the open interval (0, 1).
    // The method will not return either end point.
    /// <summary>
    /// Produces a uniform random sample from the open interval (0, 1).
    /// </summary>
    /// <returns>
    /// A uniformly distributed random <see cref="double"/> greater than 0 and less than 1.
    /// </returns>
    public static double GetUniform()
    {
        // 0 <= u < 2^32
        uint u = GetUint();
        // The magic number below is 1/(2^32 + 2).
        // The result is strictly between 0 and 1.
        return (u + 1.0) * 2.328306435454494e-10;
    }

    // This is the heart of the generator.
    // It uses George Marsaglia's MWC algorithm to produce an unsigned integer.
    // See http://www.bobwheeler.com/statistics/Password/MarsagliaPost.txt
    /// <summary>
    /// Generates the next unsigned integer using Marsaglia's MWC algorithm.
    /// </summary>
    /// <returns>The next pseudo-random unsigned integer.</returns>
    /// <remarks>
    /// See http://www.bobwheeler.com/statistics/Password/MarsagliaPost.txt
    /// </remarks>
    private static uint GetUint()
    {
        m_z = 36969 * (m_z & 65535) + (m_z >> 16);
        m_w = 18000 * (m_w & 65535) + (m_w >> 16);
        return (m_z << 16) + m_w;
    }

    // Get normal (Gaussian) random sample with mean 0 and standard deviation 1
    /// <summary>
    /// Returns a normally distributed random sample with mean 0 and standard deviation 1.
    /// </summary>
    /// <returns>A standard normal random sample.</returns>
    public static double GetNormal()
    {
        // Use Box-Muller algorithm
        double u1 = GetUniform();
        double u2 = GetUniform();
        double r = Math.Sqrt(-2.0 * Math.Log(u1));
        double theta = 2.0 * Math.PI * u2;
        return r * Math.Sin(theta);
    }

    // Get normal (Gaussian) random sample with specified mean and standard deviation
    /// <summary>
    /// Returns a normally distributed random sample with the specified mean and standard deviation.
    /// </summary>
    /// <param name="mean">The mean of the distribution.</param>
    /// <param name="standardDeviation">The standard deviation of the distribution.</param>
    /// <returns>A normally distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="standardDeviation"/> is less than or equal to 0.
    /// </exception>
    public static double GetNormal(double mean, double standardDeviation)
    {
        if (standardDeviation <= 0.0)
        {
            string msg = string.Format("Shape must be positive. Received {0}.", standardDeviation);
            throw new ArgumentOutOfRangeException(msg);
        }
        return mean + standardDeviation * GetNormal();
    }

    // Get exponential random sample with mean 1
    /// <summary>
    /// Returns an exponentially distributed random sample with mean 1.
    /// </summary>
    /// <returns>An exponential random sample.</returns>
    public static double GetExponential()
    {
        return -Math.Log(GetUniform());
    }

    // Get exponential random sample with specified mean
    /// <summary>
    /// Returns an exponentially distributed random sample with the specified mean.
    /// </summary>
    /// <param name="mean">The mean of the distribution.</param>
    /// <returns>An exponential random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="mean"/> is less than or equal to 0.
    /// </exception>
    public static double GetExponential(double mean)
    {
        if (mean <= 0.0)
        {
            string msg = string.Format("Mean must be positive. Received {0}.", mean);
            throw new ArgumentOutOfRangeException(msg);
        }
        return mean * GetExponential();
    }

    /// <summary>
    /// Returns a gamma-distributed random sample with the specified shape and scale.
    /// </summary>
    /// <param name="shape">The shape parameter of the gamma distribution.</param>
    /// <param name="scale">The scale parameter of the gamma distribution.</param>
    /// <returns>A gamma-distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="shape"/> is less than or equal to 0.
    /// </exception>
    /// <remarks>
    /// Implementation based on "A Simple Method for Generating Gamma Variables"
    /// by George Marsaglia and Wai Wan Tsang. ACM Transactions on Mathematical Software,
    /// Vol 26, No 3, September 2000, pages 363-372.
    /// </remarks>
    public static double GetGamma(double shape, double scale)
    {
        // Implementation based on "A Simple Method for Generating Gamma Variables"
        // by George Marsaglia and Wai Wan Tsang.  ACM Transactions on Mathematical Software
        // Vol 26, No 3, September 2000, pages 363-372.

        double d, c, x, xsquared, v, u;

        if (shape >= 1.0)
        {
            d = shape - 1.0 / 3.0;
            c = 1.0 / Math.Sqrt(9.0 * d);
            for (; ; )
            {
                do
                {
                    x = GetNormal();
                    v = 1.0 + c * x;
                }
                while (v <= 0.0);
                v = v * v * v;
                u = GetUniform();
                xsquared = x * x;
                if (u < 1.0 - .0331 * xsquared * xsquared || Math.Log(u) < 0.5 * xsquared + d * (1.0 - v + Math.Log(v)))
                    return scale * d * v;
            }
        }
        else if (shape <= 0.0)
        {
            string msg = string.Format("Shape must be positive. Received {0}.", shape);
            throw new ArgumentOutOfRangeException(msg);
        }
        else
        {
            double g = GetGamma(shape + 1.0, 1.0);
            double w = GetUniform();
            return scale * g * Math.Pow(w, 1.0 / shape);
        }
    }

    /// <summary>
    /// Returns a chi-square distributed random sample with the specified degrees of freedom.
    /// </summary>
    /// <param name="degreesOfFreedom">The number of degrees of freedom.</param>
    /// <returns>A chi-square distributed random sample.</returns>
    public static double GetChiSquare(double degreesOfFreedom)
    {
        // A chi squared distribution with n degrees of freedom
        // is a gamma distribution with shape n/2 and scale 2.
        return GetGamma(0.5 * degreesOfFreedom, 2.0);
    }

    /// <summary>
    /// Returns an inverse gamma-distributed random sample with the specified shape and scale.
    /// </summary>
    /// <param name="shape">The shape parameter.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>An inverse gamma-distributed random sample.</returns>
    public static double GetInverseGamma(double shape, double scale)
    {
        // If X is gamma(shape, scale) then
        // 1/Y is inverse gamma(shape, 1/scale)
        return 1.0 / GetGamma(shape, 1.0 / scale);
    }

    /// <summary>
    /// Returns a Weibull-distributed random sample with the specified shape and scale.
    /// </summary>
    /// <param name="shape">The shape parameter.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>A Weibull-distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="shape"/> or <paramref name="scale"/> is less than or equal to 0.
    /// </exception>
    public static double GetWeibull(double shape, double scale)
    {
        if (shape <= 0.0 || scale <= 0.0)
        {
            string msg = string.Format("Shape and scale parameters must be positive. Recieved shape {0} and scale{1}.", shape, scale);
            throw new ArgumentOutOfRangeException(msg);
        }
        return scale * Math.Pow(-Math.Log(GetUniform()), 1.0 / shape);
    }

    /// <summary>
    /// Returns a Cauchy-distributed random sample with the specified median and scale.
    /// </summary>
    /// <param name="median">The median of the distribution.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>A Cauchy-distributed random sample.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="scale"/> is less than or equal to 0.
    /// </exception>
    public static double GetCauchy(double median, double scale)
    {
        if (scale <= 0)
        {
            string msg = string.Format("Scale must be positive. Received {0}.", scale);
            throw new ArgumentException(msg);
        }

        double p = GetUniform();

        // Apply inverse of the Cauchy distribution function to a uniform
        return median + scale * Math.Tan(Math.PI * (p - 0.5));
    }

    /// <summary>
    /// Returns a Student's t-distributed random sample with the specified degrees of freedom.
    /// </summary>
    /// <param name="degreesOfFreedom">The number of degrees of freedom.</param>
    /// <returns>A Student's t-distributed random sample.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="degreesOfFreedom"/> is less than or equal to 0.
    /// </exception>
    public static double GetStudentT(double degreesOfFreedom)
    {
        if (degreesOfFreedom <= 0)
        {
            string msg = string.Format("Degrees of freedom must be positive. Received {0}.", degreesOfFreedom);
            throw new ArgumentException(msg);
        }

        // See Seminumerical Algorithms by Knuth
        double y1 = GetNormal();
        double y2 = GetChiSquare(degreesOfFreedom);
        return y1 / Math.Sqrt(y2 / degreesOfFreedom);
    }

    // The Laplace distribution is also known as the double exponential distribution.
    /// <summary>
    /// Returns a Laplace-distributed random sample with the specified mean and scale.
    /// </summary>
    /// <param name="mean">The mean of the distribution.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>A Laplace-distributed random sample.</returns>
    public static double GetLaplace(double mean, double scale)
    {
        double u = GetUniform();
        return (u < 0.5) ?
            mean + scale * Math.Log(2.0 * u) :
            mean - scale * Math.Log(2 * (1 - u));
    }

    /// <summary>
    /// Returns a log-normal distributed random sample with the specified parameters.
    /// </summary>
    /// <param name="mu">The mean of the underlying normal distribution.</param>
    /// <param name="sigma">The standard deviation of the underlying normal distribution.</param>
    /// <returns>A log-normal distributed random sample.</returns>
    public static double GetLogNormal(double mu, double sigma)
    {
        return Math.Exp(GetNormal(mu, sigma));
    }

    /// <summary>
    /// Returns a beta-distributed random sample with the specified parameters.
    /// </summary>
    /// <param name="a">The first shape parameter.</param>
    /// <param name="b">The second shape parameter.</param>
    /// <returns>A beta-distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="a"/> or <paramref name="b"/> is less than or equal to 0.
    /// </exception>
    public static double GetBeta(double a, double b)
    {
        if (a <= 0.0 || b <= 0.0)
        {
            string msg = string.Format("Beta parameters must be positive. Received {0} and {1}.", a, b);
            throw new ArgumentOutOfRangeException(msg);
        }

        // There are more efficient methods for generating beta samples.
        // However such methods are a little more efficient and much more complicated.
        // For an explanation of why the following method works, see
        // http://www.johndcook.com/distribution_chart.html#gamma_beta

        double u = GetGamma(a, 1.0);
        double v = GetGamma(b, 1.0);
        return u / (u + v);
    }
}

/// <summary>
/// Simple random number generator for 32-bit floating-point numbers.
/// </summary>
public static class SimpleRNG_Fp32
{
    private static uint m_w;
    private static uint m_z;

    static SimpleRNG_Fp32()
    {
        // These values are not magical, just the default values Marsaglia used.
        // Any pair of unsigned integers should be fine.
        m_w = 521288629;
        m_z = 362436069;
    }

    // The random generator seed can be set three ways:
    // 1) specifying two non-zero unsigned integers
    // 2) specifying one non-zero unsigned integer and taking a default value for the second
    // 3) setting the seed from the system time

    /// <summary>
    /// Sets the seed using two unsigned integer values.
    /// </summary>
    /// <param name="u">The first seed value.</param>
    /// <param name="v">The second seed value.</param>
    public static void SetSeed(uint u, uint v)
    {
        if (u != 0) m_w = u;
        if (v != 0) m_z = v;
    }

    /// <summary>
    /// Sets the seed using a single unsigned integer value.
    /// </summary>
    /// <param name="u">The seed value.</param>
    public static void SetSeed(uint u)
    {
        m_w = u;
    }

    /// <summary>
    /// Sets the seed from the current system time.
    /// </summary>
    public static void SetSeedFromSystemTime()
    {
        System.DateTime dt = System.DateTime.Now;
        long x = dt.ToFileTime();
        SetSeed((uint)(x >> 16), (uint)(x % 4294967296));
    }

    // Produce a uniform random sample from the open interval (0, 1).
    // The method will not return either end point.
    /// <summary>
    /// Produces a uniform random sample from the open interval (0, 1).
    /// </summary>
    /// <returns>
    /// A uniformly distributed random <see cref="float"/> greater than 0 and less than 1.
    /// </returns>
    public static float GetUniform()
    {
        // 0 <= u < 2^32
        uint u = GetUint();
        // The magic number below is 1/(2^32 + 2).
        // The result is strictly between 0 and 1.
        return (u + 1.0f) * 2.328306435454494e-10f;
    }

    // This is the heart of the generator.
    // It uses George Marsaglia's MWC algorithm to produce an unsigned integer.
    // See http://www.bobwheeler.com/statistics/Password/MarsagliaPost.txt
    /// <summary>
    /// Generates the next unsigned integer using Marsaglia's MWC algorithm.
    /// </summary>
    /// <returns>The next pseudo-random unsigned integer.</returns>
    /// <remarks>
    /// See http://www.bobwheeler.com/statistics/Password/MarsagliaPost.txt
    /// </remarks>
    private static uint GetUint()
    {
        m_z = 36969 * (m_z & 65535) + (m_z >> 16);
        m_w = 18000 * (m_w & 65535) + (m_w >> 16);
        return (m_z << 16) + m_w;
    }

    // Get normal (Gaussian) random sample with mean 0 and standard deviation 1
    /// <summary>
    /// Returns a normally distributed random sample with mean 0 and standard deviation 1.
    /// </summary>
    /// <returns>A standard normal random sample.</returns>
    public static float GetNormal()
    {
        // Use Box-Muller algorithm
        var u1 = GetUniform();
        var u2 = GetUniform();
        var r = MathF.Sqrt(-2.0f * MathF.Log(u1));
        float theta = 2.0f * ConstantsFp32.PI * u2;
        return r * MathF.Sin(theta);
    }

    // Get normal (Gaussian) random sample with specified mean and standard deviation
    /// <summary>
    /// Returns a normally distributed random sample with the specified mean and standard deviation.
    /// </summary>
    /// <param name="mean">The mean of the distribution.</param>
    /// <param name="standardDeviation">The standard deviation of the distribution.</param>
    /// <returns>A normally distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="standardDeviation"/> is less than or equal to 0.
    /// </exception>
    public static float GetNormal(float mean, float standardDeviation)
    {
        if (standardDeviation <= 0.0f)
        {
            string msg = string.Format("Shape must be positive. Received {0}.", standardDeviation);
            throw new ArgumentOutOfRangeException(msg);
        }
        return mean + standardDeviation * GetNormal();
    }

    // Get exponential random sample with mean 1
    /// <summary>
    /// Returns an exponentially distributed random sample with mean 1.
    /// </summary>
    /// <returns>An exponential random sample.</returns>
    public static float GetExponential()
    {

        return -MathF.Log(GetUniform());
    }

    // Get exponential random sample with specified mean
    /// <summary>
    /// Returns an exponentially distributed random sample with the specified mean.
    /// </summary>
    /// <param name="mean">The mean of the distribution.</param>
    /// <returns>An exponential random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="mean"/> is less than or equal to 0.
    /// </exception>
    public static float GetExponential(float mean)
    {
        if (mean <= 0.0f)
        {
            string msg = string.Format("Mean must be positive. Received {0}.", mean);
            throw new ArgumentOutOfRangeException(msg);
        }
        return mean * GetExponential();
    }

    /// <summary>
    /// Returns a gamma-distributed random sample with the specified shape and scale.
    /// </summary>
    /// <param name="shape">The shape parameter of the gamma distribution.</param>
    /// <param name="scale">The scale parameter of the gamma distribution.</param>
    /// <returns>A gamma-distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="shape"/> is less than or equal to 0.
    /// </exception>
    /// <remarks>
    /// Implementation based on "A Simple Method for Generating Gamma Variables"
    /// by George Marsaglia and Wai Wan Tsang. ACM Transactions on Mathematical Software,
    /// Vol 26, No 3, September 2000, pages 363-372.
    /// </remarks>
    public static float GetGamma(float shape, float scale)
    {
        // Implementation based on "A Simple Method for Generating Gamma Variables"
        // by George Marsaglia and Wai Wan Tsang.  ACM Transactions on Mathematical Software
        // Vol 26, No 3, September 2000, pages 363-372.

        float d, c, x, xsquared, v, u;

        if (shape >= 1.0f)
        {
            d = shape - 1.0f / 3.0f;
            c = 1.0f / MathF.Sqrt(9.0f * d);
            for (; ; )
            {
                do
                {
                    x = GetNormal();
                    v = 1.0f + c * x;
                }
                while (v <= 0.0f);
                v = v * v * v;
                u = GetUniform();
                xsquared = x * x;
                if (u < 1.0f - .0331f * xsquared * xsquared || MathF.Log(u) < 0.5f * xsquared + d * (1.0f - v + MathF.Log(v)))
                    return scale * d * v;
            }
        }
        else if (shape <= 0.0f)
        {
            string msg = string.Format("Shape must be positive. Received {0}.", shape);
            throw new ArgumentOutOfRangeException(msg);
        }
        else
        {
            float g = GetGamma(shape + 1.0f, 1.0f);
            float w = GetUniform();
            return scale * g * MathF.Pow(w, 1.0f / shape);
        }
    }

    /// <summary>
    /// Returns a chi-square distributed random sample with the specified degrees of freedom.
    /// </summary>
    /// <param name="degreesOfFreedom">The number of degrees of freedom.</param>
    /// <returns>A chi-square distributed random sample.</returns>
    public static float GetChiSquare(float degreesOfFreedom)
    {
        // A chi squared distribution with n degrees of freedom
        // is a gamma distribution with shape n/2 and scale 2.
        return GetGamma(0.5f * degreesOfFreedom, 2.0f);
    }

    /// <summary>
    /// Returns an inverse gamma-distributed random sample with the specified shape and scale.
    /// </summary>
    /// <param name="shape">The shape parameter.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>An inverse gamma-distributed random sample.</returns>
    public static float GetInverseGamma(float shape, float scale)
    {
        // If X is gamma(shape, scale) then
        // 1/Y is inverse gamma(shape, 1/scale)
        return 1.0f / GetGamma(shape, 1.0f / scale);
    }

    /// <summary>
    /// Returns a Weibull-distributed random sample with the specified shape and scale.
    /// </summary>
    /// <param name="shape">The shape parameter.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>A Weibull-distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="shape"/> or <paramref name="scale"/> is less than or equal to 0.
    /// </exception>
    public static float GetWeibull(float shape, float scale)
    {
        if (shape <= 0.0f || scale <= 0.0f)
        {
            string msg = string.Format("Shape and scale parameters must be positive. Recieved shape {0} and scale{1}.", shape, scale);
            throw new ArgumentOutOfRangeException(msg);
        }
        return scale * MathF.Pow(-MathF.Log(GetUniform()), 1.0f / shape);
    }

    /// <summary>
    /// Returns a Cauchy-distributed random sample with the specified median and scale.
    /// </summary>
    /// <param name="median">The median of the distribution.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>A Cauchy-distributed random sample.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="scale"/> is less than or equal to 0.
    /// </exception>
    public static float GetCauchy(float median, float scale)
    {
        if (scale <= 0)
        {
            string msg = string.Format("Scale must be positive. Received {0}.", scale);
            throw new ArgumentException(msg);
        }

        float p = GetUniform();

        // Apply inverse of the Cauchy distribution function to a uniform
        return median + scale * MathF.Tan(ConstantsFp32.PI * (p - 0.5f));
    }

    /// <summary>
    /// Returns a Student's t-distributed random sample with the specified degrees of freedom.
    /// </summary>
    /// <param name="degreesOfFreedom">The number of degrees of freedom.</param>
    /// <returns>A Student's t-distributed random sample.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="degreesOfFreedom"/> is less than or equal to 0.
    /// </exception>
    public static float GetStudentT(float degreesOfFreedom)
    {
        if (degreesOfFreedom <= 0.0f)
        {
            string msg = string.Format("Degrees of freedom must be positive. Received {0}.", degreesOfFreedom);
            throw new ArgumentException(msg);
        }

        // See Seminumerical Algorithms by Knuth
        float y1 = GetNormal();
        float y2 = GetChiSquare(degreesOfFreedom);
        return y1 / MathF.Sqrt(y2 / degreesOfFreedom);
    }

    // The Laplace distribution is also known as the double exponential distribution.
    /// <summary>
    /// Returns a Laplace-distributed random sample with the specified mean and scale.
    /// </summary>
    /// <param name="mean">The mean of the distribution.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>A Laplace-distributed random sample.</returns>
    public static float GetLaplace(float mean, float scale)
    {
        float u = GetUniform();
        return (u < 0.5f) ?
            mean + scale * MathF.Log(2.0f * u) :
            mean - scale * MathF.Log(2 * (1 - u));
    }

    /// <summary>
    /// Returns a log-normal distributed random sample with the specified parameters.
    /// </summary>
    /// <param name="mu">The mean of the underlying normal distribution.</param>
    /// <param name="sigma">The standard deviation of the underlying normal distribution.</param>
    /// <returns>A log-normal distributed random sample.</returns>
    public static float GetLogNormal(float mu, float sigma)
    {
        return MathF.Exp(GetNormal(mu, sigma));
    }

    /// <summary>
    /// Returns a beta-distributed random sample with the specified parameters.
    /// </summary>
    /// <param name="a">The first shape parameter.</param>
    /// <param name="b">The second shape parameter.</param>
    /// <returns>A beta-distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="a"/> or <paramref name="b"/> is less than or equal to 0.
    /// </exception>
    public static float GetBeta(float a, float b)
    {
        if (a <= 0.0 || b <= 0.0)
        {
            string msg = string.Format("Beta parameters must be positive. Received {0} and {1}.", a, b);
            throw new ArgumentOutOfRangeException(msg);
        }

        // There are more efficient methods for generating beta samples.
        // However such methods are a little more efficient and much more complicated.
        // For an explanation of why the following method works, see
        // http://www.johndcook.com/distribution_chart.html#gamma_beta

        float u = GetGamma(a, 1.0f);
        float v = GetGamma(b, 1.0f);
        return u / (u + v);
    }
}

/// <summary>
/// Simple random number generator for floating generic type numbers.
/// </summary>
/// <typeparam name="T">
/// The floating-point type used by this generator. The type must implement <see cref="IFloatingPointIeee754{TSelf}"/>.
/// </typeparam>
public static class RandomNumberGenerator<T>
    where T : IFloatingPointIeee754<T>
{
    private static uint m_w;
    private static uint m_z;

    static RandomNumberGenerator()
    {
        // These values are not magical, just the default values Marsaglia used.
        // Any pair of unsigned integers should be fine.
        m_w = 521288629;
        m_z = 362436069;
    }

    // The random generator seed can be set three ways:
    // 1) specifying two non-zero unsigned integers
    // 2) specifying one non-zero unsigned integer and taking a default value for the second
    // 3) setting the seed from the system time

    /// <summary>
    /// Sets the seed using two unsigned integer values.
    /// </summary>
    /// <param name="u">The first seed value.</param>
    /// <param name="v">The second seed value.</param>
    public static void SetSeed(uint u, uint v)
    {
        if (u != 0) m_w = u;
        if (v != 0) m_z = v;
    }

    /// <summary>
    /// Sets the seed using a single unsigned integer value.
    /// </summary>
    /// <param name="u">The seed value.</param>
    public static void SetSeed(uint u)
    {
        m_w = u;
    }

    /// <summary>
    /// Sets the seed from the current system time.
    /// </summary>
    public static void SetSeedFromSystemTime()
    {
        System.DateTime dt = System.DateTime.Now;
        long x = dt.ToFileTime();
        SetSeed((uint)(x >> 16), (uint)(x % 4294967296));
    }

    /// <summary>
    /// Produces a uniform random sample from the open interval (0, 1).
    /// </summary>
    /// <returns>
    /// A uniformly distributed random value greater than 0 and less than 1.
    /// </returns>
    public static T GetUniform()
    {
        // 0 <= u < 2^32
        uint u = GetUint();
        // The magic number below is 1/(2^32 + 2).
        // The result is strictly between 0 and 1.
        return T.CreateTruncating((u + 1.0) * 2.328306435454494e-10);
    }

    /// <summary>
    /// Generates the next unsigned integer using Marsaglia's MWC algorithm.
    /// </summary>
    /// <returns>The next pseudo-random unsigned integer.</returns>
    /// <remarks>
    /// See http://www.bobwheeler.com/statistics/Password/MarsagliaPost.txt
    /// </remarks>
    private static uint GetUint()
    {
        m_z = 36969 * (m_z & 65535) + (m_z >> 16);
        m_w = 18000 * (m_w & 65535) + (m_w >> 16);
        return (m_z << 16) + m_w;
    }

    /// <summary>
    /// Returns a normally distributed random sample with mean 0 and standard deviation 1.
    /// </summary>
    /// <returns>A standard normal random sample.</returns>
    public static T GetNormal()
    {
        // Use Box-Muller algorithm
        T u1 = GetUniform();
        T u2 = GetUniform();
        var r = T.Sqrt(T.CreateTruncating(-2.0) * T.Log(u1));
        var theta = T.CreateTruncating(2.0) * T.Pi * u2;
        return r * T.Sin(theta);
    }

    // Get normal (Gaussian) random sample with specified mean and standard deviation
    /// <summary>
    /// Returns a normally distributed random sample with the specified mean and standard deviation.
    /// </summary>
    /// <param name="mean">The mean of the distribution.</param>
    /// <param name="standardDeviation">The standard deviation of the distribution.</param>
    /// <returns>A normally distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="standardDeviation"/> is less than or equal to 0.
    /// </exception>
    public static T GetNormal(T mean, T standardDeviation)
    {
        if (standardDeviation <= T.Zero)
        {
            string msg = string.Format("Shape must be positive. Received {0}.", standardDeviation);
            throw new ArgumentOutOfRangeException(msg);
        }
        return mean + standardDeviation * GetNormal();
    }

    /// <summary>
    /// Returns an exponentially distributed random sample with mean 1.
    /// </summary>
    /// <returns>An exponential random sample.</returns>
    public static T GetExponential()
    {
        return -T.Log(GetUniform());
    }

    /// <summary>
    /// Returns an exponentially distributed random sample with the specified mean.
    /// </summary>
    /// <param name="mean">The mean of the distribution.</param>
    /// <returns>An exponential random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="mean"/> is less than or equal to 0.
    /// </exception>
    public static T GetExponential(T mean)
    {
        if (mean <= T.Zero)
        {
            string msg = string.Format("Mean must be positive. Received {0}.", mean);
            throw new ArgumentOutOfRangeException(msg);
        }
        return mean * GetExponential();
    }

    /// <summary>
    /// Returns a gamma-distributed random sample with the specified shape and scale.
    /// </summary>
    /// <param name="shape">The shape parameter of the gamma distribution.</param>
    /// <param name="scale">The scale parameter of the gamma distribution.</param>
    /// <returns>A gamma-distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="shape"/> is less than or equal to 0.
    /// </exception>
    /// <remarks>
    /// Implementation based on "A Simple Method for Generating Gamma Variables"
    /// by George Marsaglia and Wai Wan Tsang. ACM Transactions on Mathematical Software
    /// Vol 26, No 3, September 2000, pages 363-372.
    /// </remarks>
    public static T GetGamma(T shape, T scale)
    {
        // Implementation based on "A Simple Method for Generating Gamma Variables"
        // by George Marsaglia and Wai Wan Tsang.  ACM Transactions on Mathematical Software
        // Vol 26, No 3, September 2000, pages 363-372.

        T d, c, x, xsquared, v, u;

        if (shape >= T.One)
        {
            d = shape - T.CreateTruncating(1.0 / 3.0);
            c = T.One / T.Sqrt(T.CreateTruncating(9.0) * d);
            for (; ; )
            {
                do
                {
                    x = GetNormal();
                    v = T.One + c * x;
                }
                while (v <= T.One);
                v = v * v * v;
                u = GetUniform();
                xsquared = x * x;
                if (u < T.One - T.CreateTruncating(.0331) * xsquared * xsquared || T.Log(u) < T.CreateTruncating(0.5) * xsquared + d * (T.One - v + T.Log(v)))
                    return scale * d * v;
            }
        }
        else if (shape <= T.Zero)
        {
            string msg = string.Format("Shape must be positive. Received {0}.", shape);
            throw new ArgumentOutOfRangeException(msg);
        }
        else
        {
            T g = GetGamma(shape + T.One, T.One);
            T w = GetUniform();
            return scale * g * T.Pow(w, T.One / shape);
        }
    }

    /// <summary>
    /// Returns a chi-square distributed random sample with the specified degrees of freedom.
    /// </summary>
    /// <param name="degreesOfFreedom">The number of degrees of freedom.</param>
    /// <returns>A chi-square distributed random sample.</returns>
    public static T GetChiSquare(T degreesOfFreedom)
    {
        // A chi squared distribution with n degrees of freedom
        // is a gamma distribution with shape n/2 and scale 2.
        return GetGamma(T.CreateTruncating(0.5) * degreesOfFreedom, T.CreateTruncating(2.0));
    }

    /// <summary>
    /// Returns an inverse gamma-distributed random sample with the specified shape and scale.
    /// </summary>
    /// <param name="shape">The shape parameter.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>An inverse gamma-distributed random sample.</returns>
    public static T GetInverseGamma(T shape, T scale)
    {
        // If X is gamma(shape, scale) then
        // 1/Y is inverse gamma(shape, 1/scale)
        return T.One / GetGamma(shape, T.One / scale);
    }

    /// <summary>
    /// Returns a Weibull-distributed random sample with the specified shape and scale.
    /// </summary>
    /// <param name="shape">The shape parameter.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>A Weibull-distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="shape"/> or <paramref name="scale"/> is less than or equal to 0.
    /// </exception>
    public static T GetWeibull(T shape, T scale)
    {
        if (shape <= T.Zero || scale <= T.Zero)
        {
            string msg = string.Format("Shape and scale parameters must be positive. Recieved shape {0} and scale{1}.", shape, scale);
            throw new ArgumentOutOfRangeException(msg);
        }
        return scale * T.Pow(-T.Log(GetUniform()), T.One / shape);
    }

    /// <summary>
    /// Returns a Cauchy-distributed random sample with the specified median and scale.
    /// </summary>
    /// <param name="median">The median of the distribution.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>A Cauchy-distributed random sample.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="scale"/> is less than or equal to 0.
    /// </exception>
    public static T GetCauchy(T median, T scale)
    {
        if (scale <= T.Zero)
        {
            string msg = string.Format("Scale must be positive. Received {0}.", scale);
            throw new ArgumentException(msg);
        }

        T p = GetUniform();

        // Apply inverse of the Cauchy distribution function to a uniform
        return median + scale * T.Tan(T.Pi * (p - T.CreateTruncating(0.5)));
    }

    /// <summary>
    /// Returns a Student's t-distributed random sample with the specified degrees of freedom.
    /// </summary>
    /// <param name="degreesOfFreedom">The number of degrees of freedom.</param>
    /// <returns>A Student's t-distributed random sample.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="degreesOfFreedom"/> is less than or equal to 0.
    /// </exception>
    public static T GetStudentT(T degreesOfFreedom)
    {
        if (degreesOfFreedom <= T.Zero)
        {
            string msg = string.Format("Degrees of freedom must be positive. Received {0}.", degreesOfFreedom);
            throw new ArgumentException(msg);
        }

        // See Seminumerical Algorithms by Knuth
        T y1 = GetNormal();
        T y2 = GetChiSquare(degreesOfFreedom);
        return y1 / T.Sqrt(y2 / degreesOfFreedom);
    }

    /// <summary>
    /// Returns a Laplace-distributed random sample with the specified mean and scale.
    /// </summary>
    /// <param name="mean">The mean of the distribution.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>A Laplace-distributed random sample.</returns>
    public static T GetLaplace(T mean, T scale)
    {
        T u = GetUniform();
        return (u < T.CreateTruncating(0.5)) ?
            mean + scale * T.Log(T.CreateTruncating(2.0) * u) :
            mean - scale * T.Log(T.CreateTruncating(2) * (T.One - u));
    }

    /// <summary>
    /// Returns a log-normal distributed random sample with the specified parameters.
    /// </summary>
    /// <param name="mu">The mean of the underlying normal distribution.</param>
    /// <param name="sigma">The standard deviation of the underlying normal distribution.</param>
    /// <returns>A log-normal distributed random sample.</returns>
    public static T GetLogNormal(T mu, T sigma)
    {
        return T.Exp(GetNormal(mu, sigma));
    }

    /// <summary>
    /// Returns a beta-distributed random sample with the specified parameters.
    /// </summary>
    /// <param name="a">The first shape parameter.</param>
    /// <param name="b">The second shape parameter.</param>
    /// <returns>A beta-distributed random sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="a"/> or <paramref name="b"/> is invalid.
    /// </exception>
    public static T GetBeta(T a, T b)
    {
        if (a <= T.One || b <= T.One)
        {
            string msg = string.Format("Beta parameters must be positive. Received {0} and {1}.", a, b);
            throw new ArgumentOutOfRangeException(msg);
        }

        // There are more efficient methods for generating beta samples.
        // However such methods are a little more efficient and much more complicated.
        // For an explanation of why the following method works, see
        // http://www.johndcook.com/distribution_chart.html#gamma_beta

        var u = GetGamma(a, T.One);
        var v = GetGamma(b, T.One);
        return u / (u + v);
    }
}