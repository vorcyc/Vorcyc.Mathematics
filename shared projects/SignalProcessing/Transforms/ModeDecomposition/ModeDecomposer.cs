using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Unified entry points for mode-decomposition algorithms (Signal Studio / apps).
/// </summary>
public static class ModeDecomposer
{
    public static EmdResult<T> Emd<T>(
        ReadOnlySpan<T> signal,
        EmdOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => EmpiricalModeDecomposition.Decompose(signal, options, cancellationToken, progress);

    public static EmdResult<T> Eemd<T>(
        ReadOnlySpan<T> signal,
        EemdOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => EnsembleEmpiricalModeDecomposition.Decompose(signal, options, cancellationToken, progress);

    public static EmdResult<T> Ceemdan<T>(
        ReadOnlySpan<T> signal,
        CeemdanOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => CompleteEnsembleEmd.Decompose(signal, options, cancellationToken, progress);

    public static VmdResult<T> Vmd<T>(
        ReadOnlySpan<T> signal,
        VmdOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => VariationalModeDecomposition.Decompose(signal, options, cancellationToken, progress);

    public static HhtResult<T> Hht<T>(
        ReadOnlySpan<T> signal,
        float samplingRate,
        HhtOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => HilbertHuangTransform.Analyze(signal, samplingRate, options, cancellationToken, progress);

    public static HhtResult<T> Instantaneous<T>(
        IReadOnlyList<T[]> modes,
        float samplingRate,
        T[]? residual = null,
        HhtOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => HilbertHuangTransform.AnalyzeModes(modes, samplingRate, residual, options, cancellationToken, progress);

    public static MemdResult<T> Memd<T>(
        IReadOnlyList<T[]> channels,
        MemdOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => MultivariateEmpiricalModeDecomposition.Decompose(channels, options, cancellationToken, progress);

    public static MvmdResult<T> Mvmd<T>(
        IReadOnlyList<T[]> channels,
        MvmdOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => MultivariateVariationalModeDecomposition.Decompose(channels, options, cancellationToken, progress);

    public static SsaResult<T> Ssa<T>(
        ReadOnlySpan<T> signal,
        SsaOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => SingularSpectrumAnalysis.Decompose(signal, options, cancellationToken, progress);

    public static EwtResult<T> Ewt<T>(
        ReadOnlySpan<T> signal,
        EwtOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => EmpiricalWaveletTransform.Decompose(signal, options, cancellationToken, progress);
}
