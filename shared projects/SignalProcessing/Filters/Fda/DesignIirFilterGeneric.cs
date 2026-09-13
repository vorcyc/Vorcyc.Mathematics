using System.Numerics;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

public static partial class DesignFilter
{
    /// <summary>Designs an IIR notch filter in <typeparamref name="T"/>.</summary>
    public static TransferFunction<T> IirNotch<T>(T frequency, T? q = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardNormFreq(frequency, "Center frequency");
        var qq = q ?? T.CreateChecked(20);
        var w0 = T.CreateChecked(2) * frequency * T.Pi;
        var bw = w0 / qq;
        var gb = T.One / T.Sqrt(T.CreateChecked(2));
        var beta = T.Sqrt(T.One - gb * gb) / gb * T.Tan(bw / T.CreateChecked(2));
        var gain = T.One / (T.One + beta);
        var num = new[] { gain, T.CreateChecked(-2) * T.Cos(w0) * gain, gain };
        var den = new[] { T.One, T.CreateChecked(-2) * T.Cos(w0) * gain, T.CreateChecked(2) * gain - T.One };
        return new TransferFunction<T>(num, den);
    }

    /// <summary>Designs an IIR peak filter in <typeparamref name="T"/>.</summary>
    public static TransferFunction<T> IirPeak<T>(T frequency, T? q = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardNormFreq(frequency, "Center frequency");
        var qq = q ?? T.CreateChecked(20);
        var w0 = T.CreateChecked(2) * frequency * T.Pi;
        var bw = w0 / qq;
        var gb = T.One / T.Sqrt(T.CreateChecked(2));
        var beta = gb / T.Sqrt(T.One - gb * gb) * T.Tan(bw / T.CreateChecked(2));
        var gain = T.One / (T.One + beta);
        var num = new[] { T.One - gain, T.Zero, gain - T.One };
        var den = new[] { T.One, T.CreateChecked(-2) * T.Cos(w0) * gain, T.CreateChecked(2) * gain - T.One };
        return new TransferFunction<T>(num, den);
    }

    /// <summary>Designs an IIR comb-notch filter in <typeparamref name="T"/>.</summary>
    public static TransferFunction<T> IirCombNotch<T>(T frequency, T? q = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardNormFreq(frequency, "Center frequency");
        var qq = q ?? T.CreateChecked(20);
        var w0 = T.CreateChecked(2) * frequency * T.Pi;
        var bw = w0 / qq;
        var gb = T.One / T.Sqrt(T.CreateChecked(2));
        var n = (int)double.CreateChecked(T.One / frequency);
        var beta = T.Sqrt((T.One - gb * gb) / (gb * gb)) * T.Tan(T.CreateChecked(n) * bw / T.CreateChecked(4));
        var num = new T[n + 1];
        var den = new T[n + 1];
        num[0] = T.One / (T.One + beta);
        num[^1] = -T.One / (T.One + beta);
        den[0] = T.One;
        den[^1] = -(T.One - beta) / (T.One + beta);
        return new TransferFunction<T>(num, den);
    }

    /// <summary>Designs an IIR comb-peak filter in <typeparamref name="T"/>.</summary>
    public static TransferFunction<T> IirCombPeak<T>(T frequency, T? q = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardNormFreq(frequency, "Center frequency");
        var qq = q ?? T.CreateChecked(20);
        var w0 = T.CreateChecked(2) * frequency * T.Pi;
        var bw = w0 / qq;
        var gb = T.One / T.Sqrt(T.CreateChecked(2));
        var n = (int)double.CreateChecked(T.One / frequency);
        var beta = T.Sqrt(gb * gb / (T.One - gb * gb)) * T.Tan(T.CreateChecked(n) * bw / T.CreateChecked(4));
        var num = new T[n + 1];
        var den = new T[n + 1];
        num[0] = beta / (T.One + beta);
        num[^1] = -beta / (T.One + beta);
        den[0] = T.One;
        den[^1] = (T.One - beta) / (T.One + beta);
        return new TransferFunction<T>(num, den);
    }

