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
/// 提供各种概率分布的计算方法，包括正态分布、泊松分布、指数分布、二项分布、多项分布、Gamma分布和Beta分布。
/// </summary>
/// <remarks>
/// 该类包含以下概率分布的计算方法：
/// <list type="bullet">
/// <item>
/// <description>正态分布 (Normal Distribution): 计算正态分布的概率密度函数和累积分布函数。</description>
/// </item>
/// <item>
/// <description>泊松分布 (Poisson Distribution): 计算泊松分布的概率质量函数和累积分布函数。</description>
/// </item>
/// <item>
/// <description>指数分布 (Exponential Distribution): 计算指数分布的概率密度函数和累积分布函数。</description>
/// </item>
/// <item>
/// <description>二项分布 (Binomial Distribution): 计算二项分布的概率质量函数和累积分布函数。</description>
/// </item>
/// <item>
/// <description>多项分布 (Multinomial Distribution): 计算多项分布的概率质量函数。</description>
/// </item>
/// <item>
/// <description>Gamma分布 (Gamma Distribution): 计算Gamma分布的概率密度函数和累积分布函数。</description>
/// </item>
/// <item>
/// <description>Beta分布 (Beta Distribution): 计算Beta分布的概率密度函数和累积分布函数。</description>
/// </item>
/// </list>
/// </remarks>
public static class Distribution
{

    #region 正态分布


    /// <summary>
    /// 计算正态分布的概率密度函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">变量值。</param>
    /// <param name="mean">均值。</param>
    /// <param name="stdDev">标准差。</param>
    /// <returns>正态分布的概率密度值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NormalPDF<T>(T x, T mean, T stdDev) where T : IFloatingPointIeee754<T>
    {
        T exponent = T.Exp(-T.CreateChecked(0.5) * T.Pow((x - mean) / stdDev, T.CreateChecked(2)));
        return (T.One / (stdDev * T.Sqrt(T.CreateChecked(2) * T.Pi))) * exponent;
    }

    /// <summary>
    /// 计算正态分布的累积分布函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">变量值。</param>
    /// <param name="mean">均值。</param>
    /// <param name="stdDev">标准差。</param>
    /// <returns>正态分布的累积分布值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NormalCDF<T>(T x, T mean, T stdDev) where T : IFloatingPointIeee754<T>
    {
        return T.CreateChecked(0.5) * (T.One + VMath.Erf((x - mean) / (stdDev * T.Sqrt(T.CreateChecked(2)))));
    }



    #endregion


    #region 泊松分布

    /// <summary>
    /// 计算泊松分布的概率质量函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="k">事件发生的次数。</param>
    /// <param name="lambda">单位时间内事件的平均发生率。</param>
    /// <returns>泊松分布的概率质量值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T PoissonPMF<T>(int k, T lambda) where T : IFloatingPointIeee754<T>
    {
        return (T.Pow(lambda, T.CreateChecked(k)) * T.Exp(-lambda)) / VMath.Factorial<T>(T.CreateChecked(k));
    }

    /// <summary>
    /// 计算泊松分布的累积分布函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="k">事件发生的次数。</param>
    /// <param name="lambda">单位时间内事件的平均发生率。</param>
    /// <returns>泊松分布的累积分布值。</returns>
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


    #region 指数分布

