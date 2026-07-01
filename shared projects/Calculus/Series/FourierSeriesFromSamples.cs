namespace Vorcyc.Mathematics.Calculus.Series;

using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Transforms;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

/// <summary>
/// 从周期采样数据估计傅里叶级数系数（基于 FFT，要求采样数为 2 的幂）。
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
    /// 使用一个周期内的均匀采样初始化。
    /// </summary>
    /// <param name="samples">一个周期内的函数采样，长度须为 2 的幂</param>
    /// <param name="period">周期 T</param>
    public FourierSeriesFromSamples(ReadOnlySpan<float> samples, float period)
    {
        if (samples.IsEmpty) throw new ArgumentException("采样不能为空", nameof(samples));
        if (period <= 0) throw new ArgumentException("周期必须为正", nameof(period));
        if (!IsPowerOfTwo(samples.Length))
            throw new ArgumentException("采样数必须为 2 的幂", nameof(samples));

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

    /// <summary>采样点数。</summary>
    public int SampleCount => _sampleCount;

    /// <summary>周期。</summary>
    public float Period => _period;

    /// <summary>
    /// 获取余弦系数 aₙ（与 <see cref="FourierSeries{T}"/> 定义一致）。
    /// </summary>
    public float GetCosineCoefficient(int n)
    {
        ValidateHarmonic(n);
        EnsureHarmonicCoeffs(n);
        return _aCoeffs![n];
    }

    /// <summary>
    /// 获取正弦系数 bₙ。
    /// </summary>
    public float GetSineCoefficient(int n)
    {
        ValidateHarmonic(n);
        EnsureHarmonicCoeffs(n);
        return _bCoeffs![n];
    }

    /// <summary>
    /// 用估计系数重构 f(x)。
    /// </summary>
    public float Evaluate(float x, int order)
    {
        if (order < 0) throw new ArgumentException("阶数必须非负", nameof(order));

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
        if (n < 0) throw new ArgumentException("谐波阶数必须非负", nameof(n));
        if (n > _sampleCount / 2)
            throw new ArgumentException($"谐波阶数不能超过 {_sampleCount / 2}", nameof(n));
    }

    private static bool IsPowerOfTwo(int value) => value > 0 && (value & (value - 1)) == 0;
}
