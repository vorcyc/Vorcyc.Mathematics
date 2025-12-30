namespace Vorcyc.Mathematics.SignalProcessing.Signals;

using Vorcyc.Mathematics.Framework;

/*
 * 与原版本的不同处：
 * 1. Samples 使用 PinnableArray<float> 替换原来的 float[] ，同时实现 IDispose 接口。
 * 2. 与新的 AudioFrame 接轨，使其可以当作专门的离散信号来孤立使用（与AudioFrame无关），也可将其当作音频信号来使用（与AudioFrame有关）。
 * 3. 其它小调整：
 * a.Duration 类型用 TimeSpan 替换 double。
 *  b.支持 System.Range 的 Indexer。
 * 
 */

/// <summary>
/// Represents a finite real-valued discrete-time signal with a fixed sampling rate and a set of samples.
/// Provides basic operations and measurements such as energy, RMS, zero-crossing rate, and entropy.
/// This type uses <see cref="PinnableArray{T}"/> to store samples and implements <see cref="IDisposable"/>.
/// </summary>
/// <remarks>
/// See also <c>DiscreteSignalExtensions</c> for extension functionality.
/// The sample count is determined by either a specified count or derived from the duration and sampling rate.
/// </remarks>
public class DiscreteSignal : ICloneable<DiscreteSignal>, IDisposable
{
    private PinnableArray<float> _samples;

    private readonly int _samplingRate;

    //样本量
    internal int _numOfSample;

    internal TimeSpan _duration;


