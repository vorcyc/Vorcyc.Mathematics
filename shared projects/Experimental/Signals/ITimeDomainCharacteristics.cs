using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// Defines an interface for accessing key time-domain characteristics of a signal, such as period, amplitude, power,
/// energy, root mean square (RMS), zero-crossing rate, and entropy. Provides methods and properties to analyze the
/// statistical and structural features of a signal in the time domain.
/// </summary>
/// <remarks>This interface is intended for use with signal processing applications that require extraction of
/// fundamental time-domain features. Implementations should ensure that property values are computed according to
/// standard definitions for each characteristic. All properties are read-only and represent precomputed or
/// on-demand-calculated metrics for the underlying signal data. Thread safety and performance considerations depend on
/// the specific implementation.</remarks>
public interface ITimeDomainCharacteristics
{
    /// <summary>
    /// Gets the period of the signal. For discrete sampled signals, this can be viewed as the reciprocal
    /// of the sampling rate or the reciprocal of the fundamental frequency, depending on the specific definition.
    /// Used to describe the periodicity of the signal in the time domain.
    /// </summary>
    float Period { get; }

    /// <summary>
    /// Gets the amplitude of the signal, defined as the difference between the maximum and minimum sample values.
    /// Reflects the overall amplitude range of the signal in the time domain.
    /// </summary>
    float Amplitude { get; }

    /// <summary>
    /// Gets the total power of the signal, calculated as the sum of squares of all samples.
    /// Suitable for evaluating the overall energy intensity of the signal (unnormalized).
    /// </summary>
    float TotalPower { get; }

    /// <summary>
    /// Gets the average power of the signal, calculated as the sum of squared samples divided by the number of samples.
    /// Represents the average energy intensity per sample.
    /// </summary>
    float AveragePower { get; }

    /// <summary>
    /// Gets the total energy of the signal, calculated identically to total power (sum of squared samples).
    /// For finite-length time series, total energy is equivalent to total power.
    /// </summary>
    float TotalEnergy { get; }

    /// <summary>
    /// Gets the average energy of the signal, calculated as the mean of squared sample values (sum of squares / number of samples).
    /// </summary>
    float AverageEnergy { get; }

    /// <summary>
    /// Gets the Root Mean Square (RMS), defined as the square root of the average energy.
    /// Commonly used to measure the effective amplitude of a signal.
    /// </summary>
    float Rms { get; }

    /// <summary>
    /// Gets the Zero-Crossing Rate (ZCR), representing the frequency of sign changes between adjacent samples.
    /// The value ranges from [0, 1]; a higher value indicates more frequent zero crossings.
    /// </summary>
    float ZeroCrossingRate { get; }

    /// <summary>
    /// Gets the normalized Shannon entropy of the signal, used to measure the complexity or randomness of the amplitude distribution.
    /// The value ranges from [0, 1]; a higher value indicates a more uniform distribution and greater randomness.
    /// </summary>
    float Entropy { get; }

    /// <summary>
    /// Calculates the Shannon entropy of the data using the specified number of bins.
    /// </summary>
    /// <param name="binCount">The number of bins used to partition the data for entropy calculation; must be greater than 0.</param>
    /// <returns>A floating-point value representing the calculated entropy. Returns 0 when the data is empty or all values fall into a single bin.</returns>
    float GetEntropy(int binCount = 32);



    #region Amplitude

    /// <summary>
    /// Gets the amplitude of the signal (difference between the maximum and minimum values).
    /// </summary>
    /// <remarks>This is the baseline version, used to verify the correctness of other optimized versions.</remarks>
    internal static float GetAmplitude_Normal(Span<float> samples)
    {
        var max = samples.Max();
        var min = samples.Min();
        return max - min;
    }

    /// <summary>
    /// Gets the amplitude of the signal (difference between the maximum and minimum values).
    /// </summary>
    internal static float GetAmplitude_SIMD(Span<float> samples)
    {
        var (max, min) = ExtremeValueFinder.FindExtremeValue(samples);
        return max - min;
    }

