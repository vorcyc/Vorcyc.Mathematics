namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Result of empirical wavelet transform.
/// The lowest-frequency band is reported as <see cref="Residual"/>; higher bands are <see cref="Modes"/>.
/// </summary>
public sealed class EwtResult<T>
    where T : unmanaged
{
    /// <summary>Band-pass modes (highest-frequency band first).</summary>
    public required IReadOnlyList<T[]> Modes { get; init; }

    /// <summary>Lowest-frequency band (approximation / trend).</summary>
    public required T[] Residual { get; init; }

    /// <summary>Detected segment boundary frequencies in Hz (ascending).</summary>
    public required double[] BoundaryFrequenciesHz { get; init; }

    public int ModeCount => Modes.Count;
}
