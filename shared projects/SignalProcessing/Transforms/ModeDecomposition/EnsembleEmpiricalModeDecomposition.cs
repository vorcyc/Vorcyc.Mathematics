using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Ensemble Empirical Mode Decomposition (Wu &amp; Huang, 2009): average IMFs from
/// multiple noise-augmented EMD trials to reduce mode mixing.
/// </summary>
public static class EnsembleEmpiricalModeDecomposition
{
    private const int MinLength = 4;

    /// <summary>Decompose a real signal into ensemble-averaged IMFs + residual.</summary>
    public static EmdResult<T> Decompose<T>(
        ReadOnlySpan<T> signal,
        EemdOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Only float and double are supported.");

        options ??= new EemdOptions();

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

        var emdOpts = ResolveEmdOptions(options);
        int maxImf = Math.Clamp(emdOpts.MaxImf, 1, 256);

        double signalStd = StdDev(signal);
        double noiseScale = noiseRatio * signalStd;
        var rng = options.RandomSeed is int seed ? new Random(seed) : new Random();

        var signalCopy = signal.ToArray();
        var noisy = new T[n];
        var imfSums = new double[maxImf][];
        for (int k = 0; k < maxImf; k++)
            imfSums[k] = new double[n];

        var trialModeCounts = new int[ensembleCount];

        for (int trial = 0; trial < ensembleCount; trial++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ModeDecompositionProgress
            {
                Algorithm = "EEMD",
                CurrentMode = 0,
                TotalModes = maxImf,
                Iteration = trial,
                Fraction = (double)trial / ensembleCount,
                Message = $"Ensemble trial {trial + 1}/{ensembleCount}",
            });

            for (int i = 0; i < n; i++)
            {
                double noise = NextGaussian(rng) * noiseScale;
                noisy[i] = T.CreateChecked(Convert.ToDouble(signalCopy[i]) + noise);
            }

            var trialResult = EmpiricalModeDecomposition.Decompose(
                noisy, emdOpts, cancellationToken);

            int modes = trialResult.IntrinsicModeFunctions.Count;
            trialModeCounts[trial] = modes;

            for (int k = 0; k < modes && k < maxImf; k++)
            {
                var imf = trialResult.IntrinsicModeFunctions[k];
                for (int i = 0; i < n; i++)
                    imfSums[k][i] += Convert.ToDouble(imf[i]);
            }
        }

        int imfCount = trialModeCounts.Min();
        if (imfCount == 0)
        {
            return new EmdResult<T>
            {
                IntrinsicModeFunctions = Array.Empty<T[]>(),
                Residual = signalCopy,
                StopReason = EmdStopReason.ResidualTooFewExtrema,
            };
        }

        imfCount = Math.Min(imfCount, maxImf);
        var imfs = new List<T[]>(imfCount);
        for (int k = 0; k < imfCount; k++)
        {
            var imf = new T[n];
            double inv = 1.0 / ensembleCount;
            for (int i = 0; i < n; i++)
                imf[i] = T.CreateChecked(imfSums[k][i] * inv);
            imfs.Add(imf);
        }

        var residual = signalCopy;
        var ctx = emdOpts.ComputingContext;
        for (int k = 0; k < imfCount; k++)
            SubtractInPlace(residual, imfs[k], ctx);

        var stop = imfCount >= maxImf ? EmdStopReason.MaxImfReached : EmdStopReason.ResidualTooFewExtrema;

        return new EmdResult<T>
        {
            IntrinsicModeFunctions = imfs,
            Residual = residual,
            StopReason = stop,
        };
    }

    private static EmdOptions ResolveEmdOptions(EemdOptions options)
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

    private static double NextGaussian(Random rng)
    {
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    }

    private static void SubtractInPlace<T>(T[] residual, T[] imf, ComputingContext? ctx)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = residual.Length;
        ComputingContextExecution.ForEach(ctx, 0, n, i =>
        {
            residual[i] -= imf[i];
        }, workPerItem: 2);
    }
}
