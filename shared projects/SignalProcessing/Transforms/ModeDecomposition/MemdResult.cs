namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Result of multivariate empirical mode decomposition: IMF<sub>1…K</sub> + per-channel residual.
/// Each IMF is a <c>channels × samples</c> matrix; reconstruction:
/// <c>channel[c][t] ≈ Σ IMFᵢ[c][t] + Residual[c][t]</c>.
/// </summary>
/// <typeparam name="T">Sample type (<see cref="float"/> or <see cref="double"/>).</typeparam>
public sealed class MemdResult<T>
    where T : unmanaged
{
    /// <summary>
    /// Intrinsic mode functions ordered from highest-frequency to lower-frequency.
    /// Each entry is <c>[channel][sample]</c>.
    /// </summary>
    public required IReadOnlyList<T[][]> IntrinsicModeFunctions { get; init; }

    /// <summary>Final per-channel residual (trend / monotonic remainder).</summary>
    public required T[][] Residual { get; init; }

    /// <summary>Number of extracted multivariate IMFs.</summary>
    public int ModeCount => IntrinsicModeFunctions.Count;

    /// <summary>Why extraction stopped.</summary>
    public EmdStopReason StopReason { get; init; }
}
