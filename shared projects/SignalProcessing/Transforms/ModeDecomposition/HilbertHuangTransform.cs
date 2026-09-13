using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Hilbert–Huang Transform: EMD (or supplied modes) + instantaneous amplitude / frequency
/// via the analytic signal, with optional sparse Hilbert spectrum.
/// </summary>
public static class HilbertHuangTransform
{
    /// <summary>
    /// Run EMD then compute instantaneous amplitude and frequency for each IMF.
    /// For already extracted modes, use <c>AnalyzeModes</c>.
    /// </summary>
    public static HhtResult<T> Analyze<T>(
        ReadOnlySpan<T> signal,
        float samplingRate,
        HhtOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        options ??= new HhtOptions();
        if (!(samplingRate > 0) || float.IsNaN(samplingRate) || float.IsInfinity(samplingRate))
            throw new ArgumentOutOfRangeException(nameof(samplingRate), "Sampling rate must be positive.");

        var emdOpts = options.EmdOptions ?? new EmdOptions();
        if (options.ComputingContext is not null && emdOpts.ComputingContext is null)
        {
            emdOpts = new EmdOptions
            {
                MaxImf = emdOpts.MaxImf,
                MaxSiftIterations = emdOpts.MaxSiftIterations,
                SiftingTolerance = emdOpts.SiftingTolerance,
                MinExtremaToContinue = emdOpts.MinExtremaToContinue,
                ComputingContext = options.ComputingContext,
            };
        }

        var emd = EmpiricalModeDecomposition.Decompose(signal, emdOpts, cancellationToken, progress);
        var modes = new List<T[]>(emd.IntrinsicModeFunctions);
        if (options.AnalyzeResidual && emd.Residual.Length > 0)
            modes.Add(emd.Residual);

        return AnalyzeModes(modes, emd.Residual, samplingRate, emd.StopReason, options, cancellationToken, progress);
    }

    /// <summary>
    /// Compute instantaneous amplitude / frequency for already extracted modes
    /// (EMD IMFs or VMD modes).
    /// </summary>
    public static HhtResult<T> AnalyzeModes<T>(
        IReadOnlyList<T[]> modes,
        float samplingRate,
        T[]? residual = null,
        HhtOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
        => AnalyzeModes(
            modes,
            residual ?? Array.Empty<T>(),
            samplingRate,
            emdStop: null,
            options ?? new HhtOptions(),
            cancellationToken,
            progress);

    private static HhtResult<T> AnalyzeModes<T>(
        IReadOnlyList<T[]> modes,
        T[] residual,
        float samplingRate,
        EmdStopReason? emdStop,
        HhtOptions options,
        CancellationToken cancellationToken,
        IProgress<ModeDecompositionProgress>? progress)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        ModeDecompositionSupport.EnsureFloatOrDouble<T>();
        ArgumentNullException.ThrowIfNull(modes);
        if (!(samplingRate > 0))
            throw new ArgumentOutOfRangeException(nameof(samplingRate));

        var modeList = new List<T[]>(modes.Count + 1);
        for (int i = 0; i < modes.Count; i++)
            modeList.Add(modes[i] ?? throw new ArgumentException("Mode array is null.", nameof(modes)));
        if (options.AnalyzeResidual && residual.Length > 0
            && (modeList.Count == 0 || !ReferenceEquals(modeList[^1], residual)))
            modeList.Add(residual);

        var ctx = options.ComputingContext;
        var amps = new T[modeList.Count][];
        var freqs = new T[modeList.Count][];
        for (int m = 0; m < modeList.Count; m++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ModeDecompositionProgress
            {
                Algorithm = "HHT",
                CurrentMode = m,
                TotalModes = modeList.Count,
                Iteration = 0,
                Fraction = modeList.Count == 0 ? 1 : (double)m / modeList.Count,
                Message = $"Instantaneous mode {m + 1}",
            });
            Instantaneous(modeList[m], samplingRate, ctx, out amps[m], out freqs[m]);
        }

        var spectrum = options.BuildSpectrum
            ? BuildSpectrum(amps, freqs, samplingRate, options, cancellationToken)
            : Array.Empty<HilbertSpectrumSample>();

        return new HhtResult<T>
        {
            Modes = modeList,
            Residual = residual ?? Array.Empty<T>(),
            InstantaneousAmplitudes = amps,
            InstantaneousFrequenciesHz = freqs,
            SamplingRate = samplingRate,
            EmdStopReason = emdStop,
            Spectrum = spectrum,
        };
    }

    private static HilbertSpectrumSample[] BuildSpectrum<T>(
        T[][] amps,
        T[][] freqs,
        float samplingRate,
        HhtOptions options,
        CancellationToken cancellationToken)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int stride = Math.Max(1, options.SpectrumTimeStride);
        double minRel = options.SpectrumMinRelativeAmplitude;
        if (!(minRel >= 0)) minRel = 0.02;
        var list = new List<HilbertSpectrumSample>(amps.Length * 64);

        for (int m = 0; m < amps.Length; m++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var a = amps[m];
            var f = freqs[m];
            T peak = T.Zero;
            for (int i = 0; i < a.Length; i++)
            {
                T abs = T.Abs(a[i]);
                if (abs > peak) peak = abs;
            }
            T thr = peak * T.CreateChecked(minRel);

            for (int i = 0; i < a.Length; i += stride)
            {
                T amp = T.Abs(a[i]);
                if (amp < thr) continue;
                T freq = f[i];
                if (!(freq > T.Zero)) continue;
                list.Add(new HilbertSpectrumSample(
                    i / (double)samplingRate,
                    Convert.ToDouble(freq),
                    Convert.ToDouble(amp)));
            }
        }

        return list.ToArray();
    }

    private static void Instantaneous<T>(
        T[] mode, float samplingRate, ComputingContext? ctx, out T[] amplitude, out T[] frequencyHz)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = mode.Length;
        amplitude = new T[n];
        frequencyHz = new T[n];
        if (n == 0) return;

        var re = (T[])mode.Clone();
        var im = new T[n];
        HilbertTransform<T>.AnalyticSignalInPlace(re, im, ctx);

        var phase = new T[n];
        for (int i = 0; i < n; i++)
        {
            amplitude[i] = T.Sqrt(re[i] * re[i] + im[i] * im[i]);
            phase[i] = T.Atan2(im[i], re[i]);
        }

        T pi = T.Pi;
        T twoPi = pi + pi;
        T offset = T.Zero;
        for (int i = 1; i < n; i++)
        {
            T d = phase[i] - phase[i - 1] + offset;
            if (d > pi)
            {
                offset -= twoPi;
                d -= twoPi;
            }
            else if (d < -pi)
            {
                offset += twoPi;
                d += twoPi;
            }
            phase[i] = phase[i - 1] + d;
        }

        T dt = T.One / T.CreateChecked(samplingRate);
        T twoPiDt = twoPi * dt;
        for (int i = 0; i < n - 1; i++)
        {
            T fq = (phase[i + 1] - phase[i]) / twoPiDt;
            if (fq < T.Zero) fq = T.Zero;
            frequencyHz[i] = fq;
        }
        frequencyHz[n - 1] = n >= 2 ? frequencyHz[n - 2] : T.Zero;
    }
}
