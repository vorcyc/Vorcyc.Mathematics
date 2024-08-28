using System.Numerics;

namespace Vorcyc.Mathematics.Statistics;

internal class another_SimpleLinearRegression
{

    public static (float slope, float intercept) ComputeParameters(Span<float> x, Span<float> y)
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

        for (; i <= n - simdLength; i += simdLength)
        {
            var vx = new Vector<float>(x.Slice(i, simdLength));
            var vy = new Vector<float>(y.Slice(i, simdLength));

            sumX += vx;
            sumY += vy;
            sumXY += vx * vy;
            sumX2 += vx * vx;
        }

        float totalSumX = 0;
        float totalSumY = 0;
        float totalSumXY = 0;
        float totalSumX2 = 0;

        for (int j = 0; j < simdLength; j++)
        {
            totalSumX += sumX[j];
            totalSumY += sumY[j];
            totalSumXY += sumXY[j];
            totalSumX2 += sumX2[j];
        }

        for (; i < n; i++)
        {
            totalSumX += x[i];
            totalSumY += y[i];
            totalSumXY += x[i] * y[i];
            totalSumX2 += x[i] * x[i];
        }

        float slope = (n * totalSumXY - totalSumX * totalSumY) / (n * totalSumX2 - totalSumX * totalSumX);
        float intercept = (totalSumY - slope * totalSumX) / n;

        return (slope, intercept);
    }

}
