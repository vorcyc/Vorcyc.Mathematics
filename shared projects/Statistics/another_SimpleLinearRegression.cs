using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace Vorcyc.Mathematics.Statistics;

internal class another_SimpleLinearRegression
{
    public static (float slope, float intercept, float correlationCoefficient) ComputeParameters(Span<float> x, Span<float> y)
    {
        if (x.Length != y.Length || x.Length < 2)
        {
            throw new ArgumentException("x 和 y 的长度必须相等且大于 1。");
        }

        var avgX = x.Average();
        var avgY = y.Average();

        float sumOfSquaresX = 0f;
        float sumOfSquaresY = 0f;
        float sumOfProductsXY = 0f;

        if (Vector.IsHardwareAccelerated)
        {
            var vecAvgX = new Vector<float>(avgX);
            var vecAvgY = new Vector<float>(avgY);
            var vecSumOfSquaresX = Vector<float>.Zero;
            var vecSumOfSquaresY = Vector<float>.Zero;
            var vecSumOfProductsXY = Vector<float>.Zero;

            int i = 0;
            for (; i <= x.Length - Vector<float>.Count; i += Vector<float>.Count)
            {
                var vecX = new Vector<float>(x.Slice(i));
                var vecY = new Vector<float>(y.Slice(i));

                var vecDeltaX = vecX - vecAvgX;
                var vecDeltaY = vecY - vecAvgY;

                vecSumOfSquaresX += vecDeltaX * vecDeltaX;
                vecSumOfSquaresY += vecDeltaY * vecDeltaY;
                vecSumOfProductsXY += vecDeltaX * vecDeltaY;
            }

            sumOfSquaresX = Vector.Dot(vecSumOfSquaresX, Vector<float>.One);
            sumOfSquaresY = Vector.Dot(vecSumOfSquaresY, Vector<float>.One);
            sumOfProductsXY = Vector.Dot(vecSumOfProductsXY, Vector<float>.One);

            for (; i < x.Length; i++)
            {
                var deltaX = x[i] - avgX;
                var deltaY = y[i] - avgY;

                sumOfSquaresX += deltaX * deltaX;
                sumOfSquaresY += deltaY * deltaY;
                sumOfProductsXY += deltaX * deltaY;
            }
        }
        else
        {
            for (int i = 0; i < x.Length; i++)
            {
                var deltaX = x[i] - avgX;
                var deltaY = y[i] - avgY;

                sumOfSquaresX += deltaX * deltaX;
                sumOfSquaresY += deltaY * deltaY;
                sumOfProductsXY += deltaX * deltaY;
            }
        }

        var slope = sumOfProductsXY / sumOfSquaresX;
        var intercept = avgY - slope * avgX;
        var correlationCoefficient = sumOfProductsXY / MathF.Sqrt(sumOfSquaresX * sumOfSquaresY);

        return (slope, intercept, correlationCoefficient);
    }
}
