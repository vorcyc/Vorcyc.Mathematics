namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Options for <see cref="EnsembleEmpiricalModeDecomposition"/> (Wu &amp; Huang, 2009).
/// </summary>
public sealed class EemdOptions
{
    /// <summary>Number of noise-added ensemble trials (default 100, clamped to [2, 500]).</summary>
    public int EnsembleCount { get; init; } = 100;

    /// <summary>
    /// Gaussian noise scale as a fraction of the signal standard deviation (default 0.2).
    /// </summary>
    public double NoiseRatio { get; init; } = 0.2;

    /// <summary>Optional RNG seed for reproducible noise ensembles.</summary>
    public int? RandomSeed { get; init; }

    /// <summary>Underlying EMD sift settings (notably <see cref="EmdOptions.MaxImf"/>).</summary>
    public EmdOptions? EmdOptions { get; init; }

    /// <summary>
    /// Optional CPU execution policy; applied to <see cref="EmdOptions"/> when that
    /// property is null or its <see cref="EmdOptions.ComputingContext"/> is unset.
    /// </summary>
    public ComputingContext? ComputingContext { get; init; }
}
