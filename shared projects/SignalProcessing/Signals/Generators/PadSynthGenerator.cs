using Vorcyc.Mathematics.SignalProcessing.Transforms;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// PADSynth wave-table generator (Nasca Octavian Paul algorithm).
/// </summary>
public sealed class PadSynthGenerator : WaveTableGenerator
{
    private readonly Random _rand = new();

    private float _frequency = 440f;
    private float[]? _amplitudes;
    private float _bw = 40f;
    private float _bwScale = 1.25f;
    private RealFft? _fft;
    private int _fftSize = 2048;
    private float[]? _re;
    private float[]? _im;

    /// <summary>
    /// Sampling rate in Hz.
    /// </summary>
    public float SamplingRate { get; set; } = 44100f;

    /// <summary>
    /// Note frequency in Hz.
    /// </summary>
    public float Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            GenerateWavetable();
        }
    }

    /// <summary>
    /// FFT size (power of 2).
    /// </summary>
    public int FftSize
    {
        get => _fftSize;
        set
        {
            _fftSize = value;
            if (_fft is null || _fft.Size != _fftSize)
            {
                _fft = new RealFft(_fftSize);
                _re = new float[_fftSize];
                _im = new float[_fftSize];
                _samples = new float[_fftSize];
            }
            GenerateWavetable();
        }
    }

    /// <summary>
    /// Bandwidth of the first harmonic.
    /// </summary>
    public float Bandwidth
    {
        get => _bw;
        set
        {
            _bw = value;
            GenerateWavetable();
        }
    }

    /// <summary>
    /// Bandwidth scale per harmonic.
    /// </summary>
    public float BandwidthScale
    {
        get => _bwScale;
        set
        {
            _bwScale = value;
            GenerateWavetable();
        }
    }

    /// <summary>
    /// Sets harmonic amplitudes and regenerates the wave table.
    /// </summary>
    public PadSynthGenerator SetAmplitudes(float[] amplitudes)
    {
        _amplitudes = amplitudes;
        GenerateWavetable();
        return this;
    }

    private void GenerateWavetable()
    {
        if (_fft is null || _amplitudes is null || _frequency <= 0 || SamplingRate <= 0)
        {
            return;
        }

        Array.Clear(_re!, 0, _re!.Length);
        Array.Clear(_im!, 0, _im!.Length);

        var fftHalfSize = _fftSize / 2;

        for (var i = 1; i <= _amplitudes.Length; i++)
        {
            if (_amplitudes[i - 1] == 0) continue;

            var bwHz = (Math.Pow(2, _bw / 1200) - 1.0) * _frequency * Math.Pow(i, _bwScale);
            var fi = _frequency * i / SamplingRate;
            var bwi = bwHz / (2.0 * SamplingRate);

            var s = (int)(fi * fftHalfSize);
            if (s >= fftHalfSize) continue;

            var h = 1.0;
            var j = s;
            while (h > 1e-10)
            {
                h = Profile(1.0 * j / fftHalfSize - fi, bwi);
                _re[j--] += (float)h * _amplitudes[i - 1];
            }
            h = 1.0;
            j = s + 1;
            while (h > 1e-10)
            {
                h = Profile(1.0 * j / fftHalfSize - fi, bwi);
                _re[j++] += (float)h * _amplitudes[i - 1];
            }
        }

        for (var i = 0; i < _re.Length; i++)
        {
            var mag = _re[i];
            var phase = _rand.NextDouble() * 2 * Math.PI;
            _re[i] = (float)(mag * Math.Cos(phase));
            _im[i] = (float)(mag * Math.Sin(phase));
        }

        _fft.Inverse(_re, _im, _samples);

        var norm = 1 / _samples.Max();
        for (var i = 0; i < _samples.Length; _samples[i++] *= norm) ;
    }

    private static double Profile(double f, double bw)
    {
        var x = f / bw;
        return Math.Exp(-x * x) / bw;
    }
}
