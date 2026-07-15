namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

/// <summary>
/// <para>Represents Complex Fast Fourier Transform for 64-bit data:</para>
/// <list type="bullet">
///     <item>Direct FFT</item>
///     <item>Inverse FFT</item>
///     <item>Magnitude spectrum</item>
///     <item>Power spectrum</item>
/// </list>
/// </summary>
public class Fft64
{
    /// <summary>
    /// Gets FFT size.
    /// </summary>
    public int Size => _fftSize;
    private readonly int _fftSize;

    /// <summary>
    /// Precomputed cosines.
    /// </summary>
    private readonly double[] _cosTbl;

    /// <summary>
    /// Precomputed sines.
    /// </summary>
    private readonly double[] _sinTbl;

    /// <summary>
    /// Constructs FFT transformer with given <paramref name="fftSize"/>. FFT size must be a power of two.
    /// </summary>
    /// <param name="fftSize">FFT size</param>
    public Fft64(int fftSize = 512)
    {
        Guard.AgainstNotPowerOfTwo(fftSize, "FFT size");

        _fftSize = fftSize;

        // int.Log2：勿用 (int)Math.Log —— 部分 2 幂浮点 Log 会略小于整数，表短一格后写表越界。
        var tblSize = int.Log2(fftSize);

        _cosTbl = new double[tblSize];
        _sinTbl = new double[tblSize];

        for (int i = 1, pos = 0; i < _fftSize; i *= 2, pos++)
        {
            _cosTbl[pos] = Math.Cos(2 * Math.PI * i / _fftSize);
            _sinTbl[pos] = Math.Sin(2 * Math.PI * i / _fftSize);
        }
    }

    /// <summary>
    /// Does Fast Fourier Transform in-place.
    /// </summary>
    /// <param name="re">Array of real parts</param>
    /// <param name="im">Array of imaginary parts</param>
    public void Direct(double[] re, double[] im) => DirectInPlace(re, im);

    /// <summary>
    /// Does Fast Fourier Transform in-place over spans (real/imaginary parts).
    /// </summary>
    /// <param name="re">Span of real parts</param>
    /// <param name="im">Span of imaginary parts</param>
    public void Direct(Span<double> re, Span<double> im) => DirectInPlace(re, im);

    /// <summary>
    /// Scalar in-place decimation-in-frequency FFT (forward), shared by the array and span overloads.
    /// </summary>
    private void DirectInPlace(Span<double> re, Span<double> im)
    {
        var L = _fftSize;
        var M = _fftSize >> 1;
        var S = _fftSize - 1;
        var ti = 0;
        while (L >= 2)
        {
            var l = L >> 1;
            var u1 = 1.0;
            var u2 = 0.0;
            var c = _cosTbl[ti];
            var s = -_sinTbl[ti];
            ti++;
            for (var j = 0; j < l; j++)
            {
                for (var i = j; i < _fftSize; i += L)
                {
                    var p = i + l;
                    var t1 = re[i] + re[p];
                    var t2 = im[i] + im[p];
                    var t3 = re[i] - re[p];
                    var t4 = im[i] - im[p];
                    re[p] = t3 * u1 - t4 * u2;
                    im[p] = t4 * u1 + t3 * u2;
                    re[i] = t1;
                    im[i] = t2;
                }
                var u3 = u1 * c - u2 * s;
                u2 = u2 * c + u1 * s;
                u1 = u3;
            }
            L >>= 1;
        }
        for (int i = 0, j = 0; i < S; i++)
        {
            if (i > j)
            {
                var t1 = re[j];
                var t2 = im[j];
                re[j] = re[i];
                im[j] = im[i];
                re[i] = t1;
                im[i] = t2;
            }
            var k = M;
            while (j >= k)
            {
                j -= k;
                k >>= 1;
            }
            j += k;
        }
    }

    /// <summary>
    /// Does Inverse Fast Fourier Transform in-place.
    /// </summary>
    /// <param name="re">Array of real parts</param>
    /// <param name="im">Array of imaginary parts</param>
    public void Inverse(double[] re, double[] im) => InverseInPlace(re, im);

    /// <summary>
    /// Does Inverse Fast Fourier Transform in-place over spans (real/imaginary parts).
    /// </summary>
    /// <param name="re">Span of real parts</param>
    /// <param name="im">Span of imaginary parts</param>
    public void Inverse(Span<double> re, Span<double> im) => InverseInPlace(re, im);

