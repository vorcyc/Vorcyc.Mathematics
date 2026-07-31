namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Options for <see cref="MultivariateEmpiricalModeDecomposition"/> (Rehman &amp; Mandic).
/// </summary>
public sealed class MemdOptions
{
    /// <summary>Maximum number of multivariate IMFs to extract (default 16).</summary>
    public int MaxImf { get; init; } = 16;

    /// <summary>Maximum sifting iterations per IMF (default 100).</summary>
    public int MaxSiftIterations { get; init; } = 100;

    /// <summary>
    /// Cauchy-type sifting stopping threshold on relative change (typical 0.2–0.3).
    /// </summary>
    public double SiftingTolerance { get; init; } = 0.2;

    /// <summary>
    /// Number of projection directions on the unit (n−1)-sphere (default 64).
    /// </summary>
    public int DirectionCount { get; init; } = 64;

    /// <summary>
    /// Stop extracting further IMFs when no projection direction has at least this many
    /// extrema (maxima + minima). Default 2.
    /// </summary>
    public int MinExtremaToContinue { get; init; } = 2;

    /// <summary>
    /// Optional CPU execution policy for bulk channel updates (accumulate / scale / mean / residual).
    /// Envelope construction remains sequential.
    /// </summary>
    public ComputingContext? ComputingContext { get; init; }
}
