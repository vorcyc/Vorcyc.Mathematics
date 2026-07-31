using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Multivariate Empirical Mode Decomposition (Rehman &amp; Mandic): extract
/// multichannel IMFs by averaging envelope estimates from multiple projection directions.
/// </summary>
public static class MultivariateEmpiricalModeDecomposition
{
    private const int MinLength = 4;

    /// <summary>Decompose equal-length multichannel data into IMFs + per-channel residual.</summary>
    public static MemdResult<T> Decompose<T>(
        IReadOnlyList<T[]> channels,
        MemdOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Only float and double are supported.");

        if (channels is null || channels.Count < 2)
            throw new ArgumentException("At least two channels are required.", nameof(channels));

        options ??= new MemdOptions();

        int channelCount = channels.Count;
        int n = channels[0].Length;
        for (int c = 1; c < channelCount; c++)
        {
            if (channels[c] is null || channels[c].Length != n)
                throw new ArgumentException("All channels must have the same length.", nameof(channels));
        }

        if (n < MinLength)
        {
            return new MemdResult<T>
            {
                IntrinsicModeFunctions = Array.Empty<T[][]>(),
                Residual = CloneChannels(channels),
                StopReason = EmdStopReason.InputRejected,
            };
        }

        int maxImf = Math.Clamp(options.MaxImf, 1, 256);
        int maxSift = Math.Clamp(options.MaxSiftIterations, 1, 10_000);
        double tol = options.SiftingTolerance;
        if (!(tol > 0) || double.IsNaN(tol) || double.IsInfinity(tol))
            tol = 0.2;
        int directionCount = Math.Clamp(options.DirectionCount, 4, 4096);
        int minExtrema = Math.Max(2, options.MinExtremaToContinue);
        var ctx = options.ComputingContext;

        var directions = GenerateDirections(channelCount, directionCount);
        var residual = CloneChannels(channels);
        var imfs = new List<T[][]>(capacity: Math.Min(maxImf, 16));

        var work = new T[channelCount][];
        var prev = new T[channelCount][];
        for (int c = 0; c < channelCount; c++)
        {
            work[c] = new T[n];
            prev[c] = new T[n];
        }

        var projection = new double[n];
        var maxIdx = new List<int>(n / 4 + 4);
        var minIdx = new List<int>(n / 4 + 4);
        var upper = new T[n];
        var lower = new T[n];
        var meanUpper = new T[channelCount][];
        var meanLower = new T[channelCount][];
        for (int c = 0; c < channelCount; c++)
        {
            meanUpper[c] = new T[n];
            meanLower[c] = new T[n];
        }

        var stop = EmdStopReason.MaxImfReached;

        for (int mode = 0; mode < maxImf; mode++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ModeDecompositionProgress
            {
                Algorithm = "MEMD",
                CurrentMode = mode,
                TotalModes = maxImf,
                Iteration = 0,
                Fraction = (double)mode / maxImf,
                Message = $"IMF {mode + 1}",
            });

            if (!AnyChannelHasEnoughExtrema(residual, directions, projection, maxIdx, minIdx, minExtrema))
            {
                stop = EmdStopReason.ResidualTooFewExtrema;
                break;
            }

            CopyChannels(residual, work);

            for (int iter = 0; iter < maxSift; iter++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                CopyChannels(work, prev);

                for (int c = 0; c < channelCount; c++)
                {
                    meanUpper[c].AsSpan().Clear();
                    meanLower[c].AsSpan().Clear();
                }

                int validDirections = 0;
                for (int d = 0; d < directions.Length; d++)
                {
                    Project(work, directions[d], projection);
                    CountExtrema(projection, maxIdx, minIdx);
                    if (maxIdx.Count < 2 || minIdx.Count < 2)
                        continue;

                    bool dirOk = true;
                    for (int c = 0; c < channelCount; c++)
                    {
                        if (!TryBuildEnvelope(work[c], maxIdx, upper) ||
                            !TryBuildEnvelope(work[c], minIdx, lower))
                        {
                            dirOk = false;
                            break;
                        }

                        Accumulate(meanUpper[c], upper, ctx);
                        Accumulate(meanLower[c], lower, ctx);
                    }

                    if (dirOk)
                        validDirections++;
                }

                if (validDirections == 0)
                    break;

                double invDirs = 1.0 / validDirections;
                for (int c = 0; c < channelCount; c++)
                {
                    ScaleInPlace(meanUpper[c], invDirs, ctx);
                    ScaleInPlace(meanLower[c], invDirs, ctx);
                }
                SubtractMeanEnvelope(work, meanUpper, meanLower, ctx);

                if (iter > 0 && RelativeChange(prev, work) < tol)
                    break;
            }

            var imf = CloneChannels(work);
            imfs.Add(imf);
            SubtractChannelsInPlace(residual, imf, ctx);

            if (!AnyChannelHasEnoughExtrema(residual, directions, projection, maxIdx, minIdx, minExtrema))
            {
                stop = EmdStopReason.ResidualTooFewExtrema;
                break;
            }

            if (mode == maxImf - 1)
                stop = EmdStopReason.MaxImfReached;
        }

