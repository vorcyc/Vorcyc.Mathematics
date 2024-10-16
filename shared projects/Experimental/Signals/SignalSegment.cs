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
    /// 获取信号段的长度。
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// 获取信号段的采样数据。
    /// </summary>
    public Span<float> Samples => new(_signal._samples, _start, _length);

    /// <summary>
    /// 获取信号段的持续时间。
    /// </summary>
    public TimeSpan Duration => TimeSpan.FromSeconds(1f / (float)_signal.SamplingRate * _length);

    /// <summary>
    /// 获取信号的采样率。
    /// </summary>
    public int SamplingRate => _signal.SamplingRate;

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



    public ITimeDomainSignal Clone()
    {
        var result = new SignalSegment(_signal, _start, _length);
        this.Samples.CopyTo(result.Samples);
        return result;
    }



    /// <summary>
    /// 将信号段转换为频域信号。
    /// </summary>
    /// <param name="window">窗口类型，可选参数。</param>
    /// <returns>频域信号对象。</returns>
    public FrequencyDomain TransformToFrequencyDomain(WindowType? window = null)
    {
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

    public Signal Resample(
            int destnationSamplingRate,
            FirFilter? filter = null,
            int order = 15)
    {
        return ITimeDomainSignal.Resample(this, destnationSamplingRate, filter, order);
    }

}
