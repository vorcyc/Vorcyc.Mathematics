using Vorcyc.Mathematics.Helpers;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// 表示一个信号段的类，实现了 ITimeDomainSignal 接口。
/// </summary>
public class SignalSegment : ITimeDomainSignal
{
    private readonly Signal _signal;
    private readonly int _start, _length;

    /// <summary>
    /// 内部构造函数，用于初始化 SignalSegment 类的新实例。
    /// </summary>
    /// <param name="signal">信号对象。</param>
    /// <param name="start">信号段的起始位置。</param>
    /// <param name="length">信号段的长度。</param>
    internal SignalSegment(Signal signal, int start, int length)
    {
        _signal = signal;
        _start = start;
        _length = length;
    }

    /// <summary>
    /// 获取信号段的起始位置。
    /// </summary>
    public int Start => _start;

    /// <summary>
    /// 当前信号段的起始时间
    /// </summary>
    public TimeSpan StartTime => ITimeDomainSignal.ArrayIndexOrLengthToTime(_start, _signal.SamplingRate);

    /// <summary>
    /// 获取信号段的长度。
    /// </summary>
    public int Length => _length;


    /// <summary>
    /// 获取信号段的持续时间。
    /// </summary>
    public TimeSpan Duration => ITimeDomainSignal.ArrayIndexOrLengthToTime(_length, _signal.SamplingRate);// TimeSpan.FromSeconds(1f / (float)_signal.SamplingRate * _length);

    /// <summary>
    /// 获取信号段的采样数据。
    /// </summary>
    //public Span<float> Samples => new(_signal._samples, _start, _length);
    public Span<float> Samples => _signal._samples.AsSpan(_start, _length);


    /// <summary>
    /// 获取信号的采样率。
    /// </summary>
    public float SamplingRate => _signal.SamplingRate;

    /// <summary>
    /// 获取信号段的振幅。
    /// </summary>
    public float Amplitude => ITimeDomainCharacteristics.GetAmplitude_SIMD(this.Samples);

    /// <summary>
    /// 获取信号的周期。
    /// </summary>
    public float Period => _signal.Period;

    /// <summary>
    /// 获取信号段的功率。
    /// </summary>
    public float Power => ITimeDomainCharacteristics.GetPower_SIMD(this.Samples);

    /// <summary>
    /// 获取信号段的能量。
    /// </summary>
    public float Energy => ITimeDomainCharacteristics.GetEnergy_SIMD(this.Samples);





    /// <summary>
    /// 将信号段转换为频域信号。
    /// </summary>
    /// <param name="window">窗口类型，可选参数。</param>
    /// <param name="fftVersion">FFT的执行方式。建议小规模数据用<see cref="FftVersion.Normal"/>，大规模数据用<see cref="FftVersion.Parallel"/>。默认为<see cref="FftVersion.Normal"/></param>
    /// <returns>频域信号对象。</returns>
    public FrequencyDomain TransformToFrequencyDomain(WindowType? window = null, FftVersion fftVersion = FftVersion.Normal)
    {
        FastFourierTransform.Version = fftVersion;
        if (window is null && _length.IsPowerOf2())
        {
            FastFourierTransform.Forward(_signal._samples, _start, out var result, _length);
            return new FrequencyDomain(_start, _length, _length, result, this, null);
        }
        else
        {
            var windowedSamples = ITimeDomainSignal.PadZerosAndWindowing(_signal._samples, _start, _length.NextPowerOf2(), _length, window);
            FastFourierTransform.Forward(windowedSamples, 0, out var result, windowedSamples.Length);
            return new FrequencyDomain(_start, windowedSamples.Length, _length, result, this, window);
        }
    }

    /// <summary>
    /// 重采样并返回新的信号。
    /// </summary>
    /// <param name="destnationSamplingRate"></param>
    /// <param name="filter"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public Signal Resample(
            int destnationSamplingRate,
            FirFilter? filter = null,
            int order = 15)
    {
        return ITimeDomainSignal.Resample(this, destnationSamplingRate, filter, order);
    }


    /// <summary>
    /// 从所在信号中脱离成为单独的信号。
    /// </summary>
    /// <returns></returns>
    public Signal Decouple()
    {
        var result = new Signal(this._length, this._signal.SamplingRate);
        this.Samples.CopyTo(result.Samples);
        return result;
    }




    #region Operators

    /// <summary>
    /// 将信号段与一个浮点数相加。
    /// </summary>
    /// <param name="left">信号段。</param>
    /// <param name="right">浮点数。</param>
    /// <returns>相加后的新信号。</returns>
    public static Signal operator +(SignalSegment left, float right)
    {
        var result = left.Decouple();
        result.Samples.Add(right);
        return result;
    }

    /// <summary>
    /// 将两个信号段相加。
    /// </summary>
    /// <param name="left">左侧信号段。</param>
    /// <param name="right">右侧信号段。</param>
    /// <returns>相加后的新信号，如果长度或采样率不匹配则返回 null。</returns>
    public static Signal? operator +(SignalSegment left, SignalSegment right)
    {
        if (left.Length != right.Length || left.SamplingRate != right.SamplingRate)
            return null;
        var result = left.Decouple();
        result.Samples.Add(right.Samples);
        return result;
    }

    /// <summary>
    /// 将信号段与一个浮点数相减。
    /// </summary>
    /// <param name="left">信号段。</param>
    /// <param name="right">浮点数。</param>
    /// <returns>相减后的新信号。</returns>
    public static Signal operator -(SignalSegment left, float right)
    {
        var result = left.Decouple();
        result.Samples.Subtract(right);
        return result;
    }

    /// <summary>
    /// 将两个信号段相减。
    /// </summary>
    /// <param name="left">左侧信号段。</param>
    /// <param name="right">右侧信号段。</param>
    /// <returns>相减后的新信号，如果长度或采样率不匹配则返回 null。</returns>
    public static Signal? operator -(SignalSegment left, SignalSegment right)
    {
        if (left.Length != right.Length || left.SamplingRate != right.SamplingRate)
            return null;
        var result = left.Decouple();
        result.Samples.Subtract(right.Samples);
        return result;
    }

    /// <summary>
    /// 将信号段与一个浮点数相乘。
    /// </summary>
    /// <param name="left">信号段。</param>
    /// <param name="right">浮点数。</param>
    /// <returns>相乘后的新信号。</returns>
    public static Signal operator *(SignalSegment left, float right)
    {
        var result = left.Decouple();
        result.Samples.Multiply(right);
        return result;
    }

    /// <summary>
    /// 将两个信号段相乘。
    /// </summary>
    /// <param name="left">左侧信号段。</param>
    /// <param name="right">右侧信号段。</param>
    /// <returns>相乘后的新信号，如果长度或采样率不匹配则返回 null。</returns>
    public static Signal? operator *(SignalSegment left, SignalSegment right)
    {
        if (left.Length != right.Length || left.SamplingRate != right.SamplingRate)
            return null;
        var result = left.Decouple();
        result.Samples.Multiply(right.Samples);
        return result;
    }

    /// <summary>
    /// 将信号段与一个浮点数相除。
    /// </summary>
    /// <param name="left">信号段。</param>
    /// <param name="right">浮点数。</param>
    /// <returns>相除后的新信号。</returns>
    public static Signal operator /(SignalSegment left, float right)
    {
        var result = left.Decouple();
        result.Samples.Divide(right);
        return result;
    }


    #endregion




}
