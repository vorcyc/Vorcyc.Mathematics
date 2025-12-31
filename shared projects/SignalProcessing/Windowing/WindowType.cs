namespace Vorcyc.Mathematics.SignalProcessing.Windowing;

/// <summary>
/// Defines the most commonly used window functions.
/// </summary>
public enum WindowType
{
    /// <summary>
    /// Rectangular window.
    /// </summary>
    Rectangular,

    /// <summary>
    /// Triangular window.
    /// </summary>
    Triangular,

    /// <summary>
    /// Hamming window (symmetric; denominator N-1).
    /// </summary>
    Hamming,

    /// <summary>
    /// Hamming window (periodic; denominator N).
    /// </summary>
    HammingPeriodic,

    /// <summary>
    /// Blackman window (symmetric; denominator N-1).
    /// </summary>
    Blackman,

    /// <summary>
    /// Blackman window (periodic; denominator N).
    /// </summary>
    BlackmanPeriodic,

    /// <summary>
    /// Hann window (symmetric; denominator N-1).
    /// </summary>
    Hann,

    /// <summary>
    /// Hann window (periodic; denominator N).
    /// </summary>
    HannPeriodic,

    /// <summary>
    /// Gaussian window.
    /// </summary>
    Gaussian,

    /// <summary>
    /// Kaiser window.
    /// </summary>
    Kaiser,

    /// <summary>
    /// Kaiser-Bessel Derived window.
    /// </summary>
    Kbd,

    /// <summary>
    /// Bartlett-Hann window.
    /// </summary>
    BartlettHann,

    /// <summary>
    /// Lanczos window.
    /// </summary>
    Lanczos,

    /// <summary>
    /// Power-of-sine window.
    /// </summary>
    PowerOfSine,

    /// <summary>
    /// Flat-top window.
    /// </summary>
    Flattop,

    /// <summary>
    /// Window for cepstral liftering.
    /// </summary>
    Liftering,

    /// <summary>
    /// Blackman Harris window.
    /// </summary>
    BlackmanHarris
}