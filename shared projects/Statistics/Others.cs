//其他统计函数
//28.	协方差 (Covariance): 计算两组数据的协方差。
//29.	相关系数 (Correlation Coefficient): 计算两组数据的相关系数。
//30.	线性回归 (Linear Regression): 实现简单的线性回归分析。

using System.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Provides additional statistical functions, including covariance, correlation coefficient, and linear regression analysis.
/// </summary>
public static class Others
{

    /// <summary>
    /// Computes the covariance of two data sets.
    /// </summary>
    /// <param name="x">The first data set.</param>
    /// <param name="y">The second data set.</param>
    /// <returns>The covariance of the two data sets.</returns>
    /// <remarks>
    /// Covariance: measures the joint variability of two data sets.
    /// A positive covariance indicates that the two data sets change in the same direction, while a negative covariance indicates that they change in opposite directions.
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
    /// Computes the correlation coefficient of two data sets.
    /// </summary>
    /// <param name="x">The first data set.</param>
    /// <param name="y">The second data set.</param>
    /// <returns>The correlation coefficient of the two data sets.</returns>
    /// <remarks>
    /// Correlation Coefficient: measures the linear correlation between two data sets.
    /// The value ranges from -1 to 1; a positive value indicates a positive correlation, a negative value indicates a negative correlation, and 0 indicates no correlation.
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
    /// Performs simple linear regression analysis. For more complete functionality, use <see cref="Vorcyc.Mathematics.MachineLearning.Regression.SimpleLinearRegression{T}"/>.
    /// </summary>
    /// <param name="x">The independent variable data.</param>
    /// <param name="y">The dependent variable data.</param>
    /// <returns>A tuple containing the regression slope and intercept.</returns>
    /// <remarks>
    /// Linear Regression: fits a straight line that minimizes the sum of squared errors between the independent and dependent variables.
    /// The slope represents the effect of the independent variable on the dependent variable, and the intercept represents where the line crosses the y axis.
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
