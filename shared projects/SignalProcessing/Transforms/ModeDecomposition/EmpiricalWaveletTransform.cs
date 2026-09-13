using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Empirical Wavelet Transform (Gilles, 2013): adaptively partition the Fourier
/// spectrum into band-limited modes using Meyer-like raised-cosine filters.
/// </summary>
public static class EmpiricalWaveletTransform
{
    private const int MinLength = 8;

    /// <summary>Decompose a real signal into empirical wavelet bands.</summary>
    public static EwtResult<T> Decompose<T>(
        ReadOnlySpan<T> signal,
        EwtOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        ModeDecompositionSupport.EnsureFloatOrDouble<T>();

        options ??= new EwtOptions();
        int n0 = signal.Length;
        if (n0 < MinLength)
            throw new ArgumentException($"Signal length must be ≥ {MinLength}.", nameof(signal));

        int maxBands = Math.Clamp(options.MaxBands, 1, 32);
        double gamma = options.TransitionWidth > 0 ? options.TransitionWidth : 0.05;
        gamma = Math.Clamp(gamma, 1e-4, 0.25);
        float sr = options.SamplingRate > 0 ? options.SamplingRate : 1f;
        double minPeak = Math.Clamp(options.MinPeakHeight, 0, 1);
        var ctx = options.ComputingContext;

        int fftSize = ModeDecompositionSupport.NextPow2(n0);
        var re = new T[fftSize];
        var im = new T[fftSize];
        signal.CopyTo(re.AsSpan(0, n0));

        var fft = new Fft<T>(fftSize);
        fft.Direct(re, im, ctx);

        int halfBins = fftSize / 2 + 1;
        var mag = new double[halfBins];
        for (int i = 0; i < halfBins; i++)
        {
            double r = Convert.ToDouble(re[i]);
            double ii = Convert.ToDouble(im[i]);
            mag[i] = Math.Sqrt(r * r + ii * ii);
        }

        double maxMag = 0;
        for (int i = 1; i < halfBins - 1; i++)
            if (mag[i] > maxMag) maxMag = mag[i];
        double peakThresh = maxMag * minPeak;

        var peaks = FindLocalMaxima(mag, peakThresh);
        peaks.Sort((a, b) => mag[b].CompareTo(mag[a]));
        int boundaryCount = Math.Min(maxBands - 1, peaks.Count);
        var selectedPeaks = peaks.Take(boundaryCount).OrderBy(p => p).ToList();

        var boundaries = new List<double>();
        for (int p = 0; p < selectedPeaks.Count - 1; p++)
        {
            int lo = selectedPeaks[p];
            int hi = selectedPeaks[p + 1];
            int valley = lo;
            double vmin = mag[lo];
            for (int i = lo + 1; i < hi; i++)
            {
                if (mag[i] < vmin)
                {
                    vmin = mag[i];
                    valley = i;
                }
            }
            boundaries.Add((double)valley / fftSize);
        }

        if (boundaries.Count == 0 && maxBands > 1)
        {
            for (int b = 1; b < maxBands; b++)
                boundaries.Add(0.5 * b / maxBands);
        }

        int bandCount = boundaries.Count + 1;
        var bandEdges = new double[bandCount + 1];
        bandEdges[0] = 0;
        for (int b = 0; b < boundaries.Count; b++)
            bandEdges[b + 1] = boundaries[b];
        bandEdges[^1] = 0.5;

        var bands = new T[bandCount][];
        for (int b = 0; b < bandCount; b++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ModeDecompositionProgress
            {
                Algorithm = "EWT",
                CurrentMode = b,
                TotalModes = bandCount,
                Iteration = 0,
                Fraction = (double)(b + 1) / bandCount,
                Message = $"Band {b + 1}",
            });

            var br = (T[])re.Clone();
            var bi = (T[])im.Clone();
            ApplyBandpass(br, bi, fftSize, bandEdges[b], bandEdges[b + 1], gamma, ctx);

            fft.InverseNorm(br, bi, ctx);

            bands[b] = new T[n0];
            br.AsSpan(0, n0).CopyTo(bands[b]);
        }

        var residual = (T[])bands[0].Clone();
        var modes = new List<T[]>(Math.Max(0, bandCount - 1));
        for (int b = bandCount - 1; b >= 1; b--)
            modes.Add(bands[b]);

        var hz = new double[boundaries.Count];
        for (int i = 0; i < boundaries.Count; i++)
            hz[i] = boundaries[i] * sr;

        return new EwtResult<T>
        {
            Modes = modes,
            Residual = residual,
            BoundaryFrequenciesHz = hz,
        };
    }

    private static List<int> FindLocalMaxima(double[] mag, double threshold)
    {
        var peaks = new List<int>();
        for (int i = 1; i < mag.Length - 1; i++)
        {
            if (mag[i] >= threshold && mag[i] > mag[i - 1] && mag[i] >= mag[i + 1])
                peaks.Add(i);
        }
        return peaks;
    }

    private static void ApplyBandpass<T>(
        T[] re, T[] im, int fftSize,
        double fLow, double fHigh, double gamma,
        ComputingContext? ctx)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = fftSize;
        int half = n / 2;
        ComputingContextExecution.ForEach(ctx, 0, n, i =>
        {
            double f = i <= half ? (double)i / n : (double)(n - i) / n;
            T w = T.CreateChecked(RaisedCosineBand(f, fLow, fHigh, gamma));
            re[i] *= w;
            im[i] *= w;
        }, workPerItem: 8);
    }

    private static double RaisedCosineBand(double f, double fLow, double fHigh, double gamma)
    {
        if (f < fLow - gamma || f > fHigh + gamma)
            return 0;

        if (f < fLow)
        {
            double t = (f - (fLow - gamma)) / (2.0 * gamma);
            return 0.5 * (1.0 - Math.Cos(Math.PI * t));
        }

        if (f <= fHigh)
            return 1.0;

        double tu = (f - fHigh) / (2.0 * gamma);
        return 0.5 * (1.0 + Math.Cos(Math.PI * tu));
    }
}
