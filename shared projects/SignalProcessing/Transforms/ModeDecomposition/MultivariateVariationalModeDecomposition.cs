using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Multivariate Variational Mode Decomposition: coupled ADMM extraction of K shared
/// band-limited modes across C channels (simplified coupled MVMD).
/// </summary>
/// <remarks>
/// Center frequencies ω<sub>k</sub> are shared across channels; each channel c has modes
/// u<sub>c,k</sub>. ω<sub>k</sub> is updated from the summed power spectrum over channels.
/// </remarks>
public static class MultivariateVariationalModeDecomposition
{
    private const int MinLength = 8;

    /// <summary>Decompose C aligned channels into K shared variational modes.</summary>
    public static MvmdResult<T> Decompose<T>(
        IReadOnlyList<T[]> channels,
        MvmdOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Only float and double are supported.");

        if (channels is null || channels.Count == 0)
            throw new ArgumentException("At least one channel is required.", nameof(channels));

        options ??= new MvmdOptions();
        int C = channels.Count;
        int n0 = channels[0].Length;
        for (int c = 1; c < C; c++)
        {
            if (channels[c] is null)
                throw new ArgumentException($"Channel {c} is null.", nameof(channels));
            if (channels[c].Length != n0)
                throw new ArgumentException("All channels must have equal length.", nameof(channels));
        }

        if (n0 < MinLength)
            throw new ArgumentException($"Signal length must be ≥ {MinLength}.", nameof(channels));

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

        int evenLen = n0 - (n0 & 1);
        if (evenLen < MinLength)
            throw new ArgumentException($"Effective even length must be ≥ {MinLength}.", nameof(channels));

        var f0 = new double[C][];
        for (int c = 0; c < C; c++)
        {
            f0[c] = new double[evenLen];
            for (int i = 0; i < evenLen; i++)
                f0[c][i] = Convert.ToDouble(channels[c][i]);
        }

        var core = DecomposeCore(
            f0, K, alpha, tau, tol, maxIter, dc, init, options.RandomSeed, ctx,
            cancellationToken, progress);

        var modes = new T[K][][];
        for (int k = 0; k < K; k++)
        {
            modes[k] = new T[C][];
            for (int c = 0; c < C; c++)
            {
                modes[k][c] = new T[n0];
                for (int i = 0; i < evenLen; i++)
                    modes[k][c][i] = T.CreateChecked(core.Modes[k][c][i]);
                if (n0 > evenLen)
                    modes[k][c][n0 - 1] = modes[k][c][evenLen - 1];
            }
        }

        var residual = new T[C][];
        for (int c = 0; c < C; c++)
        {
            residual[c] = new T[n0];
            for (int i = 0; i < n0; i++)
            {
                double sum = 0;
                for (int k = 0; k < K; k++)
                    sum += Convert.ToDouble(modes[k][c][i]);
                residual[c][i] = T.CreateChecked(Convert.ToDouble(channels[c][i]) - sum);
            }
        }

        var hz = new double[K];
        for (int k = 0; k < K; k++)
            hz[k] = core.OmegaNorm[k] * sr;

        return new MvmdResult<T>
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
        public required double[][][] Modes { get; init; }
        public required double[] OmegaNorm { get; init; }
        public required int Iterations { get; init; }
        public required bool Converged { get; init; }
    }

