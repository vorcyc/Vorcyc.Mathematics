using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.Experimental.Signals;


/// <summary>
/// Represents the time-domain characteristics of signal.
/// </summary>
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
        var (max, min) = Statistics.ExtremeValueFinder.FindExtremeValue(samples);
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

/// <summary>
/// 定义用于表示时域信号的接口。
/// </summary>
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
    float SamplingRate { get; }

    /// <summary>
    /// 获取信号的长度。
    /// </summary>
    int Length { get; }


    #region Operators


    #region 老叫我去实现接口，用类的不行


    //static abstract ITimeDomainSignal operator +(ITimeDomainSignal left, float right);

    //static abstract ITimeDomainSignal operator +(ITimeDomainSignal left, ITimeDomainSignal right);


    //static abstract ITimeDomainSignal operator -(ITimeDomainSignal left, float right);

    //static abstract ITimeDomainSignal operator -(ITimeDomainSignal left, ITimeDomainSignal right);


    //static abstract ITimeDomainSignal operator *(ITimeDomainSignal left, float right);

    //static abstract ITimeDomainSignal operator *(ITimeDomainSignal left, ITimeDomainSignal right);


    //static abstract ITimeDomainSignal operator /(ITimeDomainSignal left, float right);

    //static abstract ITimeDomainSignal operator /(ITimeDomainSignal left, ITimeDomainSignal right);

    #endregion


    #region 这样做毫无意义 ，我认为是语言漏洞

    //static ITimeDomainSignal operator +(ITimeDomainSignal left, float right)
    //{
    //    var result = left.Clone();
    //    result.Samples.Add(right);
    //    return result;
    //}

    //static ITimeDomainSignal? operator +(ITimeDomainSignal left, ITimeDomainSignal right)
    //{
    //    if (left.Length != right.Length || left.SamplingRate != right.SamplingRate)
    //        return null;
    //    var result = left.Clone();
    //    result.Samples.Add(right.Samples);
    //    return result;
    //}


    //static ITimeDomainSignal operator -(ITimeDomainSignal left, float right)
    //{
    //    var result = left.Clone();
    //    result.Samples.Subtract(right);
    //    return result;
    //}

    //static ITimeDomainSignal? operator -(ITimeDomainSignal left, ITimeDomainSignal right)
    //{
    //    if (left.Length != right.Length || left.SamplingRate != right.SamplingRate)
    //        return null;
    //    var result = left.Clone();
    //    result.Samples.Subtract(right.Samples);
    //    return result;
    //}

    //static ITimeDomainSignal operator *(ITimeDomainSignal left, float right)
    //{
    //    var result = left.Clone();
    //    result.Samples.Multiply(right);
    //    return result;
    //}

    //static ITimeDomainSignal? operator *(ITimeDomainSignal left, ITimeDomainSignal right)
    //{
    //    if (left.Length != right.Length || left.SamplingRate != right.SamplingRate)
    //        return null;
    //    var result = left.Clone();
    //    result.Samples.Multiply(right.Samples);
    //    return result;
    //}

    //static ITimeDomainSignal operator /(ITimeDomainSignal left, float right)
    //{
    //    var result = left.Clone();
    //    result.Samples.Divide(right);
    //    return result;
    //}  


    #endregion


    #endregion


    /// <summary>
    /// 获取数组的片段。
    /// </summary>
    /// <param name="array">输入数组。</param>
    /// <param name="start">起始索引。</param>
    /// <param name="length">片段的长度。</param>
    /// <returns>数组片段的 <see cref="Span{T}"/> 对象。</returns>
    /// <exception cref="ArgumentNullException">当数组为空时抛出。</exception>
    /// <exception cref="ArgumentOutOfRangeException">当起始索引或长度不在有效范围内时抛出。</exception>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float[] PadZerosAndWindowing
        (
            float[] samples,
            int offset,
            int desiredLen,
            int actualLen,
            WindowType? windowingType = null
        )
    {
        float[] tempSamples = new float[desiredLen];

        Array.Copy(samples, offset, tempSamples, 0, actualLen);

        //if (windowingType is not null)
        //    Vorcyc.Mathematics.SignalProcessing.Windowing.WindowApplier.Apply(tempSamples.AsSpan(), 0, desiredLen, windowingType.Value);  
        if (windowingType is not null)
            WindowApplier.Apply(tempSamples, windowingType.Value, true);

        return tempSamples;
    }


    /// <summary>
    /// 将时域信号转换为频域信号。
    /// </summary>
    /// <param name="window">加窗类型（可选）。</param>
    /// <param name="fftVersion">FFT的执行方式。建议小规模数据用<see cref="FftVersion.Normal"/>，大规模数据用<see cref="FftVersion.Parallel"/>。默认为<see cref="FftVersion.Normal"/></param>
    /// <returns>频域信号的 <see cref="FrequencyDomain"/> 对象。</returns>
    FrequencyDomain TransformToFrequencyDomain(WindowType? window = null, FftVersion fftVersion = FftVersion.Normal);


    /// <summary>
    /// 重采样信号。
    /// </summary>
    /// <param name="destnationSamplingRate">目标采样率。</param>
    /// <param name="filter">可选的 FIR 滤波器。</param>
    /// <param name="order">滤波器阶数，默认为 15。</param>
    /// <returns>重采样后的 <see cref="ITimeDomainSignal"/> 对象。</returns>
    Signal Resample(int destnationSamplingRate, FirFilter? filter = null, int order = 15);

    /// <summary>
    /// 重采样信号。
    /// </summary>
    /// <param name="signal">输入的时域信号。</param>
    /// <param name="destnationSamplingRate">目标采样率。</param>
    /// <param name="filter">可选的 FIR 滤波器。</param>
    /// <param name="order">滤波器阶数，默认为 15。</param>
    /// <returns>重采样后的 <see cref="ITimeDomainSignal"/> 对象。</returns>
    internal static Signal Resample(
        ITimeDomainSignal signal,
        float destnationSamplingRate,
        FirFilter? filter = null,
        int order = 15)
    {
        if (signal.SamplingRate == destnationSamplingRate)
        {
            var sameResult = new Signal(signal.Length, signal.SamplingRate);
            signal.Samples.CopyTo(sameResult.Samples);
            return sameResult;
        }

        var g = destnationSamplingRate / signal.SamplingRate;

        var input = signal.Samples;
        var output = new float[(int)(input.Length * g)];

        if (g < 1 && filter is null)
        {
            filter = new FirFilter(DesignFilter.FirWinLp(101, g / 2));

            input = filter.ProcessAllSamples(signal.Samples);  // filter.ApplyTo(signal).Samples;
        }

        var step = 1 / g;

        for (var n = 0; n < output.Length; n++)
        {
            var x = n * step;

            for (var i = -order; i < order; i++)
            {
                var j = (int)Math.Floor(x) - i;

                if (j < 0 || j >= input.Length)
                {
                    continue;
                }

                var t = x - j;
                float w = 0.5f * (1.0f + MathF.Cos(t / order * ConstantsFp32.PI));    // Hann window
                float sinc = TrigonometryHelper.Sinc(t);                             // Sinc function
                output[n] += w * sinc * input[j];
            }
        }

        var result = new Signal(output.Length, destnationSamplingRate);
        output.CopyTo(result.Samples);
        return result;
    }


    /// <summary>
    /// 转成数据量(对应于数组的索引或长度)
    /// </summary>
    /// <param name="time"></param>
    /// <param name="samplingRate"></param>
    /// <returns></returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    internal static int TimeToArrayIndexOrLength(TimeSpan time, float samplingRate)
        => (int)(time.TotalSeconds * samplingRate);


    /// <summary>
    /// 数据量(对应于数组的索引或长度) 转成时间
    /// </summary>
    /// <param name="indexOrLength"></param>
    /// <param name="samplingRate"></param>
    /// <returns></returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    internal static TimeSpan ArrayIndexOrLengthToTime(int indexOrLength, float samplingRate)
        => TimeSpan.FromSeconds(indexOrLength / samplingRate);



}