    /// <summary>Designs a lowpass pole filter in <typeparamref name="T"/>.</summary>
    public static TransferFunction<T> IirLpTf<T>(T frequency, Complex<T>[] poles, Complex<T>[]? zeros = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardNormFreq(frequency, "Cutoff frequency");
        var warped = T.Tan(T.Pi * frequency);
        FilterGenericSupport.SplitReIm(Scale(poles, warped), out var pre, out var pim);
        VMath.BilinearTransform(pre, pim);

        T[] zre, zim;
        if (zeros != null)
        {
            FilterGenericSupport.SplitReIm(Scale(zeros, warped), out zre, out zim);
            VMath.BilinearTransform(zre, zim);
        }
        else
        {
            zre = Repeat(T.CreateChecked(-1), poles.Length);
            zim = new T[poles.Length];
        }

        var tf = new TransferFunction<T>(TransferFunction<T>.ZpToTf(zre, zim), TransferFunction<T>.ZpToTf(pre, pim));
        tf.NormalizeAt(T.Zero);
        return tf;
    }

    /// <summary>Designs a highpass pole filter in <typeparamref name="T"/>.</summary>
    public static TransferFunction<T> IirHpTf<T>(T frequency, Complex<T>[] poles, Complex<T>[]? zeros = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardNormFreq(frequency, "Cutoff frequency");
        var warped = T.Tan(T.Pi * frequency);
        FilterGenericSupport.SplitReIm(InvertScale(poles, warped), out var pre, out var pim);
        VMath.BilinearTransform(pre, pim);

        T[] zre, zim;
        if (zeros != null)
        {
            FilterGenericSupport.SplitReIm(InvertScale(zeros, warped), out zre, out zim);
            VMath.BilinearTransform(zre, zim);
        }
        else
        {
            zre = Repeat(T.One, poles.Length);
            zim = new T[poles.Length];
        }

        var tf = new TransferFunction<T>(TransferFunction<T>.ZpToTf(zre, zim), TransferFunction<T>.ZpToTf(pre, pim));
        tf.NormalizeAt(T.Pi);
        return tf;
    }

    /// <summary>Designs a bandpass pole filter in <typeparamref name="T"/>.</summary>
    public static TransferFunction<T> IirBpTf<T>(T frequencyLow, T frequencyHigh, Complex<T>[] poles, Complex<T>[]? zeros = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardBand(frequencyLow, frequencyHigh);
        SpectralTransformBp(poles, frequencyLow, frequencyHigh, out var pre, out var pim);
        VMath.BilinearTransform(pre, pim);

        T[] zre, zim;
        if (zeros != null)
        {
            SpectralTransformBp(zeros, frequencyLow, frequencyHigh, out zre, out zim);
            VMath.BilinearTransform(zre, zim);
        }
        else
        {
            zre = Repeat(T.CreateChecked(-1), poles.Length).Concat(Repeat(T.One, poles.Length)).ToArray();
            zim = new T[poles.Length * 2];
        }

        var center = T.CreateChecked(2) * T.Pi * (frequencyLow + frequencyHigh) / T.CreateChecked(2);
        var tf = new TransferFunction<T>(TransferFunction<T>.ZpToTf(zre, zim), TransferFunction<T>.ZpToTf(pre, pim));
        tf.NormalizeAt(center);
        return tf;
    }

    /// <summary>Designs a bandstop pole filter in <typeparamref name="T"/>.</summary>
    public static TransferFunction<T> IirBsTf<T>(T frequencyLow, T frequencyHigh, Complex<T>[] poles, Complex<T>[]? zeros = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        FilterGenericSupport.GuardBand(frequencyLow, frequencyHigh);
        var f1 = T.Tan(T.Pi * frequencyLow);
        var f2 = T.Tan(T.Pi * frequencyHigh);
        var f0 = T.Sqrt(f1 * f2);
        var bw = f2 - f1;
        var centerFreq = T.CreateChecked(2) * T.Atan(f0);

        SpectralTransformBs(poles, f0, bw, out var pre, out var pim);
        VMath.BilinearTransform(pre, pim);

        T[] zre, zim;
        if (zeros != null)
        {
            SpectralTransformBs(zeros, f0, bw, out zre, out zim);
            VMath.BilinearTransform(zre, zim);
        }
        else
        {
            zre = new T[poles.Length * 2];
            zim = new T[poles.Length * 2];
            var c = T.Cos(centerFreq);
            var s = T.Sin(centerFreq);
            for (var k = 0; k < poles.Length; k++)
            {
                zre[k] = c;
                zim[k] = s;
                zre[poles.Length + k] = c;
                zim[poles.Length + k] = -s;
            }
        }

        var tf = new TransferFunction<T>(TransferFunction<T>.ZpToTf(zre, zim), TransferFunction<T>.ZpToTf(pre, pim));
        tf.NormalizeAt(T.Zero);
        return tf;
    }

