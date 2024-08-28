using System.Numerics;

namespace Vorcyc.Mathematics.Statistics;

internal class another_SimpleLinearRegression
{
    /// <summary>
    /// 计算线性回归模型的斜率、截距和相关系数。
    /// </summary>
    /// <param name="x">自变量的值。</param>
    /// <param name="y">因变量的值。</param>
    /// <returns>返回一个包含斜率、截距和相关系数的元组。</returns>
    /// <exception cref="ArgumentException">当 x 和 y 的长度不相等或小于 2 时抛出。</exception>
    public static (float slope, float intercept, float correlationCoefficient) ComputeParameters(Span<float> x, Span<float> y)
    {
        if (x.Length != y.Length || x.Length < 2)
        {
            throw new ArgumentException("x 和 y 的长度必须相等且大于 1。");
        }

        int n = x.Length;
        int simdLength = Vector<float>.Count;
        int i = 0;

        Vector<float> sumX = Vector<float>.Zero;
        Vector<float> sumY = Vector<float>.Zero;
        Vector<float> sumXY = Vector<float>.Zero;
        Vector<float> sumX2 = Vector<float>.Zero;
        Vector<float> sumY2 = Vector<float>.Zero;

        for (; i <= n - simdLength; i += simdLength)
        {
            var vx = new Vector<float>(x.Slice(i, simdLength));
            var vy = new Vector<float>(y.Slice(i, simdLength));

            sumX += vx;
            sumY += vy;
            sumXY += vx * vy;
            sumX2 += vx * vx;
            sumY2 += vy * vy;
        }

        float totalSumX = 0;
        float totalSumY = 0;
        float totalSumXY = 0;
        float totalSumX2 = 0;
        float totalSumY2 = 0;

        for (int j = 0; j < simdLength; j++)
        {
            totalSumX += sumX[j];
            totalSumY += sumY[j];
            totalSumXY += sumXY[j];
            totalSumX2 += sumX2[j];
            totalSumY2 += sumY2[j];
        }

        for (; i < n; i++)
        {
            totalSumX += x[i];
            totalSumY += y[i];
            totalSumXY += x[i] * y[i];
            totalSumX2 += x[i] * x[i];
            totalSumY2 += y[i] * y[i];
        }

        float avgX = totalSumX / n;
        float avgY = totalSumY / n;

        float sumOfSquaresX = totalSumX2 - n * avgX * avgX;
        float sumOfSquaresY = totalSumY2 - n * avgY * avgY;
        float sumOfProductsXY = totalSumXY - n * avgX * avgY;

        float slope = sumOfProductsXY / sumOfSquaresX;
        float intercept = avgY - slope * avgX;
        float correlationCoefficient = sumOfProductsXY / MathF.Sqrt(sumOfSquaresX * sumOfSquaresY);

        return (slope, intercept, correlationCoefficient);
    }
}