    private static CoreResult DecomposeCore(
        double[][] fEven,
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
        int C = fEven.Length;
        int T = fEven[0].Length;
        int half = T / 2;

        var fHatRe = new double[C][];
        var fHatIm = new double[C][];
        int fftSize = 0;
        double[] freqs = Array.Empty<double>();

        for (int c = 0; c < C; c++)
        {
            var mirrored = new double[2 * T];
            for (int i = 0; i < half; i++)
                mirrored[i] = fEven[c][half - 1 - i];
            for (int i = 0; i < T; i++)
                mirrored[half + i] = fEven[c][i];
            for (int i = 0; i < half; i++)
                mirrored[half + T + i] = fEven[c][T - 1 - i];

            int M = mirrored.Length;
            fftSize = NextPow2(M);
            var re = new double[fftSize];
            var im = new double[fftSize];
            mirrored.AsSpan().CopyTo(re);

            var fft = new Fft64(fftSize);
            fft.Direct(re, im, ctx);

            if (c == 0)
            {
                freqs = new double[fftSize];
                for (int i = 0; i < fftSize; i++)
                    freqs[i] = (double)i / fftSize - 0.5;
            }

            FftShiftInPlace(re, im);

            fHatRe[c] = new double[fftSize];
            fHatIm[c] = new double[fftSize];
            for (int i = 0; i < fftSize; i++)
            {
                if (freqs[i] >= 0)
                {
                    fHatRe[c][i] = re[i];
                    fHatIm[c][i] = im[i];
                }
            }
        }

        var uRe = new double[C][][];
        var uIm = new double[C][][];
        var uRePrev = new double[C][][];
        var uImPrev = new double[C][][];
        for (int c = 0; c < C; c++)
        {
            uRe[c] = new double[K][];
            uIm[c] = new double[K][];
            uRePrev[c] = new double[K][];
            uImPrev[c] = new double[K][];
            for (int k = 0; k < K; k++)
            {
                uRe[c][k] = new double[fftSize];
                uIm[c][k] = new double[fftSize];
                uRePrev[c][k] = new double[fftSize];
                uImPrev[c][k] = new double[fftSize];
            }
        }

        var omega = new double[K];
        InitOmega(omega, K, init, dc, seed);

        var lambdaRe = new double[C][];
        var lambdaIm = new double[C][];
        for (int c = 0; c < C; c++)
        {
            lambdaRe[c] = new double[fftSize];
            lambdaIm[c] = new double[fftSize];
        }

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
                    Algorithm = "MVMD",
                    CurrentMode = 0,
                    TotalModes = K,
                    Iteration = nIter,
                    Fraction = (double)nIter / maxIter,
                    Message = $"ADMM iter {nIter}",
                });
            }

            for (int c = 0; c < C; c++)
            {
                for (int k = 0; k < K; k++)
                {
                    Array.Copy(uRe[c][k], uRePrev[c][k], fftSize);
                    Array.Copy(uIm[c][k], uImPrev[c][k], fftSize);
                }
            }

            for (int k = 0; k < K; k++)
            {
                double om = omega[k];

                for (int c = 0; c < C; c++)
                {
                    Array.Clear(sumRe);
                    Array.Clear(sumIm);
                    for (int j = 0; j < K; j++)
                    {
                        var ur = uRe[c][j];
                        var ui = uIm[c][j];
                        for (int i = 0; i < fftSize; i++)
                        {
                            sumRe[i] += ur[i];
                            sumIm[i] += ui[i];
                        }
                    }

                    var ukRe = uRe[c][k];
                    var ukIm = uIm[c][k];
                    for (int i = 0; i < fftSize; i++)
                    {
                        sumRe[i] -= ukRe[i];
                        sumIm[i] -= ukIm[i];
                    }

                    ComputingContextExecution.ForEach(ctx, 0, fftSize, i =>
                    {
                        if (freqs[i] < 0)
                        {
                            ukRe[i] = 0;
                            ukIm[i] = 0;
                            return;
                        }
                        double numRe = fHatRe[c][i] - sumRe[i] - lambdaRe[c][i] * 0.5;
                        double numIm = fHatIm[c][i] - sumIm[i] - lambdaIm[c][i] * 0.5;
                        double den = 1.0 + 2.0 * alpha * (freqs[i] - om) * (freqs[i] - om);
                        ukRe[i] = numRe / den;
                        ukIm[i] = numIm / den;
                    }, workPerItem: 12);
                }

                if (!(dc && k == 0))
                {
                    double num = 0, den = 0;
                    for (int c = 0; c < C; c++)
                    {
                        var ukRe = uRe[c][k];
                        var ukIm = uIm[c][k];
                        for (int i = 0; i < fftSize; i++)
                        {
                            if (freqs[i] < 0) continue;
                            double p = ukRe[i] * ukRe[i] + ukIm[i] * ukIm[i];
                            num += freqs[i] * p;
                            den += p;
                        }
                    }
                    omega[k] = den > eps ? num / den : 0;
                }
                else
                {
                    omega[k] = 0;
                }
            }

            if (Math.Abs(tau) > eps)
            {
                for (int c = 0; c < C; c++)
                {
                    for (int i = 0; i < fftSize; i++)
                    {
                        double sRe = 0, sIm = 0;
                        for (int k = 0; k < K; k++)
                        {
                            sRe += uRe[c][k][i];
                            sIm += uIm[c][k][i];
                        }
                        lambdaRe[c][i] += tau * (fHatRe[c][i] - sRe);
                        lambdaIm[c][i] += tau * (fHatIm[c][i] - sIm);
                    }
                }
            }

            double diff = 0, bas = 0;
            for (int c = 0; c < C; c++)
            {
                for (int k = 0; k < K; k++)
                {
                    for (int i = 0; i < fftSize; i++)
                    {
                        double dRe = uRe[c][k][i] - uRePrev[c][k][i];
                        double dIm = uIm[c][k][i] - uImPrev[c][k][i];
                        diff += dRe * dRe + dIm * dIm;
                        bas += uRePrev[c][k][i] * uRePrev[c][k][i] + uImPrev[c][k][i] * uImPrev[c][k][i];
                    }
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

        var fftOut = new Fft64(fftSize);
        var modes = new double[K][][];
        for (int k = 0; k < K; k++)
        {
            modes[k] = new double[C][];
            for (int c = 0; c < C; c++)
            {
                var mRe = (double[])uRe[c][k].Clone();
                var mIm = (double[])uIm[c][k].Clone();
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
                fftOut.InverseNorm(mRe, mIm, ctx);

                var mode = new double[T];
                int start = half;
                for (int i = 0; i < T; i++)
                    mode[i] = mRe[start + i];
                modes[k][c] = mode;
            }
        }

        var order = Enumerable.Range(0, K).OrderBy(k => omega[k]).ToArray();
        var sortedModes = new double[K][][];
        var sortedOmega = new double[K];
        for (int i = 0; i < K; i++)
        {
            sortedModes[i] = modes[order[i]];
            sortedOmega[i] = Math.Abs(omega[order[i]]);
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
        int h = n / 2;
        for (int i = 0; i < h; i++)
        {
            int j = i + h;
            (re[i], re[j]) = (re[j], re[i]);
            (im[i], im[j]) = (im[j], im[i]);
        }
    }

    private static void IfftShiftInPlace(double[] re, double[] im)
        => FftShiftInPlace(re, im);

    private static int NextPow2(int n)
    {
        if (n <= 1) return 1;
        int p = 1;
        while (p < n) p <<= 1;
        return p;
    }
}
