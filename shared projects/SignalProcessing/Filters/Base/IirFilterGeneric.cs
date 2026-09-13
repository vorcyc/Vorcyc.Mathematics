using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// IIR difference-equation filter in <typeparamref name="T"/>.
/// <see cref="float"/> matches <see cref="IirFilter"/> numerically; <see cref="double"/>
/// keeps coefficient and state precision (the existing
/// <c>IirFilter(IEnumerable&lt;double&gt;, IEnumerable&lt;double&gt;)</c> ctor still casts to float).
/// Use <see cref="TransferFunction{T}"/> from generic FDA, or widen a float
/// <see cref="TransferFunction"/> via <see cref="IirFilter{T}(TransferFunction)"/>.
/// </summary>
public class IirFilter<T> : IOnlineFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    protected readonly T[] _b;
    protected readonly T[] _a;
    protected readonly int _numeratorSize;
    protected readonly int _denominatorSize;
    protected TransferFunction<T>? _tf;
    protected readonly T[] _delayLineA;
    protected readonly T[] _delayLineB;
    protected int _delayLineOffsetA;
    protected int _delayLineOffsetB;

    /// <summary>Gets or sets default truncated impulse-response length for analysis helpers.</summary>
    public int DefaultImpulseResponseLength { get; set; } = 512;

    /// <summary>Gets the transfer function (lazily built from coefficients).</summary>
    public TransferFunction<T> Tf
    {
        get
        {
            if (_tf is not null) return _tf;
            var num = new T[_numeratorSize];
            _b.AsSpan(0, _numeratorSize).CopyTo(num);
            _tf = new TransferFunction<T>(num, (T[])_a.Clone());
            return _tf;
        }
        protected set => _tf = value;
    }

    /// <summary>Constructs from numerator <paramref name="b"/> and denominator <paramref name="a"/>.</summary>
    public IirFilter(IEnumerable<T> b, IEnumerable<T> a)
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("IirFilter<T> supports float and double only.");

        ArgumentNullException.ThrowIfNull(b);
        ArgumentNullException.ThrowIfNull(a);

        var bArr = b as T[] ?? b.ToArray();
        var aArr = a as T[] ?? a.ToArray();
        if (bArr.Length == 0 || aArr.Length == 0)
            throw new ArgumentException("Numerator and denominator must be non-empty.");

        _numeratorSize = bArr.Length;
        _denominatorSize = aArr.Length;
        _b = new T[_numeratorSize * 2];
        for (var i = 0; i < _numeratorSize; i++)
            _b[i] = _b[_numeratorSize + i] = bArr[i];
        _a = (T[])aArr.Clone();

        _delayLineB = new T[_numeratorSize];
        _delayLineA = new T[_denominatorSize];
        _delayLineOffsetB = _numeratorSize - 1;
        _delayLineOffsetA = _denominatorSize - 1;
    }

    /// <summary>Constructs from a generic transfer function.</summary>
    public IirFilter(TransferFunction<T> tf)
        : this(tf.Numerator, tf.Denominator)
    {
        Tf = tf;
    }

    /// <summary>Widens a float transfer function (FDA output) into this precision.</summary>
    public IirFilter(TransferFunction tf)
        : this(TransferFunction<T>.From(tf))
    {
    }

    /// <inheritdoc />
    public virtual T Process(T sample)
    {
        var output = T.Zero;
        _delayLineB[_delayLineOffsetB] = sample;

        for (int i = 0, j = _numeratorSize - _delayLineOffsetB; i < _numeratorSize; i++, j++)
            output += _delayLineB[i] * _b[j];

        var pos = 1;
        for (var p = _delayLineOffsetA + 1; p < _a.Length; p++)
            output -= _a[pos++] * _delayLineA[p];
        for (var p = 0; p < _delayLineOffsetA; p++)
            output -= _a[pos++] * _delayLineA[p];

        _delayLineA[_delayLineOffsetA] = output;

        if (--_delayLineOffsetB < 0)
            _delayLineOffsetB = _numeratorSize - 1;
        if (--_delayLineOffsetA < 0)
            _delayLineOffsetA = _denominatorSize - 1;

        return output;
    }

    /// <summary>Filters an entire buffer (does not reset first).</summary>
    public T[] ApplyTo(ReadOnlySpan<T> samples)
    {
        var output = new T[samples.Length];
        for (int i = 0; i < samples.Length; i++)
            output[i] = Process(samples[i]);
        return output;
    }

    /// <summary>Direct difference-equation (no delay-line wrap), matching <see cref="IirFilter"/>'s <c>DifferenceEquation</c> path.</summary>
    public T[] ApplyDifferenceEquation(ReadOnlySpan<T> samples)
    {
        var output = new T[samples.Length];
        for (var n = 0; n < samples.Length; n++)
        {
            for (var k = 0; k < _numeratorSize; k++)
            {
                if (n >= k) output[n] += _b[k] * samples[n - k];
            }
            for (var m = 1; m < _denominatorSize; m++)
            {
                if (n >= m) output[n] -= _a[m] * output[n - m];
            }
        }
        return output;
    }

    /// <summary>Replaces numerator coefficients online (same length required).</summary>
    public void ChangeNumeratorCoeffs(ReadOnlySpan<T> b)
    {
        if (b.Length != _numeratorSize) return;
        for (var i = 0; i < _numeratorSize; i++)
            _b[i] = _b[_numeratorSize + i] = b[i];
    }

    /// <summary>Replaces denominator coefficients online (same length required).</summary>
    public void ChangeDenominatorCoeffs(ReadOnlySpan<T> a)
    {
        if (a.Length != _denominatorSize) return;
        a.CopyTo(_a);
    }

    /// <summary>Replaces coefficients from a transfer function (same lengths required).</summary>
    public void Change(TransferFunction<T> tf)
    {
        ChangeNumeratorCoeffs(tf.Numerator);
        ChangeDenominatorCoeffs(tf.Denominator);
        _tf = tf;
    }

    /// <inheritdoc />
    public virtual void Reset()
    {
        _delayLineOffsetB = _numeratorSize - 1;
        _delayLineOffsetA = _denominatorSize - 1;
        Array.Clear(_delayLineB);
        Array.Clear(_delayLineA);
    }

    /// <summary>Divides all coefficients by a[0].</summary>
    public void Normalize()
    {
        var a0 = _a[0];
        if (T.Abs(a0 - T.One) < T.CreateChecked(1e-10))
            return;
        if (T.Abs(a0) < T.CreateChecked(1e-30))
            throw new ArgumentException("The coefficient a[0] can not be zero!");

        for (var i = 0; i < _a.Length; i++)
            _a[i] /= a0;
        for (var i = 0; i < _b.Length; i++)
            _b[i] /= a0;
        _tf?.Normalize();
    }

    /// <summary>Series connection.</summary>
    public static IirFilter<T> operator *(IirFilter<T> filter1, IirFilter<T> filter2)
        => new(filter1.Tf * filter2.Tf);

    /// <summary>Parallel connection.</summary>
    public static IirFilter<T> operator +(IirFilter<T> filter1, IirFilter<T> filter2)
        => new(filter1.Tf + filter2.Tf);
}

/// <summary>Double-precision IIR filter (same as <see cref="IirFilter{T}"/> of <see cref="double"/>).</summary>
public sealed class IirFilter64 : IirFilter<double>
{
    /// <inheritdoc />
    public IirFilter64(IEnumerable<double> b, IEnumerable<double> a) : base(b, a)
    {
    }

    /// <inheritdoc />
    public IirFilter64(TransferFunction<double> tf) : base(tf)
    {
    }

    /// <summary>Widens a float transfer function.</summary>
    public IirFilter64(TransferFunction tf) : base(tf)
    {
    }
}
