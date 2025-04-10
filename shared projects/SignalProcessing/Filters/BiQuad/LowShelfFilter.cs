using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

/// <summary>
/// Represents BiQuad low-shelving filter.
/// </summary>
public class LowShelfFilter : BiQuadFilter
{
    /// <summary>
    /// Gets shelf midpoint frequency.
    /// </summary>
    public float Frequency { get; protected set; }

    /// <summary>
    /// Gets Q factor.
    /// </summary>
    public float Q { get; protected set; }

    /// <summary>
    /// Gets gain (in dB).
    /// </summary>
    public float Gain { get; protected set; }

    /// <summary>
    /// Constructs <see cref="LowShelfFilter"/>.
    /// </summary>
    /// <param name="frequency">Normalized shelf midpoint frequency in range [0..0.5]</param>
    /// <param name="q">Q factor</param>
    /// <param name="gain">Gain (in dB)</param>
    public LowShelfFilter(float frequency, float q = 1, float gain = 1.0f)
    {
        SetCoefficients(frequency, q, gain);
    }

    /// <summary>
    /// Sets filter coefficients.
    /// </summary>
    /// <param name="frequency">Normalized shelf midpoint frequency in range [0..0.5]</param>
    /// <param name="q">Q factor</param>
    /// <param name="gain">Gain (in dB)</param>
    private void SetCoefficients(float frequency, float q, float gain)
    {
        // The coefficients are calculated according to 
        // audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.

        Frequency = frequency;
        Q = q;
        Gain = gain;

        var ga = MathF.Pow(10, gain / 40);
        var asqrt = MathF.Sqrt(ga);
        var omega = 2 * ConstantsFp32.PI * frequency;
        var alpha = MathF.Sin(omega) / 2 * MathF.Sqrt((ga + 1 / ga) * (1 / q - 1) + 2);
        var cosw = MathF.Cos(omega);

        _b[0] = ga * (ga + 1 - (ga - 1) * cosw + 2 * asqrt * alpha);
        _b[1] = 2 * ga * (ga - 1 - (ga + 1) * cosw);
        _b[2] = ga * (ga + 1 - (ga - 1) * cosw - 2 * asqrt * alpha);

        _a[0] = ga + 1 + (ga - 1) * cosw + 2 * asqrt * alpha;
        _a[1] = -2 * (ga - 1 + (ga + 1) * cosw);
        _a[2] = ga + 1 + (ga - 1) * cosw - 2 * asqrt * alpha;

        Normalize();
    }

    /// <summary>
    /// Changes filter coefficients online (preserving the state of the filter).
    /// </summary>
    /// <param name="frequency">Normalized shelf midpoint frequency in range [0..0.5]</param>
    /// <param name="q">Q factor</param>
    /// <param name="gain">Gain (in dB)</param>
    public void Change(float frequency, float q = 1, float gain = 1.0f)
    {
        SetCoefficients(frequency, q, gain);
    }
}