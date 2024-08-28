using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;

namespace Vorcyc.Mathematics.Statistics;

internal static class old_extremeValueFinder
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (float max, float min) FindExtremeValue_Vector128(this Span<float> span)
    {
        if (span.IsEmpty)
            throw new ArgumentException("Span 不能为空", nameof(span));

        if (!Sse.IsSupported)
            throw new PlatformNotSupportedException("SSE is not supported on this platform.");

        var vectorSize = Vector128<float>.Count;
        var maxVector = Vector128.Create(float.MinValue);
        var minVector = Vector128.Create(float.MaxValue);

        int i = 0;
        for (; i <= span.Length - vectorSize; i += vectorSize)
        {
            var currentVector = Vector128.Create(span[i], span[i + 1], span[i + 2], span[i + 3]);
            //var currentVector = Vector128.LoadUnsafe(ref span[i]);
            maxVector = Sse.Max(currentVector, maxVector);
            minVector = Sse.Min(currentVector, minVector);
        }

        // 提取向量中的最大值和最小值
        //float max = maxVector.ToScalar();
        //float min = minVector.ToScalar();
        float max = maxVector[0];
        float min = minVector[0];
        for (int j = 1; j < vectorSize; j++)
        {
            //max = Math.Max(max, maxVector.GetElement(j));
            //min = Math.Min(min, minVector.GetElement(j));   
            max = Math.Max(max, maxVector[j]);
            min = Math.Min(min, minVector[j]);
        }

        // 处理剩余的数据
        for (; i < span.Length; i++)
        {
            max = Math.Max(span[i], max);
            min = Math.Min(span[i], min);
        }

        return (max, min);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (float max, float min) FindExtremeValue_Vector256(this Span<float> span)
    {
        if (span.IsEmpty)
            throw new ArgumentException("Span 不能为空", nameof(span));

        if (!Avx2.IsSupported)
            throw new PlatformNotSupportedException("AVX2 is not supported on this platform.");

        var vectorSize = Vector256<float>.Count;
        var maxVector = Vector256.Create(float.MinValue);
        var minVector = Vector256.Create(float.MaxValue);

        int i = 0;
        for (; i <= span.Length - vectorSize; i += vectorSize)
        {
            var currentVector = Vector256.Create(span[i], span[i + 1], span[i + 2], span[i + 3],
                                                 span[i + 4], span[i + 5], span[i + 6], span[i + 7]);

            //var currentVector = Vector256.LoadUnsafe(ref span[i]);
            maxVector = Avx.Max(currentVector, maxVector);
            minVector = Avx.Min(currentVector, minVector);
        }

        //float max = maxVector.ToScalar();
        //float min = minVector.ToScalar();
        float max = maxVector[0];
        float min = minVector[0];
        for (int j = 1; j < vectorSize; j++)
        {
            //max = Math.Max(max, maxVector.GetElement(j));
            //min = Math.Min(min, minVector.GetElement(j));   
            max = Math.Max(max, maxVector[j]);
            min = Math.Min(min, minVector[j]);
        }
        // 处理剩余的数据
        for (; i < span.Length; i++)
        {
            max = Math.Max(span[i], max);
            min = Math.Min(span[i], min);
        }

        return (max, min);
    }
}
