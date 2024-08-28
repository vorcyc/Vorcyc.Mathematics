using System.Buffers;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

/// <summary>
/// FFT Allows real input and output.
/// </summary>
public class RealOnlyFFT_Fp32
{


    /// <summary>
    /// Gets FFT size.
    /// </summary>
    public int Size => _fftSize * 2;

    /// <summary>
    /// Half of FFT size (for calculations).
    /// </summary>
    private readonly int _fftSize;

    /// <summary>
    /// Precomputed cosines.
    /// </summary>
    private readonly float[] _cosTbl;

    /// <summary>
    /// Precomputed sines.
    /// </summary>
    private readonly float[] _sinTbl;

    /// <summary>
    /// Precomputed coefficients.
    /// </summary>
    private readonly float[] _ar, _br, _ai, _bi;

    // Internal buffers

    private readonly float[] _re;
    private readonly float[] _im;
    //private readonly float[] _realSpectrum;
    //private readonly float[] _imagSpectrum;

    //private readonly ComplexFp32[] _spectrum;

    /// <summary>
    /// Constructs FFT transformer with given <paramref name="size"/>. FFT size must be a power of two.
    /// </summary>
    /// <param name="size">FFT size</param>
    public RealOnlyFFT_Fp32(int size)
    {
        Guard.AgainstNotPowerOfTwo(size, "Size of FFT");

        _fftSize = size / 2;

        _re = new float[_fftSize];
        _im = new float[_fftSize];

        //_realSpectrum = new float[_fftSize + 1];
        //_imagSpectrum = new float[_fftSize + 1];
        //_spectrum = new ComplexFp32[_fftSize + 1];

        // precompute coefficients:

        var tblSize = (int)MathF.Log(_fftSize, 2);

        _cosTbl = new float[tblSize];
        _sinTbl = new float[tblSize];

        for (int i = 1, pos = 0; i < _fftSize; i *= 2, pos++)
        {
            _cosTbl[pos] = MathF.Cos(2 * ConstantsFp32.PI * i / _fftSize);
            _sinTbl[pos] = MathF.Sin(2 * ConstantsFp32.PI * i / _fftSize);
        }

        _ar = new float[_fftSize];
        _br = new float[_fftSize];
        _ai = new float[_fftSize];
        _bi = new float[_fftSize];

        var f = ConstantsFp32.PI / _fftSize;

        for (var i = 0; i < _fftSize; i++)
        {
            _ar[i] = (0.5f * (1 - MathF.Sin(f * i)));
            _ai[i] = (-0.5f * MathF.Cos(f * i));
            _br[i] = (0.5f * (1 + MathF.Sin(f * i)));
            _bi[i] = (0.5f * MathF.Cos(f * i));
        }
    }


    #region Forward


