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
/// From 0.10.18 the ADMM / FFT path stays in <typeparamref name="T"/> via <see cref="Fft{T}"/>.
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
        ModeDecompositionSupport.EnsureFloatOrDouble<T>();

        options ??= new VmdOptions();
        int n0 = signal.Length;
        if (n0 < MinLength)
            throw new ArgumentException($"Signal length must be ≥ {MinLength}.", nameof(signal));

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
            throw new ArgumentException($"Effective even length must be ≥ {MinLength}.", nameof(signal));

        var fEven = new T[evenLen];
        signal.Slice(0, evenLen).CopyTo(fEven);

        var core = DecomposeCore(
            fEven, K, alpha, tau, tol, maxIter, dc, init, options.RandomSeed, ctx,
            cancellationToken, progress);

        var modes = new T[K][];
        for (int k = 0; k < K; k++)
        {
            modes[k] = new T[n0];
            core.Modes[k].AsSpan(0, evenLen).CopyTo(modes[k]);
            if (n0 > evenLen)
                modes[k][n0 - 1] = modes[k][evenLen - 1];
        }

        var residual = new T[n0];
        for (int i = 0; i < n0; i++)
        {
            T sum = T.Zero;
            for (int k = 0; k < K; k++)
                sum += modes[k][i];
            residual[i] = signal[i] - sum;
        }

        var hz = new double[K];
        for (int k = 0; k < K; k++)
            hz[k] = Convert.ToDouble(core.OmegaNorm[k]) * sr;

        var omegaNorm = new double[K];
        for (int k = 0; k < K; k++)
            omegaNorm[k] = Convert.ToDouble(core.OmegaNorm[k]);

        return new VmdResult<T>
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
        public required T[][] Modes { get; init; }
        public required T[] OmegaNorm { get; init; }
        public required int Iterations { get; init; }
        public required bool Converged { get; init; }
    }

    private static CoreResult<T> DecomposeCore<T>(
        T[] fEven,
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
        int len = fEven.Length;
        int half = len / 2;

        var mirrored = new T[2 * len];
        for (int i = 0; i < half; i++)
            mirrored[i] = fEven[half - 1 - i];
        for (int i = 0; i < len; i++)
            mirrored[half + i] = fEven[i];
        for (int i = 0; i < half; i++)
            mirrored[half + len + i] = fEven[len - 1 - i];

        int M = mirrored.Length;
        int fftSize = ModeDecompositionSupport.NextPow2(M);
        var re = new T[fftSize];
        var im = new T[fftSize];
        mirrored.AsSpan().CopyTo(re);

        var fft = new Fft<T>(fftSize);
        fft.Direct(re, im, ctx);

        var freqs = new T[fftSize];
        T invN = T.One / T.CreateChecked(fftSize);
        T halfCycle = T.CreateChecked(0.5);
        for (int i = 0; i < fftSize; i++)
            freqs[i] = T.CreateChecked(i) * invN - halfCycle;

        ModeDecompositionSupport.FftShiftInPlace(re, im);

        var fHatRe = new T[fftSize];
        var fHatIm = new T[fftSize];
        for (int i = 0; i < fftSize; i++)
        {
            if (freqs[i] >= T.Zero)
            {
                fHatRe[i] = re[i];
                fHatIm[i] = im[i];
            }
        }

        var uRe = new T[K][];
        var uIm = new T[K][];
        var uRePrev = new T[K][];
        var uImPrev = new T[K][];
        for (int k = 0; k < K; k++)
        {
            uRe[k] = new T[fftSize];
            uIm[k] = new T[fftSize];
            uRePrev[k] = new T[fftSize];
            uImPrev[k] = new T[fftSize];
        }

        var omega = new T[K];
        InitOmega(omega, K, init, dc, seed);

        var lambdaRe = new T[fftSize];
        var lambdaIm = new T[fftSize];
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

                T om = omega[k];
                ComputingContextExecution.ForEach(ctx, 0, fftSize, i =>
                {
                    if (freqs[i] < T.Zero)
                    {
                        ukRe[i] = T.Zero;
                        ukIm[i] = T.Zero;
                        return;
                    }
                    T numRe = fHatRe[i] - sumRe[i] - lambdaRe[i] * halfT;
                    T numIm = fHatIm[i] - sumIm[i] - lambdaIm[i] * halfT;
                    T df = freqs[i] - om;
                    T den = T.One + two * alpha * df * df;
                    ukRe[i] = numRe / den;
                    ukIm[i] = numIm / den;
                }, workPerItem: 12);

                if (!(dc && k == 0))
                {
                    T num = T.Zero, den = T.Zero;
                    for (int i = 0; i < fftSize; i++)
                    {
                        if (freqs[i] < T.Zero) continue;
                        T p = ukRe[i] * ukRe[i] + ukIm[i] * ukIm[i];
                        num += freqs[i] * p;
                        den += p;
                    }
                    omega[k] = den > eps ? num / den : T.Zero;
                }
                else
                {
                    omega[k] = T.Zero;
                }

                for (int i = 0; i < fftSize; i++)
                {
                    sumRe[i] += ukRe[i];
                    sumIm[i] += ukIm[i];
                }
            }

            if (T.Abs(tau) > eps)
            {
                for (int i = 0; i < fftSize; i++)
                {
                    T sRe = T.Zero, sIm = T.Zero;
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
                    double dRe = Convert.ToDouble(uRe[k][i] - uRePrev[k][i]);
                    double dIm = Convert.ToDouble(uIm[k][i] - uImPrev[k][i]);
                    diff += dRe * dRe + dIm * dIm;
                    double pRe = Convert.ToDouble(uRePrev[k][i]);
                    double pIm = Convert.ToDouble(uImPrev[k][i]);
                    bas += pRe * pRe + pIm * pIm;
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

        var modes = new T[K][];
        for (int k = 0; k < K; k++)
        {
            var mRe = (T[])uRe[k].Clone();
            var mIm = (T[])uIm[k].Clone();
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
            fft.InverseNorm(mRe, mIm, ctx);

            var mode = new T[len];
            int start = half;
            for (int i = 0; i < len; i++)
                mode[i] = mRe[start + i];
            modes[k] = mode;
        }

        var order = Enumerable.Range(0, K).OrderBy(k => Convert.ToDouble(omega[k])).ToArray();
        var sortedModes = new T[K][];
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