    #region constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteSignal"/> class using either a sample count or a duration.
    /// </summary>
    /// <param name="samplingRate">The sampling rate (samples per second). Must be positive.</param>
    /// <param name="count">Optional total sample count.</param>
    /// <param name="duration">Optional duration of the signal. If specified, sample count is derived by <c>duration * samplingRate</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown when both <paramref name="count"/> and <paramref name="duration"/> are <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="samplingRate"/> is not positive.</exception>
    private DiscreteSignal(int samplingRate, int? count, TimeSpan? duration)
    {
        Guard.AgainstNonPositive(samplingRate, "Sampling rate");

        if (count is null && duration is null)
            throw new ArgumentNullException("Both paramaters of count and duration cannot null.");

        _samplingRate = samplingRate;
        //_strategy = strategy;

        if (count is not null && duration is null)
        {
            _numOfSample = count.Value;
            _duration = TimeSpan.FromSeconds((double)_numOfSample / samplingRate);
        }
        else if (duration is not null && count is null)
        {
            _duration = duration.Value;
            _numOfSample = (int)(duration.Value.TotalSeconds * samplingRate);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteSignal"/> class with the specified sampling rate and duration.
    /// </summary>
    /// <param name="samplingRate">The sampling rate (samples per second). Must be positive.</param>
    /// <param name="duration">The duration of the signal.</param>
    /// <param name="pinSamples">When <c>true</c>, pins the underlying sample buffer to avoid GC relocation.</param>
    public DiscreteSignal(int samplingRate, TimeSpan duration, bool pinSamples = false)
        : this(samplingRate, count: null, duration: duration)
    {
        _samples = new(_numOfSample, pinSamples);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteSignal"/> class with the specified sampling rate and sample count.
    /// </summary>
    /// <param name="samplingRate">The sampling rate (samples per second). Must be positive.</param>
    /// <param name="count">The total number of samples to allocate.</param>
    /// <param name="pinSamples">When <c>true</c>, pins the underlying sample buffer to avoid GC relocation.</param>
    public DiscreteSignal(int samplingRate, int count, bool pinSamples = false)
        : this(samplingRate, count: count, duration: null)
    {
        _samples = new(_numOfSample, pinSamples);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteSignal"/> class using the provided sample array.
    /// </summary>
    /// <param name="samplingRate">The sampling rate (samples per second). Must be positive.</param>
    /// <param name="samples">The sample array to use.</param>
    /// <param name="pinSamples">When <c>true</c>, pins the underlying sample buffer to avoid GC relocation.</param>
    /// <remarks>
    /// The provided samples are wrapped by <see cref="PinnableArray{T}"/>; no copy is performed unless required by the implementation of <c>PinnableArray</c>.
    /// </remarks>
    public DiscreteSignal(int samplingRate, float[] samples, bool pinSamples = false)
        : this(samplingRate, count: samples.Length, duration: null)
    {
        _samples = new(samples, pinSamples);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteSignal"/> class using a slice of the provided sample array.
    /// </summary>
    /// <param name="samplingRate">The sampling rate (samples per second). Must be positive.</param>
    /// <param name="samples">The source sample array.</param>
    /// <param name="offset">The starting index in <paramref name="samples"/>.</param>
    /// <param name="count">The number of samples to take from <paramref name="samples"/> starting at <paramref name="offset"/>.</param>
    /// <param name="pinSamples">When <c>true</c>, pins the underlying sample buffer to avoid GC relocation.</param>
    public DiscreteSignal(int samplingRate, float[] samples, int offset, int count, bool pinSamples = false)
         : this(samplingRate, count: count, duration: null)
    {
        _samples = new(samples, offset, count, pinSamples);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteSignal"/> class using an enumerable sequence of samples.
    /// </summary>
    /// <param name="samplingRate">The sampling rate (samples per second). Must be positive.</param>
    /// <param name="samples">A sequence of samples.</param>
    /// <param name="pinSamples">When <c>true</c>, pins the underlying sample buffer to avoid GC relocation.</param>
    public DiscreteSignal(int samplingRate, IEnumerable<float> samples, bool pinSamples = false)
        : this(samplingRate, samples.ToArray(), pinSamples)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteSignal"/> class using an <see cref="ArraySegment{T}"/>.
    /// </summary>
    /// <param name="samplingRate">The sampling rate (samples per second). Must be positive.</param>
    /// <param name="segment">The segment of an array containing samples.</param>
    /// <param name="pinSamples">When <c>true</c>, pins the underlying sample buffer to avoid GC relocation.</param>
    public DiscreteSignal(int samplingRate, ArraySegment<float> segment, bool pinSamples = false)
        : this(samplingRate, count: segment.Count, duration: null)
    {
        _samples = new(segment, pinSamples);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscreteSignal"/> class using a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="samplingRate">The sampling rate (samples per second). Must be positive.</param>
    /// <param name="span">The span of samples.</param>
    /// <param name="pinSamples">When <c>true</c>, pins the underlying sample buffer to avoid GC relocation.</param>
    public DiscreteSignal(int samplingRate, Span<float> span, bool pinSamples = false)
         : this(samplingRate, count: span.Length, duration: null)
    {
        _samples = new(span, pinSamples);
    }

    #endregion

    #region 特殊常量信号

    /// <summary>
    /// Generates a unit impulse signal of the specified <paramref name="length"/> sampled at <paramref name="samplingRate"/>.
    /// </summary>
    /// <param name="length">The length of the unit impulse (number of samples).</param>
    /// <param name="samplingRate">The sampling rate (samples per second).</param>
    /// <param name="pinSamples">When <c>true</c>, pins the underlying sample buffer to avoid GC relocation.</param>
    /// <returns>A new <see cref="DiscreteSignal"/> with the first sample set to 1 and the rest to 0.</returns>
    public static DiscreteSignal Unit(int length, int samplingRate = 1, bool pinSamples = false)
    {
        //var unit = new float[length];
        //unit[0] = 1;

        //return new DiscreteSignal(samplingRate, unit);

        var result = new DiscreteSignal(samplingRate, length, pinSamples);
        result._samples[0] = 1;
        return result;
    }

    /// <summary>
    /// Generates a constant-valued signal of the specified <paramref name="length"/> sampled at <paramref name="samplingRate"/>.
    /// </summary>
    /// <param name="constant">The constant value to fill in all samples.</param>
    /// <param name="length">The length of the constant signal (number of samples).</param>
    /// <param name="samplingRate">The sampling rate (samples per second).</param>
    /// <param name="pinSamples">When <c>true</c>, pins the underlying sample buffer to avoid GC relocation.</param>
    /// <returns>A new <see cref="DiscreteSignal"/> with all samples set to <paramref name="constant"/>.</returns>
    public static DiscreteSignal Constant(float constant, int length, int samplingRate = 1, bool pinSamples = false)
    {
        var result = new DiscreteSignal(samplingRate, length, pinSamples);
        result._samples.Fill(constant);
        return result;
    }

    #endregion

    #region properites

    /// <summary>
    /// Gets the sampling rate (samples per second).
    /// </summary>
    public int SamplingRate => _samplingRate;

    /// <summary>
    /// Number of samples in the signal.
    /// </summary>
    /// <remarks>
    /// Gets the total number of allocated samples. This does not change after allocation.
    /// </remarks>
    public int SampleCount => _numOfSample;

    /// <summary>
    /// Gets the duration of the signal.
    /// </summary>
    public TimeSpan Duration => _duration;

    /// <summary>
    /// Gets the underlying sample buffer.
    /// </summary>
    /// <remarks>
    /// The buffer is a <see cref="PinnableArray{T}"/> which can be pinned to avoid GC relocation if needed.
    /// </remarks>
    public PinnableArray<float> Samples => _samples;

    #endregion

    #region ICloneable<T>

    /// <summary>
    /// Creates a copy of the current <see cref="DiscreteSignal"/> instance.
    /// </summary>
    /// <returns>A new <see cref="DiscreteSignal"/> that contains the same sampling rate and samples.</returns>
    public DiscreteSignal Clone()
    {
        return new(_samplingRate, _samples, false);
    }

    #endregion

    #region indexer

    /// <summary>
    /// Gets or sets the sample value at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The zero-based sample index.</param>
    /// <returns>The sample value at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="index"/> is out of range.</exception>
    public float this[int index]
    {
        get => _samples[index];
        set => _samples[index] = value;
    }

    /// <summary>
    /// Gets a new <see cref="DiscreteSignal"/> representing a fragment between <paramref name="startPos"/> (inclusive)
    /// and <paramref name="endPos"/> (exclusive).
    /// </summary>
    /// <param name="startPos">The inclusive start index.</param>
    /// <param name="endPos">The exclusive end index.</param>
    /// <returns>A new <see cref="DiscreteSignal"/> containing the selected fragment.</returns>
    /// <exception cref="ArgumentException">Thrown when the range is invalid.</exception>
    public DiscreteSignal this[int startPos, int endPos]
    {
        get
        {
            Guard.AgainstInvalidRange(startPos, endPos, "Left index", "Right index");

            // Implementaion is LINQ-less, since Skip() would be less efficient:
            //     return new DiscreteSignal(SamplingRate, Samples.Skip(startPos).Take(endPos - startPos));

            return new(_samplingRate, _samples.Values.FastCopyFragment(endPos - startPos, startPos), false);
        }
    }

    /// <summary>
    /// Gets a new <see cref="DiscreteSignal"/> representing a fragment defined by <see cref="Range"/>.
    /// </summary>
    /// <param name="range">The range of samples to copy.</param>
    /// <returns>A new <see cref="DiscreteSignal"/> containing the selected fragment.</returns>
    public DiscreteSignal this[Range range]
    {
        get
        {
            //RuntimeHelpers.GetSubArray
            return new(
                _samplingRate,
                _samples.Values.FastCopyFragment(range.End.Value - range.Start.Value, range.Start.Value),
                false);
        }
    }

    #endregion

    #region operators

    /// <summary>
    /// Superimposes two signals <paramref name="s1"/> and <paramref name="s2"/> (sample-wise addition).
    /// If sizes differ, the smaller signal is broadcast to match the larger one.
    /// </summary>
    /// <param name="s1">The first signal.</param>
    /// <param name="s2">The second signal.</param>
    /// <returns>A new <see cref="DiscreteSignal"/> that is the superposition of the two signals.</returns>
    public static DiscreteSignal operator +(DiscreteSignal s1, DiscreteSignal s2)
    {
        return s1.Superimpose(s2);
    }

    /// <summary>
    /// Creates a negated copy of the signal (sample-wise <c>-x</c>).
    /// </summary>
    /// <param name="s">The source signal.</param>
    /// <returns>A new <see cref="DiscreteSignal"/> with each sample negated.</returns>
    public static DiscreteSignal operator -(DiscreteSignal s)
    {
        return new DiscreteSignal(s._samplingRate, s._samples.Select(x => -x), false);
    }

    /// <summary>
    /// Subtracts signal <paramref name="s2"/> from <paramref name="s1"/> (sample-wise subtraction).
    /// If sizes differ, the smaller signal is broadcast to match the larger one.
    /// </summary>
    /// <param name="s1">The minuend signal.</param>
    /// <param name="s2">The subtrahend signal.</param>
    /// <returns>A new <see cref="DiscreteSignal"/> representing <c>s1 - s2</c>.</returns>
    public static DiscreteSignal operator -(DiscreteSignal s1, DiscreteSignal s2)
    {
        return s1.Subtract(s2);
    }

    /// <summary>
    /// Adds a constant to each sample of the signal.
    /// </summary>
    /// <param name="s">The source signal.</param>
    /// <param name="constant">The constant to add to each sample.</param>
    /// <returns>A new <see cref="DiscreteSignal"/> with <paramref name="constant"/> added to each sample.</returns>
    public static DiscreteSignal operator +(DiscreteSignal s, float constant)
    {
        return new DiscreteSignal(s.SamplingRate, s.Samples.Select(x => x + constant), false);
    }

    /// <summary>
    /// Subtracts a constant from each sample of the signal.
    /// </summary>
    /// <param name="s">The source signal.</param>
    /// <param name="constant">The constant to subtract from each sample.</param>
    /// <returns>A new <see cref="DiscreteSignal"/> with <paramref name="constant"/> subtracted from each sample.</returns>
    public static DiscreteSignal operator -(DiscreteSignal s, float constant)
    {
        return new DiscreteSignal(s.SamplingRate, s.Samples.Select(x => x - constant), false);
    }

    /// <summary>
    /// Multiplies each sample of the signal by <paramref name="coeff"/> (amplification or attenuation).
    /// </summary>
    /// <param name="s">The source signal.</param>
    /// <param name="coeff">The amplification/attenuation coefficient.</param>
    /// <returns>A new <see cref="DiscreteSignal"/> scaled by <paramref name="coeff"/>.</returns>
    public static DiscreteSignal operator *(DiscreteSignal s, float coeff)
    {
        var signal = s.Clone();
        signal.Amplify(coeff);
        return signal;
    }

    #endregion

    #region time-domain characteristics

    /// <summary>
    /// Computes the average energy of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive).</param>
    /// <param name="endPos">Index of the last sample (exclusive).</param>
    /// <returns>The average energy of the fragment.</returns>
    public float Energy(int startPos, int endPos)
    {
        var total = 0.0f;
        for (var i = startPos; i < endPos; i++)
        {
            total += Samples[i] * Samples[i];
        }

        return total / (endPos - startPos);
    }

    /// <summary>
    /// Computes the average energy of a signal fragment defined by <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The sample range.</param>
    /// <returns>The average energy of the fragment.</returns>
    public float Energy(Range range) => Energy(range.Start.Value, range.End.Value);

    /// <summary>
    /// Computes the average energy of the entire signal.
    /// </summary>
    /// <returns>The average energy.</returns>
    public float Energy() => Energy(0, SampleCount);

    /// <summary>
    /// Computes the RMS value of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive).</param>
    /// <param name="endPos">Index of the last sample (exclusive).</param>
    /// <returns>The RMS value of the fragment.</returns>
    public float Rms(int startPos, int endPos)
    {
        return MathF.Sqrt(Energy(startPos, endPos));
    }

    /// <summary>
    /// Computes the RMS value of a signal fragment defined by <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The sample range.</param>
    /// <returns>The RMS value of the fragment.</returns>
    public float Rms(Range range) => MathF.Sqrt(Energy(range));

    /// <summary>
    /// Computes the RMS value of the entire signal.
    /// </summary>
    /// <returns>The RMS value.</returns>
    public float Rms() => MathF.Sqrt(Energy(0, SampleCount));

    /// <summary>
    /// Computes the zero-crossing rate of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive).</param>
    /// <param name="endPos">Index of the last sample (exclusive).</param>
    /// <returns>The zero-crossing rate (normalized count).</returns>
    public float ZeroCrossingRate(int startPos, int endPos)
    {
        const float disbalance = 1e-4f;

        var prevSample = Samples[startPos] + disbalance;

        var rate = 0;
        for (var i = startPos + 1; i < endPos; i++)
        {
            var sample = Samples[i] + disbalance;

            if ((sample >= 0) != (prevSample >= 0))
            {
                rate++;
            }

            prevSample = sample;
        }

        return (float)rate / (endPos - startPos - 1);
    }

    /// <summary>
    /// Computes the zero-crossing rate of a signal fragment defined by <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The sample range.</param>
    /// <returns>The zero-crossing rate (normalized count).</returns>
    public float ZeroCrossingRate(Range range) => ZeroCrossingRate(range.Start.Value, range.End.Value);

    /// <summary>
    /// Computes the zero-crossing rate of the entire signal.
    /// </summary>
    /// <returns>The zero-crossing rate.</returns>
    public float ZeroCrossingRate() => ZeroCrossingRate(0, SampleCount);// ZeroCrossingRate(..);

    /// <summary>
    /// Computes the Shannon entropy of a signal fragment using uniformly distributed bins between the minimum and maximum of absolute sample values.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive).</param>
    /// <param name="endPos">Index of the last sample (exclusive).</param>
    /// <param name="binCount">The number of bins used to estimate the distribution.</param>
    /// <returns>The Shannon entropy in bits.</returns>
    public float Entropy(int startPos, int endPos, int binCount = 32)
    {
        var len = endPos - startPos;

        if (len < binCount)
        {
            binCount = len;
        }

        var bins = new int[binCount + 1];

        var min = Samples[0];
        var max = Samples[0];
        for (var i = startPos; i < endPos; i++)
        {
            var sample = Math.Abs(Samples[i]);

            if (sample < min)
            {
                min = sample;
            }
            if (sample > max)
            {
                max = sample;
            }
        }

        if (max - min < 1e-8f)
        {
            return 0;
        }

        var binLength = (max - min) / binCount;

        for (var i = startPos; i < endPos; i++)
        {
            bins[(int)((Math.Abs(Samples[i]) - min) / binLength)]++;
        }

        var entropy = 0.0f;
        for (var i = 0; i < binCount; i++)
        {
            var p = (float)bins[i] / (endPos - startPos);

            if (p > 1e-8f)
            {
                entropy += p * MathF.Log(p, 2);
            }
        }

        return (-entropy / MathF.Log(binCount, 2));
    }

    /// <summary>
    /// Computes the Shannon entropy of the entire signal using uniformly distributed bins.
    /// </summary>
    /// <param name="binCount">The number of bins used to estimate the distribution.</param>
    /// <returns>The Shannon entropy in bits.</returns>
    public float Entropy(int binCount = 32) => Entropy(0, SampleCount, binCount);

    #endregion

    #region Conversion with DiscreteSignal<float>

    /// <summary>
    /// Converts a <see cref="DiscreteSignal{T}"/> specialized for <see cref="float"/> to <see cref="DiscreteSignal"/>.
    /// </summary>
    /// <param name="signal">The source signal.</param>
    /// <returns>A <see cref="DiscreteSignal"/> with equivalent sampling rate and samples.</returns>
    public static DiscreteSignal From(DiscreteSignal<float> signal) => signal;

    /// <summary>
    /// Implicitly converts a <see cref="DiscreteSignal{T}"/> specialized for <see cref="float"/> to <see cref="DiscreteSignal"/>.
    /// </summary>
    /// <param name="signal">The source signal.</param>
    public static implicit operator DiscreteSignal(DiscreteSignal<float> signal)
    {
        return new(signal.SamplingRate, signal.Samples, signal.Samples.IsPinned);
    }

    /// <summary>
    /// Implicitly converts a <see cref="DiscreteSignal"/> to <see cref="DiscreteSignal{T}"/> specialized for <see cref="float"/>.
    /// </summary>
    /// <param name="signal">The source signal.</param>
    public static implicit operator DiscreteSignal<float>(DiscreteSignal signal)
    {
        return new(signal.SamplingRate, signal.Samples, signal.Samples.IsPinned);
    }

    #endregion

    #region dispose pattern

    private bool _isDisposed;

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="DiscreteSignal"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _samples.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _isDisposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~DiscreteSignal2()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}