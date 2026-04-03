using System.Diagnostics.CodeAnalysis;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.Experimental.Signals;


/// <summary>
/// Represents a read-only view over a contiguous segment of a <see cref="Signal"/>,
/// providing lazily computed time-domain characteristics and frequency-domain transforms.
/// </summary>
public readonly struct SignalSegment : ITimeDomainSignal, ISingleThreadTimeDomainSignal
{

    private readonly Signal _signal;
    private readonly int _start, _length;
    private readonly TimeSpan _startTime, _duration;
    private readonly Lazy<float> _lazyAmplitude;
    private readonly Lazy<float> _lazyTotalPower;
    private readonly Lazy<float> _lazyAveragePower;
    private readonly Lazy<float> _lazyTotalEnergy;
    private readonly Lazy<float> _lazyAverageEnergy;
    private readonly Lazy<float> _lazyRms;
    private readonly Lazy<float> _lazyZeroCrossingRate;
    private readonly Lazy<float> _lazyEntropy;

    internal SignalSegment(Signal signal, int start, int length)
    {
        _signal = signal;
        _start = start; _length = length;
        _startTime = ITimeDomainSignal.ArrayIndexOrLengthToTime(_start, _signal.SamplingRate);
        _duration = ITimeDomainSignal.ArrayIndexOrLengthToTime(_length, _signal.SamplingRate);

        //var span = signal._baseSamples!.AsSpan(_start, _length);
        _lazyAmplitude = new Lazy<float>(() => ITimeDomainCharacteristics.GetAmplitude_SIMD(signal._buffer![start, length]), isThreadSafe: true);
        _lazyTotalPower = new Lazy<float>(() => ITimeDomainCharacteristics.GetTotalPower_SIMD(signal._buffer![start, length]), isThreadSafe: true);
        _lazyAveragePower = new Lazy<float>(() => ITimeDomainCharacteristics.GetAveragePower_SIMD(signal._buffer![start, length]), isThreadSafe: true);
        _lazyTotalEnergy = new Lazy<float>(() => ITimeDomainCharacteristics.GetTotalEnergy_SIMD(signal._buffer![start, length]), isThreadSafe: true);
        _lazyAverageEnergy = new Lazy<float>(() => ITimeDomainCharacteristics.GetAverageEnergy_SIMD(signal._buffer![start, length]), isThreadSafe: true);
        _lazyRms = new Lazy<float>(() => ITimeDomainCharacteristics.GetRms_SIMD(signal._buffer![start, length]), isThreadSafe: true);
        _lazyZeroCrossingRate = new Lazy<float>(() => ITimeDomainCharacteristics.GetZeroCrossingRate_NEWSIMD_Grok(signal._buffer![start, length]), isThreadSafe: true);
        _lazyEntropy = new Lazy<float>(() => ITimeDomainCharacteristics.GetEntropy_SIMD(signal._buffer![start, length]), isThreadSafe: true);

    }

    /// <summary>
    /// Gets the parent <see cref="Signals.Signal"/> that this segment belongs to.
    /// </summary>
    public Signal Signal => _signal;

    /// <summary>
    /// Gets the starting index of this segment within the parent signal's sample buffer.
    /// </summary>
    public int Start => _start;

    /// <summary>
    /// Gets the start time of this segment relative to the beginning of the parent signal.
    /// </summary>
    public TimeSpan StartTime => _startTime;


    #region Signal Properties


    /// <summary>
    /// Gets a <see cref="Span{T}"/> view over the sample data of this segment.
    /// </summary>
    public Span<float> Samples => _signal._buffer![_start, _length];

    /// <inheritdoc cref="ITimeDomainSignal.Length"/>
    public int Length => _length;

    /// <inheritdoc cref="ITimeDomainSignal.SamplingRate"/>
    public float SamplingRate => _signal.SamplingRate;

    /// <inheritdoc cref="ITimeDomainSignal.Duration"/>
    public TimeSpan Duration => _duration;

    /// <inheritdoc cref="ITimeDomainSignal.NotifySamplesModified"/>
    public void NotifySamplesModified()
    {
        // 清除所有延迟计算缓存
        _signal.NotifySamplesModified();
    }

    #endregion


    #region Time-Domain Characteristics

    ///// <inheritdoc cref="ITimeDomainCharacteristics.Amplitude"/>
    //public float Amplitude => ITimeDomainCharacteristics.GetAmplitude_SIMD(Samples);

    ///// <inheritdoc cref="ITimeDomainCharacteristics.Period"/>
    //public float Period => _signal.Period;

    ///// <inheritdoc cref="ITimeDomainCharacteristics.TotalPower"/>
    //public float TotalPower => ITimeDomainCharacteristics.GetTotalPower_SIMD(Samples);

    ///// <inheritdoc cref="ITimeDomainCharacteristics.AveragePower"/>
    //public float AveragePower => ITimeDomainCharacteristics.GetAveragePower_SIMD(Samples);

    ///// <inheritdoc cref="ITimeDomainCharacteristics.TotalEnergy"/>
    //public float TotalEnergy => ITimeDomainCharacteristics.GetTotalEnergy_SIMD(Samples);

    ///// <inheritdoc cref="ITimeDomainCharacteristics.AverageEnergy"/>
    //public float AverageEnergy => ITimeDomainCharacteristics.GetAverageEnergy_SIMD(Samples);

    ///// <inheritdoc cref="ITimeDomainCharacteristics.Rms"/>
    //public float Rms => ITimeDomainCharacteristics.GetRms_SIMD(Samples);

    ///// <inheritdoc cref="ITimeDomainCharacteristics.ZeroCrossingRate"/>
    //public float ZeroCrossingRate => ITimeDomainCharacteristics.GetZeroCrossingRate_NEWSIMD_Grok(Samples);

    ///// <inheritdoc cref="ITimeDomainCharacteristics.Entropy"/>
    //public float Entropy => ITimeDomainCharacteristics.GetEntropy_SIMD(Samples);

    /// <inheritdoc cref="ITimeDomainCharacteristics.Amplitude"/>
    public float Amplitude => _lazyAmplitude.Value;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Period"/>
    public float Period => _signal.Period;

    /// <inheritdoc cref="ITimeDomainCharacteristics.TotalPower"/>
    public float TotalPower => _lazyTotalPower.Value;

    /// <inheritdoc cref="ITimeDomainCharacteristics.AveragePower"/>
    public float AveragePower => _lazyAveragePower.Value;

    /// <inheritdoc cref="ITimeDomainCharacteristics.TotalEnergy"/>
    public float TotalEnergy => _lazyTotalEnergy.Value;

    /// <inheritdoc cref="ITimeDomainCharacteristics.AverageEnergy"/>
    public float AverageEnergy => _lazyAverageEnergy.Value;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Rms"/>
    public float Rms => _lazyRms.Value;

    /// <inheritdoc cref="ITimeDomainCharacteristics.ZeroCrossingRate"/>
    public float ZeroCrossingRate => _lazyZeroCrossingRate.Value;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Entropy"/>
    public float Entropy => _lazyEntropy.Value;

    /// <inheritdoc cref="ITimeDomainCharacteristics.GetEntropy(int)"/>
    public float GetEntropy(int binCount = 32) => ITimeDomainCharacteristics.GetEntropy_SIMD(Samples, binCount);


    #endregion


    #region Decouple
    
    /// <summary>
    /// Decouples this segment from its parent signal and returns an independent <see cref="Signal"/>
    /// containing a copy of the segment's sample data.
    /// </summary>
    /// <returns>A new standalone <see cref="Signal"/> instance.</returns>
    public Signal Decouple()
    {
        var result = new Signal(this._length, this._signal.SamplingRate);
        this.Samples.CopyTo(result.Samples);
        return result;
    }

    #endregion


    #region To Frequency-domain    

    /// <inheritdoc cref="ITimeDomainSignal.TransformToFrequencyDomain(WindowType?, FftVersion)"/>
    public FrequencyDomain TransformToFrequencyDomain(WindowType? window = null, FftVersion fftVersion = FftVersion.Normal)
    {
        FastFourierTransform.Version = fftVersion;
        if (window is null && _length.IsPowerOf2())
        {
            var result = new Vorcyc.Mathematics.Numerics.ComplexFp32[_length];
            FastFourierTransform.Forward(Samples, result);
            return new FrequencyDomain(_start, _length, _length, result, this, null);
        }
        else
        {
            var windowedSamples = ITimeDomainSignal.PadZerosAndWindowing(Samples, _length.NextPowerOf2(), window);
            FastFourierTransform.Forward(windowedSamples, 0, out var result, windowedSamples.Length);
            return new FrequencyDomain(_start, windowedSamples.Length, _length, result, this, window);
        }
    }

    #endregion


    #region Resample    

    /// <summary>
    /// Resamples this segment to the specified sampling rate and returns a new independent <see cref="Signal"/>.
    /// </summary>
    /// <param name="destnationSamplingRate">The target sampling rate in Hz.</param>
    /// <param name="filter">An optional FIR anti-aliasing filter. If <see langword="null"/>, one is created automatically when downsampling.</param>
    /// <param name="order">The sinc interpolation kernel half-width. Defaults to 15.</param>
    /// <returns>A new <see cref="Signal"/> resampled at the target rate.</returns>
    public Signal Resample(
            int destnationSamplingRate,
            FirFilter? filter = null,
            int order = 15)
    {
        return SignalResamplingExtension.Resample(this, destnationSamplingRate, filter, order);
    }

    #endregion


    #region overrides

    /// <summary>
    /// Determines whether the specified object is equal to the current SignalSegment instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current SignalSegment instance.</param>
    /// <returns>true if the specified object is a SignalSegment and has the same signal, start, and length as the current
    /// instance; otherwise, false.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SignalSegment segment &&
               EqualityComparer<Signal>.Default.Equals(_signal, segment._signal) &&
               _start == segment._start &&
               _length == segment._length;
    }

    /// <summary>
    /// Returns a string that represents the current object, including a comma-separated list of sample values and the total
    /// item count.
    /// </summary>
    /// <remarks>If the collection contains more than 50 items, only the first 50 are included in the output, followed
    /// by an ellipsis to indicate additional items.</remarks>
    /// <returns>A string representation of the object that lists up to 50 sample values, followed by the total number of items in
    /// the collection.</returns>
    public override string ToString()
    {
        const int Max = 50;
        var sb = new System.Text.StringBuilder("[");
        int count = Math.Min(_length, Max);
        for (int i = 0; i < count; i++)
        {
            sb.Append(Samples[i]);
            if (i < count - 1) sb.Append(", ");
        }
        if (_length > Max) sb.Append("... ");
        sb.Append($"({_length} items)]");
        return sb.ToString();
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        //return (_signal.GetHashCode(), _start, _length).GetHashCode();
        return HashCode.Combine(_signal.GetHashCode(), _start, _length);
    }

    #endregion

}