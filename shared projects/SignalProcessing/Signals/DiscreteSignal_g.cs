namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/*
 * 与原版本的不同处：
 * 1. Samples 使用 PinnableArray<float> 替换原来的 float[] ，同时实现 IDispose 接口。
 * 2. 与新的 AudioFrame 接轨，使其可以当作专门的离散信号来孤立使用（与AudioFrame无关），也可将其当作音频信号来使用（与AudioFrame有关）。
 * 3. 其它小调整：
 *  a. Duration 类型用 TimeSpan 替换 double。
 *  b. 支持 System.Range 的 Indexer。
 * 
 */

using System.Numerics;
using Vorcyc.Mathematics.Framework;

/// <summary>
/// Represents a finite real-valued discrete-time signal parameterized by sample type <typeparamref name="TSample"/>.
/// Stores samples in a <see cref="PinnableArray{T}"/> and provides basic operations and measurements
/// such as energy, RMS, zero-crossing rate, and entropy.
/// </summary>
/// <typeparam name="TSample">
/// Unmanaged IEEE 754 floating-point sample type, e.g., <see cref="float"/> or <see cref="double"/>.
/// Must implement <see cref="IFloatingPointIeee754{TSelf}"/>.
/// </typeparam>
/// <remarks>
/// The total sample count is derived either from an explicit count or from the provided duration and sampling rate.
/// The buffer can optionally be pinned to avoid GC relocation.
/// </remarks>
public class DiscreteSignal<TSample> : ICloneable<DiscreteSignal<TSample>>, IDisposable
    where TSample : unmanaged, IFloatingPointIeee754<TSample>
{

    private PinnableArray<TSample> _samples;

    private readonly int _samplingRate;

    //样本量
    internal int _numOfSample;

    internal TimeSpan _duration;

    #region constructors

    /// <summary>
    /// Initializes a new instance using either a sample <paramref name="count"/> or a <paramref name="duration"/>.
    /// </summary>
    /// <param name="samplingRate">Sampling rate (samples per second). Must be positive.</param>
    /// <param name="count">Optional total sample count.</param>
    /// <param name="duration">Optional signal duration. If set, sample count is derived from <c>duration * samplingRate</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown if both <paramref name="count"/> and <paramref name="duration"/> are <c>null</c>.</exception>
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
    /// Initializes a new instance with specified <paramref name="samplingRate"/> and <paramref name="duration"/>.
    /// </summary>
    /// <param name="samplingRate">Sampling rate (samples per second).</param>
    /// <param name="duration">Signal duration.</param>
    /// <param name="pinSamples">Whether to pin the underlying sample buffer.</param>
    public DiscreteSignal(int samplingRate, TimeSpan duration, bool pinSamples = false)
        : this(samplingRate, count: null, duration: duration)
    {
        _samples = new(_numOfSample, pinSamples);
    }

    /// <summary>
    /// Initializes a new instance with specified <paramref name="samplingRate"/> and sample <paramref name="count"/>.
    /// </summary>
    /// <param name="samplingRate">Sampling rate (samples per second).</param>
    /// <param name="count">Total number of samples.</param>
    /// <param name="pinSamples">Whether to pin the underlying sample buffer.</param>
    public DiscreteSignal(int samplingRate, int count, bool pinSamples = false)
        : this(samplingRate, count: count, duration: null)
    {
        _samples = new(_numOfSample, pinSamples);
    }

    /// <summary>
    /// Initializes a new instance that wraps an existing sample array.
    /// </summary>
    /// <param name="samplingRate">Sampling rate (samples per second).</param>
    /// <param name="samples">Source sample array.</param>
    /// <param name="pinSamples">Whether to pin the underlying sample buffer.</param>
    /// <remarks>
    /// The array is wrapped by <see cref="PinnableArray{T}"/>. Copy semantics depend on <c>PinnableArray</c> implementation.
    /// </remarks>
    public DiscreteSignal(int samplingRate, TSample[] samples, bool pinSamples = false)
        : this(samplingRate, count: samples.Length, duration: null)
    {
        _samples = new(samples, pinSamples);
    }

    /// <summary>
    /// Initializes a new instance from a slice of an existing sample array.
    /// </summary>
    /// <param name="samplingRate">Sampling rate (samples per second).</param>
    /// <param name="samples">Source sample array.</param>
    /// <param name="offset">Start index within <paramref name="samples"/>.</param>
    /// <param name="count">Number of samples to wrap.</param>
    /// <param name="pinSamples">Whether to pin the underlying sample buffer.</param>
    public DiscreteSignal(int samplingRate, TSample[] samples, int offset, int count, bool pinSamples = false)
         : this(samplingRate, count: count, duration: null)
    {
        _samples = new(samples, offset, count, pinSamples);
    }

    /// <summary>
    /// Initializes a new instance from an enumerable sequence of samples.
    /// </summary>
    /// <param name="samplingRate">Sampling rate (samples per second).</param>
    /// <param name="samples">Sequence of samples.</param>
    /// <param name="pinSamples">Whether to pin the underlying sample buffer.</param>
    public DiscreteSignal(int samplingRate, IEnumerable<TSample> samples, bool pinSamples = false)
        : this(samplingRate, samples.ToArray(), pinSamples)
    { }

    /// <summary>
    /// Initializes a new instance from an <see cref="ArraySegment{T}"/> of samples.
    /// </summary>
    /// <param name="samplingRate">Sampling rate (samples per second).</param>
    /// <param name="segment">Segment of an array containing samples.</param>
    /// <param name="pinSamples">Whether to pin the underlying sample buffer.</param>
    public DiscreteSignal(int samplingRate, ArraySegment<TSample> segment, bool pinSamples = false)
        : this(samplingRate, count: segment.Count, duration: null)
    {
        _samples = new(segment, pinSamples);
    }

    /// <summary>
    /// Initializes a new instance from a <see cref="Span{T}"/> of samples.
    /// </summary>
    /// <param name="samplingRate">Sampling rate (samples per second).</param>
    /// <param name="span">Span containing samples.</param>
    /// <param name="pinSamples">Whether to pin the underlying sample buffer.</param>
    public DiscreteSignal(int samplingRate, Span<TSample> span, bool pinSamples = false)
         : this(samplingRate, count: span.Length, duration: null)
    {
        _samples = new(span, pinSamples);
    }

    #endregion

    #region 特殊常量信号

    /// <summary>
    /// Generates a unit impulse signal of the specified <paramref name="length"/> at <paramref name="samplingRate"/>.
    /// The first sample is <see cref="TSample.One"/>, remaining samples are zero.
    /// </summary>
    /// <param name="length">Number of samples in the impulse signal.</param>
    /// <param name="samplingRate">Sampling rate (samples per second).</param>
    /// <param name="pinSamples">Whether to pin the underlying sample buffer.</param>
    /// <returns>A new unit impulse <see cref="DiscreteSignal{TSample}"/>.</returns>
    public static DiscreteSignal<TSample> Unit(int length, int samplingRate = 1, bool pinSamples = false)
    {
        //var unit = new float[length];
        //unit[0] = 1;

        //return new DiscreteSignal(samplingRate, unit);

        var result = new DiscreteSignal<TSample>(samplingRate, length, pinSamples);
        result._samples[0] = TSample.One;
        return result;
    }

    /// <summary>
    /// Generates a constant-valued signal of the specified <paramref name="length"/> at <paramref name="samplingRate"/>.
    /// </summary>
    /// <param name="constant">Constant value to assign to each sample.</param>
    /// <param name="length">Number of samples in the signal.</param>
    /// <param name="samplingRate">Sampling rate (samples per second).</param>
    /// <param name="pinSamples">Whether to pin the underlying sample buffer.</param>
    /// <returns>A new constant-valued <see cref="DiscreteSignal{TSample}"/>.</returns>
    public static DiscreteSignal<TSample> Constant(TSample constant, int length, int samplingRate = 1, bool pinSamples = false)
    {
        var result = new DiscreteSignal<TSample>(samplingRate, length, pinSamples);
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
    public int SampleCount => _numOfSample;

    /// <summary>
    /// Gets the total duration of the signal.
    /// </summary>
    public TimeSpan Duration => _duration;

    /// <summary>
    /// Gets the underlying sample buffer.
    /// </summary>
    /// <remarks>
    /// The buffer is <see cref="PinnableArray{T}"/>, which can be pinned to avoid GC relocation if needed.
    /// </remarks>
    public PinnableArray<TSample> Samples => _samples;

    #endregion

    #region ICloneable<T>

    /// <summary>
    /// Creates a copy of the current <see cref="DiscreteSignal{TSample}"/>.
    /// </summary>
    /// <returns>A new <see cref="DiscreteSignal{TSample}"/> with the same sampling rate and samples.</returns>
    public DiscreteSignal<TSample> Clone()
    {
        return new(_samplingRate, _samples, false);
    }

    #endregion

    #region indexer

    /// <summary>
    /// Gets or sets the sample at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Zero-based sample index.</param>
    /// <returns>The sample value at the index.</returns>
    public TSample this[int index]
    {
        get => _samples[index];
        set => _samples[index] = value;
    }

    /// <summary>
    /// Returns a fragment of the signal between <paramref name="startPos"/> (inclusive) and <paramref name="endPos"/> (exclusive).
    /// </summary>
    /// <param name="startPos">Inclusive start index.</param>
    /// <param name="endPos">Exclusive end index.</param>
    /// <returns>A new <see cref="DiscreteSignal{TSample}"/> containing the fragment.</returns>
    public DiscreteSignal<TSample> this[int startPos, int endPos]
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
    /// Returns a fragment of the signal defined by <see cref="Range"/>.
    /// </summary>
    /// <param name="range">Range specifying the fragment [start..end).</param>
    /// <returns>A new <see cref="DiscreteSignal{TSample}"/> containing the fragment.</returns>
    public DiscreteSignal<TSample> this[Range range]
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
    /// Superimposes two signals sample-wise (addition). If sizes differ, the smaller signal is broadcast.
    /// </summary>
    /// <param name="s1">First signal.</param>
    /// <param name="s2">Second signal.</param>
    /// <returns>Resulting superimposed signal.</returns>
    public static DiscreteSignal<TSample> operator +(DiscreteSignal<TSample> s1, DiscreteSignal<TSample> s2)
    {
        return s1.Superimpose(s2);
    }

    /// <summary>
    /// Returns a negated copy of the signal (sample-wise unary minus).
    /// </summary>
    /// <param name="s">Source signal.</param>
    /// <returns>Negated signal.</returns>
    public static DiscreteSignal<TSample> operator -(DiscreteSignal<TSample> s)
    {
        return new DiscreteSignal<TSample>(s._samplingRate, s._samples.Select(x => -x), false);
    }

    /// <summary>
    /// Subtracts <paramref name="s2"/> from <paramref name="s1"/> sample-wise. If sizes differ, the smaller signal is broadcast.
    /// </summary>
    /// <param name="s1">Minuend signal.</param>
    /// <param name="s2">Subtrahend signal.</param>
    /// <returns>Difference signal.</returns>
    public static DiscreteSignal<TSample> operator -(DiscreteSignal<TSample> s1, DiscreteSignal<TSample> s2)
    {
        return s1.Subtract(s2);
    }

    /// <summary>
    /// Adds a constant to each sample.
    /// </summary>
    /// <param name="s">Source signal.</param>
    /// <param name="constant">Constant value to add.</param>
    /// <returns>Resulting signal.</returns>
    public static DiscreteSignal<TSample> operator +(DiscreteSignal<TSample> s, TSample constant)
    {
        return new DiscreteSignal<TSample>(s.SamplingRate, s.Samples.Select(x => x + constant), false);
    }

    /// <summary>
    /// Subtracts a constant from each sample.
    /// </summary>
    /// <param name="s">Source signal.</param>
    /// <param name="constant">Constant value to subtract.</param>
    /// <returns>Resulting signal.</returns>
    public static DiscreteSignal<TSample> operator -(DiscreteSignal<TSample> s, TSample constant)
    {
        return new DiscreteSignal<TSample>(s.SamplingRate, s.Samples.Select(x => x - constant), false);
    }

    /// <summary>
    /// Multiplies each sample by <paramref name="coeff"/> (amplification/attenuation).
    /// </summary>
    /// <param name="s">Source signal.</param>
    /// <param name="coeff">Scaling coefficient.</param>
    /// <returns>Scaled signal.</returns>
    public static DiscreteSignal<TSample> operator *(DiscreteSignal<TSample> s, TSample coeff)
    {
        var signal = s.Clone();
        signal.Amplify(coeff);
        return signal;
    }

    #endregion

    #region time-domain characteristics

    /// <summary>
    /// Computes average energy of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive).</param>
    /// <param name="endPos">Index of the last sample (exclusive).</param>
    /// <returns>Average energy of the fragment.</returns>
    public TSample Energy(int startPos, int endPos)
    {
        var total = TSample.Zero;
        for (var i = startPos; i < endPos; i++)
        {
            total += _samples[i] * _samples[i];
        }

        return total / TSample.CreateChecked(endPos - startPos);
    }

    /// <summary>
    /// Computes average energy of a fragment defined by <paramref name="range"/>.
    /// </summary>
    /// <param name="range">Sample range.</param>
    /// <returns>Average energy of the fragment.</returns>
    public TSample Energy(Range range) => Energy(range.Start.Value, range.End.Value);

    /// <summary>
    /// Computes average energy of the entire signal.
    /// </summary>
    /// <returns>Average energy.</returns>
    public TSample Energy() => Energy(..);

    /// <summary>
    /// Computes RMS value of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive).</param>
    /// <param name="endPos">Index of the last sample (exclusive).</param>
    /// <returns>RMS of the fragment.</returns>
    public TSample Rms(int startPos, int endPos) => TSample.Sqrt(Energy(startPos, endPos));

    /// <summary>
    /// Computes RMS value of a fragment defined by <paramref name="range"/>.
    /// </summary>
    /// <param name="range">Sample range.</param>
    /// <returns>RMS of the fragment.</returns>
    public TSample Rms(Range range) => TSample.Sqrt(Energy(range));

    /// <summary>
    /// Computes RMS value of the entire signal.
    /// </summary>
    /// <returns>RMS value.</returns>
    public TSample Rms() => TSample.Sqrt(Energy());

    /// <summary>
    /// Computes zero-crossing rate of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive).</param>
    /// <param name="endPos">Index of the last sample (exclusive).</param>
    /// <returns>Zero-crossing rate (normalized count).</returns>
    public TSample ZeroCrossingRate(int startPos, int endPos)
    {
        //const float disbalance = 1e-4f;
        TSample disbalance = TSample.CreateChecked(1e-4);

        var prevSample = _samples[startPos] + disbalance;

        var rate = 0;
        for (var i = startPos + 1; i < endPos; i++)
        {
            var sample = _samples[i] + disbalance;

            if ((sample >= TSample.Zero) != (prevSample >= TSample.Zero))
            {
                rate++;
            }

            prevSample = sample;
        }

        return TSample.CreateChecked(rate / (startPos - startPos - 1));
    }

    /// <summary>
    /// Computes zero-crossing rate of a fragment defined by <paramref name="range"/>.
    /// </summary>
    /// <param name="range">Sample range.</param>
    /// <returns>Zero-crossing rate.</returns>
    public TSample ZeroCrossingRate(Range range) => ZeroCrossingRate(range.Start.Value, range.End.Value);

    /// <summary>
    /// Computes zero-crossing rate of the entire signal.
    /// </summary>
    /// <returns>Zero-crossing rate.</returns>
    public TSample ZeroCrossingRate() => ZeroCrossingRate(..);

    /// <summary>
    /// Computes Shannon entropy of a fragment using uniformly distributed bins between min and max of absolute sample values.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive).</param>
    /// <param name="endPos">Index of the last sample (exclusive).</param>
    /// <param name="binCount">Number of histogram bins.</param>
    /// <returns>Shannon entropy (in bits).</returns>
    public TSample Entropy(int startPos, int endPos, int binCount = 32)
    {
        var len = endPos - startPos;

        if (len < binCount)
        {
            binCount = len;
        }

        var bins = new int[binCount + 1];

        var min = _samples[0];
        var max = _samples[0];
        for (var i = startPos; i < endPos; i++)
        {
            var sample = TSample.Abs(_samples[i]);

            if (sample < min)
            {
                min = sample;
            }
            if (sample > max)
            {
                max = sample;
            }
        }

        if (max - min < TSample.CreateChecked(1e-8))
        {
            return TSample.Zero;
        }

        var binLength = (max - min) / TSample.CreateChecked(binCount);

        for (var i = startPos; i < endPos; i++)
        {
            bins[int.CreateChecked((TSample.Abs(_samples[i]) - min) / binLength)]++;
        }

        var entropy = TSample.Zero;
        for (var i = 0; i < binCount; i++)
        {
            var p = TSample.CreateChecked(bins[i] / (endPos - startPos));

            if (p > TSample.CreateChecked(1e-8))
            {
                entropy += p * TSample.Log(p, TSample.CreateTruncating(2));
            }
        }

        return -entropy / TSample.Log(TSample.CreateChecked(binCount), TSample.CreateTruncating(2));
    }

    /// <summary>
    /// Computes Shannon entropy of a fragment defined by <paramref name="range"/>.
    /// </summary>
    /// <param name="range">Sample range.</param>
    /// <param name="binCount">Number of histogram bins.</param>
    /// <returns>Shannon entropy (in bits).</returns>
    public TSample Entropy(Range range, int binCount = 32) => Entropy(range.Start.Value, range.End.Value, binCount);

    /// <summary>
    /// Computes Shannon entropy of the entire signal using uniformly distributed bins.
    /// </summary>
    /// <param name="binCount">Number of histogram bins.</param>
    /// <returns>Shannon entropy (in bits).</returns>
    public TSample Entropy(int binCount = 32) => Entropy(.., binCount);

    #endregion

    #region dispose pattern

    private bool disposedValue;

    /// <summary>
    /// Releases resources used by the signal.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release managed and unmanaged resources; <c>false</c> to release unmanaged only.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _samples.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~DiscreteSignal2()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    /// <summary>
    /// Performs application-defined tasks associated with freeing resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

}