    /// <summary>
    /// 计算指数分布的概率密度函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">变量值。</param>
    /// <param name="lambda">分布的参数。</param>
    /// <returns>指数分布的概率密度值。</returns>
    /// <remarks>
    /// 指数分布（Exponential Distribution）是一种连续概率分布，常用于描述独立事件发生的时间间隔。
    /// 概率密度函数（PDF）的公式为：
    /// <code>
    /// f(x; λ) = λ * e^(-λx)  for x >= 0, λ > 0
    /// </code>
    /// 其中，λ 是分布的参数，表示事件发生的平均速率。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ExponentialPDF<T>(T x, T lambda) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero) return T.Zero;
        return lambda * T.Exp(-lambda * x);
    }

    /// <summary>
    /// 计算指数分布的累积分布函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">变量值。</param>
    /// <param name="lambda">分布的参数。</param>
    /// <returns>指数分布的累积分布值。</returns>
    /// <remarks>
    /// 指数分布（Exponential Distribution）是一种连续概率分布，常用于描述独立事件发生的时间间隔。
    /// 累积分布函数（CDF）的公式为：
    /// <code>
    /// F(x; λ) = 1 - e^(-λx)  for x >= 0, λ > 0
    /// </code>
    /// 其中，λ 是分布的参数，表示事件发生的平均速率。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ExponentialCDF<T>(T x, T lambda) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero) return T.Zero;
        return T.One - T.Exp(-lambda * x);
    }

    #endregion


    #region 二项分布

    /// <summary>
    /// 计算二项分布的概率质量函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="k">成功的次数。</param>
    /// <param name="n">试验的总次数。</param>
    /// <param name="p">每次试验成功的概率。</param>
    /// <returns>二项分布的概率质量值。</returns>
    /// <remarks>
    /// 二项分布（Binomial Distribution）是一种离散概率分布，描述在 n 次独立试验中成功 k 次的概率，每次试验成功的概率为 p。
    /// 概率质量函数（PMF）的公式为：
    /// <code>
    /// P(X = k) = C(n, k) * p^k * (1 - p)^(n - k)
    /// </code>
    /// 其中，C(n, k) 是组合数，表示从 n 个元素中选取 k 个元素的方式数。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BinomialPMF<T>(int k, int n, T p) where T : IFloatingPointIeee754<T>
    {
        return Combinatorics.Combinations<T>(n, k) * T.Pow(p, T.CreateChecked(k)) * T.Pow(T.One - p, T.CreateChecked(n - k));
    }

    /// <summary>
    /// 计算二项分布的累积分布函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="k">成功的次数。</param>
    /// <param name="n">试验的总次数。</param>
    /// <param name="p">每次试验成功的概率。</param>
    /// <returns>二项分布的累积分布值。</returns>
    /// <remarks>
    /// 二项分布（Binomial Distribution）是一种离散概率分布，描述在 n 次独立试验中成功 k 次的概率，每次试验成功的概率为 p。
    /// 累积分布函数（CDF）的公式为：
    /// <code>
    /// F(X &lt;= k) = Σ P(X = i)  for i = 0 to k
    /// </code>
    /// 其中，P(X = i) 是概率质量函数（PMF）。
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


    #region 多项分布

    /// <summary>
    /// 计算多项分布的概率质量函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="counts">每个类别的计数。</param>
    /// <param name="probabilities">每个类别的概率。</param>
    /// <returns>多项分布的概率质量值。</returns>
    /// <remarks>
    /// 多项分布（Multinomial Distribution）是一种离散概率分布，描述在 n 次独立试验中，每个类别出现的次数。
    /// 概率质量函数（PMF）的公式为：
    /// <code>
    /// P(X1 = x1, X2 = x2, ..., Xk = xk) = n! / (x1! * x2! * ... * xk!) * p1^x1 * p2^x2 * ... * pk^xk
    /// </code>
    /// 其中，n 是总试验次数，xi 是第 i 类别出现的次数，pi 是第 i 类别的概率。
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


    #region Gamma分布

    /// <summary>
    /// 计算Gamma分布的概率密度函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">变量值。</param>
    /// <param name="shape">形状参数。</param>
    /// <param name="scale">尺度参数。</param>
    /// <returns>Gamma分布的概率密度值。</returns>
    /// <remarks>
    /// Gamma分布（Gamma Distribution）是一种连续概率分布，常用于描述等待时间。
    /// 概率密度函数（PDF）的公式为：
    /// <code>
    /// f(x; α, β) = (β^α * x^(α - 1) * e^(-βx)) / Γ(α)  for x > 0, α > 0, β > 0
    /// </code>
    /// 其中，α 是形状参数，β 是尺度参数，Γ(α) 是Gamma函数。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GammaPDF<T>(T x, T shape, T scale) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero) return T.Zero;
        return (T.Pow(x, shape - T.One) * T.Exp(-x / scale)) / (T.Pow(scale, shape) * VMath.Gamma(shape));
    }

    /// <summary>
    /// 计算Gamma分布的累积分布函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">变量值。</param>
    /// <param name="shape">形状参数。</param>
    /// <param name="scale">尺度参数。</param>
    /// <returns>Gamma分布的累积分布值。</returns>
    /// <remarks>
    /// Gamma分布（Gamma Distribution）是一种连续概率分布，常用于描述等待时间。
    /// 累积分布函数（CDF）的公式为：
    /// <code>
    /// F(x; α, β) = γ(α, βx) / Γ(α)  for x > 0, α > 0, β > 0
    /// </code>
    /// 其中，α 是形状参数，β 是尺度参数，Γ(α) 是Gamma函数，γ(α, βx) 是下不完全Gamma函数。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GammaCDF<T>(T x, T shape, T scale) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero) return T.Zero;
        return VMath.LowerIncompleteGamma(shape, x / scale) / VMath.Gamma(shape);
    }



    #endregion


    #region Beta分布

    /// <summary>
    /// 计算Beta分布的概率密度函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">变量值。</param>
    /// <param name="alpha">形状参数α。</param>
    /// <param name="beta">形状参数β。</param>
    /// <returns>Beta分布的概率密度值。</returns>
    /// <remarks>
    /// Beta分布（Beta Distribution）是一种连续概率分布，常用于描述概率或比例。
    /// 概率密度函数（PDF）的公式为：
    /// <code>
    /// f(x; α, β) = (x^(α - 1) * (1 - x)^(β - 1)) / B(α, β)  for 0 &lt;= x &lt;= 1, α > 0, β > 0
    /// </code>
    /// 其中，α 和 β 是形状参数，B(α, β) 是Beta函数。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BetaPDF<T>(T x, T alpha, T beta) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero || x > T.One) return T.Zero;
        return (T.Pow(x, alpha - T.One) * T.Pow(T.One - x, beta - T.One)) / VMath.Beta(alpha, beta);
    }

    /// <summary>
    /// 计算Beta分布的累积分布函数。
    /// </summary>
    /// <typeparam name="T">必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口的泛型类型。</typeparam>
    /// <param name="x">变量值。</param>
    /// <param name="alpha">形状参数α。</param>
    /// <param name="beta">形状参数β。</param>
    /// <returns>Beta分布的累积分布值。</returns>
    /// <remarks>
    /// Beta分布（Beta Distribution）是一种连续概率分布，常用于描述概率或比例。
    /// 累积分布函数（CDF）的公式为：
    /// <code>
    /// F(x; α, β) = I_x(α, β)  for 0 &lt;= x &lt;= 1, α > 0, β > 0
    /// </code>
    /// 其中，α 和 β 是形状参数，I_x(α, β) 是正则化不完全Beta函数。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BetaCDF<T>(T x, T alpha, T beta) where T : IFloatingPointIeee754<T>
    {
        if (x < T.Zero || x > T.One) return T.Zero;
        return VMath.RegularizedIncompleteBeta(x, alpha, beta);
    }

    




    #endregion

}
