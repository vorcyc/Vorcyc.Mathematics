using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Complete Ensemble EMD with Adaptive Noise (Torres et al., ICASSP 2011).
/// Precomputes EMD of each noise realization, then extracts modes stage-wise:
/// <c>IMF₁ = avg E₁(x + ε₀ wⁱ)</c>,
/// <c>IMFₖ = avg E₁(rₖ₋₁ + εₖ₋₁ Eₖ₋₁(wⁱ))</c> for k ≥ 2,
/// with exact reconstruction <c>x = Σ IMF + r</c>.
/// Ensemble stages honor <see cref="ComputingContext"/>.
/// </summary>
public static class CompleteEnsembleEmd
{
    private const int MinLength = 4;

    /// <summary>Decompose a real signal into CEEMDAN IMFs + residual (Torres 2011).</summary>
    public static EmdResult<T> Decompose<T>(
        ReadOnlySpan<T> signal,
        CeemdanOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Only float and double are supported.");

        options ??= new CeemdanOptions();

        int n = signal.Length;
        if (n < MinLength)
        {
            return new EmdResult<T>
            {
                IntrinsicModeFunctions = Array.Empty<T[]>(),
                Residual = signal.ToArray(),
                StopReason = EmdStopReason.InputRejected,
            };
        }

        int ensembleCount = Math.Clamp(options.EnsembleCount, 2, 500);
        double noiseRatio = options.NoiseRatio;
        if (!(noiseRatio > 0) || double.IsNaN(noiseRatio) || double.IsInfinity(noiseRatio))
            noiseRatio = 0.2;

        int maxImf = Math.Clamp(options.MaxImf, 1, 256);
        if (options.EmdOptions is not null)
            maxImf = Math.Min(maxImf, Math.Clamp(options.EmdOptions.MaxImf, 1, 256));

        var baseEmd = ResolveEmdOptions(options);
        var stageEmd = StageOptions(baseEmd);
        var noiseEmd = NoiseEmdOptions(baseEmd, maxImf);
        int minExtrema = Math.Max(2, baseEmd.MinExtremaToContinue);
        var ctx = baseEmd.ComputingContext;
        int baseSeed = options.RandomSeed ?? Environment.TickCount;

        progress?.Report(new ModeDecompositionProgress
        {
            Algorithm = "CEEMDAN",
            CurrentMode = 0,
            TotalModes = maxImf,
            Iteration = 0,
            Fraction = 0,
            Message = "Precompute noise EMD",
        });

        // Torres: precompute full EMD of each unit-variance white-noise realization.
        var noises = new T[ensembleCount][];
        var noiseImfs = new IReadOnlyList<T[]>[ensembleCount];
        ComputingContextExecution.ForEach(ctx, 0, ensembleCount, trial =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rng = new Random(unchecked(baseSeed + trial * 9973 + 1));
            var w = new T[n];
            FillUnitGaussianNoise(w, rng);
            noises[trial] = w;

            var emd = EmpiricalModeDecomposition.Decompose(w, noiseEmd, cancellationToken);
            noiseImfs[trial] = emd.IntrinsicModeFunctions;
        }, workPerItem: n * 80L);

        var residual = signal.ToArray();
        var imfs = new List<T[]>(capacity: Math.Min(maxImf, 16));
        var maxIdx = new List<int>(n / 4 + 4);
        var minIdx = new List<int>(n / 4 + 4);
        var stop = EmdStopReason.MaxImfReached;

        for (int mode = 0; mode < maxImf; mode++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ModeDecompositionProgress
            {
                Algorithm = "CEEMDAN",
                CurrentMode = mode,
                TotalModes = maxImf,
                Iteration = 0,
                Fraction = (double)mode / maxImf,
                Message = $"IMF {mode + 1}",
            });

            CountExtrema(residual, maxIdx, minIdx);
            if (maxIdx.Count + minIdx.Count < minExtrema)
            {
                stop = EmdStopReason.ResidualTooFewExtrema;
                break;
            }

            // ε_k = noiseRatio · σ(r_k)  (adaptive per Torres / common practice)
            double epsilon = noiseRatio * StdDev(residual);
            var trialFirst = new T[ensembleCount][];

