using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Buffers;
using Vorcyc.Mathematics.Helpers;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// Represents a single-threaded time-domain signal with pinned sample storage,
/// cached time-domain statistics, frequency-domain transforms, and resampling.
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
    /// <param name="samplingRate">The sampling rate in Hz. Must be greater than 0.</param>
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
    /// <param name="samplingRate">The sampling rate in Hz. Must be greater than 0.</param>
    public Signal(TimeSpan duration, float samplingRate)
        : this(ITimeDomainSignal.TimeToArrayIndexOrLength(duration, samplingRate), samplingRate)
    {
    }

    /// <summary>
    /// Creates a unit impulse signal (first sample 1, remainder 0).
    /// </summary>
    public static Signal Unit(int length, float samplingRate = 1f)
    {
        var signal = new Signal(length, samplingRate);
        if (length > 0)
        {
            signal.Samples[0] = 1f;
        }

        return signal;
    }

    /// <summary>
    /// Creates a constant-valued signal.
    /// </summary>
    public static Signal Constant(float value, int length, float samplingRate = 1f)
    {
        var signal = new Signal(length, samplingRate);
        signal.Samples.Fill(value);
        return signal;
    }

    /// <summary>
    /// Creates a signal by copying samples into a new owned buffer.
    /// </summary>
    public static Signal FromCopy(ReadOnlySpan<float> samples, float samplingRate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(samplingRate, 0f, nameof(samplingRate));
        if (samples.Length == 0)
        {
            throw new ArgumentException("At least one sample is required.", nameof(samples));
        }

        var signal = new Signal(samples.Length, samplingRate);
        samples.CopyTo(signal.Samples);
        return signal;
    }

    /// <summary>
    /// Creates a signal by copying samples from an array.
    /// </summary>
    public static Signal FromCopy(float[] samples, float samplingRate)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return FromCopy(samples.AsSpan(), samplingRate);
    }

    /// <summary>
    /// Creates a signal by copying samples from an array segment.
    /// </summary>
    public static Signal FromCopy(ArraySegment<float> segment, float samplingRate)
        => FromCopy(segment.AsSpan(), samplingRate);

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
        ClearAllCaches();
    }

    #endregion

    #region Time-Domain Characteristics

    private float? _amplitude;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Amplitude"/>
    public float Amplitude => _amplitude ??= ITimeDomainCharacteristics.GetAmplitude_SIMD(Samples);

    /// <inheritdoc cref="ITimeDomainCharacteristics.Period"/>
    public float Period => 1f / _samplingRate;

    private float? _totalPower;

    /// <inheritdoc cref="ITimeDomainCharacteristics.TotalPower"/>
    public float TotalPower => _totalPower ??= ITimeDomainCharacteristics.GetTotalPower_SIMD(Samples);

    private float? _averagePower;

    /// <inheritdoc cref="ITimeDomainCharacteristics.AveragePower"/>
    public float AveragePower => _averagePower ??= ITimeDomainCharacteristics.GetAveragePower_SIMD(Samples);

    private float? _totalEnergy;

    /// <inheritdoc cref="ITimeDomainCharacteristics.TotalEnergy"/>
    public float TotalEnergy => _totalEnergy ??= ITimeDomainCharacteristics.GetTotalEnergy_SIMD(Samples);

    private float? _averageEnergy;

    /// <inheritdoc cref="ITimeDomainCharacteristics.AverageEnergy"/>
    public float AverageEnergy => _averageEnergy ??= ITimeDomainCharacteristics.GetAverageEnergy_SIMD(Samples);

    private float? _rms;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Rms"/>
    public float Rms => _rms ??= ITimeDomainCharacteristics.GetRms_SIMD(Samples);

    private float? _zeroCrossingRate;

    /// <inheritdoc cref="ITimeDomainCharacteristics.ZeroCrossingRate"/>
    public float ZeroCrossingRate => _zeroCrossingRate ??= ITimeDomainCharacteristics.GetZeroCrossingRate_NEWSIMD_Grok(Samples);

    private float? _entropy;

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
    public Signal Clone()
    {
        ThrowIfDisposed();
        var result = new Signal(_length, _samplingRate);
        _buffer!.Span.CopyTo(result._buffer!.Span);
        return result;
    }

    /// <summary>
    /// Creates an owning copy of the sample range <c>[start, start + length)</c>.
    /// </summary>
    public Signal CloneRange(int start, int length)
    {
        ThrowIfDisposed();
        ValidateSegmentRange(start, length, throwOnError: true);

        var result = new Signal(length, _samplingRate);
        _buffer![start, length].CopyTo(result.Samples);
        return result;
    }

    /// <summary>
    /// Creates an owning copy of the sample range described by <paramref name="range"/>.
    /// </summary>
    public Signal CloneRange(Range range)
    {
        ThrowIfDisposed();
        var (offset, length) = range.GetOffsetAndLength(_length);
        return CloneRange(offset, length);
    }

    #endregion

    #region To Frequency-domain

    /// <inheritdoc/>
    public FrequencyDomain TransformToFrequencyDomain(ComputingContext? context = null, WindowType? window = null)
    {
        ThrowIfDisposed();

        if (window is null && _length.IsPowerOf2())
        {
            var result = new ComplexFp32[_length];
            FastFourierTransform.Forward(_buffer!.Span, result, context);
            return new FrequencyDomain(0, _length, _length, result, this, window);
        }

        var windowedSamples = ITimeDomainSignal.PadZerosAndWindowing(_buffer!.Span, _length.NextPowerOf2(), window);
        FastFourierTransform.Forward(windowedSamples, 0, out var resultPadded, windowedSamples.Length, context);
        return new FrequencyDomain(0, windowedSamples.Length, _length, resultPadded, this, window);
    }

    #endregion

    #region Resample

    /// <inheritdoc cref="ISingleThreadTimeDomainSignal.Resample(float, FirFilter?, int)"/>
    public Signal Resample(float destinationSamplingRate, FirFilter? filter = null, int order = 15)
    {
        ThrowIfDisposed();
        return SignalResamplingExtension.Resample(this, destinationSamplingRate, filter, order);
    }

    #endregion

    #region Indexer

    /// <summary>
    /// Gets or sets the sample value at the specified index.
    /// </summary>
    public float this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return Samples[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            ThrowIfDisposed();
            Samples[index] = value;
            NotifySamplesModified();
        }
    }

    /// <summary>
    /// Gets a zero-copy <see cref="SignalSegment"/> by start index and length.
    /// </summary>
    public SignalSegment? this[int start, int length, bool throwException = false]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();

            if (!ValidateSegmentRange(start, length, throwException))
            {
                return null;
            }

            return new SignalSegment(this, start, length);
        }
    }

    /// <summary>
    /// Gets a zero-copy <see cref="SignalSegment"/> for a <see cref="Range"/> of samples.
    /// </summary>
    public SignalSegment? this[Range range, bool throwException = false]
    {
        get
        {
            var (offset, length) = range.GetOffsetAndLength(_length);
            return this[offset, length, throwException];
        }
    }

    /// <summary>
    /// Gets a zero-copy <see cref="SignalSegment"/> by start time and duration.
    /// </summary>
    public SignalSegment? this[TimeSpan startTime, TimeSpan duration, bool throwException = false]
        => this
        [
            ITimeDomainSignal.TimeToArrayIndexOrLength(startTime, _samplingRate),
            ITimeDomainSignal.TimeToArrayIndexOrLength(duration, _samplingRate),
            throwException
        ];

    private bool ValidateSegmentRange(int start, int length, bool throwOnError)
    {
        if (throwOnError)
        {
            if (start < 0 || start >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "Start index is out of range.");
            }

            if (length <= 0 || length > _length || (start + length) > _length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length is out of range.");
            }

            return true;
        }

        if (start < 0 || start >= _length)
        {
            return false;
        }

        if (length <= 0 || length > _length || (start + length) > _length)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region IEquatable<Signal>

    public unsafe bool Equals(Signal? other)
    {
        if (other is null) return false;
        if (_length != other._length) return false;

        return _buffer!.UnmanagedPointer == other._buffer!.UnmanagedPointer &&
               _samplingRate == other._samplingRate;
    }

    public static bool operator ==(Signal? left, Signal? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Signal? left, Signal? right) => !(left == right);

    #endregion

    #region overrides

    public override bool Equals(object? obj) => obj is Signal other && Equals(other);

    public override unsafe int GetHashCode()
    {
        ThrowIfDisposed();
        return ((nint)_buffer!.UnmanagedPointer, _length, _samplingRate).GetHashCode();
    }

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
        result.NotifySamplesModified();
        return result;
    }

    public static Signal operator +(Signal left, Signal right)
    {
        EnsureSameShape(left, right);
        var result = left.Clone();
        result.Samples.Add(right.Samples);
        result.NotifySamplesModified();
        return result;
    }

    public static Signal operator -(Signal left, float right)
    {
        var result = left.Clone();
        result.Samples.Subtract(right);
        result.NotifySamplesModified();
        return result;
    }

    public static Signal operator -(Signal left, Signal right)
    {
        EnsureSameShape(left, right);
        var result = left.Clone();
        result.Samples.Subtract(right.Samples);
        result.NotifySamplesModified();
        return result;
    }

    public static Signal operator *(Signal left, float right)
    {
        var result = left.Clone();
        result.Samples.Multiply(right);
        result.NotifySamplesModified();
        return result;
    }

    public static Signal operator *(Signal left, Signal right)
    {
        EnsureSameShape(left, right);
        var result = left.Clone();
        result.Samples.Multiply(right.Samples);
        result.NotifySamplesModified();
        return result;
    }

    public static Signal operator /(Signal left, float right)
    {
        var result = left.Clone();
        result.Samples.Divide(right);
        result.NotifySamplesModified();
        return result;
    }

    private static void EnsureSameShape(Signal left, Signal right)
    {
        if (left.Length != right.Length || left.SamplingRate != right.SamplingRate)
        {
            throw new ArgumentException("Signals must have the same length and sampling rate.");
        }
    }

    #endregion

    #region Dispose pattern

    private bool _isDisposed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _buffer?.Dispose();
                _buffer = null;
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~Signal()
    {
        Dispose(disposing: false);
    }

    #endregion
}
