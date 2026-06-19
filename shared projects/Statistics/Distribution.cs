//7.正态分布(Normal Distribution): 计算正态分布的概率密度函数和累积分布函数。
//8.	泊松分布 (Poisson Distribution): 计算泊松分布的概率质量函数和累积分布函数。
//9.	指数分布 (Exponential Distribution): 计算指数分布的概率密度函数和累积分布函数。
//10.	二项分布 (Binomial Distribution): 计算二项分布的概率质量函数和累积分布函数。
//11.	多项分布 (Multinomial Distribution): 计算多项分布的概率质量函数。
//12.	Gamma分布 (Gamma Distribution): 计算Gamma分布的概率密度函数和累积分布函数。
//13.	Beta分布 (Beta Distribution): 计算Beta分布的概率密度函数和累积分布函数。  
namespace Vorcyc.Mathematics.Statistics;

using System.Numerics;

/// <summary>
/// Provides computation methods for various probability distributions, including the normal, Poisson, exponential, binomial, multinomial, Gamma, and Beta distributions.
/// </summary>
/// <remarks>
/// This class contains computation methods for the following probability distributions:
/// <list type="bullet">
/// <item>
/// <description>Normal Distribution: Computes the probability density function and cumulative distribution function of the normal distribution.</description>
/// </item>
/// <item>
/// <description>Poisson Distribution: Computes the probability mass function and cumulative distribution function of the Poisson distribution.</description>
/// </item>
/// <item>
/// <description>Exponential Distribution: Computes the probability density function and cumulative distribution function of the exponential distribution.</description>
/// </item>
/// <item>
/// <description>Binomial Distribution: Computes the probability mass function and cumulative distribution function of the binomial distribution.</description>
/// </item>
/// <item>
/// <description>Multinomial Distribution: Computes the probability mass function of the multinomial distribution.</description>
/// </item>
/// <item>
/// <description>Gamma Distribution: Computes the probability density function and cumulative distribution function of the Gamma distribution.</description>
/// </item>
/// <item>
/// <description>Beta Distribution: Computes the probability density function and cumulative distribution function of the Beta distribution.</description>
/// </item>
/// </list>
/// </remarks>
public static partial class Distribution
{

    #region Normal Distribution

    /// <summary>
    /// Computes the probability density function of the normal distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="mean">The mean.</param>
    /// <param name="stdDev">The standard deviation.</param>
    /// <returns>The probability density value of the normal distribution.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NormalPDF<T>(T x, T mean, T stdDev) where T : IFloatingPointIeee754<T>
    {
        T exponent = T.Exp(-T.CreateChecked(0.5) * T.Pow((x - mean) / stdDev, T.CreateChecked(2)));
        return (T.One / (stdDev * T.Sqrt(T.CreateChecked(2) * T.Pi))) * exponent;
    }
    /// <summary>
    /// Computes the cumulative distribution function of the normal distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="mean">The mean.</param>
    /// <param name="stdDev">The standard deviation.</param>
    /// <returns>The cumulative distribution value of the normal distribution.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NormalCDF<T>(T x, T mean, T stdDev) where T : IFloatingPointIeee754<T>
    {
        return T.CreateChecked(0.5) * (T.One + VMath.Erf((x - mean) / (stdDev * T.Sqrt(T.CreateChecked(2)))));
    }

    #endregion

    #region Poisson Distribution

    /// <summary>
    /// Computes the probability mass function of the Poisson distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="k">The number of times the event occurs.</param>
    /// <param name="lambda">The average rate of event occurrence per unit time.</param>
    /// <returns>The probability mass value of the Poisson distribution.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T PoissonPMF<T>(int k, T lambda) where T : IFloatingPointIeee754<T>
    {
        return (T.Pow(lambda, T.CreateChecked(k)) * T.Exp(-lambda)) / VMath.Factorial<T>(T.CreateChecked(k));
    }