    /// <summary>
    /// Scalar in-place decimation-in-frequency FFT (inverse), shared by the array and span overloads.
    /// </summary>
    private void InverseInPlace(Span<double> re, Span<double> im)
    {
        var L = _fftSize;
        var M = _fftSize >> 1;
        var S = _fftSize - 1;
        var ti = 0;
        while (L >= 2)
        {
            var l = L >> 1;
            var u1 = 1.0;
            var u2 = 0.0;
            var c = _cosTbl[ti];
            var s = _sinTbl[ti];
            ti++;
            for (var j = 0; j < l; j++)
            {
                for (var i = j; i < _fftSize; i += L)
                {
                    var p = i + l;
                    var t1 = re[i] + re[p];
                    var t2 = im[i] + im[p];
                    var t3 = re[i] - re[p];
                    var t4 = im[i] - im[p];
                    re[p] = t3 * u1 - t4 * u2;
                    im[p] = t4 * u1 + t3 * u2;
                    re[i] = t1;
                    im[i] = t2;
                }
                var u3 = u1 * c - u2 * s;
                u2 = u2 * c + u1 * s;
                u1 = u3;
            }
            L >>= 1;
        }
        for (int i = 0, j = 0; i < S; i++)
        {
            if (i > j)
            {
                var t1 = re[j];
                var t2 = im[j];
                re[j] = re[i];
                im[j] = im[i];
                re[i] = t1;
                im[i] = t2;
            }
            var k = M;
            while (j >= k)
            {
                j -= k;
                k >>= 1;
            }
            j += k;
        }
    }

    /// <summary>
    /// Does normalized Inverse Fast Fourier Transform in-place.
    /// </summary>
    /// <param name="re">Array of real parts</param>
    /// <param name="im">Array of imaginary parts</param>
    public void InverseNorm(double[] re, double[] im) => InverseNorm(re.AsSpan(), im.AsSpan());

    /// <summary>
    /// Does normalized Inverse Fast Fourier Transform in-place over spans.
    /// </summary>
    /// <param name="re">Span of real parts</param>
    /// <param name="im">Span of imaginary parts</param>
    public void InverseNorm(Span<double> re, Span<double> im)
    {
        InverseInPlace(re, im);

        for (int i = 0; i < _fftSize; i++)
        {
            re[i] /= _fftSize;
            im[i] /= _fftSize;
        }
    }

    #region ComputingContext-aware overloads

    /// <summary>
    /// Does Fast Fourier Transform in-place, selecting a scalar / SIMD / parallel kernel according to <paramref name="context"/>.
    /// </summary>
    /// <param name="re">Array of real parts</param>
    /// <param name="im">Array of imaginary parts</param>
    /// <param name="context">Optional execution policy.</param>
    public void Direct(double[] re, double[] im, ComputingContext? context)
    {
        var mode = ComputingContext.Resolve(context).ResolveCpuMode(_fftSize);
        if (!FftButterflyFp64.WillAccelerate(mode, _fftSize)) { Direct(re, im); return; }
        FftButterflyFp64.Transform(re, im, _fftSize, inverse: false, mode, context);
    }

    /// <summary>
    /// Does Inverse Fast Fourier Transform in-place, selecting a scalar / SIMD / parallel kernel according to <paramref name="context"/>.
    /// </summary>
    /// <param name="re">Array of real parts</param>
    /// <param name="im">Array of imaginary parts</param>
    /// <param name="context">Optional execution policy.</param>
    public void Inverse(double[] re, double[] im, ComputingContext? context)
    {
        var mode = ComputingContext.Resolve(context).ResolveCpuMode(_fftSize);
        if (!FftButterflyFp64.WillAccelerate(mode, _fftSize)) { Inverse(re, im); return; }
        FftButterflyFp64.Transform(re, im, _fftSize, inverse: true, mode, context);
    }

    /// <summary>
    /// Does normalized Inverse Fast Fourier Transform in-place, selecting a scalar / SIMD / parallel kernel according to <paramref name="context"/>.
    /// </summary>
    /// <param name="re">Array of real parts</param>
    /// <param name="im">Array of imaginary parts</param>
    /// <param name="context">Optional execution policy.</param>
    public void InverseNorm(double[] re, double[] im, ComputingContext? context)
    {
        Inverse(re, im, context);

        for (int i = 0; i < _fftSize; i++)
        {
            re[i] /= _fftSize;
            im[i] /= _fftSize;
        }
    }

    #endregion

