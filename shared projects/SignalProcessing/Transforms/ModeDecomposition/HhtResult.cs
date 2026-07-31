namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Hilbert–Huang transform result: modes plus instantaneous amplitude / frequency.
/// </summary>
public sealed class HhtResult<T>
    where T : unmanaged
{
    /// <summary>Modes that were analyzed (IMFs and optionally residual).</summary>
    public required IReadOnlyList<T[]> Modes { get; init; }

    /// <summary>Residual from EMD when available; otherwise empty.</summary>
    public required T[] Residual { get; init; }

    /// <summary>Instantaneous amplitude a(t) per mode (same length as each mode).</summary>
    public required IReadOnlyList<T[]> InstantaneousAmplitudes { get; init; }

    /// <summary>
    /// Instantaneous frequency in Hz per mode (length = N−1 samples at midpoints;
    /// last sample repeats the previous value for length-N convenience).
    /// </summary>
    public required IReadOnlyList<T[]> InstantaneousFrequenciesHz { get; init; }

    /// <summary>Sampling rate used for Hz conversion.</summary>
    public float SamplingRate { get; init; }

    /// <summary>Underlying EMD stop reason when EMD was run; otherwise null.</summary>
    public EmdStopReason? EmdStopReason { get; init; }

    /// <summary>
    /// Sparse Hilbert spectrum samples (t, f, a) when requested via <see cref="HhtOptions.BuildSpectrum"/>.
    /// </summary>
    public IReadOnlyList<HilbertSpectrumSample> Spectrum { get; init; } = Array.Empty<HilbertSpectrumSample>();

    public int ModeCount => Modes.Count;
}