    /// <summary>
    /// Computes the cumulative distribution function of the Poisson distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="k">The number of times the event occurs.</param>
    /// <param name="lambda">The average rate of event occurrence per unit time.</param>
    /// <returns>The cumulative distribution value of the Poisson distribution.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T PoissonCDF<T>(int k, T lambda) where T : IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (int i = 0; i <= k; i++)
        {
            sum += PoissonPMF(i, lambda);
        }
        return sum;
    }

    #endregion

    #region Exponential Distribution
    /// <summary>
    /// Computes the probability density function of the exponential distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="lambda">The parameter of the distribution.</param>
    /// <returns>The probability density value of the exponential distribution.</returns>
    /// <remarks>
    /// The exponential distribution is a continuous probability distribution commonly used to describe the time interval between independent events.
    /// The formula for the probability density function (PDF) is:
    /// <code>
    /// f(x; λ) = λ * e^(-λx)  for x >= 0, λ > 0
    /// </code>
    /// where λ is the parameter of the distribution, representing the average rate of event occurrence.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ExponentialPDF<T>(T x, T lambda) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero) return T.Zero;
        return lambda * T.Exp(-lambda * x);
    }
    /// <summary>
    /// Computes the cumulative distribution function of the exponential distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="lambda">The parameter of the distribution.</param>
    /// <returns>The cumulative distribution value of the exponential distribution.</returns>
    /// <remarks>
    /// The exponential distribution is a continuous probability distribution commonly used to describe the time interval between independent events.
    /// The formula for the cumulative distribution function (CDF) is:
    /// <code>
    /// F(x; λ) = 1 - e^(-λx)  for x >= 0, λ > 0
    /// </code>
    /// where λ is the parameter of the distribution, representing the average rate of event occurrence.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ExponentialCDF<T>(T x, T lambda) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero) return T.Zero;
        return T.One - T.Exp(-lambda * x);
    }
    #endregion

    #region Binomial Distribution
    /// <summary>
    /// Computes the probability mass function of the binomial distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="k">The number of successes.</param>
    /// <param name="n">The total number of trials.</param>
    /// <param name="p">The probability of success in each trial.</param>
    /// <returns>The probability mass value of the binomial distribution.</returns>
    /// <remarks>
    /// The binomial distribution is a discrete probability distribution that describes the probability of achieving k successes in n independent trials, where the probability of success in each trial is p.
    /// The formula for the probability mass function (PMF) is:
    /// <code>
    /// P(X = k) = C(n, k) * p^k * (1 - p)^(n - k)
    /// </code>
    /// where C(n, k) is the binomial coefficient, representing the number of ways to choose k elements from n elements.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BinomialPMF<T>(int k, int n, T p) where T : IFloatingPointIeee754<T>
    {
        return Combinatorics.Combinations<T>(n, k) * T.Pow(p, T.CreateChecked(k)) * T.Pow(T.One - p, T.CreateChecked(n - k));
    }
    /// <summary>
    /// Computes the cumulative distribution function of the binomial distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="k">The number of successes.</param>
    /// <param name="n">The total number of trials.</param>
    /// <param name="p">The probability of success in each trial.</param>
    /// <returns>The cumulative distribution value of the binomial distribution.</returns>
    /// <remarks>
    /// The binomial distribution is a discrete probability distribution that describes the probability of achieving k successes in n independent trials, where the probability of success in each trial is p.
    /// The formula for the cumulative distribution function (CDF) is:
    /// <code>
    /// F(X &lt;= k) = Σ P(X = i)  for i = 0 to k
    /// </code>
    /// where P(X = i) is the probability mass function (PMF).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BinomialCDF<T>(int k, int n, T p) where T : IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (int i = 0; i <= k; i++)
        {
            sum += BinomialPMF(i, n, p);
        }
        return sum;
    }

    #endregion

    #region Multinomial Distribution
    /// <summary>
    /// Computes the probability mass function of the multinomial distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="counts">The count of each category.</param>
    /// <param name="probabilities">The probability of each category.</param>
    /// <returns>The probability mass value of the multinomial distribution.</returns>
    /// <remarks>
    /// The multinomial distribution is a discrete probability distribution that describes the number of occurrences of each category in n independent trials.
    /// The formula for the probability mass function (PMF) is:
    /// <code>
    /// P(X1 = x1, X2 = x2, ..., Xk = xk) = n! / (x1! * x2! * ... * xk!) * p1^x1 * p2^x2 * ... * pk^xk
    /// </code>
    /// where n is the total number of trials, xi is the number of occurrences of the i-th category, and pi is the probability of the i-th category.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T MultinomialPMF<T>(int[] counts, T[] probabilities) where T : IFloatingPointIeee754<T>
    {
        int n = counts.Sum();
        T result = VMath.Factorial<T>(T.CreateChecked(n));
        for (int i = 0; i < counts.Length; i++)
        {
            result *= T.Pow(probabilities[i], T.CreateChecked(counts[i])) / VMath.Factorial<T>(T.CreateChecked(counts[i]));
        }
        return result;
    }

    #endregion

    #region Gamma Distribution
    /// <summary>
    /// Computes the probability density function of the Gamma distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="shape">The shape parameter.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>The probability density value of the Gamma distribution.</returns>
    /// <remarks>
    /// The Gamma distribution is a continuous probability distribution commonly used to describe waiting times.
    /// The formula for the probability density function (PDF) is:
    /// <code>
    /// f(x; α, β) = (β^α * x^(α - 1) * e^(-βx)) / Γ(α)  for x > 0, α > 0, β > 0
    /// </code>
    /// where α is the shape parameter, β is the scale parameter, and Γ(α) is the Gamma function.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GammaPDF<T>(T x, T shape, T scale) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero) return T.Zero;
        return (T.Pow(x, shape - T.One) * T.Exp(-x / scale)) / (T.Pow(scale, shape) * VMath.Gamma(shape));
    }
    /// <summary>
    /// Computes the cumulative distribution function of the Gamma distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="shape">The shape parameter.</param>
    /// <param name="scale">The scale parameter.</param>
    /// <returns>The cumulative distribution value of the Gamma distribution.</returns>
    /// <remarks>
    /// The Gamma distribution is a continuous probability distribution commonly used to describe waiting times.
    /// The formula for the cumulative distribution function (CDF) is:
    /// <code>
    /// F(x; α, β) = γ(α, βx) / Γ(α)  for x > 0, α > 0, β > 0
    /// </code>
    /// where α is the shape parameter, β is the scale parameter, Γ(α) is the Gamma function, and γ(α, βx) is the lower incomplete Gamma function.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GammaCDF<T>(T x, T shape, T scale) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero) return T.Zero;
        return VMath.LowerIncompleteGamma(shape, x / scale) / VMath.Gamma(shape);
    }

    #endregion

    #region Beta Distribution
    /// <summary>
    /// Computes the probability density function of the Beta distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="alpha">The shape parameter α.</param>
    /// <param name="beta">The shape parameter β.</param>
    /// <returns>The probability density value of the Beta distribution.</returns>
    /// <remarks>
    /// The Beta distribution is a continuous probability distribution commonly used to describe probabilities or proportions.
    /// The formula for the probability density function (PDF) is:
    /// <code>
    /// f(x; α, β) = (x^(α - 1) * (1 - x)^(β - 1)) / B(α, β)  for 0 &lt;= x &lt;= 1, α > 0, β > 0
    /// </code>
    /// where α and β are shape parameters, and B(α, β) is the Beta function.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BetaPDF<T>(T x, T alpha, T beta) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero || x > T.One) return T.Zero;
        return (T.Pow(x, alpha - T.One) * T.Pow(T.One - x, beta - T.One)) / VMath.Beta(alpha, beta);
    }
    /// <summary>
    /// Computes the cumulative distribution function of the Beta distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="alpha">The shape parameter α.</param>
    /// <param name="beta">The shape parameter β.</param>
    /// <returns>The cumulative distribution value of the Beta distribution.</returns>
    /// <remarks>
    /// The Beta distribution is a continuous probability distribution commonly used to describe probabilities or proportions.
    /// The formula for the cumulative distribution function (CDF) is:
    /// <code>
    /// F(x; α, β) = I_x(α, β)  for 0 &lt;= x &lt;= 1, α > 0, β > 0
    /// </code>
    /// where α and β are shape parameters, and I_x(α, β) is the regularized incomplete Beta function.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BetaCDF<T>(T x, T alpha, T beta) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero || x > T.One) return T.Zero;
        return VMath.RegularizedIncompleteBeta(x, alpha, beta);
    }

    #endregion

    #region Uniform Distribution

    /// <summary>
    /// Computes the probability density function of the continuous uniform distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="min">The lower bound of the distribution.</param>
    /// <param name="max">The upper bound of the distribution.</param>
    /// <returns>The probability density value of the uniform distribution.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="max"/> is less than or equal to <paramref name="min"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UniformPDF<T>(T x, T min, T max) where T : IFloatingPointIeee754<T>
    {
        if (max <= min)
            throw new ArgumentException("The upper bound must be greater than the lower bound.", nameof(max));
        if (x < min || x > max) return T.Zero;
        return T.One / (max - min);
    }

    /// <summary>
    /// Computes the cumulative distribution function of the continuous uniform distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value.</param>
    /// <param name="min">The lower bound of the distribution.</param>
    /// <param name="max">The upper bound of the distribution.</param>
    /// <returns>The cumulative probability value of the uniform distribution.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="max"/> is less than or equal to <paramref name="min"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UniformCDF<T>(T x, T min, T max) where T : IFloatingPointIeee754<T>
    {
        if (max <= min)
            throw new ArgumentException("The upper bound must be greater than the lower bound.", nameof(max));
        if (x < min) return T.Zero;
        if (x > max) return T.One;
        return (x - min) / (max - min);
    }

    #endregion

    #region Log-Normal Distribution

    /// <summary>
    /// Computes the probability density function of the log-normal distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value. Must be positive; non-positive values return zero density.</param>
    /// <param name="mu">The mean of the variable's natural logarithm.</param>
    /// <param name="sigma">The standard deviation of the variable's natural logarithm. Must be positive.</param>
    /// <returns>The probability density value of the log-normal distribution.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sigma"/> is not positive.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T LogNormalPDF<T>(T x, T mu, T sigma) where T : IFloatingPointIeee754<T>
    {
        if (sigma <= T.Zero)
            throw new ArgumentException("The standard deviation must be positive.", nameof(sigma));
        if (x <= T.Zero) return T.Zero;

        T two = T.CreateChecked(2);
        T logX = T.Log(x);
        T z = (logX - mu) / sigma;
        T coefficient = T.One / (x * sigma * T.Sqrt(two * T.Pi));
        return coefficient * T.Exp(-(z * z) / two);
    }

    /// <summary>
    /// Computes the cumulative distribution function of the log-normal distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value. Must be positive; non-positive values return zero.</param>
    /// <param name="mu">The mean of the variable's natural logarithm.</param>
    /// <param name="sigma">The standard deviation of the variable's natural logarithm. Must be positive.</param>
    /// <returns>The cumulative probability value of the log-normal distribution.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sigma"/> is not positive.</exception>
    /// <remarks>
    /// The log-normal cumulative distribution function equals the normal cumulative distribution
    /// function evaluated at the natural logarithm of <paramref name="x"/>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T LogNormalCDF<T>(T x, T mu, T sigma) where T : IFloatingPointIeee754<T>
    {
        if (sigma <= T.Zero)
            throw new ArgumentException("The standard deviation must be positive.", nameof(sigma));
        if (x <= T.Zero) return T.Zero;
        return NormalCDF(T.Log(x), mu, sigma);
    }

    #endregion

    #region Weibull Distribution

    /// <summary>
    /// Computes the probability density function of the Weibull distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value. Must be non-negative; negative values return zero density.</param>
    /// <param name="shape">The shape parameter (k). Must be positive.</param>
    /// <param name="scale">The scale parameter (lambda). Must be positive.</param>
    /// <returns>The probability density value of the Weibull distribution.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="shape"/> or <paramref name="scale"/> is not positive.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T WeibullPDF<T>(T x, T shape, T scale) where T : IFloatingPointIeee754<T>
    {
        if (shape <= T.Zero)
            throw new ArgumentException("The shape parameter must be positive.", nameof(shape));
        if (scale <= T.Zero)
            throw new ArgumentException("The scale parameter must be positive.", nameof(scale));
        if (x < T.Zero) return T.Zero;

        T ratio = x / scale;
        T powTerm = T.Pow(ratio, shape - T.One);
        return (shape / scale) * powTerm * T.Exp(-T.Pow(ratio, shape));
    }

    /// <summary>
    /// Computes the cumulative distribution function of the Weibull distribution.
    /// </summary>
    /// <typeparam name="T">The generic type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="x">The variable value. Must be non-negative; negative values return zero.</param>
    /// <param name="shape">The shape parameter (k). Must be positive.</param>
    /// <param name="scale">The scale parameter (lambda). Must be positive.</param>
    /// <returns>The cumulative probability value of the Weibull distribution.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="shape"/> or <paramref name="scale"/> is not positive.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T WeibullCDF<T>(T x, T shape, T scale) where T : IFloatingPointIeee754<T>
    {
        if (shape <= T.Zero)
            throw new ArgumentException("The shape parameter must be positive.", nameof(shape));
        if (scale <= T.Zero)
            throw new ArgumentException("The scale parameter must be positive.", nameof(scale));
        if (x < T.Zero) return T.Zero;

        return T.One - T.Exp(-T.Pow(x / scale, shape));
    }

    #endregion
}
