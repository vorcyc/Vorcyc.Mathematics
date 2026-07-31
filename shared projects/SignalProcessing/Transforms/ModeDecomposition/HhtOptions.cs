namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>Options for Hilbert–Huang analysis on IMFs / VMD modes.</summary>
public sealed class HhtOptions
{
    /// <summary>
    /// EMD options used by <see cref="HilbertHuangTransform.Analyze{T}"/>.
    /// Ignored by <c>AnalyzeModes</c>.
    /// </summary>
    public EmdOptions? EmdOptions { get; init; }

    /// <summary>
    /// Include the residual trend as an additional “mode” for instantaneous analysis.
    /// Default false (residual is usually not oscillatory).
    /// Honored by <see cref="HilbertHuangTransform.Analyze{T}"/> and by
    /// <c>AnalyzeModes</c> when a non-empty residual is supplied.
    /// </summary>
    public bool AnalyzeResidual { get; init; } = false;

    /// <summary>
    /// When true (default), build a sparse Hilbert spectrum from instantaneous a(t), f(t).
    /// </summary>
    public bool BuildSpectrum { get; init; } = true;

    /// <summary>Keep every N-th time sample in the spectrum (default 1 = all).</summary>
    public int SpectrumTimeStride { get; init; } = 1;

    /// <summary>Discard spectrum samples with amplitude below this (relative to mode peak).</summary>
    public double SpectrumMinRelativeAmplitude { get; init; } = 0.02;

    /// <summary>
    /// Optional CPU policy: forwarded to EMD when used, and to the Hilbert analytic FFT
    /// (honors <see cref="ComputingScope"/> when null).
    /// </summary>
    public ComputingContext? ComputingContext { get; init; }
}
