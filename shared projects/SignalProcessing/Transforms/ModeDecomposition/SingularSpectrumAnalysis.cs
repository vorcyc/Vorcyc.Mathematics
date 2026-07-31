using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Singular Spectrum Analysis: embed a time series into a trajectory matrix,
/// decompose via SVD, and reconstruct grouped components by diagonal averaging.
/// </summary>
public static class SingularSpectrumAnalysis
{
    private const int MinLength = 4;

    /// <summary>Decompose a real signal into SSA components plus residual.</summary>
    public static SsaResult<T> Decompose<T>(
        ReadOnlySpan<T> signal,
        SsaOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Only float and double are supported.");

        options ??= new SsaOptions();
        int N = signal.Length;
        if (N < MinLength)
            throw new ArgumentException($"Signal length must be ≥ {MinLength}.", nameof(signal));

        int L = options.WindowLength ?? (N / 2);
        L = Math.Clamp(L, 2, N / 2);
        int K = N - L + 1;
        if (K < 2)
            throw new ArgumentException("Window length too large for signal.", nameof(signal));

        int maxRank = Math.Min(L, K);
        int componentCount = options.ComponentCount ?? Math.Min(L, 20);
        componentCount = Math.Clamp(componentCount, 1, maxRank);
        int groupSize = Math.Max(1, options.GroupSize);
        var ctx = options.ComputingContext;

        // Copy once so parallel workers can index without capturing Span.
        var samples = new double[N];
        for (int i = 0; i < N; i++)
            samples[i] = Convert.ToDouble(signal[i]);

        progress?.Report(new ModeDecompositionProgress
        {
            Algorithm = "SSA",
            CurrentMode = 0,
            TotalModes = (componentCount + groupSize - 1) / groupSize,
            Iteration = 0,
            Fraction = 0,
            Message = "Embed",
        });

        var x = new double[L, K];
        ComputingContextExecution.ForEach(ctx, 0, K, j =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int i = 0; i < L; i++)
                x[i, j] = samples[i + j];
        }, workPerItem: L);

        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new ModeDecompositionProgress
        {
            Algorithm = "SSA",
            CurrentMode = 0,
            TotalModes = (componentCount + groupSize - 1) / groupSize,
            Iteration = 0,
            Fraction = 0.05,
            Message = "SVD",
        });

        var trajectory = new Matrix<double>(x);
        var svd = MatrixDecomposition.SingularValueDecomposition(
            trajectory,
            tolerance: null,
            context: ctx,
            cancellationToken: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        int groupCount = (componentCount + groupSize - 1) / groupSize;
        var components = new List<T[]>(groupCount);
        var recon = new double[N];

        for (int g = 0; g < groupCount; g++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int start = g * groupSize;
            int end = Math.Min(start + groupSize, componentCount);

            progress?.Report(new ModeDecompositionProgress
            {
                Algorithm = "SSA",
                CurrentMode = g,
                TotalModes = groupCount,
                Iteration = 0,
                Fraction = 0.1 + 0.9 * (g + 1) / groupCount,
                Message = $"Group {g + 1}",
            });

            var elem = new double[L, K];
            for (int t = start; t < end; t++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                double sigma = svd.SingularValues[t];
                ComputingContextExecution.ForEach(ctx, 0, L, i =>
                {
                    double ui = svd.U[i, t];
                    for (int j = 0; j < K; j++)
                        elem[i, j] += sigma * ui * svd.VT[t, j];
                }, workPerItem: K);
            }

            var series = DiagonalAverage(elem, L, K, ctx, cancellationToken);
            var comp = new T[N];
            ComputingContextExecution.ForEach(ctx, 0, N, i =>
            {
                comp[i] = T.CreateChecked(series[i]);
                // recon is accumulated sequentially after the parallel fill of comp
            }, workPerItem: 1);

            for (int i = 0; i < N; i++)
                recon[i] += series[i];

            components.Add(comp);
        }

        var residual = new T[N];
        ComputingContextExecution.ForEach(ctx, 0, N, i =>
        {
            residual[i] = T.CreateChecked(samples[i] - recon[i]);
        }, workPerItem: 1);

        var singularValues = new double[svd.SingularValues.Length];
        for (int i = 0; i < singularValues.Length; i++)
            singularValues[i] = svd.SingularValues[i];

        return new SsaResult<T>
        {
            Components = components,
            Residual = residual,
            SingularValues = singularValues,
            WindowLength = L,
        };
    }

    private static double[] DiagonalAverage(
        double[,] m, int L, int K, ComputingContext? ctx, CancellationToken ct)
    {
        int N = L + K - 1;
        var result = new double[N];
        var counts = new int[N];

        // Diagonal averaging has races if parallelized over (i,j); keep sequential
        // but honor cancel and optionally parallelize the final divide.
        for (int i = 0; i < L; i++)
        {
            ct.ThrowIfCancellationRequested();
            for (int j = 0; j < K; j++)
            {
                int n = i + j;
                result[n] += m[i, j];
                counts[n]++;
            }
        }

        ComputingContextExecution.ForEach(ctx, 0, N, n =>
        {
            result[n] /= counts[n];
        }, workPerItem: 1);

        return result;
    }
}
