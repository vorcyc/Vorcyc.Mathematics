//其他统计函数
//28.	协方差 (Covariance): 计算两组数据的协方差。
//29.	相关系数 (Correlation Coefficient): 计算两组数据的相关系数。
//30.	线性回归 (Linear Regression): 实现简单的线性回归分析。

using System.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// 提供其他统计函数，包括协方差、相关系数和线性回归分析。
/// </summary
public static class Others
{
    /// <summary>
    /// 计算两组数据的协方差。
    /// </summary>
    /// <param name="x">第一组数据。</param>
    /// <param name="y">第二组数据。</param>
    /// <returns>两组数据的协方差。</returns>
    /// <remarks>
    /// 协方差 (Covariance): 衡量两组数据的联合变异性。
    /// 正协方差表示两组数据同向变化，负协方差表示两组数据反向变化。
    /// </remarks>
    public static T Covariance<T>(this Span<T> x, Span<T> y)
        where T : INumber<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        T meanX = x.Average();
        T meanY = y.Average();
        T covariance = T.Zero;

        int vectorSize = Vector<T>.Count;
        int i = 0;

        // 使用 SIMD 进行并行计算
        Vector<T> meanXVector = new Vector<T>(meanX);
        Vector<T> meanYVector = new Vector<T>(meanY);
        Vector<T> covarianceVector = Vector<T>.Zero;

        for (; i <= x.Length - vectorSize; i += vectorSize)
        {
            Vector<T> xVector = new Vector<T>(x.Slice(i, vectorSize));
            Vector<T> yVector = new Vector<T>(y.Slice(i, vectorSize));
            covarianceVector += (xVector - meanXVector) * (yVector - meanYVector);
        }

        covariance = Vector.Dot(covarianceVector, Vector<T>.One);

        // 处理剩余的元素
        for (; i < x.Length; i++)
        {
            covariance += (x[i] - meanX) * (y[i] - meanY);
        }

        return covariance / T.CreateChecked(x.Length);
    }

    /// <summary>
    /// 计算两组数据的相关系数。
    /// </summary>
    /// <param name="x">第一组数据。</param>
    /// <param name="y">第二组数据。</param>
    /// <returns>两组数据的相关系数。</returns>
    /// <remarks>
    /// 相关系数 (Correlation Coefficient): 衡量两组数据的线性相关性。
    /// 相关系数的取值范围为 -1 到 1，正相关系数表示正相关，负相关系数表示负相关，0 表示无相关性。
    /// </remarks>
    public static T CorrelationCoefficient<T>(this Span<T> x, Span<T> y)
        where T : IFloatingPointIeee754<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        T covariance = Covariance(x, y);
        //T stdDevX = T.Sqrt(x.Sum(val => T.Pow(val - x.Average(), T.CreateChecked(2))) / x.Length);
        //T stdDevY = T.Sqrt(y.Sum(val => T.Pow(val - y.Average(), T.CreateChecked(2))) / y.Length);
        T stdDevX = x.StandardDeviation();
        T stdDevY = y.StandardDeviation();

        return covariance / (stdDevX * stdDevY);
    }

    /// <summary>
    /// 实现简单的线性回归分析。如果需要更完整的功能，请使用<see cref="Vorcyc.Mathematics.MachineLearning.SimpleLinearRegression{T}"/>.
    /// </summary>
    /// <param name="x">自变量数据。</param>
    /// <param name="y">因变量数据。</param>
    /// <returns>包含回归系数和截距的元组。</returns>
    /// <remarks>
    /// 线性回归 (Linear Regression): 用于拟合一条直线，使得自变量和因变量之间的误差平方和最小。
    /// 回归系数表示自变量对因变量的影响，截距表示直线在 y 轴上的截距。
    /// </remarks>
    public static (T Slope, T Intercept) LinearRegression<T>(this Span<T> x, Span<T> y)
        where T : IFloatingPointIeee754<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        T meanX = x.Average();
        T meanY = y.Average();
        T numerator = T.Zero;
        T denominator = T.Zero;

        int vectorSize = Vector<T>.Count;
        int i = 0;

        // 使用 SIMD 进行并行计算
        Vector<T> meanXVector = new Vector<T>(meanX);
        Vector<T> meanYVector = new Vector<T>(meanY);
        Vector<T> numeratorVector = Vector<T>.Zero;
        Vector<T> denominatorVector = Vector<T>.Zero;

        for (; i <= x.Length - vectorSize; i += vectorSize)
        {
            Vector<T> xVector = new Vector<T>(x.Slice(i, vectorSize));
            Vector<T> yVector = new Vector<T>(y.Slice(i, vectorSize));
            numeratorVector += (xVector - meanXVector) * (yVector - meanYVector);
            denominatorVector += Vector.Multiply(xVector - meanXVector, xVector - meanXVector);
        }

        numerator = Vector.Dot(numeratorVector, Vector<T>.One);
        denominator = Vector.Dot(denominatorVector, Vector<T>.One);

        // 处理剩余的元素
        for (; i < x.Length; i++)
        {
            numerator += (x[i] - meanX) * (y[i] - meanY);
            denominator += T.Pow(x[i] - meanX, T.CreateChecked(2));
        }

        T slope = numerator / denominator;
        T intercept = meanY - slope * meanX;

        return (slope, intercept);
    }
}



