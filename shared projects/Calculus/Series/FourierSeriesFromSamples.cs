namespace Vorcyc.Mathematics.Calculus.Series;

using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Transforms;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

/// <summary>
/// Estimates Fourier series coefficients from periodic sample data (FFT-based; the sample count must be a power of 2).
/// </summary>
public sealed class FourierSeriesFromSamples
{
    private readonly float[] _samples;
    private readonly float _period;
    private readonly int _sampleCount;
    private readonly float[] _realSpectrum;
    private readonly float[] _imagSpectrum;
    private readonly float _cosScale;
    private readonly float _sinScale;
    private readonly float _twoPiOverPeriod;

    private float[]? _aCoeffs;
    private float[]? _bCoeffs;
    private int _cachedMaxOrder = -1;

    /// <summary>
    /// Initializes the instance using uniform samples over one period.
    /// </summary>
    /// <param name="samples">The function samples over one period; the length must be a power of 2.</param>
    /// <param name="period">The period T.</param>
    public FourierSeriesFromSamples(ReadOnlySpan<float> samples, float period)
    {
        if (samples.IsEmpty) throw new ArgumentException("Samples cannot be empty.", nameof(samples));
        if (period <= 0) throw new ArgumentException("Period must be positive.", nameof(period));
        if (!IsPowerOfTwo(samples.Length))
            throw new ArgumentException("Sample count must be a power of 2.", nameof(samples));

        _sampleCount = samples.Length;
        _period = period;
        _samples = samples.ToArray();
        _realSpectrum = new float[_sampleCount];
        _imagSpectrum = new float[_sampleCount];
        _cosScale = 2f / _sampleCount;
        _sinScale = -_cosScale;
        _twoPiOverPeriod = 2f * MathF.PI / _period;

        var fft = new Fft(_sampleCount);
        _samples.CopyTo(_realSpectrum, 0);
        Array.Clear(_imagSpectrum, 0, _sampleCount);
        fft.Direct(_realSpectrum, _imagSpectrum);
    }

    /// <summary>The number of samples.</summary>
    public int SampleCount => _sampleCount;

    /// <summary>The period.</summary>
    public float Period => _period;

    /// <summary>
    /// Gets the cosine coefficient aₙ (consistent with the definition in <see cref="FourierSeries{T}"/>).
    /// </summary>
    public float GetCosineCoefficient(int n)
    {
        ValidateHarmonic(n);
        EnsureHarmonicCoeffs(n);
        return _aCoeffs![n];
    }

    /// <summary>
    /// Gets the sine coefficient bₙ.
    /// </summary>
    public float GetSineCoefficient(int n)
    {
        ValidateHarmonic(n);
        EnsureHarmonicCoeffs(n);
        return _bCoeffs![n];
    }

    /// <summary>
    /// Reconstructs f(x) using the estimated coefficients.
    /// </summary>
    public float Evaluate(float x, int order)
    {
        if (order < 0) throw new ArgumentException("Order must be non-negative.", nameof(order));

        EnsureHarmonicCoeffs(order);

        float sum = _aCoeffs![0] * 0.5f;
        for (int n = 1; n <= order; n++)
        {
            float angle = n * _twoPiOverPeriod * x;
            sum += _aCoeffs[n] * MathF.Cos(angle) + _bCoeffs![n] * MathF.Sin(angle);
        }

        return sum;
    }

    private void EnsureHarmonicCoeffs(int order)
    {
        if (_cachedMaxOrder >= order)
            return;

        int need = order + 1;
        if (_aCoeffs is null || _aCoeffs.Length < need)
        {
            _aCoeffs = new float[need];
            _bCoeffs = new float[need];
            _cachedMaxOrder = -1;
        }

        int start = _cachedMaxOrder + 1;
        for (int n = start; n <= order; n++)
        {
            if (n == 0)
            {
                _aCoeffs[0] = _cosScale * _realSpectrum[0];
                _bCoeffs[0] = 0f;
                continue;
            }

            _aCoeffs[n] = _cosScale * _realSpectrum[n];
            _bCoeffs[n] = _sinScale * _imagSpectrum[n];
        }

        _cachedMaxOrder = order;
    }

    private void ValidateHarmonic(int n)
    {
        if (n < 0) throw new ArgumentException("Harmonic order must be non-negative.", nameof(n));
        if (n > _sampleCount / 2)
            throw new ArgumentException($"Harmonic order cannot exceed {_sampleCount / 2}.", nameof(n));
    }

    private static bool IsPowerOfTwo(int value) => value > 0 && (value & (value - 1)) == 0;
}