    /// <summary>
    /// Performs a Fast Fourier Transform (FFT) on the input data.
    /// Converts real input data to complex output data.
    /// </summary>
    /// <param name="input">The real input data.</param>
    /// <param name="output">The complex output data to be filled.</param>
    public void Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output)
    {
        // do half-size complex FFT:

        for (int i = 0, k = 0; i < _fftSize; i++)
        {
            _re[i] = input[k++];
            _im[i] = input[k++];
        }

        var L = _fftSize;
        var M = _fftSize >> 1;
        var S = _fftSize - 1;
        var ti = 0;
        while (L >= 2)
        {
            var l = L >> 1;
            var u1 = 1.0f;
            var u2 = 0.0f;
            var c = _cosTbl[ti];
            var s = -_sinTbl[ti];
            ti++;
            for (var j = 0; j < l; j++)
            {
                for (var i = j; i < _fftSize; i += L)
                {
                    var p = i + l;
                    var t1 = _re[i] + _re[p];
                    var t2 = _im[i] + _im[p];
                    var t3 = _re[i] - _re[p];
                    var t4 = _im[i] - _im[p];
                    _re[p] = t3 * u1 - t4 * u2;
                    _im[p] = t4 * u1 + t3 * u2;
                    _re[i] = t1;
                    _im[i] = t2;
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
                var t1 = _re[j];
                var t2 = _im[j];
                _re[j] = _re[i];
                _im[j] = _im[i];
                _re[i] = t1;
                _im[i] = t2;
            }
            var k = M;
            while (j >= k)
            {
                j -= k;
                k >>= 1;
            }
            j += k;
        }

        // do the last step:

        output[0].Real = _re[0] * _ar[0] - _im[0] * _ai[0] + _re[0] * _br[0] + _im[0] * _bi[0];
        output[0].Imaginary = _im[0] * _ar[0] + _re[0] * _ai[0] + _re[0] * _bi[0] - _im[0] * _br[0];

        for (var k = 1; k < _fftSize; k++)
        {
            output[k].Real = _re[k] * _ar[k] - _im[k] * _ai[k] + _re[_fftSize - k] * _br[k] + _im[_fftSize - k] * _bi[k];
            output[k].Imaginary = _im[k] * _ar[k] + _re[k] * _ai[k] + _re[_fftSize - k] * _bi[k] - _im[_fftSize - k] * _br[k];
        }

        output[_fftSize].Real = _re[0] - _im[0];
        output[_fftSize].Imaginary = 0;
    }


    //public void Forward(float[] input, ComplexFp32[] output)
    //    => Forward(input.AsSpan(), output.AsSpan());

    /// <summary>
    /// Performs a Fast Fourier Transform (FFT) on the input data and returns the result.
    /// </summary>
    /// <param name="input">The real input data.</param>
    /// <returns>An array of complex numbers representing the FFT result.</returns>
    public ComplexFp32[] Forward(ReadOnlySpan<float> input)
    {
        var result = new ComplexFp32[_fftSize + 1];
        //这里不能使用租赁模式
        Forward(input, result);
        return result;
    }


    #endregion


    #region Inverse

    /// <summary>
    /// Performs an Inverse Fast Fourier Transform (IFFT) on the input data.
    /// Converts complex input data to real output data.
    /// </summary>
    /// <param name="input">The complex input data.</param>
    /// <param name="output">The real output data to be filled.</param>
    public void Inverse(ReadOnlySpan<ComplexFp32> input, Span<float> output)
    {
        // do the first step:

        for (var k = 0; k < _fftSize; k++)
        {
            _re[k] = input[k].Real * _ar[k] + input[k].Imaginary * _ai[k] + input[_fftSize - k].Real * _br[k] - input[_fftSize - k].Imaginary * _bi[k];
            _im[k] = input[k].Imaginary * _ar[k] - input[k].Real * _ai[k] - input[_fftSize - k].Real * _bi[k] - input[_fftSize - k].Imaginary * _br[k];
        }

        // do half-size complex FFT:

        var L = _fftSize;
        var M = _fftSize >> 1;
        var S = _fftSize - 1;
        var ti = 0;
        while (L >= 2)
        {
            var l = L >> 1;
            var u1 = 1.0f;
            var u2 = 0.0f;
            var c = _cosTbl[ti];
            var s = _sinTbl[ti];
            ti++;
            for (var j = 0; j < l; j++)
            {
                for (var i = j; i < _fftSize; i += L)
                {
                    var p = i + l;
                    var t1 = _re[i] + _re[p];
                    var t2 = _im[i] + _im[p];
                    var t3 = _re[i] - _re[p];
                    var t4 = _im[i] - _im[p];
                    _re[p] = t3 * u1 - t4 * u2;
                    _im[p] = t4 * u1 + t3 * u2;
                    _re[i] = t1;
                    _im[i] = t2;
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
                var t1 = _re[j];
                var t2 = _im[j];
                _re[j] = _re[i];
                _im[j] = _im[i];
                _re[i] = t1;
                _im[i] = t2;
            }
            var k = M;
            while (j >= k)
            {
                j -= k;
                k >>= 1;
            }
            j += k;
        }

        // fill output:

        for (int i = 0, k = 0; i < _fftSize; i++)
        {
            output[k++] = _re[i] * 2;
            output[k++] = _im[i] * 2;
        }
    }

    /// <summary>
    /// Performs an Inverse Fast Fourier Transform (IFFT) on the input data and returns the result.
    /// </summary>
    /// <param name="input">The complex input data.</param>
    /// <returns>An array of real numbers representing the IFFT result.</returns>
    public float[] Inverse(ReadOnlySpan<ComplexFp32> input)
    {
        var result = new float[_fftSize << 1];
        Inverse(input, result);
        return result;
    }

    #endregion


    #region InverseNorm


    /// <summary>
    /// Performs a normalized Inverse Fast Fourier Transform (IFFT) on the input data.
    /// Converts complex input data to real output data.
    /// </summary>
    /// <param name="input">The complex input data.</param>
    /// <param name="output">The real output data to be filled.</param>

    public void InverseNorm(ReadOnlySpan<ComplexFp32> input, Span<float> output)
    {
        // do the first step:

        for (var k = 0; k < _fftSize; k++)
        {
            _re[k] = input[k].Real * _ar[k] + input[k].Imaginary * _ai[k] + input[_fftSize - k].Real * _br[k] - input[_fftSize - k].Imaginary * _bi[k];
            _im[k] = input[k].Imaginary * _ar[k] - input[k].Real * _ai[k] - input[_fftSize - k].Real * _bi[k] - input[_fftSize - k].Imaginary * _br[k];
        }

        // do half-size complex FFT:

        var L = _fftSize;
        var M = _fftSize >> 1;
        var S = _fftSize - 1;
        var ti = 0;
        while (L >= 2)
        {
            var l = L >> 1;
            var u1 = 1.0f;
            var u2 = 0.0f;
            var c = _cosTbl[ti];
            var s = _sinTbl[ti];
            ti++;
            for (var j = 0; j < l; j++)
            {
                for (var i = j; i < _fftSize; i += L)
                {
                    var p = i + l;
                    var t1 = _re[i] + _re[p];
                    var t2 = _im[i] + _im[p];
                    var t3 = _re[i] - _re[p];
                    var t4 = _im[i] - _im[p];
                    _re[p] = t3 * u1 - t4 * u2;
                    _im[p] = t4 * u1 + t3 * u2;
                    _re[i] = t1;
                    _im[i] = t2;
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
                var t1 = _re[j];
                var t2 = _im[j];
                _re[j] = _re[i];
                _im[j] = _im[i];
                _re[i] = t1;
                _im[i] = t2;
            }
            var k = M;
            while (j >= k)
            {
                j -= k;
                k >>= 1;
            }
            j += k;
        }

        // fill output with normalization:

        for (int i = 0, k = 0; i < _fftSize; i++)
        {
            output[k++] = _re[i] / _fftSize;
            output[k++] = _im[i] / _fftSize;
        }
    }

    /// <summary>
    /// Performs a normalized Inverse Fast Fourier Transform (IFFT) on the input data.
    /// Converts complex input data to real output data.
    /// </summary>
    /// <param name="input">The complex input data.</param>
    /// <returns>An array of real numbers representing the normalized IFFT result.</returns>

    public float[] InverseNorm(ReadOnlySpan<ComplexFp32> input)
    {
        var result = new float[_fftSize << 1];
        InverseNorm(input, result);
        return result;
    }


    #endregion


    #region MagnitudeSpectrum



    /// <summary>
    /// Computes the magnitude spectrum from <paramref name="samples"/>:
    /// <code>
    ///     spectrum = sqrt(re * re + im * im)
    /// </code>
    /// Fills the array <paramref name="spectrum"/>. It must have a size of at least fftSize/2+1.
    /// </summary>
    /// <param name="samples">Array of samples.</param>
    /// <param name="spectrum">Magnitude spectrum (the array to fill).</param>
    /// <param name="normalize">Normalize by FFT size or not.</param>

    public void MagnitudeSpectrum(ReadOnlySpan<float> samples, Span<float> spectrum, bool normalize = false)
    {
        //var tempSpectrum = new ComplexFp32[_fftSize + 1];
        ComplexFp32[] tempSpectrum = ArrayPool<ComplexFp32>.Shared.Rent(_fftSize + 1);

        Forward(samples, tempSpectrum);

        // Since for realFFT: im[0] = im[fftSize/2] = 0
        // we don't process separately these elements (like in case of FFT)

        if (normalize)
        {
            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = MathF.Sqrt(tempSpectrum[i].Real + tempSpectrum[i].Real * tempSpectrum[i].Imaginary + tempSpectrum[i].Imaginary) / _fftSize;
            }
        }
        else
        {
            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = MathF.Sqrt(tempSpectrum[i].Real + tempSpectrum[i].Real * tempSpectrum[i].Imaginary + tempSpectrum[i].Imaginary);
            }
        }

        ArrayPool<ComplexFp32>.Shared.Return(tempSpectrum);
    }

    /// <summary>
    /// Computes the magnitude spectrum from <paramref name="samples"/>:
    /// <code>
    ///     spectrum = sqrt(re * re + im * im)
    /// </code>
    /// Returns the array spectrum. It is the size of fftSize/2+1.
    /// </summary>
    /// <param name="samples">Array of samples.</param>
    /// <param name="normalize">Normalize by FFT size or not.</param>
    /// <returns>An array of real numbers representing the magnitude spectrum.</returns>
    public float[] MagnitudeSpectrum(ReadOnlySpan<float> samples, bool normalize = false)
    {
        var result = new float[_fftSize + 1];
        MagnitudeSpectrum(samples, result, normalize);
        return result;
    }





    #endregion


    #region PowerSpectrum

    /// <summary>
    /// <para>Computes power spectrum from <paramref name="samples"/>:</para>
    /// <code>
    ///     spectrum = (re * re + im * im)
    /// </code>
    /// <para>Method fills array <paramref name="spectrum"/>. It must have size at least fftSize/2+1.</para>
    /// </summary>
    /// <param name="samples">Array of samples</param>
    /// <param name="spectrum">Magnitude spectrum (The array to fill )</param>
    /// <param name="normalize">Normalize by FFT size or not</param>
    public void PowerSpectrum(ReadOnlySpan<float> samples, Span<float> spectrum, bool normalize = true)
    {
        //var tempSpectrum = new ComplexFp32[_fftSize + 1];
        ComplexFp32[] tempSpectrum = ArrayPool<ComplexFp32>.Shared.Rent(_fftSize + 1);
        Forward(samples, tempSpectrum);

        // Since for realFFT: im[0] = im[fftSize/2] = 0
        // we don't process separately these elements (like in case of FFT)

        if (normalize)
        {
            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = (tempSpectrum[i].Real + tempSpectrum[i].Real * tempSpectrum[i].Imaginary + tempSpectrum[i].Imaginary) / _fftSize;
            }
        }
        else
        {
            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = tempSpectrum[i].Real + tempSpectrum[i].Real * tempSpectrum[i].Imaginary + tempSpectrum[i].Imaginary;
            }
        }

        ArrayPool<ComplexFp32>.Shared.Return(tempSpectrum);
    }

    /// <summary>
    /// Computes the power spectrum from <paramref name="samples"/>:
    /// <code>
    ///     spectrum = (re * re + im * im)
    /// </code>
    /// Fills the array <paramref name="spectrum"/>. It must have a size of at least fftSize/2+1.
    /// </summary>
    /// <param name="samples">Array of samples.</param>
    /// <param name="normalize">Normalize by FFT size or not.</param>
    /// <returns>An array of real numbers representing the power spectrum.</returns>
    public float[] PowerSpectrum(ReadOnlySpan<float> samples, bool normalize = true)
    {
        var result = new float[_fftSize + 1];
        PowerSpectrum(samples, result, normalize);
        return result;
    }


    #endregion


    /// <summary>
    /// FFT shift in-place. Throws <see cref="ArgumentException"/> if array of <paramref name="samples"/> has odd length.
    /// </summary>
    public static void Shift(float[] samples)
    {
        if ((samples.Length & 1) == 1)
        {
            throw new ArgumentException("FFT shift is not supported for arrays with odd lengths");
        }

        var mid = samples.Length / 2;

        for (var i = 0; i < samples.Length / 2; i++)
        {
            var shift = i + mid;
            var tmp = samples[i];
            samples[i] = samples[shift];
            samples[shift] = tmp;
        }
    }






























































}
