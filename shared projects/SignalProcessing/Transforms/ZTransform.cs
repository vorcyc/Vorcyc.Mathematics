using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms;

public static class ZTransform
{

    public static ComplexFp32[] Transform(float[] input, int numPoints)
    {
        ComplexFp32[] result = new ComplexFp32[numPoints];
        for (int k = 0; k < numPoints; k++)
        {
            float angle = -2f * ConstantsFp32.PI * k / numPoints;
            ComplexFp32 zk = new ComplexFp32(MathF.Cos(angle), MathF.Sin(angle));
            ComplexFp32 sum = ComplexFp32.Zero;

            for (int n = 0; n < input.Length; n++)
            {
                sum += input[n] * ComplexFp32.Pow(zk, -n);
            }
            result[k] = sum;
        }
        return result;
    }

    /// <summary>
    /// 频率响应
    /// </summary>
    /// <param name="b"></param>
    /// <param name="a"></param>
    /// <param name="omega"></param>
    /// <returns></returns>
    /// <remarks>
    /// 通过 Z 变换，可以得到系统的频率响应，这反映了系统对不同频率输入的响应。
    /// </remarks>
    public static ComplexFp32 EvaluateFrequencyResponse(float[] b, float[] a, float omega)
    {
        ComplexFp32 z = ComplexFp32.Exp(ComplexFp32.ImaginaryOne * omega);
        ComplexFp32 numerator = ComplexFp32.Zero;
        ComplexFp32 denominator = ComplexFp32.Zero;

        for (int i = 0; i < b.Length; i++)
        {
            numerator += b[i] * ComplexFp32.Pow(z, -i);
        }
        for (int i = 0; i < a.Length; i++)
        {
            denominator += a[i] * ComplexFp32.Pow(z, -i);
        }
        return numerator / denominator;
    }

    /// <summary>
    /// 极点和零点
    /// </summary>
    /// <param name="zTransform"></param>
    /// <returns></returns>
    /// <remarks>
    /// 通过 Z 变换，可以得到系统的极点和零点，帮助我们分析系统的稳定性。
    /// </remarks>
    public static (ComplexFp32[] poles, ComplexFp32[] zeros) GetPolesAndZeros(ComplexFp32[] zTransform)
    {
        int degree = zTransform.Length - 1;
        ComplexFp32[] poles = new ComplexFp32[degree];
        ComplexFp32[] zeros = new ComplexFp32[degree];

        // 计算极点和零点
        for (int i = 0; i < degree; i++)
        {
            poles[i] = zTransform[i] != 0 ? 1 / zTransform[i] : ComplexFp32.Zero;
            zeros[i] = zTransform[i] == 0 ? ComplexFp32.One : ComplexFp32.Zero;
        }
        return (poles, zeros);


    }































    public static Complex<T>[] Transform<T>(T[] input, int numPoints)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var result = new Complex<T>[numPoints];
        for (int k = 0; k < numPoints; k++)
        {
            T angle = -Constants<T>.Two * Constants<T>.Pi * T.CreateChecked(k / numPoints);
            var zk = new Complex<T>(T.Cos(angle), T.Sin(angle));
            var sum = Complex<T>.Zero;

            for (int n = 0; n < input.Length; n++)
            {
                sum += input[n] * Complex<T>.Pow(zk, -n);
            }
            result[k] = sum;
        }
        return result;
    }

}
