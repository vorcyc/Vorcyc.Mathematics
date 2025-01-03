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
/// Base class for finite real-valued discrete-time signals. 
/// In general, any finite DT signal is simply an array of data sampled at certain sampling rate. 
/// See also <see cref="DiscreteSignalExtensions"/> for extra functionality of DT signals.
/// </summary>
public class DiscreteSignal : ICloneable<DiscreteSignal>, IDisposable
{


    private PinnableArray<float> _samples;

    private readonly int _samplingRate;

    //样本量
    internal int _numOfSample;

    //有效样本量
    internal int _validNumOfSample;

    internal TimeSpan _duration;

    private MemoryStrategy _strategy;


    #region constructors

    private DiscreteSignal(int samplingRate, int? count, TimeSpan? duration, MemoryStrategy strategy = MemoryStrategy.Immediate)
    {
        Guard.AgainstNonPositive(samplingRate, "Sampling rate");

        if (count is null && duration is null)
            throw new ArgumentNullException("Both paramaters of count and duration cannot null.");

        _samplingRate = samplingRate;
        _strategy = strategy;

        if (count is not null && duration is null)
        {
            _numOfSample = count.Value;
            _validNumOfSample = count.Value;
            _duration = TimeSpan.FromSeconds((double)_numOfSample / samplingRate);
        }
        else if (duration is not null && count is null)
        {
            // 此时这里 _frame 一定是空的
            {
                _duration = duration.Value;
                _numOfSample = (int)(duration.Value.TotalSeconds * samplingRate);
                _validNumOfSample = (int)(duration.Value.TotalSeconds * samplingRate);

            }
        }
    }

    public DiscreteSignal(int samplingRate, TimeSpan duration, MemoryStrategy strategy = MemoryStrategy.Immediate)
        : this(samplingRate, count: null, duration: duration, strategy)
    {
        if (strategy == MemoryStrategy.Pre_Allocated)
            _samples = new(_numOfSample);
        else
            _samples = new(_numOfSample, false);
    }

    public DiscreteSignal(int samplingRate, int count, MemoryStrategy strategy = MemoryStrategy.Immediate)
        : this(samplingRate, count: count, duration: null, strategy)
    {
        if (strategy == MemoryStrategy.Pre_Allocated)
            _samples = new(_numOfSample);
        else
            _samples = new(_numOfSample, false);
    }


    public DiscreteSignal(int samplingRate, float[] samples, MemoryStrategy strategy = MemoryStrategy.Immediate)
        : this(samplingRate, count: samples.Length, duration: null, strategy)
    {
        //if (strategy == PacketMemoryStrategy.Pre_Allocated)
        //    _samples = new(samples, pin: true, copy: true);
        //else
        //    _samples = new(samples, false, copy: true);

        if (strategy == MemoryStrategy.Pre_Allocated)
            _samples = new(samples, pin: true);
        else
            _samples = new(samples, false);
    }

    public DiscreteSignal(int samplingRate, float[] samples, int offset, int count, MemoryStrategy strategy = MemoryStrategy.Immediate)
         : this(samplingRate, count: count, duration: null, strategy)
    {
        if (strategy == MemoryStrategy.Pre_Allocated)
            _samples = new(samples, offset, count, pin: true);
        else
            _samples = new(samples, offset, count, false);
    }


    public DiscreteSignal(int samplingRate, IEnumerable<float> samples, MemoryStrategy strategy = MemoryStrategy.Immediate)
        : this(samplingRate, samples.ToArray(), strategy)
    { }

    public DiscreteSignal(int samplingRate, ArraySegment<float> segment, MemoryStrategy strategy = MemoryStrategy.Immediate)
        : this(samplingRate, count: segment.Count, duration: null, strategy)
    {
        if (strategy == MemoryStrategy.Pre_Allocated)
            _samples = new(segment, pin: true);
        else
            _samples = new(segment, false);
    }


    public DiscreteSignal(int samplingRate, Span<float> span, MemoryStrategy strategy = MemoryStrategy.Immediate)
         : this(samplingRate, count: span.Length, duration: null, strategy)
    {
        if (strategy == MemoryStrategy.Pre_Allocated)
            _samples = new(span, pin: true);
        else
            _samples = new(span, false);
    }



