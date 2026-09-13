using System.Globalization;
using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// Transfer function with coefficients in <typeparamref name="T"/>.
/// Covers construction (including zpk / state-space), impulse / frequency / group / phase
/// response, steady-state <see cref="Zi"/>, normalize, and series / parallel combination.
/// </summary>
public class TransferFunction<T>
    where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    /// <summary>Gets numerator coefficients.</summary>
    public T[] Numerator { get; protected set; }

    /// <summary>Gets denominator coefficients.</summary>
    public T[] Denominator { get; protected set; }

    /// <summary>Gets or sets max iterations for calculating zeros/poles. Default 25000.</summary>
    public int CalculateZpIterations { get; set; } = VMath.PolyRootsIterations;

    protected Complex<T>[]? _zeros;
    protected Complex<T>[]? _poles;

    /// <summary>Gets zeros ('z' in zpk).</summary>
    public Complex<T>[]? Zeros => _zeros ?? TfToZp(Numerator, CalculateZpIterations);

    /// <summary>Gets poles ('p' in zpk).</summary>
    public Complex<T>[]? Poles => _poles ?? TfToZp(Denominator, CalculateZpIterations);

    /// <summary>Gets gain (first numerator coefficient).</summary>
    public T Gain => Numerator.Length == 0 ? T.Zero : Numerator[0];

    /// <summary>Constructs a transfer function from numerator and denominator.</summary>
    public TransferFunction(T[] numerator, T[]? denominator = null)
    {
        ArgumentNullException.ThrowIfNull(numerator);
        if (numerator.Length == 0)
            throw new ArgumentException("Numerator must not be empty.", nameof(numerator));
        Numerator = numerator;
        Denominator = denominator is { Length: > 0 } ? denominator : [T.One];
    }

    /// <summary>Constructs from zeros, poles and gain.</summary>
    public TransferFunction(Complex<T>[]? zeros, Complex<T>[]? poles, T? gain = null)
    {
        _zeros = zeros;
        _poles = poles;
        var k = gain ?? T.One;

        Denominator = poles is { Length: > 0 } ? ZpToTf(poles) : [T.One];
        Numerator = zeros is { Length: > 0 } ? ZpToTf(zeros) : [T.One];
        for (var i = 0; i < Numerator.Length; i++)
            Numerator[i] *= k;
    }

    /// <summary>Constructs from a state-space representation.</summary>
    public TransferFunction(StateSpace<T> stateSpace)
    {
        ArgumentNullException.ThrowIfNull(stateSpace);
        var a = stateSpace.A;
        Denominator = new T[a.Rows + 1];
        Denominator[0] = T.One;
        for (var i = 1; i < Denominator.Length; i++)
            Denominator[i] = -a[0, i - 1];

        var c = stateSpace.C;
        var d = stateSpace.D;
        var num = new T[a.Rows + 1];
        for (var i = 0; i < a.Rows; i++)
            num[i + 1] = -(a[0, i] - c[i]) + (d[0] - T.One) * Denominator[i + 1];

        var zeroTol = T.CreateChecked(1e-8);
        var index = 0;
        for (var i = 1; i < num.Length; i++)
        {
            if (T.Abs(num[i]) > zeroTol)
            {
                index = i;
                break;
            }
        }

        if (T.Abs(d[0]) > zeroTol)
            index--;

        Numerator = new T[num.Length - index];
        Array.Copy(num, index, Numerator, 0, Numerator.Length);
        if (T.Abs(d[0]) > zeroTol)
            Numerator[0] = d[0];
    }

    /// <summary>Widens a float transfer function into <typeparamref name="T"/>.</summary>
    public static TransferFunction<T> From(TransferFunction tf)
    {
        ArgumentNullException.ThrowIfNull(tf);
        return new TransferFunction<T>(FilterGenericSupport.Widen<T>(tf.Numerator),
                                       FilterGenericSupport.Widen<T>(tf.Denominator));
    }

    /// <summary>Copies coefficients into a float transfer function (narrowing for <see cref="double"/>).</summary>
    public TransferFunction ToFloat()
        => new(FilterGenericSupport.Narrow(Numerator), FilterGenericSupport.Narrow(Denominator));

    /// <summary>Gets controllable canonical state-space form.</summary>
    public StateSpace<T> StateSpace
    {
        get
        {
            var M = Numerator.Length;
            var K = Denominator.Length;
            if (M > K)
                throw new ArgumentException("Numerator size must not exceed denominator size");

            var a0 = Denominator[0];
            if (K == 1)
            {
                return new StateSpace<T>
                {
                    A = new Matrix<T>(1),
                    B = new T[M],
                    C = new T[M],
                    D = [Numerator[0] / a0]
                };
            }

            var num = Numerator;
            if (M < K)
            {
                num = new T[K];
                Numerator.CopyTo(num, K - M);
            }

            var a = new Matrix<T>(K - 1);
            for (var i = 0; i < K - 1; i++)
                a[0, i] = -Denominator[i + 1] / a0;
            for (var i = 1; i < K - 1; i++)
                a[i, i - 1] = T.One;

            var b = new T[K - 1];
            b[0] = T.One;

            var c = new T[K - 1];
            for (var i = 0; i < K - 1; i++)
                c[i] = (num[i + 1] - num[0] * Denominator[i + 1] / a0) / a0;

            return new StateSpace<T>
            {
                A = a,
                B = b,
                C = c,
                D = [num[0] / a0]
            };
        }
    }

    /// <summary>Steady-state initial conditions for <see cref="ZiFilter{T}"/> (step response).</summary>
    public T[] Zi
    {
        get
        {
            var size = Math.Max(Numerator.Length, Denominator.Length);
            var a = FilterGenericSupport.PadTo(Denominator, size);
            var b = FilterGenericSupport.PadTo(Numerator, size);
            var a0 = a[0];
            for (var i = 0; i < a.Length; i++) a[i] /= a0;
            for (var i = 0; i < b.Length; i++) b[i] /= a0;

            var B = new T[size - 1];
            for (var i = 1; i < size; i++)
                B[i - 1] = b[i] - a[i] * b[0];

            var m = Matrix<T>.Eye(size - 1) - Matrix<T>.Companion(a).Transpose();
            var sum = T.Zero;
            for (var i = 0; i < size - 1; i++)
                sum += m[i, 0];

            var zi = new T[size];
            zi[0] = FilterGenericSupport.Sum(B) / sum;

            var asum = T.One;
            var csum = T.Zero;
            for (var i = 1; i < size - 1; i++)
            {
                asum += a[i];
                csum += b[i] - a[i] * b[0];
                zi[i] = asum * zi[0] - csum;
            }
            return zi;
        }
    }

    /// <summary>Impulse response of the given <paramref name="length"/> (FIR returns a copy of the numerator when shorter).</summary>
    public T[] ImpulseResponse(int length = 512)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (Denominator.Length == 1)
        {
            var copy = new T[Numerator.Length];
            Numerator.AsSpan().CopyTo(copy);
            return copy;
        }

        var b = Numerator;
        var a = Denominator;
        var response = new T[length];
        for (var n = 0; n < response.Length; n++)
        {
            if (n < b.Length) response[n] = b[n];
            for (var m = 1; m < a.Length; m++)
            {
                if (n >= m) response[n] -= a[m] * response[n - m];
            }
        }
        return response;
    }

    /// <summary>
    /// Frequency response via <see cref="Fft{T}"/>. <paramref name="length"/> must be a power of two.
    /// Returns the non-negative half (N/2+1 bins).
    /// </summary>
    public (T[] Real, T[] Imag) FrequencyResponse(int length = 512)
    {
        var ir = ImpulseResponse(length);
        var re = new T[length];
        var im = new T[length];
        ir.AsSpan(0, Math.Min(ir.Length, length)).CopyTo(re);
        new Fft<T>(length).Direct(re, im);
        int half = length / 2 + 1;
        var outRe = new T[half];
        var outIm = new T[half];
        re.AsSpan(0, half).CopyTo(outRe);
        im.AsSpan(0, half).CopyTo(outIm);
        return (outRe, outIm);
    }

    /// <summary>Group delay of length <paramref name="length"/>.</summary>
    public T[] GroupDelay(int length = 512)
    {
        var cc = FilterGenericSupport.CrossCorrelate(Numerator, Denominator);
        var cr = new T[cc.Length];
        for (var i = 0; i < cc.Length; i++)
            cr[i] = T.CreateChecked(i) * cc[i];
        cr = FilterGenericSupport.Reverse(cr);
        cc = FilterGenericSupport.Reverse(cc);

        var step = T.Pi / T.CreateChecked(length);
        var omega = T.Zero;
        var dn = T.CreateChecked(Denominator.Length - 1);
        var gd = new T[length];
        var tiny = T.CreateChecked(1e-30);

        for (var i = 0; i < gd.Length; i++)
        {
            var z = Complex<T>.FromPolarCoordinates(T.One, -omega);
            var num = VMath.EvaluatePolynomial(cr, z);
            var den = VMath.EvaluatePolynomial(cc, z);
            gd[i] = Complex<T>.Abs(den) < tiny ? T.Zero : (num / den).Real - dn;
            omega += step;
        }
        return gd;
    }

    /// <summary>Phase delay (integrated group delay).</summary>
    public T[] PhaseDelay(int length = 512)
    {
        var gd = GroupDelay(length);
        var pd = new T[gd.Length];
        var acc = T.Zero;
        for (var i = 0; i < pd.Length; i++)
        {
            acc += gd[i];
            pd[i] = acc / T.CreateChecked(i + 1);
        }
        return pd;
    }

    /// <summary>Normalizes frequency response at the given digital frequency (radians).</summary>
    public void NormalizeAt(T freq)
    {
        var w = Complex<T>.FromPolarCoordinates(T.One, freq);
        T den = Complex<T>.Abs(VMath.EvaluatePolynomial(Denominator, w));
        T num = Complex<T>.Abs(VMath.EvaluatePolynomial(Numerator, w));
        T tiny = T.CreateChecked(1e-30);
        if (num < tiny) return;
        T gain = den / num;
        for (var i = 0; i < Numerator.Length; i++)
            Numerator[i] *= gain;
    }

    /// <summary>Divides numerator and denominator by a[0].</summary>
    public void Normalize()
    {
        var a0 = Denominator[0];
        T tiny = T.CreateChecked(1e-30);
        if (T.Abs(a0) < tiny)
            throw new ArgumentException("The first denominator coefficient can not be zero!");

        for (var i = 0; i < Denominator.Length; i++)
            Denominator[i] /= a0;
        for (var i = 0; i < Numerator.Length; i++)
            Numerator[i] /= a0;
    }

    /// <summary>Converts zeros (or poles) to numerator (or denominator) coefficients.</summary>
    public static T[] ZpToTf(Complex<T>[] zp)
    {
        var poly = new[] { Complex<T>.One, -zp[0] };
        for (var k = 1; k < zp.Length; k++)
        {
            var poly1 = new[] { Complex<T>.One, -zp[k] };
            poly = VMath.MultiplyPolynomials(poly, poly1);
        }
        var tf = new T[poly.Length];
        for (var i = 0; i < poly.Length; i++)
            tf[i] = poly[i].Real;
        return tf;
    }

    /// <summary>Converts zeros/poles given as separate real/imaginary parts.</summary>
    public static T[] ZpToTf(T[] re, T[]? im = null)
    {
        im ??= new T[re.Length];
        var zp = new Complex<T>[re.Length];
        for (var i = 0; i < re.Length; i++)
            zp[i] = new Complex<T>(re[i], im[i]);
        return ZpToTf(zp);
    }

    /// <summary>Converts numerator (or denominator) to zeros (or poles).</summary>
    public static Complex<T>[]? TfToZp(T[] numeratorOrDenominator, int maxIterations = VMath.PolyRootsIterations)
    {
        if (numeratorOrDenominator.Length <= 1)
            return null;
        return VMath.PolynomialRoots(numeratorOrDenominator, maxIterations);
    }

    /// <summary>Series connection.</summary>
    public static TransferFunction<T> operator *(TransferFunction<T> tf1, TransferFunction<T> tf2)
        => new(FilterGenericSupport.Convolve(tf1.Numerator, tf2.Numerator),
               FilterGenericSupport.Convolve(tf1.Denominator, tf2.Denominator));

    /// <summary>Parallel connection.</summary>
    public static TransferFunction<T> operator +(TransferFunction<T> tf1, TransferFunction<T> tf2)
    {
        var num1 = FilterGenericSupport.Convolve(tf1.Numerator, tf2.Denominator);
        var num2 = FilterGenericSupport.Convolve(tf2.Numerator, tf1.Denominator);
        var num = num1.Length >= num2.Length ? (T[])num1.Clone() : (T[])num2.Clone();
        var add = num1.Length >= num2.Length ? num2 : num1;
        for (var i = 0; i < add.Length; i++)
            num[i] += add[i];
        return new TransferFunction<T>(num, FilterGenericSupport.Convolve(tf1.Denominator, tf2.Denominator));
    }

    /// <summary>Loads numerator and denominator from a two-line CSV stream.</summary>
    public static TransferFunction<T> FromCsv(Stream stream, char delimiter = ',')
    {
        using var reader = new StreamReader(stream);
        var num = ParseLine(reader.ReadLine(), delimiter);
        var den = ParseLine(reader.ReadLine(), delimiter);
        return new TransferFunction<T>(num, den);
    }

    /// <summary>Writes numerator and denominator as two CSV lines.</summary>
    public void ToCsv(Stream stream, char delimiter = ',')
    {
        using var writer = new StreamWriter(stream);
        writer.WriteLine(string.Join(delimiter, Numerator.Select(k => k.ToString(null, CultureInfo.InvariantCulture))));
        writer.WriteLine(string.Join(delimiter, Denominator.Select(k => k.ToString(null, CultureInfo.InvariantCulture))));
    }

    private static T[] ParseLine(string? content, char delimiter)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("CSV transfer function is missing a coefficient line.");
        return content.Split(delimiter)
                      .Select(s => T.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture))
                      .ToArray();
    }
}

/// <summary>Double-precision transfer function (same as <see cref="TransferFunction{T}"/> of <see cref="double"/>).</summary>
public sealed class TransferFunction64 : TransferFunction<double>
{
    /// <inheritdoc />
    public TransferFunction64(double[] numerator, double[]? denominator = null)
        : base(numerator, denominator)
    {
    }

    /// <inheritdoc />
    public TransferFunction64(Complex<double>[]? zeros, Complex<double>[]? poles, double gain = 1)
        : base(zeros, poles, gain)
    {
    }

    /// <inheritdoc />
    public TransferFunction64(StateSpace<double> stateSpace)
        : base(stateSpace)
    {
    }

    /// <summary>Widens a float transfer function.</summary>
    public TransferFunction64(TransferFunction tf)
        : base(From(tf).Numerator, From(tf).Denominator)
    {
    }
}
