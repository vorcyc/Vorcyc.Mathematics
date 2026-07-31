namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Options for <see cref="MultivariateVariationalModeDecomposition"/> (coupled multivariate VMD).
/// </summary>
public sealed class MvmdOptions
{
    /// <summary>Number of shared modes K (default 3).</summary>
    public int ModeCount { get; init; } = 3;

    /// <summary>Bandwidth / data-fidelity trade-off α (default 2000).</summary>
    public double Alpha { get; init; } = 2000;

    /// <summary>Dual-ascent time step τ. Use 0 to disable Lagrange updates.</summary>
    public double Tau { get; init; } = 0;

    /// <summary>Convergence tolerance on relative mode change (default 1e-7).</summary>
    public double Tolerance { get; init; } = 1e-7;

    /// <summary>Maximum ADMM iterations (default 500).</summary>
    public int MaxIterations { get; init; } = 500;

    /// <summary>When true, mode 0 is constrained as a DC component (ω₀ stays 0).</summary>
    public bool DcMode { get; init; } = false;

    /// <summary>
    /// Center-frequency initialization: 0 = all zero,
    /// 1 = uniform in [0, ½) cycles/sample, 2 = random in [0, ½).
    /// </summary>
    public int OmegaInit { get; init; } = 1;

    /// <summary>Optional RNG seed when <see cref="OmegaInit"/> = 2.</summary>
    public int? RandomSeed { get; init; }

    /// <summary>Sampling rate in Hz for center-frequency reporting.</summary>
    public float SamplingRate { get; init; } = 1f;

    /// <summary>Optional CPU execution policy (per-bin mode updates).</summary>
    public ComputingContext? ComputingContext { get; init; }
}
