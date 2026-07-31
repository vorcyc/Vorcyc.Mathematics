namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Result of multivariate variational mode decomposition.
/// <see cref="Modes"/>[k][c] is mode k on channel c.
/// </summary>
public sealed class MvmdResult<T>
    where T : unmanaged
{
    /// <summary>K modes; each entry has C channel time series of equal length.</summary>
    public required IReadOnlyList<T[][]> Modes { get; init; }

    /// <summary>Per-channel residual: signal[c] − Σ<sub>k</sub> Modes[k][c].</summary>
    public required T[][] Residual { get; init; }

    /// <summary>Shared center frequencies in Hz.</summary>
    public required double[] CenterFrequenciesHz { get; init; }

    /// <summary>Shared normalized center frequencies ω ∈ [0, 0.5] (cycles per sample).</summary>
    public required double[] CenterFrequenciesNormalized { get; init; }

    /// <summary>ADMM iterations performed.</summary>
    public int Iterations { get; init; }

    /// <summary>True when relative mode change fell below tolerance.</summary>
    public bool Converged { get; init; }

    public int ModeCount => Modes.Count;
    public int ChannelCount => Residual.Length;
}
