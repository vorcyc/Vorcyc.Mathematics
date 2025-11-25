using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

/// <summary>
/// Represents BiQuad bandpass filter.
/// </summary>
public class BandPassFilter : BiQuadFilter
{
    /// <summary>
    /// Gets center frequency.
    /// </summary>
    public float Frequency { get; protected set; }

    /// <summary>
    /// Gets Q factor.
    /// </summary>
    public float Q { get; protected set; }

    /// <summary>
    /// Constructs <see cref="BandPassFilter"/>.
    /// </summary>
    /// <param name="frequency">Normalized center frequency in range [0..0.5]</param>
    /// <param name="q">Q factor</param>
    public BandPassFilter(float frequency, float q = 1)
    {
        SetCoefficients(frequency, q);
    }

    /// <summary>
    /// Sets filter coefficients.
    /// </summary>
    /// <param name="frequency">Normalized center frequency in range [0..0.5]</param>
    /// <param name="q">Q factor</param>
    private void SetCoefficients(float frequency, float q)
    {
        // The coefficients are calculated according to 
        // audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.

        Frequency = frequency;
        Q = q;

        var omega = 2 * ConstantsFp32.PI * frequency;
        var alpha = MathF.Sin(omega) / (2 * q);
        var cosw = MathF.Cos(omega);

        _b[0] = alpha;
        _b[1] = 0;
        _b[2] = -_b[0];

        _a[0] = 1 + alpha;
        _a[1] = -2 * cosw;
        _a[2] = 1 - alpha;

        Normalize();
    }

    /// <summary>
    /// Changes filter coefficients online (preserving the state of the filter).
    /// </summary>
    /// <param name="frequency">Normalized center frequency in range [0..0.5]</param>
    /// <param name="q">Q factor</param>
    public void Change(float frequency, float q = 1)
    {
        SetCoefficients(frequency, q);
    }
}