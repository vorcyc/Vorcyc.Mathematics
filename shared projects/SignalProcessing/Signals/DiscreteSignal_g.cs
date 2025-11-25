namespace Vorcyc.Mathematics.SignalProcessing.Signals;

using Vorcyc.Mathematics.Framework;

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

public class DiscreteSignal<TSample> : ICloneable<DiscreteSignal<TSample>>, IDisposable
    where TSample : unmanaged, IFloatingPointIeee754<TSample>
{

    private PinnableArray<TSample> _samples;

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


    public DiscreteSignal(int samplingRate, TSample[] samples, MemoryStrategy strategy = MemoryStrategy.Immediate)
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

    public DiscreteSignal(int samplingRate, TSample[] samples, int offset, int count, MemoryStrategy strategy = MemoryStrategy.Immediate)
         : this(samplingRate, count: count, duration: null, strategy)
    {
        if (strategy == MemoryStrategy.Pre_Allocated)
            _samples = new(samples, offset, count, pin: true);
        else
            _samples = new(samples, offset, count, false);
    }


    public DiscreteSignal(int samplingRate, IEnumerable<TSample> samples, MemoryStrategy strategy = MemoryStrategy.Immediate)
        : this(samplingRate, samples.ToArray(), strategy)
    { }

    public DiscreteSignal(int samplingRate, ArraySegment<TSample> segment, MemoryStrategy strategy = MemoryStrategy.Immediate)
        : this(samplingRate, count: segment.Count, duration: null, strategy)
    {
        if (strategy == MemoryStrategy.Pre_Allocated)
            _samples = new(segment, pin: true);
        else
            _samples = new(segment, false);
    }


    public DiscreteSignal(int samplingRate, Span<TSample> span, MemoryStrategy strategy = MemoryStrategy.Immediate)
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
    public static DiscreteSignal<TSample> Unit(int length, int samplingRate = 1, MemoryStrategy strategy = MemoryStrategy.Pre_Allocated)
    {
        //var unit = new float[length];
        //unit[0] = 1;

        //return new DiscreteSignal(samplingRate, unit);

        var result = new DiscreteSignal<TSample>(samplingRate, length, strategy);
        result._samples[0] = TSample.One;
        return result;
    }

    /// <summary>
    /// Generates constant signal of given <paramref name="length"/> sampled at given <paramref name="samplingRate"/>.
    /// </summary>
    /// <param name="constant">Constant value</param>
    /// <param name="length">Length of constant signal</param>
    /// <param name="samplingRate">Sampling rate</param>
    public static DiscreteSignal<TSample> Constant(TSample constant, int length, int samplingRate = 1, MemoryStrategy strategy = MemoryStrategy.Pre_Allocated)
    {
        var result = new DiscreteSignal<TSample>(samplingRate, length, strategy);
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
    /// The valid sample count.会依据<see cref="AudioFrame_old.BufferPacket.ValidSize"/>而变。
    /// 有效样本量。
    /// </summary>
    public int ValidSampleCount => _validNumOfSample;

    public TimeSpan Duration => _duration;


    public PinnableArray<TSample> Samples => _samples;

    public MemoryStrategy MemoryStrategy => _strategy;

    #endregion


    #region ICloneable<T>

    public DiscreteSignal<TSample> Clone()
    {
        return new( _samplingRate, _samples, _strategy);
    }

    #endregion


    #region indexer

    public TSample this[int index]
    {
        get => _samples[index];
        set => _samples[index] = value;
    }

    public DiscreteSignal<TSample> this[int startPos, int endPos]
    {
        get
        {
            Guard.AgainstInvalidRange(startPos, endPos, "Left index", "Right index");

            // Implementaion is LINQ-less, since Skip() would be less efficient:
            //     return new DiscreteSignal(SamplingRate, Samples.Skip(startPos).Take(endPos - startPos));

            return new(_samplingRate, _samples.Values.FastCopyFragment(endPos - startPos, startPos), _strategy);
        }
    }


    public DiscreteSignal<TSample> this[Range range]
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
    public static DiscreteSignal<TSample> operator +(DiscreteSignal<TSample> s1, DiscreteSignal<TSample> s2)
    {
        return s1.Superimpose(s2);
    }

    /// <summary>
    /// Creates negated copy of signal <paramref name="s"/>.
    /// </summary>
    /// <param name="s">Signal</param>
    public static DiscreteSignal<TSample> operator -(DiscreteSignal<TSample> s)
    {
        return new DiscreteSignal<TSample>(s._samplingRate, s._samples.Select(x => -x), MemoryStrategy.Immediate);
    }

    /// <summary>
    /// Subtracts signal <paramref name="s2"/> from signal <paramref name="s1"/>. 
    /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
    /// </summary>
    /// <param name="s1">First signal</param>
    /// <param name="s2">Second signal</param>
    public static DiscreteSignal<TSample> operator -(DiscreteSignal<TSample> s1, DiscreteSignal<TSample> s2)
    {
        return s1.Subtract(s2);
    }

    /// <summary>
    /// Creates new signal by adding <paramref name="constant"/> to signal <paramref name="s"/>.
    /// </summary>
    /// <param name="s">Signal</param>
    /// <param name="constant">Constant to add to each sample</param>
    public static DiscreteSignal<TSample> operator +(DiscreteSignal<TSample> s, TSample constant)
    {
        return new DiscreteSignal<TSample>(s.SamplingRate, s.Samples.Select(x => x + constant), MemoryStrategy.Immediate);
    }

    /// <summary>
    /// Creates new signal by subtracting <paramref name="constant"/> from signal <paramref name="s"/>.
    /// </summary>
    /// <param name="s">Signal</param>
    /// <param name="constant">Constant to subtract from each sample</param>
    public static DiscreteSignal<TSample> operator -(DiscreteSignal<TSample> s, TSample constant)
    {
        return new DiscreteSignal<TSample>(s.SamplingRate, s.Samples.Select(x => x - constant), MemoryStrategy.Immediate);
    }

    /// <summary>
    /// Creates new signal by multiplying <paramref name="s"/> by <paramref name="coeff"/> (amplification/attenuation).
    /// </summary>
    /// <param name="s">Signal</param>
    /// <param name="coeff">Amplification/attenuation coefficient</param>
    public static DiscreteSignal<TSample> operator *(DiscreteSignal<TSample> s, TSample coeff)
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
    public TSample Energy(int startPos, int endPos)
    {
        var total = TSample.Zero;
        for (var i = startPos; i < endPos; i++)
        {
            total += _samples[i] * _samples[i];
        }

        return total / TSample.CreateChecked(endPos - startPos);
    }

    public TSample Energy(Range range) => Energy(range.Start.Value, range.End.Value);

    /// <summary>
    /// Computes energy of entire signal.
    /// </summary>
    public TSample Energy() => Energy(..);



    public TSample Rms(int startPos, int endPos) => TSample.Sqrt(Energy(startPos, endPos));

    /// <summary>
    /// Computes RMS of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive)</param>
    /// <param name="endPos">Index of the last sample (exclusive)</param>
    public TSample Rms(Range range) => TSample.Sqrt(Energy(range));

    /// <summary>
    /// Computes RMS of entire signal.
    /// </summary>
    public TSample Rms() => TSample.Sqrt(Energy());



    /// <summary>
    /// Computes Zero-crossing rate of a signal fragment.
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive)</param>
    /// <param name="endPos">Index of the last sample (exclusive)</param>
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


    public TSample ZeroCrossingRate(Range range) => ZeroCrossingRate(range.Start.Value, range.End.Value);


    /// <summary>
    /// Computes Zero-crossing rate of entire signal.
    /// </summary>
    public TSample ZeroCrossingRate() => ZeroCrossingRate(..);



    /// <summary>
    /// Computes Shannon entropy of a signal fragment 
    /// (from bins distributed uniformly between the minimum and maximum values of samples).
    /// </summary>
    /// <param name="startPos">Index of the first sample (inclusive)</param>
    /// <param name="endPos">Index of the last sample (exclusive)</param>
    /// <param name="binCount">Number of bins</param>
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


    public TSample Entropy(Range range, int binCount = 32) => Entropy(range.Start.Value, range.End.Value, binCount);

    /// <summary>
    /// Computes Shannon entropy of entire signal 
    /// (from bins distributed uniformly between the minimum and maximum values of samples).
    /// </summary>
    /// <param name="binCount">Number of bins</param>
    public TSample Entropy(int binCount = 32) => Entropy(.., binCount);



    #endregion






    #region dispose pattern

    private bool disposedValue;

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

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    #endregion

}
