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
/// From 0.10.18 the ADMM / FFT path stays in <typeparamref name="T"/> via <see cref="Fft{T}"/>.
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
        ModeDecompositionSupport.EnsureFloatOrDouble<T>();

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
        T alpha = T.CreateChecked(options.Alpha > 0 ? options.Alpha : 2000);
        double tauD = options.Tau;
        if (double.IsNaN(tauD) || double.IsInfinity(tauD)) tauD = 0;
        T tau = T.CreateChecked(tauD);
        double tol = options.Tolerance > 0 ? options.Tolerance : 1e-7;
        int maxIter = Math.Clamp(options.MaxIterations, 1, 10_000);
        bool dc = options.DcMode;
        int init = Math.Clamp(options.OmegaInit, 0, 2);
        float sr = options.SamplingRate > 0 ? options.SamplingRate : 1f;
        var ctx = options.ComputingContext;

        int evenLen = n0 - (n0 & 1);
        if (evenLen < MinLength)
            throw new ArgumentException($"Effective even length must be ≥ {MinLength}.", nameof(channels));

        var f0 = new T[C][];
        for (int c = 0; c < C; c++)
        {
            f0[c] = new T[evenLen];
            channels[c].AsSpan(0, evenLen).CopyTo(f0[c]);
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
                core.Modes[k][c].AsSpan(0, evenLen).CopyTo(modes[k][c]);
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
                T sum = T.Zero;
                for (int k = 0; k < K; k++)
                    sum += modes[k][c][i];
                residual[c][i] = channels[c][i] - sum;
            }
        }

        var hz = new double[K];
        var omegaNorm = new double[K];
        for (int k = 0; k < K; k++)
        {
            omegaNorm[k] = Convert.ToDouble(core.OmegaNorm[k]);
            hz[k] = omegaNorm[k] * sr;
        }

        return new MvmdResult<T>
        {
            Modes = modes,
            Residual = residual,
            CenterFrequenciesHz = hz,
            CenterFrequenciesNormalized = omegaNorm,
            Iterations = core.Iterations,
            Converged = core.Converged,
        };
    }

    private readonly struct CoreResult<T>
        where T : unmanaged
    {
        public required T[][][] Modes { get; init; }
        public required T[] OmegaNorm { get; init; }
        public required int Iterations { get; init; }
        public required bool Converged { get; init; }
    }

    private static CoreResult<T> DecomposeCore<T>(
        T[][] fEven,
        int K,
        T alpha,
        T tau,
        double tol,
        int maxIter,
        bool dc,
        int init,
        int? seed,
        ComputingContext? ctx,
        CancellationToken cancellationToken,
        IProgress<ModeDecompositionProgress>? progress)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int C = fEven.Length;
        int len = fEven[0].Length;
        int half = len / 2;

        var fHatRe = new T[C][];
        var fHatIm = new T[C][];
        int fftSize = 0;
        T[] freqs = Array.Empty<T>();
        Fft<T>? fft = null;

        T invN = T.Zero;
        T halfCycle = T.CreateChecked(0.5);

        for (int c = 0; c < C; c++)
        {
            var mirrored = new T[2 * len];
            for (int i = 0; i < half; i++)
                mirrored[i] = fEven[c][half - 1 - i];
            for (int i = 0; i < len; i++)
                mirrored[half + i] = fEven[c][i];
            for (int i = 0; i < half; i++)
                mirrored[half + len + i] = fEven[c][len - 1 - i];

            int M = mirrored.Length;
            fftSize = ModeDecompositionSupport.NextPow2(M);
            var re = new T[fftSize];
            var im = new T[fftSize];
            mirrored.AsSpan().CopyTo(re);

            fft ??= new Fft<T>(fftSize);
            fft.Direct(re, im, ctx);

            if (c == 0)
            {
                invN = T.One / T.CreateChecked(fftSize);
                freqs = new T[fftSize];
                for (int i = 0; i < fftSize; i++)
                    freqs[i] = T.CreateChecked(i) * invN - halfCycle;
            }

            ModeDecompositionSupport.FftShiftInPlace(re, im);

            fHatRe[c] = new T[fftSize];
            fHatIm[c] = new T[fftSize];
            for (int i = 0; i < fftSize; i++)
            {
                if (freqs[i] >= T.Zero)
                {
                    fHatRe[c][i] = re[i];
                    fHatIm[c][i] = im[i];
                }
            }
        }

        var uRe = new T[C][][];
        var uIm = new T[C][][];
        var uRePrev = new T[C][][];
        var uImPrev = new T[C][][];
        for (int c = 0; c < C; c++)
        {
            uRe[c] = new T[K][];
            uIm[c] = new T[K][];
            uRePrev[c] = new T[K][];
            uImPrev[c] = new T[K][];
            for (int k = 0; k < K; k++)
            {
                uRe[c][k] = new T[fftSize];
                uIm[c][k] = new T[fftSize];
                uRePrev[c][k] = new T[fftSize];
                uImPrev[c][k] = new T[fftSize];
            }
        }

        var omega = new T[K];
        InitOmega(omega, K, init, dc, seed);

        var lambdaRe = new T[C][];
        var lambdaIm = new T[C][];
        for (int c = 0; c < C; c++)
        {
            lambdaRe[c] = new T[fftSize];
            lambdaIm[c] = new T[fftSize];
        }

        var sumRe = new T[fftSize];
        var sumIm = new T[fftSize];

        bool converged = false;
        int nIter;
        T halfT = T.CreateChecked(0.5);
        T two = T.CreateChecked(2);
        T eps = T.CreateChecked(double.Epsilon);

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
                T om = omega[k];

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
                        if (freqs[i] < T.Zero)
                        {
                            ukRe[i] = T.Zero;
                            ukIm[i] = T.Zero;
                            return;
                        }
                        T numRe = fHatRe[c][i] - sumRe[i] - lambdaRe[c][i] * halfT;
                        T numIm = fHatIm[c][i] - sumIm[i] - lambdaIm[c][i] * halfT;
                        T df = freqs[i] - om;
                        T den = T.One + two * alpha * df * df;
                        ukRe[i] = numRe / den;
                        ukIm[i] = numIm / den;
                    }, workPerItem: 12);
                }

                if (!(dc && k == 0))
                {
                    T num = T.Zero, den = T.Zero;
                    for (int c = 0; c < C; c++)
                    {
                        var ukRe = uRe[c][k];
                        var ukIm = uIm[c][k];
                        for (int i = 0; i < fftSize; i++)
                        {
                            if (freqs[i] < T.Zero) continue;
                            T p = ukRe[i] * ukRe[i] + ukIm[i] * ukIm[i];
                            num += freqs[i] * p;
                            den += p;
                        }
                    }
                    omega[k] = den > eps ? num / den : T.Zero;
                }
                else
                {
                    omega[k] = T.Zero;
                }
            }

            if (T.Abs(tau) > eps)
            {
                for (int c = 0; c < C; c++)
                {
                    for (int i = 0; i < fftSize; i++)
                    {
                        T sRe = T.Zero, sIm = T.Zero;
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
                        double dRe = Convert.ToDouble(uRe[c][k][i] - uRePrev[c][k][i]);
                        double dIm = Convert.ToDouble(uIm[c][k][i] - uImPrev[c][k][i]);
                        diff += dRe * dRe + dIm * dIm;
                        double pRe = Convert.ToDouble(uRePrev[c][k][i]);
                        double pIm = Convert.ToDouble(uImPrev[c][k][i]);
                        bas += pRe * pRe + pIm * pIm;
                    }
                }
            }
            if (bas < 1e-300) bas = 1e-300;
            if (diff / bas < tol)
            {
                converged = true;
                nIter++;
                break;
            }
        }

        var modes = new T[K][][];
        for (int k = 0; k < K; k++)
        {
            modes[k] = new T[C][];
            for (int c = 0; c < C; c++)
            {
                var mRe = (T[])uRe[c][k].Clone();
                var mIm = (T[])uIm[c][k].Clone();
                for (int i = 1; i < fftSize; i++)
                {
                    if (freqs[i] < T.Zero)
                    {
                        int pos = fftSize - i;
                        mRe[i] = mRe[pos];
                        mIm[i] = -mIm[pos];
                    }
                }

                ModeDecompositionSupport.FftShiftInPlace(mRe, mIm);
                fft!.InverseNorm(mRe, mIm, ctx);

                var mode = new T[len];
                int start = half;
                for (int i = 0; i < len; i++)
                    mode[i] = mRe[start + i];
                modes[k][c] = mode;
            }
        }

        var order = Enumerable.Range(0, K).OrderBy(k => Convert.ToDouble(omega[k])).ToArray();
        var sortedModes = new T[K][][];
        var sortedOmega = new T[K];
        for (int i = 0; i < K; i++)
        {
            sortedModes[i] = modes[order[i]];
            sortedOmega[i] = T.Abs(omega[order[i]]);
        }

        return new CoreResult<T>
        {
            Modes = sortedModes,
            OmegaNorm = sortedOmega,
            Iterations = nIter,
            Converged = converged,
        };
    }

    private static void InitOmega<T>(T[] omega, int K, int init, bool dc, int? seed)
        where T : unmanaged, IFloatingPointIeee754<T>
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
                omega[k] = T.CreateChecked(rng.NextDouble() * 0.5);
            if (dc) omega[0] = T.Zero;
            Array.Sort(omega);
            return;
        }
        T step = T.CreateChecked(0.5 / K);
        for (int k = 0; k < K; k++)
            omega[k] = step * T.CreateChecked(k);
        if (dc) omega[0] = T.Zero;
    }
}
