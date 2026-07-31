namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Options for <see cref="SingularSpectrumAnalysis"/>.
/// </summary>
public sealed class SsaOptions
{
    /// <summary>
    /// Embedding window length L. Default: N/2. Values outside [2, N/2] are clamped.
    /// </summary>
    public int? WindowLength { get; init; }

    /// <summary>
    /// Maximum singular components to reconstruct (default min(L, 20)).
    /// </summary>
    public int? ComponentCount { get; init; }

    /// <summary>
    /// Number of consecutive singular components merged per reconstructed group (default 1).
    /// </summary>
    public int GroupSize { get; init; } = 1;

    /// <summary>
    /// Embedding / reconstruction / residual kernels honor this context.
    /// SVD Householder bidiagonalization and factor combines also honor it;
    /// the bidiagonal QR iteration remains sequential (cancel is polled).
    /// </summary>
    public ComputingContext? ComputingContext { get; init; }
}
