namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Result of variational mode decomposition.
/// Reconstruction: <c>signal ≈ Σ Modes[k]</c> (residual holds the leftover).
/// </summary>
public sealed class VmdResult<T>
    where T : unmanaged
{
    /// <summary>Band-limited modes u<sub>k</sub>(t), length = original signal length.</summary>
    public required IReadOnlyList<T[]> Modes { get; init; }

    /// <summary><c>signal − Σ modes</c> (should be near zero when τ = 0 and converged).</summary>
    public required T[] Residual { get; init; }

    /// <summary>Center frequencies in Hz (using <see cref="VmdOptions.SamplingRate"/>).</summary>
    public required double[] CenterFrequenciesHz { get; init; }

    /// <summary>Normalized center frequencies ω ∈ [0, 0.5] (cycles per sample).</summary>
    public required double[] CenterFrequenciesNormalized { get; init; }

    /// <summary>ADMM iterations performed.</summary>
    public int Iterations { get; init; }

    /// <summary>True when relative mode change fell below tolerance.</summary>
    public bool Converged { get; init; }

    public int ModeCount => Modes.Count;
}
