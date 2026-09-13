using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// LTI filter with a state vector (initial conditions) in <typeparamref name="T"/>.
/// Provides <see cref="ZeroPhase"/> (filtfilt). <see cref="float"/> matches <see cref="ZiFilter"/>.
/// </summary>
public class ZiFilter<T> : IOnlineFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    protected T[] _b;
    protected T[] _a;
    protected readonly T[] _zi;
    protected TransferFunction<T>? _tf;

    /// <summary>Gets the state vector.</summary>
    public T[] Zi => _zi;

    /// <summary>Gets the transfer function.</summary>
    public TransferFunction<T> Tf
    {
        get => _tf ?? new TransferFunction<T>(FilterGenericSupport.Clone(_b), FilterGenericSupport.Clone(_a));
        protected set => _tf = value;
    }

    /// <summary>Constructs from numerator and denominator (padded to equal length).</summary>
    public ZiFilter(IEnumerable<T> b, IEnumerable<T> a)
    {
        FilterGenericSupport.EnsureFloatOrDouble<T>();
        ArgumentNullException.ThrowIfNull(b);
        ArgumentNullException.ThrowIfNull(a);

        _b = b as T[] ?? b.ToArray();
        _a = a as T[] ?? a.ToArray();
        if (_b.Length == 0 || _a.Length == 0)
            throw new ArgumentException("Numerator and denominator must be non-empty.");

        var maxLength = Math.Max(_a.Length, _b.Length);
        if (_b.Length < maxLength)
            _b = FilterGenericSupport.PadTo(_b, maxLength);
        if (_a.Length < maxLength)
            _a = FilterGenericSupport.PadTo(_a, maxLength);

        _zi = new T[maxLength];
    }

    /// <summary>Constructs from a generic transfer function.</summary>
    public ZiFilter(TransferFunction<T> tf)
        : this(tf.Numerator, tf.Denominator)
    {
        Tf = tf;
    }

    /// <summary>Widens a float transfer function.</summary>
    public ZiFilter(TransferFunction tf)
        : this(TransferFunction<T>.From(tf))
    {
    }

    /// <summary>Initializes the state vector.</summary>
    public virtual void Init(ReadOnlySpan<T> zi)
        => zi[..Math.Min(zi.Length, _zi.Length)].CopyTo(_zi);

    /// <inheritdoc />
    public virtual T Process(T sample)
    {
        var output = _b[0] * sample + _zi[0];
        for (var j = 1; j < _zi.Length; j++)
            _zi[j - 1] = _b[j] * sample - _a[j] * output + _zi[j];
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

    /// <summary>
    /// Zero-phase filtering (MATLAB / SciPy <c>filtfilt</c>).
    /// Default pad length is 3 × (max(len(b), len(a)) − 1).
    /// </summary>
    public T[] ZeroPhase(ReadOnlySpan<T> input, int padLength = 0)
    {
        if (padLength <= 0)
            padLength = 3 * (Math.Max(_a.Length, _b.Length) - 1);

        if (padLength >= input.Length)
            throw new ArgumentException("pad length must be less than the signal length.");

        var output = new T[input.Length];
        var edgeLeft = new T[padLength];
        var edgeRight = new T[padLength];
        var two = T.CreateChecked(2);

        var initialZi = Tf.Zi;
        var zi = FilterGenericSupport.Clone(initialZi);
        var baseSample = two * input[0] - input[padLength];
        for (int i = 0; i < zi.Length; i++)
            zi[i] *= baseSample;
        Init(zi);

        baseSample = input[0];
        for (int k = 0, i = padLength; i > 0; k++, i--)
            edgeLeft[k] = Process(two * baseSample - input[i]);

        for (int i = 0; i < input.Length; i++)
            output[i] = Process(input[i]);

        baseSample = input[input.Length - 1];
        for (int k = 0, i = input.Length - 2; i > input.Length - 2 - padLength; k++, i--)
            edgeRight[k] = Process(two * baseSample - input[i]);

        zi = FilterGenericSupport.Clone(initialZi);
        baseSample = edgeRight[edgeRight.Length - 1];
        for (int i = 0; i < zi.Length; i++)
            zi[i] *= baseSample;
        Init(zi);

        for (int i = padLength - 1; i >= 0; i--)
            Process(edgeRight[i]);
        for (int i = output.Length - 1; i >= 0; i--)
            output[i] = Process(output[i]);
        for (int i = padLength - 1; i >= 0; i--)
            Process(edgeLeft[i]);

        return output;
    }

    /// <summary>Replaces numerator coefficients online (same length required).</summary>
    public void ChangeNumeratorCoeffs(ReadOnlySpan<T> b)
    {
        if (b.Length == _b.Length)
            b.CopyTo(_b);
    }

    /// <summary>Replaces denominator coefficients online (same length required).</summary>
    public void ChangeDenominatorCoeffs(ReadOnlySpan<T> a)
    {
        if (a.Length == _a.Length)
            a.CopyTo(_a);
    }

    /// <summary>Replaces coefficients from a transfer function (padded to current length).</summary>
    public void Change(TransferFunction<T> tf)
    {
        var b = FilterGenericSupport.PadTo(tf.Numerator, _b.Length);
        var a = FilterGenericSupport.PadTo(tf.Denominator, _a.Length);
        if (b.Length == _b.Length)
            b.CopyTo(_b, 0);
        if (a.Length == _a.Length)
            a.CopyTo(_a, 0);
        _tf = tf;
    }

    /// <inheritdoc />
    public virtual void Reset() => Array.Clear(_zi);
}

/// <summary>Double-precision state-vector filter (same as <see cref="ZiFilter{T}"/> of <see cref="double"/>).</summary>
public sealed class ZiFilter64 : ZiFilter<double>
{
    /// <inheritdoc />
    public ZiFilter64(IEnumerable<double> b, IEnumerable<double> a) : base(b, a)
    {
    }

    /// <inheritdoc />
    public ZiFilter64(TransferFunction<double> tf) : base(tf)
    {
    }

    /// <summary>Widens a float transfer function.</summary>
    public ZiFilter64(TransferFunction tf) : base(tf)
    {
    }
}
