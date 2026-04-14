namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using System.Runtime.InteropServices;

/// <summary>
/// SIMD-accelerated helpers for batch normalization.
/// </summary>
internal static class BatchNormMath
{
    public static void ComputeMeanAndVariance<T>(ReadOnlySpan<T> values, out T mean, out T variance)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        if (values.Length == 0)
        {
            mean = T.Zero;
            variance = T.Zero;
            return;
        }

        mean = SumSimd(values) / T.CreateTruncating(values.Length);

        T sumSq = T.Zero;
        int vectorSize = Vector<T>.Count;
        int i = 0;
        var meanVec = new Vector<T>(mean);
        if (vectorSize > 1)
        {
            var vAcc = Vector<T>.Zero;
            for (; i <= values.Length - vectorSize; i += vectorSize)
            {
                var diff = new Vector<T>(values.Slice(i, vectorSize)) - meanVec;
                vAcc += diff * diff;
            }

            for (int j = 0; j < vectorSize; j++)
            {
                sumSq += vAcc[j];
            }
        }

        for (; i < values.Length; i++)
        {
            var diff = values[i] - mean;
            sumSq += diff * diff;
        }

        variance = sumSq / T.CreateTruncating(values.Length);
    }

    public static void NormalizeScaleShift<T>(
        ReadOnlySpan<T> input,
        Span<T> normalized,
        Span<T> output,
        T mean,
        T invStd,
        T scale,
        T shift)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        int vectorSize = Vector<T>.Count;
        int i = 0;
        var meanVec = new Vector<T>(mean);
        var invStdVec = new Vector<T>(invStd);
        var scaleVec = new Vector<T>(scale);
        var shiftVec = new Vector<T>(shift);

        if (vectorSize > 1)
        {
            for (; i <= input.Length - vectorSize; i += vectorSize)
            {
                var inVec = new Vector<T>(input.Slice(i, vectorSize));
                var normVec = (inVec - meanVec) * invStdVec;
                normVec.CopyTo(normalized.Slice(i));
                (normVec * scaleVec + shiftVec).CopyTo(output.Slice(i));
            }
        }

        for (; i < input.Length; i++)
        {
            var norm = (input[i] - mean) * invStd;
            normalized[i] = norm;
            output[i] = norm * scale + shift;
        }
    }

    public static T SumSimd<T>(ReadOnlySpan<T> values)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        int vectorSize = Vector<T>.Count;
        int i = 0;
        if (vectorSize > 1)
        {
            var vSum = Vector<T>.Zero;
            for (; i <= values.Length - vectorSize; i += vectorSize)
            {
                vSum += new Vector<T>(values.Slice(i, vectorSize));
            }

            for (int j = 0; j < vectorSize; j++)
            {
                sum += vSum[j];
            }
        }

        for (; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    public static void AccumulateDotSimd<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, ref T accumulator)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        int vectorSize = Vector<T>.Count;
        int i = 0;
        if (vectorSize > 1)
        {
            var vAcc = Vector<T>.Zero;
            for (; i <= a.Length - vectorSize; i += vectorSize)
            {
                vAcc += new Vector<T>(a.Slice(i, vectorSize)) * new Vector<T>(b.Slice(i, vectorSize));
            }

            for (int j = 0; j < vectorSize; j++)
            {
                accumulator += vAcc[j];
            }
        }

        for (; i < a.Length; i++)
        {
            accumulator += a[i] * b[i];
        }
    }
}