    /// <summary>
    /// Does Fast Fourier Transform: 
    /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
    /// </summary>
    /// <param name="inRe">Input data (real parts)</param>
    /// <param name="inIm">Input data (imaginary parts)</param>
    /// <param name="outRe">Output data (real parts)</param>
    /// <param name="outIm">Output data (imaginary parts)</param>
    public void Direct(double[] inRe, double[] inIm, double[] outRe, double[] outIm)
    {
        inRe.FastCopyTo(outRe, inRe.Length);
        inIm.FastCopyTo(outIm, inIm.Length);

        Direct(outRe, outIm);
    }

    /// <summary>
    /// Does Fast Fourier Transform:
    /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
    /// </summary>
    /// <param name="inRe">Input data (real parts)</param>
    /// <param name="inIm">Input data (imaginary parts)</param>
    /// <param name="outRe">Output data (real parts)</param>
    /// <param name="outIm">Output data (imaginary parts)</param>
    public void Direct(ReadOnlySpan<double> inRe, ReadOnlySpan<double> inIm, Span<double> outRe, Span<double> outIm)
    {
        inRe.CopyTo(outRe);
        inIm.CopyTo(outIm);

        DirectInPlace(outRe, outIm);
    }

    /// <summary>
    /// Does normalized Fast Fourier Transform: 
    /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
    /// </summary>
    /// <param name="inRe">Input data (real parts)</param>
    /// <param name="inIm">Input data (imaginary parts)</param>
    /// <param name="outRe">Output data (real parts)</param>
    /// <param name="outIm">Output data (imaginary parts)</param>
    public void DirectNorm(double[] inRe, double[] inIm, double[] outRe, double[] outIm)
    {
        Direct(inRe, inIm, outRe, outIm);
    }

    /// <summary>
    /// Does normalized Fast Fourier Transform:
    /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
    /// </summary>
    /// <param name="inRe">Input data (real parts)</param>
    /// <param name="inIm">Input data (imaginary parts)</param>
    /// <param name="outRe">Output data (real parts)</param>
    /// <param name="outIm">Output data (imaginary parts)</param>
    public void DirectNorm(ReadOnlySpan<double> inRe, ReadOnlySpan<double> inIm, Span<double> outRe, Span<double> outIm)
    {
        Direct(inRe, inIm, outRe, outIm);
    }

    /// <summary>
    /// Does Inverse Fast Fourier Transform: 
    /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
    /// </summary>
    /// <param name="inRe">Input data (real parts)</param>
    /// <param name="inIm">Input data (imaginary parts)</param>
    /// <param name="outRe">Output data (real parts)</param>
    /// <param name="outIm">Output data (imaginary parts)</param>
    public void Inverse(double[] inRe, double[] inIm, double[] outRe, double[] outIm)
    {
        inRe.FastCopyTo(outRe, inRe.Length);
        inIm.FastCopyTo(outIm, inIm.Length);

        Inverse(outRe, outIm);
    }

    /// <summary>
    /// Does Inverse Fast Fourier Transform:
    /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
    /// </summary>
    /// <param name="inRe">Input data (real parts)</param>
    /// <param name="inIm">Input data (imaginary parts)</param>
    /// <param name="outRe">Output data (real parts)</param>
    /// <param name="outIm">Output data (imaginary parts)</param>
    public void Inverse(ReadOnlySpan<double> inRe, ReadOnlySpan<double> inIm, Span<double> outRe, Span<double> outIm)
    {
        inRe.CopyTo(outRe);
        inIm.CopyTo(outIm);

        InverseInPlace(outRe, outIm);
    }

    /// <summary>
    /// Does normalized Inverse Fast Fourier Transform: 
    /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
    /// </summary>
    /// <param name="inRe">Input data (real parts)</param>
    /// <param name="inIm">Input data (imaginary parts)</param>
    /// <param name="outRe">Output data (real parts)</param>
    /// <param name="outIm">Output data (imaginary parts)</param>
    public void InverseNorm(double[] inRe, double[] inIm, double[] outRe, double[] outIm)
    {
        inRe.FastCopyTo(outRe, inRe.Length);
        inIm.FastCopyTo(outIm, inIm.Length);

        InverseNorm(outRe, outIm);
    }

    /// <summary>
    /// Does normalized Inverse Fast Fourier Transform:
    /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
    /// </summary>
    /// <param name="inRe">Input data (real parts)</param>
    /// <param name="inIm">Input data (imaginary parts)</param>
    /// <param name="outRe">Output data (real parts)</param>
    /// <param name="outIm">Output data (imaginary parts)</param>
    public void InverseNorm(ReadOnlySpan<double> inRe, ReadOnlySpan<double> inIm, Span<double> outRe, Span<double> outIm)
    {
        inRe.CopyTo(outRe);
        inIm.CopyTo(outIm);

        InverseNorm(outRe, outIm);
    }

}