    #endregion


    #region Power

    /// <summary>
    /// Gets the total power of the signal (sum of squared samples).
    /// </summary>
    /// <remarks>This is the baseline version, used to verify the correctness of other optimized versions.</remarks>
    internal static float GetTotalPower_Normal(ReadOnlySpan<float> samples)
    {
        float sumOfSquares = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            sumOfSquares += samples[i] * samples[i];
        }
        return sumOfSquares;
    }

    /// <summary>
    /// Gets the average power of the signal (mean of squared samples).
    /// </summary>
    /// <param name="samples"></param>
    /// <returns></returns>
    /// <remarks>This is the baseline version, used to verify the correctness of other optimized versions.</remarks>
    internal static float GetAveragePower_Normal(ReadOnlySpan<float> samples) => GetTotalPower_Normal(samples) / samples.Length;

    /// <summary>
    /// Gets the total power of the signal (sum of squared samples) using SIMD optimization.
    /// </summary>
    internal static float GetTotalPower_SIMD(ReadOnlySpan<float> samples)
    {
        float sumOfSquares = 0;
        int vectorSize = Vector<float>.Count;
        int i = 0;

        // 使用SIMD进行并行计算
        for (; i <= samples.Length - vectorSize; i += vectorSize)
        {
            var vector = new Vector<float>(samples.Slice(i, vectorSize));
            sumOfSquares += Vector.Dot(vector, vector);
        }

        // 处理剩余的元素
        for (; i < samples.Length; i++)
        {
            sumOfSquares += samples[i] * samples[i];
        }

        return sumOfSquares;
    }


    internal static float GetAveragePower_SIMD(ReadOnlySpan<float> samples) => GetTotalPower_SIMD(samples) / samples.Length;


    #endregion


    #region Energy

    /// <summary>
    /// Gets the total energy of the signal (sum of squared samples).
    /// </summary>
    /// <remarks>This is the baseline version, used to verify the correctness of other optimized versions.</remarks>
    internal static float GetTotalEnergy_Normal(ReadOnlySpan<float> samples)
    {
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }
        return sum;
    }

    /// <summary>
    /// Gets the average energy of the signal (mean of squared samples).
    /// </summary>
    /// <remarks>This is the baseline version, used to verify the correctness of other optimized versions.</remarks>
    internal static float GetAverageEnergy_Normal(ReadOnlySpan<float> samples) => GetTotalEnergy_Normal(samples) / samples.Length;

    /// <summary>
    /// Gets the total energy of the signal (sum of squared samples) using SIMD optimization.
    /// </summary>
    internal static float GetTotalEnergy_SIMD(ReadOnlySpan<float> samples)
    {
        float sum = 0f;
        int vectorSize = Vector<float>.Count; // 获取SIMD向量的大小
        int i = 0;

        // 使用SIMD进行并行计算
        for (; i <= samples.Length - vectorSize; i += vectorSize)
        {
            var vector = new Vector<float>(samples.Slice(i, vectorSize));
            sum += Vector.Dot(vector, vector); // 计算向量的点积并累加到sum
        }

        // 处理剩余的元素
        for (; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        return sum;
    }

    /// <summary>
    /// Gets the average energy of the signal (mean of squared samples) using SIMD optimization.
    /// </summary>
    internal static float GetAverageEnergy_SIMD(ReadOnlySpan<float> samples) => GetTotalEnergy_SIMD(samples) / samples.Length;

    #endregion


    #region RMS (Root Mean Square)

    /// <summary>
    /// Baseline implementation for calculating the Root Mean Square (RMS).
    /// Defined as the square root of the average energy: RMS = sqrt(Avg(x²)).
    /// Used to verify the correctness of other optimized versions.
    /// </summary>
    /// <param name="samples">A read-only span of samples.</param>
    /// <returns>The RMS value of the input samples.</returns>
    /// <remarks>This is the baseline version, used to verify the correctness of other optimized versions.</remarks>
    internal static float GetRms_Normal(ReadOnlySpan<float> samples) => MathF.Sqrt(GetAverageEnergy_Normal(samples));

    /// <summary>
    /// SIMD-optimized Root Mean Square (RMS) calculation.
    /// Accelerates the average energy computation via vectorization, then takes the square root to obtain the RMS.
    /// Offers higher performance on large-scale data.
    /// </summary>
    /// <param name="samples">A read-only span of samples.</param>
    /// <returns>The RMS value of the input samples.</returns>
    internal static float GetRms_SIMD(ReadOnlySpan<float> samples) => MathF.Sqrt(GetAverageEnergy_SIMD(samples));

    #endregion


    #region Zero Crossing Rate

    /// <summary>
    /// Calculates the Zero-Crossing Rate (ZCR) of the signal.
    /// The ZCR is the proportion of adjacent sample pairs where a sign change occurs relative to the total number of adjacent pairs.
    /// This implementation adds a small positive bias (1e-4f) to each sample to effectively avoid false crossings
    /// caused by floating-point precision issues or noise near zero, improving robustness and stability
    /// in real audio signals, especially in low-energy segments.
    /// </summary>
    /// <param name="samples">A read-only span of samples.</param>
    /// <returns>The normalized zero-crossing rate in the range [0, 1]; a higher value indicates more frequent zero crossings.</returns>
    /// <remarks>
    /// The normalization denominator is (samples.Length - 1), i.e., the number of adjacent sample pairs.
    /// This method is commonly used in voiced/unvoiced speech classification, voice activity detection (VAD),
    /// and audio event classification.
    /// This is the baseline version, used to verify the correctness of other optimized versions.
    /// </remarks>
    internal static float GetZeroCrossingRate_Normal(ReadOnlySpan<float> samples)
    {
        if (samples.Length <= 1)
            return 0f; // 样本不足以形成相邻对，直接返回 0

        const float disbalance = 1e-4f; // 微小正偏置，防止零附近噪声导致虚假过零

        // 取第一个样本并加上偏置作为前一样本
        var prevSample = samples[0] + disbalance;

        var rate = 0;
        for (var i = 1; i < samples.Length; i++)
        {
            var sample = samples[i] + disbalance;

            // 符号变化则计为一次过零
            if ((sample >= 0) != (prevSample >= 0))
            {
                rate++;
            }

            prevSample = sample;
        }

        // 除以相邻样本对数量进行归一化
        return (float)rate / (samples.Length - 1);
    }

    /// <summary>
    /// Not working.
    /// </summary>
    /// <param name="samples"></param>
    /// <returns></returns>
    [Obsolete]
    internal static unsafe float GetZeroCrossingRate_SIMD(ReadOnlySpan<float> samples)
    {
        int len = samples.Length;
        if (len <= 1) return 0f;

        const float disbalance = 1e-4f;

        // Fallback when intrinsics not supported or too few samples
        if (!Sse.IsSupported || len < Vector128<float>.Count)
        {
            int rateScalar = 0;
            float prev = samples[0] + disbalance;
            for (int idx = 1; idx < len; idx++)
            {
                float cur = samples[idx] + disbalance;
                if ((cur >= 0) != (prev >= 0)) rateScalar++;
                prev = cur;
            }
            return (float)rateScalar / (len - 1);
        }

        int transitions = 0;
        int i = 0;

        bool useAvx = Avx.IsSupported;
        int blockSize = useAvx ? Vector256<float>.Count : Vector128<float>.Count; // 8 or 4

        // Bias vectors
        Vector256<float> bias256 = useAvx ? Vector256.Create(disbalance) : default;
        Vector128<float> bias128 = Vector128.Create(disbalance);

        // Boundary handling
        float prevScalar = samples[0] + disbalance;

        // Base ref for pointer arithmetic into the span
        ref float baseRef = ref System.Runtime.InteropServices.MemoryMarshal.GetReference(samples);

        if (useAvx)
        {
            for (; i <= len - blockSize; i += blockSize)
            {
                float* ptr = (float*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.Add(ref baseRef, i));
                var v = Avx.LoadVector256(ptr);
                v = Avx.Add(v, bias256);

                // MoveMask returns sign-bit mask per lane (1 = negative)
                int maskNeg = Avx.MoveMask(v);
                int maskNonNeg = (~maskNeg) & 0xFF; // 1 = >= 0

                // XOR with left-shift detects differences between adjacent lanes
                int xorMask = maskNonNeg ^ (maskNonNeg << 1);
                // Keep transitions for pairs (0-1 .. 6-7). Bit positions 1..7
                xorMask &= 0xFE;

                transitions += System.Numerics.BitOperations.PopCount((uint)xorMask);

                // Boundary transition from previous scalar tail to current block head
                float head = samples[i] + disbalance;
                if ((head >= 0) != (prevScalar >= 0)) transitions++;

                // Update prev to block tail
                prevScalar = samples[i + blockSize - 1] + disbalance;
            }
        }
        else
        {
            for (; i <= len - blockSize; i += blockSize)
            {
                float* ptr = (float*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.Add(ref baseRef, i));
                var v = Sse.LoadVector128(ptr);
                v = Sse.Add(v, bias128);

                int maskNeg = Sse.MoveMask(v);
                int maskNonNeg = (~maskNeg) & 0x0F;

                int xorMask = maskNonNeg ^ (maskNonNeg << 1);
                // Keep transitions for pairs (0-1,1-2,2-3) => bits 1..3
                xorMask &= 0x0E;

                transitions += System.Numerics.BitOperations.PopCount((uint)xorMask);

                float head = samples[i] + disbalance;
                if ((head >= 0) != (prevScalar >= 0)) transitions++;

                prevScalar = samples[i + blockSize - 1] + disbalance;
            }
        }

        // Scalar tail
        for (; i < len; i++)
        {
            float cur = samples[i] + disbalance;
            if ((cur >= 0) != (prevScalar >= 0)) transitions++;
            prevScalar = cur;
        }

        return (float)transitions / (len - 1);
    }


    /// <summary>
    /// Garbage.
    /// </summary>
    /// <param name="samples"></param>
    /// <returns></returns>
    [Obsolete]
    internal static float GetZeroCrossingRate_Vector(ReadOnlySpan<float> samples)
    {
        int len = samples.Length;
        if (len <= 1) return 0f;

        const float disbalance = 1e-4f;
        int transitions = 0;

        int vsz = Vector<float>.Count;
        int i = 0;

        // First sample for boundary
        float prevScalar = samples[0] + disbalance;

        // Process full vectors
        for (; i <= len - vsz; i += vsz)
        {
            var v = new Vector<float>(samples.Slice(i, vsz));
            v = v + new Vector<float>(disbalance);

            // Extract lanes to scalar (no move-mask available)
            // Note: this per-lane extraction reduces the gain substantially.
            float lane0 = v[0];

            // Boundary transition: previous tail -> current head
            if ((lane0 >= 0) != (prevScalar >= 0))
                transitions++;

            // Intra-vector transitions
            float prev = lane0;
            for (int k = 1; k < vsz; k++)
            {
                float cur = v[k];
                if ((cur >= 0) != (prev >= 0))
                    transitions++;
                prev = cur;
            }

            // Update prevScalar to tail of the vector
            prevScalar = v[vsz - 1];
        }

        // Tail scalar
        for (; i < len; i++)
        {
            float cur = samples[i] + disbalance;
            if ((cur >= 0) != (prevScalar >= 0))
                transitions++;
            prevScalar = cur;
        }

        return (float)transitions / (len - 1);
    }

    /// <summary>
    /// Optimal implementation.
    /// </summary>
    /// <param name="samples"></param>
    /// <returns></returns>
    internal static float GetZeroCrossingRate_NEWSIMD_Grok(ReadOnlySpan<float> samples)
    {
        int len = samples.Length;
        if (len <= 1)
            return 0f;

        const float disbalance = 1e-4f;

        // 短序列或无硬件加速时 fallback 到标量（你已有 GetZeroCrossingRate_Normal）
        if (len < Vector<float>.Count || !Vector.IsHardwareAccelerated)
            return GetZeroCrossingRate_Normal(samples);

        int transitions = 0;
        int vectorSize = Vector<float>.Count;

        Vector<float> bias = new(disbalance);

        float prevScalar = samples[0] + disbalance;

        ref float baseRef = ref MemoryMarshal.GetReference(samples);

        int i = 0;

        for (; i <= len - vectorSize; i += vectorSize)
        {
            // 不要求对齐，安全加载向量
            ref float blockStart = ref Unsafe.Add(ref baseRef, i);
            Vector<float> v = Unsafe.ReadUnaligned<Vector<float>>(
                ref Unsafe.As<float, byte>(ref blockStart));

            v += bias;

            // 获取 MoveMask（负号掩码）
            int maskNeg;
            if (Avx.IsSupported && vectorSize == 8)
            {
                maskNeg = Avx.MoveMask(v.AsVector256());
            }
            else // vectorSize == 4 (SSE)
            {
                maskNeg = Sse.MoveMask(v.AsVector128());
            }

            // >=0 的掩码（低 vectorSize 位有效）
            int maskBits = (1 << vectorSize) - 1;
            int nonNegativeMask = (~maskNeg) & maskBits;

            // 相邻 lane 符号变化掩码
            int diffMask = nonNegativeMask ^ (nonNegativeMask << 1);

            // 只取内部 vectorSize-1 个相邻对对应的位（bit 1 到 bit vectorSize-1）
            int validMask = maskBits ^ 1; // 等价于 (1 << vectorSize) - 2
            transitions += BitOperations.PopCount((uint)(diffMask & validMask));

            // 块间边界过渡（前一块尾部 -> 当前块头部）
            float head = samples[i] + disbalance;
            if ((head >= 0) != (prevScalar >= 0))
                transitions++;

            // 更新为当前块尾部
            prevScalar = samples[i + vectorSize - 1] + disbalance;
        }

        // 剩余尾部标量处理
        for (; i < len; i++)
        {
            float cur = samples[i] + disbalance;
            if ((cur >= 0) != (prevScalar >= 0))
                transitions++;
            prevScalar = cur;
        }

        return (float)transitions / (len - 1);
    }

    #endregion


    #region Entropy

    /// <summary>
    /// Calculates the normalized Shannon entropy of the signal, used to measure the complexity or randomness of the amplitude distribution.
    /// Estimates the probability distribution by uniformly binning the absolute sample values, computes the entropy,
    /// and normalizes it to the [0, 1] range.
    /// </summary>
    /// <param name="samples">A read-only span of samples.</param>
    /// <param name="binCount">The number of histogram bins; defaults to 32. Recommended range is 8–64.</param>
    /// <returns>The normalized Shannon entropy (0 to 1).
    /// 0 indicates complete certainty (e.g., silence or a pure tone); 1 indicates maximum randomness (e.g., white noise).</returns>
    internal static float GetEntropy_Normal(ReadOnlySpan<float> samples, int binCount = 32)
    {
        int len = samples.Length;
        if (len == 0) return 0f;

        // 限制 binCount 合理范围，至少 2 个 bin 才有意义
        if (len < binCount) binCount = len;
        if (binCount < 2) return 0f; // 单个 bin 或更少，熵为 0

        // 计算绝对值的最小和最大值
        float min = MathF.Abs(samples[0]);
        float max = min;
        for (int i = 1; i < len; i++)
        {
            float absVal = MathF.Abs(samples[i]);
            if (absVal < min) min = absVal;
            if (absVal > max) max = absVal;
        }

        // 若信号幅度几乎恒定，熵为 0
        if (max - min < 1e-8f) return 0f;

        float binLength = (max - min) / binCount;
        var bins = new int[binCount];

        // 统计落入每个 bin 的样本数，注意边界 clamp
        for (int i = 0; i < len; i++)
        {
            float value = MathF.Abs(samples[i]);
            int idx = (int)((value - min) / binLength);
            if (idx < 0) idx = 0;
            if (idx >= binCount) idx = binCount - 1;
            bins[idx]++;
        }

        // 计算香农熵（累加负值）
        float entropy = 0f;
        float invLen = 1f / len;
        for (int i = 0; i < binCount; i++)
        {
            float p = bins[i] * invLen;
            if (p > 1e-8f)
            {
                entropy += p * MathF.Log2(p);  // p*log2(p) ≤ 0
            }
        }

        // 取负并归一化到 [0, 1]
        float maxPossibleEntropy = MathF.Log2(binCount);
        return -entropy / maxPossibleEntropy;
    }


    /// <summary>
    /// SIMD-optimized normalized Shannon entropy calculation.
    /// Accelerates the min/max search of absolute values via vectorization; builds a histogram and computes
    /// the entropy, normalized to the [0, 1] range.
    /// </summary>
    /// <param name="samples">A read-only span of samples.</param>
    /// <param name="binCount">The number of histogram bins; defaults to 32. Recommended range is 8–64.</param>
    internal static float GetEntropy_SIMD(ReadOnlySpan<float> samples, int binCount = 32)
    {
        int len = samples.Length;
        if (len == 0)
        {
            return 0f;
        }

        if (len < binCount)
        {
            binCount = len;
        }
        if (binCount < 2)
        {
            return 0f;
        }

        // 1) SIMD 计算 |samples| 的 min/max
        int vsz = Vector<float>.Count;
        int i = 0;

        var vmin = new Vector<float>(float.PositiveInfinity);
        var vmax = new Vector<float>(float.NegativeInfinity);

        for (; i <= len - vsz; i += vsz)
        {
            var v = new Vector<float>(samples.Slice(i, vsz));
            v = Vector.Abs(v);
            vmin = Vector.Min(vmin, v);
            vmax = Vector.Max(vmax, v);
        }

        float min = float.PositiveInfinity;
        float max = float.NegativeInfinity;

        // 归约 SIMD 部分
        for (int k = 0; k < vsz; k++)
        {
            float a = vmin[k];
            float b = vmax[k];
            if (a < min) min = a;
            if (b > max) max = b;
        }

        // 处理余数（不足一个向量的尾部）
        for (; i < len; i++)
        {
            float a = MathF.Abs(samples[i]);
            if (a < min) min = a;
            if (a > max) max = a;
        }

        if (max - min < 1e-8f)
        {
            return 0f;
        }

        // 2) 构建直方图（标量写入更稳妥），减少重复开销
        float binLength = (max - min) / binCount;
        float invBinLength = 1f / binLength;

        var bins = new int[binCount];

        // 使用预计算的倒数与 clamp，避免除法与越界
        for (int idxSample = 0; idxSample < len; idxSample++)
        {
            float value = MathF.Abs(samples[idxSample]);
            int idx = (int)((value - min) * invBinLength);
            if (idx < 0) idx = 0;
            if (idx >= binCount) idx = binCount - 1;
            bins[idx]++;
        }

        // 3) 计算熵并归一化到 [0, 1]
        float entropy = 0f;
        float invLen = 1f / len;
        for (int b = 0; b < binCount; b++)
        {
            float p = bins[b] * invLen;
            if (p > 1e-8f)
            {
                entropy += p * MathF.Log2(p); // 累加负值
            }
        }

        return -entropy / MathF.Log2(binCount);
    }


    internal static void EntropyResultCompareTest()
    {
        int i = 0;
        while (i++ < 100)
        {
            var consoleColor = (ConsoleColor)Random.Shared.Next(0, 15);

            var samples = new float[Random.Shared.Next(10000, 100000)];
            samples.FillWithRandomNumber();

            ITimeDomainCharacteristics.GetEntropy_Normal(samples).PrintLine(consoleColor);
            ITimeDomainCharacteristics.GetEntropy_SIMD(samples).PrintLine(consoleColor);

        }
    }

    #endregion
}