    /// <summary>Converts SOS sections to a single transfer function.</summary>
    public static TransferFunction<T> SosToTf<T>(TransferFunction<T>[] sos)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
        => sos.Aggregate((tf, s) => tf * s);

    /// <summary>Splits a transfer function into second-order sections.</summary>
    public static TransferFunction<T>[] TfToSos<T>(TransferFunction<T> tf)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var zeros = (tf.Zeros ?? []).ToList();
        var poles = (tf.Poles ?? []).ToList();
        if (zeros.Count != poles.Count)
        {
            if (zeros.Count > poles.Count) poles.AddRange(new Complex<T>[zeros.Count - poles.Count]);
            if (zeros.Count < poles.Count) zeros.AddRange(new Complex<T>[poles.Count - zeros.Count]);
        }

        var sosCount = (poles.Count + 1) / 2;
        if (poles.Count % 2 == 1)
        {
            zeros.Add(Complex<T>.Zero);
            poles.Add(Complex<T>.Zero);
        }

        RemoveConjugated(zeros);
        RemoveConjugated(poles);

        var gains = new T[sosCount];
        gains[0] = tf.Gain;
        for (var i = 1; i < gains.Length; i++) gains[i] = T.One;

        var sos = new TransferFunction<T>[sosCount];
        var tiny = T.CreateChecked(1e-10);

        bool IsReal(Complex<T> c) => T.Abs(c.Imaginary) < tiny;
        bool IsComplex(Complex<T> c) => T.Abs(c.Imaginary) > tiny;
        bool Any(Complex<T> _) => true;

        for (var i = sosCount - 1; i >= 0; i--)
        {
            Complex<T> z1, z2, p1, p2;
            var pos = ClosestToUnitCircle(poles, Any);
            p1 = poles[pos];
            poles.RemoveAt(pos);

            if (IsReal(p1) && poles.All(IsComplex))
            {
                pos = ClosestToComplexValue(zeros, p1, IsReal);
                z1 = zeros[pos];
                zeros.RemoveAt(pos);
                p2 = Complex<T>.Zero;
                z2 = Complex<T>.Zero;
            }
            else
            {
                pos = IsComplex(p1) && zeros.Count(IsReal) == 1
                    ? ClosestToComplexValue(zeros, p1, IsComplex)
                    : ClosestToComplexValue(zeros, p1, Any);
                z1 = zeros[pos];
                zeros.RemoveAt(pos);

                if (IsComplex(p1))
                {
                    p2 = Complex<T>.Conjugate(p1);
                    if (IsComplex(z1))
                    {
                        z2 = Complex<T>.Conjugate(z1);
                    }
                    else
                    {
                        pos = ClosestToComplexValue(zeros, p1, IsReal);
                        z2 = zeros[pos];
                        zeros.RemoveAt(pos);
                    }
                }
                else if (IsComplex(z1))
                {
                    z2 = Complex<T>.Conjugate(z1);
                    pos = ClosestToComplexValue(poles, z1, IsReal);
                    p2 = poles[pos];
                    poles.RemoveAt(pos);
                }
                else
                {
                    pos = ClosestToUnitCircle(poles, IsReal);
                    p2 = poles[pos];
                    poles.RemoveAt(pos);
                    pos = ClosestToComplexValue(zeros, p2, IsReal);
                    z2 = zeros[pos];
                    zeros.RemoveAt(pos);
                }
            }

            sos[i] = new TransferFunction<T>([z1, z2], [p1, p2], gains[i]);
        }

