using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.Experimental.Signals;

public class FrequencyDomain : IFrequencyDomain
{

    private readonly float _resolution;
    private readonly int _offset, _transformLength, _actualLength;

    private readonly ComplexFp32[] _fftResult;

    private readonly ITimeDomainSignal _signal;
    private readonly WindowType? _windowType;

    internal FrequencyDomain(
        int offset,
        int transformLength,
        int actualLength,
        ComplexFp32[] frequencySignal,
        ITimeDomainSignal timeDomainSignal,
        WindowType? window = null)
    {
        _offset = offset;
        _transformLength = transformLength;
        _actualLength = actualLength;


        _fftResult = frequencySignal;
        _signal = timeDomainSignal;
        _windowType = window;

        //频率分辨率用的是补 0 后的点数
        _resolution = (float)timeDomainSignal.SamplingRate / (float)transformLength;
    }

    /// <summary>
    /// 频域信号在原始数据中的起始量。
    /// </summary>
    public int Offset => _offset;


    /// <summary>
    /// 未 补0 的长度。也是实际有效数据的长度。
    /// </summary>
    public int ActualLength => _actualLength;

    /// <summary>
    /// FFT 的长度。该值为 2的N次方，通常会比 <see cref="ActualLength"/> 大。
    /// </summary>
    public int TransformLength => _transformLength;


    /// <summary>
    /// 频率分辨率。
    /// </summary>
    public float Resolution => _resolution;

    /// <summary>
    /// FFT 所使用的窗口类型。
    /// </summary>
    public WindowType? WindowApplied => _windowType;

    /// <summary>
    /// 频域信号。即FFT变换的结果。
    /// </summary>
    public ComplexFp32[] Result => _fftResult;

    /// <summary>
    /// 所关联的实信号。
    /// </summary>
    public ITimeDomainSignal Signal => _signal;


    /// <summary>
    /// 使用复信号的一半（<see cref="Result"/> 的一半）来计算幅度。这样会丢弃镜像部分。
    /// </summary>
    public float[] Magnitudes => IFrequencyDomainCharacteristics.GetMagnitudes(_fftResult, _transformLength >> 1);

    /// <summary>
    /// 使用复信号的一半（<see cref="Result"/> 的一半）来计算质心（Mass Center）。这样会丢弃镜像部分。
    /// </summary>
    public float Centroid => IFrequencyDomainCharacteristics.GetCentroid(Magnitudes, _signal.SamplingRate);

    /// <summary>
    /// 使用完整的复信号（未截取<see cref="Result"/> 的一半）来计算频率。
    /// </summary>
    public float Frequency => IFrequencyDomainCharacteristics.GetFrequency(Magnitudes, _signal.SamplingRate, _resolution);

    /// <summary>
    /// 使用复信号的一半（<see cref="Result"/> 的一半）来计算相位。这样会丢弃镜像部分。
    /// </summary>
    public float[] Phases => IFrequencyDomainCharacteristics.GetPhases(_fftResult, _transformLength >> 1);

    /// <summary>
    /// 根据复信号的相位计算角速度。
    /// </summary>
    public float[] AngularVelocities => IFrequencyDomainCharacteristics.GetAngularVelocities(Phases, _signal.SamplingRate);


    /// <summary>
    /// 功率谱密度
    /// </summary>
    public float[] PowerSpectralDensity => IFrequencyDomainCharacteristics.GetPowerSpectralDensity(Magnitudes, _signal.SamplingRate);


    public static float IndexToFrequency(int index, int samplingRate, int fftLen)
    {
        if (index > fftLen)
            return 0.0f;
        else if (index <= fftLen / 2)
        {
            //分辨率
            var resolution = (float)samplingRate / fftLen;
            return index * resolution;
            //两种表达式等价
            //return index / fftLen * samplingRate;
        }
        else
            return -(fftLen - index) / fftLen * samplingRate;
    }


    public float IndexToFrequency(int index)
    {
        if (index > ActualLength)
            throw new ArgumentOutOfRangeException(nameof(index));
        else if (index <= ActualLength / 2)
            return index * _resolution;//小于一半，返回真实频率
        else
            return -(ActualLength - index) / (float)ActualLength * _signal.SamplingRate;//大于一半，返回负频率
    }


    public int FrequencyToIndex(float frequency)
    {
        return (int)(frequency / _resolution);
    }



    /// <summary>
    /// 对 FFT 结果进行逆变换，并将结果写回信号的采样数据中。
    /// </summary>
    public void Inverse()
    {
        // 执行 FFT 逆变换
        FastFourierTransform.Inverse(_fftResult);

        // 将逆变换结果写回信号的采样数据中
        for (int signalIndex = _offset, freqIndex = 0;
             signalIndex < (_offset + _actualLength);
             signalIndex++, freqIndex++)
        {
            _signal.Samples[signalIndex] = _fftResult[freqIndex].Magnitude; // 使用幅度代替实部
        }
    }




}
