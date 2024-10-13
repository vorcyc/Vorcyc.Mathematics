using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// 表示一个信号类，包含信号样本和采样率，并提供计算信号属性的方法。
/// </summary>
public class Signal : ITimeDomainSignal
{
    /// <summary>
    /// 有可能是补 0  后的长度。这样虽然会浪费内存空间，但是节省了后期分配的时间。
    /// </summary>
    internal float[] _samples;
    private int _length;
    private int _samplingRate;

    public Signal(int count, int samplingRate)
    {
        _length = count;
        _samples = new float[count];
        _samplingRate = samplingRate;
    }

    /// <summary>
    /// 获取信号样本数组。(实际长度，而非补 0 后的长度）
    /// </summary>
    public Span<float> Samples => _samples.AsSpan(0, _length);

    /// <summary>
    /// 获取信号的采样率。
    /// </summary>
    public int SamplingRate => _samplingRate;


    public TimeSpan Duration => TimeSpan.FromSeconds(1f / (float)_samplingRate * _length);


    public int Length => _length;



    //public float Amplitude => ITimeDomainSignal.Amplitude_Normal(this.ValidSamples);

    /// <summary>
    /// 获取信号的幅度（最大值与最小值之差）。
    /// </summary>
    public float Amplitude => ITimeDomainCharacteristics.GetAmplitude_SIMD(this.Samples);

    /// <summary>
    /// 获取信号的周期（采样率的倒数）。
    /// </summary>
    public float Period => 1f / (float)_samplingRate;


    //public float Power => ITimeDomainSignal.GetPower_Normal(this.ValidSamples);

    /// <summary>
    /// 获取信号的功率（样本平方和的平均值）。
    /// </summary>
    public float Power => ITimeDomainCharacteristics.GetPower_SIMD(this.Samples);


    //public float Energy => ITimeDomainSignal.GetEnergy_Normal(this.ValidSamples);

    /// <summary>
    /// 获取信号的能量（样本平方和）。
    /// </summary>
    public float Energy => ITimeDomainCharacteristics.GetEnergy_SIMD(this.Samples);



    public FrequencyDomain TransformToFrequencyDomain(WindowType? window = null)
    {
        if (window is null && _length.IsPowerOf2())//若不应用窗函数，则直接使用补过 0 后的样本进行变换
        {
            FastFourierTransform.Forward(_samples, 0, out var result, _length);
            return new FrequencyDomain(0, _length, _length, result, this, window);
        }
        else//由于窗函数需要修改样本值，所以只要使用窗函数都需要创建临时副本
        {
            var windowedSamples = ITimeDomainSignal.PadZerosAndWindowing(_samples, 0, _length.NextPowerOf2(), _length, window);
            FastFourierTransform.Forward(windowedSamples, 0, out var result, windowedSamples.Length);
            return new FrequencyDomain(0, windowedSamples.Length, _length, result, this, window);
        }
    }


    /// <summary>
    /// 获取或设置指定索引处的采样值。
    /// </summary>
    /// <param name="index">采样值的索引。</param>
    /// <returns>指定索引处的采样值。</returns>
    public float this[int index]
    {
        get => Samples[index];
        set => Samples[index] = value;
    }

    /// <summary>
    /// 获取信号段的子段。
    /// </summary>
    /// <param name="start">子段的起始位置。</param>
    /// <param name="length">子段的长度。</param>
    /// <returns>指定起始位置和长度的信号子段。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当起始位置或长度不在有效范围内时抛出。</exception>
    public SignalSegment this[int start, int length]
    {
        get
        {
            if (start < 0 || start >= _length)
                throw new ArgumentOutOfRangeException(nameof(start), "Start index is out of range.");

            if (length <= 0 || length > _length || (start + length) > _length)
                throw new ArgumentOutOfRangeException(nameof(length), "Length is out of range.");

            return new SignalSegment(this, start, length);
        }
    }


}