        return sos;
    }

    private static Complex<T>[] Scale<T>(Complex<T>[] z, T warped)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var r = new Complex<T>[z.Length];
        for (var i = 0; i < z.Length; i++)
            r[i] = warped * z[i];
        return r;
    }

    private static Complex<T>[] InvertScale<T>(Complex<T>[] z, T warped)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var r = new Complex<T>[z.Length];
        for (var i = 0; i < z.Length; i++)
            r[i] = warped / z[i];
        return r;
    }

    private static void SpectralTransformBp<T>(Complex<T>[] z, T frequencyLow, T frequencyHigh, out T[] re, out T[] im)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var warped1 = T.Tan(T.Pi * frequencyLow);
        var warped2 = T.Tan(T.Pi * frequencyHigh);
        var f0 = T.Sqrt(warped1 * warped2);
        var bw = warped2 - warped1;
        re = new T[z.Length * 2];
        im = new T[z.Length * 2];
        for (var k = 0; k < z.Length; k++)
        {
            var alpha = bw / T.CreateChecked(2) * z[k];
            var beta = Complex<T>.Sqrt(T.One - Complex<T>.Pow(f0 / alpha, T.CreateChecked(2)));
            var p1 = alpha * (T.One + beta);
            re[k] = p1.Real;
            im[k] = p1.Imaginary;
            var p2 = alpha * (T.One - beta);
            re[z.Length + k] = p2.Real;
            im[z.Length + k] = p2.Imaginary;
        }
    }

    private static void SpectralTransformBs<T>(Complex<T>[] z, T f0, T bw, out T[] re, out T[] im)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        re = new T[z.Length * 2];
        im = new T[z.Length * 2];
        for (var k = 0; k < z.Length; k++)
        {
            var alpha = bw / T.CreateChecked(2) / z[k];
            var beta = Complex<T>.Sqrt(T.One - Complex<T>.Pow(f0 / alpha, T.CreateChecked(2)));
            var p1 = alpha * (T.One + beta);
            re[k] = p1.Real;
            im[k] = p1.Imaginary;
            var p2 = alpha * (T.One - beta);
            re[z.Length + k] = p2.Real;
            im[z.Length + k] = p2.Imaginary;
        }
    }

    private static T[] Repeat<T>(T value, int count)
    {
        var a = new T[count];
        Array.Fill(a, value);
        return a;
    }

    private static int ClosestToComplexValue<T>(List<Complex<T>> arr, Complex<T> value, Func<Complex<T>, bool> condition)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var pos = 0;
        var minDistance = T.MaxValue;
        for (var i = 0; i < arr.Count; i++)
        {
            if (!condition(arr[i])) continue;
            var distance = Complex<T>.Abs(arr[i] - value);
            if (distance < minDistance)
            {
                minDistance = distance;
                pos = i;
            }
        }
        return pos;
    }

    private static int ClosestToUnitCircle<T>(List<Complex<T>> arr, Func<Complex<T>, bool> condition)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var pos = 0;
        var minDistance = T.MaxValue;
        for (var i = 0; i < arr.Count; i++)
        {
            if (!condition(arr[i])) continue;
            var distance = T.Abs(Complex<T>.Abs(arr[i]) - T.One);
            if (distance < minDistance)
            {
                minDistance = distance;
                pos = i;
            }
        }
        return pos;
    }

    private static void RemoveConjugated<T>(List<Complex<T>> c)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var tiny = T.CreateChecked(1e-10);
        for (var i = 0; i < c.Count; i++)
        {
            if (T.Abs(c[i].Imaginary) < tiny) continue;
            var j = i + 1;
            for (; j < c.Count; j++)
            {
                if (T.Abs(c[i].Real - c[j].Real) < tiny &&
                    T.Abs(c[i].Imaginary + c[j].Imaginary) < tiny)
                    break;
            }
            if (j == c.Count)
                throw new ArgumentException($"Complex array does not contain conjugated pair for {c[i]}");
            c.RemoveAt(j);
        }
    }
}