internal static class Others2
{

    /// <summary>
    /// 计算两组数据的协方差。
    /// </summary>
    /// <param name="x">第一组数据。</param>
    /// <param name="y">第二组数据。</param>
    /// <returns>两组数据的协方差。</returns>
    /// <remarks>
    /// 协方差 (Covariance): 衡量两组数据的联合变异性。
    /// 正协方差表示两组数据同向变化，负协方差表示两组数据反向变化。
    /// </remarks>
    public static double Covariance(double[] x, double[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        double meanX = x.Average();
        double meanY = y.Average();
        double covariance = 0.0;

        for (int i = 0; i < x.Length; i++)
        {
            covariance += (x[i] - meanX) * (y[i] - meanY);
        }

        return covariance / x.Length;
    }   
    
    
    public static float Covariance(float[] x, float[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        float meanX = x.AsSpan().Average();
        float meanY = y.AsSpan().Average();
        float covariance = 0.0f;

        for (int i = 0; i < x.Length; i++)
        {
            covariance += (x[i] - meanX) * (y[i] - meanY);
        }

        return covariance / x.Length;
    }

    /// <summary>
    /// 计算两组数据的相关系数。
    /// </summary>
    /// <param name="x">第一组数据。</param>
    /// <param name="y">第二组数据。</param>
    /// <returns>两组数据的相关系数。</returns>
    /// <remarks>
    /// 相关系数 (Correlation Coefficient): 衡量两组数据的线性相关性。
    /// 相关系数的取值范围为 -1 到 1，正相关系数表示正相关，负相关系数表示负相关，0 表示无相关性。
    /// </remarks>
    public static double CorrelationCoefficient(double[] x, double[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        double covariance = Covariance(x, y);
        double stdDevX = Math.Sqrt(x.Sum(val => Math.Pow(val - x.Average(), 2)) / x.Length);
        double stdDevY = Math.Sqrt(y.Sum(val => Math.Pow(val - y.Average(), 2)) / y.Length);

        return covariance / (stdDevX * stdDevY);
    }   
    
    public static float CorrelationCoefficient(float[] x, float[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        float covariance = Covariance(x, y);
        float stdDevX = MathF.Sqrt(x.Sum(val => MathF.Pow(val - x.Average(), 2)) / x.Length);
        float stdDevY = MathF.Sqrt(y.Sum(val => MathF.Pow(val - y.Average(), 2)) / y.Length);

        return covariance / (stdDevX * stdDevY);
    }

    /// <summary>
    /// 实现简单的线性回归分析。
    /// </summary>
    /// <param name="x">自变量数据。</param>
    /// <param name="y">因变量数据。</param>
    /// <returns>包含回归系数和截距的元组。</returns>
    /// <remarks>
    /// 线性回归 (Linear Regression): 用于拟合一条直线，使得自变量和因变量之间的误差平方和最小。
    /// 回归系数表示自变量对因变量的影响，截距表示直线在 y 轴上的截距。
    /// </remarks>
    public static (double Slope, double Intercept) LinearRegression(double[] x, double[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        double meanX = x.Average();
        double meanY = y.Average();
        double numerator = 0.0;
        double denominator = 0.0;

        for (int i = 0; i < x.Length; i++)
        {
            numerator += (x[i] - meanX) * (y[i] - meanY);
            denominator += Math.Pow(x[i] - meanX, 2);
        }

        double slope = numerator / denominator;
        double intercept = meanY - slope * meanX;

        return (slope, intercept);
    }  
    
    
    public static (float Slope, float Intercept) LinearRegression(float[] x, float[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        float meanX = x.Average();
        float meanY = y.Average();
        float numerator = 0.0f;
        float denominator = 0.0f;

        for (int i = 0; i < x.Length; i++)
        {
            numerator += (x[i] - meanX) * (y[i] - meanY);
            denominator += MathF.Pow(x[i] - meanX, 2);
        }

        float slope = numerator / denominator;
        float intercept = meanY - slope * meanX;

        return (slope, intercept);
    }


}


internal static class Others3
{
    /// <summary>
    /// 计算两组数据的协方差。
    /// </summary>
    /// <param name="x">第一组数据。</param>
    /// <param name="y">第二组数据。</param>
    /// <returns>两组数据的协方差。</returns>
    /// <remarks>
    /// 协方差 (Covariance): 衡量两组数据的联合变异性。
    /// 正协方差表示两组数据同向变化，负协方差表示两组数据反向变化。
    /// </remarks>
    public static double Covariance(double[] x, double[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        double meanX = 0.0;
        double meanY = 0.0;
        for (int i = 0; i < x.Length; i++)
        {
            meanX += x[i];
            meanY += y[i];
        }
        meanX /= x.Length;
        meanY /= y.Length;

        double covariance = 0.0;
        for (int i = 0; i < x.Length; i++)
        {
            covariance += (x[i] - meanX) * (y[i] - meanY);
        }

        return covariance / x.Length;
    }

    /// <summary>
    /// 计算两组数据的相关系数。
    /// </summary>
    /// <param name="x">第一组数据。</param>
    /// <param name="y">第二组数据。</param>
    /// <returns>两组数据的相关系数。</returns>
    /// <remarks>
    /// 相关系数 (Correlation Coefficient): 衡量两组数据的线性相关性。
    /// 相关系数的取值范围为 -1 到 1，正相关系数表示正相关，负相关系数表示负相关，0 表示无相关性。
    /// </remarks>
    public static double CorrelationCoefficient(double[] x, double[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        double covariance = Covariance(x, y);

        double meanX = 0.0;
        double meanY = 0.0;
        for (int i = 0; i < x.Length; i++)
        {
            meanX += x[i];
            meanY += y[i];
        }
        meanX /= x.Length;
        meanY /= y.Length;

        double stdDevX = 0.0;
        double stdDevY = 0.0;
        for (int i = 0; i < x.Length; i++)
        {
            stdDevX += Math.Pow(x[i] - meanX, 2);
            stdDevY += Math.Pow(y[i] - meanY, 2);
        }
        stdDevX = Math.Sqrt(stdDevX / x.Length);
        stdDevY = Math.Sqrt(stdDevY / y.Length);

        return covariance / (stdDevX * stdDevY);
    }

    /// <summary>
    /// 实现简单的线性回归分析。
    /// </summary>
    /// <param name="x">自变量数据。</param>
    /// <param name="y">因变量数据。</param>
    /// <returns>包含回归系数和截距的元组。</returns>
    /// <remarks>
    /// 线性回归 (Linear Regression): 用于拟合一条直线，使得自变量和因变量之间的误差平方和最小。
    /// 回归系数表示自变量对因变量的影响，截距表示直线在 y 轴上的截距。
    /// </remarks>
    public static (double Slope, double Intercept) LinearRegression(double[] x, double[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two arrays must be equal.");

        double meanX = 0.0;
        double meanY = 0.0;
        for (int i = 0; i < x.Length; i++)
        {
            meanX += x[i];
            meanY += y[i];
        }
        meanX /= x.Length;
        meanY /= y.Length;

        double numerator = 0.0;
        double denominator = 0.0;
        for (int i = 0; i < x.Length; i++)
        {
            numerator += (x[i] - meanX) * (y[i] - meanY);
            denominator += Math.Pow(x[i] - meanX, 2);
        }

        double slope = numerator / denominator;
        double intercept = meanY - slope * meanX;

        return (slope, intercept);
    }
}