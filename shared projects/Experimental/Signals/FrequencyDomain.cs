using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// Represents a frequency-domain signal derived from a time-domain signal using a Fourier transform. Provides access to
/// frequency-domain characteristics, including magnitudes, phases, power spectral density, and conversion between
/// frequency bins and values.
/// </summary>
/// <remarks>This struct encapsulates the results of a Fourier transform, including the original signal, windowing
/// information, and computed frequency-domain features. It supports conversion between frequency bin indices and
/// frequency values, as well as calculation of spectral characteristics such as centroid and angular velocities. The
/// struct is immutable and thread-safe for read operations. Use the Inverse method to perform an inverse transform and
/// update the associated time-domain signal.</remarks>
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

        //频率分辨率用的是补 0 后的点数
        _resolution = (float)timeDomainSignal.SamplingRate / (float)transformLength;
    }

    /// <summary>
    /// Gets the starting offset of the frequency-domain signal within the original data.
    /// </summary>
    public int Offset => _offset;

    /// <summary>
    /// Gets the length without zero-padding. This is the actual effective data length.
    /// </summary>
    public int ActualLength => _actualLength;

    /// <summary>
    /// Gets the FFT length. This value is a power of 2 and is typically greater than <see cref="ActualLength"/>.
    /// </summary>
    public int TransformLength => _transformLength;


    /// <summary>
    /// Gets the frequency resolution.
    /// </summary>
    public float Resolution => _resolution;

    /// <summary>
    /// Gets the window type used for the FFT.
    /// </summary>
    public WindowType? WindowApplied => _windowType;

    /// <summary>
    /// Gets the frequency-domain signal, i.e., the result of the FFT transform.
    /// </summary>
    public ComplexFp32[] Result => _fftResult;

    /// <summary>
    /// Gets the associated real signal.
    /// </summary>
    public ITimeDomainSignal Signal => _signal;


    /// <summary>
    /// Calculates the magnitudes using half of the complex signal (half of <see cref="Result"/>), discarding the mirror portion.
    /// </summary>
    public float[] Magnitudes => IFrequencyDomainCharacteristics.GetMagnitudes(_fftResult, _transformLength >> 1);

    /// <summary>
    /// Calculates the spectral centroid (mass center) using half of the complex signal (half of <see cref="Result"/>), discarding the mirror portion.
    /// </summary>
    public float Centroid => IFrequencyDomainCharacteristics.GetCentroid(Magnitudes, _signal.SamplingRate);

    /// <summary>
    /// Calculates the frequency using the full complex signal (without truncating <see cref="Result"/> to half).
    /// </summary>
    public float Frequency => IFrequencyDomainCharacteristics.GetFrequency(Magnitudes, _signal.SamplingRate, _resolution);

    /// <summary>
    /// Calculates the phases using half of the complex signal (half of <see cref="Result"/>), discarding the mirror portion.
    /// </summary>
    public float[] Phases => IFrequencyDomainCharacteristics.GetPhases(_fftResult, _transformLength >> 1);

    /// <summary>
    /// Calculates the angular velocities from the phases of the complex signal.
    /// </summary>
    public float[] AngularVelocities => IFrequencyDomainCharacteristics.GetAngularVelocities(Phases, _signal.SamplingRate);


    /// <summary>
    /// Gets the power spectral density.
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

    /// <summary>
    /// Converts a frequency bin index to its corresponding frequency value based on the signal's sampling rate and
    /// resolution.
    /// </summary>
    /// <remarks>For indices less than or equal to half the actual length, the method returns the positive
    /// frequency. For indices greater than half the actual length, the method returns the negative frequency, which is
    /// relevant in the context of discrete Fourier transforms.</remarks>
    /// <param name="index">The zero-based index of the frequency bin to convert. Must be within the valid range of frequency bins.</param>
    /// <returns>The frequency value, in hertz, corresponding to the specified bin index. Returns a negative frequency for
    /// indices greater than half the actual length.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified index is greater than the actual length of the frequency bins.</exception>
    public float IndexToFrequency(int index)
    {
        if (index > ActualLength)
            throw new ArgumentOutOfRangeException(nameof(index));
        else if (index <= ActualLength / 2)
            return index * _resolution;//小于一半，返回真实频率
        else
            return -(ActualLength - index) / (float)ActualLength * _signal.SamplingRate;//大于一半，返回负频率
    }

    /// <summary>
    /// Converts a frequency value to its corresponding index based on the current resolution.
    /// </summary>
    /// <remarks>The resolution used for conversion is determined by the value of the '_resolution' field. The
    /// returned index may be truncated if the frequency is not an exact multiple of the resolution.</remarks>
    /// <param name="frequency">The frequency to convert to an index. Must be a non-negative value.</param>
    /// <returns>The index corresponding to the specified frequency, calculated using the current resolution.</returns>
    public int FrequencyToIndex(float frequency)
    {
        return (int)(frequency / _resolution);
    }

    /// <inheritdoc cref="IFrequencyDomain.Inverse()"/>
    public void Inverse()
    {
        // 执行 FFT 逆变换
        Vorcyc.Mathematics.SignalProcessing.Fourier.FastFourierTransform.Inverse(_fftResult);

        // 将逆变换结果写回信号的采样数据中
        for (int signalIndex = _offset, freqIndex = 0;
             signalIndex < (_offset + _actualLength);
             signalIndex++, freqIndex++)
        {
            if (_signal is ISingleThreadTimeDomainSignal singleThreadSignal)
                singleThreadSignal.Samples[signalIndex] = _fftResult[freqIndex].Magnitude; // 使用幅度代替实部
            else if (_signal is IModifiableTimeDomainSignal modifiableSignal)
            {
                using var lockedSamples = modifiableSignal.Samples;
                var samples = lockedSamples.Span;
                samples[signalIndex] = _fftResult[freqIndex].Magnitude; // 使用幅度代替实部
            }
        }
    }




}