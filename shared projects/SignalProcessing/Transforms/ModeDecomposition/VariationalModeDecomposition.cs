using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Variational Mode Decomposition (Dragomiretskiy &amp; Zosso, 2014): concurrent
/// extraction of K band-limited modes via ADMM in the Fourier domain.
/// </summary>
/// <remarks>
/// The mirrored signal is zero-padded to the next power of two for FFT.
/// Center frequencies are reported both normalized (cycles/sample) and in Hz.
/// </remarks>
public static class VariationalModeDecomposition
{
    private const int MinLength = 8;

    /// <summary>Decompose a real signal into K variational modes.</summary>
    public static VmdResult<T> Decompose<T>(
        ReadOnlySpan<T> signal,
        VmdOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Only float and double are supported.");

        options ??= new VmdOptions();
        int n0 = signal.Length;
        if (n0 < MinLength)
            throw new ArgumentException($"Signal length must be ≥ {MinLength}.", nameof(signal));

        int K = Math.Clamp(options.ModeCount, 1, 64);
        double alpha = options.Alpha > 0 ? options.Alpha : 2000;
        double tau = options.Tau;
        if (double.IsNaN(tau) || double.IsInfinity(tau)) tau = 0;
        double tol = options.Tolerance > 0 ? options.Tolerance : 1e-7;
        int maxIter = Math.Clamp(options.MaxIterations, 1, 10_000);
        bool dc = options.DcMode;
        int init = Math.Clamp(options.OmegaInit, 0, 2);
        float sr = options.SamplingRate > 0 ? options.SamplingRate : 1f;
        var ctx = options.ComputingContext;

        var f0 = new double[n0];
        for (int i = 0; i < n0; i++)
            f0[i] = Convert.ToDouble(signal[i]);

        int evenLen = n0 - (n0 & 1);
        if (evenLen < MinLength)
            throw new ArgumentException($"Effective even length must be ≥ {MinLength}.", nameof(signal));

        var core = DecomposeCore(
            f0.AsSpan(0, evenLen), K, alpha, tau, tol, maxIter, dc, init, options.RandomSeed, ctx,
            cancellationToken, progress);

        var modes = new T[K][];
        for (int k = 0; k < K; k++)
        {
            modes[k] = new T[n0];
            for (int i = 0; i < evenLen; i++)
                modes[k][i] = T.CreateChecked(core.Modes[k][i]);
            if (n0 > evenLen)
                modes[k][n0 - 1] = modes[k][evenLen - 1];
        }

        var residual = new T[n0];
        for (int i = 0; i < n0; i++)
        {
            double sum = 0;
            for (int k = 0; k < K; k++)
                sum += Convert.ToDouble(modes[k][i]);
            residual[i] = T.CreateChecked(Convert.ToDouble(signal[i]) - sum);
        }

        var hz = new double[K];
        for (int k = 0; k < K; k++)
            hz[k] = core.OmegaNorm[k] * sr;

        return new VmdResult<T>
        {
            Modes = modes,
            Residual = residual,
            CenterFrequenciesHz = hz,
            CenterFrequenciesNormalized = core.OmegaNorm,
            Iterations = core.Iterations,
            Converged = core.Converged,
        };
    }

    private readonly struct CoreResult
    {
        public required double[][] Modes { get; init; }
        public required double[] OmegaNorm { get; init; }
        public required int Iterations { get; init; }
        public required bool Converged { get; init; }
    }

