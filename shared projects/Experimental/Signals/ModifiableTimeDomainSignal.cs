using System.Threading.Channels;
using Vorcyc.Mathematics.Buffers;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// A time-domain signal implementation that supports safe runtime sample modification,
/// providing lock-protected writable and read-only views, asynchronous sample appending
/// with batch flush, and frequency-domain transformation with cached statistical characteristics.
/// </summary>
public class ModifiableTimeDomainSignal : IModifiableTimeDomainSignal, IDisposable
{
    private readonly Channel<float[]> _channel;
    private POHBuffer<float>? _buffer;

    private volatile int _length;
    private readonly float _samplingRate;

    private readonly object _viewLock = new();

    #region Constructors

    /// <summary>
    /// Initializes a new signal with the specified sample count and sampling rate.
    /// </summary>
    /// <param name="sampleCount">The number of samples. Must be greater than 0.</param>
    /// <param name="samplingRate">The sampling rate in Hz.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="sampleCount"/> is less than or equal to 0.
    /// </exception>
    public ModifiableTimeDomainSignal(int sampleCount, float samplingRate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sampleCount, 0, nameof(sampleCount));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(samplingRate, 0, nameof(samplingRate));

        _length = sampleCount;
        _samplingRate = samplingRate;
        _buffer = new(sampleCount);

