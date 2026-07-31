namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Result of singular spectrum analysis.
/// Reconstruction: <c>signal ≈ Σ Components</c> (residual holds the leftover).
/// </summary>
public sealed class SsaResult<T>
    where T : unmanaged
{
    /// <summary>Reconstructed SSA component time series.</summary>
    public required IReadOnlyList<T[]> Components { get; init; }

    /// <summary><c>signal − Σ components</c>.</summary>
    public required T[] Residual { get; init; }

    /// <summary>Singular values σᵢ of the trajectory matrix (descending).</summary>
    public required double[] SingularValues { get; init; }

    /// <summary>Embedding window length L used.</summary>
    public int WindowLength { get; init; }

    public int ComponentCount => Components.Count;
}