            ComputingContextExecution.ForEach(ctx, 0, ensembleCount, trial =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                T[]? noiseComp = ResolveNoiseComponent(mode, noises[trial], noiseImfs[trial]);
                if (noiseComp is null)
                    return;

                var stageInput = new T[n];
                for (int i = 0; i < n; i++)
                {
                    stageInput[i] = T.CreateChecked(
                        Convert.ToDouble(residual[i]) + epsilon * Convert.ToDouble(noiseComp[i]));
                }

                var stageResult = EmpiricalModeDecomposition.Decompose(
                    stageInput, stageEmd, cancellationToken);
                if (stageResult.IntrinsicModeFunctions.Count == 0)
                    return;

                trialFirst[trial] = stageResult.IntrinsicModeFunctions[0];
            }, workPerItem: n * 60L);

            int successTrials = 0;
            var imfSum = new double[n];
            for (int trial = 0; trial < ensembleCount; trial++)
            {
                var first = trialFirst[trial];
                if (first is null) continue;
                successTrials++;
                for (int i = 0; i < n; i++)
                    imfSum[i] += Convert.ToDouble(first[i]);
            }

            if (successTrials == 0)
            {
                stop = EmdStopReason.ResidualTooFewExtrema;
                break;
            }

            var imf = new T[n];
            double inv = 1.0 / successTrials;
            ComputingContextExecution.ForEach(ctx, 0, n, i =>
            {
                imf[i] = T.CreateChecked(imfSum[i] * inv);
            }, workPerItem: 1);

            imfs.Add(imf);
            SubtractInPlace(residual, imf, ctx);

            CountExtrema(residual, maxIdx, minIdx);
            if (maxIdx.Count + minIdx.Count < minExtrema)
            {
                stop = EmdStopReason.ResidualTooFewExtrema;
                break;
            }

            if (mode == maxImf - 1)
                stop = EmdStopReason.MaxImfReached;
        }

        return new EmdResult<T>
        {
            IntrinsicModeFunctions = imfs,
            Residual = residual,
            StopReason = stop,
        };
    }

    /// <summary>
    /// Torres: mode 0 uses white noise wⁱ; mode k≥1 uses Eₖ(wⁱ) (1-based),
    /// i.e. noise IMF index k−1.
    /// </summary>
    private static T[]? ResolveNoiseComponent<T>(
        int modeZeroBased, T[] whiteNoise, IReadOnlyList<T[]> noiseModes)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (modeZeroBased == 0)
            return whiteNoise;

        int noiseImfIndex = modeZeroBased - 1; // E_k for extracting IMF_{k+1}
        if (noiseImfIndex < noiseModes.Count)
            return noiseModes[noiseImfIndex];

        // Paper/practice: if E_k missing, fall back to last available IMF or white noise.
        if (noiseModes.Count > 0)
            return noiseModes[^1];
        return whiteNoise;
    }

    private static EmdOptions ResolveEmdOptions(CeemdanOptions options)
    {
        var emd = options.EmdOptions ?? new EmdOptions();
        if (emd.ComputingContext is null && options.ComputingContext is not null)
        {
            emd = new EmdOptions
            {
                MaxImf = emd.MaxImf,
                MaxSiftIterations = emd.MaxSiftIterations,
                SiftingTolerance = emd.SiftingTolerance,
                MinExtremaToContinue = emd.MinExtremaToContinue,
                ComputingContext = options.ComputingContext,
            };
        }
        return emd;
    }

    private static EmdOptions StageOptions(EmdOptions baseEmd) => new()
    {
        MaxImf = 1,
        MaxSiftIterations = baseEmd.MaxSiftIterations,
        SiftingTolerance = baseEmd.SiftingTolerance,
        MinExtremaToContinue = baseEmd.MinExtremaToContinue,
        ComputingContext = baseEmd.ComputingContext,
    };

    private static EmdOptions NoiseEmdOptions(EmdOptions baseEmd, int maxImf) => new()
    {
        MaxImf = Math.Max(maxImf, 2),
        MaxSiftIterations = baseEmd.MaxSiftIterations,
        SiftingTolerance = baseEmd.SiftingTolerance,
        MinExtremaToContinue = baseEmd.MinExtremaToContinue,
        ComputingContext = baseEmd.ComputingContext,
    };

    private static void FillUnitGaussianNoise<T>(T[] noise, Random rng)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < noise.Length; i++)
            noise[i] = T.CreateChecked(NextGaussian(rng));

        double std = StdDev(noise);
        if (std < 1e-15) return;
        double inv = 1.0 / std;
        for (int i = 0; i < noise.Length; i++)
            noise[i] = T.CreateChecked(Convert.ToDouble(noise[i]) * inv);
    }

    private static double NextGaussian(Random rng)
    {
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    }

    private static double StdDev<T>(ReadOnlySpan<T> x)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (x.Length == 0) return 0;
        double mean = 0;
        for (int i = 0; i < x.Length; i++)
            mean += Convert.ToDouble(x[i]);
        mean /= x.Length;

        double var = 0;
        for (int i = 0; i < x.Length; i++)
        {
            double d = Convert.ToDouble(x[i]) - mean;
            var += d * d;
        }
        return Math.Sqrt(var / x.Length);
    }

    private static void CountExtrema<T>(Span<T> x, List<int> maxima, List<int> minima)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        maxima.Clear();
        minima.Clear();
        int len = x.Length;
        if (len < 3) return;

        for (int i = 1; i < len - 1; i++)
        {
            T left = x[i - 1], mid = x[i], right = x[i + 1];
            if (mid > left && mid > right)
                maxima.Add(i);
            else if (mid < left && mid < right)
                minima.Add(i);
        }
    }

    private static void SubtractInPlace<T>(T[] residual, T[] imf, ComputingContext? ctx)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int len = residual.Length;
        ComputingContextExecution.ForEach(ctx, 0, len, i =>
        {
            residual[i] -= imf[i];
        }, workPerItem: 2);
    }
}