        return new MemdResult<T>
        {
            IntrinsicModeFunctions = imfs,
            Residual = residual,
            StopReason = stop,
        };
    }

    private static bool AnyChannelHasEnoughExtrema<T>(
        T[][] channels,
        double[][] directions,
        double[] projection,
        List<int> maxIdx,
        List<int> minIdx,
        int minExtrema)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        foreach (var dir in directions)
        {
            Project(channels, dir, projection);
            CountExtrema(projection, maxIdx, minIdx);
            if (maxIdx.Count + minIdx.Count >= minExtrema)
                return true;
        }
        return false;
    }

    private static double[][] GenerateDirections(int channelCount, int count)
    {
        var dirs = new double[count][];
        if (channelCount == 2)
        {
            for (int i = 0; i < count; i++)
            {
                double theta = Math.PI * i / Math.Max(1, count - 1);
                dirs[i] = [Math.Cos(theta), Math.Sin(theta)];
            }
            return dirs;
        }

        for (int i = 0; i < count; i++)
        {
            var v = new double[channelCount];
            double normSq = 0;

            if (channelCount == 1)
            {
                v[0] = 1;
            }
            else
            {
                double product = 1.0;
                for (int d = 0; d < channelCount - 2; d++)
                {
                    double theta = Math.PI * 0.5 * Hammersley(i, d, count);
                    v[d] = product * Math.Cos(theta);
                    product *= Math.Sin(theta);
                }

                double phi = 2.0 * Math.PI * Hammersley(i, channelCount - 2, count);
                v[^2] = product * Math.Cos(phi);
                v[^1] = product * Math.Sin(phi);
            }

            for (int c = 0; c < channelCount; c++)
                normSq += v[c] * v[c];

            double norm = Math.Sqrt(normSq);
            if (norm < 1e-30)
            {
                v[0] = 1;
                for (int c = 1; c < channelCount; c++)
                    v[c] = 0;
            }
            else
            {
                for (int c = 0; c < channelCount; c++)
                    v[c] /= norm;
            }
            dirs[i] = v;
        }
        return dirs;
    }

    /// <summary>Radical-inverse Hammersley component in [0, 1).</summary>
    private static double Hammersley(int index, int dimension, int count)
    {
        int prime = Primes[dimension % Primes.Length];
        double f = 1, r = 0;
        int i = index;
        while (i > 0)
        {
            f /= prime;
            r += f * (i % prime);
            i /= prime;
        }
        return r;
    }

    private static readonly int[] Primes =
        [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53];

    private static void Project<T>(
        T[][] channels, double[] direction, double[] projection)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = projection.Length;
        int cCount = channels.Length;
        for (int i = 0; i < n; i++)
        {
            double sum = 0;
            for (int c = 0; c < cCount; c++)
                sum += Convert.ToDouble(channels[c][i]) * direction[c];
            projection[i] = sum;
        }
    }

    private static void CountExtrema(double[] x, List<int> maxima, List<int> minima)
    {
        maxima.Clear();
        minima.Clear();
        int len = x.Length;
        if (len < 3) return;

        for (int i = 1; i < len - 1; i++)
        {
            if (x[i] > x[i - 1] && x[i] > x[i + 1])
                maxima.Add(i);
            else if (x[i] < x[i - 1] && x[i] < x[i + 1])
                minima.Add(i);
        }
    }

    private static bool TryBuildEnvelope<T>(
        Span<T> signal,
        List<int> extrema,
        Span<T> envelope)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = signal.Length;
        if (extrema.Count < 2)
            return false;

        int e0 = extrema[0];
        int e1 = extrema[^1];
        int leftIdx = -e0;
        int rightIdx = 2 * (n - 1) - e1;

        int m = extrema.Count + 2;
        var xs = new double[m];
        var ys = new double[m];

        xs[0] = leftIdx;
        ys[0] = Convert.ToDouble(signal[e0]);
        for (int k = 0; k < extrema.Count; k++)
        {
            xs[k + 1] = extrema[k];
            ys[k + 1] = Convert.ToDouble(signal[extrema[k]]);
        }
        xs[^1] = rightIdx;
        ys[^1] = Convert.ToDouble(signal[e1]);

        for (int k = 1; k < m; k++)
        {
            if (!(xs[k] > xs[k - 1]))
                xs[k] = xs[k - 1] + 1e-6;
        }

        if (!NaturalCubicSpline.TryCreate(xs, ys, out var spline))
        {
            double mean = 0;
            for (int k = 0; k < ys.Length; k++) mean += ys[k];
            mean /= ys.Length;
            envelope.Fill(T.CreateChecked(mean));
            return true;
        }

        for (int i = 0; i < n; i++)
            envelope[i] = T.CreateChecked(spline.Evaluate(i));
        return true;
    }

    private static void Accumulate<T>(T[] acc, T[] add, ComputingContext? ctx)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = acc.Length;
        ComputingContextExecution.ForEach(ctx, 0, n, i =>
        {
            acc[i] += add[i];
        }, workPerItem: 2);
    }

    private static void ScaleInPlace<T>(T[] x, double scale, ComputingContext? ctx)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = x.Length;
        ComputingContextExecution.ForEach(ctx, 0, n, i =>
        {
            x[i] = T.CreateChecked(Convert.ToDouble(x[i]) * scale);
        }, workPerItem: 2);
    }

    private static void SubtractMeanEnvelope<T>(
        T[][] channels, T[][] upper, T[][] lower, ComputingContext? ctx)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = upper[0].Length;
        T half = T.CreateChecked(0.5);
        for (int c = 0; c < channels.Length; c++)
        {
            var h = channels[c];
            var u = upper[c];
            var l = lower[c];
            ComputingContextExecution.ForEach(ctx, 0, n, i =>
            {
                T mean = (u[i] + l[i]) * half;
                h[i] -= mean;
            }, workPerItem: 4);
        }
    }

    private static double RelativeChange<T>(T[][] prev, T[][] curr)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        double num = 0, den = 0;
        for (int c = 0; c < prev.Length; c++)
        {
            for (int i = 0; i < prev[c].Length; i++)
            {
                double p = Convert.ToDouble(prev[c][i]);
                double v = Convert.ToDouble(curr[c][i]);
                double d = p - v;
                num += d * d;
                den += p * p;
            }
        }
        if (den < 1e-30) return 0;
        return num / den;
    }

    private static T[][] CloneChannels<T>(IReadOnlyList<T[]> channels)
        where T : unmanaged
    {
        var copy = new T[channels.Count][];
        for (int c = 0; c < channels.Count; c++)
            copy[c] = (T[])channels[c].Clone();
        return copy;
    }

    private static void CopyChannels<T>(T[][] src, T[][] dst)
        where T : unmanaged
    {
        for (int c = 0; c < src.Length; c++)
            src[c].AsSpan().CopyTo(dst[c]);
    }

    private static void SubtractChannelsInPlace<T>(T[][] residual, T[][] imf, ComputingContext? ctx)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        for (int c = 0; c < residual.Length; c++)
        {
            var r = residual[c];
            var m = imf[c];
            int n = r.Length;
            ComputingContextExecution.ForEach(ctx, 0, n, i =>
            {
                r[i] -= m[i];
            }, workPerItem: 2);
        }
    }

    private readonly struct NaturalCubicSpline
    {
        private readonly double[] _x;
        private readonly double[] _a;
        private readonly double[] _b;
        private readonly double[] _c;
        private readonly double[] _d;
        private readonly int _seg;

        private NaturalCubicSpline(double[] x, double[] a, double[] b, double[] c, double[] d, int seg)
        {
            _x = x;
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _seg = seg;
        }

        public static bool TryCreate(double[] x, double[] y, out NaturalCubicSpline spline)
        {
            spline = default;
            int n = x.Length - 1;
            if (n < 1 || x.Length != y.Length) return false;

            var h = new double[n];
            for (int i = 0; i < n; i++)
            {
                h[i] = x[i + 1] - x[i];
                if (!(h[i] > 0)) return false;
            }

            var alpha = new double[n];
            for (int i = 1; i < n; i++)
                alpha[i] = 3.0 * ((y[i + 1] - y[i]) / h[i] - (y[i] - y[i - 1]) / h[i - 1]);

            var l = new double[n + 1];
            var mu = new double[n];
            var z = new double[n + 1];
            var c = new double[n + 1];
            var b = new double[n];
            var d = new double[n];

            l[0] = 1;
            for (int i = 1; i < n; i++)
            {
                l[i] = 2.0 * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
                if (Math.Abs(l[i]) < 1e-30) return false;
                mu[i] = h[i] / l[i];
                z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
            }

            l[n] = 1;
            for (int j = n - 1; j >= 0; j--)
            {
                c[j] = z[j] - mu[j] * c[j + 1];
                b[j] = (y[j + 1] - y[j]) / h[j] - h[j] * (c[j + 1] + 2.0 * c[j]) / 3.0;
                d[j] = (c[j + 1] - c[j]) / (3.0 * h[j]);
            }

            spline = new NaturalCubicSpline(x, y, b, c, d, n);
            return true;
        }

        public double Evaluate(double xi)
        {
            if (xi <= _x[0]) return _a[0];
            if (xi >= _x[_seg]) return _a[_seg];

            int i = 0;
            while (i < _seg && xi > _x[i + 1]) i++;
            if (i == _seg) i--;
            double dx = xi - _x[i];
            return _a[i] + _b[i] * dx + _c[i] * dx * dx + _d[i] * dx * dx * dx;
        }
    }
}