    #endregion


    #region 特殊常量信号

    /// <summary>
    /// Generates unit impulse of given <paramref name="length"/> sampled at given <paramref name="samplingRate"/>.
    /// </summary>
    /// <param name="length">Length of unit impulse</param>
    /// <param name="samplingRate">Sampling rate</param>
    public static DiscreteSignal Unit(int length, int samplingRate = 1, MemoryStrategy strategy = MemoryStrategy.Pre_Allocated)
    {
        //var unit = new float[length];
        //unit[0] = 1;

        //return new DiscreteSignal(samplingRate, unit);

        var result = new DiscreteSignal(samplingRate, length, strategy);
        result._samples[0] = 1;
        return result;
    }

    /// <summary>
    /// Generates constant signal of given <paramref name="length"/> sampled at given <paramref name="samplingRate"/>.
    /// </summary>
    /// <param name="constant">Constant value</param>
    /// <param name="length">Length of constant signal</param>
    /// <param name="samplingRate">Sampling rate</param>
    public static DiscreteSignal Constant(float constant, int length, int samplingRate = 1, MemoryStrategy strategy = MemoryStrategy.Pre_Allocated)
    {
        var result = new DiscreteSignal(samplingRate, length, strategy);
        result._samples.Fill(constant);
        return result;
    }

    #endregion


    #region properites


    public int SamplingRate => _samplingRate;

    /// <summary>
    /// 样本量，分配后就不变。
    /// </summary>
    public int SampleCount => _numOfSample;

    /// <summary>
    /// The valid sample count.会依据<see cref="AudioFrame.Buffer.ValidSize"/>而变。
    /// 有效样本量。
    /// </summary>
    public int ValidSampleCount => _validNumOfSample;

    public TimeSpan Duration => _duration;


    public PinnableArray<float> Samples => _samples;

    public MemoryStrategy MemoryStrategy => _strategy;

    #endregion


    #region ICloneable<T>

    public DiscreteSignal Clone()
    {
        return new(_samplingRate, _samples, _strategy);
    }

    #endregion


    #region indexer

    public float this[int index]
    {
        get => _samples[index];
        set => _samples[index] = value;
    }

    public DiscreteSignal this[int startPos, int endPos]
    {
        get
        {
            Guard.AgainstInvalidRange(startPos, endPos, "Left index", "Right index");

            // Implementaion is LINQ-less, since Skip() would be less efficient:
            //     return new DiscreteSignal(SamplingRate, Samples.Skip(startPos).Take(endPos - startPos));

            return new(_samplingRate, _samples.Values.FastCopyFragment(endPos - startPos, startPos), _strategy);
        }
    }


    public DiscreteSignal this[Range range]
    {
        get
        {
            //RuntimeHelpers.GetSubArray
            return new(
                _samplingRate,
                _samples.Values.FastCopyFragment(range.End.Value - range.Start.Value, range.Start.Value),
                _strategy);
        }
    }

    #endregion


    #region operators


    /// <summary>
    /// Creates new signal by superimposing signals <paramref name="s1"/> and <paramref name="s2"/>. 
    /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
    /// </summary>
    /// <param name="s1">First signal</param>
    /// <param name="s2">Second signal</param>
    public static DiscreteSignal operator +(DiscreteSignal s1, DiscreteSignal s2)
    {
        return s1.Superimpose(s2);
    }

    /// <summary>
    /// Creates negated copy of signal <paramref name="s"/>.
    /// </summary>
    /// <param name="s">Signal</param>
    public static DiscreteSignal operator -(DiscreteSignal s)
    {
        return new DiscreteSignal(s._samplingRate, s._samples.Select(x => -x), MemoryStrategy.Immediate);
    }

    /// <summary>
    /// Subtracts signal <paramref name="s2"/> from signal <paramref name="s1"/>. 
    /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
    /// </summary>
    /// <param name="s1">First signal</param>
    /// <param name="s2">Second signal</param>
    public static DiscreteSignal operator -(DiscreteSignal s1, DiscreteSignal s2)
    {
        return s1.Subtract(s2);
    }

