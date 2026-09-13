using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// FIR filter in <typeparamref name="T"/>. <see cref="float"/> matches <see cref="FirFilter"/>
/// numerically; <see cref="double"/> keeps kernel precision (the existing
/// <c>FirFilter(IEnumerable&lt;double&gt;)</c> ctor still casts to float).
/// </summary>
public class FirFilter<T> : IOnlineFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    protected readonly T[] _b;
    protected int _kernelSize;
    protected TransferFunction<T>? _tf;
    protected readonly T[] _delayLine;
    protected int _delayLineOffset;

    /// <summary>Gets a copy of the filter kernel.</summary>
    public T[] Kernel
    {
        get
        {
            var k = new T[_kernelSize];
            _b.AsSpan(0, _kernelSize).CopyTo(k);
            return k;
        }
    }

    /// <summary>Gets the transfer function (FIR: denominator = 1).</summary>
    public TransferFunction<T> Tf
    {
        get
        {
            if (_tf is not null) return _tf;
            _tf = new TransferFunction<T>(Kernel);
            return _tf;
        }
        protected set => _tf = value;
    }

    /// <summary>Constructs from a kernel.</summary>
    public FirFilter(IEnumerable<T> kernel)
    {
        FilterGenericSupport.EnsureFloatOrDouble<T>();
        ArgumentNullException.ThrowIfNull(kernel);
        var k = kernel as T[] ?? kernel.ToArray();
        if (k.Length == 0)
            throw new ArgumentException("FIR kernel must be non-empty.");

        _kernelSize = k.Length;
        _b = new T[_kernelSize * 2];
        for (var i = 0; i < _kernelSize; i++)
            _b[i] = _b[_kernelSize + i] = k[i];
        _delayLine = new T[_kernelSize];
        _delayLineOffset = _kernelSize - 1;
    }

    /// <summary>Constructs from a generic transfer function (uses the numerator).</summary>
    public FirFilter(TransferFunction<T> tf)
        : this(tf.Numerator)
    {
        Tf = tf;
    }

    /// <summary>Widens a float transfer function.</summary>
    public FirFilter(TransferFunction tf)
        : this(TransferFunction<T>.From(tf))
    {
    }

    /// <inheritdoc />
    public T Process(T sample)
    {
        _delayLine[_delayLineOffset] = sample;
        var output = T.Zero;
        for (int i = 0, j = _kernelSize - _delayLineOffset; i < _kernelSize; i++, j++)
            output += _delayLine[i] * _b[j];
        if (--_delayLineOffset < 0)
            _delayLineOffset = _kernelSize - 1;
        return output;
    }

    /// <summary>Filters a buffer (does not reset first). Output length is input + kernel − 1.</summary>
    public T[] ProcessAllSamples(ReadOnlySpan<T> samples)
    {
        var filtered = new T[samples.Length + _kernelSize - 1];
        var k = 0;
        while (k < samples.Length)
        {
            _delayLine[_delayLineOffset] = samples[k];
            var output = T.Zero;
            for (int i = 0, j = _kernelSize - _delayLineOffset; i < _kernelSize; i++, j++)
                output += _delayLine[i] * _b[j];
            if (--_delayLineOffset < 0)
                _delayLineOffset = _kernelSize - 1;
            filtered[k++] = output;
        }
        while (k < filtered.Length)
            filtered[k++] = Process(T.Zero);
        return filtered;
    }

    /// <summary>Filters an entire buffer without the FIR tail (same length as input).</summary>
    public T[] ApplyTo(ReadOnlySpan<T> samples)
    {
        var output = new T[samples.Length];
        for (int i = 0; i < samples.Length; i++)
            output[i] = Process(samples[i]);
        return output;
    }

    /// <summary>Direct convolution matching <see cref="FirFilter"/>'s difference-equation path (includes tail).</summary>
    public T[] ApplyDifferenceEquation(ReadOnlySpan<T> samples)
    {
        var output = new T[samples.Length + _kernelSize - 1];
        for (var n = 0; n < output.Length; n++)
        {
            for (var k = 0; k < _kernelSize; k++)
            {
                if (n >= k && n < samples.Length + k)
                    output[n] += _b[k] * samples[n - k];
            }
        }
        return output;
    }

    /// <summary>Replaces the kernel online (same length required).</summary>
    public void ChangeKernel(ReadOnlySpan<T> kernel)
    {
        if (kernel.Length != _kernelSize) return;
        for (var i = 0; i < _kernelSize; i++)
            _b[i] = _b[_kernelSize + i] = kernel[i];
    }

    /// <inheritdoc />
    public void Reset()
    {
        _delayLineOffset = _kernelSize - 1;
        Array.Clear(_delayLine);
    }

    /// <summary>Series connection of two FIR filters.</summary>
    public static FirFilter<T> operator *(FirFilter<T> filter1, FirFilter<T> filter2)
        => new(filter1.Tf * filter2.Tf);

    /// <summary>Series connection of FIR and IIR.</summary>
    public static IirFilter<T> operator *(FirFilter<T> filter1, IirFilter<T> filter2)
        => new(filter1.Tf * filter2.Tf);

    /// <summary>Parallel connection of two FIR filters.</summary>
    public static FirFilter<T> operator +(FirFilter<T> filter1, FirFilter<T> filter2)
        => new(filter1.Tf + filter2.Tf);

    /// <summary>Parallel connection of FIR and IIR.</summary>
    public static IirFilter<T> operator +(FirFilter<T> filter1, IirFilter<T> filter2)
        => new(filter1.Tf + filter2.Tf);
}

/// <summary>Double-precision FIR filter (same as <see cref="FirFilter{T}"/> of <see cref="double"/>).</summary>
public sealed class FirFilter64 : FirFilter<double>
{
    /// <inheritdoc />
    public FirFilter64(IEnumerable<double> kernel) : base(kernel)
    {
    }

    /// <inheritdoc />
    public FirFilter64(TransferFunction<double> tf) : base(tf)
    {
    }

    /// <summary>Widens a float transfer function.</summary>
    public FirFilter64(TransferFunction tf) : base(tf)
    {
    }
}
