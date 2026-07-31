namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Options for <see cref="EmpiricalWaveletTransform"/> (Gilles, 2013).
/// </summary>
public sealed class EwtOptions
{
    /// <summary>Maximum number of frequency bands (default 4).</summary>
    public int MaxBands { get; init; } = 4;

    /// <summary>
    /// Normalized transition width for Meyer-like raised-cosine boundaries (default 0.05).
    /// </summary>
    public double TransitionWidth { get; init; } = 0.05;

    /// <summary>Sampling rate in Hz for boundary-frequency reporting.</summary>
    public float SamplingRate { get; init; } = 1f;

    /// <summary>
    /// Minimum peak height as a fraction of the spectrum maximum (default 0.05).
    /// </summary>
    public double MinPeakHeight { get; init; } = 0.05;

    /// <summary>Optional CPU execution policy (FFT / filter application).</summary>
    public ComputingContext? ComputingContext { get; init; }
}
