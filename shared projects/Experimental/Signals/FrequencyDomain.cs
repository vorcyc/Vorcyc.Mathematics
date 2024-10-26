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


    public int Offset => _offset;


    public int TransformLength => _transformLength;


    public int ActualLength => _actualLength;


    public float Resolution => _resolution;


    public WindowType? WindowApplied => _windowType;


    public ComplexFp32[] Result => _fftResult;


    public ITimeDomainSignal Signal => _signal;



    public float[] Magnitudes => IFrequencyDomainCharacteristics.GetMagnitudes(_fftResult, _actualLength);

    public float Centroid => IFrequencyDomainCharacteristics.GetCentroid(_fftResult, _actualLength, _signal.SamplingRate);

    public float Frequency => IFrequencyDomainCharacteristics.GetFrequency(Magnitudes, _signal.SamplingRate, _resolution);

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


    public float[] Phases => IFrequencyDomainCharacteristics.GetPhases(_fftResult, _actualLength);

    public float[] AngularVelocities => IFrequencyDomainCharacteristics.GetAngularVelocities(Phases, _signal.SamplingRate);


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
