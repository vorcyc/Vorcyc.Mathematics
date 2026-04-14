using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// Represents a frequency-domain signal derived from a time-domain signal using a Fourier transform.
/// </summary>
public readonly struct FrequencyDomain : IFrequencyDomain
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
        _resolution = (float)timeDomainSignal.SamplingRate / transformLength;
    }

    public int Offset => _offset;

    public int ActualLength => _actualLength;

    public int TransformLength => _transformLength;

    public float Resolution => _resolution;

    public WindowType? WindowApplied => _windowType;

    public ComplexFp32[] Result => _fftResult;

    public ITimeDomainSignal Signal => _signal;

    public float[] Magnitudes => IFrequencyDomainCharacteristics.GetMagnitudes(_fftResult, _transformLength >> 1);

    public float Centroid => IFrequencyDomainCharacteristics.GetCentroid(Magnitudes, _signal.SamplingRate);

    public float Frequency => IFrequencyDomainCharacteristics.GetFrequency(Magnitudes, _signal.SamplingRate, _resolution);

    public float[] Phases => IFrequencyDomainCharacteristics.GetPhases(_fftResult, _transformLength >> 1);

    public float[] AngularVelocities => IFrequencyDomainCharacteristics.GetAngularVelocities(Phases, _signal.SamplingRate);

    public float[] PowerSpectralDensity => IFrequencyDomainCharacteristics.GetPowerSpectralDensity(Magnitudes, _signal.SamplingRate);

    public static float IndexToFrequency(int index, float samplingRate, int fftLen)
    {
        if (index > fftLen)
        {
            return 0.0f;
        }

        if (index <= fftLen / 2)
        {
            return index * samplingRate / fftLen;
        }

        return -(fftLen - index) / (float)fftLen * samplingRate;
    }

    public float IndexToFrequency(int index)
    {
        if (index > ActualLength)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (index <= ActualLength / 2)
        {
            return index * _resolution;
        }

        return -(ActualLength - index) / (float)ActualLength * _signal.SamplingRate;
    }

    public int FrequencyToIndex(float frequency) => (int)(frequency / _resolution);

    public void Inverse()
    {
        Vorcyc.Mathematics.SignalProcessing.Fourier.FastFourierTransform.Inverse(_fftResult);

        for (int signalIndex = _offset, freqIndex = 0;
             signalIndex < (_offset + _actualLength);
             signalIndex++, freqIndex++)
        {
            if (_signal is ISingleThreadTimeDomainSignal singleThreadSignal)
            {
                singleThreadSignal.Samples[signalIndex] = _fftResult[freqIndex].Magnitude;
            }
            else if (_signal is IModifiableTimeDomainSignal modifiableSignal)
            {
                using var lockedSamples = modifiableSignal.Samples;
                lockedSamples.Span[signalIndex] = _fftResult[freqIndex].Magnitude;
            }
        }
    }
}
