namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Result of empirical mode decomposition: IMF<sub>1…K</sub> + residual.
/// Reconstruction: <c>signal ≈ Σ IMFᵢ + Residual</c>.
/// </summary>
/// <typeparam name="T">Sample type (<see cref="float"/> or <see cref="double"/>).</typeparam>
public sealed class EmdResult<T>
    where T : unmanaged
{
    /// <summary>Intrinsic mode functions ordered from highest-frequency to lower-frequency.</summary>
    public required IReadOnlyList<T[]> IntrinsicModeFunctions { get; init; }

    /// <summary>Final residual (trend / monotonic remainder).</summary>
    public required T[] Residual { get; init; }

    /// <summary>Number of extracted IMFs.</summary>
    public int ModeCount => IntrinsicModeFunctions.Count;

    /// <summary>Why extraction stopped.</summary>
    public EmdStopReason StopReason { get; init; }
}

/// <summary>Termination reason for EMD extraction.</summary>
public enum EmdStopReason
{
    /// <summary>Residual no longer has enough extrema to continue.</summary>
    ResidualTooFewExtrema = 0,

    /// <summary>Reached <see cref="EmdOptions.MaxImf"/>.</summary>
    MaxImfReached = 1,

    /// <summary>Input was too short or otherwise unsuitable.</summary>
    InputRejected = 2,
}
