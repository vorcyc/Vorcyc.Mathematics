namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Options for <see cref="CompleteEnsembleEmd"/> (Torres et al., ICASSP 2011 CEEMDAN).
/// </summary>
public sealed class CeemdanOptions
{
    /// <summary>Number of noise-added ensemble trials (default 50, clamped to [2, 500]).</summary>
    public int EnsembleCount { get; init; } = 50;

    /// <summary>
    /// Base noise scale as a fraction of the current residual standard deviation
    /// (εₖ = NoiseRatio · σ(rₖ); default 0.2).
    /// </summary>
    public double NoiseRatio { get; init; } = 0.2;

    /// <summary>
    /// Maximum number of IMFs to extract (default 16).
    /// When <see cref="EmdOptions"/> is also set, the effective limit is
    /// <c>min(MaxImf, EmdOptions.MaxImf)</c>.
    /// </summary>
    public int MaxImf { get; init; } = 16;

    /// <summary>Optional RNG seed for reproducible noise ensembles.</summary>
    public int? RandomSeed { get; init; }

    /// <summary>
    /// Base EMD sift settings (sifting tolerance, max sift iterations, min extrema).
    /// Each CEEMDAN stage extracts only the first IMF (<c>MaxImf = 1</c>);
    /// noise realizations are fully decomposed up to the outer IMF budget.
    /// <see cref="EmdOptions.MaxImf"/> also tightens the outer IMF budget (see <see cref="MaxImf"/>).
    /// </summary>
    public EmdOptions? EmdOptions { get; init; }

    /// <summary>
    /// Optional CPU execution policy for noise precompute / ensemble stages / residual updates;
    /// also applied to nested <see cref="EmdOptions"/> when that property is null or its
    /// <see cref="EmdOptions.ComputingContext"/> is unset.
    /// </summary>
    public ComputingContext? ComputingContext { get; init; }
}