    /// <summary>
    /// Creates new signal by adding <paramref name="constant"/> to signal <paramref name="s"/>.
    /// </summary>
    /// <param name="s">Signal</param>
    /// <param name="constant">Constant to add to each sample</param>
    public static DiscreteSignal operator +(DiscreteSignal s, float constant)
    {
        return new DiscreteSignal(s.SamplingRate, s.Samples.Select(x => x + constant), MemoryStrategy.Immediate);
    }

    /// <summary>
    /// Creates new signal by subtracting <paramref name="constant"/> from signal <paramref name="s"/>.
    /// </summary>
    /// <param name="s">Signal</param>
    /// <param name="constant">Constant to subtract from each sample</param>
    public static DiscreteSignal operator -(DiscreteSignal s, float constant)
    {
        return new DiscreteSignal(s.SamplingRate, s.Samples.Select(x => x - constant), MemoryStrategy.Immediate);
    }

    /// <summary>
    /// Creates new signal by multiplying <paramref name="s"/> by <paramref name="coeff"/> (amplification/attenuation).
    /// </summary>
    /// <param name="s">Signal</param>
    /// <param name="coeff">Amplification/attenuation coefficient</param>
    public static DiscreteSignal operator *(DiscreteSignal s, float coeff)
    {
        var signal = s.Clone();
        signal.Amplify(coeff);
        return signal;
    }


    #endregion




    #region time-domain characteristics

    /// <summary>
    /// Computes energy of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive)</param>
    /// <param name="endPos">Index of the last sample (exclusive)</param>
    public float Energy(int startPos, int endPos)
    {
        var total = 0.0f;
        for (var i = startPos; i < endPos; i++)
        {
            total += Samples[i] * Samples[i];
        }

        return total / (endPos - startPos);
    }


    public float Energy(Range range) => Energy(range.Start.Value, range.End.Value);

    /// <summary>
    /// Computes energy of entire signal.
    /// </summary>
    public float Energy() => Energy(0, SampleCount);

    /// <summary>
    /// Computes RMS of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive)</param>
    /// <param name="endPos">Index of the last sample (exclusive)</param>
    public float Rms(int startPos, int endPos)
    {
        return MathF.Sqrt(Energy(startPos, endPos));
    }

    /// <summary>
    /// Computes RMS of a signal fragment.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public float Rms(Range range) => MathF.Sqrt(Energy(range));

    /// <summary>
    /// Computes RMS of entire signal.
    /// </summary>
    public float Rms() => MathF.Sqrt(Energy(0, SampleCount));

    /// <summary>
    /// Computes Zero-crossing rate of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive)</param>
    /// <param name="endPos">Index of the last sample (exclusive)</param>
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


    public float ZeroCrossingRate(Range range) => ZeroCrossingRate(range.Start.Value, range.End.Value);

    /// <summary>
    /// Computes Zero-crossing rate of entire signal.
    /// </summary>
    public float ZeroCrossingRate() => ZeroCrossingRate(0, SampleCount);// ZeroCrossingRate(..);

    /// <summary>
    /// Computes Shannon entropy of a signal fragment 
    /// (from bins distributed uniformly between the minimum and maximum values of samples).
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive)</param>
    /// <param name="endPos">Index of the last sample (exclusive)</param>
    /// <param name="binCount">Number of bins</param>
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
    /// Computes Shannon entropy of entire signal 
    /// (from bins distributed uniformly between the minimum and maximum values of samples).
    /// </summary>
    /// <param name="binCount">Number of bins</param>
    public float Entropy(int binCount = 32) => Entropy(0, SampleCount, binCount);

    #endregion


    #region Conversion with DiscreteSignal<float>


    public static DiscreteSignal From(DiscreteSignal<float> signal)
        => signal;


    public static implicit operator DiscreteSignal(DiscreteSignal<float> signal)
    {
        return new(signal.SamplingRate, signal.Samples, signal.MemoryStrategy);
    }



    public static implicit operator DiscreteSignal<float>(DiscreteSignal signal)
    {
        return new(signal.SamplingRate, signal.Samples, signal.MemoryStrategy);
    }


    #endregion




    #region dispose pattern

    private bool _isDisposed;

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

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    #endregion

}
