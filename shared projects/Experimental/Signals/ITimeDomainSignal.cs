using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Windowing;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.Experimental.Signals;


public interface ITimeDomainCharacteristics
{
    /// <summary>
    /// 获取信号的幅度（最大值与最小值之差）。
    /// </summary>
    float Amplitude { get; }

    /// <summary>
    /// 获取信号的周期（采样率的倒数）。
    /// </summary>
    float Period { get; }

    /// <summary>
    /// 获取信号的功率（样本平方和的平均值）。
    /// </summary>
    float Power { get; }

    /// <summary>
    /// 获取信号的能量（样本平方和）。
    /// </summary>
    float Energy { get; }


    #region Amplitude

    /// <summary>
    /// 获取信号的幅度（最大值与最小值之差）。
    /// </summary>
    internal static float GetAmplitude_Normal(Span<float> samples)
    {
        var max = samples.Max();
        var min = samples.Min();
        return max - min;
    }

    /// <summary>
    /// 获取信号的幅度（最大值与最小值之差）。
    /// </summary>
    internal static float GetAmplitude_SIMD(Span<float> samples)
    {
        var (max, min) = ExtremeValueFinder.FindExtremeValue(samples);
        return max - min;
    }

    #endregion


    #region Power

    /// <summary>
    /// 获取信号的功率（样本平方和的平均值）。
    /// </summary>
    internal static float GetPower_Normal(Span<float> samples)
    {
        float sumOfSquares = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            sumOfSquares += samples[i] * samples[i];
        }
        return sumOfSquares / samples.Length;
    }

    /// <summary>
    /// 获取信号的功率（样本平方和的平均值）。
    /// </summary>
    internal static float GetPower_SIMD(Span<float> samples)
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

        return sumOfSquares / samples.Length;
    }

    #endregion


    #region Energy

    /// <summary>
    /// 获取信号的能量（样本平方和）。
    /// </summary>
    internal static float GetEnergy_Normal(Span<float> samples)
    {
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }
        return sum;
    }

    /// <summary>
    /// 获取信号的能量（样本平方和）。
    /// </summary>
    internal static float GetEnergy_SIMD(Span<float> samples)
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

    #endregion




}

public interface ITimeDomainSignal : ITimeDomainCharacteristics
{

    /// <summary>
    /// 获取信号样本数组。
    /// </summary>
    Span<float> Samples { get; }

    /// <summary>
    /// 持续时间
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// 获取信号的采样率。
    /// </summary>
    int SamplingRate { get; }

    /// <summary>
    /// 获取信号的长度。
    /// </summary>
    int Length { get; }


    internal static Span<float> GetArraySegment(float[] array, int start, int length)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        if (start < 0 || start >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(start), "Start index is out of range.");

        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");

        if (start + length > array.Length)
            throw new ArgumentOutOfRangeException(nameof(length), "The segment exceeds the array bounds.");

        int originalLength = Math.Min(length, array.Length - start);
        int paddedLength = originalLength.NextPowerOf2();

        if (paddedLength == originalLength)
        {
            return new Span<float>(array, start, length);
        }
        else
        {
            float[] paddedArray = new float[paddedLength];
            new Span<float>(array, start, originalLength).CopyTo(paddedArray);
            return new Span<float>(paddedArray);
        }
    }


    /// <summary>
    /// 创建临时区，并根据需要加窗。（不修改原始样本）
    /// </summary>
    /// <param name="samples"></param>
    /// <param name="offset"></param>
    /// <param name="desiredLen">期望长度：就是补0后的长度，为2的N次方。该长度用于FFT</param>
    /// <param name="actualLen">实际长度，用于从源数组中拷贝的数据长度</param>
    /// <param name="windowingType"></param>
    /// <returns></returns>
    internal static float[] PadZerosAndWindowing
        (
            float[] samples,
            int offset,
            int desiredLen,
            int actualLen,
            WindowType? windowingType = null
        )
    {
        var tempSamples = new float[desiredLen];

        Array.Copy(samples, offset, tempSamples, 0, actualLen);

        if (windowingType is not null)
            Windowing.Apply(tempSamples, 0, desiredLen, windowingType.Value);

        return tempSamples;
    }




    FrequencyDomain TransformToFrequencyDomain(WindowType? window = null);




}
