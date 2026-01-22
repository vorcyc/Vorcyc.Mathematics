using Vorcyc.Mathematics.Buffers;
using Vorcyc.Mathematics.Helpers;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// Represents a single-threaded time-domain signal wrapper that provides sample storage,
/// time/energy/power statistics, frequency-domain transforms, and resampling.
/// </summary>
public class Signal : ISingleThreadTimeDomainSignal, ICloneable<Signal>, IDisposable, IEquatable<Signal>
{
    internal POHBuffer<float>? _buffer;

    private volatile int _length;
    private readonly float _samplingRate;

    /// <summary>
    /// Initializes a signal with the specified sample count and sampling rate.
    /// </summary>
    /// <param name="sampleCount">The number of samples. Must be greater than 0.</param>
    /// <param name="samplingRate">The sampling rate in Hz.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="sampleCount"/> or <paramref name="samplingRate"/> is less than or equal to 0.
    /// </exception>
    public Signal(int sampleCount, float samplingRate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sampleCount, 0, nameof(sampleCount));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(samplingRate, 0f, nameof(samplingRate));

        _length = sampleCount;
        _samplingRate = samplingRate;
        _buffer = new(sampleCount);
    }

    /// <summary>
    /// Initializes a signal with the specified duration and sampling rate.
    /// </summary>
    /// <param name="duration">The signal duration.</param>
    /// <param name="samplingRate">The sampling rate in Hz.</param>
    public Signal(TimeSpan duration, float samplingRate)
        : this(ITimeDomainSignal.TimeToArrayIndexOrLength(duration, samplingRate), samplingRate)
    {
    }


    #region Signal Properties

    /// <summary>
    /// Gets the underlying pinned buffer.
    /// </summary>
    public POHBuffer<float> UnderlyingBuffer
    {
        get
        {
            ThrowIfDisposed();
            return _buffer!;
        }
    }

    /// <summary>
    /// Gets a <see cref="Span{T}"/> view over the sample data.
    /// </summary>
    public Span<float> Samples
    {
        get
        {
            ThrowIfDisposed();
            return _buffer!.Span;
        }
    }

    /// <inheritdoc cref="ITimeDomainSignal.Length"/>
    public int Length => _length;

    /// <inheritdoc cref="ITimeDomainSignal.SamplingRate"/>
    public float SamplingRate => _samplingRate;

    /// <inheritdoc cref="ITimeDomainSignal.Duration"/>
    public TimeSpan Duration => ITimeDomainSignal.ArrayIndexOrLengthToTime(_length, _samplingRate);

    /// <inheritdoc cref="ITimeDomainSignal.NotifySamplesModified"/>
    public void NotifySamplesModified()
    {
        ThrowIfDisposed();
        // 清除所有延迟计算缓存
        ClearAllCaches();
    }

    #endregion


    #region Time-Domain Characteristics

    private float? _amplitude = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Amplitude"/>
    public float Amplitude => _amplitude ??= ITimeDomainCharacteristics.GetAmplitude_SIMD(Samples);


    /// <inheritdoc cref="ITimeDomainCharacteristics.Period"/>
    public float Period => 1f / _samplingRate;

    private float? _totalPower = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.TotalPower"/>
    public float TotalPower => _totalPower ??= ITimeDomainCharacteristics.GetTotalPower_SIMD(Samples);

    private float? _averagePower = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.AveragePower"/>
    public float AveragePower => _averagePower ??= ITimeDomainCharacteristics.GetAveragePower_SIMD(Samples);

    private float? _totalEnergy = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.TotalEnergy"/>
    public float TotalEnergy => _totalEnergy ??= ITimeDomainCharacteristics.GetTotalEnergy_SIMD(Samples);

    private float? _averageEnergy = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.AverageEnergy"/>
    public float AverageEnergy => _averageEnergy ??= ITimeDomainCharacteristics.GetAverageEnergy_SIMD(Samples);

    private float? _rms = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Rms"/>
    public float Rms => _rms ??= ITimeDomainCharacteristics.GetRms_SIMD(Samples);

    private float? _zeroCrossingRate = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.ZeroCrossingRate"/>
    public float ZeroCrossingRate => _zeroCrossingRate ??= ITimeDomainCharacteristics.GetZeroCrossingRate_NEWSIMD_Grok(Samples);

    private float? _entropy = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Entropy"/>
    public float Entropy => _entropy ??= ITimeDomainCharacteristics.GetEntropy_SIMD(Samples);


    /// <inheritdoc cref="ITimeDomainCharacteristics.GetEntropy(int)"/>
    public float GetEntropy(int binCount = 32)
    {
        ThrowIfDisposed();
        return ITimeDomainCharacteristics.GetEntropy_SIMD(Samples, binCount);
    }

    /// <summary>
    /// Clears all lazily computed cached characteristic values.
    /// </summary>
    protected void ClearAllCaches()
    {
        _amplitude = null;
        _totalPower = null;
        _averagePower = null;
        _totalEnergy = null;
        _averageEnergy = null;
        _rms = null;
        _zeroCrossingRate = null;
        _entropy = null;
    }

    #endregion


    #region IClone<T>

    /// <summary>
    /// Creates a deep copy of this signal, including its sample buffer and metadata.
    /// </summary>
    /// <returns>A new <see cref="Signal"/> instance.</returns>
    public Signal Clone()
    {
        ThrowIfDisposed();
        var result = new Signal(_length, _samplingRate);
        _buffer!.Span.CopyTo(result._buffer!.Span);
        return result;
    }

    #endregion


    #region To Frequency-domain

    /// <inheritdoc cref="ITimeDomainSignal.TransformToFrequencyDomain(WindowType?, FftVersion)"/>
    public FrequencyDomain TransformToFrequencyDomain(WindowType? window = null, FftVersion fftVersion = FftVersion.Normal)
    {
        ThrowIfDisposed();
        FastFourierTransform.Version = fftVersion;

        if (window is null && _length.IsPowerOf2())//若不应用窗函数，则直接使用补过 0 后的样本进行变换
        {
            var result = new ComplexFp32[_length];
            FastFourierTransform.Forward(_buffer!.Span, result);
            return new FrequencyDomain(0, _length, _length, result, this, window);
        }
        else//由于窗函数需要修改样本值，所以只要使用窗函数都需要创建临时副本
        {
            var windowedSamples = ITimeDomainSignal.PadZerosAndWindowing(_buffer!.Span, _length.NextPowerOf2(), window);
            FastFourierTransform.Forward(windowedSamples, 0, out var result, windowedSamples.Length);
            return new FrequencyDomain(0, windowedSamples.Length, _length, result, this, window);
        }
    }


    #endregion


    #region Resample

    /// <inheritdoc cref="ITimeDomainSignal.Resample(int, FirFilter?, int)"/>
    public Signal Resample(int destnationSamplingRate, FirFilter? filter = null, int order = 15)
    {
        ThrowIfDisposed();
        return SignalResamplingExtension.Resample(this, destnationSamplingRate, filter, order);
    }

    #endregion


    #region Indexer


    /// <summary>
    /// Gets or sets the sample value at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the sample.</param>
    /// <returns>The sample value at the specified index.</returns>
    /// <exception cref="ObjectDisposedException">The signal has been disposed.</exception>
    public float this[int index]
    {
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return Samples[index];
        }
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        set
        {
            ThrowIfDisposed();
            Samples[index] = value;
        }
    }



    /// <summary>
    /// Gets a signal segment by index-based start position and length.
    /// </summary>
    /// <param name="start">The start index of the segment.</param>
    /// <param name="length">The length of the segment.</param>
    /// <param name="throwException">
    /// If <see langword="true"/>, throws on out-of-range arguments;
    /// if <see langword="false"/>, returns <see langword="null"/> instead.
    /// </param>
    /// <returns>A <see cref="SignalSegment"/> for the specified range, or <see langword="null"/> if out of bounds and <paramref name="throwException"/> is <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> or <paramref name="length"/> is out of range and <paramref name="throwException"/> is <see langword="true"/>.</exception>
    /// <exception cref="ObjectDisposedException">The signal has been disposed.</exception>
    public SignalSegment? this[int start, int length, bool throwException = false]
    {
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();

            if (throwException)
            {
                if (start < 0 || start >= _length)
                    throw new ArgumentOutOfRangeException(nameof(start), "Start index is out of range.");

                if (length <= 0 || length > _length || (start + length) > _length)
                    throw new ArgumentOutOfRangeException(nameof(length), "Length is out of range.");
            }
            else
            {
                if (start < 0 || start >= _length)
                    return null;

                if (length <= 0 || length > _length || (start + length) > _length)
                    return null;
            }

            return new SignalSegment(this, start, length);
        }
    }

    /// <summary>
    /// Gets a signal segment by time-based start position and duration.
    /// </summary>
    /// <param name="startTime">The start time of the segment.</param>
    /// <param name="duration">The duration of the segment.</param>
    /// <param name="throwException">
    /// If <see langword="true"/>, throws on out-of-range arguments;
    /// if <see langword="false"/>, returns <see langword="null"/> instead.
    /// </param>
    /// <returns>A <see cref="SignalSegment"/> for the specified time range, or <see langword="null"/> if out of bounds and <paramref name="throwException"/> is <see langword="false"/>.</returns>
    public SignalSegment? this[TimeSpan startTime, TimeSpan duration, bool throwException = false]
        => this
        [
            ITimeDomainSignal.TimeToArrayIndexOrLength(startTime, _samplingRate),
            ITimeDomainSignal.TimeToArrayIndexOrLength(duration, _samplingRate)
        ];



    #endregion


    #region IEquatable<Signal>

    /// <summary>
    /// Determines whether this signal is equal to another by comparing the underlying
    /// buffer pointer, length, and sampling rate.
    /// </summary>
    /// <param name="other">The signal to compare with.</param>
    /// <returns><see langword="true"/> if the signals share the same buffer identity, length, and sampling rate; otherwise, <see langword="false"/>.</returns>
    public unsafe bool Equals(Signal? other)
    {
        if (other is null) return false;
        if (_length != other._length) return false;

        return _buffer!.UnmanagedPointer == other._buffer!.UnmanagedPointer &&
               _samplingRate == other._samplingRate;
    }

    /// <summary>
    /// Determines whether two <see cref="Signal"/> instances are equal.
    /// </summary>
    public static bool operator ==(Signal? left, Signal? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Signal"/> instances are not equal.
    /// </summary>
    public static bool operator !=(Signal? left, Signal? right)
    {
        return !(left == right);
    }

    #endregion


    #region overrides

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Signal other && Equals(other);
    }

    /// <inheritdoc />
    public override unsafe int GetHashCode()
    {
        ThrowIfDisposed();
        return ((nint)_buffer!.UnmanagedPointer, _length, _samplingRate).GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        ThrowIfDisposed();
        return _buffer!.ToString();
    }

    #endregion


    #region Operators

    public static Signal operator +(Signal left, float right)
    {
        var result = left.Clone();
        result.Samples.Add(right);
        return result;
    }

    public static Signal? operator +(Signal left, Signal right)
    {
        if (left.Length != right.Length || left.SamplingRate != right.SamplingRate)
            return null;
        var result = left.Clone();
        result.Samples.Add(right.Samples);
        return result;
    }


    public static Signal operator -(Signal left, float right)
    {
        var result = left.Clone();
        result.Samples.Subtract(right);
        return result;
    }

    public static Signal? operator -(Signal left, Signal right)
    {
        if (left.Length != right.Length || left.SamplingRate != right.SamplingRate)
            return null;
        var result = left.Clone();
        result.Samples.Subtract(right.Samples);
        return result;
    }

    public static Signal operator *(Signal left, float right)
    {
        var result = left.Clone();
        result.Samples.Multiply(right);
        return result;
    }

    public static Signal? operator *(Signal left, Signal right)
    {
        if (left.Length != right.Length || left.SamplingRate != right.SamplingRate)
            return null;
        var result = left.Clone();
        result.Samples.Multiply(right.Samples);
        return result;
    }

    public static Signal operator /(Signal left, float right)
    {
        var result = left.Clone();
        result.Samples.Divide(right);
        return result;
    }


    #endregion


    #region Dispose pattern

    private bool _isDisposed;

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if this instance has been disposed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // 释放托管资源
                _buffer?.Dispose();
                _buffer = null;

            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// Releases all managed and unmanaged resources and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        // 请勿修改此方法，清理代码应放入 'Dispose(bool disposing)' 中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer. Releases unmanaged resources if <see cref="Dispose()"/> was not called explicitly.
    /// </summary>
    ~Signal()
    {
        Dispose(disposing: false);
    }

    #endregion

}