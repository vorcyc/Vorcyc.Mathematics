namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Options for <see cref="EmpiricalModeDecomposition"/>.
/// </summary>
public sealed class EmdOptions
{
    /// <summary>Maximum number of intrinsic mode functions to extract (default 16).</summary>
    public int MaxImf { get; init; } = 16;

    /// <summary>Maximum sifting iterations per IMF (default 100).</summary>
    public int MaxSiftIterations { get; init; } = 100;

    /// <summary>
    /// Cauchy-type sifting stopping threshold on
    /// Σ(h<sub>prev</sub>−h)² / Σ h<sub>prev</sub>² (Huang et al.; typical 0.2–0.3).
    /// </summary>
    public double SiftingTolerance { get; init; } = 0.2;

    /// <summary>
    /// Stop extracting further IMFs when the residual has fewer than this many extrema
    /// (maxima + minima). Default 2 (monotonic / trend-like residual).
    /// </summary>
    public int MinExtremaToContinue { get; init; } = 2;

    /// <summary>
    /// Optional CPU execution policy for bulk vector updates (mean subtract / residual).
    /// Envelope spline construction remains sequential.
    /// </summary>
    public ComputingContext? ComputingContext { get; init; }
}