        var options = new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false,
        };

        _channel = Channel.CreateUnbounded<float[]>(options);
    }

    /// <summary>
    /// Initializes a new signal with the specified duration and sampling rate.
    /// </summary>
    /// <param name="duration">The signal duration.</param>
    /// <param name="samplingRate">The sampling rate in Hz.</param>
    public ModifiableTimeDomainSignal(TimeSpan duration, float samplingRate)
        : this(ITimeDomainSignal.TimeToArrayIndexOrLength(duration, samplingRate), samplingRate)
    {
    }

    #endregion

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
    /// Gets a lock-holding view for safe, direct access to the sample <see cref="Span{T}"/>.
    /// Must be used with a <c>using</c> statement to ensure the lock is properly released.
    /// <example><code>
    /// using var view = signal.Samples;
    /// Span&lt;float&gt; span = view.Span;
    /// // Use span here...
    /// // Lock is released when Dispose is called (end of using block).
    /// </code></example>
    /// </summary>
    public LockedSamplesView Samples
    {
        get
        {
            ThrowIfDisposed();
            return new(this);
        }
    }

    /// <inheritdoc cref="ITimeDomainSignal.Length"/>
    public int Length => _length;

    /// <inheritdoc cref="ITimeDomainSignal.SamplingRate"/>
    public float SamplingRate => _samplingRate;

    /// <inheritdoc cref="ITimeDomainSignal.Duration"/>
    public TimeSpan Duration => ITimeDomainSignal.ArrayIndexOrLengthToTime(_length, _samplingRate);

    /// <inheritdoc />
    public void NotifySamplesModified()
    {
        ClearAllCaches();
    }

    /// <summary>
    /// Called when the sample collection has been modified.
    /// Derived classes can override this to perform additional logic or refresh dependent data.
    /// </summary>
    protected virtual void OnSamplesModified()
    {
        ClearAllCaches();
    }

    #endregion

    #region LockedSamplesView

    /// <summary>
    /// Represents a lock-protected writable view of the samples, ensuring safe modification
    /// in concurrent scenarios. Call <see cref="Dispose"/> to release the lock.
    /// </summary>
    public readonly ref struct LockedSamplesView : IDisposable
    {
        private readonly ModifiableTimeDomainSignal _signal;
        private readonly bool _lockTaken;

        /// <summary>
        /// Gets the writable sample span.
        /// </summary>
        public readonly Span<float> Span;

        internal LockedSamplesView(ModifiableTimeDomainSignal signal)
        {
            _signal = signal;
            _lockTaken = false;
            Monitor.Enter(signal._viewLock, ref _lockTaken);

            if (signal._buffer is null || signal._buffer.IsDisposed)
            {
                if (_lockTaken) Monitor.Exit(signal._viewLock);
                _lockTaken = false;
                Span = default;
                throw new ObjectDisposedException(nameof(ModifiableTimeDomainSignal));
            }

            Span = signal._buffer.Span;
        }

        /// <summary>
        /// Releases the lock, ending safe access to the samples.
        /// </summary>
        public void Dispose()
        {
            if (_lockTaken)
            {
                Monitor.Exit(_signal._viewLock);
            }
        }
    }

    #endregion

    #region Time-Domain Characteristics

    private float? _amplitude = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Amplitude"/>
    public float Amplitude
    {
        get
        {
            ThrowIfDisposed();
            var cached = _amplitude;
            if (cached is null)
            {
                using var samples = Samples;
                cached = ITimeDomainCharacteristics.GetAmplitude_SIMD(samples.Span);
                _amplitude = cached;
            }
            return cached.Value;
        }
    }

    /// <inheritdoc cref="ITimeDomainCharacteristics.Period"/>
    public float Period => 1f / _samplingRate;

    private float? _totalPower = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.TotalPower"/>
    public float TotalPower
    {
        get
        {
            ThrowIfDisposed();
            var cached = _totalPower;
            if (cached is null)
            {
                using var view = Samples;
                cached = ITimeDomainCharacteristics.GetTotalPower_SIMD(view.Span);
                _totalPower = cached;
            }
            return cached.Value;
        }
    }

    private float? _averagePower = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.AveragePower"/>
    public float AveragePower
    {
        get
        {
            ThrowIfDisposed();
            var cached = _averagePower;
            if (cached is null)
            {
                using var view = Samples;
                cached = ITimeDomainCharacteristics.GetAveragePower_SIMD(view.Span);
                _averagePower = cached;
            }
            return cached.Value;
        }
    }

    private float? _totalEnergy = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.TotalEnergy"/>
    public float TotalEnergy
    {
        get
        {
            ThrowIfDisposed();
            var cached = _totalEnergy;
            if (cached is null)
            {
                using var view = Samples;
                cached = ITimeDomainCharacteristics.GetTotalEnergy_SIMD(view.Span);
                _totalEnergy = cached;
            }
            return cached.Value;
        }
    }

    private float? _averageEnergy = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.AverageEnergy"/>
    public float AverageEnergy
    {
        get
        {
            ThrowIfDisposed();
            var cached = _averageEnergy;
            if (cached is null)
            {
                using var view = Samples;
                cached = ITimeDomainCharacteristics.GetAverageEnergy_SIMD(view.Span);
                _averageEnergy = cached;
            }
            return cached.Value;
        }
    }

    private float? _rms = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Rms"/>
    public float Rms
    {
        get
        {
            ThrowIfDisposed();
            var cached = _rms;
            if (cached is null)
            {
                using var view = Samples;
                cached = ITimeDomainCharacteristics.GetRms_SIMD(view.Span);
                _rms = cached;
            }
            return cached.Value;
        }
    }

    private float? _zeroCrossingRate = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.ZeroCrossingRate"/>
    public float ZeroCrossingRate
    {
        get
        {
            ThrowIfDisposed();
            var cached = _zeroCrossingRate;
            if (cached is null)
            {
                using var view = Samples;
                cached = ITimeDomainCharacteristics.GetZeroCrossingRate_NEWSIMD_Grok(view.Span);
                _zeroCrossingRate = cached;
            }
            return cached.Value;
        }
    }

    private float? _entropy = null;

    /// <inheritdoc cref="ITimeDomainCharacteristics.Entropy"/>
    public float Entropy
    {
        get
        {
            ThrowIfDisposed();
            var cached = _entropy;
            if (cached is null)
            {
                using var view = Samples;
                cached = ITimeDomainCharacteristics.GetEntropy_SIMD(view.Span);
                _entropy = cached;
            }
            return cached.Value;
        }
    }

    /// <inheritdoc cref="ITimeDomainCharacteristics.GetEntropy(int)"/>
    public float GetEntropy(int binCount = 32)
    {
        ThrowIfDisposed();
        using var view = Samples;
        return ITimeDomainCharacteristics.GetEntropy_SIMD(view.Span, binCount);
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

    #region Clone

    /// <summary>
    /// Creates a deep copy of the current signal, including the sample buffer and metadata.
    /// </summary>
    /// <returns>A new <see cref="ModifiableTimeDomainSignal"/> instance.</returns>
    public ModifiableTimeDomainSignal Clone()
    {
        using var view = Samples;
        var result = new ModifiableTimeDomainSignal(_length, _samplingRate);
        view.Span.CopyTo(result._buffer!.Span);
        return result;
    }

    #endregion

    #region Frequency-Domain Transformation

    /// <inheritdoc cref="ITimeDomainSignal.TransformToFrequencyDomain(WindowType?, FftVersion)"/>
    public FrequencyDomain TransformToFrequencyDomain(WindowType? window = null, FftVersion fftVersion = FftVersion.Normal)
    {
        using var samples = Samples;
        FastFourierTransform.Version = fftVersion;

        if (window is null && _length.IsPowerOf2())
        {
            var result = new Vorcyc.Mathematics.Numerics.ComplexFp32[_length];
            FastFourierTransform.Forward(samples.Span, result);
            return new FrequencyDomain(0, _length, _length, result, this, window);
        }
        else
        {
            var windowedSamples = ITimeDomainSignal.PadZerosAndWindowing(samples.Span, _length.NextPowerOf2(), window);
            FastFourierTransform.Forward(windowedSamples, 0, out var result, windowedSamples.Length);
            return new FrequencyDomain(0, windowedSamples.Length, _length, result, this, window);
        }
    }



    #endregion

    #region Resampling

    /// <inheritdoc cref="ITimeDomainSignal.Resample(int, FirFilter?, int)"/>
    public ModifiableTimeDomainSignal Resample(int destnationSamplingRate, FirFilter? filter = null, int order = 15)
    {
        return SignalResamplingExtension.Resample(this, destnationSamplingRate, filter, order);
    }

    #endregion

    #region Modify (Append / Insert / Remove)

    /// <summary>
    /// Asynchronously appends a block of sample data. Intended to be called from sensor or
    /// acquisition threads. The data is buffered in a channel and merged into the main buffer
    /// when <see cref="FlushPendingAppends"/> is called.
    /// </summary>
    /// <param name="samples">The sample block to append.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask AppendAsync(float[] samples, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (samples is null || samples.Length == 0)
        {
            return;
        }

        if (_channel.Writer.TryWrite(samples))
        {
            return;
        }

        await _channel.Writer.WriteAsync(samples, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Merges all pending append blocks from the channel into the main buffer in a single batch,
    /// reducing copy overhead and lock contention.
    /// </summary>
    /// <returns>The number of new samples merged in this call.</returns>
    public int FlushPendingAppends()
    {
        var blocks = new List<float[]>();
        int totalAdded = 0;

        while (_channel.Reader.TryRead(out var block))
        {
            if (block is { Length: > 0 })
            {
                blocks.Add(block);
                totalAdded += block.Length;
            }
        }

        if (totalAdded == 0) return 0;

        lock (_viewLock)
        {
            ThrowIfDisposed();

            var oldLen = _length;
            var newLen = checked(oldLen + totalAdded);

            var merged = new POHBuffer<float>(newLen);

            if (oldLen > 0)
            {
                _buffer[0, oldLen].CopyTo(merged[0, oldLen]);
            }

            int offset = oldLen;
            foreach (var b in blocks)
            {
                b.AsSpan().CopyTo(merged[offset, b.Length]);
                offset += b.Length;
            }

            _buffer.Dispose();
            _buffer = merged;
            _length = newLen;
        }

        OnSamplesModified();
        return totalAdded;
    }

    /// <summary>
    /// Inserts a block of samples at the specified index.
    /// An index equal to <see cref="Length"/> appends to the end.
    /// </summary>
    /// <param name="index">The insertion index.</param>
    /// <param name="samples">The sample array to insert. Must have a length greater than 0.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="samples"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="samples"/> has zero length.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> exceeds the current signal length.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the internal buffer is unavailable.</exception>
    public void Insert(int index, float[] samples)
    {
        if (samples is null)
            throw new ArgumentNullException(nameof(samples));
        if (samples.Length == 0)
            throw new ArgumentException("插入数组长度必须大于 0。", nameof(samples));

        if ((uint)index > (uint)_length)
            throw new ArgumentOutOfRangeException(nameof(index), "索引超出当前信号长度范围。");

        lock (_viewLock)
        {
            if (_buffer is null || _buffer.IsDisposed)
                throw new ObjectDisposedException(nameof(_buffer), "缓冲区不可用。");

            var newLen = checked(_length + samples.Length);
            var result = new POHBuffer<float>(newLen);

            if (index > 0)
            {
                _buffer[0, index].CopyTo(result[0, index]);
            }

            samples.AsSpan().CopyTo(result[index, samples.Length]);

            var rightCount = _length - index;
            if (rightCount > 0)
            {
                _buffer[index, rightCount].CopyTo(result[index + samples.Length, rightCount]);
            }

            _buffer.Dispose();
            _buffer = result;
            _length = newLen;
        }

        OnSamplesModified();
    }

    /// <summary>
    /// Inserts a block of samples at the position corresponding to the specified time point.
    /// </summary>
    /// <param name="timePoint">The time point for the insertion position.</param>
    /// <param name="samples">The sample array to insert.</param>
    public void Insert(TimeSpan timePoint, float[] samples)
    {
        var index = ITimeDomainSignal.TimeToArrayIndexOrLength(timePoint, _samplingRate);
        Insert(index, samples);
    }

    /// <summary>
    /// Removes a range of samples starting at the specified index.
    /// </summary>
    /// <param name="index">The starting index. An index equal to <see cref="Length"/> with count 0 is a no-op.</param>
    /// <param name="count">The number of samples to remove. Must be non-negative.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index or count is out of range.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the internal buffer is unavailable.</exception>
    /// <exception cref="InvalidOperationException">Thrown when removal would result in an empty sequence.</exception>
    public void RemoveRange(int index, int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "删除数量必须为非负数。");
        if ((uint)index > (uint)_length)
            throw new ArgumentOutOfRangeException(nameof(index), "索引超出当前信号长度范围。");
        if (count == 0 || index == _length)
            return;

        if (index + count > _length)
            throw new ArgumentOutOfRangeException(nameof(count), "删除范围超出当前信号长度。");

        lock (_viewLock)
        {
            if (_buffer is null || _buffer.IsDisposed)
                throw new ObjectDisposedException(nameof(_buffer), "缓冲区不可用。");

            var newLen = _length - count;

            if (newLen <= 0) throw new InvalidOperationException("不允许删除至空序列");

            var result = new POHBuffer<float>(newLen);

            if (index > 0)
            {
                _buffer[0, index].CopyTo(result[0, index]);
            }

            var rightStart = index + count;
            var rightCount = _length - rightStart;
            if (rightCount > 0)
            {
                _buffer[rightStart, rightCount].CopyTo(result[index, rightCount]);
            }

            _buffer.Dispose();
            _buffer = result;
            _length = newLen;
        }

        OnSamplesModified();
    }

    /// <summary>
    /// Removes a range of samples corresponding to the specified time interval.
    /// </summary>
    /// <param name="startTimePoint">The start time of the removal range.</param>
    /// <param name="duration">The duration of the removal range.</param>
    public void RemoveRange(TimeSpan startTimePoint, TimeSpan duration)
    {
        var index = ITimeDomainSignal.TimeToArrayIndexOrLength(startTimePoint, _samplingRate);
        var count = ITimeDomainSignal.TimeToArrayIndexOrLength(duration, _samplingRate);
        RemoveRange(index, count);
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Throws <see cref="ObjectDisposedException"/> if the object has been disposed,
    /// preventing access to released resources.
    /// </summary>
    internal void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_buffer is null || _buffer.IsDisposed || _isDisposed, nameof(ModifiableTimeDomainSignal));
    }

    private volatile bool _isDisposed = false;

    /// <summary>
    /// Releases managed and unmanaged resources and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources. When <paramref name="disposing"/> is <see langword="true"/>,
    /// managed resources (buffer and channel) are released.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release managed resources; <see langword="false"/> for
    /// the finalizer path (unmanaged resources only).
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _channel.Writer.Complete();

            while (_channel.Reader.TryRead(out _)) { }

            lock (_viewLock)
            {
                if (_buffer is not null)
                {
                    _buffer.Dispose();
                    _buffer = null;
                }
                _length = 0;
            }
        }

        _isDisposed = true;
    }

    /// <summary>
    /// Destructor. Ensures resources are released if the consumer forgets to call <see cref="Dispose()"/>.
    /// </summary>
    ~ModifiableTimeDomainSignal()
    {
        Dispose(false);
    }

    #endregion
}