    private static CoreResult DecomposeCore(
        ReadOnlySpan<double> fEven,
        int K,
        double alpha,
        double tau,
        double tol,
        int maxIter,
        bool dc,
        int init,
        int? seed,
        ComputingContext? ctx,
        CancellationToken cancellationToken,
        IProgress<ModeDecompositionProgress>? progress)
    {
        int T = fEven.Length;
        int half = T / 2;

        var mirrored = new double[2 * T];
        for (int i = 0; i < half; i++)
            mirrored[i] = fEven[half - 1 - i];
        for (int i = 0; i < T; i++)
            mirrored[half + i] = fEven[i];
        for (int i = 0; i < half; i++)
            mirrored[half + T + i] = fEven[T - 1 - i];

        int M = mirrored.Length;
        int fftSize = NextPow2(M);
        var re = new double[fftSize];
        var im = new double[fftSize];
        mirrored.AsSpan().CopyTo(re);

        var fft = new Fft64(fftSize);
        fft.Direct(re, im, ctx);

        var freqs = new double[fftSize];
        for (int i = 0; i < fftSize; i++)
            freqs[i] = (double)i / fftSize - 0.5;

        FftShiftInPlace(re, im);

        var fHatRe = new double[fftSize];
        var fHatIm = new double[fftSize];
        for (int i = 0; i < fftSize; i++)
        {
            if (freqs[i] >= 0)
            {
                fHatRe[i] = re[i];
                fHatIm[i] = im[i];
            }
        }

        var uRe = new double[K][];
        var uIm = new double[K][];
        var uRePrev = new double[K][];
        var uImPrev = new double[K][];
        for (int k = 0; k < K; k++)
        {
            uRe[k] = new double[fftSize];
            uIm[k] = new double[fftSize];
            uRePrev[k] = new double[fftSize];
            uImPrev[k] = new double[fftSize];
        }

        var omega = new double[K];
        InitOmega(omega, K, init, dc, seed);

        var lambdaRe = new double[fftSize];
        var lambdaIm = new double[fftSize];
        var sumRe = new double[fftSize];
        var sumIm = new double[fftSize];

        bool converged = false;
        int nIter;
        double eps = double.Epsilon;

        for (nIter = 0; nIter < maxIter; nIter++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((nIter & 7) == 0)
            {
                progress?.Report(new ModeDecompositionProgress
                {
                    Algorithm = "VMD",
                    CurrentMode = 0,
                    TotalModes = K,
                    Iteration = nIter,
                    Fraction = (double)nIter / maxIter,
                    Message = $"ADMM iter {nIter}",
                });
            }

            for (int k = 0; k < K; k++)
            {
                Array.Copy(uRe[k], uRePrev[k], fftSize);
                Array.Copy(uIm[k], uImPrev[k], fftSize);
            }

            Array.Clear(sumRe);
            Array.Clear(sumIm);
            for (int k = 0; k < K; k++)
            {
                var ur = uRe[k];
                var ui = uIm[k];
                for (int i = 0; i < fftSize; i++)
                {
                    sumRe[i] += ur[i];
                    sumIm[i] += ui[i];
                }
            }

            for (int k = 0; k < K; k++)
            {
                var ukRe = uRe[k];
                var ukIm = uIm[k];
                for (int i = 0; i < fftSize; i++)
                {
                    sumRe[i] -= ukRe[i];
                    sumIm[i] -= ukIm[i];
                }

                double om = omega[k];
                ComputingContextExecution.ForEach(ctx, 0, fftSize, i =>
                {
                    if (freqs[i] < 0)
                    {
                        ukRe[i] = 0;
                        ukIm[i] = 0;
                        return;
                    }
                    double numRe = fHatRe[i] - sumRe[i] - lambdaRe[i] * 0.5;
                    double numIm = fHatIm[i] - sumIm[i] - lambdaIm[i] * 0.5;
                    double den = 1.0 + 2.0 * alpha * (freqs[i] - om) * (freqs[i] - om);
                    ukRe[i] = numRe / den;
                    ukIm[i] = numIm / den;
                }, workPerItem: 12);

                if (!(dc && k == 0))
                {
                    double num = 0, den = 0;
                    for (int i = 0; i < fftSize; i++)
                    {
                        if (freqs[i] < 0) continue;
                        double p = ukRe[i] * ukRe[i] + ukIm[i] * ukIm[i];
                        num += freqs[i] * p;
                        den += p;
                    }
                    omega[k] = den > eps ? num / den : 0;
                }
                else
                {
                    omega[k] = 0;
                }

                for (int i = 0; i < fftSize; i++)
                {
                    sumRe[i] += ukRe[i];
                    sumIm[i] += ukIm[i];
                }
            }

            if (Math.Abs(tau) > eps)
            {
                for (int i = 0; i < fftSize; i++)
                {
                    double sRe = 0, sIm = 0;
                    for (int k = 0; k < K; k++)
                    {
                        sRe += uRe[k][i];
                        sIm += uIm[k][i];
                    }
                    lambdaRe[i] += tau * (fHatRe[i] - sRe);
                    lambdaIm[i] += tau * (fHatIm[i] - sIm);
                }
            }

            double diff = 0, bas = 0;
            for (int k = 0; k < K; k++)
            {
                for (int i = 0; i < fftSize; i++)
                {
                    double dRe = uRe[k][i] - uRePrev[k][i];
                    double dIm = uIm[k][i] - uImPrev[k][i];
                    diff += dRe * dRe + dIm * dIm;
                    bas += uRePrev[k][i] * uRePrev[k][i] + uImPrev[k][i] * uImPrev[k][i];
                }
            }
            if (bas < eps) bas = eps;
            if (diff / bas < tol)
            {
                converged = true;
                nIter++;
                break;
            }
        }

        // Hermitian completion + ifftshift + IFFT → time modes; crop mirror center.
        var modes = new double[K][];
        for (int k = 0; k < K; k++)
        {
            var mRe = (double[])uRe[k].Clone();
            var mIm = (double[])uIm[k].Clone();
            for (int i = 1; i < fftSize; i++)
            {
                if (freqs[i] < 0)
                {
                    int pos = fftSize - i;
                    mRe[i] = mRe[pos];
                    mIm[i] = -mIm[pos];
                }
            }

            IfftShiftInPlace(mRe, mIm);
            fft.InverseNorm(mRe, mIm, ctx);

            var mode = new double[T];
            int start = half;
            for (int i = 0; i < T; i++)
                mode[i] = mRe[start + i];
            modes[k] = mode;
        }

        // Sort modes by ascending center frequency for stable API
        var order = Enumerable.Range(0, K).OrderBy(k => omega[k]).ToArray();
        var sortedModes = new double[K][];
        var sortedOmega = new double[K];
        for (int i = 0; i < K; i++)
        {
            sortedModes[i] = modes[order[i]];
            sortedOmega[i] = Math.Abs(omega[order[i]]); // cycles/sample in [0,0.5]
        }

        return new CoreResult
        {
            Modes = sortedModes,
            OmegaNorm = sortedOmega,
            Iterations = nIter,
            Converged = converged,
        };
    }

    private static void InitOmega(double[] omega, int K, int init, bool dc, int? seed)
    {
        if (init == 0)
        {
            Array.Clear(omega);
            return;
        }
        if (init == 2)
        {
            var rng = seed is int s ? new Random(s) : new Random();
            for (int k = 0; k < K; k++)
                omega[k] = rng.NextDouble() * 0.5;
            if (dc) omega[0] = 0;
            Array.Sort(omega);
            return;
        }
        for (int k = 0; k < K; k++)
            omega[k] = (0.5 / K) * k;
        if (dc) omega[0] = 0;
    }

    private static void FftShiftInPlace(double[] re, double[] im)
    {
        int n = re.Length;
        int half = n / 2;
        for (int i = 0; i < half; i++)
        {
            int j = i + half;
            (re[i], re[j]) = (re[j], re[i]);
            (im[i], im[j]) = (im[j], im[i]);
        }
    }

    private static void IfftShiftInPlace(double[] re, double[] im)
        => FftShiftInPlace(re, im); // even length: ifftshift == fftshift

    private static int NextPow2(int n)
    {
        if (n <= 1) return 1;
        int p = 1;
        while (p < n) p <<= 1;
        return p;
    }
}
